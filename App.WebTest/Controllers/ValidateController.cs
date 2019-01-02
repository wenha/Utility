using App.Common.ValidateCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App.WebTest.Controllers
{
    public class ValidateController : Controller
    {
        /// <summary>
        /// 获取图片验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateIndex()
        {
            var validateCode = new GraphicValidateCode();
            var code = validateCode.GenerateCode(4);

            var image = validateCode.GenerateValidateGraphic(code);
            var base64 = "data:image/jpeg;base64," + Convert.ToBase64String(image);

            ViewBag.ValidateImg = base64;
            return View();
        }
    }
}