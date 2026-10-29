using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using Telerik.Documents.AI.AgentTools.Spreadsheet;
using Telerik.Documents.AI.Tools.Spreadsheet.Core;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ConversationalUI;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace AgentToolsInSpreadsheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private Author? userAuthor;
        private Author? assistantAuthor;
        private string? lastUserMessage;
        private List<AITool>? toolRegistry;
        private ChatClientAgent? agent;

        #endregion

        #region Constructor

        public MainWindow()
        {
            StyleManager.ApplicationTheme = new Windows11Theme();

            InitializeComponent();

            this.radSpreadsheet.Loaded += this.RadSpreadsheet_Loaded;
            this.radSpreadsheet.WorkbookChanged += this.RadSpreadsheet_WorkbookChanged;
        }

        #endregion

        #region Event Handlers

        private void RadSpreadsheet_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeChatAuthors();
            this.InitializeNewChat(recreateTools: true);
            this.ImportSampleDocument();
            this.radSpreadsheet.Focus();
        }

        private void RadSpreadsheet_WorkbookChanged(object? sender, EventArgs e)
        {
            this.InitializeNewChat(recreateTools: true);
        }

        private async void RadChat_SendMessage(object? sender, SendMessageEventArgs e)
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
                this.radChat.SuggestedActions.Clear();
                this.radChat.SuggestedActionsVisibility = Visibility.Collapsed;

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

        private void RadChat_UserVoted(object? sender, UserVotedEventArgs e)
        {
            string voteMessage = e.VoteType == VoteType.UpVote
                ? "Thank you for your feedback! I'm glad I could help."
                : "Thank you for your feedback. I'll try to provide better responses.";

            this.AddAIMessage(voteMessage);
        }

        private async void RadChat_RegenerateResponse(object? sender, RegenerateResponseEventArgs e)
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
                    this.radChat.AddMessage(userMessage);

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

        private async void RadChat_SuggestedActionReported(object? sender, SuggestedActionsEventArgs e)
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
                this.radChat.SuggestedActions.Clear();
                this.radChat.SuggestedActionsVisibility = Visibility.Collapsed;

                // Add user message to chat
                TextMessage userMessage = new(this.userAuthor!, suggestion);
                this.radChat.AddMessage(userMessage);

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
            this.radChat.CurrentAuthor = this.userAuthor;

            // Subscribe to SendMessage event
            this.radChat.SendMessage += this.RadChat_SendMessage;

            // Subscribe to AIMessage events
            this.radChat.UserVoted += this.RadChat_UserVoted;
            this.radChat.RegenerateResponse += this.RadChat_RegenerateResponse;

            // Subscribe to SuggestedActionReported event
            this.radChat.SuggestedActionReported += this.RadChat_SuggestedActionReported;

            // Add welcome message
            this.AddAIMessage("Hello! I'm your AI assistant for spreadsheet analysis. I can help you understand your data, create formulas, generate charts, and answer questions about your spreadsheet. What would you like to know?");
        }

        private void InitializeNewChat(bool recreateTools)
        {
            // Clear previous chat history when a new workbook/session is initialized.
            this.radChat?.MessageListItems?.Clear();
            this.radChat?.MessageGroups?.Clear();
            this.radChat?.SuggestedActions.Clear();
            if (this.radChat != null)
            {
                this.radChat.SuggestedActionsVisibility = Visibility.Collapsed;
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
                IWorkbookRepository repository = new SingleWorkbookRepository(this.radSpreadsheet.Workbook);
                this.toolRegistry = new SpreadProcessingAnalysisAgentTools(repository).GetTools().ToList();
            }

            // Create the AI agent: first convert ChatClient to IChatClient, then create agent
            var chatClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(key))
                .GetChatClient(model);

            this.agent = chatClient.AsIChatClient().AsAIAgent(
                instructions: Prompts.AnalyzerPrompt,
                name: "SpreadsheetAnalyzer",
                tools: this.toolRegistry);

            this.AddAIMessage("AI agent initialized and ready to help with your spreadsheet!");
        }

        private void ImportSampleDocument()
        {
            using(Stream input = File.OpenRead("Electric_Vehicle_Population_Data.xlsx"))
            {
                XlsxFormatProvider formatProvider = new();
                this.radSpreadsheet.Workbook = formatProvider.Import(input, null);
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

                // Display the response
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.typingIndicator.Visibility = Visibility.Collapsed;
                    ReceiveResponseAndAddAIMessage(agentResponse);
                });
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

        private void ReceiveResponseAndAddAIMessage(AgentResponse agentResponse)
        {
            bool hasResponse = false;
            Dictionary<string, string> toolNames = new();
            const int maxToolResultLength = 1000;

            foreach (ChatMessage responseMessage in agentResponse.Messages)
            {
                if (responseMessage.Role != ChatRole.Assistant && responseMessage.Role != ChatRole.Tool)
                {
                    continue;
                }

                foreach (AIContent content in responseMessage.Contents)
                {
                    if (content is FunctionCallContent toolCall)
                    {
                        toolNames[toolCall.CallId] = toolCall.Name;
                        CardMessage toolMessage = new(this.assistantAuthor!)
                        {
                            Title = "Tool called",
                            SubTitle = toolCall.Name,
                            Text = toolCall.Arguments is { Count: > 0 }
                                ? JsonSerializer.Serialize(toolCall.Arguments, new JsonSerializerOptions { WriteIndented = true })
                                : "No arguments"
                        };

                        this.radChat.AddMessage(toolMessage);
                        hasResponse = true;
                    }
                    else if (content is FunctionResultContent toolResult)
                    {
                        string resultText = FormatToolResult(toolResult.Result);

                        if (resultText.Length > maxToolResultLength)
                        {
                            resultText = resultText[..maxToolResultLength] + "\n... [truncated]";
                        }

                        CardMessage resultMessage = new(this.assistantAuthor!)
                        {
                            Title = "Tool result",
                            SubTitle = toolNames.TryGetValue(toolResult.CallId, out string? toolName) ? toolName : "Tool response",
                            Text = resultText
                        };

                        this.radChat.AddMessage(resultMessage);
                        hasResponse = true;
                    }
                    else if (responseMessage.Role == ChatRole.Assistant && content is TextContent text && !string.IsNullOrWhiteSpace(text.Text))
                    {
                        this.AddAIMessage(text.Text);
                        hasResponse = true;
                    }
                }
            }


            if (!hasResponse)
            {
                this.AddAIMessage("I couldn't generate a response. Please try again.");
            }
        }

        private static string FormatToolResult(object? result)
        {
            if (result == null)
            {
                return "No result returned";
            }

            string resultText = result is string text ? text : JsonSerializer.Serialize(result);

            try
            {
                using JsonDocument document = JsonDocument.Parse(resultText);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("message", out JsonElement message)
                    && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString()!;
                }

                if (root.ValueKind == JsonValueKind.String)
                {
                    return root.GetString()!;
                }

                return JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (JsonException)
            {
                return resultText;
            }
        }

        private void AddAIMessage(string message)
        {
            if(this.radChat == null)
            {
                return;
            }

            AIMessage aiMessage = new(this.assistantAuthor!)
            {
                AIAnswer = message,
                AIName = "AI Assistant"
            };

            this.radChat.AddMessage(aiMessage);
        }

        #endregion
    }
}