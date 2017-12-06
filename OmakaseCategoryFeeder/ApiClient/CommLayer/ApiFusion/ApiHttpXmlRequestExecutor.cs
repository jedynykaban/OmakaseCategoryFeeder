using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack;
using ServiceStack.Text;
using Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.ApiFusion;
using Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.Configuration;
using Signia.OmakaseCategoryFeeder.ApiClient.Extensions;
using Signia.OmakaseCategoryFeeder.Diagnostic;

namespace Fusion.Tests.Api.Client.CommLayer.ApiFusion
{
    /// <summary>
    /// Author: mo, 28.09.2016
    /// Ticket: FSN-1221
    /// 
    /// Exutor of API request in XML data format
    /// </summary>
    public class ApiHttpXmlRequestExecutor : BaseApiRequestExecutor
    {
        private const string cHost = @"http://127.0.0.1:1337";
        protected override string DefaultHost => cHost;

        public ApiHttpXmlRequestExecutor(ILogger logger)
            : this(string.Empty, null, null, CompressionType.None, logger)
        {
        }

        public ApiHttpXmlRequestExecutor(string host, string userName, string password, CompressionType compressionType, ILogger logger)
            : base(host, userName, password, compressionType, logger)
        {
            var config = new HttpClientConfig
            {
                Headers = new Dictionary<string, string>
                {
                    {"X-Fusion-Api-Key", "123456789!"},
                    {
                        "Authorization",
                        $"{"Basic"} {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{UserName}:{Password}"))}"
                    }
                },
                ContentTypeHeader = "application/xml",
                HostBasicName = Host,
            };
            ConfigureConnection(config);
        }


        protected string GetFormalizedXml<T>(T obj, string requestMethodName)
        {
            return XmlSerializer
                .SerializeToString(obj)
                .ToFormalizedXml<T>(requestMethodName);
        }


        public override T HttpGetDeserialized<T>(string routePath)
        {
            var xmlContent = HttpGet(new Uri(new Uri(_config.HostBasicName), routePath));
            return XmlSerializer.DeserializeFromString<T>(xmlContent);
        }

        public override IList<T> HttpGetListDeserialized<T>(string routePath)
        {
            var xmlContent = HttpGet(new Uri(new Uri(_config.HostBasicName), routePath));
            return XmlSerializer.DeserializeFromString<IList<T>>(xmlContent);
        }

        public override T HttpPostDeserialized<T>(string routePath, string postContent)
        {
            var xmlContent = HttpPost(new Uri(new Uri(_config.HostBasicName), routePath), postContent);
            return XmlSerializer.DeserializeFromString<T>(xmlContent);
        }

        public override T HttpPostDeserialized<T>(string routePath, T postData)
        {
            var postContent = GetFormalizedXml(postData, HttpMethods.Post);
            return HttpPostDeserialized<T>(routePath, postContent);
        }

        public override T HttpPatchDeserialized<T>(string routePath, string patchContent)
        {
            var xmlContent = HttpPatch(new Uri(new Uri(_config.HostBasicName), routePath), patchContent);
            return XmlSerializer.DeserializeFromString<T>(xmlContent);
        }

        public override T HttpPatchDeserialized<T>(string routePath, T patchData)
        {
            var patchContent = GetFormalizedXml(patchData, HttpMethods.Patch);
            return HttpPatchDeserialized<T>(routePath, patchContent);
        }

    }
}
