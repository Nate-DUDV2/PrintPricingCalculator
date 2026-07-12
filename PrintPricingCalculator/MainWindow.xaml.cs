using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows.Documents;
using System.Reflection;

namespace PrintPricingCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        private void SetTitleBarTheme(bool isDark)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
                if (hwnd == IntPtr.Zero)
                {
                    return;
                }

                int[] themeValue = new int[] { isDark ? 1 : 0 };
                DwmSetWindowAttribute(hwnd, 20, themeValue, 4);
                DwmSetWindowAttribute(hwnd, 19, themeValue, 4);
            }
            catch { }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double efficiency = double.Parse(txtEfficiency.Text);
                double laborRate = double.Parse(txtLaborRate.Text);
                double printerCost = double.Parse(txtPrinterCost.Text);
                double maintenance = double.Parse(txtMaintenance.Text);
                double lifeYears = double.Parse(txtLife.Text);
                double uptimePct = double.Parse(txtUptime.Text) / 100.0;
                double powerW = double.Parse(txtPower.Text);
                double elecCost = double.Parse(txtElecCost.Text);
                double buffer = double.Parse(txtBuffer.Text);

                double filCost = double.Parse(txtFilamentCost.Text);
                double filReq = double.Parse(txtFilamentReq.Text);
                double printTime = double.Parse(txtPrintTime.Text);
                double laborTimeMins = double.Parse(txtLaborTime.Text);
                double hardwareCost = double.Parse(txtHardwareCost.Text);
                double packagingCost = double.Parse(txtPackagingCost.Text);
                double postage = double.Parse(txtPostage.Text);
                double taxPct = double.Parse(txtTaxRate.Text) / 100.0;
                double discountPct = double.Parse(txtDiscount.Text) / 100.0;

                double licenseCost = double.Parse(txtLicenseCost.Text);
                double expectedSales = double.Parse(txtExpectedSales.Text);
                if (expectedSales <= 0)
                {
                    expectedSales = 1;
                }

                double appliedLicenseCost = licenseCost / expectedSales;

                double lifetimeCost = printerCost + (maintenance * lifeYears);
                double uptimeHrsYr = uptimePct * 8760.0;
                double capitalCostPerHr = (uptimeHrsYr * lifeYears) > 0 ? lifetimeCost / (uptimeHrsYr * lifeYears) : 0;
                double elecCostPerHr = (powerW / 1000.0) * elecCost;
                double printTimeRate = (capitalCostPerHr + elecCostPerHr) * buffer;

                double printedPartCost = (filReq / 1000.0) * filCost * efficiency;
                double totalMaterials = printedPartCost + hardwareCost;
                double totalLabor = (laborTimeMins / 60.0) * laborRate;
                double machineCostTotal = printTime * printTimeRate;
                double totalPackaging = packagingCost + postage;

                double preTaxCost = totalMaterials + totalLabor + totalPackaging + machineCostTotal + appliedLicenseCost;
                double taxAmount = preTaxCost * taxPct;
                double landedCost = preTaxCost + taxAmount;

                lblMaterialsCost.Text = totalMaterials.ToString("C");
                lblLaborCost.Text = totalLabor.ToString("C");
                lblMachineCost.Text = machineCostTotal.ToString("C");
                lblTaxCost.Text = taxAmount.ToString("C");
                lblLandedCost.Text = landedCost.ToString("C");

                double price40 = landedCost / (1.0 - 0.40);
                double price50 = landedCost / (1.0 - 0.50);
                double price60 = landedCost / (1.0 - 0.60);
                double price70 = landedCost / (1.0 - 0.70);

                lblMargin40.Text = price40.ToString("C");
                lblMargin50.Text = price50.ToString("C");
                lblMargin60.Text = price60.ToString("C");
                lblMargin70.Text = price70.ToString("C");

                double selectedBasePrice = 0;
                if (rbMargin40.IsChecked == true)
                {
                    selectedBasePrice = price40;
                }
                else if (rbMargin50.IsChecked == true)
                {
                    selectedBasePrice = price50;
                }
                else if (rbMargin60.IsChecked == true)
                {
                    selectedBasePrice = price60;
                }
                else if (rbMargin70.IsChecked == true)
                {
                    selectedBasePrice = price70;
                }

                double finalCustomerPrice = selectedBasePrice * (1.0 - discountPct);
                lblFinalQuotePrice.Text = finalCustomerPrice.ToString("C");
            }
            catch
            {
                lblMaterialsCost.Text = "$0.00";
                lblLaborCost.Text = "$0.00";
                lblMachineCost.Text = "$0.00";
                lblTaxCost.Text = "$0.00";
                lblLandedCost.Text = "$0.00";
                lblMargin40.Text = "$0.00";
                lblMargin50.Text = "$0.00";
                lblMargin60.Text = "$0.00";
                lblMargin70.Text = "$0.00";
                lblFinalQuotePrice.Text = "$0.00";
            }
        }

        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            SetTitleBarTheme(chkDarkMode.IsChecked == true);
            if (chkDarkMode.IsChecked == true)
            {
                Resources["AppBackground"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#1E1E1E");
                Resources["CardBackground"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
                Resources["PrimaryText"] = Brushes.White;
                Resources["SecondaryText"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#CCCCCC");
            }
            else
            {
                Resources["AppBackground"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#F3F4F6");
                Resources["CardBackground"] = Brushes.White;
                Resources["PrimaryText"] = Brushes.Black;
                Resources["SecondaryText"] = (SolidColorBrush)new BrushConverter().ConvertFrom("#555555");
            }
            try
            {
                RegistryKey appKey = Registry.CurrentUser.CreateSubKey(@"Software\3BCCreations\PricingCalculator");
                appKey.SetValue("IsDarkMode", chkDarkMode.IsChecked == true ? 1 : 0);
            }
            catch { }
        }

        private void BrowseLogo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (dlg.ShowDialog() == true)
            {
                txtLogoPath.Text = dlg.FileName;
            }
        }

        public class QuoteData
        {
            public string FileName { get; set; }
            public string DesignerName { get; set; }
            public string QuoteNumber { get; set; }
            public string CustomerName { get; set; }
            public string Efficiency { get; set; }
            public string LaborRate { get; set; }
            public string PrinterCost { get; set; }
            public string Maintenance { get; set; }
            public string Life { get; set; }
            public string Uptime { get; set; }
            public string Power { get; set; }
            public string ElecCost { get; set; }
            public string Buffer { get; set; }
            public string LicenseCost { get; set; }
            public string ExpectedSales { get; set; }
            public string FilamentCost { get; set; }
            public string FilamentReq { get; set; }
            public string PrintTime { get; set; }
            public string LaborTime { get; set; }
            public string HardwareCost { get; set; }
            public string PackagingCost { get; set; }
            public string Postage { get; set; }
            public string TaxRate { get; set; }
            public string Discount { get; set; }
            public string SelectedMargin { get; set; }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string marginSelection = "50";
            if (rbMargin40.IsChecked == true)
            {
                marginSelection = "40";
            }

            if (rbMargin60.IsChecked == true)
            {
                marginSelection = "60";
            }

            if (rbMargin70.IsChecked == true)
            {
                marginSelection = "70";
            }

            QuoteData data = new QuoteData
            {
                FileName = txtFileName.Text,
                DesignerName = txtDesignerName.Text,
                QuoteNumber = txtQuoteNumber.Text,
                CustomerName = txtCustomerName.Text,
                Efficiency = txtEfficiency.Text,
                LaborRate = txtLaborRate.Text,
                PrinterCost = txtPrinterCost.Text,
                Maintenance = txtMaintenance.Text,
                Life = txtLife.Text,
                Uptime = txtUptime.Text,
                Power = txtPower.Text,
                ElecCost = txtElecCost.Text,
                Buffer = txtBuffer.Text,
                LicenseCost = txtLicenseCost.Text,
                ExpectedSales = txtExpectedSales.Text,
                FilamentCost = txtFilamentCost.Text,
                FilamentReq = txtFilamentReq.Text,
                PrintTime = txtPrintTime.Text,
                LaborTime = txtLaborTime.Text,
                HardwareCost = txtHardwareCost.Text,
                PackagingCost = txtPackagingCost.Text,
                Postage = txtPostage.Text,
                TaxRate = txtTaxRate.Text,
                Discount = txtDiscount.Text,
                SelectedMargin = marginSelection
            };

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "3D Print Quote (*.3dquote)|*.3dquote|JSON File (*.json)|*.json";
            saveFileDialog.Title = "Save Pricing Quote";
            saveFileDialog.FileName = txtQuoteNumber.Text + " - " + txtFileName.Text;

            if (saveFileDialog.ShowDialog() == true)
            {
                string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFileDialog.FileName, jsonString);
                MessageBox.Show("Quote saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "3D Print Quote (*.3dquote)|*.3dquote|JSON File (*.json)|*.json";
            openFileDialog.Title = "Load Pricing Quote";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = File.ReadAllText(openFileDialog.FileName);
                    QuoteData data = JsonSerializer.Deserialize<QuoteData>(jsonString);

                    txtFileName.Text = data.FileName;
                    txtDesignerName.Text = data.DesignerName;
                    txtQuoteNumber.Text = data.QuoteNumber ?? "1001";
                    txtCustomerName.Text = data.CustomerName ?? "Walk-in Customer";
                    txtEfficiency.Text = data.Efficiency;
                    txtLaborRate.Text = data.LaborRate;
                    txtPrinterCost.Text = data.PrinterCost;
                    txtMaintenance.Text = data.Maintenance;
                    txtLife.Text = data.Life;
                    txtUptime.Text = data.Uptime;
                    txtPower.Text = data.Power;
                    txtElecCost.Text = data.ElecCost;
                    txtBuffer.Text = data.Buffer;
                    txtLicenseCost.Text = data.LicenseCost ?? "0.00";
                    txtExpectedSales.Text = data.ExpectedSales ?? "1";
                    txtFilamentCost.Text = data.FilamentCost;
                    txtFilamentReq.Text = data.FilamentReq;
                    txtPrintTime.Text = data.PrintTime;
                    txtLaborTime.Text = data.LaborTime;
                    txtHardwareCost.Text = data.HardwareCost;
                    txtPackagingCost.Text = data.PackagingCost;
                    txtPostage.Text = data.Postage;
                    txtTaxRate.Text = data.TaxRate ?? "0.0";
                    txtDiscount.Text = data.Discount ?? "0.0";

                    string savedMargin = data.SelectedMargin ?? "50";
                    if (savedMargin == "40") rbMargin40.IsChecked = true;
                    else if (savedMargin == "50") rbMargin50.IsChecked = true;
                    else if (savedMargin == "60") rbMargin60.IsChecked = true;
                    else if (savedMargin == "70") rbMargin70.IsChecked = true;

                    Calculate_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not load the file.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
        private void AddTableRow(TableRowGroup group, string label, string value, bool isBold = false, Brush color = null, int fontSize = 14)
        {
            TableRow row = new TableRow();
            Paragraph pLabel = new Paragraph(new Run(label)) { Margin = new Thickness(0, 5, 0, 5), FontSize = fontSize };

            
            string safeValue = string.IsNullOrWhiteSpace(value) ? " " : value;
            Paragraph pValue = new Paragraph(new Run(safeValue)) { Margin = new Thickness(0, 5, 0, 5), FontSize = fontSize, TextAlignment = TextAlignment.Right };

            if (isBold) { pLabel.FontWeight = FontWeights.Bold; pValue.FontWeight = FontWeights.Bold; }
            if (color != null) { pLabel.Foreground = color; pValue.Foreground = color; }

            row.Cells.Add(new TableCell(pLabel));
            row.Cells.Add(new TableCell(pValue));
            group.Rows.Add(row);
        }

        private void BuildHeader(FlowDocument doc, string title)
        {
            Table headerTable = new Table();
            
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(400) });
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(250) });

            TableRowGroup hrg = new TableRowGroup();
            headerTable.RowGroups.Add(hrg);
            TableRow hRow = new TableRow();

            TableCell logoCell = new TableCell();
            if (!string.IsNullOrWhiteSpace(txtLogoPath.Text) && File.Exists(txtLogoPath.Text))
            {
                try
                {
                    
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(txtLogoPath.Text, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    Image logoImg = new Image();
                    logoImg.Source = bitmap;
                    logoImg.MaxHeight = 80;
                    logoImg.MaxWidth = 250;
                    logoImg.Stretch = Stretch.Uniform;
                    logoImg.HorizontalAlignment = HorizontalAlignment.Left;

                    logoCell.Blocks.Add(new BlockUIContainer(logoImg));
                }
                catch
                {
                    
                }
            }
            hRow.Cells.Add(logoCell);

            TableCell titleCell = new TableCell() { TextAlignment = TextAlignment.Right };
            titleCell.Blocks.Add(new Paragraph(new Run(title)) { FontSize = 28, FontWeight = FontWeights.Bold, Foreground = Brushes.SteelBlue, Margin = new Thickness(0) });
            titleCell.Blocks.Add(new Paragraph(new Run($"Quote #: {txtQuoteNumber.Text}")) { Margin = new Thickness(0, 5, 0, 0), FontSize = 14 });
            titleCell.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now.ToShortDateString()}")) { Margin = new Thickness(0), FontSize = 14 });
            hRow.Cells.Add(titleCell);

            hrg.Rows.Add(hRow);
            doc.Blocks.Add(headerTable);

           
            doc.Blocks.Add(new Paragraph(new Run(new string('_', 100))) { Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 10) });

            Paragraph pInfo = new Paragraph() { LineHeight = 22 };
            pInfo.Inlines.Add(new Run("Project: ") { FontWeight = FontWeights.Bold });
            pInfo.Inlines.Add(new Run(txtFileName.Text + "\n"));
            pInfo.Inlines.Add(new Run("Customer: ") { FontWeight = FontWeights.Bold });
            pInfo.Inlines.Add(new Run(txtCustomerName.Text + "\n"));
            pInfo.Inlines.Add(new Run("Designer: ") { FontWeight = FontWeights.Bold });
            pInfo.Inlines.Add(new Run(txtDesignerName.Text));
            doc.Blocks.Add(pInfo);

            
            doc.Blocks.Add(new Paragraph(new Run(new string('_', 100))) { Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 10) });
        }

        private void PrintInternal_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();

               
                doc.PageWidth = 793;
                doc.PageHeight = 1056;
                doc.PagePadding = new Thickness(40);
                doc.ColumnWidth = 713; 
                doc.FontFamily = new FontFamily("Segoe UI");

                BuildHeader(doc, "INTERNAL QUOTE");

                Table itemsTable = new Table();
                
                itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(450) });
                itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(200) });

                TableRowGroup irg = new TableRowGroup();
                itemsTable.RowGroups.Add(irg);

                AddTableRow(irg, "Materials Cost:", lblMaterialsCost.Text);
                AddTableRow(irg, "Labor Cost:", lblLaborCost.Text);
                AddTableRow(irg, "Machine Cost:", lblMachineCost.Text);
                AddTableRow(irg, "Tax:", lblTaxCost.Text);

                doc.Blocks.Add(itemsTable);

                
                doc.Blocks.Add(new Paragraph(new Run(new string('_', 100))) { Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 10) });

                Table marginsTable = new Table();
                marginsTable.Columns.Add(new TableColumn() { Width = new GridLength(450) });
                marginsTable.Columns.Add(new TableColumn() { Width = new GridLength(200) });

                TableRowGroup mrg = new TableRowGroup();
                marginsTable.RowGroups.Add(mrg);

                AddTableRow(mrg, "Total Landed Cost:", lblLandedCost.Text, true, Brushes.DarkRed, 16);
                AddTableRow(mrg, "Suggested Retail Pricing:", " ", true, Brushes.Black, 16);
                AddTableRow(mrg, "40% Margin:", lblMargin40.Text);
                AddTableRow(mrg, "50% Margin:", lblMargin50.Text);
                AddTableRow(mrg, "60% Margin:", lblMargin60.Text);
                AddTableRow(mrg, "70% Margin:", lblMargin70.Text);
                AddTableRow(mrg, "Applied Discount:", $"{txtDiscount.Text}%");

                doc.Blocks.Add(marginsTable);

               
                doc.Blocks.Add(new Paragraph(new Run(new string('_', 100))) { Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 10) });

                Paragraph finalP = new Paragraph(new Run($"Final Customer Quote: {lblFinalQuotePrice.Text}")) { FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Brushes.ForestGreen, TextAlignment = TextAlignment.Right };
                doc.Blocks.Add(finalP);

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Internal Quote");
            }
        }

        private void PrintCustomer_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();

                doc.PageWidth = 793;
                doc.PageHeight = 1056;
                doc.PagePadding = new Thickness(40);
                doc.ColumnWidth = 713;
                doc.FontFamily = new FontFamily("Segoe UI");

                BuildHeader(doc, "PROJECT QUOTE");

                double landedCost = double.Parse(lblLandedCost.Text, System.Globalization.NumberStyles.Currency);
                double baseChosenPrice = 0;
                if (rbMargin40.IsChecked == true)
                {
                    baseChosenPrice = double.Parse(lblMargin40.Text, System.Globalization.NumberStyles.Currency);
                }
                else if (rbMargin50.IsChecked == true)
                {
                    baseChosenPrice = double.Parse(lblMargin50.Text, System.Globalization.NumberStyles.Currency);
                }
                else if (rbMargin60.IsChecked == true)
                {
                    baseChosenPrice = double.Parse(lblMargin60.Text, System.Globalization.NumberStyles.Currency);
                }
                else if (rbMargin70.IsChecked == true)
                {
                    baseChosenPrice = double.Parse(lblMargin70.Text, System.Globalization.NumberStyles.Currency);
                }

                double serviceFee = baseChosenPrice - landedCost;

                Table itemsTable = new Table();
               
                itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(450) });
                itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(200) });

                TableRowGroup irg = new TableRowGroup();
                itemsTable.RowGroups.Add(irg);

                AddTableRow(irg, "Materials:", lblMaterialsCost.Text);
                AddTableRow(irg, "Labor:", lblLaborCost.Text);
                AddTableRow(irg, "Machine & Electricity:", lblMachineCost.Text);
                AddTableRow(irg, "Tax:", lblTaxCost.Text);
                AddTableRow(irg, "Customization & Service:", serviceFee.ToString("C"));

                double discountPct = double.Parse(txtDiscount.Text);
                if (discountPct > 0)
                {
                    AddTableRow(irg, "Discount Applied:", $"-{discountPct}%", true, Brushes.DarkRed);
                }

                doc.Blocks.Add(itemsTable);

              
                doc.Blocks.Add(new Paragraph(new Run(new string('_', 100))) { Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 10) });

                Paragraph finalP = new Paragraph(new Run($"Total Due: {lblFinalQuotePrice.Text}")) { FontSize = 22, FontWeight = FontWeights.Bold, Foreground = Brushes.ForestGreen, TextAlignment = TextAlignment.Right };
                doc.Blocks.Add(finalP);

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Customer Quote");
            }
        }

        private void SaveDefaults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey appKey = Registry.CurrentUser.CreateSubKey(@"Software\3BCCreations\PricingCalculator");
                appKey.SetValue("DefElecCost", txtElecCost.Text);
                appKey.SetValue("DefLaborRate", txtLaborRate.Text);
                appKey.SetValue("DefEfficiency", txtEfficiency.Text);
                appKey.SetValue("DefPrinterCost", txtPrinterCost.Text);
                appKey.SetValue("DefMaintenance", txtMaintenance.Text);
                appKey.SetValue("DefPower", txtPower.Text);
                appKey.SetValue("DefLicenseCost", txtLicenseCost.Text);
                appKey.SetValue("DefExpectedSales", txtExpectedSales.Text);
                appKey.SetValue("DefDesignerName", txtDesignerName.Text);
                appKey.SetValue("DefTaxRate", txtTaxRate.Text);
                appKey.SetValue("DefLogoPath", txtLogoPath.Text);

                _ = MessageBox.Show("Default rates & logo saved successfully!", "Defaults Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show("Could not save defaults: " + ex.Message);
            }
        }

        private void LoadDefaults()
        {
            try
            {
                RegistryKey appKey = Registry.CurrentUser.OpenSubKey(@"Software\3BCCreations\PricingCalculator");
                if (appKey != null)
                {
                    if (appKey.GetValue("DefElecCost") != null) txtElecCost.Text = appKey.GetValue("DefElecCost").ToString();
                    if (appKey.GetValue("DefLaborRate") != null) txtLaborRate.Text = appKey.GetValue("DefLaborRate").ToString();
                    if (appKey.GetValue("DefEfficiency") != null) txtEfficiency.Text = appKey.GetValue("DefEfficiency").ToString();
                    if (appKey.GetValue("DefPrinterCost") != null) txtPrinterCost.Text = appKey.GetValue("DefPrinterCost").ToString();
                    if (appKey.GetValue("DefMaintenance") != null) txtMaintenance.Text = appKey.GetValue("DefMaintenance").ToString();
                    if (appKey.GetValue("DefPower") != null) txtPower.Text = appKey.GetValue("DefPower").ToString();
                    if (appKey.GetValue("DefLicenseCost") != null) txtLicenseCost.Text = appKey.GetValue("DefLicenseCost").ToString();
                    if (appKey.GetValue("DefExpectedSales") != null) txtExpectedSales.Text = appKey.GetValue("DefExpectedSales").ToString();
                    if (appKey.GetValue("DefDesignerName") != null) txtDesignerName.Text = appKey.GetValue("DefDesignerName").ToString();
                    if (appKey.GetValue("DefTaxRate") != null) txtTaxRate.Text = appKey.GetValue("DefTaxRate").ToString();
                    if (appKey.GetValue("DefLogoPath") != null) txtLogoPath.Text = appKey.GetValue("DefLogoPath").ToString();
                }
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            lblSubtitle.Text = $"Your ultimate 3D print quote wizard | v{ver.Major}.{ver.Minor}.{ver.Build}";
            LoadDefaults();
            LoadThemePreference();
        }

        private void LoadThemePreference()
        {
            try
            {
                RegistryKey appKey = Registry.CurrentUser.OpenSubKey(@"Software\3BCCreations\PricingCalculator");
                if (appKey != null && appKey.GetValue("IsDarkMode") != null)
                {
                    int savedMode = (int)appKey.GetValue("IsDarkMode");
                    chkDarkMode.IsChecked = (savedMode == 1);
                }
                else
                {
                    RegistryKey winKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                    if (winKey != null && winKey.GetValue("AppsUseLightTheme") != null)
                    {
                        int systemLightMode = (int)winKey.GetValue("AppsUseLightTheme");
                        chkDarkMode.IsChecked = (systemLightMode == 0);
                    }
                }
                DarkMode_Click(null, null);
            }
            catch { }
        }

        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                try { Calculate_Click(null, null); } catch { }
            }
        }

        private void GitHubLogo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Nate-DUDV2/PrintPricingCalculator",
                UseShellExecute = true
            });
        }
    }
}