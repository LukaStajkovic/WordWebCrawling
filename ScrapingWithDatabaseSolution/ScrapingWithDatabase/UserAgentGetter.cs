using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapingWithDatabase
{
    public class UserAgentGetter
    {
        public static List<UserAgentsDto> GetUserAgents()
        {
            //List of user agents, currently empty
            List<UserAgentsDto> userAgents = new List<UserAgentsDto>();
            //Go to website that offers list of user-agents
            WebPage page = ScrapingHelperClass.GoToPage("https://developers.whatismybrowser.com/useragents/explore/software_name/chrome/2");
            //Scrape user-agents 
            var tableRows = page.Html.CssSelect("div.corset table tbody tr");
            //Save scraped to list
            foreach (var row in tableRows)
            {
                userAgents.Add(new UserAgentsDto
                {
                    UserAgent = row.CssSelect("td.useragent a").First().InnerText
                });
            }

            //Return full list with user-agents
            return userAgents;
        }
    }
}
