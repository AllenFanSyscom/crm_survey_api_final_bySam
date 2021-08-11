using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.cht.messaging.sns
{
    public class RecvDataModel
    {
        /**
 * SNS執行的結果代碼
 */
        protected byte code;
        /**
         * 手機上傳簡訊的編碼,利用此欄位輔助來呈現上傳簡訊內容
         */
        protected byte coding;
        /**
         * 手機上傳簡訊的長度
         */
        protected byte length;
        /**
         * 手機上傳簡訊的發送號碼
         */
        protected String sendMsisdn;
        protected String recvMsisdn;
        /**
         * SNS執行結果的說明,如果傳送簡訊執行成功,此欄位為簡訊查詢代碼
         */

        protected byte[] desc = null;

        public RecvDataModel()
        {
        }

        /**
         * 將SNS Server傳回的位元串流解析成SNS Protocol對映的欄位資料
         * @param data byte[] - SNS Server傳回的位元串流
         * @return RecvDataModel
         */
        public static RecvDataModel parse_v1(byte[] data)
        {
            RecvDataModel dm = new RecvDataModel();
            dm.setCode(data[0]);
            dm.setCoding(data[1]);
            dm.setLength(data[2]);
            dm.setSendMsisdn(System.Text.Encoding.UTF8.GetString(data).Substring(3, 13).Trim());
            dm.setRecvMsisdn(System.Text.Encoding.UTF8.GetString(data).Substring(16, 13).Trim());
            //dm.setSendMsisdn(new String(data, 3, 13).trim());
            //dm.setRecvMsisdn(new string(data, 16, 13).trim());

            byte[] buf = new byte[160];
            Array.Copy(data, 29, buf, 0, buf.Length);
            //System.arraycopy(data, 29, buf, 0, buf.length);
            dm.setDesc(buf);

            return dm;
        }
        /**
         * 將系統回傳碼放對映至code變數
         * @param code byte
         */
        public void setCode(byte code)
        {
            this.code = code;
        }
        /**
         * 將簡訊編碼格式對映至coding變數
         * @param coding byte
         */
        public void setCoding(byte coding)
        {
            this.coding = coding;
        }
        /**
         * 將發送簡訊的手機號碼對映至sendMsisdn變數
         * @param sendMsisdn String
         */
        public void setSendMsisdn(String sendMsisdn)
        {
            this.sendMsisdn = sendMsisdn;
        }
        /**
         * 將特碼對映至recvMsisdn變數
         * @param recvMsisdn String
         */
        public void setRecvMsisdn(String recvMsisdn)
        {
            this.recvMsisdn = recvMsisdn;
        }
        /**
         * 將簡訊長度對映至length變數
         * @param length byte
         */
        public void setLength(byte length)
        {
            this.length = length;
        }

        public void setDesc(byte[] desc)
        {
            this.desc = desc;
        }

        /**
         * 將系統回傳敘述對映至desc變數
         * @param desc String
         */

        /**
         * 傳回訊息結果代碼
         */
        public byte getCode()
        {
            return code;
        }

        public byte getCoding()
        {
            return coding;
        }

        public String getSendMsisdn()
        {
            return sendMsisdn;
        }

        public String getRecvMsisdn()
        {
            return recvMsisdn;
        }

        public byte getLength()
        {
            return length;
        }

        public byte[] getDesc()
        {
            return desc;
        }
    }
}
