using Remedin.Application.Catalog.Search;

namespace Remedin.Application.Tests.Catalog.Search;

public class MedicineAlternativesTests
{
    private int _lastRegistration;


    [Fact]
    public void Economia_e_por_comprimido_e_nao_por_caixa()
    {
        // R$ 150,36 em 28 cápsulas contra R$ 43,70 em 56: comparar as caixas
        // daria R$ 106,66, número que não corresponde a nada que a pessoa paga.
        var result = Result(
            Alternative("REFERÊNCIA", price: 150.36m, dosage: 20, units: 28, current: true),
            Alternative("GENÉRICO", price: 43.70m, dosage: 20, units: 56));

        Assert.Equal(4.59m, result.SavingsPerUnit);
        Assert.Equal("GENÉRICO", result.CheapestComparable?.Name);
    }

    [Fact]
    public void Dosagem_diferente_fica_fora_da_comparacao()
    {
        var result = Result(
            Alternative("CONSULTADO", price: 60m, dosage: 20, units: 30, current: true),
            Alternative("METADE DA DOSE", price: 15m, dosage: 10, units: 30));

        Assert.Null(result.CheapestComparable);
        Assert.Null(result.SavingsPerUnit);
    }

    [Fact]
    public void Consultado_mais_barato_nao_gera_economia()
    {
        var result = Result(
            Alternative("CONSULTADO", price: 20m, dosage: 20, units: 30, current: true),
            Alternative("MAIS CARO", price: 90m, dosage: 20, units: 30));

        Assert.Null(result.CheapestComparable);
        Assert.Null(result.SavingsPerUnit);
    }

    [Fact]
    public void Sem_quantidade_legivel_no_consultado_nao_ha_economia()
    {
        // Frasco de xarope: não dá para dizer quantas doses tem.
        var result = Result(
            Alternative("XAROPE", price: 30m, dosage: 20, units: null, current: true),
            Alternative("COMPRIMIDO", price: 12m, dosage: 20, units: 30));

        Assert.Null(result.SavingsPerUnit);
    }

    [Fact]
    public void Alternativa_sem_quantidade_legivel_nao_entra_na_comparacao()
    {
        var result = Result(
            Alternative("CONSULTADO", price: 60m, dosage: 20, units: 30, current: true),
            Alternative("SEM QUANTIDADE", price: 10m, dosage: 20, units: null),
            Alternative("COMPARÁVEL", price: 45m, dosage: 20, units: 30));

        Assert.Equal("COMPARÁVEL", result.CheapestComparable?.Name);
        Assert.Equal(0.50m, result.SavingsPerUnit);
    }

    [Fact]
    public void Sem_o_consultado_na_lista_nao_ha_com_o_que_comparar()
    {
        // Acontece quando o medicamento consultado não tem preço publicado.
        var result = Result(Alternative("OUTRO", price: 20m, dosage: 20, units: 30));

        Assert.Null(result.CheapestComparable);
        Assert.Null(result.SavingsPerUnit);
    }

    private static AlternativesResult Result(params MedicineAlternative[] alternatives) =>
        new("102980354", "OMEPRAZOL", "SP", 18m, alternatives);

    private MedicineAlternative Alternative(
        string name,
        decimal price,
        decimal? dosage,
        int? units,
        bool current = false) =>
        new(
            RegistrationNumber: (++_lastRegistration).ToString("D9"),
            Name: name,
            Manufacturer: "FABRICANTE",
            Presentation: $"{dosage} MG COM CT BL X {units}",
            ConsumerPrice: price,
            DosageInMilligrams: dosage,
            UnitCount: units,
            IsCurrent: current);
}
