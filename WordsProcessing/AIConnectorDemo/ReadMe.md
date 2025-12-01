In order to run the project, you need to replace the key and endpoint with your Azure Open AI key and endpoint.
They are located in the launchSettings.json file in the Properties folder.

In NetStandard you need to implement IEmbedder if you would like to use the PartialContextQuestionProcessor.
There is a sample implementation called CustomOpenAIEmbedder in the NetStandard project.