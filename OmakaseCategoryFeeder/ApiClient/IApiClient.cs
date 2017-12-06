using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Signia.OmakaseCategoryFeeder.Model;

namespace Signia.OmakaseCategoryFeeder.ApiClient
{
    public interface IApiClient
    {
        void SendCategory(Category category);
    }
}
