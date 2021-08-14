using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using ClosedXML.Excel;
using System.ComponentModel;
using ToolList_Ver1.Class;
using System.Configuration;

namespace ToolList_Ver1
{
    public partial class Form1 : Form
    {
        private static Form1 form = null;
        public Form1()
        {
            InitializeComponent();
            form = this;
            getDataWTM();
            getProfile();
        }        
        //Giá listing
        public static string price;
        //Quantity listing
        public static string quantity;
        public static bool mucDich=true; 
        public static bool dangList=false;
        //List Shop có trong máy
        private BindingList<Shop> lstShop = new BindingList<Shop>();
        //List Sản phẩm được chọn 
        List<sanPham> ListSanPham = new List<sanPham>();
        //List data của từng WTM trong từng loại sản phẩm
        System.Collections.SortedList listDataWTM = new System.Collections.SortedList();
        BindingSource DataWTMgSource1 = new BindingSource();
        List<image> lstImageRun = new List<image>();
        private int maxThread = 1;
        //Lấy profile trong máy
        private void getProfile()
        {
            try
            {
                foreach (string d in Directory.GetDirectories(Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles"))
                {
                    string a = Path.GetFileName(d).Split('.')[1];
                    Shop temp = new Shop(d, a);
                    if (getLog(a) != null)
                    {
                        temp.Status = getLog(a);
                        lstShop.Add(temp);
                        dtgvData.Rows.Add(temp.Name_Profile, "", temp.Status);
                    }
                    else
                    {
                        Log(a,"Ready");
                        temp.Status = "Ready";
                        lstShop.Add(temp);
                        dtgvData.Rows.Add(temp.Name_Profile, "", temp.Status);
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
            string DescapSVG = File.ReadAllText(strFileName+@"\SVG.txt");
            string DescapPNG = File.ReadAllText(strFileName + @"\PNG.txt");
            try
            {
                XLWorkbook workbook = new XLWorkbook(pathXlm);
                var rows = workbook.Worksheet(1).RowsUsed();
                foreach (var row in rows)
                {
                    List<string> temp = new List<string>();
                    DataWTM t;
                    foreach (IXLCell item in row.Cells())
                    {
                        temp.Add(item.Value.ToString());
                    }                                       
                    if(temp[0].ToUpper().Contains("PNG"))
                        t = new DataWTM(temp[0], temp[1], DescapPNG, temp[2]);
                    else
                        t= new DataWTM(temp[0], temp[1], DescapSVG, temp[2]);
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
                    //đếm file trong thư mục
                    //temp.wtmCount= Directory.GetDirectories(d, "*.*", SearchOption.AllDirectories).Length;
                    ListSanPham.Add(temp);
                }
            }
            catch (Exception)
            {
            }
            int i = 0;
            foreach (sanPham item in ListSanPham)
            {
                item.getListWTM(listDataWTM);
                this.dtgvWtm.Rows.Add(item.idSanPham);
                this.dtgvWtm.Rows[i].Cells[1].Value = Directory.GetDirectories(item.filePath).Length;
                i++;
            }
            addSanPhamToCombo();
        }
        //Thêm danh sách sản phẩm vào comobox trên dtgv danh sách shop
        private void addSanPhamToCombo()
        {
            foreach (sanPham item in ListSanPham)
            {
                ((DataGridViewComboBoxColumn)dtgvData.Columns["Column4"]).Items.Add(item);
            }
            ((DataGridViewComboBoxColumn)dtgvData.Columns["Column4"]).DisplayMember = "idSanPham";
            ((DataGridViewComboBoxColumn)dtgvData.Columns["Column4"]).ValueMember = "Temp";
        }
        //Button Starts
        private void btnStart_Click(object sender, EventArgs e)
        {
            this.trackBar1.Enabled = false;
            this.btnStart.Enabled = false;
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
                    Thread sub = new Thread(lstShopRun[autodem].createShop(lstImageRun));
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
            Invoke(new MethodInvoker(delegate ()
            {
                this.btnStart.Enabled = true;
                this.trackBar1.Enabled = true;
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
        public static void Log(string keyLog, string logMessage)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(keyLog);
            config.AppSettings.Settings.Add(keyLog, logMessage);
            config.Save(ConfigurationSaveMode.Minimal);
        }
        private string getLog(string keyLog)
        {
            return (string)ConfigurationManager.AppSettings[keyLog];
        }

        private void rdListDigital_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio.Checked)
                if (radio.Name.Equals("rdListDigital"))
                    dangList = true;
                else
                    dangList = false;
        }
        

        private void rdQuangCao_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio.Checked)
                if (radio.Name.Equals("rdQuangCao"))
                    mucDich = true;
                else
                    mucDich = false;
        }
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
        public static void updateStatus(int id , string h)
        {
            if (form != null)
                form.UpdateDgv(id, "", "", h);
        }

        private void dtgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
