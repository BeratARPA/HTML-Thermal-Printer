using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrintHTML.Core.Formatters
{
    public class FormattedDocument
    {
        private readonly IList<ILineFormatter> _lineFormatters;
        private static int[] _lastColumnWidths;

        public FormattedDocument(IEnumerable<string> documentLines, int maxWidth)
        {
            _lineFormatters = new List<ILineFormatter>();

            foreach (var documentLine in documentLines)
            {
                var lineFormatter = CreateLineFormatter(documentLine, maxWidth);
                if (_lineFormatters.Count > 0 && _lineFormatters[_lineFormatters.Count - 1].Tag.TagName == "j" && lineFormatter.Tag.TagName != "j")
                    _lastColumnWidths = null;
                _lineFormatters.Add(lineFormatter);
            }
        }

        private static ILineFormatter CreateLineFormatter(string documentLine, int maxWidth)
        {
            if (CompositeFormatter.HasMultipleTags(documentLine))
                return new CompositeFormatter(documentLine, maxWidth);

            // Tek etiket için mevcut formatter'ları kullan
            var lowerLine = documentLine.ToLower().TrimStart();

            if (lowerLine.StartsWith("<l"))
                return new LeftAlignFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<r"))
                return new RightAlignFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<c"))
                return new CenterAlignFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<f"))
                return new HorizontalRuleFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<t"))
                return new BoldFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<bx"))
                return new BoxFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<j"))
                return GetJustifiedFormatter(documentLine, maxWidth, false);
            if (lowerLine.StartsWith("<eb") || lowerLine.StartsWith("<db"))
                return new MultiLineBoldFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<ascii"))
                return new AsciiFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<qr"))
                return new QRCodeFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<barcode"))
                return new BarcodeFormatter(documentLine, maxWidth);
            if (lowerLine.StartsWith("<picture"))
                return new ImageFormatter(documentLine, maxWidth);

            return new GenericFormatter(documentLine, maxWidth);
        }

        private static ILineFormatter GetJustifiedFormatter(string documentLine, int maxWidth, bool shouldBreak)
        {
            var match = Regex.Match(documentLine, "<[j|J][^:]+(:[^>]+)>");
            var mt = match.Success ? match.Groups[1].Value : "";
            var ratio = 1d;
            if (!string.IsNullOrEmpty(mt))
            {
                documentLine = documentLine.Replace(mt + ">", ">");
                ratio = Convert.ToDouble(mt.Trim(':'));
            }
            var fmtr = new JustifyAlignFormatter(documentLine, maxWidth, shouldBreak, ratio, _lastColumnWidths);
            _lastColumnWidths = fmtr.GetColumnWidths();
            return fmtr;
        }

        public IEnumerable<string> GetFormattedDocument()
        {
            return _lineFormatters.Select(x => x.GetFormattedLine());
        }
      
        public string GetFormattedText()
        {
            return _lineFormatters
                .Select(x => x.GetFormattedLineWithoutTags())
                .Aggregate("", (current, s) => current + s);
        }

        internal string GetSeparator(string current)
        {
            return !string.IsNullOrEmpty(current) ? "\r\n" : "";
        }

        public IEnumerable<ILineFormatter> GetFormatters()
        {
            return _lineFormatters;
        }
    }
}
