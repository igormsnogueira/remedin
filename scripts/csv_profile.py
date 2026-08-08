"""
Funcoes compartilhadas de perfil exploratorio de CSV.

Usadas pelos scripts de analise de cada fonte (analyze_anvisa.py,
analyze_price.py). Nao e um script de execucao direta - so funcoes.
"""

import csv
import os
import re
from contextlib import contextmanager, redirect_stdout
from datetime import datetime
from typing import NamedTuple

import pandas as pd

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

ENCODINGS = ["utf-8", "utf-8-sig", "latin1", "cp1252"]
SEPARATORS = [";", ",", "\t", "|"]

HEADER_SCAN_LINES = 200
SEPARATOR_SAMPLE_BYTES = 64 * 1024

# Sequencia tipica de UTF-8 lido como latin1 ("Ã§", "Ã£"): a letra acentuada
# virou dois caracteres. Nao basta procurar "Ã" sozinho, que e letra valida
# em portugues (APRESENTAÇÃO, NÃO, SÃO).
MOJIBAKE = re.compile(r"[ÃÂ][-¿]")

# Letra acentuada substituida por "?" no meio da palavra (FITOTER?PICO).
# Diferente do mojibake: aqui o caractere original se perdeu.
LOST_CHARACTER = re.compile(r"[A-Za-zÀ-ÿ]\?[A-Za-zÀ-ÿ]")

REPLACEMENT_CHARACTER = "�"


class LoadedCsv(NamedTuple):
    frame: pd.DataFrame
    encoding: str
    separator: str
    header_row: int


def detect_encoding(path):
    for enc in ENCODINGS:
        try:
            with open(path, "r", encoding=enc) as f:
                for _ in range(500):
                    if not f.readline():
                        break
            return enc
        except (UnicodeDecodeError, LookupError):
            continue
    return None


def detect_separator(path, encoding):
    # Amostra varias linhas, nao so a primeira: em arquivo com preambulo a
    # primeira linha nao representa a estrutura do resto.
    with open(path, "r", encoding=encoding) as f:
        sample = f.read(SEPARATOR_SAMPLE_BYTES)
    try:
        return csv.Sniffer().sniff(sample, delimiters="".join(SEPARATORS)).delimiter
    except csv.Error:
        return max(SEPARATORS, key=sample.count)


def detect_header_row(path, encoding, separator):
    # A lista da CMED comeca com dezenas de linhas de texto legal que ocupam
    # so a primeira coluna. O cabecalho e a linha que preenche mais colunas.
    # Em arquivo sem preambulo isso devolve 0.
    widest = 0
    header_row = 0
    with open(path, "r", encoding=encoding, newline="") as f:
        for index, row in enumerate(csv.reader(f, delimiter=separator)):
            if index >= HEADER_SCAN_LINES:
                break
            filled = sum(1 for value in row if value.strip())
            if filled > widest:
                widest = filled
                header_row = index
    return header_row


def section(text):
    print(f"\n-- {text}")


def display_path(path):
    # Caminho curto quando a saida esta dentro do repositorio; absoluto
    # quando o usuario apontou pra fora dele.
    relative = os.path.relpath(path, REPO_ROOT)
    return path if relative.startswith(os.pardir) else relative


def repo_path(*parts):
    # Saida sempre na raiz do repositorio, independente de onde o script foi
    # chamado - senao rodar de dentro de scripts/ cria uma arvore duplicada.
    return os.path.join(REPO_ROOT, *parts)


@contextmanager
def report_to(destination, title, source_path):
    # O perfil e longo demais pro terminal e serve de evidencia da analise,
    # entao vai pra arquivo. Bloco de codigo em markdown preserva o
    # alinhamento das tabelas do pandas.
    os.makedirs(os.path.dirname(destination), exist_ok=True)
    with open(destination, "w", encoding="utf-8") as handle:
        handle.write(f"# {title}\n\n")
        handle.write(
            f"Gerado em {datetime.now():%d/%m/%Y %H:%M} a partir de "
            f"`{os.path.basename(source_path)}`.\n\n"
        )
        handle.write("Saída bruta do script. Serve de evidência da análise, não editar à mão.\n\n")
        handle.write("```\n")
        try:
            with redirect_stdout(handle):
                yield
        finally:
            handle.write("```\n")
    print(f"relatório salvo em {display_path(destination)}")


def load_csv(path):
    # dtype=str preserva zero a esquerda em campos que parecem numero.
    encoding = detect_encoding(path)
    if encoding is None:
        raise ValueError("nenhum encoding testado leu o arquivo")

    separator = detect_separator(path, encoding)
    header_row = detect_header_row(path, encoding, separator)
    frame = pd.read_csv(
        path,
        sep=separator,
        encoding=encoding,
        skiprows=header_row,
        dtype=str,
        keep_default_na=False,
        na_values=[""],
        on_bad_lines="warn",
        low_memory=False,
    )
    frame.columns = frame.columns.str.strip()
    return LoadedCsv(frame, encoding, separator, header_row)


def print_file_info(path, loaded):
    size_mb = os.path.getsize(path) / (1024 * 1024)
    total_rows, total_columns = loaded.frame.shape
    print(
        f"tamanho: {size_mb:.1f} MB | encoding: {loaded.encoding} "
        f"| delimitador: {loaded.separator!r}"
    )
    print(f"linhas: {total_rows:,} | colunas: {total_columns}".replace(",", "."))
    if loaded.header_row:
        print(f"preambulo descartado: {loaded.header_row} linhas antes do cabeçalho")


def print_column_summary(df):
    total_rows = len(df)
    summary = pd.DataFrame({
        "nulos": df.isna().sum(),
        "preenchidos_%": (df.notna().sum() / total_rows * 100).round(1),
        "distintos": df.nunique(dropna=True),
    })
    summary["exemplo"] = [
        (df[c].dropna().iloc[0][:45] if df[c].notna().any() else "")
        for c in df.columns
    ]
    pd.set_option("display.max_rows", None)
    pd.set_option("display.max_columns", None)
    pd.set_option("display.width", 0)
    pd.set_option("display.max_colwidth", 48)
    print(summary.to_string())


def find_key_candidates(df, extra_terms=()):
    # "PROCESSO" (ou um termo extra da fonte, ex.: GGREM/EAN) já basta
    # sozinho. "REGISTRO" só conta se vier com prefixo de numero/codigo,
    # senao pega SITUACAO_REGISTRO, EMPRESA_DETENTORA_REGISTRO etc., que
    # citam "registro" no nome mas nao sao chave de linha.
    number_prefixes = ("NUMERO", "NRO", "NUM", "N_", "CODIGO", "CÓDIGO", "COD")

    def looks_like_key(name):
        n = name.upper()
        if n.startswith("DATA"):
            # DATA_FINALIZACAO_PROCESSO cita "processo" mas e data, nao chave.
            return False
        if "PROCESSO" in n or any(t in n for t in extra_terms):
            return True
        if "REGISTRO" not in n:
            return False
        return any(p in n for p in number_prefixes) or n in ("REGISTRO", "REGISTRO_PRODUTO")

    return [c for c in df.columns if looks_like_key(c)]


def print_key_analysis(df, candidates, sample_columns_limit=8):
    total_columns = len(df.columns)
    print(f"colunas candidatas: {candidates}")

    for col in candidates:
        # Linha vazia nao repete chave nenhuma: contar as duas coisas sobre o
        # total do arquivo infla a média em coluna com muito nulo.
        values = df[col].dropna()
        distinct = values.nunique()
        unique = distinct == len(values)
        print(
            f"\n[{col}] preenchidos: {len(values):,} | distintos: {distinct:,} "
            f"| sem repetição: {unique}".replace(",", ".")
        )

        if not unique and distinct > 0:
            avg = len(values) / distinct
            print(f"  linhas por valor (média entre os preenchidos): {avg:.2f}")

            repeated = df[col].value_counts()
            repeated = repeated[repeated > 1]
            if len(repeated) > 0:
                example = repeated.index[0]
                qty = repeated.iloc[0]
                print(f"  exemplo repetido: {example} ({qty} linhas)")
                sample_columns = df.columns[: min(sample_columns_limit, total_columns)]
                print(df[df[col] == example][sample_columns].to_string(index=False))


def print_quality_checks(df):
    duplicates = df.duplicated().sum()
    print(f"linhas totalmente duplicadas: {duplicates:,}".replace(",", "."))

    empty_columns = [c for c in df.columns if df[c].isna().all()]
    if empty_columns:
        print(f"colunas 100% vazias: {empty_columns}")

    columns_with_spaces = [
        c for c in df.columns
        if len(df[c].dropna()) and (df[c].dropna() != df[c].dropna().str.strip()).any()
    ]
    if columns_with_spaces:
        print(f"colunas com espaços sobrando (quebram agrupamento e comparação): {columns_with_spaces}")

    # Os dois defeitos de acento pedem tratamento diferente: mojibake dá pra
    # reverter reprocessando o encoding, "?" no lugar da letra não.
    scrambled = []
    lost = []
    for col in df.columns:
        values = df[col].dropna().astype(str)
        if values.empty:
            continue
        has_replacement = values.str.contains(REPLACEMENT_CHARACTER, regex=False).any()
        if has_replacement or values.str.contains(MOJIBAKE).any():
            scrambled.append(col)
        if values.str.contains(LOST_CHARACTER).any():
            lost.append(col)

    if scrambled:
        print(f"colunas com acento embaralhado (erro de encoding, recuperável): {scrambled}")
    if lost:
        print(f"colunas com acento perdido (letra virou '?', não recuperável): {lost}")
    if not scrambled and not lost:
        print("acentuação: nenhum defeito detectado")


def save_sample(df, destination, separator, rows=100):
    os.makedirs(os.path.dirname(destination), exist_ok=True)
    df.head(rows).to_csv(destination, sep=separator, encoding="utf-8", index=False)
    print(f"{rows} linhas salvas em {display_path(destination)} (utf-8)")
