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

            // Link to photo attributions


            var attributionsRunText = AppResources.AboutPage_AttributionsRun_Text;
            var attributionsRunTextSpans = attributionsRunText.Split(new string[] { "{0}" }, StringSplitOptions.None);

            var attributionsRunSpan1 = new Run();
            attributionsRunSpan1.Text = attributionsRunTextSpans[0];

            var attributionsLink = new Hyperlink();
            attributionsLink.Inlines.Add(AppResources.AboutPage_Hyperlink_Attributions_Text);
            attributionsLink.Click += AttributionsLink_Click;

            var attributionsRunSpan2 = new Run();
            attributionsRunSpan2.Text = attributionsRunTextSpans[1] + "\n";

            AttributionsParagraph.Inlines.Add(attributionsRunSpan1);
            AttributionsParagraph.Inlines.Add(attributionsLink);
            AttributionsParagraph.Inlines.Add(attributionsRunSpan2);
        }

        private void ProjectLink_Click(object sender, RoutedEventArgs e)
        {
            var webBrowserTask = new WebBrowserTask()
            {
                Uri = new Uri(AppResources.AboutPage_Hyperlink_Project_Url, UriKind.Absolute)
            };

            webBrowserTask.Show();
        }

        private void AttributionsLink_Click(object sender, RoutedEventArgs e)
        {
            var webBrowserTask = new WebBrowserTask()
            {
                Uri = new Uri(AppResources.AboutPage_Hyperlink_Attributions_Url, UriKind.Absolute)
            };

            webBrowserTask.Show();
        }
    }
}