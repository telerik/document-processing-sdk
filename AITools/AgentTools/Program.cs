using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.ClientModel;
using System.Diagnostics;
using System.Text;
using Telerik.Documents.AI.AgentTools.Conversion;
using Telerik.Documents.AI.AgentTools.Fixed;
using Telerik.Documents.AI.AgentTools.Spreadsheet;
using Telerik.Documents.AI.Tools.Core;
using Telerik.Documents.AI.Tools.Fixed.Core;
using Telerik.Documents.AI.Tools.Flow.Core;
using Telerik.Documents.AI.Tools.Spreadsheet.Core;
using Telerik.Documents.ImageUtils;
using Telerik.Windows.Documents.Extensibility;

namespace TestApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Configure document processing options with security timeout
            var documentOptions = new DocumentProcessingOptions
            {
                ImportTimeout = TimeSpan.FromSeconds(30)
            };

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddUserSecrets<Program>(optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<AzureOpenAISettings>(context.Configuration.GetSection(AzureOpenAISettings.SectionName));
                    services.AddTransient<IGreetingService, GreetingService>();
                    services.AddTransient<App>();
                    services.AddSpreadsheetAgentTools(documentOptions);
                    services.AddFixedAgentTools(documentOptions);
                    services.AddFlowAgentTools(documentOptions);
                })
                .Build();

            var app = host.Services.GetRequiredService<App>();
            await app.RunAsync();
        }
    }

    internal class App
    {
        const string OutputDir = @"..\..\..\output";

        private readonly IGreetingService _greetingService;
        private readonly IWorkbookRepository workbookRepository;
        private readonly IFlowDocumentRepository flowDocumentRepository;
        private readonly IFixedDocumentRepository fixedDocumentRepository;
        private readonly AzureOpenAISettings _azureOpenAISettings;
        DocumentRepositoryRegistry registry;

        public App(IGreetingService greetingService, IWorkbookRepository workbookRepository, IFlowDocumentRepository flowDocumentRepository, IFixedDocumentRepository fixedDocumentRepository, DocumentRepositoryRegistry registry, IOptions<AzureOpenAISettings> azureOpenAISettings)
        {
            this._greetingService = greetingService;
            this.workbookRepository = workbookRepository;
            this.flowDocumentRepository = flowDocumentRepository;
            this.fixedDocumentRepository = fixedDocumentRepository;
            this.registry = registry;
            this._azureOpenAISettings = azureOpenAISettings.Value;
        }

        public async Task RunAsync()
        {
            var message = _greetingService.GetGreeting();
            Console.WriteLine(message);
            await this.TestAllAIScenarios();
            //await this.ChatWithLLMAndToolsToProcessDocuments();
        }

        internal async Task TestAllAIScenarios()
        {
            FixedExtensibilityManager.ImagePropertiesResolver = new ImagePropertiesResolver();
            FixedExtensibilityManager.JpegImageConverter = new JpegImageConverter();

            var scenarios = new Dictionary<string, string>
            {
                ["s1"] = "Create a new Excel file called 'products.xlsx' with a worksheet named 'Inventory'. Add a header row with columns: Product, Quantity, Price. Then add three products: Laptop (5, $999), Mouse (50, $25), Keyboard (30, $75).",
                ["s2"] = "Create a PDF document called 'welcome.pdf' with a title 'Welcome to Our Company' in bold 18pt font, followed by a paragraph: 'We are excited to have you on board. This handbook will guide you through your first weeks.'",
                ["s3"] = "Convert the Excel file 'sales_report.xlsx' to PDF format and save it as 'sales_report.pdf'.",

                ["i1"] = "Create a budget spreadsheet 'monthly_budget.xlsx' with two worksheets: 'Income' and 'Expenses'. In the Income sheet, add headers (Source, Amount) and data: Salary ($5000), Freelance ($1200). In the Expenses sheet, add headers (Category, Amount) and data: Rent ($1500), Groceries ($600), Utilities ($200). Add a formula in the Expenses sheet to calculate the total. Auto-fit all columns in both worksheets.",
                ["i2"] = "Create a report 'quarterly_report.pdf' with: - Title 'Q4 2025 Sales Report' (bold, 20pt) - A paragraph describing the quarter performance - A table showing monthly sales: October ($50K), November ($62K), December ($78K) with headers - An image from 'chart.png' - A concluding paragraph about future projections",
                ["i3"] = "Show me all the form fields in 'job_application.pdf', then fill it out with: FirstName='Sarah', LastName='Chen', Email='sarah.chen@email.com', Position='Senior Developer', IsFullTime=true. Save the filled form as 'sarah_chen_application.pdf'.",

                ["c1"] = "Create a comprehensive sales analysis spreadsheet 'sales_analysis_2025.xlsx': 1. Create three worksheets: 'Q1_Sales', 'Q2_Sales', and 'Summary' 2. In Q1_Sales, add monthly data with headers (Month, Region, Revenue, Units): - January: North ($45000, 150), South ($38000, 127), East ($52000, 173) - February: North ($48000, 160), South ($41000, 137), East ($55000, 183) - March: North ($51000, 170), South ($44000, 147), East ($58000, 193) 3. In Q2_Sales, add similar data for April-June (use 10% higher values) 4. In the Summary sheet: - Add a title row 'Annual Sales Summary 2025' - Create a table with Q1 Total and Q2 Total calculations using formulas - Apply bold styling to headers - Apply a 'Good' style to cells showing totals over $100,000 5. Auto-fit all columns in all worksheets 6. Get the used cell range for the Summary worksheet to verify the layout",
                ["c2"] = "Create a multi-section employee handbook 'employee_handbook.pdf': 1. First, create the PDF with: - Cover page: 'Employee Handbook 2025' (bold, 24pt) centered - Section break for new page layout - 'Table of Contents' heading (bold, 18pt) - List of sections (14pt) 2. Then add content sections: - 'Company Policies' heading with PageBreak before - A table of 5 policies with Name and Description columns - 'Benefits Overview' heading - A table showing benefits: Health Insurance, 401k, PTO, etc. - Company logo image from 'logo.png' (200x100px) - 'Contact Information' section with HR contact details 3. The content should flow naturally across pages with proper spacing and formatting",
                ["c3"] = "I need to create a quarterly business review package: 1. Create an Excel spreadsheet 'q4_data.xlsx' with worksheets: - 'Revenue' with monthly breakdown by region (3 regions, 3 months, with revenue and growth % formulas) - 'Expenses' with category breakdown (calculate total with formula) - 'Profit' (use formulas to calculate: Revenue - Expenses) - Apply professional styling to headers and format number cells - Auto-fit all columns 2. Convert this spreadsheet to PDF as 'q4_data.pdf' 3. Create a pdf document summary 'q4_summary.pdf' with: - Executive summary text - Key findings and recommendations 4. Create another pdf document 'q4_appendix.pdf' with supplementary data 5. Merge 'q4_summary.pdf' and 'q4_appendix.pdf' into 'q4_text_report.pdf' 6. Finally, merge 'q4_text_report.pdf' and 'q4_data.pdf' into a final 'Q4_Complete_Package.pdf' This creates a complete business review package with data analysis and narrative report combined.",
                ["c4"] = "Create and manipulate a complex project tracking spreadsheet 'project_tracker.xlsx': 1. Create with worksheets: 'Team_A', 'Team_B', 'Team_C', 'Archive', 'Dashboard' 2. In each team worksheet, add project data with headers (Project, Status, Budget, Owner) 3. Populate Team_A with 5 projects, Team_B with 4 projects, Team_C with 3 projects 4. Read all worksheet names to verify structure 5. Delete the 'Archive' worksheet as it's not needed yet 6. Rename 'Dashboard' to 'Master_Summary' 7. In Master_Summary, create formulas that reference Team worksheets for totals 8. Apply cell styles: 'Bad' for over-budget items, 'Good' for completed projects 9. Get the used cell range for Master_Summary 10. Read the cell values and styles back to verify everything is correct 11. Auto-fit all rows and columns for optimal viewing",

                ["h1"] = "Hey,I need to put together something for my boss by end of day. We have our quarterly sales numbers - Q1 was like 125k, Q2 was 142k, Q3 was 138k, and Q4 we crushed it at 189k.Can you make me a spreadsheet with that data and also a nice PDF summary I can attach to an email ? Oh and the spreadsheet should probably show the quarter - over - quarter growth percentages too.Call it something like q4_review or whatever makes sense.",
                ["h2"] = "I'm onboarding 3 new hires next week and need to prepare their paperwork. Can you create offer letters for them? John Smith is coming in as a Senior Developer at $120k, Maria Garcia is a Project Manager at $95k, and Alex Wong is joining as QA Lead at $88k. They all start January 15th. Just make them look professional - you know, company header, the standard welcome stuff, salary details, signature lines at the bottom.",
                ["h3"] = "So we track our inventory in this spreadsheet but it's a mess. Can you make me a new one that's actually organized ? We sell electronics - phones, tablets, laptops, accessories.I need to see what's in stock, what's running low(like under 10 units), and what each item costs us vs what we sell it for. Throw in some sample data so I can see how it looks.Actually can you also add a sheet that calculates our potential profit if we sold everything? And make it look nice, like with proper headers and stuff.",
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine($"\n\n{'='}{new string('=', 80)}");
                Console.WriteLine($"Starting scenario: {scenario.Key.ToUpper()}");
                Console.WriteLine($"{'='}{new string('=', 80)}\n");

                await this.ChatWithLLMAndToolsToProcessDocuments(scenario.Value, scenario.Key);

                Console.WriteLine($"\n{'='}{new string('=', 80)}");
                Console.WriteLine($"Completed scenario: {scenario.Key.ToUpper()}");
                Console.WriteLine($"{'='}{new string('=', 80)}\n\n");

                await System.Threading.Tasks.Task.Delay(1000);
            }

            Debug.WriteLine("\n\n*** ALL SCENARIOS COMPLETED ***\n");
        }

        private async Task ChatWithLLMAndToolsToProcessDocuments(string prompt = null, string outputFileName = null)
        {
            string key = _azureOpenAISettings.ApiKey;
            string endpoint = _azureOpenAISettings.Endpoint;
            string model = _azureOpenAISettings.Model;

            DocumentRepositoryRegistry documentRepositoryRegistry = new DocumentRepositoryRegistry();
            documentRepositoryRegistry.RegisterRepository(DocumentType.Workbook, this.workbookRepository);
            documentRepositoryRegistry.RegisterRepository(DocumentType.FixedDocument, this.fixedDocumentRepository);
            documentRepositoryRegistry.RegisterRepository(DocumentType.FlowDocument, this.flowDocumentRepository);

            SpreadProcessingReadAgentTools readToolsWrapper = new SpreadProcessingReadAgentTools(this.workbookRepository);
            SpreadProcessingWriteAgentTools writeToolsWrapper = new SpreadProcessingWriteAgentTools(this.workbookRepository);
            SpreadProcessingWorksheetAgentTools worksheetAgentToolsWrapper = new SpreadProcessingWorksheetAgentTools(this.workbookRepository);
            SpreadProcessingFileManagementAgentTools fileManagementAgentTools = new SpreadProcessingFileManagementAgentTools(this.workbookRepository, OutputDir);

            MergeDocumentsAgentTool mergeTool = new MergeDocumentsAgentTool(documentRepositoryRegistry, OutputDir);
            ConvertDocumentsAgentTool convertTool = new ConvertDocumentsAgentTool(documentRepositoryRegistry, OutputDir);

            FixedDocumentFormAgentTools fixedDocumentFormAgentTools = new FixedDocumentFormAgentTools(this.fixedDocumentRepository);
            FixedDocumentContentAgentTools fixedContentTools = new FixedDocumentContentAgentTools(this.fixedDocumentRepository, OutputDir);
            FixedFileManagementAgentTools fixedFileManagementTools = new FixedFileManagementAgentTools(this.fixedDocumentRepository, OutputDir);

            List<AITool> tools = new List<AITool>(readToolsWrapper.GetTools());
            tools.AddRange(writeToolsWrapper.GetTools());
            tools.AddRange(fileManagementAgentTools.GetTools());
            tools.AddRange(worksheetAgentToolsWrapper.GetTools());

            tools.AddRange(convertTool.GetTools());
            tools.AddRange(mergeTool.GetTools());

            tools.AddRange(fixedDocumentFormAgentTools.GetTools());
            tools.AddRange(fixedContentTools.GetTools());
            tools.AddRange(fixedFileManagementTools.GetTools());

            AIAgent agent = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(key))
                .GetChatClient(model).AsIChatClient()
                .CreateAIAgent(
                    instructions: "",
                    name: "Analyzer",
                    tools: tools);

            if (prompt == null)
            {
                Console.WriteLine(@"Enter your message to the AI agent or type ""exit"" to exit the chat:");
            }
            else
            {
                Console.WriteLine($"The current prompt is: {prompt}. You can type \"exit\" to exit the chat.");
            }

            await ChatWithLLM(agent, prompt, outputFileName);
        }

        private static async Task ChatWithLLM(AIAgent agent, string prompt = null, string outputFileName = null)
        {
            string userInput = prompt;
            if (userInput == null)
            {
                userInput = Console.ReadLine();
            }

            List<ChatMessage> currentMessages = new List<ChatMessage>();

            StringBuilder outputLog = new StringBuilder();

            while (userInput != null && userInput.ToLower() != "exit")
            {
                AgentRunResponse response = null;
                currentMessages.Add(new ChatMessage(ChatRole.User, userInput));

                outputLog.AppendLine(userInput);

                try
                {
                    response = await agent.RunAsync(currentMessages);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(2000);
                }

                if (response != null)
                {
                    foreach (ChatMessage message in response.Messages)
                    {
                        currentMessages.Add(message);

                        foreach (AIContent content in message.Contents)
                        {
                            if (content is TextContent textContent)
                            {
                                if (!string.IsNullOrEmpty(textContent.Text))
                                {
                                    string logEntry = $"\n{message.Role}: {textContent.Text}";
                                    Debug.WriteLine(logEntry);
                                    Console.WriteLine(logEntry);
                                    outputLog.AppendLine(logEntry);
                                }
                            }
                            else if (content is UsageContent usageContent)
                            {
                                string logEntry = $"Input tokens count: {usageContent.Details.InputTokenCount} Output tokens count: {usageContent.Details.OutputTokenCount}: Total tokens count: {usageContent.Details.TotalTokenCount}";
                                Debug.WriteLine(logEntry);
                                Console.WriteLine(logEntry);
                                outputLog.AppendLine(logEntry);
                            }
                            else if (content is FunctionCallContent functionCallContent)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine($"\nFunction call received:");
                                sb.Append("Function name: ");
                                sb.AppendLine(functionCallContent.Name);
                                sb.Append("Parameters: ");

                                foreach (KeyValuePair<string, object> arg in functionCallContent.Arguments)
                                {
                                    sb.AppendLine($"{arg.Key}: {arg.Value}");
                                }

                                string logEntry = sb.ToString();
                                Debug.WriteLine(logEntry);
                                Console.WriteLine(logEntry);
                                outputLog.AppendLine(logEntry);
                            }
                            else if (content is FunctionResultContent functionResultContent)
                            {
                                string logEntry = $"\nFunction result received: {functionResultContent.Result}";
                                Debug.WriteLine(logEntry);
                                Console.WriteLine(logEntry);
                                outputLog.AppendLine(logEntry);
                            }
                        }
                    }
                }

                userInput = Console.ReadLine();
            }

            if (!string.IsNullOrEmpty(outputFileName))
            {
                string logFilePath = Path.Combine(OutputDir, $"{outputFileName}.txt");
                File.WriteAllText(logFilePath, outputLog.ToString());
                Debug.WriteLine($"\n=== Results saved to: {logFilePath} ===");
            }
        }
    }

    internal interface IGreetingService
    {
        string GetGreeting();
    }

    internal class GreetingService : IGreetingService
    {
        public string GetGreeting()
        {
            return "Hello, World!";
        }
    }
}
