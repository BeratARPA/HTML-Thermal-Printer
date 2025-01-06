using System.IO;
using System.Net;
using System.Xml;

namespace PrintHTML.Core.HtmlConverter
{
    public class HtmlEncodedTextWriter : XmlTextWriter
    {
        public HtmlEncodedTextWriter(TextWriter w) : base(w) { }

        #region Overrides of XmlTextWriter

        /// <inheritdoc />
        public override void WriteString(string text)
        {
            text = WebUtility.HtmlEncode(text);
            WriteRaw(text);
        }

        #endregion
    }
}
