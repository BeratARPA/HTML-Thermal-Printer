﻿using System.Text;

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
            const string s = " ";

            if (expandLabel) line = ExpandLabel(line);

            var sb = new StringBuilder();

            sb.AppendLine(tl + hl.PadLeft(MaxWidth - 2, hl[0]) + tr + "<br/>") ;

            string text = vl + line.PadLeft((((MaxWidth - 2) + line.Length) / 2), s[0]);

            sb.AppendLine(text + vl.PadLeft(MaxWidth - text.Length, s[0]) + "<br/>");

            sb.Append(bl + hl.PadLeft(MaxWidth - 2, hl[0]) + br);

            return $"{sb.ToString()}<br/>";
        }
    }
}