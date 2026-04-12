using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Importer.Core.Common
{
    public static class HttpClientUtils
    {
        public static HttpClient GetHttpClientWithTimeout(string baseUri = "")
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(baseUri))
            {
                client.BaseAddress = new Uri(baseUri);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var webRequestTimeOut = Environment.GetEnvironmentVariable("WebRequestTimeOut");
            client.Timeout = TimeSpan.FromMilliseconds(string.IsNullOrEmpty(webRequestTimeOut) ? 120000000: Convert.ToInt32(webRequestTimeOut));
            return client;
        }
        
        public static Func<System.Net.Http.HttpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool> GenerateServerCertificateCustomValidationCallback()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ALLOW_SELF_SIGNED_CERTIFICATES")) && bool.Parse(Environment.GetEnvironmentVariable("ALLOW_SELF_SIGNED_CERTIFICATES")))
            {
                return (message, cert, chain, errors) => true;
            }
            return null;
        }
    }
}
