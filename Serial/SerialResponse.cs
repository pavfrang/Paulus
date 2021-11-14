using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial
{
    public class SerialMessage
    {
        /// <summary>
        /// Message sent without the prefix, suffix.
        /// </summary>
        public string MessageSent;
        /// <summary>
        /// Received message without the response suffix.
        /// </summary>
        public string ReceivedFullMessage;

        /// <summary>
        /// Received message without special characters such as character escapes.
        /// </summary>
        public string ReceivedFilteredMessage;

        /// <summary>
        /// Received message #2 without special characters such as character escapes.
        /// </summary>
        public string ReceivedFilteredMessage2;

        /// <summary>
        /// On error response, the flag must be set to true. An error message could be received, even if no Exception is thrown.
        /// </summary>
        public bool IsError;

        /// <summary>
        /// If an exception is thrown then it is stored here.
        /// </summary>
        public System.Exception Exception;
    }
}
