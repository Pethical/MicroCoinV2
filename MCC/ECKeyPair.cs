using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using System.IO;
using System.Text;
using System;
using System.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;

namespace MCC
{

    public class ECSig
    {
        public byte[] r { get; set; }
        public byte[] s { get; set; }

        public ECSig() { }
        public ECSig(Stream stream) {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                ushort len = br.ReadUInt16();
                r = new byte[len];
                br.Read(r, 0, len);
                len = br.ReadUInt16();
                s = new byte[len];
                br.Read(s, 0, len);
            }
        }

        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write((ushort)r.Length);
                bw.Write(r);
                bw.Write((ushort)s.Length);
                bw.Write(s);
            }
        }
    }

    public class ECKeyPair
    {
        public BigInteger priv;
        public bool compressed;
        public ECPoint pub;

        public static ECKeyPair createNew(bool compressed)
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
            ECKeyPair k = new ECKeyPair();
            k.priv = privParams.D;
            k.compressed = compressed;
            if (compressed)
            {
                ECPoint q = pubParams.Q;
                k.pub = new FpPoint(domain.Curve, q.X, q.Y, true);

            }
            else
            {
                k.pub = pubParams.Q;
            }
            return k;
        }

        public void SaveToStream(Stream s, bool WriteLength=true)
        {
            using(BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                BigInteger bg = pub.X.ToBigInteger();                
                int len = pub.X.GetEncoded().Length + pub.Y.GetEncoded().Length + 6;                
                if(WriteLength) bw.Write((ushort)len);
                bw.Write((ushort)714);
                bw.Write((ushort)pub.X.GetEncoded().Length);
                bw.Write(pub.X.GetEncoded());
                bw.Write((ushort)pub.Y.GetEncoded().Length);
                bw.Write(pub.Y.GetEncoded());
                
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
                ushort type = br.ReadUInt16();
                ushort xLen = br.ReadUInt16();
                byte[] xKey = br.ReadBytes(xLen);
                ushort yLen = br.ReadUInt16();
                byte[] yKey = br.ReadBytes(yLen);
                X9ECParameters curve;
                if (type == 716)
                {

                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp521r1");
                    
                } else if (type == 715)
                {
                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp384r1");
                    
                }
                else if (type == 729)
                {
                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("sect283k1");
                    F2mCurve c1 = (F2mCurve)curve.Curve;
                    pub = new FpPoint(c1, new F2mFieldElement(c1.M, c1.K1, new BigInteger(+1, xKey)),
                        new F2mFieldElement(c1.M, c1.K1, new BigInteger(+1, yKey)))
                        ;
                    //pub = new FpPoint(c1, new FpFieldElement(c1.H, new BigInteger(+1, xKey)), new FpFieldElement(c1.Q, ));
                    return;
                }
                else
                {
                    curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
                }
                
                FpCurve c = (FpCurve)curve.Curve;                
                pub = new FpPoint(c,new FpFieldElement(c.Q, new BigInteger(+1, xKey)), new FpFieldElement(c.Q, new BigInteger(+1, yKey)));
            }
        }

        
    }
}