using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/***************************************************
 * This class used to bind the rates from json
 * ************************************************/
namespace FreightExchangeCalcAPI.Models
{
    public class Rates
    {
        public string Sailing_Code { get; set; }
        public double Rate { get; set; }
        public string Rate_Currency { get; set; }
    }

    public class RatesList
    {
        public List<Rates> Rates { get; set; }

    }
}
