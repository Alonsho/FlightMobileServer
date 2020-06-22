using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightMobileWeb
{

    public enum Result { Ok, NotOk }


    public class Command
    {
        public double Aileron { get; set; }

        public double Rudder { get; set; }

        public double Elevator { get; set; }

        public double Throttle { get; set; }
    }

    public class AsyncCommand
    {
        public Command Command { get; private set; }
        public Task<Result> Task { get => Completion.Task; }
        public TaskCompletionSource<Result> Completion { get; private set; }
        public AsyncCommand(Command input)
        {
            Command = input;
            // Watch out! Run Continuations Async is important!
            Completion = new TaskCompletionSource<Result>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

}
