using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Signia.OmakaseCategoryFeeder.Services.Impl
{
    /// <summary>
    /// Reads passed in constructor parameter CSV file and return its content in list (lines) of string array (splitted content)
    /// </summary>
    public class CsvFileReader
    {
        private readonly string _csvFileLocation;
        private readonly char _csvFileSeparator;

        public CsvFileReader(string csvFileLocation, char csvFileSeparator)
        {
            _csvFileLocation = csvFileLocation;
            _csvFileSeparator = csvFileSeparator;
        }

        public List<string[]> ReadFile(bool mustRead)
        {
            if (!File.Exists(_csvFileLocation))
            {
                if (!mustRead)
                    return new List<string[]>();
                throw new IOException($"File: {_csvFileLocation} doesnot exists. File read cannot be executed.");
            }

            var result = File
                .ReadLines(_csvFileLocation)
                .Select(line => line
                    .Split(_csvFileSeparator)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s.Trim())
                    .ToArray())
                .ToList();

            return result;
        }

    }
}
