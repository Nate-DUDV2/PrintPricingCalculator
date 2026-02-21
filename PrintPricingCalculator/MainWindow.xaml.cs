using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;

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
            catch (Exception ex)
            {
                MessageBox.Show("Please ensure all inputs are valid numbers.\n\nError: " + ex.Message, "Input Error");
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
        }
    }
}
