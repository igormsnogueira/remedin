namespace Remedin.Domain.Medicines;

/// <summary>
/// Um medicamento registrado na ANVISA, com as apresentações que a CMED
/// publica preço.
///
/// É a raiz do agregado: as apresentações só são alteradas por aqui, o que
/// mantém num lugar só a regra de que não existem duas com o mesmo código.
/// </summary>
public sealed class Medicine
{
    private readonly List<Presentation> _presentations = [];

    private Medicine(RegistrationNumber registrationNumber, string name, RegistrationStatus status)
    {
        RegistrationNumber = registrationNumber;
        Name = name;
        Status = status;
    }

    public RegistrationNumber RegistrationNumber { get; }

    public string Name { get; private set; }

    public string? ActiveIngredient { get; private set; }

    public string? Manufacturer { get; private set; }

    /// <summary>Código e descrição da classe terapêutica, no padrão da CMED.</summary>
    public string? TherapeuticClassCode { get; private set; }

    public string? TherapeuticClassName { get; private set; }

    /// <summary>Tarja, que indica a exigência de receita. Nem sempre informada.</summary>
    public string? PrescriptionBand { get; private set; }

    public RegistrationStatus Status { get; private set; }

    public IReadOnlyList<Presentation> Presentations => _presentations;

    /// <summary>
    /// Medicamento sem apresentação é estado válido: a carga de preço pode não
    /// ter rodado, e parte dos registros ativos não tem preço publicado.
    /// </summary>
    public bool HasPrice => _presentations.Any(p => p.ConsumerPrice is not null || p.FactoryPrice is not null);

    public bool IsSoldInPharmacy => _presentations.Any(p => !p.HospitalOnly);

    public bool WasSoldRecently => _presentations.Any(p => p.SoldRecently);

    /// <summary>Menor preço ao consumidor entre as apresentações de balcão.</summary>
    public decimal? CheapestConsumerPrice => _presentations
        .Where(p => !p.HospitalOnly)
        .Select(p => p.ConsumerPrice)
        .Where(price => price is not null)
        .Min();

    public static Medicine Register(
        RegistrationNumber registrationNumber,
        string name,
        RegistrationStatus status)
    {
        ArgumentNullException.ThrowIfNull(registrationNumber);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Medicamento exige nome.", nameof(name));
        }

        return new Medicine(registrationNumber, name.Trim(), status);
    }

    public void Describe(
        string? activeIngredient = null,
        string? manufacturer = null,
        string? therapeuticClassCode = null,
        string? therapeuticClassName = null,
        string? prescriptionBand = null)
    {
        ActiveIngredient = Normalize(activeIngredient);
        Manufacturer = Normalize(manufacturer);
        TherapeuticClassCode = Normalize(therapeuticClassCode);
        TherapeuticClassName = Normalize(therapeuticClassName);
        PrescriptionBand = Normalize(prescriptionBand);
    }

    public void AddPresentation(Presentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        if (_presentations.Any(existing => existing.GgremCode == presentation.GgremCode))
        {
            throw new ArgumentException(
                $"A apresentação {presentation.GgremCode} já existe neste medicamento.",
                nameof(presentation));
        }

        _presentations.Add(presentation);
    }

    /// <summary>
    /// Troca o conjunto inteiro de apresentações. É como a carga mensal
    /// funciona: a CMED publica a lista completa, não um incremento.
    /// </summary>
    public void ReplacePresentations(IEnumerable<Presentation> presentations)
    {
        ArgumentNullException.ThrowIfNull(presentations);

        var replacement = presentations.ToList();

        var duplicated = replacement
            .GroupBy(p => p.GgremCode)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicated is not null)
        {
            throw new ArgumentException(
                $"A apresentação {duplicated.Key} aparece mais de uma vez.",
                nameof(presentations));
        }

        _presentations.Clear();
        _presentations.AddRange(replacement);
    }

    public void ChangeStatus(RegistrationStatus status) => Status = status;

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
