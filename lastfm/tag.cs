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
            RequestParameters rParams = new RequestParameters();
            rParams.Add("tag", HttpUtility.UrlEncode(tagName));
            rParams.Add("method", "tag.search");
            rParams.Add("limit", limit.ToString());
            rParams.Add("page", page.ToString());
            XDocument returnedXml = await Request.MakeRequest(rParams);
            if (Request.CheckStatus(returnedXml) == 0)
            {
                List<tagInfo> tags = new List<tagInfo>((from item in returnedXml.Descendants("tagmatches").Elements() select new tagInfo(item)));
                return tags;
            }
            else
                MessageBox.Show("Sorry, there was some error while executing your request. " + Request.CheckStatus(returnedXml).ToString());
            return null;
        }
        public static async Task<tagInfo> getInfo(string tagName, string lang = "en")
        {
            RequestParameters rParams = new RequestParameters();
            rParams.Add("tag", HttpUtility.UrlEncode(tagName));
            rParams.Add("method", "tag.getinfo");
            rParams.Add("lang", lang);
            XDocument returnedXml = await Request.MakeRequest(rParams);
            if (Request.CheckStatus(returnedXml) == 0)
            {
                tagInfo retTag = new tagInfo(returnedXml.Element("lfm").Element("tag"));
                return retTag;
            }
            else
                MessageBox.Show("Sorry, there was some error while executing your request. " + Request.CheckStatus(returnedXml).ToString());
            return null;
        }
    }
}
