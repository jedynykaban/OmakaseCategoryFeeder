using System.Collections.Generic;
using Signia.OmakaseCategoryFeeder.Model;

namespace Signia.OmakaseCategoryFeeder.Services.Interface
{
    public interface ICategoryColorReaderAndAssigner
    {
        void AssignColorsToCategories(IEnumerable<Category> categories, bool assignColorFromParent);
    }
}
