# Runbook

Como operar o Remedin publicado. Organizado por situação: procure o que está acontecendo, não a tecnologia.

Para rodar na máquina local, veja a seção "Rodando localmente" do [README](../README.md).

## Onde cada peça roda

| Peça | Onde | Endereço |
|---|---|---|
| Interface | Render, site estático | https://remedin.onrender.com |
| API | Render, Docker, região Ohio | https://remedin-api.onrender.com |
| Banco | Neon, PostgreSQL 18, região us-east-2 (Ohio) | painel do Neon |
| Carga de dados | GitHub Actions, toda segunda às 06:17 | aba Actions → Carga de dados |
| Migrations | GitHub Actions, disparo manual | aba Actions → Migrations |
| Verificação diária | GitHub Actions, todo dia às 08:43 | aba Actions → Verificação de produção |

A configuração dos dois serviços do Render está em [`render.yaml`](../render.yaml). Mudar o arquivo e mergear na `main` é o que muda o serviço.

A string de conexão do banco existe em dois lugares, com o mesmo valor:

- GitHub: secret `DATABASE_CONNECTION`, usado pela carga e pelas migrations
- Render: variável `ConnectionStrings__Remedin` do serviço `remedin-api`

O formato é o do Npgsql, e não a URL que o Neon mostra:

```
Host=...neon.tech;Database=neondb;Username=neondb_owner;Password=...;SSL Mode=Require;Channel Binding=Require
```

## Publicar uma mudança

Merge na `main`. A CI roda, e quando passa o Render publica sozinho o que mudou: a API se o código .NET ou o `Dockerfile` mudaram, o front se `src/Remedin.Web` mudou.

**Se a mudança tem migration**, rode o workflow Migrations logo depois do merge. O Render publica a API em paralelo, e por alguns minutos a versão nova pode estar no ar com o schema antigo. Para isso não quebrar nada, a migration precisa ser compatível com a versão anterior da API: acrescentar coluna pode, apagar ou renomear coluna que a versão no ar usa não pode. Quando for preciso, divida em duas entregas — primeiro a API deixa de usar a coluna, depois a migration remove.

## Aplicar migration

Actions → Migrations → Run workflow. Aplica só o que falta; rodar com o banco em dia não faz nada.

## Carga de dados

Roda sozinha toda segunda. Quando a fonte não publicou arquivo novo, a execução termina como `Skipped` sem gravar nada — é o normal na maioria das semanas.

**Forçar uma carga**, por exemplo depois de mudar a regra de leitura: Actions → Carga de dados → Run workflow, marcando "reprocessar".

**Histórico**, além da aba Actions:

```sql
SELECT source, outcome, rows_read, accepted, started_at
FROM ingestion_runs
ORDER BY started_at DESC
LIMIT 10;
```

**Se a carga falhou**, o GitHub manda e-mail e os dados anteriores continuam no ar. No log do job:

- Erro de rede ou HTTP: a fonte estava fora do ar. Rode de novo mais tarde.
- "Cobertura do cruzamento abaixo do mínimo": menos de 95% dos preços casaram com um medicamento registrado. Quase sempre significa que a CMED mudou o formato do arquivo. Não force a carga — investigue o arquivo primeiro.

## Trocar a senha do banco

Necessário sempre que a senha aparecer em qualquer lugar fora dos dois cofres: chat, issue, commit, log, print de tela. Os logs do GitHub Actions deste repositório são públicos.

1. Neon → projeto `remedin` → Connect → Reset password.
2. GitHub → Settings → Secrets and variables → Actions → editar `DATABASE_CONNECTION`.
3. Render → `remedin-api` → Environment → editar `ConnectionStrings__Remedin` → salvar e publicar.

Entre os passos 1 e 3 a API fica sem banco. Faça os três em sequência.

## Quando algo dá errado

**A primeira busca demora uns 25 segundos.** Normal: a API estava dormindo. A tela avisa depois de 4 segundos.

**O site abre, mas nenhuma busca carrega, e o console do navegador fala em CORS.** O endereço do front não bate com `Cors__AllowedOrigins__0` no `render.yaml`.

**`/health` responde 503.** O log da API no Render mostra a causa na linha do health check. "Timeout" logo depois de um período parado é o Neon acordando, e passa sozinho. "Password authentication failed" é a senha — veja a seção acima.

**A verificação diária falhou.** Abra o job: o nome do passo que falhou diz o que parou — a API, a busca, o site ou a rota da ficha.

**Os agendamentos pararam de rodar.** O GitHub desliga agendamento de repositório público depois de 60 dias sem nenhum commit, e avisa por e-mail antes. Para religar: Actions → o workflow → Enable workflow.

## Limites dos planos gratuitos

| Serviço | Limite | Consequência |
|---|---|---|
| Render, API | Dorme depois de 15 minutos sem acesso; 0,1 CPU e 512 MB | Primeira requisição leva 25 s (medido em 26/10/2026); acordada, 0,2 s |
| Render, site | Servido por CDN | Não dorme |
| Neon | 1 GB de armazenamento; compute suspende quando parada | O banco ocupa cerca de 100 MB depois da carga |
| GitHub Actions | Minutos ilimitados em repositório público | Agendamentos desligados depois de 60 dias sem atividade |

**Não use monitor externo que acesse o site a cada poucos minutos.** Ele manteria a API e o banco acordados o tempo todo, e o plano gratuito do Neon tem limite de horas de processamento. A verificação diária existe para cobrir isso sem esse custo.

**Antes de apresentar o projeto**, abra o site um minuto antes, para a API já estar acordada.
