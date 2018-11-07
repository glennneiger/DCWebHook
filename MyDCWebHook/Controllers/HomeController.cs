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
using Google.Apis.Dialogflow.v2;


namespace MyDCWebHook.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int a)
        {
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;

            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" + DateTime.Now.ToString("hh:mm:ss") : ".NET";
            return View();

        }

        public GoogleCloudDialogflowV2WebhookResponse Index()
        {
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;
            GoogleCloudDialogflowV2WebhookResponse firstres = new GoogleCloudDialogflowV2WebhookResponse();
            firstres.FulfillmentText = "Test first";
            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" + DateTime.Now.ToString("hh:mm:ss") : ".NET";
            //return View();
            return firstres;
        }




        //[System.Web.Mvc.HttpPost]
        //public GoogleCloudDialogflowV2WebhookResponse Index([FromBody] GoogleCloudDialogflowV2WebhookRequest request)
        //{
        //    //OrchestratorApi oApi = new OrchestratorApi("https://platform.uipath.com", "admin", "shan999km", "shanslab");
        //    //JObject jobj = new JObject();
        //    //JsonSerializer js = new JsonSerializer();
        //    GoogleCloudDialogflowV2WebhookResponse res = new GoogleCloudDialogflowV2WebhookResponse();
        //    res.FulfillmentText="got it bro";
        //    res.Source = "DCBot";



        //    //var j = JsonConvert.SerializeObject(request.QueryResult.Parameters);
        //    //Console.WriteLine(j);
        //    ////ViewData["req"] = ViewData["req"]+request.QueryResult.Parameters["Ops"].ToString()+Environment.NewLine;
        //    //if(request.QueryResult.Parameters["Ops"].Equals("join")){
        //    //    oApi.addObjectToQueue(j.ToString(), "Test");
        //    //}
        //    //else if(request.QueryResult.Parameters["Ops"].Equals("reset")){
        //    //    string username = request.QueryResult.Parameters["username"].ToString().Trim();;
        //    //    string pin = request.QueryResult.Parameters["pin"].ToString().Trim();
        //    //    if (!checkUser(username, pin).Equals(""))
        //    //    {

        //    //        Console.WriteLine(j.ToString());
        //    //        oApi.addObjectToQueue(j.ToString(), "Test");
        //    //    }
        //    //    else{

        //    //    }
        //    //}
        //    return res;
        //}




        [System.Web.Mvc.HttpPost]
        public String Index([FromBody] GoogleCloudDialogflowV2WebhookRequest request)
        {
            string jsonstring = "";

            OrchestratorApi oApi = new OrchestratorApi("https://platform.uipath.com", "admin", "shan999km", "shanslab");
            JObject jobj = new JObject();
            JsonSerializer js = new JsonSerializer();
            //String jsonstring = "{\"fulfillmentText\": \"response text\",\"fulfillmentMessages\": [{\"simpleResponses\": {\"simpleResponses\": [   {\"textToSpeech\": \"response text\",\"displayText\": \"response text\"}]}}]}";
           

            // String jsonstring = "{\"fulfillmentText\": \"This is a text response\",\"fulfillmentMessages\": [{\"simpleResponses\": {\"textToSpeech\": \"this is text to speech\",\"ssml\": \"this is ssml\",\"displayText\": \"this is display text\"}}]}";
            // String jsonstring = "{\"payload\": {\"google\": {\"expectUserResponse\": true,\"richResponse\": {\"items\": [{\"simpleResponse\": {\"textToSpeech\": \"this is a simple response\"}}]}}}}";
            //request.QueryResult.Parameters.Add("email", "Testmail");
            var j = JsonConvert.SerializeObject(request.QueryResult.Parameters);
            Console.WriteLine(j);
            //////ViewData["req"] = ViewData["req"]+request.QueryResult.Parameters["Ops"].ToString()+Environment.NewLine;
            if (request.QueryResult.Parameters["Ops"].Equals("join"))
            {
                oApi.addObjectToQueue(j.ToString(), "Test");
            }

            string username = request.QueryResult.Parameters["username"].ToString().Trim(); ;
            string pin = request.QueryResult.Parameters["pin"].ToString().Trim();
            string useremail = checkUser(username, pin);
            if (!useremail.Equals(""))
            {
                j = j.Replace("\"}", ",\"email\":\"" + useremail + "\"}");
                jsonstring = "{\"fulfillmentText\": \"response text\",\"fulfillmentMessages\": [{\"simpleResponses\": {\"simpleResponses\": [   {\"textToSpeech\": \"response text\",\"displayText\": \"response text\"}]}}],\"followupEventInput\": {\"name\": \"Dummy\",\"languageCode\": \"en-US\",\"parameters\": {\"msg\": \"password is sent to:"+useremail+"\"}}}";
                Console.WriteLine(j.ToString());
                oApi.addObjectToQueue(j.ToString(), "Test");
            }
            else
            {
                 jsonstring = "{\"fulfillmentText\": \"response text\",\"fulfillmentMessages\": [{\"simpleResponses\": {\"simpleResponses\": [   {\"textToSpeech\": \"response text\",\"displayText\": \"response text\"}]}}],\"followupEventInput\": {\"name\": \"AD_Pass_Reset\",\"languageCode\": \"en-US\",\"parameters\": {\"msg\": \"Please check your pin!\"}}}";
            }
        
            return jsonstring;
        }



        private string checkUser(string username,string pin){
            string ret = "";
            List<user> ulist = new List<user>();
            ulist.Add(new user("john", "1110", "john@cerebtech.com"));
            ulist.Add(new user("jacob", "1111", "jacob@cerebtech.com"));
            ulist.Add(new user("james", "1112", "james@cerebtech.com"));
            ulist.Add(new user("ram", "1113", "ram@cerebtech.com"));
            foreach(user u in ulist){
                if(u.getUsername().Equals(username)){
                    if(u.getPin().Equals(pin)){
                        ret = u.getEmail();
                        break;
                    }
                }
                else{
                    ret = "";
                }
            }
            return ret;
        }
        class user{
            private string username;
            private string pin;
            private string email;
            public user(string un,string pin,string email){
                this.username = un;
                this.pin = pin;
                this.email = email;
            }
            public string getUsername(){
                return this.username;
            }
            public string getPin(){
                return this.pin;
            }
            public string getEmail(){
                return this.email;
            }
        }




    //    public static int DetectIntentFromTexts(string projectId,
    //                                    string sessionId,
    //                                    string[] texts,
    //                                    string languageCode = "en-US")
    //    {
            
    //        var client = SessionsClient.Create();

    //        foreach (var text in texts)
    //        {
    //            var response = client.DetectIntent(
    //                session: new SessionName(projectId, sessionId),
    //                queryInput: new GoogleCloudDialogflowV2QueryInput()
    //                {
    //                    Text = new TextInput()
    //                    {
    //                        Text = text,
    //                        LanguageCode = languageCode
    //                    }
    //                }
    //            );

    //            var queryResult = response.QueryResult;

    //            Console.WriteLine($"Query text: {queryResult.QueryText}");
    //            if (queryResult.Intent != null)
    //            {
    //                Console.WriteLine($"Intent detected: {queryResult.Intent.DisplayName}");
    //            }
    //            Console.WriteLine($"Intent confidence: {queryResult.IntentDetectionConfidence}");
    //            Console.WriteLine($"Fulfillment text: {queryResult.FulfillmentText}");
    //            Console.WriteLine();
    //        }

    //        return 0;
    //    }
    }
}
