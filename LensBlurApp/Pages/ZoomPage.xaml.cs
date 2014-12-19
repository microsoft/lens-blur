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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Lumia.Imaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;
using Lumia.Imaging.Adjustments;

namespace LensBlurApp.Pages
{
    public partial class ZoomPage : PhoneApplicationPage
    {
        private bool _processing;
        private WriteableBitmap _bitmap;
        private double _scale = 1.0;
        private bool _pinching = false;
        private Point _relativeCenter;

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

        public ZoomPage()
        {
            InitializeComponent();
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
                AttemptUpdateImageAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Image.Source = null;

            _bitmap = null;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (Processing)
            {
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private async void AttemptUpdateImageAsync()
        {
            if (!Processing)
            {
                Processing = true;

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

                var maxSide = lowMemory ? 2048.0 : 4096.0;

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

                    var info = await source.GetInfoAsync();

                    double scaler, rotation;
                    var width = info.ImageSize.Width;
                    var height = info.ImageSize.Height;

                    if (width > height)
                    {
                        scaler = maxSide / width;
                        rotation = 90;

                        var t = width; // We're rotating the image, so swap width and height
                        width = height;
                        height = t;
                    }
                    else
                    {
                        scaler = maxSide / height;
                        rotation = 0;
                    }

                    scaler = Math.Max(1, scaler);

                    _bitmap = new WriteableBitmap((int)(width * scaler), (int)(height * scaler));

                    using (var blurEffect = new LensBlurEffect(source, new LensBlurPredefinedKernel(Model.KernelShape, (uint)Model.KernelSize)))
                    using (var filterEffect = new FilterEffect(blurEffect) { Filters = new[] { new RotationFilter(rotation) }})
                    using (var renderer = new WriteableBitmapRenderer(filterEffect, _bitmap))
                    {
                        blurEffect.KernelMap = segmenter;

                        try
                        {
                            await renderer.RenderAsync();

                            Image.Source = _bitmap;

                            _bitmap.Invalidate();

                            ConfigureViewport();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("AttemptUpdateImageAsync rendering failed: " + ex.Message);
                        }
                    }
                }

                Processing = false;
            }
        }

        private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConfigureViewport();
        }

        private void ConfigureViewport()
        {
            if (_bitmap != null)
            {
                if (_bitmap.PixelWidth < _bitmap.PixelHeight)
                {
                    _scale = LayoutRoot.ActualWidth / _bitmap.PixelWidth;
                }
                else
                {
                    _scale = LayoutRoot.ActualHeight / _bitmap.PixelHeight;
                }

                Image.Width = _bitmap.PixelWidth * _scale;
                Image.Height = _bitmap.PixelHeight * _scale;

                Viewport.Bounds = new Rect(0, 0, Image.Width, Image.Height);
                Viewport.SetViewportOrigin(new Point(
                    Viewport.Bounds.Width / 2 - LayoutRoot.ActualWidth / 2,
                    Viewport.Bounds.Height / 2 - LayoutRoot.ActualHeight / 2));
            }
        }

        private void Viewport_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_pinching)
            {
                e.Handled = true;

                CompletePinching();
            }
        }

        private void Viewport_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                e.Handled = true;

                if (!_pinching)
                {
                    _pinching = true;

                    _relativeCenter = new Point(
                        e.PinchManipulation.Original.Center.X / Image.Width,
                        e.PinchManipulation.Original.Center.Y / Image.Height);
                }

                var pixelWidth = (double)_bitmap.PixelWidth;
                var pixelHeight = (double)_bitmap.PixelHeight;

                double w, h;

                if (pixelWidth < pixelHeight)
                {
                    w = pixelWidth * _scale * e.PinchManipulation.CumulativeScale;
                    w = Math.Max(LayoutRoot.ActualWidth, w);
                    w = Math.Min(w, pixelWidth);
                    w = Math.Min(w, 4096);

                    h = w * pixelHeight / pixelWidth;

                    if (h > 4096)
                    {
                        var scaler = 4096.0 / h;
                        h *= scaler;
                        w *= scaler;
                    }
                }
                else
                {
                    h = pixelHeight * _scale * e.PinchManipulation.CumulativeScale;
                    h = Math.Max(LayoutRoot.ActualHeight, h);
                    h = Math.Min(h, pixelHeight);
                    h = Math.Min(h, 4096);

                    w = h * pixelWidth / pixelHeight;

                    if (w > 4096)
                    {
                        var scaler = 4096.0 / w;
                        w *= scaler;
                        h *= scaler;
                    }
                }

                Image.Width = w;
                Image.Height = h;

                Viewport.Bounds = new Rect(0, 0, w, h);

                GeneralTransform transform = Image.TransformToVisual(Viewport);
                Point p = transform.Transform(e.PinchManipulation.Original.Center);

                double x = _relativeCenter.X * w - p.X;
                double y = _relativeCenter.Y * h - p.Y;

                if (w < pixelWidth && h < pixelHeight)
                {
                    //System.Diagnostics.Debug.WriteLine("Viewport.ActualWidth={0} .ActualHeight={1} Origin.X={2} .Y={3} Image.Width={4} .Height={5}",
                    //    Viewport.ActualWidth, Viewport.ActualHeight, x, y, Image.Width, Image.Height);

                    Viewport.SetViewportOrigin(new Point(x, y));
                }
            }
            else if (_pinching)
            {
                e.Handled = true;

                CompletePinching();
            }
        }

        private void Viewport_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_pinching)
            {
                e.Handled = true;

                CompletePinching();
            }
        }

        private void CompletePinching()
        {
            _pinching = false;

            double sw = Image.Width / _bitmap.PixelWidth;
            double sh = Image.Height / _bitmap.PixelHeight;

            _scale = Math.Min(sw, sh);
        }

        private void ContentPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Processing)
            {
                NavigationService.GoBack();
            }
        }
    }
}