using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Bosch
{
    public class BoschCommander : SerialDevice
    {
        public BoschCommander(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : base(portName,ResponseMode.Monitor, "",baudRate, parity, dataBits, stopBits)
        {
        }

        public BoschCommander(SerialPort port) : base(port,ResponseMode.Monitor)
        {
        }

        protected override void setMessagePrefixSuffix()
        {
            ReceiveMessageSuffix = "\r\n";
            SendMessageSuffix = "\n";
        }

        #region Send Commands
        //All commands return true if the command is successfully sent
        public bool StartRegen1()
        {
            return SendMessage("START REGEN 1");
        }

        public bool StartRegen2()
        {
            return SendMessage("START REGEN 2");
        }

        public bool StartMeasure1()
        {
            return SendMessage("START MEASURE 1");
        }

        public bool StartMeasure2()
        {
            return SendMessage("START MEASURE 2");
        }

        public bool StopRegen1()
        {
            return SendMessage("STOP REGEN 1");
        }

        public bool StopRegen2()
        {
            return SendMessage("STOP REGEN 2");
        }

        public bool StopMeasure1()
        {
            return SendMessage("STOP MEASURE 1");
        }

        public bool StopMeasure2()
        {
            return SendMessage("STOP MEASURE 2");
        }

        public override bool Reset()
        {
            return SendMessage("RESET");
        }


        #endregion

        private BoschSensor _sensor1;
        public BoschSensor Sensor1
        {
            set
            {
                if (_sensor1 == value)
                    return;


                _sensor1 = value;
                if (_sensor1 != null)
                {
                    _sensor1.StatusChanged += Sensor_StatusChanged;
                    _sensor1.StatusChanging += Sensor_StatusChanging;
                    //_sensor1.DurationUntilNextStepChanged += Sensor1_DurationUntilNextStepChanged;
                }
            }
            get
            {
                return _sensor1;
            }
        }

         private BoschSensor _sensor2;
        public BoschSensor Sensor2
        {
            set
            {
                if (_sensor2 == value)
                    return;


                _sensor2 = value;
                if (_sensor2 != null)
                {
                    _sensor2.StatusChanged += Sensor_StatusChanged;
                    _sensor2.StatusChanging += Sensor_StatusChanging;
                    //_sensor2.DurationUntilNextStepChanged += Sensor1_DurationUntilNextStepChanged;
                }
            }
            get
            {
                return _sensor2;
            }
        }

        private void Sensor_StatusChanging(object sender, EventArgs e)
        {
            if (sender == _sensor1)
                switch (_sensor1.CurrentStatus)
                {
                    case SensorStatus.Measuring:
                        StopMeasure1();
                        break;
                    case SensorStatus.Regenerating:
                        StopRegen1();
                        break;
                }
            else if (sender == _sensor2)
                switch (_sensor2.CurrentStatus)
                {
                    case SensorStatus.Measuring:
                        StopMeasure2();
                        break;
                    case SensorStatus.Regenerating:
                        StopRegen2();
                        break;
                }

        }

        private void Sensor_StatusChanged(object sender, EventArgs e)
        {
            if (sender == _sensor1)

                switch (_sensor1.CurrentStatus)
                {
                    case SensorStatus.Measuring:
                        StartMeasure1();
                        break;
                    case SensorStatus.Regenerating:
                        StartRegen1();
                        break;
                }
            else if (sender == _sensor2)
                switch (_sensor2.CurrentStatus)
                {
                    case SensorStatus.Measuring:
                        StartMeasure2();
                        break;
                    case SensorStatus.Regenerating:
                        StartRegen2();
                        break;
                }

        }


    }
}
