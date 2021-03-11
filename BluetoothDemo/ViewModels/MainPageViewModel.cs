using Acr.UserDialogs;
using BluetoothDemo.Helpers;
using BluetoothDemo.Services.BluetoothService;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace BluetoothDemo.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public IAdapter _adapterService;
        private readonly IBluetoothService _bluetoothService;

        private List<IDevice> _deviceList;

        public List<IDevice> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }

        private IDevice _selectedItem;

        public IDevice SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private bool isRefreshing = false;

        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set { SetProperty(ref isRefreshing, value); }
        }

        private DelegateCommand<IDevice> _itemSelectedCommand;

        public DelegateCommand<IDevice> ItemSelectedCommand =>
            _itemSelectedCommand ?? (_itemSelectedCommand = new DelegateCommand<IDevice>(async (a) => await ExecuteItemSelectedCommand(a)));

        private DelegateCommand _refreshCommand;

        public DelegateCommand RefreshCommand =>
            _refreshCommand ?? (_refreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand()));

        private async Task ExecuteRefreshCommand()
        {
            IsRefreshing = true;
            await GetBlutoothDevices();
            IsRefreshing = false;
        }

        private async Task ExecuteItemSelectedCommand(IDevice device)
        {
            try
            {

                IsRefreshing = true;
                await _adapterService.ConnectToDeviceAsync(device);

                //await _bluetoothService.ConnectToBluetoothDeviceAsync(device);
                await ExecuteRefreshCommand();
                IsRefreshing = false;
            }
            catch (DeviceConnectionException e)
            {
                // ... could not connect to device
            }
        }

        public MainPageViewModel(INavigationService navigationService, IUserDialogs userDialogsService, IBluetoothService bluetoothService)
            : base(navigationService, userDialogsService)
        {
            _adapterService = CrossBluetoothLE.Current.Adapter;
            IsRefreshing = true;
            _bluetoothService = bluetoothService;
        }

        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
        }

        public async override void OnAppearing()
        {
            base.OnAppearing();
            
        }

        private async Task GetBlutoothDevices()
        {
            var LocationPermission = await PermissionHelpers.RequestIfNeeded<Permissions.LocationAlways>();
            if (LocationPermission==PermissionStatus.Granted)
            {
                DeviceList = new List<IDevice>();
              
                await _adapterService.StartScanningForDevicesAsync();
                _adapterService.DeviceDiscovered += (s, a) => DeviceList.Add(a.Device);

            }
            else
            {
                await UserDialogsService.AlertAsync("Location Permission is required");
            }
           
        }
    }
}