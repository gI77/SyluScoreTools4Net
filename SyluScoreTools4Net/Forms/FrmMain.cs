﻿using SyluScoreTools4Net.Class;
using SyluScroeTools4Net.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SyluScoreTools4Net.Forms
{
    public partial class FrmMain : Form
    {
        private FrmAcc FrmAcc = new FrmAcc();
        private WebPost WebPost = new WebPost();
        private string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ScroeTools\";
        private string FilePath;
        private JavaScriptSerializer Json = new JavaScriptSerializer();

        private UserInfo NowUser = new UserInfo();
        public FrmMain()
        {
            InitializeComponent();
            if (!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
            FilePath = DirPath + "data.dat";
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            labVIPScore.Alignment = ToolStripItemAlignment.Right;
            if (File.Exists(FilePath))
            {
                NowUser = Json.Deserialize<UserInfo>(File.ReadAllText(FilePath));
                dgvData.DataSource = NowUser.ScoreList;
                ShowVIPClass();
            }
        }

        async private void btnAddAcc_Click(object sender, EventArgs e)
        {
            FrmAcc.DialogResult = DialogResult.No;
            if (FrmAcc.ShowDialog() == DialogResult.OK)
            {
                await Task.Run(() =>
                {
                    ChangeStatusLabel("正在获取成绩中...");
                    GetScoreInfo();
                    ChangeStatusLabel("成绩获取成功!");
                });
            }
        }

        private void ChangeDgvDataSource(object data)
        {
            if (this.dgvData.InvokeRequired)
            {
                this.Invoke(new ChangeControlData(ChangeDgvDataSource), data);
            }
            else
            {
                this.dgvData.DataSource = data;
            }
        }

        private void ChangeStatusLabel(object data)
        {
            if (this.statusStrip.InvokeRequired)
            {
                this.Invoke(new ChangeControlData(ChangeStatusLabel), data);
            }
            else
            {
                tslFrmStatus.Text = data.ToString();
            }
        }

        private void ChangeRowColor(object data)
        {
            if (this.dgvData.InvokeRequired)
            {
                this.Invoke(new ChangeControlData(ChangeRowColor), data);
            }
            else
            {
                var i = (int)data;
                this.dgvData.Rows[i].DefaultCellStyle.BackColor = Color.Red;
            }
        }

        async private void btnCalcVIP_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                ChangeStatusLabel("正在获取学位课列表中...");
                ShowVIPClass();
                ChangeStatusLabel("学位课列表获取成功!");
            });
        }
        private void SaveData()
        {
            using (var file = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(file))
                {
                    sw.WriteLine(Json.Serialize(NowUser));
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        private void ShowVIPClass()
        {
            var webPost = new WebPost();
            webPost.UserName = FrmAcc.UserName;
            webPost.PassWord = FrmAcc.Password;
            NowUser.VIPClassList = webPost.GetAllVIP();
            for (int i = 0; i < NowUser.ScoreList.Count; i++)
            {
                var classInfo = NowUser.ScoreList[i];
                foreach (var vipInfo in NowUser.VIPClassList)
                {
                    if (classInfo.ClassID == vipInfo.ClassID)
                    {
                        ChangeRowColor(i);
                        //this.dgvData.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        break;
                    }
                }
            }
            SaveData();
        }
        private void GetScoreInfo()
        {
            NowUser.UserName = FrmAcc.UserName;
            NowUser.Password = FrmAcc.Password;
            WebPost.UserName = FrmAcc.UserName;
            WebPost.PassWord = FrmAcc.Password;
            NowUser.ScoreList = WebPost.GetAllScore();
            ChangeDgvDataSource(NowUser.ScoreList);
            SaveData();
        }
    }
}