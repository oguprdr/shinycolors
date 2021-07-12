using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace shinycolors_client
{
    public partial class Form1 : Form
    {
        private Size meterSize = new Size(220, 22);
        private Size windowSize = new Size(872, 492);
        private Bitmap bitmap = new Bitmap(220, 22);

        private int appeal = 0;
        private bool autoMeter = false, initialLoad = false, searching = false;

        public string applicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\shinycolors";
        public int delay1 = 5, delay2 = 10;

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public Form1()
        {
            InitializeComponent();
            InitializeWebView();
            this.Text = "アイドルマスター　シャイニーカラーズ";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = this.windowSize;
            webView1.Location = new System.Drawing.Point(0, 0);
            webView1.Size = this.ClientSize;


            button1.Text = "自動ゲージ OFF";

            label1.Text = "";
        }

        private async void InitializeWebView()
        {
            var enviroment = await CoreWebView2Environment.CreateAsync(null, applicationPath);
            await webView1.EnsureCoreWebView2Async(enviroment);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            webView1.Size = this.ClientSize;
        }

        private async void webView1_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webView1.CoreWebView2.Navigate("https://shinycolors.enza.fun/");

            if (!initialLoad)
            {
                await webView1.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                    document.addEventListener('mouseup',function(event){
                        window.chrome.webview.postMessage(event.screenX+','+event.screenY);
                    });
                ");
                initialLoad = true;
            }
        }

        private void webView1_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (autoMeter&&!searching)
            {
                this._Click();
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            autoMeter = autoMeter ? false : true;
            if (autoMeter)
            {
                button1.Text = "自動ゲージ ON";
                label1.Text = "自動ゲージ ON";
            }
            else
            {
                button1.Text = "自動ゲージ OFF";
                label1.Text = "自動ゲージ OFF";
                appeal = 0;
                searching = false;
            }
        }

        private void _Click()
        {
            var point = PointToClient(Cursor.Position);


            if (point.X > 30 && point.X < 160 && point.Y > 60 && point.Y < 360)
            {
                if (appeal == 0)label1.Text = "アピール未確定   X:"+point.X+" Y:"+point.Y+" APPEAL"+appeal;
                else
                {
                    label1.Text = "審査員選択";
                    LoopW();
                }
            }
            else if (point.Y > 350 && point.Y < 492)
            {
                if (point.X > 215 && point.X < 360)
                {
                    appeal = appeal == 1 ? 0: 1;
                }
                else if (point.X > 361 && point.X < 505)
                {
                    appeal = appeal == 2 ? 0 : 2;
                }
                else if (point.X > 506 && point.X < 651)
                {
                    appeal = appeal == 3 ? 0 : 3;
                }

                if (appeal == 0)
                {
                    label1.Text = "アピール未選択   X:" + point.X + " Y:" + point.Y;
                }
                else
                {
                    label1.Text = "アピール　" + appeal + " 選択中";
                }
            }
        }

        private async void LoopW()
        {
            await Task.Delay(315);
            searching = true;
            int i = FindPerfectPoint();

            if (i == 0)
            {
                label1.Text = "失敗";

                appeal = 0;
                searching = false;
                return;
            }

            else if (i == 193 || i == 194 || i == 171 || i == 172) i -= delay1;
            else if (i == 167 || i == 168 || i == 147 || i == 148) i -= delay2;

            while (appeal != 0)
            {
                await Task.Delay(1);
                Loop(i);
            }
        }

        private int FindPerfectPoint()
        {
            var meterPoint = PointToScreen(new Point(370, 310));
            Graphics.FromImage(bitmap).CopyFromScreen(meterPoint, new Point(0,0), meterSize);
            for (int i = 0; i < meterSize.Width; i++)
            {
                var color = bitmap.GetPixel(i, 3);
                if (color.R > 250 && color.G > 250 && color.B > 250)
                {
                    return i;
                }
            }

            return 0;
        }

        private void Loop(int perfectPoint)
        {
            var meterPoint = PointToScreen(new Point(370, 310));
            Graphics.FromImage(bitmap).CopyFromScreen(meterPoint, new Point(0, 0), meterSize);
            var color = bitmap.GetPixel(perfectPoint, 20);

            if (color.R > 200 && color.G > 200 && color.B > 105)
            {
                label1.Text = "座標 " + perfectPoint + "でクリック";

                _MouseClick();
                appeal = 0;
                searching = false;
            }
            else if(appeal != 0)
            {
                if (!label1.Text.Equals("パーフェクト待機中"))
                {
                    label1.Text = "パーフェクト待機中";
                }
            }
        }

        private void _MouseClick()
        {
            var point = PointToScreen(new Point(436, 240));
            SetCursorPos(point.X, point.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}
