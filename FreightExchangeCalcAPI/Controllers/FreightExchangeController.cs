using FreightExchangeCalcAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreightExchangeCalcAPI.Models;
/*****************************************
 * This is controller file used to Get the Data
 * **************************************/
namespace FreightExchangeCalcAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FreightExchangeController : ControllerBase
    {
      
        public IData data { get; set; }
        //To achieve dependency Injection allocate the memory through construction class of controller 
        public FreightExchangeController(IData _data)
        {
            data = _data;   
        }


        //This function is used retrieve the Cheapest direct sailing
        [HttpGet("GetCheapestDirectSailing")]
        public async Task<IEnumerable<Sailing_Info>> GetCheapestDirectSailing()
        {
            try
            {
                return await data.GetCheapestDirectSailingData();
               
            }
            catch (Exception ex)
            {
            #pragma warning disable CA2200 // Rethrow to preserve stack details
                throw ex;
            #pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }

        //This function is used to retrieve the Cheapest Sailing either direct or indirect 
        [HttpGet("GetCheapestSailingData")]
        public async Task<IEnumerable<Sailing_Info>> GetCheapestSailingData()
        {
            try
            {
                return await data.GetCheapestSailingData();

            }
            catch (Exception ex)
            {
            #pragma warning disable CA2200 // Rethrow to preserve stack details
                throw ex;
            #pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        }


        //This function is used to retieve the  fastest sailing legs based on departure date and arrival date
        [HttpGet("GetFastestSailingData")]
        public async Task<IEnumerable<Sailing_Info>> GetFastestSailingData()
        {
            try
            {
                return await data.GetFastestSailingData();

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
