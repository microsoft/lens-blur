/*
 * Copyright (c) 2014 Nokia Corporation. All rights reserved.
 *
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 *
 * See the license text file for license information.
 */

using Nokia.Graphics.Imaging;
using System.IO;
using System.Windows.Media;

namespace LensBlurApp.Models
{
    public class Model
    {
        // TODO: Tombstoning support

        private static Stream _originalImageStream = null;
        private static Bitmap _annotationsBitmap = null;

        public static readonly SolidColorBrush ForegroundBrush = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush BackgroundBrush = new SolidColorBrush(Colors.Blue);

        public static Stream OriginalImage
        {
            get
            {
                return _originalImageStream;
            }

            set
            {
                if (_originalImageStream != value)
                {
                    if (_originalImageStream != null)
                    {
                        _originalImageStream.Close();
                    }

                    _originalImageStream = value;
                }
            }
        }

        public static Bitmap AnnotationsBitmap
        {
            get
            {
                return _annotationsBitmap;
            }

            set
            {
                if (_annotationsBitmap != value)
                {
                    if (_annotationsBitmap != null)
                    {
                        _annotationsBitmap.Dispose();
                    }

                    _annotationsBitmap = value;
                }
            }
        }

        public static LensBlurPredefinedKernelShape KernelShape { get; set; }
        public static double KernelSize { get; set; }
        public static bool Saved { get; set; }
        public static bool CursorEnabled { get; set; }
    }
}
