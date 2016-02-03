using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Controllers;
using WebAPI.Managers.Models;

namespace WebAPI
{
    /// <summary>
    /// Summary description for Swagger
    /// </summary>
    public class Swagger : IHttpHandler
    {
        public async void ProcessRequest(HttpContext context)
        {
            if (HttpContext.Current.Request["statuses"] != null)
            {
                var statusCodes = ClientsManager.ApiClient().GetErrorCodesDictionary();

                foreach (var ee in Enum.GetValues(typeof(StatusCode)))
                {
                    int eVal = (int)Enum.Parse(typeof(StatusCode), ee.ToString());
                    //Prevents duplicates
                    if (statusCodes.Where(xx => xx.Value == eVal).Count() == 0)
                        statusCodes.Add(ee.ToString(), eVal);
                }

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(statusCodes);
                context.Response.ContentType = "application/json";
                context.Response.Write(json);
            }
            else
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(context.Request.Url.AbsoluteUri.Replace("swagger.ashx", "docs/v1/swagger"));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // New code:
                    HttpResponseMessage response = await client.GetAsync("");
                    if (response.IsSuccessStatusCode)
                    {
                        // get default group (for roles)
                        var group = GroupsManager.GetGroup(0, context);
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

                        Assembly asm = Assembly.GetExecutingAssembly();

                        var paths = d.paths;

                        foreach (var k in paths)
                        {
                            var key = k.Name.Split('/');

                            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", key[2].ToString()), true, true);

                            var method = controller.GetMethods().Where(x => x.Name.ToLower() == key[4].ToString().ToLower()).FirstOrDefault();

                            if (method == null)
                                continue;

                            string roles = "";

                            var actionKey = string.Format("{0}_{1}", controller.Name.Replace("Controller", ""), method.Name).ToLower();
                            if (group != null && group.PermissionItemsRolesMapping != null && group.PermissionItemsRolesMapping.ContainsKey(actionKey))
                            {
                                var actionRolesDict = group.PermissionItemsRolesMapping[actionKey];
                                if (actionRolesDict != null && group.RolesIdsNamesMapping != null)
                                {
                                    roles = string.Join(", ", actionRolesDict.Keys.Select(r => group.RolesIdsNamesMapping.ContainsKey(r) ? group.RolesIdsNamesMapping[r] : string.Empty));
                                }
                            }

                            if (!string.IsNullOrEmpty(roles))
                            {
                                if (k.First.post.summary == null)
                                    k.First.post.summary = "";
                                k.First.post.summary.Value = string.Format("{0} (Available for: {1})", k.First.post.summary.Value, roles);
                            }
                        }
                        //if (method.GetCustomAttribute<ApiAuthorizeAttribute>() != null && method.GetCustomAttribute<ApiAuthorizeAttribute>().allowAnonymous)
                        //{
                        //    k.First.post.summary.Value = string.Format("{0} ({1})", k.First.post.summary.Value, "Available Anonymously");
                        //}


                        var defs = d.definitions;

                        foreach (var k in defs)
                        {
                            k.First.properties.objectType.Add("default", k.Name);

                            foreach (var kk in k.First.properties)
                            {
                                if (kk.First.@default == null && kk.First.type != null && kk.First.type.Value == "string")
                                    kk.First.Add("default", "");
                            }
                        }

                        json = Newtonsoft.Json.JsonConvert.SerializeObject(d).Replace("_service/", "api_v3/service/");

                        context.Response.ContentType = "application/json";
                        context.Response.Write(json);
                    }
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}