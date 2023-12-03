using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipClipAndGo
{
    internal static class Program
    {
        // Global Key Hook
        static LowLevelKeyboardHook kbh;
        static bool lctrlKeyPressed;
        static bool cKeyPressed;

        static Form1 mainForm;
        static DateTime last_time = DateTime.MinValue;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CreateNotifyIcon();

            // Global Key Hook
            kbh = new LowLevelKeyboardHook();
            kbh.OnKeyPressed += kbh_OnKeyPressed;
            kbh.OnKeyUnpressed += kbh_OnKeyUnpressed;
            kbh.HookKeyboard();
            
            mainForm = new Form1();
            mainForm.Icon = new System.Drawing.Icon("Clip.ico");

            //mainForm.Visible = false;
            //mainForm.WindowState = FormWindowState.Minimized;
            //mainForm.Show();
            //mainForm.WindowState = FormWindowState.Normal;
            //mainForm.Visible = true;


            Application.Run();

            kbh.UnHookKeyboard();
        }

        // Show/Activate the mainForm by double-ctrl+C using low level keyboard hook(global hook)
        static private void kbh_OnKeyPressed(object sender, Keys e)
        {
            if (e == Keys.LControlKey)
            {
                lctrlKeyPressed = true;
            }
            else if (e == Keys.C)
            {
                cKeyPressed = true;
            }

            if (lctrlKeyPressed && cKeyPressed)
            {
                DateTime now = DateTime.Now;
                if ((now - last_time).TotalMilliseconds < 500)
                {
                    mainForm.StartPosition = FormStartPosition.CenterScreen;
                    mainForm.TopMost = true;
                    mainForm.Show();
                    mainForm.Activate();
                    mainForm.TopMost = false;
                    mainForm.isFirstKey = true;
                }
                else
                {
                    last_time = now;
                }
            }
        }

        static private void kbh_OnKeyUnpressed(object sender, Keys e)
        {
            if (e == Keys.LControlKey)
            {
                lctrlKeyPressed = false;
            }
            else if (e == Keys.C)
            {
                cKeyPressed = false;
            }
        }

        private static void CreateNotifyIcon()
        {
            NotifyIcon icon = new NotifyIcon();
            icon.Icon = new System.Drawing.Icon("Clip.ico");
            icon.ContextMenuStrip = ContextMenu();
            icon.Text = "Clip2Go";
            icon.Visible = true;
        }

        private static ContextMenuStrip ContextMenu()
        {
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add("Exit Appication", null, (s, e) =>
            {
                Application.Exit();
            });
            return cms;
        }

        // ref:
        // https://www.carifred.com/quick_any2ico/
        // https://web-dev.hatenablog.com/entry/csharp/notify-icon

    }
}
