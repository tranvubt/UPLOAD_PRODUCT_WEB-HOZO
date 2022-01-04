using OpenQA.Selenium;
using System.Collections.Generic;
using System;
using OpenQA.Selenium.Firefox;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ToolList_Ver1.Class
{
    class Shop
    {
        public string Name_Profile { get; set; }
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
        }
        //Tạo driver
        private IWebDriver createDriver(string filePath)
        {
            FirefoxProfile profile = new FirefoxProfile(filePath);
            profile.SetPreference("intl.accept_languages", "us");
            FirefoxOptions options = new FirefoxOptions();
            options.Profile = profile;
            options.AddArguments("--no-sandbox");
            options.AddArguments("--disable-application-cache");
            options.AddArguments("--disable-gpu");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--disable-extensions");
            options.SetPreference("dom.webdriver.enabled", false);
            options.SetPreference("webdriver_enable_native_events", false);
            options.SetPreference("webdriver_assume_untrusted_issuer", false);
            options.SetPreference("media.peerconnection.enabled", false);
            options.SetPreference("media.navigator.permission.disabled", true);
            //Ẩn trình duyệt
            //options.AddArguments("--headless");
            var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            FirefoxDriver driver = new FirefoxDriver(driverService, options, TimeSpan.FromMinutes(2));
            driver.Manage().Window.Size = new Size(900,500);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(5);
            return driver;
        }

        public ThreadStart createShop(List<string> LogSPDaList)
        {
            return delegate
            {
                if (SanPham == null)
                    return;
                if (!Status.Split('|')[0].Equals("") && !Status.Split('|')[0].Equals(SanPham.idSanPham))
                {
                    DialogResult res = MessageBox.Show(Status.Split('|')[0]+ " chưa được list xong, bạn muốn list sản phẩm khác?", "Cảnh Báo!", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (res == DialogResult.OK)
                    {
                        Status = SanPham.idSanPham + "||";
                        Form1.Log(Name_Profile, Status);
                        Form1.updateStatus(idDgv,"", SanPham.idSanPham, "");
                    }
                    if (res == DialogResult.Cancel)
                    {
                        Form1.enabledView();
                        return;
                    }
                }
                IWebDriver driver = createDriver(this.filePathProfile);
                try
                {
                    Form1.updateStatus(idDgv, "", SanPham.idSanPham, "");
                    this.SanPham.listting(driver, Status, Name_Profile, idDgv, LogSPDaList);
                }
                catch (Exception )
                {
                    Status = Form1.getLog(this.Name_Profile);
                    this.SanPham.listting(driver, Status, Name_Profile, idDgv, LogSPDaList);
                }                
                Form1.Log(Name_Profile, "||");
                Form1.updateStatus(idDgv, "", "Ready", "Ready");
                driver.Quit();
                driver.Dispose();
            };
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
