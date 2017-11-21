using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNumberingPlugin
{
    public class AutoIncrementPlugin: IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {

                // Obtain the target entity from the input parameters.
                Entity syt_autonumber = (Entity)context.InputParameters["Target"];


                try
                {   // To set generated code field (starting from 01,001,etc according to length)
                    int x = Convert.ToInt32(syt_autonumber["syt_length"]);
                    string y = "1";
                    y = y.PadLeft(x, '0');
                    syt_autonumber["syt_generatedcode"] = y;
                    //To retrieve records with same entity name
                    QueryExpression query = new QueryExpression();
                    query.EntityName = "syt_autonumber";
                    query.ColumnSet = new ColumnSet() { AllColumns = true };

                    query.Criteria = new FilterExpression();
                    query.Criteria.FilterOperator = LogicalOperator.And;
                    query.Criteria.Conditions.Add
                    (
                        new ConditionExpression("syt_entityset", ConditionOperator.Equal, ((OptionSetValue)syt_autonumber["syt_entityset"]).Value)
                    );
                    //Adding all such records to a collection

                    EntityCollection entityCollection = service.RetrieveMultiple(query);
                    //If more than 1 record found, then set 'is enabled' field of that record to "No"
                    if (entityCollection.Entities.Count > 1)
                    {
                        foreach (Entity autoNumber in entityCollection.Entities)
                        {
                            autoNumber["syt_checkenabled"] = false;
                            service.Update(autoNumber);
                        }
                        //Set 'is enabled' field of the current record to "Yes"
                        syt_autonumber["syt_checkenabled"] = true;
                        service.Update(syt_autonumber);
                    }
                    else
                    {
                        service.Update(syt_autonumber);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
    }
}
