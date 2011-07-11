using System;
using System.IO;

namespace OsmImportToSqlServer.Helpers.Log
{
    public interface ILoggeble
    {
        void WriteLog(string messageLog);
    }
}
