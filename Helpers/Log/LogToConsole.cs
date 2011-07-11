using System;
using System.IO;

namespace OsmImportToSqlServer.Helpers.Log
{
    public class LogToConsole : ILoggeble
    {
        public void WriteLog(string messageLog)
        {
            Console.WriteLine(DateTime.Now + " " + messageLog);
        }
    }
}
