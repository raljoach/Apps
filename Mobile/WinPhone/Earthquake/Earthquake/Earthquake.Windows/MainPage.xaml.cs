using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Earthquake
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Fetch();
        }

        private async void Fetch()
        {
            HttpClient client = new HttpClient();

            var response = await client.GetStreamAsync(new Uri(
               "http://api.geonames.org/earthquakesJSON?formatted=true&north=44.1&south=-9.9&east=-22.4&west=55.2&username=mkoppelman"));

            var serializer = new DataContractJsonSerializer(typeof(RootObject));

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(RootObject));
            RootObject feed = (RootObject)ser.ReadObject(response);
            /*foreach (Earthquake e in feed.earthquakes)
            {
                System.Diagnostics.Debug.WriteLine(e.src + " " + e.magnitude + " '" + e.lat + "' '" + e.lng + "'");
            }*/
            myListView.DataContext = feed;

        }
    }
}
