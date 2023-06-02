using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crawlbase
{
    public class ScraperAPI : API
    {
        #region Properties

        public int RemainingRequests { get; private set; }

        #endregion

        #region Constructors

        public ScraperAPI(string token) : base(token)
        {
        }

        #endregion

        #region Methods

        public override void Post(string url, IDictionary data = null, IDictionary<string, object> options = null)
        {
            throw new Exception("Only GET is allowed for the ScraperAPI");
        }

        public override async Task PostAsync(string url, IDictionary data = null, IDictionary<string, object> options = null)
        {
            throw new Exception("Only GET is allowed for the ScraperAPI");
        }

        #endregion

        #region Helper Methods

        protected override string GetBaseUrl()
        {
            return "https://api.crawlbase.com/scraper";
        }

        protected override void ExtractResponse(HttpWebResponse response, string format)
        {
            base.ExtractResponse(response, "json");
        }

        protected override void ExtractJsonResponseBody(string body)
        {
            base.ExtractJsonResponseBody(body);
            var jobject = (JObject)JsonConvert.DeserializeObject(body);
            foreach (var token in jobject.Children())
            {
                string propertyName = token.Path;
                string value = token.Last?.ToString();
                if (propertyName == "remaining_requests")
                {
                    int _remainingRequests = 0;
                    if (int.TryParse(value, out _remainingRequests))
                    {
                        RemainingRequests = _remainingRequests;
                    }
                }
                else if (propertyName == "body")
                {
                    Body = value;
                }
            }
        }

        #endregion
    }
}