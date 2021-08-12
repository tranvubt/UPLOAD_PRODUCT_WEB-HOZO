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
        public void listting(IWebDriver driver, image image, bool digital, bool mucDich)
        {
            bool flag = false;
            foreach (WTM item in lstWTM)
            {
                if (item.data == null)
                    continue;
                string filePathImage = item.filePath + @"\" + image.name;
                string title = item.data.Title;
                string tag = item.data.Tags;
                string description = item.data.Description;
                //click button thêm sản phẩm
                driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[1]/div[1]/div/div[1]/a"),40).Click();
                driver.FindElement(By.XPath("//input[starts-with(@id,\"html5\")]")).SendKeys(filePathImage);
                driver.FindElement(By.Id("post-title")).SendKeys(title);
                driver.FindElement(By.Id("_regular_price")).SendKeys("2.5");
                driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[2]/div[4]/span/span[1]/span/ul/li/input"), 10).SendKeys("sub");
                driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[2]/div[4]/span/span[1]/span/ul/li/input")).SendKeys(Keys.Enter);
                addTag(driver, tag);
                driver.FindElement(By.Id("post_content_ifr")).SendKeys(description);
                int a = 2;
            }
        }

        private void addTag(IWebDriver driver, string tag)
        {
            IWebElement element = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div/div/div[2]/div/div/div/article/div/form/div[1]/div[2]/div[5]/span/span[1]/span/ul/li/input"), 10);
            foreach (string item in tag.Split(','))
            {                                
                element.SendKeys(item);
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
            }
        }

        private void performPhysical(IWebDriver driver, string title, string tag, string description, string filePathImage, ref bool flag, bool mucDich)
        {
            while (true)
            {
                try
                {
                    driver.FindElement(By.Id("listing-edit-image-upload"), 120).SendKeys(filePathImage);
                    break;
                }
                catch (Exception)
                {
                }
            }
            //Điền title
            addTitle(driver, title);
            //Chọn About this listing
            addAboutlisting(driver);
            //Chọn Category
            addtaxonomy(driver);
            //Điền Description
            driver.FindElement(By.Id("description-text-area-input"), 60).SendKeys(description);
            //Điền tag
            driver.FindElement(By.Id("tags"), 60).SendKeys(tag);
            if (driver.ElementVisible(By.XPath("/html/body/div[3]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[5]/div[24]/div/fieldset/div[2]/div[1]/div[1]/div/div[2]"), 0))
                driver.FindElement(By.XPath("/html/body/div[3]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[5]/div[24]/div/fieldset/div[2]/div[1]/div[1]/div/div[2]")).Click();
            else
                driver.FindElement(By.XPath("/html/body/div[4]/div[2]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[5]/div[27]/div/fieldset/div[2]/div[1]/div[1]/div/div[2]")).Click();
            //Điền Price
            driver.FindElement(By.Id("price_retail-input"), 60).SendKeys(Form1.price);
            //Điền Quantity
            driver.FindElement(By.Id("quantity_retail-input"), 60).SendKeys(Form1.quantity);
            //[Điền form Shipping]            
            //Chọn Shipping prices            
            new SelectElement(driver.FindElement(By.Id("profile_type"), 60)).SelectByValue("manual");
            if (driver.ElementVisible(By.Id("origin_postal_code"), 0))
                driver.FindElement(By.Id("origin_postal_code")).SendKeys("96773");
            //Chọn Processing time
            new SelectElement(driver.FindElement(By.Id("processing_time_select"), 60)).SelectByValue("4");
            //Chờ load ảnh xong và continue
            if (mucDich)
                if (driver.ElementVisible(By.XPath("/html/body/div[4]/div[2]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[2]/div/div/div[3]"), 180))
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("document.getElementsByClassName('btn btn-primary')[0].click()");
                    File.Delete(filePathImage);
                    flag = false;
                }
                else
                {
                    driver.Navigate().Refresh();
                    performPhysical(driver, title, tag, description, filePathImage, ref flag, mucDich);
                }
            else
            if (driver.ElementVisible(By.XPath("/html/body/div[3]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[2]/div/div/div[3]"), 180))
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("document.getElementsByClassName('btn btn-primary')[0].click()");
                File.Delete(filePathImage);
                flag = false;
            }
            else
            {
                driver.Navigate().Refresh();
                performPhysical(driver, title, tag, description, filePathImage, ref flag, mucDich);
            }
        }
        public void performDigital(IWebDriver driver, string title, string tag, string description, string filePathImage, string filePathZip, ref bool flag, bool mucDich)
        {
            driver.FindElement(By.Id("listing-edit-image-upload"), 120).SendKeys(filePathImage);
            //Điền title
            addTitle(driver, title);
            //Chọn About this listing
            addAboutlisting(driver);
            //Chọn Category
            addtaxonomy(driver);
            driver.FindElement(By.XPath("/html/body/div[4]/div[2]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[5]/div[16]/div/div/div[2]/div/div[2]"), 0).Click();
            //Điền Description
            driver.FindElement(By.Id("description-text-area-input"), 60).SendKeys(description);
            //Điền tag
            driver.FindElement(By.Id("tags"), 60).SendKeys(tag);
            if (driver.ElementVisible(By.XPath("/html/body/div[3]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[5]/div[24]/div/fieldset/div[2]/div[1]/div[1]/div/div[2]"), 0))
                driver.FindElement(By.XPath("/html/body/div[3]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[5]/div[24]/div/fieldset/div[2]/div[1]/div[1]/div/div[2]")).Click();
            else
                driver.FindElement(By.XPath("/html/body/div[4]/div[2]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[5]/div[26]/div/fieldset/div[2]/div[1]/div[1]/div/div[2]")).Click();
            //Điền Price
            driver.FindElement(By.Id("price_retail-input"), 60).SendKeys(Form1.price);
            //Điền Quantity
            driver.FindElement(By.Id("quantity_retail-input"), 60).SendKeys(Form1.quantity);
            //[Điền form Shipping]
            //Chọn Shipping prices
            driver.FindElement(By.Id("listing-edit-digital-upload"), 0).SendKeys(filePathZip);
            //Chờ load ảnh xong và continue            
            try
            {
                bool check1;
                bool check2 = driver.ElementVisible(By.Id("file-name-"), 180);
                if (mucDich)
                    check1 = driver.ElementVisible(By.XPath("/html/body/div[4]/div[2]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[2]/div/div/div[3]"), 180);
                else
                    check1 = driver.ElementVisible(By.XPath("/html/body/div[3]/section/div/div[4]/div/div/div/div[2]/div/div/div/div[2]/div/div/div[3]"), 180);
                if (check1 && check2)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("document.getElementsByClassName('btn btn-primary')[0].click()");
                    File.Delete(filePathZip);
                    File.Delete(filePathImage);
                    flag = false;
                }
            }
            catch (Exception)
            {
                driver.Navigate().Refresh();
                performDigital(driver, title, tag, description, filePathImage, filePathZip, ref flag, mucDich);
            }
        }
        public void addTitle(IWebDriver driver, string title)
        {
            int check = driver.ElementVisible(By.Id("title-input"), 0) ? 0 : 1;
            switch (check)
            {
                case 0:
                    if(title.Length <= 140)
                        driver.FindElement(By.Id("title-input"), 10).SendKeys(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.TextInfo.ToLower(title)));
                    else
                        driver.FindElement(By.Id("title-input"), 10).SendKeys(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.TextInfo.ToLower(title.Substring(0, 139))));
                    break;
                case 1:
                    if (title.Length <= 140)
                        driver.FindElement(By.Id("title"), 10).SendKeys(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.TextInfo.ToLower(title)));
                    else
                        driver.FindElement(By.Id("title"), 10).SendKeys(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.TextInfo.ToLower(title.Substring(0, 139))));
                    break;
            }
        }
        public void addtaxonomy(IWebDriver driver)
        {
            driver.FindElement(By.Id("taxonomy-search"), 10).Click();
            driver.FindElement(By.Id("taxonomy-search")).SendKeys("Logos & Branding");
            if (driver.ElementVisible(By.Id("taxonomy-search-results-option-2"), 30))
            {
                driver.FindElement(By.Id("taxonomy-search")).SendKeys(Keys.Enter);
            }
            else
            {
                driver.FindElement(By.Id("taxonomy-search")).Clear();
                addtaxonomy(driver);
            }
        }
        public void addAboutlisting(IWebDriver driver)
        {
            if (driver.ElementVisible(By.Id("who_made-input"), 0))
                new SelectElement(driver.FindElement(By.Id("who_made-input"), 10)).SelectByValue("i_did");
            else
                new SelectElement(driver.FindElement(By.Id("who_made"), 10)).SelectByValue("i_did");
            if (driver.ElementVisible(By.Id("is_supply-input"), 0))
                new SelectElement(driver.FindElement(By.Id("is_supply-input"), 10)).SelectByValue("false");
            else
                new SelectElement(driver.FindElement(By.Id("is_supply"), 10)).SelectByValue("0");
            if (driver.ElementVisible(By.Id("when_made-input"), 0))
                new SelectElement(driver.FindElement(By.Id("when_made-input"), 10)).SelectByValue("made_to_order");
            else
                new SelectElement(driver.FindElement(By.Id("when_made"), 10)).SelectByValue("made_to_order");
        }
    }
}
