using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections;
using Packet_Library;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace soccerForm
{
    public partial class Result : Form
    {
        Form1 parent;
        private TcpClient m_client = null;                                  //클라이언트
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림

        //버퍼
        private byte[] sendBuf = new byte[1024 * 4];   //보내기 버퍼
        private byte[] readBuf = new byte[1024 * 4];   //읽기 버퍼

        previous_result m_pre_result;
        string Tname = "";
        int Tscore;
        string OPTeam = "";
        int OPscore;
        string result = "";
        string date = "";
        int time;

        public Result()
        {
            InitializeComponent();
        }

        public Result(Form1 parent)
        {
            InitializeComponent();
            m_client = parent.SetPacket();
            this.parent = parent;
            Tname = parent.returnID();
            initial();
            getResult();
        }

        public void Send()
        {
            this.m_networkstream.Write(this.sendBuf, 0, this.sendBuf.Length);
            this.m_networkstream.Flush();

            for (int i = 0; i < 1024 * 4; i++)
            {
                this.sendBuf[i] = 0;
            }
        }

        public void Recv()
        {
            try
            {
                this.m_networkstream.Read(readBuf, 0, 1024 * 4);
            }
            catch
            {
                this.m_networkstream = null;
            }
        }

        public void initial()
        {
            label1.Text = "";
            label2.Text = "";
            textBox1.Text = "";
            textBox2.Text = "";
        }

        public void getResult()
        {
            this.m_pre_result = new previous_result();
            this.m_pre_result.Type = (int)PacketType.이전경기결과;
            this.m_pre_result.myTeam = Tname;
            this.m_pre_result.date = DateTime.Today.ToShortDateString();

            Packet.Serialize(this.m_pre_result).CopyTo(this.sendBuf, 0);
            this.m_networkstream = m_client.GetStream();
            this.Send();

            this.Recv();
            Packet packet = (Packet)Packet.Deserialize(this.readBuf);
            if((int)packet.Type == (int)PacketType.이전경기결과)
            {
                this.m_pre_result = (previous_result)Packet.Deserialize(this.readBuf);
                if (m_pre_result.pre_result != 0)
                {
                    //////////////수정/////////////
                    this.Tname = m_pre_result.myTeam;
                    this.OPTeam = m_pre_result.opTeam;
                    this.date = m_pre_result.date;
                    this.time = m_pre_result.time;


                    label5.Text = this.Tname;
                    label4.Text = this.OPTeam;
                    label1.Text = this.date;
                    label2.Text = parent.returnTime(this.time);
                    ////////////////////////////////
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("경기결과를 입력해 주십시오");
            }
            else
            {
                this.m_pre_result = new previous_result();
                this.m_pre_result.Type = (int)PacketType.이전경기결과제출;
                judgeID();
                this.m_pre_result.myTeam = this.Tname;
                this.m_pre_result.opTeam = this.OPTeam;
                this.m_pre_result.myScore = int.Parse(textBox1.Text);
                this.m_pre_result.OPScore = int.Parse(textBox2.Text);
                this.m_pre_result.time = this.time;
                this.m_pre_result.date = this.date;
                this.m_pre_result.loginID = parent.returnID();

                Packet.Serialize(this.m_pre_result).CopyTo(this.sendBuf, 0);
                this.m_networkstream = m_client.GetStream();
                this.Send();
            }
            initial();
            getResult();
        }

        public void judgeID()
        {
            if(Tname != parent.returnID())
            {
                string temp = Tname;
                Tname = OPTeam;
                OPTeam = temp;

                string temp2 = textBox1.Text;
                textBox1.Text = textBox2.Text;
                textBox2.Text = temp2;
            }
        }
    }
}
