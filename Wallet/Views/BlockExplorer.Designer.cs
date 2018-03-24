namespace Wallet.Views
{
    partial class BlockExplorer
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
            DevExpress.XtraGrid.GridLevelNode gridLevelNode1 = new DevExpress.XtraGrid.GridLevelNode();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colSignerAccount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTargetAccount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colAmount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colFee1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.blockBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colBlockNumber = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTransactionCount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTransactionsType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colBlockSignature = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colProtocolVersion = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colAvailableProtocol = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colAccountKey = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colReward = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colFee = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTimestamp = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCompactTarget = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNonce = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colPayload = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCheckPointHash = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTransactionHash = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colProofOfWork = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemProgressBar1 = new DevExpress.XtraEditors.Repository.RepositoryItemProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blockBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemProgressBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridView2
            // 
            this.gridView2.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colSignerAccount,
            this.colTargetAccount,
            this.colAmount,
            this.colFee1});
            this.gridView2.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.gridView2.GridControl = this.gridControl1;
            this.gridView2.Name = "gridView2";
            this.gridView2.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridView2.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridView2.OptionsBehavior.AllowFixedGroups = DevExpress.Utils.DefaultBoolean.False;
            this.gridView2.OptionsBehavior.Editable = false;
            this.gridView2.OptionsBehavior.ReadOnly = true;
            this.gridView2.OptionsFilter.AllowFilterEditor = false;
            this.gridView2.OptionsFilter.AllowFilterIncrementalSearch = false;
            this.gridView2.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gridView2.OptionsView.ShowGroupPanel = false;
            // 
            // colSignerAccount
            // 
            this.colSignerAccount.Caption = "Aláíró (Küldő)";
            this.colSignerAccount.FieldName = "SignerAccount";
            this.colSignerAccount.Name = "colSignerAccount";
            this.colSignerAccount.OptionsColumn.AllowEdit = false;
            this.colSignerAccount.OptionsColumn.ReadOnly = true;
            this.colSignerAccount.OptionsFilter.AllowAutoFilter = false;
            this.colSignerAccount.OptionsFilter.AllowFilter = false;
            this.colSignerAccount.Visible = true;
            this.colSignerAccount.VisibleIndex = 0;
            // 
            // colTargetAccount
            // 
            this.colTargetAccount.Caption = "Cél";
            this.colTargetAccount.FieldName = "TargetAccount";
            this.colTargetAccount.Name = "colTargetAccount";
            this.colTargetAccount.Visible = true;
            this.colTargetAccount.VisibleIndex = 1;
            // 
            // colAmount
            // 
            this.colAmount.Caption = "Összeg";
            this.colAmount.DisplayFormat.FormatString = "N4";
            this.colAmount.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colAmount.FieldName = "Amount";
            this.colAmount.Name = "colAmount";
            this.colAmount.Visible = true;
            this.colAmount.VisibleIndex = 2;
            // 
            // colFee1
            // 
            this.colFee1.Caption = "Költség";
            this.colFee1.DisplayFormat.FormatString = "N4";
            this.colFee1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colFee1.FieldName = "Fee";
            this.colFee1.Name = "colFee1";
            this.colFee1.Visible = true;
            this.colFee1.VisibleIndex = 3;
            // 
            // gridControl1
            // 
            this.gridControl1.DataSource = this.blockBindingSource;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            gridLevelNode1.LevelTemplate = this.gridView2;
            gridLevelNode1.RelationName = "Transactions";
            this.gridControl1.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] {
            gridLevelNode1});
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemProgressBar1});
            this.gridControl1.Size = new System.Drawing.Size(800, 450);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.UseEmbeddedNavigator = true;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1,
            this.gridView2});
            // 
            // blockBindingSource
            // 
            this.blockBindingSource.DataSource = typeof(MicroCoin.Chain.Block);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colBlockNumber,
            this.colTransactionCount,
            this.colTransactionsType,
            this.colBlockSignature,
            this.colProtocolVersion,
            this.colAvailableProtocol,
            this.colAccountKey,
            this.colReward,
            this.colFee,
            this.colTimestamp,
            this.colCompactTarget,
            this.colNonce,
            this.colPayload,
            this.colCheckPointHash,
            this.colTransactionHash,
            this.colProofOfWork});
            this.gridView1.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            this.gridView1.OptionsBehavior.AllowFixedGroups = DevExpress.Utils.DefaultBoolean.False;
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsBehavior.ReadOnly = true;
            this.gridView1.OptionsDetail.EnableDetailToolTip = true;
            this.gridView1.OptionsDetail.ShowDetailTabs = false;
            this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            // 
            // colBlockNumber
            // 
            this.colBlockNumber.AppearanceCell.Options.UseTextOptions = true;
            this.colBlockNumber.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colBlockNumber.AppearanceHeader.Options.UseTextOptions = true;
            this.colBlockNumber.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colBlockNumber.Caption = "Blokk";
            this.colBlockNumber.FieldName = "BlockNumber";
            this.colBlockNumber.Name = "colBlockNumber";
            this.colBlockNumber.Visible = true;
            this.colBlockNumber.VisibleIndex = 0;
            this.colBlockNumber.Width = 45;
            // 
            // colTransactionCount
            // 
            this.colTransactionCount.AppearanceCell.Options.UseTextOptions = true;
            this.colTransactionCount.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colTransactionCount.AppearanceHeader.Options.UseTextOptions = true;
            this.colTransactionCount.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colTransactionCount.Caption = "Tranzakciók";
            this.colTransactionCount.FieldName = "TransactionCount";
            this.colTransactionCount.Name = "colTransactionCount";
            this.colTransactionCount.Visible = true;
            this.colTransactionCount.VisibleIndex = 6;
            this.colTransactionCount.Width = 72;
            // 
            // colTransactionsType
            // 
            this.colTransactionsType.FieldName = "TransactionsType";
            this.colTransactionsType.Name = "colTransactionsType";
            this.colTransactionsType.Width = 45;
            // 
            // colBlockSignature
            // 
            this.colBlockSignature.FieldName = "BlockSignature";
            this.colBlockSignature.Name = "colBlockSignature";
            this.colBlockSignature.Width = 45;
            // 
            // colProtocolVersion
            // 
            this.colProtocolVersion.FieldName = "ProtocolVersion";
            this.colProtocolVersion.Name = "colProtocolVersion";
            this.colProtocolVersion.Width = 45;
            // 
            // colAvailableProtocol
            // 
            this.colAvailableProtocol.FieldName = "AvailableProtocol";
            this.colAvailableProtocol.Name = "colAvailableProtocol";
            this.colAvailableProtocol.Width = 45;
            // 
            // colAccountKey
            // 
            this.colAccountKey.FieldName = "AccountKey";
            this.colAccountKey.Name = "colAccountKey";
            this.colAccountKey.Width = 45;
            // 
            // colReward
            // 
            this.colReward.AppearanceCell.Options.UseTextOptions = true;
            this.colReward.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colReward.AppearanceHeader.Options.UseTextOptions = true;
            this.colReward.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colReward.Caption = "Jutalom";
            this.colReward.DisplayFormat.FormatString = "{0:N4} MCC";
            this.colReward.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colReward.FieldName = "Reward.value";
            this.colReward.FieldNameSortGroup = "Reward.value";
            this.colReward.Name = "colReward";
            this.colReward.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colReward.Visible = true;
            this.colReward.VisibleIndex = 2;
            this.colReward.Width = 45;
            // 
            // colFee
            // 
            this.colFee.AppearanceCell.Options.UseTextOptions = true;
            this.colFee.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colFee.AppearanceHeader.Options.UseTextOptions = true;
            this.colFee.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colFee.Caption = "Költség";
            this.colFee.DisplayFormat.FormatString = "{0:N4} MCC";
            this.colFee.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colFee.FieldName = "Fee.value";
            this.colFee.FieldNameSortGroup = "Fee.value";
            this.colFee.Name = "colFee";
            this.colFee.Visible = true;
            this.colFee.VisibleIndex = 3;
            this.colFee.Width = 45;
            // 
            // colTimestamp
            // 
            this.colTimestamp.AppearanceCell.Options.UseTextOptions = true;
            this.colTimestamp.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colTimestamp.AppearanceHeader.Options.UseTextOptions = true;
            this.colTimestamp.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colTimestamp.Caption = "Dátum / idő";
            this.colTimestamp.DisplayFormat.FormatString = "d";
            this.colTimestamp.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.colTimestamp.FieldName = "Timestamp";
            this.colTimestamp.Name = "colTimestamp";
            this.colTimestamp.Visible = true;
            this.colTimestamp.VisibleIndex = 1;
            this.colTimestamp.Width = 45;
            // 
            // colCompactTarget
            // 
            this.colCompactTarget.AppearanceCell.Options.UseTextOptions = true;
            this.colCompactTarget.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colCompactTarget.AppearanceHeader.Options.UseTextOptions = true;
            this.colCompactTarget.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colCompactTarget.Caption = "Nehézség";
            this.colCompactTarget.DisplayFormat.FormatString = "X";
            this.colCompactTarget.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colCompactTarget.FieldName = "CompactTarget";
            this.colCompactTarget.Name = "colCompactTarget";
            this.colCompactTarget.Visible = true;
            this.colCompactTarget.VisibleIndex = 5;
            this.colCompactTarget.Width = 45;
            // 
            // colNonce
            // 
            this.colNonce.AppearanceCell.Options.UseTextOptions = true;
            this.colNonce.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colNonce.AppearanceHeader.Options.UseTextOptions = true;
            this.colNonce.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colNonce.Caption = "Nonce";
            this.colNonce.FieldName = "Nonce";
            this.colNonce.Name = "colNonce";
            this.colNonce.Visible = true;
            this.colNonce.VisibleIndex = 7;
            this.colNonce.Width = 45;
            // 
            // colPayload
            // 
            this.colPayload.Caption = "Bányász";
            this.colPayload.FieldName = "Payload";
            this.colPayload.Name = "colPayload";
            this.colPayload.Visible = true;
            this.colPayload.VisibleIndex = 4;
            this.colPayload.Width = 45;
            // 
            // colCheckPointHash
            // 
            this.colCheckPointHash.FieldName = "CheckPointHash";
            this.colCheckPointHash.Name = "colCheckPointHash";
            this.colCheckPointHash.Width = 45;
            // 
            // colTransactionHash
            // 
            this.colTransactionHash.FieldName = "TransactionHash";
            this.colTransactionHash.Name = "colTransactionHash";
            this.colTransactionHash.Width = 45;
            // 
            // colProofOfWork
            // 
            this.colProofOfWork.AppearanceCell.Options.UseTextOptions = true;
            this.colProofOfWork.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colProofOfWork.AppearanceHeader.Options.UseTextOptions = true;
            this.colProofOfWork.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colProofOfWork.Caption = "Munka (PoW)";
            this.colProofOfWork.FieldName = "ProofOfWork";
            this.colProofOfWork.Name = "colProofOfWork";
            this.colProofOfWork.Visible = true;
            this.colProofOfWork.VisibleIndex = 8;
            this.colProofOfWork.Width = 80;
            // 
            // repositoryItemProgressBar1
            // 
            this.repositoryItemProgressBar1.Name = "repositoryItemProgressBar1";
            // 
            // BlockExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.gridControl1);
            this.Name = "BlockExplorer";
            this.Text = "Block felfedező";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BlockExplorer_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blockBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemProgressBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.BindingSource blockBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colTransactionCount;
        private DevExpress.XtraGrid.Columns.GridColumn colTransactionsType;
        private DevExpress.XtraGrid.Columns.GridColumn colBlockSignature;
        private DevExpress.XtraGrid.Columns.GridColumn colProtocolVersion;
        private DevExpress.XtraGrid.Columns.GridColumn colAvailableProtocol;
        private DevExpress.XtraGrid.Columns.GridColumn colBlockNumber;
        private DevExpress.XtraGrid.Columns.GridColumn colAccountKey;
        private DevExpress.XtraGrid.Columns.GridColumn colReward;
        private DevExpress.XtraGrid.Columns.GridColumn colFee;
        private DevExpress.XtraGrid.Columns.GridColumn colTimestamp;
        private DevExpress.XtraGrid.Columns.GridColumn colCompactTarget;
        private DevExpress.XtraGrid.Columns.GridColumn colNonce;
        private DevExpress.XtraGrid.Columns.GridColumn colPayload;
        private DevExpress.XtraGrid.Columns.GridColumn colCheckPointHash;
        private DevExpress.XtraGrid.Columns.GridColumn colTransactionHash;
        private DevExpress.XtraGrid.Columns.GridColumn colProofOfWork;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraGrid.Columns.GridColumn colSignerAccount;
        private DevExpress.XtraGrid.Columns.GridColumn colTargetAccount;
        private DevExpress.XtraGrid.Columns.GridColumn colFee1;
        private DevExpress.XtraGrid.Columns.GridColumn colAmount;
        private DevExpress.XtraEditors.Repository.RepositoryItemProgressBar repositoryItemProgressBar1;
    }
}