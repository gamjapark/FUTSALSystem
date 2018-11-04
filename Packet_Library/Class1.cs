using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Packet_Library
{
    public enum PacketType
    {
        팀이름확인 = 0,
        학번확인,
        학번저장,
        로그인정보저장,
        패스워드확인,
        팀이름리스트,
        메시지송신,
        송신리스트, 
        수신리스트,
        수신확인, 
        메시지개수,
        로그아웃,
        팀인원수,
        매칭,
        혼자,
        예약정보,
        예약가능,
        예약내역,
        예약취소,
        로그인정보확인,
        학번정보,
        팀이름수정,
        학번수정,
        팀인원수수정,
        전적,
        이전경기결과,
        이전경기결과확인,
        이전경기결과제출,
        불발,
        패스워드변경,
        최근결과

    }
    public enum PacketSendERROR
    {
        정상 = 0, 에러, 존재
    }

    sealed class Binder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            assemblyName = Assembly.GetExecutingAssembly().FullName;
            return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
        }
    }

    [Serializable]
    public class Packet
    {
        public int Length;
        public int Type;
        public int Error;

        public Packet()
        {
            this.Length = 0;
            this.Type = 0;
            this.Error = 0;
        }

        public static byte[] Serialize(Object o)
        {
            MemoryStream ms = new MemoryStream(1024 * 4);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, o);
            return ms.ToArray();
        }
        public static Object Deserialize(byte[] bt)
        {
            MemoryStream ms = new MemoryStream(1024 * 4);
            foreach (byte b in bt)
            {
                ms.WriteByte(b);
            }
            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            Object o = bf.Deserialize(ms);
            ms.Close();
            return o;
        }
    }

    [Serializable]
    public class Change_Team_Info : Packet
    {
        public string old_tName { get; set; }
        public string new_tName { get; set; }
    }

    [Serializable]
    public class Change_SI_Info : Packet
    {
        public string tName { get; set; }
        public string old_SI { get; set; }
        public string new_SI { get; set; }
    }
    [Serializable]
    public class Team_Info : Packet
    {
        public string t_name { get; set; }
        public string pw { get; set; }
        public int stCount { get; set; } = 0;
    }
    [Serializable]
    public class Student_ID : Packet
    {
        public string t_name { get; set; }
        public string s_id { get; set; }
    }
    [Serializable]
    public class team_list : Packet
    {
        public string t_name { get; set; }
        public int count = 0;
    }
    [Serializable]
    public class Msg_list : Packet
    {
        public string send_name { get; set; }
        public string recv_name { get; set; }
        public string text { get; set; }
        public string date { get; set; }
        public int count = 0;
        public int used = 0;
    }
    [Serializable]
    public class Reserve : Packet
    {
        public string send_name { get; set; }
        public string recv_name { get; set; }
        public string date { get; set; }
        public int match_num = 0;
        public int tnum = 0;
        public int time = 0;
        public int used = 0;
        public int cancel = 0;
    }

    [Serializable]
    public class List : Packet
    {
        public string send_name { get; set; }
        public int where_check { get; set; } = 0;
        public int win = 0;
        public int lose = 0;
        public int draw = 0;
    }

    [Serializable]
    public class previous_result : Packet
    {
        public int pre_result { get; set; } // flag 0 = 이전 경기 결과 작성O  1 = 경기 결과 작성X
        public string myTeam { get; set; }
        public string opTeam { get; set; }
        public int time { get; set; }
        public string date { get; set; }
        public int myScore { get; set; }
        public int OPScore { get; set; }
        public string loginID { get; set; }
        public string curDate { get; set; }
    }

    [Serializable]
    public class delete_match : Packet
    {
        public string curDate { get; set; }
    }
}
