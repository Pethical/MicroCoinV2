using System;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MicroCoin;
using MicroCoin.Chain;
using MicroCoin.Cryptography;

namespace Wallet.Views
{
    public partial class AccountEdit : XtraForm
    {
        public AccountEdit()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.heart;
        }

        public static bool EditAccount(Account account)
        {
            AccountEdit accountEdit = new AccountEdit();
            accountEdit.accountName.DataBindings.Add("EditValue", account, "Name", false, DataSourceUpdateMode.Never);
            accountEdit.keys.DataBindings.Add("EditValue", account.AccountInfo, "AccountKey", false, DataSourceUpdateMode.Never);
            accountEdit.keys.Properties.Items.AddRange(Node.Keys.ToArray());
            if(accountEdit.ShowDialog()== DialogResult.OK)
            {
                account.Name = accountEdit.accountName.Text;
                if (!account.AccountInfo.AccountKey.Equals((ECKeyPair)accountEdit.keys.SelectedItem))
                {
                    account.AccountInfo.NewPublicKey = (ECKeyPair)accountEdit.keys.SelectedItem;
                }
                return true;
            }
            return false;
        }

        private void targetAccount_Properties_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {

        }

        private void targetAccount_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
