using Ninject;
using Ninject.Modules;
using OsmImportToSqlServer.Helpers.Log;

namespace OsmImportToSqlServer.Config.Ninject
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

