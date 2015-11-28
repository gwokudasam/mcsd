﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Encryption
{
    class Program
    {
        /*
            JUST REMEMBER:
                - SHA1 is for Hashing
                - AES is symmetric
                - RSA is asymmetric (private-public key pair)
        */

        const string dataToProtect = "This is a bunch of super secret content!";
        static byte[] dataToProtectAsArray = Encoding.Unicode.GetBytes(dataToProtect);

        static void Main(string[] args)
        {
            Hashing();
            SymmetricEncryption();
            AsymmetricEncryption();

            Console.ReadKey();
        }

        private static void Hashing()
        {
            // hashing - one-way encryption

            // this represents a hashed password stored in a database
            var storedPasswordHash = new byte[]
                {
                    148, 152, 235, 251, 242, 51, 18, 100, 176, 51, 147, 249, 128, 175, 164, 106, 204, 48, 47, 154, 75,
                    82, 83, 170, 111, 8, 107, 51, 13, 83, 2, 252
                };

            var password = Encoding.Unicode.GetBytes("P4ssw0rd!");
            var passwordHash = SHA256.Create().ComputeHash(password);

            // nice convenience method - can also supply a custom comparator
            if (passwordHash.SequenceEqual(storedPasswordHash))
            {
                Console.WriteLine("Passwords match!");
            }
        }

        private static void SymmetricEncryption()
        {
            // symmetric encryption

            // Uses Rijndael as an algorithm
            // two classes Rijndael and Aes - use Aes (more secure)

            // array of 16 random bytes - must be used for decryption - should be secret
            var key = new byte[] { 12, 2, 56, 117, 12, 67, 33, 23, 12, 2, 56, 117, 12, 67, 33, 23 };

            // another list of 16 bytes - can be shared publically, should be changed for each message exchange
            var initializationVector = new byte[] { 37, 99, 102, 23, 12, 22, 156, 204, 11, 12, 23, 44, 55, 1, 157, 233 };

            byte[] symEncryptedData;

            // save for reuse
            var algorithm = Aes.Create();

            // encrypt
            using (var encryptor = algorithm.CreateEncryptor(key, initializationVector))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(dataToProtectAsArray, 0, dataToProtectAsArray.Length);
                cryptoStream.FlushFinalBlock();
                symEncryptedData = memoryStream.ToArray();
            }

            // decrypt
            byte[] symUnencryptedData;
            using (var decryptor = algorithm.CreateDecryptor(key, initializationVector))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(symEncryptedData, 0, symEncryptedData.Length);
                cryptoStream.FlushFinalBlock();
                symUnencryptedData = memoryStream.ToArray();
            }

            algorithm.Dispose();

            if (dataToProtectAsArray.SequenceEqual(symUnencryptedData))
            {
                Console.WriteLine("Symmetric encrypted values match!");
            }
        }

        private static void AsymmetricEncryption()
        {
            byte[] signature;
            byte[] publicAndPrivateKey;
            byte[] publicKeyOnly;
            var hashImplementation = SHA1.Create();

            // create a signature, create our public and private keys - we could save these out as XML, etc.
            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                signature = rsaProvider.SignData(dataToProtectAsArray, hashImplementation);
                publicAndPrivateKey = rsaProvider.ExportCspBlob(true);
                publicKeyOnly = rsaProvider.ExportCspBlob(false);
            }

            // create a new RSA
            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                // import our public key
                rsaProvider.ImportCspBlob(publicKeyOnly);

                // has it been tampered with?
                if (!rsaProvider.VerifyData(dataToProtectAsArray, hashImplementation, signature))
                {
                    Console.WriteLine("Data has been tampered with");
                }

                // now let's tamper with our data

                dataToProtectAsArray[5] = 255;
                if (!rsaProvider.VerifyData(dataToProtectAsArray, hashImplementation, signature))
                {
                    Console.WriteLine("Data has been tampered with");
                }
            }

            hashImplementation.Dispose();
        }
    }
}
