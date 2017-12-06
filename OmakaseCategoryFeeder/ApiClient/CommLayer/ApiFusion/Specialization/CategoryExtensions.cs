using System;
using System.Collections.Generic;
using Signia.OmakaseCategoryFeeder.Model;

namespace Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.ApiFusion.Specialization
{
    public static class ValidPeriodsExtensions
    {
        private const string PrefixRoutePath = "/v2/categories";

        public static IList<Category> GetAllCategoriesRequest(this BaseApiRequestExecutor apiClient)
        {
            throw new NotImplementedException("Parsing raw result needs to be added first");

            //return apiClient.HttpGetListDeserialized<Category>(PrefixRoutePath);
        }

        public static Category CategoryByIdRequest(this BaseApiRequestExecutor apiClient, string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException(nameof(categoryId));

            var reqRoutePathPattern = PrefixRoutePath + "/{0}";

            var resultCandidate = apiClient.HttpGetDeserialized<Category>(string.Format(reqRoutePathPattern, categoryId));
            resultCandidate.FullPath = resultCandidate.Name;
            var parentId = resultCandidate.ParentID;
            while (!string.IsNullOrEmpty(parentId))
            {
                var parent = apiClient.HttpGetDeserialized<Category>(string.Format(reqRoutePathPattern, parentId));
                resultCandidate.FullPath = $"{parent.Name}{Category.CFullPathSeparator}{resultCandidate.FullPath}";
                parentId = parent.ParentID;
            }

            return resultCandidate;
        }

        public static Category CategoryCreateRequest(this BaseApiRequestExecutor apiClient, Category newCategory)
        {

            if (apiClient == null)
                throw new ArgumentNullException(nameof(apiClient));

            if (newCategory == null)
                throw new ArgumentNullException(nameof(newCategory));

            var reqRoutePath = PrefixRoutePath;

            return apiClient.HttpPostDeserialized(reqRoutePath, newCategory);
        }
    }
}
