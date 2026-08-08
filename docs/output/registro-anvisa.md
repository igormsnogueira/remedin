# Perfil da base de registro da ANVISA

Gerado em 03/08/2026 02:03 a partir de `DADOS_ABERTOS_MEDICAMENTOS.csv`.

Saída bruta do script. Serve de evidência da análise, não editar à mão.

```

-- arquivo
tamanho: 7.9 MB | encoding: latin1 | delimitador: ';'
linhas: 43.397 | colunas: 11

-- colunas: preenchimento e cardinalidade
                            nulos  preenchidos_%  distintos                                        exemplo
TIPO_PRODUTO                    0          100.0          1                                    MEDICAMENTO
NOME_PRODUTO                    0          100.0      21175                                        ZONIDRA
DATA_FINALIZACAO_PROCESSO     114           99.7       5324                                     29/12/2023
CATEGORIA_REGULATORIA        1827           95.8         13                                        Similar
NUMERO_REGISTRO_PRODUTO     10768           75.2      32626                                      102980592
DATA_VENCIMENTO_REGISTRO     6106           85.9        608                                         122033
NUMERO_PROCESSO             10654           75.4      32740                              25351061026202116
CLASSE_TERAPEUTICA          11464           73.6        500                              ANTIGLAUCOMATOSOS
EMPRESA_DETENTORA_REGISTRO      0          100.0        849  44734671000151 - CRISTÁLIA PRODUTOS QUÍMICOS 
SITUACAO_REGISTRO               0          100.0          2                                          Ativo
PRINCIPIO_ATIVO             14167           67.4       5798                      cloridrato de dorzolamida

-- situacao do registro
                   linhas     %
SITUACAO_REGISTRO              
Inativo             26141  60.2
Ativo               17256  39.8

-- candidatos a chave
colunas candidatas: ['NUMERO_REGISTRO_PRODUTO', 'NUMERO_PROCESSO']

[NUMERO_REGISTRO_PRODUTO] preenchidos: 32.629 | distintos: 32.626 | sem repetição: False
  linhas por valor (média entre os preenchidos): 1.00
  exemplo repetido: 116540035 (4 linhas)
TIPO_PRODUTO NOME_PRODUTO DATA_FINALIZACAO_PROCESSO CATEGORIA_REGULATORIA NUMERO_REGISTRO_PRODUTO DATA_VENCIMENTO_REGISTRO   NUMERO_PROCESSO           CLASSE_TERAPEUTICA
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO

[NUMERO_PROCESSO] preenchidos: 32.743 | distintos: 32.740 | sem repetição: False
  linhas por valor (média entre os preenchidos): 1.00
  exemplo repetido: 25351164021200273 (4 linhas)
TIPO_PRODUTO NOME_PRODUTO DATA_FINALIZACAO_PROCESSO CATEGORIA_REGULATORIA NUMERO_REGISTRO_PRODUTO DATA_VENCIMENTO_REGISTRO   NUMERO_PROCESSO           CLASSE_TERAPEUTICA
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO

-- nulos de NUMERO_REGISTRO_PRODUTO por CATEGORIA_REGULATORIA
                       total  sem_registro  %_sem_registro
CATEGORIA_REGULATORIA                                     
BAIXO RISCO             7112          7112           100.0
DINAMIZADO              3456          3456           100.0
FITOTER?PICO              86            86           100.0
Gases Medici              96            96           100.0
Radiofármaco              69            17            24.6
NaN                     1827             1             0.1
Biológico                950             0             0.0
Dinamizado               278             0             0.0
Específico              1535             0             0.0
Fitoterápico            1235             0             0.0
Genérico                8134             0             0.0
Novo                    3229             0             0.0
Produto de T               8             0             0.0
Similar                15382             0             0.0

-- qualidade
linhas totalmente duplicadas: 3.438
colunas com espaços sobrando (quebram cruzamento de chave): ['NOME_PRODUTO', 'DATA_VENCIMENTO_REGISTRO', 'CLASSE_TERAPEUTICA', 'EMPRESA_DETENTORA_REGISTRO', 'PRINCIPIO_ATIVO']
colunas com acento perdido (letra virou '?', não recuperável): ['NOME_PRODUTO', 'CATEGORIA_REGULATORIA']

-- amostra para o repositório
100 linhas salvas em data/samples/medicamentos-sample.csv (utf-8)
```
