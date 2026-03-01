# 🖨️ 3D Print Pricing Calculator

A native Windows desktop application built with C# and WPF to help 3D printing businesses and hobbyists accurately calculate the total landed cost of their 3D printed products and generate suggested retail prices.

![App Screenshot](https://github.com/Nate-DUDV2/PrintPricingCalculator/blob/master//image1.png)


## 🚀 Features

This calculator takes the guesswork out of pricing by factoring in hidden costs that are often forgotten, including machine wear-and-tear and electricity.

* **Core Material Tracking:** Calculate exact filament costs based on print weight, plus any extra hardware used (screws, magnets, inserts).
* **Labor & Packaging:** Factor in post-processing time, boxing, and postage.
* **Advanced Machine Depreciation:** Calculates the exact cost per hour to run your machine based on:
  * Printer purchase price & upgrades
  * Estimated lifetime and annual maintenance
  * Printer uptime percentage
  * Local electricity rates and machine power consumption
* **Margin-Based Pricing:** Instantly generates suggested retail prices at 50%, 60%, and 70% profit margins based on the Total Landed Cost.

## 🛠️ Built With

* **C#**
* **WPF (Windows Presentation Foundation)**
* **.NET Framework / .NET Core**
* Visual Studio 2019

## 💻 Installation (For Users)

If you just want to use the app without looking at the code, you can download the installer:

1. Go to the Releases tab of this repository.
2. Download the latest setup.exe and PricingAppInstaller.msi files.
3. Run setup.exe to install the application to your Windows desktop. 
   *(Note: If Windows SmartScreen displays a warning, click "More info" -> "Run anyway").*


   ## 👨‍💻 Building from Source (For Developers)

To clone and run this application from source, you'll need Git and Visual Studio 2019 (or newer) installed on your computer.

1. Clone the repository:
   git clone https://github.com/Nate-DUDV2/PrintPricingCalculator.git

2. Open the .sln file in Visual Studio.
3. Set PrintPricingCalculator as the startup project.
4. Press F5 or click Start to build and run the application.

## 📝 How to Use

1. **Advanced Inputs (Sidebar):** Start here to define your shop's variables. Set your electricity cost, hourly labor rate, and machine depreciation factors. These usually stay the same across most of your prints.
2. **Core Inputs:** Enter the specifics for the individual part you are quoting (Filament used, Print Time, extra hardware).
3. **Packaging:** Add the cost of your shipping boxes and postage.
4. **Calculate:** Click the blue Calculate Pricing button to get your total landed cost and suggested retail prices!

## 🏆 Credits & Acknowledgments

A huge thank you to the awesome team who helped bring this application to life!

* **3BC Creations** - The Spreadsheet Wiz 📊 *(Original math and calculator spreadsheet design)*
* **Nates Print Shop** - Programmer 💻 *(C# & WPF Application Development)*
* **Michael Print Craft with Cards** - User Experience (UX) ✨ *(Application layout, flow, and design)*
* **3D Everything** - Beta Tester 🐛 *(Testing the math, finding bugs, and QA)*

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! 
Feel free to check the issues page if you want to contribute.

## 📜 License

This project is open-source and available under the MIT License.


