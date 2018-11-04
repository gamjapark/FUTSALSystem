using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Packet_Library;
using MySql.Data.MySqlClient;

namespace soccer_Server
{
    public partial class Form1 : Form
    {
        private TcpListener m_listener = null;                              //서버 작동 리스너
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림

        //버퍼
        private byte[] sendBuf = new byte[1024 * 4];   //보내기 버퍼
        private byte[] readBuf = new byte[1024 * 4];   //읽기 버퍼

        //클라이언트
        static int clientCount = 0;
        private TcpClient m_client = null;
        public List<TcpClient> m_clients = new List<TcpClient>();                               
        public Dictionary<TcpClient, string> clientName = new Dictionary<TcpClient, string>();
        public Dictionary<TcpClient, int> clientNum = new Dictionary<TcpClient, int>();

        public ArrayList nickNameList = new ArrayList(); //???


        //for receive
        public Team_Info m_Team_Info;
        public Student_ID m_Student_ID;
        public Change_Team_Info m_Change_Team_Info;
        public Change_SI_Info m_Change_SI_Info;


        string SqlString = "Server=192.168.123.162;Database=FutsalDb;Uid=soccer_Team;Pwd=KwangWoon726";

        //string SqlString = "Server=172.20.10.13;Database=FutsalDb;Uid=soccer_Team;Pwd=KwangWoon726";

        //string SqlString = "Server=10.10.91.214;Database=FutsalDb;Uid=soccer_Team;Pwd=KwangWoon726";
        //string SqlString = "Server=127.0.0.1;Database=futsaldb;Uid=root;Pwd=hyejinzz03";
        int dateGap = 0;

        public Form1()
        {
            InitializeComponent();

            this.Text = "SoccerServer";

            Thread start_thread = new Thread(ServerPacketStart);      //서버 스레드 설정
            start_thread.IsBackground = true;                   //스레드 백그라운드로 설정
            start_thread.Start();                               //서버 스레드 시작

            printServerStudent();
            printServerMessage();
        }

        private void printServerMessage()
        {
            listView2.Items.Clear();
            MySqlConnection conn = new MySqlConnection(SqlString);
            conn.Open();
            try
            {
                //존재하는 학번들 정보 가져오기
                string str = "SELECT SendID, RecvID, Date, Text FROM MyMsg;";
                MySqlCommand cmd = new MySqlCommand(str, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    // 리스트 뷰에 추가 
                    ListViewItem item = new ListViewItem
                        (new[] { rdr["SendID"].ToString(), rdr["RecvID"].ToString(),
                            rdr["Date"].ToString(), rdr["Text"].ToString() });
                    listView2.Items.Add(item);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void printServerStudent()
        {
            listView1.Items.Clear();
            MySqlConnection conn = new MySqlConnection(SqlString);
            conn.Open();

            // this.label4.Text = clientCount.ToString();
            try
            {
                //존재하는 학번들 정보 가져오기
                string str = "SELECT * FROM MyTeam;";
                MySqlCommand cmd = new MySqlCommand(str, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    List<string> a = new List<string>();
                    a.Add(rdr["Student_ID"].ToString());
                    a.Add(rdr["Tname"].ToString());


                    ListViewItem l = new ListViewItem(a.ToArray());
                    this.listView1.Items.Add(l);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void ServerPacketStart()
        {
            this.m_listener = new TcpListener(Dns.GetHostByName(Dns.GetHostName()).AddressList[0], 8080);
            this.m_listener.Start();
            this.m_client = default(TcpClient);

            while (true)
            {
                try
                {
                    this.m_client = m_listener.AcceptTcpClient();       //클라이언트 받기
                    clientCount++;                                      //클라이언트 수 증가

                    if(label4.Text.Equals("0")) this.clientNum.Add(this.m_client, 1); 
                    else this.clientNum.Add(this.m_client, clientNum.Count + 1);
                    this.clientName.Add(this.m_client, null);
                    this.m_clients.Add(this.m_client);

                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        this.label4.Text = clientCount.ToString();
                    }));



                    Client_Handler c = new Client_Handler();
                    c.Receive += new Client_Handler.Receive_Handler(this.Receive);
                    c.DisConnect += new Client_Handler.DisConnect_Handler(this.Disconnect);
                    c.Client_Start(m_client, clientName, clientNum);
                }
                catch (SocketException s)
                {
                    MessageBox.Show("Socket Exception : " + s.Message);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception : " + e.Message);
                }
            }
        }

        private void Receive(int Type, byte[] readBuf, int ClientCount)
        {
            switch (Type)
            {
                case (int)PacketType.최근결과:
                    {
                        previous_result pre = (previous_result)Packet.Deserialize(readBuf);
                        string name = pre.loginID;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string sql = "SELECT * FROM MyResult WHERE T1name = '" + name + "'OR T2name = '" + name + "' ORDER BY Date DESC;";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            int count = 0;
                            while (rdr.Read())
                            {
                                count++;
                                pre = new previous_result();
                                pre.Type = (int)PacketType.최근결과;
                                pre.myTeam = rdr["T1name"].ToString();
                                pre.opTeam = rdr["T2name"].ToString();
                                pre.date = rdr["Date"].ToString();
                                pre.myScore = int.Parse(rdr["T1Score"].ToString());
                                pre.OPScore = int.Parse(rdr["T2Score"].ToString());
                                Packet.Serialize(pre).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                                if (count == 4)
                                    break;
                            }
                            pre = new previous_result();
                            pre.Type = (int)PacketType.최근결과;
                            pre.myScore = -1;
                            Packet.Serialize(pre).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;

                case (int)PacketType.패스워드변경:
                    {
                        //클라이언트로부터 팀인원수 정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string t_name = this.m_Team_Info.t_name;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            //FJoin Table
                            string str = "UPDATE FJoin SET PW='" + m_Team_Info.pw + "'where Tname='" + t_name + "';";
                            MySqlDataReader rdr = (new MySqlCommand(str, conn)).ExecuteReader();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.팀인원수수정:
                    {
                        //클라이언트로부터 팀인원수 정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string t_name = this.m_Team_Info.t_name;
                        int stCount = this.m_Team_Info.stCount;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            //FJoin Table
                            string str = "UPDATE FJoin SET Tnum='" + stCount + "'where Tname='" + t_name + "';";
                            MySqlDataReader rdr = (new MySqlCommand(str, conn)).ExecuteReader();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            //수정 후 서버 정보 변경 
                            printServerStudent();
                            printServerMessage();
                        }
                    }
                    break;
                case (int)PacketType.학번수정:
                    {
                        //클라이언트로부터 학번 정보 받기 
                        this.m_Change_SI_Info = (Change_SI_Info)Packet.Deserialize(readBuf);
                        string tname = this.m_Change_SI_Info.tName;
                        string old_SI = this.m_Change_SI_Info.old_SI;
                        string new_SI = this.m_Change_SI_Info.new_SI;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            //MyTeam Table
                            if (old_SI.Equals("-1"))
                            {

                                MySqlCommand c = conn.CreateCommand();
                                c.CommandText = "INSERT INTO MyTeam(Tname,Student_ID)VALUES(@Tname,@Student_ID)";
                                c.Parameters.AddWithValue("@Tname", tname);
                                c.Parameters.AddWithValue("@Student_ID", new_SI);
                                c.ExecuteNonQuery();
                            }
                            else
                            {
                                string str = "UPDATE MyTeam SET Student_ID='" + new_SI + "'where Student_ID='" + old_SI + "';";
                                MySqlDataReader rdr = (new MySqlCommand(str, conn)).ExecuteReader();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.팀이름수정:
                    {                        //클라이언트로부터 팀이름 정보 받기 
                        this.m_Change_Team_Info = (Change_Team_Info)Packet.Deserialize(readBuf);
                        string old_tName = this.m_Change_Team_Info.old_tName;
                        string new_tName = this.m_Change_Team_Info.new_tName;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        string str;

                        try
                        {
                            //FJoin Table
                            str = "UPDATE FJoin SET Tname='" + new_tName + "' where Tname='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            //MyMsg Table
                            str = "UPDATE MyMsg SET SendID='" + new_tName + "' where SendID='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            str = "UPDATE MyMsg SET RecvID='" + new_tName + "' where RecvID='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            //MyPage Table
                            str = "UPDATE MyPage SET Tname='" + new_tName + "' where Tname='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            //MyReserve Table
                            str = "UPDATE MyReserve SET Tname='" + new_tName + "' where Tname='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            str = "UPDATE MyReserve SET OPTeam='" + new_tName + "' where OPTeam='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            //MyResult Table
                            str = "UPDATE MyResult SET T1name='" + new_tName + "' where T1name='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            str = "UPDATE MyResult SET T2name='" + new_tName + "' where T2name='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();
                            conn.Open();
                            //MyTeam Table
                            str = "UPDATE MyTeam SET Tname='" + new_tName + "' where Tname='" + old_tName + "';";
                            (new MySqlCommand(str, conn)).ExecuteReader();


                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                for (int i = 0; i < listView3.Items.Count; i++)
                                {
                                    if (listView3.Items[i].SubItems[1].Text.Equals(old_tName))
                                    {
                                        listView3.Items[i].SubItems[1].Text = new_tName;
                                        break;
                                    }

                                }
                            }));
                            this.clientName[m_client] = new_tName;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.학번정보:
                    {   //팀이름 정보 받기
                        this.m_Student_ID = (Student_ID)Packet.Deserialize(readBuf);
                        string tName = this.m_Student_ID.t_name;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string str = "SELECT Student_ID FROM MyTeam where Tname='" + tName + "';";
                            var c = new MySqlCommand(str, conn);
                            MySqlDataReader rdr = c.ExecuteReader();
                            //해당 팀이름에 맞는 학번 보내기 
                            while (rdr.Read())
                            {
                                this.m_Student_ID = new Student_ID();
                                this.m_Student_ID.Type = (int)PacketType.학번정보;
                                this.m_Student_ID.t_name = tName;
                                this.m_Student_ID.s_id = rdr["Student_ID"].ToString();

                                Packet.Serialize(this.m_Student_ID).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.로그인정보확인:
                    {
                        //클라이언트로부터 팀이름 정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string tName = this.m_Team_Info.t_name;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string str = "SELECT PW, Tnum FROM FJoin where Tname='" + tName + "';";
                            var c = new MySqlCommand(str, conn);
                            MySqlDataReader rdr = c.ExecuteReader();

                            while (rdr.Read())
                            {   //패스워드, 팀원수 정보 보내기
                                this.m_Team_Info = new Team_Info();
                                this.m_Team_Info.Type = (int)PacketType.로그인정보확인;
                                this.m_Team_Info.t_name = tName;
                                this.m_Team_Info.stCount = int.Parse(rdr["Tnum"].ToString());
                                this.m_Team_Info.pw = rdr["PW"].ToString();

                                Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;

                case (int)PacketType.로그아웃:
                    {
                        //클라이언트로부터 팀이름 정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string tName = this.m_Team_Info.t_name;

                        for (int i = 0; i < listView3.Items.Count; i++)
                        {
                            if (listView3.Items[i].SubItems[1].Text.Equals(tName))
                            {
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    //최종 로그인 정보는 맞지만, 이미 열린 클라이언트가 있음
                                    listView3.Items[i].SubItems[1].Text = "";

                                }));
                                break;
                            }
                        }

                    }
                    break;
                case (int)PacketType.팀이름확인:
                    {
                        //클라이언트로부터 팀이름 정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string tName = this.m_Team_Info.t_name;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string str = "SELECT Tname FROM FJoin where Tname='" + tName + "';";
                            var c = new MySqlCommand(str, conn);

                            this.m_Team_Info = new Team_Info();
                            this.m_Team_Info.Type = (int)PacketType.팀이름확인;
                            this.m_Team_Info.t_name = tName;

                            if (!(c.ExecuteReader().HasRows))                           //같은 팀이름 없음
                            {
                                this.m_Team_Info.Error = (int)PacketSendERROR.정상;
                            }
                            else                                                        //같은 팀이름 있음
                            {
                                this.m_Team_Info.Error = (int)PacketSendERROR.에러;
                            }

                            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.학번확인:
                    {
                        //클라이언트로부터 학번 정보 받기 
                        this.m_Student_ID = (Student_ID)Packet.Deserialize(readBuf);
                        string sid = this.m_Student_ID.s_id;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {   //학번 확인해서 같은 것이 있으면 오류, 같은 것이 없으면 정상 메시지 보내기
                            string str = "SELECT Student_ID FROM MyTeam where Student_ID='" + sid + "';";
                            var c = new MySqlCommand(str, conn);

                            this.m_Student_ID = new Student_ID();
                            this.m_Student_ID.Type = (int)PacketType.학번확인;
                            this.m_Student_ID.s_id = sid;

                            if (!(c.ExecuteReader().HasRows))                           //같은 학번 없음
                            {
                                this.m_Student_ID.Error = (int)PacketSendERROR.정상;
                            }
                            else                                                        //같은 학번 있음
                            {
                                this.m_Student_ID.Error = (int)PacketSendERROR.에러;
                            }

                            Packet.Serialize(m_Student_ID).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.학번저장:
                    {
                        //클라이언트로부터 학번 정보 받기 
                        this.m_Student_ID = (Student_ID)Packet.Deserialize(readBuf);
                        string tname = this.m_Student_ID.t_name;
                        string sid = this.m_Student_ID.s_id;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            //학번 정보 디비에 넣기
                            MySqlCommand c = conn.CreateCommand();
                            c.CommandText = "INSERT INTO MyTeam(Tname,Student_ID)VALUES(@Tname,@Student_ID)";
                            c.Parameters.AddWithValue("@Tname", tname);
                            c.Parameters.AddWithValue("@Student_ID", sid);
                            c.ExecuteNonQuery();

                            //학번 정보 디비에 넣었는지 한 번 더 확인해서 
                            //같은 학번이 있으면 정상, 같은 학번이 없으면 오류 메시지 보내기
                            string str = "SELECT Student_ID FROM MyTeam where Student_ID='" + sid + "';";
                            var c1 = new MySqlCommand(str, conn);

                            this.m_Student_ID = new Student_ID();
                            this.m_Student_ID.Type = (int)PacketType.학번저장;
                            this.m_Student_ID.s_id = sid;
                            this.m_Student_ID.t_name = tname;

                            if (!(c1.ExecuteReader().HasRows))                           //같은 학번 없으면
                            {
                                this.m_Student_ID.Error = (int)PacketSendERROR.에러;
                            }
                            else                                                         //같은 학번 있음
                            {
                                this.m_Student_ID.Error = (int)PacketSendERROR.정상;
                            }

                            Packet.Serialize(m_Student_ID).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.로그인정보저장:
                    {
                        //클라이언트로부터 팀이름, 패스워드, 팀 인원수정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string tname = this.m_Team_Info.t_name;
                        string pw = this.m_Team_Info.pw;
                        int stCount = this.m_Team_Info.stCount;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            //팀이름, 패스워드, 팀 인원수 정보 디비에 넣기
                            MySqlCommand c = conn.CreateCommand();
                            c.CommandText = "INSERT INTO FJoin(Tname,PW,Tnum)VALUES(@Tname,@PW,@Tnum)";
                            c.Parameters.AddWithValue("@Tname", tname);
                            c.Parameters.AddWithValue("@PW", pw);
                            c.Parameters.AddWithValue("@Tnum", stCount);
                            c.ExecuteNonQuery();

                            c = conn.CreateCommand();
                            c.CommandText = "INSERT INTO MyPage(Tname)VALUES(@Tname)";
                            c.Parameters.AddWithValue("@Tname", tname);
                            c.ExecuteNonQuery();

                            //팀이름 정보 디비에 넣었는지 한 번 더 확인해서 
                            //같은 팀이름이 있으면 정상, 같은 팀이름이 없으면 오류 메시지 보내기
                            string str = "SELECT Tname FROM FJoin where Tname='" + tname + "';";
                            var c1 = new MySqlCommand(str, conn);

                            this.m_Team_Info = new Team_Info();
                            this.m_Team_Info.Type = (int)PacketType.로그인정보저장;
                            this.m_Team_Info.t_name = tname;
                            this.m_Team_Info.pw = pw;
                            this.m_Team_Info.stCount = stCount;

                            if (!(c1.ExecuteReader().HasRows))                           //같은 팀이름 없으면
                            {
                                this.m_Team_Info.Error = (int)PacketSendERROR.에러;
                            }
                            else                                                         //같은 팀이름 있음
                            {
                                this.m_Team_Info.Error = (int)PacketSendERROR.정상;
                            }

                            Packet.Serialize(m_Team_Info).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            //수정 후 서버 정보 변경 
                            printServerStudent();
                        }
                    }
                    break;
                case (int)PacketType.패스워드확인:
                    {
                        //클라이언트로부터 패스워드 정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string tName = this.m_Team_Info.t_name;
                        string pw = this.m_Team_Info.pw;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string str = "SELECT PW FROM FJoin where Tname='" + tName + "';";
                            var c = new MySqlCommand(str, conn);
                            var sqlRead = c.ExecuteReader();
                            this.m_Team_Info = new Team_Info();
                            this.m_Team_Info.Type = (int)PacketType.패스워드확인;

                            if (sqlRead.HasRows)
                            {
                                if (sqlRead.Read())
                                {
                                    if (sqlRead["PW"].ToString() == pw)      //패스워드 일치      
                                    {
                                        bool b = false;
                                        for( int i = 0; i <listView3.Items.Count; i++)
                                        {
                                            if (listView3.Items[i].SubItems[1].Text.Equals(tName)){
                                                //최종 로그인 정보는 맞지만, 이미 열린 클라이언트가 있음
                                                this.m_Team_Info.Error = (int)PacketSendERROR.존재;
                                                b = true;
                                                break;
                                            }
                                        }
                                       
                                        if (!b)
                                        {
                                            this.m_Team_Info.Error = (int)PacketSendERROR.정상;
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                bool k = true;
                                                for (int i = 0; i < listView3.Items.Count; i++)
                                                {
                                                    if (listView3.Items[i].SubItems[0].Text.Equals(clientNum[m_client].ToString()))
                                                    {
                                                        listView3.Items[i].SubItems[1].Text = tName;
                                                        k = false;
                                                        break;
                                                    }
                                                }
                                                if (k)
                                                {
                                                    String[] a = new String[2];
                                                    a[0] = clientNum[m_client].ToString();
                                                    a[1] = tName;
                                                    listView3.Items.Add(new ListViewItem(a));
                                                }

                                            }));
                                            this.clientName[m_client] = tName;
                                        }

                                    }
                                    else                                    //패스워드 불일치
                                    {
                                        this.m_Team_Info.Error = (int)PacketSendERROR.에러;
                                    }
                                }
                            }

                            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.팀이름리스트:
                    {
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            MySqlCommand cmd = new MySqlCommand();
                            cmd.Connection = conn;
                            cmd.CommandText = "SELECT Tname FROM FJoin;";
                            DataSet ds = new DataSet();
                            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT Tname FROM FJoin", conn);
                            adapter.Fill(ds);

                            team_list list;
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                list = new team_list();
                                list.Type = (int)PacketType.팀이름리스트;
                                list.t_name = row["Tname"].ToString();
                                list.count = 0;
                                Packet.Serialize(list).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }
                            list = new team_list();
                            list.Type = (int)PacketType.팀이름리스트;
                            list.count = -1;
                            Packet.Serialize(list).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.메시지송신:
                    {
                        // 디비에 메시지 정보 저장 
                        Msg_list send = (Msg_list)Packet.Deserialize(readBuf);

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            MySqlCommand c = conn.CreateCommand();
                            c.CommandText = "INSERT INTO MyMsg(SendID, RecvID, Date, Text, Used)VALUES(@SendID,@RecvID,@Date,@Text, @Used)";
                            c.Parameters.AddWithValue("@SendID", send.send_name);
                            c.Parameters.AddWithValue("@RecvID", send.recv_name);
                            c.Parameters.AddWithValue("@Date", send.date);
                            c.Parameters.AddWithValue("@Text", send.text);
                            c.Parameters.AddWithValue("@Used", 0);
                            c.ExecuteNonQuery();

                            // 리스트 뷰에 추가 
                            ListViewItem item = new ListViewItem(new[] { send.send_name, send.recv_name, send.date, send.text });
                            listView2.Items.Add(item);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.송신리스트:
                    {
                        Msg_list send = (Msg_list)Packet.Deserialize(readBuf);

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에서 메시지 정보 가져오기 
                            string sql = "SELECT * FROM MyMsg WHERE SendID = '" + send.recv_name + "';";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            while (rdr.Read())
                            {
                                send = new Msg_list();
                                send.Type = (int)PacketType.송신리스트;
                                send.send_name = rdr["RecvID"].ToString();
                                send.date = rdr["Date"].ToString();
                                send.text = rdr["Text"].ToString();
                                send.count = 0;
                                send.used = int.Parse(rdr["Used"].ToString());

                                Packet.Serialize(send).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }

                            send = new Msg_list();
                            send.Type = (int)PacketType.송신리스트;
                            send.count = -1;
                            Packet.Serialize(send).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.수신리스트:
                    {
                        Msg_list send = (Msg_list)Packet.Deserialize(readBuf);

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT * FROM MyMsg WHERE RecvID = '" + send.recv_name + "';";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            while (rdr.Read())
                            {
                                send = new Msg_list();
                                send.Type = (int)PacketType.수신리스트;
                                send.send_name = rdr["SendID"].ToString();
                                send.date = rdr["Date"].ToString();
                                send.text = rdr["Text"].ToString();
                                send.used = int.Parse(rdr["Used"].ToString());
                                send.count = 0;
                                Packet.Serialize(send).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }
                            send = new Msg_list();
                            send.Type = (int)PacketType.수신리스트;
                            send.count = -1;
                            Packet.Serialize(send).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.수신확인:
                    {
                        Msg_list msg = (Msg_list)Packet.Deserialize(readBuf);

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "UPDATE MyMsg SET Used = 1 WHERE RecvID = '" + msg.recv_name + "'AND SendID = '" + msg.send_name + "'AND Text = '" + msg.text + "'AND Date = '" + msg.date + "';";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.메시지개수:
                    {
                        Msg_list msg = (Msg_list)Packet.Deserialize(readBuf);

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT COUNT(*) FROM MyMsg WHERE RecvID = '" + msg.recv_name + "' AND Used = 0;";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();


                            if (rdr.Read())
                            {
                                msg = new Msg_list();
                                msg.Type = (int)PacketType.메시지개수;
                                msg.count = int.Parse(rdr["COUNT(*)"].ToString());
                                Packet.Serialize(msg).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
                case (int)PacketType.팀인원수:
                    {
                        //클라이언트로부터 패스워드 정보 받기 
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(readBuf);
                        string tName = this.m_Team_Info.t_name;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string str = "SELECT COUNT(*) FROM MyTeam where Tname='" + tName + "';";
                            var c = new MySqlCommand(str, conn);
                            var sqlRead = c.ExecuteReader();
                            this.m_Team_Info = new Team_Info();
                            this.m_Team_Info.Type = (int)PacketType.팀인원수;

                            if (sqlRead.HasRows)
                            {
                                if (sqlRead.Read())
                                {
                                    this.m_Team_Info.stCount = int.Parse(sqlRead["COUNT(*)"].ToString());     //패스워드 일치      
                                }
                            }

                            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                            m_networkstream = m_client.GetStream();
                            this.Send();

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.혼자:
                    {
                        Reserve reserve = (Reserve)Packet.Deserialize(readBuf);
                        string tname = reserve.send_name;
                        string date = reserve.date;
                        int time = reserve.time;
                        int match_num = reserve.match_num;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT * FROM MyReserve WHERE Date ="+"'"+date+"' AND Time ="+time+";";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            if (rdr.Read())
                            {
                                string cur = rdr["Tname"].ToString();
                                string match = rdr["OPTeam"].ToString();
                                int num = int.Parse(rdr["Match_num"].ToString());

                                if(num == 2 && match == null || cur == null)
                                {
                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.혼자;
                                    reserve.used = 2; 
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                } else
                                {
                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.혼자;
                                    reserve.used = 1;
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                }
                            } else
                            {
                                conn.Close();
                                conn = new MySqlConnection(SqlString);
                                conn.Open();
                                MySqlCommand c = conn.CreateCommand();
                                c.CommandText = "INSERT INTO MyReserve(Tname, Date, Time, Match_num)values(@Tname, @Date, @Time, @Match_num)";
                                c.Parameters.AddWithValue("@Tname", tname);
                                c.Parameters.AddWithValue("@Date", date);
                                c.Parameters.AddWithValue("@Time", time);
                                c.Parameters.AddWithValue("@Match_num", match_num);
                                c.ExecuteNonQuery();

                                reserve = new Reserve();
                                reserve.Type = (int)PacketType.혼자;
                                reserve.used = 0;
                                Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();

                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.매칭:
                    {
                        Reserve reserve = (Reserve)Packet.Deserialize(readBuf);
                        string tname = reserve.send_name;
                        string date = reserve.date;
                        int time = reserve.time;
                        int match_num = reserve.match_num;
                        int tnum = reserve.tnum;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT * FROM MyReserve WHERE Date =" + "'" + date + "' AND Time =" + time + ";";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            if (rdr.Read())
                            {
                                string cur = rdr["Tname"].ToString();
                                string match = rdr["OPTeam"].ToString();
                                int num = int.Parse(rdr["Match_num"].ToString());
                                int tcount = int.Parse(rdr["Tnum"].ToString());
                                if(tcount != tnum)
                                {
                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.매칭;
                                    reserve.used = tcount;
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                } 
                                else if (num == 2 && (match == "" || cur == ""))
                                {
                                    if(match != "")
                                    {
                                        cur = tname;
                                        conn.Close();
                                        conn = new MySqlConnection(SqlString);
                                        conn.Open();
                                        //MySqlCommand c = conn.CreateCommand();
                                        sql = "UPDATE MyReserve SET Tname = '"+tname+ "' WHERE Date = '" + date + "'AND Time = " + time + ";";
                                        cmd = new MySqlCommand(sql, conn);
                                        rdr = cmd.ExecuteReader();
                                    } else
                                    {
                                        match = tname;
                                        conn.Close();
                                        conn = new MySqlConnection(SqlString);
                                        conn.Open();
                                        //MySqlCommand c = conn.CreateCommand();
                                        sql = "UPDATE MyReserve SET OPTeam = '" + tname + "' WHERE Date = '" + date + "'AND Time = " + time + ";";
                                        cmd = new MySqlCommand(sql, conn);
                                        rdr = cmd.ExecuteReader();
                                    }

                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.매칭;
                                    reserve.used = 6;
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                }
                                else
                                {
                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.매칭;
                                    reserve.used = 1;
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                }
                            }
                            else
                            {
                                conn.Close();
                                conn = new MySqlConnection(SqlString);
                                conn.Open();
                                MySqlCommand c = conn.CreateCommand();
                                c.CommandText = "INSERT INTO MyReserve(Tname, Date, Time, Match_num, Tnum, pre_result)values(@Tname, @Date, @Time, @Match_num, @Tnum, 1)";
                                c.Parameters.AddWithValue("@Tname", tname);
                                c.Parameters.AddWithValue("@Date", date);
                                c.Parameters.AddWithValue("@Time", time);
                                c.Parameters.AddWithValue("@Match_num", match_num);
                                c.Parameters.AddWithValue("@Tnum", tnum);
                                c.ExecuteNonQuery();

                                reserve = new Reserve();
                                reserve.Type = (int)PacketType.매칭;
                                reserve.used = 0;
                                Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.예약정보:
                    {
                        Reserve reserve = (Reserve)Packet.Deserialize(readBuf);
                        string date = reserve.date;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT * FROM MyReserve WHERE Date ='" +date+ "';";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            while (rdr.Read())
                            {
                                string cur = rdr["Tname"].ToString();
                                string match = rdr["OPTeam"].ToString();
                                int num = int.Parse(rdr["Match_num"].ToString());
                                int time = int.Parse(rdr["Time"].ToString());

                                if(num == 2 && (cur == "" || match == "")) { // 매칭 & 둘중 하나 비어있음 
                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.예약정보;
                                    reserve.send_name = cur;
                                    reserve.time = time;
                                    reserve.used = 2;
                                    reserve.tnum = int.Parse(rdr["Tnum"].ToString());
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                }
                                else if(num == 1) { // 혼자 사용 
                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.예약정보;
                                    reserve.time = time;
                                    reserve.used = 1;
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                } else
                                {
                                    reserve = new Reserve();
                                    reserve.Type = (int)PacketType.예약정보;
                                    reserve.time = time;
                                    reserve.used = 0;
                                    reserve.tnum = int.Parse(rdr["Tnum"].ToString());
                                    reserve.recv_name = match;
                                    reserve.send_name = cur;
                                    Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                    this.m_networkstream = m_client.GetStream();
                                    this.Send();
                                }                           
                            }
                            reserve = new Reserve();
                            reserve.Type = (int)PacketType.예약정보;
                            reserve.used = -1;
                            Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.예약가능:
                    {
                        Reserve reserve = (Reserve)Packet.Deserialize(readBuf);
                        string date = reserve.date;
                        string tname = reserve.send_name;
                    
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT COUNT(*) FROM MyReserve WHERE Tname = '"+ tname +"' AND Date ='" + date + "';";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            if (rdr.Read())
                            {
                                reserve = new Reserve();
                                reserve.Type = (int)PacketType.예약가능;
                                reserve.used = int.Parse(rdr["COUNT(*)"].ToString());
                                Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.예약내역:
                    {
                        Reserve reserve = (Reserve)Packet.Deserialize(readBuf);
                        string tname = reserve.send_name;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT * FROM MyReserve WHERE Tname = '" + tname + "' OR OPTeam = '" + tname + "' ORDER BY Date DESC;";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            int count = 0;
                            while (rdr.Read())
                            {
                                reserve = new Reserve();
                                reserve.Type = (int)PacketType.예약내역;
                                reserve.date = rdr["Date"].ToString();
                                reserve.time = int.Parse(rdr["Time"].ToString());
                                reserve.used = 1;
                                Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                                count++;
                                if (count == 4) break;
                            }
                            reserve = new Reserve();
                            reserve.Type = (int)PacketType.예약내역;
                            reserve.used = -1;
                            Packet.Serialize(reserve).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.예약취소:
                    {
                        Reserve reserve = (Reserve)Packet.Deserialize(readBuf);
                        string tname = reserve.send_name;
                        string mdate = reserve.date;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "SELECT * FROM MyReserve WHERE Date = '" + mdate + "' AND Tname = '" + tname + "' OR OPTeam ='"+tname +"';";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            if (rdr.Read())
                            {
                                int match_num = int.Parse(rdr["Match_num"].ToString());
                                string Tname = rdr["Tname"].ToString();
                                string opteam = rdr["OPTeam"].ToString();

                                int tnum = 0;
                                if (match_num == 2)
                                    tnum = int.Parse(rdr["Tnum"].ToString());
                                int time = int.Parse(rdr["Time"].ToString());

                                string inset;
                                conn.Close();
                                conn = new MySqlConnection(SqlString);
                                conn.Open();
                                sql = "DELETE FROM MyReserve WHERE Date = '" + mdate + "' AND Tname = '" + tname + "' OR OPTeam ='" + tname + "';";
                                cmd = new MySqlCommand(sql, conn);
                                rdr = cmd.ExecuteReader();
                                if (match_num == 1 || Tname=="" || opteam =="")
                                {
                                    ;
                                } else
                                {
                                    if(tname == Tname)
                                    {
                                        inset = opteam;

                                    } else
                                    {
                                        inset = Tname;
                                    }

                                    conn.Close();
                                    conn = new MySqlConnection(SqlString);
                                    conn.Open();
                                    MySqlCommand c = conn.CreateCommand();
                                    c.CommandText = "INSERT INTO MyReserve(Tname, Date, Time, Match_num, Tnum, pre_result)values(@Tname, @Date, @Time, @Match_num, @Tnum, 1)";
                                    c.Parameters.AddWithValue("@Tname", inset);
                                    c.Parameters.AddWithValue("@Date", mdate);
                                    c.Parameters.AddWithValue("@Time", time);
                                    c.Parameters.AddWithValue("@Match_num", match_num);
                                    c.Parameters.AddWithValue("@Tnum", tnum);
                                    c.ExecuteNonQuery();
                                }
                                
                            } 
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.전적:
                    {
                        List lst = (List)Packet.Deserialize(readBuf);

                        int w_check = lst.where_check;
                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 디비에 메시지 정보 저장 
                            string sql = "";
                            if (w_check == 0)
                            {
                                string tname = lst.send_name;
                                sql = "SELECT * FROM MyPage WHERE Tname = '" + tname + "';";
                            }
                            else sql = "SELECT * FROM MyPage ORDER BY Win DESC, Draw DESC, Lose ASC;";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            int i = 1;
                            while (rdr.Read())
                            {

                                lst = new List();
                                lst.Type = (int)PacketType.전적;
                                lst.where_check = w_check;
                                lst.send_name = rdr["Tname"].ToString();
                                lst.win = int.Parse(rdr["Win"].ToString());
                                lst.draw = int.Parse(rdr["Draw"].ToString());
                                lst.lose = int.Parse(rdr["Lose"].ToString());

                                Packet.Serialize(lst).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                                ++i;
                                if (i == 11) break;
                            }
                            if (w_check == 1)
                            {
                                lst = new List();
                                lst.Type = (int)PacketType.전적;
                                lst.where_check = -1;
                                Packet.Serialize(lst).CopyTo(this.sendBuf, 0);
                                this.m_networkstream = m_client.GetStream();
                                this.Send();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }

                    }
                    break;
                case (int)PacketType.이전경기결과확인:
                    {
                        previous_result pre = (previous_result)Packet.Deserialize(readBuf);

                        string myName = pre.myTeam;
                        string curDate = pre.curDate;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            // 결과 안적은 경기 있는지 판단
                            string sql = "SELECT pre_result, Date FROM MyReserve WHERE Tname = '" + myName + "' OR OPTeam = '" + myName + "';";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            pre = new previous_result();
                            pre.Type = (int)PacketType.이전경기결과확인;

                            bool t = true;
                            while (rdr.Read())
                            {
                                string reserDate = rdr["Date"].ToString();
                                int comResult = compareDate(curDate, reserDate);
                                if (int.Parse(rdr["pre_result"].ToString()) == 1 && comResult == 1)
                                {
                                    pre.pre_result = 1;
                                    t = false;
                                    break;
                                }
                            }

                            if(t) pre.pre_result = 0;

                            Packet.Serialize(pre).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;

                case (int)PacketType.이전경기결과:
                    {
                        previous_result pre = (previous_result)Packet.Deserialize(readBuf);

                        string myName = pre.myTeam;
                        string today_date = pre.date;
                        

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string sql = "SELECT * FROM MyReserve WHERE (Tname = '" + myName + "' OR OPTeam = '" + myName + "') and pre_result = 1;";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            pre = new previous_result();
                            pre.Type = (int)PacketType.이전경기결과;

                            bool t = true;
                            while (rdr.Read())
                            {
                                string a = rdr["Date"].ToString();

                                int  bbb = DateTime.Compare(Convert.ToDateTime(today_date), Convert.ToDateTime(a));
                                if (bbb > 0)
                                {
                                    pre.myTeam = rdr["Tname"].ToString();
                                    pre.opTeam = rdr["OPTeam"].ToString();
                                    pre.time = int.Parse(rdr["Time"].ToString());
                                    pre.date = a;
                                    pre.pre_result = 1;
                                    t = false;
                                    break;
                                }
                            }

                            if (t) pre.pre_result = 0;
                            Packet.Serialize(pre).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();


                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;

                case (int)PacketType.이전경기결과제출:
                    {
                        previous_result pre = (previous_result)Packet.Deserialize(readBuf);
                       // pre = new previous_result();
                       // pre.Type = (int)PacketType.이전경기결과제출;
                        string myName = pre.myTeam;
                        string OPName = pre.opTeam;
                        int myScore = pre.myScore;
                        int OPScore = pre.OPScore;
                        string result = "";
                        string date = pre.date;
                        int time = pre.time;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {

                            if (myName == pre.loginID)              //tname
                            {
                                if (myScore > OPScore)
                                {
                                    result = "승";
                                    string str1 = "UPDATE MyPage SET Win=Win+1 where Tname = '" + myName + "';";
                                    MySqlDataReader rdr1 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();

                                    conn.Open();
                                    string str2 = "UPDATE MyPage SET Lose=Lose+1 where Tname = '" + OPName + "';";
                                    MySqlDataReader rdr2 = (new MySqlCommand(str2, conn)).ExecuteReader();
                                    conn.Close();
                                }
                                else if (myScore < OPScore)
                                {
                                    result = "패";
                                    string str1 = "UPDATE MyPage SET Win=Win+1 where Tname = '" + OPName + "';";
                                    MySqlDataReader rdr1 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();

                                    conn.Open();
                                    string str2 = "UPDATE MyPage SET Lose=Lose+1 where Tname = '" + myName + "';";
                                    MySqlDataReader rdr2 = (new MySqlCommand(str2, conn)).ExecuteReader();
                                    conn.Close();
                                }
                                else
                                {
                                    result = "무";
                                    string str1 = "UPDATE MyPage SET Draw=Draw+1 where Tname = '" + OPName + "';";
                                    MySqlDataReader rdr1 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();

                                    conn.Open();
                                    string str2 = "UPDATE MyPage SET Draw=Draw+1 where Tname = '" + myName + "';";
                                    MySqlDataReader rdr2 = (new MySqlCommand(str2, conn)).ExecuteReader();
                                    conn.Close();
                                }


                            }
                            else                                    //OPteam
                            {
                                if (myScore < OPScore)
                                {
                                    result = "승";
                                    string str1 = "UPDATE MyPage SET Win=Win+1 where Tname = '" + OPName + "';";
                                    MySqlDataReader rdr1 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();

                                    conn.Open();
                                    string str2 = "UPDATE MyPage SET Lose=Lose+1 where Tname = '" + myName + "';";
                                    MySqlDataReader rdr2 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();
                                }
                                else if (myScore > OPScore)
                                {
                                    result = "패";
                                    string str1 = "UPDATE MyPage SET Win=Win+1 where Tname = '" + myName + "';";
                                    MySqlDataReader rdr1 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();

                                    conn.Open();
                                    string str2 = "UPDATE MyPage SET Lose=Lose+1 where Tname = '" + OPName + "';";
                                    MySqlDataReader rdr2 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();
                                }
                                else
                                {
                                    result = "무";
                                    string str1 = "UPDATE MyPage SET Draw=Draw+1 where Tname = '" + OPName + "';";
                                    MySqlDataReader rdr1 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();

                                    conn.Open();
                                    string str2 = "UPDATE MyPage SET Draw=Draw+1 where Tname = '" + myName + "';";
                                    MySqlDataReader rdr2 = (new MySqlCommand(str1, conn)).ExecuteReader();
                                    conn.Close();
                                }
                            }



                            conn.Open();
                            string str = "UPDATE MyReserve SET pre_result = " + 0 + " where (Tname = '" + myName + "' OR OPTeam = '" + myName + "') and pre_result = 1 and Time = " + time + " and Date = '" + date + "';";
                            MySqlDataReader rdr = (new MySqlCommand(str, conn)).ExecuteReader();
                            conn.Close();


                            conn.Open();
                            MySqlCommand c = conn.CreateCommand();
                            c.CommandText = "INSERT INTO MyResult(T1name,T2name,T1Score,T2Score,Result,Date,Time)VALUES(@T1name,@T2name,@T1Score,@T2Score,@Result,@Date,@time)";
                            c.Parameters.AddWithValue("@T1name", myName);
                            c.Parameters.AddWithValue("@T2name", OPName);
                            c.Parameters.AddWithValue("@T1Score", myScore);
                            c.Parameters.AddWithValue("@T2Score", OPScore);
                            c.Parameters.AddWithValue("@Result", result);
                            c.Parameters.AddWithValue("@Date", date);
                            c.Parameters.AddWithValue("@Time", time);
                            c.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;

                case (int)PacketType.불발:
                    {
                        delete_match del = (delete_match)Packet.Deserialize(readBuf);
                        string cur = del.curDate;

                        MySqlConnection conn = new MySqlConnection(SqlString);
                        conn.Open();
                        try
                        {
                            string sql = "SELECT Date, Match_num FROM MyReserve WHERE OPTeam is null;";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            while (rdr.Read())
                            {
                                string reserDate = rdr["Date"].ToString();
                                compareDate(cur, reserDate);
                                if (dateGap >= 1)   // 현재보다 이전
                                {
                                    if(int.Parse(rdr["Match_num"].ToString()) == 2) // 매칭 못함
                                    {
                                        MySqlConnection conn1 = new MySqlConnection(SqlString);
                                        conn1.Open();
                                        string sql2 = "DELETE FROM MyReserve WHERE Date = '" + reserDate + "' AND OPTeam is null and pre_result = " + 1 + ";";
                                        MySqlCommand cmd2 = new MySqlCommand(sql2, conn1);
                                        MySqlDataReader rdr2 = cmd2.ExecuteReader();
                                        conn1.Close();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                        }
                    }
                    break;
            }
        }
        private void Disconnect(TcpClient m_client)
        {
           
          //  MessageBox.Show(clientName[m_client] + " client Disconnected");
            for (int i = 0; i < listView3.Items.Count; i++) // 전달받은 아이디가 리스트에 있는지 확인 
            {
                if (listView3.Items[i].SubItems[0].Text.Equals(clientNum[m_client].ToString()))
                {

                    listView3.Items.RemoveAt(i);
                    break;
                }
            }

            this.Invoke(new MethodInvoker(delegate ()
            {
                label4.Text = (--clientCount).ToString();
            }));

        }

        class Client_Handler
        {
            private TcpClient m_client = null;
            private NetworkStream m_networkstream = null;
            public Dictionary<TcpClient, string> clientName = new Dictionary<TcpClient, string>();
            public Dictionary<TcpClient, int> clientNum = new Dictionary<TcpClient, int>();

            //버퍼
            private byte[] sendBuf = new byte[1024 * 4];   //보내기 버퍼
            private byte[] readBuf = new byte[1024 * 4];   //읽기 버퍼

            //메시지 받기
            public delegate void Receive_Handler(int Type, byte[] readBuf, int ClientCount);
            public event Receive_Handler Receive;

            //연결 끊기
            public delegate void DisConnect_Handler(TcpClient m_client);
            public event DisConnect_Handler DisConnect;

            public void Client_Start(TcpClient m_client, 
                Dictionary<TcpClient, string> clientName, 
                Dictionary<TcpClient, int> clientNum)
            {
                this.m_client = m_client;
                this.clientName = clientName;
                this.clientNum = clientNum;

                Thread start_thread = new Thread(ClientPacketStart);        //클라이언트 스레드 설정
                start_thread.IsBackground = true;                           //스레드 백그라운드로 설정
                start_thread.Start();                                       //클라이언트 스레드 시작
            }

            private void ClientPacketStart()
            {
                try
                {
                    int MessageCount = 0;
                    while (true)
                    {
                        MessageCount++;
                        try
                        {
                            m_networkstream = m_client.GetStream();
                            m_networkstream.Read(this.readBuf, 0, this.readBuf.Length);
                            Packet p = (Packet)Packet.Deserialize(this.readBuf);
                            if (Receive != null) Receive(p.Type, this.readBuf, clientNum[m_client]);
                        }
                        catch (Exception e)
                        {
                            if (m_client != null)
                            {
                                if (DisConnect != null)
                                {
                                    DisConnect(m_client);
                                    DisConnect = null;
                                }
                                m_client.Close();
                                m_networkstream.Close();
                            }
                          //  MessageBox.Show(e.Message);
                        }
                    }


                }
                catch (SocketException s)
                {
                    //MessageBox.Show("Socket Exception : " + s.Message);

                    if(m_client != null)
                    {
                        if (DisConnect != null) DisConnect(m_client);
                        m_client.Close();
                        m_networkstream.Close();
                    }
                }
                catch (Exception e)
                {
                   // MessageBox.Show("Exception : " + e.Message);
                    if (m_client != null)
                    {
                        if (DisConnect != null) DisConnect(m_client);   
                        m_client.Close();
                        m_networkstream.Close();
                    }
                }
            }
        }

        //보내기
        public void Send()
        {
            this.m_networkstream.Write(this.sendBuf, 0, this.sendBuf.Length);
            this.m_networkstream.Flush();

            for (int i = 0; i < 1024 * 4; i++)
            {
                this.sendBuf[i] = 0;
            }
        }

        public int compareDate(string d1, string d2)
        {
            DateTime cur = Convert.ToDateTime(d1);
            DateTime rdate = Convert.ToDateTime(d2);

            int cmp = DateTime.Compare(cur, rdate);

            if (cmp>0)  // 현재보다 이전
            {
                dateGap = 10;
                return 1;
            }
            else
            {
                    dateGap = 0;
                    return 0;
            }
        }

    }
}
