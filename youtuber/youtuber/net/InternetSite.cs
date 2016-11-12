/*
 * Copyright (C) 2016 Raphael Emberger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace youtuber.net
{
    /// <summary>
    ///     Contains the most basic methods for interacting with an internet site.
    /// </summary>
    public class InternetSite
    {
        protected HttpWebResponse response;

        public InternetSite(string content){
            Content = content;
        }

        public string Content {get; private set;}

        public static InternetSite FromContent(string content){
            return new InternetSite(content);
        }

        public static async Task<InternetSite> FromResponse(HttpWebResponse response){
            var respStream = response.GetResponseStream();
            if (respStream == null) {
                throw new WebException("No response stream recieved.");
            }
            var content = string.Empty;
            using (var strmReader = new StreamReader(respStream)) {
                content = await strmReader.ReadToEndAsync();
                strmReader.DiscardBufferedData();
            }
            var site = FromContent(content);
            site.response = response;
            return site;
        }

        protected static async Task<T> Load<T>(string url, string method = Http.Get) where T : InternetSite{
            var req = WebRequest.CreateHttp(url);
            req.Method = method;
            req.KeepAlive = true;
            return await Load<T>(req);
        }

        protected static async Task<T> Load<T>(HttpWebRequest request) where T : InternetSite{
            var resp = await request.GetResponseAsync();
            var respStream = resp.GetResponseStream();
            if (respStream == null) {
                throw new WebException("No response stream recieved.");
            }
            var content = string.Empty;
            using (var strmReader = new StreamReader(respStream)) {
                content = await strmReader.ReadToEndAsync();
                strmReader.DiscardBufferedData();
            }
            return (T) FromContent(content);
        }
    }
}