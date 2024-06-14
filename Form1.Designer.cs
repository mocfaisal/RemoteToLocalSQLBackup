namespace RemoteToLocalSQLBackup
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnTransfer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbLocalPath = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label2 = new System.Windows.Forms.Label();
            this.tbConn = new System.Windows.Forms.TextBox();
            this.cbUseSameDBASTemp = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel_1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.rbBuild = new System.Windows.Forms.RadioButton();
            this.rbString = new System.Windows.Forms.RadioButton();
            this.panel_2 = new System.Windows.Forms.Panel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblDBSize = new System.Windows.Forms.Label();
            this.txtPlainConn = new System.Windows.Forms.TextBox();
            this.btnGen = new System.Windows.Forms.Button();
            this.txtRemotePath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.groupBox1.SuspendLayout();
            this.panel_1.SuspendLayout();
            this.panel_2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnTransfer
            // 
            this.btnTransfer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTransfer.Enabled = false;
            this.btnTransfer.Location = new System.Drawing.Point(303, 507);
            this.btnTransfer.Name = "btnTransfer";
            this.btnTransfer.Size = new System.Drawing.Size(97, 23);
            this.btnTransfer.TabIndex = 2;
            this.btnTransfer.Text = "Transfer to local";
            this.btnTransfer.UseVisualStyleBackColor = true;
            this.btnTransfer.Click += new System.EventHandler(this.btnTransfer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Local Path:";
            // 
            // tbLocalPath
            // 
            this.tbLocalPath.Location = new System.Drawing.Point(12, 84);
            this.tbLocalPath.Name = "tbLocalPath";
            this.tbLocalPath.Size = new System.Drawing.Size(356, 20);
            this.tbLocalPath.TabIndex = 0;
            this.tbLocalPath.Text = "D:\\Work\\PMI\\PMI DKI\\Backup";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 543);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(388, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(12, 527);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(41, 13);
            this.lblProgress.TabIndex = 4;
            this.lblProgress.Text = "State...";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Connection String:";
            // 
            // tbConn
            // 
            this.tbConn.Location = new System.Drawing.Point(18, 35);
            this.tbConn.Multiline = true;
            this.tbConn.Name = "tbConn";
            this.tbConn.Size = new System.Drawing.Size(330, 53);
            this.tbConn.TabIndex = 6;
            this.tbConn.Text = "Server=JSD00001\\sqlexpress;Initial Catalog=Jambala;User ID=sa;Password=1;Multiple" +
    "ActiveResultSets=True;Timeout=1200";
            // 
            // cbUseSameDBASTemp
            // 
            this.cbUseSameDBASTemp.AutoSize = true;
            this.cbUseSameDBASTemp.Location = new System.Drawing.Point(12, 358);
            this.cbUseSameDBASTemp.Name = "cbUseSameDBASTemp";
            this.cbUseSameDBASTemp.Size = new System.Drawing.Size(178, 17);
            this.cbUseSameDBASTemp.TabIndex = 7;
            this.cbUseSameDBASTemp.Text = "Use same DB as temp database";
            this.cbUseSameDBASTemp.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel_1);
            this.groupBox1.Controls.Add(this.rbBuild);
            this.groupBox1.Controls.Add(this.rbString);
            this.groupBox1.Controls.Add(this.panel_2);
            this.groupBox1.Location = new System.Drawing.Point(12, 116);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(378, 236);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection Settings";
            // 
            // panel_1
            // 
            this.panel_1.Controls.Add(this.label4);
            this.panel_1.Controls.Add(this.label3);
            this.panel_1.Controls.Add(this.txtPass);
            this.panel_1.Controls.Add(this.txtHost);
            this.panel_1.Controls.Add(this.label5);
            this.panel_1.Controls.Add(this.txtDatabase);
            this.panel_1.Controls.Add(this.txtUsername);
            this.panel_1.Controls.Add(this.lblDatabase);
            this.panel_1.Location = new System.Drawing.Point(8, 55);
            this.panel_1.Name = "panel_1";
            this.panel_1.Size = new System.Drawing.Size(362, 170);
            this.panel_1.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Server";
            // 
            // txtPass
            // 
            this.txtPass.Location = new System.Drawing.Point(26, 103);
            this.txtPass.Name = "txtPass";
            this.txtPass.Size = new System.Drawing.Size(312, 20);
            this.txtPass.TabIndex = 2;
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(26, 25);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(312, 20);
            this.txtHost.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Username";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(25, 142);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(312, 20);
            this.txtDatabase.TabIndex = 3;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(26, 64);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(312, 20);
            this.txtUsername.TabIndex = 1;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(25, 126);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(53, 13);
            this.lblDatabase.TabIndex = 4;
            this.lblDatabase.Text = "Database";
            // 
            // rbBuild
            // 
            this.rbBuild.AutoSize = true;
            this.rbBuild.Checked = true;
            this.rbBuild.Location = new System.Drawing.Point(130, 19);
            this.rbBuild.Name = "rbBuild";
            this.rbBuild.Size = new System.Drawing.Size(48, 17);
            this.rbBuild.TabIndex = 0;
            this.rbBuild.TabStop = true;
            this.rbBuild.Text = "Build";
            this.rbBuild.UseVisualStyleBackColor = true;
            this.rbBuild.CheckedChanged += new System.EventHandler(this.rbBuild_CheckedChanged);
            // 
            // rbString
            // 
            this.rbString.AutoSize = true;
            this.rbString.Location = new System.Drawing.Point(184, 19);
            this.rbString.Name = "rbString";
            this.rbString.Size = new System.Drawing.Size(52, 17);
            this.rbString.TabIndex = 1;
            this.rbString.Text = "String";
            this.rbString.UseVisualStyleBackColor = true;
            this.rbString.CheckedChanged += new System.EventHandler(this.rbString_CheckedChanged);
            // 
            // panel_2
            // 
            this.panel_2.Controls.Add(this.label2);
            this.panel_2.Controls.Add(this.tbConn);
            this.panel_2.Enabled = false;
            this.panel_2.Location = new System.Drawing.Point(8, 42);
            this.panel_2.Name = "panel_2";
            this.panel_2.Size = new System.Drawing.Size(362, 106);
            this.panel_2.TabIndex = 9;
            this.panel_2.Visible = false;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(374, 82);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(27, 22);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblDBSize
            // 
            this.lblDBSize.AutoSize = true;
            this.lblDBSize.Location = new System.Drawing.Point(9, 512);
            this.lblDBSize.Name = "lblDBSize";
            this.lblDBSize.Size = new System.Drawing.Size(60, 13);
            this.lblDBSize.TabIndex = 9;
            this.lblDBSize.Text = "DB Size : 0";
            // 
            // txtPlainConn
            // 
            this.txtPlainConn.Location = new System.Drawing.Point(199, 359);
            this.txtPlainConn.Name = "txtPlainConn";
            this.txtPlainConn.Size = new System.Drawing.Size(142, 20);
            this.txtPlainConn.TabIndex = 10;
            // 
            // btnGen
            // 
            this.btnGen.Location = new System.Drawing.Point(160, 385);
            this.btnGen.Name = "btnGen";
            this.btnGen.Size = new System.Drawing.Size(142, 26);
            this.btnGen.TabIndex = 11;
            this.btnGen.Text = "Tranfer to Local Direct";
            this.btnGen.UseVisualStyleBackColor = true;
            this.btnGen.Click += new System.EventHandler(this.btnGen_Click);
            // 
            // txtRemotePath
            // 
            this.txtRemotePath.Location = new System.Drawing.Point(12, 25);
            this.txtRemotePath.Name = "txtRemotePath";
            this.txtRemotePath.Size = new System.Drawing.Size(356, 20);
            this.txtRemotePath.TabIndex = 12;
            this.txtRemotePath.Text = "C:\\Program Files\\Microsoft SQL Server\\MSSQL10_50.MSSQLSERVER\\MSSQL\\Backup";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Remote Path:";
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(7, 423);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(394, 78);
            this.txtLog.TabIndex = 14;
            this.txtLog.Text = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 407);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Log :";
            // 
            // backgroundWorker2
            // 
            this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 577);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.txtRemotePath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnGen);
            this.Controls.Add(this.txtPlainConn);
            this.Controls.Add(this.lblDBSize);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.cbUseSameDBASTemp);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.tbLocalPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnTransfer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RemoteToLocal SQL Backup";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel_1.ResumeLayout(false);
            this.panel_1.PerformLayout();
            this.panel_2.ResumeLayout(false);
            this.panel_2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTransfer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbLocalPath;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgress;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbConn;
        private System.Windows.Forms.CheckBox cbUseSameDBASTemp;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbBuild;
        private System.Windows.Forms.RadioButton rbString;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Panel panel_2;
        private System.Windows.Forms.Panel panel_1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblDBSize;
        private System.Windows.Forms.TextBox txtPlainConn;
        private System.Windows.Forms.Button btnGen;
        private System.Windows.Forms.TextBox txtRemotePath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Label label7;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
    }
}

