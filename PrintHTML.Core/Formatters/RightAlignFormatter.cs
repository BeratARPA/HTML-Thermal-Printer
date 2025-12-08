namespace PrintHTML.Core.Formatters
{
    internal class RightAlignFormatter : AbstractLineFormatter
    {
        public RightAlignFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            var result = Line.PadLeft(MaxWidth, ' ');
            return $"<span>{result.Replace(" ", "&nbsp;")}</span><br/>";
        }
    }
}
