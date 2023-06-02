using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Crawlbase
{
    public class StorageAPI
    {
        #region Inner Class

        public class Response
        {
            public int? OriginalStatus { get; internal set; }

            public int? CrawlbaseStatus { get; internal set; }

            public string URL { get; internal set; }

            public string RID { get; internal set; }

            public DateTime? StoredAt { get; internal set; }

            public string Body { get; internal set; }
        }

        #endregion

        #region Constants

        private const string INVALID_TOKEN = "Token is required";
        private const string INVALID_RID = "RID is required";
        private const string INVALID_RID_ARRAY = "One or more RIDs are required";
        private const string INVALID_URL = "URL is required";
        private const int NO_LIMIT = -1;
        private const string BASE_URL = "https://api.crawlbase.com/storage";

        #endregion

        #region Properties

        public string Token { get; protected set; }

        public int? StatusCode { get; protected set; }

        public string Body { get; protected set; }

        #endregion

        #region Constructors

        public StorageAPI(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception(INVALID_TOKEN);
            }
            Token = token;
        }

        #endregion

        #region Methods

        public Response GetByUrl(string url, string format = "html")
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception(INVALID_URL);
            }

            var uriBuilder = new UriBuilder(BASE_URL);
            var optionList = new List<string>();
            optionList.Add($"token={Token}");
            optionList.Add($"url={Uri.EscapeDataString(url)}");
            optionList.Add($"format={format}");
            var query = string.Join("&", optionList.ToArray());
            uriBuilder.Query = query;

            var request = CreateWebRequest(uriBuilder.Uri);
            var response = GetResponse(request);

            StatusCode = (int)response.StatusCode;
            int? originalStatus = null;
            int _originalStatus = 0;
            if (int.TryParse(response.Headers["original_status"], out _originalStatus))
            {
                originalStatus = _originalStatus;
            }
            int? crawlbaseStatus = null;
            int _pcStatus = 0;
            if (int.TryParse(response.Headers["pc_status"], out _pcStatus))
            {
                crawlbaseStatus = _pcStatus;
            }
            string returnedUrl = response.Headers["url"];
            string returnedRid = response.Headers["rid"];
            DateTime? storedAt = null;
            DateTime _storedAt = DateTime.Now;
            if (DateTime.TryParse(response.Headers["stored_at"], out _storedAt))
            {
                storedAt = _storedAt;
            }
            Body = ReadResponseBody(response);

            return new Response()
            {
                OriginalStatus = originalStatus,
                CrawlbaseStatus = crawlbaseStatus,
                URL = returnedUrl,
                RID = returnedRid,
                StoredAt = storedAt,
                Body = Body
            };
        }

        public async Task<Response> GetByUrlAsync(string url, string format = "html")
        {
            return await Task<Response>.Run(() =>
            {
                return GetByUrl(url, format);
            });
        }

        public Response GetByRID(string rid, string format = "html")
        {
            if (string.IsNullOrEmpty(rid))
            {
                throw new Exception(INVALID_RID);
            }

            var uriBuilder = new UriBuilder(BASE_URL);
            var optionList = new List<string>();
            optionList.Add($"token={Token}");
            optionList.Add($"rid={rid}");
            optionList.Add($"format={format}");
            var query = string.Join("&", optionList.ToArray());
            uriBuilder.Query = query;

            var request = CreateWebRequest(uriBuilder.Uri);
            var response = GetResponse(request);

            StatusCode = (int)response.StatusCode;
            int? originalStatus = null;
            int _originalStatus = 0;
            if (int.TryParse(response.Headers["original_status"], out _originalStatus))
            {
                originalStatus = _originalStatus;
            }
            int? crawlbaseStatus = null;
            int _pcStatus = 0;
            if (int.TryParse(response.Headers["pc_status"], out _pcStatus))
            {
                crawlbaseStatus = _pcStatus;
            }
            string returnedUrl = response.Headers["url"];
            string returnedRid = response.Headers["rid"];
            DateTime? storedAt = null;
            DateTime _storedAt = DateTime.Now;
            if (DateTime.TryParse(response.Headers["stored_at"], out _storedAt))
            {
                storedAt = _storedAt;
            }
            Body = ReadResponseBody(response);

            return new Response()
            {
                OriginalStatus = originalStatus,
                CrawlbaseStatus = crawlbaseStatus,
                URL = returnedUrl,
                RID = returnedRid,
                StoredAt = storedAt,
                Body = Body
            };
        }

        public async Task<Response> GetByRIDAsync(string url, string format = "html")
        {
            return await Task<Response>.Run(() =>
            {
                return GetByRID(url, format);
            });
        }

        public bool Delete(string rid)
        {
            bool success = false;

            if (string.IsNullOrEmpty(rid))
            {
                throw new Exception(INVALID_RID);
            }

            var uriBuilder = new UriBuilder(BASE_URL);
            var optionList = new List<string>();
            optionList.Add($"token={Token}");
            optionList.Add($"rid={rid}");
            var query = string.Join("&", optionList.ToArray());
            uriBuilder.Query = query;

            var request = CreateWebRequest(uriBuilder.Uri);
            request.Method = "DELETE";
            var response = GetResponse(request);

            StatusCode = (int)response.StatusCode;
            Body = ReadResponseBody(response);

            using (var reader = new JsonTextReader(new StringReader(Body)))
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
                        else if (currentPropertyName == "success")
                        {
                            success = true;
                        }
                    }
                }
            }

            return success;
        }

        public async Task<bool> DeleteAsync(string rid)
        {
            return await Task<bool>.Run(() =>
            {
                return Delete(rid);
            });
        }

        public Response[] Bulk(IList<string> rids)
        {
            var responses = new List<Response>();

            if (rids == null || rids.Count == 0)
            {
                throw new Exception(INVALID_RID_ARRAY);
            }

            var uriBuilder = new UriBuilder($"{BASE_URL}/bulk");
            var optionList = new List<string>();
            optionList.Add($"token={Token}");
            var query = string.Join("&", optionList.ToArray());
            uriBuilder.Query = query;

            var request = CreateWebRequest(uriBuilder.Uri);
            request.Method = "POST";
            var data = new Dictionary<string, IList<string>>()
            {
                { "rids", rids }
            };
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            request.ContentType = "application/json";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
            }
            var response = GetResponse(request);

            StatusCode = (int)response.StatusCode;
            Body = ReadResponseBody(response);

            using (var reader = new JsonTextReader(new StringReader(Body)))
            {
                int? originalStatus = null;
                int? crawlbaseStatus = null;
                string returnedUrl = null;
                string returnedRid = null;
                string returnedBody = null;
                DateTime? storedAt = null;

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
                            var _response = new Response()
                            {
                                OriginalStatus = originalStatus,
                                CrawlbaseStatus = crawlbaseStatus,
                                URL = returnedUrl,
                                RID = returnedRid,
                                StoredAt = storedAt,
                                Body = returnedBody
                            };
                            responses.Add(_response);
                        }
                        objectLevel--;
                    }
                    else if (currentPropertyName == "original_status" && arrayLevel == 1 && objectLevel == 1)
                    {
                        int _originalStatus = 0;
                        if (int.TryParse(reader.Value.ToString(), out _originalStatus))
                        {
                            originalStatus = _originalStatus;
                        }
                    }
                    else if (currentPropertyName == "pc_status" && arrayLevel == 1 && objectLevel == 1)
                    {
                        int _pcStatus = 0;
                        if (int.TryParse(reader.Value.ToString(), out _pcStatus))
                        {
                            crawlbaseStatus = _pcStatus;
                        }
                    }
                    else if (currentPropertyName == "url" && arrayLevel == 1 && objectLevel == 1)
                    {
                        returnedUrl = reader.Value.ToString();
                    }
                    else if (currentPropertyName == "rid" && arrayLevel == 1 && objectLevel == 1)
                    {
                        returnedRid = reader.Value.ToString();
                    }
                    else if (currentPropertyName == "stored_at" && arrayLevel == 1 && objectLevel == 1)
                    {
                        DateTime _storedAt = DateTime.Now;
                        if (DateTime.TryParse(reader.Value.ToString(), out _storedAt))
                        {
                            storedAt = _storedAt;
                        }
                    }
                    else if (currentPropertyName == "body" && arrayLevel == 1 && objectLevel == 1)
                    {
                        returnedBody = reader.Value.ToString();
                    }
                }
            }

            return responses.ToArray();
        }

        public async Task<Response[]> BulkAsync(IList<string> rids)
        {
            return await Task<Response[]>.Run(() =>
            {
                return Bulk(rids);
            });
        }

        public string[] RIDs(int limit = NO_LIMIT)
        {
            var rids = new List<string>();

            var uriBuilder = new UriBuilder($"{BASE_URL}/rids");
            var optionList = new List<string>();
            optionList.Add($"token={Token}");
            if (limit >= 0)
            {
                optionList.Add($"limit={limit}");
            }
            var query = string.Join("&", optionList.ToArray());
            uriBuilder.Query = query;

            var request = CreateWebRequest(uriBuilder.Uri);
            var response = GetResponse(request);

            StatusCode = (int)response.StatusCode;
            Body = ReadResponseBody(response);

            using (var reader = new JsonTextReader(new StringReader(Body)))
            {
                int arrayLevel = 0;
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
                    else if (reader.TokenType == JsonToken.String && arrayLevel == 1)
                    {
                        rids.Add(reader.Value.ToString());
                    }
                }
            }

            return rids.ToArray();
        }

        public async Task<string[]> RIDsAsync(int limit = NO_LIMIT)
        {
            return await Task<string[]>.Run(() =>
            {
                return RIDs(limit);
            });
        }

        public int TotalCount()
        {
            var totalCount = 0;

            var uriBuilder = new UriBuilder($"{BASE_URL}/total_count");
            var optionList = new List<string>();
            optionList.Add($"token={Token}");
            var query = string.Join("&", optionList.ToArray());
            uriBuilder.Query = query;

            var request = CreateWebRequest(uriBuilder.Uri);
            var response = GetResponse(request);

            StatusCode = (int)response.StatusCode;
            Body = ReadResponseBody(response);

            using (var reader = new JsonTextReader(new StringReader(Body)))
            {
                int objectLevel = 0;
                string currentPropertyName = null;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        objectLevel++;
                    }
                    else if (reader.TokenType == JsonToken.EndObject)
                    {
                        objectLevel--;
                    }
                    else if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currentPropertyName = reader.Value.ToString();
                    }
                    else if (currentPropertyName == "totalCount" && objectLevel == 1)
                    {
                        int _totalCount = 0;
                        if (int.TryParse(reader.Value.ToString(), out _totalCount))
                        {
                            totalCount = _totalCount;
                        }
                    }
                }
            }

            return totalCount;
        }

        public async Task<int> TotalCountAsync()
        {
            return await Task<int>.Run(() =>
            {
                return TotalCount();
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

        #endregion
    }
}
