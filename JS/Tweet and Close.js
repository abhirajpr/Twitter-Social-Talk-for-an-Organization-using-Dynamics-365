function RetweetAndClose(tweetId,typename)
{
    debugger;
    //Dialog Box creation
    var DialogOption = new window.parent.Xrm.DialogOptions;
    DialogOption.width = 400; DialogOption.height = 530;
    //Tweet message content obtained and passing the value to CallbackFunction()
    window.parent.Xrm.Internal.openDialog("/WebResources/syt_retweetpopup", DialogOption, null, null, CallbackFunction);

    function CallbackFunction(tweetMessage) 
    {
        debugger;
        //TweetId and name are obtained
        var id= tweetId.replace("{","").replace("}","").toLowerCase();
        var Name=typename;
        var tweet;
        var thandle;
        var retweetmsg;
        var organizationUrl = parent.Xrm.Page.context.getClientUrl(); 


        //Checking for Tweeting back is to be done from within the Activity 
        if(Name=="syt_tweet")
        {
            var req = new XMLHttpRequest(); 
            req.open("GET", organizationUrl + "/api/data/v9.0/syt_tweets/", false); 
            req.setRequestHeader("Accept", "application/json"); 
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8"); 
            req.setRequestHeader("OData-MaxVersion", "4.0"); 
            req.setRequestHeader("OData-Version", "4.0"); 
            req.onreadystatechange = function (){ 
            if (this.readyState == 4) 
            { 
                req.onreadystatechange = null; 
                if (this.status == 200) 
                { 
                    var result = JSON.parse(this.response);
                    for (var i = 0; i < result.value.length; i++) 
                    {
                        if( result.value[i].activityid.toString() == id.toString())
                            {
                                    tweet=result.value[i].syt_tweetid.toString();
                                    thandle=result.value[i].syt_twitterhandle.toString();

                            }
                    }

                 } 
                 else 
                 { 
                    var error = JSON.parse(this.response).error; 
                    alert(error.message); 
                 } 
                    
            } 
            };

            req.send();
        }
        //Checking for Tweeting back is to be done from Case    
        else if(Name=="incident")
        {
            var req = new XMLHttpRequest(); 
            req.open("GET", organizationUrl + "/api/data/v9.0/incidents/", false); 
            req.setRequestHeader("Accept", "application/json"); 
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8"); 
            req.setRequestHeader("OData-MaxVersion", "4.0"); 
            req.setRequestHeader("OData-Version", "4.0"); 
            req.onreadystatechange = function () { 
            if (this.readyState == 4) 
            { 
                req.onreadystatechange = null; 
                if (this.status == 200) 
                { 
                    var result = JSON.parse(this.response);
                    for (var i = 0; i < result.value.length; i++) 
                    {
                        if( result.value[i].incidentid.toString() == id.toString())
                            {
                                    tweet=result.value[i].syt_tweetid.toString();
                                    thandle=result.value[i].syt_twitterhandle.toString();

                            }
                    }

                } 
                else 
                { 
                    var error = JSON.parse(this.response).error; 
                    alert(error.message); 
                } 
            } 
            };

            req.send();
        }
        //Checking for Tweeting back is to be done from opportunity
        else if(Name=="opportunity")
        {
            var req = new XMLHttpRequest(); 
            req.open("GET", organizationUrl + "/api/data/v9.0/opportunities/", false); 
            req.setRequestHeader("Accept", "application/json"); 
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8"); 
            req.setRequestHeader("OData-MaxVersion", "4.0"); 
            req.setRequestHeader("OData-Version", "4.0"); 
            req.onreadystatechange = function () { 
            if (this.readyState == 4) 
            { 
                req.onreadystatechange = null; 
                if (this.status == 200) 
                { 
                    var result = JSON.parse(this.response);
                    for (var i = 0; i < result.value.length; i++) 
                    {
                        if( result.value[i].opportunityid.toString() == id.toString())
                        {
                            tweet=result.value[i].syt_tweetid.toString();
                            thandle=result.value[i].syt_twitterhandle.toString();

                        }
                    }
                } 
                else 
                { 
                    var error = JSON.parse(this.response).error; 
                    alert(error.message); 
                } 
            } 
            };

            req.send();
        }
        
        if((tweetMessage!=undefined)&&(id!=undefined))
        {
            retweetmsg=tweet+" "+thandle+" "+id+" "+Name;
            var organizationUrl = parent.Xrm.Page.context.getClientUrl(); 
            var data = { 
                "Retweetid": retweetmsg, 
                "Message": tweetMessage
            };
            var req = new XMLHttpRequest(); 
            //Calling the action syt_retweet
            req.open("POST", organizationUrl + "/api/data/v9.0/syt_retweet/", false); 
            req.setRequestHeader("Accept", "application/json"); 
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8"); 
            req.setRequestHeader("OData-MaxVersion", "4.0"); 
            req.setRequestHeader("OData-Version", "4.0"); 
            req.onreadystatechange = function () { 
            if (this.readyState == 4) 
            { 
                req.onreadystatechange = null; 
                if (this.status == 200) 
                { 
                    alert("Action called successfully");
                } 
                else 
                { 
                    var error = JSON.parse(this.response).error; 
                    alert(error.message); 
                } 
            } 
            };
            //Tweet message with twitter handle ,tweetId are send
            req.send(window.JSON.stringify(data));
        }

    }

}