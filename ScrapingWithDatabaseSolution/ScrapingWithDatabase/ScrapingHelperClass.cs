using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapingWithDatabase
{
    public class ScrapingHelperClass
    {
        // I use this two lists to mask my PC so it can't be noticed that easy, which could result in getting my crawler banned.

        // List of User-Agents
        static List<UserAgentsDto> userAgents = UserAgentGetter.GetUserAgents();
        // List of proxies (IP adresses and ports)
        static List<ProxiesDto> proxies = ProxyGetter.GetProxies();
        
        //This method is used in program.cs for starting scraping on specific page number
        public async static Task<List<WordDto>> GetDataFromPageAsync(int pageNumber)
        {
            // List of word objects
            List<WordDto> words = new List<WordDto>();
            // List of list items
            List<HtmlNode> listItems = await GetListContentAsync(pageNumber);

            //In each list item, select anchor since it's the element that actually contains the word
            foreach (var item in listItems.CssSelect("li a"))
            {
                //Check if word contains only letters
                if (ContainsOnlyLetters(item.InnerText))
                {
                    //Add that word to list
                    words.Add(new WordDto
                    {
                        Word = item.InnerText
                    });
                }
            }

            //Return acquired words 
            return words;
        }

        // PAGE NUMBER GET METHOD
        // Method used for getting page number from website, so program knew how many pages it should crawl over 
        public static int GetPageNumber()
        {
            // Take page content
            WebPage page = GoToPage($"https://www.dictionary.com/list/a");
            int pageNumber = 0;
            // Try pulling out last page number
            try
            {
                HtmlNode lastListItem = page.Html.CssSelect("ol.css-o29oxt.e1wvt9ur0 li a").Last();
                pageNumber = int.Parse(lastListItem.Attributes["data-page"].Value);
                Console.WriteLine(pageNumber);
                return pageNumber;
            }   
            catch(Exception ex)
            {
                Console.WriteLine($"Problem occured while trying to get page number with following error: \n\n{ex.ToString()}");
                return 0;
            }
        }

        // LIST ITEM GET METHOD
        // Method that is used for taking words from page content that's returned from GoToPage method.
        private async static Task<List<HtmlNode>> GetListContentAsync(int pageNumber)
        {
            // Random index that is going to be used for choosing user-agent and IP address.
            int index = GetIndex();
            List<HtmlNode> listItems = null;

            // Since some IPs that are scraped from site are maybe already used, GoToPageAsync will return null which results that listItems wont be changed so it stays null.
            // So while listItems is null ( while IPs are invalid ) try going to that page again, but now with different IP and User-Agent.
            while (listItems == null)
            {
                // Go to page with given page number, IP and User-Agent
                WebPage page = await GoToPageAsync("https://www.dictionary.com/list/a/" + pageNumber.ToString(),
                     proxies.ElementAt(index), userAgents.ElementAt(index));

                //If page content was successfully saved  
                if(page != null)
                {
                    // Try taking items from list 
                    try
                    {
                        // This code takes all list (ul) items with given classes (.css-8j2tgf .e1j8zk4s0)
                        listItems = page.Html.CssSelect("ul.css-8j2tgf.e1j8zk4s0").ToList();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Couldn't get table content. Trying again . . .");
                    }
                }
                //If page content isn't saved that means that IP and User-Agent are not working
                //So this code deletes from lists that IP and User-Agent because it should not be used again
                else
                {
                    //Check if userAgent still contains elements, if it does, delete the IP and User-Agent that were used and did not work.
                    if(userAgents.Count != 0)
                    {
                        userAgents.RemoveAt(index);
                        proxies.RemoveAt(index);
                    }
                    //If it's 0 it means all IPs and User-Agents were deleted, so try taking new one's
                    else
                    {
                        userAgents = UserAgentGetter.GetUserAgents();
                        proxies = ProxyGetter.GetProxies();
                    }
                    //Generate new random index
                    index = GetIndex();
                }
            }

            //In case everything went well, return list items.
            return listItems;
        }

        // CHECK IF STRING CONTAINS ONLY LETTERS
        //There are some words with numbers, dots or slashes, this is "filter" method that is used for skipping that kind of words
        private static bool ContainsOnlyLetters(string word)
        {
            //Checkes every char in word
            //If char is not letter it returns false, else it return true
            foreach (char c in word)
            {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        }

        // PAGE LOADING METHOD WITH USER-AGENT AND PROXY
        // Method that is used for accessing website pages, with proxy
        public async static Task<WebPage> GoToPageAsync(string uri, ProxiesDto portip, UserAgentsDto userAgent)
        {
            try
            {
                //Make new scraping browser
                ScrapingBrowser scrapingBrowser = new ScrapingBrowser();
                //Give that browser other IP Adress and user-agent so it doesn't get banned by site.
                scrapingBrowser.Proxy = new WebProxy(portip.IpAddress, portip.Port);
                scrapingBrowser.UserAgent = new FakeUserAgent("user-agent", userAgent.UserAgent);
                //Set timeout for request
                scrapingBrowser.Timeout = new TimeSpan(5000); // For some reason, this timeout is not respected, sometimes it takes 2 seconds to notice bad proxy and sometimes 5 minutes.
                //Navigate to page and gather it's content
                WebPage page = await scrapingBrowser.NavigateToPageAsync(new Uri(uri));
                //Return page content
                return page;
            }
            catch(Exception)
            {
                Console.WriteLine($"Problem occured while trying to open the page with ip adress: {portip.IpAddress}:{portip.Port}. Trying another one . . .");
                return null;
            }
        }

        // PAGE LOADING METHOD WITHOUT USER-AGENT AND PROXY
        // I use this only for scraping IP adresses and user-agents
        public static WebPage GoToPage(string uri)
        {
            try
            {
                //Make new scraping browser
                ScrapingBrowser scrapingBrowser = new ScrapingBrowser();
                //Navigate to page and gather it's content
                WebPage page = scrapingBrowser.NavigateToPage(new Uri(uri));
                //Return page content
                return page;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem occured while trying to open the page with the url of {uri}. Exception message: \n\n {ex.ToString()}");
                return null;
            }
        }

        // RANDOM INDEX GENERATOR METHOD
        //Method used for random generating index that is going to be used to choose IP and user-agent from lists
        public static int GetIndex()
        {
            Random rnd = new Random();
            return rnd.Next(0, userAgents.Count);
        }
    }
}
