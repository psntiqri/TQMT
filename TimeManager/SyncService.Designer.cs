namespace Exilesoft.TimeManager
{
    partial class SyncService
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncService));
            this.myTimeEventLog = new System.Diagnostics.EventLog();
            this.button_sync = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.axBioBridgeSDK1 = new AxBioBridgeSDKLib.AxBioBridgeSDK();
            this.txtCloudSyncLog = new System.Windows.Forms.TextBox();
            this.lblCloudSyncLog = new System.Windows.Forms.Label();
            this.lblTimeRecordsSyncLog = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.myTimeEventLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axBioBridgeSDK1)).BeginInit();
            this.SuspendLayout();
            // 
            // myTimeEventLog
            // 
            this.myTimeEventLog.SynchronizingObject = this;
            // 
            // button_sync
            // 
            this.button_sync.Location = new System.Drawing.Point(12, 12);
            this.button_sync.Name = "button_sync";
            this.button_sync.Size = new System.Drawing.Size(75, 23);
            this.button_sync.TabIndex = 0;
            this.button_sync.Text = "Start Sync";
            this.button_sync.UseVisualStyleBackColor = true;
            this.button_sync.Click += new System.EventHandler(this.button_sync_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 69);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(663, 161);
            this.textBoxLog.TabIndex = 1;
            // 
            // axBioBridgeSDK1
            // 
            this.axBioBridgeSDK1.Enabled = true;
            this.axBioBridgeSDK1.Location = new System.Drawing.Point(573, 12);
            this.axBioBridgeSDK1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.axBioBridgeSDK1.Name = "axBioBridgeSDK1";
            this.axBioBridgeSDK1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axBioBridgeSDK1.OcxState")));
            this.axBioBridgeSDK1.Size = new System.Drawing.Size(101, 23);
            this.axBioBridgeSDK1.TabIndex = 2;
            this.axBioBridgeSDK1.OnConnect += new System.EventHandler(this.axBioBridgeSDK1_OnConnect);
            // 
            // txtCloudSyncLog
            // 
            this.txtCloudSyncLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCloudSyncLog.Location = new System.Drawing.Point(12, 257);
            this.txtCloudSyncLog.Multiline = true;
            this.txtCloudSyncLog.Name = "txtCloudSyncLog";
            this.txtCloudSyncLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCloudSyncLog.Size = new System.Drawing.Size(663, 166);
            this.txtCloudSyncLog.TabIndex = 3;
            // 
            // lblCloudSyncLog
            // 
            this.lblCloudSyncLog.AutoSize = true;
            this.lblCloudSyncLog.Location = new System.Drawing.Point(12, 239);
            this.lblCloudSyncLog.Name = "lblCloudSyncLog";
            this.lblCloudSyncLog.Size = new System.Drawing.Size(82, 13);
            this.lblCloudSyncLog.TabIndex = 4;
            this.lblCloudSyncLog.Text = "Cloud Sync Log";
            // 
            // lblTimeRecordsSyncLog
            // 
            this.lblTimeRecordsSyncLog.AutoSize = true;
            this.lblTimeRecordsSyncLog.Location = new System.Drawing.Point(12, 49);
            this.lblTimeRecordsSyncLog.Name = "lblTimeRecordsSyncLog";
            this.lblTimeRecordsSyncLog.Size = new System.Drawing.Size(121, 13);
            this.lblTimeRecordsSyncLog.TabIndex = 5;
            this.lblTimeRecordsSyncLog.Text = "Time Records Sync Log";
            // 
            // SyncService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 432);
            this.Controls.Add(this.lblTimeRecordsSyncLog);
            this.Controls.Add(this.lblCloudSyncLog);
            this.Controls.Add(this.txtCloudSyncLog);
            this.Controls.Add(this.axBioBridgeSDK1);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.button_sync);
            this.Name = "SyncService";
            this.Text = "SyncService";
            ((System.ComponentModel.ISupportInitialize)(this.myTimeEventLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axBioBridgeSDK1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Diagnostics.EventLog myTimeEventLog;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button button_sync;

        private AxBioBridgeSDKLib.AxBioBridgeSDK axBioBridgeSDK1;
        private System.Windows.Forms.TextBox txtCloudSyncLog;
        private System.Windows.Forms.Label lblTimeRecordsSyncLog;
        private System.Windows.Forms.Label lblCloudSyncLog;
    }
}