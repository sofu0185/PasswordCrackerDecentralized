using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Common;

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
        /// <param name="wordsChunk">A chunk of words from the dictionary</param>
        /// <param name="encryptedPasswordsBase64">List of names and encrypted password pairs</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        public (bool, List<UserInfo>) CheckWords(List<string> wordsChunk, List<UserInfo> encryptedPasswordsBase64)
        {
            List<(UserInfo, byte[])> encryptedPasswords = new List<(UserInfo, byte[])>();
            foreach (UserInfo encryptedPasswordBase64 in encryptedPasswordsBase64)
            {
                encryptedPasswords.Add((encryptedPasswordBase64, Convert.FromBase64String(encryptedPasswordBase64.HashedPassword)));
            }

            List<UserInfo> crackedPasswords = new List<UserInfo>();

            foreach (string dictionaryEntry in wordsChunk)
            {
                String possiblePassword = dictionaryEntry.ToLower();
                (bool, List<UserInfo>) partialResult = CheckSingleVariations(encryptedPasswords, possiblePassword);
                if (partialResult.Item1)
                {
                    crackedPasswords.AddRange(partialResult.Item2);
                }

                String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
                (bool, List<UserInfo>) partialResultUpperCase = CheckSingleVariations(encryptedPasswords, possiblePasswordUpperCase);
                if (partialResultUpperCase.Item1)
                {
                    crackedPasswords.AddRange(partialResultUpperCase.Item2);
                }

                String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
                (bool, List<UserInfo>) partialResultCapitalized = CheckSingleVariations(encryptedPasswords, possiblePasswordCapitalized);
                if (partialResultCapitalized.Item1)
                {
                    crackedPasswords.AddRange(partialResultCapitalized.Item2);
                }

                String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
                (bool, List<UserInfo>) partialResultReverse = CheckSingleVariations(encryptedPasswords, possiblePasswordReverse);
                if (partialResultReverse.Item1)
                {
                    crackedPasswords.AddRange(partialResultReverse.Item2);
                }

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordEndDigit = dictionaryEntry + i;
                    (bool, List<UserInfo>) partialResultEndDigit = CheckSingleVariations(encryptedPasswords, possiblePasswordEndDigit);
                    if (partialResultEndDigit.Item1)
                    {
                        crackedPasswords.AddRange(partialResultEndDigit.Item2);
                    }
                }

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordStartDigit = i + dictionaryEntry;
                    (bool, List<UserInfo>) partialResultStartDigit = CheckSingleVariations(encryptedPasswords, possiblePasswordStartDigit);
                    if (partialResultStartDigit.Item1)
                    {
                        crackedPasswords.AddRange(partialResultStartDigit.Item2);
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                        (bool, List<UserInfo>) partialResultStartEndDigit = CheckSingleVariations(encryptedPasswords, possiblePasswordStartEndDigit);
                        if (partialResultStartEndDigit.Item1)
                        {
                            crackedPasswords.AddRange(partialResultStartEndDigit.Item2);
                        }
                    }
                }
            }
            bool crackSucceded = crackedPasswords.Count > 0;
            return (crackSucceded, crackedPasswords);
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="encryptedPasswords">List of encrypted passwords from the password file</param>
        /// <param name="possiblePassword"></param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private (bool, List<UserInfo>) CheckSingleVariations(List<(UserInfo, byte[])> encryptedPasswords, string possiblePassword)
        {
            //HASH VALUE
            byte[] possibleEncryptedPasswordByte = HashPossiblePassword(possiblePassword);

            bool passwordFound = false;
            List<UserInfo> crackedPaswords = new List<UserInfo>();

            foreach((UserInfo userInfo, byte[] passByte) encryptedPassword in encryptedPasswords)
            {
                if (CompareBytes(encryptedPassword.passByte, possibleEncryptedPasswordByte))  //compares byte arrays
                {
                    encryptedPassword.userInfo.PlainTextPassword = possiblePassword;
                    crackedPaswords.Add(encryptedPassword.userInfo);
                    passwordFound = true;
                }
            }
            return (passwordFound, crackedPaswords);
        }

        private byte[] HashPossiblePassword(string possiblePassword)
        {
            char[] charArray = possiblePassword.ToCharArray();
            byte[] passwordAsBytes = Array.ConvertAll(charArray, Converter);
            return _messageDigest.ComputeHash(passwordAsBytes);
        }

        /// <summary>
        /// Compares to byte arrays. Encrypted words are byte arrays
        /// </summary>
        /// <param name="firstArray"></param>
        /// <param name="secondArray"></param>
        /// <returns></returns>
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
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
