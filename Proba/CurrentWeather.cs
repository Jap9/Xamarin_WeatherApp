using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using CreateDatabaseWithSQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Proba
{
    [Activity(Label = "Weather", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener

    {
        private LocationManager locMgr;
        private string tag = "MainActivity";
        private ProgressDialog progressDialog;
        private Boolean executed = false;
        private String pathToDatabase;
        private Bundle pass_resources = new Bundle();
        // Get extra resources that will be passed to the Forecast activity

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.CurrentWeatherLayout);

            //CreateDB - SQLite3, we will add some weather data after
            pathToDatabase = createDB();

            /* Location listener will execute API connections and data parsing after receiving location
            *  So no method is called in an explicit way here
            */

            LinearLayout mainLayout = FindViewById<LinearLayout>(Resource.Id.MainLayout);
            mainLayout.Visibility = ViewStates.Gone;

            progressDialog = ProgressDialog.Show(this, "Please wait...", "Getting location and checking weather...", true);

            GridLayout WeekForecast = FindViewById<GridLayout>(Resource.Id.GridLayoutForecast);
            WeekForecast.Click += delegate
            {
                Intent intent = new Intent(this, typeof(Forecast));

                // Pass data that we already have downloaded
                intent.PutExtra("extra", pass_resources);
                var options = ActivityOptions.MakeSceneTransitionAnimation(this, WeekForecast, "activity_shared");
                StartActivity(intent, options.ToBundle());
            };
        }

        protected void onActivityResult(int requestCode, int resultCode, Intent data)
        {
        }

        // Get JSON from API methods
        private Newtonsoft.Json.Linq.JObject GetCurrentWeather(String city)
        {
            //Get current Weather
            string url = "http://api.openweathermap.org/data/2.5/weather?q=" + city +
                "&format=json" + "&APPID=" + "44a54ba452df022234d6111083ccc668";

            pass_resources.PutString("city", city.ToString());
            return connectURL(url);
        }

        private Newtonsoft.Json.Linq.JObject GetForecast(String city)
        {
            //Get 7 day forecast for city (only 5 will be show in this activity)
            string url = "http://api.openweathermap.org/data/2.5/forecast/daily?q=" + city + "&mode=json&units=metric" +
                "&cnt=7&APPID=44a54ba452df022234d6111083ccc668";

            Newtonsoft.Json.Linq.JObject json_forecast = connectURL(url);
            pass_resources.PutString("json_forecast", json_forecast.ToString());

            return json_forecast;
        }

        private Newtonsoft.Json.Linq.JObject connectURL(String url)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(url);
            StreamReader reader = new StreamReader(stream);

            Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(reader.ReadLine());
            stream.Close();
            return json;
        }

        // Parse JSON and show results methods
        private void ParseAndDisplayCurrent(Newtonsoft.Json.Linq.JObject json)
        {
            // Set up fields
            TextView location = FindViewById<TextView>(Resource.Id.locationText);

            TextView temp = FindViewById<TextView>(Resource.Id.temperature);
            TextView maxTemp = FindViewById<TextView>(Resource.Id.maxTemp);
            TextView minTemp = FindViewById<TextView>(Resource.Id.minTemp);

            TextView description = FindViewById<TextView>(Resource.Id.descriptionText);
            TextView humidity = FindViewById<TextView>(Resource.Id.humidText);
            TextView wind = FindViewById<TextView>(Resource.Id.windText);
            TextView pressure = FindViewById<TextView>(Resource.Id.pressureText);

            ImageView weather_image = FindViewById<ImageView>(Resource.Id.weather_image);

            Newtonsoft.Json.Linq.JObject token = Newtonsoft.Json.Linq.JObject.Parse(json.ToString());

            //Location
            string city = (string)token["name"];
            string country = (string)token["sys"]["country"];
            location.Text = city + ", " + country;

            // Temperature
            double temperature = (double)token["main"]["temp"] - 273.15;
            temp.Text = String.Format("{0:F2}", temperature) + "°";

            double temperature_min = (double)token["main"]["temp_min"] - 273.15;
            minTemp.Text = String.Format("{0:F2}", temperature_min) + "°  |  ";

            double temperature_max = (double)token["main"]["temp_max"] - 273.15;
            maxTemp.Text = String.Format("{0:F2}", temperature_max) + "°";

            //Info
            description.Text = "Description: " + (string)token["weather"][0]["description"];
            humidity.Text = "Humidity: " + (string)token["main"]["humidity"] + " %";
            wind.Text = "Wind: " + (string)token["wind"]["speed"] + " m/s";
            pressure.Text = "Pressure: " + (string)token["main"]["pressure"] + " hPa";

            //Icon
            string icon = (string)token["weather"][0]["icon"];
            var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon + ".png");

            weather_image.SetImageBitmap(imageBitmap);

            //Insert some Weather data to test
            var insert = insertUpdateData(new WeatherStructureDB
            {
                City = city,
                Country = country,
                Temperature = temperature.ToString()
            }, pathToDatabase);

            var records = CountAll(pathToDatabase);
            Toast.MakeText(this, string.Format("Currently {0} weather records saved!\n", records), ToastLength.Long).Show();
        }

        private void ParseAndDisplayForecast(Newtonsoft.Json.Linq.JObject json)
        {
            Newtonsoft.Json.Linq.JObject token = Newtonsoft.Json.Linq.JObject.Parse(json.ToString());

            //Get weather forecast for each day
            double temp_dia1 = (double)token["list"][0]["temp"]["day"];
            double temp_dia2 = (double)token["list"][1]["temp"]["day"];
            double temp_dia3 = (double)token["list"][2]["temp"]["day"];
            double temp_dia4 = (double)token["list"][3]["temp"]["day"];
            double temp_dia5 = (double)token["list"][4]["temp"]["day"];
            // Two more that will be used in the Forecast activity
            double temp_dia6 = (double)token["list"][5]["temp"]["day"];
            double temp_dia7 = (double)token["list"][6]["temp"]["day"];

            //Get weather icon
            string icon_dia1 = (string)token["list"][0]["weather"][0]["icon"];
            string icon_dia2 = (string)token["list"][1]["weather"][0]["icon"];
            string icon_dia3 = (string)token["list"][2]["weather"][0]["icon"];
            string icon_dia4 = (string)token["list"][3]["weather"][0]["icon"];
            string icon_dia5 = (string)token["list"][4]["weather"][0]["icon"];
            // Two more that will be used in the Forecast activity
            string icon_dia6 = (string)token["list"][5]["weather"][0]["icon"];
            string icon_dia7 = (string)token["list"][6]["weather"][0]["icon"];

            //Set up weather in Layout
            TextView day1_text = FindViewById<TextView>(Resource.Id.day1_text);
            TextView day2_text = FindViewById<TextView>(Resource.Id.day2_text);
            TextView day3_text = FindViewById<TextView>(Resource.Id.day3_text);
            TextView day4_text = FindViewById<TextView>(Resource.Id.day4_text);
            TextView day5_text = FindViewById<TextView>(Resource.Id.day5_text);

            day1_text.Text = String.Format("{0:F1}", temp_dia1) + "°";
            day2_text.Text = String.Format("{0:F1}", temp_dia2) + "°";
            day3_text.Text = String.Format("{0:F1}", temp_dia3) + "°";
            day4_text.Text = String.Format("{0:F1}", temp_dia4) + "°";
            day5_text.Text = String.Format("{0:F1}", temp_dia5) + "°";

            // Put to bundle data that will get pass to the other activity
            pass_resources.PutString("temp_dia1", String.Format("{0:F1}", temp_dia1) + "°");
            pass_resources.PutString("temp_dia2", String.Format("{0:F1}", temp_dia2) + "°");
            pass_resources.PutString("temp_dia3", String.Format("{0:F1}", temp_dia3) + "°");
            pass_resources.PutString("temp_dia4", String.Format("{0:F1}", temp_dia4) + "°");
            pass_resources.PutString("temp_dia5", String.Format("{0:F1}", temp_dia5) + "°");
            pass_resources.PutString("temp_dia6", String.Format("{0:F1}", temp_dia6) + "°");
            pass_resources.PutString("temp_dia7", String.Format("{0:F1}", temp_dia7) + "°");

            //Set up icon in Layout
            ImageView day1_icon = FindViewById<ImageView>(Resource.Id.day1_icon);
            ImageView day2_icon = FindViewById<ImageView>(Resource.Id.day2_icon);
            ImageView day3_icon = FindViewById<ImageView>(Resource.Id.day3_icon);
            ImageView day4_icon = FindViewById<ImageView>(Resource.Id.day4_icon);
            ImageView day5_icon = FindViewById<ImageView>(Resource.Id.day5_icon);

            //Download and set Icons
            // Bundle data to avoid redownload
            MemoryStream stream1 = new MemoryStream();

            //Icon 1
            Bitmap bitmap_icon = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon_dia1 + ".png");
            day1_icon.SetImageBitmap(bitmap_icon);

            bitmap_icon.Compress(Bitmap.CompressFormat.Png, 0, stream1);
            byte[] bitmapData = stream1.ToArray();
            pass_resources.PutByteArray("icon1", bitmapData);

            //Icon 2
            MemoryStream stream2 = new MemoryStream();
            bitmap_icon = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon_dia2 + ".png");
            day2_icon.SetImageBitmap(bitmap_icon);

            bitmap_icon.Compress(Bitmap.CompressFormat.Png, 0, stream2);
            byte[] bitmapData2 = stream2.ToArray();
            pass_resources.PutByteArray("icon2", bitmapData2);

            //Icon 3
            MemoryStream stream3 = new MemoryStream();
            bitmap_icon = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon_dia3 + ".png");
            day3_icon.SetImageBitmap(bitmap_icon);

            bitmap_icon.Compress(Bitmap.CompressFormat.Png, 0, stream3);
            byte[] bitmapData3 = stream3.ToArray();
            pass_resources.PutByteArray("icon3", bitmapData3);

            //Icon 4
            MemoryStream stream4 = new MemoryStream();
            bitmap_icon = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon_dia4 + ".png");
            day4_icon.SetImageBitmap(bitmap_icon);

            bitmap_icon.Compress(Bitmap.CompressFormat.Png, 0, stream4);
            byte[] bitmapData4 = stream4.ToArray();
            pass_resources.PutByteArray("icon4", bitmapData4);

            //Icon 5
            MemoryStream stream5 = new MemoryStream();
            bitmap_icon = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon_dia5 + ".png");
            day5_icon.SetImageBitmap(bitmap_icon);

            bitmap_icon.Compress(Bitmap.CompressFormat.Png, 0, stream5);
            byte[] bitmapData5 = stream5.ToArray();
            pass_resources.PutByteArray("icon5", bitmapData5);

            //Icon 6
            MemoryStream stream6 = new MemoryStream();
            bitmap_icon = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon_dia6 + ".png");
            day5_icon.SetImageBitmap(bitmap_icon);

            bitmap_icon.Compress(Bitmap.CompressFormat.Png, 0, stream6);
            byte[] bitmapData6 = stream6.ToArray();
            pass_resources.PutByteArray("icon6", bitmapData6);

            //Icon 7
            MemoryStream stream7 = new MemoryStream();
            bitmap_icon = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon_dia7 + ".png");
            day5_icon.SetImageBitmap(bitmap_icon);

            bitmap_icon.Compress(Bitmap.CompressFormat.Png, 0, stream7);
            byte[] bitmapData7 = stream7.ToArray();
            pass_resources.PutByteArray("icon7", bitmapData7);
        }

        // Extra methods
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

        private String getCity(String latitude, String longitude)
        {
            string url = "http://maps.googleapis.com/maps/api/geocode/json?latlng=" + latitude + "," + longitude + "&sensor=true";
            WebClient webclient = new WebClient();
            var data = webclient.DownloadString(url);
            Newtonsoft.Json.Linq.JObject decodedCity = Newtonsoft.Json.Linq.JObject.Parse(data);

            return (string)decodedCity["results"][0]["address_components"][2]["long_name"];
        }

        private void ExecuteTasks(String city)
        {
            LinearLayout mainLayout = FindViewById<LinearLayout>(Resource.Id.MainLayout);

            //Get Current weather
            Newtonsoft.Json.Linq.JObject json = GetCurrentWeather(city);

            //Get Forecast weather
            Newtonsoft.Json.Linq.JObject forecast = GetForecast(city);

            ParseAndDisplayCurrent(json);
            ParseAndDisplayForecast(forecast);
            mainLayout.Visibility = ViewStates.Visible;

            progressDialog.Hide();
        }

        // Location methods
        protected override void OnStart()
        {
            base.OnStart();
            Log.Debug(tag, "OnStart called");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Log.Debug(tag, "OnResume called");

            // Initialize location manager
            locMgr = GetSystemService(Context.LocationService) as LocationManager;

            if (locMgr.AllProviders.Contains(LocationManager.NetworkProvider)
                && locMgr.IsProviderEnabled(LocationManager.NetworkProvider))
            {
                locMgr.RequestLocationUpdates(LocationManager.NetworkProvider, 60000, 50, this);
            }
            else
            {
                // Not GPS provider is used, load default city Barcelona
                if (executed == false)
                {
                    Toast.MakeText(this, "Enable Network GPS Provider! Default city loaded", ToastLength.Long).Show();
                    ExecuteTasks("Barcelona");
                    executed = true;
                }
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Stop sending location updates when the application goes into the background
            locMgr.RemoveUpdates(this);
            Log.Debug(tag, "Location updates paused because application is entering the background");
        }

        protected override void OnStop()
        {
            base.OnStop();
            Log.Debug(tag, "OnStop called");
            locMgr.RemoveUpdates(this);
        }

        public void OnLocationChanged(Android.Locations.Location location)
        {
            /*
             * First receive location object with coordinate
             * Get City from coordinates
             * Launch Thread with Dialog
             * Pass city to Forecast and Current Weather
             *
             * */

            Log.Debug(tag, "Location changed");

            String latitude_decimal = location.Latitude.ToString().Replace(",", ".");
            String longitude_decimal = location.Longitude.ToString().Replace(",", ".");

            //Get address from coordinates
            String city = getCity(latitude_decimal, longitude_decimal);
            //Toast.MakeText(this, "Loation: " + location.Latitude.ToString() + " - " + location.Longitude.ToString()+" - "+city, ToastLength.Long).Show();

            ExecuteTasks(city);

            //Hide on finish
            locMgr.RemoveUpdates(this);
        }

        public void OnProviderDisabled(string provider)
        {
            Log.Debug(tag, provider + " disabled by user");
        }

        public void OnProviderEnabled(string provider)
        {
            Log.Debug(tag, provider + " enabled by user");
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Log.Debug(tag, provider + " availability has changed to " + status.ToString());
        }

        // DB methods
        private string createDatabase(string path)
        {
            try
            {
                var connection = new SQLiteConnection(path);
                connection.CreateTable<WeatherStructureDB>();
                return "Database created";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        private string insertUpdateData(WeatherStructureDB data, string path)
        {
            try
            {
                var db = new SQLiteConnection(path);
                if (db.Insert(data) != 0)
                    db.Update(data);
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        private string insertUpdateAllData(IEnumerable<WeatherStructureDB> data, string path)
        {
            try
            {
                var db = new SQLiteConnection(path);
                if (db.InsertAll(data) != 0)
                    db.UpdateAll(data);
                return "List of data inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        private int CountAll(string path)
        {
            try
            {
                var db = new SQLiteConnection(path);
                //  Counts all records in the database
                return db.ExecuteScalar<int>("SELECT Count(*) FROM WeatherStructureDB");
            }
            catch (SQLiteException)
            {
                return -1;
            }
        }

        private String createDB()
        {
            var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var pathToDatabase = System.IO.Path.Combine(folder, "db_sqlnet.db");

            //Check if Successfull
            var result = createDatabase(pathToDatabase);
            /*if (result == "Database created")
                Toast.MakeText(this, "SQLite DB created successfully to: " + pathToDatabase, ToastLength.Long).Show();*/

            return pathToDatabase;
        }
    }
}