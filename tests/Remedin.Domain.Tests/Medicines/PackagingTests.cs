using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class PackagingTests
{
    [Theory]
    [InlineData("50 MG COM REV CT BL AL AL X 30", 50, 30)]
    [InlineData("20 MG CAP DURA LIB RETARD CT BL AL PLAS PVC TRANS X 7", 20, 7)]
    [InlineData("500 MG COM CT BL AL PLAS INC X 20", 500, 20)]
    public void Le_dosagem_e_quantidade_da_forma_solida(string description, decimal dosage, int units)
    {
        var packaging = Packaging.Read(description);

        Assert.Equal(dosage, packaging.DosageInMilligrams);
        Assert.Equal(units, packaging.UnitCount);
    }

    [Theory]
    [InlineData("0,5 MG COM REV CT BL AL PLAS TRANS X 28", 0.5)]
    [InlineData("12,5 MG COM CT BL AL PLAS X 30", 12.5)]
    public void Le_dosagem_com_virgula_decimal(string description, decimal expected)
    {
        Assert.Equal(expected, Packaging.Read(description).DosageInMilligrams);
    }

    [Fact]
    public void Sem_espaco_entre_numero_e_unidade_tambem_le()
    {
        // "20MG CAP DURA CT FR PLAS OPC X 28" aparece assim na origem.
        var packaging = Packaging.Read("20MG CAP DURA CT FR PLAS OPC X 28");

        Assert.Equal(20m, packaging.DosageInMilligrams);
        Assert.Equal(28, packaging.UnitCount);
    }

    [Theory]
    [InlineData("(3,00+3,00) MG/ML SUS INJ CT 25  AMP VD AMB X 1ML")]
    [InlineData("(10 + 0,4) MG/G CREM DERM CT BG AL X 40G")]
    public void Numero_com_unidade_depois_nao_e_quantidade(string description)
    {
        // Aqui o número após o X é volume ou peso, não quantidade de doses.
        // Lê-lo como quantidade daria preço por unidade absurdo, em silêncio.
        Assert.Null(Packaging.Read(description).UnitCount);
    }

    [Theory]
    [InlineData("50 MG/ML SOL OR CT FR VD AMB X 100")]
    [InlineData("10 MG/G CREM DERM CT BG X 30")]
    public void Concentracao_nao_e_dosagem_por_unidade(string description)
    {
        // "50 MG/ML" é a concentração do líquido, não a dose de um comprimido.
        Assert.Null(Packaging.Read(description).DosageInMilligrams);
    }

    [Theory]
    [InlineData("(0,5 + 0,1) MG COM REV CT BL AL PLAS PCTFE TRANS X 28")]
    [InlineData("(600 + 70 + 100) MG COM CT BL AL PLAS PVC AMB X 8")]
    public void Dose_combinada_nao_tem_valor_unico_para_comparar(string description)
    {
        Assert.Null(Packaging.Read(description).DosageInMilligrams);
    }

    [Fact]
    public void Quantidade_com_texto_depois_e_ignorada()
    {
        // "X 30 (EMB FRAC)" é embalagem fracionada, e o 30 pode não ser o que
        // se compra. Na dúvida, ausência.
        Assert.Null(Packaging.Read("40 MG CAP CT BL AL X 30 (EMB FRAC)").UnitCount);
    }

    [Fact]
    public void Frasco_sem_quantidade_declarada_fica_sem_preco_por_unidade()
    {
        var packaging = Packaging.Read("250 MG PO LIOF SOL INJ CT 1 FA + SER DESCARTÁVEL");

        Assert.Equal(250m, packaging.DosageInMilligrams);
        Assert.Null(packaging.UnitCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SOL AA + SOL GLIC INFUS IV CX 5 BOLS PLAS PP TRANS BIP SIST FECH X 1000 ML")]
    public void Texto_sem_dosagem_legivel_devolve_ausencia(string? description)
    {
        var packaging = Packaging.Read(description);

        Assert.Null(packaging.DosageInMilligrams);
        Assert.Null(packaging.UnitCount);
    }
}
