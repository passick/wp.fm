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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

namespace lastfm
{
    public class tag
    {
        public static async Task<List<tagInfo>> search(string tagName, int page = 0, int limit = 30)
        {
            RequestParameters rParams = new RequestParameters("tag.search");
            rParams.Add("tag", tagName);
            rParams.Add("limit", limit.ToString());
            rParams.Add("page", page.ToString());
            XDocument returnedXml = await Request.MakeRequest(rParams);
            if (Request.CheckStatus(returnedXml) == 0)
            {
                List<tagInfo> tags = new List<tagInfo>((from item in returnedXml.Descendants("tagmatches").Elements() select new tagInfo(item)));
                XNamespace opensearch = @"http://a9.com/-/spec/opensearch/1.1/";
                IEnumerable<XElement> opensearch_ = from el in returnedXml.Element("lfm").Element("results").Elements()
                                                    where el.Name.Namespace == opensearch
                                                    select el;
                int totalResults = Int32.Parse((from el in opensearch_ where el.Name.LocalName == "totalResults" select el.Value).First());
                int startIndex = Int32.Parse((from el in opensearch_ where el.Name.LocalName == "startIndex" select el.Value).First());
                int itemsPerPage = Int32.Parse((from el in opensearch_ where el.Name.LocalName == "itemsPerPage" select el.Value).First());
                if (totalResults - startIndex < 0)
                    throw new IndexOutOfRangeException("Page being shown is the first page");
                return tags;
            }
            else
                throw new LastFmAPIException(returnedXml);
        }
        public static async Task<tagInfo> getInfo(string tagName, string lang = "en")
        {
            RequestParameters rParams = new RequestParameters("tag.getinfo");
            rParams.Add("tag", tagName);
            rParams.Add("lang", lang);
            XDocument returnedXml = await Request.MakeRequest(rParams);
            if (Request.CheckStatus(returnedXml) == 0)
            {
                tagInfo retTag = new tagInfo(returnedXml.Element("lfm").Element("tag"));
                return retTag;
            }
            else
                throw new LastFmAPIException(returnedXml);
        }
    }
}
