using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace youtuber.Net.Youtube.Official
{
    public sealed class Search : ApiBase
    {
        private const string baseUrl = "https://www.googleapis.com/youtube/v3/search";
        private string requestedUrl;
        private dynamic json;
        private string nextPageToken;

        public Search(string apiKey) : base(apiKey){
            Results = new List<Result>();
        }

        public Search(string apiKey, CookieCollection cookies) : base(apiKey, cookies){
            Results = new List<Result>();
        }

        public string ETag {get; private set;}

        public bool HasNextPage
        {
            get
            {
                try {
                    nextPageToken = json.nextPageToken;
                    return !string.IsNullOrEmpty(nextPageToken);
                } catch (Exception) { return false; }
            }
        }

        public string RegionCode {get; private set;}
        public int TotalResults {get; private set;}
        public int ResultsPerPage {get; private set;}
        public List<Result> Results {get;}

        private async Task<List<Result>> Execute(Uri uri){
            json = await GetResponse(uri);
            List<Result> results = new List<Result>();
            try {
                ETag = json.etag;
                RegionCode = json.regionCode;
                TotalResults = int.Parse(json.pageInfo.totalResults.ToString());
                ResultsPerPage = int.Parse(json.pageInfo.resultsPerPage.ToString());
                JArray items = json.items;
                foreach (dynamic item in items) { results.Add(Result.FromJson(item)); }
            } catch (Exception e) {
                Success = false;
                return new List<Result>();
            }
            Results.AddRange(results);
            return results;
        }

        public async Task<List<Result>> Execute(string query, string[] args){
            requestedUrl =
                $"{baseUrl}?q={WebUtility.UrlEncode(query)}&{string.Join("&", args)}&part=id,snippet&key={ApiKey}";
            return await Execute(new Uri(requestedUrl));
        }

        public async Task<List<Result>> Execute(string query){
            requestedUrl = $"{baseUrl}?q={WebUtility.UrlEncode(query)}&part=id,snippet&key={ApiKey}";
            return await Execute(new Uri(requestedUrl));
        }

        public async Task<List<Result>> GetNextPage(){
            if (HasNextPage) { return await Execute(new Uri($"{requestedUrl}&{Params.PageToken(nextPageToken)}")); }
            return new List<Result>();
        }

        public class Result
        {
            protected dynamic Json;
            public string ETag {get;}
            public DateTime PublishedAt {get;}
            public string ChannelID {get;}
            public string Title {get;}
            public string Description {get;}
            public List<Image> Thumbnails {get;}
            public string ChannelTitle {get;}
            public string LiveBroadcastContent {get;}

            protected Result(Object json){
                Json = json;
                ETag = Json.etag.ToString();
                dynamic snippet = Json.snippet;
                PublishedAt = DateTime.Parse(snippet.publishedAt.ToString());
                ChannelID = snippet.channelId.ToString();
                Title = snippet.title.ToString();
                Description = snippet.description.ToString();
                Thumbnails = new List<Image>();
                Thumbnails.Add(new Image(snippet.thumbnails.@default, "default"));
                Thumbnails.Add(new Image(snippet.thumbnails.medium, "medium"));
                Thumbnails.Add(new Image(snippet.thumbnails.high, "high"));
                ChannelTitle = snippet.channelTitle.ToString();
                LiveBroadcastContent = snippet.liveBroadcastContent.ToString();
            }

            internal static Result FromJson(dynamic json){
                switch ((string) (json.id.kind.ToString())) {
                    case "youtube#video":
                        return Video.FromJson(json);
                    case "youtube#playlist":
                        return Playlist.FromJson(json);
                    case "youtube#channel":
                        return Channel.FromJson(json);
                    default:
                        return new Result(json);
                }
            }

            public override string ToString(){
                return Title;
            }

            public class Video : Result
            {
                protected Video(Object json) : base(json){
                    VideoID = Json.id.videoId.ToString();
                }

                public string VideoID {get;}

                internal new static Result FromJson(dynamic json){
                    return new Video(json);
                }

                public override string ToString(){
                    return $"Video: {base.ToString()}";
                }
            }

            public class Playlist : Result
            {
                protected Playlist(Object json) : base(json){
                    PlaylistID = Json.id.playlistId.ToString();
                }

                public string PlaylistID {get;}

                internal new static Result FromJson(dynamic json){
                    return new Playlist(json);
                }

                public override string ToString(){
                    return $"Playlist: {base.ToString()}";
                }
            }

            public class Channel : Result
            {
                protected Channel(Object json) : base(json){
                    ChannelID = Json.id.channelId.ToString();
                }

                public string ChannelID {get;}

                internal new static Result FromJson(dynamic json){
                    return new Channel(json);
                }

                public override string ToString(){
                    return $"Channel: {base.ToString()}";
                }
            }
        }

        public class Image
        {
            protected dynamic Json;

            public Uri Url {get;}
            public int Width {get;}
            public int Height {get;}
            public string Type {get;}

            internal Image(Object json, string type){
                Json = json;
                Type = type;
                Url = new Uri(Json.url.ToString());
                try {
                    Width = int.Parse(Json.width.ToString());
                    Height = int.Parse(Json.height.ToString());
                } catch (Exception) { Width = Height = 0; }
            }

            public override string ToString(){
                return $"{Type}: {Width}x{Height}";
            }
        }

        public static class Params
        {
            public static string ChannelID(string channelId){
                return $"channelId={channelId}";
            }

            public static string ChannelType(ChannelTypes channelType){
                string prefix = "channelType=";
                switch (channelType) {
                    case ChannelTypes.Any:
                        return prefix + "any";
                    case ChannelTypes.Show:
                        return prefix + "show";
                    default:
                        return String.Empty;
                }
            }

            public enum ChannelTypes {
                Any,
                Show
            }

            public static string EventType(EventTypes eventType) {
                string prefix = "eventType=";
                switch (eventType) {
                    case EventTypes.Completed:
                        return prefix + "completed";
                    case EventTypes.Live:
                        return prefix + "live";
                    case EventTypes.Upcoming:
                        return prefix + "upcoming";
                    default:
                        return String.Empty;
                }
            }

            public enum EventTypes {
                Completed,
                Live,
                Upcoming
            }

            public static string Location(double latitude, double longitude) {
                return $"location=({latitude},{longitude})";
            }

            public static string LocationRadius(double radius, Unit measurementUnit) {
                string prefix = "locationRadius=" + radius;
                switch (measurementUnit) {
                    case Unit.Meter:
                        return prefix + "m";
                    case Unit.Kilometer:
                        return prefix + "km";
                    case Unit.Feet:
                        return prefix + "ft";
                    case Unit.Miles:
                        return prefix + "mi";
                    default:
                        return String.Empty;
                }
            }

            public enum Unit
            {
                Meter, Kilometer, Feet, Miles
            }

            public static string MaxResults(int amount = 5) {
                return $"maxResults={Math.Min(50, Math.Max(0, amount))}";
            }

            public static string Order(Orders order) {
                string prefix = "order=";
                switch (order) {
                    case Orders.Date:
                        return prefix + "date";
                    case Orders.Rating:
                        return prefix + "rating";
                    case Orders.Relevance:
                        return prefix + "relevance";
                    case Orders.Title:
                        return prefix + "title";
                    case Orders.VideoCount:
                        return prefix + "videoCount";
                    case Orders.ViewCount:
                        return prefix + "viewCount";
                    default:
                        return String.Empty;
                }
            }

            public enum Orders {
                Date,Rating,Relevance,Title,VideoCount,ViewCount
            }

            public static string PageToken(string token) {
                return $"pageToken={token}";
            }

            public static string PublishedAfter(DateTime date) {
                return $"publishedAfter={XmlConvert.ToString(date, XmlDateTimeSerializationMode.Utc)}";
            }

            public static string PublishedBefore(DateTime date) {
                return $"publishedBefore={XmlConvert.ToString(date, XmlDateTimeSerializationMode.Utc)}";
            }
        }
    }
}