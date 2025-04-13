global using OpenQA.Selenium;
global using OpenQA.Selenium.Support.UI;
global using Fn = System.Func<OpenQA.Selenium.IWebDriver, bool>;
namespace WhatsAppBulkSender;
public class ExitException : Exception;
internal static class Ext
{
    public static bool Handle(this Fn Fun, IWebDriver driver)
    {
        try
        {
            return Fun.Invoke(driver);
        }
        catch (NoSuchWindowException)
        {
            driver.Quit();
            System.Environment.Exit(0);
            throw new ExitException();
        }
    }
    public static void WaitForCaptcha(this IWebDriver driver)
    {
        try
        {
            new WebDriverWait(driver, TimeSpan.FromSeconds(3)).Until(((Fn)(d => d.FindElement(By.TagName("h4")).Displayed)).Handle);
            for (var retry = 0; retry < 2 && driver.FindElement(By.TagName("h4")).Text is "Enter the characters you see below"; retry++)
                ((IJavaScriptExecutor)driver).ExecuteScript("window.location.reload()");
        }
        catch
        {
            Console.WriteLine("Done");
        }
    }
}