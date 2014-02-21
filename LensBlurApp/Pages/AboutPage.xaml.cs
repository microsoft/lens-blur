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
using Microsoft.Phone.Tasks;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

namespace LensBlurApp.Pages
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            // Application version number

            var xElement = XDocument.Load("WMAppManifest.xml").Root;
            
            if (xElement != null)
            {
                var element = xElement.Element("App");
                if (element != null)
                {
                    var xAttribute = element.Attribute("Version");
                    if (xAttribute != null)
                    {
                        var version = xAttribute.Value;

                        var versionRun = new Run
                        {
                            Text = String.Format(AppResources.AboutPage_VersionText, version) + "\n"
                        };

                        VersionParagraph.Inlines.Add(versionRun);
                    }
                }
            }

            // Application about text

            var aboutRun = new Run
            {
                Text = AppResources.AboutPage_AboutText + "\n"
            };

            AboutParagraph.Inlines.Add(aboutRun);

            // Application disclaimer text

            var disclaimerRun = new Run
            {
                Text = AppResources.AboutPage_DisclaimerText + "\n"
            };

            DisclaimerParagraph.Inlines.Add(disclaimerRun);

            // Link to project website

            var projectRunText = AppResources.AboutPage_ProjectRun_Text;
            var projectRunTextSpans = projectRunText.Split(new string[] { "{0}" }, StringSplitOptions.None);

            var projectRunSpan1 = new Run();
            projectRunSpan1.Text = projectRunTextSpans[0];

            var projectLink = new Hyperlink();
            projectLink.Inlines.Add(AppResources.AboutPage_Hyperlink_Project_Text);
            projectLink.Click += ProjectLink_Click;

            var projectRunSpan2 = new Run();
            projectRunSpan2.Text = projectRunTextSpans[1] + "\n";

            ProjectParagraph.Inlines.Add(projectRunSpan1);
            ProjectParagraph.Inlines.Add(projectLink);
            ProjectParagraph.Inlines.Add(projectRunSpan2);
        }

        private void ProjectLink_Click(object sender, RoutedEventArgs e)
        {
            var webBrowserTask = new WebBrowserTask()
            {
                Uri = new Uri(AppResources.AboutPage_Hyperlink_Project_Url, UriKind.Absolute)
            };

            webBrowserTask.Show();
        }
    }
}