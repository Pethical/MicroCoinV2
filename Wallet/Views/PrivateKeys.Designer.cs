namespace Wallet.Views
{
    partial class PrivateKeys
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
            this.listBoxControl1 = new DevExpress.XtraEditors.ListBoxControl();
            this.newKey = new DevExpress.XtraEditors.SimpleButton();
            this.showPublicKey = new DevExpress.XtraEditors.SimpleButton();
            this.closeButton = new DevExpress.XtraEditors.SimpleButton();
            this.exportButton = new DevExpress.XtraEditors.SimpleButton();
            this.renameButton = new DevExpress.XtraEditors.SimpleButton();
            this.eCKeyPairBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.listBoxControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eCKeyPairBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxControl1
            // 
            this.listBoxControl1.DataSource = this.eCKeyPairBindingSource;
            this.listBoxControl1.DisplayMember = "Name";
            this.listBoxControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBoxControl1.Location = new System.Drawing.Point(0, 0);
            this.listBoxControl1.Name = "listBoxControl1";
            this.listBoxControl1.Size = new System.Drawing.Size(617, 378);
            this.listBoxControl1.TabIndex = 0;
            // 
            // newKey
            // 
            this.newKey.Image = global::Wallet.Properties.Resources.list_add;
            this.newKey.Location = new System.Drawing.Point(11, 402);
            this.newKey.Name = "newKey";
            this.newKey.Size = new System.Drawing.Size(84, 23);
            this.newKey.TabIndex = 5;
            this.newKey.Text = "Új kulcs";
            this.newKey.Click += new System.EventHandler(this.newKey_Click);
            // 
            // showPublicKey
            // 
            this.showPublicKey.Image = global::Wallet.Properties.Resources.edit_copy;
            this.showPublicKey.Location = new System.Drawing.Point(281, 402);
            this.showPublicKey.Name = "showPublicKey";
            this.showPublicKey.Size = new System.Drawing.Size(149, 23);
            this.showPublicKey.TabIndex = 4;
            this.showPublicKey.Text = "Publikus kulcs másolása";
            this.showPublicKey.Click += new System.EventHandler(this.showPublicKey_Click);
            // 
            // closeButton
            // 
            this.closeButton.Image = global::Wallet.Properties.Resources.dialog_ok_apply;
            this.closeButton.Location = new System.Drawing.Point(529, 402);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(82, 23);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Bezárás";
            this.closeButton.Click += new System.EventHandler(this.simpleButton3_Click);
            // 
            // exportButton
            // 
            this.exportButton.Image = global::Wallet.Properties.Resources.document_save;
            this.exportButton.Location = new System.Drawing.Point(191, 402);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(82, 23);
            this.exportButton.TabIndex = 2;
            this.exportButton.Text = "Exportálás";
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // renameButton
            // 
            this.renameButton.Image = global::Wallet.Properties.Resources.document_edit;
            this.renameButton.Location = new System.Drawing.Point(101, 402);
            this.renameButton.Name = "renameButton";
            this.renameButton.Size = new System.Drawing.Size(82, 23);
            this.renameButton.TabIndex = 1;
            this.renameButton.Text = "Átnevezés";
            // 
            // eCKeyPairBindingSource
            // 
            this.eCKeyPairBindingSource.DataSource = typeof(MicroCoin.Cryptography.ECKeyPair);
            // 
            // PrivateKeys
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 434);
            this.Controls.Add(this.newKey);
            this.Controls.Add(this.showPublicKey);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.renameButton);
            this.Controls.Add(this.listBoxControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PrivateKeys";
            this.ShowInTaskbar = false;
            this.Text = "PrivateKeys";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.listBoxControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eCKeyPairBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.BindingSource eCKeyPairBindingSource;
        private DevExpress.XtraEditors.ListBoxControl listBoxControl1;
        private DevExpress.XtraEditors.SimpleButton renameButton;
        private DevExpress.XtraEditors.SimpleButton exportButton;
        private DevExpress.XtraEditors.SimpleButton closeButton;
        private DevExpress.XtraEditors.SimpleButton showPublicKey;
        private DevExpress.XtraEditors.SimpleButton newKey;
    }
}