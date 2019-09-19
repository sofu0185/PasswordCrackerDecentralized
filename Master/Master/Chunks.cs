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

        private List<String> _fullList = new List<string>();
        public List<List<string>> ChunkList { get; set; }
        public List<string> SerializedStringChunks { get; }
        public List<byte[]> SerializedByteChunks { get; }

        private int chunkSize = Constants.CHUNK_SIZE;
        public int CurrenntChunkIndex { get; private set; } = 0;
        public int CurrentChunkSize { get => ChunkList[CurrenntChunkIndex].Count; }
        public bool EndOfChunks { get; set; }

        public Chunks()
        {
            _fullList = FileHandler.ReadAllWordsInDictionary();
            ChunkList = new List<List<string>>();
            SplitChunks();

            SerializedStringChunks = new List<string>();
            SerializedByteChunks = new List<byte[]>();
            //SerializeByteChunks();
            SerializeStringChunks();

            Console.WriteLine("Chunks ready");
        }

        public void ResetCount()
        {
            CurrenntChunkIndex = 0;

        }

        private void SerializeStringChunks()
        {
            //compute the comma seperated strings for sending to the slave
            foreach (var chunk in ChunkList)
            {
                string s = JsonConvert.SerializeObject(chunk);
                SerializedStringChunks.Add(s);
            }
        }
        private void SerializeByteChunks()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using(MemoryStream ms = new MemoryStream())
            {
                foreach(List<string> chunk in ChunkList)
                {
                    formatter.Serialize(ms, chunk);
                    SerializedByteChunks.Add(ms.ToArray());
                    ms.SetLength(0);
                }
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

        public byte[] GetByteChunk()
        {
            int i = CurrenntChunkIndex;
            CurrenntChunkIndex++;
            if (CurrenntChunkIndex == SerializedStringChunks.Count)
            {
                EndOfChunks = true;
            }
            return SerializedByteChunks[i];
        }

        /// <summary>Returns a list with chunks (a list consisting of strings)  
        /// <para>The chunk can maximum have 10000 strings</para>
        /// </summary>
        private void SplitChunks()
        {
            int count = 0;
            //Ny chunk
            List<string> chunk = new List<string>();
            foreach(string s in _fullList)
            {
                //Ny chunk hvis ord i chunk er 10000
                if (count == chunkSize)
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
    }
}
