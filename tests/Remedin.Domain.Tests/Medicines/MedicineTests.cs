using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class MedicineTests
{
    private static Medicine AnyMedicine() =>
        Medicine.Register(RegistrationNumber.Parse("102980592"), "DIPIRONA MONOIDRATADA", RegistrationStatus.Active);

    private static Presentation AnyPresentation(
        string ggremCode = "538912020009303",
        decimal? consumerPrice = 12.34m,
        bool hospitalOnly = false,
        bool soldRecently = true) =>
        Presentation.Create(
            ggremCode,
            "500 MG COM CT BL AL PLAS INC X 10",
            consumerPrice,
            factoryPrice: 9.10m,
            hospitalOnly,
            soldRecently);

    [Fact]
    public void Medicamento_nasce_sem_apresentacao()
    {
        var medicine = AnyMedicine();

        Assert.Empty(medicine.Presentations);
        Assert.False(medicine.HasPrice);
        Assert.Null(medicine.CheapestConsumerPrice);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Medicamento_exige_nome(string name)
    {
        var number = RegistrationNumber.Parse("102980592");

        Assert.Throws<ArgumentException>(
            () => Medicine.Register(number, name, RegistrationStatus.Active));
    }

    [Fact]
    public void Nao_aceita_duas_apresentacoes_com_o_mesmo_codigo()
    {
        var medicine = AnyMedicine();
        medicine.AddPresentation(AnyPresentation("538912020009303"));

        Assert.Throws<ArgumentException>(
            () => medicine.AddPresentation(AnyPresentation("538912020009303")));
    }

    [Fact]
    public void Carga_mensal_troca_o_conjunto_inteiro_de_apresentacoes()
    {
        var medicine = AnyMedicine();
        medicine.AddPresentation(AnyPresentation("111111111111111"));

        medicine.ReplacePresentations([
            AnyPresentation("222222222222222"),
            AnyPresentation("333333333333333"),
        ]);

        Assert.Equal(2, medicine.Presentations.Count);
        Assert.DoesNotContain(medicine.Presentations, p => p.GgremCode == "111111111111111");
    }

    [Fact]
    public void Carga_com_apresentacao_repetida_e_recusada_inteira()
    {
        var medicine = AnyMedicine();
        medicine.AddPresentation(AnyPresentation("111111111111111"));

        Assert.Throws<ArgumentException>(() => medicine.ReplacePresentations([
            AnyPresentation("222222222222222"),
            AnyPresentation("222222222222222"),
        ]));

        Assert.Single(medicine.Presentations);
    }

    [Fact]
    public void O_preco_exibido_e_o_da_apresentacao_mais_barata()
    {
        var medicine = AnyMedicine();
        medicine.AddPresentation(AnyPresentation("111111111111111", consumerPrice: 45.00m));
        medicine.AddPresentation(AnyPresentation("222222222222222", consumerPrice: 12.50m));
        medicine.AddPresentation(AnyPresentation("333333333333333", consumerPrice: 30.00m));

        Assert.Equal(12.50m, medicine.CheapestConsumerPrice);
    }

    [Fact]
    public void Apresentacao_hospitalar_nao_entra_no_preco_de_balcao()
    {
        var medicine = AnyMedicine();
        medicine.AddPresentation(AnyPresentation("111111111111111", consumerPrice: 45.00m));
        medicine.AddPresentation(
            AnyPresentation("222222222222222", consumerPrice: 5.00m, hospitalOnly: true));

        Assert.Equal(45.00m, medicine.CheapestConsumerPrice);
    }

    [Fact]
    public void Medicamento_so_de_uso_hospitalar_nao_e_vendido_em_farmacia()
    {
        var medicine = AnyMedicine();
        medicine.AddPresentation(
            AnyPresentation("111111111111111", consumerPrice: null, hospitalOnly: true));

        Assert.False(medicine.IsSoldInPharmacy);
        Assert.Null(medicine.CheapestConsumerPrice);
    }

    [Fact]
    public void Apresentacao_sem_preco_ao_consumidor_ainda_conta_como_com_preco()
    {
        // Produto hospitalar tem Preço Fábrica e não tem PMC. Continua sendo
        // um medicamento com preço publicado.
        var medicine = AnyMedicine();
        medicine.AddPresentation(
            AnyPresentation("111111111111111", consumerPrice: null, hospitalOnly: true));

        Assert.True(medicine.HasPrice);
    }

    [Fact]
    public void Descricao_em_branco_vira_ausencia_de_informacao()
    {
        var medicine = AnyMedicine();

        medicine.Describe(activeIngredient: "  ", prescriptionBand: " Tarja Vermelha ");

        Assert.Null(medicine.ActiveIngredient);
        Assert.Equal("Tarja Vermelha", medicine.PrescriptionBand);
    }

    [Fact]
    public void Apresentacao_recusa_preco_negativo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Presentation.Create("111111111111111", "descrição", consumerPrice: -1m));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Apresentacao_exige_codigo(string ggremCode)
    {
        Assert.Throws<ArgumentException>(
            () => Presentation.Create(ggremCode, "descrição"));
    }
}
