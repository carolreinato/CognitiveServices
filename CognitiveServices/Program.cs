using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Rest;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using System.IO;
using System.Text;
using Newtonsoft.Json;

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
        private const string CogServicesSecret = "ec9917d1f873431caf2fc6c5dbd3b57d";
        private const string Endpoint = "https://atividademlcarol.cognitiveservices.azure.com/";

        private static readonly string TextFile = "TextFile.txt";
        private static string TextOutputFile = "TextModerationOutput.txt";

        static void Main()
        {
            Console.WriteLine("Exemplos cognitivos");
            DetectLanguage().Wait();
            DetectSentiment().Wait();
            ModerateText(TextFile, TextOutputFile);
        }

        public static async Task DetectLanguage()
        {
            var credentials = new ApiKeyServiceClientCreds(CogServicesSecret);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = Endpoint
            };

            var inputData = new LanguageBatchInput(
                new List<LanguageInput>
                {
                    new LanguageInput("1","J'aime programmer et développer des systèmes."),
                    new LanguageInput("2","I love to program and develop systems."),
                    new LanguageInput("3","Me encanta programar y desarrollar sistemas.")
                });

            var results = await client.DetectLanguageBatchAsync(inputData);

            Console.WriteLine("**** Detecção de idiomas ****");
            foreach (var item in results.Documents)
            {
                Console.WriteLine($"IdDocumento: {item.Id}, Idioma: {item.DetectedLanguages[0].Name}");
            }
            Console.WriteLine();
        }

        public static async Task DetectSentiment()
        {
            var credentials = new ApiKeyServiceClientCreds(CogServicesSecret);
            var sentimentMeaning = "";

            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = Endpoint
            };

            var inputData = new MultiLanguageBatchInput(
                new List<MultiLanguageInput>
                {
                    new MultiLanguageInput("1", "I hate it here.", "en"),
                    new MultiLanguageInput("2", "What a wonderful picture.", "en"),
                    new MultiLanguageInput("3", "I'm so confused.", "en")
                });

            var results = await client.SentimentBatchAsync(inputData);

            Console.WriteLine("**** Análise de sentimentos ****");
            foreach (var item in results.Documents)
            {
                if (item.Score > 0.5)
                {
                    sentimentMeaning = "Positivo";
                }
                else
                {
                    sentimentMeaning = "Negativo";
                }

                Console.WriteLine($"IdDocumento: {item.Id} é {sentimentMeaning}, Score de sentimento: {item.Score}");
            }
            Console.WriteLine();
        }

        public static void ModerateText(string inputFile, string outputFile)
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

            //using (StreamWriter outputWriter = new StreamWriter(outputFile, false))
            //{
            using (client)
            {
                // Screen the input text: check for profanity, classify the text into three categories,
                // do autocorrect text, and check for personally identifying information (PII)
                //outputWriter.WriteLine("Autocorrect typos, check for matching terms, PII, and classify.");

                // Moderate the text
                var screenResult = client.TextModeration.ScreenText("text/plain", stream, "eng", true, true, null, true);
                //outputWriter.WriteLine(JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                Console.WriteLine($"O texto auto corrigido é: {screenResult.AutoCorrectedText}");
            }

            //outputWriter.Flush();
            //outputWriter.Close();
            //}

            Console.WriteLine();
        }
    }
}
