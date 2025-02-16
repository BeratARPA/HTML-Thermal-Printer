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
            var parts = documentLine.Split('|');
            if (columnWidths == null || columnWidths.Count() != parts.Length)
                columnWidths = new int[parts.Count()];
            for (int i = 0; i < parts.Count(); i++)
            {
                if (columnWidths[i] < GetLength(parts[i]))
                    columnWidths[i] = GetLength(parts[i]);
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
                return line;

            // Her sütunun uzunluğunu hesaplayalım.
            int totalTextLength = 0;
            foreach (var col in columns)
            {
                totalTextLength += col.Length;
            }

            int gapCount = columns.Length - 1;
            int totalSpaces = maxWidth - totalTextLength;

            // Eğer boşluk miktarı yetersizse, direk orijinal metni döndür.
            //if (totalSpaces < gapCount)
            //    return line; // Bu durumda satır genişliği aşılmıştır.

            // Her boşluk için temel boşluk sayısı ve fazladan boşluk sayısını hesapla.
            int baseSpace = gapCount > 0 ? totalSpaces / gapCount : 0;
            int extraSpace = gapCount > 0 ? totalSpaces % gapCount : 0;

            // Sütunları ve aralarına &nbsp; ekleyerek yeni satırı oluştur.
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                sb.Append(columns[i]);
                if (i < gapCount)
                {
                    int gap = baseSpace + (i < extraSpace ? 1 : 0);
                    // Her boşluk yerine gap kadar "&nbsp;" ekle.
                    for (int j = 0; j < gap; j++)
                    {
                        sb.Append("&nbsp;");
                    }
                }
            }

            return sb.ToString() + "<br/>";
        }

        public int[] GetColumnWidths()
        {
            return _columnWidths;
        }
    }
}