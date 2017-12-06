using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Signia.OmakaseCategoryFeeder.ApiClient.Extensions
{
    public static class DtoExtensions
    {
        public static DtoWrapper<T> ToDtoWrapped<T>(this T dto)
        {
            return new DtoWrapper<T>(dto);
        }

        public static T ToDtoUnwrapped<T>(this DtoWrapper<T> dtoWrapped)
        {
            return dtoWrapped.WrappedObj;
        }

        public static string ToFormalizedJson<T>(this string dtoWrappedJson)
        {
            if (string.IsNullOrEmpty(dtoWrappedJson))
                return string.Empty;

            var propInfo = typeof (DtoWrapper<T>)
                .GetProperties()
                .FirstOrDefault(pi => pi.PropertyType == typeof(T));
            if (propInfo == null)
                return dtoWrappedJson;

            return dtoWrappedJson.Replace(propInfo.Name, typeof (T).Name);
        }

        public static string FromFormalizedJson<T>(this string dtoWrappedJson)
        {
            if (string.IsNullOrEmpty(dtoWrappedJson))
                return null;

            if (typeof (T).Name.IndexOf("WrappedObj", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                if (typeof (T).Name.Equals("WrappedObj", StringComparison.InvariantCultureIgnoreCase))
                    return dtoWrappedJson;
                throw new ArgumentException("Json cannot contains 'WrappedObj' substring in its content");
            }

            int index;
            while ((index = dtoWrappedJson.IndexOf(typeof(T).Name, StringComparison.InvariantCultureIgnoreCase)) != -1)
            {
                var strNeedle = dtoWrappedJson.Substring(index, typeof (T).Name.Length);
                dtoWrappedJson = dtoWrappedJson.Replace(strNeedle, "WrappedObj");
            }

            return dtoWrappedJson;
        }

        public static string ToFormalizedXml<T>(this string dtoxml, string httpMethodName)
        {
            if (string.IsNullOrEmpty(dtoxml))
                return string.Empty;

            switch (httpMethodName)
            {
                case "PATCH": // HttpMethods.Patch:
                    break;
                case "POST": // HttpMethods.Post:
                    httpMethodName = "Create";
                    break;
                case "PUT": // HttpMethods.Put:
                    httpMethodName = "Update";
                    break;
                default:
                    throw new ArgumentException($@"Not supported {nameof(httpMethodName)} parameter value: {httpMethodName}", nameof(httpMethodName));
            }

            httpMethodName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(httpMethodName.ToLower());

            var output = Regex.Replace(dtoxml, "<\\?[A-Za-z0-9_\"=.\\- ]*\\?>", "");
            var objName = typeof(T).Name;
            var firstCharIdxAfterObjName = output.IndexOf(objName, StringComparison.InvariantCulture) + objName.Length;

            var mainTagSufix = $"{httpMethodName}Request";
            var mainTagName = $"{objName}{mainTagSufix}";

            output = output.Insert(firstCharIdxAfterObjName, mainTagSufix);
            return (output.Insert(output.IndexOf('>') + 1, $"<{objName}>") + $"</{mainTagName}>").Replace("\\\"", "\"");
        }

        public static string UnwrapJson<T>(this string dtoWrappedJson)
        {
            if (string.IsNullOrEmpty(dtoWrappedJson))
                return null;

            if (typeof(T).Name.IndexOf("WrappedObj", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                if (typeof(T).Name.Equals("WrappedObj", StringComparison.InvariantCultureIgnoreCase))
                    return dtoWrappedJson;
                throw new ArgumentException("Json cannot contains 'WrappedObj' substring in its content");
            }

            var index = dtoWrappedJson.IndexOf(typeof (T).Name, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return dtoWrappedJson;

            index += typeof (T).Name.Length;
            return dtoWrappedJson.Substring(index + 2, dtoWrappedJson.Length - index - 3);
        }

    }

    public class DtoWrapper<T>
    {
        public DtoWrapper(T wrappedObj)
        {
            WrappedObj = wrappedObj;
        }
        public T WrappedObj { get; }
    }
}
