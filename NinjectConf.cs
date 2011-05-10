using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;

namespace OsmImportToSqlServer
{
    public static class Ninject
    {
        private static IKernel iKernel = new StandardKernel(new NinjectConf());

        public static ILoggeble GetLog()
        {
            return iKernel.Get<ILoggeble>();
        }
    }

    public class NinjectConf : NinjectModule
    {
        public override void Load()
        {
            Bind<ILoggeble>().To<LogToFile>();
        }
    }
}

