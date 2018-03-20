using System.ComponentModel;
using System.Linq;
using DevExpress.XtraEditors;
using MicroCoin;
using MicroCoin.Chain;

namespace Wallet.Views
{
    public partial class BlockExplorer : XtraForm
    {
        public BlockExplorer()
        {
            InitializeComponent();

            blockBindingSource.DataSource = new BindingList<Block>(
                    Node.Instance.BlockChain.Get(0, (uint)Node.Instance.BlockChain.BlockHeight())
                        .OrderByDescending(p => p.BlockNumber).ToList());
        }
    }
}
