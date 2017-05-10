﻿using System.Net;

namespace EventStore.Common.Utils
{
    public static class IPEndPointExtensions
    {
        public static string ToHttpsUrl(this IPEndPoint endPoint, string rawUrl = null)
        {
            return string.Format("https://{0}:{1}/{2}",
                                 endPoint.Address,
                                 endPoint.Port,
                                 rawUrl != null ? rawUrl.TrimStart('/') : string.Empty);
        }

        public static string ToHttpUrl(this IPEndPoint endPoint, string rawUrl = null)
        {
            return string.Format("http://{0}:{1}/{2}",
                                 endPoint.Address,
                                 endPoint.Port,
                                 rawUrl != null ? rawUrl.TrimStart('/') : string.Empty);
        }
    }
}
