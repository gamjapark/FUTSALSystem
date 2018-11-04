using System;
using System.Drawing;
using System.Windows.Forms;
using Packet_Library;
using System.Net.Sockets;
using System.Collections;

namespace soccerForm
{
    public partial class chat : Form
    {
        private TcpClient m_client = null;                                  //클라이언트
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림
        private byte[] sendBuffer = new byte[1024 * 4];
        private byte[] readBuffer = new byte[1024 * 4];
        private ArrayList client_list = new ArrayList();                    // 클라이언트 리스트관리
        private ArrayList msg_list = new ArrayList();                       // 메시지 관리 
        private ArrayList date_list = new ArrayList();                      // 날짜 관리 
        private ArrayList used_list = new ArrayList();                      // 읽음 관리 
        private Label[] la;
        private PictureBox[] pic;
        private string curID;
        private Form1 form;
        public chat()
        {
            InitializeComponent();
        }
        public chat(Form1 pForm)
        {
            InitializeComponent();
            m_client = pForm.SetPacket();
            curID = pForm.loginID();
            form = pForm.getForm();
            m_networkstream = m_client.GetStream();
            pic_false();
            panel2.Parent = panel3;
            pan_list(null); // 폼 첫번째 화면 설정 
        }
        
        public void Send() // 패킷 전송 함수 
        {
            this.m_networkstream.Write(this.sendBuffer, 0, this.sendBuffer.Length);
            this.m_networkstream.Flush();
            for (int i = 0; i < 1024 * 4; i++)
                this.sendBuffer[i] = 0;
        }

         Packet Recv() // 패킷 수신 함수 
        {
            int nRead = 0;
            try
            {
                nRead = this.m_networkstream.Read(readBuffer, 0, 1024 * 4);
            } catch
            {
                this.m_networkstream = null;
            }
            Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
            return packet;
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawLine(new Pen(Color.Black, 10), 10, 0, panel1.Width - 10, 0);
        }


        private void pic_false() // 모두 비어있는 상태로 설정 
        {
            pictureBox1.Image = Image.FromFile("user.png");
            pictureBox2.Image = Image.FromFile("send.png");
            pictureBox3.Image = Image.FromFile("recv.png");
            panel3.Visible = false;
            panel4.Visible = false;
            panel6.Visible = false;
        }

        private void send_msg(object sender, EventArgs e) // 메시지 전송 함수 
        {
            for(int i=0; i<client_list.Count; i++)
            {
                PictureBox box = sender as PictureBox;
                if(box == pic[i])
                {
                    sendMsg mChild = new sendMsg(form, la[i].Text);
                    mChild.ShowDialog();
                    break;
                }
            }
        }


        private void send_inform(object sender, EventArgs e)
        {
            for (int i = 0; i < client_list.Count; i++)
            {
                PictureBox box = sender as PictureBox;
                if (box == pic[i])
                {
                    sendMsg mChild = new sendMsg(form, la[i].Text, date_list[i].ToString(), msg_list[i].ToString());
                    mChild.ShowDialog();
                    break;
                }
            }
        }
        private void recv_inform(object sender, EventArgs e)
        {
            for (int i = 0; i < client_list.Count; i++)
            {
                PictureBox box = sender as PictureBox;
                if (box == pic[i])
                {
                    if (la[i].ForeColor == Color.Black) // 수신 확인 표시 
                    {
                        la[i].ForeColor = Color.Gray;
                        Msg_list msg = new Msg_list();
                        msg.Type = (int)PacketType.수신확인;
                        msg.recv_name = curID;
                        msg.send_name = la[i].Text;
                        msg.date = date_list[i].ToString();
                        msg.text = msg_list[i].ToString();
                        Packet.Serialize(msg).CopyTo(this.sendBuffer, 0);
                        this.Send();
                        form.update_msg_panel();
                    }
                    sendMsg mChild = new sendMsg(form, la[i].Text, date_list[i].ToString(), msg_list[i].ToString(), 0);
                    mChild.ShowDialog();
                    break;
                }
            }
        }
        private void msg_list_inform(int num) // 메시지 송수신 리스트 관리
        {
            int type;
            Panel pan;
            if(num == 1) // 송신 리스트 
            {
                type = (int)PacketType.송신리스트;
                pan = panel4;
            } else // 수신 리스트 
            {
                type = (int)PacketType.수신리스트;
                pan = panel6;
            }
            pan.Controls.Clear();

            Msg_list send = new Msg_list();
            send.Type = type;
            send.recv_name = curID;
            Packet.Serialize(send).CopyTo(this.sendBuffer, 0);
            this.Send();

            // 계정 리스트 수신
            Packet packet = this.Recv();
            if (packet.Type == type) // 송수신 리스트 받아오기 
            {
                while (true)
                {
                    send = (Msg_list)Packet.Deserialize(this.readBuffer);
                    if (send.count == -1)
                        break;
                    //   MessageBox.Show(send.send_name);
                    client_list.Add(send.send_name);
                    msg_list.Add(send.text);
                    date_list.Add(send.date);
                    used_list.Add(send.used);
                    packet = this.Recv();
                }

                int k = 0;
                
                // 계정의 개수 만큼 picturbox 및 label 생성 
                pic = new PictureBox[client_list.Count];
                la = new Label[client_list.Count];
                for (int x = client_list.Count - 1; x >= 0; x--) // 패널에 계정 리스트 label 및 picturbox 추가 
                {
                    string sendID = client_list[x].ToString();
                    pic[x] = new PictureBox();
                    pic[x].Image = Image.FromFile("detail.png");
                    pic[x].SizeMode = PictureBoxSizeMode.StretchImage;
                    pic[x].Location = new Point(215, 20 + k);
                    pic[x].Size = new Size(25, 25);
                    pan.Controls.Add(pic[x]);
                    if(num == 1)
                        pic[x].Click += new EventHandler(send_inform);
                    else
                        pic[x].Click += new EventHandler(recv_inform);

                    DateTime date = Convert.ToDateTime(date_list[x]);
                    la[x] = new Label();
                    if (int.Parse(used_list[x].ToString()) == 0 || num == 1)
                        la[x].ForeColor = Color.Black;
                    else
                        la[x].ForeColor = Color.Gray;
                    la[x].Text = sendID;
                    la[x].AutoSize = false;
                    la[x].Font = new Font("Arial", 10);
                    la[x].Location = new Point(20, 20 + k);
                    la[x].Size = new Size(210, 20);
                    pan.Controls.Add(la[x]);

                    k += 30;
                }
            }
        }
        private void pan_list(string name) // 계정 리스트 화면 설정하는 함수 
        {
            // 이전에 있었던 컨트롤 초기화 
            client_list.Clear();
            pic = null;
            la = null;
            pic_false();
            panel3.Visible = true;
            panel2.Controls.Clear();
            pictureBox1.Image = Image.FromFile("s_user.png");

            team_list team = new team_list();
            team.Type = (int)PacketType.팀이름리스트;
            Packet.Serialize(team).CopyTo(this.sendBuffer, 0);
            this.Send();

            // 계정 리스트 수신
            Packet packet = this.Recv();
            if (packet.Type == (int)PacketType.팀이름리스트) 
            {
                while (true)
                {
                    team_list Team_list = (team_list)Packet.Deserialize(this.readBuffer);
                    if (Team_list.count == -1)
                        break;

                    if (name == null || Team_list.t_name.Contains(name))
                    {

                        if(curID != Team_list.t_name)
                          client_list.Add(Team_list.t_name);
                    }
                    packet = this.Recv();                    
                }

                int k = 0;
                int i = 0;
                // 계정의 개수 만큼 picturbox 및 label 생성 
                pic = new PictureBox[client_list.Count];
                la = new Label[client_list.Count];
                foreach(string client in client_list) // 패널에 계정 리스트 label 및 picturbox 추가 
                {
                    pic[i] = new PictureBox();
                    pic[i].Image = Image.FromFile("send1.png");
                    pic[i].SizeMode = PictureBoxSizeMode.StretchImage;
                    pic[i].Location = new Point(215, 20 + k);
                    pic[i].Size = new Size(25, 25);
                    panel2.Controls.Add(pic[i]);
                    pic[i].Click += new EventHandler(send_msg);

                    la[i] = new Label();
                    la[i].Text = client;
                    la[i].AutoSize = false;
                    la[i].Font = new Font("Arial", 12);
                    la[i].Location = new Point(10, 20 + k);
                    la[i].Size = new Size(210, 25);
                    panel2.Controls.Add(la[i]);
                    k += 30;
                    i++;
                }
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pan_list(null);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            client_list.Clear();
            msg_list.Clear();
            date_list.Clear();
            used_list.Clear();
        
            pic = null;
            la = null;
            pic_false();
            panel4.Controls.Clear();
            panel4.Visible = true;
            pictureBox2.Image = Image.FromFile("s_send.png");

            msg_list_inform(1);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            client_list.Clear();
            msg_list.Clear();
            date_list.Clear();
            used_list.Clear();

            pic = null;
            la = null;
            pic_false();
            panel6.Controls.Clear();
            panel6.Visible = true;
            pictureBox3.Image = Image.FromFile("s_recv.png");
            msg_list_inform(0);
        }

        private void ID_txtBox_Click(object sender, EventArgs e)
        {
            ID_txtBox.SelectAll();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            pan_list(ID_txtBox.Text); // 검색할 문자열 함께 넘겨줌 
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawLine(new Pen(Color.Black, 5), 10, panel5.Height, panel1.Width - 10, panel5.Height);
        }

        private void chat_FormClosed(object sender, FormClosedEventArgs e)
        {
            form.update_msg_panel();
        }
    }
}
