namespace Wallet.Views
{
    partial class SellAccount
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
            this.accountPrice = new DevExpress.XtraEditors.SpinEdit();
            this.cancelButton = new DevExpress.XtraEditors.SimpleButton();
            this.okButton = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.sellerAccount = new DevExpress.XtraEditors.ButtonEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.fee = new DevExpress.XtraEditors.SpinEdit();
            ((System.ComponentModel.ISupportInitialize)(this.accountPrice.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sellerAccount.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fee.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // accountPrice
            // 
            this.accountPrice.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.accountPrice.Location = new System.Drawing.Point(27, 44);
            this.accountPrice.Name = "accountPrice";
            this.accountPrice.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.accountPrice.Properties.DisplayFormat.FormatString = "N4";
            this.accountPrice.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.accountPrice.Properties.EditFormat.FormatString = "N4";
            this.accountPrice.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.accountPrice.Properties.Increment = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.accountPrice.Size = new System.Drawing.Size(83, 20);
            this.accountPrice.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Image = global::Wallet.Properties.Resources.dialog_cancel;
            this.cancelButton.Location = new System.Drawing.Point(114, 104);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Mégsem";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Image = global::Wallet.Properties.Resources.dialog_ok_apply;
            this.okButton.Location = new System.Drawing.Point(217, 104);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "Rendben";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(27, 25);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(46, 13);
            this.labelControl1.TabIndex = 4;
            this.labelControl1.Text = "Eladási ár";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(138, 25);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(61, 13);
            this.labelControl2.TabIndex = 5;
            this.labelControl2.Text = "Eladó számla";
            this.labelControl2.Click += new System.EventHandler(this.labelControl2_Click);
            // 
            // sellerAccount
            // 
            this.sellerAccount.Location = new System.Drawing.Point(138, 44);
            this.sellerAccount.Name = "sellerAccount";
            this.sellerAccount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.sellerAccount.Properties.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.sellerAccount_Properties_ButtonClick);
            this.sellerAccount.Size = new System.Drawing.Size(135, 20);
            this.sellerAccount.TabIndex = 6;
            this.sellerAccount.EditValueChanged += new System.EventHandler(this.sellerAccount_EditValueChanged);
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(296, 25);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(35, 13);
            this.labelControl3.TabIndex = 8;
            this.labelControl3.Text = "Költség";
            // 
            // fee
            // 
            this.fee.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.fee.Location = new System.Drawing.Point(296, 44);
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
            this.fee.Size = new System.Drawing.Size(83, 20);
            this.fee.TabIndex = 7;
            // 
            // SellAccount
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(406, 139);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.fee);
            this.Controls.Add(this.sellerAccount);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.accountPrice);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SellAccount";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Számla eladása";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.accountPrice.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sellerAccount.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fee.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SpinEdit accountPrice;
        private DevExpress.XtraEditors.SimpleButton cancelButton;
        private DevExpress.XtraEditors.SimpleButton okButton;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.ButtonEdit sellerAccount;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SpinEdit fee;
    }
}