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
    class Chunks
    {
        private readonly List<string> _fullList = new List<string>();
        private readonly int _chunkSize = Constants.CHUNK_SIZE;

        public List<List<string>> ChunkList { get; private set; }
        public List<string> SerializedStringChunks { get; }
        public int CurrenntChunkIndex { get; private set; }
        public int CurrentChunkSize { get => ChunkList[CurrenntChunkIndex].Count; }
        public bool EndOfChunks { get; private set; }

        public Chunks()
        {
            CurrenntChunkIndex = 0;
            _fullList = FileHandler.ReadAllWordsInDictionary();
            ChunkList = new List<List<string>>();
            SplitChunks(_fullList);

            SerializedStringChunks = new List<string>();
            SerializeStringChunks();

            Console.WriteLine("\tChunks ready");
        }

        /// <summary>Splits full list of words from dictionary into chunks (a list consisting of strings)  
        /// </summary>
        private void SplitChunks(List<string> listOfWords)
        {
            int count = 0;
            //Ny chunk
            List<string> chunk = new List<string>();
            foreach (string s in listOfWords)
            {
                if (count == _chunkSize)
                {
                    ChunkList.Add(chunk);
                    chunk = new List<string>();
                    count = 0;
                }
                chunk.Add(s);
                count++;
            }
            ChunkList.Add(chunk);
        }

        private void SerializeStringChunks()
        {
            foreach (var chunk in ChunkList)
            {
                string s = JsonConvert.SerializeObject(chunk);
                SerializedStringChunks.Add(s);
            }
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

        public void ResetCount()
        {
            CurrenntChunkIndex = 0;

        }        
    }
}
