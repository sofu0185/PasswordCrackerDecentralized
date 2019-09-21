using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    /// <summary>
    /// Contains program specific constants
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Represents the size of each word chunk being send to the client
        /// </summary>
        internal const int CHUNK_SIZE = 20_000 ;
        internal const int TCP_SERVER_PORT = 6789;
        internal const string DICTIONARY_PATH = "Text files/webster-dictionary.txt";
        internal const string PASSWORD_PATH = "Text files/passwords.txt";
    }
}
