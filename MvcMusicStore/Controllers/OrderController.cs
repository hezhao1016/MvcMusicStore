using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using MvcMusicStore.Helpers;

namespace MvcMusicStore.Controllers
{
    public class OrderController : Controller
    {

        private MvcMusicStoreEntities context = new MvcMusicStoreEntities();

        //显示用户订单首页
        public ActionResult Index(int pageIndex = 1)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int uid = (Session["User"] as UserInfo).Id;
            int pageSize = 5;
            var orders = context.Orders.Where(o => o.UserId == uid).OrderByDescending(o=>o.OrderDate).Select(o => o).ToList();
            decimal totalMoneys = 0;//所有订单总额
            foreach (var o in orders)
            {
                totalMoneys += o.Amount;
            }
            ViewBag.TotalMoneys = totalMoneys;
            var pagedOrders = new PagedList<Order>(orders, pageSize, pageIndex);
            return View(pagedOrders);
        }

        //显示订单详情
        public ActionResult Detail(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index","Home");
            }
            var order = context.Orders.Find(id);
            return View(order);
        }

        //删除一个详情订单
        public ActionResult DeleteOrderDetail(int id)
        {
            try
            {
                OrderDetail od = new OrderDetail { OrderDetailId = id};
                context.OrderDetails.Attach(od);
                context.OrderDetails.Remove(od);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                string message = "数据库在执行删除详细订单操作时，出现异常。";
                Exception ex = new Exception(message);
                return View("Error", new HandleErrorInfo(ex, "Order", "DeleteOrderDetail"));
            }
        }

        //删除一个订单
        public ActionResult DeleteOrder(int id)
        {
            try
            {
                //先删除此订单下的详情订单
                var orderDetails = context.OrderDetails.Where(od => od.OrderId == id).Select(od => od).ToList();
                foreach (var od in orderDetails)
                {
                    context.OrderDetails.Remove(od);
                }
                //再删除订单
                var order = new Order { OrderId = id};
                context.Orders.Attach(order);
                context.Orders.Remove(order);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                string message = "数据库在执行删除订单操作时，出现异常。";
                Exception ex = new Exception(message);
                return View("Error", new HandleErrorInfo(ex, "Order", "DeleteOrder"));
            }
        }

        //清空该用户所有的订单
        public ActionResult EmptyUsersOrder()
        {
            try
            {
                int uid = (Session["User"] as UserInfo).Id;
                //获取订单集合
                var orders = context.Orders.Where(o => o.UserId == uid).Select(o => o).ToList();
                foreach (var o in orders)
			    {
			        //先删除该用户所有的详情订单
                    var ods = context.OrderDetails.Where(od => od.OrderId == o.OrderId).Select(od => od).ToList();
                    foreach (var od in ods)
                    {
                        context.OrderDetails.Remove(od);
                    }
                    //再删除该用户所有的订单
                    context.Orders.Remove(o);
			    }
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                string message = "数据库在执行清空用户所有的订单操作时，出现异常。";
                Exception ex = new Exception(message);
                return View("Error", new HandleErrorInfo(ex, "Order", "EmptyUsersOrder"));
            }
        }
    }
}
