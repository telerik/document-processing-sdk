using LangChain.DocumentLoaders;

namespace AIConnectorDemo
{
    internal class TextLoader : IDocumentLoader
    {
        public async Task<IReadOnlyCollection<Document>> LoadAsync(DataSource dataSource, DocumentLoaderSettings? settings = null, CancellationToken cancellationToken = default)
        {
            using (Stream inputStream = await dataSource.GetStreamAsync(cancellationToken))
            {
                StreamReader reader = new StreamReader(inputStream);
                string content = reader.ReadToEnd();

                string[] pages = content.Split(["----------"], System.StringSplitOptions.RemoveEmptyEntries);

                return pages.Select(x => new Document(x)).ToList();
            }
        }
    }
}