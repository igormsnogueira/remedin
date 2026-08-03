"""
Analise exploratoria da base "Medicamentos Registrados no Brasil" (ANVISA).

Levanta estrutura, volume, qualidade e candidatos a chave antes de modelar
o dominio. Script de exploracao, nao de producao.

Fonte: https://dados.anvisa.gov.br/dados/DADOS_ABERTOS_MEDICAMENTOS.csv

Uso: python analyze_anvisa.py caminho/para/DADOS_ABERTOS_MEDICAMENTOS.csv
Requisitos: pip install pandas
"""

import csv
import os
import sys

import pandas as pd

ENCODINGS = ["utf-8", "utf-8-sig", "latin1", "cp1252"]
SEPARATORS = [";", ",", "\t", "|"]
SAMPLE_ROWS = 100


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
    with open(path, "r", encoding=encoding) as f:
        header = f.readline()
    try:
        return csv.Sniffer().sniff(header, delimiters="".join(SEPARATORS)).delimiter
    except csv.Error:
        return max(SEPARATORS, key=header.count)


def section(text):
    print(f"\n-- {text}")


def analyze(path):
    if not os.path.exists(path):
        print(f"erro: arquivo nao encontrado: {path}")
        sys.exit(1)

    size_mb = os.path.getsize(path) / (1024 * 1024)

    section("arquivo")
    encoding = detect_encoding(path)
    if encoding is None:
        print("erro: nenhum encoding testado leu o arquivo")
        sys.exit(1)
    separator = detect_separator(path, encoding)
    print(f"tamanho: {size_mb:.1f} MB | encoding: {encoding} | delimitador: {separator!r}")

    # dtype=str preserva zeros a esquerda em numeros de registro.
    df = pd.read_csv(
        path,
        sep=separator,
        encoding=encoding,
        dtype=str,
        keep_default_na=False,
        na_values=[""],
        on_bad_lines="warn",
        low_memory=False,
    )
    total_rows, total_columns = df.shape
    print(f"linhas: {total_rows:,} | colunas: {total_columns}".replace(",", "."))

    section("colunas: preenchimento e cardinalidade")
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

    section("situacao do registro")
    status_col = next((c for c in df.columns if "SITUACAO" in c.upper()), None)
    if status_col:
        count = df[status_col].value_counts(dropna=False)
        percentage = (count / total_rows * 100).round(1)
        print(pd.DataFrame({"linhas": count, "%": percentage}))
    else:
        print("coluna de situacao nao encontrada")

    section("candidatos a chave")

    def looks_like_key(name):
        n = name.upper()
        if "PROCESSO" in n:
            return True
        if "REGISTRO" not in n:
            return False
        # SITUACAO_REGISTRO e EMPRESA_DETENTORA_REGISTRO nao sao chaves.
        prefixes = ("NUMERO", "NRO", "NUM", "N_", "CODIGO", "COD")
        return any(p in n for p in prefixes) or n in ("REGISTRO", "REGISTRO_PRODUTO")

    candidates = [c for c in df.columns if looks_like_key(c)]
    if not candidates:
        candidates = [c for c in df.columns if "REGISTRO" in c.upper()]
    print(f"colunas candidatas: {candidates}")

    for col in candidates:
        distinct = df[col].nunique(dropna=True)
        unique = distinct == total_rows
        print(f"\n[{col}] distintos: {distinct:,} | unica: {unique}".replace(",", "."))

        if not unique and distinct > 0:
            avg = total_rows / distinct
            print(f"  linhas por valor (média): {avg:.2f}")

            repeated = df[col].value_counts()
            repeated = repeated[repeated > 1]
            if len(repeated) > 0:
                example = repeated.index[0]
                qty = repeated.iloc[0]
                print(f"  exemplo repetido: {example} ({qty} linhas)")
                sample_columns = df.columns[: min(8, total_columns)]
                print(df[df[col] == example][sample_columns].to_string(index=False))

    # nulos de registro correlacionam com categoria regulatoria?
    if "NUMERO_REGISTRO_PRODUTO" in df.columns and "CATEGORIA_REGULATORIA" in df.columns:
        section("nulos de NUMERO_REGISTRO_PRODUTO por CATEGORIA_REGULATORIA")
        null_rows = df[df["NUMERO_REGISTRO_PRODUTO"].isna()]
        total_by_category = df["CATEGORIA_REGULATORIA"].value_counts(dropna=False)
        nulls_by_category = null_rows["CATEGORIA_REGULATORIA"].value_counts(dropna=False)
        crosstab = pd.DataFrame(
            {"total": total_by_category, "sem_registro": nulls_by_category}
        ).fillna(0).astype({"sem_registro": int})
        crosstab["%_sem_registro"] = (
            crosstab["sem_registro"] / crosstab["total"] * 100
        ).round(1)
        print(crosstab.sort_values("%_sem_registro", ascending=False).to_string())

    section("qualidade")
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
        print(f"colunas com espaços sobrando (quebram cruzamento de chave): {columns_with_spaces}")

    columns_with_bad_encoding = [
        c for c in df.columns
        if len(df[c].dropna())
        and df[c].dropna().astype(str).str.contains("Ã|Â|�", regex=True).any()
    ]
    if columns_with_bad_encoding:
        print(f"colunas com lixo de acentuação: {columns_with_bad_encoding}")

    section("amostra para o repositório")
    destination = os.path.join("data", "samples", "medicamentos-sample.csv")
    os.makedirs(os.path.dirname(destination), exist_ok=True)
    df.head(SAMPLE_ROWS).to_csv(destination, sep=separator, encoding="utf-8", index=False)
    print(f"{SAMPLE_ROWS} linhas salvas em {destination} (utf-8)")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    analyze(sys.argv[1])
