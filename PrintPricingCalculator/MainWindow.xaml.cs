using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows.Documents;

namespace PrintPricingCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Native Windows API to change the Title Bar color
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Read Advanced Inputs
                double efficiency = double.Parse(txtEfficiency.Text);
                double laborRate = double.Parse(txtLaborRate.Text);
                double printerCost = double.Parse(txtPrinterCost.Text);
                double maintenance = double.Parse(txtMaintenance.Text);
                double lifeYears = double.Parse(txtLife.Text);
                double uptimePct = double.Parse(txtUptime.Text) / 100.0;
                double powerW = double.Parse(txtPower.Text);
                double elecCost = double.Parse(txtElecCost.Text);
                double buffer = double.Parse(txtBuffer.Text);

                // 2. Read Core Inputs
                double filCost = double.Parse(txtFilamentCost.Text);
                double filReq = double.Parse(txtFilamentReq.Text);
                double printTime = double.Parse(txtPrintTime.Text);
                double laborTimeMins = double.Parse(txtLaborTime.Text);
                double hardwareCost = double.Parse(txtHardwareCost.Text);
                double packagingCost = double.Parse(txtPackagingCost.Text);
                double postage = double.Parse(txtPostage.Text);

                // 3. Machine Cost Calculations 
                double lifetimeCost = printerCost + (maintenance * lifeYears);
                double uptimeHrsYr = uptimePct * 8760.0;
                double capitalCostPerHr = (uptimeHrsYr * lifeYears) > 0 ? lifetimeCost / (uptimeHrsYr * lifeYears) : 0;
                double elecCostPerHr = (powerW / 1000.0) * elecCost;
                double printTimeRate = (capitalCostPerHr + elecCostPerHr) * buffer;

                // 4. Final Cost Calculations
                double printedPartCost = (filReq / 1000.0) * filCost * efficiency;
                double totalMaterials = printedPartCost + hardwareCost;
                double totalLabor = (laborTimeMins / 60.0) * laborRate;
                double machineCostTotal = printTime * printTimeRate;
                double totalPackaging = packagingCost + postage;

                double landedCost = totalMaterials + totalLabor + totalPackaging + machineCostTotal;

                // 5. Update UI Labels
                lblMaterialsCost.Text = totalMaterials.ToString("C");
                lblLaborCost.Text = totalLabor.ToString("C");
                lblMachineCost.Text = machineCostTotal.ToString("C");
                lblLandedCost.Text = landedCost.ToString("C");

                lblMargin50.Text = (landedCost / (1.0 - 0.50)).ToString("C");
                lblMargin60.Text = (landedCost / (1.0 - 0.60)).ToString("C");
                lblMargin70.Text = (landedCost / (1.0 - 0.70)).ToString("C");
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Please ensure all inputs are valid numbers.\n\nError: " + ex.Message, "Input Error");
            //}
            catch
            {
                // If the user leaves a box blank or types a letter, don't show an annoying popup!
                // Just silently reset the totals to $0.00 until they type a valid number again.
                lblMaterialsCost.Text = "$0.00";
                lblLaborCost.Text = "$0.00";
                lblMachineCost.Text = "$0.00";
                lblLandedCost.Text = "$0.00";
                lblMargin50.Text = "$0.00";
                lblMargin60.Text = "$0.00";
                lblMargin70.Text = "$0.00";
            }

        }

        // --- NEW DARK MODE LOGIC ---
        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            // Get the unique ID of our application window
            IntPtr windowPtr = new WindowInteropHelper(this).Handle;

            if (chkDarkMode.IsChecked == true)
            {
                // --- 1. SET TITLE BAR TO DARK ---
                // "20" is the secret Windows code for Immersive Dark Mode
                int[] darkOn = new int[] { 1 };
                DwmSetWindowAttribute(windowPtr, 20, darkOn, 4);

                // --- 2. SET APP BACKGROUND TO DARK ---
                this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                chkDarkMode.Foreground = Brushes.White;

                // Update all TextBlocks to White
                Style darkText = new Style(typeof(TextBlock));
                darkText.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.WhiteSmoke));
                this.Resources[typeof(TextBlock)] = darkText;

                // Update all TextBoxes to Dark Grey
                Style darkTextBox = new Style(typeof(TextBox));
                darkTextBox.Setters.Add(new Setter(TextBox.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"))));
                darkTextBox.Setters.Add(new Setter(TextBox.ForegroundProperty, Brushes.White));
                darkTextBox.Setters.Add(new Setter(TextBox.BorderBrushProperty, Brushes.Gray));
                this.Resources[typeof(TextBox)] = darkTextBox;
            }
            else
            {
                // --- 1. SET TITLE BAR TO LIGHT ---
                int[] darkOff = new int[] { 0 };
                DwmSetWindowAttribute(windowPtr, 20, darkOff, 4);

                // --- 2. SET APP BACKGROUND TO LIGHT ---
                this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F6"));
                chkDarkMode.Foreground = Brushes.Black;

                // Remove the overrides so it goes back to standard light mode
                this.Resources.Remove(typeof(TextBlock));
                this.Resources.Remove(typeof(TextBox));
            }

            // --- NEW: SAVE PREFERENCE TO WINDOWS REGISTRY ---
            try
            {
                RegistryKey appKey = Registry.CurrentUser.CreateSubKey(@"Software\3BCCreations\PricingCalculator");
                appKey.SetValue("IsDarkMode", chkDarkMode.IsChecked == true ? 1 : 0);
            }
            catch { }

        }


        // This class holds all the data we want to save to the file
        public class QuoteData
        {
            public string FileName { get; set; }
            public string DesignerName { get; set; }
            public string Efficiency { get; set; }
            public string LaborRate { get; set; }
            public string PrinterCost { get; set; }
            public string Maintenance { get; set; }
            public string Life { get; set; }
            public string Uptime { get; set; }
            public string Power { get; set; }
            public string ElecCost { get; set; }
            public string Buffer { get; set; }
            public string FilamentCost { get; set; }
            public string FilamentReq { get; set; }
            public string PrintTime { get; set; }
            public string LaborTime { get; set; }
            public string HardwareCost { get; set; }
            public string PackagingCost { get; set; }
            public string Postage { get; set; }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 1. Gather all current text box values into our data object
            QuoteData data = new QuoteData
            {
                FileName = txtFileName.Text,
                DesignerName = txtDesignerName.Text,
                Efficiency = txtEfficiency.Text,
                LaborRate = txtLaborRate.Text,
                PrinterCost = txtPrinterCost.Text,
                Maintenance = txtMaintenance.Text,
                Life = txtLife.Text,
                Uptime = txtUptime.Text,
                Power = txtPower.Text,
                ElecCost = txtElecCost.Text,
                Buffer = txtBuffer.Text,
                FilamentCost = txtFilamentCost.Text,
                FilamentReq = txtFilamentReq.Text,
                PrintTime = txtPrintTime.Text,
                LaborTime = txtLaborTime.Text,
                HardwareCost = txtHardwareCost.Text,
                PackagingCost = txtPackagingCost.Text,
                Postage = txtPostage.Text
            };

            // 2. Open standard Windows Save File dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "3D Print Quote (*.3dquote)|*.3dquote|JSON File (*.json)|*.json";
            saveFileDialog.Title = "Save Pricing Quote";
            saveFileDialog.FileName = txtFileName.Text + " - Quote"; // Default file name

            if (saveFileDialog.ShowDialog() == true)
            {
                // 3. Convert data to text and save to hard drive!
                string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFileDialog.FileName, jsonString);
                MessageBox.Show("Quote saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            // 1. Open standard Windows Open File dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "3D Print Quote (*.3dquote)|*.3dquote|JSON File (*.json)|*.json";
            openFileDialog.Title = "Load Pricing Quote";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 2. Read the file and convert it back to our data object
                    string jsonString = File.ReadAllText(openFileDialog.FileName);
                    QuoteData data = JsonSerializer.Deserialize<QuoteData>(jsonString);

                    // 3. Fill the UI text boxes back up!
                    txtFileName.Text = data.FileName;
                    txtDesignerName.Text = data.DesignerName;
                    txtEfficiency.Text = data.Efficiency;
                    txtLaborRate.Text = data.LaborRate;
                    txtPrinterCost.Text = data.PrinterCost;
                    txtMaintenance.Text = data.Maintenance;
                    txtLife.Text = data.Life;
                    txtUptime.Text = data.Uptime;
                    txtPower.Text = data.Power;
                    txtElecCost.Text = data.ElecCost;
                    txtBuffer.Text = data.Buffer;
                    txtFilamentCost.Text = data.FilamentCost;
                    txtFilamentReq.Text = data.FilamentReq;
                    txtPrintTime.Text = data.PrintTime;
                    txtLaborTime.Text = data.LaborTime;
                    txtHardwareCost.Text = data.HardwareCost;
                    txtPackagingCost.Text = data.PackagingCost;
                    txtPostage.Text = data.Postage;

                    // Automatically calculate pricing after loading
                    Calculate_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not load the file. It might be corrupted.\n\n" + ex.Message, "Error Loading File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {
            // 1. Open the standard Windows Print Dialog
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // 2. Build a clean "Invoice" document in memory
                FlowDocument doc = new FlowDocument();
                doc.PagePadding = new Thickness(50);
                doc.FontFamily = new FontFamily("Segoe UI");

                // Header
                doc.Blocks.Add(new Paragraph(new Run("3D Print Pricing Quote")) { FontSize = 24, FontWeight = FontWeights.Bold });
                doc.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now.ToShortDateString()}")));
                doc.Blocks.Add(new Paragraph(new Run($"Project: {txtFileName.Text}")));
                doc.Blocks.Add(new Paragraph(new Run($"Designer: {txtDesignerName.Text}")));

                doc.Blocks.Add(new BlockUIContainer(new Separator()));

                // Line Items
                doc.Blocks.Add(new Paragraph(new Run($"Materials Cost: \t{lblMaterialsCost.Text}")));
                doc.Blocks.Add(new Paragraph(new Run($"Labor Cost: \t\t{lblLaborCost.Text}")));
                doc.Blocks.Add(new Paragraph(new Run($"Machine Cost: \t{lblMachineCost.Text}")));

                doc.Blocks.Add(new BlockUIContainer(new Separator()));

                // Total
                doc.Blocks.Add(new Paragraph(new Run($"Total Landed Cost: {lblLandedCost.Text}")) { FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkRed });

                // Margin Pricing
                doc.Blocks.Add(new Paragraph(new Run("Suggested Retail Pricing:")) { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 0) });
                doc.Blocks.Add(new Paragraph(new Run($"50% Margin: \t{lblMargin50.Text}")));
                doc.Blocks.Add(new Paragraph(new Run($"60% Margin: \t{lblMargin60.Text}")));
                doc.Blocks.Add(new Paragraph(new Run($"70% Margin: \t{lblMargin70.Text}")));

                // 3. Send the document to the Printer or PDF Saver!
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "3D Print Quote");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // The window is fully loaded, let's check our theme!
            LoadThemePreference();
        }

        private void LoadThemePreference()
        {
            try
            {
                // 1. Check if the user has manually saved a preference for OUR app before
                RegistryKey appKey = Registry.CurrentUser.OpenSubKey(@"Software\3BCCreations\PricingCalculator");

                if (appKey != null && appKey.GetValue("IsDarkMode") != null)
                {
                    // They have used the app before! Load their saved preference.
                    int savedMode = (int)appKey.GetValue("IsDarkMode");
                    chkDarkMode.IsChecked = (savedMode == 1);
                }
                else
                {
                    // 2. First time opening the app! Let's check the Windows System Theme.
                    RegistryKey winKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                    if (winKey != null && winKey.GetValue("AppsUseLightTheme") != null)
                    {
                        int systemLightMode = (int)winKey.GetValue("AppsUseLightTheme");
                        // If Windows is set to 0, it means the system is in Dark Mode
                        chkDarkMode.IsChecked = (systemLightMode == 0);
                    }
                }

                // 3. Actually apply the colors by triggering our existing Dark Mode code!
                DarkMode_Click(null, null);
            }
            catch
            {
                // If anything goes wrong (like a user on a very old Windows version), 
                // silently ignore it and let the app stay in default Light Mode.
            }
        }


        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Only try to auto-calculate if the window is fully loaded and visible.
            // (This prevents the app from crashing while it's still drawing the text boxes on startup!)
            if (this.IsLoaded)
            {
                try
                {
                    // Force the Calculate button to "click" itself!
                    Calculate_Click(null, null);
                }
                catch
                {
                    // If they backspace a number and the box is temporarily empty (""), 
                    // the math will fail. We use this catch block to silently ignore 
                    // the error until they type a new number!
                }
            }
        }

    }


}