﻿//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// Block.cs - Copyright (c) 2018 Németh Péter
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


using MicroCoin.Transactions;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroCoin.Chain
{
    /// <summary>
    /// Block with transaction list.
    /// </summary>
    public class Block : BlockBase
    {
        public uint TransactionCount { get; set; }
        internal TransactionType TransactionsType { get; set; }
        public List<Transaction> Transactions {get; set;}  = new List<Transaction>();
        public new static Block GenesisBlock => new Block
        {
            BlockNumber = 0
        };

        public Block() 
        {
            BlockSignature = 4;
        }

        public bool BlockIsValid()
        {
            if (Reward < 0) return false;
            if (Fee < 0) return false;
            return true;
        }

        internal Block(Stream s) : base(s)
        {            
            if (BlockSignature==1 || BlockSignature == 3)
            {
                return;
            }
            using (var br = new BinaryReader(s, Encoding.Default, true))
            {
                TransactionCount = br.ReadUInt32();
                if (TransactionCount <= 0) return;
                Transactions = new List<Transaction>();
                for (var i = 0; i < TransactionCount; i++)
                {
                    TransactionsType = (TransactionType)br.ReadUInt32();
                    Transaction t;
                    switch (TransactionsType)
                    {
                        case TransactionType.Transaction:
                        case TransactionType.BuyAccount:
                            t = new TransferTransaction(s);
                            break;
                        case TransactionType.ChangeKey:
                        case TransactionType.ChangeKeySigned:
                            t = new ChangeKeyTransaction(s, TransactionsType);
                            break;
                        case TransactionType.ListAccountForSale:
                        case TransactionType.DeListAccountForSale:
                            t = new ListAccountTransaction(s);
                            break;
                        case TransactionType.ChangeAccountInfo:
                            t = new ChangeAccountInfoTransaction(s);
                            break;
                        default:
                            s.Position = s.Length;
                            return;
                    }
                    t.TransactionType = TransactionsType;
                    Transactions.Add(t);
                }
            }
        }
        internal override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            if (BlockSignature == 1 || BlockSignature == 3)
            {
                return;
            }
            using (var bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                if (Transactions == null)
                {
                    bw.Write((uint)0);
                    bw.Write((uint)TransactionsType);
                    return;
                }
                bw.Write((uint)Transactions.Count);
                foreach (var t in Transactions)
                {
                    bw.Write((uint)t.TransactionType);
                    t.SaveToStream(s);
                }
            }
        }
    }
}