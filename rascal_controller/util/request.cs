﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace rascal_controller.util
{
    class webRequest
    {
        public bool CheckURLValid(string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }
        public struct response
        {
            public string responeText;
            public int responseLength;
            public byte[] bytes;

            public string[][] headers;

            public float RTP;
            public float RT;

            public bool success;
            public string message;
        }
        public async Task<PingReply> PingAsync(string url)
        {

            Ping ping = new Ping();

            PingReply result = await ping.SendPingAsync(url);
            return result;
        }
        public response request(string url, string[][] headers)
        {
            Stopwatch stopw = new Stopwatch();
            stopw.Start();
            response r = new response();
            if (headers.Length > 99)
            {
                r.success = false;
                r.message = "to many headers!";
                return r;
            }
            foreach (string[] sa in headers)
            {
                if (sa.Length > 2)
                {
                    r.success = false;
                    r.message = "to many sub-headers!";
                    return r;
                }
            }
            if (!CheckURLValid(url))
            {
                r.success = false;
                r.message = "url invalid";
                return r;
            }
            PingReply png = PingAsync(url).Result;
            if (png.Status != IPStatus.Success)
            {
                r.success = false;
                r.message = "ping failed";
                return r;
            }
            else
            {
                r.RTP = png.RoundtripTime;
            }

            r.headers = headers;

            using (var client = new WebClient())
            {
                foreach (string[] hdr in headers)
                {
                    client.Headers.Add(hdr[0], hdr[1]);
                }
                r.bytes = client.DownloadData(url);
                var str = Encoding.Default.GetString(r.bytes);
                r.responeText = str;
                r.responseLength = r.bytes.Length;
            }
            GC.Collect();
            r.RT = stopw.ElapsedMilliseconds;

            return r;
        }
    }
}
