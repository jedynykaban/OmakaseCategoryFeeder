namespace Signia.OmakaseCategoryFeeder.Model
{
    public class Category
    {
        public const char CFullPathSeparator = '|';

        public Category Parent { get; set; }

        public string Name { get; set; }
        public string FullPath { get; set; }

        public int[] Color { get; }
        public int[] Gradient { get; }

        public Category()
        {
            Color = new int[3]; //HLS
            Gradient = new int[3]; //HLS
        }
    }
}
