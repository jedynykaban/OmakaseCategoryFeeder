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

            //for API test purposes only
            var apiClient = new ApiRequestExecutor(@"http://localhost:8001", logger);
            var allStoredCategories = apiClient.GetAllCategoriesRequest();
            foreach (var category in categories)
            {
                try
                {
                    var exists = allStoredCategories.FirstOrDefault(c => c.FullPath?.ToLower() == category.FullPath?.ToLower());
                    if (exists != null)
                    {
                        logger.Log(LogLevels.Info, $"category: {category} already exists with ID: {exists.ID} (NO NEED TO ADD)");
                        continue;
                    }

                    var parentCategoryPath = category.ParentCategoryPath;
                    //adjustment about ID/ParentID before possible add category, but would be useful to store info about ID/ParentID
                    //so... let's try to find matching category
                    if (!string.IsNullOrEmpty(parentCategoryPath))
                    {
                        var parentCategory = allStoredCategories.FirstOrDefault(c => c.FullPath?.ToLower() == parentCategoryPath.ToLower());
                        if (parentCategory != null)
                            category.ParentID = parentCategory.ID;
                        else if (!string.IsNullOrEmpty(category.Parent?.ID))
                            category.ParentID = category.Parent.ID;
                        else
                            throw new Exception($"Cannot determine parent of category: {category}");
                    }

                    var apiResult = apiClient.CategoryCreateRequest(category);
                    logger.Log(LogLevels.Info, apiResult.ToString());

                    //must store information about obtained ID
                    category.ID = apiResult.ID;
                }
                catch (Exception e)
                {
                    logger.LogException(e, $"Cannot add category: {category}");
                    //this problem possibly can disallow to build planned tree hierarchy; break execution
                    throw;
                }
            }

            Console.ReadKey();
        }
    }
}
