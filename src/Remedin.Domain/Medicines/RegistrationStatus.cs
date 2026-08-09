namespace Remedin.Domain.Medicines;

/// <summary>
/// Situação do registro na ANVISA.
///
/// A base publica os dois estados. "Inativo" indica registro cancelado ou
/// caduco, ou seja, produto fora do mercado — ainda que a CMED publique preço
/// vigente para parte deles.
/// </summary>
public enum RegistrationStatus
{
    Inactive = 0,
    Active = 1,
}
