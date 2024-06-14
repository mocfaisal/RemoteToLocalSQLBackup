/*
okarpov: oleksandr karpov, 2015

Modified by : Mochammad Faisal , 2024-06-14
NOTE : Error cannot restore by splitting files
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using IniParser;
using IniParser.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace RemoteToLocalSQLBackup
{
    public partial class Form1 : Form
    {
        string remotePath = string.Empty;
        string localPath = string.Empty;
        string iniFilename = "config";
        bool useSameDBASTemp = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            string connString = genConnString();

            btnTransfer.Enabled = false;
            localPath = tbLocalPath.Text;
            useSameDBASTemp = cbUseSameDBASTemp.Checked;
            backgroundWorker1.RunWorkerAsync(connString);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string logDownFile = string.Empty;

            SqlBackup sqlbak = new SqlBackup(e.Argument as string, tbLocalPath.Text, useSameDBASTemp);
            try
            {
                worker.ReportProgress(0, "Get DB Size...");
                long dbsize = sqlbak.GetDBSize();

                MethodInvoker inv = delegate
                {
                    this.lblDBSize.Text = "DB Size : " + dbsize.ToString("N0") + " MB";
                };

                this.Invoke(inv);

                worker.ReportProgress(5, "Prepare BAK Files...");
                int fnum = sqlbak.CreateBakupFiles();

                worker.ReportProgress(10, "Create TempDB...");
                sqlbak.CreateTempDB();

                double progress = 10;
                double progressStep = 85 / fnum;

                for (int i = 0; i < fnum; i++)
                {
                    worker.ReportProgress((int)progress, "Access BAK File " + (i + 1).ToString() + " from " + fnum + "...");
                    sqlbak.InsertBakFile(i + 1);

                    worker.ReportProgress((int)progress, "Download BAK File " + (i + 1).ToString() + " from " + fnum + "...");
                    logDownFile = sqlbak.DownloadBakFile(i + 1);

                    MethodInvoker inv2 = delegate
                    {
                        txtLog.Text += logDownFile + Environment.NewLine;
                    };
                    this.Invoke(inv2);

                    progress += progressStep;

                    worker.ReportProgress((int)progress, "Delete BAK File " + (i + 1).ToString() + " from " + fnum + "...");
                    sqlbak.DeleteBakFile();
                }

                worker.ReportProgress(85, "Combining Bak Files DB...");
                sqlbak.CombineDownloadedBakFile(fnum);

                worker.ReportProgress(95, "Delete temp DB...");
                sqlbak.DeleteTempDB();

                sqlbak.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            worker.ReportProgress(100);
        }

        //public void fileDownloaded(object param)
        //{
        //    string fileName = ((dynamic)param).fname;
        //    int i = ((dynamic)param).i;

        //    backgroundWorker1.ReportProgress(i, "Downloaded BAK File " + fileName);
        //}

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblProgress.Text = "Status: " + e.UserState as string;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblProgress.Text = "Status: Completed";
            btnTransfer.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkIniConfig();
        }
        private string genConnString()
        {
            string connString = "";

            if (rbString.Checked)
            {
                connString = tbConn.Text;
            }
            else if (rbBuild.Checked)
            {
                string hostName = txtHost.Text;
                string databaseName = txtDatabase.Text;
                string username = txtUsername.Text;
                string password = txtPass.Text;

                connString = "Server=" + hostName + ";Initial Catalog=" + databaseName + ";User ID=" + username + ";Password=" + password + ";MultipleActiveResultSets=True;Timeout=1200";
            }

            this.remotePath = txtRemotePath.Text;

            writeIniConfig();

            return connString;
        }
        private void connGroupSwitch(int typeRB)
        {
            if (typeRB == 0)
            {
                // Radio Button Build
                panel_1.Enabled = true;
                panel_1.Visible = true;

                panel_2.Enabled = false;
                panel_2.Visible = false;
            }
            else if (typeRB == 1)
            {
                // Radio Button String
                panel_1.Enabled = false;
                panel_1.Visible = false;

                panel_2.Enabled = true;
                panel_2.Visible = true;
            }
        }

        private void rbBuild_CheckedChanged(object sender, EventArgs e)
        {
            connGroupSwitch(0);
        }

        private void rbString_CheckedChanged(object sender, EventArgs e)
        {
            connGroupSwitch(1);
        }

        private void checkIniConfig()
        {
            var parser = new FileIniDataParser();
            if (File.Exists(iniFilename + ".ini"))
            {
                IniData data = parser.ReadFile(iniFilename + ".ini");

                if (File.Exists(iniFilename + ".ini"))
                {
                    readIniConfig(data);
                }

            }
            else
            {
                writeIniConfig();
            }
        }

        private void readIniConfig(IniData data)
        {
            string hostName = data["connection"]["hostname"] ?? "localhost";
            string dbName = data["connection"]["database"] ?? "master";
            string username = data["connection"]["username"] ?? "sa";
            string password = data["connection"]["password"] ?? "";

            txtHost.Text = hostName;
            txtDatabase.Text = dbName;
            txtUsername.Text = username;
            txtPass.Text = password;

        }
        private void writeIniConfig()
        {
            var parser = new FileIniDataParser();
            IniData data = new IniData();

            string hostName = txtHost.Text;
            string databaseName = txtDatabase.Text;
            string username = txtUsername.Text;
            string password = txtPass.Text;

            data["connection"]["hostname"] = hostName;
            data["connection"]["database"] = databaseName;
            data["connection"]["username"] = username;
            data["connection"]["password"] = password;

            parser.WriteFile(iniFilename + ".ini", data);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                localPath = fbd.SelectedPath;
                tbLocalPath.Text = localPath;
            }
        }

        private void btnGen_Click(object sender, EventArgs e)
        {
            txtPlainConn.Text = genConnString();

            SqlBackup sqlbak = new SqlBackup(txtPlainConn.Text, txtRemotePath.Text, tbLocalPath.Text, useSameDBASTemp);
            try
            {
                backgroundWorker2.RunWorkerAsync(sqlbak);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            SqlBackup sqlbak = e.Argument as SqlBackup;

            try
            {
                sqlbak.DownloadBackupFileByName(txtDatabase.Text);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show($"Error occurred: {e.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Backup file downloaded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
