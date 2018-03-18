using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MicroCoin.Chain;

namespace Wallet.Views
{
    public partial class ChangePublicKeyForm : XtraForm
    {
        public ChangePublicKeyForm(Account account = null)
        {
            InitializeComponent();
            if (account != null)
            {
                signer.Text = account.AccountNumber.ToString();
            }
        }

        public static dynamic ShowForm(IWin32Window owner, Account account)
        {
            using (ChangePublicKeyForm form = new ChangePublicKeyForm(account))
            {
                if (form.ShowDialog(owner) != DialogResult.OK) return false;
                return new
                {
                    NewKey = form.newPublicKey.Text,
                    Signer = form.signer.Text,
                    Payload = form.payload.Text,
                    Fee = form.fee.Value
                };
            }
            
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
