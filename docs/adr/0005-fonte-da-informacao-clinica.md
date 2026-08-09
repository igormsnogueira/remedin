# 0005 — Fonte da informação clínica

**Status:** aceita · **Data:** 08/08/2026

## Contexto

O produto promete três coisas além do preço: para que o medicamento serve, se ele exige receita, e quais são as alternativas com o mesmo princípio ativo.

A hipótese inicial era que isso viria da bula, obtida por crawl do Bulário Eletrônico. O perfil da lista da CMED mostra que as três já estão lá, preenchidas em 100% das 25.702 linhas:

| Informação | Campo | Preenchimento | Formato |
|---|---|---|---|
| Exige receita | `TARJA` | 100% | 5 valores, definidos no dicionário oficial |
| Para que serve | `CLASSE TERAPÊUTICA` | 100% | 540 valores, código EphMRA hierárquico |
| Princípio ativo | `SUBSTÂNCIA` | 100% | 2.251 valores |

`SUBSTÂNCIA` é o campo antes chamado `PRINCÍPIO ATIVO`, conforme a tabela de/para do dicionário. É melhor que o `PRINCIPIO_ATIVO` da base de registro, que está em 67,4%.

`CLASSE TERAPÊUTICA` vem como `CÓDIGO - DESCRIÇÃO` em 100% dos casos (`R5C - EXPECTORANTES`). A primeira letra é o grupo anatômico, o que permite navegar em árvore: 540 folhas agrupadas em cerca de 14 categorias.

## Decisão

A informação clínica do MVP vem da lista da CMED.

A ficha do medicamento traz um **link para a bula oficial no Bulário**, sem baixar nem armazenar o PDF.

A navegação por finalidade usa a hierarquia da classe terapêutica, com um mapeamento curado de código para linguagem simples ("M2A" para "dor muscular e articular"). O mapeamento é dado do projeto, versionado no repositório.

A ingestão de bula fica fora do caminho crítico da entrega.

## Por quê

A classe terapêutica responde o que o medicamento é: analgésico, antiácido, anti-histamínico. É o suficiente para "para que serve" e para navegar por finalidade, que é a forma de uso mais provável do público-alvo.

A bula responderia mais: indicação específica, com sintoma nomeado. Ela é necessária para busca por sintoma, e isso continua sendo o passo seguinte do produto. Mas não é pré-requisito de nenhuma das três promessas atuais, e o custo dela — crawl de milhares de PDFs, extração, possível OCR, armazenamento — não cabe no caminho crítico junto com ETL, busca, front-end e deploy.

Vale notar que a bula também não entregaria busca por sintoma sozinha: a bula escreve "cefaleia" e o cidadão digita "dor de cabeça". Os dois caminhos precisam de uma camada de sinônimos curada; o da bula precisa do crawl **além** dela.

## Consequências

"Para que serve" fica em nível de categoria, não de indicação. `M1A1 - ANTIRREUMÁTICOS NÃO ESTEROIDAIS PUROS` vira "anti-inflamatório para dor e inflamação", e não a lista de condições da bula.

Busca por sintoma fica fora do MVP. É a funcionalidade que mais pede enquadramento cuidadoso — o resultado precisa ser apresentado como "medicamentos cuja indicação menciona este termo", nunca como recomendação — e não deve estrear numa entrega apertada.

O mapeamento de classe para linguagem simples é trabalho manual, estimado em 540 termos. As categorias mais frequentes cobrem a maior parte do catálogo, então dá para começar pelas 80 principais.

Medicamento sem preço publicado fica sem informação clínica, porque a fonte é a lista da CMED. São 2.053 dos 10.278 registros ativos.

"Exige receita" não cobre o catálogo inteiro: dentro do catálogo efetivo, a `TARJA` vem como `- (*)` em 21,4% das linhas. A ficha informa que o dado não está disponível, em vez de omitir o campo.

## Quando reabrir

Quando a busca por sintoma entrar no escopo. Aí a ingestão de bula volta como job agendado no worker, com checkpoint por número de registro, PDF em disco e texto extraído no mesmo banco do catálogo — a extração é tratável porque a bula do paciente tem seções obrigatórias com títulos fixos, definidas pela RDC 47/2009.
