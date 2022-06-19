using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/***************************************************
 * This class used to bind the exchange rates from json
 * ************************************************/
namespace FreightExchangeCalcAPI.Models
{
    public class Rootobject
    {
         [JsonProperty("exchange_rates")]
          public Dictionary<string, Dictionary<string, double>> exchange_rates { get; set; }
       
    }



}
