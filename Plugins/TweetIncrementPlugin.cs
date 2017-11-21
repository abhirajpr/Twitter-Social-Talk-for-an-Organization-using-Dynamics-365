using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweetNumberingPlugin
{
    public class TweetIncrementPlugin: IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {

                // Obtain the target entity from the input parameters.
                Entity activitypointer = (Entity)context.InputParameters["Target"];


                try
                {   // Retrieve the autonumbering records with entity name specified in the current form
                    QueryExpression query = new QueryExpression();
                    query.EntityName = "syt_autonumber";
                    query.ColumnSet = new ColumnSet() { AllColumns = true };
                    query.Criteria = new FilterExpression();
                    query.Criteria.FilterOperator = LogicalOperator.And;
                    query.Criteria.Conditions.AddRange(
                        new ConditionExpression("syt_checkenabled", ConditionOperator.Equal, true),
                        new ConditionExpression("syt_entityset", ConditionOperator.Equal, 214900000)

                    );
                    EntityCollection entities = service.RetrieveMultiple(query);
                    //If there is no autonumbering record exists for this activity then a random number is assigned
                    if(entities.Entities.Count==0)
                    {
                        Random rnd = new Random();
                        int dice = rnd.Next(1, 100);
                        activitypointer["syt_activitynumber"] = dice.ToString();
                        service.Update(activitypointer);
                        return;
                    }
                   
                    Entity entity = entities[0];
                    string hash = activitypointer["syt_hashtag"].ToString();
                    hash = hash.Remove(0, 1);
                    //Set the activity number of that tweet activity 
                    activitypointer["syt_activitynumber"] = string.Concat(entity.Attributes["syt_prefix"], "-", hash, "-", entity.Attributes["syt_generatedcode"]);

                    int x = Convert.ToInt32(entity.Attributes["syt_generatedcode"]);
                    x++;
                    int y = Convert.ToInt32(entity.Attributes["syt_length"]);
                    string str = x.ToString();
                    str = str.PadLeft(y, '0');
                    entity.Attributes["syt_generatedcode"] = str;
                    service.Update(activitypointer);
                    service.Update(entity);


                }

                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
    }
}
