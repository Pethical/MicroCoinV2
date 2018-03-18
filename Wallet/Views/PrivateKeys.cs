//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// PrivateKeys.cs - Copyright (c) 2018 Németh Péter
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
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MicroCoin;
using MicroCoin.Cryptography;

namespace Wallet.Views
{
    public partial class PrivateKeys : XtraForm
    {
        public PrivateKeys()
        {
            InitializeComponent();

            eCKeyPairBindingSource.DataSource = Node.Keys;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showPublicKey_Click(object sender, EventArgs e)
        {
            var keyPair = (ECKeyPair)eCKeyPairBindingSource.Current;
            var s = keyPair.ToEncodedString();
            Clipboard.SetText(s);
            XtraMessageBox.Show("A publikus kulcsot a vágólapra másoltuk.","Publikus kulcs",MessageBoxButtons.OK,MessageBoxIcon.Information);            
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            var keyPair = (ECKeyPair)eCKeyPairBindingSource.Current;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.CreatePrompt = false;
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.FileName = keyPair.Name;
                saveFileDialog.DefaultExt = "key";
                saveFileDialog.Filter = "MicroCoin kulcs|*.key";
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (FileStream fs = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        keyPair.SaveToStream(fs, false, true, true);
                    }
                }
            }
        }

        private void newKey_Click(object sender, EventArgs e)
        {
            ECKeyPair keyPair = ECKeyPair.CreateNew(false, DateTime.UtcNow.ToString());
            eCKeyPairBindingSource.Add(keyPair);
        }
    }
}
