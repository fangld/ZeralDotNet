namespace ZeraldotNet.UserInterface
{
    partial class mainForm
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
            this.pbPercentage = new DevExpress.XtraEditors.ProgressBarControl();
            this.lbDownloadRate = new DevExpress.XtraEditors.LabelControl();
            this.lbPercentage = new DevExpress.XtraEditors.LabelControl();
            this.lbTimeEstimate = new DevExpress.XtraEditors.LabelControl();
            this.btnDownload = new DevExpress.XtraEditors.SimpleButton();
            this.lbUploadRate = new DevExpress.XtraEditors.LabelControl();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbPercentage.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // pbPercentage
            // 
            this.pbPercentage.Location = new System.Drawing.Point(12, 294);
            this.pbPercentage.Name = "pbPercentage";
            this.pbPercentage.Size = new System.Drawing.Size(480, 18);
            this.pbPercentage.TabIndex = 1;
            // 
            // lbDownloadRate
            // 
            this.lbDownloadRate.Location = new System.Drawing.Point(12, 274);
            this.lbDownloadRate.Name = "lbDownloadRate";
            this.lbDownloadRate.Size = new System.Drawing.Size(88, 14);
            this.lbDownloadRate.TabIndex = 2;
            this.lbDownloadRate.Text = "lbDownloadRate";
            // 
            // lbPercentage
            // 
            this.lbPercentage.Location = new System.Drawing.Point(12, 318);
            this.lbPercentage.Name = "lbPercentage";
            this.lbPercentage.Size = new System.Drawing.Size(72, 14);
            this.lbPercentage.TabIndex = 3;
            this.lbPercentage.Text = "lbPercentage";
            // 
            // lbTimeEstimate
            // 
            this.lbTimeEstimate.Location = new System.Drawing.Point(12, 338);
            this.lbTimeEstimate.Name = "lbTimeEstimate";
            this.lbTimeEstimate.Size = new System.Drawing.Size(83, 14);
            this.lbTimeEstimate.TabIndex = 4;
            this.lbTimeEstimate.Text = "lbTimeEstimate";
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(417, 364);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 23);
            this.btnDownload.TabIndex = 5;
            this.btnDownload.Text = "Download";
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // lbUploadRate
            // 
            this.lbUploadRate.Location = new System.Drawing.Point(422, 274);
            this.lbUploadRate.Name = "lbUploadRate";
            this.lbUploadRate.Size = new System.Drawing.Size(71, 14);
            this.lbUploadRate.TabIndex = 6;
            this.lbUploadRate.Text = "lbUploadRate";
            // 
            // rtbLog
            // 
            this.rtbLog.Location = new System.Drawing.Point(12, 12);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(481, 222);
            this.rtbLog.TabIndex = 7;
            this.rtbLog.Text = "";
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 446);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.lbUploadRate);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.lbTimeEstimate);
            this.Controls.Add(this.lbPercentage);
            this.Controls.Add(this.lbDownloadRate);
            this.Controls.Add(this.pbPercentage);
            this.Name = "mainForm";
            this.Text = "mainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbPercentage.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.ProgressBarControl pbPercentage;
        private DevExpress.XtraEditors.LabelControl lbDownloadRate;
        private DevExpress.XtraEditors.LabelControl lbPercentage;
        private DevExpress.XtraEditors.LabelControl lbTimeEstimate;
        private DevExpress.XtraEditors.SimpleButton btnDownload;
        private DevExpress.XtraEditors.LabelControl lbUploadRate;
        private System.Windows.Forms.RichTextBox rtbLog;
    }
}