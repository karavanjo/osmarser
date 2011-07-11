using System;
using System.IO;

namespace OsmImportToSqlServer.Helpers.Log
{
    public static class Log
    {
        public static void Write(string mesage)
        {
            Config.Ninject.Ninject.GetLog().WriteLog(mesage);
        }
    }
}
