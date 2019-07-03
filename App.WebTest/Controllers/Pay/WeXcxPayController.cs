using App.Pay.WePay;
using App.Pay.WePay.XcxPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace App.WebTest.Controllers
{
    /// <summary>
    /// 微信小程序支付
    /// </summary>
    public class WeXcxPayController : BaseController
    {
        /// <summary>
        /// 小程序下单
        /// </summary>
        /// <param name="oIds">订单Id</param>
        /// <param name="code">临时登录凭证</param>
        /// <returns></returns>
        public ActionResult WeXcxPay(int[] oIds, string code)
        {
            #region 验证订单是否有效,并合计价格

            //订单价格
            decimal payPrice = 0;

            //订单描述
            string detail = "";

            //验证订单.....


            #endregion

            #region 统一下单

            try
            {
                //支付回调通知地址
                var address = WebConfigurationManager.AppSettings["WxXcxNotifyUrl"].ToString();
                XcxPayData data = new XcxPayData();
                data.SetValue("body", "商品购买");

                //可以将用户Id和订单Id同时封装在attach中
                data.SetValue("attach", String.Join(",", oIds).ToString());
                Random rd = new Random();

                //外部商户订单号
                var payNum = DateTime.Now.ToString("yyyyMMddHHmmss") + rd.Next(0, 1000).ToString().PadLeft(3, '0');
                data.SetValue("out_trade_no", payNum);
                data.SetValue("detail", detail.Substring(0, detail.Length - 1));
                data.SetValue("total_fee", Convert.ToInt32(payPrice * 100));
                data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
                data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
                data.SetValue("notify_url", address);
                //data.SetValue("goods_tag", "test");
                data.SetValue("trade_type", "JSAPI");
                data.SetValue("openid", WeHelper.Code2Session(code).openid);

                XcxPayData result = XcxPayApi.UnifiedOrder(data);
                var flag = true;
                var msg = "";
                var nonceStr = "";
                var appId = "";
                var package = "";
                var mch_id = "";
                if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
                {
                    flag = false;
                    msg = "下单失败";
                    return Json(new { Result = false, Msg = "下单失败！" });
                }
                else
                {
                    //统一下单

                    ///TO Do......
                    /// 修改订单状态

                    nonceStr = result.GetValue("nonce_str").ToString();
                    appId = result.GetValue("appid").ToString();
                    mch_id = result.GetValue("mch_id").ToString();
                    package = "prepay_id=" + result.GetValue("prepay_id").ToString();
                }
                var signType = "MD5";
                var timeStamp = ((DateTime.Now.Ticks - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).Ticks) / 10000).ToString();
                XcxPayData applet = new XcxPayData();
                applet.SetValue("appId", appId);
                applet.SetValue("nonceStr", nonceStr);
                applet.SetValue("package", package);
                applet.SetValue("signType", signType);
                applet.SetValue("timeStamp", timeStamp);
                var appletSign = applet.MakeSign();
                return Json(new { timeStamp, nonceStr, package, signType, paySign = appletSign, Result = flag, msg });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, msg = "缺少参数" });
            }
            #endregion
        }

        /// <summary>
        /// 微信小程序支付回调通知
        /// </summary>
        /// <returns></returns>
        public void WeXcxNotifyUrl()
        {
            Pay.Log Log = new Pay.Log(XcxPayConfig.LogPath);
            Log.Info("WxXcxNotifyUrl", "支付回调");
            XcxPayNotify notify = new XcxPayNotify(System.Web.HttpContext.Current);
            XcxPayData notifyData = notify.GetNotifyData();

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                XcxPayData res = new XcxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                Response.Write(res.ToXml());
                Response.End();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!XcxQueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                XcxPayData res = new XcxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Log.Error(this.GetType().ToString(), "Order query failure : " + res.ToXml());

                Response.Write(res.ToXml());
                Response.End();
            }
            //查询订单成功
            else
            {
                XcxPayData res = new XcxPayData();
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                Log.Info(this.GetType().ToString(), "Order query success : " + res.ToXml());
                Log.Info(this.GetType().ToString(), "Order query success,notifyData : " + notifyData.ToXml());
                var returnCode = notifyData.GetValue("return_code").ToString();
                var transactionNo = transaction_id;//微信订单号
                var outTradeNo = notifyData.GetValue("out_trade_no").ToString();//自定义订单号
                var attach = notifyData.GetValue("attach").ToString();//身份证
                var endTime = notifyData.GetValue("time_end").ToString();//交易结束时间
                //var body = notifyData.GetValue("body").ToString();//projectIdlist
                var totalFee = notifyData.GetValue("total_fee").ToString(); ;//支付金额

                int userId = Convert.ToInt32(attach.Split('|')[0]);
                string msg;
                try
                {
                    //var result = OrderBll.Value.CompleteWePay(userId, totalFee, transactionNo, returnCode, outTradeNo, attach, endTime, out msg);

                    var result = true;

                    Log.Info(this.GetType().ToString(), "CompleteWePay:" + result);
                }
                catch (Exception e)
                {
                    Log.Error(this.GetType().ToString(), "CompleteWePay:" + e.ToString());
                }

                Response.Write(res.ToXml());
                Response.End();
            }
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="transaction_id">微信交易订单号</param>
        /// <returns></returns>
        private bool XcxQueryOrder(string transaction_id)
        {
            XcxPayData req = new XcxPayData();
            req.SetValue("transaction_id", transaction_id);
            XcxPayData res = XcxPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" && res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}