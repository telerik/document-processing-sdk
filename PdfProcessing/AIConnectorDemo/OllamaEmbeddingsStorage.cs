using LangChain.Databases;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using LangChain.Providers.Ollama;
using Telerik.Documents.AIConnector;

namespace AIConnectorDemo
{
    // Necessary steps to get this working:
    // 1. Install Ollama: https://ollama.com/
    // 2. Pull the all-minilm model we'll use for embeddings: ollama pull all-minilm
    // 3. Ensure Ollama is running: ollama serve
    internal class OllamaEmbeddingsStorage : IEmbeddingsStorage
    {
        private const string AllMinilmEmbeddingModelName = "all-minilm";
        private const string DBName = "vectors.db";
        private const int DimensionsForAllMinilm = 384; // Should be 384 for all-minilm
        private static readonly string defaultCollectionName = "defaultName";

        private readonly SqLiteVectorDatabase vectorDatabase;
        private readonly OllamaEmbeddingModel embeddingModel;

        IVectorCollection vectorCollection;

        public OllamaEmbeddingsStorage()
        {
            OllamaProvider provider = new OllamaProvider();
            this.embeddingModel = new OllamaEmbeddingModel(provider, id: AllMinilmEmbeddingModelName);
            this.vectorDatabase = new SqLiteVectorDatabase(dataSource: DBName);
        }

        public async Task<string> GetQuestionContext(string question)
        {
            IReadOnlyCollection<Document> similarDocuments = await this.vectorCollection.GetSimilarDocuments(this.embeddingModel, question, amount: 5);

            return similarDocuments.AsString();
        }

        public void SetText(string text, PartialContextProcessorSettings settings)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);
            writer.Write(text);

            if (this.vectorDatabase.IsCollectionExistsAsync(defaultCollectionName).Result)
            {
                this.vectorDatabase.DeleteCollectionAsync(defaultCollectionName).Wait();
            }

            this.vectorCollection = this.vectorDatabase.AddDocumentsFromAsync<TextLoader>(
                this.embeddingModel,
                dimensions: DimensionsForAllMinilm,
                dataSource: DataSource.FromBytes(memoryStream.ToArray()),
                textSplitter: null,
                collectionName: defaultCollectionName,
                behavior: AddDocumentsToDatabaseBehavior.JustReturnCollectionIfCollectionIsAlreadyExists).Result;

        }

        public void Dispose()
        {
            this.vectorDatabase.Dispose();
        }
    }
}