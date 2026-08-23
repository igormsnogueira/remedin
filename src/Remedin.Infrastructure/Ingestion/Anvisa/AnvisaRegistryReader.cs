using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

/// <summary>
/// Lê o CSV de medicamentos registrados da ANVISA.
///
/// As regras vêm da análise da base real, documentada em
/// docs/analise-dados-anvisa.md.
/// </summary>
public sealed partial class AnvisaRegistryReader
{
    /// <summary>
    /// O arquivo é latin1. Declarar o encoding é obrigatório: lido como UTF-8
    /// a acentuação quebra sem gerar erro.
    /// </summary>
    private static readonly Encoding FileEncoding = Encoding.Latin1;

    /// <summary>O arquivo real tem 7,9 MB; abaixo disso veio truncado.</summary>
    public const long MinimumBytes = 1_000_000;

    private const string RegistrationNumberColumn = "NUMERO_REGISTRO_PRODUTO";
    private const string NameColumn = "NOME_PRODUTO";
    private const string StatusColumn = "SITUACAO_REGISTRO";
    private const string ActiveIngredientColumn = "PRINCIPIO_ATIVO";
    private const string ManufacturerColumn = "EMPRESA_DETENTORA_REGISTRO";
    private const string TherapeuticClassColumn = "CLASSE_TERAPEUTICA";

    private static readonly string[] RequiredColumns =
        [RegistrationNumberColumn, NameColumn, StatusColumn];

    private const string ActiveStatus = "Ativo";

    public RegistryReadResult Read(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);

        SourceFile.EnsureLooksLikeCsv(content, MinimumBytes);

        return ReadRows(content);
    }

    /// <summary>Sem a validação de arquivo. Serve para ler amostra em teste.</summary>
    public RegistryReadResult ReadRows(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var medicines = new Dictionary<string, Medicine>();
        var rejected = new List<RejectedRow>();
        var rowsRead = 0;
        var duplicates = 0;

        using var reader = new StreamReader(content, FileEncoding, leaveOpen: true);
        using var csv = new CsvReader(reader, Configuration());

        csv.Read();
        csv.ReadHeader();
        EnsureRequiredColumns(csv);

        while (csv.Read())
        {
            rowsRead++;

            if (!RegistrationNumber.TryParse(Field(csv, RegistrationNumberColumn), out var number))
            {
                // Um quarto da base não tem número de registro. É o filtro de
                // escopo do catálogo, não um defeito do arquivo.
                rejected.Add(new RejectedRow(csv.Parser.Row, "sem número de registro"));
                continue;
            }

            var name = Field(csv, NameColumn);

            if (string.IsNullOrWhiteSpace(name))
            {
                rejected.Add(new RejectedRow(csv.Parser.Row, "sem nome do produto"));
                continue;
            }

            // A base traz 3.438 linhas duplicadas por inteiro. A primeira vale.
            if (medicines.ContainsKey(number.Value))
            {
                duplicates++;
                continue;
            }

            medicines.Add(number.Value, BuildMedicine(csv, number, name));
        }

        return new RegistryReadResult([.. medicines.Values], rowsRead, rejected, duplicates);
    }

    private static Medicine BuildMedicine(CsvReader csv, RegistrationNumber number, string name)
    {
        var status = string.Equals(Field(csv, StatusColumn), ActiveStatus, StringComparison.OrdinalIgnoreCase)
            ? RegistrationStatus.Active
            : RegistrationStatus.Inactive;

        var medicine = Medicine.Register(number, name, status);

        medicine.Describe(
            activeIngredient: Field(csv, ActiveIngredientColumn),
            manufacturer: WithoutCompanyId(Field(csv, ManufacturerColumn)),
            therapeuticClassName: Field(csv, TherapeuticClassColumn));

        return medicine;
    }

    private static CsvConfiguration Configuration() =>
        new(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            // A base tem aspas soltas em nome de produto. Interromper a carga
            // inteira por causa disso custa mais do que ignorar a linha.
            BadDataFound = null,
            MissingFieldFound = null,
        };

    private static void EnsureRequiredColumns(CsvReader csv)
    {
        var header = csv.HeaderRecord ?? [];
        var missing = RequiredColumns.Except(header, StringComparer.OrdinalIgnoreCase).ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidDataException(
                $"Colunas obrigatórias ausentes: {string.Join(", ", missing)}. " +
                "O layout da origem mudou.");
        }
    }

    private static string? Field(CsvReader csv, string column) =>
        csv.TryGetField<string>(column, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;

    /// <summary>
    /// A ANVISA publica o fabricante como "CNPJ - RAZÃO SOCIAL". O CNPJ não
    /// ajuda quem procura o remédio e sujaria o índice de busca.
    /// </summary>
    private static string? WithoutCompanyId(string? manufacturer) =>
        manufacturer is null ? null : CompanyIdPrefix().Replace(manufacturer, string.Empty).Trim();

    [GeneratedRegex(@"^\d[\d./-]*\s*-\s*")]
    private static partial Regex CompanyIdPrefix();
}
