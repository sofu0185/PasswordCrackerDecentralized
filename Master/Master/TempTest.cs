//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;

//namespace Master
//{
//    class TempTest
//    {
//        public void TestOutputAllChunks()
//        {
//            Dictionary dic = new Dictionary();
//            Password pass = new Password();
//            var list = dic.SplitChunks();

//            foreach (var VARIABLE in list)
//            {
//                int index = 0;
//                foreach (var V in VARIABLE)
//                {
//                    Console.WriteLine(V);
//                    index++;
//                }
//                Console.WriteLine(index);
//                Console.WriteLine("Chunk udført! Fortsætter næste chunk:");
//                Console.WriteLine(pass.GetNextPass());
//                Thread.Sleep(2000);
//            }
//            Console.WriteLine("Færdig med alle chunks!");
//        }
//    }
//}
