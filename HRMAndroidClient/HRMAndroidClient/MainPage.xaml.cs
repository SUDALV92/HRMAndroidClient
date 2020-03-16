using HRMAndroidClient.ViewModel;
using Plugin.BLE.Abstractions.Contracts;
using System.ComponentModel;
using Xamarin.Forms;

namespace HRMAndroidClient
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainViewModel MainViewModel { get; set; }
        public MainPage()
        {
            InitializeComponent();
            BindingContext = MainViewModel = new MainViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MainViewModel.Load();
        }

        private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.CurrentSelection.Count > 0)
            {
                var device = (IDevice)e.CurrentSelection[0];
                await MainViewModel.Connect(device);
            }
        }
    }
}
