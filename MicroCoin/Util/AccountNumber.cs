//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// AccountNumber.cs - Copyright (c) 2018 Németh Péter
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

namespace MicroCoin.Util
{
    public struct AccountNumber : IEquatable<object>, IEquatable<uint>, IEquatable<string>
    {

        private uint value;

        public AccountNumber(string value) {
            if (value.Contains("-"))
            {
                this.value = Convert.ToUInt32(value.Split('-')[0]);
            }
            else
            {
                this.value = Convert.ToUInt32(value);
            }
        }

        public AccountNumber(uint value) {
            this.value = value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public bool Equals(uint obj)
        {
            return value.Equals(obj);
        }

        public static bool operator ==(AccountNumber an, AccountNumber an2)
        {
            return an.value == an2.value;
        }

        public static bool operator !=(AccountNumber an, AccountNumber an2)
        {
            return an.value != an2.value;
        }

        public static bool operator ==(AccountNumber an, string an2)
        {
            return an.Equals(an2);
        }

        public static bool operator !=(AccountNumber an, string an2)
        {
            return !an.Equals(an2);
        }


        public override bool Equals(object obj)
        {
            return value.Equals(obj);

        }

        override public string ToString()
        {
            var checksum = ((value * 101) % 89) + 10;
            return value.ToString() + '-' + checksum;
        }

        public bool Equals(string other)
        {
            AccountNumber an = new AccountNumber(other);
            return an.value == value;
        }

        public static implicit operator AccountNumber(string s)
        {
            return new AccountNumber(s);
        }

        public static implicit operator AccountNumber(uint s)
        {
            return new AccountNumber(s);
        }

        public static implicit operator UInt32(AccountNumber number)
        {
            return number.value;
        }
        public static implicit operator Int32(AccountNumber number)
        {
            return (int) number.value;
        }

    }
}
