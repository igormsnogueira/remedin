# 0009 — Fonte de verdade por campo

**Status:** aceita · **Data:** 24/08/2026

## Contexto

As duas fontes descrevem o mesmo medicamento e discordam em vários campos.

| Campo | ANVISA | CMED |
|---|---|---|
| Nome | 21.175 valores distintos | 5.934 valores distintos |
| Princípio ativo | 67,4% preenchido, texto livre | 100% preenchido |
| Classe terapêutica | Sem código, sem acento (`ANTIGLAUCOMATOSOS`) | Com código padronizado (`S1E - ANTIGLAUCOMATOSOS`) |
| Exigência de receita | Não existe | `TARJA`, em 78,6% do catálogo |
| Fabricante | Detentor do registro, com CNPJ no valor | Laboratório que comercializa |

Sem regra escrita, cada carga sobrescreveria o que a outra gravou, e o resultado dependeria da ordem de execução.

## Decisão

| Campo | Fonte |
|---|---|
| Nome do produto | ANVISA |
| Fabricante | ANVISA |
| Situação do registro | ANVISA |
| Princípio ativo | CMED, com a ANVISA de reserva |
| Classe terapêutica | CMED, com a ANVISA de reserva |
| Exigência de receita | CMED |

A carga de preço atualiza apenas os campos da coluna CMED, e nunca apaga um valor existente quando a CMED não publica aquele campo.

## Por quê

**A ANVISA manda na identidade.** Nome e detentor do registro são o ato regulatório; a CMED lista quem comercializa, o que é informação de mercado. Se a CMED nomeia o produto de outro jeito, quem está certo é o registro.

**A CMED manda no que é clínico.** Ela preenche princípio ativo em 100% contra 67,4%, e traz a classe terapêutica com código padronizado em vez de texto solto. A exigência de receita só existe nela.

**Reserva em vez de substituição** porque os dois recortes não coincidem: 2.053 registros ativos não aparecem na lista de preço, e para eles o dado da ANVISA é tudo o que existe. Sobrescrever com nulo apagaria informação boa.

## Consequências

A ordem das cargas passa a importar por um segundo motivo: além de o preço depender do registro para achar o medicamento, ele também completa campos que a carga anterior deixou vazios.

A regra vive na carga de preço, num lugar só. Quem ler o comando de importação vê qual campo vem de onde.

O índice de busca é recalculado sozinho, porque a coluna que o alimenta é gerada pelo banco a partir desses campos.

Medicamento sem preço fica com a classe terapêutica sem código e sem tarja. É o mesmo recorte de 20% já declarado na ADR 0005, e a ficha informa a ausência em vez de escondê-la.
