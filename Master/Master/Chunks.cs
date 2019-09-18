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
        public List<string> chunks;
        private int index = 0;
        private int chunkSize = 50000;
        public int nextChunk = 0;

        public Chunks()
        {
            _fullList = FileHandler.ReadAllWordsInDictionary();
            SplitChunks();
            chunks = new List<string>();
            foreach (var chunk in ChunkList)
            {
                string s = "";
                foreach (var str in chunk)
                {
                    s += "," + str;
                }
                chunks.Add(s);
            }

            Console.WriteLine("Chunks ready");
        }

        //public string ChunkToString()
        //{
        //    List<string> chunk = GetNextChunk();
        //    string s = "";
        //    foreach (string line in chunk)
        //    {
        //        s += "," + line;
        //    }

        //    return s;
        //}

        public void ResetCount()
        {
            nextChunk = 0;

        }
        public string GetNextChunk()
        {
            if (nextChunk == chunks.Count)
            {
                nextChunk = 0;
            }

            int i = nextChunk;
            nextChunk++;
            return chunks[i];
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

            int o = 0;
            foreach (var ll in ChunkList)
            {
                o += ll.Count;
            }

            Console.WriteLine(o);
        }
    }
}
