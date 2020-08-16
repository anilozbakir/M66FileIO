using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace M66FileIO
{
   public class Bluetooth
    {
        SerialPort serialport;
        public bool debug;

        public Bluetooth(SerialPort port)
        {
            this.serialport = port;
        }
        private async Task<string> Send(string v, int timeout)
        {
            int timeoutCount = 0;
            if (serialport != null && serialport.IsOpen)
            {
                this.serialport.Write(v);
                if (debug)
                {
                    Console.WriteLine(v);
                }
                while (true)
                {
                    Thread.Sleep(10);
                    if (serialport.BytesToRead > 0)
                    {//there is a  new packet.

                        Thread.Sleep(10);//wait for 10ms more to finish 
                        var buff = new byte[serialport.BytesToRead];
                        this.serialport.Read(buff, 0, serialport.BytesToRead);
                        if (debug)
                        {
                            Console.WriteLine(Encoding.ASCII.GetString(buff));
                        }
                        return Encoding.ASCII.GetString(buff);
                    }
                    timeoutCount++;
                    if (timeoutCount > (timeout / 10)) return null;
                }




            }
            return null;
        }
        public bool EnableBluetooth()
        {
            return false;
        }
    }
}
