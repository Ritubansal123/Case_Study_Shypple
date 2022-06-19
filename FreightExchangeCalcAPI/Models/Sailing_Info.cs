using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/***************************************************
 * This class used to bind and return the values
 * ************************************************/
namespace FreightExchangeCalcAPI.Models
{
    public class Sailing_Info
    {
        public string Origin_Port { get; set; }
        public string Destination_Port { get; set; }
        public string Departure_Date { get; set; }
        public string Arrival_Date { get; set; }
        public string Sailing_Code { get; set; }
        public double Rate { get; set; }
        public string Rate_Currency { get; set; }
    }
}
