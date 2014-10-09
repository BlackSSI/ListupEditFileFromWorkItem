using System;
using System.Collections.Generic;

namespace ListupEditFileFromWorkItem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("接続先={0}, 作業項目の種類={1}, 作業項目ID={2}", args[0], args[1], args[2]);

            var wi = new TfsWorkItem(args[0]);

            var wiList = wi.GetWorkItemsFromId(args[1], args[2]);

            var changeFileList = wi.GetChangeFileList(wiList);

            Console.WriteLine("***** 修正ファイル一覧 開始 *****");
            foreach (KeyValuePair<string, string> keyValuePair in changeFileList)
            {
                Console.WriteLine("File={0}, Changes={1}", keyValuePair.Key, keyValuePair.Value);
            }
            Console.WriteLine("***** 修正ファイル一覧 終了 *****");

            Console.ReadLine();
        }
    }
}
