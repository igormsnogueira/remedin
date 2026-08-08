# Análise — Lista de Preços de Medicamentos (CMED)

Rodada em 03/08/2026 sobre a lista publicada em 21/07/2026 (25.702 linhas). Duas perguntas: dá para ligar preço ao registro da ANVISA por chave, e quantos produtos ficam com preço.

## Fonte

A CMED é a Câmara de Regulação do Mercado de Medicamentos. Define os tetos de preço; a ANVISA publica a lista.

- **PF (Preço Fábrica)**: teto que a indústria cobra da farmácia.
- **PMC (Preço Máximo ao Consumidor)**: teto que a farmácia cobra do cliente.

Links:

- CSV: https://dados.anvisa.gov.br/dados/TA_PRECO_MEDICAMENTO.csv
- Dicionário oficial: https://dados.anvisa.gov.br/dados/TA_PRECO_MEDICAMENTO%20-%20Dicionario%20de%20Dados.pdf (cópia em `docs/fontes/`)
- Página do conjunto: https://dados.gov.br/dados/conjuntos-dados/preco-de-medicamentos-no-brasil-consumidor

Existe uma lista "Governo" (PF + PMVG) para compras públicas. Fora do escopo.

## Arquivo

15,8 MB, encoding `utf-8`, delimitador `;`, 25.702 linhas, 74 colunas.

**As 41 primeiras linhas são texto jurídico, não dados.** Ocupam só a primeira coluna, e o cabeçalho está na linha 42. Sem tratar isso o arquivo lê sem erro e sai errado: 73 das 74 colunas ficam sem nome e nenhuma chave aparece. O script acha o cabeçalho pela linha que preenche mais colunas, porque nada garante que o preâmbulo tenha sempre 41.

`SUBSTÂNCIA` tem valores com `;` dentro, entre aspas (`"21-ACETATO DE DEXAMETASONA;CLOTRIMAZOL"`). `split(';')` desalinha a linha inteira; precisa de parser de CSV.

## A chave e o cruzamento

`REGISTRO` na CMED tem 13 dígitos, `NUMERO_REGISTRO_PRODUTO` na ANVISA tem 9. Os 9 primeiros são o mesmo número; os 4 finais são apresentação e dígito verificador.

```
CMED    1705600230032
ANVISA  170560023 ----
                  └ apresentação + dígito verificador
```

| Medida | Valor |
|---|---|
| Linhas de preço que acham produto no registro | 25.691 de 25.702 (99,96%) |
| Registros distintos com preço | 8.935 |
| Apresentações por registro | 2,88 em média, 42 no máximo |

Onze linhas ficam sem par, de dois registros (`110390172` e `110390173`). Dispensa o plano B previsto na issue, que era casar por nome ou princípio ativo.

### O que muda na issue #2

Na base da ANVISA o registro é 1 para 1 com a linha, e a issue #2 concluiu que `Medicamento` não precisava de coleção de apresentações. A CMED mostra o contrário: 2,88 apresentações por registro, cada uma com preço próprio, identificadas por `CÓDIGO GGREM`. O agregado passa a ter apresentações, alimentadas pela CMED.

## Cobertura

8.933 dos 32.626 registros da ANVISA têm preço (27,4%). Esse denominador inclui os 60,2% de registros inativos, que não têm preço vigente.

Só entre os ativos: **8.225 de 10.278 têm preço, ou 80%.**

Os outros 20% precisam de tratamento na interface: mostrar o medicamento e dizer que não há preço publicado.

## As 52 colunas de preço

52 das 74 colunas são preço: 26 de PF e 26 de PMC, uma por alíquota de ICMS (de 0% a 23%), cada uma com variante ALC para a Zona Franca de Manaus, mais uma sem impostos. O preço legal depende do estado.

O MVP precisa escolher qual alíquota exibir, e isso é decisão de produto em aberto. Guardar as 52 é desperdício; guardar uma só impede busca por estado depois.

`PMC` está vazio em 3.878 linhas (15,1%), quase o mesmo conjunto de `RESTRIÇÃO HOSPITALAR = Sim` (3.884). Produto restrito a hospital não vai ao balcão, então não tem preço ao consumidor.

## Campos

| Campo | Para quê |
|---|---|
| `REGISTRO` | Chave do cruzamento com a ANVISA |
| `CÓDIGO GGREM` | Identidade da apresentação |
| `PRODUTO`, `SUBSTÂNCIA` | Busca |
| `APRESENTAÇÃO` | Dosagem e embalagem |
| `LABORATÓRIO` | Fabricante |
| `TARJA` | Exigência de receita |
| `RESTRIÇÃO HOSPITALAR` | Decide se exibe PMC |
| Uma coluna de PF e uma de PMC | Conforme a alíquota escolhida |

Ficam fora: `CNPJ` (redundante com `LABORATÓRIO`), `EAN 1/2/3`, `CAP`, `CONFAZ 87`, `ICMS 0%`, `ANÁLISE RECURSAL` (0,6% preenchida), `LISTA DE CONCESSÃO DE CRÉDITO TRIBUTÁRIO` e as 50 colunas das alíquotas não escolhidas.

`CÓDIGO GGREM` é a única coluna única do arquivo: 25.702 distintos em 25.702 linhas.

| Campo | Distribuição |
|---|---|
| `REGIME DE PREÇO` | Regulado 89,9%, Liberado 10,1% |
| `RESTRIÇÃO HOSPITALAR` | Não 84,9%, Sim 15,1% |
| `COMERCIALIZAÇÃO 2025` | Sim 50,9%, Não 49,1% |
| `TARJA` | Vermelha 51,4%, Vermelha sob restrição 20,5%, sem informação 18,2%, Sem Tarja 7,1%, Preta 2,8% |

Metade da lista tem `COMERCIALIZAÇÃO 2025 = Não`: preço publicado não garante produto na farmácia.

## Qualidade

Nenhuma linha duplicada e nenhum defeito de acentuação. O que precisa de tratamento:

- `-` no lugar de nulo em `EAN 2`, `EAN 3`, `TIPO DE PRODUTO` e `TARJA`. Sem conversão vira categoria fantasma no filtro. Na `TARJA` aparece como `- (*)` em 18,2% das linhas.
- Espaço sobrando em `EAN 1/2/3`, `APRESENTAÇÃO`, `CLASSE TERAPÊUTICA`, `TIPO DE PRODUTO` e `TARJA`. Trim antes de comparar.
- `DESTINAÇÃO COMERCIAL` é 100% vazia.
- `1018600330018` aparece em duas linhas, com `CÓDIGO GGREM` diferente. Não quebra o cruzamento, mas é inconsistência da origem.
- 24 dos 32.629 registros da ANVISA não têm 9 dígitos (variam de 1 a 10). Justifica validar o formato antes de gravar.

## Riscos e pendências

- **Alíquota de ICMS a exibir**: em aberto, trava o modelo de persistência do preço.
- **Preâmbulo de tamanho variável**: resolvido pela detecção do cabeçalho. Vale validar o número de colunas antes de processar.
- **Data de publicação** está na linha 2 do preâmbulo (`Publicada em 21/07/2026 17h30min`). Serve como versão da carga, para não reprocessar a mesma lista. Não implementado.
- **Sem data de vigência por item.** Histórico de preço, se entrar no escopo, terá que ser acumulado carga a carga.

## Como reproduzir

```bash
pip install pandas
python scripts/analyze_price.py caminho/TA_PRECO_MEDICAMENTO.csv
python scripts/check_price_registry_join.py caminho/DADOS_ABERTOS_MEDICAMENTOS.csv caminho/TA_PRECO_MEDICAMENTO.csv
```

Saída bruta em `docs/output/`, amostra de 100 linhas em `data/samples/preco-sample.csv`.
