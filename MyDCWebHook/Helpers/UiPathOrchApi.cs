using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using Newtonsoft.Json;

namespace MyDCWebHook.Helpers.UiPathOrchApi
{
    class OrchestratorApi
    {
        static String ORCH_URL = "";
        static String ORCH_USERNAME = "";
        static String ORCH_PASSWORD = "";
        static String ORCH_TENANT = "";
        public OrchestratorApi(string OrchestratorUrl, string username, string password, string tenant)
        {
            ORCH_URL = OrchestratorUrl;
            ORCH_USERNAME = username;
            ORCH_PASSWORD = password;
            ORCH_TENANT = tenant;
        }
        static void Main(string[] args)
        {
            try
            {
                //startJob();
                // ORCH_URL = "https://rpa14248";
                //  ORCH_USERNAME = "admin";
                // ORCH_PASSWORD = "890iop";
                //getFreeBots();
                // startJob(3);
                //getEnv();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }


        private String GetReleaseKey(string processName)
        {
            string url = ORCH_URL + "/odata/Releases";
            string reskey = getAuthKey(ORCH_USERNAME, ORCH_PASSWORD, ORCH_TENANT);
            JObject jo = apiCall(reskey, url);
            String resp = "";
            foreach (var item in jo["value"])
            {
                if (item["ProcessKey"].ToString().Equals(processName))
                {
                    resp = item["Key"].ToString().Trim();
                }
            }
            return resp;
        }
        public void startJob(int count)
        {
            String Bots = "";
            List<String> botIds = getFreeBots();
            if (botIds.Count > 0)
            {

                for (int i = 0; i < count; i++)
                {
                    Bots = Bots + botIds[i] + ",";
                }
                Bots = Bots.Remove(Bots.Length - 1);
            }
            else
            {
                Bots = "6";
            }
            string url = ORCH_URL + "/odata/Jobs/UiPath.Server.Configuration.OData.StartJobs";
            String reskey = getAuthKey(ORCH_USERNAME, ORCH_PASSWORD, ORCH_TENANT);
            String releaseKey = GetReleaseKey("TTUM_INTEG_ROBO_BULK");
            JObject jo = JObject.Parse("{\"startInfo\": {\"ReleaseKey\":\"" + releaseKey.Trim() + "\",\"Strategy\":\"Specific\",\"RobotIds\":[" + Bots.Trim() + "],\"NoOfRobots\":0}}");
            apiCall(url, jo, reskey);
        }
        public List<String> getFreeBots()
        {
            List<String> BotIDs = new List<string>();
            string reskey = getAuthKey(ORCH_USERNAME, ORCH_PASSWORD, ORCH_TENANT);
            Dictionary<String, String> param = new Dictionary<string, string>();
            param.Add("$filter", "State eq 'Available'");
            param.Add("$expand", "Robot");
            JObject jo = apiCall(reskey, param, ORCH_URL + "/odata/Sessions");
            if (Convert.ToInt32(jo.GetValue("@odata.count").ToString().Trim()) > 0)
            {
                foreach (var item in jo.GetValue("value"))
                {
                    BotIDs.Add(item["Robot"]["Id"].ToString());
                }
            }
            return BotIDs;
        }
        public void addToQueue(string fullFilePath, string QueueName)
        {
            //String[] fileArray = Directory.GetFiles(folderpath, "*.txt", SearchOption.AllDirectories);
            string url = ORCH_URL + "/odata/Queues/UiPathODataSvc.AddQueueItem";
            String reskey = getAuthKey(ORCH_USERNAME, ORCH_PASSWORD, ORCH_TENANT);
            //Console.WriteLine(fileArray.Length.ToString());
            //foreach(string item in fileArray){
            JObject jo = JObject.Parse("{\"itemData\": {\"Priority\": \"Normal\",\"DeferDate\": null,\"DueDate\": null,\"Name\": \"" + QueueName.Trim() + "\",\"SpecificContent\": {\"filepath@odata.type\": \"#String\",\"filepath\": \"" + fullFilePath.Replace("\\", "\\\\") + "\"}}}");
            apiCall(url, jo, reskey);
            //  }
        }

        public void addObjectToQueue(string obj, string QueueName)
        {
            //String[] fileArray = Directory.GetFiles(folderpath, "*.txt", SearchOption.AllDirectories);
            string url = ORCH_URL + "/odata/Queues/UiPathODataSvc.AddQueueItem";
            String reskey = getAuthKey(ORCH_USERNAME, ORCH_PASSWORD, ORCH_TENANT);
            //Console.WriteLine(fileArray.Length.ToString());
            //foreach(string item in fileArray){
            try
            {
                string testjson = "{\"itemData\": {\"Priority\": \"Normal\",\"DeferDate\": null,\"DueDate\": null,\"Name\": \"" + QueueName.Trim() + "\",\"SpecificContent\":" + obj + "}}";
                JObject jo = JObject.Parse("{\"itemData\": {\"Priority\": \"Normal\",\"DeferDate\": null,\"DueDate\": null,\"Name\": \"" + QueueName.Trim() + "\",\"SpecificContent\":" + obj + "}}");
                apiCall(url, jo, reskey);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //  }
        }
        public JObject getEnv()
        {
            string url = ORCH_URL + "/odata/Environments?";
            Dictionary<String, String> param = new Dictionary<string, string>();
            // param.Add("$top", "1");
            //String postdata = "$top=10";
            String reskey = getAuthKey(ORCH_USERNAME, ORCH_PASSWORD, ORCH_TENANT);
            JObject results = new JObject();
            results = apiCall(reskey, param, url);
            return results;
        }
        public JObject apiCall(string url, JObject jobj, string reskey)
        {
            JObject result = new JObject();
            HttpWebResponse httpResponse = null;
            HttpWebRequest httpWebRequest = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer" + " " + reskey);
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jobj);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = JObject.Parse(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (httpResponse != null) httpResponse = null;
                if (httpWebRequest != null) httpWebRequest = null;


            }
            return result;
        }
        public JObject apiCall(String key, Dictionary<String, String> param, String url)
        {
            JObject json = new JObject();
            string WEBSERVICE_URL = url;
            string paramList = "";
            try
            {
                if (param.Count > 0)
                {
                    foreach (var item in param)
                    {
                        paramList = paramList + item.Key + "=" + item.Value + "&";
                    }

                    WEBSERVICE_URL = WEBSERVICE_URL + "?" + paramList.Remove(paramList.Length - 1, 1);
                }
                var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 20000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("Authorization", "Bearer" + " " + key);

                    using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                        {
                            var jsonResponse = sr.ReadToEnd();
                            json = JObject.Parse(jsonResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            return json;
        }
        public JObject apiCall(String key, String url)
        {
            JObject json = new JObject();
            string WEBSERVICE_URL = url;
            try
            {
                var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 20000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("Authorization", "Bearer" + " " + key);
                    using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                        {
                            var jsonResponse = sr.ReadToEnd();
                            json = JObject.Parse(jsonResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            return json;
        }
        private JObject AuthapiCall(String postData, string url)
        {
            JObject jsonresponse = new JObject();
            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.Timeout = 3000;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;



            using (var stream = request.GetRequestStream())
            {

                stream.Write(data, 0, data.Length);
            }
            Console.WriteLine("inside Authapicall");
             //var response = (HttpWebResponse)request.GetResponse();




            using (var response = (HttpWebResponse)request.GetResponse())
            {
                response.GetResponseStream().ReadTimeout = 1000;
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                jsonresponse = JObject.Parse(responseString);
            }

           


            return jsonresponse;
        }
        private String getAuthKey(string username, string password, string tenant)
        {
            String url = ORCH_URL + "/api/account/authenticate";
            String key = "";
            String postData = "tenancyName=" + tenant;
            postData += "&usernameOrEmailAddress=" + username;
            postData += "&password=" + password;
            JObject autreq = AuthapiCall(postData, url);
            if (autreq.HasValues)
            {
                //Console.WriteLine(autreq.GetValue("success").ToString() );
                //Console.ReadKey();
                if (autreq.GetValue("success").ToString().ToLower() == "true")
                {
                    key = autreq.GetValue("result").ToString();
                }
                else
                {
                    key = "Failed";
                }
            }
            return key;
        }
    }
}
