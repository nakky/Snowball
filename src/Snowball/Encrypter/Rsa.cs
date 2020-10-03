using System;
using System.Security.Cryptography;
using System.Xml;

namespace Snowball
{
    public class RsaEncrypter : IEncrypter
    {
        RSACryptoServiceProvider rsa;

        public RsaEncrypter()
        {
            rsa = new RSACryptoServiceProvider();
        }

        public byte[] Encrypt(byte[] source)
        {
            return rsa.Encrypt(source, false);
        }

        public void FromPublicKeyXmlString(string xmlString)
        {
            RSAParameters parameters = new RSAParameters();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus":
                            parameters.Modulus = Convert.FromBase64String(node.InnerText);
                            break;
                        case "Exponent":
                            parameters.Exponent = Convert.FromBase64String(node.InnerText);
                            break;
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }

            rsa.ImportParameters(parameters);
        }

    }

    public class RsaDecrypter : IDecrypter
    {
        RSACryptoServiceProvider rsa;

        public RsaDecrypter(RSAParameters privateKey)
        {
            rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(privateKey);
        }

        public byte[] Decrypt(byte[] source)
        {
            return rsa.Decrypt(source, false);
        }

        public string ToPublicKeyXmlString()
        {
            RSAParameters parameters = rsa.ExportParameters(false);

            string ret = "";

            try
            {
                ret = string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(parameters.Modulus),
                Convert.ToBase64String(parameters.Exponent)
                );
            }
            catch(Exception e)
            {
                Util.Log("RSA:" + e.Message);
            }

            return ret;
            
        }
    }

}
