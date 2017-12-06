using System.Collections.Generic;

namespace Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.Configuration
{
    public class HttpClientConfig
    {
        public IDictionary<string, string> Headers { get; set; }
        public string ContentTypeHeader { get; set; }
        public string HostBasicName { get; set; }
    }
}
