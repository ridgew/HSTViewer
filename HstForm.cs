using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace HSTViewer
{
    public partial class HstForm : Form
    {
        public HstForm()
        {
            InitializeComponent();
        }

        private void HstForm_Load(object sender, EventArgs e)
        {
            Instance = this;
            if (!string.IsNullOrEmpty(InitialFilePath) && File.Exists(InitialFilePath))
            {
                ViewHstFile(InitialFilePath);
                BindCurrentFileInfo(InitialFilePath);
            }
            Quokka.UI.WebBrowsers.PluggableProtocol.Register(KLine.UrlStreamProvider.Instance, Assembly.GetExecutingAssembly());
        }

        internal string InitialFilePath = string.Empty;

        static HstForm _instance;
        static HstForm Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        internal static void ShowEror(Form target, string msgFormat, params object[] fmtArgs)
        {
            MessageBox.Show(target, string.Format(msgFormat, fmtArgs),
                            target.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static void ShowEror(string msgFormat, params object[] fmtArgs)
        {
            Form target = Instance;
            MessageBox.Show(target, string.Format(msgFormat, fmtArgs),
                            target.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void FileDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void hstGrid_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string str in data)
                {
                    ViewHstFile(str);
                    break;
                }
            }
        }

        void ViewHstFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length > int.MaxValue)
                {
                    ShowEror("文件大小{0}超过可以显示的最大长度[{1}]!", fs.Length, int.MaxValue);
                    return;
                }

                #region 绑定数据
                BindDataWithFileStream(fs);
                #endregion
            }
        }

        List<RateInfo> FileRateInfoList = null;

        void BindDataWithFileStream(FileStream fs)
        {
            byte[] bufBytes = new byte[64];

            //version int(4) = version
            int iIntVer = fs.ReadInt(ref bufBytes);
            lblVersion.Text = iIntVer.ToString();

            //copyright string(64) = 版权信息
            string strVal = fs.ReadString(ref bufBytes, Convert.ToInt32(tbxFHCopyright.Text.Trim()));
            tbxCopyright.Text = strVal;

            // symbol 货币对名称，如"EURUSD"
            strVal = fs.ReadString(ref bufBytes, Convert.ToInt32(tbxFHSymbolSize.Text.Trim()));
            lblSymbol.Text = strVal;
            KLine.UrlStreamProvider.Instance.CandlestickSize = Convert.ToInt32(tbxCandlestickSize.Text.Trim());
            KLine.UrlStreamProvider.Instance.ResourceName = strVal;

            // period 数据周期：15代表 M15周期
            iIntVer = fs.ReadInt(ref bufBytes);
            lblPeriod.Text = iIntVer.FormatPeriod();
            KLine.UrlStreamProvider.Instance.RatePeriod = lblPeriod.Text.Trim();

            // digits 数据格式：小数点位数     //例如5，代表有效值至小数点5位，1.
            int iDigitNum = fs.ReadInt(ref bufBytes);
            lblDigits.Text = iDigitNum.ToString();

            DataGridViewCellStyle dgvCellStyleCurrency = new DataGridViewCellStyle();
            dgvCellStyleCurrency.Format = string.Format("N{0}", iDigitNum);
            //hstGrid.DefaultCellStyle = dgvCellStyleCurrency;
            hstGrid.Columns["Open"].DefaultCellStyle = dgvCellStyleCurrency;
            hstGrid.Columns["High"].DefaultCellStyle = dgvCellStyleCurrency;
            hstGrid.Columns["Low"].DefaultCellStyle = dgvCellStyleCurrency;
            hstGrid.Columns["Close"].DefaultCellStyle = dgvCellStyleCurrency;
            //hstGrid.Columns[7].DefaultCellStyle = dgvCellStyleCurrency;

            // time_t time sign 文件的创建时间
            // 1500959476 = 2017/7/25 13:11:16
            DateTime createTime = fs.ReadTime(ref bufBytes);
            strVal = createTime.ToString("yyyy-MM-dd HH:mm:ss");
            lblCreateTime.Text = strVal;

            //略过备用字节大小
            fs.Position += Convert.ToInt32(tbxFHUnuseSize.Text.Trim());

            List<RateInfo> ratesCol = new List<RateInfo>();

            ReadRateInfo:
            RateInfo rate = new RateInfo(iDigitNum)
            {
                Index = fs.Position,
                UnusedEmptySize = Convert.ToInt32(tbxRateUnuseSize.Text.Trim())
            };
            rate.CTM = fs.ReadTime(ref bufBytes);
            rate.Open = fs.ReadDouble(ref bufBytes);
            rate.High = fs.ReadDouble(ref bufBytes);
            rate.Low = fs.ReadDouble(ref bufBytes);
            rate.Close = fs.ReadDouble(ref bufBytes);
            rate.Volume = fs.ReadLong(ref bufBytes);
            ratesCol.Add(rate);
            if (fs.Position + rate.UnusedEmptySize < fs.Length - 1)
            {
                fs.Position += rate.UnusedEmptySize;
                goto ReadRateInfo;
            }

            SortableBindingList<RateInfo> rataList = new SortableBindingList<RateInfo>(ratesCol);
            hstGrid.DataSource = rataList;
            List<RateInfo> rateInfoList = ratesCol;
            KLine.UrlStreamProvider.Instance.KRateInfo = ratesCol;
            if (rateInfoList != null && rateInfoList.Any())
            {
                dtPickBegin.Value = rateInfoList.First().CTM;
                dtPickEnd.Value = rateInfoList.Last().CTM;
                btnApplyFilter.Enabled = true;
            }
            FileRateInfoList = rateInfoList;
        }

        private void hstGrid_DragEnter(object sender, DragEventArgs e)
        {
            FileDragEnter(sender, e);
        }

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            if (FileRateInfoList != null && FileRateInfoList.Any())
            {
                List<RateInfo> nInfosBinds = FileRateInfoList.Where(m =>
                        m.CTM >= dtPickBegin.Value.Date
                        && m.CTM < dtPickEnd.Value.AddDays(1.0).Date).ToList();
                SortableBindingList<RateInfo> rataList = new SortableBindingList<RateInfo>(nInfosBinds);
                hstGrid.DataSource = rataList;
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            DialogResult dResult = rateFileOpenDlg.ShowDialog();
            if (dResult == DialogResult.OK)
            {
                BindCurrentFileInfo(rateFileOpenDlg.FileName);
            }
        }

        void BindCurrentFileInfo(string strRateFilePath)
        {
            tbxRateFileFullPath.Text = strRateFilePath;
            FileInfo rateFile = new FileInfo(tbxRateFileFullPath.Text);
            lblFileTotalSize.Text = rateFile.Length.FormatSize();
        }

        private void btnReadRateFile_Click(object sender, EventArgs e)
        {
            string filePath = tbxRateFileFullPath.Text;
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                ViewHstFile(filePath);
                tabContainer.SelectedTab = tabPageDat;
                KLineBrowser.Navigate("hstv://host/index.html");
            }
        }

        private void tabContainer_TabIndexChanged(object sender, EventArgs e)
        {
            if (tabContainer.SelectedTab == tabPageKLine)
            {
                
            }
        }

        private void KLineBrowser_SizeChanged(object sender, EventArgs e)
        {
            KLineBrowser.Navigate("hstv://host/index.html");
        }
    }
}
