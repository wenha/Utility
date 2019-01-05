using App.Common.Extension;
using App.Pay.WePay.AppPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace App.WebTest.Controllers
{
    public class WeAppPayController : Controller
    {
        /// <summary>
        /// 微信app支付
        /// </summary>
        /// <param name="oidStr">订单编号</param>
        /// <returns></returns>
        public ActionResult WxPayApp(string oidStr)
        {
            // App调用只能传参
            int[] oIds = Serialize.JsonTo<List<int>>(oidStr).ToArray();

            #region 验证订单是否有效

            decimal payPrice = 0;
            string detail = "";

            /// 验证订单是否有效，并统计订单总金额
            /// ...

            #endregion

            #region 统一下单
            try
            {
                //string userId = LoginUser.Id.ToString();
                var address = WebConfigurationManager.AppSettings["WxAppNotifyUrl"].ToString();
                AppPayData data = new AppPayData();
                data.SetValue("body", "民政社工培训-课程购买");
                //data.SetValue("attach", userId + "|" + String.Join(",", oIds).ToString());
                data.SetValue("attach", String.Join(",", oIds).ToString());
                Random rd = new Random();
                var payNum = DateTime.Now.ToString("yyyyMMddHHmmss") + rd.Next(0, 1000).ToString().PadLeft(3, '0');
                data.SetValue("out_trade_no", payNum);
                data.SetValue("detail", detail.Substring(0, detail.Length - 1));
                data.SetValue("total_fee", Convert.ToInt32(payPrice * 100));
                data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
                data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
                data.SetValue("notify_url", address);

                //注意，这里交易方式是APP
                data.SetValue("trade_type", "APP");

                AppPayData result = AppPayApi.UnifiedOrder(data);
                var appid = "";
                var partnerid = "";
                var prepayid = "";
                var package = "";
                var nonceStr = "";
                var timestamp = "";
                var sign = "";
                if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
                {
                    return Json(false, "下单失败！");
                }
                else
                {
                    //统一下单
                    /// 修改订单状态
                    //OrderBll.Value.UpdateOrderApp(oIds, payNum);

                    appid = result.GetValue("appid").ToString();
                    nonceStr = result.GetValue("nonce_str").ToString();
                    partnerid = result.GetValue("mch_id").ToString();
                    prepayid = result.GetValue("prepay_id").ToString();
                    package = "Sign=WXPay";// "prepay_id=" + result.GetValue("prepay_id").ToString();
                }
                var signType = "MD5";
                timestamp = ((DateTime.Now.Ticks - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).Ticks) / 10000).ToString();
                AppPayData applet = new AppPayData();
                applet.SetValue("appid", appid);
                applet.SetValue("noncestr", nonceStr);
                applet.SetValue("package", package);
                applet.SetValue("partnerid", partnerid);
                applet.SetValue("prepayid", prepayid);
                //applet.SetValue("signType", signType);
                applet.SetValue("timestamp", timestamp);
                sign = applet.MakeSign();
                return Json(new { appid, partnerid, prepayid, package, nonceStr, timestamp, sign });
            }
            catch (Exception ex)
            {
                return Json(false, "缺少参数");
            }

            #endregion

        }

        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <returns></returns>
        public string WxAppNotifyUrl()
        {
            Pay.Log Log = new Pay.Log(AppPayConfig.LogPath);
            Log.Info("WxAppNotifyUrl", "支付回调");
            AppPayNotify notify = new AppPayNotify(System.Web.HttpContext.Current);
            AppPayData notifyData = notify.GetNotifyData();

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                AppPayData res = new AppPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                Response.Write(res.ToXml());
                Response.End();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!AppQueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                AppPayData res = new AppPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Log.Error(this.GetType().ToString(), "Order query failure : " + res.ToXml());

                Response.Write(res.ToXml());
                Response.End();
            }
            //查询订单成功
            else
            {
                AppPayData res = new AppPayData();
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
                    ///修改数据库订单状态
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

            return "";
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="transaction_id"></param>
        /// <returns></returns>
        private bool AppQueryOrder(string transaction_id)
        {
            AppPayData req = new AppPayData();
            req.SetValue("transaction_id", transaction_id);
            AppPayData res = AppPayApi.OrderQuery(req);
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