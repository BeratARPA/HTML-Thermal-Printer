using System;

namespace PrintHTML.Core.Formatters
{
    internal class MultiLineBoldFormatter : AbstractLineFormatter
    {
        public MultiLineBoldFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            // <eb> etiketi bulunmuşsa <b> ile değiştir
            if (Tag.Tag.Trim().StartsWith("<eb>", StringComparison.OrdinalIgnoreCase))
                Line = Tag.Tag.ToLower().Replace("<eb>", "<b>");

            // <db> etiketi bulunmuşsa </b> ile değiştir
            if (Tag.Tag.Trim().StartsWith("<db>", StringComparison.OrdinalIgnoreCase))
                Line = Tag.Tag.ToLower().Replace("<db>", "</b>")+ "<br/>";

            // Satırı döndür
            return $"<span>{Line}</span>";          
        }
    }
}
