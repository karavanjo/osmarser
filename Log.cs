using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace Osm
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
        public void WriteLog(string messageLog)
        {
            throw new NotImplementedException();
        }
    }
}
