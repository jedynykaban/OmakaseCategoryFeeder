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

                if (categoryColors.Item1.Length != category.ColorArr.Length)
                {
                    var msg =
                        $"Something went very wrong..., color HLS len: {categoryColors.Item1.Length} vs. {category.ColorArr.Length})";
                    _logger.Log(LogLevels.Fatal, msg);
                    throw new Exception(msg);
                }

                if (categoryColors.Item2.Length != category.GradientArr.Length)
                {
                    var msg =
                        $"Something went very wrong..., color HLS len: {categoryColors.Item2.Length} vs. {category.GradientArr.Length})";
                    _logger.Log(LogLevels.Fatal, msg);
                    throw new Exception(msg);
                }

                for (var i = 0; i < category.ColorArr.Length; i++)
                    category.ColorArr[i] = categoryColors.Item1[i];
                for (var i = 0; i < category.GradientArr.Length; i++)
                    category.GradientArr[i] = categoryColors.Item2[i];
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

        private const int SaturationMin = 30;
        private const int SaturationMax = 80;
        private const int LumMin = 40;
        private const int LumMax = 80;
        private const int GradientSaturationMin = 30;
        private const int GradientSaturationMax = 100;
        private const int GradientLumMin = 40;
        private const int GradientLumMax = 80;

        private Dictionary<string, Tuple<int[], int[]>> ParseRawColors(IEnumerable<string[]> rawColors)
        {
            var result = new Dictionary<string, Tuple<int[], int[]>>();
            foreach (var raw in rawColors)
            {
                if (raw.Length < 1)
                {
                    var nameForLog = raw.Length > 0 ? raw[0] : "(?)";
                    _logger.Log(LogLevels.Warning, $"Wrongly defined row {nameForLog} (length: {raw.Length})");
                    continue;
                }

                if (raw.Length < 4 || string.IsNullOrEmpty(raw[0]))
                {
                    var nameForLog = raw[0];
                    _logger.Log(LogLevels.Warning, $"Entry with category name has wrongly defined structure [{nameForLog}]   (length: {raw.Length})");
                    continue;
                }

                if (!HslTryParse(raw[3], out int[] hslColor))
                    _logger.Log(LogLevels.Warning, $"Hsl color for category: {raw[0]} wrongly defined: {raw[3]}");
                else
                {
                    if (hslColor[1] < SaturationMin)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} saturantion needs to be corrected: {hslColor[1]}");
                        hslColor[1] = SaturationMin;
                    }
                    if (hslColor[1] > SaturationMax)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} saturantion needs to be corrected: {hslColor[1]}");
                        hslColor[1] = SaturationMax;
                    }
                    if (hslColor[2] < LumMin)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} lum needs to be corrected: {hslColor[2]}");
                        hslColor[2] = LumMin;
                    }
                    if (hslColor[2] > LumMax)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} lum needs to be corrected: {hslColor[2]}");
                        hslColor[2] = LumMax;
                    }
                    //extra condition
                    if (hslColor[0] > 230 && hslColor[0] < 240)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} hue needs to be corrected: {hslColor[0]}");
                        hslColor[0] = hslColor[0] < 235 ? 230 : 240;
                    }
                }
                if (!HslTryParse(raw[5], out int[] hslGradient))
                {
                    _logger.Log(LogLevels.Debug, $"Hsl gradient for category: {raw[0]} wrongly defined: {raw[5]}; assigning default value");
                    hslGradient = _defaultGradient;
                }
                else
                {
                    if (hslGradient[1] < GradientSaturationMin)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} saturantion needs to be corrected: {hslGradient[1]}");
                        hslGradient[1] = GradientSaturationMin;
                    }
                    if (hslGradient[1] > GradientSaturationMax)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} saturantion needs to be corrected: {hslGradient[1]}");
                        hslGradient[1] = GradientSaturationMax;
                    }
                    if (hslGradient[2] < GradientLumMin)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} lum needs to be corrected: {hslGradient[2]}");
                        hslGradient[2] = GradientLumMin;
                    }
                    if (hslGradient[2] > GradientLumMax)
                    {
                        _logger.Log(LogLevels.Debug, $"Hsl color for category: {raw[0]} lum needs to be corrected: {hslGradient[2]}");
                        hslGradient[2] = GradientLumMax;
                    }
                }

                result.Add(CreateCategoryDictKey(raw[0]), new Tuple<int[], int[]>(hslColor, hslGradient));
            }
            return result;
        }

        private string CreateCategoryDictKey(string candidateKey) => candidateKey
            .Replace("&", "and")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("'", "")
            .ToLower();

        private bool HslTryParse(string colorCode, out int[] parseResult)
        {
            parseResult = _defaultGradient;
            if (string.IsNullOrEmpty(colorCode))
                return false;

            var colorCodeCleaned = colorCode
                .ToLower()
                .Replace("hsl", "")
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
                    .ReadFile(false, false)
                    // to possibly exclude first row that can contain column names
                    .Where(line => line.Length > 0 && !line[0].ToLower().StartsWith("IAB Categories".ToLower()))
                    .ToList();

            return _csvContent;
        }

    }
}
