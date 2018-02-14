using System;

namespace MicroCoin.Util
{

    public struct Timestamp
    {
        private uint UnixTimestamp;
        public Timestamp(uint unixTimestamp)
        {
            UnixTimestamp = unixTimestamp;
        }
        public static implicit operator Timestamp(DateTime dt)
        {
            return new Timestamp((uint)dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static implicit operator DateTime(Timestamp t)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);            
            dtDateTime = dtDateTime.AddSeconds(t.UnixTimestamp).ToLocalTime();
            return dtDateTime;
        }

        public static implicit operator Timestamp(uint dt)
        {
            return new Timestamp(dt);
        }

        public static implicit operator UInt32(Timestamp t)
        {
            return t.UnixTimestamp;
        }

    }

}