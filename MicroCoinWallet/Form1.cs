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
