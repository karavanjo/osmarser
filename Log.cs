using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ninject;

namespace OsmImportToSqlServer
{
    public interface ILoggeble
    {
        void WriteLog(string messageLog);
    }

    public static class Log
    {
        public static void Write(string mesage)
        {
            Ninject.GetLog().WriteLog(mesage);
        }
    }

    public class LogToConsole : ILoggeble
    {
        public void WriteLog(string messageLog)
        {
            Console.WriteLine(DateTime.Now + " " + messageLog);
        }
    }

    
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
