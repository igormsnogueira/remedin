using Npgsql;
using NpgsqlTypes;
using Remedin.Application.Catalog.Search;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// Monta a ficha do medicamento com os preços do estado consultado.
/// </summary>
public sealed class PostgresMedicineDetails(RemedinDbContext context) : IMedicineDetails
{
    /// <summary>
    /// Uma consulta só, com o medicamento repetido por apresentação. Duas
    /// idas ao banco custariam mais que a repetição de sete colunas.
    /// </summary>
    private const string Sql = """
        SELECT m.registration_number, m.name, m.active_ingredient, m.manufacturer,
               m.therapeutic_class_code, m.therapeutic_class_name, m.prescription_band, m.status,
               p.ggrem_code, p.description, p.hospital_only, p.sold_recently,
               consumer.amount, factory.amount
        FROM medicines m
        LEFT JOIN presentations p USING (registration_number)
        LEFT JOIN prices consumer
               ON consumer.registration_number = p.registration_number
              AND consumer.ggrem_code = p.ggrem_code
              AND consumer.kind = 'Consumer'
              AND consumer.icms_rate = @rate
              AND NOT consumer.free_trade_zone
        LEFT JOIN prices factory
               ON factory.registration_number = p.registration_number
              AND factory.ggrem_code = p.ggrem_code
              AND factory.kind = 'Factory'
              AND factory.icms_rate = @rate
              AND NOT factory.free_trade_zone
        WHERE m.registration_number = @registration
        ORDER BY consumer.amount NULLS LAST, p.ggrem_code;
        """;

    public async Task<MedicineDetail?> FindAsync(
        string registrationNumber,
        string state,
        CancellationToken cancellationToken)
    {
        if (!RegistrationNumber.TryParse(registrationNumber, out var registration))
        {
            return null;
        }

        var rate = IcmsRates.For(state);
        var connection = await PostgresMedicineSearch.OpenAsync(context, cancellationToken);

        await using var command = new NpgsqlCommand(Sql, connection);
        command.Parameters.Add(
            new NpgsqlParameter("registration", NpgsqlDbType.Char) { Value = registration.Value });
        command.Parameters.Add(new NpgsqlParameter("rate", NpgsqlDbType.Numeric) { Value = rate });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        MedicineDetail? detail = null;
        var presentations = new List<PresentationDetail>();

        while (await reader.ReadAsync(cancellationToken))
        {
            detail ??= new MedicineDetail(
                RegistrationNumber: reader.GetString(0),
                Name: reader.GetString(1),
                ActiveIngredient: reader.IsDBNull(2) ? null : reader.GetString(2),
                Manufacturer: reader.IsDBNull(3) ? null : reader.GetString(3),
                TherapeuticClassCode: reader.IsDBNull(4) ? null : reader.GetString(4),
                TherapeuticClassName: reader.IsDBNull(5) ? null : reader.GetString(5),
                PrescriptionBand: reader.IsDBNull(6) ? null : reader.GetString(6),
                IsActive: reader.GetString(7) == "Active",
                State: state.ToUpperInvariant(),
                IcmsRate: rate,
                Presentations: presentations);

            // A junção à esquerda devolve uma linha sem apresentação quando o
            // medicamento ainda não tem preço carregado.
            if (reader.IsDBNull(8))
            {
                continue;
            }

            presentations.Add(new PresentationDetail(
                GgremCode: reader.GetString(8),
                Description: reader.GetString(9),
                HospitalOnly: reader.GetBoolean(10),
                SoldRecently: reader.GetBoolean(11),
                ConsumerPrice: reader.IsDBNull(12) ? null : reader.GetDecimal(12),
                FactoryPrice: reader.IsDBNull(13) ? null : reader.GetDecimal(13)));
        }

        return detail;
    }
}
