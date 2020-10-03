using System;
namespace Snowball
{
    public interface IEncrypter
    {
        byte[] Encrypt(byte[] source);
    }

    public interface IDecrypter
    {
        byte[] Decrypt(byte[] source);
    }
}
