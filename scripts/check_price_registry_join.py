"""
PoC do cruzamento entre a base de registro (ANVISA) e a lista de preco (CMED).

Responde a pergunta que trava a modelagem: da pra ligar preco a registro por
chave, com que cobertura, e em que cardinalidade.

Hipotese testada: o REGISTRO da CMED tem 13 digitos e os 9 primeiros sao o
NUMERO_REGISTRO_PRODUTO da ANVISA; os 4 restantes identificam a apresentacao
e o digito verificador. Se confirmar, a relacao e 1 registro para N precos.

Uso:
    python check_price_registry_join.py DADOS_ABERTOS_MEDICAMENTOS.csv TA_PRECO_MEDICAMENTO.csv [saida.md]

Requisitos: pip install pandas

O relatorio vai pra docs/output/
"""

import os
import sys

from csv_profile import load_csv, repo_path, report_to, section

REPORT_TITLE = "Cruzamento entre preço (CMED) e registro (ANVISA)"
DEFAULT_REPORT = repo_path("docs", "output", "cruzamento-preco-registro.md")

REGISTRY_KEY = "NUMERO_REGISTRO_PRODUTO"
PRICE_KEY = "REGISTRO"

# A base de registro guarda o historico inteiro, incluindo registro cancelado.
# Cobertura sobre o total nao diz nada; o que decide o MVP e a cobertura sobre
# o que ainda esta valendo.
STATUS_COLUMN = "SITUACAO_REGISTRO"
ACTIVE_STATUS = "Ativo"

# Os 9 primeiros digitos do registro da CMED devem ser o registro da ANVISA.
REGISTRY_KEY_LENGTH = 9

EXAMPLES_TO_SHOW = 5


def digits_only(series):
    return series.dropna().astype(str).str.replace(r"\D", "", regex=True)


def require_column(df, column, source):
    if column not in df.columns:
        sys.exit(
            f"erro: coluna {column!r} nao existe em {source}\n"
            f"colunas disponiveis: {list(df.columns)}"
        )


def describe_key(label, values):
    print(f"\n[{label}]")
    print(f"  valores preenchidos: {len(values):,}".replace(",", "."))
    print(f"  distintos: {values.nunique():,}".replace(",", "."))
    print("  comprimento em dígitos:")
    for length, qty in values.str.len().value_counts().sort_index().items():
        print(f"    {length}: {qty:,}".replace(",", "."))
    print(f"  exemplos: {values.head(EXAMPLES_TO_SHOW).tolist()}")


def report_coverage(label, keys, reference):
    reference_set = set(reference)
    matched = keys.isin(reference_set)
    rate = matched.mean() * 100 if len(keys) else 0.0
    print(
        f"{label}: {matched.sum():,} de {len(keys):,} ({rate:.1f}%)".replace(",", ".")
    )
    missing = keys[~matched]
    if not missing.empty:
        print(f"  exemplos sem par: {missing.drop_duplicates().head(EXAMPLES_TO_SHOW).tolist()}")
    return matched


def check(registry_path, price_path, report_path):
    for path in (registry_path, price_path):
        if not os.path.exists(path):
            sys.exit(f"erro: arquivo nao encontrado: {path}")

    registry = load_csv(registry_path).frame
    price = load_csv(price_path).frame
    require_column(registry, REGISTRY_KEY, "base de registro")
    require_column(price, PRICE_KEY, "lista de preço")

    with report_to(report_path, REPORT_TITLE, price_path):
        write_join_report(registry, price)


def write_join_report(registry, price):
    registry_keys = digits_only(registry[REGISTRY_KEY])
    price_keys_full = digits_only(price[PRICE_KEY])
    price_keys = price_keys_full.str[:REGISTRY_KEY_LENGTH]

    section("formato das chaves")
    describe_key(f"registro ANVISA . {REGISTRY_KEY}", registry_keys)
    describe_key(f"preço CMED . {PRICE_KEY} (completo)", price_keys_full)
    describe_key(
        f"preço CMED . {PRICE_KEY} (primeiros {REGISTRY_KEY_LENGTH} dígitos)", price_keys
    )

    section("cobertura do cruzamento")
    print("linhas de preço que encontram produto na base de registro:")
    report_coverage("  preço -> registro", price_keys, registry_keys)
    print("\nregistros que têm ao menos um preço publicado:")
    report_coverage("  registro -> preço", registry_keys.drop_duplicates(), price_keys)

    if STATUS_COLUMN in registry.columns:
        active = registry[registry[STATUS_COLUMN].str.strip() == ACTIVE_STATUS]
        active_keys = digits_only(active[REGISTRY_KEY]).drop_duplicates()
        print(f"\nmesmo recorte, só {STATUS_COLUMN} = {ACTIVE_STATUS}:")
        report_coverage("  registro ativo -> preço", active_keys, price_keys)

    section("cardinalidade")
    per_registry = price_keys.value_counts()
    print(f"registros distintos com preço: {per_registry.size:,}".replace(",", "."))
    print(f"apresentações por registro: média {per_registry.mean():.2f} | máximo {per_registry.max()}")
    top = per_registry.head(3)
    for key, qty in top.items():
        print(f"  {key}: {qty} apresentações")

    duplicated_full = price_keys_full.duplicated().sum()
    print(f"registros de 13 dígitos repetidos na lista de preço: {duplicated_full:,}".replace(",", "."))


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)
    check(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else DEFAULT_REPORT)
