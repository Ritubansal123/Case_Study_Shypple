using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreightExchangeCalcAPI.Models;
using Newtonsoft.Json.Linq;
using System.Collections;
using Microsoft.Extensions.Configuration;
/***************************************************
 * This class used to retrieve the data 
 * ************************************************/
namespace FreightExchangeCalcAPI.Repository
{
    public class Data : IData
    {                                                 
     
        //Define and initialize the variable used to get the data
        dynamic jsonSailing=null, jsonRailing = null, jsonExchange = null;
        string jsonFilePath = "";
        IEnumerable<Sailing_Info> sailing_data = null;
        string Origin_Port = "", Destination_port = "", Departure_Date = "", sailing_code = "", rate_curr = "", Arrival_Date = "";
        double rate = 0.0, amt = 0.0;
        List<Sailings> sail = new List<Sailings>();
        List<string> discard_cd = new List<string>();
        List<Rates> rate_data = new List<Rates>();
        Dictionary<string, double> dict_sailing_amt = new Dictionary<string, double>();
        Dictionary<string, double> dict_indirect_amt = new Dictionary<string, double>();

        IConfiguration _configuration { get; set; }
        //In constructor set the path, read the JSON file and set the values
        public Data(IConfiguration configuration) 
        {
            _configuration = configuration;
            jsonFilePath = System.IO.File.ReadAllText(_configuration["JsonFilePath"]);
            jsonSailing = JsonConvert.DeserializeObject<SailingsList>(jsonFilePath);
            jsonRailing = JsonConvert.DeserializeObject<RatesList>(jsonFilePath);
            jsonExchange = JsonConvert.DeserializeObject<Rootobject>(jsonFilePath);
            sail = jsonSailing.sailings;
            rate_data = jsonRailing.Rates;
        
        }

        //This function is used fetching the cheapest direct sailing data
        public async Task<IEnumerable<Sailing_Info>> GetCheapestDirectSailingData()
        {
            try
            {
                if (jsonSailing.sailings != null)
                {
                    for (int i = 0; i < jsonSailing.sailings.Count; i++)
                    {
                        Origin_Port = jsonSailing.sailings[i].Origin_Port;
                        Destination_port = jsonSailing.sailings[i].Destination_Port;
                        Departure_Date = jsonSailing.sailings[i].Departure_Date;
                        sailing_code = jsonSailing.sailings[i].Sailing_Code;
                        GetRates();
                        if (Origin_Port == "CNSHA" && Destination_port == "NLRTM")
                        {
                            GetExchangeData(Departure_Date, rate_curr, sailing_code,true);
                        }
                    }
                }
                //Create JSON and send the response
                sailing_data = GetData(dict_sailing_amt,true);
                return await Task.FromResult(sailing_data);
            }
            catch (Exception ex) {
            #pragma warning disable CA2200 // Rethrow to preserve stack details
                throw ex;
            #pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }

        //This function is used to calculate the exchange rate of sailing port
        private void GetExchangeData(string Departure_Date,string rate_curr,string sailing_code,Boolean checkVal)
        {
            foreach (dynamic kvp in jsonExchange.exchange_rates)
            {
                dynamic departureDT = ((System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.Dictionary<string, double>>)kvp).Key;
                if (departureDT == Departure_Date)
                {
                    foreach (dynamic ex_rate in ((System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.Dictionary<string, double>>)kvp).Value)
                    {
                        if (rate_curr.ToUpper() == ((System.Collections.Generic.KeyValuePair<string, double>)ex_rate).Key.ToUpper() || rate_curr.ToUpper() == "EUR")
                        {
                            amt = ((System.Collections.Generic.KeyValuePair<string, double>)ex_rate).Value;
                            double sailingamt = Math.Round(amt * rate, 2);
                            if(checkVal)
                                dict_sailing_amt.Add(sailing_code, sailingamt);
                            else
                               dict_indirect_amt.Add(sailing_code, sailingamt);

                            break;
                        }
                    }
                }
            }
        }

        //This function retrieve json and send the response to server
        private IEnumerable<Sailing_Info> GetData(Dictionary<string, double> dict_sailing_amt,Boolean checkVal)
        {
            try
            {
               var sailing_code = (dynamic)null;
               IEnumerable<Sailing_Info> allInts = null;
               if (checkVal)
                {
                    sailing_code = dict_sailing_amt.OrderBy(k => k.Value).FirstOrDefault();
                    sailing_data = (from hh in sail
                                    join kk in rate_data on hh.Sailing_Code equals kk.Sailing_Code
                                    where hh.Sailing_Code == Convert.ToString(sailing_code.Key)
                                    select new Sailing_Info
                                    {
                                        Origin_Port = hh.Origin_Port,
                                        Destination_Port = hh.Destination_Port,
                                        Departure_Date = hh.Departure_Date,
                                        Arrival_Date = hh.Arrival_Date,
                                        Sailing_Code = hh.Sailing_Code,
                                        Rate = kk.Rate,
                                        Rate_Currency = kk.Rate_Currency
                                    });
                }
                else {
                    foreach (var key in dict_sailing_amt)
                    {
                        sailing_data = (from hh in sail
                                        join kk in rate_data on hh.Sailing_Code equals kk.Sailing_Code
                                        where hh.Sailing_Code == Convert.ToString(key.Key)
                                        select new Sailing_Info
                                        {
                                            Origin_Port = hh.Origin_Port,
                                            Destination_Port = hh.Destination_Port,
                                            Departure_Date = hh.Departure_Date,
                                            Arrival_Date = hh.Arrival_Date,
                                            Sailing_Code = hh.Sailing_Code,
                                            Rate = kk.Rate,
                                            Rate_Currency = kk.Rate_Currency
                                        }).ToArray();
                        if (allInts == null)
                            allInts = sailing_data;
                        else
                            allInts = allInts.Concat(sailing_data);
                    }
                    sailing_data = allInts;
                }
                return sailing_data ;
            }
            catch (Exception ex) {
            #pragma warning disable CA2200 // Rethrow to preserve stack details
                throw ex;
            #pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }

        //This function is used to sum the value of two sailing legs
        private double CheckCalcValues(double sailing_value, Dictionary<string, double> dict)
        {
            double Total_sum = 0;
            foreach (var (key, value) in dict)
            {
                double val = sailing_value + value;
                if (Total_sum < val)
                    Total_sum = val;
                else
                    dict.Remove(key);
              
                discard_cd.Add(key);
            }
            return Total_sum;
        }

        //This function is used to get the rates on the basis of sailing code 
        private void GetRates() 
        {
            foreach (Rates rates in jsonRailing.Rates)
            {
                if (rates.Sailing_Code == sailing_code)
                {
                    rate = rates.Rate;
                    rate_curr = rates.Rate_Currency;
                    break;
                }
            }
        }

        //This function is used to get the cheapest sailing leg for direct and indirect both
        public async Task<IEnumerable<Sailing_Info>> GetCheapestSailingData()
        {
            try
            {
                if (jsonSailing.sailings != null)
                {
                    for (int i = 0; i < jsonSailing.sailings.Count; i++)
                    {
                        Origin_Port = jsonSailing.sailings[i].Origin_Port;
                        Destination_port = jsonSailing.sailings[i].Destination_Port;
                        Departure_Date = jsonSailing.sailings[i].Departure_Date;
                        sailing_code = jsonSailing.sailings[i].Sailing_Code;
                        if (!discard_cd.Contains(sailing_code))
                        {
                            GetRates();
                            GetExchangeData(Departure_Date, rate_curr, sailing_code, true);

                            if (Destination_port != "NLRTM")
                            {
                                List<Sailing_Info> indirectSail = new List<Sailing_Info>();
                                indirectSail = (from hh in sail
                                                join kk in rate_data on hh.Sailing_Code equals kk.Sailing_Code
                                                where hh.Origin_Port == Destination_port && hh.Destination_Port == "NLRTM" && hh.Sailing_Code != sailing_code
                                                select new Sailing_Info
                                                {
                                                    Departure_Date = hh.Departure_Date,
                                                    Sailing_Code = hh.Sailing_Code,
                                                    Rate = kk.Rate,
                                                    Rate_Currency = kk.Rate_Currency
                                                }).ToList();
                                if (indirectSail.Count != 0)
                                {
                                    foreach (var checkdata in indirectSail)
                                    {
                                        GetExchangeData(checkdata.Departure_Date, checkdata.Rate_Currency, checkdata.Sailing_Code, false);
                                    }
                                    var sailing = dict_sailing_amt.Last();
                                    dict_sailing_amt[sailing.Key] = CheckCalcValues(sailing.Value, dict_indirect_amt);
                                    var sailing_cd = dict_sailing_amt.OrderBy(k => k.Value).FirstOrDefault();
                                    foreach (var item in dict_sailing_amt.Where(i => i.Key != sailing_cd.Key))
                                    {
                                        dict_sailing_amt.Remove(item.Key);
                                    }
                                    if (dict_sailing_amt.ContainsKey(sailing.Key))
                                    {
                                        var keyValue = dict_indirect_amt.First();
                                        dict_sailing_amt.Add(Convert.ToString(keyValue.Key), Convert.ToDouble(keyValue.Value));
                                    }
                                }
                            }
                         
                        }
                    }
                }
                var sail_rem_cd = dict_sailing_amt.OrderBy(k => k.Value).LastOrDefault();
                foreach (var item in dict_sailing_amt.Where(i => i.Key == sail_rem_cd.Key))
                {
                    dict_sailing_amt.Remove(item.Key);
                }
                sailing_data = GetData(dict_sailing_amt,false);
                return await Task.FromResult(sailing_data);
            }
            catch (Exception ex)
            {
            #pragma warning disable CA2200 // Rethrow to preserve stack details
                throw ex;
            #pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }

        //This function is used to get the fastest sailing data fro json
        public async Task<IEnumerable<Sailing_Info>> GetFastestSailingData()
        {
            try
            {
                if (jsonSailing.sailings != null)
                {
                    for (int i = 0; i < jsonSailing.sailings.Count; i++)
                    {
                        Arrival_Date = jsonSailing.sailings[i].Arrival_Date;
                        Departure_Date = jsonSailing.sailings[i].Departure_Date;
                        sailing_code = jsonSailing.sailings[i].Sailing_Code;
                        DateTime arrivalDate = Convert.ToDateTime(Arrival_Date);
                        DateTime DepartureDate = Convert.ToDateTime(Departure_Date);
                        double diff= (arrivalDate - DepartureDate).TotalDays;
                        dict_sailing_amt.Add(sailing_code, diff);
                    }
                }
                sailing_data = GetData(dict_sailing_amt, true);
                return await Task.FromResult(sailing_data);
            }
            catch (Exception ex)
            {
#pragma warning disable CA2200 // Rethrow to preserve stack details
                throw ex;
#pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }

    }
}
