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

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;

namespace LensBlurApp.Pages
{
    public class Photo
    {
        public StorageFile File { get; private set; }

        public BitmapImage Thumbnail
        {
            get
            {
                var width = 226.0 * (Application.Current.Host.Content.ScaleFactor / 100.0);

                return new BitmapImage(new Uri("/Assets/Photos/" + File.Name, UriKind.Relative)) { DecodePixelWidth = (int)width };
            }
        }

        public Photo(StorageFile file)
        {
            File = file;
        }
    }

    public class GalleryPageViewModel
    {
        public Photo Photo1 { get; private set; }
        public Photo Photo2 { get; private set; }
        public Photo Photo3 { get; private set; }
        public Photo Photo4 { get; private set; }
        public Photo Photo5 { get; private set; }
        public Photo Photo6 { get; private set; }
        public Photo Photo7 { get; private set; }
        public Photo Photo8 { get; private set; }
        public Photo Photo9 { get; private set; }
        public Photo Photo10 { get; private set; }
        public Photo Photo11 { get; private set; }
        public Photo Photo12 { get; private set; }
        public Photo Photo13 { get; private set; }
        public Photo Photo14 { get; private set; }

        public async Task Initialize()
        {
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync("Assets");
            folder = await folder.GetFolderAsync("Photos");

            Photo1 = new Photo(await folder.GetFileAsync("6882423442_b5bb97ff4d_o.jpg"));
            Photo2 = new Photo(await folder.GetFileAsync("6837287796_7712fe11e7_o.jpg"));
            Photo3 = new Photo(await folder.GetFileAsync("7539149542_14e5b69513_o.jpg"));
            Photo4 = new Photo(await folder.GetFileAsync("7940420418_b3012fd8b4_o.jpg"));
            Photo5 = new Photo(await folder.GetFileAsync("buildings-160061_1280.png"));
            Photo6 = new Photo(await folder.GetFileAsync("climber-4048_1920.jpg"));
            Photo7 = new Photo(await folder.GetFileAsync("delicate-4625_1920.jpg"));
            Photo8 = new Photo(await folder.GetFileAsync("lovebirds-4234_1920.jpg"));
            Photo9 = new Photo(await folder.GetFileAsync("usa-123384_1920.jpg"));
            Photo10 = new Photo(await folder.GetFileAsync("statue-of-liberty-216109_1920.jpg"));
            Photo11 = new Photo(await folder.GetFileAsync("coffee-56340_1920.jpg"));
            Photo12 = new Photo(await folder.GetFileAsync("mount-50920_1920.jpg"));
            Photo13 = new Photo(await folder.GetFileAsync("stop-207329_1920.jpg"));
            Photo14 = new Photo(await folder.GetFileAsync("matterhorn-113331_1920.jpg"));
        }
    }
}
