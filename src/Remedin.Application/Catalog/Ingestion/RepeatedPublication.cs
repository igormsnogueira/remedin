namespace Remedin.Application.Catalog.Ingestion;

/// <summary>
/// O que fazer quando a fonte publica o mesmo arquivo da última carga bem-sucedida.
///
/// Enum em vez de booleano: <c>ExecuteAsync(true)</c> não diz nada a quem lê
/// a chamada, e <c>ExecuteAsync(RepeatedPublication.Reload)</c> diz.
/// </summary>
public enum RepeatedPublication
{
    /// <summary>
    /// O comportamento normal. A carga agendada roda toda semana, e a fonte
    /// publica uma vez por mês; sem pular, cada semana regravaria o mesmo dado.
    /// </summary>
    Skip,

    /// <summary>
    /// Carrega mesmo assim. Para quando a regra de leitura mudou e o dado
    /// gravado precisa refletir isso — um campo novo, um filtro corrigido.
    /// </summary>
    Reload,
}
