using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Text;
using Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.Configuration;
using Signia.OmakaseCategoryFeeder.ApiClient.Extensions;
using Signia.OmakaseCategoryFeeder.Diagnostic;

namespace Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.ApiFusion
{
    public class ApiRequestExecutor : BaseApiRequestExecutor
    {

        private const string cHost = @"http://localhost:8001";
        protected override string DefaultHost => cHost;
        private DataType _dataType;

        public ApiRequestExecutor(ILogger logger)
            : this(string.Empty, DataType.json, CompressionType.None, logger)
        { }

        public ApiRequestExecutor(string host, ILogger logger)
            : this(host, DataType.json, CompressionType.None, logger)
        { }

        public ApiRequestExecutor(string host, DataType dataType, CompressionType compressionType, ILogger logger)
            : base(host, compressionType, logger)
        {
            _dataType = dataType;
            var config = new HttpClientConfig
            {
                Headers = new Dictionary<string, string>(),
                ContentTypeHeader = "application/" + _dataType,
                HostBasicName = Host
            };
            ConfigureConnection(config);
        }
        
        public override T HttpGetDeserialized<T>(string routePath)
        {
            var content = HttpGet(new Uri(new Uri(_config.HostBasicName), routePath));
            switch (_dataType)
            {
                case DataType.xml:
                    return XmlSerializer.DeserializeFromString<T>(content);
                case DataType.json:
                    return JsonSerializer.DeserializeFromString<T>(content);
            }
            throw new NotSupportedException();
        }

        public override IList<T> HttpGetListDeserialized<T>(string routePath)
        {
            var content = HttpGet(new Uri(new Uri(_config.HostBasicName), routePath));
            switch (_dataType)
            {
                case DataType.xml:
                    return XmlSerializer.DeserializeFromString<IList<T>>(content);
                case DataType.json:
                    return JsonSerializer.DeserializeFromString<IList<T>>(content);
            }
            throw new NotSupportedException();
        }

        public override T HttpPostDeserialized<T>(string routePath, string postContent)
        {
            var jsonContent = HttpPost(new Uri(new Uri(_config.HostBasicName), routePath), postContent);
            switch (_dataType)
            {
                case DataType.json:
                    return JsonSerializer.DeserializeFromString<T>(jsonContent);
                case DataType.xml:
                    return XmlSerializer.DeserializeFromString<T>(jsonContent);
            }
            throw new NotSupportedException();
        }

        public override T HttpPostDeserialized<T>(string routePath, T postData)
        {
            string postContent ;
            switch (_dataType)
            {
                case DataType.json:
                    postContent = GetFormalizedJson(postData);
                    break;
                case DataType.xml:
                    postContent = GetFormalizedXml(postData, "POST");
                    break;
                default:
                    throw new NotSupportedException();
            }
            return HttpPostDeserialized<T>(routePath, postContent);
        }

        protected string GetFormalizedXml<T>(T obj, string requestMethodName)
        {
            return XmlSerializer
                .SerializeToString(obj)
                .ToFormalizedXml<T>(requestMethodName);
        }

        protected string GetFormalizedJson<T>(T obj)
        {
            return JsonSerializer
                .SerializeToString(obj);
            //(obj.ToDtoWrapped())
            //.ToFormalizedJson<T>();
        }

        public override T HttpPatchDeserialized<T>(string routePath, string patchContent)
        {
            var content = HttpPatch(new Uri(new Uri(_config.HostBasicName), routePath), patchContent);
            switch (_dataType)
            {
                case DataType.json:
                    return JsonSerializer.DeserializeFromString<T>(content);
                case DataType.xml:
                    return XmlSerializer.DeserializeFromString<T>(content);
            }
            throw new NotSupportedException();
        }

        public override T HttpPatchDeserialized<T>(string routePath, T patchData)
        {
            switch (_dataType)
            {

                case DataType.json:
                {
                    var patchContent = GetFormalizedJson(patchData);

                    return HttpPatchDeserialized<T>(routePath, patchContent);
                }
                case DataType.xml:
                {
                    var patchContent = GetFormalizedXml(patchData, "PATCH");
                    return HttpPatchDeserialized<T>(routePath, patchContent);
                }
            }
            throw new NotSupportedException();
        }

        public override string HttpGet(string routePath)
            => HttpGet(new Uri(new Uri(_config.HostBasicName), routePath));

        public override string HttpPost(string routePath, string postContent)
            => HttpPost(new Uri(new Uri(_config.HostBasicName), routePath), postContent);

    }
}
