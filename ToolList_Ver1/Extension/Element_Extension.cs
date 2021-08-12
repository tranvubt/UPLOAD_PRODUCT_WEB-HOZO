using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using SeleniumExtras.WaitHelpers;
using System.Collections.ObjectModel;

namespace ToolList_Ver1.Extension
{
    static class Element_Extension
    {
        public static void WaitUntilDocumentIsReady(this IWebDriver driver, int timeoutInSeconds)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(wd => js.ExecuteScript("return document.readyState").ToString() == "complete");
        }        
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            IWebElement element = null;
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                element = wait.Until(ExpectedConditions.ElementExists(by));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                System.Threading.Thread.Sleep(500);
                return element;
            }
            catch (Exception)
            {
                return element;
            }  
        }
        public static bool ElementVisible(this IWebDriver driver,By by, int timeoutInSeconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));            
            try
            {
                IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(by));
                bool c = element.Displayed && element.Enabled;
                return c;
            }
            catch (WebDriverTimeoutException )
            {
                return false;
            }            
        }
    }
}
