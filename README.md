# 🖨️ 3D Print Pricing Calculator

A native C# WPF Windows desktop application and standalone Web App built to help 3D printing businesses and hobbyists accurately calculate the total landed cost of their 3D printed products and generate suggested retail prices.

![App Screenshot](https://raw.githubusercontent.com/Nate-DUDV2/PrintPricingCalculator/master/image.png)

---

## 🚀 Features

This calculator takes the guesswork out of pricing by factoring in hidden expenses—like machine wear-and-tear and local electricity rates—that are frequently overlooked.

* **⚡ Real-Time Auto-Calculation:** Numbers and profit margins update dynamically as you type.
* **🎨 Smart UI & Themes:** Modern "Card" interface with dynamic Light/Dark modes (Desktop version persists preferences via Windows Registry).
* **💾 Save, Load & Export:** Save custom quote configurations as `.3dquote` files and export invoices directly to PDF.
* **🧵 Core Material & Hardware Tracking:** Calculate exact filament costs based on print weight, plus additional hardware (screws, heat-set inserts, magnets).
* **📦 Labor & Packaging:** Account for post-processing time, prep labor, shipping boxes, and postage.
* **⚙️ Advanced Machine Depreciation:** Calculates exact machine hourly running costs using:
  * Printer purchase price & upgrades
  * Estimated lifetime and annual maintenance costs
  * Printer uptime percentage
  * Local electricity rates ($/kWh) and power consumption (W)
* **📈 Margin-Based Pricing:** Instantly generates suggested retail prices at **50%**, **60%**, and **70%** profit margins based on Total Landed Cost.

---

## 🛠️ Built With

* **Desktop Application:** C#, WPF (Windows Presentation Foundation), .NET Framework / .NET Core
* **Web Application:** HTML5, CSS3, Vanilla JavaScript
* **IDE:** Visual Studio 2019+

---

## 💻 How to Access

### Option 1: Live Web Version
Access the web calculator directly in your browser with zero installation:
👉 **[Price Your Prints on DUDV2](https://dudv2.com/pages/price-your-ptints)**  
👉 **[GitHub Pages Live App](https://nate-dudv2.github.io/PrintPricingCalculator/)**

### Option 2: Windows Desktop Application
For the full native Windows desktop experience:
1. Go to the [Releases Page](https://github.com/Nate-DUDV2/PrintPricingCalculator/releases).
2. Download the latest installer files (`setup.exe` and `PricingAppInstaller.msi`).
3. Run `setup.exe` to install the desktop application.  
   *(If Windows SmartScreen appears, click **More info** -> **Run anyway**).*

---

## 👨‍💻 Building from Source

To clone and build the desktop application locally, you will need Git and Visual Studio 2019 (or newer) with .NET desktop development workloads installed.

```bash
git clone [https://github.com/Nate-DUDV2/PrintPricingCalculator.git](https://github.com/Nate-DUDV2/PrintPricingCalculator.git)
