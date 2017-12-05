using System.Collections.Generic;
using Signia.OmakaseCategoryFeeder.Model;

namespace Signia.OmakaseCategoryFeeder.Services.Interface
{
    public interface ICategoryReader
    {
        List<Category> ReadAllCategories(bool autoCreateMissingParentCategories);
    }
}
