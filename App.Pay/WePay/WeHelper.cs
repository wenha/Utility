using App.Common.Extension;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace App.Pay.WePay
{
    public class WeHelper
    {
        // 小程序
        private static string _appid = ConfigurationManager.AppSettings["wxAPPID"];
        // 小程序
        private static string _appSecret = ConfigurationManager.AppSettings["wxAppSecret"];

        public static WxSession Code2Session(string code)
        {
            var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={_appid}&secret={_appSecret}&js_code={code}&grant_type=authorization_code";
            try
            {
                var request = WebRequest.Create(url);
                using (var response = request.GetResponse())
                {
                    using (var rs = response.GetResponseStream())
                    {
                        using (var s = new System.IO.StreamReader(rs))
                        {
                            return s.ReadToEnd().JsonTo<WxSession>();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class WxSession
    {
        public string openid { get; set; }
        public string session_key { get; set; }
        public string errcode { get; set; }
        public string errMsg { get; set; }
        public string unionid { get; set; }
    }

}
