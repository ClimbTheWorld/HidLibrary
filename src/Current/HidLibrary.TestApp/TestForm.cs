using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HidLibrary.TestApp
{
    [Flags]
    public enum G13Keys : ulong
    {
        // G keys
        G1 = 0x0000000001,
        G2 = 0x0000000002,
        G3 = 0x0000000004,
        G4 = 0x0000000008,
        G5 = 0x0000000010,
        G6 = 0x0000000020,
        G7 = 0x0000000040,
        G8 = 0x0000000080,
        G9 = 0x0000000100,
        G10 = 0x0000000200,
        G11 = 0x0000000400,
        G12 = 0x0000000800,
        G13 = 0x0000001000,
        G14 = 0x0000002000,
        G15 = 0x0000004000,
        G16 = 0x0000008000,
        G17 = 0x0000010000,
        G18 = 0x0000020000,
        G19 = 0x0000040000,
        G20 = 0x0000080000,
        G21 = 0x0000100000,
        G22 = 0x0000200000,

        // Joystick buttons
        J1 = 0x0200000000, // left
        J2 = 0x0400000000, // bottom
        J3 = 0x0800000000, // middle (i.e., on the stick)

        // M keys
        M1 = 0x0020000000,
        M2 = 0x0040000000,
        M3 = 0x0080000000,
        M4 = 0x0100000000,

        // LCD keys
        L0 = 0x0001000000, // LCD Select Applet
        L1 = 0x0002000000, 
        L2 = 0x0004000000,
        L3 = 0x0008000000,
        L4 = 0x0010000000,
        L5 = 0x6000000000, // LCD Light
    }

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

        delegate void UpdateInfoDelegate(string value);

        [StructLayout(LayoutKind.Explicit)]
        struct KeyState
        {
            [FieldOffset(0)] public byte B0;
            [FieldOffset(1)] public byte B1;
            [FieldOffset(2)] public byte B2;
            [FieldOffset(3)] public byte B3;
            [FieldOffset(4)] public byte B4;

            [FieldOffset(0)] public ulong UL;
        }

        KeyState currentState = new KeyState();

        void ReadG13Data(HidDeviceData data)
        {
            KeyState state = new KeyState();

            if (data.Status == HidDeviceData.ReadStatus.Success)
            {
                var bytes = data.Data;

                var reportId = bytes[0];
                var joystickX = bytes[1] - 0x80;
                var joystickY = bytes[2] - 0x80;

                state.B0 = bytes[3];
                state.B1 = bytes[4];
                state.B2 = bytes[5] < 0x80 ? bytes[5] : (byte)(bytes[5] - 0x80);
                state.B3 = bytes[6];
                state.B4 = bytes[7] < 0x80 ? bytes[7] : (byte)(bytes[7] - 0x80);

                var sb = new StringBuilder();

#if display_verbose_data
                sb.Append("Joystick: ")
                    .Append(joystickX.ToString().PadLeft(4, ' ')).Append(", ")
                    .Append(joystickY.ToString().PadLeft(4, ' ')).Append("; ");
                for (int i = 3; i < bytes.Length; i++)
                    sb
                        .Append(i > 3 ? ", " : "")
                        .Append(bytes[i].ToString("x").PadLeft(2, ' '));

                sb.Append(" : ").Append(state.UL.ToString("x").PadLeft(10, '0'));
#else
                var dif = currentState.UL ^ state.UL;
                sb.Append(((G13Keys)dif).ToString());
                var pressed = (dif & state.UL) > 0;
                sb.Append(" ").Append(pressed);

                if ((G13Keys)dif == G13Keys.G1)
                {
                    if (pressed)
                    {
                        SendInputManager.KeyDown(SendInputManager.ScanCode.a, false, true);
                    }
                    else
                    {
                        SendInputManager.KeyUp(SendInputManager.ScanCode.a);
                    }

                    //SendInputManager.KeyTap(SendInputManager.ScanCode.a);
                }
#endif

                sb.Append(Environment.NewLine);
                currentState = state;

                rtbInfo.AppendText(sb.ToString());
            }
        }
    }
}