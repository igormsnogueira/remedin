======================================================================
1. ARQUIVO, ENCODING E DELIMITADOR
======================================================================
Arquivo ...... DADOS_ABERTOS_MEDICAMENTOS.csv
Tamanho ...... 7.9 MB
Encoding ..... latin1
Delimitador .. ';'

>> ATENCAO: o arquivo NAO e UTF-8. O ETL precisara declarar
   este encoding explicitamente, senao a acentuacao quebra.

======================================================================
2. CARREGANDO
======================================================================
Linhas ....... 43.397
Colunas ...... 11

======================================================================
3. COLUNAS, PREENCHIMENTO E CARDINALIDADE
======================================================================
                            nulos  preenchidos_%  valores_distintos                                        exemplo
TIPO_PRODUTO                    0          100.0                  1                                    MEDICAMENTO
NOME_PRODUTO                    0          100.0              21175                                        ZONIDRA
DATA_FINALIZACAO_PROCESSO     114           99.7               5324                                     29/12/2023
CATEGORIA_REGULATORIA        1827           95.8                 13                                        Similar
NUMERO_REGISTRO_PRODUTO     10768           75.2              32626                                      102980592
DATA_VENCIMENTO_REGISTRO     6106           85.9                608                                         122033
NUMERO_PROCESSO             10654           75.4              32740                              25351061026202116
CLASSE_TERAPEUTICA          11464           73.6                500                              ANTIGLAUCOMATOSOS
EMPRESA_DETENTORA_REGISTRO      0          100.0                849  44734671000151 - CRISTÁLIA PRODUTOS QUÍMICOS 
SITUACAO_REGISTRO               0          100.0                  2                                          Ativo
PRINCIPIO_ATIVO             14167           67.4               5798                      cloridrato de dorzolamida

======================================================================
4. SITUACAO DO REGISTRO (valido x cancelado)
======================================================================
                   linhas     %
SITUACAO_REGISTRO              
Inativo             26141  60.2
Ativo               17256  39.8

>> Decisao de produto: exibir apenas registros validos?

======================================================================
5. CHAVE CANDIDATA: NUMERO DE REGISTRO
======================================================================
Colunas candidatas: ['DATA_FINALIZACAO_PROCESSO', 'NUMERO_REGISTRO_PRODUTO', 'NUMERO_PROCESSO']

[DATA_FINALIZACAO_PROCESSO]
  valores distintos ... 5.324
  e chave unica? ...... NAO
  linhas por valor .... 8.15 em media
  exemplo repetido .... 28/01/2016 (186 linhas)

  >> PERGUNTA-CHAVE: o que difere entre essas linhas?
     Se for apresentacao/embalagem, o agregado e
     Medicamento contendo uma colecao de Apresentacao.
TIPO_PRODUTO  NOME_PRODUTO DATA_FINALIZACAO_PROCESSO CATEGORIA_REGULATORIA NUMERO_REGISTRO_PRODUTO DATA_VENCIMENTO_REGISTRO NUMERO_PROCESSO CLASSE_TERAPEUTICA
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN
 MEDICAMENTO N?O DECLARADO                28/01/2016            DINAMIZADO                     NaN                   012026             NaN                NaN

[NUMERO_REGISTRO_PRODUTO]
  valores distintos ... 32.626
  e chave unica? ...... NAO
  linhas por valor .... 1.33 em media
  exemplo repetido .... 116540035 (4 linhas)

  >> PERGUNTA-CHAVE: o que difere entre essas linhas?
     Se for apresentacao/embalagem, o agregado e
     Medicamento contendo uma colecao de Apresentacao.
TIPO_PRODUTO NOME_PRODUTO DATA_FINALIZACAO_PROCESSO CATEGORIA_REGULATORIA NUMERO_REGISTRO_PRODUTO DATA_VENCIMENTO_REGISTRO   NUMERO_PROCESSO           CLASSE_TERAPEUTICA
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO

[NUMERO_PROCESSO]
  valores distintos ... 32.740
  e chave unica? ...... NAO
  linhas por valor .... 1.33 em media
  exemplo repetido .... 25351164021200273 (4 linhas)

  >> PERGUNTA-CHAVE: o que difere entre essas linhas?
     Se for apresentacao/embalagem, o agregado e
     Medicamento contendo uma colecao de Apresentacao.
TIPO_PRODUTO NOME_PRODUTO DATA_FINALIZACAO_PROCESSO CATEGORIA_REGULATORIA NUMERO_REGISTRO_PRODUTO DATA_VENCIMENTO_REGISTRO   NUMERO_PROCESSO           CLASSE_TERAPEUTICA
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO
 MEDICAMENTO      BIMOXIN                15/08/2002               Similar               116540035                   082007 25351164021200273 PENICILINA DE AMPLO ESPECTRO


======================================================================
6. QUALIDADE DOS DADOS
======================================================================
Linhas totalmente duplicadas: 3.438

Colunas com espacos no inicio/fim (afetam o cruzamento):
  - NOME_PRODUTO
  - DATA_VENCIMENTO_REGISTRO
  - CLASSE_TERAPEUTICA
  - EMPRESA_DETENTORA_REGISTRO
  - PRINCIPIO_ATIVO

Teste de acentuacao (procurando lixo tipo 'Ã§', 'Ã£'):
  >> ENCODING ERRADO nas colunas: ['NOME_PRODUTO', 'CLASSE_TERAPEUTICA', 'EMPRESA_DETENTORA_REGISTRO']

======================================================================
7. GERANDO AMOSTRA PARA O REPOSITORIO
======================================================================
Amostra de 100 linhas salva em: data/samples/medicamentos-sample.csv
(convertida para UTF-8 para facilitar leitura no GitHub)

======================================================================
FIM - copie estes resultados para docs/analise-dados-anvisa.md
======================================================================
