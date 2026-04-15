using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Documents.AI.AgentTools.Fixed; 
using Telerik.Documents.AI.Tools.Fixed.Core; 
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ConversationalUI;
using Telerik.Windows.Documents.Fixed;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf; 

namespace AgentToolsInPdfViewerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private string AgentToolsSamplePdfPath = "GenAI Document Insights Test Document.pdf";
        private const string AgentToolsImageDir = @"..\..\..\agent-tools-images";
        private Author? userAuthor;
        private Author? assistantAuthor;
        private string? lastUserMessage;
        private List<AITool>? toolRegistry;
        private ChatClientAgent? agent;
        private SingleFixedDocumentRepository? repository;
        private const int timeoutSeconds = 10;
        private static readonly HashSet<string> ExcludedToolNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ExportFixedDocument",
            "ImportFixedDocument"
        };
        private static readonly HashSet<string> ReadOnlyToolNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DescribePdfDocument",
            "ExtractTextFromPdf",
            "ListFixedDocuments"
        };
        private static readonly string DefaultSystemPrompt = @"You are a PDF document assistant running in a desktop-based demo environment. You can create, modify, and analyze PDF documents using the available tools.

ENVIRONMENT CONSTRAINTS:
- The user interacts with you only through a text chat. They cannot upload, attach, or provide any files, images, or resources.
- You have access to a single pre-loaded placeholder image ('placeholder.png'). This is the only image available.
- Do not ask the user to provide, upload, or specify file paths for images or any other resources. They have no way to do so.
- If the user requests an image, use 'placeholder.png' automatically without asking.

IMPORTANT RULES:
- When adding images, always use the file name 'placeholder.png' as the image path.
- Never add more than 50 content segments in a single AddContentSegmentsToPdf call.
- When creating tables, keep them reasonable in size (no more than 20 rows per table).
- After making changes, briefly describe what you did.
- If the user asks to create a new document, use the CreateFixedDocument tool and then always add a text segment with a space to ensure the document has at least one page.
- The document uses device-independent pixels (DIPs) where 96 DIPs = 1 inch.
- The tool AddContentSegmentsToPdf always starts on a new page automatically. Never begin the segments array with a PageBreak — doing so creates an unwanted blank page. Only use PageBreak segments in the middle of the array when you intentionally need to split content across multiple pages.";

        #endregion

        #region Constructor
        public MainWindow()
        {
            StyleManager.ApplicationTheme = new Windows11Theme();

            InitializeComponent();
            this.pdfViewer.Loaded += PdfViewer_Loaded;
           
        }

        private void PdfViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.ImportSampleDocument();
            this.InitializeChatAuthors();
            this.InitializeNewChat(recreateTools: true);
          
            this.pdfViewer.Focus();
            this.pdfViewer.DocumentChanged += PdfViewer_DocumentChanged;
        }

        #endregion

        #region Event Handlers

        private void PdfViewer_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            this.InitializeNewChat(recreateTools: true);
        }

        private async void chat_SendMessage(object? sender, SendMessageEventArgs e)
        {
            try
            {
                string userMessage = string.Empty;

                // Extract text from the message
                if (e.Message is TextMessage textMessage)
                {
                    userMessage = textMessage.Text;
                }

                // Validate message is not empty
                if (string.IsNullOrWhiteSpace(userMessage))
                {
                    return;
                }

                // Track last user message for regeneration
                this.lastUserMessage = userMessage;

                // Clear suggested actions
                this.chat.SuggestedActions.Clear();
                this.chat.SuggestedActionsVisibility = Visibility.Collapsed;

                // Show typing indicator
                this.typingIndicator.Visibility = Visibility.Visible;

                // Send message to agent
                await this.SendChatMessageAsync(userMessage);
            }
            catch (Exception ex)
            {
                // Hide typing indicator on error
                this.typingIndicator.Visibility = Visibility.Collapsed;

                // Show error message in chat
                this.AddAIMessage($"Sorry, an error occurred: {ex.Message}");
            }
        }

        private void chat_UserVoted(object? sender, UserVotedEventArgs e)
        {
            string voteMessage = e.VoteType == VoteType.UpVote
                ? "Thank you for your feedback! I'm glad I could help."
                : "Thank you for your feedback. I'll try to provide better responses.";

            this.AddAIMessage(voteMessage);
        }

        private async void chat_RegenerateResponse(object? sender, RegenerateResponseEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.lastUserMessage))
                {
                    this.AddAIMessage("No previous message to regenerate.");
                    return;
                }

                // Show typing indicator
                this.typingIndicator.Visibility = Visibility.Visible;

                // Add context message
                this.AddAIMessage("Let me try that again...");

                // Resend the last message
                await this.SendChatMessageAsync(this.lastUserMessage);
            }
            catch (Exception ex)
            {
                this.typingIndicator.Visibility = Visibility.Collapsed;
                this.AddAIMessage($"Error regenerating response: {ex.Message}");
            }
        }

        private async void SuggestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Telerik.Windows.Controls.RadButton button && button.Tag is string suggestion)
            {
                try
                {
                    // Track last user message for regeneration
                    this.lastUserMessage = suggestion;

                    // Add user message to chat
                    TextMessage userMessage = new(this.userAuthor!, suggestion);
                    this.chat.AddMessage(userMessage);

                    // Show typing indicator
                    this.typingIndicator.Visibility = Visibility.Visible;

                    // Send message to agent
                    await this.SendChatMessageAsync(suggestion);
                }
                catch (Exception ex)
                {
                    this.typingIndicator.Visibility = Visibility.Collapsed;
                    this.AddAIMessage($"Sorry, an error occurred: {ex.Message}");
                }
            }
        }

        private async void chat_SuggestedActionReported(object? sender, SuggestedActionsEventArgs e)
        {
            string suggestion = e.Text;

            if (string.IsNullOrWhiteSpace(suggestion))
            {
                return;
            }

            try
            {
                // Track last user message for regeneration
                this.lastUserMessage = suggestion;

                // Clear suggested actions
                this.chat.SuggestedActions.Clear();
                this.chat.SuggestedActionsVisibility = Visibility.Collapsed;

                // Add user message to chat
                TextMessage userMessage = new(this.userAuthor!, suggestion);
                this.chat.AddMessage(userMessage);

                // Show typing indicator
                this.typingIndicator.Visibility = Visibility.Visible;

                // Send message to agent
                await this.SendChatMessageAsync(suggestion);
            }
            catch (Exception ex)
            {
                this.typingIndicator.Visibility = Visibility.Collapsed;
                this.AddAIMessage($"Sorry, an error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Chat Initialization

        private void InitializeChatAuthors()
        {
            // Initialize authors
            this.userAuthor = new Author("You");
            this.assistantAuthor = new Author("AI Assistant");

            // Set current author for user messages
            this.chat.CurrentAuthor = this.userAuthor;

            // Subscribe to SendMessage event
            this.chat.SendMessage += this.chat_SendMessage;

            // Subscribe to AIMessage events
            this.chat.UserVoted += this.chat_UserVoted;
            this.chat.RegenerateResponse += this.chat_RegenerateResponse;

            // Subscribe to SuggestedActionReported event
            this.chat.SuggestedActionReported += this.chat_SuggestedActionReported;

            // Add welcome message
            this.AddAIMessage("Hello! I'm your AI assistant for spreadsheet analysis. I can help you understand your data, create formulas, generate charts, and answer questions about your spreadsheet. What would you like to know?");
        }

        private void InitializeNewChat(bool recreateTools)
        {
            // Clear previous chat history when a new workbook/session is initialized.
            this.chat?.MessageListItems?.Clear();
            this.chat?.MessageGroups?.Clear();
            this.chat?.SuggestedActions.Clear();
            if (this.chat != null)
            {
                this.chat.SuggestedActionsVisibility = Visibility.Collapsed;
            }

            // Reset last user message tracking
            this.lastUserMessage = null;

            string? key = Environment.GetEnvironmentVariable("AZUREOPENAI_KEY");
            string? endpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");
            string model = "gpt-4.1-mini";

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(endpoint))
            {
                this.AddAIMessage("⚠️ Azure OpenAI credentials not found. Please set AZUREOPENAI_KEY and AZUREOPENAI_ENDPOINT environment variables.");
                return;
            }

            // Create or reuse tools
            if (recreateTools || this.toolRegistry == null)
            {
                this.repository = new SingleFixedDocumentRepository(this.pdfViewer.Document, "Sample document");
                IFixedDocumentRepository repository = this.repository;
                FixedFileManagementAgentTools fileTools = new FixedFileManagementAgentTools(repository, null);
                FixedDocumentContentAgentTools contentTools = new FixedDocumentContentAgentTools(repository, AgentToolsImageDir);
                FixedDocumentFormAgentTools formTools = new FixedDocumentFormAgentTools(repository);
                FixedDocumentReadAgentTools readAgentTools = new FixedDocumentReadAgentTools(repository);

                List<AITool> tools = new List<AITool>();
                foreach (AITool tool in fileTools.GetTools())
                {
                    if (!ExcludedToolNames.Contains(tool.Name))
                    {
                        tools.Add(tool);
                    }
                }
                tools.AddRange(contentTools.GetTools());
                tools.AddRange(formTools.GetTools());
                tools.AddRange(readAgentTools.GetTools());

                this.toolRegistry = tools;
            }

            // Create the AI agent: first convert ChatClient to IChatClient, then create agent
            var chatClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(key))
                .GetChatClient(model);

            this.agent = chatClient.AsIChatClient().AsAIAgent(
                instructions: DefaultSystemPrompt,
                name: "PdfAnalyzer",
                tools: this.toolRegistry);

            this.AddAIMessage("AI agent initialized and ready to help with your PDF document!"
            );
        }

        private void ImportSampleDocument()
        {
            using (Stream input = File.OpenRead(AgentToolsSamplePdfPath))
            {
                PdfFormatProvider formatProvider = new();
                this.pdfViewer.Document = formatProvider.Import(input, null);
            }
        }

        #endregion

        #region Chat Message Handling

        private async Task SendChatMessageAsync(string message)
        {
            if (this.agent == null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.typingIndicator.Visibility = Visibility.Collapsed;
                    this.AddAIMessage("Agent not initialized. Please reload the application.");
                });
                return;
            }

            try
            {
                // Create a chat message and send to the agent
                ChatMessage userChatMessage = new(ChatRole.User, message);
                List<ChatMessage> messages = [userChatMessage];
                AgentResponse agentResponse = await this.agent.RunAsync(messages);

                // Extract the response text from the agent's messages
                string response = string.Join("\n", agentResponse.Messages
                    .Where(m => m.Role == ChatRole.Assistant)
                    .Select(m => m.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t)));

                // Display the response
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.typingIndicator.Visibility = Visibility.Collapsed;

                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        this.AddAIMessage(response);
                    }
                    else
                    {
                        this.AddAIMessage("I couldn't generate a response. Please try again.");
                    }
                });

                List<object> segments = ExtractResponseSegments(agentResponse);

                bool documentChanged = HasDocumentModifyingToolCall(agentResponse);
                if (documentChanged)
                {
                    PdfFormatProvider provider = new PdfFormatProvider();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        provider.Export(this.repository.GetDocument(), ms, TimeSpan.FromSeconds(timeoutSeconds));
                        ms.Position = 0;
                        this.pdfViewer.Document = provider.Import(ms, TimeSpan.FromSeconds(timeoutSeconds));
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.typingIndicator.Visibility = Visibility.Collapsed;
                    this.AddAIMessage($"Error: {ex.Message}");
                });
            }
        }
        private static bool HasDocumentModifyingToolCall(AgentResponse response)
        {
            foreach (ChatMessage message in response.Messages)
            {
                foreach (AIContent content in message.Contents)
                {
                    if (content is FunctionCallContent functionCall
                        && !ReadOnlyToolNames.Contains(functionCall.Name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private static List<object> ExtractResponseSegments(AgentResponse response)
        {
            List<object> segments = new List<object>(8);

            foreach (ChatMessage message in response.Messages)
            {
                foreach (AIContent content in message.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        if (!string.IsNullOrWhiteSpace(textContent.Text))
                        {
                            segments.Add(new { type = "text", text = textContent.Text });
                        }
                    }
                    else if (content is FunctionCallContent functionCallContent)
                    {
                        Dictionary<string, string> args = new Dictionary<string, string>(StringComparer.Ordinal);
                        if (functionCallContent.Arguments != null)
                        {
                            foreach (KeyValuePair<string, object> arg in functionCallContent.Arguments)
                            {
                                args[arg.Key] = arg.Value?.ToString() ?? string.Empty;
                            }
                        }

                        segments.Add(new
                        {
                            type = "toolCall",
                            name = functionCallContent.Name,
                            arguments = args
                        });
                    }
                    else if (content is FunctionResultContent functionResultContent)
                    {
                        string resultText = functionResultContent.Result?.ToString() ?? string.Empty;
                        bool isError = resultText.IndexOf("isError", StringComparison.OrdinalIgnoreCase) >= 0
                            && resultText.IndexOf("true", StringComparison.OrdinalIgnoreCase) >= 0;

                        segments.Add(new
                        {
                            type = "toolResult",
                            result = resultText,
                            isError = isError
                        });
                    }
                }
            }

            if (segments.Count == 0)
            {
                segments.Add(new { type = "text", text = "The operation completed but no text response was generated." });
            }

            return segments;
        }
        private void AddAIMessage(string message)
        {
            if (this.chat == null)
            {
                return;
            }

            AIMessage aiMessage = new(this.assistantAuthor!)
            {
                AIAnswer = message,
                AIName = "AI Assistant"
            };

            this.chat.AddMessage(aiMessage);
        }

        #endregion
    }
}
