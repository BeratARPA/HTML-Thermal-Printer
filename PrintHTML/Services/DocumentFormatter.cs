using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintHTML.Services
{
    public class DocumentFormatter
    {
        public static string FormatLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return string.Empty;

            // Özel etiketleri HTML'e dönüştür
            if (line.StartsWith("<F>") && line.Length > 3)
                return FormatFillLine(line) + "<br/>";

            if (line.StartsWith("<T>"))
                return FormatBoldLine(line) + "<br/>";

            if (line.StartsWith("<C") || line.StartsWith("<L") || line.StartsWith("<R"))
                return FormatAlignedLine(line) + "<br/>";

            if (line.StartsWith("<EB>"))
                return line.Replace("<EB>", "<B>");

            if (line.StartsWith("<DB>"))
            {
                var end = line.Replace("<DB>", "</B>");
                return end + "<br/>";
            }

            #region Tables          
            if (line.StartsWith("<J") && line.Length > 3 && (line[2] == '>' || char.IsDigit(line[2])))
            {
                var tables = new Dictionary<string, List<string>>();
                var lastLine = "";
                var tableCount = 0;
                var tableLines = new List<string>();

                if (!lastLine.StartsWith("<J"))
                {
                    tableCount++;
                    tableLines.Add("table" + tableCount);
                }

                var tableName = "table" + tableCount;
                if (!tables.ContainsKey(tableName))
                    tables.Add(tableName, new List<string>());
                tables[tableName].Add(RemoveTag(line));

                if (!line.Contains("<") && !string.IsNullOrEmpty(line.Trim()))
                    tableLines.Add(line);

                lastLine = line;

                foreach (var table in tables)
                {
                    tableLines.InsertRange(tableLines.IndexOf(table.Key), GetTableLines(table.Value, GetMaxWidth()));
                    tableLines.Remove(table.Key);
                }

                for (int i = 0; i < tableLines.Count; i++)
                {
                    tableLines[i] = tableLines[i].TrimEnd();
                    if ((!tableLines[i].ToLower().EndsWith("<br/>") && RemoveTag(tableLines[i]).Trim().Length > 0) || tableLines[i].Trim().Length == 0)
                        tableLines[i] += "<br/>";
                    tableLines[i] = tableLines[i].Replace(" ", "&nbsp;");
                }

                var htmlBuilder = new StringBuilder();
                foreach (var tableLine in tableLines)
                {
                    htmlBuilder.AppendLine(tableLine);
                }

                return htmlBuilder.ToString();
            }
            #endregion

            // Normal metin satırı
            return FormatTextLine(line);
        }

        private static string FormatFillLine(string line)
        {
            return string.Format("<span>{0}</span>", line[3].ToString().PadLeft(GetMaxWidth(), line[3]));
        }

        private static string FormatBoldLine(string line)
        {
            return $"<B>{RemoveTag(line)}</B>";
        }

        private static string FormatAlignedLine(string line)
        {
            var content = RemoveTag(line);

            var alignment = "left";
            if (line.StartsWith("<C"))
                alignment = "center";
            else if (line.StartsWith("<R"))
                alignment = "right";
            else if (line.StartsWith("<L"))
                alignment = "left";

            var maxWidth = GetMaxWidth();

            switch (alignment)
            {
                case "center":
                    // String uzunluğunu al
                    int stringLength = content.Length;

                    // Sol boşluk miktarını hesapla
                    int leftPadding = (maxWidth - stringLength) / 2;

                    // Eğer genişlikten büyük bir string verilirse, kırp
                    if (leftPadding < 0)
                    {
                        return content.Substring(0, maxWidth);
                    }

                    // Sol boşlukları ekle ve stringi oluştur
                    content = new string(' ', leftPadding) + content;
                    break;
                case "right":
                    content = content.PadLeft(maxWidth);
                    break;
                case "left":
                default:
                    content = content.PadRight(maxWidth);
                    break;
            }

            return string.Format("<span>{0}</span>", content.Replace(" ", "&nbsp;"));
        }

        #region Tables
        private static IEnumerable<string> GetTableLines(IList<string> lines, int maxWidth)
        {
            int colCount = GetColumnCount(lines) + 1;
            var colWidths = new int[colCount];

            var maxCol = lines.Select(x => x.Split('|').Length).Max();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Split('|').Length < maxCol)
                    lines[i] = lines[i].Replace("|", "| |");
            }

            for (int i = 0; i < colCount; i++)
            {
                colWidths[i] = GetMaxLine(lines, i) + 1;
            }

            if (colWidths.Sum() < maxWidth)
                colWidths[colCount - 1] = (maxWidth - colWidths.Sum()) + colWidths[colCount - 1];

            if (colWidths[colCount - 1] < 1) colWidths[colCount - 1] = 1;

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = string.Format("<span>{0}</span>", GetFormattedLine(lines[i], colWidths));
            }

            return lines;
        }

        private static int GetSize(string val, char sep, int index)
        {
            var parts = val.Split(sep);
            if (index > parts.Length - 1) return 0;
            return parts[index].Trim().Length;
        }

        private static int GetMaxLine(IEnumerable<string> lines, int columnNo)
        {
            return lines.Select(x => GetSize(x, '|', columnNo)).Max() + 1;
        }

        private static int GetColumnCount(IEnumerable<string> value)
        {
            return value.Select(item => item.Length - item.Replace("|", "").Length).Aggregate(0, (current, len) => len > current ? len : current);
        }

        private static string GetFormattedLine(string s, IList<int> colWidths)
        {
            var parts = s.Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i == 0)
                    parts[i] = parts[i].Trim().PadRight(colWidths[i]);
                else
                    parts[i] = parts[i].Trim().PadLeft(colWidths[i]);
            }
            return string.Join("", parts);
        }
        #endregion

        private static string FormatTextLine(string line)
        {
            return $"{line}";
        }

        private static string RemoveTag(string line)
        {
            var tagEnd = line.IndexOf('>');
            return tagEnd >= 0 ? line.Substring(tagEnd + 1) : line;
        }

        private static int GetMaxWidth()
        {
            // Maksimum genişliği belirleyin
            return 42; // Örnek değer
        }
    }
}
