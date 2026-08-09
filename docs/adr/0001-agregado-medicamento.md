# 0001 — Medicamento com apresentações

**Status:** aceita · **Data:** 03/08/2026

## Contexto

A base de registro da ANVISA tem uma linha por medicamento: 32.629 números preenchidos para 32.626 valores distintos.

A lista de preços da CMED tem várias linhas para o mesmo número: 2,88 em média, até 42. Cada uma é uma embalagem ou dosagem diferente, com preço próprio. O registro `101180676` é uma linha na ANVISA e 42 na CMED.

## Decisão

`Medicamento` é o agregado raiz, identificado pelo objeto de valor `NumeroRegistro`.

`Apresentacao` é entidade filha dentro do agregado, identificada pelo `CÓDIGO GGREM`, com dosagem, embalagem e preço.

Entram no catálogo os registros com `NUMERO_REGISTRO_PRODUTO` preenchido.

## Por quê

Apresentação não existe sozinha: não se busca uma embalagem sem o nome do produto, e o ciclo de vida dela é o do registro. Por isso é entidade filha, e não agregado próprio.

O filtro é pelo campo preenchido, e não por lista de categorias regulatórias, porque a ausência do número não é regra de categoria: Radiofármaco fica em 24,6%, e a mesma categoria aparece grafada de dois jeitos com resultados opostos.

## Consequências

Montar um `Medicamento` completo exige dado das duas fontes.

Medicamento sem apresentação é estado válido: 20% dos registros ativos não têm preço publicado, e o ETL de preço pode não ter rodado. O domínio não trata isso como erro.

Os 25% sem número ficam fora do catálogo, o que inclui as categorias BAIXO RISCO (7.112 linhas) e DINAMIZADO (3.456). A busca precisa responder alguma coisa quando o usuário procurar um desses.

32.605 dos 32.629 números têm 9 dígitos. Os 24 fora do padrão (de 1 a 10 dígitos) foram testados com zero à esquerda preenchido e **nenhum passou a casar com preço**: não é zero perdido, é lixo. Eles não alcançam o catálogo, porque o recorte exige preço publicado.

Base: [`analise-dados-anvisa.md`](../analise-dados-anvisa.md) e [`analise-dados-cmed.md`](../analise-dados-cmed.md).
