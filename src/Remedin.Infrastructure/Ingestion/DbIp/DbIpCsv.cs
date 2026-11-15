using System.Text;

namespace Remedin.Infrastructure.Ingestion.DbIp;

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
