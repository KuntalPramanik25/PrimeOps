using Microsoft.AspNetCore.Mvc;
using PrimeOps.DAL;
using PrimeOps.DAL.Enums;
using PrimeOps.DAL.Tables;

namespace PrimeOps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly PrimeOps.DAL.Linq _mapper = new PrimeOps.DAL.Linq();

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            var result = _mapper.Select<TItemsMaster>(TableName.TItemsMaster);

            return Ok(result);
        }
    }
}