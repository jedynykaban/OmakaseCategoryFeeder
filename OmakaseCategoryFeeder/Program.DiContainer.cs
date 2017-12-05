using LightInject;
using Signia.OmakaseCategoryFeeder.Diagnostic;
using Signia.OmakaseCategoryFeeder.Services.Impl;
using Signia.OmakaseCategoryFeeder.Services.Interface;

namespace Signia.OmakaseCategoryFeeder
{
    partial class Program
    {
        private const string CAppName = "OmakaseCategoryFeeder";

        internal static ServiceContainer Container = new ServiceContainer();

        private static ServiceContainer RegisterDi(params string[] args)
        {
            Container.RegisterInstance(new LoggerConfig
            {
                AppName = CAppName,
                AppVersion = "0.0.1.0",
                OperationMode = ConfigurationSource.Offline
            });
            Container.Register<ILogger, Logger>();
            Container.Register(c => new CsvFileReader(args[0], ';'), "category");
            Container.Register(c => new CsvFileReader(args[1], ';'), "color");
            Container.Register<ICategoryReader>(
                c => new CsvCategoriesHierarchyReader(c.GetInstance<CsvFileReader>("category"), c.GetInstance<ILogger>()), 
                    "csv");
            Container.Register<ICategoryColorReaderAndAssigner>(
                c => new CsvMosaiq2017CategoriesColorsPlusGradientReader(c.GetInstance<CsvFileReader>("color"), c.GetInstance<ILogger>()), 
                    "csv");

            return Container;
        }



    }
}
