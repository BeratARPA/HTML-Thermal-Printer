using Figgle;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PrintHTML.Core.Formatters
{
    internal class CompositeFormatter : AbstractLineFormatter
    {
        private readonly HashSet<string> _styleTags;      // t, eb, db
        private readonly string _alignmentTag;             // l, c, r
        private readonly string _specialTag;               // f, j, ascii (bx kaldırıldı)
        private readonly string _content;
        private readonly int _maxWidthValue;

        // Tüm desteklenen etiketler (bx kaldırıldı)
        private static readonly HashSet<string> StyleTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "t", "eb", "db" };
        private static readonly HashSet<string> AlignmentTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "l", "c", "r" };
        private static readonly HashSet<string> SpecialTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "f", "j", "ascii" };

        public CompositeFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
            _maxWidthValue = MaxWidth;
            _styleTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _content = ExtractTagsAndContent(documentLine, out _alignmentTag, out _specialTag);
        }

        private string ExtractTagsAndContent(string line, out string alignmentTag, out string specialTag)
        {
            alignmentTag = null;
            specialTag = null;

            // Tüm etiketleri bul (boyut bilgisi dahil) - bx hariç
            var tagPattern = new Regex(@"<(t|l|c|r|eb|db|f|j|ascii)(\d{0,2})>", RegexOptions.IgnoreCase);
            var matches = tagPattern.Matches(line);

            string remainingContent = line;

            foreach (Match match in matches)
            {
                string tagName = match.Groups[1].Value.ToLower();

                if (StyleTags.Contains(tagName))
                {
                    _styleTags.Add(tagName);
                }
                else if (AlignmentTags.Contains(tagName))
                {
                    alignmentTag = tagName;
                }
                else if (SpecialTags.Contains(tagName))
                {
                    specialTag = tagName;
                }

                // Etiketi içerikten kaldır
                remainingContent = remainingContent.Replace(match.Value, "");
            }

            return remainingContent.Trim();
        }

        public override string GetFormattedLine()
        {
            string result = _content;

            // 1. Önce özel etiketleri işle (J, F, ASCII)
            if (!string.IsNullOrEmpty(_specialTag))
            {
                result = ApplySpecialFormatting(result, _specialTag);
            }
            else
            {
                // 2.  Hizalama uygula (sadece özel etiket yoksa)
                result = ApplyAlignment(result, _alignmentTag ?? "l");
                result = $"<span>{result}</span><br/>";
            }

            // 3. Stil etiketlerini uygula (bold)
            result = ApplyStyleTags(result);

            return result;
        }

        private string ApplyStyleTags(string content)
        {
            string openingTags = "";
            string closingTags = "";

            // <t> etiketi - tam bold (açılış ve kapanış)
            if (_styleTags.Contains("t"))
            {
                openingTags += "<b>";
                closingTags = "</b>" + closingTags;
            }

            // <eb> etiketi - enable bold (sadece açılış, sonraki satırlara devam eder)
            if (_styleTags.Contains("eb"))
            {
                openingTags += "<b>";
            }

            // <db> etiketi - disable bold (sadece kapanış)
            if (_styleTags.Contains("db"))
            {
                closingTags = "</b>" + closingTags;
            }

            return openingTags + content + closingTags;
        }

        private string ApplyAlignment(string text, string alignment)
        {
            int textLength = GetLength(text);

            switch (alignment.ToLower())
            {
                case "c":
                    int totalPadding = _maxWidthValue - textLength;
                    int leftPad = Math.Max(0, totalPadding / 2);
                    int rightPad = Math.Max(0, totalPadding - leftPad);
                    return new string(' ', leftPad).Replace(" ", "&nbsp;") +
                           text +
                           new string(' ', rightPad).Replace(" ", "&nbsp;");

                case "r":
                    int rightPadding = _maxWidthValue - textLength;
                    return new string(' ', Math.Max(0, rightPadding)).Replace(" ", "&nbsp;") + text;

                case "l":
                default:
                    int leftPadding = _maxWidthValue - textLength;
                    return text + new string(' ', Math.Max(0, leftPadding)).Replace(" ", "&nbsp;");
            }
        }

        private string ApplySpecialFormatting(string content, string specialTag)
        {
            switch (specialTag.ToLower())
            {
                case "f":
                    // Horizontal rule - karakterle doldur
                    char fillChar = !string.IsNullOrEmpty(content) ? content[0] : '-';
                    return $"<span>{new string(fillChar, _maxWidthValue)}</span><br/>";

                case "j":
                    // Justify/Table formatter
                    return CreateJustifiedLine(content);

                case "ascii":
                    // ASCII art
                    return CreateAsciiArt(content);

                default:
                    return content + "<br/>";
            }
        }

        private string CreateJustifiedLine(string content)
        {
            string[] columns = content.Split('|');
            if (columns.Length <= 1)
            {
                string aligned = ApplyAlignment(content, _alignmentTag ?? "l");
                return $"<span>{aligned}</span><br/>";
            }

            // Basit tablo oluştur
            int colWidth = _maxWidthValue / columns.Length;
            var sb = new StringBuilder();

            for (int i = 0; i < columns.Length; i++)
            {
                string col = columns[i].Trim();
                if (i < columns.Length - 1)
                    sb.Append(ExpandStrRight(col, colWidth));
                else
                    sb.Append(col);
            }

            return $"<span>{sb}</span><br/>";
        }

        private string CreateAsciiArt(string content)
        {
            try
            {
                var asciiArt = FiggleFonts.Standard.Render(content);
                var sb = new StringBuilder();

                foreach (var line in asciiArt.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string formattedLine = line.Replace(" ", "&nbsp;");

                    // Hizalama uygula
                    if (_alignmentTag?.ToLower() == "c")
                    {
                        int padding = (_maxWidthValue - line.Length) / 2;
                        formattedLine = new string(' ', Math.Max(0, padding)).Replace(" ", "&nbsp;") + formattedLine;
                    }
                    else if (_alignmentTag?.ToLower() == "r")
                    {
                        int padding = _maxWidthValue - line.Length;
                        formattedLine = new string(' ', Math.Max(0, padding)).Replace(" ", "&nbsp;") + formattedLine;
                    }

                    sb.Append(formattedLine + "<br/>");
                }

                return $"<pre>{sb}</pre>";
            }
            catch
            {
                return content + "<br/>";
            }
        }

        /// <summary>
        /// Satırda birden fazla etiket olup olmadığını kontrol eder.
        /// BX etiketi varsa false döner (tek etiket formatter'ı kullanılmalı).
        /// </summary>
        public static bool HasMultipleTags(string line)
        {
            // BX etiketi varsa composite formatter kullanma
            if (Regex.IsMatch(line, @"<bx(\d{0,2})>", RegexOptions.IgnoreCase))
            {
                return false;
            }

            var tagPattern = new Regex(@"<(t|l|c|r|eb|db|f|j|ascii)(\d{0,2})>", RegexOptions.IgnoreCase);
            return tagPattern.Matches(line).Count > 1;
        }
    }
}