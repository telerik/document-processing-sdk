using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.IO;
using Telerik.Documents.AI.Core;

#if NETWINDOWS
using Telerik.Windows.Documents.AIConnector;
#else
using Telerik.Documents.AIConnector;
#endif
using Telerik.Windows.Documents.TextRepresentation;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.Json;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace SpreadAIConnectorDemo
{
    internal class Program
    {
        static int maxTokenCount = 128000;
        static int embeddingTokenSize = 60000;
        static int totalContextTokenLimit = 60000;
        static IChatClient iChatClient;
        static string tokenizationEncoding = "cl100k_base";
        static string model = "gpt-4o-mini";
        static string key = Environment.GetEnvironmentVariable("AZUREOPENAI_KEY");
        static string endpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            CreateChatClient();

            using (Stream input = File.OpenRead("GenAI Document Insights Test Document.xlsx"))
            {
                XlsxFormatProvider xlsxFormatProvider = new XlsxFormatProvider();
                Workbook inputXlsx = xlsxFormatProvider.Import(input, null);
                SimpleTextDocument simpleDocument = inputXlsx.ToSimpleTextDocument(TimeSpan.FromSeconds(10));

                Summarize(simpleDocument);

                Console.WriteLine("--------------------------------------------------");

                AskQuestion(simpleDocument);

                Console.WriteLine("--------------------------------------------------");

                AskPartialContextQuestion(simpleDocument);
            }
        }

        private static void CreateChatClient()
        {
            AzureOpenAIClient azureClient = new(
                new Uri(endpoint),
                new Azure.AzureKeyCredential(key),
                new AzureOpenAIClientOptions());
            ChatClient chatClient = azureClient.GetChatClient(model);

            iChatClient = new OpenAIChatClient(chatClient); 
        }

        private static void Summarize(SimpleTextDocument simpleDocument)
        {
            string additionalPrompt = "Summarize the text in a few sentences. Be concise and clear.";
            SummarizationProcessorSettings summarizationProcessorSettings = new SummarizationProcessorSettings(maxTokenCount, additionalPrompt);
            SummarizationProcessor summarizationProcessor = new SummarizationProcessor(iChatClient, summarizationProcessorSettings);

            summarizationProcessor.SummaryResourcesCalculated += SummarizationProcessor_SummaryResourcesCalculated;

            string summary = summarizationProcessor.Summarize(simpleDocument).Result;
            Console.WriteLine(summary);
        }

        private static void SummarizationProcessor_SummaryResourcesCalculated(object? sender, SummaryResourcesCalculatedEventArgs e)
        {
            Console.WriteLine($"The summary will require {e.EstimatedCallsRequired} calls and {e.EstimatedTokensRequired} tokens");
            e.ShouldContinueExecution = true;
        }

        private static void AskQuestion(SimpleTextDocument simpleDocument)
        {
            CompleteContextProcessorSettings completeContextProcessorSettings = new CompleteContextProcessorSettings(maxTokenCount, model, tokenizationEncoding, false);
            CompleteContextQuestionProcessor completeContextQuestionProcessor = new CompleteContextQuestionProcessor(iChatClient, completeContextProcessorSettings);

            string question = "What content does the document contain?";
            string answer = completeContextQuestionProcessor.AnswerQuestion(simpleDocument, question).Result;
            Console.WriteLine(question);
            Console.WriteLine(answer);
        }

        private static void AskPartialContextQuestion(SimpleTextDocument simpleDocument)
        {
            IEmbeddingSettings settings = EmbeddingSettingsFactory.CreateSettingsForSpreadDocuments(maxTokenCount, model, tokenizationEncoding, embeddingTokenSize, false, totalContextTokenLimit);
#if NETWINDOWS
            PartialContextQuestionProcessor partialContextQuestionProcessor = new PartialContextQuestionProcessor(iChatClient, settings, simpleDocument);
#else
            IEmbedder embedder = new CustomOpenAIEmbedder();
            PartialContextQuestionProcessor partialContextQuestionProcessor = new PartialContextQuestionProcessor(iChatClient, embedder, settings, simpleDocument);
#endif
            string question = "What are the main conclusions of the document?";
            string answer = partialContextQuestionProcessor.AnswerQuestion(question).Result;
            Console.WriteLine(question);
            Console.WriteLine(answer);
        }
    }
}
