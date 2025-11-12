# MergeFunctionApp

An HTTP-triggered Azure Function that accepts PDF files via multipart form data and returns a single merged PDF document.

## Overview

The **MergeFunction** is an HTTP-triggered Azure Function that accepts multiple PDF files via multipart form data and returns a single merged PDF document. It leverages the Telerik Document Processing library for reliable PDF manipulation.

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/) - Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)
- [HttpMultipartParser](https://www.nuget.org/packages/HttpMultipartParser/)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Telerik RadPdfProcessing](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/overview)
