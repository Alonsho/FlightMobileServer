using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileWeb.Controllers
{
    // TODO add connect command and get screenshot command
    
    [ApiController]
    public class CommandController : ControllerBase
    {
        private FlightGearClient comm_thread;


        public CommandController(FlightGearClient client)
        {
            comm_thread = client;
        }

        // POST: api/connect
        [HttpPost]
        [Route("api/connect")]
        public StatusCodeResult Post()
        {
            var connected = comm_thread.Connect();
            if (connected)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(500);
            }
        }

        // POST: api/Command
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<StatusCodeResult> Post(Command command)
        {
            // TODO remove test
            /*if (command.Rudder == 0.8)
            {
                return StatusCode(500);
            }*/
            try
            {
                var response = await comm_thread.Execute(command);
                if (response == Result.Ok)
                {
                    return StatusCode(200);
                }
                else
                {
                    return StatusCode(500);
                }
                // TODO change response code according to error (null pointer is connection fail and io is timeout)
            } catch
            {
                return StatusCode(500);
            }
        }


        [HttpGet]
        [Route("api/screenshot")]
        public async Task<ActionResult> Get()
        {
            string to = "http://127.0.0.1:5000/screenshot";
            byte[] response = await new HttpClient().GetByteArrayAsync(to);
            if (response != null) //success
                try
                {
                    var file = File(response, "image/jpeg"); //200 Ok  + the image
                    return file;
                } catch
                {
                    return StatusCode(500);
                }
            return StatusCode(500);
        }
    }
}
