using System;
using System.Collections.Generic;
using System.Linq;
using MimeKit;

namespace SpamFilter.SpamFiltering
{
    public class BayesClassifier
    {
        private readonly Dictionary<TextType, int> trainDatasetCount;
        public readonly Dictionary<TextType, Dictionary<string, int>> wordCounter;
        private readonly EmailProcessor emailProcessor;

        public BayesClassifier()
        {
            emailProcessor = new EmailProcessor();
            trainDatasetCount = new Dictionary<TextType, int>();
            wordCounter = new Dictionary<TextType, Dictionary<string, int>>
            {
                [TextType.Spam] = new Dictionary<string, int>(),
                [TextType.Ham] = new Dictionary<string, int>()
            };
        }

        public double Classify(MimeMessage message, TextType textClass)
        {
            var words = emailProcessor.ProcessMessage(message);

            var h = words.Where(w => wordCounter.All(c => c.Value.ContainsKey(w))).
                Select(word => GetProbability(textClass, word)).
                Select(p => Math.Log((1 - p) - Math.Log(p))).
                Sum();

            return 1 / (1 + Math.Exp(h));

        }

        public void Train(List<MimeMessage> messages, TextType type)
        {
            trainDatasetCount[type] = messages.Count;
            messages.ForEach(msg => emailProcessor.ProcessMessage(msg).
                     ForEach(word =>
                     {
                         if (!wordCounter[type].ContainsKey(word))
                             wordCounter[type].Add(word, 0);
                         wordCounter[type][word]++;
                     }));
        }

        public double GetProbability(TextType type, string word)
        {
            if (!wordCounter[type].ContainsKey(word))
                return 0;

            var numberOfHam = wordCounter[TextType.Ham].ContainsKey(word)
                ? wordCounter[TextType.Ham][word]
                : 0;
            var spamFrequency = wordCounter[TextType.Spam][word] * 1.0 / trainDatasetCount[TextType.Spam];
            var hamFrequency = numberOfHam * 1.0 / trainDatasetCount[TextType.Ham];

            return type == TextType.Spam
                ? spamFrequency / (spamFrequency + hamFrequency)
                : hamFrequency / (spamFrequency + hamFrequency);
        }

        public IEnumerable<Tuple<string, double, double>> GetValuableWords(int count)
        {
            return wordCounter[TextType.Spam].Keys.
                Where(k => wordCounter.All(dic => dic.Value.ContainsKey(k))).
                Select(k => Tuple.Create(k, GetProbability(TextType.Spam, k), GetProbability(TextType.Ham, k))).
                OrderByDescending(t => Math.Max(t.Item2 / t.Item3, t.Item3 / t.Item2)).
                Take(count);
        }
    }
}