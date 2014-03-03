/*
 * Copyright (c) 2014 Nokia Corporation. All rights reserved.
 *
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 *
 * See the license text file for license information.
 */

using LensBlurApp.Models;
using LensBlurApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LensBlurApp.Pages
{
    public partial class GalleryPage : PhoneApplicationPage
    {
        private GalleryPageViewModel _viewModel;
        private PhotoChooserTask _task = new PhotoChooserTask();
        private PhotoResult _photoResult;
        private ApplicationBarMenuItem _helpMenuItem;
        private ApplicationBarMenuItem _aboutMenuItem;

        public GalleryPage()
        {
            InitializeComponent();

            _task.ShowCamera = true;
            _task.Completed += PhotoChooserTask_Completed;

            _helpMenuItem = new ApplicationBarMenuItem
            {
                Text = AppResources.Application_HelpMenuItem
            };

            _aboutMenuItem = new ApplicationBarMenuItem
            {
                Text = AppResources.Application_AboutMenuItem
            };

            _helpMenuItem.Click += HelpMenuItem_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.MenuItems.Add(_helpMenuItem);
            ApplicationBar.MenuItems.Add(_aboutMenuItem);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_photoResult != null)
            {
                Model.OriginalImage = _photoResult.ChosenPhoto;
                Model.Saved = false;

                _photoResult = null;

                NavigationService.Navigate(new Uri("/Pages/SegmenterPage.xaml", UriKind.Relative));
            }
            else if (_viewModel == null)
            {
                _viewModel = new GalleryPageViewModel();

                await _viewModel.Initialize();

                DataContext = _viewModel;
            }

            Model.AnnotationsBitmap = null;
            Model.KernelShape = Nokia.Graphics.Imaging.LensBlurPredefinedKernelShape.Circle;
            Model.KernelSize = 0.0;
            Model.OriginalImage = null;
            Model.Saved = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            DataContext = null;

            _viewModel = null;
        }

        private void Thumbnail_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var image = sender as Image;
            var photo = image.Tag as Photo;

            if (photo != null)
            {
                var task = photo.File.OpenReadAsync().AsTask();

                task.Wait();

                Model.OriginalImage = task.Result.AsStream();
                Model.Saved = false;

                NavigationService.Navigate(new Uri("/Pages/SegmenterPage.xaml", UriKind.Relative));
            }
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

        private void OpenTextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            _task.Show();
        }
    }
}