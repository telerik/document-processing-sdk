using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace CentaurSpreadsheetDemo_NetCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        public readonly List<string> Themes = new List<string>()
        {
            "Office_Black",
            "Office_Blue",
            "Office_Silver",
            "Windows7",
            "Summer",
            "Vista",
            "Transparent",
            "Expression_Dark",
            "Windows8",
            "Windows8Touch",
            "Office2013",
            "Office2013_LightGray",
            "Office2013_DarkGray",
            "VisualStudio2013",
            "VisualStudio2013_Blue",
            "VisualStudio2013_Dark",
            "Green_Light",
            "Green_Dark",
            "Office2016",
            "Office2016Touch",
            "Fluent",
            "Fluent_Dark",
            "Material",
            "Crystal",
            "Crystal_Dark",
            "VisualStudio2019",
            "Office2019",
            "Office2019_Gray",
            "Office2019_Dark",
        };

        string[] assembliesXamlFiles = {
            "/System.Windows.xaml",
            "/Telerik.Windows.Controls.xaml",
            "/Telerik.Windows.Controls.Input.xaml",
            "/Telerik.Windows.Controls.Navigation.xaml",
            "/Telerik.Windows.Controls.RibbonView.xaml",
            "/Telerik.Windows.Controls.Spreadsheet.xaml",
            "/Telerik.Windows.Controls.SpreadsheetUI.xaml"
        };

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty FocusedElementTypeProperty =
            DependencyProperty.Register("FocusedElementType", typeof(string), typeof(MainWindow), new PropertyMetadata(null));
        public string FocusedElementType
        {
            get { return (string)this.GetValue(FocusedElementTypeProperty); }
            set { this.SetValue(FocusedElementTypeProperty, value); }
        }

        public static readonly DependencyProperty FocusedElementNameProperty =
            DependencyProperty.Register("FocusedElementName", typeof(string), typeof(MainWindow), new PropertyMetadata(null));
        public string FocusedElementName
        {
            get { return (string)this.GetValue(FocusedElementNameProperty); }
            set { this.SetValue(FocusedElementNameProperty, value); }
        }

        #endregion


        #region Constructors

        public MainWindow()
        {
            //To test with localized strings, uncomment the lines below.
            //LocalizationManager.Manager = new LocalizationManager()
            //{
            //    ResourceManager = new PseudoLocalizerResourceManager(LocalizationManager.DefaultResourceManager.BaseName,
            //        typeof(LocalizationManager).Assembly)
            //};

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            InitializeComponent();

            this.MainContentPresenter.Content = new MainDemoControl();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

            this.DataContext = this;
            this.themesComboBox.ItemsSource = Themes;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            object focusedObject = FocusManagerHelper.GetFocusedElement(this);
            Control control = focusedObject as Control;

            this.FocusedElementName = (control != null) ? control.Name : "Unknown";
            this.FocusedElementType = (focusedObject != null) ? focusedObject.GetType().ToString() : "null";
        }

        #endregion

        #region Methods

        private void themesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string themeName = e.AddedItems[0].ToString();

            //Uncomment the following line when using StyleManager
            this.ChangeThemeXaml(themeName);

            //Uncomment the following line when using NoXaml dlls
            //this.ChangeThemeNoXaml(themeName);

            this.ResetContentPresenterContent();
        }

        private void ResetContentPresenterContent()
        {
            MainContentPresenter.Content = null;
            MainContentPresenter.Content = new MainDemoControl();
        }

        private void ChangeThemeXaml(string themeName)
        {
            switch (themeName)
            {
                case "Office_Black":
                    StyleManager.ApplicationTheme = new Office_BlackTheme();
                    break;
                case "Office_Blue":
                    StyleManager.ApplicationTheme = new Office_BlueTheme();
                    break;
                case "Office_Silver":
                    StyleManager.ApplicationTheme = new Office_SilverTheme();
                    break;
                case "Summer":
                    StyleManager.ApplicationTheme = new SummerTheme();
                    break;
                case "Transparent":
                    StyleManager.ApplicationTheme = new TransparentTheme();
                    break;
                case "Vista":
                    StyleManager.ApplicationTheme = new VistaTheme();
                    break;
                case "Windows7":
                    StyleManager.ApplicationTheme = new Windows7Theme();
                    break;
                case "Expression_Dark":
                    StyleManager.ApplicationTheme = new Expression_DarkTheme();
                    break;
                case "Windows8":
                    StyleManager.ApplicationTheme = new Windows8Theme();
                    break;
                case "Windows8Touch":
                    StyleManager.ApplicationTheme = new Windows8TouchTheme();
                    break;
                case "Office2013":
                    Office2013Palette.LoadPreset(Office2013Palette.ColorVariation.White);
                    StyleManager.ApplicationTheme = new Office2013Theme();
                    break;
                case "Office2013_LightGray":
                    Office2013Palette.LoadPreset(Office2013Palette.ColorVariation.LightGray);
                    StyleManager.ApplicationTheme = new Office2013Theme();
                    break;
                case "Office2013_DarkGray":
                    Office2013Palette.LoadPreset(Office2013Palette.ColorVariation.DarkGray);
                    StyleManager.ApplicationTheme = new Office2013Theme();
                    break;
                case "VisualStudio2013":
                    VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Light);
                    StyleManager.ApplicationTheme = new VisualStudio2013Theme();
                    break;
                case "VisualStudio2013_Dark":
                    VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Dark);
                    StyleManager.ApplicationTheme = new VisualStudio2013Theme();
                    break;
                case "VisualStudio2013_Blue":
                    VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Blue);
                    StyleManager.ApplicationTheme = new VisualStudio2013Theme();
                    break;
                case "Green_Dark":
                    GreenPalette.LoadPreset(GreenPalette.ColorVariation.Dark);
                    StyleManager.ApplicationTheme = new GreenTheme();
                    break;
                case "Green_Light":
                    GreenPalette.LoadPreset(GreenPalette.ColorVariation.Light);
                    StyleManager.ApplicationTheme = new GreenTheme();
                    break;
                case "Material":
                    StyleManager.ApplicationTheme = new MaterialTheme();
                    break;
                case "Office2016":
                    StyleManager.ApplicationTheme = new Office2016Theme();
                    break;
                case "Office2016Touch":
                    StyleManager.ApplicationTheme = new Office2016TouchTheme();
                    break;
                case "Fluent":
                    FluentPalette.LoadPreset(FluentPalette.ColorVariation.Light);
                    StyleManager.ApplicationTheme = new FluentTheme();
                    break;
                case "Fluent_Dark":
                    FluentPalette.LoadPreset(FluentPalette.ColorVariation.Dark);
                    StyleManager.ApplicationTheme = new FluentTheme();
                    break;
                case "Crystal":
                    CrystalPalette.LoadPreset(CrystalPalette.ColorVariation.Light);
                    StyleManager.ApplicationTheme = new CrystalTheme();
                    break;
                case "Crystal_Dark":
                    CrystalPalette.LoadPreset(CrystalPalette.ColorVariation.Dark);
                    StyleManager.ApplicationTheme = new CrystalTheme();
                    break;
                case "VisualStudio2019":
                    StyleManager.ApplicationTheme = new VisualStudio2019Theme();
                    break;
                case "Office2019":
                    Office2019Palette.LoadPreset(Office2019Palette.ColorVariation.Light);
                    StyleManager.ApplicationTheme = new Office2019Theme();
                    break;
                case "Office2019_Gray":
                    Office2019Palette.LoadPreset(Office2019Palette.ColorVariation.Dark);
                    StyleManager.ApplicationTheme = new Office2019Theme();
                    break;
                case "Office2019_Dark":
                    Office2019Palette.LoadPreset(Office2019Palette.ColorVariation.Dark);
                    StyleManager.ApplicationTheme = new Office2019Theme();
                    break;
            }
        }

        private void ChangeThemeNoXaml(string themeName)
        {
            UpdateAppBackground(themeName);

            if (themeName.Contains("Office2013"))
            {
                switch (themeName)
                {
                    case "Office2013_LightGrey":
                        Office2013Palette.LoadPreset(Office2013Palette.ColorVariation.LightGray);
                        break;
                    case "Office2013_DarkGrey":
                        Office2013Palette.LoadPreset(Office2013Palette.ColorVariation.DarkGray);
                        break;
                    default:
                        Office2013Palette.LoadPreset(Office2013Palette.ColorVariation.White);
                        break;
                }
                themeName = "Office2013";
            }
            else if (themeName.Contains("VisualStudio2013"))
            {
                switch (themeName)
                {
                    case "VisualStudio2013_Dark":
                        VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Dark);
                        break;
                    case "VisualStudio2013_Blue":
                        VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Blue);
                        break;
                    default:
                        VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Light);
                        break;
                }
                themeName = "VisualStudio2013";
            }
            else if (themeName.Contains("Green"))
            {
                switch (themeName)
                {
                    case "Green_Dark":
                        GreenPalette.LoadPreset(GreenPalette.ColorVariation.Dark);
                        break;
                    default:
                        GreenPalette.LoadPreset(GreenPalette.ColorVariation.Light);
                        break;
                }
                themeName = "Green";
            }
            else if (themeName.Contains("Fluent"))
            {
                switch (themeName)
                {
                    case "Fluent_Dark":
                        FluentPalette.LoadPreset(FluentPalette.ColorVariation.Dark);
                        break;
                    default:
                        FluentPalette.LoadPreset(FluentPalette.ColorVariation.Light);
                        break;
                }
                themeName = "Fluent";
            }
            else if (themeName.Contains("Crystal"))
            {
                switch (themeName)
                {
                    case "Crystal_Dark":
                        CrystalPalette.LoadPreset(CrystalPalette.ColorVariation.Dark);
                        break;
                    default:
                        CrystalPalette.LoadPreset(CrystalPalette.ColorVariation.Light);
                        break;
                }
                themeName = "Crystal";
            }
            else if (themeName.Contains("Office2019"))
            {
                switch (themeName)
                {
                    case "Office2019_Gray":
                        Office2019Palette.LoadPreset(Office2019Palette.ColorVariation.Gray);
                        break;
                    case "Office2019_Dark":
                        Office2019Palette.LoadPreset(Office2019Palette.ColorVariation.Dark);
                        break;
                    default:
                        Office2019Palette.LoadPreset(Office2019Palette.ColorVariation.Light);
                        break;
                }
                themeName = "Office2019";
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            foreach (var file in assembliesXamlFiles)
            {
                string a = string.Format(@"/Telerik.Windows.Themes.{0};component/Themes" + file, themeName);
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri(a, UriKind.RelativeOrAbsolute)
                });
            }
        }

        private void UpdateAppBackground(string themeName)
        {
            if (themeName.Contains("Dark"))
            {
                this.MainWindowRootGrid.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                this.MainWindowRootGrid.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void iconSetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var iconSet = this.iconSetComboBox.Text;
            switch (iconSet)
            {
                case "Dark":
                    IconSources.ChangeIconsSet(IconsSet.Dark);
                    break;
                case "Modern":
                    IconSources.ChangeIconsSet(IconsSet.Modern);
                    break;
                default:
                    IconSources.ChangeIconsSet(IconsSet.Light);
                    break;
            }
        }

        #endregion
    }
}
