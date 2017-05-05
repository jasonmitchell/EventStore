using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.Transport.Http;
using Newtonsoft.Json.Linq;
using HttpStatusCode = EventStore.ClientAPI.Transport.Http.HttpStatusCode;

namespace EventStore.ClientAPI.Projections
{
    internal class ProjectionsClient
    {
        private readonly HttpAsyncClient _client;

        public ProjectionsClient(ILogger log, TimeSpan operationTimeout)
        {
            _client = new HttpAsyncClient(operationTimeout);
        }

        public Task Enable(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/command/enable", name);
            return SendPost(url, string.Empty, userCredentials, HttpStatusCode.OK);
        }

        public Task Disable(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/command/disable", name);
            return SendPost(url, string.Empty, userCredentials, HttpStatusCode.OK);
        }

        public Task Abort(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/command/abort", name);
            return SendPost(url, string.Empty, userCredentials, HttpStatusCode.OK);
        }

        public Task CreateOneTime(HttpEndPoint endPoint, string query, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/onetime?type=JS");
            return SendPost(url, query, userCredentials, HttpStatusCode.Created);
        }

        public Task CreateTransient(HttpEndPoint endPoint, string name, string query, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/transient?name={0}&type=JS", name);
            return SendPost(url, query, userCredentials, HttpStatusCode.Created);
        }

        public Task CreateContinuous(HttpEndPoint endPoint, string name, string query, bool trackEmitted, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/continuous?name={0}&type=JS&emit=1&trackemittedstreams={1}", name, trackEmitted);
            return SendPost(url, query, userCredentials, HttpStatusCode.Created);
        }

        [Obsolete("Use 'Task<List<ProjectionDetails>> ListAll' instead")]
        public Task<string> ListAllAsString(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/any");
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<List<ProjectionDetails>> ListAll(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/any");
            return SendGet(url, userCredentials, HttpStatusCode.OK)
                    .ContinueWith(x =>
                    {
                        if (x.IsFaulted) throw x.Exception;
                        var r = JObject.Parse(x.Result);
                        return r["projections"] != null ? r["projections"].ToObject<List<ProjectionDetails>>() : null;
                    });
        }

        [Obsolete("Use 'Task<List<ProjectionDetails>> ListOneTime' instead")]
        public Task<string> ListOneTimeAsString(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/onetime");
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<List<ProjectionDetails>> ListOneTime(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/onetime");
            return SendGet(url, userCredentials, HttpStatusCode.OK)
                    .ContinueWith(x =>
                    {
                        if (x.IsFaulted) throw x.Exception;
                        var r = JObject.Parse(x.Result);
                        return r["projections"] != null ? r["projections"].ToObject<List<ProjectionDetails>>() : null;
                    });
        }

        [Obsolete("Use 'Task<List<ProjectionDetails>> ListContinuous' instead")]
        public Task<string> ListContinuousAsString(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/continuous");
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<List<ProjectionDetails>> ListContinuous(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projections/continuous");
            return SendGet(url, userCredentials, HttpStatusCode.OK)
                    .ContinueWith(x =>
                    {
                        if (x.IsFaulted) throw x.Exception;
                        var r = JObject.Parse(x.Result);
                        return r["projections"] != null ? r["projections"].ToObject<List<ProjectionDetails>>() : null;
                    });
        }

        public Task<string> GetStatus(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}", name);
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<string> GetState(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/state", name);
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<string> GetPartitionStateAsync(HttpEndPoint endPoint, string name, string partition, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/state?partition={1}", name, partition);
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<string> GetResult(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/result", name);
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<string> GetPartitionResultAsync(HttpEndPoint endPoint, string name, string partition, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/result?partition={1}", name, partition);
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<string> GetStatistics(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/statistics", name);
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task<string> GetQuery(HttpEndPoint endPoint, string name, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/query", name);
            return SendGet(url, userCredentials, HttpStatusCode.OK);
        }

        public Task UpdateQuery(HttpEndPoint endPoint, string name, string query, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}/query?type=JS", name);
            return SendPut(url, query, userCredentials, HttpStatusCode.OK);
        }

        public Task Delete(HttpEndPoint endPoint, string name, bool deleteEmittedStreams, UserCredentials userCredentials = null)
        {
            var url = endPoint.ToUrl("/projection/{0}?deleteEmittedStreams={1}", name, deleteEmittedStreams);
            return SendDelete(url, userCredentials, HttpStatusCode.OK);
        }

        private Task<string> SendGet(string url, UserCredentials userCredentials, int expectedCode)
        {
            var source = new TaskCompletionSource<string>();
            _client.Get(url,
                        userCredentials,
                        response =>
                        {
                            if (response.HttpStatusCode == expectedCode)
                                source.SetResult(response.Body);
                            else
                                source.SetException(new ProjectionCommandFailedException(
                                                            response.HttpStatusCode,
                                                            string.Format("Server returned {0} ({1}) for GET on {2}",
                                                                          response.HttpStatusCode,
                                                                          response.StatusDescription,
                                                                          url)));
                        },
                        source.SetException);

            return source.Task;
        }

        private Task<string> SendDelete(string url, UserCredentials userCredentials, int expectedCode)
        {
            var source = new TaskCompletionSource<string>();
            _client.Delete(url,
                           userCredentials,
                           response =>
                           {
                               if (response.HttpStatusCode == expectedCode)
                                   source.SetResult(response.Body);
                               else
                                   source.SetException(new ProjectionCommandFailedException(
                                                               response.HttpStatusCode,
                                                               string.Format("Server returned {0} ({1}) for DELETE on {2}",
                                                                             response.HttpStatusCode,
                                                                             response.StatusDescription,
                                                                             url)));
                           },
                           source.SetException);

            return source.Task;
        }

        private Task SendPut(string url, string content, UserCredentials userCredentials, int expectedCode)
        {
            var source = new TaskCompletionSource<object>();
            _client.Put(url,
                        content,
                        "application/json",
                        userCredentials,
                        response =>
                        {
                            if (response.HttpStatusCode == expectedCode)
                                source.SetResult(null);
                            else
                                source.SetException(new ProjectionCommandFailedException(
                                                            response.HttpStatusCode,
                                                            string.Format("Server returned {0} ({1}) for PUT on {2}",
                                                                          response.HttpStatusCode,
                                                                          response.StatusDescription,
                                                                          url)));
                        },
                        source.SetException);

            return source.Task;
        }

        private Task SendPost(string url, string content, UserCredentials userCredentials, int expectedCode)
        {
            var source = new TaskCompletionSource<object>();
            _client.Post(url,
                         content,
                         "application/json",
                         userCredentials,
                         response =>
                         {
                             if (response.HttpStatusCode == expectedCode)
                                 source.SetResult(null);
                             else if (response.HttpStatusCode == 409)
                                 source.SetException(new ProjectionCommandConflictException(response.HttpStatusCode, response.StatusDescription));
                             else
                                 source.SetException(new ProjectionCommandFailedException(
                                                             response.HttpStatusCode,
                                                             string.Format("Server returned {0} ({1}) for POST on {2}",
                                                                           response.HttpStatusCode,
                                                                           response.StatusDescription,
                                                                           url)));
                         },
                         source.SetException);

            return source.Task;
        }
    }
}