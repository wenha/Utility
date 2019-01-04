using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Web;
using System.Web.Mvc;

namespace App.WebTest.Controllers
{
    public abstract class BaseController : Controller
    {
        protected BaseController()
        {
        }

        #region 提示信息

        private static ResourceManager _resourceManager;

        private ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = new ResourceManager("ETL.CivilAffairsEdu.Web.App_LocalResources.Message", Assembly.GetExecutingAssembly());
                }
                return _resourceManager;
            }
        }

        protected string GetMessage(string key)
        {
            return ResourceManager.GetString(key);
        }

        #endregion

        protected JsonResult Json(bool success, string status)
        {
            return Json(success, null, status, JsonRequestBehavior.DenyGet);
        }
        protected JsonResult Json(bool success, string status, JsonRequestBehavior behavior)
        {
            return Json(success, null, status, behavior);
        }

        protected JsonResult Json(bool success, object data, string status)
        {
            return Json(success, data, status, JsonRequestBehavior.AllowGet);
        }

        protected virtual JsonResult Json(bool success, object data, string status, JsonRequestBehavior behavior)
        {
            if (success && string.IsNullOrWhiteSpace(status))
            {
                status = "OK";
            }
            return Json(new { success, data, status, msg = GetMessage(status) }, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult JsonMsg(bool success, string status, string msg)
        {
            if (success && string.IsNullOrWhiteSpace(status))
            {
                status = "OK";
            }
            return Json(new { success, status, msg }, JsonRequestBehavior.AllowGet);
        }

    }
}