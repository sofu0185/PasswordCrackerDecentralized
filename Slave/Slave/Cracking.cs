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
        /// <param name="wordsChunk">A chunk of words from the dictionary</param>
        /// <param name="encryptedPasswordsBase64">List of names and encrypted password pairs</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        public (bool, List<(string name, string pass)>) CheckWords(List<string> wordsChunk, List<(string name, string pass)> encryptedPasswordsBase64)
        {
            List<(string, byte[])> encryptedPasswords = new List<(string, byte[])>();
            foreach ((string name, string pass) encryptedPasswordBase64 in encryptedPasswordsBase64)
            {
                encryptedPasswords.Add((encryptedPasswordBase64.name, Convert.FromBase64String(encryptedPasswordBase64.pass)));
            }

            List<(string, string)> crackedPasswords = new List<(string, string)>();

            foreach (string dictionaryEntry in wordsChunk)
            {
                String possiblePassword = dictionaryEntry.ToLower();
                (bool, List<(string, string)>) partialResult = CheckSingleVariations(encryptedPasswords, possiblePassword);
                if (partialResult.Item1)
                {
                    crackedPasswords.AddRange(partialResult.Item2);
                }

                String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
                (bool, List<(string, string)>) partialResultUpperCase = CheckSingleVariations(encryptedPasswords, possiblePasswordUpperCase);
                if (partialResultUpperCase.Item1)
                {
                    crackedPasswords.AddRange(partialResultUpperCase.Item2);
                }

                String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
                (bool, List<(string, string)>) partialResultCapitalized = CheckSingleVariations(encryptedPasswords, possiblePasswordCapitalized);
                if (partialResultCapitalized.Item1)
                {
                    crackedPasswords.AddRange(partialResultCapitalized.Item2);
                }

                String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
                (bool, List<(string, string)>) partialResultReverse = CheckSingleVariations(encryptedPasswords, possiblePasswordReverse);
                if (partialResultReverse.Item1)
                {
                    crackedPasswords.AddRange(partialResultReverse.Item2);
                }

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordEndDigit = dictionaryEntry + i;
                    (bool, List<(string, string)>) partialResultEndDigit = CheckSingleVariations(encryptedPasswords, possiblePasswordEndDigit);
                    if (partialResultEndDigit.Item1)
                    {
                        crackedPasswords.AddRange(partialResultEndDigit.Item2);
                    }
                }

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordStartDigit = i + dictionaryEntry;
                    (bool, List<(string, string)>) partialResultStartDigit = CheckSingleVariations(encryptedPasswords, possiblePasswordStartDigit);
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
                        (bool, List<(string, string)>) partialResultStartEndDigit = CheckSingleVariations(encryptedPasswords, possiblePasswordStartEndDigit);
                        if (partialResultStartEndDigit.Item1)
                        {
                            crackedPasswords.AddRange(partialResultStartEndDigit.Item2);
                        }
                    }
                }
            }
            bool crackSucceded = crackedPasswords.Count != 0;
            return (crackSucceded, crackedPasswords);
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="encryptedPasswords">List of encrypted passwords from the password file</param>
        /// <param name="possiblePassword"></param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private (bool, List<(string, string)>) CheckSingleVariations(List<(string, byte[])> encryptedPasswords, String possiblePassword)
        {
            //HASH VALUE
            byte[] possibleEncryptedPasswordByte = HashPossiblePassword(possiblePassword);

            bool passwordFound = false;
            List<(string, string)> crackedPaswords = new List<(string, string)>();

            foreach((string name, byte[] passByte) encryptedPassword in encryptedPasswords)
            {
                if (CompareBytes(encryptedPassword.passByte, possibleEncryptedPasswordByte))  //compares byte arrays
                {
                    crackedPaswords.Add((encryptedPassword.name, possiblePassword));
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
