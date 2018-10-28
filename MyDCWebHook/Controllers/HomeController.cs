using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Google.Apis.Dialogflow.v2.Data;
using MyDCWebHook.Helpers.UiPathOrchApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyDCWebHook.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;

            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" : ".NET";


            return View();
        }

        [System.Web.Mvc.HttpPost]
        public void Index([FromBody] GoogleCloudDialogflowV2WebhookRequest request)
        {
            OrchestratorApi oApi = new OrchestratorApi("https://platform.uipath.com", "admin", "*********", "********");
            JObject jobj = new JObject();
            JsonSerializer js = new JsonSerializer();
            var j = JsonConvert.SerializeObject(request.QueryResult.Parameters);
            Console.WriteLine(j);
            if(request.QueryResult.Parameters["Ops"].Equals("join")){
                oApi.addObjectToQueue(j.ToString(), "Test");
            }
           


        }

    }
}
