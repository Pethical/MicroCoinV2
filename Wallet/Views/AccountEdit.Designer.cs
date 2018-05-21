namespace Wallet.Views
{
    partial class AccountEdit
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
            this.accountName = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.keys = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.targetAccount = new DevExpress.XtraEditors.ButtonEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.accountName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.keys.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.targetAccount.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // accountName
            // 
            this.accountName.Location = new System.Drawing.Point(12, 32);
            this.accountName.Name = "accountName";
            this.accountName.Size = new System.Drawing.Size(294, 20);
            this.accountName.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 13);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(60, 13);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "Számla neve";
            // 
            // keys
            // 
            this.keys.Location = new System.Drawing.Point(12, 80);
            this.keys.Name = "keys";
            this.keys.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.keys.Properties.ImmediatePopup = true;
            this.keys.Size = new System.Drawing.Size(294, 20);
            this.keys.TabIndex = 2;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(12, 61);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(54, 13);
            this.labelControl2.TabIndex = 3;
            this.labelControl2.Text = "Privát kulcs";
            // 
            // targetAccount
            // 
            this.targetAccount.Location = new System.Drawing.Point(79, 116);
            this.targetAccount.Name = "targetAccount";
            this.targetAccount.Properties.Appearance.Options.UseTextOptions = true;
            this.targetAccount.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.targetAccount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.targetAccount.Properties.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.targetAccount_Properties_ButtonClick);
            this.targetAccount.Size = new System.Drawing.Size(100, 20);
            this.targetAccount.TabIndex = 30;
            this.targetAccount.ToolTip = "Itt választhatod ki a számlát amely az utalt összeget fogadni fogja";
            this.targetAccount.ToolTipTitle = "Cél számla";
            this.targetAccount.EditValueChanged += new System.EventHandler(this.targetAccount_EditValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Aláíró számla";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Image = global::Wallet.Properties.Resources.dialog_ok_apply;
            this.simpleButton1.Location = new System.Drawing.Point(166, 183);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 23);
            this.simpleButton1.TabIndex = 31;
            this.simpleButton1.Text = "Rendben";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.simpleButton2.Image = global::Wallet.Properties.Resources.dialog_cancel;
            this.simpleButton2.Location = new System.Drawing.Point(79, 183);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 23);
            this.simpleButton2.TabIndex = 32;
            this.simpleButton2.Text = "Mégsem";
            // 
            // AccountEdit
            // 
            this.AcceptButton = this.simpleButton1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.simpleButton2;
            this.ClientSize = new System.Drawing.Size(313, 212);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.targetAccount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.keys);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.accountName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AccountEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Számla módosítása";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.accountName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.keys.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.targetAccount.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.TextEdit accountName;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.ComboBoxEdit keys;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.ButtonEdit targetAccount;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
    }
}