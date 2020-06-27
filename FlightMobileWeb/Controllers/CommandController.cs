using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlightMobileWeb.Controllers
{
    
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly FlightGearClient comm_thread;
        


        public CommandController(FlightGearClient client, IConfiguration configuration)
        {
            comm_thread = client;
            config = configuration;
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
            } catch
            {
                return StatusCode(500);
            }
        }


        [HttpGet]
        [Route("api/screenshot")]
        public async Task<ActionResult> Get()
        {
            string simulatorIP = config.GetSection("ServerSettings").GetSection("serverIP").Value;
            var TCPPORT_s = config.GetSection("ServerSettings").GetSection("HTTPPORT").Value;
            string to = simulatorIP;
            if (!simulatorIP.StartsWith("http://"))
            {
                to = "http://" + simulatorIP;
            }
            to += ":" + TCPPORT_s + "/screenshot";
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
