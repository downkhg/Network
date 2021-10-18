using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Serialization
{
    static class OpenSSL
    {

        public enum DesType
        {
            Encrypt = 0,    // 암호화
            Decrypt = 1     // 복호화
        }

        public static string DES(string key, DesType type, string input)
        {
            byte[] keyBuffer = GetKeyBuffer(key);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider()
            {
                Key = keyBuffer,
                IV = keyBuffer
            };

            MemoryStream ms = new MemoryStream();

            // 익명 타입으로 transform / data 정의
            var property = new
            {
                transform = type.Equals(DesType.Encrypt) ? des.CreateEncryptor() : des.CreateDecryptor(),
                data = type.Equals(DesType.Encrypt) ? Encoding.UTF8.GetBytes(input.ToCharArray()) : Convert.FromBase64String(input)
            };

            CryptoStream cryStream = new CryptoStream(ms, property.transform, CryptoStreamMode.Write);
            byte[] data = property.data;

            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            return type.Equals(DesType.Encrypt) ? Convert.ToBase64String(ms.ToArray()) : Encoding.UTF8.GetString(ms.GetBuffer());
        }

        public static byte[] GetKeyBuffer(string key)
        {
            return ASCIIEncoding.ASCII.GetBytes(key);
        }

        public static void Sample()
        {
            string strKey = "test1234";

            string strEncrypt = OpenSSL.DES(strKey, DesType.Encrypt, "gdtc2021");
            Console.WriteLine("Encypt:" + strEncrypt);
            string strDecrypt = OpenSSL.DES(strKey, DesType.Decrypt, strEncrypt); ;
            Console.WriteLine("Decrypt:" + strDecrypt);
        }
    }
}
