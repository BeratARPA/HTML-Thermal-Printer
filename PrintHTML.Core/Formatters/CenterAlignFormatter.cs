namespace PrintHTML.Core.Formatters
{
    internal class CenterAlignFormatter : AbstractLineFormatter
    {
        public CenterAlignFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            var result = Line.PadLeft(((MaxWidth + Line.Length) / 2), ' ').PadRight(MaxWidth);

            return $"<span>{result.Replace(" ", "&nbsp;")}</span><br/>";
        }
    }
}
