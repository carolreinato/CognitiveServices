using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Rest;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using System.IO;
using System.Text;
using CognitiveServices.Models;

namespace CognitiveServices
{
    class ApiKeyServiceClientCreds : ServiceClientCredentials
    {
        private readonly string _subscriptionKey;
        public ApiKeyServiceClientCreds(string subscriptionKey)
        {
            _subscriptionKey = subscriptionKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentException("request");
            }
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    class Program
    {
        private const string CogServicesSecret = "<Secret Key>";
        private const string Endpoint = "https://atividademlcarol.cognitiveservices.azure.com/";

        private static readonly string TextFile = "TextFile.txt";

        static void Main()
        {
            Console.WriteLine("************* Exemplos de serviços cognitivos *************");
            Console.WriteLine();

            var content = ModerateText(TextFile).Result;
            DetectSentiment(content.Language, content.Text).Wait();
        }

        public static async Task<TextModerationModel> ModerateText(string inputFile)
        {
            var credentials = new ApiKeyServiceClientCreds(CogServicesSecret);

            var text = File.ReadAllText(inputFile);
            text = text.Replace(Environment.NewLine, " ");

            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(textBytes);

            var client = new ContentModeratorClient(credentials)
            {
                Endpoint = Endpoint
            };

            var screenResult = await client.TextModeration.ScreenTextAsync("text/plain", stream, null, true, true, null, true);
            Console.WriteLine("Analisando...");
            if (screenResult.PII.Email.Count > 0 || screenResult.PII.Address.Count > 0 || screenResult.PII.Phone.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Seu texto contém informações pessoais! Cuidado ao compartilhá-las!");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Parabéns por não compartilhar informações pessoais!");
                Console.WriteLine();
            }

            var result = new TextModerationModel
            {
                Language = screenResult.Language.Remove(screenResult.Language.Length-1),
                Text = screenResult.AutoCorrectedText
            };
            return result;
        }

        public static async Task DetectSentiment(string language, string text)
        {
            var credentials = new ApiKeyServiceClientCreds(CogServicesSecret);
            var sentimentMeaning = "";

            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = Endpoint
            };

            var results = await client.SentimentAsync(text, language);

            Console.WriteLine("**** Análise de sentimentos ****");

            sentimentMeaning = results.Score > 0.5 ? "Positivo" : "Negativo";

            Console.WriteLine($"O sentimento do texto é {sentimentMeaning}, Score de sentimento: {results.Score}");
            Console.WriteLine();
        }
    }
}
