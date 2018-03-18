namespace Wallet.Views
{
    partial class ChangePublicKeyForm
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
            this.newPublicKey = new DevExpress.XtraEditors.TextEdit();
            this.fee = new DevExpress.XtraEditors.SpinEdit();
            this.signer = new DevExpress.XtraEditors.ButtonEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.payload = new DevExpress.XtraEditors.MemoEdit();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.cancelButton = new DevExpress.XtraEditors.SimpleButton();
            this.okButton = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.newPublicKey.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fee.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.signer.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.payload.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // newPublicKey
            // 
            this.newPublicKey.Location = new System.Drawing.Point(17, 40);
            this.newPublicKey.Name = "newPublicKey";
            this.newPublicKey.Size = new System.Drawing.Size(574, 20);
            this.newPublicKey.TabIndex = 0;
            // 
            // fee
            // 
            this.fee.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.fee.Location = new System.Drawing.Point(17, 89);
            this.fee.Name = "fee";
            this.fee.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.fee.Properties.DisplayFormat.FormatString = "N4";
            this.fee.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.fee.Properties.EditFormat.FormatString = "N4";
            this.fee.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.fee.Properties.Increment = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.fee.Size = new System.Drawing.Size(100, 20);
            this.fee.TabIndex = 1;
            // 
            // signer
            // 
            this.signer.Location = new System.Drawing.Point(133, 90);
            this.signer.Name = "signer";
            this.signer.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.signer.Size = new System.Drawing.Size(100, 20);
            this.signer.TabIndex = 2;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(20, 24);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(150, 13);
            this.labelControl1.TabIndex = 3;
            this.labelControl1.Text = "Az új tulajdonos publikus kulcsa";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(17, 76);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(39, 13);
            this.labelControl2.TabIndex = 4;
            this.labelControl2.Text = "Költség:";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(133, 76);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(62, 13);
            this.labelControl3.TabIndex = 5;
            this.labelControl3.Text = "Aláíró számla";
            // 
            // payload
            // 
            this.payload.Location = new System.Drawing.Point(17, 139);
            this.payload.Name = "payload";
            this.payload.Size = new System.Drawing.Size(574, 110);
            this.payload.TabIndex = 6;
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(17, 120);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(112, 13);
            this.labelControl4.TabIndex = 7;
            this.labelControl4.Text = "Megjegyzés/Közlemény";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Image = global::Wallet.Properties.Resources.dialog_cancel;
            this.cancelButton.Location = new System.Drawing.Point(203, 299);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(87, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Mégsem";
            // 
            // okButton
            // 
            this.okButton.Image = global::Wallet.Properties.Resources.dialog_ok_apply;
            this.okButton.Location = new System.Drawing.Point(312, 300);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(87, 23);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "Rendben";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // ChangePublicKeyForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(603, 345);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.signer);
            this.Controls.Add(this.fee);
            this.Controls.Add(this.newPublicKey);
            this.Controls.Add(this.payload);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ChangePublicKeyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kulcs módosítása";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.newPublicKey.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fee.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.signer.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.payload.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.TextEdit newPublicKey;
        private DevExpress.XtraEditors.SpinEdit fee;
        private DevExpress.XtraEditors.ButtonEdit signer;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.MemoEdit payload;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.SimpleButton cancelButton;
        private DevExpress.XtraEditors.SimpleButton okButton;
    }
}