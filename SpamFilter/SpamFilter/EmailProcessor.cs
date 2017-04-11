using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MimeKit;
using NHunspell;

namespace SpamFilter.SpamFiltering
{
    internal class EmailProcessor : IDisposable
    {
        private readonly Regex wordPattern = new Regex("[a-zA-Z|\'|]+", RegexOptions.Compiled);
        private readonly Regex tagsRemover = new Regex("<[^>]*(>|$)", RegexOptions.Compiled);
        private readonly HashSet<string> stopWords;
        private readonly Hunspell speller;

        public EmailProcessor(string stopwordsPath = "stopWords.txt")
        {
            speller = new Hunspell("en_US.aff", "en_US.dic");
            stopWords = new HashSet<string>(File.ReadAllLines(stopwordsPath));
        }

        public List<string> ProcessMessage(MimeMessage message)
        {
            var messageText = message.TextBody ?? message.HtmlBody ?? string.Empty;
            messageText = RemoveHtmlEntities(messageText);

            return wordPattern.Matches(messageText).OfType<Match>()
                .Select(m => m.Value.ToLower())
                .Where(speller.Spell)
                .Select(w => speller.Stem(w).Last())
                .Distinct()
                .Except(stopWords)
                .ToList();
        }
        

        private string RemoveHtmlEntities(string message)
        {
            return WebUtility.HtmlDecode(tagsRemover.Replace(message, string.Empty));
        }

        public void Dispose()
        {
            speller?.Dispose();
        }
    }
}
