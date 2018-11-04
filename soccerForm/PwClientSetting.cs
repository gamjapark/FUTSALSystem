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
using System.Net.Sockets;
using System.IO;

namespace soccerForm
{
    public partial class PwClientSetting : Form
    {

        private TcpClient m_client = null;                                  //클라이언트
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림

        //버퍼
        private byte[] sendBuf = new byte[1024 * 4];   //보내기 버퍼
        private byte[] readBuf = new byte[1024 * 4];   //읽기 버퍼

        private string team_name = "";
        public string team_pw;
        public Team_Info m_Team_Info;


        public PwClientSetting()
        {
            InitializeComponent();
        }
        public PwClientSetting(Client_Setting parentForm)
        {
            InitializeComponent();
            this.m_client = parentForm.SetPacket();
            this.m_networkstream = m_client.GetStream();
            this.team_name = parentForm.loginID();
        }

        public void Send() // 패킷 전송 함수 
        {
            this.m_networkstream.Write(this.sendBuf, 0, this.sendBuf.Length);
            this.m_networkstream.Flush();
            for (int i = 0; i < 1024 * 4; i++)
                this.sendBuf[i] = 0;
        }

        public void Recv() // 패킷 수신 함수 
        {
            int nRead = 0;
            try
            {
                nRead = this.m_networkstream.Read(readBuf, 0, 1024 * 4);
            }
            catch
            {
                this.m_networkstream = null;
            }
        }

        private void register_btn_Click(object sender, EventArgs e)
        {
            if (ID_txtBox.Text != "" && PW_txtBox.Text != "" && Confirm_txtBox.Text != ""
                && PW_txtBox.Text != "New Password" && Confirm_txtBox.Text != "Confirm New Password")
            {
                if (PW_txtBox.Text.Equals(Confirm_txtBox.Text)){
                    //패스워드 정보 서버에 보내기
                    this.m_Team_Info = new Team_Info();
                    this.m_Team_Info.Type = (int)PacketType.패스워드확인;
                    this.m_Team_Info.t_name = team_name;
                    this.m_Team_Info.pw = ID_txtBox.Text;

                    Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                    this.m_networkstream = m_client.GetStream();
                    this.Send();

                    //(정상인지 오류인지) 해당 패스워드가 맞는지 확인하는 정보 받기
                    this.Recv();
                    Packet packet2 = (Packet)Packet.Deserialize(this.readBuf);
                    if ((int)packet2.Type == (int)PacketType.패스워드확인)
                    {
                        this.m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuf);
                        if ((int)m_Team_Info.Error == (int)PacketSendERROR.정상 ||
                            (int)m_Team_Info.Error == (int)PacketSendERROR.존재)     //패스워드정상
                        {
                            //패스워드 정보 서버에 보내기
                            this.m_Team_Info = new Team_Info();
                            this.m_Team_Info.Type = (int)PacketType.패스워드변경;
                            this.m_Team_Info.t_name = team_name;
                            this.m_Team_Info.pw = PW_txtBox.Text;

                            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();


                            this.DialogResult = DialogResult.OK;                  //완료 결과
                        }
                        else if ((int)m_Team_Info.Error == (int)PacketSendERROR.에러)
                        {
                            MessageBox.Show("Password Incorrect!", "Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.None);         //에러 메시지 띄우기
                        }

                    }
                }
                else
                {
                    MessageBox.Show("Password Incorrect!", "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
                }
            }
            else
            {
                MessageBox.Show("Input your Previous Password!", "Empty",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txt_Click(object sender, EventArgs e)
        {   //텍스트 박스 눌렀을 때 모두 선택
            ((TextBox)sender).SelectAll();
        }
    }
}
