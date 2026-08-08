# Cruzamento entre preço (CMED) e registro (ANVISA)

Gerado em 03/08/2026 02:03 a partir de `TA_PRECO_MEDICAMENTO.csv`.

Saída bruta do script. Serve de evidência da análise, não editar à mão.

```

-- formato das chaves

[registro ANVISA . NUMERO_REGISTRO_PRODUTO]
  valores preenchidos: 32.629
  distintos: 32.626
  comprimento em dígitos:
    1: 1
    2: 1
    3: 3
    5: 2
    6: 1
    7: 1
    8: 14
    9: 32.605
    10: 1
  exemplos: ['102980592', '112130229', '167730106', '105710099', '154000045']

[preço CMED . REGISTRO (completo)]
  valores preenchidos: 25.702
  distintos: 25.701
  comprimento em dígitos:
    13: 25.702
  exemplos: ['1705600230032', '1018003900019', '1018003900078', '1126001990018', '1126001990034']

[preço CMED . REGISTRO (primeiros 9 dígitos)]
  valores preenchidos: 25.702
  distintos: 8.935
  comprimento em dígitos:
    9: 25.702
  exemplos: ['170560023', '101800390', '101800390', '112600199', '112600199']

-- cobertura do cruzamento
linhas de preço que encontram produto na base de registro:
  preço -> registro: 25.691 de 25.702 (100.0%)
  exemplos sem par: ['110390172', '110390173']

registros que têm ao menos um preço publicado:
  registro -> preço: 8.933 de 32.626 (27.4%)
  exemplos sem par: ['112130229', '167730106', '105710099', '154000045', '122830080']

mesmo recorte, só SITUACAO_REGISTRO = Ativo:
  registro ativo -> preço: 8.225 de 10.278 (80.0%)
  exemplos sem par: ['155840400', '176780003', '188300114', '192900015', '102351378']

-- cardinalidade
registros distintos com preço: 8.935
apresentações por registro: média 2.88 | máximo 42
  101180676: 42 apresentações
  114620001: 38 apresentações
  135170064: 36 apresentações
registros de 13 dígitos repetidos na lista de preço: 1
```
