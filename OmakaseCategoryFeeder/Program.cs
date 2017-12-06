using System;
using System.Collections.Generic;
using System.Linq;
using Signia.OmakaseCategoryFeeder.Diagnostic;
using LightInject;
using Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.ApiFusion;
using Signia.OmakaseCategoryFeeder.Model;
using Signia.OmakaseCategoryFeeder.Services.Interface;
using Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.ApiFusion.Specialization;

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
            logger.Log(LogLevels.Info, $"All categories: {categories.Count()}");
            foreach (var category in categories.Where(c => c.FullPath.Count(ch => ch == Category.CFullPathSeparator) == 0))
            {
                logger.Log(LogLevels.Info, category.ToString());
            }

            //for API test purposes only
            var apiClient = new ApiRequestExecutor(@"http://localhost:8001", logger);
            foreach (var category in categories.Where(c => c.FullPath.Count(ch => ch == Category.CFullPathSeparator) == 0).Take(2))
            {
                var apiResult = apiClient.CategoryByIdRequest("EhMKCENhdGVnb3J5EICAgLaS_IAK");

                logger.Log(LogLevels.Info, apiResult.ToString());
            }


            Console.ReadKey();
        }
    }
}
