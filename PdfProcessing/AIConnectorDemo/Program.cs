using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.IO;
#if NETWINDOWS
using Telerik.Windows.Documents.AIConnector;
#else
using Telerik.Documents.AIConnector;
#endif
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.TextRepresentation;

namespace AIConnectorDemo
{
    internal class Program
    {
        static int maxTokenCount = 128000;
        static IChatClient iChatClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            CreateChatClient();

            using (Stream input = File.OpenRead("John Grisham.pdf"))
            {
                PdfFormatProvider pdfFormatProvider = new PdfFormatProvider();
                RadFixedDocument inputPdf = pdfFormatProvider.Import(input, null);
                ISimpleTextDocument simpleDocument = inputPdf.ToSimpleTextDocument();

                Summarize(simpleDocument);

                Console.WriteLine("--------------------------------------------------");

                AskQuestion(simpleDocument);

                Console.WriteLine("--------------------------------------------------");

                AskPartialContextQuestion(simpleDocument);
            }
        }

        private static void CreateChatClient()
        {
            string key = Environment.GetEnvironmentVariable("AZUREOPENAI_KEY");
            string endpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");
            string model = "gpt-4o-mini";

            AzureOpenAIClient azureClient = new(
                new Uri(endpoint),
                new Azure.AzureKeyCredential(key),
                new AzureOpenAIClientOptions());
            ChatClient chatClient = azureClient.GetChatClient(model);

            iChatClient = new OpenAIChatClient(chatClient);
        }

        private static void Summarize(ISimpleTextDocument simpleDocument)
        {
            SummarizationProcessor summarizationProcessor = new SummarizationProcessor(iChatClient, maxTokenCount);
            summarizationProcessor.Settings.PromptAddition = "Summarize the text in a few sentences. Be concise and clear.";
            summarizationProcessor.SummaryResourcesCalculated += SummarizationProcessor_SummaryResourcesCalculated;

            string summary = summarizationProcessor.Summarize(simpleDocument).Result;
            Console.WriteLine(summary);
        }

        private static void SummarizationProcessor_SummaryResourcesCalculated(object? sender, SummaryResourcesCalculatedEventArgs e)
        {
            Console.WriteLine($"The summary will require {e.EstimatedCallsRequired} calls and {e.EstimatedTokensRequired} tokens");
            e.ShouldContinueExecution = true;
        }

        private static void AskQuestion(ISimpleTextDocument simpleDocument)
        {
            CompleteContextQuestionProcessor completeContextQuestionProcessor = new CompleteContextQuestionProcessor(iChatClient, maxTokenCount);

            string question = "How many pages is the document and what is it about?";
            string answer = completeContextQuestionProcessor.AnswerQuestion(simpleDocument, question).Result;
            Console.WriteLine(question);
            Console.WriteLine(answer);
        }

        private static void AskPartialContextQuestion(ISimpleTextDocument simpleDocument)
        {
#if NETWINDOWS
            PartialContextQuestionProcessor partialContextQuestionProcessor = new PartialContextQuestionProcessor(iChatClient, maxTokenCount, simpleDocument);
#else
            IEmbeddingsStorage embeddingsStorage = new OllamaEmbeddingsStorage();
            PartialContextQuestionProcessor partialContextQuestionProcessor = new PartialContextQuestionProcessor(iChatClient, embeddingsStorage, maxTokenCount, simpleDocument);
#endif
            string question = "What is the last book by John Grisham?";
            string answer = partialContextQuestionProcessor.AnswerQuestion(question).Result;
            Console.WriteLine(question);
            Console.WriteLine(answer);
        }
    }
}
