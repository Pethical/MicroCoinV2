//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// MainForm.cs - Copyright (c) 2018 Németh Péter
//-----------------------------------------------------------------------
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//-------------------------------------------------------------------------
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.Utils.Drawing;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using MicroCoin;
using MicroCoin.Chain;
using MicroCoin.Cryptography;
using MicroCoin.Util;

namespace Wallet.Views
{
    public partial class MainForm : RibbonForm
    {
        public MainForm()
        {
            InitializeComponent();
            SkinManager.EnableFormSkins();
            repositoryItemMarqueeProgressBar1.ProgressAnimationMode = ProgressAnimationMode.PingPong;
        }

        private List<ECKeyPair> Keys { get; } = new List<ECKeyPair>();

        private void Form1_Load(object sender, EventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MicroCoin";
            var fs = File.Open(path + "\\WalletKeys.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using (var br = new BinaryReader(fs))
            {
                var magic = ByteString.ReadFromStream(br);
                var version = br.ReadUInt32();
                var count = br.ReadUInt32();
                for (var i = 0; i < count; i++)
                {
                    var keyPair = new ECKeyPair();
                    keyPair.LoadFromStream(fs, false, true, true);
                    Keys.Add(keyPair);
                    keyPair.DecriptKey("");
                }
            }

            Task.Run(async () =>
            {
                Node.Instance.NodeServers.NewNode += (ob, ev) =>
                {
                    barStaticItem7.Caption = Node.Instance.NodeServers.Count.ToString();
                };
                await Node.StartNode(4004, Keys);
                Action action = () =>
                {                    
                    repositoryItemMarqueeProgressBar1.Paused = true;
                    var myAccounts = Node.Instance.Accounts.Where(p => Keys.Contains(p.AccountInfo.AccountKey));
                    accountBindingSource.DataSource = myAccounts;
                    accountCount.Text = myAccounts.Count().ToString("N0") + " db";
                    currentBalance.Text = myAccounts.Sum(p => p.VisibleBalance).ToString("N") + " MCC";
                    var b = Node.Instance.BlockChain.GetNewTarget();
                    var lastBlock = Node.Instance.BlockChain.Get(2048);
                    lastBlock.GetBlockHeaderForHash();
                    /*var timer = new Timer
                    {
                        Interval = 5000
                    };
                    timer.Tick += (o, ev) => { barStaticItem7.Caption = Node.Instance.NodeServers.Count.ToString(); };
                    timer.Enabled = true;*/
                };
                Invoke(action);
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Node.Instance.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var signer = (Account) accountBindingSource.Current;
            var target =
                ((IEnumerable<Account>) accountBindingSource.DataSource).FirstOrDefault(p =>
                    p.AccountNumber == targetAccount.Text);
            var accountKey = Keys.FirstOrDefault(p =>
                p.PublicKey.X.SequenceEqual((byte[]) signer.AccountInfo.AccountKey.X) &&
                p.PublicKey.Y.SequenceEqual((byte[]) signer.AccountInfo.AccountKey.Y)
            );
            if (accountKey == null)
            {
                XtraMessageBox.Show("A számla kulcsa nem található!", "Hiba", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (XtraMessageBox.Show(this, "Biztosan elutalod?", "Kérdés", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
            Node.Instance.SendCoin(signer, target, amountToPay.Value, fee.Value, accountKey, payload.Text);
        }

        private void barButtonItem13_ItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        private void targetAccount_Properties_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            var selector = new AccountSelector();
            if (selector.ShowDialog(this) == DialogResult.OK)
                targetAccount.Text = selector.SelectedAccount.AccountNumber.ToString();
        }

        private void targetAccount_EditValueChanged(object sender, EventArgs e)
        {
        }

        private void targetAccount_Validating(object sender, CancelEventArgs e)
        {
            AccountNumber an = targetAccount.Text;
        }

        private void barButtonItem6_ItemClick(object sender, ItemClickEventArgs e)
        {
            new PrivateKeys().ShowDialog();
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            var t = new Transactions();
            var acc = (Account) accountBindingSource.Current;
            t.ShowAccount(acc.AccountNumber);
            t.ShowDialog(this);
        }

        private void editAccount_ItemClick(object sender, ItemClickEventArgs e)
        {
            var account = (Account) accountBindingSource.Current;
            var accountKey = Keys.FirstOrDefault(p =>
                p.PublicKey.X.SequenceEqual((byte[]) account.AccountInfo.AccountKey.X) &&
                p.PublicKey.Y.SequenceEqual((byte[]) account.AccountInfo.AccountKey.Y)
            );
            account.AccountInfo.AccountKey = accountKey;
            if (AccountEdit.EditAccount(account)) Node.Instance.ChangeAccountInfo(account, 0, "", accountKey);
        }

        private void sellAccount_ItemClick(object sender, ItemClickEventArgs e)
        {
            var account = (Account) accountBindingSource.Current;
            var accountKey = Keys.FirstOrDefault(p =>
                p.PublicKey.X.SequenceEqual((byte[]) account.AccountInfo.AccountKey.X) &&
                p.PublicKey.Y.SequenceEqual((byte[]) account.AccountInfo.AccountKey.Y)
            );
            account.AccountInfo.AccountKey = accountKey;
            var a = SellAccount.ShowSellAccount();
            if (a is bool) return;
            Node.Instance.SellAccount(account, a.Price, a.Fee, a.Seller, accountKey);
        }

        private void buyAccount_ItemClick(object sender, ItemClickEventArgs e)
        {
            var accountSelector = new AccountSelector(Node.Instance.Accounts
                .Where(p => p.AccountInfo.State == AccountInfo.AccountState.Sale).ToList());
            if (accountSelector.ShowDialog(this) == DialogResult.OK)
            {
                var buyer = (Account) accountBindingSource.Current;
                var account = accountSelector.SelectedAccount;
                if (XtraMessageBox.Show(this,
                        $"Biztosan szeretnéd megásárolni a(z) {account.AccountNumber} számlát {account.AccountInfo.VisiblePrice} MicroCoinért? A számlát árát a {buyer.AccountNumber} számládról fizeted.",
                        "Megerősítés", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                var accountKey = Keys.FirstOrDefault(p =>
                    p.PublicKey.X.SequenceEqual((byte[]) buyer.AccountInfo.AccountKey.X) &&
                    p.PublicKey.Y.SequenceEqual((byte[]) buyer.AccountInfo.AccountKey.Y)
                );
                buyer.AccountInfo.AccountKey = accountKey;

                Node.Instance.BuyAccount(account, 0, "", buyer, accountKey);
            }
        }

        private void targetAccount_Properties_Validating(object sender, CancelEventArgs e)
        {
        }

        private void changeAccountKey_ItemClick(object sender, ItemClickEventArgs e)
        {
            var account = (Account) accountBindingSource.Current;
            var result = ChangePublicKeyForm.ShowForm(this, account);

            if (result is bool && result == false) return;

            var accountKey = Keys.FirstOrDefault(p =>
                p.PublicKey.X.SequenceEqual((byte[]) account.AccountInfo.AccountKey.X) &&
                p.PublicKey.Y.SequenceEqual((byte[]) account.AccountInfo.AccountKey.Y)
            );

            account.AccountInfo.AccountKey = accountKey;

            if (XtraMessageBox.Show(this,
                    "Biztosan megváltoztatod a publikus kulcsot? A változtatás után csak az új kulcs tulajdonosa kezelheti a számlát.",
                    "Megerősítés", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) ==
                DialogResult.Yes)
                Node.Instance.ChangeAccountKey(account, result.Fee, result.Payload, account, accountKey, result.NewKey);
        }

        private void exportKeys_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.CreatePrompt = false;
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.FileName = "";
                saveFileDialog.DefaultExt = "keys";
                saveFileDialog.Filter = "MicroCoin kulcstár|*.keys";
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                    using (var fs = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write,
                        FileShare.None))
                    {
                        foreach (var keyPair in Node.Keys) keyPair.SaveToStream(fs, false, true, true);
                    }
            }
        }

        private void importKeys_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "MicroCoin kulcstár|*.keys";
                openFileDialog.CheckFileExists = true;
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var walletKeys = new List<ECKeyPair>();
                    using (var fs = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        while (fs.Position < fs.Length - 1)
                        {
                            var keyPair = new ECKeyPair();
                            keyPair.LoadFromStream(fs, false, true, true);
                            walletKeys.Add(keyPair);
                        }

                        Node.Keys.Clear();
                        Node.Keys = walletKeys;
                        XtraMessageBox.Show($"Sikeresen importáltunk {walletKeys.Count} kulcsot!", "Importálás",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void blockExplorer_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var blockExplorerForm = new BlockExplorer())
            {
                blockExplorerForm.ShowDialog();
            }
        }

        private void gridView1_MasterRowExpanding(object sender, MasterRowCanExpandEventArgs e)
        {
        }

        private void accountsButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var asForm = new AccountSelector())
            {
                asForm.ShowDialog(this);
            }
        }
    }
}