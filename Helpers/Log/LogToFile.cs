using System;
using System.IO;

namespace OsmImportToSqlServer.Helpers.Log
{
    public class LogToFile : ILoggeble
    {
        private static object lockObject = new object();
        private static string pathFileLog = @"c:\log.log";
        public void WriteLog(string messageLog)
        {
            lock (lockObject)
            {
                FileStream fileStream =
                    File.Exists(pathFileLog)
                        ? new FileStream(pathFileLog, FileMode.Append)
                        : File.Create(pathFileLog);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(DateTime.Now + " " + messageLog);
                streamWriter.Close();
            }
        }
    }
}
