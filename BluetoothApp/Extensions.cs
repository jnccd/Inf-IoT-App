using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlertDialog = Android.App.AlertDialog;

namespace BluetoothApp
{
    public static class Extensions
    {
        public static void ShowAsAlert(this AppCompatActivity a, string title = "", string message = "", EventHandler<DialogClickEventArgs> buttonEvent = null)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(a);
            AlertDialog alert = dialog.Create();
            alert.SetTitle(title);
            alert.SetMessage(message);
            if (buttonEvent != null)
            {
                alert.SetButton("Okay", buttonEvent);
                alert.SetCancelable(false);
            }
            alert.Show();
        }
        public static string Combine(this IEnumerable<string> s, string combinator = "")
        {
            return s.Count() == 0 ? "" : s.Foldl("", (x, y) => x + combinator + y).Remove(0, combinator.Length);
        }
        public static b Foldl<a, b>(this IEnumerable<a> xs, b y, Func<b, a, b> f)
        {
            foreach (a x in xs)
                y = f(y, x);
            return y;
        }
        public static b Foldl<a, b>(this IEnumerable<a> xs, Func<b, a, b> f)
        {
            return xs.Foldl(default, f);
        }
    }
}