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

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace youtuber.net
{
    /// <summary>
    ///     Contains the most basic methods for interacting with an internet site.
    /// </summary>
    public abstract class InternetSite
    {
        public static HttpWebRequest DefaultHttpWebRequest = null;

        public static string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:52.0) Gecko/20100101 Firefox/52.0";

        internal string content = string.Empty;

        protected HttpWebRequest request;
        protected HttpWebResponse response;

        protected InternetSite(Uri uri){
            Uri = uri;
            Cookies = new CookieCollection();
        }

        protected InternetSite(Uri uri, CookieCollection cookies) {
            Uri = uri;
            Cookies = cookies;
        }

        public CookieCollection Cookies{get;}

        protected abstract void SetCookies();

        public bool Success {get; protected set;}

        public Uri Uri { get; protected set; }

        protected async Task Load() {
            if (DefaultHttpWebRequest == null) {
                request = WebRequest.CreateHttp(Uri);
                request.UserAgent = UserAgent;
                SetCookies();
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies);
            } else { request = DefaultHttpWebRequest; }
            using (response = (HttpWebResponse) await request.GetResponseAsync()) {
                Stream responseStream = response.GetResponseStream();
                if (responseStream == null) throw new WebException("No HttpWebResponse stream recieved.");
                using (StreamReader strmReader = new StreamReader(responseStream, Encoding.UTF8, true)) {
                    content = await strmReader.ReadToEndAsync();
                }
            }
            Success = true;
        }
    }
}