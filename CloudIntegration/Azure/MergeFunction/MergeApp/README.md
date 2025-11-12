# MergeApp

A .NET 8.0 console test client application that sends PDF files to the MergeFunctionApp Azure Function for merging.

## Overview

**MergeApp** is a test client application designed to work with the **MergeFunctionApp** Azure Function. It demonstrates the complete workflow of:
- Loading PDF files from disk
- Packaging files as multipart form data
- Sending HTTP requests to the Azure Function
- Receiving and saving the merged PDF result
- Automatically opening the result in the default PDF viewer

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [HttpClient Best Practices](https://docs.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Multipart Form Data in .NET](https://docs.microsoft.com/aspnet/web-api/overview/advanced/sending-html-form-data-part-2)
- [Azure Functions HTTP Triggers](https://docs.microsoft.com/azure/azure-functions/functions-bindings-http-webhook-trigger)
