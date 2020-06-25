using Microsoft.AspNetCore.Mvc;
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
        //Values for http requests.

        static private string AILERON_PATH = "/controls/flight/aileron";
        static private string THROTTLE_PATH = "/controls/engines/current-engine/throttle";
        static private string ELEVATOR_PATH = "/controls/flight/elevator";
        static private string RUDDER_PATH = "/controls/flight/rudder";

        public FlightGearClient()
        {
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
                //Connection phase.
                // TODO set ip and port according to appsettings
                _client.Connect("127.0.0.1", 5403);
                // TODO check if need to change timeout value
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

        //Strart processing commands.
        public void Start()
        {
            Task.Factory.StartNew(ProcessCommands);
        }


        //This function iterates given commands, sends each one and checks if received.
        public void ProcessCommands()
        {
            /*NetworkStream stream = null;
            try
            {
                // TODO set ip and port according to appsettings
                _client.Connect("127.0.0.1", 5403);
                // TODO check if need to change timeout value
                _client.ReceiveTimeout = 15000;
                stream = _client.GetStream();
                byte[] start = System.Text.Encoding.ASCII.GetBytes("data\n");
                stream.Write(start, 0, start.Length);
            } catch
            // catch is left empty to allow program to continue so that the exception will eventually be thrown to controller
            {

            }*/
            //Iterate commands from queue, and send them.
            foreach (AsyncCommand command in _queue.GetConsumingEnumerable())
            {
                try
                {
                    Result res;
                    sendData(stream, command.Command);
                    //Check if message received.
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

        //Sending data with given command.
        private void sendData(NetworkStream stream, Command command)
        {
            var aileronVal = command.Aileron;
            var rudderVal = command.Rudder;
            var throttleVal = command.Throttle;
            var elevatorVal = command.Elevator;
            //Creating strings to send.
            string toSend = "set " + AILERON_PATH + " " + aileronVal.ToString() + " \r\n";
            toSend += "set " + RUDDER_PATH + " " + rudderVal.ToString() + " \r\n";
            toSend += "set " + THROTTLE_PATH + " " + throttleVal.ToString() + " \r\n";
            toSend += "set " + ELEVATOR_PATH + " " + elevatorVal.ToString() + " \r\n";
            byte[] sendBuffer = System.Text.Encoding.ASCII.GetBytes(toSend);
            //Sending phase.
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


    //Check if requests were received
    private bool checkRecieved(NetworkStream stream, Command command)
        {
            byte[] sendBuffer = new byte[1024];
            byte[] recvBuffer = new byte[1024];
            int bytes;
            double recvValue;
            //The getter of aileron.
            sendBuffer = System.Text.Encoding.ASCII.GetBytes("get " + AILERON_PATH + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            bytes = stream.Read(recvBuffer, 0, recvBuffer.Length);
            string retString = System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes);
            recvValue = double.Parse(System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes));
            if (command.Aileron != recvValue)
            {
                return false;
            }
            //The getter of aileron.
            sendBuffer = System.Text.Encoding.ASCII.GetBytes("get " + RUDDER_PATH + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            bytes = stream.Read(recvBuffer, 0, recvBuffer.Length);
            recvValue = double.Parse(System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes));
            if (command.Rudder != recvValue)
            {
                return false;
            }
            //The getter of throttle.
            sendBuffer = System.Text.Encoding.ASCII.GetBytes("get " + THROTTLE_PATH + " \r\n");
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            bytes = stream.Read(recvBuffer, 0, recvBuffer.Length);
            recvValue = double.Parse(System.Text.Encoding.ASCII.GetString(recvBuffer, 0, bytes));
            if (command.Throttle != recvValue)
            {
                return false;
            }
            //The getter of elevator.
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
