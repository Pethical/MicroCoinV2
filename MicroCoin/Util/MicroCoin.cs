using System;
using System.IO;
using System.Text;

namespace MicroCoin.Util
{
    public struct MicroCoin
    {
        public decimal value { get; set; }

        public MicroCoin(decimal value)
        {
            this.value = value;
        }

        public static implicit operator ulong(MicroCoin m)
        {
            return (ulong) (m.value * 10000M);
        }

        public static implicit operator decimal(MicroCoin m)
        {
            return m.value;
        }

        public static implicit operator MicroCoin(ulong m)
        {
            return new MicroCoin( m / 10000M );
        }
        public static MicroCoin operator +(MicroCoin a, MicroCoin b)
        {
            return new MicroCoin(a.value + b.value);
        }
        public static MicroCoin operator -(MicroCoin a, MicroCoin b)
        {
            return new MicroCoin(a.value - b.value);
        }
        public static MicroCoin operator -(MicroCoin a, ulong b)
        {
            return new MicroCoin(a.value - b);
        }
        public static MicroCoin operator +(MicroCoin a, ulong b)
        {
            return new MicroCoin(a.value + b);
        }
        public override bool Equals(object obj)
        {
            return ((ulong)this).Equals(obj);
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
}