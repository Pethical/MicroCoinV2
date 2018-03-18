using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Wallet.Views
{
    public partial class SellAccount : XtraForm
    {
        public SellAccount()
        {
            InitializeComponent();
        }

        public static dynamic ShowSellAccount()
        {
            SellAccount sellAccount = new SellAccount();
            if (sellAccount.ShowDialog() == DialogResult.OK)
            {

                return new
                {
                    Fee = sellAccount.fee.Value,
                    Price = sellAccount.accountPrice.Value,
                    Seller = sellAccount.sellerAccount.Text
                };
            }
            else
            {
                return false;
            }
        }

        private void labelControl2_Click(object sender, EventArgs e)
        {

        }

        private void sellerAccount_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void sellerAccount_Properties_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            var selector = new AccountSelector();
            if (selector.ShowDialog(this) == DialogResult.OK)
            {
                sellerAccount.Text = selector.SelectedAccount.AccountNumber.ToString();
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
