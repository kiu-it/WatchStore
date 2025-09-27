using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WatchStore.Models;

namespace WatchStore.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        // GET: Admin/Base
        public BaseController()
        {
            if (System.Web.HttpContext.Current.Session["Admin_Name"] == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("~/Admin/Login");
            }
            else
            {
                // Kiểm tra xem người dùng có quyền admin không
                WatchStoreDbContext db = new WatchStoreDbContext();
                int userId = Convert.ToInt32(System.Web.HttpContext.Current.Session["Admin_ID"]);
                var user = db.Users.Find(userId);
                if (user == null || user.Access != 0) // Chỉ cho phép Access = 0 (admin)
                {
                    System.Web.HttpContext.Current.Session["Admin_Name"] = null;
                    System.Web.HttpContext.Current.Session["Admin_ID"] = null;
                    System.Web.HttpContext.Current.Session["Admin_Images"] = null;
                    System.Web.HttpContext.Current.Session["Admin_Email"] = null;
                    System.Web.HttpContext.Current.Session["Admin_Created_at"] = null;
                    System.Web.HttpContext.Current.Response.Redirect("~/Admin/Login");
                }
            }
        }
    }
}