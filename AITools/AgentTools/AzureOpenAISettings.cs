namespace TestApp
{
    public class AzureOpenAISettings
    {
        public const string SectionName = "AzureOpenAI";

        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4.1-mini";
    }
}
