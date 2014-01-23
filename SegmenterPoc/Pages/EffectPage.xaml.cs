/*
 * Copyright (c) 2014 Nokia Corporation. All rights reserved.
 *
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 *
 * See the license text file for license information.
 */

using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Nokia.Graphics.Imaging;
using SegmenterPoc.Models;
using SegmenterPoc.Resources;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Storage.Streams;

namespace SegmenterPoc.Pages
{
    public partial class EffectPage : PhoneApplicationPage
    {
        private bool _processing = false;
        private bool _processingPending = false;
        private LensBlurPredefinedKernelShape _shape = LensBlurPredefinedKernelShape.Circle;
        private ApplicationBarIconButton _saveButton = null;
        private ApplicationBarMenuItem _aboutMenuItem = null;

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
        }

        private void CreateButtons()
        {
            _saveButton = new ApplicationBarIconButton()
            {
                Text = AppResources.EffectPage_SaveButton,
                IconUri = new Uri("Assets/Icons/Save.png", UriKind.Relative),
            };

            _aboutMenuItem = new ApplicationBarMenuItem()
            {
                Text = AppResources.Application_AboutMenuItem
            };

            _saveButton.Click += SaveButton_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.Buttons.Add(_saveButton);
            ApplicationBar.MenuItems.Add(_aboutMenuItem);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            AttemptSave();
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AttemptUpdatePreviewAsync();

            Model.Saved = false;

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

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (Processing && e.IsCancelable)
            {
                e.Cancel = true;
            }

            base.OnNavigatingFrom(e);
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

            CircleButton.Background = _shape == LensBlurPredefinedKernelShape.Circle ? accentColorBrush : transparentBrush;
            HexagonButton.Background = _shape == LensBlurPredefinedKernelShape.Hexagon ? accentColorBrush : transparentBrush;
            FlowerButton.Background = _shape == LensBlurPredefinedKernelShape.Flower ? accentColorBrush : transparentBrush;
            StarButton.Background = _shape == LensBlurPredefinedKernelShape.Star ? accentColorBrush : transparentBrush;
            HeartButton.Background = _shape == LensBlurPredefinedKernelShape.Heart ? accentColorBrush : transparentBrush;

            _saveButton.IsEnabled = !Model.Saved;
        }

        private async void AttemptUpdatePreviewAsync()
        {
            if (!Processing)
            {
                Processing = true;

                Model.OriginalImage.Position = 0;

                using (var source = new StreamImageSource(Model.OriginalImage))
                using (var segmenter = new Nokia.Graphics.Imaging.InteractiveForegroundSegmenter(source))
                using (var annotationsSource = new BitmapImageSource(Model.AnnotationsBitmap))
                {
                    segmenter.IsPreview = true;
                    segmenter.AnnotationsSource = annotationsSource;

                    var foregroundColor = Model.ForegroundBrush.Color;
                    var backgroundColor = Model.BackgroundBrush.Color;

                    segmenter.ForegroundColor = Windows.UI.Color.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
                    segmenter.BackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);

                    do
                    {
                        _processingPending = false;

                        var previewBitmap = new WriteableBitmap((int)Model.AnnotationsBitmap.Dimensions.Width, (int)Model.AnnotationsBitmap.Dimensions.Height);

                        using (var effect = new LensBlurEffect(source, new LensBlurPredefinedKernel(_shape, (uint)SizeSlider.Value)))
                        using (var renderer = new WriteableBitmapRenderer(effect, previewBitmap))
                        {
                            effect.KernelMap = segmenter;

                            await renderer.RenderAsync();

                            PreviewImage.Source = previewBitmap;

                            previewBitmap.Invalidate();
                        }
                    }
                    while (_processingPending);
                }

                Processing = false;
            }
            else
            {
                _processingPending = true;
            }
        }

        private async void AttemptSave()
        {
            if (!Processing)
            {
                Processing = true;

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
                    segmenter.IsPreview = lowMemory;
                    segmenter.AnnotationsSource = annotationsSource;

                    var foregroundColor = Model.ForegroundBrush.Color;
                    var backgroundColor = Model.BackgroundBrush.Color;

                    segmenter.ForegroundColor = Windows.UI.Color.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
                    segmenter.BackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);

                    using (var effect = new LensBlurEffect(source, new LensBlurPredefinedKernel(_shape, (uint)SizeSlider.Value)))
                    using (var renderer = new JpegRenderer(effect))
                    {
                        effect.KernelMap = segmenter;

                        buffer = await renderer.RenderAsync();
                    }
                }

                using (var library = new MediaLibrary())
                using (var stream = buffer.AsStream())
                {
                    library.SavePicture("lensblur_" + DateTime.Now.Ticks, stream);

                    Model.Saved = true;

                    AdaptButtonsToState();
                }

                Processing = false;
            }
        }

        private void CircleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shape != LensBlurPredefinedKernelShape.Circle)
            {
                _shape = LensBlurPredefinedKernelShape.Circle;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void HexagonButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shape != LensBlurPredefinedKernelShape.Hexagon)
            {
                _shape = LensBlurPredefinedKernelShape.Hexagon;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void FlowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shape != LensBlurPredefinedKernelShape.Flower)
            {
                _shape = LensBlurPredefinedKernelShape.Flower;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shape != LensBlurPredefinedKernelShape.Star)
            {
                _shape = LensBlurPredefinedKernelShape.Star;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }

        private void HeartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shape != LensBlurPredefinedKernelShape.Heart)
            {
                _shape = LensBlurPredefinedKernelShape.Heart;

                AttemptUpdatePreviewAsync();

                Model.Saved = false;

                AdaptButtonsToState();
            }
        }
    }
}