using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using ToolList_Ver1.Extension;
using System.Globalization;
using System.IO.Compression;

namespace ToolList_Ver1.Class
{
    class sanPham
    {
        public string idSanPham { set; get; }
        public string filePath { set; get; }
        public List<WTM> lstWTM = new List<WTM>();
        public void getListWTM(System.Collections.SortedList h)
        {
            foreach (string d in Directory.GetDirectories(filePath))
            {
                WTM temp = new WTM();
                temp.filePath = d;
                temp.getListImg();
                temp.FileCode = Path.GetFileName(d);
                temp.setDataWTM(h);
                lstWTM.Add(temp);
            }
        }
        public sanPham Temp
        {
            get { return this; }
        }
        public void listting(IWebDriver driver, image image)
        {
            foreach (WTM item in lstWTM)
            {
                if (item.data == null)
                    continue;
                string filePathImage = item.filePath + @"\" + image.name;
                string title = item.data.Title;
                string tag = item.data.Tags;
                string description = item.data.Description;
                string fileCode = item.FileCode +"-"+new Random().Next(10,100);
                //click button thêm sản phẩm
                string startFile = Directory.GetDirectories(item.filePath)[0];
                string fileZip = startFile + ".zip";
                if (!File.Exists(fileZip))
                    ZipFile.CreateFromDirectory(startFile, fileZip);
                performDigital(driver, title, tag, description, filePathImage, fileZip, fileCode);
            }
        }

        private void addTag(IWebDriver driver, string tag)
        {            
            IWebElement element = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[2]/div[5]/span/span[1]/span/ul/li/input"), 10);
            int i = 0;
            foreach (string item in tag.Split(','))
            {                                
                element.SendKeys(item);
                i++;                
                while (true)
                {
                    List<IWebElement> elementList = new List<IWebElement>();
                    elementList.AddRange(driver.FindElements(By.XPath("/html/body/span/span/span/ul/li")));
                    element.SendKeys(Keys.Enter);
                    if (elementList.Count == 0)
                    {                        
                        break;
                    }                        
                }
                if (i == 5)
                    break;
            }
        }
        private void clickBtn(IWebElement check)
        {
            while (true)
            {
                if (check.Enabled)
                {
                    check.Click();
                    break;
                }
            }
        }

        public void performDigital(IWebDriver driver, string title, string tag, string description, string filePathImage, string filePathZip,string fileCode)
        {
            driver.Navigate().GoToUrl("https://hozomarket.com/dashboard-2/new-product/");            
            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[1]/div[1]/div/div[1]/a"), 40).Click();
            driver.FindElement(By.XPath("//input[starts-with(@id,\"html5\")]"), 60).SendKeys(filePathImage);
            IWebElement check = driver.FindElement(By.XPath("/html/body/div[12]/div[1]/div/div/div[4]/div/div[2]/button"));
            clickBtn(check);
            driver.FindElement(By.Id("post-title")).SendKeys(title);
            driver.FindElement(By.Id("_regular_price")).SendKeys("2.5");
            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[2]/div[4]/span/span[1]/span/ul/li/input"), 10).SendKeys("sub");
            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[2]/div[4]/span/span[1]/span/ul/li/input")).SendKeys(Keys.Enter);
            addTag(driver, tag);
            driver.FindElement(By.Id("post_content_ifr")).SendKeys(description);
            IWebElement btnSubmit = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[3]/button[2]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnSubmit);
            driver.WaitUntilDocumentIsReady(120);
            driver.FindElement(By.Id("_downloadable"), 60).Click();
            driver.FindElement(By.Id("_virtual")).Click();
            driver.FindElement(By.Id("_sku"), 20).SendKeys(fileCode);
            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[5]/div[2]/div/div/table/tfoot/tr/th/a"), 20).Click();
            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[5]/div[2]/div/div/table/tbody/tr/td[1]/input"), 10).SendKeys(fileCode);
            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[5]/div[2]/div/div/table/tbody/tr/td[2]/p/a")).Click();
            driver.FindElement(By.XPath("//input[starts-with(@id,\"html5\")]"), 60).SendKeys(filePathZip);
            check = driver.FindElement(By.XPath("/html/body/div[16]/div[1]/div/div/div[4]/div/div[2]/button"));
            if (driver.ElementVisible(By.XPath("/html/body/div[16]/div[1]/div/div/div[3]/div[2]/div/div[3]/div/div[3]/div"), 10))
            {
                driver.Navigate().Refresh();
                performDigital(driver,title,tag,description,filePathImage,filePathZip,fileCode);
            }
            clickBtn(check);
            btnSubmit = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/input[4]"), 20);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnSubmit);
            driver.WaitUntilDocumentIsReady(120);
        }        
    }
}
