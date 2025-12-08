namespace PrintHTML.Core.Formatters
{
    internal class LeftAlignFormatter : AbstractLineFormatter
    {
        public LeftAlignFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        { }


        public override string GetFormattedLine()
        {
            var result = Line.PadRight(MaxWidth, ' ');
            return $"<span>{result.Replace(" ", "&nbsp;")}</span><br/>";
        }
    }
}