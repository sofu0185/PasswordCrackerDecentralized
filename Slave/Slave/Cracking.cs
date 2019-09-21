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
        public List<UserInfo> CheckWords(string[] wordsChunk, List<UserInfo> encryptedPasswordsBase64)
        {
            List<(UserInfo, byte[])> encryptedPasswords = new List<(UserInfo, byte[])>();
            foreach (UserInfo encryptedPasswordBase64 in encryptedPasswordsBase64)
            {
                encryptedPasswords.Add((encryptedPasswordBase64, Convert.FromBase64String(encryptedPasswordBase64.HashedPassword)));
            }

            List<UserInfo> crackedPasswords = new List<UserInfo>();

            for(int chunkIndex = 0; chunkIndex < wordsChunk.Length; chunkIndex++)
            {
                string dictionaryEntry = wordsChunk[chunkIndex];
                // Check for exact match
                String possiblePassword = dictionaryEntry;
                crackedPasswords.AddRange(CheckSingleVariations(encryptedPasswords, possiblePassword));

                // Check for uppercase
                String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
                crackedPasswords.AddRange(CheckSingleVariations(encryptedPasswords, possiblePasswordUpperCase));

                // Check for capitalization
                String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
                crackedPasswords.AddRange(CheckSingleVariations(encryptedPasswords, possiblePasswordCapitalized));

                // Check for reverse
                String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
                crackedPasswords.AddRange(CheckSingleVariations(encryptedPasswords, possiblePasswordReverse));

                // Check for end digits between 0 and 99
                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordEndDigit = dictionaryEntry + i;
                    crackedPasswords.AddRange(CheckSingleVariations(encryptedPasswords, possiblePasswordEndDigit));
                }

                // Check for leading digits between 0 and 99
                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordStartDigit = i + dictionaryEntry;
                    crackedPasswords.AddRange(CheckSingleVariations(encryptedPasswords, possiblePasswordStartDigit));
                }

                // Check for leading and end digits between 0 and 9
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                        crackedPasswords.AddRange(CheckSingleVariations(encryptedPasswords, possiblePasswordStartEndDigit));
                    }
                }
            }
            return crackedPasswords;
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="encryptedPasswords">List of encrypted passwords from the password file</param>
        /// <param name="possiblePassword"></param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private List<UserInfo> CheckSingleVariations(List<(UserInfo, byte[])> encryptedPasswords, string possiblePassword)
        {
            //HASH VALUE
            byte[] possibleEncryptedPasswordByte = HashPossiblePassword(possiblePassword);

            List<UserInfo> crackedPaswords = new List<UserInfo>();

            foreach ((UserInfo userInfo, byte[] passByte) encryptedPassword in encryptedPasswords)
            {
                if (CompareBytesArrays(encryptedPassword.passByte, possibleEncryptedPasswordByte))  //compares byte arrays
                {
                    encryptedPassword.userInfo.PlainTextPassword = possiblePassword;
                    crackedPaswords.Add(encryptedPassword.userInfo);
                }
            }
            return crackedPaswords;
        }

        private byte[] HashPossiblePassword(string possiblePassword)
        {
            byte[] passwordAsBytes = Encoding.ASCII.GetBytes(possiblePassword);
            return _messageDigest.ComputeHash(passwordAsBytes);
        }

        /// <summary>
        /// Compares to byte arrays. Encrypted words are byte arrays
        /// </summary>
        /// <param name="firstArray"></param>
        /// <param name="secondArray"></param>
        /// <returns></returns>
        private bool CompareBytesArrays(byte[] firstArray, byte[] secondArray)
        {
            if (firstArray.Length != secondArray.Length)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Length; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }

    }
}
