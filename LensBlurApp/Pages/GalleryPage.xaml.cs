/*
 * Copyright (c) 2014 Microsoft Mobile
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
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
            else
            {
                if (_viewModel == null)
                {
                    _viewModel = new GalleryPageViewModel();

                    await _viewModel.Initialize();

                    DataContext = _viewModel;
                }

                Model.AnnotationsBitmap = null;
                Model.KernelShape = Lumia.Imaging.Adjustments.LensBlurPredefinedKernelShape.Circle;
                Model.KernelSize = 0.0;
                Model.OriginalImage = null;
                Model.Saved = false;
            }
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