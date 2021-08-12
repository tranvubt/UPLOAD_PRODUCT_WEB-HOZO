using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System;
using OpenQA.Selenium.Firefox;
using System.Drawing;
using ToolList_Ver1.Extension;
using System.Threading;

namespace ToolList_Ver1.Class
{
    class Shop
    {
        public string Name_Profile { get; set; }
        public string Loai_SanPham { set; get; }
        public string Date_Creat { get; set; }
        public string Email { set; get; }
        public string Link { set; get; }
        public string Status { set; get; }
        public int  idDgv { set; get; }
        private string filePathProfile { set; get; }
        private sanPham SanPham { set; get; }
        public string getFilePathProfile()
        {
            return this.filePathProfile;
        }
        public void setSanPham(sanPham t)
        {
            this.SanPham = t;
            this.Loai_SanPham = t.idSanPham;
        }
        //Random tên shop
        private string randomNameShop(int n)
        {
            string chars = "abcdefghijklmnopqrstuvwxyz01234567890";
            char[] stringChars = new char[n];
            Random rd = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[rd.Next(chars.Length)];
            }
            return new string(stringChars) + "Design";
        }
        //Tạo driver
        private IWebDriver createDriver(string filePath)
        {
            FirefoxProfile profile = new FirefoxProfile(filePath);
            profile.SetPreference("intl.accept_languages", "us");
            FirefoxOptions options = new FirefoxOptions();
            options.Profile = profile;
            options.AddArguments("disable-infobars");
            options.AddArguments("--no-sandbox");
            options.AddArguments("--disable-application-cache");
            options.AddArguments("--disable-gpu");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--disable-extensions");
            //Ẩn trình duyệt
            //options.AddArguments("--headless");
            var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            FirefoxDriver driver = new FirefoxDriver(driverService, options, TimeSpan.FromMinutes(2));
            driver.Manage().Window.Size = new Size(350, 650);
            return driver;
        }

        public ThreadStart createShop(List<image> lstImageCheck)
        {
            return delegate
            {
                IWebDriver driver = createDriver(this.filePathProfile);
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                switch (Form1.mucDich)
                {
                    case true:
                        driver.Navigate().GoToUrl("https://hozomarket.com/dashboard-2/new-product/");
                        this.SanPham.listting(driver, getImageList(lstImageCheck), Form1.dangList, Form1.mucDich);
                        break;
                    case false:
                        driver.Navigate().GoToUrl("https://www.etsy.com/your/shops/me/tools/listings");
                        this.SanPham.listting(driver, getImageList(lstImageCheck), Form1.dangList, Form1.mucDich);
                        Form1.Log(Name_Profile, "Listed");
                        this.Status = "Listed";
                        Form1.updateStatus(idDgv, Status);
                        break;
                }
                driver.Quit();
                driver.Dispose();
            };
        }
        private image getImageList(List<image> lstImageCheck)
        {
            image image = null;
            if (Status != "Ready" && Status != "Listed")
            {
                image = new image(Status);
            }
            else if (Status.Equals("Ready"))
            {
                foreach (WTM item in SanPham.lstWTM)
                {
                    if (item.lstImage.Count > 0 && item.data != null)
                    {
                        foreach (image a in item.lstImage)
                        {
                            if (!lstImageCheck.Contains(a))
                            {
                                Form1.Log(Name_Profile, a.name);
                                image = a;
                                Status = a.name;
                                Form1.updateStatus(idDgv, Status);
                                lstImageCheck.Add(a);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return image;
        }
        public Shop(string filePathProfile, string nameProfile)
        {
            this.filePathProfile = filePathProfile;
            this.Name_Profile = nameProfile;
        }

        public Shop()
        {
        }
    }
}
