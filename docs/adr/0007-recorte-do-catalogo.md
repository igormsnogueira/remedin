# 0007 — Recorte do catálogo e da busca

**Status:** aceita · **Data:** 08/08/2026

## Contexto

A base de registro tem 43.397 linhas, e o produto não mostra nem um quarto disso. O funil medido:

| Etapa | Restam |
|---|---|
| Linhas na base de registro | 43.397 |
| Com número de registro | 32.629 |
| Registros ativos | 10.278 |
| Ativos com preço publicado | 8.225 |
| Com apresentação de balcão | 6.894 |
| Comercializada no último ano | 5.021 |

Cada corte esconde produtos que alguém pode procurar. Um catálogo restrito aos 5.021 devolve zero resultado para quem digita o nome de um remédio que existe, e zero resultado é a pior resposta possível: o usuário conclui que o site é incompleto e não volta.

Há ainda uma discordância entre as fontes: 708 registros têm preço vigente publicado pela CMED e estão marcados como `Inativo` na ANVISA.

## Decisão

A busca indexa **os 10.278 registros ativos mais os 708 com preço vigente**, ou 10.986 medicamentos.

A ficha declara o que falta, em vez de esconder o produto:

| Situação | Quantos | O que a ficha diz |
|---|---|---|
| Completo | 5.021 | Preço, finalidade, exigência de receita |
| Sem preço na CMED | 2.053 | "Sem preço publicado pela CMED" — sem informação clínica |
| Só apresentação hospitalar | 1.331 | Preço Fábrica, com aviso de uso restrito a hospital |
| Não comercializado no último ano | 1.873 | "Sem registro de comercialização desde 2025" |
| Registro inativo com preço vigente | 708 | Preço, com aviso de que o registro consta como inativo |
| `TARJA` ausente | 21,4% do catálogo | "Exigência de receita não informada" |

Os 24 números de registro fora do padrão de 9 dígitos ficam de fora: foi testado preencher com zero à esquerda e nenhum passou a casar com preço, então são dado corrompido, não zero perdido.

## Por quê

Achar o medicamento e descobrir que falta preço é informação útil. Não achar não é.

O produto se apresenta como derivado de fontes oficiais, então expor a discordância entre elas — em vez de escolher uma em silêncio — é mais honesto e mais defensável. O usuário decide o que fazer com a informação de que a CMED publica preço para um registro que a ANVISA dá como inativo.

Os rótulos também protegem contra a decepção mais provável do produto: alguém encontra um medicamento descontinuado com preço e conclui que o site está errado.

## Consequências

A busca opera sobre 10.986 registros, e a ordenação precisa favorecer os completos. Resultado sem preço aparece depois de resultado com preço, para o mesmo grau de relevância.

Cada estado do funil vira uma coluna ou flag na projeção de leitura, calculada na carga. A ficha não deduz nada em tempo de consulta.

A interface tem seis mensagens de ausência para escrever, e elas precisam ser claras para leitor com pouca familiaridade. É trabalho de texto, não de código.

Os 5.021 completos são o recorte para navegação por finalidade e para "alternativas mais baratas com o mesmo princípio ativo", porque essas duas funcionalidades dependem de preço.
