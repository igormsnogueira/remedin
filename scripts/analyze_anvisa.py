"""
Analise exploratoria da base "Medicamentos Registrados no Brasil" (ANVISA).

Levanta estrutura, volume, qualidade e candidatos a chave antes de modelar
o dominio. Script de exploracao, nao de producao.

Fonte: https://dados.anvisa.gov.br/dados/DADOS_ABERTOS_MEDICAMENTOS.csv

Uso: python analyze_anvisa.py caminho/para/DADOS_ABERTOS_MEDICAMENTOS.csv [saida.md]
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

REPORT_TITLE = "Perfil da base de registro da ANVISA"
DEFAULT_REPORT = repo_path("docs", "output", "registro-anvisa.md")


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

    section("situacao do registro")
    status_col = next((c for c in df.columns if "SITUACAO" in c.upper()), None)
    if status_col:
        total_rows = len(df)
        count = df[status_col].value_counts(dropna=False)
        percentage = (count / total_rows * 100).round(1)
        print(pd.DataFrame({"linhas": count, "%": percentage}))
    else:
        print("coluna de situacao nao encontrada")

    section("candidatos a chave")
    candidates = find_key_candidates(df)
    print_key_analysis(df, candidates)

    # nulos de registro correlacionam com categoria regulatoria? Especifico
    # deste dataset - achado da issue #2, ver docs/analise-dados-anvisa.md.
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
    print_quality_checks(df)

    section("amostra para o repositório")
    destination = repo_path("data", "samples", "medicamentos-sample.csv")
    save_sample(df, destination, loaded.separator, SAMPLE_ROWS)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    analyze(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else DEFAULT_REPORT)
