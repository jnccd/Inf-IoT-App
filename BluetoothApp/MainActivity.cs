using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.Snackbar;
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
        bool connected = false;
        Button sendButton;
        Switch connectionSwitch;
        DateTime startTime;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _lastSendTime = DateTime.Now;

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            sendButton = FindViewById<Button>(Resource.Id.sendButton);
            sendButton.Touch += async (object sender, TouchEventArgs e) => await SendBlue("0");
            connectionSwitch = FindViewById<Switch>(Resource.Id.connectionSwitch);
            connectionSwitch.Click += (object sender, EventArgs e) => {
                startTime = DateTime.Now;
                if (connectionSwitch.Checked)
                    AttemptConnection();
                else
                {
                    socc.Close();
                    socc = null;
                    connected = false;
                    UpdateUI();
                }
            };

            var chron = FindViewById<TextView>(Resource.Id.timer);
            startTime = DateTime.Now;
            Task.Run(async () =>
            {
                while (true)
                {
                    var timeElapsed = DateTime.Now - startTime;
                    RunOnUiThread(() => chron.Text = $"{timeElapsed.Minutes}:{timeElapsed.Seconds}.{timeElapsed.Milliseconds}");
                    await Task.Delay(16);
                }
            });

            AttemptConnection();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            if (socc != null)
                socc.Close();
            base.OnDestroy();
        }

        private void AttemptConnection()
        {
            RunOnUiThread(() => connectionSwitch.Enabled = false);
            try
            {
                ConnectBlue();

                connected = true;
                UpdateUI();

                Snackbar.Make(FindViewById<RelativeLayout>(Resource.Id.baseLayout), "Connected!", Snackbar.LengthShort).Show();
            }
            catch
            {
                try { socc.Close(); } catch { }
                socc = null;

                connected = false;
                UpdateUI();

                Snackbar.Make(FindViewById<RelativeLayout>(Resource.Id.baseLayout), "Connection failed!", Snackbar.LengthShort).Show();
            }
            RunOnUiThread(() => connectionSwitch.Enabled = true);
        }

        private void UpdateUI()
        {
            RunOnUiThread(() =>
            {
                sendButton.Enabled = connected;
                connectionSwitch.Checked = connected;

                if (connected)
                    sendButton.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                else
                    sendButton.SetBackgroundColor(Android.Graphics.Color.LightGray);
            });
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
            socc.Connect();
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