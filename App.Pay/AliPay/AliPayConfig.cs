using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace App.Pay.AliPay
{
    public class AliPayConfig
    {
        //支付宝网关地址
        public static string serviceUrl = WebConfigurationManager.AppSettings["aliServiceUrl"].ToString();

        //应用ID
        public static string appId = WebConfigurationManager.AppSettings["aliAppId"].ToString();

        //开发者私钥，由开发者自己生成
        public static string privateKey = WebConfigurationManager.AppSettings["aliPrivateKey"].ToString();

        //支付宝的应用公钥
        public static string publicKey = WebConfigurationManager.AppSettings["aliPublicKey"].ToString();

        //支付宝的支付公钥
        public static string payKey = WebConfigurationManager.AppSettings["aliPayKey"].ToString();

        //服务器异步通知页面路径
        public static string notify_url = WebConfigurationManager.AppSettings["aliNotifyUrl"].ToString();

        //页面跳转同步通知页面路径
        public static string return_url = WebConfigurationManager.AppSettings["aliReturnUrl"].ToString();

        //参数返回格式，只支持json
        public static string format = WebConfigurationManager.AppSettings["aliFormat"].ToString();

        // 调用的接口版本，固定为：1.0
        public static string version = WebConfigurationManager.AppSettings["aliVersion"].ToString();

        // 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        public static string signType = WebConfigurationManager.AppSettings["aliSignType"].ToString();

        // 字符编码格式 目前支持utf-8
        public static string charset = WebConfigurationManager.AppSettings["aliCharset"].ToString();

        // false 表示不从文件加载密钥
        public static bool keyFromFile = false;

        // 日志记录
        public static string LogPath = WebConfigurationManager.AppSettings["AliLog"].ToString();
    }
}
