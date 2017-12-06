using System;
using System.Collections.Generic;
using Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.Configuration;
using Signia.OmakaseCategoryFeeder.Diagnostic;

namespace Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.ApiFusion
{
    public abstract class BaseApiRequestExecutor
    {
        public abstract T HttpGetDeserialized<T>(string routePath);
        public abstract IList<T> HttpGetListDeserialized<T>(string routePath);
        public abstract T HttpPostDeserialized<T>(string routePath, string postContent);
        public abstract T HttpPostDeserialized<T>(string routePath, T postData);
        public abstract T HttpPatchDeserialized<T>(string routePath, string patchContent);
        public abstract T HttpPatchDeserialized<T>(string routePath, T patchData);

        public abstract string HttpGet(string routePath);
        public abstract string HttpPost(string routePath, string postContent);

        internal HttpClientConfig ApiHttpClientConfig { get; private set; }
        public HttpRequestExecutor ApiHttpClient { get; protected set; }

        protected abstract string DefaultHost { get; }

        private string _host;
        public string Host => !string.IsNullOrEmpty(_host) ? _host : DefaultHost;



        #region name wrapper

        protected HttpClientConfig _config => ApiHttpClientConfig;
        protected HttpRequestExecutor _httpRequestExecutor => ApiHttpClient;

        #endregion

        public BaseApiRequestExecutor(ILogger logger)
            : this(string.Empty, CompressionType.None, logger)
        {
        }

        public BaseApiRequestExecutor(string host, CompressionType compressionType, ILogger logger)
        {
            _host = host;

            ApiHttpClient = new HttpRequestExecutor(compressionType, logger);
        }

        protected void ConfigureConnection(HttpClientConfig config)
        {
            ApiHttpClientConfig = config;
        }

        protected string HttpGet(Uri reqUri)
        {
            return _httpRequestExecutor.Get(reqUri, _config.Headers, null, _config.ContentTypeHeader);
        }

        protected string HttpPost(Uri reqUri, string postContent)
        {
            return _httpRequestExecutor
                .Post(postContent, reqUri, _config.Headers, _config.ContentTypeHeader, _config.ContentTypeHeader);
        }

        protected string HttpPatch(Uri reqUri, string patchContent)
        {
            return _httpRequestExecutor
                .Patch(patchContent, reqUri, _config.Headers, _config.ContentTypeHeader, _config.ContentTypeHeader);
        }

    }
}
