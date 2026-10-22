# 0012 — Guardar apenas os preços que o catálogo serve

**Status:** aceita · **Data:** 12/10/2026

## Contexto

O preço-teto depende do ICMS estadual, e a CMED resolve isso publicando a tabela já calculada para cada alíquota possível: 0, 12, 17, 17,5, 18, 19, 19,5, 20, 20,5, 21, 22, 22,5 e 23 por cento, mais "sem impostos", e cada uma dessas repetida para preço de fábrica, preço ao consumidor e as variantes de área de livre comércio. São 52 colunas de preço por linha do arquivo.

Guardando tudo, o banco chegou a 842 MB, dos quais 791 MB são a tabela de preços com 1.199.220 linhas.

O catálogo não pergunta a alíquota, pergunta o estado. As 27 unidades da federação usam quatro alíquotas distintas, e são as únicas que alguma consulta pede:

| O que está guardado | Linhas | Alcançável pelo site |
|---|---|---|
| 17, 17,5, 18 e 20 por cento, fora de zona franca | 184.028 | sim |
| Demais alíquotas | 920.140 | não |
| Zona franca | 46.007 × 11 | não |
| Sem impostos e 0 por cento | 95.052 | não |

As linhas de "sem impostos" chegam com alíquota nula, e as consultas comparam `icms_rate = @rate` — em SQL, `NULL = 18` não é verdadeiro, então elas são inalcançáveis por construção.

## Decisão

A carga guarda apenas os preços cuja alíquota pertence à tabela de estados, e descarta as linhas de área de livre comércio.

O conjunto de alíquotas é derivado de `IcmsRates`, e não escrito à parte.

O descarte acontece na leitura do arquivo, antes de montar as entidades.

## Por quê

Oitenta e cinco por cento da maior tabela do banco era dado que nenhuma linha de código alcança. Isso não é reserva para o futuro: é custo de armazenamento, de carga e de backup por informação que o produto não oferece.

O limite prático apareceu no deploy. Nenhum plano gratuito de PostgreSQL passa de 1 GB, e o projeto é uma prova de conceito sem orçamento.

Medido depois do recorte e de compactar o banco: 190.768 linhas de preço e 66 MB no total. Parte da diferença para os 842 MB iniciais era espaço morto de cargas anteriores, que o PostgreSQL só devolve com `VACUUM FULL`. Com as cargas mensais reaproveitando o espaço liberado, o tamanho deve estabilizar perto do dobro do medido.

Derivar o conjunto da tabela de estados, em vez de listar as quatro alíquotas num segundo lugar, é o que impede as duas listas de divergirem. Quando um estado mudar de alíquota, a tabela muda e a carga passa a guardar a nova sozinha.

Filtrar na leitura e não na gravação evita montar 1,2 milhão de objetos em memória para descartar a maioria. O processo de carga roda num executor gratuito, onde memória é recurso contado.

## Consequências

Se um estado mudar de alíquota, o preço daquele estado fica indisponível até a carga seguinte rodar. A carga é mensal, então a janela é de até um mês. A alternativa seria guardar todas as alíquotas por precaução, que é o que se está desfazendo aqui.

Preço de área de livre comércio deixa de existir no banco. Oferecer isso exigiria o site perguntar o município em vez do estado, o que é outra funcionalidade e não uma consulta a mais.

Não há histórico: a carga substitui a tabela inteira. Quem quiser o preço de uma alíquota descartada roda a carga a partir do arquivo da CMED, que continua público.

O leitor da CMED continua lendo as 52 colunas. O recorte é de armazenamento, não de interpretação do arquivo — e o teste que garante a leitura das colunas de zona franca continua valendo.
