using Packet_Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace soccerForm
{
    public partial class match : Form
    {
        private TcpClient m_client = null;                                  //클라이언트
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림
        private string curID;
        private string recvID;
        Form1 pForm;
        //버퍼
        private byte[] sendBuffer = new byte[1024 * 4];
        private byte[] readBuffer = new byte[1024 * 4];

        public match()
        {
            InitializeComponent();
        }

        public match(Form1 pForm)
        {
            InitializeComponent();
            this.Location = new Point(200, 200);
            m_client = pForm.SetPacket();
            m_networkstream = m_client.GetStream();
            curID = pForm.loginID();
            this.pForm = pForm;
            pForm.sel_num = -1;
            panel4.Visible = false;
            panel1.Visible = true;
        }

        public match(Form1 pForm, int match)
        {
            InitializeComponent();
            pForm.sel_num = -1;
            this.Location = new Point(200, 200);
            m_client = pForm.SetPacket();
            m_networkstream = m_client.GetStream();
            curID = pForm.loginID();
            this.pForm = pForm;
            panel1.Visible = false;
            panel4.Visible = true;
            person_num();
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
            }
            catch
            {
                this.m_networkstream = null;
            }
            Packet packet = (Packet)Packet.Deserialize(this.readBuffer);
            return packet;
        }
        private void person_num()
        {
            Team_Info m_Team_Info = new Team_Info();
            m_Team_Info.Type = (int)PacketType.팀인원수;
            m_Team_Info.t_name = curID;
            int count = 0;
            Packet.Serialize(m_Team_Info).CopyTo(this.sendBuffer, 0);
            this.Send();

            //(정상인지 오류인지) 해당 팀 이름이 이미 사용중인지 아닌지 정보 받기
            this.Recv();
            Packet packet = (Packet)Packet.Deserialize(this.readBuffer);

            if ((int)packet.Type == (int)PacketType.팀인원수)
            {
                m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuffer);
                count = m_Team_Info.stCount;
                if (count < 4)
                {
                    MessageBox.Show("Team Match 불가 \n(팀 인원 수 최소 3명)");
                    pForm.sel_num = -1;
                    this.Close();
                }
            }

            comboBox1.Items.Clear();
            for (int i = count; i > 2; i--)
                comboBox1.Items.Add(i + " 명");
            comboBox1.SelectedIndex = 0;
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            person_num();
            
            panel1.Visible = false;
            panel4.Visible = true;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pForm.sel_num = 1;
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            String sel = comboBox1.SelectedItem.ToString().Substring(0,1);
            pForm.sel_num = 2;
            pForm.match_num = int.Parse(sel);
            this.Close();
        }
    }
}
