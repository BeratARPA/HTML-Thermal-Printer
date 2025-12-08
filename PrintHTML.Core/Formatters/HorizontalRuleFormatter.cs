namespace PrintHTML.Core.Formatters
{
    internal class HorizontalRuleFormatter : AbstractLineFormatter
    {
        public HorizontalRuleFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            if (string.IsNullOrEmpty(Line))
                return "";

            char fillChar = Line.Length > 0 ? Line[0] : ' ';
            string result = new string(fillChar, MaxWidth);

            return $"<span>{result}</span><br/>";
        }
    }
}