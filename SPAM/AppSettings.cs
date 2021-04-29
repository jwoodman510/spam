using System.Collections.Generic;

namespace SPAM
{
    public class AppSettings
    {
        public int BatchSize { get; set; }

        public int BatchThrottleSeconds { get; set; }

        public IEnumerable<Request> Requests { get; set; }

        public bool Validate()
        {
            if (BatchSize <= 1)
            {
                Consoler.WriteError($"Invalid Batch Size: {BatchSize}.");

                return false;
            }

            if (BatchThrottleSeconds <= 0)
            {
                Consoler.WriteError($"Invalid Batch Throttle: {BatchThrottleSeconds}.");

                return false;
            }

            if (Requests == null)
            {
                Consoler.WriteError("Invalid Requests Configuration.");

                return false;
            }

            return true;
        }
    }
}
