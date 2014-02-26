/*
 * Copyright (c) 2014 Nokia Corporation. All rights reserved.
 *
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 *
 * See the license text file for license information.
 */

using System.Linq;
using LensBlurApp.Models;
using LensBlurApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LensBlurApp.Pages
{
    public partial class SegmenterPage : PhoneApplicationPage
    {
        private PhotoChooserTask _task = new PhotoChooserTask();
        private SolidColorBrush _brush;
        private Polyline _polyline;
        private bool _processing;
        private bool _processingPending;
        private ApplicationBarIconButton _openButton;
        private ApplicationBarIconButton _undoButton;
        private ApplicationBarIconButton _resetButton;
        private ApplicationBarIconButton _acceptButton;
        private ApplicationBarMenuItem _helpMenuItem;
        private ApplicationBarMenuItem _aboutMenuItem;
        private PhotoResult _photoResult;
        private bool _manipulating;

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

        private bool AnnotationsDrawn
        {
            get
            {
                return AnnotationsCanvas.Children.Count > 0;
            }
        }

        private bool ForegroundAnnotationsDrawn
        {
            get
            {
                return AnnotationsCanvas.Children.Cast<Polyline>().Any(p => p.Stroke == Model.ForegroundBrush);
            }
        }

        private bool BackgroundAnnotationsDrawn
        {
            get
            {
                return AnnotationsCanvas.Children.Cast<Polyline>().Any(p => p.Stroke == Model.BackgroundBrush);
            }
        }

        public SegmenterPage()
        {
            InitializeComponent();

            CreateButtons();

            _task.ShowCamera = true;
            _task.Completed += PhotoChooserTask_Completed;

            OriginalImage.LayoutUpdated += OriginalImage_LayoutUpdated;
        }

        private void CreateButtons()
        {
            _openButton = new ApplicationBarIconButton
            {
                Text = AppResources.SegmenterPage_OpenButton,
                IconUri = new Uri("Assets/Icons/Folder.png", UriKind.Relative),
            };

            _undoButton = new ApplicationBarIconButton
            {
                Text = AppResources.SegmenterPage_UndoButton,
                IconUri = new Uri("Assets/Icons/Undo.png", UriKind.Relative),
            };

            _resetButton = new ApplicationBarIconButton
            {
                Text = AppResources.SegmenterPage_ResetButton,
                IconUri = new Uri("Assets/Icons/Delete.png", UriKind.Relative),
            };

            _acceptButton = new ApplicationBarIconButton
            {
                Text = AppResources.SegmenterPage_AcceptButton,
                IconUri = new Uri("Assets/Icons/Check.png", UriKind.Relative),
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
            _undoButton.Click += UndoButton_Click;
            _resetButton.Click += ResetButton_Click;
            _acceptButton.Click += AcceptButton_Click;
            _helpMenuItem.Click += HelpMenuItem_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.Buttons.Add(_openButton);
            ApplicationBar.Buttons.Add(_undoButton);
            ApplicationBar.Buttons.Add(_resetButton);
            ApplicationBar.Buttons.Add(_acceptButton);
            ApplicationBar.MenuItems.Add(_helpMenuItem);
            ApplicationBar.MenuItems.Add(_aboutMenuItem);
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/HelpPage.xaml", UriKind.Relative));
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
        }

        private void OriginalImage_LayoutUpdated(object sender, EventArgs e)
        {
            MaskImage.Width = OriginalImage.ActualWidth;
            MaskImage.Height = OriginalImage.ActualHeight;

            AnnotationsCanvas.Width = OriginalImage.ActualWidth;
            AnnotationsCanvas.Height = OriginalImage.ActualHeight;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_photoResult != null)
            {
                Model.OriginalImage = _photoResult.ChosenPhoto;

                _photoResult = null;

                AnnotationsCanvas.Children.Clear();

                Model.Saved = false;
            }

            if (Model.OriginalImage != null)
            {
                if (_brush == null)
                {
                    _brush = Model.ForegroundBrush;
                }

                var originalBitmap = new BitmapImage
                {
                    DecodePixelWidth = (int)(480.0 * Application.Current.Host.Content.ScaleFactor / 100.0)
                };

                Model.OriginalImage.Position = 0;

                originalBitmap.SetSource(Model.OriginalImage);

                OriginalImage.Source = originalBitmap;

                AttemptUpdatePreviewAsync();
            }
            else
            {
                _brush = null;
            }

            AdaptButtonsToState();

            ManipulationArea.ManipulationStarted += AnnotationsCanvas_ManipulationStarted;
            ManipulationArea.ManipulationDelta += AnnotationsCanvas_ManipulationDelta;
            ManipulationArea.ManipulationCompleted += AnnotationsCanvas_ManipulationCompleted;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (Processing && e.IsCancelable)
            {
                e.Cancel = true;
            }
            else
            {
                ManipulationArea.ManipulationStarted -= AnnotationsCanvas_ManipulationStarted;
                ManipulationArea.ManipulationDelta -= AnnotationsCanvas_ManipulationDelta;
                ManipulationArea.ManipulationCompleted -= AnnotationsCanvas_ManipulationCompleted;
            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            MaskImage.Source = null;
            OriginalImage.Source = null;
        }

        private void AdaptButtonsToState()
        {
            _openButton.IsEnabled = !Processing;
            _undoButton.IsEnabled = AnnotationsDrawn && !Processing;
            _resetButton.IsEnabled = AnnotationsDrawn && !Processing;
            _acceptButton.IsEnabled = ForegroundAnnotationsDrawn && BackgroundAnnotationsDrawn && !Processing;
            _helpMenuItem.IsEnabled = !Processing;
            _aboutMenuItem.IsEnabled = !Processing;

            if (Model.OriginalImage != null)
            {
                ForegroundButton.IsEnabled = true;
                BackgroundButton.IsEnabled = true;

                ForegroundButton.Background = _brush == Model.ForegroundBrush ? Model.ForegroundBrush : null;
                BackgroundButton.Background = _brush == Model.BackgroundBrush ? Model.BackgroundBrush : null;

                GuideTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ForegroundButton.IsEnabled = false;
                BackgroundButton.IsEnabled = false;

                GuideTextBlock.Visibility = Visibility.Visible;
            }
        }

        private Point NearestPointInElement(double x, double y, FrameworkElement element)
        {
            var clampedX = Math.Min(Math.Max(0, x), element.ActualWidth);
            var clampedY = Math.Min(Math.Max(0, y), element.ActualHeight);

            return new Point(clampedX, clampedY);
        }

        private void AnnotationsCanvas_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            _manipulating = true;

            _polyline = new Polyline
            {
                Stroke = _brush,
                StrokeThickness = 6
            };

            var manipulationAreaDeltaX = ManipulationArea.Margin.Left;
            var manipulationAreaDeltaY = ManipulationArea.Margin.Top;

            var point = NearestPointInElement(e.ManipulationOrigin.X + manipulationAreaDeltaX, e.ManipulationOrigin.Y + manipulationAreaDeltaY, AnnotationsCanvas);

            _polyline.Points.Add(point);

            CurrentAnnotationCanvas.Children.Add(_polyline);
        }

        private void AnnotationsCanvas_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            var manipulationAreaDeltaX = ManipulationArea.Margin.Left;
            var manipulationAreaDeltaY = ManipulationArea.Margin.Top;

            var x = e.ManipulationOrigin.X - e.DeltaManipulation.Translation.X + manipulationAreaDeltaX;
            var y = e.ManipulationOrigin.Y - e.DeltaManipulation.Translation.Y + manipulationAreaDeltaY;

            var point = NearestPointInElement(x, y, AnnotationsCanvas);

            _polyline.Points.Add(point);
        }

        private void AnnotationsCanvas_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (_polyline.Points.Count < 2)
            {
                CurrentAnnotationCanvas.Children.Clear();

                _manipulating = false;
            }
            else
            {
                CurrentAnnotationCanvas.Children.RemoveAt(CurrentAnnotationCanvas.Children.Count - 1);

                AnnotationsCanvas.Children.Add(_polyline);

                Model.Saved = false;

                AdaptButtonsToState();

                _manipulating = false;

                AttemptUpdatePreviewAsync();
            }

            _polyline = null;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            _task.Show();
        }

        private void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _photoResult = e;
            }
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            AnnotationsCanvas.Children.RemoveAt(AnnotationsCanvas.Children.Count - 1);

            Model.Saved = false;

            AdaptButtonsToState();

            AttemptUpdatePreviewAsync();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            AnnotationsCanvas.Children.Clear();

            Model.Saved = false;

            AdaptButtonsToState();

            AttemptUpdatePreviewAsync();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (!Processing && !_manipulating && Model.AnnotationsBitmap != null)
            {
                NavigationService.Navigate(new Uri("/Pages/EffectPage.xaml", UriKind.Relative));
            }
        }

        private async void AttemptUpdatePreviewAsync()
        {
            if (!Processing)
            {
                Processing = true;
                
                AdaptButtonsToState();
                
                do
                {
                    _processingPending = false;

                    if (Model.OriginalImage != null && ForegroundAnnotationsDrawn && BackgroundAnnotationsDrawn)
                    {
                        Model.OriginalImage.Position = 0;

                        var maskBitmap = new WriteableBitmap((int)AnnotationsCanvas.ActualWidth, (int)AnnotationsCanvas.ActualHeight);
                        var annotationsBitmap = new WriteableBitmap((int)AnnotationsCanvas.ActualWidth, (int)AnnotationsCanvas.ActualHeight);

                        annotationsBitmap.Render(AnnotationsCanvas, new ScaleTransform
                        {
                            ScaleX = 1,
                            ScaleY = 1
                        });

                        annotationsBitmap.Invalidate();

                        Model.OriginalImage.Position = 0;

                        using (var source = new StreamImageSource(Model.OriginalImage))
                        using (var segmenter = new InteractiveForegroundSegmenter(source))
                        using (var renderer = new WriteableBitmapRenderer(segmenter, maskBitmap))
                        using (var annotationsSource = new BitmapImageSource(annotationsBitmap.AsBitmap()))
                        {
                            var foregroundColor = Model.ForegroundBrush.Color;
                            var backgroundColor = Model.BackgroundBrush.Color;

                            segmenter.ForegroundColor = Windows.UI.Color.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
                            segmenter.BackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
                            segmenter.Quality = 0.25;
                            segmenter.AnnotationsSource = annotationsSource;

                            try
                            {
                            await renderer.RenderAsync();

                            MaskImage.Source = maskBitmap;

                            maskBitmap.Invalidate();

                            Model.AnnotationsBitmap = (Bitmap)annotationsBitmap.AsBitmap();
                        }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("AttemptUpdatePreviewAsync rendering failed: " + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        MaskImage.Source = null;
                    }
                }
                while (_processingPending && !_manipulating);

                Processing = false;

                AdaptButtonsToState();
            }
            else
            {
                _processingPending = true;
            }
        }

        private void ForegroundButton_Click(object sender, RoutedEventArgs e)
        {
            _brush = Model.ForegroundBrush;

            AdaptButtonsToState();
        }

        private void BackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            _brush = Model.BackgroundBrush;

            AdaptButtonsToState();
        }
    }
}