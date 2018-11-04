using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Packet_Library;
using System.IO;
using System.Net.Sockets;
using System.Net;
namespace soccerForm
{
    public partial class Form1 : Form
    {
        TcpClient clientSocket = new TcpClient();
        NetworkStream stream = default(NetworkStream);
        private byte[] sendBuffer = new byte[1024 * 4];
        private byte[] readBuffer = new byte[1024 * 4];

        DateTimePicker picker;

        public int sel_num;
        public int match_num;
        public TcpClient SetPacket() { return clientSocket; }
        //IPAddress ip = IPAddress.Parse("10.10.84.249");
        IPAddress ip = IPAddress.Parse(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString()); // 접속할 아이피 주소로 바꾸기 
        int port = 8080;
        public string loginID() { return ID_txtBox.Text; }
        public Form1 getForm() { return this; }
        Team_Info m_Team_Info;
        previous_result m_pre_result;
        delete_match m_delete;


        public void Send()
        {
            this.stream.Write(this.sendBuffer, 0, this.sendBuffer.Length);
            this.stream.Flush();

            for (int i = 0; i < 1024 * 4; i++)
            {
                this.sendBuffer[i] = 0;
            }
        }

        public void Recv()
        {
            try
            {
                this.stream.Read(readBuffer, 0, 1024 * 4);
            }
            catch
            {
                this.stream = null;
            }

            Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
        }
        public void unVisible()
        {
            panel1.Visible = false;         // home panel unvisible

            btn_reser.BackgroundImage = Image.FromFile("unReservation.png");
            btn_team.BackgroundImage = Image.FromFile("unTeamMatch.png");
            btn_leader.BackgroundImage = Image.FromFile("unLeaderboard.png");
            btn_my.BackgroundImage = Image.FromFile("unMyPage.png");

            pn_reser.Visible = false;
            pn_team.Visible = false;
            pn_leader.Visible = false;
            pn_my.Visible = false;

            // picker.Visible = false;
        }

        public Form1()
        {
            InitializeComponent();

            this.Text = "SoccerClient";

            panel1.BorderStyle = BorderStyle.FixedSingle;

            Label l = new Label();
            l.Text = "Kwangwoon";
            l.Font = new Font("Arial", 20, FontStyle.Bold);
            l.Size = new Size(200, 30);
            l.Left = (this.Width - l.Width) / 2;
            l.Top = 30;
            panel1.Controls.Add(l);

            l = new Label();
            l.Text = "FUTSAL SYSTEM";
            l.Font = new Font("Arial", 35, FontStyle.Bold);
            l.Size = new Size(450, 50);
            l.Left = (this.Width - l.Width) / 2;
            l.Top = 60;
            panel1.Controls.Add(l);

            picMsg.BackColor = Color.Transparent;
            // msg_count.BackColor = Color.Transparent;
            picMsg.Parent = pan_msg;
            //msg_count.Parent = picMsg;
            msg_count.Visible = false;
            picMsg.Visible = false;

            PW_txtBox.BorderStyle = BorderStyle.None;
            ID_txtBox.BorderStyle = BorderStyle.None;

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawLine(new Pen(Color.Green, 10), 10, 135, this.Width - 30, 135);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //피커 설정
            picker = new DateTimePicker();
            //피서 위치, 사이즈 설정
            picker.Location = new Point(260, 50);
            picker.Size = new Size(170, 28);
            //피커 날짜 제한 설정
            DateTime curDate = DateTime.Today.AddDays(1);
            picker.MinDate = new DateTime(curDate.Year, curDate.Month, curDate.Day);
            DateTime newdate = DateTime.Today.AddDays(7);
            picker.MaxDate = new DateTime(newdate.Year, newdate.Month, newdate.Day);

            picker.ValueChanged += new EventHandler(change_picker);
            unVisible();
            panel1.Visible = true;
            pn_menu.Visible = false;
            pn_sideMenu.Visible = false;
            Controls.Add(pn_sideMenu);

            try
            {
                this.clientSocket = new TcpClient();
                clientSocket.Connect(ip, port);
                stream = this.clientSocket.GetStream();
            }
            catch
            {
                this.Close();
                return;
            }

            // 현재 날짜 기준 이전 날짜에 매칭 안돼있으면 삭제
            this.m_delete = new delete_match();
            this.m_delete.Type = (int)PacketType.불발;
            this.m_delete.curDate = DateTime.Today.ToShortDateString(); ;

            Packet.Serialize(this.m_delete).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();
        }
        private void all_initial()
        {
            for (int i = 1; i < 14; i++)
            {
                string btn = "btn_res" + i.ToString();
                string btn1 = btn + "_1";

                Control[] con = this.Controls.Find(btn, true);
                PictureBox pic = (PictureBox)con[0];
                pic.Image = Image.FromFile("btn_green.png");

                con = this.Controls.Find(btn1, true);
                pic = (PictureBox)con[0];
                pic.Image = null;
            }
        }
        private void change_picker(object sender, EventArgs e)
        {
            res_inform();
        }

        private void pre_result(int n)
        {
            // 현재 날짜 기준 이전경기 결과 존재 확인
            this.m_pre_result = new previous_result();
            this.m_pre_result.Type = (int)PacketType.이전경기결과확인;
            this.m_pre_result.myTeam = ID_txtBox.Text;
            this.m_pre_result.curDate = DateTime.Today.ToShortDateString();

            Packet.Serialize(this.m_pre_result).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();

            this.Recv();
            Packet packet3 = (Packet)Packet.Deserialize(this.readBuffer);

            if ((int)packet3.Type == (int)PacketType.이전경기결과확인)
            {
                this.m_pre_result = (previous_result)Packet.Deserialize(this.readBuffer);
                if ((int)m_pre_result.pre_result == 1)
                {
                    Result myResult = new Result(this);

                    if (myResult.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("Completed!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.None);       //성공 메시지 띄우기
                    }
                }
                else if (n == -1)
                {
                    MessageBox.Show("입력하실 경기 결과가 없습니다.");
                }
            }
        }

        private void Login_btn_Click(object sender, EventArgs e)
        {
            if (ID_txtBox.Text != "")
            {
                if (PW_txtBox.Text != "")
                {
                    //팀이름 정보 서버에 보내기
                    this.m_Team_Info = new Team_Info();
                    this.m_Team_Info.Type = (int)PacketType.팀이름확인;
                    this.m_Team_Info.t_name = ID_txtBox.Text;

                    Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuffer, 0);
                    this.stream = clientSocket.GetStream();
                    this.Send();

                    //(정상인지 오류인지) 해당 팀 이름이 이미 사용중인지 아닌지 정보 받기
                    this.Recv();
                    Packet packet = (Packet)Packet.Deserialize(this.readBuffer);

                    if ((int)packet.Type == (int)PacketType.팀이름확인)
                    {
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuffer);
                        if ((int)m_Team_Info.Error == (int)PacketSendERROR.에러)     //이미 존재하는 팀이름
                        {
                            //패스워드 정보 서버에 보내기
                            this.m_Team_Info = new Team_Info();
                            this.m_Team_Info.Type = (int)PacketType.패스워드확인;
                            this.m_Team_Info.t_name = ID_txtBox.Text;
                            this.m_Team_Info.pw = PW_txtBox.Text;

                            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuffer, 0);
                            this.stream = clientSocket.GetStream();
                            this.Send();

                            //(정상인지 오류인지) 해당 패스워드가 맞는지 확인하는 정보 받기
                            this.Recv();
                            Packet packet2 = (Packet)Packet.Deserialize(this.readBuffer);
                            if ((int)packet2.Type == (int)PacketType.패스워드확인)
                            {
                                this.m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuffer);
                                if ((int)m_Team_Info.Error == (int)PacketSendERROR.정상)     //패스워드정상
                                {
                                    // 현재 날짜 기준 이전경기 결과 존재 확인
                                    this.m_pre_result = new previous_result();
                                    this.m_pre_result.Type = (int)PacketType.이전경기결과확인;
                                    this.m_pre_result.myTeam = ID_txtBox.Text;
                                    this.m_pre_result.curDate = DateTime.Today.ToShortDateString();

                                    Packet.Serialize(this.m_pre_result).CopyTo(this.sendBuffer, 0);
                                    this.stream = clientSocket.GetStream();
                                    this.Send();

                                    this.Recv();
                                    Packet packet3 = (Packet)Packet.Deserialize(this.readBuffer);

                                    if ((int)packet3.Type == (int)PacketType.이전경기결과확인)
                                    {
                                        this.m_pre_result = (previous_result)Packet.Deserialize(this.readBuffer);
                                        if ((int)m_pre_result.pre_result == 1)
                                        {
                                            Result myResult = new Result(this);

                                            if (myResult.ShowDialog() == DialogResult.OK)
                                            {
                                                MessageBox.Show("Completed!", "Success",
                                                    MessageBoxButtons.OK, MessageBoxIcon.None);       //성공 메시지 띄우기
                                            }
                                        }
                                    }

                                    pn_reser.Controls.Add(picker);
                                    unVisible();
                                    res_inform();
                                    pn_menu.Visible = true;
                                    pn_sideMenu.Visible = true;
                                    update_msg_panel();
                                    pn_reser.Visible = true;
                                    btn_reser.BackgroundImage = Image.FromFile("onReservation.png");
                                    pn_sideMenu.BringToFront();     // sideMenu Panel 맨 앞으로   
                                    this.Text = ID_txtBox.Text;
                                }
                                else if ((int)m_Team_Info.Error == (int)PacketSendERROR.존재)
                                {
                                    MessageBox.Show("Existing Client!", "Failed",
                                        MessageBoxButtons.OK, MessageBoxIcon.None);         //에러 메시지 띄우기
                                }
                                else if ((int)m_Team_Info.Error == (int)PacketSendERROR.에러)
                                {
                                    MessageBox.Show("Password Incorrect!", "Failed",
                                        MessageBoxButtons.OK, MessageBoxIcon.None);         //에러 메시지 띄우기
                                }

                            }
                        }
                        else if ((int)m_Team_Info.Error == (int)PacketSendERROR.정상)//존재하지 않음
                        {
                            MessageBox.Show("Team Name Incorrect!\n If you don't have your Team Name, Just Please Join, First!", "Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.None);         //에러 메시지 띄우기
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Input your Password!", "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
                }
            }
            else
            {
                MessageBox.Show("Input your Team Name!", "Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
            }
        }
        private void Join_btn_Click(object sender, EventArgs e)
        {
            Form_Join mChild = new Form_Join(this);
            if (mChild.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Completed Join!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.None);       //성공 메시지 띄우기
            }
        }

        private void res_inform()
        {
            all_initial();
            team_initial();
            Reserve reservation = new Reserve();
            reservation.Type = (int)PacketType.예약정보;
            reservation.date = picker.Value.ToShortDateString();
            Packet.Serialize(reservation).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();

            while (true)
            {
                this.Recv();
                Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
                if ((int)packet.Type == (int)PacketType.예약정보)
                {
                    reservation = (Reserve)Packet.Deserialize(this.readBuffer);
                    if (reservation.used == -1)
                        break;
                    int time = reservation.time;
                    string btn = "btn_res" + time.ToString();
                    string btn1 = btn + "_1";
                    string team1 = "t" + time;
                    string team2 = team1 + "_1";
                    string pteam1 = "pt" + time;
                    string pteam2 = pteam1 + "_1";

                    if (reservation.used == 0) // 매칭완료 
                    {
                        Control[] con = this.Controls.Find(btn1, true);
                        PictureBox pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile("football.png");
                        pic.SizeMode = PictureBoxSizeMode.StretchImage;

                        con = this.Controls.Find(btn, true);
                        pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile("btn_red.png");

                        string t1 = reservation.send_name;
                        string t2 = reservation.recv_name;
                        int tnum = reservation.tnum;
                        string imgname = tnum + ".png";
                        con = this.Controls.Find(team1, true);
                        Label la1 = (Label)con[0];
                        la1.TextAlign = ContentAlignment.MiddleLeft;
                        la1.ForeColor = Color.Black;
                        la1.Text = t1;

                        con = this.Controls.Find(pteam1, true);
                        pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile(imgname);
                        pic.BringToFront();

                        con = this.Controls.Find(team2, true);
                        Label la2 = (Label)con[0];
                        la2.TextAlign = ContentAlignment.MiddleLeft;
                        la2.ForeColor = Color.Black;
                        la2.Text = t2;

                        con = this.Controls.Find(pteam2, true);
                        pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile(imgname);
                        pic.BringToFront();
                    }
                    else if (reservation.used == 1) // 혼자사용 
                    {
                        Control[] con = this.Controls.Find(btn1, true);
                        PictureBox pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile("goal.png");
                        pic.SizeMode = PictureBoxSizeMode.StretchImage;

                        con = this.Controls.Find(btn, true);
                        pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile("btn_red.png");

                        con = this.Controls.Find(team1, true);
                        Label la1 = (Label)con[0];
                        la1.TextAlign = ContentAlignment.MiddleRight;
                        la1.ForeColor = Color.DimGray;
                        la1.Text = "Already";

                        con = this.Controls.Find(team2, true);
                        Label la2 = (Label)con[0];
                        la2.TextAlign = ContentAlignment.MiddleLeft;
                        la2.ForeColor = Color.DimGray;
                        la2.Text = "Reserved";
                    }
                    else if (reservation.used == 2) // 비어있음 
                    {
                        Control[] con = this.Controls.Find(btn1, true);
                        PictureBox pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile("football.png");
                        pic.SizeMode = PictureBoxSizeMode.StretchImage;

                        string t1 = reservation.send_name;
                        int tnum = reservation.tnum;
                        string imgname = tnum + ".png";
                        con = this.Controls.Find(team1, true);
                        Label la1 = (Label)con[0];
                        la1.TextAlign = ContentAlignment.MiddleLeft;
                        la1.ForeColor = Color.Black;
                        la1.Text = t1;

                        con = this.Controls.Find(pteam1, true);
                        pic = (PictureBox)con[0];
                        pic.Image = Image.FromFile(imgname);
                        pic.BringToFront();

                    }
                }
            }
        }
        private void btn_reser_Click(object sender, EventArgs e)
        {
            unVisible();
            res_inform();
            btn_reser.BackgroundImage = Image.FromFile("onReservation.png");
            update_msg_panel();
            pn_reser.Visible = true;
           // picker.Visible = true;
            pn_reser.Controls.Add(picker);
        }

        private void btn_match_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;

            if (label.Text != "" || date_confirm() == -1)
            {
                return;
            }

            string name = label.Name;
            string pic_name = "p" + name;
            Control[] con = this.Controls.Find(pic_name, true);
            PictureBox picture = (PictureBox)con[0];

            int time_num = 0;
            if (name.Length > 3)
                time_num = int.Parse(name.Substring(0, name.Length - 2).Substring(1));
            else
                time_num = int.Parse(name.Substring(1));

            string date = picker.Value.ToShortDateString();
            try
            {
                match m = new match(this, 1);
                m.ShowDialog();
            }
            catch
            {
                return;
            }
            if (sel_num != -1)
            {
                Reserve reservation = new Reserve();
                reservation.Type = (int)PacketType.매칭;
                reservation.send_name = ID_txtBox.Text;
                reservation.time = time_num;
                reservation.match_num = 2;
                reservation.tnum = match_num;
                reservation.date = date;
                Packet.Serialize(reservation).CopyTo(this.sendBuffer, 0);
                this.stream = clientSocket.GetStream();
                this.Send();

                this.Recv();
                Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
                if ((int)packet.Type == (int)PacketType.매칭)
                {
                    reservation = (Reserve)Packet.Deserialize(this.readBuffer);
                    int num = reservation.used; // 0 : 성공 & 매칭 대기, 1 : 전체 사용중, 6 : 성공 & 꽉참 나머지 : 팀 인원수 불일치 (상대팀 인원수) 

                    if (num == 0 || num == 6)
                    {
                        string img = match_num + ".png";
                        MessageBox.Show("Success Reservation !!!");
                        picture.Image = Image.FromFile(img);
                        picture.SizeMode = PictureBoxSizeMode.StretchImage;

                        label.Text = ID_txtBox.Text;

                    }
                    else if (num == 1)
                    {
                        MessageBox.Show("Already reserved !!!");
                    }
                    else
                    {
                        MessageBox.Show("Number of Oposite Team : " + num.ToString() + "\nMatch Not Available !!!");
                    }
                }
            }

            unVisible();
            res_inform();
            update_msg_panel();
            btn_team.BackgroundImage = Image.FromFile("onTeamMatch.png");
            pn_team.Visible = true;
        }

        private void team_initial()
        {
            for (int i = 1; i < 14; i++)
            {
                string team1 = "t" + i;
                string team2 = team1 + "_1";
                string pteam1 = "pt" + i;
                string pteam2 = pteam1 + "_1";

                Control[] con = this.Controls.Find(team1, true);
                Label L1 = (Label)con[0];
                L1.Text = "";
                L1.BringToFront();

                con = this.Controls.Find(team2, true);
                Label L2 = (Label)con[0];
                L2.Text = "";
                L2.BringToFront();

                con = this.Controls.Find(pteam1, true);
                PictureBox pic1 = (PictureBox)con[0];
                pic1.Image = null;

                con = this.Controls.Find(pteam1, true);
                PictureBox pic2 = (PictureBox)con[0];
                pic2.Image = null;
            }
        }
        private void btn_team_Click(object sender, EventArgs e)
        {
            unVisible();
            res_inform();
            update_msg_panel();
            btn_team.BackgroundImage = Image.FromFile("onTeamMatch.png");
            pn_team.Visible = true;
            //    picker.Visible = true;
            pn_team.Controls.Add(picker);

        }

        private void btn_leader_Click(object sender, EventArgs e)
        {

            List lst = new List();
            lst.Type = (int)PacketType.전적;
            lst.where_check = 1;

            Packet.Serialize(lst).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();

            int i = 1;
            while (true) { 
                this.Recv();
                Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
                if ((int)packet.Type == (int)PacketType.전적)
                {
                    lst = (List)Packet.Deserialize(this.readBuffer);
                    if (lst.where_check == -1) break;
                    if (lst.where_check == 1)
                    {
                        ((Label)this.Controls.Find("team" + i, true)[0]).Text = lst.send_name;
                        ((Label)this.Controls.Find("team" + i + "_1", true)[0]).Text =
                            lst.win + "W " + lst.draw + "D " + lst.lose + "L";
                        ++i;
                    }
                }
            }

            unVisible();
            update_msg_panel();
            btn_leader.BackgroundImage = Image.FromFile("onLeaderboard.png");
            pn_leader.Visible = true;

        }
        private void myinitial()
        {
            for (int i = 1; i <= 4; i++)
            {
                string mdate = "myD" + i;
                string mtime = "time" + i;
                string mrdate = "res_date" + i;
                string mresult = "result" + i;
                string mstate = "state" + i;
                string mlable = mstate + "_1";

                Control[] con = this.Controls.Find(mdate, true);
                Label date = (Label)con[0];
                date.Text = "";

                con = this.Controls.Find(mtime, true);
                Label time = (Label)con[0];
                time.Text = "";

                con = this.Controls.Find(mrdate, true);
                Label rdate = (Label)con[0];
                rdate.Text = "";

                con = this.Controls.Find(mresult, true);
                Label result = (Label)con[0];
                result.Text = "";
                result.BringToFront();

                con = this.Controls.Find(mstate, true);
                PictureBox state = (PictureBox)con[0];
                state.Image = null;

                con = this.Controls.Find(mlable, true);
                Label label = (Label)con[0];
                label.Text = "";

            }
        }

        private string trans_time(int n)
        {
            string return_value = null;
            switch (n)
            {
                case 1:
                    return_value = "09:00 - 10:00";
                    break;
                case 2:
                    return_value = "10:00 - 11:00";
                    break;
                case 3:
                    return_value = "11:00 - 12:00";
                    break;
                case 4:
                    return_value = "12:00 - 13:00";
                    break;
                case 5:
                    return_value = "13:00 - 14:00";
                    break;
                case 6:
                    return_value = "14:00 - 15:00";
                    break;
                case 7:
                    return_value = "15:00 - 16:00";
                    break;
                case 8:
                    return_value = "16:00 - 17:00";
                    break;
                case 9:
                    return_value = "17:00 - 18:00";
                    break;
                case 10:
                    return_value = "18:00 - 19:00";
                    break;
                case 11:
                    return_value = "19:00 - 20:00";
                    break;
                case 12:
                    return_value = "20:00 - 21:00";
                    break;
                case 13:
                    return_value = "21:00 - 22:00";
                    break;

            }
            return return_value;
        }
        private void myReser_inform()
        {
            myinitial();
            string name = ID_txtBox.Text;
            Reserve res = new Reserve();
            res.Type = (int)PacketType.예약내역;
            res.send_name = name;
            Packet.Serialize(res).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();

            int count = 0;
            while (true)
            {
                this.Recv();
                Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
                if ((int)packet.Type == (int)PacketType.예약내역)
                {
                    res = (Reserve)Packet.Deserialize(this.readBuffer);

                    count++;
                    if (res.used == -1)
                        break;
                    string mdate = "myD" + count;
                    string mtime = "time" + count;
                    string date = res.date;
                    string time = trans_time(res.time);

                    Control[] con = this.Controls.Find(mdate, true);
                    Label date1 = (Label)con[0];
                    date1.Text = date;

                    con = this.Controls.Find(mtime, true);
                    Label time1 = (Label)con[0];
                    time1.Text = time;

                    DateTime cur = DateTime.Today;
                    DateTime rdate = Convert.ToDateTime(date);

                    int cmp = DateTime.Compare(cur, rdate);

                    string pic = "state" + count;
                    string pic_name = pic + "_1";
                    if (cmp > 0)
                    {
                        con = this.Controls.Find(pic_name, true);
                        Label la = (Label)con[0];
                        la.BringToFront();
                        la.Text = "사용완료";
                        la.ForeColor = Color.Black;
                    }
                    else if (cmp == 0)
                    {
                        con = this.Controls.Find(pic_name, true);
                        Label la = (Label)con[0];
                        la.BringToFront();
                        la.Text = "취소불가";
                        la.ForeColor = Color.Red;
                    }
                    else
                    {
                        con = this.Controls.Find(pic, true);
                        PictureBox picture = (PictureBox)con[0];
                        picture.Image = Image.FromFile("cancel.png");
                        picture.BringToFront();

                    }

                }
            }
        }

        private void my_list()
        {
            List lst = new List();
            lst.Type = (int)PacketType.전적;
            lst.send_name = ID_txtBox.Text;
            Packet.Serialize(lst).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();

            this.Recv();
            Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
            if ((int)packet.Type == (int)PacketType.전적)
            {
                lst = (List)Packet.Deserialize(this.readBuffer);
                int win = lst.win;
                int draw = lst.draw;
                int lose = lst.lose;

                label54.Text = "W" + win.ToString() + " D" + draw.ToString() + " L" + lose.ToString();
            }

        }
        /// <summary>
        /// ////////////////////////////// 수정 ///////////////////////////////
        /// </summary>
        private void my_result()
        {
            previous_result r = new previous_result();
            r.Type = (int)PacketType.최근결과;
            r.loginID = ID_txtBox.Text;

            Packet.Serialize(r).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();
            int count = 0;
            while (true)
            {
                this.Recv();
                Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
                if ((int)packet.Type == (int)PacketType.최근결과)
                {
                    r = (previous_result)Packet.Deserialize(this.readBuffer);
                    count++;

                    if (r.myScore == -1)
                        break;

                    string date = r.date;
                    string t1 = r.myTeam;
                    string t2 = r.opTeam;
                    int s1 = r.myScore;
                    int s2 = r.OPScore;
                    int res = 0; // 1 : 승, -1 : 패, 0 : 무
                    string dlabel = "res_date" + count;
                    Control[] con = this.Controls.Find(dlabel, true);
                    Label date1 = (Label)con[0];
                    date1.Text = date;
                    //MessageBox.Show(t1 + " "+t2);
                    string lt1 = "a" + count;
                    string lt2 = "b" + count;
                    string pic = "p" + count;
                    string pic1 = "d" + count;

                    con = this.Controls.Find(lt1, true);
                    Label a1 = (Label)con[0];

                    con = this.Controls.Find(lt2, true);
                    Label b1 = (Label)con[0];

                    con = this.Controls.Find(pic, true);
                    PictureBox c1 = (PictureBox)con[0];

                    con = this.Controls.Find(pic1, true);
                    PictureBox d1 = (PictureBox)con[0];

                    if (t1 == ID_txtBox.Text)
                    {
                        if (s1 > s2)
                        {
                            //c1.Image = Image.FromFile("win.png");
                            c1.BringToFront();
                        }

                        a1.Text = t1 + "  " + s1;
                        b1.Text = s2 + "  " + t2;
                        a1.BringToFront();
                        b1.BringToFront();
                        d1.BringToFront();
                    }
                    else
                    {
                        if (s1 < s2)
                        {
                            //c1.Image = Image.FromFile("win.png");
                            c1.BringToFront();
                        }
                        a1.Text = t2 + "  " + s2;
                        b1.Text = s1 + "  " + t1;
                        a1.BringToFront();
                        b1.BringToFront();
                        d1.BringToFront();
                    }
                }
            }
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_my_Click(object sender, EventArgs e)
        {
            unVisible();
            myReser_inform();
            my_list();
            my_result();
            update_msg_panel();
            btn_my.BackgroundImage = Image.FromFile("onMyPage.png");
            pn_my.Visible = true;
        }

        private void ID_PW_txtBox_Click(object sender, EventArgs e)
        {   //ID_PW 텍스트 박스 클릭시 텍스트 전체 선택 이벤트 핸들러
            ((TextBox)sender).SelectAll();
        }

        private void btn_logout_Click(object sender, EventArgs e)
        {
            //팀이름 정보 서버에 보내기
            this.m_Team_Info = new Team_Info();
            this.m_Team_Info.Type = (int)PacketType.로그아웃;
            this.m_Team_Info.t_name = this.Text;

            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();

            unVisible();
            pn_menu.Visible = false;
            pn_sideMenu.Visible = false;
            panel1.Visible = true;
            ID_txtBox.Text = "Team Name";
            PW_txtBox.Text = "password";
            this.Text = "SoccerClient";
        }

        private int date_confirm()
        {
            string date = picker.Value.ToShortDateString();

            Reserve reservation1 = new Reserve();
            reservation1.Type = (int)PacketType.예약가능;
            reservation1.send_name = ID_txtBox.Text;
            reservation1.date = date;
            Packet.Serialize(reservation1).CopyTo(this.sendBuffer, 0);
            this.stream = clientSocket.GetStream();
            this.Send();

            this.Recv();
            Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
            if ((int)packet.Type == (int)PacketType.예약가능)
            {

                reservation1 = (Reserve)Packet.Deserialize(this.readBuffer);
                if (reservation1.used == 1)
                {
                    MessageBox.Show("Not Available !!!!\n해당 날짜(" + date + ")에 이미 예약하셨습니다.");
                    return -1;
                }
            }
            return 0;
        }
        private void res_button_click(object sender, EventArgs e)
        {
            PictureBox picture = (PictureBox)sender;

            string pic_name = picture.Name + "_1";
            string date = picker.Value.ToShortDateString();

            if (date_confirm() == -1)
                return;

            int time_num = int.Parse(picture.Name.Substring(7, 1));
            sel_num = -1;

            try
            {
                match m = new match(this);
                m.ShowDialog();
            }
            catch
            {
                return;
            }

            if (sel_num != -1)
            {
                if (sel_num == 1) // 혼자
                {

                    Reserve reservation = new Reserve();
                    reservation.Type = (int)PacketType.혼자;
                    reservation.send_name = ID_txtBox.Text;
                    reservation.time = time_num;
                    reservation.match_num = 1;
                    reservation.date = date;
                    Packet.Serialize(reservation).CopyTo(this.sendBuffer, 0);
                    this.stream = clientSocket.GetStream();
                    this.Send();

                    this.Recv();
                    Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
                    if ((int)packet.Type == (int)PacketType.혼자)
                    {
                        reservation = (Reserve)Packet.Deserialize(this.readBuffer);
                        int num = reservation.used; // 0 : 성공, 1 : 전체다 사용중, 2 : 매칭만 가능 

                        if (num == 0)
                        {
                            MessageBox.Show("Success Reservation !!!");
                            picture.Image = Image.FromFile("btn_red.png");
                            Control[] con = this.Controls.Find(pic_name, true);
                            PictureBox pic = (PictureBox)con[0];
                            pic.Image = Image.FromFile("goal.png");
                            pic.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        else if (num == 1)
                        {
                            MessageBox.Show("Already reserved !!!");
                        }
                        else
                        {
                            MessageBox.Show("Only Match Available !!!");
                        }
                    }

                }
                else // 매칭 
                {
                    Reserve reservation = new Reserve();
                    reservation.Type = (int)PacketType.매칭;
                    reservation.send_name = ID_txtBox.Text;
                    reservation.time = time_num;
                    reservation.match_num = 2;
                    reservation.tnum = match_num;
                    reservation.date = date;
                    Packet.Serialize(reservation).CopyTo(this.sendBuffer, 0);
                    this.stream = clientSocket.GetStream();
                    this.Send();

                    this.Recv();
                    Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
                    if ((int)packet.Type == (int)PacketType.매칭)
                    {
                        reservation = (Reserve)Packet.Deserialize(this.readBuffer);
                        int num = reservation.used; // 0 : 성공 & 매칭 대기, 1 : 전체 사용중, 6 : 성공 & 꽉참 나머지 : 팀 인원수 불일치 (상대팀 인원수) 

                        if (num == 0)
                        {
                            MessageBox.Show("Success Reservation !!!");
                            //picture.Image = Image.FromFile("btn_red.png");
                            Control[] con = this.Controls.Find(pic_name, true);
                            PictureBox pic = (PictureBox)con[0];
                            pic.Image = Image.FromFile("football.png");
                            pic.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        else if (num == 6)
                        {
                            MessageBox.Show("Success Reservation !!!");
                            picture.Image = Image.FromFile("btn_red.png");
                            Control[] con = this.Controls.Find(pic_name, true);
                            PictureBox pic = (PictureBox)con[0];
                            pic.Image = Image.FromFile("football.png");
                            pic.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        else if (num == 1)
                        {
                            MessageBox.Show("Already reserved !!!");
                        }
                        else
                        {
                            MessageBox.Show("Number of Oposite Team : " + num.ToString() + "\nMatch Not Available !!!");
                        }
                    }

                }
            }
        }

        private void cancel_button_click(object sender, EventArgs e)
        {
            PictureBox picture = (PictureBox)sender;
            if (picture.Image != null)
            {
                picture.Image = null;
                string pic_name = picture.Name + "_1";
                string num = pic_name.Substring(5, 1);
                string mdate = "myD" + num;

                Control[] con = this.Controls.Find(mdate, true);
                Label la = (Label)con[0];

                Reserve res = new Reserve();
                res.Type = (int)PacketType.예약취소;
                res.send_name = ID_txtBox.Text;
                res.date = la.Text;
                Packet.Serialize(res).CopyTo(this.sendBuffer, 0);
                this.Send();

                con = this.Controls.Find(pic_name, true);
                Label la1 = (Label)con[0];
                la1.BringToFront();
                la1.Text = "예약취소";
                la1.ForeColor = Color.Red;
            }
        }

        private void pan_msg_Click(object sender, EventArgs e)
        {
            chat mChild = new chat(this);
            if (mChild.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Completed Join!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.None);       //성공 메시지 띄우기
            }
        }

        public void update_msg_panel() // 채팅창 닫힐 때 패널 업데이트 
        {
            Msg_list msg = new Msg_list();
            msg.Type = (int)PacketType.메시지개수;
            msg.recv_name = ID_txtBox.Text;
            Packet.Serialize(msg).CopyTo(this.sendBuffer, 0);
            this.Send();

            this.Recv();
            Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
            if (packet.Type == (int)PacketType.메시지개수)
            {
                msg = (Msg_list)Packet.Deserialize(this.readBuffer);
                if (msg.count == 0)
                    msg_count.Visible = false;
                else
                {
                    msg_count.Text = msg.count.ToString();
                    msg_count.Visible = true;
                }
            }
        }

        private void keyDown(object sender, KeyEventArgs e)
        {   //텍스트 박스에서 키 눌렀을 때 이벤트 
            TextBox t = sender as TextBox;
            if (t == ID_txtBox)               //아이디 텍스트 박스에서 
            {
                if (e.KeyCode == Keys.Tab)    //탭키눌렀을 때 패스워드 텍스트 박스로 포커스
                {
                    this.ActiveControl = PW_txtBox;
                }
            }
            else if (t == PW_txtBox)          //패스워드 텍스트 박스에서
            {
                if (e.KeyCode == Keys.Enter)  //엔터키 눌럿을 때 로그인 버튼 눌러짐
                {
                    Login_btn_Click(sender, e);
                }
            }
        }

        private void btn_set_Click(object sender, EventArgs e)
        {
            Client_Setting mChild = new Client_Setting(this);
            if (mChild.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Completed Setting!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.None);       //성공 메시지 띄우기

            }
        }

        public string returnID()
        {
            return ID_txtBox.Text;
        }

        public string returnTime(int n)
        {
            return trans_time(n);
        }

        private void btn_info_Click(object sender, EventArgs e)
        {
            pre_result(-1);
        }
    }

}