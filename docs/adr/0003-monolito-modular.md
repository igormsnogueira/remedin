# 0003 — Monolito modular

**Status:** aceita · **Data:** 08/08/2026

## Contexto

O sistema carrega três fontes públicas e responde buscas do usuário.

Registro e preço são dois CSV do mesmo domínio, com 43.397 e 25.702 linhas, atualizados uma vez por mês. A carga leva segundos.

O deploy é em um servidor, com um PostgreSQL.

## Decisão

Monolito modular, com duas unidades deployáveis:

- `Remedin.Api` — busca e ficha do medicamento
- `Remedin.Worker` — execução das cargas

As camadas ficam em projetos separados, conforme a [ADR 0004](0004-clean-architecture-e-cqrs.md).

Os contextos delimitados — catálogo, preço, bula — são pastas dentro de cada camada, não projetos separados.

API e worker são separados porque a carga é agendada, não deve ser disparada por tráfego web, e precisa poder rodar sob demanda para reprocessar. Também é o que dá isolamento de falha: uma carga travada não afeta a busca, porque são processos diferentes.

## Por quê

Separar as ingestões em serviços próprios não se paga neste volume:

- As duas fontes são CSV do mesmo domínio, com a mesma cadência mensal. O evento que faz mexer num ETL faz mexer no outro.
- Elas compartilham o parser de CSV, a validação de arquivo e o objeto de valor `NumeroRegistro`.
- Serviços separados escreveriam nas mesmas tabelas do mesmo banco, o que elimina o deploy independente e deixa só o custo de operar processos separados.
- Com um servidor e um PostgreSQL, a disputa por recurso acontece no banco, não entre processos.

## Consequências

Perde-se deploy e escala independentes por fonte. Nenhum dos dois é necessário no volume atual.

Ganha-se uma unidade de build, uma migration, um log e um ambiente local.

A fronteira entre as camadas passa a ser verificada por teste, não apenas desenhada.

## Quando extrair um serviço

1. **Carga degradando a latência da busca**, medido e não suposto.
2. **Mais de uma pessoa no projeto**, com necessidade de deploy sem coordenação.
3. **Uma fonte com perfil de obtenção diferente das atuais**, como é o caso da bula.
