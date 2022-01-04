using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using ClosedXML.Excel;
using System.ComponentModel;
using ToolList_Ver1.Class;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ToolList_Ver1
{
    public partial class Form1 : Form
    {
        private static Form1 form = null;
        //List Shop có trong máy
        private BindingList<Shop> lstShop = new BindingList<Shop>();
        //List Sản phẩm được chọn 
        List<sanPham> ListSanPham = new List<sanPham>();
        //List data của từng WTM trong từng loại sản phẩm
        System.Collections.SortedList listDataWTM = new System.Collections.SortedList();
        BindingSource DataWTMgSource1 = new BindingSource();
        private int maxThread = 1;
        private List<string> LogSPDaList = new List<string>();
        public static int countTag;
        public static int countWTM;
        private DateTime today = DateTime.Today;
        public static string linkWeb = "https://bqsmarket.com/dashboard/new-product/";
        public static bool listWeb = false;
        public static bool listShopyfi = false;

        public Form1()
        {
            InitializeComponent();
            form = this;            
        }       

        //Lấy profile trong máy
        private void getProfile()
        {
            try
            {
                if(!string.IsNullOrEmpty(getLog("LogItemList")))
                    LogSPDaList = getLog("LogItemList").Split(',').ToList();
                foreach (string d in Directory.GetDirectories(Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles"))
                {
                    string a = Path.GetFileName(d).Split('.')[1];
                    Shop temp = new Shop(d, a);
                    if (getLog(a) != null)
                    {
                        temp.Status = getLog(a);
                        lstShop.Add(temp);
                        dtgvData.Rows.Add(temp.Name_Profile,temp.Status.Split('|')[0],temp.Status.Split('|')[2]);
                    }
                    else
                    {
                        Log(a, "||");
                        temp.Status = "||";
                        lstShop.Add(temp);
                        dtgvData.Rows.Add(temp.Name_Profile, "", "");
                    }                      
                }
            }
            catch (Exception)
            {
            }
        }

        //Tự load dataWTM 
        private void getDataWTM()
        {
            string filePath = @".\.\Data"; 
            string date = today.ToString("dd/MM/yyy");            
            if (!date.Equals(getLog("LogRun")))
            {
                Log("LogRun", date);
                logErr("======================" + date + "======================");
            }
            getTitleandTag(filePath);
        }

        private void getTitleandTag(string filePath)
        {            
            GetBindingSourceExcel(filePath);
            if (DataWTMgSource1.Count == 0)
            {
                FolderBrowserDialog open = new FolderBrowserDialog() { Description = "Chọn Folder Thông Tin WTM" };
                if (open.ShowDialog() == DialogResult.OK)
                {
                    GetBindingSourceExcel(open.SelectedPath);
                }
            }
            dtgvDataWTM.DataSource = DataWTMgSource1;
            if (DataWTMgSource1.Count > 0)
            {
                dtgvDataWTM.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                dtgvDataWTM.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }
        }

        //Load File excel ra BindingSource và đối tượng
        public void GetBindingSourceExcel(string strFileName)
        {
            string pathXlm = strFileName + @"\dataWTM.xlsx";
            try
            {
                XLWorkbook workbook = new XLWorkbook(pathXlm);
                var rows = workbook.Worksheet(1).RowsUsed();
                foreach (var row in rows)
                {
                    List<string> temp = new List<string>();
                    DataWTM t = new DataWTM();
                    foreach (IXLCell item in row.Cells())
                    {
                        if(!item.IsEmpty())
                            temp.Add(item.Value.ToString());
                    }
                    if(temp.Count ==7)
                        t = new DataWTM(temp[0], temp[1], temp[2], temp[3], temp[4],temp[5], temp[6]);
                    else
                        t = new DataWTM(temp[0], temp[1], temp[2], temp[3], "", temp[4], temp[5]);
                    if (!listDataWTM.ContainsKey(temp[0]))
                        listDataWTM.Add(temp[0], t);
                    if (!DataWTMgSource1.Contains(t))
                        DataWTMgSource1.Add(t);
                }
            }
            catch (Exception)
            {
            }
        }

        //Button get profile
        private void button3_Click(object sender, EventArgs e)
        {
            dtgvData.DataSource = null;
            dtgvData.Rows.Clear();
            lstShop.Clear();
            getProfile();
        }

        //Button xoá profile
        private void button6_Click(object sender, EventArgs e)
        {
            List<Shop> shopDelete = new List<Shop>();
            foreach (DataGridViewRow row in dtgvData.Rows)
            {
                if (row.Cells[4].Value != null && (bool)row.Cells[4].Value == true)
                {
                    foreach  (Shop item in lstShop)
                    {
                        if (item.Name_Profile.Equals(row.Cells[0].Value))
                        {
                            DirectoryInfo di = new DirectoryInfo(item.getFilePathProfile());
                            di.Delete(true);
                            shopDelete.Add(item);
                        }
                    }
                    dtgvData.Rows.RemoveAt(row.Index);
                }
            }
            foreach (Shop item in shopDelete)
            {
                lstShop.Remove(item);
            }
        }

        //Button get danh sách sản phẩm 
        private void loadWTM_Click(object sender, EventArgs e)
        {
            string filePath = "";            
            ((DataGridViewComboBoxColumn)dtgvData.Columns["Column4"]).Items.Clear();
            ListSanPham.Clear();
            dtgvWtm.Rows.Clear();
            FolderBrowserDialog open = new FolderBrowserDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                this.dtgvWtm.ClearSelection();
                filePath = open.SelectedPath;
            }
            try
            {
                foreach (string d in Directory.GetDirectories(filePath))
                {
                    sanPham temp = new sanPham();
                    temp.filePath = d;
                    temp.idSanPham = Path.GetFileName(d);
                    ListSanPham.Add(temp);
                }
            }
            catch (Exception)
            {
            }            
            addSanPhamToCombo();
        }

        //Thêm danh sách sản phẩm vào comobox trên dtgv danh sách shop
        private void addSanPhamToCombo()
        {
            int i = 0;
            foreach (sanPham item in ListSanPham)
            {
                item.getListWTM(listDataWTM);
                this.dtgvWtm.Rows.Add(item.idSanPham);
                this.dtgvWtm.Rows[i].Cells[1].Value = Directory.GetDirectories(item.filePath).Length;
                ((DataGridViewComboBoxColumn)dtgvData.Columns["Column4"]).Items.Add(item);
                i++;
            }
            ((DataGridViewComboBoxColumn)dtgvData.Columns["Column4"]).DisplayMember = "idSanPham";
            ((DataGridViewComboBoxColumn)dtgvData.Columns["Column4"]).ValueMember = "Temp";
        }

        //Button Starts
        private void btnStart_Click(object sender, EventArgs e)
        {
            disnabledView();
            countTag = (int)nbDowSlTag.Value == 0 ? 1 : (int)nbDowSlTag.Value;
            countWTM = (int)nbDownSlanh.Value == 0 ? 1 : (int)nbDownSlanh.Value;
            backgroundWorker1.RunWorkerAsync();
        }

        //trackBar điều chỉnh số lượng thread
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            maxThread = trackBar1.Value;
            lbMaxThread.Text = "Max Thread: " + maxThread;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Shop> lstShopRun = new List<Shop>();
            List<Thread> lThreads = new List<Thread>();
            int autodem = 0;
            foreach (DataGridViewRow row in dtgvData.Rows)
            {
                if (row.Cells[4].Value != null && (bool)row.Cells[4].Value != false)
                {
                    foreach (Shop item in lstShop)
                    {
                        if (item.Name_Profile.Equals(row.Cells[0].Value))
                        {
                            item.idDgv = row.Index;
                            lstShopRun.Add(item);
                        }
                    }
                }
            }
            for (; autodem < lstShopRun.Count;)
            {
                for (int i = 0; i < maxThread; i++)
                {
                    if (autodem == lstShopRun.Count)
                    {
                        break;
                    }
                    Thread sub = new Thread(lstShopRun[autodem].createShop(LogSPDaList));
                    sub.IsBackground = true;
                    sub.Start();
                    lThreads.Add(sub);
                    autodem++;
                }
                foreach (Thread machineThread in lThreads)
                {
                    machineThread.Join();
                }
                lThreads.Clear();
            }
            Log("LogItemList", "");
            enabledView();
        }

        public static void disnabledView()
        {
            form.Invoke(new MethodInvoker(delegate ()
            {
                form.tableLayoutPanel11.Enabled = false;
            }));
        }

        public static void enabledView()
        {
            form.Invoke(new MethodInvoker(delegate ()
            {
                form.tableLayoutPanel11.Enabled = true;
            }));
        }

        //Chọn sản phẩm list cho từng Shop
        private void dtgvData_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ComboBox combo = e.Control as ComboBox;
            if (combo != null)
            {
                combo.SelectedIndexChanged -= new EventHandler(ComboBox_SelectedIndexChanged);
                combo.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (DataGridViewComboBoxEditingControl)sender;
            lstShop[comboBox.EditingControlRowIndex].setSanPham((sanPham)comboBox.Items[comboBox.SelectedIndex]);
            dtgvData.Update();
        }

        //Ghi log vào config
        public static void Log(string keyLog, string logMessage)
        {
            object syncObj = new object();
            lock (syncObj)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove(keyLog);
                config.AppSettings.Settings.Add(keyLog, logMessage);
                config.Save(ConfigurationSaveMode.Minimal);
            }            
        }

        //Get log từ config
        public static string getLog(string keyLog)
        {
            return (string)ConfigurationManager.AppSettings[keyLog];
        }

        //Cập nhập trạng thái trên dtgv
        private void UpdateDgv(int id, string u, string f, string l)
        {
            base.BeginInvoke(new MethodInvoker(delegate
            {
                bool flag = !string.IsNullOrEmpty(u);
                if (flag)
                {
                    this.dtgvData.Rows[id].Cells[0].Value = u;
                }
                bool flag2 = !string.IsNullOrEmpty(f);
                if (flag2)
                {
                    this.dtgvData.Rows[id].Cells[1].Value = f;
                }
                bool flag3 = !string.IsNullOrEmpty(l);
                if (flag3)
                {
                    this.dtgvData.Rows[id].Cells[2].Value = l;
                }
            }));
        }

        public static void updateStatus(int id , string h, string k, string v)
        {
            if (form != null)
                form.UpdateDgv(id, h, k, v);
        }

        private void dtgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }        

        //btn find data
        private void button5_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dtgvDataWTM.Rows)
            {
                try
                {
                    if (row.Cells[0].Value.Equals(txtFindData.Text))
                    {
                        row.Selected = true;
                        dtgvDataWTM.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
                catch (Exception)
                {
                }

            }
        }

        //ghi file log txt
        public static void logErr(string log)
        {
            object syncObj = new object();
            lock (syncObj)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(log + "\n");
                File.AppendAllText(@".\.\Data\logError.txt", sb.ToString());
                sb.Clear();
            }            
        }

        //Kill geckodriver
        private void killGecko()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("Process.exe");
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "taskkill /F /IM geckodriver.exe";
            process.StartInfo = startInfo;
            process.Start();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            getDataWTM();
            getProfile();
            killGecko();
        }

        private void rdHozomarket_CheckedChanged(object sender, EventArgs e)
        {
            if (rdBqsmarket.Checked)
                linkWeb = "https://bqsmarket.com/dashboard/new-product/";
        }

        private void rdVectorency_CheckedChanged(object sender, EventArgs e)
        {
            if(rdVectorency.Checked)
                linkWeb = "https://vectorency.com/dashboard/new-product/";
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (rdWebkhac.Checked)
            {
                txtLinkWeb.Enabled = true;
                linkWeb = txtLinkWeb.Text;
                listWeb = true;
            }
            else
            {
                txtLinkWeb.Enabled = false;
                listWeb = false;
            }                
        }

        private void txtLinkWeb_TextChanged(object sender, EventArgs e)
        {
            linkWeb = txtLinkWeb.Text;
        }

        private void rdShopify_CheckedChanged(object sender, EventArgs e)
        {
            if (rdShopify.Checked)
            {
                txtLinkShopify.Enabled = true;
                linkWeb = txtLinkShopify.Text;
                listShopyfi = true;
                listWeb = true;
            }
            else
            {
                txtLinkShopify.Enabled = false;
                listShopyfi = false;
                listWeb = false;
            }
        }

        private void txtLinkShopify_TextChanged(object sender, EventArgs e)
        {
            linkWeb = txtLinkShopify.Text;
        }
    }
}
