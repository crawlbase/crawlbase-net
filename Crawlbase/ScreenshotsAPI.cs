using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;

namespace Crawlbase
{
    public class ScreenshotsAPI : API
    {
        #region Constants

        private const string INVALID_SAVE_TO_PATH_FILENAME = "Filename must end with .jpg or .jpeg";
        private const string SAVE_TO_PATH_FILENAME_PATTERN = @".+\.(jpg|JPG|jpeg|JPEG)$";
        private const string SAVE_TO_PATH_KEY = "save_to_path";

        #endregion

        #region Properties

        public string ScreenshotPath { get; private set; }

        public bool Success { get; private set; }

        public int RemainingRequests { get; private set; }

        public string ScreenshotUrl { get; private set; }

        #endregion

        #region Constructors

        public ScreenshotsAPI(string token) : base(token)
        {
        }

        #endregion

        #region Methods

        public override void Post(string url, IDictionary data = null, IDictionary<string, object> options = null)
        {
            throw new Exception("Only GET is allowed for the ScreenshotsAPI");
        }

        public override async Task PostAsync(string url, IDictionary data = null, IDictionary<string, object> options = null)
        {
            throw new Exception("Only GET is allowed for the ScraperAPI");
        }

        public override void Get(string url, IDictionary<string, object> options = null)
        {
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }
            string screenshotPath = null;
            if (options.ContainsKey(SAVE_TO_PATH_KEY))
            {
                screenshotPath = options[SAVE_TO_PATH_KEY].ToString();
                options.Remove(SAVE_TO_PATH_KEY);
            }
            else
            {
                screenshotPath = GenerateFilePath();
            }
            ScreenshotPath = screenshotPath;
            var regex = new Regex(SAVE_TO_PATH_FILENAME_PATTERN);
            if (!regex.IsMatch(screenshotPath))
            {
                throw new Exception(INVALID_SAVE_TO_PATH_FILENAME);
            }
            base.Get(url, options);
        }

        #endregion

        #region Helper Methods

        protected override string GetBaseUrl()
        {
            return "https://api.crawlbase.com/screenshots";
        }

        protected override void ExtractResponse(HttpWebResponse response, string format)
        {
            base.ExtractResponse(response, null);
        }

        protected override string ReadResponseBody(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                using (var fileStream = File.Create(ScreenshotPath))
                {
                    int iStrmByte;
                    while ((iStrmByte = stream.ReadByte()) != -1)
                    {
                        fileStream.WriteByte(Convert.ToByte(iStrmByte));
                    }
                }
            }
            byte[] bytes = File.ReadAllBytes(ScreenshotPath);
            return Convert.ToBase64String(bytes);
        }

        protected override void ExtractResponseBody(HttpWebResponse response, string body)
        {
            base.ExtractResponseBody(response, body);
            int _remainingRequests = 0;
            if (int.TryParse(response.Headers["remaining_requests"], out _remainingRequests))
            {
                RemainingRequests = _remainingRequests;
            }
            bool _success = false;
            bool.TryParse(response.Headers["success"], out _success);
            Success = _success;
            ScreenshotUrl = response.Headers["screenshot_url"];
        }

        private string GenerateFilename()
        {
            return $@"{Guid.NewGuid()}.jpg";
        }

        private string GenerateFilePath()
        {
            return Path.Combine(Path.GetTempPath(), GenerateFilename());
        }

        #endregion
    }
}
