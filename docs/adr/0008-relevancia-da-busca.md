# 0008 — Relevância da busca

**Status:** aceita · **Data:** 23/08/2026

## Contexto

A busca é a funcionalidade central do produto, e o público erra a grafia e nem sempre sabe o nome do princípio ativo.

A primeira versão tratava os quatro campos pesquisáveis como iguais e usava o maior entre três sinais como nota. Testada contra os 32.642 medicamentos reais, ela falhou no caso mais comum: buscar `dipirona` devolvia ABERALGINA, ALIVDIP, ATROVERAN DIP em ordem alfabética, e o medicamento chamado DIPIRONA nem aparecia. Centenas de produtos têm esse princípio ativo, todos empatavam com nota máxima, e o desempate caía no alfabeto.

## Decisão

O índice textual usa os quatro níveis de peso do PostgreSQL:

| Campo | Peso |
|---|---|
| Nome comercial | A |
| Princípio ativo | B |
| Finalidade em linguagem comum | B |
| Classe terapêutica, como a fonte escreve | C |
| Fabricante | D |

A finalidade divide o peso B com o princípio ativo porque é o único campo escrito na língua de quem procura. Acerto nela significa "este remédio serve para isso", que informa mais que coincidir com o nome do laboratório ou com o jargão da classe.

A nota soma sinais em vez de pegar o maior:

| Sinal | Peso |
|---|---|
| Nome idêntico ao termo | 10 |
| Nome começa com o termo | 3 |
| Relevância textual, já ponderada por campo | 4× |
| Semelhança por trigrama no nome | 1× |
| Semelhança por trigrama no princípio ativo | 0,5× |

Registro ativo ordena antes de inativo, para a mesma nota.

Termo com menos de três caracteres devolve vazio: abaixo disso o trigrama casa com quase todo o catálogo.

## Por quê

Somar, e não pegar o maior, faz um produto que casa no nome **e** no princípio ativo passar à frente de quem casa só num dos dois. Era essa a informação que se perdia.

Busca textual e trigrama cobrem falhas diferentes e por isso convivem: a textual reduz a palavra ao radical, então "analgésicos" encontra "analgésico"; o trigrama compara pedaços de três letras e é o que faz "dipirna" encontrar DIPIRONA.

Verificado contra a base real: `dipirona`, `dipirna`, `dorflex` e `acido acetilsalicilico` devolvem o resultado esperado em primeiro lugar.

## Consequências

Os pesos são um chute calibrado, não uma verdade. Ficam em números legíveis dentro da consulta justamente para serem ajustados quando um caso novo mostrar onde erram.

A consulta é SQL escrito à mão, não LINQ. As duas estratégias combinadas não têm tradução natural, e essa consulta define a qualidade do produto — vale poder lê-la inteira num lugar só.

Alterar o peso de um campo exige recriar a coluna gerada, porque ela é materializada. Com 32 mil linhas isso leva segundos.

O radicalizador do português pode igualar um sintoma a um nome de marca. `azia` reduz para `azi`, que é o nome de uma azitromicina: o antibiótico casa no nome, peso A, e aparece antes dos antiácidos. O acerto é correto do ponto de vista do índice, e mexer nos pesos até esse caso passar seria calibrar a régua para uma consulta só. Fica registrado, sem correção.

Buscar um princípio ativo comum devolve muitos produtos com o mesmo nome, de fabricantes diferentes. São registros distintos e a lista está correta, mas visualmente parece repetição: `dipirona` traz cinco linhas escritas "DIPIRONA".

Decisão de interface, anotada aqui para não se perder: o resultado exibe **nome e fabricante juntos**, no formato `DIPIRONA — Laboratório Teuto`. É o que diferencia as linhas sem esconder produto, e o fabricante é justamente o que muda o preço entre genéricos equivalentes. Agrupar por nome fica descartado por ora, porque esconderia a opção mais barata.
