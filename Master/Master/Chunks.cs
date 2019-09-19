using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    /// <summary>A class handling inputting a word dictionary in txt format and outputting a splitted dictionary in chunks (list consisting of strings). 
    /// </summary>
    class Chunks
    {
        private List<String> _fullList = new List<string>();
        public List<List<string>> ChunkList;
        public List<string> stringChunks;
        private int chunkSize = Constants.CHUNK_SIZE;
        public int CurrenntChunkIndex { get; private set; } = 0;
        public bool EndOfChunks { get; set; }

        public Chunks()
        {
            _fullList = FileHandler.ReadAllWordsInDictionary();
            SplitChunks();
            stringChunks = new List<string>();

            // compute the comma seperated strings for sending to the slave
            foreach (var chunk in ChunkList)
            {
                string s = "";
                foreach (var str in chunk)
                {
                    s += "," + str;
                }
                s = s.TrimStart(',');
                stringChunks.Add(s);
            }

            Console.WriteLine("Chunks ready");
        }

        public void ResetCount()
        {
            CurrenntChunkIndex = 0;

        }

        public string GetChunk()
        {
            int i = CurrenntChunkIndex;
            CurrenntChunkIndex++;
            if (CurrenntChunkIndex == stringChunks.Count)
            {
                EndOfChunks = true;
            }
            return stringChunks[i];
            
        }

        /// <summary>Returns a list with chunks (a list consisting of strings)  
        /// <para>The chunk can maximum have 10000 strings</para>
        /// </summary>
        public void SplitChunks()
        {
            ChunkList = new List<List<string>>();
            
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
