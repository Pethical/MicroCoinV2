using MicroCoin.Util;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace MicroCoin.Cryptography
{
    public static class Utils
    {
        public static ECSignature GenerateSignature(Hash data, ECKeyPair keyPair)
        {

            using (var eC = ECDsa.Create(keyPair))
            {
                Hash sign = eC.SignHash(data);
                ECSignature ecs = new ECSignature(sign);
                return ecs;
            }

        }

        public static bool ValidateSignature(Hash data, ECSignature signature, ECKeyPair keyPair)
        {
            using (var ecdsa = ECDsa.Create(keyPair))
            {
                return ecdsa.VerifyHash(data, signature.Signature);                
            }
        }
        public static ByteString DecryptString(Hash em, ECKeyPair keyPair)
        {
            ECParameters parameters = new ECParameters();
            byte[] ems = em;
            parameters.Q.X = ems.Skip(1).Take(32).ToArray();
            parameters.Q.Y = ems.Skip(33).Take(32).ToArray();
            parameters.D = keyPair.D;
            parameters.Curve = keyPair.ECParameters.Curve;
            parameters.Validate();
            using (var ecDiffieHellmanCng = new ECDiffieHellmanCng())
            {
                ecDiffieHellmanCng.ImportParameters(parameters);
                var ek = ecDiffieHellmanCng.DeriveKeyFromHash(ecDiffieHellmanCng.PublicKey, HashAlgorithmName.SHA256, null, new byte[] {0, 0, 0, 1});
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.Padding = PaddingMode.PKCS7;
                    var d = aes.CreateDecryptor(ek, new byte[16]);
                    var bs = d.TransformFinalBlock(ems.Skip(65).Take(ems.Length - 65 - 32).ToArray(), 0,
                        ems.Length - 65 - 32);
                    return bs;
                }
            }
        }

        public static Hash DoubleSha256(Hash data)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                Hash h = sha.ComputeHash(data);
                return sha.ComputeHash(h);
            }
        }

        public static Hash EncryptString(ByteString data, ECKeyPair keyPair)
        {
            using (var ecDiffieHellman = ECDiffieHellman.Create(keyPair))
            {
                using (var ephem = ECDiffieHellman.Create(keyPair.ECParameters.Curve))
                {
                    ECParameters ephemPublicParams = ephem.ExportParameters(false);
                    int pointLen = ephemPublicParams.Q.X.Length;
                    byte[] rBar = new byte[pointLen * 2 + 1];
                    rBar[0] = (byte) (keyPair.X.Length + keyPair.Y.Length);
                    Buffer.BlockCopy(ephemPublicParams.Q.X, 0, rBar, 1, pointLen);
                    Buffer.BlockCopy(ephemPublicParams.Q.Y, 0, rBar, 1 + pointLen, pointLen);
                    var ek = ephem.DeriveKeyFromHash(ecDiffieHellman.PublicKey, HashAlgorithmName.SHA256, null,
                        new byte[] {0, 0, 0, 1});
                    var mk = ephem.DeriveKeyFromHash(ecDiffieHellman.PublicKey, HashAlgorithmName.SHA256, null,
                        new byte[] {0, 0, 0, 2});

                    using (RijndaelManaged aes = new RijndaelManaged())
                    {
                        aes.Padding = PaddingMode.PKCS7;
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(ek, new byte[16]))
                        {
                            if (!encryptor.CanTransformMultipleBlocks)
                            {
                                throw new InvalidOperationException();
                            }

                            Hash em = encryptor.TransformFinalBlock(data, 0, data.Length);
                            byte[] da;
                            using (HMAC hmac = new HMACSHA256(mk))
                            {
                                da = hmac.ComputeHash(em);
                            }

                            return rBar.Concat((byte[]) em).Concat(da).ToArray();
                        }
                    }
                }
            }
        }


    }
}
