using System.Net;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SpamFilter.Tests
{
    [TestFixture]
    internal class RegexPatternShould
    {
        private Regex pattern;

        [SetUp]
        public void SetUp()
        {
            pattern = new Regex("<[^>]*(>|$)", RegexOptions.Compiled);
        }

        [TestCase("<html>inner text</html>", 
            ExpectedResult = "inner text", TestName = "Tags")]
        [TestCase("<p style=\"red\" font=\"Arial\">inner text</p>", 
            ExpectedResult = "inner text", TestName = "Tags and attributes")]
        public string RomoveHtmlTags(string html)
        {
            return WebUtility.HtmlDecode(pattern.Replace(html, string.Empty));
        }
    }
}
