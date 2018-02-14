// This file is part of MicroCoin.
// 
// Copyright (c) 2018 Peter Nemeth
//
// MicroCoin is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MicroCoin is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MicroCoin. If not, see <http://www.gnu.org/licenses/>.


using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Text;

namespace MicroCoin.Cryptography
{
    public enum CurveType : ushort {
        Empty = 0,
        Secp256K1 = 714,
        Secp384R1 = 715,
        Secp521R1 = 716,
        Sect283K1 = 729
    };
    public class ECKeyPair
    {
        public BigInteger PrivateKey { get; set; }
        public bool Compressed { get; set; }
        public ECPoint PublicKey { get; set; }
        public CurveType CurveType { get; set; }

        public static ECKeyPair CreateNew(bool compressed)
        {            
            SecureRandom secureRandom = new SecureRandom();
            X9ECParameters curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
            ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            ECKeyPairGenerator generator = new ECKeyPairGenerator();
            ECKeyGenerationParameters keygenParams = new ECKeyGenerationParameters(domain, secureRandom);
            generator.Init(keygenParams);
            AsymmetricCipherKeyPair keypair = generator.GenerateKeyPair();
            ECPrivateKeyParameters privParams = (ECPrivateKeyParameters)keypair.Private;
            ECPublicKeyParameters pubParams = (ECPublicKeyParameters)keypair.Public;
            ECKeyPair k = new ECKeyPair
            {
                CurveType = CurveType.Secp256K1,
                PrivateKey = privParams.D,
                Compressed = compressed
            };
            if (compressed)
            {
                ECPoint q = pubParams.Q;
                k.PublicKey = new FpPoint(domain.Curve, q.AffineXCoord, q.AffineYCoord, true);

            }
            else
            {
                k.PublicKey = pubParams.Q;
            }
            return k;
        }

        public void SaveToStream(Stream s, bool writeLength=true)
        {
            using(BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                int len = PublicKey.AffineXCoord.GetEncoded().Length + PublicKey.AffineYCoord.GetEncoded().Length + 6;                
                if(writeLength) bw.Write((ushort)len);
                bw.Write((ushort)CurveType);
                bw.Write((ushort)PublicKey.AffineXCoord.GetEncoded().Length);
                bw.Write(PublicKey.AffineXCoord.GetEncoded());
                bw.Write((ushort)PublicKey.AffineYCoord.GetEncoded().Length);
                bw.Write(PublicKey.AffineYCoord.GetEncoded());
                
            }
        }

        public void LoadFromStream(Stream stream, bool doubleLen = true)
        {
            using(BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                if (doubleLen)
                {
                    ushort len = br.ReadUInt16();
                    if (len == 0) return;
                }
                CurveType = (CurveType) br.ReadUInt16();
                ushort xLen = br.ReadUInt16();
                byte[] xKey = br.ReadBytes(xLen);
                ushort yLen = br.ReadUInt16();
                byte[] yKey = br.ReadBytes(yLen);
                X9ECParameters curve;
                if (CurveType == CurveType.Secp521R1)
                {

                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp521r1");

                }
                else if (CurveType == CurveType.Secp384R1)
                {
                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp384r1");

                }
                else if (CurveType == CurveType.Sect283K1)
                {
                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("sect283k1");
                    F2mCurve c1 = (F2mCurve)curve.Curve;
                    PublicKey = new FpPoint(c1, new F2mFieldElement(c1.M, c1.K1, new BigInteger(+1, xKey)),
                        new F2mFieldElement(c1.M, c1.K1, new BigInteger(+1, yKey)));                    
                    return;
                }
                else if (CurveType == CurveType.Secp256K1)
                {
                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
                }
                else if (CurveType != 0)
                {
                    throw new Exception($"{CurveType} TYPE");
                }
                else
                {
                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
                }
                FpCurve c = (FpCurve)curve.Curve;
                PublicKey = new FpPoint(c, new FpFieldElement(c.Q, new BigInteger(+1, xKey)),
                    new FpFieldElement(c.Q, new BigInteger(+1, yKey)));
            }
        }

        
    }
}