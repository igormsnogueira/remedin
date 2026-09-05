# 0010 — Equivalência por princípio ativo

**Status:** aceita · **Data:** 03/09/2026

## Contexto

O produto promete mostrar alternativas mais baratas com o mesmo princípio ativo. O caso é real: três OMEPRAZOL no catálogo custam R$ 24,99, R$ 57,96 e R$ 150,49.

Para agrupar equivalentes é preciso comparar o campo de princípio ativo da CMED, e ele varia de seis formas nos 8.933 medicamentos com preço:

| Variação | Exemplo |
|---|---|
| Ordem dos componentes | `CAFEÍNA ANIDRA;DIPIRONA MONOIDRATADA` e `DIPIRONA;CITRATO DE ORFENADRINA;CAFEÍNA ANIDRA` |
| Sal ou hidratação | `DIPIRONA` e `DIPIRONA MONOIDRATADA`, 40 medicamentos cada |
| Sinônimo | `MALEATO DE CLORFENAMINA` e `MALEATO DE CLORFENIRAMINA` |
| Sufixo | `CAFEÍNA` e `CAFEÍNA ANIDRA` |
| Repetição na mesma linha | `DIPIRONA;DIPIRONA MONOIDRATADA` |
| Espaço duplicado | `CLORIDRATO  DE ONDANSETRONA` |

1.548 dos 8.933 são associações de mais de uma substância.

## Decisão

Uma chave canônica calculada na carga: separar os componentes, remover acento, normalizar espaço e caixa, eliminar repetição e ordenar em ordem alfabética.

Dois medicamentos são apresentados como alternativa quando essa chave é idêntica.

Sal, hidratação e sinônimo **não** são normalizados.

A interface descreve o resultado como "medicamentos com o mesmo princípio ativo", e não como substituto. A escolha do que trocar é do farmacêutico ou do médico.

## Por quê

A chave resolve ordem, repetição e espaço, que são variação de escrita da mesma informação. Isso é seguro: o conjunto de substâncias é o mesmo, escrito de outro jeito.

Sal e sinônimo exigiriam um dicionário farmacêutico curado, e normalizar por heurística seria adivinhar. **Os dois erros possíveis não custam a mesma coisa:** agrupar de menos deixa de mostrar uma alternativa que existe, e a pessoa continua com a informação que já tinha. Agrupar de mais apresenta como equivalente algo que pode não ser, para alguém decidindo o que comprar. Diante da assimetria, o desenho erra para o lado conservador.

## Consequências

`DIPIRONA` e `DIPIRONA MONOIDRATADA` ficam em grupos separados, e a pessoa não vê uma como alternativa da outra. São 80 medicamentos onde a comparação seria útil e não aparece.

A comparação é por princípio ativo, não por dosagem nem por quantidade: a CMED descreve as duas dentro do texto da apresentação, sem campo próprio. A lista de OMEPRAZOL traz 10 MG, 20 MG e 40 MG lado a lado, e embalagens de 7 e de 28 cápsulas.

Por isso **a resposta não traz cálculo de economia**. Subtrair o preço de uma caixa de 7 do preço de uma caixa de 28 dá um número errado, e número que o site afirma a pessoa acredita — diferente da lista, onde ela vê a apresentação e compara. A lista vem acompanhada de aviso para conferir dosagem e quantidade, e para confirmar a troca com o farmacêutico.

Comparar por unidade exige extrair dosagem e quantidade do texto. O padrão parece regular (`20 MG ... X 28`), mas precisa ser medido sobre as 25.691 apresentações antes de virar código: extração que falha em silêncio produziria um preço por unidade errado, que é pior que não ter.

A chave é calculada na carga e guardada em coluna própria, com índice. Calcular na consulta exigiria processar o texto de 8.933 medicamentos a cada busca.

Um dicionário de sais e sinônimos ampliaria a cobertura, e é o próximo passo natural desta funcionalidade. Ele não entra agora porque exige curadoria com fonte farmacêutica, e não palpite.
