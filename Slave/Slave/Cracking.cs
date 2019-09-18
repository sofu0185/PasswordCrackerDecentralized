using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Slave
{
    public class Cracking
    {
        /// <summary>
        /// The algorithm used for encryption.
        /// Must be exactly the same algorithm that was used to encrypt the passwords in the password file
        /// </summary>
        private readonly HashAlgorithm _messageDigest;

        private static readonly Converter<char, byte> Converter = CharToByte;

        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
            //_messageDigest = new MD5CryptoServiceProvider();
            // seems to be same speed
        }




        /// <summary>
        /// Generates a lot of variations, encrypts each of the and compares it to all entries in the password file
        /// </summary>
        /// <param name="dictionaryEntry">A single word from the dictionary</param>
        /// <param name="userInfos">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        public (bool, string) CheckWordsWithVariations(List<string> wordsChunk, string entryptedPasswordBase64)
        {
            byte[] encryptedPassword = Convert.FromBase64String(entryptedPasswordBase64);

            foreach (string dictionaryEntry in wordsChunk)
            {
                String possiblePassword = dictionaryEntry;
                (bool, string) partialResult = CheckSingleWord(encryptedPassword, possiblePassword);
                if (partialResult.Item1)
                {
                    return (true, partialResult.Item2);
                }

                String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
                (bool, string) partialResultUpperCase = CheckSingleWord(encryptedPassword, possiblePasswordUpperCase);
                if (partialResultUpperCase.Item1)
                {
                    return (true, partialResultUpperCase.Item2);
                }

                String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
                (bool, string) partialResultCapitalized = CheckSingleWord(encryptedPassword, possiblePasswordCapitalized);
                if (partialResultCapitalized.Item1)
                {
                    return (true, partialResultCapitalized.Item2);
                }

                String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
                (bool, string) partialResultReverse = CheckSingleWord(encryptedPassword, possiblePasswordReverse);
                if (partialResultReverse.Item1)
                {
                    return (true, partialResultReverse.Item2);
                }

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordEndDigit = dictionaryEntry + i;
                    (bool, string) partialResultEndDigit = CheckSingleWord(encryptedPassword, possiblePasswordEndDigit);
                    if (partialResultEndDigit.Item1)
                    {
                        return (true, partialResultEndDigit.Item2);
                    }
                }

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordStartDigit = i + dictionaryEntry;
                    (bool, string) partialResultStartDigit = CheckSingleWord(encryptedPassword, possiblePasswordStartDigit);
                    if (partialResultStartDigit.Item1)
                    {
                        return (true, partialResultStartDigit.Item2);
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                        (bool, string) partialResultStartEndDigit = CheckSingleWord(encryptedPassword, possiblePasswordStartEndDigit);
                        if (partialResultStartEndDigit.Item1)
                        {
                            return (true, partialResultStartEndDigit.Item2);
                        }
                    }
                }                
            }
            return (false, null);
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="userInfos"></param>
        /// <param name="possiblePassword">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private (bool, string) CheckSingleWord(byte[] encryptedPassword, String possiblePassword)
        {
            char[] charArray = possiblePassword.ToCharArray();
            byte[] passwordAsBytes = Array.ConvertAll(charArray, Converter);

            //HASH VALUE
            byte[] possibleEncryptedPasswordByte = _messageDigest.ComputeHash(passwordAsBytes);
            //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);

            if (CompareBytes(encryptedPassword, possibleEncryptedPasswordByte))  //compares byte arrays
            {
                return (true, possiblePassword);
            }
            else
                return (false, null);
        }

        /// <summary>
        /// Compares to byte arrays. Encrypted words are byte arrays
        /// </summary>
        /// <param name="firstArray"></param>
        /// <param name="secondArray"></param>
        /// <returns></returns>
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("firstArray");
            //}
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("secondArray");
            //}
            if (firstArray.Count != secondArray.Count)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Count; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Converting a char to a byte can be done in many ways.
        /// This is one way ...
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private static byte CharToByte(char ch)
        {
            return Convert.ToByte(ch);
        }
    }
}
