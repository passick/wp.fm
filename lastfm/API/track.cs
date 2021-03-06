﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lastfm
{
    public class track
    {
        public static async Task<List<trackInfo>> search(string trackName, int page = 0, int limit = 30)
        {
            RequestParameters rParams = new RequestParameters("track.search");
            rParams.Add("track", trackName);
            rParams.Add("limit", limit.ToString());
            rParams.Add("page", page.ToString());
            XDocument returnedXml = await Request.MakeRequest(rParams);
            if (Request.CheckStatus(returnedXml) == 0)
            {
                List<trackInfo> tracks = new List<trackInfo>((from item in returnedXml.Descendants("trackmatches").Elements() select new trackInfo(item)));
                XNamespace opensearch = @"http://a9.com/-/spec/opensearch/1.1/";
                IEnumerable<XElement> opensearch_ = from el in returnedXml.Element("lfm").Element("results").Elements()
                                                    where el.Name.Namespace == opensearch
                                                    select el;
                int totalResults = Int32.Parse((from el in opensearch_ where el.Name.LocalName == "totalResults" select el.Value).First());
                int startIndex = Int32.Parse((from el in opensearch_ where el.Name.LocalName == "startIndex" select el.Value).First());
                int itemsPerPage = Int32.Parse((from el in opensearch_ where el.Name.LocalName == "itemsPerPage" select el.Value).First());
                if (totalResults - startIndex < 0)
                    throw new IndexOutOfRangeException("Page being shown is the first page");
                return tracks;
            }
            else
                throw new LastFmAPIException(returnedXml);
        }

        public static async Task<trackInfo> getInfo(string artistName, string trackName, string username = "")
        {
            RequestParameters rParams = new RequestParameters("track.getinfo");
            rParams.Add("artist", artistName);
            rParams.Add("track", trackName);
            if (!string.IsNullOrEmpty(username))
                rParams.Add("username", username);
            XDocument returnedXml = await Request.MakeRequest(rParams);
            if (Request.CheckStatus(returnedXml) == 0)
            {
                trackInfo track = new trackInfo(returnedXml.Element("lfm").Element("track"));
                return track;
            }
            else if (Request.CheckStatus(returnedXml) == 6) // Track not found
            {
                // Trying with autocorrect
                rParams.Add("autocorrect", "1");
                returnedXml = await Request.MakeRequest(rParams);
                if (Request.CheckStatus(returnedXml) == 0)
                {
                    trackInfo track = new trackInfo(returnedXml.Element("lfm").Element("track"));
                    return track;
                }
                else
                    throw new LastFmAPIException(returnedXml);
            }
            else
                throw new LastFmAPIException(returnedXml);
        }

        /// <summary>
        /// Sends scrobble request to the server
        /// </summary>
        /// <param name="artistName"> Artist who plays the track </param>
        /// <param name="trackName"> Name of the track </param>
        /// <param name="timestamp"> DateTime object representing when track was played </param>
        public static async void scrobble(string artistName, string trackName, DateTime timestamp = default(DateTime))
        {
            if (!Session.CanUseCurrentSession())
                MessageBox.Show("This service requires authentication");
            int timeStamp;
            if (timestamp != default(DateTime) && timestamp != null)
                timeStamp = (int)(timestamp.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
            else
                timeStamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            RequestParameters rParams = new RequestParameters("track.scrobble");
            rParams.Add("artist", artistName);
            rParams.Add("track", trackName);
            rParams.Add("timestamp", timeStamp.ToString());
            rParams.Add("sk", Session.CurrentSession.SessionKey);
            XDocument returnedXml = await Request.MakeRequest(rParams, true);
            if (Request.CheckStatus(returnedXml) != 0)
                throw new LastFmAPIException(returnedXml);
        }

        public static async void scrobble(List<trackInfo> tracks)
        {
            if (!Session.CanUseCurrentSession())
                MessageBox.Show("This service requires authentication");
            if (tracks.Count > 50)
                throw new ArgumentOutOfRangeException("tracks", "Number of elements in the list must not exceed 50");
            RequestParameters rParams = new RequestParameters("track.scrobble");
            rParams.Add("sk", Session.CurrentSession.SessionKey);
            for (int i = 0; i < tracks.Count; ++i)
            {
                trackInfo track = tracks[i];
                if (track != null && track.name != null && track.album != null && track.date != null)
                {
                    rParams.Add("artist[" + i + "]", track.artist.name);
                    rParams.Add("track[" + i + "]", track.name);
                    int timestamp = (int)(track.date.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                    rParams.Add("timestamp[" + i + "]", timestamp.ToString());
                }
            }
            XDocument returnedXml = await Request.MakeRequest(rParams, true);
            if (Request.CheckStatus(returnedXml) != 0)
                throw new LastFmAPIException(returnedXml);
        }

        public static async void updateNowPlaying(string artistName, string trackName, string albumName = null)
        {
            if (!Session.CanUseCurrentSession())
                MessageBox.Show("This service requires authentication");
            RequestParameters rParams = new RequestParameters("track.updateNowPlaying");
            rParams.Add("artist", artistName);
            rParams.Add("track", trackName);
            rParams.Add("sk", Session.CurrentSession.SessionKey);
            if (!String.IsNullOrEmpty(albumName))
                rParams.Add("album", albumName);
            XDocument returnedXml = await Request.MakeRequest(rParams, true);
            if (Request.CheckStatus(returnedXml) != 0)
                throw new LastFmAPIException(returnedXml);
        }

        public static async void love(string trackName, string artistName)
        {
            if (!Session.CanUseCurrentSession())
                throw new UnauthorizedException("User must login to perform this action");
            RequestParameters rParams = new RequestParameters("track.love");
            rParams.Add("artist", artistName);
            rParams.Add("track", trackName);
            rParams.Add("sk", Session.CurrentSession.SessionKey);
            XDocument returnedXml = await Request.MakeRequest(rParams, true);
            if (Request.CheckStatus(returnedXml) != 0)
                throw new LastFmAPIException(returnedXml);
        }

        public static async void unlove(string trackName, string artistName)
        {
            if (!Session.CanUseCurrentSession())
                throw new UnauthorizedException("User must login to perform this action");
            RequestParameters rParams = new RequestParameters("track.unlove");
            rParams.Add("artist", artistName);
            rParams.Add("track", trackName);
            rParams.Add("sk", Session.CurrentSession.SessionKey);
            XDocument returnedXml = await Request.MakeRequest(rParams, true);
            if (Request.CheckStatus(returnedXml) != 0)
                throw new LastFmAPIException(returnedXml);
        }
    }
}
