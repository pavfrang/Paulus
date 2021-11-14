using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;
using System.Diagnostics;
using System.Threading;

namespace Paulus.Serial.DownTino
{
    public class DownTinoCommander : DeviceCommander
    {
        /// <summary>
        /// This constructor is needed for debugging purposes (e.g. testing connection). SerialDevice.IsDeviceConnectedAt is using this constructor.
        /// </summary>
        public DownTinoCommander() : base(ResponseMode.Immediate, "")
        {
            DownTino = new DownTino(false);
        }

        /// <summary>
        /// This constructor is useful for testing purposes. The settings and the device manager are not typically needed in this case.
        /// </summary>
        /// <param name="portName"></param>
        public DownTinoCommander(string portName, string name = "") : base(portName, ResponseMode.Immediate, name)
        {
            DownTino = new DownTino(false);
        }

        /// <summary>
        /// This constructor is useful for testing purposes. The settings and the device manager are not typically needed in this case.
        /// </summary>
        /// <param name="port"></param>
        public DownTinoCommander(SerialPort port, string name = "") : base(port, ResponseMode.Immediate, name)
        {
            DownTino = new DownTino(false);

        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async override Task<bool> ReadDeviceInformation()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //
            return true;
        }


        public DownTino DownTino { get; }

        public override Bitmap Image
        {
            get
            {
                return DeviceImages.Arduino;
            }
        }

        #region Send command

        //public async Task<SimpleSerialCommandWithResponse<List<float>>>
        //    Read2(Action actionOnSuccess)
        //{
        //    return await SendGetCommand<List<float>>(
        //        SerialCommands.CreateWithListFloatResponse("READ"),
        //        (response) =>
        //        {


        //        });
        //}

        private object syncObject = new object();
        public async Task<bool> Read()
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            try
            {
                if (!Monitor.TryEnter(this.syncObject, 100))
                    throw new Exception("LOCK!");

                LastCommandSent = "READ";
                serialPort.Write("READ\n");
                string reply = await Task.Run(() => serialPort.ReadTo("\r\n"));
                Responses.Enqueue(reply);

                serialPort.ReadExisting(); //clear any buffers
                string[] tokens = reply?.Split(',');
                if (reply.Length > 0 && tokens.Length == 6)
                {
                    float value; bool parsed;

                    parsed = float.TryParse(tokens[0], out value);
                    //convert kPa to mbar
                    DownTino.RelativePressure1.ActualValue = parsed ? value * 10.0f : Signal.InvalidValue;
                    parsed = float.TryParse(tokens[1], out value);
                    DownTino.RelativePressure2.ActualValue = parsed ? value * 10.0f : Signal.InvalidValue;

                    //convert psi to mbar
                    parsed = float.TryParse(tokens[2], out value);
                    DownTino.DifferentialPressure1.ActualValue = parsed ? value * 68.9476f : Signal.InvalidValue;
                    parsed = float.TryParse(tokens[3], out value);
                    DownTino.DifferentialPressure2.ActualValue = parsed ? value * 68.9476f : Signal.InvalidValue;

                    parsed = float.TryParse(tokens[4], out value);
                    DownTino.Temperature1.ActualValue = parsed ? value : Signal.InvalidValue;
                    parsed = float.TryParse(tokens[5], out value);
                    DownTino.Temperature2.ActualValue = parsed ? value : Signal.InvalidValue;
                }

                Monitor.Exit(syncObject);
                return true;
            }
            //catch (SemaphoreFullException)
            //{
            //    Debug.WriteLine("TIMEOUT!");
            //    return false;
            //}
            catch (Exception exception)
            {
                if (exception.Message != "LOCK!")
                    Monitor.Exit(syncObject);

                Debug.WriteLine($"{exception.GetType().Name}");
                Debug.WriteLine($"OTHER EXCEPTION! {exception.Message}");
                return false;
            }
            finally
            {

                w.Stop(); Debug.WriteLine(w.ElapsedMilliseconds);
            }
        }

        public enum PressureSensor
        { DP1, DP2, RP1, RP2 }

        public async Task<bool> Calibrate(PressureSensor sensor)
        {
            string sSensor = sensor.ToString();
            string command = $"CAL{sensor}";
            try
            {
                if (!Monitor.TryEnter(this.syncObject, 100))
                    throw new Exception("LOCK!");
                LastCommandSent = command;
                serialPort.Write($"{command}\n");
                string reply = await Task.Run(() => serialPort.ReadTo("Done\r\n"));
                Responses.Enqueue(reply);


                serialPort.ReadExisting(); //clear any buffers
                Monitor.Exit(syncObject);
                return true;
            }
            catch (Exception exception)
            {
                if (exception.Message != "LOCK!")
                    Monitor.Exit(syncObject);

                Debug.WriteLine($"{exception.GetType().Name}");
                Debug.WriteLine($"OTHER EXCEPTION! {exception.Message}");
                return false;
            }
        }

        //public override Task<bool> ReadDeviceInformation()
        //{
        //    //throw new NotImplementedException();
        //}

        public string LastCommandSent;
        public Queue<string> Responses = new Queue<string>();


        public async Task<bool> Reset2()
        {
            try
            {
                if (!Monitor.TryEnter(this.syncObject, 100))
                    throw new Exception("LOCK!");
                LastCommandSent = "RESET";
                serialPort.WriteLine("RESET");
                //string reply = await Task.Run(() => serialPort.ReadTo("\r\n"));
                string reply = await Task.Run(() => serialPort.ReadTo("Welcome"));
                reply = await Task.Run(() => serialPort.ReadTo("\r\n"));
                serialPort.DiscardInBuffer();

                Responses.Enqueue(reply);
                serialPort.ReadExisting(); //clear any buffers
                if (reply.Length > 0)
                {
                    DownTino.RelativePressure1.ActualValue = 0.0f;
                    DownTino.RelativePressure2.ActualValue = 0.0f;

                    DownTino.DifferentialPressure1.ActualValue = 0.0f;
                    DownTino.DifferentialPressure2.ActualValue = 0.0f;

                    DownTino.Temperature1.ActualValue = 0.0f;
                    DownTino.Temperature2.ActualValue = 0.0f;

                }
                Monitor.Exit(syncObject);
                return true;
            }
            catch (Exception exception)
            {
                if (exception.Message != "LOCK!")
                    Monitor.Exit(syncObject);

                Debug.WriteLine($"{exception.GetType().Name}");
                Debug.WriteLine($"OTHER EXCEPTION! {exception.Message}");
                return false;
            }
        }

        #endregion
    }
}
