# 0006 — Preço por alíquota de ICMS

**Status:** aceita · **Data:** 08/08/2026

## Contexto

A lista da CMED traz 52 colunas de preço: 26 de Preço Fábrica e 26 de Preço Máximo ao Consumidor, uma por alíquota de ICMS, cada uma com variante ALC para as áreas de livre comércio.

O preço-teto legal depende do estado onde o medicamento é vendido. O dicionário oficial dá o mapeamento:

| Alíquota | Estados |
|---|---|
| 20% | RJ |
| 18% | AM, AP, BA, CE, MA, MG, PB, PE, PI, PR, RN, RS, SE, SP, TO |
| 17,5% | RO |
| 17% | os demais |
| 12% | genéricos em SP e MG |
| 0% | isentos por convênio CONFAZ |
| ALC | Manaus/Tabatinga, Boa Vista/Bonfim, Macapá/Santana, Guajará-Mirim, Brasiléia/Epitaciolândia/Cruzeiro do Sul |

O CSV tem colunas até 23%, e o texto do dicionário descreve até 22%. O mapeamento precisa ser mantido no projeto, com data.

## Decisão

Armazenar **todas** as 52 colunas, em formato longo: uma linha por apresentação, tipo (PF ou PMC), alíquota e indicador de ALC.

Manter uma tabela `aliquota_por_uf` no repositório, versionada e datada, derivada do dicionário oficial.

A ficha do medicamento exibe o preço da UF escolhida pelo usuário, com o rótulo indicando qual estado e a data da carga.

## Por quê

Guardar uma alíquota só faria o site exibir teto abaixo do legal para boa parte do país. Quem está no Rio veria o valor de 18% e reclamaria na farmácia de um preço que é legítimo. O produto se apresenta como fonte do teto legal, então errar isso é errar a única coisa que ele promete.

O custo de guardar tudo é desprezível: 25.702 linhas por 52 colunas dá cerca de 1,34 milhão de linhas em formato longo, ou algo em torno de 11 MB. Não é volume que justifique escolher.

O formato longo permite consultar por UF sem 52 colunas na tabela, e absorve alíquota nova sem migration de schema.

## Consequências

A ficha precisa de seletor de estado, e de um padrão quando o usuário não escolheu.

A tabela de alíquota por UF envelhece: mudança de ICMS estadual exige atualizá-la. Por isso ela é dado versionado, com data, e não constante no código.

`PMC` está vazio em 15,1% das linhas, praticamente o mesmo conjunto de `RESTRIÇÃO HOSPITALAR = Sim`. Nesses casos a ficha mostra apenas o Preço Fábrica, explicando que o produto é de uso hospitalar.

O valor exibido é teto, não preço de mercado, e a farmácia normalmente cobra abaixo dele. A ficha precisa deixar isso explícito, senão o usuário conclui que o site está errado quando pagar menos.
