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
    public partial class Form_Join : Form
    {
        private TcpClient m_client = null;                                  //클라이언트
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림

        //버퍼
        private byte[] sendBuf = new byte[1024 * 4];   //보내기 버퍼
        private byte[] readBuf = new byte[1024 * 4];   //읽기 버퍼

        string team_name = "";
        List<string> si = new List<string>();

        public Team_Info m_Team_Info;
        public Student_ID m_Student_ID;

        bool team_check = false;
        bool id_check1 = false;
        bool id_check2 = false;
        bool id_check3 = false;
        bool id_check4 = false;
        bool id_check5 = false;

        public Form_Join()
        {
            InitializeComponent();
        }

        public Form_Join(Form1 parentForm)
        {
            InitializeComponent();
            m_client = parentForm.SetPacket();
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

        private void register_btn_Click(object sender, EventArgs e)
        {
            if(team_check)
            {
                if(id_check1 || id_check2 || id_check3 || id_check4 || id_check5)
                {
                    if (PW_txtBox.Text.Equals(Confirm_txtBox.Text))
                    {
                        //학번 보내기
                        foreach (string s_id in si)
                        {
                            //저장한 학번들 정보 서버에 보내기
                            this.m_Student_ID = new Student_ID();
                            this.m_Student_ID.Type = (int)PacketType.학번저장;
                            this.m_Student_ID.s_id = s_id;
                            this.m_Student_ID.t_name = team_name;

                            Packet.Serialize(this.m_Student_ID).CopyTo(this.sendBuf, 0);
                            this.m_networkstream = m_client.GetStream();
                            this.Send();

                            //(정상인지 오류인지) 학번저장이 안되었다는 메시지오면 오류 메시지!
                            this.Recv();
                            Packet pack = (Packet)Packet.Deserialize(this.readBuf);

                            if ((int)pack.Type == (int)PacketType.학번저장)
                            {
                                this.m_Student_ID = (Student_ID)Packet.Deserialize(this.readBuf);
                                if ((int)m_Student_ID.Error == (int)PacketSendERROR.에러)
                                {
                                    MessageBox.Show("Student ID can Not be Saved!", "Failed",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
                                }
                            }
                        }

                        //팀이름, 패스워드, 팀 인원수 보내기
                        this.m_Team_Info = new Team_Info();
                        this.m_Team_Info.Type = (int)PacketType.로그인정보저장;
                        this.m_Team_Info.t_name = team_name;
                        this.m_Team_Info.pw = PW_txtBox.Text;
                        this.m_Team_Info.stCount = si.Count;

                        Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                        this.m_networkstream = m_client.GetStream();
                        this.Send();

                        //(정상인지 오류인지) 팀이름, 패스워드 저장이 안되었다는 메시지오면 오류 메시지!
                        this.Recv();
                        Packet packet = (Packet)Packet.Deserialize(this.readBuf);

                        if ((int)packet.Type == (int)PacketType.로그인정보저장)
                        {
                            this.m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuf);
                            if ((int)m_Team_Info.Error == (int)PacketSendERROR.에러)
                            {
                                MessageBox.Show("User Information can Not be Saved!", "Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
                            }else if((int)m_Team_Info.Error == (int)PacketSendERROR.정상)
                            {
                                this.DialogResult = DialogResult.OK;                  //완료 결과
                                //this.Close();
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
                    MessageBox.Show("Input Student ID!", "Empty",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
                }
            }
            else
            {
                MessageBox.Show("Input your Team Name!", "Empty", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtBox_Click(object sender, EventArgs e)
        {   //텍스트 박스 눌렀을 때 모두 선택
            ((TextBox)sender).SelectAll();
        }

        private void txtBox_Press(object sender, KeyPressEventArgs e)
        {   //숫자, 백스페이스만 입력 받기
            if(!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void Team_chk_btn_Click(object sender, EventArgs e)
        {
            if (ID_txtBox.Text != "" && team_check == false)
            {
                //팀이름 정보 서버에 보내기
                this.m_Team_Info = new Team_Info();
                this.m_Team_Info.Type = (int)PacketType.팀이름확인;
                this.m_Team_Info.t_name = ID_txtBox.Text;

                Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                this.m_networkstream = m_client.GetStream();
                this.Send();
                
                //(정상인지 오류인지) 해당 팀 이름이 이미 사용중인지 아닌지 정보 받기
                this.Recv();
                Packet packet = (Packet)Packet.Deserialize(this.readBuf);

                if ((int)packet.Type == (int)PacketType.팀이름확인)
                {
                    this.m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuf);
                    if ((int)m_Team_Info.Error == (int)PacketSendERROR.에러)     //이미 존재하는 팀이름
                    {
                        MessageBox.Show("Already Using Team Name!", "Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
                    }
                    else if ((int)m_Team_Info.Error == (int)PacketSendERROR.정상)//존재하지 않음
                    {
                        team_name = ID_txtBox.Text;                             //팀이름 저장해놓기
                        Team_chk_btn.Image = soccerForm.Properties.Resources.green_check;  //체크이미지 바꾸기
                        ID_txtBox.Enabled = false;
                        team_check = true;
                        MessageBox.Show("Valid Team Name!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.None);         //확인 메시지 띄우기
                    }
                } 
            }
            else if (ID_txtBox.Text == "")
                MessageBox.Show("Input your Team Name!", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Click_check(PictureBox p, TextBox t, ref bool chk)
        {
            if (t.Text != "" && chk == false)
            {
                //해당 버튼 옆에 텍스트 박스에 담긴 학번 정보 서버에 보내기
                this.m_Student_ID = new Student_ID();
                this.m_Student_ID.Type = (int)PacketType.학번확인;
                this.m_Student_ID.s_id = t.Text;

                Packet.Serialize(this.m_Student_ID).CopyTo(this.sendBuf, 0);
                this.m_networkstream = m_client.GetStream();
                this.Send();

                //(정상인지 오류인지) 해당 학번이 이미 사용중인지 아닌지 정보 받기
                this.Recv();
                Packet packet = (Packet)Packet.Deserialize(this.readBuf);

                if((int)packet.Type == (int)PacketType.학번확인)
                {
                    this.m_Student_ID = (Student_ID)Packet.Deserialize(this.readBuf);
                    if( (int)m_Student_ID.Error == (int)PacketSendERROR.에러)     //이미 존재하는 학번
                    {
                        MessageBox.Show("Already Using Student ID : " + m_Student_ID.s_id + "!", "Failed",
                             MessageBoxButtons.OK, MessageBoxIcon.Error);        //에러 메시지 띄우기
                    }
                    else if ((int)m_Student_ID.Error == (int)PacketSendERROR.정상)//존재하지 않음
                    {
                        si.Add(t.Text);                                          //학번 저장해놓기
                        p.Image = soccerForm.Properties.Resources.green_check;   //체크이미지 바꾸기
                        t.Enabled = false;
                        chk = true;
                        MessageBox.Show("Valid Team Student ID : " + m_Student_ID.s_id + "!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.None);          //확인 메시지 띄우기
                    }
                }
            }
            else if (t.Text == "")
            {
                MessageBox.Show("Input Student ID!", "Empty",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
            }
        }
        
        private void id_chk_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;

            if(pic.Name.ToString().Equals(id_chk1.Name))
            {
                Click_check(pic, SI_txtBox1, ref id_check1);
            }
            else if(pic.Name.ToString().Equals(id_chk2.Name))
            {
                Click_check(pic, SI_txtBox2, ref id_check2);
            }
            else if (pic.Name.ToString().Equals(id_chk3.Name))
            {
                Click_check(pic, SI_txtBox3, ref id_check3);
            }
            else if (pic.Name.ToString().Equals(id_chk4.Name))
            {
                Click_check(pic, SI_txtBox4, ref id_check4);
            }
            else if(pic.Name.ToString().Equals(id_chk5.Name))
            {
                Click_check(pic, SI_txtBox5, ref id_check5);
            }

        }
    }
}
