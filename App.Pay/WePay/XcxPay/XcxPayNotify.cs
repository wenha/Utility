using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace App.Pay.WePay.XcxPay
{
    /// <summary>
    /// 回调处理基类
    /// 主要负责接收微信支付后台发送过来的数据，对数据进行签名验证
    /// 子类在此类基础上进行派生并重写自己的回调处理过程
    /// </summary>
    public class XcxPayNotify
    {
        public HttpContext context { get; set; }

        public Log Log = new Log(XcxPayConfig.LogPath);

        public XcxPayNotify(HttpContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 接收从微信支付后台发送过来的数据并验证签名
        /// </summary>
        /// <returns>微信支付后台返回的数据</returns>
        public XcxPayData GetNotifyData()
        {
            //接收从微信后台POST过来的数据
            System.IO.Stream s = context.Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            //转换数据格式并验证签名
            XcxPayData data = new XcxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (WePayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                XcxPayData res = new XcxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                context.Response.Write(res.ToXml());
                context.Response.End();
            }

            Log.Info(this.GetType().ToString(), "Check sign success");
            return data;
        }

        //派生类需要重写这个方法，进行不同的回调处理
        public virtual void ProcessNotify()
        {

        }
    }
}
