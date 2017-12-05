using System;
using System.Collections.Generic;
using System.Linq;
using Signia.OmakaseCategoryFeeder.Diagnostic;
using LightInject;
using Signia.OmakaseCategoryFeeder.Model;
using Signia.OmakaseCategoryFeeder.Services.Interface;

namespace Signia.OmakaseCategoryFeeder
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(@"Path to categories resource file must be passed");
                return;
            }

            if (args.Length < 2)
            {
                Console.WriteLine(@"Path to categories colors resource not passed. Colors will not be assigned to categories.");
            }

            var container = RegisterDi(args[0], args.Length >= 2 ? args[1] : string.Empty);

            var loggerConfig = container.GetInstance<LoggerConfig>();
            var logger = container.GetInstance<ILogger>();
            logger.Log(LogLevels.Info, $"App: {loggerConfig.AppName} started");

            IEnumerable<Category> categories;
            try
            {
                if (args.Length < 2)
                {
                    logger.Log(LogLevels.Warning, "Path to categories colors resource not passed. Colors will not be assigned to categories.");
                }

                categories = container
                    .GetInstance<ICategoryReader>("csv")
                    .ReadAllCategories(true);

                container
                    .GetInstance<ICategoryColorReaderAndAssigner>("csv")
                    .AssignColorsToCategories(categories, true);
            }
            finally 
            {
                logger.Log(LogLevels.Info, $"App: {loggerConfig.AppName} stopped");
            }

            // for test purposes only
            foreach (var category in categories.Where(c => c.FullPath.All(ch => ch != Category.CFullPathSeparator)))
            {
                logger.Log(LogLevels.Info,
                    $"{category.FullPath.Count(c => c == Category.CFullPathSeparator) + 1}: {category.FullPath}, color: [{string.Join(",", category.Color)}] & gradient: [{string.Join(",", category.Gradient)}]");
            }

            Console.ReadKey();
        }
    }
}
