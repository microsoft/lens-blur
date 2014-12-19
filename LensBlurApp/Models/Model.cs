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

using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using System.IO;
using System.Windows.Media;

namespace LensBlurApp.Models
{
    public class Model
    {
        // TODO: Tombstoning support

        private static Stream _originalImageStream;
        private static Bitmap _annotationsBitmap;

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
    }
}
