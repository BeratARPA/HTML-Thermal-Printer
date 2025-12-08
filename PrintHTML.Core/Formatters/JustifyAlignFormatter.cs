using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintHTML.Core.Formatters
{
    internal class JustifyAlignFormatter : AbstractLineFormatter
    {
        private readonly bool _canBreak;
        private readonly double _ratio;
        private readonly int[] _columnWidths;

        public JustifyAlignFormatter(string documentLine, int maxWidth, bool canBreak, double ratio, int[] columnWidths = null) :
            base(documentLine, maxWidth)
        {
            _canBreak = canBreak;
            _ratio = ratio;
            _columnWidths = CalculateColumnWidths(documentLine, columnWidths);
        }

        private static int[] CalculateColumnWidths(string documentLine, int[] columnWidths)
        {
            // Tag'ı kaldır
            var cleanLine = documentLine;
            if (cleanLine.ToLower().TrimStart().StartsWith("<j"))
            {
                var tagEnd = cleanLine.IndexOf('>');
                if (tagEnd >= 0)
                    cleanLine = cleanLine.Substring(tagEnd + 1);
            }

            var parts = cleanLine.Split('|');
            if (columnWidths == null || columnWidths.Length != parts.Length)
                columnWidths = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                var partLength = GetLength(parts[i].Trim());
                if (columnWidths[i] < partLength)
                    columnWidths[i] = partLength;
            }
            return columnWidths;
        }

        public override string GetFormattedLine()
        {
            return JustifyText(MaxWidth, Line, _canBreak, _columnWidths);
        }

        private string JustifyText(int maxWidth, string line, bool canBreak, IList<int> columnWidths)
        {
            // Satırı '|' karakterine göre sütunlara ayırıyoruz.
            string[] columns = line.Split('|');
            if (columns.Length == 0)
                return line + "<br/>";

            int totalColumnWidth = columnWidths.Sum();
            int columnCount = columns.Length;

            // Kolonlar arası minimum boşluk (en az 2 karakter)
            int minGap = 2;
            int totalMinGaps = (columnCount - 1) * minGap;

            // Kalan boşluğu hesapla
            int remainingSpace = maxWidth - totalColumnWidth - totalMinGaps;

            // Ekstra boşluğu kolonlar arasına eşit dağıt
            int extraSpacePerGap = columnCount > 1 ? remainingSpace / (columnCount - 1) : 0;
            int extraSpaceRemainder = columnCount > 1 ? remainingSpace % (columnCount - 1) : 0;

            if (extraSpacePerGap < 0) extraSpacePerGap = 0;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < columns.Length; i++)
            {
                string colText = columns[i].Trim();
                int targetWidth = columnWidths[i];

                // Kolonu sabit genişliğe getir (sağa boşluk ekle)
                string paddedCol = PadRight(colText, targetWidth);
                sb.Append(paddedCol);

                // Son kolon değilse, aradaki boşluğu ekle
                if (i < columns.Length - 1)
                {
                    int gapSize = minGap + extraSpacePerGap + (i < extraSpaceRemainder ? 1 : 0);
                    for (int j = 0; j < gapSize; j++)
                    {
                        sb.Append("&nbsp;");
                    }
                }
            }

            return sb.ToString() + "<br/>";
        }

        private string PadRight(string text, int totalWidth)
        {
            int currentLength = GetLength(text);
            StringBuilder sb = new StringBuilder(text);

            while (currentLength < totalWidth)
            {
                sb.Append("&nbsp;");
                currentLength++;
            }

            return sb.ToString();
        }

        public int[] GetColumnWidths()
        {
            return _columnWidths;
        }
    }
}