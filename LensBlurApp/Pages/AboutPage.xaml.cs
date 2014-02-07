/*
 * Copyright (c) 2014 Nokia Corporation. All rights reserved.
 *
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 *
 * See the license text file for license information.
 */

using LensBlurApp.Resources;
using Microsoft.Phone.Controls;
using System;
using System.Windows.Documents;
using System.Xml.Linq;

namespace LensBlurApp.Pages
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            // Application version number

            var version = XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;

            var versionRun = new Run()
            {
                Text = String.Format(AppResources.AboutPage_VersionText, version) + "\n"
            };

            VersionParagraph.Inlines.Add(versionRun);

            // Application about text

            var aboutRun = new Run()
            {
                Text = AppResources.AboutPage_AboutText + "\n"
            };

            AboutParagraph.Inlines.Add(aboutRun);

            // Application guide text

            var guideRun = new Run()
            {
                Text = AppResources.AboutPage_GuideText + "\n"
            };

            GuideParagraph.Inlines.Add(guideRun);
        }
    }
}