using Aop.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Pay.AliPay
{
    public class AliPay
    {
        public static IAopClient GetAlipayClient()
        {
            string serviceUrl = AliPayConfig.serviceUrl;

            string appId = AliPayConfig.appId;

            string privateKey = AliPayConfig.privateKey;

            string publivKey = AliPayConfig.publicKey;

            string format = AliPayConfig.format;

            string version = AliPayConfig.version;

            string signType = AliPayConfig.signType;

            string charset = AliPayConfig.charset;

            bool keyFromFile = AliPayConfig.keyFromFile;


            IAopClient client = new DefaultAopClient(serviceUrl, appId, privateKey, format, version, signType, publivKey, charset, keyFromFile); ;

            return client;
        }
    }
}
