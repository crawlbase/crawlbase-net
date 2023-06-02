using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crawlbase
{
    public class LeadsAPI
    {
        #region Inner Class

        public class Lead
        {
            public string Email { get; private set; }
            public IList<string> Sources { get; private set; }

            internal Lead(string email, IList<string> sources)
            {
                Email = email;
                Sources = sources;
            }
        }

        #endregion

        #region Constants

        private const string INVALID_TOKEN = "Token is required";
        private const string INVALID_DOMAIN = "Domain is required";

        #endregion

        #region Properties

        public string Token { get; private set; }

        public string Body { get; private set; }

        public int StatusCode { get; private set; }

        public bool Success { get; private set; }

        public int RemainingRequests { get; private set; }

        public string Domain { get; private set; }

        public IList<Lead> Leads { get; private set; }

        #endregion

        #region Constructors

        public LeadsAPI(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception(INVALID_TOKEN);
            }
            Token = token;
        }

        #endregion

        #region Methods

        public bool Get(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                throw new Exception(INVALID_DOMAIN);
            }
            var uriBuilder = new UriBuilder("https://api.crawlbase.com/leads");
            var query = $"token={Uri.EscapeDataString(Token)}&domain={Uri.EscapeDataString(domain)}";
            uriBuilder.Query = query;
            var uri = uriBuilder.Uri;
            var request = CreateWebRequest(uri);
            var response = GetResponse(request);
            StatusCode = (int)response.StatusCode;
            Body = ReadResponseBody(response);
            try
            {
                ExtractJsonResponseFromBody();
                TryExtractLeadsFromBody();
                return Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task GetAsync(string domain)
        {
            await Task.Run(() =>
            {
                Get(domain);
            });
        }

        #endregion

        #region Helper Methods

        private HttpWebRequest CreateWebRequest(Uri uri)
        {
            return (HttpWebRequest)WebRequest.Create(uri);
        }

        private HttpWebResponse GetResponse(HttpWebRequest request)
        {
            return (HttpWebResponse)request.GetResponse();
        }

        protected virtual string ReadResponseBody(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void ExtractJsonResponseFromBody()
        {
            var jobject = (JObject)JsonConvert.DeserializeObject(Body);
            foreach (var token in jobject.Children())
            {
                string propertyName = token.Path;
                string value = token.Last?.ToString();
                if (propertyName == "success")
                {
                    bool _success = false;
                    bool.TryParse(value, out _success);
                    Success = _success;
                }
                else if (propertyName == "remaining_requests")
                {
                    int _remainingRequests = 0;
                    if (int.TryParse(value, out _remainingRequests))
                    {
                        RemainingRequests = _remainingRequests;
                    }
                }
                else if (propertyName == "domain")
                {
                    Domain = value;
                }
                else if (propertyName == "leads")
                {
                    Body = value;
                }
            }
        }

        private void TryExtractLeadsFromBody()
        {
            var leads = new List<Lead>();
            using (var reader = new JsonTextReader(new StringReader(Body)))
            {
                string email = null;
                List<string> sources = new List<string>();
                int arrayLevel = 0;
                int objectLevel = 0;
                string currentPropertyName = null;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        arrayLevel++;
                    }
                    else if (reader.TokenType == JsonToken.EndArray)
                    {
                        arrayLevel--;
                    }
                    else if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currentPropertyName = reader.Value.ToString();
                    }
                    else if (reader.TokenType == JsonToken.StartObject)
                    {
                        objectLevel++;
                    }
                    else if (reader.TokenType == JsonToken.EndObject)
                    {
                        if (objectLevel == 1 && arrayLevel == 1)
                        {
                            var lead = new Lead(email, sources.AsReadOnly());
                            leads.Add(lead);
                            email = null;
                            sources = new List<string>();
                        }
                        objectLevel--;
                    }
                    else if (currentPropertyName == "email" && arrayLevel == 1 && objectLevel == 1)
                    {
                        email = reader.Value.ToString();
                    }
                    else if (currentPropertyName == "sources" && arrayLevel == 2 && objectLevel == 1)
                    {
                        string source = reader.Value.ToString();
                        sources.Add(source);
                    }
                }
            }
            Leads = leads.AsReadOnly();
        }

        #endregion
    }
}
