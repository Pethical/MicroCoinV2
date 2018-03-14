//-----------------------------------------------------------------------
// This file is part of MicroCoin - The first hungarian cryptocurrency
// Copyright (c) 2018 Peter Nemeth
// ECKeyPair.cs - Copyright (c) 2018 Németh Péter
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
        public Hash X { get; set; }
        public Hash Y { get; set; }
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
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public string ToEncodedString()
        {
            return "";
        }

        public CurveType CurveType { get; set; } = CurveType.Empty;
        public byte[] D { get; set; }
        public BigInteger PrivateKey
        {
            get
            {
                return new BigInteger(D);
            }
            set
            {
                D = value.ToByteArray();
            }
        }

        public ByteString Name { get; set; }

        public void SaveToStream(Stream s, bool writeLength = true)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                int len = 0;
                if (X == null || Y == null)
                {
                    len = 0;
                }
                else
                {
                    len = X.Length + Y.Length + 6;
                }
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

        public void LoadFromStream(Stream stream, bool doubleLen = true, bool readPrivateKey = false)
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
                if (readPrivateKey)
                {
                    D = Hash.ReadFromStream(br);
                }
            }
        }

        public bool Equals(ECKeyPair other)
        {
            if (!X.SequenceEqual(other.X)) return false;
            if (!Y.SequenceEqual(other.Y)) return false;
            return true;
        }

        public void DecriptKey(ByteString password)
        {
            byte[] b = new byte[32];
            var salt = D.Skip(8).Take(8).ToArray();
            SHA256Managed managed = new SHA256Managed();
            managed.Initialize();
            var offset = managed.TransformBlock(password, 0, password.Length, b, 0);
            managed.TransformFinalBlock(salt, 0, salt.Length);
            var digest = managed.Hash;
            managed.Dispose();
            managed = new SHA256Managed();
            managed.Initialize();
            managed.TransformBlock(digest, 0, digest.Length, b, 0);
            managed.TransformBlock(password, 0, password.Length, b, 0);
            salt = D.Skip(8).Take(8).ToArray();
            managed.TransformFinalBlock(salt, 0, salt.Length);
            var iv = managed.Hash;
            managed.Dispose();           
            RijndaelManaged aesEncryption = new RijndaelManaged();            
            aesEncryption.KeySize = 256;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            byte[] encryptedBytes = D.Skip(16).ToArray();//Crazy Salt...
            aesEncryption.IV = iv.Take(16).ToArray();
            aesEncryption.Key = digest;
            ICryptoTransform decrypto = aesEncryption.CreateDecryptor();            
            Hash hash = decrypto.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            ByteString bs = hash;
            Hash h2 = bs.ToString(); // dirty hack
            D = h2;
        }
    }
}
