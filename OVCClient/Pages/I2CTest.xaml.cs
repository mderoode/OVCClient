using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;


namespace OVCClient.Pages
{
    public sealed partial class I2CTest : Page
    {
        private const int arduino_pin = 18;
        private GpioPin arduino;

        private I2cDevice Device;
        private Timer periodicTimer;
        private Boolean color;

        public I2CTest()
        {
            this.InitializeComponent();
            initGPIO();
            I2CInit();

            button_off.Click += Button_high_Click;
            button_on.Click += Button_low_Click;

            meter_status.Minimum = 0;
            meter_status.Maximum = 1024; //Range is from 0 to 1024
        }

        private void Button_high_Click(object sender, RoutedEventArgs e)
        {
            arduino.Write(GpioPinValue.High);
            text_output.Text = "GPIO pin set to High" + Environment.NewLine + text_output.Text;
        }

        private void Button_low_Click(object sender, RoutedEventArgs e)
        {
            arduino.Write(GpioPinValue.Low);
            text_output.Text = "GPIO pin set to Low" + Environment.NewLine + text_output.Text;
        }

        private void initGPIO()
        {
            var gpio = GpioController.GetDefault();
            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                text_output.Text += "There is no GPIO controller on this device. Program is terminated." + Environment.NewLine;
                return;
            }

            if (arduino == null)
            {
                arduino = gpio.OpenPin(arduino_pin);
                arduino.SetDriveMode(GpioPinDriveMode.Output);
            }
        }

        public async void I2CInit()
        {
            var settings = new I2cConnectionSettings(0x40); // Arduino address
            settings.BusSpeed = I2cBusSpeed.StandardMode;
            settings.SharingMode = I2cSharingMode.Shared;
            settings.SlaveAddress = 0x40;
            string aqs = I2cDevice.GetDeviceSelector("I2C1");

            var dis = await DeviceInformation.FindAllAsync(aqs);
            Device = await I2cDevice.FromIdAsync(dis[0].Id, settings);

            // periodicTimer = new Timer(this.TimerCallback, null, 0, 500); // Create a timmer 
        }
      

        private void TimerCallback(object state)
        {
            byte[] RegAddrBuf = new byte[] { 0x40 };
            byte[] ReadBuf = new byte[5];

            try
            {
                Device.Read(ReadBuf); // read the data
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
            UpdateGUIWithNewMessage(ReadBuf);
        }

        private void UpdateGUIWithNewMessage(byte[] message)
        {
            char[] cArray = System.Text.Encoding.UTF8.GetString(message, 0, 5).ToCharArray();  // Convert Byte to Char
            String rawText = new String(cArray); //Convert char array to String

            // Update GUI
            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

                if (rawText != null && rawText.Length > 0) //When some data is received 
                {
                    text_output.Text = "Read: " + rawText + Environment.NewLine + text_output.Text; //Update GUI with raw text

                    Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                    String safeText = rgx.Replace(rawText, ""); //Remove all non alphanumeric values 

                    if (safeText.Length > 0)
                    {
                        meter_status.Value = int.Parse(safeText); //Set progress bar 
                        UpdateSwitchLayoutColor(); //Blink UI ellipse
                    }

                }
            });
        }


        private void button_send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] ReadBuf = new byte[64];
                byte[] message = Encoding.UTF8.GetBytes("RF:" + text_message.Text);
                byte[] RegAddrBuf = new byte[] { 0x40 };
                //Device.Write(message);


                Device.WriteRead(RegAddrBuf, ReadBuf);
                Debug.WriteLine(ReadBuf);
                text_message.Text = "";
                text_message.Focus(FocusState.Keyboard); 
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
        }

        private void UpdateSwitchLayoutColor()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();

                mySolidColorBrush.Color = (color) ? Color.FromArgb(129, 215, 66, 0) : Color.FromArgb(221, 51, 51, 0);
                layout_update.Fill = mySolidColorBrush;

                color = !color;
            });
        }

    }
}
