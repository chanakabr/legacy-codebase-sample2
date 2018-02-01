using MailChimp.Campaigns;
using MailChimp.Lists;
using MailChimp.Lists.Members;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace MailChimp
{
    public class MailChimpManager
    {
        #region Fields
        private string dataCenter;
        private const string Url = "api.mailchimp.com/3.0/";
        private const string Get = "GET";
        private const string Post = "POST";
        private const string Patch = "PATCH";
        private const string Delete = "DELETE";
        private string apiKey;
        #endregion

        #region Properties
        public Error Error { get; private set; }
        public string ApiKey
        {
            get { return apiKey; }
            private set
            {
                apiKey = value;
                dataCenter = value.Split('-')[1];
            }
        }
        #endregion

        #region Constructor
        public MailChimpManager(string apiKey)
        {
            ApiKey = apiKey;
        }
        #endregion

        #region Method
        private T DoRequests<T>(string endPoint, string httpMethod, string payload = null)
        {
            string result = string.Empty;
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers.Add("Authorization", "apikey " + ApiKey);

                    result = webClient.UploadString(endPoint, "PUT", payload);

                    return JsonConvert.DeserializeObject<T>(result);
                }
            }

            catch (WebException we)
            {
                using (var httpWebResponse = (HttpWebResponse)we.Response)
                {
                    if (httpWebResponse == null)
                    {
                        Error = new Error { Type = we.Status.ToString(), Detail = we.Message };
                        return default(T);
                    }
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    Error = JsonConvert.DeserializeObject<Error>(result);
                }
            }

            return default(T);
        }
        private T DoRequests1<T>(string endPoint, string httpMethod, string payload = null)
        {
            string result = string.Empty;
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers.Add("Authorization", "apikey " + ApiKey);

                    result = webClient.UploadString(endPoint, "POST", payload);

                    return JsonConvert.DeserializeObject<T>(result);
                }
            }

            catch (WebException we)
            {
                using (var httpWebResponse = (HttpWebResponse)we.Response)
                {
                    if (httpWebResponse == null)
                    {
                        Error = new Error { Type = we.Status.ToString(), Detail = we.Message };
                        return default(T);
                    }
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    Error = JsonConvert.DeserializeObject<Error>(result);
                }
            }

            return default(T);
        }

        private T DoRequests2<T>(string endPoint, string httpMethod, string payload = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(endPoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add("Authorization", string.Format("{0} {1}", "apikey", ApiKey));
            if (httpMethod != Patch)
            {
                httpWebRequest.Method = httpMethod;
            }
            else
            {
                httpWebRequest.Method = WebRequestMethods.Http.Post;
                httpWebRequest.Headers.Add("X-Http-Method-Override", Patch);
            }

            if (!string.IsNullOrEmpty(payload) && (httpMethod == Post || httpMethod == Patch))
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(payload);
                }
            }

            string result;
            try
            {
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    var statusCode = (int)httpWebResponse.StatusCode;
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }
                    if (statusCode == 200) { return JsonConvert.DeserializeObject<T>(result); }
                    if (statusCode == 204) { return (T)Convert.ChangeType(true, typeof(T)); }
                }
            }
            catch (WebException we)
            {
                using (var httpWebResponse = (HttpWebResponse)we.Response)
                {
                    if (httpWebResponse == null)
                    {
                        Error = new Error { Type = we.Status.ToString(), Detail = we.Message };
                        return default(T);
                    }
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }
                    Error = JsonConvert.DeserializeObject<Error>(result);
                }
            }
            return default(T);
        }



        private string GetQueryParams(string payload)
        {
            if (payload == null) { return ""; }
            var jObject = JObject.Parse(payload);
            var sbQueryString = new StringBuilder();
            foreach (var obj in jObject)
            {
                if (obj.Value.Type == JTokenType.Array)
                {
                    sbQueryString.AppendFormat("{0}=", obj.Key);
                    foreach (var arrayValue in obj.Value)
                    {
                        sbQueryString.AppendFormat("{0}{1}", arrayValue, arrayValue.Next != null ? "," : "&");
                    }
                    continue;
                }
                sbQueryString.AppendFormat("{0}={1}&", obj.Key, obj.Value);
            }
            if (sbQueryString.Length > 0)
            {
                sbQueryString.Insert(0, "?");
                sbQueryString.Remove(sbQueryString.Length - 1, 1);
            }
            return sbQueryString.ToString();
        }

        #endregion

        #region List
        public List ReadList(string listId, ListQuery listQuery = null)
        {
            string queryString = null;
            if (listQuery != null)
            {
                var payload = JsonConvert.SerializeObject(listQuery, Formatting.None, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                queryString = GetQueryParams(payload);
            }
            var endPoint = string.Format("https://{0}.{1}lists/{2}/{3}", dataCenter, Url, listId, queryString);
            return DoRequests<List>(endPoint, Get);
        }
        public List EditList(string listId, List list)
        {
            var payload = JsonConvert.SerializeObject(list, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            var endPoint = string.Format("https://{0}.{1}lists/{2}", dataCenter, Url, listId);
            return DoRequests<List>(endPoint, Patch, payload);
        }
        public List CreateList(List list)
        {
            var payload = JsonConvert.SerializeObject(list, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            var endPoint = string.Format("https://{0}.{1}lists/", dataCenter, Url);
            return DoRequests1<List>(endPoint, Post, payload);
        }
        public bool DeleteList(string listId)
        {
            var endPoint = string.Format("https://{0}.{1}lists/{2}/", dataCenter, Url, listId);
            return DoRequests2<bool>(endPoint, Delete);
        }
        public CollectionList ReadLists(CollectionListQuery listListsQuery = null)
        {
            string queryString = null;
            if (listListsQuery != null)
            {
                var payload = JsonConvert.SerializeObject(listListsQuery, Formatting.None, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                queryString = GetQueryParams(payload);
            }
            var endPoint = string.Format("https://{0}.{1}lists/{2}", dataCenter, Url, queryString);
            return DoRequests<CollectionList>(endPoint, Get);
        }
        #endregion

        #region Member
        public Member ReadMember(string listId, string subscriberHash, MemberQuery memberQuery = null)
        {
            string queryString = null;
            if (memberQuery != null)
            {
                var payload = JsonConvert.SerializeObject(memberQuery, Formatting.None, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                queryString = GetQueryParams(payload);
            }
            var endPoint = string.Format("https://{0}.{1}lists/{2}/members/{3}/{4}", dataCenter, Url, listId, subscriberHash, queryString);
            return DoRequests<Member>(endPoint, Get);
        }
        public bool DeleteMember(string listId, string subscriberHash)
        {
            var endPoint = string.Format("https://{0}.{1}lists/{2}/members/{3}", dataCenter, Url, listId, subscriberHash);
            return DoRequests2<bool>(endPoint, Delete);
        }
        public Member CreateMember(string listId, Member member)
        {
            var payload = JsonConvert.SerializeObject(member, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            var hashedEmailAddress = string.IsNullOrEmpty(member.EmailAddress) ? "" : CalculateMD5Hash(member.EmailAddress.ToLower());

            var endPoint = string.Format("https://{0}.{1}lists/{2}/members/{3}", dataCenter, Url, listId, hashedEmailAddress);
            return DoRequests<Member>(endPoint, Post, payload);
        }

        private static string CalculateMD5Hash(string input)
        {
            // Step 1, calculate MD5 hash from input.
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string.
            var sb = new StringBuilder();
            foreach (var @byte in hash)
            {
                sb.Append(@byte.ToString("X2"));
            }
            return sb.ToString();
        }
        public Member EditMember(string listId, string subscriberHash, Member member)
        {
            var payload = JsonConvert.SerializeObject(member, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            var endPoint = string.Format("https://{0}.{1}lists/{2}/members/{3}", dataCenter, Url, listId, subscriberHash);
            return DoRequests<Member>(endPoint, Patch, payload);
        }
        public Member CreateOrEditMember(string listId, Member member, bool isForce = false)
        {
            var memberNew = CreateMember(listId, member);
            if (memberNew == null && Error.Status == 400 && Error.Title == "Member Exists")
            {
                memberNew = EditMember(listId, member.GetSubscriberHash(), member);
                if (memberNew != null)
                {
                    return memberNew;
                }
            }
            return null;
        }
        public CollectionMember ReadMembers(string listId, CollectionMemberQuery memberListsQuery = null)
        {
            string queryString = null;
            if (memberListsQuery != null)
            {
                var payload = JsonConvert.SerializeObject(memberListsQuery, Formatting.None, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                queryString = GetQueryParams(payload);
            }
            var endPoint = string.Format("https://{0}.{1}lists/{2}/members/{3}", dataCenter, Url, listId, queryString);
            return DoRequests<CollectionMember>(endPoint, Get);
        }
        public CollectionMember GetAllSubscribedMembers(string listId)
        {
            var memberListQuery = new CollectionMemberQuery();
            memberListQuery.Status = "subscribed";

            return ReadMembers(listId, memberListQuery);
        }
        public CollectionMember CollectionMemberQuery(string listId)
        {
            var memberListQuery = new CollectionMemberQuery();
            memberListQuery.Status = "unsubscribed";

            return ReadMembers(listId, memberListQuery);
        }
        public CollectionMember GetAllCleanedMembers(string listId)
        {
            var memberListQuery = new CollectionMemberQuery();
            memberListQuery.Status = "cleaned";

            return ReadMembers(listId, memberListQuery);
        }
        #endregion

        #region Campaign
        public Campaign CreateCampaign(Campaign campaign)
        {
            var payload = JsonConvert.SerializeObject(campaign, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            var endPoint = string.Format("https://{0}.{1}campaigns/", dataCenter, Url);
            return DoRequests1<Campaign>(endPoint, Post, payload);
        }

        public bool SendCampaign(string campaignId)
        {
            var endPoint = string.Format("https://{0}.{1}campaigns/{2}/actions/send", dataCenter, Url, campaignId);
            return DoRequests2<bool>(endPoint, Post);
        }

        #endregion

        #region MergeField
        public MergeField CreateMergeField(string listId, MergeField mergeField)
        {
            var payload = JsonConvert.SerializeObject(mergeField, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            var endPoint = string.Format("https://{0}.{1}lists/{2}/merge-fields", dataCenter, Url, listId);
            return DoRequests2<MergeField>(endPoint, Post, payload);
        }

        public bool DeleteMergeField(string listId, int mergeFieldId)
        {
            var endPoint = string.Format("https://{0}.{1}lists/{2}/merge-fields/{3}", dataCenter, Url, listId, mergeFieldId);
            return DoRequests2<bool>(endPoint, Delete);
        }

        public CollectionMergeField GetMergeFields(string listId)
        {
            var endPoint = string.Format("https://{0}.{1}lists/{2}/merge-fields", dataCenter, Url, listId);
            return DoRequests2<CollectionMergeField>(endPoint, Get);
        }
        #endregion
    }
}