using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ReactCRM.Services
{
    public static class FileEncryptor
    {
        private static readonly byte[] SALT = Encoding.UTF8.GetBytes("ReactCRM_Salt_2024");
        private const string PASSWORD = "ReactCRM_Encryption_Key_2024";

        public static void EncryptFile(string inputFile, string outputFile)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(PASSWORD);
            byte[] saltBytes = SALT;

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                {
                    using (CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                        {
                            byte[] buffer = new byte[1048576];
                            int read;

                            while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
        }

        public static void DecryptFile(string inputFile, string outputFile)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(PASSWORD);
            byte[] saltBytes = SALT;

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    using (CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                        {
                            byte[] buffer = new byte[1048576];
                            int read;

                            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fsOut.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
        }
    }
}