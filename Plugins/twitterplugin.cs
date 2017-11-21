using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Net;
using System.IO;

namespace Get_Tweets
{
    public class TwitterTweets : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            try
            {

                string credentials = null;
                try
                {
                    QueryExpression query = new QueryExpression();
                    query.EntityName = "syt_twittercredential";
                    query.ColumnSet = new ColumnSet() { AllColumns = true };
                    EntityCollection entities = service.RetrieveMultiple(query);
                    Entity entity = entities[0];
                    credentials = entity.Attributes["syt_twitterauth"].ToString();


                }
                catch (Exception ex)
                {

                    throw new InvalidPluginExecutionException(ex.Message);
                }


                string access_token = "";
                var post = WebRequest.Create("https://api.twitter.com/oauth2/token") as HttpWebRequest;
                post.Method = "POST";
                post.ContentType = "application/x-www-form-urlencoded";
                post.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                var reqbody = Encoding.UTF8.GetBytes("grant_type=client_credentials");
                post.ContentLength = reqbody.Length;
                using (var req = post.GetRequestStream())
                {
                    req.Write(reqbody, 0, reqbody.Length);
                }
                try
                {
                    string respbody = null;
                    using (var resp = post.GetResponse().GetResponseStream())//there request sends
                    {
                        var respR = new StreamReader(resp);
                        respbody = respR.ReadToEnd();
                    }

                    access_token = respbody.Substring(respbody.IndexOf("access_token\":\"") + "access_token\":\"".Length, respbody.IndexOf("\"}") - (respbody.IndexOf("access_token\":\"") + "access_token\":\"".Length));
                }
                catch //if credentials are not valid
                {
                    //TODO
                }

             
                var crm = WebRequest.Create("https://api.twitter.com/1.1/search/tweets.json?q=%23crm-filter:retweets&-filter:replies&result_type=recent&count=30‏") as HttpWebRequest;
                crm.Method = "GET";
                crm.Headers[HttpRequestHeader.Authorization] = "Bearer " + access_token;
                var cms = WebRequest.Create("https://api.twitter.com/1.1/search/tweets.json?q=%23cms-filter:retweets&-filter:replies&result_type=recent&count=30‏") as HttpWebRequest;
                cms.Method = "GET";
                cms.Headers[HttpRequestHeader.Authorization] = "Bearer " + access_token;
                var suyati = WebRequest.Create("https://api.twitter.com/1.1/search/tweets.json?q=%23suyati-filter:retweets&-filter:replies&result_type=recent&count=30‏") as HttpWebRequest;
                suyati.Method = "GET";
                suyati.Headers[HttpRequestHeader.Authorization] = "Bearer " + access_token;
                var salesforce = WebRequest.Create("https://api.twitter.com/1.1/search/tweets.json?q=%23salesforce-filter:retweets&-filter:replies&result_type=recent&count=30‏") as HttpWebRequest;
                salesforce.Method = "GET";
                salesforce.Headers[HttpRequestHeader.Authorization] = "Bearer " + access_token;
                var dynamics = WebRequest.Create("https://api.twitter.com/1.1/search/tweets.json?q=%23msdynamics-filter:retweets&-filter:replies&result_type=recent&count=30‏") as HttpWebRequest;
                dynamics.Method = "GET";
                dynamics.Headers[HttpRequestHeader.Authorization] = "Bearer " + access_token;
                try
                {
                    string crmtweet = null;
                    using (var resp = crm.GetResponse().GetResponseStream())//get tweets with #crm
                    {
                        var respR = new StreamReader(resp);
                        crmtweet = respR.ReadToEnd();
                    }
                    string cmstweet = null;
                    using (var resp = cms.GetResponse().GetResponseStream())//get tweets with #cms
                    {
                        var respR = new StreamReader(resp);
                        cmstweet = respR.ReadToEnd();
                    }
                    string suyatitweet = null;
                    using (var resp = suyati.GetResponse().GetResponseStream())//get tweets with #suyati
                    {
                        var respR = new StreamReader(resp);
                        suyatitweet = respR.ReadToEnd();
                    }
                    string salesforcetweet = null;
                    using (var resp = salesforce.GetResponse().GetResponseStream())//get tweets with #salesforce
                    {
                        var respR = new StreamReader(resp);
                        salesforcetweet = respR.ReadToEnd();
                    }
                    string dynamicstweet = null;
                    using (var resp = dynamics.GetResponse().GetResponseStream())//get tweets with #dynamics
                    {
                        var respR = new StreamReader(resp);
                        dynamicstweet = respR.ReadToEnd();
                    }



                    //Passing it to Action
                    context.OutputParameters["crmtweets"] = crmtweet.ToString();
                    context.OutputParameters["cmstweets"] = cmstweet.ToString();
                    context.OutputParameters["suyatitweets"] = suyatitweet.ToString();
                    context.OutputParameters["salesforcetweets"] = salesforcetweet.ToString();
                    context.OutputParameters["dynamicstweets"] = dynamicstweet.ToString();


                }
                catch //401 (access token invalid or expired)
                {
                    //TODO
                }
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
