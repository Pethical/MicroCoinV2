using System;
using System.IO;
using System.Text;

namespace MicroCoin.Util
{
    public struct MCC
    {
        public decimal value { get; set; }

        public MCC(decimal value)
        {
            this.value = value;
        }

        public static implicit operator ulong(MCC m)
        {
            return (ulong) (m.value * 10000M);
        }

        public static implicit operator decimal(MCC m)
        {
            return m.value;
        }

        public static implicit operator MCC(ulong m)
        {
            return new MCC( m / 10000M );
        }
        public static MCC operator +(MCC a, MCC b)
        {
            return new MCC(a.value + b.value);
        }
        public static MCC operator -(MCC a, MCC b)
        {
            return new MCC(a.value - b.value);
        }
        public static MCC operator -(MCC a, ulong b)
        {
            return new MCC(a.value - b);
        }
        public static MCC operator +(MCC a, ulong b)
        {
            return new MCC(a.value + b);
        }
        public override bool Equals(object obj)
        {
            return ((ulong)this).Equals(obj);
        }
        public override string ToString()
        {
            return value.ToString();
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

    }
}