﻿using System;
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
            if (documentLine.ToLower().TrimStart().StartsWith("<l"))
                return new LeftAlignFormatter(documentLine, maxWidth);
            if (documentLine.ToLower().TrimStart().StartsWith("<r"))
                return new RightAlignFormatter(documentLine, maxWidth);
            if (documentLine.ToLower().TrimStart().StartsWith("<c"))
                return new CenterAlignFormatter(documentLine, maxWidth);
            if (documentLine.ToLower().TrimStart().StartsWith("<f>"))
                return new HorizontalRuleFormatter(documentLine, maxWidth);
            if (documentLine.ToLower().TrimStart().StartsWith("<t"))
                return new BoldFormatter(documentLine, maxWidth);
            if (documentLine.ToLower().TrimStart().StartsWith("<bx"))
                return new BoxFormatter(documentLine, maxWidth);
            if (documentLine.ToLower().TrimStart().StartsWith("<j"))
                return GetJustifiedFormatter(documentLine, maxWidth, false);           
            if (documentLine.ToLower().TrimStart().StartsWith("<eb>") || documentLine.ToLower().StartsWith("<db>"))
                return new MultiLineBoldFormatter(documentLine, maxWidth);
            if (documentLine.ToLower().TrimStart().StartsWith("<ascii>"))
                return new AsciiFormatter(documentLine, maxWidth);            

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
                .Where(x => !string.IsNullOrEmpty(x))
                .Aggregate("", (current, s) => current + GetSeparator(current) + s);
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
