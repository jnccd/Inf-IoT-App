using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Util;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Android.Views.View;
using AlertDialog = Android.App.AlertDialog;
using Random = System.Random;

// Sample send / rec
//// Read data from the device
//await _socket.InputStream.ReadAsync(buffer, 0, buffer.Length);

//// Write data to the device
//await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);

namespace BluetoothApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private static readonly Encoding _enc = Encoding.Unicode;
        private DateTime _lastSendTime;

        static UUID ARDUINO_UUID = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
        BluetoothSocket socc = null;

        Random rdm = new Random();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _lastSendTime = DateTime.Now;

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ConnectBlue();

            var sendButton = FindViewById<Button>(Resource.Id.sendButton);
            sendButton.Touch += async (object sender, TouchEventArgs e) => await SendBlue("0");
            var sendButton2 = FindViewById<Button>(Resource.Id.sendButton2);
            sendButton2.Touch += async (object sender, TouchEventArgs e) => await SendBlue("1");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            socc.Close();
        }

        private void ConnectBlue()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter == null)
                throw new Exception("No Bluetooth adapter found.");

            if (!adapter.IsEnabled)
                throw new Exception("Bluetooth adapter is not enabled.");

            BluetoothDevice device = adapter.BondedDevices.First(x => x.Name == "HC-05");

            if (device == null)
                throw new Exception("Named device not found.");

            socc = device.CreateRfcommSocketToServiceRecord(ARDUINO_UUID);
            try
            {
                socc.Connect();
            }
            catch (Exception ex)
            {
                socc.Close();
                this.ShowAsAlert("UwU we made a fucky wucky (in send)", ex.ToString());
            }
        }
        private async Task SendBlue(string message)
        {
            if (DateTime.Now - _lastSendTime < TimeSpan.FromSeconds(0.5))
                return;

            try
            {
                if (socc == null)
                    throw new Exception("No Socket.");

                if (!socc.IsConnected)
                    return;

                byte[] buffer = _enc.GetBytes(message);
                await socc.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                this.ShowAsAlert("UwU we made a fucky wucky (in send)", ex.ToString());
            }

            _lastSendTime = DateTime.Now;
        }
    }
}