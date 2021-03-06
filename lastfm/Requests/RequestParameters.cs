﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace lastfm
{
    /// <summary>
    /// Dictionary<string,string> wrapper with overriden methods so as to be able to get info from last.fm servers
    /// </summary>
    public class RequestParameters : Dictionary<string, string>
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in this.Keys)
                sb.Append(key + '=' + HttpUtility.UrlEncode(this[key]) + '&');
            string ret = sb.ToString().Substring(0, sb.Length-1);
            return ret;
        }

        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        /// <summary>
        /// RequestParameters constructor.
        /// </summary>
        /// <param name="MethodName">Method name to use RequestParameters with</param>
        public RequestParameters(string MethodName) : base()
        {
            if (string.IsNullOrEmpty(MethodName))
                throw new ArgumentException("Method name cannot be empty", "MethodName");
            this.Add("method", MethodName);
        }
    }
}
