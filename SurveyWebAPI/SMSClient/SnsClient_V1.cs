using com.cht.messaging.sns.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.cht.messaging.sns
{
    public class SnsClient_V1 : SnsClientImpl
    {

        public SnsClient_V1(String ip, int port)
        {
            m_strSnsIp = ip;
            m_nSnsPort = port;
            //suport(ip, port);
            m_recvBuffer = new byte[IConstants.RECV_BUFFER_SIZE_V1];
        }

        /**
         * 登入SNS Server, 回傳值為true, 表示登入認證成功, 可以進行後續簡訊傳送/查詢命令.如果是false,表示網路障礙或是帳號資訊錯誤
         * @param sdm SubmitDataModel - 傳入SNS Server的參數物件, 必須包含帳號與密碼資訊
         * @return boolean - true, 表示登入認證成功; false, 表示登入認證失敗, 呼叫getLastMessage()取得失敗原因
         */
        public override Boolean login(SubmitDataModel sdm)
        {
            if (m_smsSocket == null)
                if (connectServer() == false)
                    return false;
            try
            {
                string sendMsg = System.Text.Encoding.Default.GetString(sdm.toByteStream_V1());
                var iii = m_smsSocket.Send(System.Text.Encoding.Default.GetBytes(removeSpecialCharactersPath(sendMsg)));
                m_smsSocket.Receive(System.Text.Encoding.Default.GetBytes(removeSpecialCharactersPath(System.Text.Encoding.Default.GetString(m_recvBuffer))));
                RecvDataModel dm = RecvDataModel.parse_v1(m_recvBuffer);
                if (dm.getCode() != 0)
                {
                    m_strLastMessage = "Server return code : " + dm.getCode() + ":" + Encoding.UTF8.GetString(dm.getDesc()).Trim();
                    disconnectServer();
                    return false;
                }
                string rmss = Encoding.UTF8.GetString(dm.getDesc()).Trim();
                return true;
            }
            catch (Exception e)
            {
                m_strLastMessage = "Exception occurs in login(" + e.Message + ")";
                disconnectServer();
                return false;
            }
        }

        private static string removeSpecialCharactersPath(string str)
        {
            string returnvalue = "";
            string pattern = "([A-Z]|[a-z]|[]|\\d|\\s|[+,-\\\\.*()_\"'|:<>@!#$%^&={}]|[\u4e00-\u9fa5])";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection a = regex.Matches(str);
            for (int i = 0; i < a.Count; i++)
            {
                returnvalue += a[i].Value.ToString();
            }
            return returnvalue;
        }


        /**
         * 傳送簡訊
         * @param sdm - 傳入SNS Server的參數物件, 必須包含手機門號及欲傳送訊息
         * @return - null, 表示網路異常造成運作中斷, 呼叫getLastMessage()取得失敗原因; 如果執行成功, 透過回傳物件的getCode()與getDesc(), 判斷訊息傳送是否成功
         */
        public override RecvDataModel submitMessage(SubmitDataModel sdm)
        {
            try
            {
                RecvDataModel dm = null;
                string sendMsg = System.Text.Encoding.Default.GetString(sdm.toByteStream_V1());
                m_smsSocket.Send(System.Text.Encoding.Default.GetBytes(removeSpecialCharactersPath(sendMsg)));
                int return_leng = m_smsSocket.Receive(System.Text.Encoding.Default.GetBytes(removeSpecialCharactersPath(System.Text.Encoding.Default.GetString(m_recvBuffer))));
                if (return_leng == 189)
                    dm = RecvDataModel.parse_v1(m_recvBuffer);
                
                return dm;
            }
            catch (Exception e)
            {
                m_strLastMessage = "Exception occurs in submitMessage(" + e.Message + ")";
                disconnectServer();
                return null;
            }
        }

        /**
         * 查詢簡訊傳送狀態
         * @param sdm 傳入SNS Server的參數物件, 包含受訊手機號碼以及message_id
         * @return null, 表示網路異常造成運作中斷, 呼叫getLastMessage()取得失敗原因; 否則透過回傳物件的getCode()與getDesc(), 判斷訊息的傳送狀態
         */
        public override RecvDataModel qryMessageStatus(SubmitDataModel sdm)
        {
            try
            {
                m_smsSocket.Send(sdm.toByteStream_V1());
                //m_socketOut.Write(sdm.toByteStream_V1());
                //m_socketOut.Flush();
                //m_socketIn = new DataInputStream(m_smsSocket.getInputStream());
                //m_socketIn.readFully(m_recvBuffer);
                m_smsSocket.Receive(m_recvBuffer);
                RecvDataModel dm = RecvDataModel.parse_v1(m_recvBuffer);
                return dm;
            }
            catch (Exception e)
            {
                m_strLastMessage = "Exception occurs in qryMessageStatus(" + e.Message + ")";
                disconnectServer();
                return null;
            }
        }

        /**
         * 接收簡訊
         * @param sdm - 傳入SNS Server的參數物件
         * @return - null, 表示網路異常造成運作中斷, 呼叫getLastMessage()取得失敗原因; 如果執行成功, 透過回傳物件的getCode()與getDesc(), 判斷訊息傳送是否成功
         */
        public override RecvDataModel getMessage(SubmitDataModel sdm)
        {
            try
            {
                m_smsSocket.Send(sdm.toByteStream_V1());
                m_smsSocket.Receive(m_recvBuffer);
                RecvDataModel dm = RecvDataModel.parse_v1(m_recvBuffer);
                return dm;
            }
            catch (Exception e)
            {
                m_strLastMessage = "Exception occurs in getMessage(" + e.Message + ")";
                disconnectServer();
                return null;
            }
        }


        /**
         * 中斷與SNS Server的連線
         */
        public override void logout()
        {
            disconnectServer();
        }
    }
}
