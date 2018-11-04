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
    public partial class Client_Setting : Form
    {
        private TcpClient m_client = null;                                  //클라이언트
        private NetworkStream m_networkstream = default(NetworkStream);     //네트워크 스트림

        //버퍼
        private byte[] sendBuf = new byte[1024 * 4];   //보내기 버퍼
        private byte[] readBuf = new byte[1024 * 4];   //읽기 버퍼

        private string team_name = "";
        public string team_pw = "";
        private List<string> student_List = new List<string>();
        private int st_count = 0;


        public Team_Info m_Team_Info;
        public Student_ID m_Student_ID;
        public Change_Team_Info m_Change_Team_Info;
        public Change_SI_Info m_Change_SI_Info;

        bool team_check = false;
        bool id_check1 = false;
        bool id_check2 = false;
        bool id_check3 = false;
        bool id_check4 = false;
        bool id_check5 = false;

        bool plus_check = false;

        public TcpClient SetPacket() { return m_client; }
        public string loginID() { return team_name; }

        public Client_Setting()
        {
            InitializeComponent();
        }
        public Client_Setting(Form1 parentForm)
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

        private void Client_Setting_Load(object sender, EventArgs e)
        {
            //정보 텍스트 박스 비활
            SI_txtBox1.Enabled = false;
            SI_txtBox2.Enabled = false;
            SI_txtBox3.Enabled = false;
            SI_txtBox4.Enabled = false;
            SI_txtBox5.Enabled = false;

            //해당 팀 정보 보여주기
            ID_txtBox.Text = team_name;

            //해당 팀 정보 불러오기 
            this.m_Team_Info = new Team_Info();
            this.m_Team_Info.Type = (int)PacketType.로그인정보확인;
            this.m_Team_Info.t_name = team_name;

            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
            this.m_networkstream = m_client.GetStream();
            this.Send();

            this.Recv();
            Packet pack = (Packet)Packet.Deserialize(this.readBuf);

            if ((int)pack.Type == (int)PacketType.로그인정보확인)
            {
                this.m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuf);
                this.team_pw = this.m_Team_Info.pw;
                this.st_count = this.m_Team_Info.stCount;

            }

            //서버한테 해당 팀이름 정보 보내주기
            this.m_Student_ID = new Student_ID();
            this.m_Student_ID.Type = (int)PacketType.학번정보;
            this.m_Student_ID.t_name = team_name;

            Packet.Serialize(this.m_Student_ID).CopyTo(this.sendBuf, 0);
            this.m_networkstream = m_client.GetStream();
            this.Send();


            for (int i = 0; i < st_count; i++)
            {

                //해당 팀의 팀원수 만큼 학번 정보 불러오기
                this.Recv();
                pack = (Packet)Packet.Deserialize(this.readBuf);
                if ((int)pack.Type == (int)PacketType.학번정보)
                {
                    this.m_Student_ID = (Student_ID)Packet.Deserialize(this.readBuf);
                    this.student_List.Add(this.m_Student_ID.s_id);

                    switch (i)
                    {
                        case 0:
                            SI_txtBox1.Text = student_List[i];
                            id_chk1.Image = soccerForm.Properties.Resources.change;
                            SI_txtBox1.Enabled = true;
                            break;
                        case 1:
                            SI_txtBox2.Text = student_List[i];
                            id_chk2.Image = soccerForm.Properties.Resources.change;
                            SI_txtBox2.Enabled = true;
                            break;
                        case 2:
                            SI_txtBox3.Text = student_List[i];
                            id_chk3.Image = soccerForm.Properties.Resources.change;
                            SI_txtBox3.Enabled = true;
                            break;
                        case 3:
                            SI_txtBox4.Text = student_List[i];
                            id_chk4.Image = soccerForm.Properties.Resources.change;
                            SI_txtBox4.Enabled = true;
                            break;
                        case 4:
                            SI_txtBox5.Text = student_List[i];
                            id_chk5.Image = soccerForm.Properties.Resources.change;
                            SI_txtBox5.Enabled = true;
                            break;
                    }

                }

            }
        }

        private void register_btn_Click(object sender, EventArgs e)
        {
            //실제 패스워드와 같은지 확인
            if (PW_txtBox.Text.Equals(team_pw))
            {   //패스워드 확인과 같은지 확인
                if (PW_txtBox.Text.Equals(Confirm_txtBox.Text))
                {
                    Image compareImage = soccerForm.Properties.Resources.green_check;
                    //팀이름 정보를 수정했으면
                    MemoryStream Team = new MemoryStream();
                    Team_chk_btn.Image.Save(Team, System.Drawing.Imaging.ImageFormat.Png);
                    String one0 = Convert.ToBase64String(Team.ToArray());
                    Team.Position = 0;

                    compareImage.Save(Team, System.Drawing.Imaging.ImageFormat.Png);
                    String two0 = Convert.ToBase64String(Team.ToArray());
                    Team.Close();
                    if (one0.Equals(two0))
                    {
                        //팀이름 정보 서버에 보내기
                        this.m_Change_Team_Info = new Change_Team_Info();
                        this.m_Change_Team_Info.Type = (int)PacketType.팀이름수정;
                        this.m_Change_Team_Info.old_tName = team_name;
                        this.m_Change_Team_Info.new_tName = ID_txtBox.Text;

                        Packet.Serialize(this.m_Change_Team_Info).CopyTo(this.sendBuf, 0);
                        this.m_networkstream = m_client.GetStream();
                        this.Send();

                        team_name = ID_txtBox.Text;
                    }
                    //학번1 정보 수정했으면 
                    MemoryStream student1 = new MemoryStream();
                    id_chk1.Image.Save(student1, System.Drawing.Imaging.ImageFormat.Png);
                    String one1 = Convert.ToBase64String(student1.ToArray());
                    student1.Position = 0;

                    compareImage.Save(student1, System.Drawing.Imaging.ImageFormat.Png);
                    String two1 = Convert.ToBase64String(student1.ToArray());
                    student1.Close();
                    if (one1.Equals(two1))
                    {
                        this.m_Change_SI_Info = new Change_SI_Info();
                        this.m_Change_SI_Info.Type = (int)PacketType.학번수정;
                        this.m_Change_SI_Info.tName = team_name;
                        this.m_Change_SI_Info.old_SI = student_List[0];
                        this.m_Change_SI_Info.new_SI = SI_txtBox1.Text;

                        Packet.Serialize(this.m_Change_SI_Info).CopyTo(this.sendBuf, 0);
                        this.m_networkstream = m_client.GetStream();
                        this.Send();

                    }
                    //학번2 정보 수정했으면
                    MemoryStream student2 = new MemoryStream();
                    id_chk2.Image.Save(student2, System.Drawing.Imaging.ImageFormat.Png);
                    String one2 = Convert.ToBase64String(student2.ToArray());
                    student2.Position = 0;

                    compareImage.Save(student2, System.Drawing.Imaging.ImageFormat.Png);
                    String two2 = Convert.ToBase64String(student2.ToArray());
                    student2.Close();
                    if (one2.Equals(two2))
                    {
                        this.m_Change_SI_Info = new Change_SI_Info();
                        this.m_Change_SI_Info.Type = (int)PacketType.학번수정;
                        this.m_Change_SI_Info.tName = team_name;
                        if (student_List.Count - 1 < 1) this.m_Change_SI_Info.old_SI = "-1";
                        else this.m_Change_SI_Info.old_SI = student_List[1];
                        this.m_Change_SI_Info.new_SI = SI_txtBox2.Text;

                        Packet.Serialize(this.m_Change_SI_Info).CopyTo(this.sendBuf, 0);
                        this.m_networkstream = m_client.GetStream();
                        this.Send();
                    }
                    //학번3 정보 수정했으면
                    MemoryStream student3 = new MemoryStream();
                    id_chk3.Image.Save(student3, System.Drawing.Imaging.ImageFormat.Png);
                    String one3 = Convert.ToBase64String(student3.ToArray());
                    student3.Position = 0;

                    compareImage.Save(student3, System.Drawing.Imaging.ImageFormat.Png);
                    String two3 = Convert.ToBase64String(student3.ToArray());
                    student3.Close();
                    if (one3.Equals(two3))
                    {
                        this.m_Change_SI_Info = new Change_SI_Info();
                        this.m_Change_SI_Info.Type = (int)PacketType.학번수정;
                        this.m_Change_SI_Info.tName = team_name;
                        if (student_List.Count - 1 < 2) this.m_Change_SI_Info.old_SI = "-1";
                        else this.m_Change_SI_Info.old_SI = student_List[2];
                        this.m_Change_SI_Info.new_SI = SI_txtBox3.Text;

                        Packet.Serialize(this.m_Change_SI_Info).CopyTo(this.sendBuf, 0);
                        this.m_networkstream = m_client.GetStream();
                        this.Send();
                    }
                    //학번4 정보 수정했으면
                    MemoryStream student4 = new MemoryStream();
                    id_chk4.Image.Save(student4, System.Drawing.Imaging.ImageFormat.Png);
                    String one4 = Convert.ToBase64String(student4.ToArray());
                    student4.Position = 0;

                    compareImage.Save(student4, System.Drawing.Imaging.ImageFormat.Png);
                    String two4 = Convert.ToBase64String(student4.ToArray());
                    student4.Close();
                    if (one4.Equals(two4))
                    {
                        this.m_Change_SI_Info = new Change_SI_Info();
                        this.m_Change_SI_Info.Type = (int)PacketType.학번수정;
                        this.m_Change_SI_Info.tName = team_name;
                        if (student_List.Count - 1 < 3) this.m_Change_SI_Info.old_SI = "-1";
                        else this.m_Change_SI_Info.old_SI = student_List[3];
                        this.m_Change_SI_Info.new_SI = SI_txtBox4.Text;

                        Packet.Serialize(this.m_Change_SI_Info).CopyTo(this.sendBuf, 0);
                        this.m_networkstream = m_client.GetStream();
                        this.Send();
                    }
                    //학번5 정보 수정했으면
                    MemoryStream student5 = new MemoryStream();
                    id_chk5.Image.Save(student5, System.Drawing.Imaging.ImageFormat.Png);
                    String one5 = Convert.ToBase64String(student5.ToArray());
                    student5.Position = 0;

                    compareImage.Save(student5, System.Drawing.Imaging.ImageFormat.Png);
                    String two5 = Convert.ToBase64String(student5.ToArray());
                    student5.Close();
                    if (one5.Equals(two5))
                    {
                        this.m_Change_SI_Info = new Change_SI_Info();
                        this.m_Change_SI_Info.Type = (int)PacketType.학번수정;
                        this.m_Change_SI_Info.tName = team_name;
                        if (student_List.Count - 1 < 4) this.m_Change_SI_Info.old_SI = "-1";
                        else this.m_Change_SI_Info.old_SI = student_List[4];
                        this.m_Change_SI_Info.new_SI = SI_txtBox5.Text;

                        Packet.Serialize(this.m_Change_SI_Info).CopyTo(this.sendBuf, 0);
                        this.m_networkstream = m_client.GetStream();
                        this.Send();
                    }


                    //팀인워수 정보 보내기
                    this.m_Team_Info = new Team_Info();
                    this.m_Team_Info.Type = (int)PacketType.팀인원수수정;
                    this.m_Team_Info.t_name = team_name;
                    this.m_Team_Info.stCount = st_count;

                    Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
                    this.m_networkstream = m_client.GetStream();
                    this.Send();

                    this.DialogResult = DialogResult.OK;                  //완료 결과
                }
                else
                {
                    MessageBox.Show("Password Incorrect!", "Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
                }
            }
            else
            {
                MessageBox.Show("Password Incorrect!", "Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
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

                if ((int)packet.Type == (int)PacketType.학번확인)
                {
                    this.m_Student_ID = (Student_ID)Packet.Deserialize(this.readBuf);
                    if ((int)m_Student_ID.Error == (int)PacketSendERROR.에러)     //이미 존재하는 학번
                    {
                        MessageBox.Show("Already Using Student ID : " + m_Student_ID.s_id + "!", "Failed",
                             MessageBoxButtons.OK, MessageBoxIcon.Error);        //에러 메시지 띄우기
                    }
                    else if ((int)m_Student_ID.Error == (int)PacketSendERROR.정상)//존재하지 않음
                    {
                        p.Image = soccerForm.Properties.Resources.green_check;   //체크이미지 바꾸기
                        t.Enabled = false;
                        chk = true;
                        MessageBox.Show("Valid Team Student ID : " + m_Student_ID.s_id + "!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.None);          //확인 메시지 띄우기
                        if (plus_check)
                        {
                            ++st_count;
                            plus_check = false;
                        }
                    }
                }
            }
            else if (t.Text == "")
            {
                MessageBox.Show("Input Student ID!", "Empty",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);       //에러 메시지 띄우기
            }
        }



        private void txtBox_Press(object sender, KeyPressEventArgs e)
        {   //숫자, 백스페이스만 입력 받기
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void txtBox_Click(object sender, MouseEventArgs e)
        {
            //텍스트 박스 눌렀을 때 모두 선택
            ((TextBox)sender).SelectAll();
        }

        private void id_chk_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;

            MemoryStream ms = new MemoryStream();
            pic.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            String one = Convert.ToBase64String(ms.ToArray());
            ms.Position = 0;

            Image aa = soccerForm.Properties.Resources.change;
            aa.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            String two = Convert.ToBase64String(ms.ToArray());
            ms.Close();

            if (one.Equals(two))
            {
                if (pic.Name.ToString().Equals(id_chk1.Name))
                {
                    Click_check(pic, SI_txtBox1, ref id_check1);
                }
                else if (pic.Name.ToString().Equals(id_chk2.Name))
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
                else if (pic.Name.ToString().Equals(id_chk5.Name))
                {

                    Click_check(pic, SI_txtBox5, ref id_check5);
                }

            }
            else
            {

                MemoryStream ms1 = new MemoryStream();
                pic.Image.Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
                String one1 = Convert.ToBase64String(ms1.ToArray());
                ms1.Position = 0;

                Image bb = soccerForm.Properties.Resources.green_check;
                bb.Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
                String two1 = Convert.ToBase64String(ms1.ToArray());
                ms1.Close();
                if (!(one1.Equals(two1)))
                {
                    pic.Image = soccerForm.Properties.Resources.change;
                    plus_check = true;
                    if (pic.Name.ToString().Equals(id_chk1.Name))
                    {
                        SI_txtBox1.Enabled = true;
                    }
                    else if (pic.Name.ToString().Equals(id_chk2.Name))
                    {
                        SI_txtBox2.Enabled = true;
                    }
                    else if (pic.Name.ToString().Equals(id_chk3.Name))
                    {
                        SI_txtBox3.Enabled = true;
                    }
                    else if (pic.Name.ToString().Equals(id_chk4.Name))
                    {
                        SI_txtBox4.Enabled = true;
                    }
                    else if (pic.Name.ToString().Equals(id_chk5.Name))
                    {
                        SI_txtBox5.Enabled = true;
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            PwClientSetting mChild = new PwClientSetting(this);
            if (mChild.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Completed Password Setting!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.None);       //성공 메시지 띄우기
            }

            //해당 팀 정보 불러오기 
            this.m_Team_Info = new Team_Info();
            this.m_Team_Info.Type = (int)PacketType.로그인정보확인;
            this.m_Team_Info.t_name = team_name;

            Packet.Serialize(this.m_Team_Info).CopyTo(this.sendBuf, 0);
            this.m_networkstream = m_client.GetStream();
            this.Send();

            this.Recv();
            Packet pack = (Packet)Packet.Deserialize(this.readBuf);

            if ((int)pack.Type == (int)PacketType.로그인정보확인)
            {
                this.m_Team_Info = (Team_Info)Packet.Deserialize(this.readBuf);
                this.team_pw = this.m_Team_Info.pw;
                this.st_count = this.m_Team_Info.stCount;
            }

        }
    }
}
