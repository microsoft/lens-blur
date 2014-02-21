Lens Blur
=========

Lens Blur is an example application on how to use Nokia Imaging SDK
InteractiveForegroundSegmenter and the LensBlurEffect to blur only selected areas of images.

This example application is hosted in GitHub:
https://github.com/nokia-developer/lens-blur/

Developed with Microsoft Visual Studio Express 2012 for Windows Phone.

Compatible with:

 * Windows Phone 8

Tested to work on:

 * Nokia Lumia 520
 * Nokia Lumia 1020
 * Nokia Lumia 1520


Instructions
------------

Make sure you have the following installed:

 * Windows 8
 * Visual Studio Express 2012 for Windows Phone
 * Nuget 2.7 or later

To build and run the sample in emulator

1. Open the SLN file:
   File > Open Project, select the solution (.sln postfix) file
2. Select the target 'Emulator' and platform 'x86'.
3. Press F5 to build the project and run it.


If the project does not compile on the first attempt it's possible that you
did not have the required packages yet. With Nuget 2.7 or later the missing
packages are fetched automatically when build process is invoked, so try
building again. If some packages cannot be found there should be an
error stating this in the Output panel in Visual Studio Express.

For more information on deploying and testing applications see:
http://msdn.microsoft.com/library/windowsphone/develop/ff402565(v=vs.105).aspx


About the implementation
------------------------

| Folder | Description |
| ------ | ----------- |
| / | Contains the project file, the license information and this file (README.md) |
| LensBlurApp | Root folder for the implementation files.  |
| LensBlurApp/Assets | Graphic assets like icons and tiles. |
| LensBlurApp/Models | Application models. |
| LensBlurApp/Pages | Application pages. |
| LensBlurApp/Resources | Localized resources. |
| LensBlurApp/Properties | Application property files. |

Important classes:

| Class | Description |
| ----- | ----------- |
| Pages.EffectPage | Contains code for applying the blur effect. |
| Pages.SegmenterPage | Contains code for the foreground/background segmentation. |


Known issues
------------

 * Application tombstoning is not supported.
 * Saving high resolution photos may run out of memory on low-memory devices.


License
-------

See the license text file delivered with this project:
https://github.com/nokia-developer/lens-blur/blob/master/License.txt


Downloads
---------

| Project | Release | Download |
| ------- | --------| -------- |
| Lens Blur | v1.0 | [lens-blur-1.0.zip](https://github.com/nokia-developer/lens-blur/archive/v1.0.zip) |


Version history
---------------

 * 1.0.0.0: First public release of Lens Blur
