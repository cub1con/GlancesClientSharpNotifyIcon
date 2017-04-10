using GlancesClientSharp.Glances.Plugins;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace GlancesClientSharp.Glances
{
    public class GlancesServer
    {
        private RestClient client;

        public string Address { get; private set; }

        public GlancesServer(string address)
        {
            Address = address;
            client = new RestClient(Address);
        }

        private GlancesPluginAttribute GetTypeAttribute<T>()
        {
            var type = typeof(T);
            var attrs = type.GetCustomAttributes(true);
            if (!attrs.Any(x => x is GlancesPluginAttribute))
                return null;
            return (GlancesPluginAttribute)attrs.First(x => x is GlancesPluginAttribute);
        }

        public T PerformQueryHack<T>(string plugin) where T : new()
        {
            using (WebClient client = new WebClient())
            {
                var url = string.Format("{0}/api/2/{1}", Address, plugin);
                var data = client.DownloadData(url);
                using (MemoryStream mem = new MemoryStream(data))
                {
                    using (StreamReader r = new StreamReader(mem))
                    {
                        var jconv = new Newtonsoft.Json.JsonSerializer();
                        return (T)jconv.Deserialize(r, typeof(T));
                    }
                }
            }

            /*var req = new RestRequest("api/2/" + plugin, Method.GET);
            //req.JsonSerializer = new NewtonsoftJsonSerializer();
            var resp = client.Execute<T>(req);
            if (resp.ErrorException != null)
                throw resp.ErrorException;

            return resp.Data;*/
        }

        public object PerformQuery<T>() where T : new()
        {
            var attr = GetTypeAttribute<T>();
            if (attr == null)
                throw new Exception("Type does not provide any GlancesPluginAttribute");

            var req = new RestRequest("api/2/" + attr.PluginName, Method.GET);
            req.JsonSerializer = new NewtonsoftJsonSerializer();
            if (attr.ReturnsArray)
            {
                var resp = client.Execute<List<T>>(req);
                return resp.Data.ToArray();
            } else
            {
                var resp = client.Execute<T>(req);
                return resp.Data;
            }
        }
    }
}
