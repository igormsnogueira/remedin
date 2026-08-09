# Funil do catálogo e cobertura da informação clínica

Gerado em 08/08/2026 23:11 a partir de `TA_PRECO_MEDICAMENTO.csv`.

Saída bruta do script. Serve de evidência da análise, não editar à mão.

```

-- funil do catálogo
linhas na base de registro..........................  43.397  
linhas com número de registro.......................  32.629   75.2% do anterior
registros distintos.................................  32.626  100.0% do anterior
registros ativos....................................  10.278   31.5% do anterior
registros ativos com preço..........................   8.225   80.0% do anterior
com ao menos uma apresentação de balcão.............   6.894   83.8% do anterior
e comercializada no último ano......................   5.021   72.8% do anterior

catálogo efetivo: 5.021 medicamentos

-- registros com preço mas inativos na ANVISA
708 registros
A CMED publica preço vigente para eles e a ANVISA os dá como inativos.
exemplos: ['106390262', '112363413', '102160266', '100410197', '109740200']

-- registros fora do padrão de 9 dígitos
24 valores
casam com preço após preencher zero à esquerda: 0

-- cobertura da informação clínica no catálogo efetivo
linhas de preço no catálogo efetivo: 10.374
  TARJA.......................  78.6% com valor útil
  SUBSTÂNCIA.................. 100.0% com valor útil
  CLASSE TERAPÊUTICA.......... 100.0% com valor útil

-- medicamentos ativos sem preço
2.053 registros ficam sem informação clínica
A fonte clínica é a lista da CMED, então eles entram só com nome e fabricante.
```
