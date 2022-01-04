using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using SeleniumExtras.WaitHelpers;
using System.Threading;

namespace ToolList_Ver1.Extension
{
    static class Element_Extension
    {
        public static void WaitUntilDocumentIsReady(this IWebDriver driver, int timeoutInSeconds)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(wd => js.ExecuteScript("return document.readyState").ToString().Equals("complete"));
        }
        public static IWebElement FindElementScroll(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            IWebElement element = null;
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                element = wait.Until(ExpectedConditions.ElementIsVisible(by));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                Thread.Sleep(1000);
                return element;
            }
            catch (Exception)
            {
                return element;
            }  
        }
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            IWebElement element = null;
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                element = wait.Until(ExpectedConditions.ElementExists(by));
                return element;
            }
            catch (Exception)
            {
                return element;
            }
        }
        public static bool ElementExit(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            try
            {
                IWebElement element = wait.Until(ExpectedConditions.ElementExists(by));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //Kiểm tra một phần tử đã bật trên trang hay chưa
        public static bool ElementVisible(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            try
            {
                IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(by));
                bool c = element.Enabled;
                return c;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }
        //Kiểm tra đã chuyển trang thành công chưa bằng url
        public static bool navigationSuccess(this IWebDriver driver, string url, int timeoutInSeconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            try
            {
                bool c = wait.Until(ExpectedConditions.UrlContains(url));
                return c;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }
        //Kiểm tra phần tử đã biến mất chưa
        public static bool invisibilityOfElementLocated(this IWebDriver driver, By elementLocator, int timeout)
        {
            bool check = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.InvisibilityOfElementLocated(elementLocator));
            return check;
        }
    }
}
