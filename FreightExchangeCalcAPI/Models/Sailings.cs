using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/***************************************************
 * This class used to bind the sailing information from json
 * ************************************************/
namespace FreightExchangeCalcAPI.Models
{
    public class Sailings
    {
        public string Origin_Port { get; set; }
        public string Destination_Port { get; set; }
        public string Departure_Date { get; set; }
        public string Arrival_Date { get; set; }
        public string Sailing_Code { get; set; }
    }

    public class SailingsList
    {
        public List<Sailings> sailings { get; set; }

    }
}





