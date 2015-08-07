using DotNet_Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace InnerBuyAlarm
{
    public partial class Form1 : Form
    {
        string sSettingFile = "setting.xml";
        FormWindowState lastFormState = FormWindowState.Normal;
        SettingModel setting;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Tick += timer1_Tick;
            webBrowser1.WBDocHostShowUIShowMessage += webBrowser1_WBDocHostShowUIShowMessage;

            if (File.Exists(sSettingFile))
            {
                //var sSearch = GetSettings();
                //txtSearch.Text = sSearch[1];
                //txtInterval.Text = sSearch[0];
                setting = SerializeHelper.Load<SettingModel>( sSettingFile);
                txtInterval.Text = setting.Interval;
                txtSearch.Text = setting.SearchText;
                checkBuy.Checked = setting.AutoBuy;
                txtProducts.Lines = setting.Products;
                txtName.Text = setting.BuyerName;
                txtAmount.Text = setting.BuyAmount;
                txtUrl.Text = setting.Url;
            }
        }

        bool backgroundStopDialog = false;
        void webBrowser1_WBDocHostShowUIShowMessage(object sender, ExtendedBrowserMessageEventArgs e)
        {
            if (backgroundStopDialog)
            {
                e.Cancel = true;
            }
        }



        protected override void OnResize(EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                lastFormState = WindowState;
            }
            base.OnResize(e);
        }

     

        private string[] GetSettings()
        {
            var sSearch = File.ReadAllLines(sSettingFile);
            return sSearch;
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            //
            webBrowser1.Navigate(txtUrl.Text);
            //webBrowser1.Refresh();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "开始")
            {
                isClicked = true;
                backgroundStopDialog = true;
                productBuyed = 0;
                btnStart.Text = "结束";
                SaveSettingFile();
                //if (!File.Exists(sSettingFile))
                //{
                //    SaveSettingFile();
                //}
                //else
                //{
                //    var sSearch = GetSettings();
                //    if (sSearch[1] != txtSearch.Text || sSearch[0] != txtInterval.Text)
                //    {
                //        SaveSettingFile();
                //    }
                //}
                webBrowser1.Url = new Uri(txtUrl.Text);
                timer1.Interval = int.Parse(txtInterval.Text) * 1000;
                timer1.Start();
            }
            else
            {
                isClicked = false;
                backgroundStopDialog = false;
                btnStart.Text = "开始";
                timer1.Stop();
            }
        }

        private void SaveSettingFile()
        {
            //string[] setting = new string[2];
            //setting[1] = txtSearch.Text;
            //setting[0] = txtInterval.Text;
            //File.WriteAllLines(sSettingFile, setting);
            if (setting == null)
                setting = new SettingModel();
            setting.Interval = txtInterval.Text;
            setting.SearchText = txtSearch.Text;
            setting.AutoBuy = checkBuy.Checked;
            setting.Products = txtProducts.Lines;
            setting.BuyerName = txtName.Text;
            setting.BuyAmount = txtAmount.Text;
            setting.Url = txtUrl.Text;
            setting.BuyLimit =Convert.ToInt32( txtLimit.Text);
            SerializeHelper.Save(setting, sSettingFile);
        }

        bool isClicked = false;
        int productIndex = 0;
        int productBuyed = 0;
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
          
            //isClicked = false ;
            var pageCorrect = webBrowser1.DocumentText.Contains(txtSearch.Text);
            label4.Text = pageCorrect.ToString();
            //this.TopMost = true;
            if (pageCorrect)
            {
                btnStart.Text = "开始";
                timer1.Stop();
                this.WindowState = lastFormState;//最小化

                //var txtAmount = webBrowser1.Document.GetElementById("ctl00_MainContent_txtAmount");
                //txtAmount.Click += txtAmount_Click;
                ////txtAmount.AttachEventHandler("click",
                //txtAmount.Style = "color:red";
                RegistPageEvent();

                SettingRowsStyle();
                if (!isClicked)
                    return;
                if (checkBuy.Checked && txtProducts.Text.Trim() != string.Empty)
                {
                    txtProducts.Text=txtProducts.Text.Trim();
                    var products = txtProducts.Lines;
                    var slt = webBrowser1.Document.GetElementById("ctl00_MainContent_ddlCommodity");
                    var amount = webBrowser1.Document.GetElementById("ctl00_MainContent_txtAmount");
                    var name = webBrowser1.Document.GetElementById("ctl00_MainContent_txtUserName");
                    var btnBuy = webBrowser1.Document.GetElementById("ctl00_MainContent_btnBuy");
                    //amount.InnerText = txtAmount.Text ;
                    amount.SetAttribute("value", txtAmount.Text);
                    //name.InnerText =txtName.Text;
                    name.SetAttribute("value", txtName.Text);
                    var options = slt.GetElementsByTagName("option");
                    while ( productIndex < products.Length&&productBuyed<setting.BuyLimit )
                    {
                        var product = products[productIndex];
                        productIndex++;
                        var option = options.OfType<HtmlElement>().FirstOrDefault(p => p.InnerText.StartsWith(product));
                        if (option != null)
                        {
                            option.SetAttribute("selected", "selected");
                            //btnBuy.RaiseEvent("onclick");
                            btnBuy.InvokeMember("click");
                            //Thread.Sleep(200);
                            productBuyed ++;
                            break;
                        }
                    }
                    if (productIndex == products.Length||productBuyed==setting.BuyLimit)
                    {
                        productIndex = 0;
                        isClicked = false;
                        backgroundStopDialog = false;
                    }
                }
            }
        }

        private void RegistPageEvent()
        {
            //翻页事件
            var tablePage = webBrowser1.Document.GetElementById("ctl00_MainContent_gvFruit_DXPagerBottom");
            var tdPages = tablePage.GetElementsByTagName("td");
            string[] cssPages = new string[] { "dxpButton", "dxpPageNumber" };
            for (var i = 0; i < tdPages.Count; i++)
            {
                var css = tdPages[i].GetAttribute("className");
                if (cssPages.Contains(css))
                {
                    tdPages[i].Click += tdPage_Click;
                }
            }
        }

        private void SettingRowsStyle()
        {
            var table = webBrowser1.Document.GetElementById("ctl00_MainContent_gvFruit_DXMainTable");
            var trs = table.GetElementsByTagName("tr");
            //var styleSimple = trs[0].GetElementsByTagName("td")[0].Style;
            for (var i = 8; i < trs.Count; i++)
            {
                var tr = trs[i];
                var tds = tr.GetElementsByTagName("td");
                if (tds[2].InnerText == tds[3].InnerText)
                {
                    tr.Style = "color:red";
                }
            }
        }

        private void tdPage_Click(object sender, HtmlElementEventArgs e)
        {
            timerWaitAjax.Start();
        }

        void txtAmount_Click(object sender, HtmlElementEventArgs e)
        {
            MessageBox.Show("");
        }

        private void timerWaitAjax_Tick(object sender, EventArgs e)
        {
            timerWaitAjax.Stop();
            RegistPageEvent();
            SettingRowsStyle();
        }




    }
}
