using Paulus.Serial.GasMixer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Paulus.Serial
{
    /// <summary>
    /// This is the base for all agents. This object is 
    /// </summary>
    public abstract class DeviceCommander : SerialDevice
    {
        #region Constructors
        public DeviceCommander(DeviceManager deviceManager, SerialPort port, ResponseMode responseMode, string name) :
            this(port, responseMode, name)
        {
            DeviceManager = deviceManager;
        }
        public DeviceCommander(DeviceManager deviceManager, ResponseMode responseMode, string name) :
            this(responseMode, name)
        {
            DeviceManager = deviceManager;
        }

        public DeviceCommander(SerialPort port, ResponseMode responseMode, string name) :
            base(port, responseMode, name)
        { }
        public DeviceCommander(string portName, ResponseMode responseMode, string name) :
            base(portName, responseMode, name)
        { }

        public DeviceCommander(ResponseMode responseMode, string name) :
            base(responseMode, name)
        { }
        #endregion

        public DeviceManager DeviceManager { get; set; }

        #region Send commands

        public abstract Task<bool> ReadDeviceInformation();

        private static Action doNothing = () => { };
        public async Task<SimpleSerialCommand> SendSetCommand(SimpleSerialCommand command) =>
            await SendSetCommand(command, doNothing);

        public async Task<SimpleSerialCommand> SendSetCommand(string commandText) =>
            await SendSetCommand(new SimpleSerialCommand(commandText), doNothing);

        public async Task<SimpleSerialCommand> SendSetCommand(SimpleSerialCommand command, Action actionOnSuccess)
        {
            SimpleSerialCommand response = await command.SendAndGetCommand(this);
            if (response.IsError)
            {
                string errorMessage = GetErrorMessage(response.Exception);

                TraceSource.TraceEvent(TraceEventType.Critical, 0, $"'{command.CommandText}' command unsuccessful. {errorMessage}.");
            }
            else
            {
                actionOnSuccess();
                TraceSource.TraceInformation($"'{command.CommandText}' command successful.");
            }

            return response;
        }

        private static string GetErrorMessage(System.Exception responseException)
        {
            string errorMessage;

            if (responseException is GasMixerException)
            {
                if ((responseException as GasMixerException).InnerException == null)
                    errorMessage = (responseException as GasMixerException).LongDescription;
                else
                    errorMessage = responseException.InnerException.Message;
                //trim periods at the end of the message
                errorMessage = errorMessage.TrimEnd('.');
            }
            else
                errorMessage = responseException?.Message ?? "";

            return errorMessage;
        }

        public async Task<SimpleSerialCommandWithResponse<T>> SendGetCommand<T>(SimpleSerialCommandWithResponse<T> command,
            Action<SimpleSerialCommandWithResponse<T>> actionOnSuccess)
        {
            SimpleSerialCommandWithResponse<T> response = await command.SendAndGetCommand(this);
            if (!response.Success)
            {
                string errorMessage = GetErrorMessage(response.Exception);

                if (TraceSource != null)
                    TraceSource.TraceEvent(TraceEventType.Critical, 0, $"'{command.CommandText}' command unsuccessful. {errorMessage}.");
            }
            else
            {
                actionOnSuccess(response);
                //string sResponse = (command.Reply is List<float>) ?
                //    string.Join(", ",(command.Reply as List<float>).Select(f => $"{f:0.000}")) :"";
                //TraceSource.TraceInformation($"'{command.CommandText}' command successful. {sResponse}");
                if (TraceSource != null)
                    TraceSource.TraceInformation($"'{command.CommandText}' command successful.");
            }
            return response;
        }

        /// <summary>
        /// Fast connection is possible when we "trust" the last device settings.
        /// The last device settings may come from either a file or the last state of the object (before disconnection).
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> FastConnect()
        {
            return
                await Connect(false, false);
        }
        #endregion

        #region Settings

        ///// <summary>
        ///// These are the initial edit settings, when they are loaded from a file. They are typically needed in order to compare for save.
        ///// </summary>
        //public DeviceSettingsBase InitialSettings { get; protected set; }

        /// <summary>
        /// Settings are the unverified device settings before sending the commands to the device.
        /// </summary>
        public DeviceSettingsBase EditSettings { get; protected set; }

        /// <summary>
        /// Runtime settings are the applied settings after successfully sending the commands to the device.
        /// </summary>
        public DeviceSettingsBase RuntimeSettings { get; protected set; }

        #endregion

        public abstract Bitmap Image { get; }

        /// <summary>
        /// A unique identifier is practical for DevExpress dockpanels (in order to be able to use saveable layouts).
        /// </summary>
        public Guid Guid { get; set; }


        #region Recorder 
        public bool IncludeInRecorder { get; set; } = true;

        //should be abstract
        public virtual List<Variable> GetVariables() { return null; }
        //should be abstract
        public virtual object[] GetVariableValues() { return null; }
        #endregion
    }
}
