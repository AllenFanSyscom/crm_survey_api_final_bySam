using com.cht.messaging.sns.Models;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace com.cht.messaging.sns
{
    public class Sns_Client
    {
        String m_strIp;
        int m_nPort;

        public Sns_Client(String ip, int port)
        {
            m_strIp = ip;
            m_nPort = port;
        }

        // Demonstrate how to submit message(String) to mobile
        /**
         * 傳送訊息的範例Method - 若return_code值為0則表示發訊成功、此時return_desc值為訊息代碼(可用來查詢簡訊是否成功發送)，
         * 若return_code不為0表示發訊失敗，查詢return_desc判斷失敗原因，若無return_code表示無法從SNS Server取得回應，由console訊息判斷失敗原因。
         * @param account String - 登入帳號
         * @param password String - 登入密碼
         * @param recv_msisdn String - 對方手機號碼
         * @param text String - 以符合Big5(中英文)、ASCII(純英數)編碼簡訊內容(String)
         */
        public void submitMessage(String account, String password, String recv_msisdn, String text)
        {
            SnsClient_V1 sns = new SnsClient_V1(m_strIp, m_nPort);
            SubmitDataModel sdm = new SubmitDataModel();
            // Login server
            sdm.setType(IConstants.SERV_LOGIN);
            sdm.setCoding(IConstants.SERV_LOGIN);
            sdm.setLength(IConstants.SERV_LOGIN);
            sdm.setTranType(IConstants.SERV_LOGIN);
            sdm.setAccount(removeSpecialCharactersPath(account));
            sdm.setPassword(removeSpecialCharactersPath(password));
            Log.Debug("submitMessage account:" + account);
            Log.Debug("submitMessage password:" + password);
            Log.Debug("submitMessage recv_msisdn:" + recv_msisdn);
            Log.Debug("submitMessage text:" + text);
            Log.Debug("submitMessage m_strIp:" + m_strIp);
            Log.Debug("submitMessage m_nPort:" + m_nPort);
            Log.Debug("submitMessage sdm.getType:" + sdm.getType());
            Log.Debug("submitMessage sdm.getCoding:" + sdm.getCoding());
            Log.Debug("submitMessage sdm.getLength:" + sdm.getLength());
            Log.Debug("submitMessage sdm.getTranType:" + sdm.getTranType());
            Log.Debug("submitMessage sdm.getAccount:" + sdm.getAccount());
            Log.Debug("submitMessage sdm.getPassword:" + sdm.getPassword());
            if (sns.login(sdm) == false)
            {
                Log.Error("login server fail(" + sns.getLastMessage() + ")");
                //Console.WriteLine("login server fail(" + sns.getLastMessage() + ")");
                sns.logout();
                sdm = null;
                return;
            }
            // Setting up proper parameters and submit it, then get the response message.
            sdm.reset();
            sdm.setType(IConstants.SERV_SUBMIT_MSG);
            sdm.setCoding((byte)1);
            sdm.setRcvMsisdn(recv_msisdn);
            sdm.setTranType(IConstants.DEFAULT_TRAN_TYPE);
            //sdm.setLength((byte)text.getBytes().length);
            sdm.setLength((byte)Encoding.UTF8.GetBytes(text).Length);
            sdm.setMessage(text);           
            RecvDataModel rdm = sns.submitMessage(sdm);
            if (rdm != null)
            {
                Log.Debug("return_code=" + rdm.getCode());
                Log.Debug("return_desc:" + Encoding.UTF8.GetString(rdm.getDesc()).Trim());
                //Console.WriteLine("return_code=" + rdm.getCode());
                //Console.WriteLine("return_desc:" + Encoding.UTF8.GetString(rdm.getDesc()).Trim());
            }
            else  // could not receive response from SNS server
            {
                Log.Error("Submit fail:" + sns.getLastMessage());
                //Console.WriteLine("Submit fail:" + sns.getLastMessage());
            }
            // Logout server
            sns.logout();
            sdm = null;
        }

        // Demonstrate how to submit message(byte[]) to mobile
        /**
         * 傳送訊息的範例Method - 若return_code值為0則表示發訊成功、此時return_desc值為訊息代碼(可用來查詢簡訊是否成功發送)，
         * 若return_code不為0表示發訊失敗，查詢return_desc判斷失敗原因，若無return_code表示無法從SNS Server取得回應，由console訊息判斷失敗原因。
         * @param account String - 登入帳號
         * @param password String - 登入密碼
         * @param recv_msisdn String - 對方手機號碼
         * @param msg - 以符合dcs編碼的簡訊內容(byte[])
         * @param dcs - 編碼值，Big5, ASCII(dcs=0x01)、UCS2(UTF-16BE, dcs=0x08)
         * @param udhi - 具有填入UDH的權限為1，否則為0
         */
        public void submitMessage(String account, String password, String recv_msisdn, byte[] msg, byte dcs, byte udhi)
        {
            SnsClient_V1 sns = new SnsClient_V1(m_strIp, m_nPort);
            SubmitDataModel sdm = new SubmitDataModel();
            // Login server
            sdm.setType(IConstants.SERV_LOGIN);
            sdm.setAccount(account);
            sdm.setPassword(password);
            if (sns.login(sdm) == false)
            {
                Console.WriteLine("login server fail(" + sns.getLastMessage() + ")");
                sns.logout();
                sdm = null;
                return;
            }
            // Setting up proper parameters and submit it, then get the response message.
            sdm.reset();
            sdm.setType(IConstants.SERV_SUBMIT_MSG);
            sdm.setCoding(dcs);
            sdm.setRcvMsisdn(recv_msisdn);
            sdm.setTranType(IConstants.DEFAULT_TRAN_TYPE);
            sdm.setLength((byte)msg.Length);
            sdm.setUdhi(udhi);
            sdm.setMessage(msg);
            RecvDataModel rdm = sns.submitMessage(sdm);
            if (rdm != null)
            {
                Console.WriteLine("return_code=" + rdm.getCode());
                Console.WriteLine("return_desc:" + Encoding.UTF8.GetString(rdm.getDesc()).Trim());
            }
            else  // could not receive response from SNS server
            {
                Console.WriteLine("Submit fail:" + sns.getLastMessage());
            }

            // Logout server
            sns.logout();
            sdm = null;
        }

        // Demonstrate how to query message status
        /**
         * 查詢訊息傳送狀態的範例Method - 若return_code值為0則表示訊息已送達對方，此時return_desc值顯示Successful及傳送成功時間，
         * 若return_code不為0，則表示訊息未送達對方，由return_desc判斷失敗原因，若無return_code表示無法從SNS Server取得回應，由console訊息判斷失敗原因。
         * @param account String - 登入帳號
         * @param password String - 登入密碼
         * @param recv_msisdn String - 對方手機號碼
         * @param msg_id String - 欲查詢簡訊的訊息代碼
         */
        public void qryMessageStatus(String account, String password, String recv_msisdn, String msg_id)
        {
            SnsClient_V1 sns = new SnsClient_V1(m_strIp, m_nPort);
            SubmitDataModel sdm = new SubmitDataModel();
            // Login server
            sdm.setType(IConstants.SERV_LOGIN);
            sdm.setAccount(account);
            sdm.setPassword(password);
            if (sns.login(sdm) == false)
            {
                Console.WriteLine("login server fail(" + sns.getLastMessage() + ")");
                sns.logout();
                sdm = null;
                return;
            }
            // Setting up proper parameters and then submit it, then get the response message.
            sdm.reset();
            sdm.setType(IConstants.SERV_QUERY_STATE);
            sdm.setRcvMsisdn(recv_msisdn);
            sdm.setMessageID(msg_id);
            RecvDataModel rdm = sns.qryMessageStatus(sdm);
            if (rdm != null)
            {
                Console.WriteLine("return_code=" + rdm.getCode());
                Console.WriteLine("return_desc:" + Encoding.UTF8.GetString(rdm.getDesc()).Trim());
            }
            else  // could not receive response from SNS server
            {
                Console.WriteLine("Submit fail:" + sns.getLastMessage());
            }
            // Logout server
            sns.logout();
            sdm = null;
        }

        // Demonstrate how to receive message from mobile
        /**
         * 接收訊息的範例Method - 若return_code值為0則表示接收成功、此時return_desc值為訊息內容
         * 若return_code不為0表示收訊失敗，查詢return_desc判斷失敗原因，若無return_code表示無法從SNS Server取得回應，由console訊息判斷失敗原因。
         * @param account String - 登入帳號
         * @param password String - 登入密碼
         */
        public void getMessage(String account, String password)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            SnsClient_V1 sns = new SnsClient_V1(m_strIp, m_nPort);
            SubmitDataModel sdm = new SubmitDataModel();
            // Login server
            sdm.setType(IConstants.SERV_LOGIN);
            sdm.setAccount(account);
            sdm.setPassword(password);
            if (sns.login(sdm) == false)
            {
                Console.WriteLine("login server fail(" + sns.getLastMessage() + ")");
                sns.logout();
                sdm = null;
                return;
            }
            // Setting up proper parameters and submit it, then get the response message.
            sdm.reset();
            sdm.setType(IConstants.SERV_GET_MSG);
            //sdm.setType(IConstants.SERV_QUERY_STATE);
            RecvDataModel rdm = sns.getMessage(sdm);
            int code = rdm.getCode();
            if (code == 0)
            {
                Console.WriteLine("return_code成功=" + rdm.getCode());
            }
            if (rdm != null)
            {
                // 如果收到的訊息是中文的話，需以UnicodeBig編碼
                if (rdm.getCoding() == 8)
                {
                    String strMsg = null;
                    try
                    {                        
                        strMsg = Encoding.BigEndianUnicode.GetString(rdm.getDesc());
                        Console.WriteLine("return_code=" + rdm.getCode());                        
                        Console.WriteLine("return_desc:" + strMsg.Trim());
                        Console.WriteLine("return_getRecvMsisdn:" + rdm.getRecvMsisdn().Trim());
                        Console.WriteLine("return_getSendMsisdn:" + rdm.getSendMsisdn().Trim());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                // 收到非中文訊息則直接印出
                else
                {
                    Console.WriteLine("return_code=" + rdm.getCode());
                    Console.WriteLine("return_desc:" + Encoding.UTF8.GetString(rdm.getDesc()).Trim());
                    Console.WriteLine("return_getRecvMsisdn:" +rdm.getRecvMsisdn().Trim());
                    Console.WriteLine("return_getSendMsisdn:" + rdm.getSendMsisdn().Trim());
                }
            }
            else  // could not receive response from SNS server
            {
                Console.WriteLine("Submit fail:" + sns.getLastMessage());
            }
            // Logout server
            sns.logout();
            sdm = null;
        }

        // Demonstrate how to submit long(concatenated) message to mobile
        /**
         * 傳送訊息的範例Method - 若return_code值為0則表示發訊成功、此時return_desc值為訊息代碼(可用來查詢簡訊是否成功發送)，
         * 若return_code不為0表示發訊失敗，查詢return_desc判斷失敗原因，若無return_code表示無法從SNS Server取得回應，由console訊息判斷失敗原因。
         * @param account String - 登入帳號
         * @param password String - 登入密碼
         * @param recv_msisdn String - 對方手機號碼
         */
        public void submitLongMessage(String account, String password, String recv_msisdn)
        {
            //byte[] by1 = new BigInteger("0500030102016211662F53F070634EBA4E0D662F4E2D570B4EBA6211662F53F070634EBA4E0D662F4E2D570B4EBA6211662F53F070634EBA4E0D662F4E2D570B4EBA6211662F53F070634EBA4E0D662F4E2D570B4EBA6211662F53F070634EBA4E0D662F4E2D570B4EBA6211662F53F070634EBA4E0D662F4E2D570B4EBA6211662F53F070634EBA4E0D662F", 16).toByteArray();
            //byte[] by2 = new BigInteger("0500030102024E2D570B4EBA6211662F", 16).toByteArray();

            byte[] by1 = new byte[140];
            byte[] by2 = new byte[140];
            try
            {
                //byte[] byTemp = "北台灣氣溫約19至24度，整天都比較濕涼，出門別忘了帶傘及加件外套；中南部受東北風影響則較小，僅山區一帶有零星短暫雨，平地可維持多雲".getBytes("UTF-16BE");
                byte[] byTemp = Encoding.BigEndianUnicode.GetBytes("北台灣氣溫約19至24度，整天都比較濕涼，出門別忘了帶傘及加件外套；中南部受東北風影響則較小，僅山區一帶有零星短暫雨，平地可維持多雲");
                by1[0] = 0x06; by1[1] = 0x08; by1[2] = 0x04; by1[3] = 0x00; by1[4] = 0x4F; by1[5] = 0x02; by1[6] = 0x01;
                Buffer.BlockCopy(byTemp, 0, by1, 7, byTemp.Length);
                //byTemp = Encoding.UTF8.GetString("的天氣，氣溫只略為下降，大致在21到29度間，日夜溫差仍稍大。".getBytes("UTF-16BE");
                byTemp = Encoding.BigEndianUnicode.GetBytes("的天氣，氣溫只略為下降，大致在21到29度間，日夜溫差仍稍大。");
                by2[0] = 0x06; by2[1] = 0x08; by2[2] = 0x04; by2[3] = 0x00; by2[4] = 0x4F; by2[5] = 0x02; by2[6] = 0x02;
                Buffer.BlockCopy(byTemp, 0, by2, 7, byTemp.Length);
            }    
            catch (Exception) { }

            submitMessage(removeSpecialCharactersPath(account), removeSpecialCharactersPath(password), removeSpecialCharactersPath(recv_msisdn), by1, (byte)0x08, (byte)0x01);
            submitMessage(removeSpecialCharactersPath(account), removeSpecialCharactersPath(password), removeSpecialCharactersPath(recv_msisdn), by2, (byte)0x08, (byte)0x01);
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
        // Demonstrate how to submit multi message to mobile using always connection with retry mechanism 
        /**
         * 傳送訊息的範例Method - 若return_code值為0則表示發訊成功、此時return_desc值為訊息代碼(可用來查詢簡訊是否成功發送)，
         * 若return_code不為0表示發訊失敗，查詢return_desc判斷失敗原因，若無return_code表示無法從SNS Server取得回應，由console訊息判斷失敗原因。
         * @param account String - 登入帳號
         * @param password String - 登入密碼
         * @param recv_msisdn Vector<String> - 對方手機號碼
         * @param text String - 簡訊內容
         */
        public void submitMultiMessage(String account, String password, String[] recv_msisdn, String text)
        {
            if ((recv_msisdn == null) || (recv_msisdn.Length <= 0))
                return;

            //final int MAX_RETRY = 2;
            int MAX_RETRY = 2;
            int nRetry;
            //boolean bLogin = false;
            Boolean bLogin = false;
            String strSendMsisdn;

            SnsClient_V1 sns = new SnsClient_V1(m_strIp, m_nPort);
            SubmitDataModel sdm = new SubmitDataModel();

            for (int nn = 0; nn < recv_msisdn.Length; nn++)
            {
                nRetry = 0;
                while ((bLogin == false) && (nRetry < MAX_RETRY))
                {
                    // Login server
                    nRetry++;
                    sdm.reset();
                    sdm.setType(IConstants.SERV_LOGIN);
                    sdm.setAccount(removeSpecialCharactersPath(account));
                    sdm.setPassword(removeSpecialCharactersPath(password));
                    if ((bLogin = sns.login(sdm)) == false)
                        sns.logout();

                    try { Thread.Sleep(1000); }
                    catch (Exception) { }
                }

                if (bLogin == false)
                {
                    Console.WriteLine("login server fail(" + sns.getLastMessage() + ")");
                    continue;
                }

                strSendMsisdn = recv_msisdn[nn];
                // Setting up proper parameters and submit it, then get the response message.
                sdm.reset();
                sdm.setType(IConstants.SERV_SUBMIT_MSG);
                sdm.setCoding((byte)1);
                sdm.setRcvMsisdn(strSendMsisdn);
                sdm.setTranType(IConstants.DEFAULT_TRAN_TYPE);
                //sdm.setLength((byte)text.getBytes().length);
                if (ContainsUnicodeCharacter(text))
                    sdm.setLength((byte)Encoding.Unicode.GetBytes(text).Length);
                else   
                    sdm.setLength((byte)Encoding.ASCII.GetBytes(text).Length);
                sdm.setMessage(text);
                RecvDataModel rdm = sns.submitMessage(sdm);
                Console.WriteLine(strSendMsisdn + " sent status:");
                if (rdm != null)
                {
                    Console.WriteLine("return_code=" + rdm.getCode());
                    Console.WriteLine("return_desc:" + Encoding.UTF8.GetString(rdm.getDesc()).Trim());
                }
                else  // could not receive response from SNS server
                {
                    Console.WriteLine("Submit fail:" + sns.getLastMessage());
                    bLogin = false;
                }
            }
            // Logout server
            sns.logout();
            sdm = null;
        }
        public bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;
            return input.Any(c => c > MaxAnsiCode);
        }
    }
}
