using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Documents.AI.Agents.Core.Configuration;
using Telerik.Documents.AI.Agents.Core.Constants;
using Telerik.Documents.AI.Agents.Core.Infrastructure;
using Telerik.Documents.AI.Agents.Core.Messages.Public;
using Telerik.Documents.AI.Agents.Core.OpenAI;
using Telerik.Documents.AI.Agents.Core.Registry;
using Telerik.Documents.AI.Agents.Core.Workflow;
using Telerik.Documents.AI.Agents.SpreadProcessing;
using Telerik.Documents.Common.Model;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ConversationalUI;
using Telerik.Windows.Controls.Spreadsheet;
using Telerik.Windows.Controls.Spreadsheet.Commands;
using Telerik.Windows.Controls.Spreadsheet.Utilities;
using Telerik.Windows.Controls.Spreadsheet.Worksheets;
using Telerik.Windows.Documents.Spreadsheet.Core;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.TextBased.Csv;
using Telerik.Windows.Documents.Spreadsheet.Model;
using Telerik.Windows.Documents.Spreadsheet.PropertySystem;

namespace CentaurSpreadsheetDemo_NetCore
{
    /// <summary>
    /// Interaction logic for MainDemoControl.xaml
    /// </summary>
    public partial class MainDemoControl : UserControl
    {
        #region Properties

        public List<string> DefaultFontFamilies
        {
            get
            {
                return new List<string>
                {
                    "Arial",
                    "Arial Black",
                    "Calibri",
                    "Comic Sans MS",
                    "Courier New",
                    "Georgia",
                    "Lucida Sans Unicode",
                    "Times New Roman",
                    "Trebuchet MS",
                    "Verdana"
                };
            }
        }

        #endregion

        #region Constructors

        static MainDemoControl()
        {
        }

        public MainDemoControl()
        {
            this.InitializeComponent();

            this.radSpreadsheet.ActiveSheetEditorChanged += this.RadSpreadsheet_ActiveSheetEditorChanged;
            this.radSpreadsheet.Loaded += this.radSpreadsheet_Loaded;
            this.radSpreadsheet.WorkbookChanged += this.RadSpreadsheet_WorkbookChangedAsync;

            this.InitializeChatAuthors();

            FunctionsProvider.SetMostRecentlyUsedFunctionsNames(new string[]
            {
                "COS",
                "Tan",
                "ABS",
                "ACOS",
                "SIN",
            });

            CompositionTarget.Rendering += this.CompositionTarget_Rendering;

            //this.radSpreadsheet.Workbook.Names.Add("GSheet1A1", "=Sheet1!A1", new CellIndex(1, 1));
            //this.radSpreadsheet.Workbook.Names.Add("aaa", @"=""tra"" & ""lala""", new CellIndex(1, 1));
            //this.radSpreadsheet.Workbook.Worksheets[0].Names.Add("LTOSheet1A1", "=Sheet1!A1", new CellIndex(1, 1));
            //this.radSpreadsheet.Workbook.Worksheets[0].Names.Add("LTOSheet1B1", "=Sheet1!A1:B1 Sheet1!B1:C1", new CellIndex(1, 1));

            //this.radSpreadsheet.Workbook.Worksheets.Add();
            //this.radSpreadsheet.Workbook.Worksheets[0].Names.Add("LTOSheet2A1", "=Sheet2!A1", new CellIndex(1, 1));

            //this.radSpreadsheet.Workbook.Worksheets[1].Names.Add("LTOSheet1A1B1", @"=Sheet1!A1:B1", new CellIndex(1, 1));
            //this.radSpreadsheet.Workbook.Worksheets[1].Names.Add("LTOSheet2A1", @"=Sheet2!A1", new CellIndex(1, 1));
            //this.radSpreadsheet.Workbook.Worksheets[1].Names.Add("LTOSheet3A1B1", @"=Sheet3!A1:B1", new CellIndex(1, 1));
        }

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

            // Add welcome message
            this.radChat.AddMessage(
                this.assistantAuthor,
                "Hello! I'm your AI assistant for spreadsheet analysis. I can help you understand your data, create formulas, generate charts, and answer questions about your spreadsheet. What would you like to know?"
            );
        }

        private async void RadChat_SendMessage(object sender, SendMessageEventArgs e)
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

                // Send message to agent workflow
                await this.SendChatMessage(userMessage);
            }
            catch (Exception ex)
            {
                // Hide typing indicator on error
                this.typingIndicator.Visibility = Visibility.Collapsed;

                // Show error message in chat
                this.radChat.AddMessage(
                    this.assistantAuthor,
                    $"Sorry, an error occurred: {ex.Message}"
                );
            }
        }

        private void RadSpreadsheet_WorkbookChangedAsync(object sender, EventArgs e)
        {
            this.InitializeNewChat(true);
        }

        private void InitializeNewChat(bool recreateTools)
        {
            // Clear previous chat history when a new workbook/session is initialized.
            if (this.radChat != null)
            {
                // Clear existing messages if the API exposes a Messages collection.
                this.radChat.MessageListItems?.Clear();
                this.radChat.MessageGroups?.Clear();
                this.radChat.SuggestedActions.Clear();
                this.radChat.SuggestedActionsVisibility = Visibility.Collapsed;
            }

            // Reset last user message tracking.
            this.lastUserMessage = null;

            string key = Environment.GetEnvironmentVariable("AZUREOPENAI_KEY");
            string endpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");
            string model = "gpt-4.1-niky-testing";

            AIAgentConfiguration configuration = new();

            if (recreateTools || this.toolRegistry == null)
            {
                this.toolRegistry = new SpreadAnalysisToolsRegistry(this.radSpreadsheet.Workbook);
            }

            IChatClientFactory chatClientFactory = new OpenAIChatClientFactory(endpoint, key, model, supportsTemperature: true);
            IChatAgentFactory chatAgentFactory = new OpenAIChatAgentFactory(chatClientFactory);

            // Initialize Magentic-One workflow using the factory
            MagenticOneWorkflowConfiguration workflowConfig = new()
            {
                MaxRounds = 20,
                StallThreshold = 3,
                EnableDetailedLogging = true,
                AgentTimeoutSeconds = 60
            };

            // Use DefaultWorkflowFactory for simplified setup
            this._magenticOneWorkflowService = new MagenticOneWorkflowService(
                chatAgentFactory,
                this.toolRegistry,
                configuration: workflowConfig);

            // Subscribe to Magentic-One workflow events
            this._magenticOneWorkflowService.OnOrchestratorMessage += this.OnOrchestratorMessage;
            this._magenticOneWorkflowService.OnAgentAssignment += this.OnAgentAssignment;
            this._magenticOneWorkflowService.OnAgentResponse += this.OnAgentResponse;
            this._magenticOneWorkflowService.OnTaskComplete += this.OnTaskComplete;
            this._magenticOneWorkflowService.OnError += this.OnWorkflowError;

            this.radChat.AddMessage(
                this.assistantAuthor,
                "🔄 Magentic-One workflow initialized! Using multi-agent orchestration."
            );

            this.sessionId = Guid.NewGuid().ToString();
            this._currentRound = 0;
        }

        public async Task SendChatMessage(string message)
        {
            // Reset round counter for this new user message
            // Each SendMessageAsync call starts fresh with round counting
            this._currentRound = 0;
            string response = await this._magenticOneWorkflowService.SendMessageAsync(this.sessionId, message);

            // Display final summary
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                this.typingIndicator.Visibility = Visibility.Collapsed;
                AIMessage aiMessage = new(this.assistantAuthor)
                {
                    AIAnswer = response
                };
                this.radChat.AddMessage(aiMessage);
            });
        }

        private void AddSuggestedActionsForQuestion(string questionContent)
        {
            this.radChat.SuggestedActions.Clear();

            // Detect question type and add appropriate suggested actions
            string lowerQuestion = questionContent.ToLower();

            if ((lowerQuestion.Contains("yes") && lowerQuestion.Contains("no")) ||
                (lowerQuestion.Contains("?") && (lowerQuestion.Contains("would you") || lowerQuestion.Contains("should i") || lowerQuestion.Contains("do you want"))))
            {
                this.radChat.SuggestedActions.Add(new SuggestedAction("Yes"));
                this.radChat.SuggestedActions.Add(new SuggestedAction("No"));
            }
            else if (lowerQuestion.Contains("how many") || lowerQuestion.Contains("which column") || lowerQuestion.Contains("what row"))
            {
                this.radChat.SuggestedActions.Add(new SuggestedAction("Show me"));
                this.radChat.SuggestedActions.Add(new SuggestedAction("Skip this"));
            }
            else
            {
                // Default suggested actions for generic questions
                this.radChat.SuggestedActions.Add(new SuggestedAction("Continue"));
                this.radChat.SuggestedActions.Add(new SuggestedAction("Tell me more"));
                this.radChat.SuggestedActions.Add(new SuggestedAction("Skip"));
            }

            this.radChat.SuggestedActionsVisibility = Visibility.Visible;
        }

        private async void RadChat_UserVoted(object sender, UserVotedEventArgs e)
        {
            try
            {
                string voteMessage = e.VoteType == VoteType.UpVote
                    ? "Thank you for your feedback! I'm glad I could help."
                    : "Thank you for your feedback. I'll try to provide better responses.";

                this.radChat.AddMessage(this.assistantAuthor, voteMessage);
            }
            catch (Exception ex)
            {
                this.radChat.AddMessage(this.assistantAuthor, $"Error processing vote: {ex.Message}");
            }
        }

        private async void RadChat_RegenerateResponse(object sender, RegenerateResponseEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.lastUserMessage))
                {
                    this.radChat.AddMessage(this.assistantAuthor, "No previous message to regenerate.");
                    return;
                }

                // Show typing indicator
                this.typingIndicator.Visibility = Visibility.Visible;

                // Add context message
                this.radChat.AddMessage(this.assistantAuthor, "Let me try that again...");

                // Resend the last message
                await this.SendChatMessage(this.lastUserMessage);
            }
            catch (Exception ex)
            {
                this.typingIndicator.Visibility = Visibility.Collapsed;
                this.radChat.AddMessage(this.assistantAuthor, $"Error regenerating response: {ex.Message}");
            }
        }

        private async Task OnAgentMessageReceived(AgentChatMessage message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // Hide typing indicator
                    this.typingIndicator.Visibility = Visibility.Collapsed;

                    // Validate message content
                    if (string.IsNullOrWhiteSpace(message.Content))
                    {
                        return;
                    }

                    string content = message.Content;

                    // Handle different message types
                    switch (message.ContentType)
                    {
                        case AgentMessageType.Text:
                            this.radChat.AddMessage(this.assistantAuthor, content);
                            break;

                        case AgentMessageType.Task:
                            this.radChat.AddMessage(this.assistantAuthor, $"📋 Task: {content}");
                            break;

                        case AgentMessageType.Question:
                            this.radChat.AddMessage(this.assistantAuthor, $"❓ {content}");
                            this.AddSuggestedActionsForQuestion(content);
                            break;

                        case AgentMessageType.Summary:
                            AIMessage aiMessage = new(this.assistantAuthor)
                            {
                                AIAnswer = content
                            };
                            this.radChat.AddMessage(aiMessage);
                            break;

                        case AgentMessageType.RenderedOutput:
                            this.radChat.AddMessage(this.assistantAuthor, $"✅ {content}");
                            break;

                        default:
                            this.radChat.AddMessage(this.assistantAuthor, content);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this.radChat.AddMessage(this.assistantAuthor, $"Error displaying message: {ex.Message}");
                }
            });
        }

        private IRadSheetEditor activeSheetEditor;

        private void radSpreadsheet_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeNewChat(true);

            //string[,] values = new string[,]
            //{
            //    { "3", "2", "1", "5", "4", "FALSE", "=5/0", "", "=#REF!", "=\"3\"", "=\"2\"", "=\"1\"", "=\"5\"", "=\"4\"", "TRUE", "=4/0" }
            //};

            //radSpreadsheet.Workbook.History.IsEnabled = false;
            //radSpreadsheet.Workbook.SuspendLayoutUpdate();

            //for (int i = 0; i < 150; i++)
            //{
            //    for (int j = 0; j <= 150; j++)
            //    {
            //        radSpreadsheet.ActiveWorksheet.Cells[i, j].SetValue(i + j);
            //    }
            //}

            ////this.radSpreadsheet.ActiveWorksheet.Filter.FilterRange = new CellRange(0, 0, 150, 0);

            //radSpreadsheet.Workbook.History.IsEnabled = true;
            //radSpreadsheet.Workbook.ResumeLayoutUpdate();

            //RadWorksheetEditor editor = (RadWorksheetEditor)this.radSpreadsheet.ActiveSheetEditor;
            //editor.Worksheet.Cells[0, 0].SetFontFamily(new ThemableFontFamily("Arial"));
            //editor.Worksheet.Cells[0, 0].SetFontSize(15);

            //this.GenerateFontsDemoDocument();

            this.radSpreadsheet.Focus();
        }

        private void RadSpreadsheet_ActiveSheetEditorChanged(object sender, EventArgs e)
        {
            if (this.activeSheetEditor != null)
            {
                this.activeSheetEditor.UICommandExecuted -= this.ActiveSheetEditor_UICommandExecuted;
            }

            this.activeSheetEditor = this.radSpreadsheet.ActiveSheetEditor;

            if (this.activeSheetEditor != null)
            {
                this.activeSheetEditor.UICommandExecuted += this.ActiveSheetEditor_UICommandExecuted;
            }
        }

        private void ActiveSheetEditor_UICommandExecuted(object sender, UICommandExecutedEventArgs e)
        {
            if (!this.radSpreadsheet.IsKeyboardFocusWithin())
            {
                this.radSpreadsheet.Focus();
            }
        }

        #endregion

        #region Methods

        private void GenerateFontsDemoDocument(int maxColumn = 5)
        {
            CellIndex currentIndex = new(0, 0);
            IFill odd = new PatternFill(PatternType.Solid, Colors.Orange, Colors.Orange);
            IFill even = new PatternFill(PatternType.Solid, Colors.Orchid, Colors.Orchid);
            Worksheet worksheet = this.radSpreadsheet.ActiveWorksheet;

            foreach (FontFamilyInfo fontFamilyName in this.radSpreadsheet.FontsProvider.RegisteredFonts)
            {
                CellSelection cell = worksheet.Cells[currentIndex];
                cell.SetFill((currentIndex.ColumnIndex + currentIndex.RowIndex) % 2 == 0 ? even : odd);
                cell.SetFontFamily(new ThemableFontFamily(fontFamilyName.FontFamily));
                string value = fontFamilyName.FontFamily.ToString();
                cell.SetValue(value);

                bool shouldMoveToNextRow = currentIndex.ColumnIndex >= maxColumn;
                int nextRow = shouldMoveToNextRow ? currentIndex.RowIndex + 1 : currentIndex.RowIndex;
                int nextColumn = shouldMoveToNextRow ? 0 : currentIndex.ColumnIndex + 1;
                currentIndex = new CellIndex(nextRow, nextColumn);
            }

            worksheet.Columns[new CellRange(new CellIndex(0, 0), new CellIndex(0, maxColumn))].AutoFitWidth();
            worksheet.Rows[new CellRange(new CellIndex(0, 0), currentIndex)].AutoFitHeight();

            worksheet.WorksheetPageSetup.PaperType = Telerik.Windows.Documents.Model.PaperTypes.A3;
            worksheet.WorksheetPageSetup.PageOrientation = Telerik.Windows.Documents.Model.PageOrientation.Landscape;
        }

        private void ToggleHorizontalScrollMode_Click(object sender, RoutedEventArgs e)
        {
            this.set = (this.set == IconsSet.Light) ? IconsSet.Dark : IconsSet.Light;
            IconSources.ChangeIconsSet(this.set);

            RadWorksheetEditor radWorksheet = (RadWorksheetEditor)this.radSpreadsheet.ActiveSheetEditor;
            if (radWorksheet.HorizontalScrollMode == ScrollMode.ItemBased)
            {
                radWorksheet.HorizontalScrollMode = ScrollMode.PixelBased;
            }
            else
            {
                radWorksheet.HorizontalScrollMode = ScrollMode.ItemBased;
            }

            this.radSpreadsheet.Focus();
        }

        private void ToggleVerticalScrollMode_Click(object sender, RoutedEventArgs e)
        {
            RadWorksheetEditor radWorksheet = (RadWorksheetEditor)this.radSpreadsheet.ActiveSheetEditor;
            if (radWorksheet.VerticalScrollMode == ScrollMode.ItemBased)
            {
                radWorksheet.VerticalScrollMode = ScrollMode.PixelBased;
            }
            else
            {
                radWorksheet.VerticalScrollMode = ScrollMode.ItemBased;
            }

            this.radSpreadsheet.Focus();
        }

        #endregion

        #region Test Buttons

        private void TestDateNumberFormat_Click(object sender, RoutedEventArgs e)
        {
            Worksheet worksheet = this.radSpreadsheet.ActiveWorksheet;

            worksheet.Cells[0, 0].SetFormat(new CellValueFormat("d.mmm"));
            worksheet.Cells[0, 0].SetValue("40916");

            worksheet.Cells[0, 1].SetFormat(new CellValueFormat("d.mmm"));
            worksheet.Cells[0, 1].SetValue("40917");
        }

        private void AddValuesButton_Click(object sender, RoutedEventArgs e)
        {
            this.radSpreadsheet.VisibleSize = new SizeI(10, 10);
        }

        private void TestScrolling_Click(object sender, RoutedEventArgs e)
        {
            this.scrollCounter = 0;
            this.start = DateTime.Now;
            //SnippetPerformanceCounter bottleNeck = new SnippetPerformanceCounter("test");
            //RadWorksheetEditor editor = (RadWorksheetEditor)this.radSpreadsheet.ActiveSheetEditor;
            //int scrollCounter = 0;

            //Action scroll = null;
            //scroll = new Action(
            //    () =>
            //    {
            //        editor.Selection.Select(new CellIndex(0, editor.Selection.ActiveCellIndex.ColumnIndex + 1));
            //        scrollCounter++;

            //        if (scrollCounter < Count)
            //        {
            //            Dispatcher.BeginInvoke(scroll);
            //        }
            //        else
            //        {
            //            MessageBox.Show(bottleNeck.TotalMilliseconds.ToString() + " of " +
            //                (DateTime.Now - start).TotalMilliseconds.ToString());
            //        }
            //    }
            //);

            //scroll();
        }

        private const int maxScrollCount = 150;
        private int scrollCounter = maxScrollCount + 1;
        private DateTime start;
        private int frameCount = 0;
        private int lastFrameCount;
        private DateTime lastFrame;
        private readonly List<int> framesPerSecond = new();

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (this.scrollCounter < maxScrollCount)
            {
                RadWorksheetEditor editor = (RadWorksheetEditor)this.radSpreadsheet.ActiveSheetEditor;
                double ellapsed = (DateTime.Now - this.lastFrame).TotalMilliseconds;
                if (this.frameCount % 5 == 0)
                {
                    int fps = (int)Math.Floor(1000d / (ellapsed / (this.frameCount - this.lastFrameCount)));
                    //FramesPerSecondTextBlock.Text = "FramesPerSecond: " + fps;

                    if (fps < 200)
                    {
                        this.framesPerSecond.Add(fps);
                    }
                }

                if (ellapsed > 1000)
                {
                    this.lastFrame = DateTime.Now;
                    this.lastFrameCount = this.frameCount;
                }

                this.frameCount++;

                Action scroll = new(
                    () =>
                    {
                        editor.Selection.Select(new CellIndex(0, editor.Selection.ActiveCellIndex.ColumnIndex + 1));

                        this.scrollCounter++;

                        // TODO: WinRT.
                        if (this.scrollCounter >= maxScrollCount)
                        {
                            //this.AutoScroll.Content = (DateTime.Now - start).TotalMinutes.ToString();

                            //CellValuesUILayer.counter.End();
                            MessageBox.Show((DateTime.Now - this.start).TotalMilliseconds.ToString());// + "\nfps:" + framesPerSecond.Average().ToString());

                            this.framesPerSecond.Clear();
                            //MessageBox.Show((DateTime.Now - start).ToString());
                        }
                    }
                );
                this.Dispatcher.BeginInvoke(scroll);
            }
        }

        private static readonly int Count = 150;
        private readonly int i = 0;
        private void AddTestData_Click(object sender, RoutedEventArgs e)
        {
            this.radSpreadsheet.ActiveWorksheet.SortState.Clear();
        }

        private void SetStyle_Click(object sender, RoutedEventArgs e)
        {
            CellStyle style = CellStyle.CreateTempStyle();
            style.CopyPropertiesFrom(this.radSpreadsheet.Workbook.Styles["Bad7"]);

            style.IsBold = true;
            style.Fill = new PatternFill(PatternType.DiagonalStripe, Colors.Red, Colors.Black);

            this.radSpreadsheet.Workbook.Styles["Bad7"].CopyPropertiesFrom(style);

            this.radSpreadsheet.Workbook.Styles.Add("Fill", CellStyleCategory.Custom, true);
            this.radSpreadsheet.Workbook.Styles["Fill"].Fill = new PatternFill(PatternType.DiagonalStripe, Colors.Red, Colors.Transparent);

            //Workbook workbook = this.radSpreadsheet.Workbook;
            //Worksheet worksheet = this.radSpreadsheet.ActiveWorksheet;
            //CellStyle cellStyle = workbook.Styles.GetByName("testStyle");

            //if (cellStyle == null)
            //{
            //    workbook.ActiveSheet.History.BeginUndoGroup();

            //    cellStyle = workbook.Styles.Add("testStyle");
            //    for (int i = 0; i < Count; i += 2)
            //    {
            //        worksheet.Cells[0, i, Count, i].SetStyleName(cellStyle.Name);
            //    }

            //    workbook.ActiveSheet.History.EndUndoGroup();
            //}

            //((RadWorksheetEditor)this.radSpreadsheet.ActiveSheetEditor).Selection.Cells.SetStyleName("testStyle");

            //this.OnChangeStyleProperties();
        }

        private void ToggleStyleBold_Click(object sender, RoutedEventArgs e)
        {
            Workbook workbook = this.radSpreadsheet.Workbook;

            CellStyle cellStyle = workbook.Styles.GetByName("testStyle");

            if (cellStyle == null)
            {
                return;
            }

            cellStyle.IsBold = !cellStyle.IsBold;
        }

        private void ChangeStyleProperties_Click(object sender, RoutedEventArgs e)
        {
            this.OnChangeStyleProperties();
        }

        private void OnChangeStyleProperties()
        {
            Workbook workbook = this.radSpreadsheet.Workbook;
            CellStyle cellStyle = workbook.Styles.GetByName("testStyle");

            if (cellStyle == null)
            {
                return;
            }

            cellStyle.BeginUpdate();
            //cellStyle.LeftBorder = new CellBorder(CellBorderStyle.DashDot, Colors.Blue);
            //cellStyle.TopBorder = new CellBorder(CellBorderStyle.DashDot, Colors.Blue);
            //cellStyle.RightBorder = new CellBorder(CellBorderStyle.DashDot, Colors.Blue);
            //cellStyle.BottomBorder = new CellBorder(CellBorderStyle.DashDot, Colors.Blue);
            //cellStyle.DiagonalUpBorder = new CellBorder(CellBorderStyle.DashDot, cellStyle.ForeColor);
            //cellStyle.DiagonalDownBorder = new CellBorder(CellBorderStyle.DashDot, cellStyle.ForeColor);
            cellStyle.FontSize++;
            //cellStyle.FontFamily = (cellStyle.FontFamily..Source == "Arial") ? new FontFamily("Verdana") : new FontFamily("Arial");
            cellStyle.IsBold = !cellStyle.IsBold;
            cellStyle.IsItalic = !cellStyle.IsItalic;
            cellStyle.Underline = (cellStyle.Underline == UnderlineType.None) ? UnderlineType.Single : UnderlineType.None;
            cellStyle.ForeColor = (ThemableColor)((cellStyle.ForeColor.LocalValue == Colors.LightGray) ? Colors.Orange : Colors.LightGray);
            cellStyle.Fill = PatternFill.CreateSolidFill((cellStyle.ForeColor.LocalValue == Colors.LightGray) ? Colors.Orange : Colors.LightGray);
            cellStyle.HorizontalAlignment = (cellStyle.HorizontalAlignment == RadHorizontalAlignment.Left) ? RadHorizontalAlignment.Right : RadHorizontalAlignment.Left;
            cellStyle.VerticalAlignment = (cellStyle.VerticalAlignment == RadVerticalAlignment.Bottom) ? RadVerticalAlignment.Top : RadVerticalAlignment.Bottom;
            //cellStyle.Indent++;
            //cellStyle.IsWrapped = !cellStyle.IsWrapped;

            cellStyle.EndUpdate();

            this.radSpreadsheet.UpdateLayout();
        }

        #endregion

        #region Sheets Tests

        private void UpdateSheetInfo()
        {
            //            this.SheetsInfo.Text =
            //                string.Format(
            //@"Active sheet index: {0}
            //Sheets count:{1}", this.radSpreadsheet.Workbook.Sheets.ActiveSheetIndex, this.radSpreadsheet.Workbook.Sheets.Count);
        }

        private void InsertSheet_Click(object sender, RoutedEventArgs e)
        {
            Workbook workbook = this.radSpreadsheet.Workbook;

            workbook.Worksheets.Add();
            workbook.Worksheets.Add();

            using (new UpdateScope(workbook.History.BeginUndoGroup, workbook.History.EndUndoGroup))
            {
                workbook.Worksheets[0].Cells[0, 0, 30, 30].SetValue(0);
                workbook.Worksheets[1].Cells[0, 0, 30, 30].SetValue(1);
                workbook.Worksheets[2].Cells[0, 0, 30, 30].SetValue(2);
            }

            //this.radSpreadsheet.Workbook.Sheets.Insert(SheetType.Worksheet);
            //this.UpdateSheetInfo();
        }

        private void RemoveSheet_Click(object sender, RoutedEventArgs e)
        {
            this.radSpreadsheet.Workbook.Sheets.Remove();
            this.UpdateSheetInfo();
        }

        private void NextWorksheet_Click(object sender, RoutedEventArgs e)
        {
            this.radSpreadsheet.Workbook.Sheets.ActiveSheetIndex = (this.radSpreadsheet.Workbook.Sheets.ActiveSheetIndex + 1) % this.radSpreadsheet.Workbook.Sheets.Count;
            this.UpdateSheetInfo();
        }

        private void PrevWorksheet_Click(object sender, RoutedEventArgs e)
        {
            this.radSpreadsheet.Workbook.Sheets.ActiveSheetIndex = (this.radSpreadsheet.Workbook.Sheets.Count + this.radSpreadsheet.Workbook.Sheets.ActiveSheetIndex - 1) % this.radSpreadsheet.Workbook.Sheets.Count;
            this.UpdateSheetInfo();
        }

        #endregion

        private IconsSet set;
        private MagenticOneWorkflowService _magenticOneWorkflowService;
        private string sessionId;
        private Author userAuthor;
        private Author assistantAuthor;
        private string lastUserMessage;
        private int _currentRound = 0;
        private SpreadProcessingToolsRegistry toolRegistry;

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CsvFormatProvider csvFormatProvider = new();
            using (Stream stream = File.OpenRead(@"C:\Users\demirev\Downloads\Electric_Vehicle_Population_Data.csv"))
            {
                Workbook workbook = csvFormatProvider.Import(stream);

                this.radSpreadsheet.Workbook = workbook;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.InitializeNewChat(false);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }

        private void TopFilterDialogButton_Click(object sender, RoutedEventArgs e)
        {
            this.radSpreadsheet.CommandDescriptors.ShowTopFilterDialog.Command.Execute(0);
        }

        private void CustomFilterDialogButton_Click(object sender, RoutedEventArgs e)
        {
            this.radSpreadsheet.CommandDescriptors.ShowCustomFilterDialog.Command.Execute(0);
        }

        #region Magentic-One Workflow Event Handlers

        private async Task OnOrchestratorMessage(string message)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this._currentRound++;

                // Display orchestrator's reasoning/planning (optional - can be verbose)
                if (message.Contains("```task") || message.Contains("```summary"))
                {
                    // Extract and display key information
                    this.radChat.AddMessage(this.assistantAuthor, $"🤖 Round {this._currentRound}: Orchestrator planning...");
                }
            });
        }

        private async Task OnAgentAssignment(OrchestratorAssignment assignment)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                // Display what task is being assigned
                string assignmentMessage = $"📋 Assigning to {assignment.TargetAgent}: {assignment.Action}";
                this.radChat.AddMessage(this.assistantAuthor, assignmentMessage);
            });
        }

        private async Task OnAgentResponse(string agentName, string response)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                // Display agent's response
                if (!string.IsNullOrWhiteSpace(response))
                {
                    // Parse structured response if it's from ToolsOwner
                    if (agentName == DefaultAgentNames.ToolOwner)
                    {
                        Telerik.Documents.AI.Agents.Core.Tools.ToolOwnerResponse toolOwnerResponse = Telerik.Documents.AI.Agents.Core.Tools.ToolOwnerResponse.Parse(response);

                        if (toolOwnerResponse.Success)
                        {
                            this.radChat.AddMessage(this.assistantAuthor, $"✅ {toolOwnerResponse.Result}");

                            if (!string.IsNullOrWhiteSpace(toolOwnerResponse.Progress))
                            {
                                this.radChat.AddMessage(this.assistantAuthor, $"📊 Progress: {toolOwnerResponse.Progress}");
                            }
                        }
                        else
                        {
                            this.radChat.AddMessage(this.assistantAuthor, $"❌ {toolOwnerResponse.Result}");

                            if (toolOwnerResponse.Errors.Any())
                            {
                                foreach (string error in toolOwnerResponse.Errors)
                                {
                                    this.radChat.AddMessage(this.assistantAuthor, $"⚠️ {error}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Display raw response for other agents
                        this.radChat.AddMessage(this.assistantAuthor, response);
                    }
                }
            });
        }

        private async Task OnTaskComplete(string summary)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                // Task complete - display final summary
                this.radChat.AddMessage(this.assistantAuthor, $"✨ Task completed in {this._currentRound} rounds");
                this.radChat.AddMessage(this.assistantAuthor, summary);

                // Reset round counter for next task
                this._currentRound = 0;
            });
        }

        private async Task OnWorkflowError(string errorMessage, Exception exception)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.radChat.AddMessage(this.assistantAuthor, $"❌ Error: {errorMessage}");
                if (exception != null)
                {
                    this.radChat.AddMessage(this.assistantAuthor, $"Details: {exception.Message}");
                }
            });
        }

        #endregion
    }
}
