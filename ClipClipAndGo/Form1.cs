using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace ClipClipAndGo
{
    public partial class Form1 : Form
    {
        private string clipText = string.Empty;

        public bool isFirstKey = false;

        public Form1()
        {
            InitializeComponent();

            listBox1.Font = Properties.Settings.Default.ActionListFont;
            listBox2.Font = Properties.Settings.Default.HistoryListFont;

            // load actions from configuration file
            ////  write here for debug
            List<Action> actions = new List<Action>();

            LoadActionConfig(actions);

            // draw actions in list box
            foreach (var action in actions)
            {
                listBox1.Items.Add(action);
            }
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += ListBox1_DrawItem;

            listBox2.DrawMode = DrawMode.OwnerDrawFixed;
            listBox2.DrawItem += ListBox2_DrawItem;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            listBox1.Width = this.Width / 2;
            listBox2.Width = this.Width / 2;
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            //this.Hide();


        }

        private void LoadActionConfig(List<Action> actions)
        {
            string fileName = "ActionConfig.xml";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            XmlSerializer serializer = new XmlSerializer(typeof(List<ActionConfig>), new XmlRootAttribute("Actions"));

            try
            {

                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    List<ActionConfig> configs = (List<ActionConfig>)serializer.Deserialize(reader);
                    foreach (var action in configs)
                    {
                        actions.Add(new Action(action.title, action.action, action.args, action.shortcutkey, action.icon_path));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Got Exception: " + ex.Message);
                Application.Exit();
            }

        }
        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                Action action = (Action)listBox1.Items[e.Index];

                // 画像の読み込み
                Image image = action.GetImage();

                // ListBoxの高さに併せて画像をリサイズする
                using (Graphics graphics = CreateGraphics()) // Graphicsオブジェクトを作成
                {
                    int heightInPixels = (int)Math.Ceiling(e.Font.GetHeight(graphics)); // フォントの高さをピクセル単位で取得
                    image = new Bitmap(image, heightInPixels - 1, heightInPixels - 1); // -1 は単なるマージン。気分。
                }

                // Itemの高さ = Fontの高さ
                ((ListBox)sender).ItemHeight = ((ListBox)sender).Font.Height;

                // 画像の描画
                e.Graphics.DrawImage(image, e.Bounds.Left, e.Bounds.Top);

                // テキストの描画
                string text = action.ToString();
                e.Graphics.DrawString(text, e.Font, Brushes.Black, e.Bounds.Left + image.Width, e.Bounds.Top);

                //罫線
                e.Graphics.DrawLine(Pens.Black, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

                e.DrawFocusRectangle();
            }
        }

        private void ListBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                // Itemの高さ
                ((ListBox)sender).ItemHeight = ((ListBox)sender).Font.Height;

                string itemText = ((ListBox)sender).GetItemText(((ListBox)sender).Items[e.Index]);

                // trim and replace new line to space
                itemText = itemText.Trim();
                itemText = Regex.Replace(itemText, @"\r?\n", " ");

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black, e.Bounds.Left, e.Bounds.Top);

                //罫線
                e.Graphics.DrawLine(Pens.Black, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

                e.DrawFocusRectangle();
            }
        }

        private void KickSelectedAction()
        {
            if (listBox1.SelectedIndex != -1)
            {
                Action action = listBox1.SelectedItem as Action;
                if (action != null)
                {
                    action.DoAction(clipText);
                }
            }
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (isFirstKey)
            {
                isFirstKey = false;
                listBox1.Select();
                listBox1.SelectedIndex = 0;
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (((ListBox)sender).SelectedIndex != -1)
                    {
                        if (sender == listBox1)
                        {
                            KickSelectedAction();
                        }
                        else
                        {
                            Clipboard.SetText(listBox2.Items[listBox2.SelectedIndex].ToString());
                        }
                        this.Hide();
                    }
                    break;
                case Keys.Escape:
                    this.Hide();
                    break;
                case Keys.Left:
                    if (sender == listBox2)
                    {
                        listBox2.SelectedIndex = -1;
                        listBox1.Select();
                        listBox1.SelectedIndex = 0;
                    }
                    break;
                case Keys.Up:
                    if (((ListBox)sender).SelectedIndex > 0)
                    {
                        ((ListBox)sender).SelectedIndex--;
                    }
                    break;
                case Keys.Right:
                    if (sender == listBox1)
                    {
                        listBox1.SelectedIndex = -1;
                        listBox2.Select();
                        listBox2.SelectedIndex = 0;
                    }
                    break;
                case Keys.Down:
                    if (((ListBox)sender).SelectedIndex < ((ListBox)sender).Items.Count - 1)
                    {
                        ((ListBox)sender).SelectedIndex++;
                    }
                    break;
                    /*
                case Keys.C:

                    break; */
                default:
                    var index = listBox1.FindString(e.KeyCode.ToString());
                    if (index != ListBox.NoMatches)
                    {

                        Action action = listBox1.Items[index] as Action;
                        action.DoAction(clipText);
                        this.Hide();
                    }
                    break;
            }
            /*
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
            }
            else if ((e.KeyCode == Keys.Enter) && listBox1.SelectedIndex != -1)
            {
                KickSelectedAction();
                this.Hide();
            }
            else if ((e.KeyCode == Keys.Right) && sender == listBox1)
            {
                listBox2.Select();
            }
            else if ((e.KeyCode == Keys.Left) && sender == listBox2)
            {
                listBox1.Select();
            }
            else
            {
                var index = listBox1.FindString(e.KeyCode.ToString());
                if (index != ListBox.NoMatches)
                {
                    Action action = listBox1.Items[index] as Action;
                    action.DoAction(clipText);
                    this.Hide();
                }
            }
            */
        }



        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            ListBox listBox = (ListBox)sender;
            int index = listBox.IndexFromPoint(e.Location);
            if (index >= 0 && index < listBox.Items.Count)
            {
                if (listBox.Name == "listBox1")
                {
                    KickSelectedAction();
                    this.Hide();
                }
                else if (listBox.Name == "listBox2")
                {
                    Clipboard.SetText(listBox.Items[listBox.SelectedIndex].ToString());
                    this.Hide();
                }
                
            }
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            int index = listBox.IndexFromPoint(listBox.PointToClient(Cursor.Position));
            if (index >= 0 && index < listBox.Items.Count)
            {
                listBox.SelectedIndex = index;
            }
        }

        private void ListBox_MouseLeave(object sender, EventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            listBox.SelectedIndex = -1;
        }


        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {

            //this.StartPosition = FormStartPosition.CenterScreen;
            this.CenterToScreen();

            if (Clipboard.ContainsText())
            {
                if (clipText == Clipboard.GetText())
                {
                    return;
                }
                clipText = Clipboard.GetText();

                listBox2.Items.Insert(0, clipText);

                if (listBox2.Items.Count > 10)
                {
                    listBox2.Items.RemoveAt(listBox2.Items.Count - 1);
                }
            }
        }

        private void ListBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;
            listBox2.SelectedIndex = listBox2.IndexFromPoint(listBox2.PointToClient(p));
            string temp = listBox2.SelectedItem.ToString();
            MessageBox.Show("got it" + temp);
        }

        private void listBox2_MouseUp(object sender, MouseEventArgs e)
        {

        }
    }
}
