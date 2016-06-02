using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using System;
using System.Net;

namespace Proba
{
    [Activity(Label = "Forecast")]
    public class Forecast : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ForecastLayout);

            // Set received data from previous activity
            getDataFromPreviousActivity();

            Bundle bundle = Intent.GetBundleExtra("extra");

            //Parse Json
            ParseAndDisplayForecast(bundle.Get("json_forecast").ToString());

            TextView cityText = FindViewById<TextView>(Resource.Id.cityText);
            cityText.Text = bundle.Get("city").ToString();
        }

        public override void OnBackPressed()
        {
            Intent returnIntent = new Intent();
            this.Finish();
            OverridePendingTransition(Resource.Animation.right_in, Resource.Animation.right_out);
        }

        private void getDataFromPreviousActivity()
        {
            // Setting up temp string
            TextView day1_text = FindViewById<TextView>(Resource.Id.day1_text);
            TextView day2_text = FindViewById<TextView>(Resource.Id.day2_text);
            TextView day3_text = FindViewById<TextView>(Resource.Id.day3_text);
            TextView day4_text = FindViewById<TextView>(Resource.Id.day4_text);
            TextView day5_text = FindViewById<TextView>(Resource.Id.day5_text);
            TextView day6_text = FindViewById<TextView>(Resource.Id.day6_text);
            TextView day7_text = FindViewById<TextView>(Resource.Id.day7_text);

            Bundle bundle = Intent.GetBundleExtra("extra");
            day1_text.Text = bundle.Get("temp_dia1").ToString();
            day2_text.Text = bundle.Get("temp_dia2").ToString();
            day3_text.Text = bundle.Get("temp_dia3").ToString();
            day4_text.Text = bundle.Get("temp_dia4").ToString();
            day5_text.Text = bundle.Get("temp_dia5").ToString();
            day6_text.Text = bundle.Get("temp_dia6").ToString();
            day7_text.Text = bundle.Get("temp_dia7").ToString();
        }

        private void ParseAndDisplayForecast(String json)
        {
            // Set up minima
            TextView day1_minima = FindViewById<TextView>(Resource.Id.day1_minima);
            TextView day2_minima = FindViewById<TextView>(Resource.Id.day2_minima);
            TextView day3_minima = FindViewById<TextView>(Resource.Id.day3_minima);
            TextView day4_minima = FindViewById<TextView>(Resource.Id.day4_minima);
            TextView day5_minima = FindViewById<TextView>(Resource.Id.day5_minima);
            TextView day6_minima = FindViewById<TextView>(Resource.Id.day6_minima);
            TextView day7_minima = FindViewById<TextView>(Resource.Id.day7_minima);

            // Set up maxima
            TextView day1_maxima = FindViewById<TextView>(Resource.Id.day1_maxima);
            TextView day2_maxima = FindViewById<TextView>(Resource.Id.day2_maxima);
            TextView day3_maxima = FindViewById<TextView>(Resource.Id.day3_maxima);
            TextView day4_maxima = FindViewById<TextView>(Resource.Id.day4_maxima);
            TextView day5_maxima = FindViewById<TextView>(Resource.Id.day5_maxima);
            TextView day6_maxima = FindViewById<TextView>(Resource.Id.day6_maxima);
            TextView day7_maxima = FindViewById<TextView>(Resource.Id.day7_maxima);

            Newtonsoft.Json.Linq.JObject token = Newtonsoft.Json.Linq.JObject.Parse(json.ToString());

            // Min temperature
            double temperature = (double)token["list"][0]["temp"]["min"];
            day1_minima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][1]["temp"]["min"];
            day2_minima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][2]["temp"]["min"];
            day3_minima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][3]["temp"]["min"];
            day4_minima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][4]["temp"]["min"];
            day5_minima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][5]["temp"]["min"];
            day6_minima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][6]["temp"]["min"];
            day7_minima.Text = String.Format("{0:F2}", temperature) + "°";

            // Min temperature
            temperature = (double)token["list"][0]["temp"]["max"];
            day1_maxima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][1]["temp"]["max"];
            day2_maxima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][2]["temp"]["max"];
            day3_maxima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][3]["temp"]["max"];
            day4_maxima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][4]["temp"]["max"];
            day5_maxima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][5]["temp"]["max"];
            day6_maxima.Text = String.Format("{0:F2}", temperature) + "°";

            temperature = (double)token["list"][6]["temp"]["max"];
            day7_maxima.Text = String.Format("{0:F2}", temperature) + "°";

            // Icons
            ImageView day1_icon = FindViewById<ImageView>(Resource.Id.day1_icon);
            ImageView day2_icon = FindViewById<ImageView>(Resource.Id.day2_icon);
            ImageView day3_icon = FindViewById<ImageView>(Resource.Id.day3_icon);
            ImageView day4_icon = FindViewById<ImageView>(Resource.Id.day4_icon);
            ImageView day5_icon = FindViewById<ImageView>(Resource.Id.day5_icon);
            ImageView day6_icon = FindViewById<ImageView>(Resource.Id.day6_icon);
            ImageView day7_icon = FindViewById<ImageView>(Resource.Id.day7_icon);

            setImage(day1_icon, "icon1");
            setImage(day2_icon, "icon2");
            setImage(day3_icon, "icon3");
            setImage(day4_icon, "icon4");
            setImage(day5_icon, "icon5");
            setImage(day6_icon, "icon6");
            setImage(day7_icon, "icon7");
        }

        private void setImage(ImageView day, String icon)
        {
            Bundle bundle = Intent.GetBundleExtra("extra");
            byte[] byteArray = (byte[])bundle.Get(icon);
            var imageBitmap = BitmapFactory.DecodeByteArray(byteArray, 0, byteArray.Length);
            day.SetImageBitmap(imageBitmap);
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            return imageBitmap;
        }
    }
}