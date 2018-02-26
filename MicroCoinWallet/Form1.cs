////////////////////////////////////////////////////////////////////////
//                   This file is part of MicroCoin.
//             Copyright (c) 2018 Peter Nemeth
////////////////////////////////////////////////////////////////////////
// 			   Copyright (c) 2018 p3thi
////////////////////////////////////////////////////////////////////////
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.
using MicroCoin.Chain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroCoinWallet
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (!SystemInformation.TerminalServerSession)
            {
                Type dgvType = dataGridView1.GetType();
                PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                  BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(dataGridView1, true, null);
            }
            if (!SystemInformation.TerminalServerSession)
            {
                Type dgvType = dataGridView2.GetType();
                PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                  BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(dataGridView2, true, null);
            }

            await MicroCoin.Node.StartNode();            
            dataGridView2.DataSource = new BindingList<AccountView>(CheckPoints.Accounts.Select(p => new AccountView
            {
                AccountNumber = p.AccountNumber,
                Name = p.Name,
                Balance = p.Balance,
                State = p.AccountInfo.State
            }).ToList());
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MicroCoin.Node.Instance.Dispose();
        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void operationExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
    class AccountView
    {
        public uint AccountNumber { get; set; }
        public string Name { get; set; }
        public ulong Balance { get; set; }
        public AccountInfo.AccountState State { get; set; }
    }

}
