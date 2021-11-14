using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial
{
    public enum BaudRate : int
    {
        BaudRate75 = 75,
        BaudRate110 = 110,
        BaudRate134 = 134,
        BaudRate150 = 150,
        BaudRate300 = 300,
        BaudRate600 = 600,
        BaudRate1200 = 1200,
        BaudRate1800 = 1800,
        BaudRate2400 = 2400,
        BaudRate4800 = 4800,
        BaudRate7200 = 7200,
        BaudRate9600 = 9600,
        BaudRate14400 = 14400,
        BaudRate19200 = 19200,
        BaudRate38400 = 38400,
        BaudRate57600 = 57600,
        BaudRate115200 = 115200,
        BaudRate128000 = 128000
    }

    public enum DataBits : int
    {
        DataBits4 = 4,
        DataBits5 = 5,
        DataBits6 = 6,
        DataBits7 = 7,
        DataBits8 = 8
    }

}
