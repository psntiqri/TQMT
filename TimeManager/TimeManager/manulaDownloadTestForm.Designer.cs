namespace TimeManager
{
    partial class manulaDownloadTestForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(manulaDownloadTestForm));
            this.axBioBridgeSDK1 = new AxBioBridgeSDKLib.AxBioBridgeSDK();
            this.downloadButton = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.poolingIntervalTextBox = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.axBioBridgeSDK1)).BeginInit();
            this.SuspendLayout();
            // 
            // axBioBridgeSDK1
            // 
            this.axBioBridgeSDK1.Enabled = true;
            this.axBioBridgeSDK1.Location = new System.Drawing.Point(742, 12);
            this.axBioBridgeSDK1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.axBioBridgeSDK1.Name = "axBioBridgeSDK1";
            this.axBioBridgeSDK1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axBioBridgeSDK1.OcxState")));
            this.axBioBridgeSDK1.Size = new System.Drawing.Size(101, 23);
            this.axBioBridgeSDK1.TabIndex = 1;
            // 
            // downloadButton
            // 
            this.downloadButton.Location = new System.Drawing.Point(280, 3);
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.Size = new System.Drawing.Size(165, 30);
            this.downloadButton.TabIndex = 3;
            this.downloadButton.Text = "Start Downloading";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.downloadButton_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.Location = new System.Drawing.Point(24, 77);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(818, 367);
            this.listView1.TabIndex = 4;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Enroll Number";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Date";
            this.columnHeader2.Width = 342;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Time";
            this.columnHeader3.Width = 520;
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Pooling Interval in seconds";
            // 
            // poolingIntervalTextBox
            // 
            this.poolingIntervalTextBox.Location = new System.Drawing.Point(164, 9);
            this.poolingIntervalTextBox.Name = "poolingIntervalTextBox";
            this.poolingIntervalTextBox.Size = new System.Drawing.Size(100, 20);
            this.poolingIntervalTextBox.TabIndex = 6;
            this.poolingIntervalTextBox.Text = "1";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(463, 6);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(240, 23);
            this.progressBar1.Step = 30;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 7;
            this.progressBar1.Visible = false;
            // 
            // manulaDownloadTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 464);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.poolingIntervalTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.downloadButton);
            this.Controls.Add(this.axBioBridgeSDK1);
            this.Name = "manulaDownloadTestForm";
            this.Text = "manulaDownloadTestForm";
            ((System.ComponentModel.ISupportInitialize)(this.axBioBridgeSDK1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxBioBridgeSDKLib.AxBioBridgeSDK axBioBridgeSDK1;
        private System.Windows.Forms.Button downloadButton;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox poolingIntervalTextBox;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}