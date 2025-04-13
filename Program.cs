using WhatsAppBulkSender;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
string logState = "Init";

#region Minimize Console
[DllImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
[DllImport("kernel32.dll", ExactSpelling = true)]
static extern IntPtr GetConsoleWindow();
const int SW_SHOWMINIMIZED = 2;
IntPtr handle = GetConsoleWindow();
ShowWindow(handle, SW_SHOWMINIMIZED);
#endregion

#region Export Manger
if (!File.Exists(@"selenium-manager\windows\selenium-manager.exe"))
{
    Directory.CreateDirectory(@"selenium-manager\windows\");
    File.WriteAllBytes(@"selenium-manager\windows\selenium-manager.exe", WhatsAppBulkSender.Data.selenium_manager);
}
#endregion


#region Setup Browser
var service = ChromeDriverService.CreateDefaultService();
service.HideCommandPromptWindow = true;

var options = new ChromeOptions();
var BraveLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "Application", "brave.exe");
options.BinaryLocation = File.Exists(BraveLocation) ? BraveLocation : Path.Combine(@"C:\\Program Files", "BraveSoftware", "Brave-Browser", "Application", "brave.exe");
//options.AddArgument("--blink-settings=imagesEnabled=false");
//eagar
 options.AddArgument("--no-sandbox");
options.AddArgument("--disable-dev-shm-usage");
options.AddArgument("--disable-gpu");
options.AddArgument("--remote-debugging-port=9222");
options.AddArgument($"--user-data-dir={Path.GetTempPath()}BraveUserTest");

options.AddArgument("--window-size=700,800");
//options.AddArgument("--incognito");
options.AddArgument("window-position=0,0");

#endregion


var Contacts = File.ReadAllLines("Contacts.txt")
   .Select(x => SpaceRemover().Replace(x, " ").Split(" "))
   .Select(x => new Account(x[0].Trim())).ToList();

var message = File.ReadAllText("Message.txt");


message = Uri.EscapeDataString(message);
using var driver = new ChromeDriver(options);
var wait = new WebDriverWait(driver, TimeSpan.FromHours(10));
foreach (var Contact in Contacts)
{
    try
    {
        driver.Navigate().GoToUrl("https://web.whatsapp.com/send?phone=" + Contact.ContactNumber + "&text=" + message + "&source&data&app_absent");
        wait.Until(((Fn)(d => d.FindElement(By.CssSelector("button[aria-label='Send']")).Displayed)).Handle);
        wait.Until(((Fn)(d => d.FindElement(By.CssSelector("button[aria-label='Send']")).Enabled)).Handle);
        //wait 2 sec
        driver.FindElement(By.CssSelector("button[aria-label='Send']")).Click();
        Console.WriteLine(Contact.ContactNumber + " Sent\n");
        Thread.Sleep(2000);
    }
    catch (ExitException)
    {
        driver.Quit();
        break;
    }

    catch (Exception ex)
    {
        // Contacts.Remove(Contact);
        Console.WriteLine(ex.Message);
        File.AppendAllText("Error Phone.txt", $"{Contact.ContactNumber}{Environment.NewLine}");
        File.AppendAllText("ErrorsLog.txt", $"{DateTime.Now:yyyy-dd-MM hh:mm:ss tt} {Environment.NewLine}{ex.Message}{Environment.NewLine}Log State:{logState}{Environment.NewLine}");
        driver.Quit();
        continue;
    }

}

driver.Quit();
Contacts.Clear();



partial class Program
{
    [GeneratedRegex(@"\s+")]
    public static partial Regex SpaceRemover();
}
record Account(string ContactNumber);