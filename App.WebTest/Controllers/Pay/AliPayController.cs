using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using App.Common.Extension;
using App.Pay.AliPay;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App.WebTest.Controllers
{
    public class AliPayController : BaseController
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        /// <param name="oidStr"></param>
        /// <returns></returns>
        public ActionResult AliPay(string oidStr)
        {
            #region 验证订单有效

            if (string.IsNullOrEmpty(oidStr))
            {
                return Json(false, "OrderError");
            }

            int[] oIds = Serialize.JsonTo<int[]>(oidStr);

            decimal payPrice = 0;

            ///订单验证，统计订单总金额

            #endregion

            #region 统一下单
            try
            {
                var notify_url = AliPayConfig.notify_url;
                var return_url = AliPayConfig.return_url;
                IAopClient client = Pay.AliPay.AliPay.GetAlipayClient();
                AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();
                //SDK已经封装掉了公共参数，这里只需要传入业务参数。以下方法为sdk的model入参方式(model和biz_content同时存在的情况下取biz_content)。
                AlipayTradeAppPayModel model = new AlipayTradeAppPayModel();
                model.Subject = "商品购买";
                model.TotalAmount = payPrice.ToString("F2");
                model.ProductCode = "QUICK_MSECURITY_PAY";
                Random rd = new Random();
                var payNum = DateTime.Now.ToString("yyyyMMddHHmmss") + rd.Next(0, 1000).ToString().PadLeft(3, '0');
                model.OutTradeNo = payNum;
                model.TimeoutExpress = "30m";
                request.SetBizModel(model);
                request.SetNotifyUrl(notify_url);
                //request.SetReturnUrl(return_url);
                //这里和普通的接口调用不同，使用的是sdkExecute
                AlipayTradeAppPayResponse response = client.SdkExecute(request);

                //统一下单
                //OrderBll.Value.UpdateOrderApp(oIds, payNum);

                return Json(true, new { response.Body }, "OK");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, msg = "缺少参数" });
            }
            #endregion
        }

        /// <summary>
        /// 页面跳转同步通知页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ReturnUrl()
        {
            Pay.Log Log = new Pay.Log(Pay.AliPay.AliPayConfig.LogPath);
            Log.Info("ReturnUrl", "支付页面同步回调");
            //将同步通知中收到的所有参数都存放到map中
            IDictionary<string, string> map = GetRequestGet();
            if (map.Count > 0) //判断是否有带返回参数
            {
                try
                {
                    //支付宝的公钥
                    string alipayPublicKey = AliPayConfig.payKey;
                    string signType = AliPayConfig.signType;
                    string charset = AliPayConfig.charset;
                    bool keyFromFile = false;
                    // 获取支付宝GET过来反馈信息  
                    bool verify_result = AlipaySignature.RSACheckV1(map, alipayPublicKey, charset, signType, keyFromFile);
                    if (verify_result)
                    {
                        // 验证成功                        
                        return Json(new { Result = true, msg = "验证成功" });
                    }
                    else
                    {
                        Log.Error("AliPayNotifyUrl", "支付验证失败");
                        return Json(new { Result = false, msg = "验证失败" });
                    }
                }
                catch (Exception e)
                {
                    //throw new Exception(e.Message);
                    return Json(new { Result = false, msg = "验证失败" });
                    Log.Error("AliPayNotifyUrl", "支付验证失败");
                }
            }
            else
            {
                return Json(new { Result = false, msg = "无返回参数" });
            }
        }

        /// <summary>
        /// 服务器异步通知页面
        /// </summary>
        public void AliPayNotifyUrl()
        {
            Pay.Log Log = new Pay.Log(AliPayConfig.LogPath);
            Log.Info("AliPayNotifyUrl", "支付页面异步回调");
            IDictionary<string, string> map = GetRequestPost();

            if (map.Count > 0)
            {
                try
                {
                    string alipayPublicKey = AliPayConfig.payKey;
                    string signType = AliPayConfig.signType;
                    string charset = AliPayConfig.charset;
                    bool keyFromFile = false;

                    bool verify_result = AlipaySignature.RSACheckV1(map, alipayPublicKey, charset, signType, keyFromFile);
                    Log.Info("AliPayNotifyUrl验签", verify_result + "");

                    //验签成功后，按照支付结果异步通知中的描述，对支付结果中的业务内容进行二次校验，校验成功后再response中返回success并继续商户自身业务处理，校验失败返回false
                    if (verify_result)
                    {
                        //商户订单号
                        string out_trade_no = map["out_trade_no"];
                        //支付宝交易号
                        string trade_no = map["trade_no"];
                        //交易创建时间
                        string gmt_create = map["gmt_create"];
                        //交易付款时间
                        string gmt_payment = map["gmt_payment"];
                        //通知时间
                        string notify_time = map["notify_time"];
                        //通知类型  trade_status_sync
                        string notify_type = map["notify_type"];
                        //通知校验ID
                        string notify_id = map["notify_id"];
                        //开发者的app_id
                        string app_id = map["app_id"];
                        //卖家支付宝用户号
                        string seller_id = map["seller_id"];
                        //买家支付宝用户号
                        string buyer_id = map["buyer_id"];
                        //实收金额
                        string receipt_amount = map["receipt_amount"];
                        //交易状态
                        string return_code = map["trade_status"];

                        //交易状态TRADE_FINISHED的通知触发条件是商户签约的产品不支持退款功能的前提下，买家付款成功；
                        //或者，商户签约的产品支持退款功能的前提下，交易已经成功并且已经超过可退款期限
                        //状态TRADE_SUCCESS的通知触发条件是商户签约的产品支持退款功能的前提下，买家付款成功
                        if (return_code == "TRADE_FINISHED" || return_code == "TRADE_SUCCESS")
                        {
                            string msg;

                            Log.Error("AliPayNotifyUrl", receipt_amount + "==" + trade_no + "==" + return_code + "==" + out_trade_no + "==" + gmt_payment);

                            //判断该笔订单是否在商户网站中已经做过处理
                            ///支付回调的业务处理
                            //bool res = OrderBll.Value.CompleteAliPay(receipt_amount, trade_no, return_code, out_trade_no, gmt_payment, out msg);
                            bool res = true;

                            if (res == false)
                            {
                                Response.Write("添加支付信息失败!");
                            }
                            Log.Error("AliPayNotifyUrl", "支付成功");
                            Response.Write("success");  //请不要修改或删除
                        }
                    }
                    else
                    {
                        //验证失败
                        Log.Error("AliPayNotifyUrl", "支付验证失败");
                        Response.Write("验证失败!");
                    }
                }
                catch (Exception e)
                {
                    Response.Write("添加支付信息失败!");
                    Log.Error("AliPayNotifyUrl", "添加支付信息失败");
                }
            }
            else
            {
                //无返回参数
                Response.Write("无返回参数!");
                Log.Error("AliPayNotifyUrl", "无返回参数");
            }
        }
        //[AllowUser]
        //public ActionResult TestAliPay()
        //{

        //    var receipt_amount = "0.01";
        //    var trade_no = "20181226220013.......";
        //    var return_code = "TRADE_SUCCESS";
        //    var out_trade_no = "20181226103124129";
        //    var gmt_payment = "2018-12-26 10:31:29";

        //    string msg = "";
        //    bool res = OrderBll.Value.CompleteAliPay(receipt_amount, trade_no, return_code, out_trade_no, gmt_payment, out msg);

        //    return Json(res);
        //}

        /// <summary>
        /// 获取支付宝Get过来的通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetRequestGet()
        {
            Pay.Log Log = new Pay.Log(Pay.AliPay.AliPayConfig.LogPath);
            int i = 0;
            IDictionary<string, string> sArry = new Dictionary<string, string>();
            NameValueCollection coll;
            coll = Request.QueryString;

            String[] requstItem = coll.AllKeys;

            for (i = 0; i < requstItem.Length; i++)
            {
                Log.Info("GetRequestGet", requstItem[i] + ":" + Request.QueryString[requstItem[i]]);
                sArry.Add(requstItem[i], Request.QueryString[requstItem[i]]);
            }

            return sArry;
        }

        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public IDictionary<string, string> GetRequestPost()
        {
            Pay.Log Log = new Pay.Log(Pay.AliPay.AliPayConfig.LogPath);
            int i = 0;
            IDictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;

            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;
            for (i = 0; i < requestItem.Length; i++)
            {
                Log.Info("GetRequestPost", requestItem[i] + ":" + Request.Form[requestItem[i]]);
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }
    }
}