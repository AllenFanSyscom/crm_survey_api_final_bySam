using com.cht.messaging.sns.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace com.cht.messaging.sns
{
    public abstract class SnsClientImpl:SubmitDataModel
    {
        protected String m_strSnsIp = "";
        protected int m_nSnsPort = 0;
        protected byte[] m_recvBuffer = null;
        //protected StreamWriter m_socketOut = null;
        //protected StreamReader m_socketIn = null;
        protected Socket m_smsSocket = null;
        protected String m_strLastMessage = "";      

        public abstract Boolean login(SubmitDataModel sdm);
        public abstract RecvDataModel submitMessage(SubmitDataModel sdn);
        public abstract RecvDataModel qryMessageStatus(SubmitDataModel sdm);
        public abstract RecvDataModel getMessage(SubmitDataModel sdm);
        public abstract void logout();
        public SnsClientImpl()
        {

        }
        //public SnsClientImpl(String ip, int port)
        //{
        //    m_strSnsIp = ip;
        //    m_nSnsPort = port;
        //}

        /**
         * 回傳上次函示執行失敗的錯誤訊息
         * @return 上次函示執行失敗的錯誤訊息
         */
        public new String getLastMessage()
        {
            return m_strLastMessage;
        }

        protected Boolean connectServer()
        {
            Boolean bRet = true;

            try
            {
                if ((m_smsSocket == null) || m_smsSocket.Connected == true)
                {                    
                    m_smsSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
                    IPEndPoint ip = new IPEndPoint(IPAddress.Parse(m_strSnsIp), m_nSnsPort);                               
                    m_smsSocket.SendTimeout=10000;
                    m_smsSocket.Connect(ip);
                    bRet = m_smsSocket.Connected;
                }
            }
            catch (Exception e)
            {
                m_strLastMessage = "Connect server fail(" + e.Message + ")";
                disconnectServer();
                bRet = false;
            }
            return bRet;
        }

        protected void disconnectServer()
        {
            try
            {
                if (m_smsSocket != null) m_smsSocket.Close(); m_smsSocket = null;
                //if (m_socketOut != null) m_socketOut.Close(); m_socketOut = null;
                //if (m_socketIn != null) m_socketIn.Close(); m_socketIn = null;
            }
            catch (Exception e)
            {
                m_strLastMessage = "disconnect server fail(" + e.Message + ")";
            }
        }
    }
}
