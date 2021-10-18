using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Serialization
{
    [Serializable]
    class NameCard
    {
        public string Name;
        public string Phone;
        public int Age;
    }

    class Program
    {
        static void FileTestMain()//직렬화 파일쓰기 및 복호화
        {
            Stream writeStream = new FileStream("a.dat", FileMode.Create);
            BinaryFormatter serializer = new BinaryFormatter();

            NameCard cNameCard = new NameCard();
            cNameCard.Name = "test";
            cNameCard.Phone = "010-1234-4567";
            cNameCard.Age = 22;

            serializer.Serialize(writeStream, cNameCard);
            writeStream.Close();

            Stream readStream = new FileStream("a.dat", FileMode.Open);
            BinaryFormatter deserializer = new BinaryFormatter();

            NameCard cNameCardCopy;
            cNameCardCopy = (NameCard)deserializer.Deserialize(readStream);
            readStream.Close();

            Console.WriteLine("Name:  {0}", cNameCardCopy.Name);
            Console.WriteLine("Phone: {0}", cNameCardCopy.Phone);
            Console.WriteLine("Age:   {0}", cNameCardCopy.Age);
        }

        static void BufferTestMain()//메모리 직렬화 및 복호화
        {
            BinaryFormatter serializer = new BinaryFormatter();

            NameCard cNameCard = new NameCard();
            cNameCard.Name = "test2222";
            cNameCard.Phone = "010-1234-4567";
            cNameCard.Age = 22;

            MemoryStream memoryStream = new MemoryStream();
        
            serializer.Serialize(memoryStream, cNameCard);
            byte[] buffuer = memoryStream.ToArray();

            MemoryStream memoryStreamCopy = new MemoryStream();
            BinaryFormatter deserializer = new BinaryFormatter();

            memoryStreamCopy.Write(buffuer, 0, buffuer.Length);
            memoryStreamCopy.Seek(0, SeekOrigin.Begin);
            NameCard cNameCardCopy;
            cNameCardCopy = (NameCard)deserializer.Deserialize(memoryStreamCopy);

            Console.WriteLine("Name:  {0}", cNameCardCopy.Name);
            Console.WriteLine("Phone: {0}", cNameCardCopy.Phone);
            Console.WriteLine("Age:   {0}", cNameCardCopy.Age);
        }

        static void Main(string[] args)
        {
            //FileTestMain();
            //BufferTestMain();
            //CryptionTest.CryptionTestMain();//오류있음. 샘플코드가 정상적으로 작동하지않음.
            //CryptionTest.DESCryptoTestMain();//DES암호화 적용 샘플
            OpenSSL.Sample();
        }
    }
}
