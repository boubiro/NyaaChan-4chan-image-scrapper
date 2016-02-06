using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace NyaaChan
{
    public partial class MainWindow : Window
    {
        public static Regex CHECK_URI_REGEX = new Regex(".*?(boards\\.4chan\\.org)(\\/)(\\w+)(\\/)(thread)(\\/)(\\d+)(\\/)?(\\s+)?",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static string DEFAULT_DIR = @"C:\Users\" + Environment.UserName + @"\Pictures";
        public static string DEFAULT_IMAGE = "http://i.4cdn.org/";
        public static string DEFAULT_THUMB = "http://t.4cdn.org/";
        public static string DEFAULT_TITLE = "にゃあちゃん";

        public MainWindow()
        {
            InitializeComponent();

            txtBoardName.IsEnabled = true;
            txtThreadName.IsEnabled = true;
            txtFolderName.IsEnabled = false;

            chkCreateBoardFolder.Checked += chkCreateBoardFolder_Checked;
            chkCreateThreadFolder.Checked += chkCreateThreadFolder_Checked;
            chkCreateBoardFolder.Unchecked += chkCreateBoardFolder_Unchecked;
            chkCreateThreadFolder.Unchecked += chkCreateThreadFolder_Unchecked;

            chkCreateBoardFolder.IsChecked = true;
            chkCreateThreadFolder.IsChecked = true;

            this.Title = "にゃあちゃん";
            txtTitle.Text = DEFAULT_TITLE;
            txtFolderName.Text = DEFAULT_DIR;

            titleBar.MouseLeftButtonDown += (o, e) => DragMove();
            btnClose.Click += btnClose_Click;
        }

        public void DownloadImage(string url, bool newBoardFolder, bool newThreadFolder, bool downloadThumbnails, string boardName, string threadName, string location)
        {
            if (CHECK_URI_REGEX.IsMatch(url))
            {
                if (url.Contains("#"))
                    url = url.Substring(0, url.Trim().LastIndexOf('#'));
                else
                    url = url + ".json";
                if (url.Contains("http://"))
                    url = url.Replace("http://", "");
                string[] tempArr = url.Split('/');
                if (tempArr.Length > 4)
                    url = url.Substring(0, url.LastIndexOf('/')) + ".json";
                url = "http://" + url;

                try
                {
                    string json = "";
                    string imageLink = "";
                    Regex r = new Regex(@"\.+");
                    Uri uri = new Uri(url);

                    WebRequest wRequest = (HttpWebRequest)WebRequest.Create(uri);
                    WebResponse wResponse = (HttpWebResponse)wRequest.GetResponse();
                    using (StreamReader reader = new StreamReader(wResponse.GetResponseStream()))
                        json = reader.ReadToEnd();
                    RootObject rObj = JsonConvert.DeserializeObject<RootObject>(json);
                    string locationString = location + "\\";

                    if (newBoardFolder)
                        if (boardName == "")
                            locationString = locationString + tempArr[1] + "\\";
                        else
                            locationString = locationString + boardName + "\\";

                    if (newThreadFolder)
                        if (threadName == "")
                            locationString = locationString + tempArr[3].Replace(".json", "") + "\\";
                        else
                            locationString = locationString + threadName + "\\";

                    DirectoryInfo dInfo = Directory.CreateDirectory(locationString);

                    DirectoryInfo thumbInfo;
                    if (downloadThumbnails)
                        thumbInfo = Directory.CreateDirectory(locationString + "Thumbnails");
                    string thumbnailLink = "";
                    foreach (Post p in rObj.posts)
                        if (p.ext != null)
                        {
                            thumbnailLink = r.Replace(DEFAULT_THUMB + tempArr[1] + "/" + p.tim + "s.jpg", ".");
                            imageLink = r.Replace(DEFAULT_IMAGE + tempArr[1] + "/" + p.tim + p.ext, ".");
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile(imageLink, locationString + p.tim + p.ext);
                                if (downloadThumbnails)
                                    client.DownloadFile(thumbnailLink, locationString + "Thumbnails\\" + p.tim + ".jpg");
                            }
                        }
                }
                catch (Exception)
                {
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            string url = txtURL.Text.Trim();
            bool newBoardFolder = (bool)chkCreateBoardFolder.IsChecked;
            bool newThreadFolder = (bool)chkCreateThreadFolder.IsChecked;
            bool downloadThumbnails = (bool)chkThumbnail.IsChecked;
            string boardName = txtBoardName.Text;
            string threadName = txtThreadName.Text;
            string location = txtFolderName.Text;
            if (txtFolderName.Text.Trim() == "")
                location = DEFAULT_DIR;
            Thread t = new Thread(() => DownloadImage(url, newBoardFolder, newThreadFolder, downloadThumbnails, boardName, threadName, location));
            t.Start();
        }

        private void btnLocation_Click(object sender, RoutedEventArgs e)
        {
            string result;
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                DialogResult dResult = fbd.ShowDialog();
                result = fbd.SelectedPath;
            }

            txtFolderName.Text = result;
        }

        private void chkCreateBoardFolder_Checked(object sender, RoutedEventArgs e)
        {
            txtBoardName.IsEnabled = true;
        }

        private void chkCreateBoardFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBoardName.IsEnabled = false;
        }

        private void chkCreateThreadFolder_Checked(object sender, RoutedEventArgs e)
        {
            txtThreadName.IsEnabled = true;
        }

        private void chkCreateThreadFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            txtThreadName.IsEnabled = false;
        }

        private void titleBar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}