using WatchStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace WatchStore.Areas.Admin.Controllers
{
    public class DashboardController : BaseController
    {
        private WatchStoreDbContext db = new WatchStoreDbContext();
        // GET: Admin/Dashboard
      
        public ActionResult Index()
        {
            ViewBag.CountOrderSuccess = db.Orders.Where(m => m.Status == 3).Count();
            ViewBag.CountOrderCancel = db.Orders.Where(m => m.Status == 1 && m.Trash !=1).Count();
            ViewBag.CountContactDoneReply = db.Contacts.Where(m => m.Flag == 0).Count();
            ViewBag.CountUser = db.Users.Where(m => m.Status != 0 && m.Access==0).Count();

            var thisMonth = DateTime.Now.Month;
            var thisYear = DateTime.Now.Year;
            ViewBag.ThisMonth = thisMonth;
            ViewBag.ThisYear = thisYear;

            ViewBag.countOrderSuccessThisMouth = 
                db.Orders.Where(m => m.Status == 3 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();
            ViewBag.countOrderCancelThisMouth =
                db.Orders.Where(m => m.Trash == 1 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();
            ViewBag.countOrderWaitingThisMouth =
                db.Orders.Where(m => m.Status == 1 && m.Trash != 1 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();
            ViewBag.countOrderSendingThisMouth =
                db.Orders.Where(m => m.Status == 2 && m.Trash != 1 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();

            

            int days = DateTime.DaysInMonth(thisYear,thisMonth);
            List<Double> list = new List<Double>();
            for(int i = 1; i <= days; i++)
            {
                var odersInDay =db.Orders.Where(m => m.Status == 3 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear && m.CreateDate.Day == i).ToList();
                Double sum = 0;
                foreach(var order in odersInDay)
                {
                    sum += db.Orderdetails.Where(m => m.OrderId == order.Id).Sum(m => m.Price * m.Quantity);
                }
                list.Add(sum);
            }
            ViewBag.dataBarChar = list;

            return View();
        }

        [HttpGet]
        public JsonResult GetDashboardData(int month, int year)
        {
            var countSuccess = db.Orders.Where(m => m.Status == 3 && m.CreateDate.Month == month && m.CreateDate.Year == year).Count();
            var countCancel = db.Orders.Where(m => m.Trash == 1 && m.CreateDate.Month == month && m.CreateDate.Year == year).Count();
            var countWaiting = db.Orders.Where(m => m.Status == 1 && m.Trash != 1 && m.CreateDate.Month == month && m.CreateDate.Year == year).Count();
            var countSending = db.Orders.Where(m => m.Status == 2 && m.Trash != 1 && m.CreateDate.Month == month && m.CreateDate.Year == year).Count();

            int days = DateTime.DaysInMonth(year, month);
            List<double> list = new List<double>();

            for (int i = 1; i <= days; i++)
            {
                var ordersInDay = db.Orders.Where(m => m.Status == 3 &&
                                                       m.CreateDate.Month == month &&
                                                       m.CreateDate.Year == year &&
                                                       m.CreateDate.Day == i).ToList();

                double sum = 0;
                foreach (var order in ordersInDay)
                {
                    sum += db.Orderdetails
                             .Where(m => m.OrderId == order.Id)
                             .Sum(m => m.Price * m.Quantity);
                }
                list.Add(sum);
            }

            return Json(new
            {
                success = true,
                month,
                year,
                countSuccess,
                countCancel,
                countWaiting,
                countSending,
                dailyRevenue = list
            }, JsonRequestBehavior.AllowGet);
        }

    }
}