using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HidLibrary.TestApp
{
    public partial class TestForm : Form
    {
        const int Logitech = 0x046d;
        const int G13 = 0xc21c;

        public TestForm()
        {
            InitializeComponent();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            DeviceTest();
        }

        void DeviceTest()
        {
            var device = HidDevices.GetDevice(Logitech, G13);
            device.Inserted += d => Debug.WriteLine("Inserted");
            device.Removed += d => Debug.WriteLine("Removed");
            device.DataRead += (dev, data) => ReadG13Data(data); // Debug.WriteLine(data.ToString());
            device.MonitorDeviceEvents = true;
            device.StartReading();
        }

        void device_DataRead(HidDevice device, HidDeviceData data)
        {
            Debug.WriteLine(data.ToString());
        }

        bool[] keys = new bool[0x24];

        void ReadG13Data(HidDeviceData data)
        {
            if (data.Status == HidDeviceData.ReadStatus.Success)
            {
                var bytes = data.Data;

                var reportId = bytes[0];
                var joystickX = bytes[1] - 0x80;
                var joystickY = bytes[2] - 0x80;

                /*
                int i = 0;
                int d;
                for (int b = 3; b <= 6; b++)
                {
                    d = bytes[b];
                    for (int k = 1; k <= 0x80; k *= 2)
                    {
                        if (i > (keys.Length - 1))
                            break;

                        var key = keys[i];
                        if ((d & k) != 0)
                            keys[i] = true;
                    }
                }
                */

                var sb = new StringBuilder();
                sb.Append("Joystick: ").Append(joystickX).Append(", ").Append(joystickY).Append("; ");
                for (int i = 3; i < bytes.Length; i++)
                    sb
                        .Append(i > 3 ? ", " : "")
                        .Append(bytes[i].ToString("x").PadLeft(2, ' '));

                Debug.WriteLine(sb.ToString());
            }
        }
    }
}
