using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;


namespace UpdateTweet
{
    
    public class Update : IPlugin
    {


        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            try
            {
                //Get the twitter credentials from CRM
                QueryExpression query = new QueryExpression();
                query.EntityName = "syt_twittercredential";
                query.ColumnSet = new ColumnSet() { AllColumns = true };
                EntityCollection entities = service.RetrieveMultiple(query);
                Entity entity = entities[0];
                

                //Get the details of the message to be updated
                string tweetactivityid = context.InputParameters["Retweetid"].ToString();
                string message = context.InputParameters["Message"].ToString();
                var arr = tweetactivityid.Split(' ');
                var twithandle = arr[1];
                var tweetid = arr[0];
                var guid = arr[2];
                
                string twitterURL = "https://api.twitter.com/1.1/statuses/update.json";

                //set the access tokens (REQUIRED)
                string oauth_consumer_key = entity.Attributes["syt_consumerkey"].ToString();
                string oauth_consumer_secret = entity.Attributes["syt_consumersecret"].ToString();
                string oauth_token = entity.Attributes["syt_accesstoken"].ToString();
                string oauth_token_secret = entity.Attributes["syt_accesstokensecret"].ToString();

                // set the oauth version and signature method
                string oauth_version = "1.0";
                string oauth_signature_method = "HMAC-SHA1";

                // create unique request details
                string oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
                System.TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
                string oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

                // create oauth signature
                string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" + "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

                string baseString = string.Format(
                    baseFormat,
                    oauth_consumer_key,
                    oauth_nonce,
                    oauth_signature_method,
                    oauth_timestamp, oauth_token,
                    oauth_version,
                    Uri.EscapeDataString(message + " https://twitter.com/" + twithandle+ "/status/" +tweetid)
                );

                string oauth_signature = null;
                using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(oauth_consumer_secret) + "&" + Uri.EscapeDataString(oauth_token_secret))))
                {
                    oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(twitterURL) + "&" + Uri.EscapeDataString(baseString))));
                }

                // create the request header
                string authorizationFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", " + "oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " + "oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

                string authorizationHeader = string.Format(
                    authorizationFormat,
                    Uri.EscapeDataString(oauth_consumer_key),
                    Uri.EscapeDataString(oauth_nonce),
                    Uri.EscapeDataString(oauth_signature),
                    Uri.EscapeDataString(oauth_signature_method),
                    Uri.EscapeDataString(oauth_timestamp),
                    Uri.EscapeDataString(oauth_token),
                    Uri.EscapeDataString(oauth_version)
                );

                HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(twitterURL);
                objHttpWebRequest.Headers.Add("Authorization", authorizationHeader);
                objHttpWebRequest.Method = "POST";
                objHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                using (Stream objStream = objHttpWebRequest.GetRequestStream())
                {
                    byte[] content = ASCIIEncoding.ASCII.GetBytes("status=" + Uri.EscapeDataString(message + " https://twitter.com/" + twithandle + "/status/" + tweetid));
                    objStream.Write(content, 0, content.Length);
                }

                var responseResult = "";
                try
                {
                    //success posting
                    WebResponse objWebResponse = objHttpWebRequest.GetResponse();
                    StreamReader objStreamReader = new StreamReader(objWebResponse.GetResponseStream());
                    responseResult = objStreamReader.ReadToEnd().ToString();
                }
                catch (Exception ex)
                {
                    //throw exception error
                    responseResult = "Twitter Post Error: " + ex.Message.ToString() + ", authHeader: " + authorizationHeader;
                }

                SetStateRequest _SetStateReq = new SetStateRequest();
                if (arr[3] == "syt_tweet")//if retweeted from an activity
                {
                    _SetStateReq.EntityMoniker = new EntityReference("syt_tweet", new Guid(guid));
                    _SetStateReq.State = new OptionSetValue(1);
                    _SetStateReq.Status = new OptionSetValue(2);
                    SetStateResponse _SetStateResp = (SetStateResponse)service.Execute(_SetStateReq);
                }
                else if(arr[3]=="incident")//if retweeted from a case
                {
                    //_SetStateReq.EntityMoniker = new EntityReference("incident", new Guid(guid));
                    //_SetStateReq.State = new OptionSetValue(2);
                    //_SetStateReq.Status = new OptionSetValue(6);
                    //SetStateResponse _SetStateResp = (SetStateResponse)service.Execute(_SetStateReq);
                    IncidentResolution res = new IncidentResolution
                    {
                        IncidentId = new EntityReference
                        {
                            LogicalName = Incident.EntityLogicalName,
                            Id = new Guid(guid)
                        }
                    };

                    CloseIncidentRequest request = new CloseIncidentRequest
                    {
                        IncidentResolution = res,
                        Status = new OptionSetValue(5) //Problem Solved
                    };

                    service.Execute(request);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        
    }
}

      