using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private FlightGearClient comm_thread;


        public CommandController(FlightGearClient client)
        {
            comm_thread = client;
        }

        // POST: api/Command
        [HttpPost]
        public async Task<StatusCodeResult> Post(Command command)
        {
            var response = await comm_thread.Execute(command);
            if (response == Result.Ok)
            {
                return StatusCode(200);
            } else
            {
                return StatusCode(500);
            }
        }
    }
}
