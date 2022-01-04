using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using ToolList_Ver1.Extension;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

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

        //Hàm đăng sản phẩm
        public void listting(IWebDriver driver,string Status,string Name_Profile,int idDgv,List<string> LogSPDaList)
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

        //List file zip
        public void performFinish(IWebDriver driver, WTM item, string Name_Profile, ref string Status, string url, int countErr)
        {
            string fileZip = "";
            string sku = "";
            driver.WaitUntilDocumentIsReady(120);

            if (driver.ElementVisible(By.ClassName("wp-die-message"), 0))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Sản phẩm đã bị xoá");
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }

            fileZip = getFilePathZip(item);
            if (string.IsNullOrEmpty(filePath))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Lỗi file Zip " + url);
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }

            sku = item.FileCode + "-" + new Random().Next(0, 1000);
            IWebElement checkBoxDowload = driver.FindElementScroll(By.Id("_downloadable"),120);
            IWebElement checkBoxVirtual = driver.FindElement(By.Id("_virtual"));
            
            if (!checkBoxDowload.Selected)
                checkBoxDowload.Click();
            if(!checkBoxVirtual.Selected)
                checkBoxVirtual.Click();

            //Điền SKU
            if(driver.ElementVisible(By.Id("_sku"), 0))
            {
                IWebElement textBoxSKU = driver.FindElementScroll(By.Id("_sku"), 10);
                if (string.IsNullOrEmpty(textBoxSKU.GetAttribute("value")))
                    textBoxSKU.SendKeys(sku);
            }            

            //Tải file zip 
            ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('.insert-file-row').click()");
            driver.FindElement(By.CssSelector(".dokan-table > tbody:nth-child(3) > tr:nth-child(1) > td:nth-child(1) > input:nth-child(1)")).SendKeys(sku);
            driver.FindElementScroll(By.CssSelector("a.dokan-btn-sm:nth-child(2)"),60).Click();
            if (!uploadZipWP(driver, fileZip, countErr))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Lỗi file Zip " + url);
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }

            //Submit đăng thành công
            ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('input.dokan-btn').click()");

            //Log sau khi hoàn thành
            Status = this.idSanPham + "||";            
            Form1.Log(Name_Profile, Status);
        }
        
        //Add category
        private void addCategory(IWebDriver driver, string description)
        {
            IWebElement element;
            if (driver.ElementVisible(By.Id("select2-product_cat-container"), 0))
            {
                driver.FindElementScroll(By.Id("select2-product_cat-container"), 10).Click();
                element = driver.FindElement(By.Id("select2-product_cat-results"));
                List<IWebElement> countriesList = element.FindElements(By.TagName("li")).ToList();
                foreach (IWebElement item in countriesList)
                {
                    if (Regex.Replace(description.ToLower(), @"\s+", "").Contains(Regex.Replace(item.Text.ToLower(), @"[^0-9a-zA-Z\._]", string.Empty)))
                    {
                        item.Click();
                        break;
                    }
                }
            }
            else
            {
                int i = 0;
                element = driver.FindElementScroll(By.XPath("/html/body/div[1]/div/div/div[2]/div/form/div[1]/div[2]/div[4]/span/span[1]/span/ul/li/input"), 10);
                foreach (string item in description.Split(','))
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
                            elementList = null;
                            break;
                        }
                    }
                }
                element.SendKeys(Keys.Enter);
            }
        }
        
        //Add tag
        private void addTag(IWebDriver driver, WTM item, int countTag)
        {
            IWebElement element;
            int i = 0;
            if (driver.ElementVisible(By.ClassName("select2-search__field"), 0))
            {
                element = driver.FindElementScroll(By.ClassName("select2-search__field"), 10);
                
                foreach (string items in item.data.Tags.Split(','))
                {
                    element.SendKeys(items);
                    i++;
                    while (true)
                    {
                        List<IWebElement> elementList = new List<IWebElement>();
                        elementList.AddRange(driver.FindElements(By.XPath("/html/body/span[2]/span/span/ul/li")));
                        if (elementList.Count == 0)
                        {
                            elementList = null;
                            break;
                        }
                        if (elementList.Count > 1)
                        {
                            foreach (IWebElement key in elementList)
                            {
                                if (key.Text.ToLower().Contains(new Regex(@"\s\s+").Replace(items.ToLower().Trim(), " ")))
                                {
                                    key.Click();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                if (elementList[0].Text.Equals("No matches found") || elementList[0].Text.Equals("Please enter 2 or more characters"))
                                {
                                    element.Click();
                                    break;
                                }
                                else
                                {
                                    foreach (IWebElement key in elementList)
                                    {
                                        if (key.Text.ToLower().Contains(new Regex(@"\s\s+").Replace(items.ToLower().Trim(), " ")))
                                        {
                                            key.Click();
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        elementList = null;
                    }
                    if (i == countTag)
                        break;
                }
                if (driver.ElementVisible(By.Id("license_-personal"), 0))
                {
                    driver.FindElementScroll(By.Id("license_-personal"), 10).Click();
                    if (!item.FileCode.Contains("PNG"))
                    {
                        driver.FindElementScroll(By.Id("file_includes-png"), 10).Click();
                        driver.FindElementScroll(By.Id("file_includes-svg"), 10).Click();
                    }
                    else
                        driver.FindElementScroll(By.Id("file_includes-png"), 10).Click();
                }                
            }
            else
            {
                element = driver.FindElementScroll(By.XPath("html/body/div[1]/div/div/div[2]/div/form/div[1]/div[2]/div[5]/span/span[1]/span/ul/li/input"), 10);
                foreach (string items in item.data.Tags.Split(','))
                {
                    if (items.Trim().Equals(string.Empty))
                        continue;
                    element.SendKeys(items);
                    i++;
                    while (true)
                    {
                        List<IWebElement> elementList = new List<IWebElement>();
                        elementList.AddRange(driver.FindElements(By.XPath("/html/body/span/span/span/ul/li")));
                        element.SendKeys(OpenQA.Selenium.Keys.Enter);
                        if (elementList.Count == 0)
                        {
                            elementList = null;
                            break;
                        }
                        elementList = null;
                    }
                    if (i == countTag)
                        break;
                }
                element.SendKeys(Keys.Enter);
            }

        }
        
        //Click btn kèm kiểm tra lỗi
        private void clickBtn(IWebElement check1, IWebElement check2)
        {
            while (true)
            {
                if (check1.Enabled)
                {
                    check1.Click();
                    break;
                }
                if (check2.Displayed)
                {
                    break;
                }
            }
        }
        
        //Up image
        private bool uploadImgWP(IWebDriver driver, WTM item, int countWTM)
        {
            for (int i = 0; i < item.lstImage.Count; i++)
            {
                if (i == countWTM)
                    break;
                if (i == 0)
                {
                    if (driver.ElementVisible(By.CssSelector(".dokan-feat-image-btn"),0))
                    {
                        driver.FindElementScroll(By.CssSelector(".dokan-feat-image-btn"), 60).Click();
                        driver.FindElement(By.XPath("//input[starts-with(@id,\"html5\")]"),60).SendKeys(item.lstImage[i].filePath);
                    }
                    else
                    {
                        driver.FindElementScroll(By.Id("set-post-thumbnail"), 10).Click();
                        //((IJavaScriptExecutor)driver).ExecuteScript("document.getElementById('set-post-thumbnail').click()");
                        driver.FindElementScroll(By.Id("menu-item-upload"),60).Click();
                        driver.FindElement(By.XPath("//input[starts-with(@id,\"html5\")]")).SendKeys(item.lstImage[i].filePath);
                    }
                }
                else
                {
                    if (driver.ElementVisible(By.CssSelector(".fa-plus"),0))
                    {
                        driver.FindElementScroll(By.CssSelector(".fa-plus"), 60).Click();
                        driver.FindElement(By.XPath("//input[starts-with(@id,\"html5\")]"), 60).SendKeys(item.lstImage[i].filePath);
                    }
                    else
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('#woocommerce-product-images>div.inside>p>a').click()");
                        driver.FindElementScroll(By.XPath("//input[starts-with(@id,\"html5\")]"), 60).SendKeys(item.lstImage[i].filePath);
                    }
                }
                IWebElement check = driver.FindElement(By.CssSelector(".media-button"));
                IWebElement err = driver.FindElement(By.CssSelector(".upload-errors"));
                clickBtn(check, err);
                if (err.Displayed)
                {
                    return false;
                }
                if (i == 0)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('.upload-errors').remove();");
                    ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('.media-button').remove();");
                }
            }
            return true;
        }

        private bool uploadImageShopify(IWebDriver driver, WTM item, int countWTM)
        {
            for (int i = 0; i < item.lstImage.Count; i++)
            {
                if (i == countWTM)
                    break;
                driver.FindElement(By.CssSelector("#PolarisDropZone1")).SendKeys(item.lstImage[i].filePath);
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return driver.invisibilityOfElementLocated(By.XPath("//span[contains(text(),'Uploading…')]"),180);
        }

        //Up zip
        private bool uploadZipWP(IWebDriver driver, string fileZip, int countErr)
        {
            driver.FindElement(By.XPath("//input[starts-with(@id,\"html5\")]"), 60).SendKeys(fileZip);
            IWebElement check = driver.FindElement(By.CssSelector(".media-button"));
            IWebElement err = driver.FindElement(By.CssSelector(".upload-errors"));
            while (true)
            {
                if (check.Enabled)
                {
                    check.Click();
                    return true;
                }
                if (err.Displayed)
                {
                    if (countErr == 2)
                    {
                        break;
                    }
                    countErr++;
                    uploadZipWP(driver, fileZip, countErr);
                }
            }
            return false;
        }

        private bool uploadZipShopify(IWebDriver driver, string fileZip)
        {
            bool check = false;
            driver.WaitUntilDocumentIsReady(200);
            try
            {
                if (driver.ElementVisible(By.CssSelector("iframe[name='app-iframe']"), 15))
                {
                    driver.SwitchTo().Frame(driver.FindElement(By.CssSelector("iframe[name='app-iframe']")));
                }
                if (driver.ElementExit(By.XPath("//*[@id=\"file\"]"), 10))
                {
                    driver.FindElement(By.XPath("//*[@id=\"file\"]")).SendKeys(fileZip);
                    if (driver.ElementVisible(By.XPath("//strong[contains(text(),'Success:')]"), 120))
                        check = true;
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                driver.SwitchTo().DefaultContent();
                driver.FindElement(By.XPath("//*[@id=\"file\"]")).SendKeys(fileZip);
                if (driver.ElementVisible(By.XPath("//strong[contains(text(),'Success:')]"), 120))
                    check = true;
            }            
            return check;
        }

        //get filePath Zip folder WTM
        private string getFilePathZip(WTM wtmItem)
        {
            string fileZip = string.Empty;
            foreach (string d in Directory.GetFiles(wtmItem.filePath))
            {
                if (Regex.IsMatch(d, @".zip"))
                {
                    fileZip = d;
                    break;
                }
            }
            if (string.IsNullOrEmpty(fileZip))
            {
                string startFile = "";
                try
                {
                    //dồn nhiều folder về 1 folder rồi nén
                    if (Directory.GetDirectories(wtmItem.filePath).Length > 1)
                    {
                        startFile = wtmItem.filePath + @"\"+ wtmItem.FileCode;
                        if (!File.Exists(startFile))
                            Directory.CreateDirectory(startFile);
                        foreach (string item in Directory.GetDirectories(wtmItem.filePath))
                        {
                            if (IsDirectoryEmpty(item))
                                Directory.Delete(item, true);
                            else
                            {
                                Directory.Move(item, Path.Combine(startFile, Path.GetFileName(item)));
                            }
                        }
                    }
                    else
                    {
                        startFile = Directory.GetDirectories(wtmItem.filePath)[0];
                        if (IsDirectoryEmpty(startFile))
                        {
                            Directory.Delete(startFile, true);
                            throw new Exception();
                        }
                    }

                }
                catch (Exception)
                {
                    //Tạo folder chứa file khác wtm để nén
                    startFile = wtmItem.filePath + @"\" + wtmItem.FileCode;
                    Directory.CreateDirectory(startFile);
                    DirectoryInfo d = new DirectoryInfo(wtmItem.filePath);
                    foreach (FileInfo items in d.GetFiles())
                    {
                        if (!Regex.IsMatch(items.Name.ToLower(), @"wtm"))
                            File.Move(items.FullName, startFile + @"\" + items.Name);
                    }
                }
                if (IsDirectoryEmpty(startFile))
                    Directory.Delete(startFile, true);
                else
                    fileZip = zipFile(startFile);
            }
            return fileZip;
        }

        //Kiểm tra folder trống hay không
        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        //Zip folder và trả về filePath file zip
        private string zipFile(string startFile)
        {
            string fileZip = startFile + ".zip";
            ZipFile.CreateFromDirectory(startFile, fileZip);
            return fileZip;
        }
    }
}
