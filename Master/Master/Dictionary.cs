using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Master
{
    /// <summary>A class handling inputting a word dictionary in txt format and outputting a splitted dictionary in chunks (list consisting of strings). 
    /// </summary>
    class Dictionary
    {
        private FileStream _fs = new FileStream("Text files/webster-dictionary.txt", FileMode.Open, FileAccess.Read);
        private List<String> fullList = new List<string>();
        public List<List<string>> ChunkList;
        private int index = 0;
        private int chunkSize = 10000;
        public int nextChunk = 0;

        public Dictionary()
        {
            GetFullList();
            SplitChunks();
        }

        public string ChunkToString()
        {
            List<string> chunk = GetNextChunk();
            string s = "";
            foreach (string line in chunk)
            {
                s += "," + line;
            }

            return s;
        }

        public void ResetCount()
        {
            nextChunk = 0;

        }
        public List<string> GetNextChunk()
        {
            if (nextChunk == ChunkList.Count)
            {
                nextChunk = 0;
            }
            List<string> chunk = ChunkList[nextChunk];
            nextChunk++;
            return chunk;
        }

        /// <summary>Returns a list with chunks (a list consisting of strings)  
        /// <para>The chunk can maximum have 10000 strings</para>
        /// </summary>
        public void SplitChunks()
        {
            ChunkList = new List<List<string>>();

            //while (index < fullList.Count)
            //{
            //    //Ny chunk
            //    List<String> chunk = new List<String>();

            //    //Der må kun indsættes chunkSize ord ind i hver chunk.
            //    for (int i = 0; i < chunkSize; i++)
            //    {
            //        //Hvis vi er nået længere end der er ord i vores liste så break ud
            //        if (index == fullList.Count) break;
            //        chunk.Add(fullList[index]);
            //        index++;
            //    }
            //    ChunkList.Add(chunk);
            //}

            int count = 0;
            //Ny chunk
            List<string> chunk = new List<string>();
            foreach(string s in fullList)
            {
                //Ny chunk hvis ord i chunk er 10000
                if (count == 10_000)
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


        public void GetFullList()
        {
            StreamReader stReader = new StreamReader(_fs);
            while (!stReader.EndOfStream)
            {
                fullList.Add(stReader.ReadLine());
            }
        }
    }
}
