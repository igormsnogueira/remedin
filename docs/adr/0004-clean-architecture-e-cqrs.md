# 0004 — Clean Architecture com CQRS

**Status:** aceita · **Data:** 08/08/2026

## Contexto

O sistema tem dois tipos de operação com requisitos opostos.

A escrita vem de cargas agendadas: importar o CSV de registro, importar a lista de preços, extrair bulas. É lote, tolera latência, e precisa validar invariantes de domínio antes de gravar.

A leitura vem da busca do usuário: frequente, sensível a latência, e melhor servida por dado desnormalizado — nome, princípio ativo, fabricante e preço na mesma linha.

## Decisão

Clean Architecture em quatro camadas, com a regra de dependência apontando sempre para dentro:

```
Remedin.Domain          Entidades, objetos de valor, regras. Sem dependência.
Remedin.Application     Casos de uso e interfaces de infraestrutura.  → Domain
Remedin.Infrastructure  EF Core, HTTP, parsers, storage.              → Application, Domain
Remedin.Api             Composição e entrada HTTP.                    → todas
Remedin.Worker          Composição e agendamento das cargas.          → todas
```

`Domain` e `Application` não conhecem EF Core, PostgreSQL nem `HttpClient`. As interfaces (`IRegistrySource`, `IMedicineRepository`) são declaradas em `Application` e implementadas em `Infrastructure`.

CQRS na camada de aplicação, com handlers separados por fluxo:

| | Comandos | Consultas |
|---|---|---|
| Exemplos | `ImportRegistrySnapshot`, `ImportPriceList` | `SearchMedicines`, `GetMedicineDetail` |
| Disparado por | `Remedin.Worker`, por agendamento | `Remedin.Api`, por requisição |
| Passa pelo domínio | Sim, valida invariantes | Não |
| Modelo | Agregado `Medicamento` | Projeção desnormalizada com `tsvector` |

## Por quê

A separação de camadas mantém o domínio testável sem banco e sem rede, e torna a troca de infraestrutura um problema local. A regra de dependência é verificada por teste de arquitetura: o build quebra se `Domain` referenciar `Infrastructure`.

CQRS se aplica porque os dois lados divergem de verdade. Usar o agregado na busca traria junções e validações inúteis para leitura; otimizar o agregado para busca sujaria o domínio com preocupação de exibição.

Comando aqui não nasce de usuário, e sim do agendador. Isso não descaracteriza o padrão: o que ele separa é o modelo de escrita do modelo de leitura, não a origem da requisição.

## Consequências

Mais projetos e mais indireção do que um CRUD exigiria. É custo assumido em troca de fronteira verificável.

Dois modelos para manter: mudança no domínio pode exigir mudança na projeção.

A projeção fica desatualizada entre a carga e a atualização dela. Aceitável, porque as fontes mudam uma vez por mês.

## Fora do escopo

**Event Sourcing.** Costuma aparecer junto de CQRS, mas é decisão separada. A fonte da verdade aqui são os arquivos públicos, e reprocessá-los reconstrói o estado. Não há histórico de eventos de domínio a preservar.

**Banco de leitura separado.** A projeção é uma tabela no mesmo PostgreSQL. Separar bancos exigiria sincronização assíncrona, que é complexidade sem problema correspondente neste volume.
