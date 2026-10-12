namespace Remedin.Application.Catalog.Search;

/// <summary>
/// A fatia da lista que a tela está pedindo.
///
/// Fala em página, e não em deslocamento, porque é o vocabulário da interface.
/// A conversão para OFFSET é detalhe de quem escreve o SQL.
/// </summary>
public sealed record Page
{
    public const int DefaultSize = 20;
    public const int MaximumSize = 50;

    private Page(int number, int size)
    {
        Number = number;
        Size = size;
    }

    /// <summary>Começa em 1, como a pessoa conta.</summary>
    public int Number { get; }

    public int Size { get; }

    public int Offset => (Number - 1) * Size;

    /// <summary>
    /// Valor fora da faixa é ajustado em vez de recusado: página e tamanho vêm
    /// da barra de endereço, onde qualquer um digita qualquer coisa, e devolver
    /// erro para "pagina=0" não ajuda ninguém.
    /// </summary>
    public static Page Of(int number, int size = DefaultSize) =>
        new(Math.Max(number, 1), Math.Clamp(size, 1, MaximumSize));
}

/// <summary>
/// Uma fatia de resultados, com o total de que ela faz parte.
///
/// Composta dentro da resposta em vez de ter os campos espalhados nela: toda
/// lista paginada do catálogo responde as mesmas perguntas, e repetir sete
/// campos em cada resposta é o começo de elas divergirem.
/// </summary>
public sealed record PagedResults<T>(
    IReadOnlyList<T> Items,
    int Total,
    int PageNumber,
    int PageSize)
{
    private int Offset => (PageNumber - 1) * PageSize;

    public bool HasNext => Offset + Items.Count < Total;

    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Posição do primeiro e do último item dentro da lista inteira, para a
    /// tela dizer "mostrando 21 a 40 de 206". Ambas zero quando a página veio
    /// vazia — sem item, não há posição, e o deslocamento sozinho produziria
    /// um intervalo que não existe.
    /// </summary>
    public int FirstPosition => Items.Count == 0 ? 0 : Offset + 1;

    public int LastPosition => Items.Count == 0 ? 0 : Offset + Items.Count;

    public static PagedResults<T> Of(IReadOnlyList<T> items, int total, Page page) =>
        new(items, total, page.Number, page.Size);
}
