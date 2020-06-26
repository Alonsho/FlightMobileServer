using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;



namespace FlightMobileWeb
{
    public class FlightGearClient
    {
        private readonly BlockingCollection<AsyncCommand> _queue;
        private readonly TcpClient _client;
        private NetworkStream stream;
        private bool isConnected = false;
        private readonly IConfiguration config;

        static private string AILERON_PATH = "/controls/flight/aileron";
        static private string THROTTLE_PATH = "/controls/engines/current-engine/throttle";
        static private string ELEVATOR_PATH = "/controls/flight/elevator";
        static private string RUDDER_PATH = "/controls/flight/rudder";

        public FlightGearClient(IConfiguration configyration)
        {
            config = configyration;
            _queue = new BlockingCollection<AsyncCommand>();
            _client = new TcpClient();
            Start();
        }



        public bool Connect()
        {
            if (isConnected)
            {
                return true;
            }
            try
            {
                string simulatorIP = config.GetSection("ServerSettings")
                    .GetSection("serverIP").Value;
                if (simulatorIP.StartsWith("http://"))
                {
                    simulatorIP = simulatorIP.Remove(0, "http://".Length);
                }
                var TCPPORT_s = config.GetSection("ServerSettings").GetSection("TCPPORT").Value;
                var TCPPORT = Int32.Parse(TCPPORT_s);
                _client.Connect(simulatorIP, TCPPORT);
                _client.ReceiveTimeout = 15000;
                stream = _client.GetStream();
                byte[] start = System.Text.Encoding.ASCII.GetBytes("data\n");
                stream.Write(start, 0, start.Length);
                isConnected = true;
                return true;
            }
            catch
            {
                return false;
            }
        }


        // Called by the WebApi Controller, it will await on the returned Task<>
        // This is not an async method, since it does not await anything.
        public Task<Result> Execute(Command cmd)
        {
            var asyncCommand = new AsyncCommand(cmd);
            _queue.Add(asyncCommand);
            return asyncCommand.Task;
        }


        public void Start()
        {
            Task.Factory.StartNew(ProcessCommands);
        }


        public void ProcessCommands()
        {
            foreach (AsyncCommand command in _queue.GetConsumingEnumerable())
            {
                try
                {
                    Result res;
                    sendData(stream, command.Command);
                    if (checkRecieved(stream, command.Command) == true)
                    {
                        res = Result.Ok;
                    }
                    else
                    {
                        res = Result.NotOk;
                    }
                    command.Completion.SetResult(res);
                } catch (Exception e)
                {
                    command.Completion.SetException(e);
                }
            }
        }


        private void sendData(NetworkStream stream, Command command)
        {
            var aileronVal = command.Aileron;
            var rudderVal = command.Rudder;
            var throttleVal = command.Throttle;
            var elevatorVal = command.Elevator;
            string toSend = "set " + AILERON_PATH + " " + aileronVal.ToString() + " \r\n";
            toSend += "set " + RUDDER_PATH + " " + rudderVal.ToString() + " \r\n";
            toSend += "set " + THROTTLE_PATH + " " + throttleVal.ToString() + " \r\n";
            toSend += "set " + ELEVATOR_PATH + " " + elevatorVal.ToString() + " \r\n";
            byte[] sendBuffer = System.Text.Encoding.ASCII.GetBytes(toSend);
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            // not sure if real simultor can process multiple 'set' commands in one message. uncomment and delete above if not
            /*byte[] sendBuffer = System.Text.Encoding.ASCII.GetBytes("set " + AILERON_PATH + " " + aileronVal.ToString() + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            sendBuffer = System.Text.Encoding.ASCII.GetBytes("set " + RUDDER_PATH + " " + rudderVal.ToString() + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            sendBuffer = System.Text.Encoding.ASCII.GetBytes("set " + THROTTLE_PATH + " " + throttleVal.ToString() + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            sendBuffer = System.Text.Encoding.ASCII.GetBytes("set " + ELEVATOR_PATH + " " + elevatorVal.ToString() + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);*/
        }


    private bool checkRecieved(NetworkStream stream, Command command)
        {
            byte[] sendBuffer = new byte[1024];
            byte[] recvBuffer = new byte[1024];
            int bytes;
            double recvValue;
            sendBuffer = System.Text.Encoding.ASCII.GetBytes("get " + AILERON_PATH + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            bytes = stream.Read(recvBuffer, 0, recvBuffer.Length);
            string retString = System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes);
            recvValue = double.Parse(System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes));
            if (command.Aileron != recvValue)
            {
                return false;
            }

            sendBuffer = System.Text.Encoding.ASCII.GetBytes("get " + RUDDER_PATH + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            bytes = stream.Read(recvBuffer, 0, recvBuffer.Length);
            recvValue = double.Parse(System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes));
            if (command.Rudder != recvValue)
            {
                return false;
            }

            sendBuffer = System.Text.Encoding.ASCII.GetBytes("get " + THROTTLE_PATH + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            bytes = stream.Read(recvBuffer, 0, recvBuffer.Length);
            recvValue = double.Parse(System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes));
            if (command.Throttle != recvValue)
            {
                return false;
            }

            sendBuffer = System.Text.Encoding.ASCII.GetBytes("get " + ELEVATOR_PATH + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            bytes = stream.Read(recvBuffer, 0, recvBuffer.Length);
            recvValue = double.Parse(System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes));
            if (command.Elevator != recvValue)
            {
                return false;
            }

            return true;
        }
    }
}
