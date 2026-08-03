# Análise — Base de Medicamentos Registrados (ANVISA)

Executada em 02/08/2026, sobre o CSV baixado no mesmo dia (43.397 linhas). Objetivo: decidir o que essa base sustenta antes de modelar o domínio.

## Fonte

Medicamentos Registrados no Brasil, ANVISA/Datavisa. Atualização mensal. Não inclui notificados nem Cannabis.

- CSV: https://dados.anvisa.gov.br/dados/DADOS_ABERTOS_MEDICAMENTOS.csv
- Dicionário oficial: https://dados.anvisa.gov.br/dados/Documentacao_e_Dicionario_de_Dados_Registros_Validos_Medicamento_V1.pdf (cópia local em `docs/fontes/`)

O dicionário oficial (seção 3.1) diz que a base traz **só registros válidos**, sem petição de cancelamento. Isso não bate com o que os dados mostram — ver "Situação do registro" abaixo.

## Arquivo

7,9 MB, encoding `latin1`, delimitador `;`, 43.397 linhas, 11 colunas. O ETL precisa declarar `latin1` explicitamente. Se ler como UTF-8, a acentuação quebra.

## Campos que entram no MVP

Entram: `NUMERO_REGISTRO_PRODUTO` (chave, ver abaixo), `NOME_PRODUTO` e `PRINCIPIO_ATIVO` (busca), `SITUACAO_REGISTRO` (ativo/inativo — significado exato do "inativo" ainda em aberto, ver abaixo), `CATEGORIA_REGULATORIA` (decide escopo, ver abaixo), `EMPRESA_DETENTORA_REGISTRO` (fabricante), `CLASSE_TERAPEUTICA` (navegação por categoria).

Ficam fora: `DATA_FINALIZACAO_PROCESSO` e `NUMERO_PROCESSO` (trâmite interno da ANVISA, sem uso pro usuário final), `TIPO_PRODUTO` (um valor único em toda a base), `DATA_VENCIMENTO_REGISTRO` (decisão de escopo: não filtra busca nem entra no cruzamento com a CMED, é só informativo; formato `MMAAAA` sem separador e 14% nulo, exigiria parser e regra de exibição pra pouco ganho na Fase 1).

## Situação do registro

60,2% inativo (26.141 linhas), 39,8% ativo (17.256).

O dicionário oficial (seção 3.1) diz que só são publicados registros válidos, sem petição de cancelamento — e não documenta `SITUACAO_REGISTRO` na tabela de campos. Isolado, isso levantava dúvida sobre o que "Inativo" significa.

Catálogo de dados da ANVISA (dados.gov.br) resolve a dúvida: os outros conjuntos "Registrados no Brasil" que seguem o mesmo padrão do Datavisa — Saneantes, Cosméticos, Produtos Fumígenos, Cannabis — descrevem explicitamente que a base "inclui registros já ativo ou inativo" (ou "válido ou cancelado/caduco"), e que isso se verifica pelo campo `SITUACAO_REGISTRO`. É o mesmo sistema, o mesmo padrão de dataset, só produto diferente. Interpretação mais provável: o "válido" da seção 3.1 do PDF quer dizer "processo legítimo no sistema", não "em vigência hoje" — e `Inativo` é mesmo registro cancelado/caduco, produto fora do mercado.

Mantida a proposta original: catálogo mostra só ativo por padrão, guarda inativo marcado como tal pra avisar quando um produto saiu do mercado. Ainda vale testar com um produto que eu sei que saiu de linha antes de travar de vez, mas não é mais bloqueio pra seguir.

## A chave: NUMERO_REGISTRO_PRODUTO

Essa seção decide o agregado da issue #8: um `Medicamento` vai nascer com uma lista de `Apresentacao` dentro dele (dosagens, embalagens diferentes do mesmo produto), ou cada linha da base já é um produto completo sozinho? A resposta depende de duas coisas: quantas linhas não têm número de registro, e o que muda entre as linhas onde o número se repete.

### Por que 25% da base não tem número de registro

10.768 das 43.397 linhas (25%) não têm `NUMERO_REGISTRO_PRODUTO` preenchido. Primeira hipótese: será que certas categorias regulatórias simplesmente não recebem número de registro? Cruzei com `CATEGORIA_REGULATORIA`:

| Categoria | Total de linhas | % sem registro |
|---|---|---|
| BAIXO RISCO | 7.112 | 100% |
| Gases Medicinais | 96 | 100% |
| DINAMIZADO (maiúsculo) | 3.456 | 100% |
| FITOTER?PICO (grafia quebrada) | 86 | 100% |
| Radiofármaco | 69 | 24,6% |
| Dinamizado (minúsculo), Fitoterápico e as outras 8 categorias | — | 0% |

Essa hipótese não se sustenta, por dois motivos:

1. Radiofármaco está em 24,6% — nem 0%, nem 100%. Se a categoria decidisse sozinha, ou todo Radiofármaco teria registro, ou nenhum teria.
2. A mesma categoria aparece escrita de duas formas com resultado oposto: `DINAMIZADO` (100% sem registro) e `Dinamizado` (0% sem registro) são a mesma coisa, só a grafia muda.

Explicação mais provável: existe um lote de dados antigo com dois defeitos ao mesmo tempo — texto de categoria corrompido (`FITOTER?PICO`, tudo maiúsculo) e número de registro ausente. Os dois vêm da mesma origem ruim; a categoria não é a causa, só é onde o defeito aparece com mais força.

### Regra de filtro do MVP

Por isso o filtro não é "excluir estas categorias". É mais simples e mais correto: entra quem tem `NUMERO_REGISTRO_PRODUTO` preenchido, sai quem não tem. Essa regra sozinha já cobre o caso Radiofármaco, sem precisar manter uma lista de categorias.

### O número de registro se repete?

43.397 − 10.768 = 32.629 linhas com número preenchido. Dessas, só 3 valores se repetem — o resto é único. O único exemplo de repetição (`116540035`, produto BIMOXIN, 4 linhas) tem as 4 linhas idênticas em todas as colunas: é a mesma linha duplicada no arquivo, não são 4 apresentações diferentes do mesmo produto.

Conclusão: o número de registro é praticamente 1 para 1 com a linha. Um `Medicamento` não precisa nascer com uma lista de `Apresentacao` — cada linha válida já representa um produto completo. Se apresentações diferentes existirem de fato, essa base não mostra isso; só vai aparecer com os dados da bula (Fase 2).

### Formato do número

Os dois exemplos vistos (`102980592`, `116540035`) têm 9 dígitos, sem ponto nem traço. Fui checar o dicionário oficial e ele não especifica o formato — a tabela de campos só diz "Número identificador do Registro do Produto", sem número de dígitos nem regra de zero à esquerda.

Uma coisa o dicionário confirma, ainda que indireta: a seção 6 avisa que campos com zero à esquerda viram número inteiro (perdendo o zero) se o arquivo for aberto direto no Excel — ou seja, a própria ANVISA sabe que a base tem campos assim. Por segurança, o script já lê tudo como string (`dtype=str`), o que evita esse problema mesmo sem saber o tamanho exato do campo. O formato exato continua em aberto; se importar pra normalização do VO, vale abrir chamado ou procurar exemplo com zero à esquerda antes de assumir 9 dígitos fixos.

## Qualidade dos dados

3.438 linhas duplicadas inteiras (7,9% da base). Dedup no ETL antes do upsert.

Espaço sobrando no início/fim em `NOME_PRODUTO`, `DATA_VENCIMENTO_REGISTRO`, `CLASSE_TERAPEUTICA`, `EMPRESA_DETENTORA_REGISTRO`, `PRINCIPIO_ATIVO`. Trim na transformação, senão quebra o cruzamento por chave.

Acentuação quebrada, de duas formas diferentes:

- Mojibake recuperável (`Ã§`, `Ã£`) em `NOME_PRODUTO`, `CLASSE_TERAPEUTICA`, `EMPRESA_DETENTORA_REGISTRO`. Resolve declarando `latin1` certo no ETL.
- Substituição por `?` (`FITOTER?PICO`, `N?O DECLARADO`) em `CATEGORIA_REGULATORIA` e `NOME_PRODUTO`. Já veio assim da origem, não tem conserto de encoding. Dá pra normalizar os valores mais comuns com um dicionário fixo.

## Riscos e pendências

- Formato exato do `NUMERO_REGISTRO_PRODUTO` não confirmado, nem pelo dicionário oficial. Mitigado no código (`dtype=str`), mas vale confirmar tamanho antes de travar a regra de normalização do VO.
- Significado de `SITUACAO_REGISTRO = Inativo` está bem indicado (registro cancelado/caduco) pelo padrão dos outros datasets da ANVISA, mas não 100% confirmado pra este dataset específico — testar com um produto conhecido antes de travar de vez a regra de exibição.
- Strings com `?` aparecem pro usuário final se não forem tratadas na UI.

## Como reproduzir

```bash
pip install pandas
python scripts/analyze_anvisa.py caminho/DADOS_ABERTOS_MEDICAMENTOS.csv
```

Gera amostra de 100 linhas em `data/samples/medicamentos-sample.csv`.
