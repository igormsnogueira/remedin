# 0002 — Cruzamento de preço e registro pelo número de registro

**Status:** aceita · **Data:** 03/08/2026

## Contexto

O catálogo só existe se as duas bases se ligarem.

Os campos têm tamanhos diferentes: `NUMERO_REGISTRO_PRODUTO` na ANVISA tem 9 dígitos, `REGISTRO` na CMED tem 13.

## Decisão

Comparar os 9 primeiros dígitos do `REGISTRO` da CMED com o `NUMERO_REGISTRO_PRODUTO` da ANVISA, removendo antes tudo que não for dígito dos dois lados.

```
REGISTRO (CMED)   1705600230032
                  └───────┘
                  9 primeiros = NUMERO_REGISTRO_PRODUTO (ANVISA)
```

A normalização fica dentro do objeto de valor `NumeroRegistro`, num lugar só.

## Por quê

O cruzamento das bases completas casou 25.691 das 25.702 linhas de preço: 99,96%. As onze sem par vêm de dois registros.

Com esse resultado, casar por nome ou princípio ativo não é necessário, e evita-se o risco de ligar o preço de um medicamento ao registro de outro.

Os 4 dígitos finais não foram decodificados e não são usados. Eles não identificam a apresentação: `1018600330018` aparece em duas linhas com `CÓDIGO GGREM` e `SUBSTÂNCIA` diferentes.

## Consequências

Linha de preço sem par vai para quarentena com o motivo e é reavaliada na carga seguinte. Isso cobre o caso de ordem: se o ETL de preço rodar primeiro, tudo é órfão na primeira execução.

A cobertura é medida a cada carga. Abaixo de 95% a carga é rejeitada e os dados anteriores permanecem; entre 95% e 99% grava e registra alerta.

Prova de conceito em [`analise-dados-cmed.md`](../analise-dados-cmed.md), script em [`scripts/check_price_registry_join.py`](../../scripts/check_price_registry_join.py).
