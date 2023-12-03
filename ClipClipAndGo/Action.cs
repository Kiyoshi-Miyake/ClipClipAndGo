using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Drawing;

namespace ClipClipAndGo
{
    public class ActionConfig
    {
        public string title { get; set; }
        public string action { get; set; }
        public string args { get; set; }
        public string shortcutkey { get; set; }
        public string icon_path { get; set; }
    }

    internal class Action
    {
        private string _title;
        private string _action;
        private string _args;
        private string _shortcutkey;
        private Image _image;


        public Action(string title, string action, string args, string shortcutkey, string icon_path)
        {
            _title = title;
            _action = action;
            _args = args;
            _shortcutkey = shortcutkey;

            _image = GetImageFromIconPath(icon_path);
        }
/*        public Action(string title, string action)
        {
            _title = title;
            _action = action;
        }*/


        public override string ToString()
        {
            return _shortcutkey.ToUpper() + ": " +_title;
        }

        public void DoAction(string content)
        {
            try
            {
                if (Regex.IsMatch(_action, @"^http"))
                {
                    string url = string.Format(_action, Uri.EscapeDataString(content));
                    Process.Start(url);
                } 
                else if (string.IsNullOrEmpty(_args))
                {
                    Process.Start(_action);
                }
                else
                {
                    ProcessStartInfo info = new ProcessStartInfo(_action);
                    info.Arguments = string.Format(_args, content);
                    Process.Start(info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Got Exception when perfoming action: {_action} : {ex.Message}");
            }
        }

        public Image GetImage()
        {
            return _image;
        }


        private Image GetImageFromIconPath(string path)
        {
            Image image = null;

            try
            {
                if (Regex.IsMatch(path, @"^http", RegexOptions.IgnoreCase))
                {
                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(path);
                        using (MemoryStream stream = new MemoryStream(imageData))
                        {
                            if (Regex.IsMatch(path, @"\.ico$", RegexOptions.IgnoreCase))
                            {
                                Icon icon = new Icon(stream);
                                image = icon.ToBitmap();
                            }
                            else
                            {
                                image = Image.FromStream(stream);
                            }
                        }
                    }
                }
                else if (Regex.IsMatch(path, @"\.exe$", RegexOptions.IgnoreCase))
                {
                    Icon icon = Icon.ExtractAssociatedIcon(path);
                    image = icon.ToBitmap();
                    icon.Dispose();
                }
                else
                {
                    image = Image.FromFile(path);
                }
            }
            catch
            {
            }

            if (image == null)
            {
                Bitmap transparentImage = new Bitmap(1, 1);
                transparentImage.MakeTransparent();
                image = transparentImage;
            }

            //resize image
            return image;
        }

    }
}
