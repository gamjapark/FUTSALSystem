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
    public partial class sendMsg : Form
    {
        private TcpClient m_client = null;                                  //클라이언트
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림
        private string curID;
        private string recvID;
        //버퍼
        private byte[] sendBuffer = new byte[1024 * 4];
        private byte[] readBuffer = new byte[1024 * 4];

        public sendMsg()
        {
            InitializeComponent();
        }

        public sendMsg(Form1 pForm, string name) // 메시지 전송 
        {
            InitializeComponent();
            m_client = pForm.SetPacket();
            m_networkstream = m_client.GetStream();
            curID = pForm.loginID();
            recvID = name;
            vis_flase();
            panel2.Visible = true;

            label2.Text = recvID;
            label2.Location = new Point(15, 6);
            label2.AutoSize = false;
            label2.Font = new Font("Arial", 13);
            label2.Size = new Size(240, 30);
            panel2.Controls.Add(label2);
        }

        public sendMsg(Form1 pForm, string name, string date, string text) // 송신 메시지 정보 출력 
        {
            InitializeComponent();
            m_client = pForm.SetPacket();
            m_networkstream = m_client.GetStream();
            curID = pForm.loginID();
            recvID = name;

            vis_flase();
            panel1.Visible = true;
            this.Text = "Sent Message";
            label4.Text = recvID;
            label4.BringToFront();
            label3.Text = date;
            textBox1.Text = text;
            textBox1.Enabled = false;
        }

        public sendMsg(Form1 pForm, string name, string date, string text, int n) // 수신 메시지 정보 출력 
        {
            InitializeComponent();
            m_client = pForm.SetPacket();
            m_networkstream = m_client.GetStream();
            curID = pForm.loginID();
            recvID = name;
            vis_flase();
            panel3.Visible = true;
            this.Text = "Recived Message";
            label5.Text = recvID;
            label5.BringToFront();
            label7.Text = date;
            textBox3.Text = text;
            textBox3.Enabled = false;
        }

        private void vis_flase()
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Msg_list send = new Msg_list();
            send.Type = (int)PacketType.메시지송신;
            send.date = DateTime.Now.ToString();
            send.send_name = curID;
            send.recv_name = recvID;
            send.text = textBox2.Text;

            Packet.Serialize(send).CopyTo(this.sendBuffer, 0);
            this.Send();

            MessageBox.Show("Message sent Complete !");
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            string sendID = label4.Text;
            vis_flase();
            panel2.Visible = true;
            this.Text = "Send Message";
            label2.Text = label5.Text;
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            label1.Text = textBox2.TextLength.ToString() + " / 100";
        }
    }
}
