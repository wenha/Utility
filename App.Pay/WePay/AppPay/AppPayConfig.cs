using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace App.Pay.WePay.AppPay
{
    public class AppPayConfig : WePayConfig
    {
        //=======【基本信息设置】=====================================
        /* 微信公众号信息配置
        * APPID：绑定支付的APPID（必须配置）
        * MCHID：商户号（必须配置）
        * KEY：商户支付密钥，参考开户邮件设置（必须配置）
        * APPSECRET：公众帐号secert（仅JSAPI支付的时候需要配置）
        */
        public static string APPID = WebConfigurationManager.AppSettings["appWxAPPID"].ToString();
        public static string MCHID = WebConfigurationManager.AppSettings["appWxMCHID"].ToString();
        public static string KEY = WebConfigurationManager.AppSettings["appWxKEY"].ToString();
        public static string APPSECRET = WebConfigurationManager.AppSettings["appWxAppSecret"].ToString();

        //=======【证书路径设置】===================================== 
        /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
        */
        public const string SSLCERT_PATH = "cert/apiclient_cert.p12";
        public const string SSLCERT_PASSWORD = "1233410002";



        //=======【支付结果通知url】===================================== 
        /* 支付结果通知回调url，用于商户接收支付结果
        */
        public static string NOTIFY_URL = WebConfigurationManager.AppSettings["WxAppNotifyUrl"].ToString();//ConfigurationManager.AppSettings["WxNotifyUrl"];

        // log记录
        public static string LogPath = WebConfigurationManager.AppSettings["WeAppLog"].ToString();
    }
}
