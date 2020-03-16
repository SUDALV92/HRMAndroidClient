using Microsoft.AspNetCore.SignalR.Client;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace HRMAndroidClient.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<IDevice> Devices { get; set; }
        HubConnection hubConnection;
        public string HrmValue
        {
            get => hrmValue; 
            set
            {
                hrmValue = value;
                OnPropertyChanged();
            }
        }
        IAdapter Adapter;
        private string hrmValue;

        public MainViewModel()
        {
            Devices = new ObservableCollection<IDevice>();
            hubConnection = new HubConnectionBuilder().WithUrl("http://192.168.1.105:1000/hrmHub").WithAutomaticReconnect().Build();
        }

        private void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
        {
            Devices.Add(e.Device);
        }

        internal void Load()
        {
            Adapter = CrossBluetoothLE.Current.Adapter;
            Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;

            foreach (var device in Adapter.GetSystemConnectedOrPairedDevices())
            {
                Devices.Add(device);
            }
        }

        internal async Task Connect(IDevice device)
        {
            await Adapter.ConnectToDeviceAsync(device, new ConnectParameters(autoConnect: false, forceBleTransport: true));
            var services = await device.GetServicesAsync();
            IService hrmService = null;
            foreach (var service in services)
            {
                if (service.Name.Equals("Heart Rate"))
                {
                    hrmService = service;
                }
            }
            var characteristics = await hrmService.GetCharacteristicsAsync();
            ICharacteristic hrmCharacteristic = null;
            foreach (var characteristic in characteristics)
            {
                if (characteristic.Name.Equals("Heart Rate Measurement"))
                {
                    hrmCharacteristic = characteristic;
                }
            }
            hrmCharacteristic.ValueUpdated += HrmCharacteristic_ValueUpdated;
            await hrmCharacteristic.StartUpdatesAsync();

            await hubConnection.StartAsync();
        }

        private async void HrmCharacteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            int value;
            HrmValue = Convert.ToString(e.Characteristic.Value[0]) + Convert.ToString(e.Characteristic.Value[1]);
            if (int.TryParse(HrmValue, out value))
            {
                if (hubConnection.State == HubConnectionState.Connected)
                    await hubConnection.InvokeAsync("HrmFromClient", value);
            }
        }
    }
}
