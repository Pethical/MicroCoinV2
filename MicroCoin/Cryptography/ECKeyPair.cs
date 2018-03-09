using MicroCoin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MicroCoin.Cryptography
{
    public class ECKeyPair : IEquatable<ECKeyPair>
    {
        public byte[] X { get; set; }
        public byte[] Y { get; set; }
        public ECPoint PublicKey
        {
            get
            {
                return new ECPoint
                {
                    X = X,
                    Y = Y
                };
            }
        }
        public CurveType CurveType { get; set; }
        public byte[] D { get; set; }
        public BigInteger PrivateKey
        {
            get
            {
                return new BigInteger(D);
            }
        }

        public void SaveToStream(Stream s, bool writeLength = true)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                int len = X.Length + Y.Length + 6;
                if (writeLength) bw.Write((ushort)len);
                bw.Write((ushort)CurveType);
                if (CurveType == CurveType.Empty)
                {
                    bw.Write((ushort)0);
                    bw.Write((ushort)0);
                    return;
                }
                ushort xLen = (ushort)X.Length;
                byte[] x = X;                
                if (x[0] == 0) xLen--;
                bw.Write((ushort)xLen);
                bw.Write(x, x[0] == 0 ? 1 : 0, x.Length - (x[0] == 0 ? 1 : 0));
                ushort yLen;
                if (CurveType == CurveType.Sect283K1)
                {
                    byte[] b = Y;
                    yLen = (ushort)Y.Length;
                    if (b[0] == 0) yLen--;
                    bw.Write((ushort)yLen);
                    bw.Write(b, b[0] == 0 ? 1 : 0, b.Length - (b[0] == 0 ? 1 : 0));
                }
                else
                {
                    byte[] b = Y;
                    yLen = (ushort)Y.Length;
                    if (b[0] == 0) yLen--;
                    bw.Write((ushort)yLen);
                    bw.Write(b, b[0] == 0 ? 1 : 0, b.Length - (b[0] == 0 ? 1 : 0));
                }
            }
        }

        public bool ValidateSignature(byte[] data, ECSig signature)
        {
            ECCurve curve = ECCurve.CreateFromFriendlyName("secp256k1");
            ECParameters parameters = new ECParameters();
            parameters.Q.X = X;
            parameters.Q.Y = Y;
            parameters.Curve = curve;
            parameters.Validate();
            ECDsa ecdsa = ECDsa.Create(parameters);            
            bool ok = ecdsa.VerifyHash(data, signature.Signature);
            return ok;
        }

        internal static ECKeyPair CreateNew(bool v)
        {
            ECCurve curve = ECCurve.CreateFromFriendlyName("secp256k1");
            var ecdsa = ECDsa.Create(curve);
            ECParameters parameters = ecdsa.ExportParameters(true);
            ECKeyPair pair = new ECKeyPair();
            pair.CurveType = CurveType.Secp256K1;
            pair.X = parameters.Q.X;
            pair.Y = parameters.Q.Y;
            pair.D = parameters.D;
            return pair;
        }

        public void LoadFromStream(Stream stream, bool doubleLen = true)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                if (doubleLen)
                {
                    ushort len = br.ReadUInt16();
                    if (len == 0) return;
                }
                CurveType = (CurveType)br.ReadUInt16();
                ushort xLen = br.ReadUInt16();
                X = br.ReadBytes(xLen);
                ushort yLen = br.ReadUInt16();
                Y = br.ReadBytes(yLen);
            }
        }

        public bool Equals(ECKeyPair other)
        {
            if (!X.SequenceEqual(other.X)) return false;
            if (!Y.SequenceEqual(other.Y)) return false;
            return true;
        }
    }
}
