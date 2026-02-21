using System;
using System.Windows;

namespace PrintPricingCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

                // 3. Machine Cost Calculations (Based strictly on your Excel sheet)
                double lifetimeCost = printerCost + (maintenance * lifeYears);
                double uptimeHrsYr = uptimePct * 8760.0;
                double capitalCostPerHr = (uptimeHrsYr * lifeYears) > 0 ? lifetimeCost / (uptimeHrsYr * lifeYears) : 0;
                double elecCostPerHr = powerW / 1000.0 * elecCost;
                double printTimeRate = (capitalCostPerHr + elecCostPerHr) * buffer;

                // 4. Final Cost Calculations
                double printedPartCost = filReq / 1000.0 * filCost * efficiency;
                double totalMaterials = printedPartCost + hardwareCost;
                double totalLabor = laborTimeMins / 60.0 * laborRate;
                double machineCostTotal = printTime * printTimeRate;
                double totalPackaging = packagingCost + postage;

                double landedCost = totalMaterials + totalLabor + totalPackaging + machineCostTotal;

                // 5. Update UI Labels (Formats as Currency "$0.00")
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
                _ = MessageBox.Show("Please ensure all inputs are valid numbers.\n\nError: " + ex.Message, "Input Error");
            }
        }
    }
}
