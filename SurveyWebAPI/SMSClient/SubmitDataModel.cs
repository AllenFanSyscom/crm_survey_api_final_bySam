using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.cht.messaging.sns.Models
{

    public interface IConstants
    {
        /**
         * 發訊封包size
         */
        public static  int SUBMIT_BUFFER_SIZE_V1 = 217;
        public static  int SUBMIT_BUFFER_SIZE_V3 = 238;
        /**
         * 回傳封包size
         */
        public static  int RECV_BUFFER_SIZE_V1 = 189;
        /**
         * 當Login時，將type設成SERV_LOGIN
         */
        public static   byte SERV_LOGIN = 0;
        /**
         * 當要change password時，將type設為SERV_CHANGE_PASSWORD
         */
        public static   byte SERV_CHANGE_PASS = 1;
        /**
         * 當要傳送訊息時，將type設為SERV_SUBMIT_MSG
         */
        public static   byte SERV_SUBMIT_MSG = 2;
        /**
         * 當要查詢簡訊傳送結果時，將type設為SERV_QUERY_STATE
         */
        public static   byte SERV_QUERY_STATE = 3;
        /**
         * 當要接收簡訊時，將type設為SERV_GET_MSG
         */
        public static   byte SERV_GET_MSG = 4;
        /**
         * 一般將簡訊編碼格式設成TEXT_CODING_VALUE
         */
        public static   byte TEXT_CODING_VALUE = 1;
        /**
         * 預設簡訊傳送方式
         */
        public static   byte DEFAULT_TRAN_TYPE = 100;
        /**
         * 每個封包的簡訊內容最大值
         */
        public static   int MAX_MSG_SIZE = 160;
    }
    public class SubmitDataModel
    {
        /**
           * Sns Protocol命令代碼. 0->登入, 2->傳送簡訊, 3->查詢狀態
           */
        protected byte type;
        /**
         * 簡訊編碼方式.一般文字請設為1
         */
        protected byte coding;
        /**
         * 簡訊長度.以byte計算
         */
        protected byte length;
        /**
         * 訊息傳送方式.立即傳送為100, 預約傳送為101
         */
        protected byte tranType;
        /**
         * 登入帳號
         */
        protected String account;
        /**
         * 登入密碼
         */
        protected String typeword;
        /**
         * 接收簡訊的手機號碼, 09xxxxxxxx
         */
        protected String rcvMsisdn;
        /**
         * 訊息查詢代碼,運用在查詢簡訊傳送狀態
         */
        protected String messageID;
        /**
         *  簡訊傳送的預約時間,格式為yymmddhhmm00,如果是立即傳送,此欄位不用填
         */
        protected String dlvTime;
        /**
         * 簡訊內容
         */
        protected String message;
        protected byte[] byMsg;
        private Boolean isByMsg = false;

        private Boolean isSetPchid = false;
        protected byte pclid;
        protected byte[] pchid = new byte[9];

        private String m_strLastMessage = "";

        public SubmitDataModel()
        {
            reset();
        }

        /**
         * 傳回錯誤訊息
         */
        public String getLastMessage()
        {
            return m_strLastMessage;
        }

        /**
         * 重設欄位資料於初始值
         */
        public void reset()
        {
            type = (byte)0;
            coding = (byte)1;
            length = (byte)0;
            tranType = (byte)100;
            account = "";
            typeword = "";
            rcvMsisdn = "";
            messageID = "";
            dlvTime = "";
            message = "";

            isByMsg = false;
            isSetPchid = false;
            for (int i = 0; i < pchid.Length; i++)
            {
                pchid[i] = (byte)0;
            }
        }

        /**
         * 將物件轉成位元串流.如果轉換失敗,將會傳回null,呼叫getLastMessage()取得失敗原因
         * @return byte[] - 物件的位元串流
         */
        public byte[] toByteStream_V1()
        {
            // modified by yengopan, 2014/05/10, extending rcvMsisdn(13->22) and messageID(9->21) for international format
            byte[] stream = new byte[IConstants.SUBMIT_BUFFER_SIZE_V1];
            //sbyte[] stream = new sbyte[IConstants.SUBMIT_BUFFER_SIZE_V3];
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            stream[0] = type;
            stream[1] = coding;
            stream[2] = length;
            stream[3] = tranType;            
            for (int i = 0; i < Encoding.UTF8.GetBytes(account).Length; i++)
            {//account
                stream[i + 4] = Convert.ToByte( Encoding.UTF8.GetBytes(account)[i]);
            }
            for (int i = 0; i < Encoding.UTF8.GetBytes(typeword).Length; i++)
            {//password
                stream[i + 13] = Convert.ToByte(Encoding.UTF8.GetBytes(typeword)[i]);
            }
            for (int i = 0; i < Encoding.UTF8.GetBytes(rcvMsisdn).Length; i++)
            {//要傳送的號碼
                if ((rcvMsisdn.Length > 12) && (rcvMsisdn[0] != '+'))
                    rcvMsisdn = "+" + rcvMsisdn;
                stream[i + 22] = Convert.ToByte(Encoding.UTF8.GetBytes(rcvMsisdn)[i]);
            }

            for (int i = 0; i < Encoding.UTF8.GetBytes(messageID).Length; i++)
            {
                stream[i + 35] = Encoding.UTF8.GetBytes(messageID)[i];                
            }

            if (isByMsg == false)
            {
                for (int i = 0; i < Encoding.GetEncoding("Big5").GetBytes(message).Length; i++)
                {
                    stream[i + 44] = Encoding.GetEncoding("Big5").GetBytes(message)[i];                    
                }
            }
            else
            {
                for (int i = 0; i < byMsg.Length; i++)
                {
                    stream[i + 44] = byMsg[i];                   
                }
            }
            for (int i = 0; i < Encoding.UTF8.GetBytes(dlvTime).Length; i++)
            {
                stream[i + 204] = Encoding.UTF8.GetBytes(dlvTime)[i];               
            }

            if (isSetPchid)
                for (int i = 0; i < pchid.Length; i++)
                {
                    stream[i + 4] = pchid[i];
                }            
            return stream;
        }
        /**
         * 傳回訊息查詢代碼
         */
        public String getMessageID()
        {
            return messageID;
        }

        public byte getCoding()
        {
            return coding;
        }

        public byte getLength()
        {
            return length;
        }

        public String getDlvTime()
        {
            return dlvTime;
        }

        public String getMessage()
        {
            return message;
        }

        public String getAccount()
        {
            return account;
        }

        public byte getType()
        {
            return type;
        }

        public byte getTranType()
        {
            return tranType;
        }

        public String getRcvMsisdn()
        {
            return rcvMsisdn;
        }
        /**
         * 設定登入密碼
         * @param password String
         */
        public void setPassword(String password)
        {
            this.typeword = removeSpecialCharactersPath(password);
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
         * 設定訊息查詢代碼
         * @param messageID String - 訊息查詢代碼,此代碼由SNS回傳給程式
         */
        public void setMessageID(String messageID)
        {
            this.messageID = messageID;
        }
        /**
         * 設定簡訊編碼格式
         * @param coding byte
         */
        public void setCoding(byte coding)
        {
            this.coding = coding;
        }
        /**
         * 設定簡訊內容長度
         * @param length byte
         */
        public void setLength(byte length)
        {
            this.length = length;
        }
        /**
         * 設定預約傳送時間
         * @param dlvTime String
         */
        public void setDlvTime(String dlvTime)
        {
            this.dlvTime = dlvTime;
        }
        /**
         * 設定簡訊內容
         * @param message String
         */
        public void setMessage(String message)
        {
            this.message = message;
        }

        public void setMessage(byte[] bymsg)
        {
            isByMsg = true;
            this.byMsg = bymsg;
        }
        /**
         * 設定登入帳號
         * @param account String
         */
        public void setAccount(String account)
        {
            this.account = account;
        }
        /**
         * 設定服務類型
         * @param type byte
         */
        public void setType(byte type)
        {
            this.type = type;
        }
        /**
         * 設定簡訊傳送方式
         * @param tranType byte
         */
        public void setTranType(byte tranType)
        {
            this.tranType = tranType;
        }
        /**
         * 設定接收訊息之手機號碼
         * @param rcvMsisdn String
         */
        public void setRcvMsisdn(String rcvMsisdn)
        {
            this.rcvMsisdn = rcvMsisdn;
        }

        public String getPassword()
        {
            return typeword;
        }

        public void setExpire(byte hour, byte min)
        {
            if ((hour > 0) || (min > 0))
            {
                isSetPchid = true;
                this.pchid[0] = hour;
                this.pchid[1] = min;
            }
        }

        public void setUdhi(byte udhi)
        {
            if (udhi == 1)
            {
                isSetPchid = true;
                this.pchid[2] |= 0x01;
            }
        }

        public void setPclid(byte pclid)
        {
            if (pclid > 0)
            {
                isSetPchid = true;
                this.pchid[2] |= 0x04;
                this.pchid[4] = pclid;
            }
        }

        //public static void main(String[] args)
        //{
        //    // 測試reset後byte會不會被清為0
        //    SubmitDataModel sdm = new SubmitDataModel();
        //    sdm.setAccount("xxxxx");
        //    sdm.setPassword("yyyyy");
        //    byte[] btmp = sdm.toByteStream_V1();
        //    System.out.println("###########################");
        //    for (int i = 0; i < btmp.length; i++)
        //    {
        //        System.out.println(", " + btmp[i]);
        //    }
        //    sdm.reset();
        //    btmp = sdm.toByteStream_V1();
        //    System.out.println("###########################");
        //    for (int i = 0; i < btmp.length; i++)
        //    {
        //        System.out.println(", " + btmp[i]);
        //    }

        //}
    }
}
