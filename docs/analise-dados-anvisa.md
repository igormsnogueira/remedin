# Análise — Base de Medicamentos Registrados (ANVISA)

Rodada em 02/08/2026 sobre o CSV baixado no mesmo dia (43.397 linhas). Pergunta: o que essa base sustenta, antes de modelar o domínio.

## Fonte

Medicamentos Registrados no Brasil, da ANVISA, extraída do sistema Datavisa. Atualização mensal. Não inclui produtos notificados nem Cannabis.

- CSV: https://dados.anvisa.gov.br/dados/DADOS_ABERTOS_MEDICAMENTOS.csv
- Dicionário oficial: https://dados.anvisa.gov.br/dados/Documentacao_e_Dicionario_de_Dados_Registros_Validos_Medicamento_V1.pdf (cópia em `docs/fontes/`)

## Arquivo

7,9 MB, encoding `latin1`, delimitador `;`, 43.397 linhas, 11 colunas. O ETL precisa declarar `latin1`; lendo como UTF-8 a acentuação quebra.

## Campos

| Campo | Para quê |
|---|---|
| `NUMERO_REGISTRO_PRODUTO` | Chave, ver seção abaixo |
| `NOME_PRODUTO`, `PRINCIPIO_ATIVO` | Busca |
| `SITUACAO_REGISTRO` | Filtra o que está em vigência |
| `CATEGORIA_REGULATORIA` | Define escopo |
| `EMPRESA_DETENTORA_REGISTRO` | Fabricante |
| `CLASSE_TERAPEUTICA` | Navegação por categoria |

Ficam fora:

- `DATA_FINALIZACAO_PROCESSO` e `NUMERO_PROCESSO`: trâmite interno da ANVISA.
- `TIPO_PRODUTO`: um valor só em toda a base.
- `DATA_VENCIMENTO_REGISTRO`: não filtra busca nem entra no cruzamento. Formato `MMAAAA` sem separador e 14% nulo, o que exigiria parser e regra de exibição para pouco retorno na Fase 1.

## Situação do registro

26.141 linhas em Inativo (60,2%), 17.256 em Ativo (39,8%).

O dicionário (seção 3.1) diz que a base traz só registros válidos e nem lista `SITUACAO_REGISTRO` na tabela de campos, o que deixa em aberto o que "Inativo" significa.

O catálogo da ANVISA no dados.gov.br resolve: os outros conjuntos do mesmo Datavisa (Saneantes, Cosméticos, Produtos Fumígenos, Cannabis) dizem na descrição que a base inclui registro ativo ou inativo, verificável por esse campo. O "válido" do PDF deve significar processo legítimo no sistema, e não produto em vigência.

Decisão: mostrar só Ativo por padrão e guardar Inativo marcado, para avisar quando um produto sai do mercado. Confirmar com um produto que se sabe fora de linha antes de fechar a regra.

## A chave: NUMERO_REGISTRO_PRODUTO

Duas perguntas decidem o desenho do agregado `Medicamento`: quantas linhas não têm número, e o que muda quando ele se repete.

### 25% da base não tem número

10.768 das 43.397 linhas estão com o campo vazio. Hipótese testada: certas categorias regulatórias não recebem número.

| Categoria | Linhas | % sem registro |
|---|---|---|
| BAIXO RISCO | 7.112 | 100% |
| DINAMIZADO (maiúsculo) | 3.456 | 100% |
| Gases Medicinais | 96 | 100% |
| FITOTER?PICO (grafia quebrada) | 86 | 100% |
| Radiofármaco | 69 | 24,6% |
| Dinamizado (minúsculo), Fitoterápico e outras 8 | — | 0% |

A hipótese cai por dois motivos:

1. Radiofármaco fica em 24,6%. Se a categoria decidisse, seria 0% ou 100%.
2. `DINAMIZADO` e `Dinamizado` são a mesma categoria escrita de dois jeitos, com resultados opostos.

O padrão aponta para um lote antigo com dois defeitos juntos: categoria corrompida (`FITOTER?PICO`, tudo maiúsculo) e número de registro ausente. Mesma origem ruim.

**Regra do MVP:** entra quem tem `NUMERO_REGISTRO_PRODUTO` preenchido. Cobre o caso do Radiofármaco sem manter lista de categorias.

### O número se repete?

32.629 linhas preenchidas para 32.626 valores distintos: sobram 3 linhas. O caso que olhei (`116540035`, produto BIMOXIN) tem 4 linhas com o mesmo número, idênticas em todas as colunas — ou seja, 3 linhas excedentes. Esse único caso explica a diferença inteira: **exatamente um número de registro se repete em toda a base**, e a repetição é linha duplicada no arquivo, não produto diferente.

Nesta base, cada linha válida é um produto completo. As apresentações existem, mas vêm da CMED: 2,88 por registro, cada uma com preço próprio (ver `analise-dados-cmed.md`). O agregado terá apresentações; elas não vêm daqui.

### Formato do número

9 dígitos, sem ponto nem traço (`102980592`). O dicionário não especifica formato, só diz "Número identificador do Registro do Produto".

A seção 6 avisa que campos com zero à esquerda perdem o zero ao abrir o arquivo no Excel, o que confirma que existem campos assim na base. Por isso o script lê tudo como texto (`dtype=str`). Sem confirmação do tamanho, não dá para fixar 9 dígitos na validação do objeto de valor `NumeroRegistro`.

## Qualidade

- 3.438 linhas duplicadas por inteiro (7,9%). Deduplicar antes de gravar.
- Espaço sobrando em `NOME_PRODUTO`, `DATA_VENCIMENTO_REGISTRO`, `CLASSE_TERAPEUTICA`, `EMPRESA_DETENTORA_REGISTRO` e `PRINCIPIO_ATIVO`. Trim na transformação: nenhuma delas é chave de cruzamento, mas espaço sobrando quebra agrupamento, deduplicação e comparação de texto, e aparece na tela.
- 24 dos 32.629 números de registro não têm 9 dígitos (variam de 1 a 10). São 0,07% da base. Como o cruzamento com preço usa 9 dígitos, esses 24 nunca casam com preço de qualquer forma.
- Letra acentuada trocada por `?` (`FITOTER?PICO`, `N?O DECLARADO`) em `CATEGORIA_REGULATORIA` e `NOME_PRODUTO`. O caractere se perdeu antes da publicação, então nenhum encoding recupera. Normalizar os casos frequentes com dicionário fixo.

Não há mojibake — o defeito em que um acento vira dois caracteres, tipo `Ã§` no lugar de `ç`. Ele só aparece lendo o arquivo com o encoding errado.

## Riscos e pendências

- Formato do `NUMERO_REGISTRO_PRODUTO` não confirmado nem pelo dicionário. Ler como texto resolve por ora.
- `SITUACAO_REGISTRO = Inativo` como registro cancelado ou caduco está indicado pelos datasets irmãos, mas não confirmado para este.
- Valores com `?` chegam na tela do usuário se ninguém tratar.

## Como reproduzir

```bash
pip install pandas
python scripts/analyze_anvisa.py caminho/DADOS_ABERTOS_MEDICAMENTOS.csv
```

Saída bruta em `docs/output/`, amostra de 100 linhas em `data/samples/medicamentos-sample.csv`.
