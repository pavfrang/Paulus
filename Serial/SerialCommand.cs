using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Paulus.Serial
{
    public abstract class SerialCommandBase
    {


        public abstract bool IsError { get; }
        public abstract Exception Exception { get; }

        //protected async Task setParentLastCommand(SerialDevice device)
        //      {
        //   await device.ReserveByCommand(this);
        //    }

        public bool Success { get; protected set; }

        public DateTime StartTime { get; protected set; }

        private DateTime endTime;
        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                //Debug.WriteLine(Lag.TotalMilliseconds);
            }
        }

        public TimeSpan Lag { get { return EndTime - StartTime; } }

    }
    public class SimpleSerialCommand : SerialCommandBase
    {
        public SimpleSerialCommand(string commandText)
        {
            CommandText = commandText;
        }

        public string CommandText { get; }
        public SerialMessage SerialMessage { get; internal set; }

        //shortcut properties
        public override bool IsError { get { return SerialMessage.IsError; } }
        public override Exception Exception { get { return SerialMessage?.Exception; } }



        protected async Task<bool> sendAndUpdateReply(SerialDevice device)
        {
            await device.ReserveByCommand(this);
            //  setParentLastCommand(device);


            StartTime = DateTime.Now;
            Debug.WriteLine($"[SENDING] {CommandText} @ {DateTime.Now:HH:mm:ss.ff}");

            SerialMessage = await device.SendMessageAndReadResponse(CommandText);

            device.ReleaseReserve();

            EndTime = DateTime.Now;
            if (!SerialMessage.IsError)
                Debug.WriteLine($"[OK]\t{SerialMessage.MessageSent}\t({Lag.TotalMilliseconds} ms)");
            else
                Debug.WriteLine($"[Error]\t{SerialMessage.MessageSent} ({SerialMessage.Exception.Message})\t({Lag.TotalMilliseconds} ms)");


            return Success = !SerialMessage.IsError;
        }

        public async Task<bool> SendAndGetReply(SerialDevice device)
        {
            await sendAndUpdateReply(device);
            return Success;
        }

        public async Task<SimpleSerialCommand> SendAndGetCommand(SerialDevice device)
        {
            await sendAndUpdateReply(device);
            return this;
        }
    }

    public abstract class SerialCommandWithResponse<U> : SerialCommandBase
    {
        public U Reply { get; internal set; }

        public async Task<U> SendAndGetReply(SerialDevice device)
        {
            await sendAndUpdateReply(device);
            return Reply;
        }

        //each override should reserve/release the device
        protected abstract Task sendAndUpdateReply(SerialDevice device);
    }


    public class SimpleSerialCommandWithResponse<T> : SerialCommandWithResponse<T>
    {
        #region Constructors
        protected SimpleSerialCommandWithResponse(string command, string answerPattern)
        {
            CommandText = command;
            AnswerPattern = answerPattern;
            regex = new Regex(answerPattern, RegexOptions.Compiled);
        }

        public SimpleSerialCommandWithResponse(string command, string answerPattern,
            Func<string, T> processAnswerString) : this(command, answerPattern)
        {
            ProcessAnswerString = processAnswerString;
        }

        public SimpleSerialCommandWithResponse(string command, string answerPattern,
         Func<Match, T> processAnswerMatch) : this(command, answerPattern)
        {
            ProcessAnswerMatch = processAnswerMatch;
        }

        #endregion


        protected Regex regex;

        public string CommandText { get; }

        public string AnswerPattern { get; }
        public Match Match { get; protected set; }
        public SerialMessage SerialMessage { get; internal set; }

        //shortcut properties
        public override bool IsError { get { return SerialMessage.IsError; } }

        public override  Exception Exception { get { return SerialMessage?.Exception; } }

        protected Func<string, T> ProcessAnswerString { get; }

        protected Func<Match, T> ProcessAnswerMatch { get; }

        /// <summary>
        /// Sends the command asynchronously and returns a Command object with full debugging information.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<SimpleSerialCommandWithResponse<T>> SendAndGetCommand(SerialDevice device)
        {
            await sendAndUpdateReply(device);
            return this;
        }

        protected override async Task sendAndUpdateReply(SerialDevice device)
        {
            //lock the device and THEN continue THIS IS CRUCIAL TO AVOID locks
            await device.ReserveByCommand(this);

            StartTime = DateTime.Now;
            Debug.WriteLine($"[SENDING] {CommandText} @ {DateTime.Now:HH:mm:ss.ff}");

            try
            {
                SerialMessage = await device.SendMessageAndReadResponse(CommandText);
            }
            finally //it is crucial to release the device or no command may be processed
            {
                device.ReleaseReserve();
            }

            //Debug.WriteLine($"[RECEIVED RESPONSE] {Command} @ {DateTime.Now:HH:mm:ss.ff}");

            if (SerialMessage.IsError) { Reply = default(T); Success = false; return; }

            //if(SerialMessage.ReceivedFilteredMessage==null) { Reply = default(T); Success = false; return; }
            if (SerialMessage.ReceivedFilteredMessage == null) { Reply = default(T); Success = false; return; }


            Match = regex.Match(SerialMessage.ReceivedFilteredMessage);
            if (!Match.Success) { Reply = default(T); Success = false; return; }

            //if (SerialMessage.MessageSent == "CONC ALL ACTUAL ?")
            //    if (SerialMessage.ReceivedFilteredMessage.ToCharArray().Count(c => c == ',') != 5)
            //    {

            //        Debugger.Break();
            //    }
            //    else
            //        Debug.WriteLine(SerialMessage.ReceivedFilteredMessage);


            //if we are here then the reply is ok
            if (ProcessAnswerString != null)
                Reply = ProcessAnswerString(Match.Value);
            else if (ProcessAnswerMatch != null)
                Reply = ProcessAnswerMatch(Match);
            else
                Reply = default(T);

            EndTime = DateTime.Now;

            if (!SerialMessage.IsError)
                Debug.WriteLine($"[OK]\t{SerialMessage.MessageSent}\t({Lag.TotalMilliseconds} ms)");
            else
                Debug.WriteLine($"[Error]\t{SerialMessage.MessageSent} ({SerialMessage.Exception.Message})\t({Lag.TotalMilliseconds} ms)");


            Success = true;
        }
    }

    public class SerialDoubleCommandWithResponse<T1, T2, U> : SerialCommandWithResponse<U>
    {
        public SerialDoubleCommandWithResponse(SimpleSerialCommandWithResponse<T1> command1, SimpleSerialCommandWithResponse<T2> command2,
            Func<T1, T2, U> processCombinedAnswersFunction)
        {
            Command1 = command1;
            Command2 = command2;
            ProcessCombinedAnswers = processCombinedAnswersFunction;
        }

        public SimpleSerialCommandWithResponse<T1> Command1 { get; }

        public SimpleSerialCommandWithResponse<T2> Command2 { get; }


        public override bool IsError { get { return Command1.SerialMessage.IsError || Command2.SerialMessage.IsError; } }

        public override Exception Exception { get { return Command1.SerialMessage.Exception ?? Command2.SerialMessage.Exception; } }

        protected Func<T1, T2, U> ProcessCombinedAnswers { get; }
        public async Task<SerialDoubleCommandWithResponse<T1, T2, U>> SendAndGetCommand(SerialDevice device)
        {
            await sendAndUpdateReply(device);
            return this;
        }

        protected override async Task sendAndUpdateReply(SerialDevice device)
        {
            T1 t1 = await Command1.SendAndGetReply(device);
            if (t1 == null) { Reply = default(U); return; }

            T2 t2 = await Command2.SendAndGetReply(device);
            if (t2 == null) { Reply = default(U); return; }

            Reply = ProcessCombinedAnswers(t1, t2);
        }
    }

    public static class SerialCommands
    {
        public static SimpleSerialCommandWithResponse<float?> CreateWithFloatResponse(string command) =>
         new SimpleSerialCommandWithResponse<float?>(command, FloatPattern, getFloat);

        public static SimpleSerialCommandWithResponse<string> CreateWithStringResponse(string command) =>
         new SimpleSerialCommandWithResponse<string>(command, StringPattern, s => s);


        public static SimpleSerialCommandWithResponse<List<float>> CreateWithListFloatResponse(string command) =>
         new SimpleSerialCommandWithResponse<List<float>>(command, FloatListPattern, getFloats);
        public static SimpleSerialCommandWithResponse<int?> CreateWithIntResponse(string command) =>
         new SimpleSerialCommandWithResponse<int?>(command, IntPattern, getInt);
        public static SimpleSerialCommandWithResponse<List<int>> CreateWithListIntResponse(string command) =>
         new SimpleSerialCommandWithResponse<List<int>>(command, IntListPattern, getInts);

        #region Common Regex patterns and value retrieval functions
        public const string StringPattern = @"[^\n]+";
        public const string FloatPattern = @"[0-9]+\.[0-9]*";
        public const string FloatListPattern = "(" + FloatPattern + ",)*" + FloatPattern;   //@"([0-9]+\.[0-9]?,)+[0-9]+\.[0-9]?";
        public const string IntPattern = @"\d+";
        public const string IntListPattern = "(" + IntPattern + ",)*" + IntPattern;  //"(\\d{1,2},)+\\d{1,2}";

        static List<int> getInts(string response)
        {
            return (from string s in response.Split(',') select int.Parse(s)).ToList();
            //MatchCollection mc = Regex.Matches(output, "\\d{1,2}");
            //return (from Match m in mc select int.Parse(m.Value)).ToList();
        }

        static int? getInt(string response)
        {
            return int.Parse(response);
        }
        static List<float> getFloats(string response)
        {
            return (from string s in response.Split(',') select float.Parse(s)).ToList();
            //MatchCollection mc = Regex.Matches(response, floatPattern);
            //return (from Match m in mc select float.Parse(m.Value)).ToList();
        }


        static float? getFloat(string response)
        {
            return float.Parse(response);
        }
        #endregion
    }




}
