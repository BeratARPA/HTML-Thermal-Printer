namespace PrintHTML.Core.Formatters
{
    internal class BoldFormatter : AbstractLineFormatter
    {
        public BoldFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {

        }

        public override string GetFormattedLine()
        {
            return PrintBoldLabel(Line);
        }

        private string PrintBoldLabel(string label)
        {
            return $"<b>{label}</b><br/>";
        }
    }
}