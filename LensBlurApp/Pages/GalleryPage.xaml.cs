using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using LensBlurApp.Models;
using System.IO;
using Microsoft.Phone.Tasks;
using LensBlurApp.Resources;

namespace LensBlurApp.Pages
{
    public class Photo
    {
        public StorageFile File { get; private set; }

        public BitmapImage Thumbnail
        {
            get
            {
                var width = 220.0 * (Application.Current.Host.Content.ScaleFactor / 100.0);

                return new BitmapImage(new Uri("/Assets/Photos/" + File.Name, UriKind.Relative)) { DecodePixelWidth = (int)width };
            }
        }

        public Photo(StorageFile file)
        {
            File = file;
        }
    }

    public partial class GalleryPage : PhoneApplicationPage
    {
        public ObservableCollection<Photo> Photos { get; private set; }

        private PhotoChooserTask _task = new PhotoChooserTask();
        private PhotoResult _photoResult;
        private ApplicationBarIconButton _openButton;
        private ApplicationBarMenuItem _helpMenuItem;
        private ApplicationBarMenuItem _aboutMenuItem;

        public GalleryPage()
        {
            InitializeComponent();

            DataContext = this;

            Photos = new ObservableCollection<Photo>();

            _task.ShowCamera = true;
            _task.Completed += PhotoChooserTask_Completed;

            _openButton = new ApplicationBarIconButton
            {
                Text = AppResources.SegmenterPage_OpenButton,
                IconUri = new Uri("Assets/Icons/Folder.png", UriKind.Relative),
            };

            _helpMenuItem = new ApplicationBarMenuItem
            {
                Text = AppResources.Application_HelpMenuItem
            };

            _aboutMenuItem = new ApplicationBarMenuItem
            {
                Text = AppResources.Application_AboutMenuItem
            };

            _openButton.Click += OpenButton_Click;

            _helpMenuItem.Click += HelpMenuItem_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.Buttons.Add(_openButton);
            ApplicationBar.MenuItems.Add(_helpMenuItem);
            ApplicationBar.MenuItems.Add(_aboutMenuItem);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_photoResult != null)
            {
                Model.OriginalImage = _photoResult.ChosenPhoto;
                Model.Saved = false;

                _photoResult = null;

                NavigationService.Navigate(new Uri("/Pages/SegmenterPage.xaml", UriKind.Relative));
            }
            else
            {
                Initialize();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Uninitialize();
        }

        private async void Initialize()
        {
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync("Assets");
            folder = await folder.GetFolderAsync("Photos");
            var files = await folder.GetFilesAsync();

            foreach (var file in files)
            {
                Photos.Add(new Photo(file));
            }
        }

        private void Uninitialize()
        {
            Photos.Clear();
        }

        private void Thumbnail_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var image = sender as Image;
            var photo = image.Tag as Photo;

            var task = photo.File.OpenReadAsync().AsTask();

            task.Wait();

            Model.OriginalImage = task.Result.AsStream();
            Model.Saved = false;

            NavigationService.Navigate(new Uri("/Pages/SegmenterPage.xaml", UriKind.Relative));
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            _task.Show();
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/HelpPage.xaml", UriKind.Relative));
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
        }

        private void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _photoResult = e;
            }
        }
    }
}