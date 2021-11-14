using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using static Paulus.Common.EnumExtensions;
using Paulus.IO;
using System.Windows.Forms;

namespace Paulus.Serial
{
    public enum ResponseMode
    {
        Undefined,
        Monitor, //receive messages without synchronizing with the send command (Monitor mode uses )
        Immediate //when we immediately wait for the command
    }

    //Virtual methods to override:
    //Connect
    //Reset
    //setNextAvailableName()

    public class SerialDevice : IDisposable
    {
        #region Constructors
        public SerialDevice(string portName, ResponseMode responseMode, string name = "",
            int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) :
            this(new SerialPort(portName, baudRate, parity, dataBits, stopBits), responseMode, name)
        { }

        public SerialDevice(string portName, ResponseMode responseMode, string name = "") :
            this(responseMode, name)
        {
            SerialPort = GetDefaultSerialPortSettings(portName);
        }


        public SerialDevice(SerialPort port, ResponseMode responseMode, string name = "") :
            this(responseMode, name)
        {
            SerialPort = port;
        }

        //the name is defined from WITHIN the xml element
        public SerialDevice(XmlElement xmlPort, ResponseMode responseMode) :
            this(responseMode, "dummy")
        {
            SerialPort = GetPortFromXml(xmlPort);
            //the device value defines the name of the port device 
            // see the getportfromxml description
            Name = xmlPort.Attributes["device"].Value;
        }

        /// <summary>
        /// The response mode defines whether the DataReceived event will be monitored or not. The name property defines the name of the trace source.
        /// </summary>
        /// <param name="responseMode"></param>
        /// <param name="name"></param>
        public SerialDevice(ResponseMode responseMode, string name = "")
        {
            ResponseMode = responseMode;
            Name = name;

            setMessagePrefixSuffix();
        }

        public static SerialPort GetPortFromXml(XmlElement xmlPort)
        {
            //<serialport name="COM13" device="Gas Mixer #1" baudrate="9600" parity="None" databits ="8" stopbits ="One" />
            var a = xmlPort.Attributes;
            string name = a["name"].Value;
            int baudrate = int.Parse(a["baudrate"].Value);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), a["parity"].Value, true);
            int dataBits = int.Parse(a["databits"].Value);
            StopBits stopBits = (StopBits)Enum.Parse(typeof(StopBits), a["stopbits"].Value, true);
            return new SerialPort(name, baudrate, parity, dataBits, stopBits);
        }

        public virtual SerialPort GetDefaultSerialPortSettings(string portName) =>
            new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);


        protected virtual void setMessagePrefixSuffix()
        {
            ReceiveMessageSuffix = "\n";
            SendMessageSuffix = "\n";
            ReceiveMessagePrefix = SendMessagePrefix = "";
        }

        #endregion

        protected CultureInfo en = CultureInfo.InvariantCulture;

        ///// <summary>
        ///// The sync object is needed in order to lock the device for synchronous read/write operations.
        ///// </summary>
        //protected object syncObject = new object();
        protected SemaphoreSlim syncSemaphore = new SemaphoreSlim(1, 1);

        #region Connection

        protected SerialPort serialPort;
        public SerialPort SerialPort
        {
            get { return serialPort; }
            set
            {
                if (serialPort == value) return;

                serialPort = value;
                serialPort.WriteTimeout = 2000;
                serialPort.ReadTimeout = 2000;

                if (responseMode == ResponseMode.Monitor)
                    serialPort.DataReceived += Port_DataReceived;

                OnSerialPortChanged();
            }
        }

        public event EventHandler SerialPortChanged;
        protected virtual void OnSerialPortChanged() =>
            SerialPortChanged?.Invoke(this, EventArgs.Empty);

        public string PortName
        {
            get
            {
                return serialPort.PortName;
            }
        }

        public async Task<bool> Connect(bool reset, bool testCommunication = false)
        {
            try
            {
                IsConnecting = true;
                //if it is  already open then just discard data
                if (serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    serialPort.Close();
                    return true;
                }

                try
                {
                    SerialPortFixer.Fix(serialPort.PortName);
                }
                catch { }
                //if (serialPort == null) MessageBox.Show("SERIAL PORT NULL!");

                serialPort.Open();

                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();

                //the Reset issues a test command
                //if it fails then an Timeout exception will be thrown
                if (reset)
                    Reset();

                if (testCommunication)
                    if (!await TestCommunication())
                    {
                        serialPort.Close();
                        return IsConnecting = false;
                    }

                //update local variable
                isConnected = serialPort.IsOpen;

                IsConnecting = false;
                OnConnected();

                return serialPort.IsOpen;
            }
            catch
            {
                serialPort.Close();
                isConnected = IsConnecting = false;

                return false;
            }
        }

        public event EventHandler Connected;
        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public bool IsConnecting { get; protected set; }

        //the isConnected is updated via the Connect() command.
        private bool isConnected;
        public bool IsConnected
        {
            get
            {
                return serialPort != null && serialPort.IsOpen && isConnected;
            }
        }

        public virtual bool Reset()
        {
            return true;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async virtual Task<bool> TestCommunication()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return true;
        }

        //public async virtual Task QuerySettings() { }

        public bool Disconnect()
        {
            try
            {
                //LastMessage2Received = LastSerialMessage.ReceivedFilteredMessage = LastMessageSent = null;
                LastSerialMessage = null;

                bool wasPortOpen = serialPort.IsOpen;
                if (wasPortOpen) // && !serialPort.IsOpen)
                {
                    OnDisconnecting();
                    serialPort.Close();
                    OnDisconnected();
                }

                isConnected = false;
                return !serialPort.IsOpen;
            }
            catch
            {
                return false;
            }
        }

        public event EventHandler Disconnecting;
        protected void OnDisconnecting() =>
            Disconnecting?.Invoke(this, EventArgs.Empty);

        public event EventHandler Disconnected;
        protected virtual void OnDisconnected() =>
            Disconnected?.Invoke(this, EventArgs.Empty);


        public static async Task<bool> IsDeviceConnectedAt<T>(string portName) where T : SerialDevice, new()
        {
            //if (portName == "COM8" && typeof(T)==typeof(InfusionPump.InfusionPumpCommander)) Debugger.Break();

            //ignore the call if the port does not exist
            if (!SerialPort.GetPortNames().Contains(portName))
                return false;

            T device = new T();

            SerialPort serialPort = device.GetDefaultSerialPortSettings(portName);

            try
            {
                SerialPortFixer.Fix(portName);
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 100;
                serialPort.Open();
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();

                device.serialPort = serialPort;

                bool success = await device.TestCommunication();

                //release the serial port
                serialPort.Close();

                return success;
            }
            catch
            {
                serialPort.Close();
                return false;
            }
        }


        #endregion

        #region Data received

        protected ResponseMode responseMode;
        public ResponseMode ResponseMode
        {
            get { return responseMode; }
            set
            {
                if (responseMode == value)
                    return;

                responseMode = value;

                if (serialPort == null) return;
                //add or remove the handler depending on whether we wait for a response
                if (responseMode == ResponseMode.Monitor)
                    serialPort.DataReceived += Port_DataReceived;
                else //Immediate Mode
                    serialPort.DataReceived -= Port_DataReceived;
            }
        }
        //public string LastMessage2Received { get; internal set; }
        //public string LastSerialMessage.ReceivedFilteredMessage { get; internal set; }
        //public string LastMessageSent { get; private set; }
        //public Exception LastException { get; private set; }

        public SerialCommandBase LastSerialCommandSent { get; private set; }

        private SemaphoreSlim commandSemaphore = new SemaphoreSlim(1, 1);
        public async Task ReserveByCommand(SerialCommandBase serialCommandBase)
        {
            await commandSemaphore.WaitAsync();
            LastSerialCommandSent = serialCommandBase;
        }

        public void ReleaseReserve()
        {
            commandSemaphore.Release();
        }


        public SerialMessage LastSerialMessage { get; protected set; } = new SerialMessage();
        //public Queue<SerialMessage> MessagesReceived { get; private set; } = new Queue<SerialMessage>();

        #region Receive message
        public string ReceiveMessagePrefix { get; set; }
        public string ReceiveMessageSuffix { get; set; }
        /// <summary>
        /// This is needed for double messages. Such as in Fluke: 0/rFluke 289,1.00/r
        /// </summary>
        public string ReceiveMessage2Suffix { get; set; }


        protected int messageReceivedDelay;
        /// <summary>
        /// The simulated time delay in ms.
        /// </summary>
        public int MessageReceivedDelay
        {
            get { return messageReceivedDelay; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(MessageReceivedDelay));
                messageReceivedDelay = value;
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            OnDataReceived(e);
        }

        public event SerialDataReceivedEventHandler DataReceived;
        protected virtual void OnDataReceived(SerialDataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, e);

            if (serialPort != null && !serialPort.IsOpen)
                return;

            receiveMessage();
        }

        /// <summary>
        /// Receives the message and updates the local LastSerialMessage info.
        /// </summary>
        /// <param name="receiveMessageSuffix">Overrides the default ReceiveMessageSuffix.</param>
        /// <returns></returns>
        protected virtual SerialMessage receiveMessage()
        {
            if (responseMode == ResponseMode.Monitor)
                LastSerialMessage = new SerialMessage();
            try
            {
                if (ReceiveMessageSuffix != null)
                {
                    //an IO exception can still be thrown here

                    LastSerialMessage.ReceivedFullMessage = serialPort.ReadTo(ReceiveMessageSuffix);
                    //if (LastSerialMessage.ReceivedFullMessage == null) throw new NullReferenceException("Empty message is received.");

                    if (!string.IsNullOrWhiteSpace(ReceiveMessagePrefix) &&
                        LastSerialMessage.ReceivedFullMessage.StartsWith(ReceiveMessagePrefix))
                        LastSerialMessage.ReceivedFilteredMessage = LastSerialMessage.ReceivedFullMessage.Substring(ReceiveMessagePrefix.Length);
                    else
                        LastSerialMessage.ReceivedFilteredMessage = LastSerialMessage.ReceivedFullMessage;

                    if (!string.IsNullOrEmpty(ReceiveMessage2Suffix))
                        LastSerialMessage.ReceivedFilteredMessage2 = serialPort.ReadTo(ReceiveMessage2Suffix);

                    if (!string.IsNullOrEmpty(ReceiveMessage2Suffix))
                        Debug.WriteLine($"{PortName} [{DateTime.Now:HH:mm:ss.f}]: '{LastSerialMessage.ReceivedFilteredMessage},{LastSerialMessage.ReceivedFilteredMessage2}'");
                    else
                        Debug.WriteLine($"{PortName} [{DateTime.Now:HH:mm:ss.f}]: '{LastSerialMessage.ReceivedFilteredMessage}'");
                        

                    //MessagesReceived.Enqueue(LastSerialMessage);
                    if (messageReceivedDelay > 0)
                        Thread.Sleep(messageReceivedDelay);

                    OnMessageReceived();
                }
                OnDataRead();
            }
            catch (Exception exception)
            {
                if (LastSerialMessage != null)
                {
                    LastSerialMessage.IsError = true;
                    LastSerialMessage.Exception = exception;
                }
                OnErrorReceived();
            }
            return LastSerialMessage;
        }

        public event EventHandler MessageReceived;
        protected virtual void OnMessageReceived()
        {
            MessageReceived?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The event is fired after the Command Received is manipulated.
        /// </summary>
        public event EventHandler DataRead;
        protected virtual void OnDataRead()
        {
            DataRead?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ErrorReceived;
        protected virtual void OnErrorReceived()
        {
            ErrorReceived?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Send message
        public string SendMessageSuffix { get; set; }
        public string SendMessagePrefix { get; set; }

        public virtual bool SendMessage(string message)
        {
            try
            {
                if (messageReceivedDelay > 0)
                    Thread.Sleep(messageReceivedDelay);

                if (responseMode == ResponseMode.Immediate)
                    LastSerialMessage = new SerialMessage();

                serialPort.Write($"{SendMessagePrefix}{message}{SendMessageSuffix}");

                LastSerialMessage.MessageSent = message;

                return true;
            }
            catch (Exception exception)
            {
                LastSerialMessage.Exception = exception;
                LastSerialMessage.IsError = true;

                if (serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                }

                OnSendMessageFailed();
                return LastSerialMessage.IsError;
            }
        }

        //bool isLocked = false;
        public async virtual Task<SerialMessage> SendMessageAndReadResponse(string message)
        {
            return await Task.Run
                (
                //async () =>
                () =>
                {
                    //lock the device for synchronous read/write operations
                    //this will guarantee that the received message will correspond to the current message sent
                    try
                    {
                        //await syncSemaphore.WaitAsync();
                        syncSemaphore.Wait();
                        bool messageSent = SendMessage(message);
                        if (!messageSent) return LastSerialMessage;

                        receiveMessage();
                    }
                    catch (Exception exception)
                    {
                        if (LastSerialMessage == null)
                        {
                            LastSerialMessage = new SerialMessage();
                            LastSerialMessage.MessageSent = message;
                        }

                        LastSerialMessage.IsError = true;
                        LastSerialMessage.Exception = exception;
                    }
                    finally
                    {
                        syncSemaphore.Release();
                    }
                    return LastSerialMessage;
                });
        }

        public event EventHandler SendMessageFailed;
        protected void OnSendMessageFailed()
        {
            SendMessageFailed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #endregion

        #region Trace source and name
        /// <summary>
        /// Trace source is automatically set when the name is set.
        /// </summary>
        public TraceSource TraceSource { get; protected set; }

        protected string name;
        /// <summary>
        /// Name is required in order to internally create the trace source.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (name == value) return;
                name = value;
                if (!string.IsNullOrWhiteSpace(name))
                    TraceSource = new TraceSource(name, SourceLevels.All);
            }
        }

        /// <summary>
        /// The function is typically called by the Dispose method.
        /// </summary>
        public void RemoveAllTraceSourceListeners() => TraceSource?.Listeners.Clear();
        #endregion


        protected static int _devicesCount;
        /// <summary>
        /// Generates the next available name typically using an internal auto-incremented counter.
        /// </summary>
        /// <returns></returns>
        protected virtual string SetNextAvailableName()
        {
            return Name = $"Serial Device #{++_devicesCount}";
        }

        public override string ToString()
        {
            string sConnected = IsConnected ? $"Connected at {PortName}" : "Disconnected";
            return $"{Name} ({sConnected})";
        }



        #region IDisposable Support
        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    serialPort.Close();
                    serialPort.Dispose();

                    TraceSource?.TraceInformation($"Disposed.");
                    RemoveAllTraceSourceListeners();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceSimulator() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
