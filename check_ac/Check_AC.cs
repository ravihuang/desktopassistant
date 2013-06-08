using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SnmpSharpNet
{
    class Check_AC
    {
        //需要按照oid严格排序
        static string[][] oids = new string[][] {
                new string[]{"摩托罗拉","1.3.6.1.4.1.388.14.3.2.1.12.3.1.33",
                                        "1.3.6.1.4.1.388.14.3.2.1.24.1.1.21"},
                new string[]{"智达康",  "1.3.6.1.4.1.1567.80.6.2.1.42",
                                         "1.3.6.1.4.1.1567.80.6.3.1.9"}
                ,new string[]{"H3C",   "1.3.6.1.4.1.2011.10.2.75.2.1.2.1.8",                                 
                                        "1.3.6.1.4.1.2011.10.2.75.3.1.1.1.12"}                
                ,new string[]{"中兴",    "1.3.6.1.4.1.3902.154.8.11.1.3.1.1.3.3.1.35",
                                         "1.3.6.1.4.1.3902.154.8.11.1.3.1.1.3.1.1.11"}
                ,new string[]{"星网锐捷", "1.3.6.1.4.1.4881.1.1.10.2.56.2.1.1.1.36",
                                         "1.3.6.1.4.1.4881.1.1.10.2.81.6.1.1.10"}  
                ,new string[]{"大唐电信", "1.3.6.1.4.1.5105.80.6.2.1.42",
                                         "1.3.6.1.4.1.5105.80.6.3.1.9"}
                ,new string[]{"Aruba",   "1.3.6.1.4.1.14823.2.2.1.5.2.2.1.1.12",                                  
                                         "1.3.6.1.4.1.14823.2.2.1.5.2.1.7.1.13",
                                         "1.3.6.1.4.1.14823.2.2.1.5.2.1.4.1.19"}
                ,new string[]{"Ruckus",  "1.3.6.1.4.1.25053.1.2.2.1.1.3.1.1.8",
                                         "1.3.6.1.4.1.25053.1.2.2.1.1.2.1.1.10"} 
                ,new string[]{"普天",    "1.3.6.1.4.1.25053.3.1.1.3.3.1.35",
                                         "1.3.6.1.4.1.25053.3.1.1.3.1.1.11"}
            };

        static string[][] enterprise = new string[][]{
            new string[]{"Cisco","9"},            new string[]{"3COM","43"}, 
            new string[]{"D-Link","171"},
            new string[]{"摩托罗拉","388"},       new string[]{"智达康",  "1567"},
            new string[]{"H3C/华为","2011"},      new string[]{"中兴",    "3902"},
            new string[]{"星网锐捷", "4881"} ,    new string[]{"大唐电信", "5105"},
            new string[]{"Buffalo", "8068"} ,
            new string[]{"Aruba",   "14823"},     new string[]{"Ruckus",  "25053"},
            new string[]{"傲天联动","31656"},     new string[]{"宏信","33940"}
        };

        private AgentParameters param;
        private UdpTarget target;
        private string ip = "127.0.0.1";
        private string port = "161";
        private string rc = "public";

        public string guess()
        {
            SnmpV2Packet result = getnext("1.3.6.1.4.1");
            if (result == null)
                return "未知,result=null!";
            Vb vb = result.Pdu.VbList[0];
            string oid = vb.Oid.ToString();
            string[] tmp = oid.Split(new char[] { '.' });
            string eid;
            if (tmp.Length > 8)
                eid = tmp[6];
            else
                return "未知 " + oid;

            for (int i = 0; i < enterprise.Length; i++)
            {
                if (enterprise[i][1].Equals(eid))
                    return enterprise[i][0];
            }
            return eid;
        }

        public bool getInput(string outp)
        {
            while (true)
            {
                Console.WriteLine(outp);
                Console.Write("输入q退出，y继续：");
                string io = Console.ReadLine();
                if (io.StartsWith("q"))
                    return false;
                if (io.StartsWith("y"))
                    return true;
            }
        }
        public SnmpV2Packet getnext(string oid)
        {
            Pdu pdu = new Pdu(PduType.GetNext);
            pdu.VbList.Add(oid);
            SnmpV2Packet result = null;
            try
            {
                result = (SnmpV2Packet)target.Request(pdu, param);
            }
            finally { }
            return result;
        }


        public bool v3()
        {
            init();
            bool isSupported = true;
            string msg = "";
            for (int i = 0; i < oids.Length; i++)
            {
                string name = oids[i][0];
                isSupported = true;
                Console.Write("is " + name + "?");
                for (int j = 1; j < oids[i].Length; j++)
                {
                    SnmpV2Packet result = getnext(oids[i][j]);
                    if (result == null)
                        return getInput("Agent连不上，请检查配置");

                    Vb vb = result.Pdu.VbList[0];
                    //返回end of mib view时，直接处理下一个
                    if (vb.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                    {
                        Console.WriteLine(" No!");
                        //如果oid是严格排过序的，则遇到一个end of mib view时就可以停止check
                        //可能会agent不符合协议的遇到的情况
                        return getInput("该AC目前不支持，生产厂商可能是:" + guess());
                        //isSupported = false;
                        //break;
                    }
                    else if (vb.Oid.ToString().Contains(oids[i][j]))
                    {
                        Console.Write(".");
                    }
                    else
                    {
                        string oid = vb.Oid.ToString();
                        int id = getIndex(oid, ref i);
                        if (id == -1)
                            return this.getInput("内部错误 split：" + oid);
                        if (i < oids.Length)
                        {
                            i --;
                            isSupported = false;
                            break;
                        }

                        Console.WriteLine(" No!");
                        return getInput("该AC目前不支持，生产厂商可能是:" + guess());
                    }
                }
                if (isSupported)
                {
                    Console.WriteLine("Yes!");
                    msg = "该AC可识别，生产厂商可能是:" + name;
                    break;
                }
                else
                {
                    Console.WriteLine(" No!");
                }
            }
            if (!isSupported)
            {
                msg = "该AC目前不支持，生产厂商可能是:" + guess();
            }
            target.Close();

            return getInput(msg);
        }
        //i 当前遍历的位置 
        //return -1 代表split长度不正确
        //       否则，代表目标位置
        private int getIndex(string oid, ref int i)
        {
            string[] tmp = oid.Split(new char[] { '.' });
            string eid;
            if (tmp.Length > 8)
                eid = tmp[6];
            else
                return -1;

            for (; i < oids.Length; i++)
            {
                if (oids[i][1].Contains("1.3.6.1.4.1." + eid + "."))
                {
                    return i;
                }
            }
            return i;
        }

        public void init()
        {
            Console.Write("please input agent ip[" + ip + "]:");
            String tmp = Console.ReadLine();
            if (tmp.StartsWith("debug"))
            {
                for (int i = 0; i < oids.Length; i++)
                {
                    Console.WriteLine(oids[i][0]);
                }
                Console.Write("please input agent ip[" + ip + "]:");
                tmp = Console.ReadLine();
            }
            ip = tmp.Length > 0 ? tmp.Trim() : ip;

            Console.Write("please input agent port[" + port + "]:");
            tmp = Console.ReadLine();
            port = tmp.Length > 0 ? tmp.Trim() : port;

            Console.Write("please input agent read community[" + rc + "]:");
            tmp = Console.ReadLine();
            rc = tmp.Length > 0 ? tmp.Trim() : rc;

            Console.WriteLine(ip + ":" + port + ":" + rc);

            OctetString community = new OctetString(rc);
            param = new AgentParameters(community);
            param.Version = SnmpVersion.Ver2;
            IpAddress agent = new IpAddress(ip);
            target = new UdpTarget((IPAddress)agent, int.Parse(port), 10000, 2);

        }
        //简单实现
        public bool v1()
        {
            init();
            bool isSupported = true;
            string msg = "";
            for (int i = 0; i < oids.Length; i++)
            {
                string name = oids[i][0];
                isSupported = true;
                Console.Write("is " + name + "?");
                for (int j = 1; j < oids[i].Length; j++)
                {
                    SnmpV2Packet result = getnext(oids[i][j]);
                    if (result == null)
                        return getInput("Agent连不上，请检查配置");

                    Vb vb = result.Pdu.VbList[0];
                    if (result.Pdu.ErrorStatus != 0 ||
                        !vb.Oid.ToString().Contains(oids[i][j]) ||
                        vb.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW ||
                        vb.Value == null)
                    {
                        isSupported = false;
                        break;
                    }
                    else
                        Console.Write(".");
                }
                if (isSupported)
                {
                    msg = "该AC可识别，生产商是:" + name;
                    break;
                }
                else
                {
                    Console.WriteLine(" No!");
                }
            }
            target.Close();
            if (!isSupported)
            {
                msg = "该AC不支持！";
            }
            return getInput(msg);
        }

        //genN 一次下发多个vb,agent可能会不支持
        public void v2(string ip, string port, string rc)
        {
            init();
            bool isSupported = true;
            string msg = "";
            for (int i = 0; i < oids.Length; i++)
            {
                string name = oids[i][0];
                isSupported = true;
                Console.Write("is " + name + "?");
                Pdu pdu = new Pdu(PduType.GetNext);
                for (int j = 1; j < oids[i].Length; j++)
                {
                    pdu.VbList.Add(oids[i][j]);
                }
                SnmpV2Packet result = null;
                try
                {
                    Console.Write(".");
                    result = (SnmpV2Packet)target.Request(pdu, param);
                }
                finally { }
                if (result == null)
                {
                    Console.Write("Agent连不上，请检查配置,输入q退出：");
                    while (!Console.ReadLine().StartsWith("q"))
                    {
                        Console.Write("Agent连不上，请检查配置,输入q退出：");
                    }
                    return;
                }
                for (int z = 0; z < result.Pdu.VbList.Count; z++)
                {
                    Vb vb = result.Pdu.VbList[z];
                    if (result.Pdu.ErrorStatus != 0 ||
                        !vb.Oid.ToString().Contains(oids[i][z]) ||
                        vb.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW ||
                        vb.Value == null)
                    {
                        isSupported = false;
                        break;
                    }
                }
                if (isSupported)
                {
                    msg = "该AC可识别，生产商是:" + name;
                    break;
                }
                else
                {
                    Console.WriteLine(" No!");
                }
            }
            target.Close();
            if (!isSupported)
            {
                msg = "该AC不支持！";

            }
            Console.Write(msg + " 输入q退出：");
            while (!Console.ReadLine().StartsWith("q"))
            {
                Console.Write(msg + " 输入q退出：");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(
@"##########################################
# Description:判断AC是否在已支持的范围
# Usage：按照提示依次输入AC的IP
#        snmp agent 端口号
#        read community   
# 日期： 20130607
# 工具支持：黄小勇
# ！！！！内部使用，请勿外传！！！！
##########################################");

            Check_AC ac = new Check_AC();
            while (ac.v3()) ;

        }
    }
}
