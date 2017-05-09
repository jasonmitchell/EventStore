using System.Net;

namespace EventStore.ClientAPI.Transport.Http
{
    internal class HttpEndPoint
    {
        private readonly IPEndPoint _endPoint;
        private readonly string _scheme;

        public HttpEndPoint(IPEndPoint endPoint, bool useHttps)
        {
            _endPoint = endPoint;
            _scheme = useHttps ? "https" : "http";
        }

        public string ToUrl(string formatString, params object[] args)
        {
            return string.Format("{0}://{1}:{2}/{3}",
                _scheme,
                _endPoint.Address,
                _endPoint.Port,
                string.Format(formatString.TrimStart('/'), args));
        }
    }
}