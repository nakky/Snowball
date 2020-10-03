using System;
using System.Security.Cryptography;

namespace Snowball
{
    [Transferable]
    public class AesKeyPair
    {
        public AesKeyPair() { }

        [Data(0)]
        public byte[] Key { get; set; }
        [Data(1)]
        public byte[] IV { get; set; }
    }

    public class AesEncrypter : IEncrypter
    {
        public Aes Aes { get; private set; }

        public AesEncrypter(Aes aes)
        {
            Aes = aes;
        }

        public byte[] Encrypt(byte[] source)
        {
            using (ICryptoTransform encrypt = Aes.CreateEncryptor())
            {
                return encrypt.TransformFinalBlock(source, 0, source.Length);
            }
        }
    }

    public class AesDecrypter : IDecrypter
    {
        public Aes Aes { get; private set; }

        public AesDecrypter(Aes aes)
        {
            Aes = aes;
        }

        public byte[] Decrypt(byte[] source)
        {
            using (ICryptoTransform encrypt = Aes.CreateDecryptor())
            {
                return encrypt.TransformFinalBlock(source, 0, source.Length);
            }
        }

    }

}
