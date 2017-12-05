using System.Collections.Generic;
using System.Linq;
using Signia.OmakaseCategoryFeeder.Diagnostic;
using Signia.OmakaseCategoryFeeder.Model;
using Signia.OmakaseCategoryFeeder.Services.Interface;

namespace Signia.OmakaseCategoryFeeder.Services.Impl
{
    /// <summary>
    /// Allows to read ordered by level category tree basis on passed 'resourceReader'
    /// </summary>
    public class CsvCategoriesHierarchyReader : ICategoryReader
    {
        private readonly CsvFileReader _csvFileReader;
        private readonly ILogger _logger;
        private List<string[]> _csvContent;

        public CsvCategoriesHierarchyReader(CsvFileReader csvFileReader, ILogger logger)
        {
            _csvFileReader = csvFileReader;
            _logger = logger;
        }

        public List<Category> ReadAllCategories(bool autoCreateMissingParentCategories)
        {
            var result = new List<Category>();

            foreach (var csvCategory in ResetReader()
                                            .Where(line => line != null && line.Length > 0)
                                            .OrderBy(line => line.Length))
            {
                CreateCategory(csvCategory, result, autoCreateMissingParentCategories);
            }
            return result;
        }

        private Category CreateCategory(string[] csvCategory, List<Category> result, bool autoCreateMissingParentCategories)
        {
            Category category = null;
            if (csvCategory.Length == 1)
            {
                category = new Category { Name = csvCategory[0], FullPath = csvCategory[0] };
            }
            else
            {
                var ancestorFullPath = csvCategory.Take(csvCategory.Length - 1).Aggregate((older, newer) => $"{older}{Category.CFullPathSeparator}{newer}");
                var candidateCategoryName = csvCategory.Last();

                var parent = result.FirstOrDefault(cat => cat.FullPath.ToLower() == ancestorFullPath.ToLower());
                if (parent != null)
                    category = new Category
                    {
                        Name = candidateCategoryName,
                        FullPath = $"{ancestorFullPath}{Category.CFullPathSeparator}{candidateCategoryName}",
                        Parent = parent
                    };
                else if (autoCreateMissingParentCategories)
                {
                    var parentCategory = CreateCategory(csvCategory.Take(csvCategory.Length - 1).ToArray(), result, true);
                    category = new Category
                    {
                        Name = candidateCategoryName,
                        FullPath = $"{ancestorFullPath}{Category.CFullPathSeparator}{candidateCategoryName}",
                        Parent = parentCategory
                    };
                }
                else
                    _logger.Log(LogLevels.Warning,
                        $"Category [{candidateCategoryName}] is wrongly defined, cannot find parent: [{ancestorFullPath}]");
            }
            if (category != null)
                result.Add(category);
            return category;
        }

        private IEnumerable<string[]> ResetReader(bool forceFileRead = false)
        {
            if (_csvContent == null || forceFileRead)
                _csvContent = _csvFileReader
                    .ReadFile(true)
                    // to possibly exclude first row that can contain column names
                    .Where(line => line.Length > 0 && line[0].ToLower() != "level 1") 
                    .ToList();

            return _csvContent;
        }

    }
}
