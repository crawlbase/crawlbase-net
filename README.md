# Crawlbase

.NET library for scraping and crawling websites using the Crawlbase API.

## Installation

See [nuget package](https://www.nuget.org/packages/CrawlbaseAPI/)

## Asynchronous Programming

Every method has a corresponding async version. 

i.e. 

`Get` has an async version  `GetAsync` while, 

`Post` has an async version named `PostAsync`, 

and so on...

## Crawling API Usage

Initialize the API with one of your account tokens, either normal or javascript token. Then make get or post requests accordingly.

You can get a token for free by [creating a Crawlbase account](https://crawlbase.com/signup) and 1000 free testing requests. You can use them for tcp calls or javascript calls or both.

```csharp
Crawlbase.API api = new Crawlbase.API("YOUR_TOKEN");
```

### GET requests

Pass the url that you want to scrape plus any options from the ones available in the [API documentation](https://crawlbase.com/dashboard/docs).

```csharp
api.Get(url, options);
```

Example:

```csharp
try {
  api.Get("https://www.facebook.com/britneyspears");
  Console.WriteLine(api.StatusCode);
  Console.WriteLine(api.OriginalStatus);
  Console.WriteLine(api.CrawlbaseStatus);
  Console.WriteLine(api.Body);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

You can pass any options of what the Crawlbase API supports in exact dictionary params format.

Example:

```csharp
api.Get("https://www.reddit.com/r/pics/comments/5bx4bx/thanks_obama/", new Dictionary<string, object>() {
  {"user_agent", "Mozilla/5.0 (Windows NT 6.2; rv:20.0) Gecko/20121202 Firefox/30.0"},
  {"format", "json"},
});

Console.WriteLine(api.StatusCode);
Console.WriteLine(api.Body);
```

Optionally pass [store](https://crawlbase.com/docs/crawling-api/parameters/#store) parameter to `true` to store a copy of the API response in the [Crawlbase Cloud Storage](https://crawlbase.com/dashboard/storage).

Example:

```csharp
api.Get("https://www.reddit.com/r/pics/comments/5bx4bx/thanks_obama/", new Dictionary<string, object>() {
  {"store", "true"},
});

Console.WriteLine(api.StorageURL);
Console.WriteLine(api.StorageRID);
```

### POST requests

Pass the url that you want to scrape, the data that you want to send which can be either a json or a string, plus any options from the ones available in the [API documentation](https://crawlbase.com/dashboard/docs).

```csharp
api.Post(url, data, options);
```

Example:

```csharp
api.Post("https://producthunt.com/search", new Dictionary<string, object>() {
  {"text", "example search"},
});
Console.WriteLine(api.StatusCode);
Console.WriteLine(api.Body);
```

You can send the data as application/json instead of x-www-form-urlencoded by setting options `post_content_type` as json.

```csharp
api.Post("https://httpbin.org/post", new Dictionary<string, object>() {
  {"some_json", "with some value"},
}, new Dictionary<string, object>() {
  {"post_content_type", "json"},
});
Console.WriteLine(api.StatusCode);
Console.WriteLine(api.Body);
```

### Javascript requests

If you need to scrape any website built with Javascript like React, Angular, Vue, etc. You just need to pass your javascript token and use the same calls. Note that only `Get` is available for javascript and not `Post`.

```csharp
Crawlbase.API api = new Crawlbase.API("YOUR_JAVASCRIPT_TOKEN");
```

```csharp
api.Get("https://www.nfl.com");
Console.WriteLine(api.StatusCode);
Console.WriteLine(api.Body);
```

Same way you can pass javascript additional options.

```csharp
api.Get("https://www.freelancer.com", new Dictionary<string, object>() {
  {"page_wait", "5000"},
});
Console.WriteLine(api.StatusCode);
```

## Original status

You can always get the original status and crawlbase status from the response. Read the [Crawlbase documentation](https://crawlbase.com/dashboard/docs) to learn more about those status.

```csharp
api.Get("https://sfbay.craigslist.org/");
Console.WriteLine(api.OriginalStatus);
Console.WriteLine(api.CrawlbaseStatus);
```

## Scraper API usage

Initialize the Scraper API using your normal token and call the `Get` method.

```csharp
Crawlbase.ScraperAPI scraper_api = new Crawlbase.ScraperAPI("YOUR_TOKEN");
```

Pass the url that you want to scrape plus any options from the ones available in the [Scraper API documentation](https://crawlbase.com/docs/scraper-api/parameters).

```csharp
scraper_api.Get(url, options);
```

Example:

```csharp
try {
  scraper_api.Get("https://www.amazon.com/Halo-SleepSack-Swaddle-Triangle-Neutral/dp/B01LAG1TOS");
  Console.WriteLine(scraper_api.StatusCode);
  Console.WriteLine(scraper_api.Body);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

## Leads API usage

Initialize with your Leads API token and call the `Get` method.

```csharp
Crawlbase.LeadsAPI leads_api = new Crawlbase.LeadsAPI("YOUR_TOKEN");

try {
  leads_api.Get("stripe.com");
  Console.WriteLine(leads_api.StatusCode);
  Console.WriteLine(leads_api.Body);
  Console.WriteLine(leads_api.Success);
  Console.WriteLine(leads_api.RemainingRequests);

  foreach (var lead in leads_api.Leads)
  {
    Console.WriteLine(lead.Email);

    foreach (var source in lead.Sources)
    {
      Console.WriteLine(source);
    }
  }
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

If you have questions or need help using the library, please open an issue or [contact us](https://crawlbase.com/contact).

## Screenshots API usage

Initialize with your Screenshots API token and call the `Get` method.

```csharp
Crawlbase.ScreenshotsAPI screenshots_api = new Crawlbase.ScreenshotsAPI("YOUR_TOKEN");

try {
  screenshots_api.Get("https://www.apple.com");
  Console.WriteLine(screenshots_api.StatusCode);
  Console.WriteLine(screenshots_api.ScreenshotPath);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

or specifying a file path

```csharp
Crawlbase.ScreenshotsAPI screenshots_api = new Crawlbase.ScreenshotsAPI("YOUR_TOKEN");

try {
  screenshots_api.Get("https://www.apple.com", new Dictionary<string, object>() {
    {"save_to_path", @"C:\Users\Default\Documents\apple.jpg"},
  });
  Console.WriteLine(screenshots_api.StatusCode);
  Console.WriteLine(screenshots_api.ScreenshotPath);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

Note that `screenshots_api.Get(url, options)` method accepts an [options](https://crawlbase.com/docs/screenshots-api/parameters)

Also note that `screenshots_api.Body` is a Base64 string representation of the binary image file.
If you want to convert the body to bytes then you have to do the following:

```csharp
byte[] bytes = Convert.FromBase64String(screenshots_api.Body);
```

## Storage API usage

Initialize the Storage API using your private token.

```csharp
Crawlbase.StorageAPI storage_api = new Crawlbase.StorageAPI("YOUR_TOKEN");
```

Pass the [url](https://crawlbase.com/docs/storage-api/parameters/#url) that you want to get from [Crawlbase Storage](https://crawlbase.com/dashboard/storage).

```csharp
try {
  var response = storage_api.GetByUrl("https://www.apple.com");
  Console.WriteLine(storage_api.StatusCode);
  Console.WriteLine(storage_api.Body);
  Console.WriteLine(response.OriginalStatus);
  Console.WriteLine(response.CrawlbaseStatus);
  Console.WriteLine(response.URL);
  Console.WriteLine(response.RID);
  Console.WriteLine(response.StoredAt);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

or you can use the [RID](https://crawlbase.com/docs/storage-api/parameters/#rid)

```csharp
try {
  var response = storage_api.GetByRID(RID);
  Console.WriteLine(storage_api.StatusCode);
  Console.WriteLine(storage_api.Body);
  Console.WriteLine(response.OriginalStatus);
  Console.WriteLine(response.CrawlbaseStatus);
  Console.WriteLine(response.URL);
  Console.WriteLine(response.RID);
  Console.WriteLine(response.StoredAt);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

### [Delete](https://crawlbase.com/docs/storage-api/delete/) request

To delete a storage item from your storage area, use the correct RID

```csharp
try {
  bool success = storage_api.Delete(RID);
  Console.WriteLine(success);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

### [Bulk](https://crawlbase.com/docs/storage-api/bulk/) request

To do a bulk request with a list of RIDs, please send the list of rids as an array

```csharp
try {
  var list = new List<string>();
  list.Add(RID1);
  list.Add(RID2);
  list.Add(RIDn);
  var responses = storage_api.Bulk(list);
  Console.WriteLine(storage_api.StatusCode);
  foreach (var response in responses)
  {
    Console.WriteLine(response.OriginalStatus);
    Console.WriteLine(response.CrawlbaseStatus);
    Console.WriteLine(response.URL);
    Console.WriteLine(response.RID);
    Console.WriteLine(response.StoredAt);
    Console.WriteLine(response.Body);
  }
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

### [RIDs](https://crawlbase.com/docs/storage-api/rids) request

To request a bulk list of RIDs from your storage area

```csharp
try {
  var rids = storage_api.RIDs();
  foreach (var rid in rids)
  {
    Console.WriteLine(rid);
  }
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

You can also specify a limit as a parameter

```csharp
var rids = storage_api.RIDs(100);
```

### [Total Count](https://crawlbase.com/docs/storage-api/total_count)

To get the total number of documents in your storage area

```csharp
try {
  var totalCount = storage_api.TotalCount();
  Console.WriteLine(totalCount);
} catch(Exception ex) {
  Console.WriteLine(ex.ToString());
}
```

If you have questions or need help using the library, please open an issue or [contact us](https://crawlbase.com/contact).

## Contributing

Bug reports and pull requests are welcome on GitHub at https://github.com/crawlbase-source/crawlbase-net. This project is intended to be a safe, welcoming space for collaboration, and contributors are expected to adhere to the [Contributor Covenant](http://contributor-covenant.org) code of conduct.

## License

The library is available as open source under the terms of the [MIT License](http://opensource.org/licenses/MIT).

## Code of Conduct

Everyone interacting in the Crawlbase project’s codebases, issue trackers, chat rooms and mailing lists is expected to follow the [code of conduct](https://github.com/crawlbase-source/crawlbase-net/blob/master/CODE_OF_CONDUCT.md).

---

Copyright 2024 Crawlbase
