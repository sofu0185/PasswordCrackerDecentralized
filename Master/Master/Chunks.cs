using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>A class handling inputting a word dictionary in txt format and outputting a splitted dictionary in chunks (list consisting of strings). 
    /// </summary>
    public class Chunks
    {
        private readonly int _chunkSize;
        //private List<List<string>> _chunkList;
        private List<string[]> _chunkArray;

        public List<string> SerializedStringChunks { get; }
        public int CurrenntChunkIndex { get; private set; }
        public int CurrentChunkSize { get => _chunkArray[CurrenntChunkIndex].Length; }
        public bool EndOfChunks { get; private set; }

        public Chunks()
        {
            _chunkSize = Constants.CHUNK_SIZE;
            CurrenntChunkIndex = 0;

            // Read all words from file and split them into chunks
            _chunkArray = SplitIntoChunks(FileHandler.ReadAllWordsInDictionarySpan());

            SerializedStringChunks = SerializeChunks();


            Console.WriteLine("\tChunks ready");
        }

        /// <summary>Splits full list of words from dictionary into chunks (a list consisting of strings)  
        /// </summary>
        public List<string[]> SplitIntoChunks(Span<string> listOfWords)
        {
            List<string[]> result = new List<string[]>();

            // Calculate amount of chunks
            int numberOfChunks = listOfWords.Length / _chunkSize;
            // Checks to see if there are a non full chunck, if so adds one more to the number of chunks
            numberOfChunks += listOfWords.Length % _chunkSize != 0 ? 1 : 0;

            for(int i = 0; i < numberOfChunks; i++)
            {
                int sliceStart = i * _chunkSize;

                // if last chunk add all remaining words to it
                if (i == numberOfChunks - 1)
                    result.Add(listOfWords.Slice(sliceStart).ToArray());
                else
                    result.Add(listOfWords.Slice(sliceStart, _chunkSize).ToArray());
            }

            return result;
        }

        private List<string> SerializeChunks()
        {
            List<string> result = new List<string>();
            foreach (string[] chunk in _chunkArray)
            {
                string s = JsonConvert.SerializeObject(chunk);
                result.Add(s);
            }
            return result;
        }

        public string GetStringChunk()
        {
            int i = CurrenntChunkIndex;
            CurrenntChunkIndex++;
            if (CurrenntChunkIndex == SerializedStringChunks.Count)
            {
                EndOfChunks = true;
            }
            return SerializedStringChunks[i];
        }       
    }
}
