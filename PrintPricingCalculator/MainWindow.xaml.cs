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

        // --- WINDOWS API MAGIC FOR DARK TITLE BAR ---
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        private void SetTitleBarTheme(bool isDark)
        {
            try
            {
                // Get the secret ID of our application window
                var hwnd = new WindowInteropHelper(this).EnsureHandle();
                if (hwnd == IntPtr.Zero) return;

                int[] themeValue = new int[] { isDark ? 1 : 0 };

                // 20 is the code for Windows 11 and newer Windows 10 versions
                DwmSetWindowAttribute(hwnd, 20, themeValue, 4);
                // 19 is the code for older Windows 10 versions
                DwmSetWindowAttribute(hwnd, 19, themeValue, 4);
            }
            catch
            {
                // Silently ignore if the user is on an ancient version of Windows!
            }
        }

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
            // Update the actual Windows title bar!
            SetTitleBarTheme(chkDarkMode.IsChecked == true);

            if (chkDarkMode.IsChecked == true)
            {
                // --- SWITCH TO DARK MODE PALETTE ---
                this.Resources["AppBackground"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#1E1E1E");
                this.Resources["CardBackground"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
                this.Resources["PrimaryText"] = Brushes.White;
                this.Resources["SecondaryText"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#CCCCCC");
            }
            else
            {
                // --- SWITCH TO LIGHT MODE PALETTE ---
                this.Resources["AppBackground"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#F3F4F6"); // Light Gray Base
                this.Resources["CardBackground"] = Brushes.White; // Pure White Cards
                this.Resources["PrimaryText"] = Brushes.Black; // Black Text
                this.Resources["SecondaryText"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#555555"); // Dark Gray Subtext
            }

            // Save preference to Windows Registry so we remember their choice!
            try
            {
                Microsoft.Win32.RegistryKey appKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\3BCCreations\PricingCalculator");
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

        private void GitHubLogo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // This tells Windows to open the user's default web browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Nate-DUDV2/PrintPricingCalculator",
                UseShellExecute = true
            });
        }

    }


}