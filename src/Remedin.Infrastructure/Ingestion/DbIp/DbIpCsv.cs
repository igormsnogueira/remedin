using System.Text;

namespace Remedin.Infrastructure.Ingestion.DbIp;

/// <summary>
/// Leitura dos campos de uma linha do CSV da DB-IP (RFC 4180).
///
/// Campo com vírgula vem entre aspas, como "Mountain View". Só os primeiros
/// campos interessam, então a leitura para assim que tem os que pediu, sem
/// percorrer cidade e coordenadas de 7 milhões de linhas.
/// </summary>
internal static class DbIpCsv
{
    public static List<string> ReadFields(string line, int count)
    {
        var fields = new List<string>(count);
        var current = new StringBuilder();
        var quoted = false;

        for (var index = 0; index < line.Length && fields.Count < count; index++)
        {
            var character = line[index];

            if (quoted)
            {
                if (character == '"')
                {
                    // Aspas dobradas dentro de campo entre aspas são uma aspa literal.
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else
                {
                    current.Append(character);
                }
            }
            else if (character == '"')
            {
                quoted = true;
            }
            else if (character == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        if (fields.Count < count)
        {
            fields.Add(current.ToString());
        }

        return fields;
    }
}
