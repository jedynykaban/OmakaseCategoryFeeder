using Fusion.Tests.Api.Client.CommLayer.ApiFusion;

namespace Signia.OmakaseCategoryFeeder.ApiClient.CommLayer.ApiFusion.Specialization
{
    public static class UtilsExtensions
    {
        public static string EchoRequest(this BaseApiRequestExecutor apiClient, string echoParam)
        {
            if (string.IsNullOrWhiteSpace(echoParam))
                throw new System.ArgumentNullException(nameof(echoParam));

            var reqRoutePath = "/api/v1/echo/{echoParam}";
            reqRoutePath = reqRoutePath.Replace("{echoParam}", echoParam);

            return apiClient.HttpGetDeserialized<string>(reqRoutePath);
        }
    }
}
