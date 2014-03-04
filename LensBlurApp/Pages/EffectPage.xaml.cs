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
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Nokia.Graphics.Imaging;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Storage.Streams;

namespace LensBlurApp.Pages
{
    public partial class EffectPage : PhoneApplicationPage
    {
        private bool _processing;
        private bool _processingPending;
        private ApplicationBarIconButton _saveButton;
        private ApplicationBarMenuItem _helpMenuItem;
        private ApplicationBarMenuItem _aboutMenuItem;

        private bool Processing
        {
            get
            {
                return _processing;
            }

            set
            {
                if (_processing != value)
                {
                    _processing = value;

                    ProgressBar.IsIndeterminate = _processing;
                    ProgressBar.Visibility = _processing ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public EffectPage()
        {
            InitializeComponent();

            CreateButtons();

            SizeSlider.ValueChanged += SizeSlider_ValueChanged;

            if (Model.KernelSize > 0.5)
            {
                SizeSlider.Value = Model.KernelSize;
            }
            else
            {
                Model.KernelSize = SizeSlider.Value;
            }
        }

        private void CreateButtons()
        {
            _saveButton = new ApplicationBarIconButton
            {
                Text = AppResources.EffectPage_SaveButton,
                IconUri = new Uri("Assets/Icons/Save.png", UriKind.Relative),
            };

            _helpMenuItem = new ApplicationBarMenuItem
            {
                Text = AppResources.Application_HelpMenuItem
            };

            _aboutMenuItem = new ApplicationBarMenuItem
            {
                Text = AppResources.Application_AboutMenuItem
            };

            _saveButton.Click += SaveButton_Click;
            _helpMenuItem.Click += HelpMenuItem_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.Buttons.Add(_saveButton);
            ApplicationBar.MenuItems.Add(_helpMenuItem);
            ApplicationBar.MenuItems.Add(_aboutMenuItem);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            AttemptSaveAsync();
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/HelpPage.xaml", UriKind.Relative));
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AttemptUpdatePreviewAsync();

            Model.Saved = false;
            Model.KernelSize = e.NewValue;

            AdaptButtonsToState();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Model.OriginalImage == null || Model.AnnotationsBitmap == null)
            {
                NavigationService.GoBack();
            }
            else
            {
                AdaptButtonsToState();

                AttemptUpdatePreviewAsync();
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (Processing)
            {
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            PreviewImage.Source = null;
        }

        private void AdaptButtonsToState()
        {
            var accentColorBrush = (Brush)Application.Current.Resources["PhoneAccentBrush"];
            var transparentBrush = (Brush)Application.Current.Resources["TransparentBrush"];

            CircleButton.Background = Model.KernelShape == LensBlurPredefinedKernelShape.Circle ? accentColorBrush : transparentBrush;
            HexagonButton.Background = Model.KernelShape == LensBlurPredefinedKernelShape.Hexagon ? accentColorBrush : transparentBrush;
            FlowerButton.Background = Model.KernelShape == LensBlurPredefinedKernelShape.Flower ? accentColorBrush : transparentBrush;
            StarButton.Background = Model.KernelShape == LensBlurPredefinedKernelShape.Star ? accentColorBrush : transparentBrush;
            HeartButton.Background = Model.KernelShape == LensBlurPredefinedKernelShape.Heart ? accentColorBrush : transparentBrush;

            _saveButton.IsEnabled = !Model.Saved && !Processing;
            _helpMenuItem.IsEnabled = !Processing;
            _aboutMenuItem.IsEnabled = !Processing;
        }

        private async void AttemptUpdatePreviewAsync()
        {
            if (!Processing)
            {
                Processing = true;

                AdaptButtonsToState();

                Model.OriginalImage.Position = 0;

                using (var source = new StreamImageSource(Model.OriginalImage))
                using (var segmenter = new InteractiveForegroundSegmenter(source))
                using (var annotationsSource = new BitmapImageSource(Model.AnnotationsBitmap))
                {
                    segmenter.Quality = 0.5;
                    segmenter.AnnotationsSource = annotationsSource;

                    var foregroundColor = Model.ForegroundBrush.Color;
                    var backgroundColor = Model.BackgroundBrush.Color;

                    segmenter.ForegroundColor = Windows.UI.Color.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
                    segmenter.BackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);

                    do
                    {
                        _processingPending = false;

                        var previewBitmap = new WriteableBitmap((int)Model.AnnotationsBitmap.Dimensions.Width, (int)Model.AnnotationsBitmap.Dimensions.Height);

                        using (var effect = new LensBlurEffect(source, new LensBlurPredefinedKernel(Model.KernelShape, (uint)Model.KernelSize)))
                        using (var renderer = new WriteableBitmapRenderer(effect, previewBitmap))
                        {
                            effect.KernelMap = segmenter;

                            try
                            {
                                await renderer.RenderAsync();

                                PreviewImage.Source = previewBitmap;

                                previewBitmap.Invalidate();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("AttemptUpdatePreviewAsync rendering failed: " + ex.Message);
                            }
                        }
                    }
                    while (_processingPending);
                }

                Processing = false;

                AdaptButtonsToState();
            }
            else
            {
                _processingPending = true;
            }
        }

        private async void AttemptSaveAsync()
        {
            if (!Processing)
            {
                Processing = true;

                AdaptButtonsToState();

                GC.Collect();

                var lowMemory = false;

                try
                {
                    long result = (long)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");

                    lowMemory = result / 1024 / 1024 < 300;
                }
                catch (ArgumentOutOfRangeException)
                {
                }

                IBuffer buffer = null;

                Model.OriginalImage.Position = 0;

                using (var source = new StreamImageSource(Model.OriginalImage))
                using (var segmenter = new InteractiveForegroundSegmenter(source))
                using (var annotationsSource = new BitmapImageSource(Model.AnnotationsBitmap))
                {
                    segmenter.Quality = lowMemory ? 0.5 : 1;
                    segmenter.AnnotationsSource = annotationsSource;

                    var foregroundColor = Model.ForegroundBrush.Color;
                    var backgroundColor = Model.BackgroundBrush.Color;

                    segmenter.ForegroundColor = Windows.UI.Color.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
                    segmenter.BackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);

                    using (var effect = new LensBlurEffect(source, new LensBlurPredefinedKernel(Model.KernelShape, (uint)Model.KernelSize)))
                    using (var renderer = new JpegRenderer(effect))
                    {
                        effect.KernelMap = segmenter;

                        try
                        {
                            buffer = await renderer.RenderAsync();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("AttemptSave rendering failed: " + ex.Message);
                        }
                    }
                }

                if (buffer != null)
                {
                    using (var library = new MediaLibrary())
                    using (var stream = buffer.AsStream())
                    {
                        library.SavePicture("lensblur_" + DateTime.Now.Ticks, stream);

                        Model.Saved = true;

                        AdaptButtonsToState();
                    }
                }

                Processing = false;

                AdaptButtonsToState();
            }
        }

        private void CircleButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model.KernelShape != LensBlurPredefinedKernelShape.Circle)
            {
                Model.KernelShape = LensBlurPredefinedKernelShape.Circle;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void HexagonButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model.KernelShape != LensBlurPredefinedKernelShape.Hexagon)
            {
                Model.KernelShape = LensBlurPredefinedKernelShape.Hexagon;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void FlowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model.KernelShape != LensBlurPredefinedKernelShape.Flower)
            {
                Model.KernelShape = LensBlurPredefinedKernelShape.Flower;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model.KernelShape != LensBlurPredefinedKernelShape.Star)
            {
                Model.KernelShape = LensBlurPredefinedKernelShape.Star;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void HeartButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model.KernelShape != LensBlurPredefinedKernelShape.Heart)
            {
                Model.KernelShape = LensBlurPredefinedKernelShape.Heart;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void PreviewImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Processing)
            {
                NavigationService.Navigate(new Uri("/Pages/ZoomPage.xaml", UriKind.Relative));
            }
        }
    }
}