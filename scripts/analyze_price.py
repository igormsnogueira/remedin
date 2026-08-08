"""
Analise exploratoria da base de preco de medicamentos (ANVISA/CMED).

O arquivo comeca com dezenas de linhas de texto legal antes do cabecalho
real; quem trata isso e o detect_header_row de csv_profile.

Fonte: https://dados.anvisa.gov.br/dados/TA_PRECO_MEDICAMENTO.csv
(mesmo dominio do CSV de registro, ver docs/analise-dados-anvisa.md)
Atencao: o caminho e case-sensitive, a extensao e minuscula.

Uso: python analyze_price.py caminho/para/TA_PRECO_MEDICAMENTO.csv [saida.md]
Requisitos: pip install pandas

O relatorio vai pra docs/output/
"""

import os
import sys

import pandas as pd

from csv_profile import (
    find_key_candidates,
    load_csv,
    print_column_summary,
    print_file_info,
    print_key_analysis,
    print_quality_checks,
    repo_path,
    report_to,
    save_sample,
    section,
)

SAMPLE_ROWS = 100

REPORT_TITLE = "Perfil da lista de preços da CMED"
DEFAULT_REPORT = repo_path("docs", "output", "preco-cmed.md")

# Termos especificos de lista de preco da CMED, alem de REGISTRO/PROCESSO
# ja cobertos por find_key_candidates.
EXTRA_KEY_TERMS = ("GGREM", "EAN")

# Colunas de dominio curto que definem o recorte do MVP (o que exibir e o
# que esconder). Nome exato conforme cabecalho da CMED.
CATEGORY_COLUMNS = (
    "TIPO DE PRODUTO (STATUS DO PRODUTO)",
    "REGIME DE PREÇO",
    "RESTRIÇÃO HOSPITALAR",
    "TARJA",
    "COMERCIALIZAÇÃO 2025",
)

# A lista repete o mesmo preco para cada aliquota de ICMS. Sao dezenas de
# colunas variando so no percentual; o MVP precisa escolher uma.
PRICE_PREFIXES = ("PF", "PMC")


def print_price_columns(df):
    for prefix in PRICE_PREFIXES:
        columns = [c for c in df.columns if c.upper().startswith(prefix + " ")]
        if not columns:
            continue
        filled = df[columns].notna().any(axis=1).sum()
        print(f"{prefix}: {len(columns)} colunas | linhas com algum valor: {filled:,}".replace(",", "."))
        print(f"  {columns}")


def print_category_breakdown(df):
    for col in CATEGORY_COLUMNS:
        if col not in df.columns:
            print(f"[{col}] coluna ausente")
            continue
        count = df[col].value_counts(dropna=False)
        percentage = (count / len(df) * 100).round(1)
        print(f"\n[{col}]")
        print(pd.DataFrame({"linhas": count, "%": percentage}).to_string())


def analyze(path, report_path):
    if not os.path.exists(path):
        sys.exit(f"erro: arquivo nao encontrado: {path}")
    try:
        loaded = load_csv(path)
    except ValueError as e:
        sys.exit(f"erro: {e}")

    with report_to(report_path, REPORT_TITLE, path):
        write_profile(path, loaded)


def write_profile(path, loaded):
    df = loaded.frame

    section("arquivo")
    print_file_info(path, loaded)

    section("colunas: preenchimento e cardinalidade")
    print_column_summary(df)

    section("candidatos a chave")
    candidates = find_key_candidates(df, extra_terms=EXTRA_KEY_TERMS)
    print_key_analysis(df, candidates)

    section("colunas de preço por alíquota")
    print_price_columns(df)

    section("categorias que definem o recorte do MVP")
    print_category_breakdown(df)

    section("qualidade")
    print_quality_checks(df)

    section("amostra para o repositório")
    destination = repo_path("data", "samples", "preco-sample.csv")
    save_sample(df, destination, loaded.separator, SAMPLE_ROWS)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    analyze(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else DEFAULT_REPORT)
