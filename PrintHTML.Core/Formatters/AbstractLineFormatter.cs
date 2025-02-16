﻿using System.Globalization;
using System.Text.RegularExpressions;

namespace PrintHTML.Core.Formatters
{
    public abstract class AbstractLineFormatter : ILineFormatter
    {
        public int FontWidth { get; set; }
        public int FontHeight { get; set; }
        protected string Line { get; set; }
        private int _maxWidth;
        protected int MaxWidth
        {
            get { return _maxWidth / (FontWidth + 1); }
            set { _maxWidth = value; }
        }

        public FormatTag Tag { get; set; }

        private static string RemoveTag(string line)
        {
            if (IsDefinedTag(line))
                return line.Substring(line.IndexOf(">", System.StringComparison.Ordinal) + 1);

            return line;
        }

        protected static bool IsDefinedTag(string line)
        {
            // Etiketleri tanımlamak için bir regex oluştur
            var regex = new Regex(@"<(l|r|c|f|t|bx|j|p|eb|db|ascii)>", RegexOptions.IgnoreCase);

            // Etiketleri sırayla işle
            var match = regex.Match(line);
            return match.Success;
        }

        protected AbstractLineFormatter(string documentLine, int maxWidth)
        {
            Tag = new FormatTag(documentLine);
            MaxWidth = maxWidth;
            FontWidth = Tag.Width;
            FontHeight = Tag.Height;
            Line = RemoveTag(documentLine);
        }

        protected static string ExpandLabel(string label)
        {
            var result = "";
            for (var i = 0; i < label.Length - 1; i++)
            {
                result += label[i] + " ";
            }
            result += label[label.Length - 1];
            return " " + result.Trim() + " ";
        }

        protected static int GetLength(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            return new StringInfo(str).LengthInTextElements;
        }

        protected static string SubStr(string str, int length)
        {
            return new StringInfo(str).SubstringByTextElements(0, length);
        }

        protected static string ExpandStrRight(string str, int lenght)
        {
            str = str.TrimEnd();
            while (GetLength(str) < lenght)
                str = str + "&nbsp;";
            return str;
        }

        protected static string ExpandStrLeft(string str, int lenght)
        {
            str = str.TrimStart();
            while (GetLength(str) < lenght)
                str = "&nbsp;" + str;
            return str;
        }

        protected string GetStrAt(string str, int index)
        {
            return new StringInfo(str).SubstringByTextElements(index, 1);
        }

        public abstract string GetFormattedLine();

        public string GetFormattedLineWithoutTags()
        {
            var result = GetFormattedLine();

            if (IsDefinedTag(result))
            {
                if (!string.IsNullOrEmpty(Tag.TagName))
                    return result = result.Replace(Tag.Tag, "");
            }

            return result;
        }
    }
}
