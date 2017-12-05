using System;
using System.Collections.Generic;
using System.Linq;
using Signia.OmakaseCategoryFeeder.Diagnostic;
using Signia.OmakaseCategoryFeeder.Model;
using Signia.OmakaseCategoryFeeder.Services.Interface;

namespace Signia.OmakaseCategoryFeeder.Services.Impl
{
    /// <summary>
    /// Allows to assign colors to passsed particular categories
    /// </summary>
    public class CsvMosaiq2017CategoriesColorsPlusGradientReader : ICategoryColorReaderAndAssigner
    {
        private readonly CsvFileReader _csvFileReader;
        private readonly ILogger _logger;
        private List<string[]> _csvContent;

        private readonly int[] _defaultGradient = {0, 0, 94};


        public CsvMosaiq2017CategoriesColorsPlusGradientReader(CsvFileReader csvFileReader, ILogger logger)
        {
            _csvFileReader = csvFileReader;
            _logger = logger;
        }

        public void AssignColorsToCategories(IEnumerable<Category> categories, bool assignColorFromParent)
        {
            var rawColors = ResetReader();
            // possible file not exists (or has no entries)
            var enumerable = rawColors as IList<string[]> ?? rawColors.ToList();
            if (enumerable.Count == 0)
                return;

            var colors = ParseRawColors(enumerable.ToList());

            foreach (var category in categories)
            {
                if (!FindColorForCategory(colors, category, assignColorFromParent, out Tuple<int[], int[]> categoryColors))
                {
                    _logger.Log(LogLevels.Info, $"Not found color for category: {category.Name} ({nameof(assignColorFromParent)}: {assignColorFromParent})");
                    continue;
                }

                if (categoryColors.Item1.Length != category.Color.Length)
                {
                    var msg =
                        $"Something went very wrong..., color HLS len: {categoryColors.Item1.Length} vs. {category.Color.Length})";
                    _logger.Log(LogLevels.Fatal, msg);
                    throw new Exception(msg);
                }

                if (categoryColors.Item2.Length != category.Gradient.Length)
                {
                    var msg =
                        $"Something went very wrong..., color HLS len: {categoryColors.Item2.Length} vs. {category.Gradient.Length})";
                    _logger.Log(LogLevels.Fatal, msg);
                    throw new Exception(msg);
                }

                for (var i = 0; i < category.Color.Length; i++)
                    category.Color[i] = categoryColors.Item1[i];
                for (var i = 0; i < category.Gradient.Length; i++)
                    category.Gradient[i] = categoryColors.Item2[i];
            }

        }

        private bool FindColorForCategory(Dictionary<string, Tuple<int[], int[]>> colorRepository, Category category,
            bool searchRecurisve, out Tuple<int[], int[]> colors)
        {
            colors = null;
            if (category == null)
                return false;

            if (colorRepository.TryGetValue(category.Name.ToLower(), out colors))
                return true;

            if (searchRecurisve)
                return FindColorForCategory(colorRepository, category.Parent, true, out colors);

            return false;
        }

        private Dictionary<string, Tuple<int[], int[]>> ParseRawColors(IEnumerable<string[]> rawColors)
        {
            var result = new Dictionary<string, Tuple<int[], int[]>>();
            foreach (var raw in rawColors)
            {
                if (raw.Length < 1)
                {
                    var nameForLog = raw.Length > 0 ? raw[0] : "(?)";
                    _logger.Log(LogLevels.Warning, $"Wrongly defined row {nameForLog} (length: {raw.Length})");
                }

                if (raw.Length < 4)
                {
                    var nameForLog = raw[0];
                    _logger.Log(LogLevels.Warning, $"Entry with category name has wrongly defined structure {nameForLog} (length: {raw.Length})");
                }

                if (!HlsTryParse(raw[3], out int[] hlsColor))
                    _logger.Log(LogLevels.Warning, $"Hls color for category: {raw[0]} wrongly defined: {raw[3]}");
                if (!HlsTryParse(raw[5], out int[] hlsGradient))
                {
                    _logger.Log(LogLevels.Debug, $"Hls gradient for category: {raw[0]} wrongly defined: {raw[5]}; assigning default value");
                    hlsGradient = _defaultGradient;
                }

                result.Add(CreateCategoryDictKey(raw[0]), new Tuple<int[], int[]>(hlsColor, hlsGradient));
            }
            return result;
        }

        private string CreateCategoryDictKey(string candidateKey) => candidateKey
            .Replace("&", "and")
            .Replace("(", "")
            .Replace(")", "")
            .ToLower();

        private bool HlsTryParse(string colorCode, out int[] parseResult)
        {
            parseResult = _defaultGradient;
            if (string.IsNullOrEmpty(colorCode))
                return false;

            var colorCodeCleaned = colorCode
                .ToLower()
                .Replace("hls", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("%", "");

            try
            {
                parseResult = colorCodeCleaned
                    .Split(',')
                    .Select(s => int.Parse(s.Trim()))
                    .ToArray();
                //it's not HLS color
                if (parseResult.Length != 3
                    || parseResult.Any(e => e < 0 || e > 360))
                    return false;
            }
            catch (Exception e)
            {
                _logger.LogException(e, $"Color code parsing execption: {colorCode}");
                return false;
            }

            return true;
        }

        private IEnumerable<string[]> ResetReader(bool forceFileRead = false)
        {
            if (_csvContent == null || forceFileRead)
                _csvContent = _csvFileReader
                    .ReadFile(false)
                    // to possibly exclude first row that can contain column names
                    .Where(line => line.Length > 0 && !line[0].ToLower().StartsWith("IAB Categories".ToLower()))
                    .ToList();

            return _csvContent;
        }

    }
}
