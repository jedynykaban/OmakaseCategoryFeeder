using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.Configuration;
using Signia.OmakaseCategoryFeeder.ApiClient.Extensions;
using Signia.OmakaseCategoryFeeder.Diagnostic;

namespace Signia.OmakaseCategoryFeeder.ApiClient.CommLayer
{
    public class HttpRequestExecutor
    {
        private readonly CompressionType _compressionType;
        private readonly ILogger _logger;

        public HttpRequestExecutor(CompressionType compressionType, ILogger logger)
        {
            _compressionType = compressionType;
            _logger = logger;
        }

        //to reconfigure certificate check, to allow using self-signed certs
        static HttpRequestExecutor()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                                    (sender, certificate, chain, error) =>
                                        chain.ChainElements[chain.ChainElements.Count - 1].Certificate.Thumbprint ==
                                        certificate.GetCertHashString();
        }

        private HttpRequestMessage GetHttpRequest(Uri uri, HttpMethod method, string content = null, string contentType = null)
        {
            return new HttpRequestMessage
            {
                RequestUri = new Uri(uri.ToString()),
                Method = method,
                Content = contentType != null ? new StringContent(content, Encoding.UTF8, contentType) : null
            };
        }

        private HttpClient GetHttpClient(IDictionary<string, string> headers, string contentTypeResponse = null)
        {
            var httpClient = new HttpClient();

            headers?.ForEachDo(h => httpClient.DefaultRequestHeaders.Add(h.Key, h.Value));
            if (!string.IsNullOrEmpty(contentTypeResponse))
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentTypeResponse));

            return httpClient;
        }
        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        private string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                if (_compressionType == CompressionType.deflate)
                {
                    using (var gs = new DeflateStream(msi, CompressionMode.Decompress))
                    {
                        gs.CopyTo(mso);
                    }
                }
                else if (_compressionType == CompressionType.gzip)
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        CopyTo(gs, mso);
                    }
                }
                var bytes1 = mso.ToArray();
                return Encoding.UTF8.GetString(bytes1);
            }
        }
        public string ExecuteHttpRequest(HttpMethod httpMethod, string content, Uri uri, IDictionary<string, string> headers = null,
            string contentTypeRequest = null, string contentTypeResponse = null)
        {
            var httpClient = GetHttpClient(headers, contentTypeResponse);
            var httpRequest = GetHttpRequest(uri, httpMethod, content, contentTypeRequest);

            var task = httpClient
                .SendAsync(httpRequest)
                .ContinueWith(taskwithmsg =>
                {
                    var httpResp = taskwithmsg.Result;
                    return httpResp;
                });
            task.Wait();
            var response = task.Result;


            string respContentAsString = string.Empty;
            try
            {
                //Read response asynchronously as JsonValue and write out top facts for each country
                if (_compressionType != CompressionType.None)
                { 
                    var respBytes = response.Content.ReadAsByteArrayAsync();
                    respBytes.Wait();
                    respContentAsString = Unzip(respBytes.Result);
                }
                else
                {
                    var respContent = response.Content.ReadAsStringAsync();
                    respContent.Wait();
                    respContentAsString = respContent.Result;
                }
                //Check that response was successful or throw exception
                response.EnsureSuccessStatusCode(); //releases response object if response code is "not successful"...
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, $"Api call exception: {respContentAsString}");
                throw new Exception(respContentAsString, ex);
            }
            return respContentAsString;
        }


        public string Get(Uri uri, IDictionary<string, string> headers = null, string contentTypeRequest = null, string contentTypeResponse = null)
            => ExecuteHttpRequest(HttpMethod.Get, null, uri, headers, contentTypeRequest, contentTypeResponse);

        public string Post(string content, Uri uri, IDictionary<string, string> headers = null, string contentTypeRequest = null, string contentTypeResponse = null)
            => ExecuteHttpRequest(HttpMethod.Post, content, uri, headers, contentTypeRequest, contentTypeResponse);

        public string Patch(string content, Uri uri, IDictionary<string, string> headers = null, string contentTypeRequest = null, string contentTypeResponse = null)
            => ExecuteHttpRequest(new HttpMethod("PATCH"), content, uri, headers, contentTypeRequest, contentTypeResponse);

    }
}
