namespace PrintHTML.Core.Formatters
{
    internal class GenericFormatter : AbstractLineFormatter
    {
        public GenericFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        { }

        public override string GetFormattedLine()
        {
            if (IsDefinedTag(Line))
            {
                var result = Line;
                return !string.IsNullOrWhiteSpace(result) ? result + "<br/>" : "<br/>";
            }
            else
            {
                var result = Line;
                return !string.IsNullOrWhiteSpace(result) ? result : "";
            }
        }
    }
}