using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SwitchBotReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 100));
            ApplicationView.PreferredLaunchViewSize = new Size(200, 100);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;


            BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
            //watcher.ScanningMode = BluetoothLEScanningMode.Active; //passive scanning is sufficient and uses less power
            watcher.Received += OnAdvertisementReceived;

            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = 2409;

            watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
            watcher.Start();
        }


        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            string mac = MacAddressToString(args.BluetoothAddress);

            Debug.WriteLine("Advertisement received");
            var data = args.Advertisement.ManufacturerData.First().Data.ToArray();
            Debug.WriteLine(mac + " " + BitConverter.ToString(data));

            //data[0] -> data[5] mac address
            //data[6], data[7], data[8]&0xF0 unknown
            float tempc = (float)((data[9] - 128) + (data[8] & 0xF) / 10.0);
            int hum = data[10];
            //data[11] present in outdoor thermometer but always 0, may be pressure
            Debug.WriteLine(tempc);
            Debug.WriteLine(hum);
            return;
        }


        private string MacAddressToString(ulong macAddress)
        {
            byte[] bytes = BitConverter.GetBytes(macAddress);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                if (i != 0)
                    sb.Insert(0, ":");

                sb.Insert(0, bytes[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public static ulong MacAddressToUlong(string macAddress)
        {
            string[] parts = macAddress.Split(':');
            byte[] bytes = new byte[8];
            for (int i = 0; i < 6; i++)
            {
                bytes[i] = byte.Parse(parts[i], System.Globalization.NumberStyles.HexNumber);
            }

            ulong ulongMacAddress = BitConverter.ToUInt64(bytes, 0);

            return ulongMacAddress;
        }
    }
}
