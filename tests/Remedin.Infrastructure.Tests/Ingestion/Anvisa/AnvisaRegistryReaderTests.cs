using System.Text;
using Remedin.Domain.Medicines;
using Remedin.Infrastructure.Ingestion.Anvisa;

namespace Remedin.Infrastructure.Tests.Ingestion.Anvisa;

public class AnvisaRegistryReaderTests
{
    private const string Header =
        "TIPO_PRODUTO;NOME_PRODUTO;DATA_FINALIZACAO_PROCESSO;CATEGORIA_REGULATORIA;" +
        "NUMERO_REGISTRO_PRODUTO;DATA_VENCIMENTO_REGISTRO;NUMERO_PROCESSO;CLASSE_TERAPEUTICA;" +
        "EMPRESA_DETENTORA_REGISTRO;SITUACAO_REGISTRO;PRINCIPIO_ATIVO";

    /// <summary>
    /// O arquivo real é latin1. Escrever a amostra no mesmo encoding é o que
    /// faz o teste cobrir a acentuação de verdade.
    /// </summary>
    private static Stream Csv(params string[] rows)
    {
        var content = string.Join("\r\n", [Header, .. rows]);
        return new MemoryStream(Encoding.Latin1.GetBytes(content));
    }

    private static string Row(
        string name = "DIPIRONA MONOIDRATADA",
        string registrationNumber = "102980592",
        string status = "Ativo",
        string therapeuticClass = "ANALGÉSICOS",
        string manufacturer = "44734671000151 - CRISTÁLIA PRODUTOS QUÍMICOS",
        string activeIngredient = "dipirona monoidratada") =>
        $"MEDICAMENTO;{name};03/12/2013;Similar;{registrationNumber};122033;" +
        $"25351061026202116;{therapeuticClass};{manufacturer};{status};{activeIngredient}";

    private static readonly AnvisaRegistryReader Reader = new();

    [Fact]
    public void Le_uma_linha_valida()
    {
        var result = Reader.ReadRows(Csv(Row()));

        var medicine = Assert.Single(result.Medicines);
        Assert.Equal("102980592", medicine.RegistrationNumber.Value);
        Assert.Equal("DIPIRONA MONOIDRATADA", medicine.Name);
        Assert.Equal(RegistrationStatus.Active, medicine.Status);
        Assert.Equal(1, result.RowsRead);
        Assert.Empty(result.Rejected);
    }

    [Fact]
    public void Preserva_a_acentuacao_do_arquivo_latin1()
    {
        var result = Reader.ReadRows(Csv(Row(therapeuticClass: "ANTIGLAUCOMATOSOS ÁCIDOS")));

        Assert.Equal("ANTIGLAUCOMATOSOS ÁCIDOS", result.Medicines[0].TherapeuticClassName);
    }

    [Fact]
    public void Remove_o_cnpj_do_nome_do_fabricante()
    {
        var result = Reader.ReadRows(Csv(Row()));

        Assert.Equal("CRISTÁLIA PRODUTOS QUÍMICOS", result.Medicines[0].Manufacturer);
    }

    [Fact]
    public void Recusa_linha_sem_numero_de_registro()
    {
        var result = Reader.ReadRows(Csv(Row(registrationNumber: "")));

        Assert.Empty(result.Medicines);
        var rejected = Assert.Single(result.Rejected);
        Assert.Contains("número de registro", rejected.Reason);
    }

    [Fact]
    public void Recusa_linha_sem_nome()
    {
        var result = Reader.ReadRows(Csv(Row(name: "")));

        Assert.Empty(result.Medicines);
        Assert.Contains("nome", result.Rejected[0].Reason);
    }

    [Fact]
    public void Linha_recusada_nao_interrompe_a_leitura()
    {
        var result = Reader.ReadRows(Csv(
            Row(registrationNumber: ""),
            Row(registrationNumber: "112130229"),
            Row(registrationNumber: "abc")));

        Assert.Single(result.Medicines);
        Assert.Equal(3, result.RowsRead);
        Assert.Equal(2, result.Rejected.Count);
    }

    [Fact]
    public void Descarta_linha_duplicada_mantendo_a_primeira()
    {
        var result = Reader.ReadRows(Csv(
            Row(name: "BIMOXIN"),
            Row(name: "BIMOXIN"),
            Row(name: "BIMOXIN")));

        Assert.Single(result.Medicines);
        Assert.Equal(2, result.Duplicates);
        Assert.Empty(result.Rejected);
    }

    [Fact]
    public void Situacao_diferente_de_ativo_vira_inativo()
    {
        var result = Reader.ReadRows(Csv(Row(status: "Inativo")));

        Assert.Equal(RegistrationStatus.Inactive, result.Medicines[0].Status);
    }

    [Fact]
    public void Campo_vazio_vira_ausencia_de_informacao()
    {
        var result = Reader.ReadRows(Csv(Row(activeIngredient: "", therapeuticClass: "")));

        Assert.Null(result.Medicines[0].ActiveIngredient);
        Assert.Null(result.Medicines[0].TherapeuticClassName);
    }

    [Fact]
    public void Falha_alto_se_o_layout_da_origem_mudar()
    {
        var withoutRegistration = new MemoryStream(
            Encoding.Latin1.GetBytes("NOME_PRODUTO;SITUACAO_REGISTRO\r\nDIPIRONA;Ativo"));

        var exception = Assert.Throws<InvalidDataException>(() => Reader.ReadRows(withoutRegistration));

        Assert.Contains("NUMERO_REGISTRO_PRODUTO", exception.Message);
    }
}
