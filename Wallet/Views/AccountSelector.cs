﻿//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// AccountSelector.cs - Copyright (c) 2018 Németh Péter
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
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MicroCoin;
using MicroCoin.Chain;

namespace Wallet.Views
{
    public partial class AccountSelector : XtraForm
    {
        public Account SelectedAccount => (Account)accountBindingSource.Current;

        public AccountSelector() : this(Node.Instance.Accounts)
        {

        }

        public AccountSelector(IList<Account> accounts)
        {
            InitializeComponent();
            accountBindingSource.DataSource = accounts;
        }

        private void AccountSelector_Load(object sender, EventArgs e)
        {

        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
