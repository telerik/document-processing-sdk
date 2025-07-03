In order to run the project, you need to replace the key and endpoint with your Azure Open AI key and endpoint.
They are located in the launchSettings.json file in the Properties folder.

In NetStandard you need to implement IEmbeddingsStorage if you would like to use the PartialContextQuestionProcessor.
There is a sample implementation called OllamaEmbeddingsStorage in the NetStandard project.
For it to work the Ollama server needs to be running locally. You can follow these steps to run it:
    1. Install Ollama: https://ollama.com/
    2. Pull the all-minilm model we'll use for embeddings: ollama pull all-minilm
    3. Ensure Ollama is running: ollama serve