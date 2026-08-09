"""
Mede o tamanho real do catalogo do MVP e a cobertura da informacao clinica.

O produto nao mostra as 43.397 linhas da base de registro. Mostra o que
sobra depois de filtrar registro preenchido, registro ativo, preco publicado,
uso nao hospitalar e comercializacao declarada. Esse numero decide o recorte
do produto e nao estava medido em lugar nenhum.

Tambem responde tres perguntas abertas:
  - registro com preco publicado mas marcado como inativo na ANVISA;
  - registro de 8 digitos casa com preco se preencher zero a esquerda?
  - quanto da informacao clinica (tarja, substancia, classe) sobrevive ao funil.

Uso:
    python measure_catalog_funnel.py DADOS_ABERTOS_MEDICAMENTOS.csv TA_PRECO_MEDICAMENTO.csv

Requisitos: pip install pandas
"""

import os
import sys

import pandas as pd

from csv_profile import display_path, load_csv, repo_path, report_to, section

REGISTRY_KEY = "NUMERO_REGISTRO_PRODUTO"
PRICE_KEY = "REGISTRO"
REGISTRY_KEY_LENGTH = 9

STATUS_COLUMN = "SITUACAO_REGISTRO"
ACTIVE_STATUS = "Ativo"

HOSPITAL_ONLY = "RESTRIÇÃO HOSPITALAR"
SOLD_LAST_YEAR = "COMERCIALIZAÇÃO 2025"
CLINICAL_COLUMNS = ("TARJA", "SUBSTÂNCIA", "CLASSE TERAPÊUTICA")

# Valor de preenchimento da CMED: "-" e variantes com espaco nao sao dado.
PLACEHOLDERS = {"-", "- (*)", ""}

REPORT_TITLE = "Funil do catálogo e cobertura da informação clínica"
DEFAULT_REPORT = repo_path("docs", "output", "funil-do-catalogo.md")


def digits_only(series):
    return series.dropna().astype(str).str.replace(r"\D", "", regex=True)


def is_filled(series):
    return ~series.fillna("").astype(str).str.strip().isin(PLACEHOLDERS)


def step(label, count, previous):
    share = f"{count / previous * 100:5.1f}% do anterior" if previous else ""
    print(f"{label:.<52} {count:>7,}  {share}".replace(",", "."))
    return count


def measure(registry_path, price_path, report_path):
    for path in (registry_path, price_path):
        if not os.path.exists(path):
            sys.exit(f"erro: arquivo nao encontrado: {path}")

    registry = load_csv(registry_path).frame
    price = load_csv(price_path).frame

    with report_to(report_path, REPORT_TITLE, price_path):
        write_funnel(registry, price)


def write_funnel(registry, price):
    section("funil do catálogo")

    total = step("linhas na base de registro", len(registry), 0)
    with_key = registry[registry[REGISTRY_KEY].notna()]
    step("linhas com número de registro", len(with_key), total)

    registry_keys = digits_only(registry[REGISTRY_KEY])
    distinct = registry_keys.drop_duplicates()
    previous = step("registros distintos", len(distinct), len(with_key))

    active = registry[registry[STATUS_COLUMN].str.strip() == ACTIVE_STATUS]
    active_keys = digits_only(active[REGISTRY_KEY]).drop_duplicates()
    previous = step("registros ativos", len(active_keys), previous)

    price_keys = digits_only(price[PRICE_KEY]).str[:REGISTRY_KEY_LENGTH]
    priced = active_keys[active_keys.isin(set(price_keys))]
    previous = step("registros ativos com preço", len(priced), previous)

    # Do lado do preco o filtro e por linha, porque restricao hospitalar e
    # comercializacao sao atributos da apresentacao, nao do registro.
    retail = price[price[HOSPITAL_ONLY].str.strip() != "Sim"]
    retail_keys = digits_only(retail[PRICE_KEY]).str[:REGISTRY_KEY_LENGTH]
    in_retail = priced[priced.isin(set(retail_keys))]
    previous = step("com ao menos uma apresentação de balcão", len(in_retail), previous)

    sold = retail[retail[SOLD_LAST_YEAR].str.strip() == "Sim"]
    sold_keys = digits_only(sold[PRICE_KEY]).str[:REGISTRY_KEY_LENGTH]
    catalog = in_retail[in_retail.isin(set(sold_keys))]
    step("e comercializada no último ano", len(catalog), previous)

    print(f"\ncatálogo efetivo: {len(catalog):,} medicamentos".replace(",", "."))

    section("registros com preço mas inativos na ANVISA")
    inactive = registry[registry[STATUS_COLUMN].str.strip() != ACTIVE_STATUS]
    inactive_keys = digits_only(inactive[REGISTRY_KEY]).drop_duplicates()
    inactive_priced = inactive_keys[inactive_keys.isin(set(price_keys))]
    print(f"{len(inactive_priced):,} registros".replace(",", "."))
    print("A CMED publica preço vigente para eles e a ANVISA os dá como inativos.")
    print(f"exemplos: {inactive_priced.head(5).tolist()}")

    section("registros fora do padrão de 9 dígitos")
    short = registry_keys[registry_keys.str.len() != REGISTRY_KEY_LENGTH]
    print(f"{len(short)} valores, comprimentos: {sorted(short.str.len().unique())}")
    padded = short.str.zfill(REGISTRY_KEY_LENGTH)
    recovered = padded[padded.isin(set(price_keys))]
    print(f"casam com preço após preencher zero à esquerda: {len(recovered)}")
    if len(recovered):
        print(f"  {recovered.tolist()}")
        print("  Zero à esquerda perdido, não lixo. O parser precisa preservar.")

    section("cobertura da informação clínica no catálogo efetivo")
    in_catalog = sold[digits_only(sold[PRICE_KEY]).str[:REGISTRY_KEY_LENGTH].isin(set(catalog))]
    rows = len(in_catalog)
    print(f"linhas de preço no catálogo efetivo: {rows:,}".replace(",", "."))
    for col in CLINICAL_COLUMNS:
        if col not in in_catalog.columns:
            print(f"  [{col}] coluna ausente")
            continue
        filled = is_filled(in_catalog[col]).sum()
        print(f"  {col:.<28} {filled / rows * 100:5.1f}% com valor útil")

    section("medicamentos ativos sem preço")
    without_price = active_keys[~active_keys.isin(set(price_keys))]
    print(f"{len(without_price):,} registros ficam sem informação clínica".replace(",", "."))
    print("A fonte clínica é a lista da CMED, então eles entram só com nome e fabricante.")


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)
    measure(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else DEFAULT_REPORT)
