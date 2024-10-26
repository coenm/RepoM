// namespace UiTests;
//
// using System.Diagnostics;
// using FlaUI.Core.Input;
// using FlaUI.Core.WindowsAPI;
// using FlaUI.Core;
// using FlaUI.UIA3;
// using RepoM.ActionMenu.Interface.YamlModel.Templating;
// using System.Windows.Automation;
// using FlaUI.Core.Tools;
// using Xunit;
//
// public class CalculatorTests : FlaUiTestBase
// {
//     protected override AutomationBase GetAutomation()
//     {
//         return new UIA3Automation();
//     }
//
//     // [Fact]
//     // public void CalculatorTest()
//     // {
//     //     var window = Application.GetMainWindow(Automation);
//     //     var calc = OperatingSystem.IsWindows10() ? (ICalculator)new Win10Calc(window) : new LegacyCalc(window);
//     //
//     //     // Switch to default mode
//     //     System.Threading.Thread.Sleep(1000);
//     //     Keyboard.TypeSimultaneously(VirtualKeyShort.ALT, VirtualKeyShort.KEY_1);
//     //     Wait.UntilInputIsProcessed();
//     //     Application.WaitWhileBusy();
//     //     System.Threading.Thread.Sleep(1000);
//     //
//     //     // Simple addition
//     //     calc.Button1.Click();
//     //     calc.Button2.Click();
//     //     calc.Button3.Click();
//     //     calc.Button4.Click();
//     //     calc.ButtonAdd.Click();
//     //     calc.Button5.Click();
//     //     calc.Button6.Click();
//     //     calc.Button7.Click();
//     //     calc.Button8.Click();
//     //     calc.ButtonEquals.Click();
//     //     Application.WaitWhileBusy();
//     //     var result = calc.Result;
//     //     // Assert.That(result, Is.EqualTo("6912"));
//     //
//     //     // Date comparison
//     //     using (Keyboard.Pressing(VirtualKeyShort.CONTROL))
//     //     {
//     //         Keyboard.Type(VirtualKeyShort.KEY_E);
//     //     }
//     // }
//
//     protected override Application StartApplication()
//     {
//         if (OperatingSystem.IsWindows10())
//         {
//             // Use the store application on those systems
//             return Application.LaunchStoreApp("Microsoft.WindowsCalculator_8wekyb3d8bbwe!App");
//         }
//
//         if (OperatingSystem.IsWindowsServer2016() || OperatingSystem.IsWindowsServer2019())
//         {
//             // The calc.exe on this system is just a stub which launches win32calc.exe
//             return Application.Launch("win32calc.exe");
//         }
//
//         return Application.Launch("calc.exe");
//     }
// }