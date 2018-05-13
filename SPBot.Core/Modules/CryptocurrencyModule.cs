using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Net;
using System.Xml.Linq;

namespace SPBot.Core
{
    public class CryptocurrencyModule : ModuleBase
    {
        [Command("CheckCoin", RunMode = RunMode.Async)]
        public async Task CheckCoin(string SearchQuery)
        {
            SearchQuery = SearchQuery.ToUpper();
            string ApiEndpoint = "https://api.kucoin.com/v1/open/currencies?coins=" + SearchQuery;
            using (WebClient WC = new WebClient())
            {
                string response = await WC.DownloadStringTaskAsync(ApiEndpoint);
                Newtonsoft.Json.Linq.JToken Token = Newtonsoft.Json.Linq.JToken.Parse(response);
                if(Token.SelectToken("success").ToObject<bool>())
                {
                    string USDValue = Token.SelectToken($"data.rates.{SearchQuery}.USD").ToString();
                    string GBPValue = Token.SelectToken($"data.rates.{SearchQuery}.GBP").ToString();
                    string MessageStr = "1 " + SearchQuery + " is currently worth $" + USDValue + " or £" + GBPValue;
                    await Context.Message.Channel.SendMessageAsync(MessageStr);
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync("Cannot find any data for " + SearchQuery + ", did you mistype it?");
                }
            }
        }
    }
}
