
using System.Configuration; //ApplicationSettingsBase
using System.Diagnostics; //DebuggerNonUserCode
using System.IO.Ports;

namespace Paulus.Serial
{
    //In order to use the following settings the App.config file must contain the following xml fragment (the userSettings is optional and is used the first time that it is used):
    //<?xml version="1.0" encoding="utf-8"?>
    //<configuration>
    //    <configSections>
    //        <sectionGroup name="userSettings" type="System.Configuration.UserSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" >
    //            <section name="Paulus.IO.Ports.PortSettings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" allowExeDefinition="MachineToLocalUser" requirePermission="false" />
    //        </sectionGroup>
    //    </configSections>
    //    <startup> 
    //        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
    //    </startup>
    //    <userSettings>
    //        <Paulus.IO.Ports.PortSettings>
    //            <setting name="LoadLastPortConfiguration" serializeAs="String">
    //                <value>True</value>
    //            </setting>
    //            <setting name="MessageMask" serializeAs="String">
    //                <value>%m\r\n</value>
    //            </setting>
    //            <setting name="PortName" serializeAs="String">
    //                <value>COM10</value>
    //            </setting>
    //            <setting name="BaudRate" serializeAs="String">
    //                <value>9600</value>
    //            </setting>
    //            <setting name="Parity" serializeAs="String">
    //                <value>None</value>
    //            </setting>
    //            <setting name="StopBits" serializeAs="String">
    //                <value>One</value>
    //            </setting>
    //            <setting name="DataBits" serializeAs="String">
    //                <value>7</value>
    //            </setting>
    //        </Paulus.IO.Ports.PortSettings>
    //    </userSettings>
    //</configuration>
    //
    //IN GENERAL SEE THE FOLLOWING FROM THE FORUM (userSettings/applicationSettings may be interchanged according to its use)
    //http://stackoverflow.com/questions/6436157/configuration-system-failed-to-initialize
    //<?xml version="1.0"?>
    //<configuration>
    //   <configSections>
    //      <sectionGroup name="applicationSettings" 
    //                    type="System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" >
    //           <section name="YourProjectName.Properties.Settings" 
    //                    type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" 
    //                    requirePermission="false" />
    //    </sectionGroup>
    //</configSections>
    //NOTE THAT THE CONFIGURATION FILES ARE USED INSIDE c:\Users\username\AppData\Local\company\appname and c:\Users\username\AppData\Roaming\company\appname
    public class SerialPortSettings : ApplicationSettingsBase
    {

        private static SerialPortSettings defaultInstance = ((SerialPortSettings)(Synchronized(new SerialPortSettings())));

        public static SerialPortSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("COM2")]
        public string PortName
        {
            get
            {
                return ((string)(this["PortName"]));
            }
            set
            {
                this["PortName"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("9600")]
        public int BaudRate
        {
            get
            {
                return ((int)(this["BaudRate"]));
            }
            set
            {
                this["BaudRate"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("8")]
        public int DataBits
        {
            get
            {
                return ((int)(this["DataBits"]));
            }
            set
            {
                this["DataBits"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("None")]
        public Parity Parity
        {
            get
            {
                return ((Parity)(this["Parity"]));
            }
            set
            {
                this["Parity"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("One")]
        public StopBits StopBits
        {
            get
            {
                return ((StopBits)(this["StopBits"]));
            }
            set
            {
                this["StopBits"] = value;
            }
        }

        //[System.Diagnostics.DebuggerNonUserCode()] //could be used to skip compiler to get inside this code
        [UserScopedSetting()]
        [DefaultSettingValue("True")]
        public bool LoadLastPortConfiguration
        {
            get
            {
                return ((bool)(this["LoadLastPortConfiguration"]));
            }
            set
            {
                this["LoadLastPortConfiguration"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("%m\\n")]
        public string MessageMask
        {
            get
            {
                return ((string)(this["MessageMask"]));
            }
            set
            {
                this["MessageMask"] = value;
            }
        }

        public SerialPort SerialPort
        {
            get
            {
                return new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            }
            set
            {
                SerialPort port = value;
                if (port == null) return;
                PortName = port.PortName;
                BaudRate = port.BaudRate;
                Parity = port.Parity;
                DataBits = port.DataBits;
                StopBits = port.StopBits;
            }
        }
    }
}
