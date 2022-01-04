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
            int countTag = Form1.countTag;
            int counWTM = Form1.countWTM;
            DateTime today = DateTime.Today;            
            if (Status.Split('|')[1].Equals(""))
            {
                foreach (WTM item in lstWTM)
                {
                    if (LogSPDaList.Any(check => check.Equals(item.FileCode)))
                        continue;
                    else
                    {
                        if (item.data == null || item.lstImage.Count == 0)
                        {
                            Form1.logErr("("+ idSanPham+") "+item.FileCode+" không có data");
                            LogSPDaList.Add(item.FileCode);
                            continue;
                        }
                        if (item.lstImage.Count == 0)
                        {
                            Form1.logErr("(" + idSanPham + ") " + item.FileCode + " không có WTM");
                            LogSPDaList.Add(item.FileCode);
                            continue;
                        }
                        Status = this.idSanPham + "|" + item.FileCode + "|Khởi tạo";
                        LogSPDaList.Add(item.FileCode);
                        Form1.Log(Name_Profile, Status);
                        Form1.updateStatus(idDgv, "", "", item.FileCode + "- Khởi tạo");
                        if (Form1.listWeb)
                            if(Form1.listShopyfi)
                                listShopify(driver, item, Name_Profile, ref Status, idDgv, counWTM);
                            else
                                listWeb(driver, item, Name_Profile, ref Status, idDgv, counWTM);
                        else
                            performStarup(driver, item, Name_Profile, ref Status, idDgv, countTag, counWTM);
                        Form1.Log("LogItemList", string.Join(",", LogSPDaList));
                    }
                }
            }
            else
            {
                WTM item = lstWTM.Find(a => Status.Split('|')[1].ToUpper().Equals(a.FileCode.ToUpper()));
                LogSPDaList.Add(item.FileCode);
                if (Status.Substring(Status.LastIndexOf("|") + 1).Equals("Khởi tạo"))
                {
                    if (Form1.listWeb)
                        if (Form1.listShopyfi)
                            listShopify(driver, item, Name_Profile, ref Status, idDgv, counWTM);
                        else
                            listWeb(driver, item, Name_Profile, ref Status, idDgv, counWTM);
                    else
                        performStarup(driver, item, Name_Profile, ref Status, idDgv, countTag, counWTM);
                }
                else
                {
                    string url = Status.Substring(Status.LastIndexOf("|") + 1);
                    driver.Navigate().GoToUrl(url);
                    performFinish(driver, item, Name_Profile, ref Status, url, 0);
                }
                Form1.Log("LogItemList", string.Join(",", LogSPDaList));
                listting(driver, Status, Name_Profile, idDgv, LogSPDaList);
            }
        }     

        public void listShopify(IWebDriver driver, WTM item, string Name_Profile, ref string Status, int idDgv, int countWTM)
        {
            string title = item.data.Title;
            string description = item.data.Description;
            string price = item.data.Price;
            string fileZip = getFilePathZip(item);
            string linkWeb = Form1.linkWeb;
            string linkUpZip= linkWeb+ @"/apps/digital-downloads/login/fwd_product?id=";

            if (string.IsNullOrEmpty(fileZip))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Không tìm thấy thư mục hoặc file Zip cần tải lên");
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }
            driver.Navigate().GoToUrl(linkWeb+ @"/products/new");

            //Điền title
            driver.FindElementScroll(By.CssSelector("input[name=\"title\"]"), 10).SendKeys(title);

            //Điền description
            driver.SwitchTo().Frame(driver.FindElement(By.CssSelector("iframe[id$='product-description_ifr']")));
            driver.FindElement(By.Id("tinymce")).Clear();
            driver.FindElement(By.Id("tinymce")).SendKeys(description);
            driver.SwitchTo().DefaultContent();

            //Up ảnh wtm lên
            if (!uploadImageShopify(driver, item, countWTM > item.lstImage.Count ? item.lstImage.Count : countWTM))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Lỗi không thể tải lên WTM, sản phẩm không được đăng lên");
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }

            //Điền giá
            driver.FindElementScroll(By.CssSelector("input[name=\"price\"]"), 10).SendKeys(price);

            //Điền Quantity
            driver.FindElementScroll(By.CssSelector("input[type=\"number\"]"), 10).SendKeys("99");

            //Điền SKU
            driver.FindElementScroll(By.Id("InventoryCardSku"), 10).SendKeys(item.FileCode);

            //Chọn loại sản phẩm digital
            if (driver.FindElement(By.CssSelector("input[name=requiresShipping]")).Selected)
                ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('label[for=ShippingCardRequiresShipping]').click()");

            //Active sản phẩm
            var education = driver.FindElement(By.XPath("//form/div/div/div/div/div/div/div/select[contains(@id,'PolarisSelect')]"));
            var selectElement = new SelectElement(education);
            selectElement.SelectByValue("ACTIVE");

            //Điền tag
            driver.FindElementScroll(By.Id("PolarisTextField8"), 10).SendKeys(item.data.Tags);

            //Submit khởi tạo sản phẩm
            driver.FindElement(By.XPath("/html/body/div/div/div[1]/div/main/div/div/div[2]/div/form/div/div[3]/div/div/div[2]/button")).Click();

            //Up zip
            if (driver.ElementVisible(By.XPath("//span[contains(text(),'Success')]"), 200))
            {
                var urlArr = driver.Url.Split('/');
                linkUpZip = linkUpZip + urlArr[urlArr.Length-1];
            }
            driver.Navigate().GoToUrl(linkUpZip);
            if (!uploadZipShopify(driver, fileZip))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Lỗi file Zip " + linkUpZip);
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }
            //Log sau khi hoàn thành
            Status = this.idSanPham + "||";
            Form1.Log(Name_Profile, Status);
        }
        
        public void listWeb(IWebDriver driver, WTM item, string Name_Profile, ref string Status, int idDgv, int countWTM)
        {
            string linkWeb = Form1.linkWeb;
            string title = item.data.Title;
            string description = item.data.Description;
            string price = item.data.Price;
            string categories = item.data.Categories;
            string Discounted_Price = item.data.discounted_price;
            int countErr = 0;

            //Kiểm tra file, tạo file zip nếu không tồn tại, nếu không thể tạo sẽ chuyển sang upload sản phẩm khác
            string fileZip = getFilePathZip(item);
            if (string.IsNullOrEmpty(filePath))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Không tìm thấy thư mục hoặc file Zip cần tải lên");
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }

            driver.Navigate().GoToUrl(linkWeb);            

            string sku = item.FileCode + "-" + new Random().Next(0, 1000);

            //Điền title
            driver.FindElement(By.Id("title")).SendKeys(title);

            //Chọn categories
            IWebElement element = driver.FindElement(By.Id("product_catchecklist"));
            List<IWebElement> countriesList = element.FindElements(By.TagName("label")).ToList();
            foreach (IWebElement items in countriesList)
            {
                if (Regex.Replace(categories.ToLower(), @"\s+", "").Contains(Regex.Replace(items.Text.ToLower(), @"[^0-9a-zA-Z\._]", string.Empty)))
                {
                    items.Click();
                    break;
                }
            }

            //descapsion
            driver.FindElement(By.Id("content-html")).Click();
            driver.FindElement(By.Id("content")).SendKeys(description);
            
            //Tag
            driver.FindElement(By.Id("new-tag-product_tag")).SendKeys(item.data.Tags);
            driver.FindElement(By.XPath("//*[@id=\"product_tag\"]/div/div[2]/input[2]")).Click();

            //Tải ảnh lên
            if (!uploadImgWP(driver, item, countWTM > item.lstImage.Count ? item.lstImage.Count : countWTM))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Lỗi không thể tải lên WTM, sản phẩm không được đăng lên");
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }

            //Click select file digital
            driver.FindElement(By.Id("_virtual")).Click();
            driver.FindElement(By.Id("_downloadable")).Click();
            
            //Điền giá
            driver.FindElement(By.Id("_regular_price")).SendKeys(price);
            if (!Discounted_Price.Equals(""))
                driver.FindElement(By.Id("_sale_price")).SendKeys(Discounted_Price);

            //Up zip
            driver.FindElement(By.XPath("//*[@id=\"general_product_data\"]/div[3]/div/table/tfoot/tr/th/a")).Click();
            driver.FindElement(By.XPath("//*[@id=\"general_product_data\"]/div[3]/div/table/tbody/tr/td[2]/input[1]")).SendKeys(sku);
            driver.FindElement(By.XPath("//*[@id=\"general_product_data\"]/div[3]/div/table/tbody/tr/td[4]/a")).Click();

            if(!uploadZipWP(driver, fileZip, countErr))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " File zip bị lỗi không thể tải lên");
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }
            //Click submit đăng sản phẩm
            bool flag = false;
            do
            {
                if (!flag)
                {
                    IWebElement btnSubmit = driver.FindElement(By.Id("publish"));
                    btnSubmit.Click();
                    flag = true;
                }
            } while (!driver.invisibilityOfElementLocated(By.CssSelector("//*[@id=\"delete-action\"]/a"), 0));

            //Cập nhập status
            if (driver.navigationSuccess("action", 300))
            {
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
            }     
        }

        //List file image
        public void performStarup(IWebDriver driver, WTM item,string Name_Profile, ref string Status, int idDgv, int countTag, int countWTM)
        {
            string title = item.data.Title;
            string description = " "+item.data.Description;
            string price = item.data.Price;
            string categories = item.data.Categories;
            string Discounted_Price = item.data.discounted_price;
            string linkWeb = Form1.linkWeb;

            driver.Navigate().GoToUrl(linkWeb);


            if (!uploadImgWP(driver, item, countWTM > item.lstImage.Count ? item.lstImage.Count : countWTM))
            {
                Form1.logErr("(" + idSanPham + ") " + item.FileCode + " Lỗi không thể tải lên WTM, sản phẩm không được đăng lên");
                Status = this.idSanPham + "||";
                Form1.Log(Name_Profile, Status);
                return;
            }
            
            driver.FindElement(By.Id("post-title")).SendKeys(title);
            driver.FindElementScroll(By.Id("_regular_price"),10).SendKeys(price);

            if (!Discounted_Price.Equals(""))
                driver.FindElement(By.Id("_sale_price")).SendKeys(Discounted_Price);

            addCategory(driver, categories);
            addTag(driver, item, countTag);
            driver.FindElementScroll(By.Id("post_content_ifr"),10).SendKeys(description);

            if (driver.ElementVisible(By.XPath("/html/body/div[1]/div/div/div[2]/div/form/div[3]/button"), 0))
                driver.FindElementScroll(By.CssSelector("button.dokan-btn"), 10).Click();
            else
                ((IJavaScriptExecutor)driver).ExecuteScript("document.querySelector('button.dokan-btn:nth-child(4)').click()");
            
            if (driver.navigationSuccess("product_id", 800))
            {
                string url = driver.Url;
                Status = this.idSanPham + "|" + item.FileCode + "|" + url;
                Form1.Log(Name_Profile, Status);
                Form1.updateStatus(idDgv, "", "", item.FileCode + "- Up zip");
                performFinish(driver, item, Name_Profile, ref Status,url, 0);
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
