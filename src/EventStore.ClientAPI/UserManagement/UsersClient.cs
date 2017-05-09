using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using HttpStatusCode = EventStore.ClientAPI.Transport.Http.HttpStatusCode;
using EventStore.ClientAPI.Common.Utils;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.Transport.Http;

namespace EventStore.ClientAPI.UserManagement
{
    internal class UsersClient
    {
        private readonly HttpAsyncClient _client;

        private readonly TimeSpan _operationTimeout;

        public UsersClient(ILogger log, TimeSpan operationTimeout)
        {
            _operationTimeout = operationTimeout;
            _client = new HttpAsyncClient(_operationTimeout);
        }

        public Task Enable(HttpEndPoint endPoint, string login, UserCredentials userCredentials = null)
        {
            return SendPost(endPoint.ToUrl("/users/{0}/command/enable", login), string.Empty, userCredentials, HttpStatusCode.OK);
        }

        public Task Disable(HttpEndPoint endPoint, string login, UserCredentials userCredentials = null)
        {
            return SendPost(endPoint.ToUrl("/users/{0}/command/disable", login), string.Empty, userCredentials, HttpStatusCode.OK);
        }

        public Task Delete(HttpEndPoint endPoint, string login, UserCredentials userCredentials = null)
        {
            return SendDelete(endPoint.ToUrl("/users/{0}", login), userCredentials, HttpStatusCode.OK);
        }

        public Task<List<UserDetails>> ListAll(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            return SendGet(endPoint.ToUrl("/users/"), userCredentials, HttpStatusCode.OK)
                    .ContinueWith(x =>
                    {
                        if (x.IsFaulted) throw x.Exception;
                        var r = JObject.Parse(x.Result);
                        return r["data"] != null ? r["data"].ToObject<List<UserDetails>>() : null;
                    });
        }

        public Task<UserDetails> GetCurrentUser(HttpEndPoint endPoint, UserCredentials userCredentials = null)
        {
            return SendGet(endPoint.ToUrl("/users/$current"), userCredentials, HttpStatusCode.OK)
                .ContinueWith(x =>
                {
                    if (x.IsFaulted) throw x.Exception;
                    var r = JObject.Parse(x.Result);
                    return r["data"] != null ? r["data"].ToObject<UserDetails>() : null;
                });
        }

        public Task<UserDetails> GetUser(HttpEndPoint endPoint, string login, UserCredentials userCredentials = null)
        {
            return SendGet(endPoint.ToUrl("/users/{0}", login), userCredentials, HttpStatusCode.OK)
                .ContinueWith(x =>
                {
                    if (x.IsFaulted) throw x.Exception;
                    var r = JObject.Parse(x.Result);
                    return r["data"] != null ? r["data"].ToObject<UserDetails>() : null;
                });
        }

        public Task CreateUser(HttpEndPoint endPoint, UserCreationInformation newUser,
            UserCredentials userCredentials = null)
        {
            var userJson = newUser.ToJson();
            return SendPost(endPoint.ToUrl("/users/"), userJson, userCredentials, HttpStatusCode.Created);
        }

        public Task UpdateUser(HttpEndPoint endPoint, string login, UserUpdateInformation updatedUser,
            UserCredentials userCredentials)
        {
            return SendPut(endPoint.ToUrl("/users/{0}", login), updatedUser.ToJson(), userCredentials, HttpStatusCode.OK);
        }

        public Task ChangePassword(HttpEndPoint endPoint, string login, ChangePasswordDetails changePasswordDetails,
            UserCredentials userCredentials)
        {
            return SendPost(endPoint.ToUrl("/users/{0}/command/change-password", login), changePasswordDetails.ToJson(), userCredentials, HttpStatusCode.OK);
        }

        public Task ResetPassword(HttpEndPoint endPoint, string login, ResetPasswordDetails resetPasswordDetails,
            UserCredentials userCredentials = null)
        {
            return SendPost(endPoint.ToUrl("/users/{0}/command/reset-password", login), resetPasswordDetails.ToJson(), userCredentials, HttpStatusCode.OK);
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
                        source.SetException(new UserCommandFailedException(
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
                        source.SetException(new UserCommandFailedException(
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
                        source.SetException(new UserCommandFailedException(
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
                        source.SetException(new UserCommandConflictException(response.HttpStatusCode, response.StatusDescription));
                    else
                        source.SetException(new UserCommandFailedException(
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
