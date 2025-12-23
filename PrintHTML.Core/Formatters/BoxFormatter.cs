using System.Text;

namespace PrintHTML.Core.Formatters
{
    internal class BoxFormatter : AbstractLineFormatter
    {
        public BoxFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            return PrintWindow(Line, false);
        }

        private string PrintWindow(string line, bool expandLabel)
        {
            const string tl = "┌";
            const string tr = "┐";
            const string bl = "└";
            const string br = "┘";
            const string vl = "│";
            const string hl = "─";
            const char s = ' '; // Normal boşluk kullanıp en son replace yapacağız

            if (expandLabel) line = ExpandLabel(line);

            // 1. Üst Çizgi (Top Line)
            // (MaxWidth - 2) kadar çizgi, başında ve sonunda köşe karakterleri
            string topLine = tl + new string(hl[0], MaxWidth - 2) + tr;

            // 2. Orta Satır (Text Line)
            // Metni ortalamak için padding hesapla
            int totalPadding = (MaxWidth - 2) - line.Length;
            int leftPad = totalPadding / 2;
            int rightPad = totalPadding - leftPad; // Kalanı sağa ver

            string middleLine = vl +
                                new string(s, leftPad) +
                                line +
                                new string(s, rightPad) +
                                vl;

            // 3. Alt Çizgi (Bottom Line)
            string bottomLine = bl + new string(hl[0], MaxWidth - 2) + br;

            // HTML Formatına Çevir
            // Boşlukları &nbsp; ile değiştir ki HTML'de daralmasın
            // Font Consolas olduğu için &nbsp; genişliği harf genişliğine eşittir.
            var sb = new StringBuilder();

            sb.Append("<span>" + topLine.Replace(" ", "&nbsp;") + "</span><br/>");
            sb.Append("<span>" + middleLine.Replace(" ", "&nbsp;") + "</span><br/>");
            sb.Append("<span>" + bottomLine.Replace(" ", "&nbsp;") + "</span><br/>");

            return sb.ToString();
        }
    }
}