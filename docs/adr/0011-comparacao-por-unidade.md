# 0011 — Comparação de preço por unidade

**Status:** aceita · **Data:** 05/09/2026

## Contexto

A lista de alternativas da ADR 0010 mostra preço de caixa. Para OMEPRAZOL 20 MG isso colocava uma caixa de 7 cápsulas por R$ 9,60 acima de uma de 28 por R$ 24,99, sugerindo que a primeira é mais barata. Por cápsula, R$ 1,37 contra R$ 0,89.

Dosagem e quantidade não têm campo próprio na CMED. Estão dentro da descrição da apresentação: `20 MG CAP DURA CT BL AL PLAS X 28`.

O padrão é regular na maior parte dos casos, com armadilhas que produzem número errado em silêncio:

| Texto | Leitura ingênua | O que é |
|---|---|---|
| `CT 25 AMP VD AMB X 1ML` | 1 unidade | volume de cada ampola |
| `CT BG AL X 40G` | 40 unidades | peso do tubo |
| `50 MG/ML SOL OR` | 50 mg por dose | concentração do líquido |
| `(0,5 + 0,1) MG COM REV` | 0,5 mg | dose combinada, sem valor único |
| `X 30 (EMB FRAC)` | 30 unidades | embalagem fracionada |

## Decisão

Ler dosagem e quantidade da descrição na carga, com duas regras estreitas:

- dosagem: número em MG no começo do texto, seguido de espaço ou fim. `MG/ML` e `MG/G` não casam.
- quantidade: número após `X` no fim do texto, sem nada depois.

Quando qualquer uma falha, o resultado é ausência, e não estimativa.

O preço por unidade só é publicado quando a quantidade é conhecida. A economia só é calculada entre apresentações de mesma dosagem, com as duas quantidades conhecidas, e quando a alternativa é de fato mais barata.

A ordenação da lista põe a mesma dosagem primeiro, depois o menor preço por unidade.

## Por quê

Regra estreita erra para o lado da ausência. Ler `X 40G` como 40 unidades daria preço por unidade 40 vezes menor que o real, com aparência de dado correto.

A restrição de dosagem no cálculo de economia é o mesmo raciocínio da ADR 0010: 10 MG e 40 MG do mesmo princípio ativo não são a mesma coisa. Sem ela, o endpoint chegou a informar economia de R$ 140,76 comparando embalagens diferentes de dosagens diferentes.

Os valores são gravados na carga porque a leitura é regra de domínio. Reescrevê-la em SQL criaria duas versões da mesma coisa, que divergiriam.

## Consequências

14.559 das 25.691 apresentações têm os dois valores. As outras 11.132 são líquidos, pomadas, injetáveis e embalagens fracionadas: aparecem na lista com preço de caixa e sem preço por unidade.

A comparação continua limitada a formas sólidas. Xarope contra xarope precisaria de volume e concentração, que é outra leitura.

`dosage_mg` e `unit_count` existem no banco e não no modelo do EF: só o SQL da consulta de alternativas as lê, como já acontece com a coluna de busca. A migration é escrita à mão.
