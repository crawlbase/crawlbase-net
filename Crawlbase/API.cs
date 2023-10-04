using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Crawlbase
{
    public class API
    {
        #region Constants

        private const string INVALID_TOKEN = "Token is required";
        private const string INVALID_URL = "URL is required";

        #endregion

        #region Properties

        public string Token { get; protected set; }

        public string Body { get; protected set; }

        public int? StatusCode { get; protected set; }

        public int? OriginalStatus { get; protected set; }

        public int? CrawlbaseStatus { get; protected set; }

        public string URL { get; protected set; }

        public string StorageURL { get; protected set; }

        public string StorageRID { get; protected set; }

        #endregion

        #region Constructors

        public API(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception(INVALID_TOKEN);
            }
            Token = token;
        }

        #endregion

        #region Methods

        public virtual void Get(string url, IDictionary<string, object> options = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception(INVALID_URL);
            }
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }
            string format = null;
            if (options.ContainsKey("format") && options["format"] != null)
            {
                format = options["format"].ToString();
            }
            var uri = PrepareUri(url, options);
            var request = CreateWebRequest(uri);
            var response = GetResponse(request);
            ExtractResponse(response, format);
        }

        public virtual async Task GetAsync(string url, IDictionary<string, object> options = null)
        {
            await Task.Run(() =>
            {
                Get(url, options);
            });
        }

        public virtual void Post(string url, IDictionary data = null, IDictionary<string, object> options = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception(INVALID_URL);
            }
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }
            string format = null;
            if (options.ContainsKey("format") && options["format"] != null)
            {
                format = options["format"].ToString();
            }
            var uri = PrepareUri(url, options);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            if (format == "json")
            {
                ConfigureJsonPostRequest(request, data);
            }
            else
            {
                ConfigurePostRequest(request, data);
            }
            var response = (HttpWebResponse)request.GetResponse();
            ExtractResponse(response, format);
        }

        public virtual async Task PostAsync(string url, IDictionary data = null, IDictionary<string, object> options = null)
        {
            await Task.Run(() =>
            {
                Post(url, data, options);
            });
        }

        #endregion

        #region Helper Methods

        protected virtual string GetBaseUrl()
        {
            return "https://api.crawlbase.com";
        }

        private HttpWebRequest CreateWebRequest(Uri uri)
        {
            return (HttpWebRequest)WebRequest.Create(uri);
        }

        private HttpWebResponse GetResponse(HttpWebRequest request)
        {
            return (HttpWebResponse)request.GetResponse();
        }

        private Uri PrepareUri(string url, IDictionary<string, object> options)
        {
            if (options.ContainsKey("url"))
            {
                options.Remove("url");
            }
            options["url"] = Uri.EscapeDataString(url);
            if (options.ContainsKey("token"))
            {
                options.Remove("token");
            }
            options["token"] = Token;
            var uriBuilder = new UriBuilder(GetBaseUrl());
            var optionList = new List<string>();
            foreach (var key in options.Keys)
            {
                object value = options[key];
                value = HandleBooleanValue(value);
                optionList.Add($"{key}={value}");
            }
            var query = string.Join("&", optionList.ToArray());
            uriBuilder.Query = query;
            return uriBuilder.Uri;
        }

        protected virtual void ConfigureJsonPostRequest(HttpWebRequest request, IDictionary data)
        {
            if (data.Count == 0) return;
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            request.ContentType = "application/json";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
            }
        }

        protected virtual void ConfigurePostRequest(HttpWebRequest request, IDictionary data)
        {
            if (data.Count == 0) return;

            var dataList = new List<string>();
            foreach (var key in data.Keys)
            {
                object value = data[key];
                value = HandleBooleanValue(value);
                value = Uri.EscapeDataString(value.ToString());
                dataList.Add($"{key}={value}");
            }
            var postData = string.Join("&", dataList.ToArray());
            var encodedData = Encoding.ASCII.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = encodedData.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(encodedData, 0, encodedData.Length);
            }
        }

        protected virtual void ExtractResponse(HttpWebResponse response, string format)
        {
            string body = ReadResponseBody(response);
            StatusCode = (int)response.StatusCode;
            if (format == "json")
            {
                ExtractJsonResponseBody(body);
            }
            else
            {
                ExtractResponseBody(response, body);
            }
            TryParseStorageRID();
        }

        protected virtual void ExtractJsonResponseBody(string body)
        {
            using (var reader = new JsonTextReader(new StringReader(body)))
            {
                string currentPropertyName = null;
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            currentPropertyName = reader.Value.ToString();
                        }
                        else if (currentPropertyName == "original_status")
                        {
                            int _originalStatus = 0;
                            if (int.TryParse(reader.Value.ToString(), out _originalStatus))
                            {
                                OriginalStatus = _originalStatus;
                            }
                        }
                        else if (currentPropertyName == "cb_status")
                        {
                            int _cbStatus = 0;
                            if (int.TryParse(reader.Value.ToString(), out _cbStatus))
                            {
                                CrawlbaseStatus = _cbStatus;
                            }
                        }
                        else if (currentPropertyName == "pc_status" && null == CrawlbaseStatus)
                        {
                            int _pcStatus = 0;
                            if (int.TryParse(reader.Value.ToString(), out _pcStatus))
                            {
                                CrawlbaseStatus = _pcStatus;
                            }
                        }
                        else if (currentPropertyName == "url")
                        {
                            URL = reader.Value.ToString();
                        }
                        else if (currentPropertyName == "storage_url")
                        {
                            StorageURL = reader.Value.ToString();
                        }
                    }
                }
            }
            Body = body;
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

        protected virtual void ExtractResponseBody(HttpWebResponse response, string body)
        {
            int _originalStatus = 0;
            if (int.TryParse(response.Headers["original_status"], out _originalStatus))
            {
                OriginalStatus = _originalStatus;
            }
            int _cbStatus = 0;
            if (int.TryParse(response.Headers["cb_status"], out _cbStatus))
            {
                CrawlbaseStatus = _cbStatus;
            } 
            else if (int.TryParse(response.Headers["pc_status"], out _cbStatus))
            {
                CrawlbaseStatus = _cbStatus;
            }
            URL = response.Headers["url"];
            StorageURL = response.Headers["storage_url"];
            Body = body;
        }

        private void TryParseStorageRID()
        {
            if (string.IsNullOrEmpty(StorageURL)) return;

            var rx = new Regex(@"rid=(\w+)");
            if (rx.IsMatch(StorageURL))
            {
                var match = rx.Match(StorageURL);
                try { StorageRID = match.Groups[1].Value; } catch { }
            }
        }

        private object HandleBooleanValue(object value)
        {
            if (value.GetType() == typeof(Boolean))
            {
                value = value.ToString().ToLower();
            }

            return value;
        }

        #endregion
    }
}
