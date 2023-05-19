/*
okarpov: oleksandr karpov, 2015
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemoteToLocalSQLBackup
{
    public partial class Form1 : Form
    {
        string localPath = string.Empty;
        bool useSameDBASTemp = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            btnTransfer.Enabled = false;
            localPath = tbLocalPath.Text;
            useSameDBASTemp = cbUseSameDBASTemp.Checked;
            backgroundWorker1.RunWorkerAsync(tbConn.Text);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            SqlBackup sqlbak = new SqlBackup(e.Argument as string, this.localPath, useSameDBASTemp);
            try
            {
                worker.ReportProgress(0, "Get DB Size...");
                sqlbak.GetDBSize();

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
                    sqlbak.DownloadBakFile(i + 1);

                    progress += progressStep;

                    worker.ReportProgress((int)progress, "Delete BAK File " + (i + 1).ToString() + " from " + fnum + "...");
                    sqlbak.DeleteBakFile();
                }

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
            btnTransfer.Enabled = true;
        }

        private void tbLocalPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                localPath = fbd.SelectedPath;
                tbLocalPath.Text = localPath;
            }
        }
    }
}
