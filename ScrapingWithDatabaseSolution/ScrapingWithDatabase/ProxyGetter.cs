using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ScrapingWithDatabase
{
    public class ProxyGetter
    {
        public static List<ProxiesDto> GetProxies()
        {
            //Create proxy list, currently empty
            List<ProxiesDto> proxy = new List<ProxiesDto>();
            //Go to website that offers free IPs and ports
            WebPage page = ScrapingHelperClass.GoToPage("https://www.us-proxy.org/");
            //Get specific rows that contains IPs and ports
            var tableRows = page.Html.CssSelect("div.table-responsive table tbody tr");
            //Save to list every IP and port
            foreach (var row in tableRows)
            {
                proxy.Add(new ProxiesDto
                {
                    //Get IP Adress
                    IpAddress = row.CssSelect("td").First().InnerText,
                    //Get port
                    Port = int.Parse(row.CssSelect("td").Skip(1).First().InnerText)
                });
            }

            //Return full list of proxies
            return proxy;
        }
    }
}
