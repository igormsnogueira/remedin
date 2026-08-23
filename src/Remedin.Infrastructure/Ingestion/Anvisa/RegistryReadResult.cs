using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

/// <summary>
/// Resultado da leitura do arquivo de registro.
///
/// Linha recusada não interrompe a carga: um quarto da base não tem número de
/// registro e fica fora do catálogo por decisão de escopo, então rejeição é o
/// caso normal, não a exceção. O que importa é a contagem ficar visível.
/// </summary>
public sealed record RegistryReadResult(
    IReadOnlyList<Medicine> Medicines,
    int RowsRead,
    IReadOnlyList<RejectedRow> Rejected,
    int Duplicates)
{
    public int Accepted => Medicines.Count;

    public override string ToString() =>
        $"{RowsRead} linhas lidas, {Accepted} aceitas, " +
        $"{Rejected.Count} recusadas, {Duplicates} duplicadas";
}

public sealed record RejectedRow(int LineNumber, string Reason);
