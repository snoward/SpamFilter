using System.Collections.Generic;
using System.IO;
using System.Linq;
using MimeKit;

namespace SpamFilter.Tests
{
    class ClassifierTests
    {
        private static List<MimeMessage> ParseDatasetPartially(string filePath, double skipCount, double takeCount)
        {
            var folder = Directory.GetFiles(filePath);
            var files = folder.Skip((int)(folder.Length * skipCount)).Take((int)(folder.Length * takeCount));

            return files.Select(filename => $"{filePath}\\{filename}").
                         Select(path => MimeMessage.Load(path)).ToList();
        }
    }
}
