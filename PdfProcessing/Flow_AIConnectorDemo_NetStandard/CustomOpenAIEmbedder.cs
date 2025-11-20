using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Telerik.Documents.AI.Core;

namespace FlowAIConnectorDemo
{
    public class CustomOpenAIEmbedder : IEmbedder
    {
        private readonly HttpClient httpClient;

        public CustomOpenAIEmbedder()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            string endpoint = Environment.GetEnvironmentVariable("AZUREEMBEDDINGOPENAI_ENDPOINT");
            string apiKey = Environment.GetEnvironmentVariable("AZUREEMBEDDINGOPENAI_KEY");

            httpClient.BaseAddress = new Uri(endpoint);
            httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.httpClient = httpClient;
        }

        public async Task<IList<Embedding>> EmbedAsync(IList<IFragment> fragments)
        {
            AzureEmbeddingsRequest requestBody = new AzureEmbeddingsRequest
            {
                Input = fragments.Select(p => p.ToEmbeddingText()).ToArray(),
                Dimensions = 3072
            };

            string json = JsonSerializer.Serialize(requestBody);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            string apiVersion = Environment.GetEnvironmentVariable("AZUREEMBEDDINGOPENAI_APIVERSION");
            string deploymentName = Environment.GetEnvironmentVariable("AZUREEMBEDDINGOPENAI_DEPLOYMENT");
            string url = $"openai/deployments/{deploymentName}/embeddings?api-version={apiVersion}";
            using HttpResponseMessage response = await this.httpClient.PostAsync(url, content, CancellationToken.None);

            Embedding[] embeddings = new Embedding[fragments.Count];

            string responseJson = await response.Content.ReadAsStringAsync(CancellationToken.None);
            AzureEmbeddingsResponse responseObj = JsonSerializer.Deserialize<AzureEmbeddingsResponse>(responseJson);

            List<EmbeddingData> sorted = responseObj.Data.OrderBy(d => d.Index).ToList();
            List<float[]> result = new List<float[]>(sorted.Count);

            for (int i = 0; i < sorted.Count; i++)
            {
                EmbeddingData item = sorted[i];
                embeddings[i] = new Embedding(fragments[i], item.Embedding);
            }

            return embeddings;
        }

        private sealed class AzureEmbeddingsRequest
        {
            [System.Text.Json.Serialization.JsonPropertyName("input")]
            public string[] Input { get; set; } = Array.Empty<string>();

            [System.Text.Json.Serialization.JsonPropertyName("dimensions")]
            public int? Dimensions { get; set; }
        }

        private sealed class AzureEmbeddingsResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("data")]
            public EmbeddingData[] Data { get; set; } = Array.Empty<EmbeddingData>();

            [System.Text.Json.Serialization.JsonPropertyName("model")]
            public string? Model { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("usage")]
            public UsageInfo? Usage { get; set; }
        }

        private sealed class UsageInfo
        {
            [System.Text.Json.Serialization.JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        private sealed class EmbeddingData
        {
            [System.Text.Json.Serialization.JsonPropertyName("embedding")]
            public float[] Embedding { get; set; } = Array.Empty<float>();

            [System.Text.Json.Serialization.JsonPropertyName("index")]
            public int Index { get; set; }
        }
    }
}