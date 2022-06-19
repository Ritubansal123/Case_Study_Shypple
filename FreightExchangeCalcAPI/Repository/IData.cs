using FreightExchangeCalcAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/***************************************************
 * This Interface is used to define the function 
 * ************************************************/
namespace FreightExchangeCalcAPI.Repository
{
    public interface IData
    {
        Task<IEnumerable<Sailing_Info>> GetCheapestDirectSailingData();
        Task<IEnumerable<Sailing_Info>> GetCheapestSailingData();
        Task<IEnumerable<Sailing_Info>> GetFastestSailingData();
    }
}
