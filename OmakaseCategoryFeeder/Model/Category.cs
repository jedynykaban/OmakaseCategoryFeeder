using System.Linq;

namespace Signia.OmakaseCategoryFeeder.Model
{
    public class Category
    {
        public const char CFullPathSeparator = '|';

        public string ID { get; set; } //managed only by API!
        public string ParentID { get; set; } //managed only by API!

        public Category Parent { get; set; }

        public string Name { get; set; }
        public string FullPath { get; set; }

        public int[] ColorArr { get; }
        public int[] GradientArr { get; }

        private Hsl _color;

        public Hsl Color
        {
            get => _color;
            set
            {
                _color = value;

                ColorArr[0] = value?.H ?? 0;
                ColorArr[1] = value?.S ?? 0;
                ColorArr[2] = value?.L ?? 0;

            }
        }
        private Hsl _gradient;
        public Hsl Gradient
        {
            get => _gradient;
            set
            {
                _gradient = value;

                GradientArr[0] = value?.H ?? 0;
                GradientArr[1] = value?.S ?? 0;
                GradientArr[2] = value?.L ?? 0;

            }
        }

        public Category()
        {
            ColorArr = new int[3]; //HLS
            GradientArr = new int[3]; //HLS
        }

        public override string ToString()
            =>
                $"Level[{FullPath?.Count(c => c == CFullPathSeparator) + 1 ?? 0}]: {(!string.IsNullOrEmpty(FullPath) ? FullPath : Name) ?? "Name is null"}, color: [{string.Join(",", ColorArr)}] & gradient: [{string.Join(",", GradientArr)}]; {nameof(ID)}: {(!string.IsNullOrEmpty(ID) ? ID : "not-set")}, {nameof(ParentID)}: {(!string.IsNullOrEmpty(ParentID) ? ParentID : "--root--")}";
    }

    public class Hsl
    {
        public int H { get; set; }
        public int S { get; set; }
        public int L { get; set; }
    }
}
