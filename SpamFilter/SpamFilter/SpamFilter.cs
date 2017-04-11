using MimeKit;
using SpamFilter.SpamFiltering;

namespace SpamFilter.SpamFilter
{
    public class SpamFilter
    {
        public BayesClassifier Classifier { get; }

        public SpamFilter(BayesClassifier classifier)
        {
            Classifier = classifier;
        }

        public bool IsSpam(MimeMessage message)
        {
            var result = Classifier.Classify(message, TextType.Spam);

            return result >= 0.5;
        }
    }
}
