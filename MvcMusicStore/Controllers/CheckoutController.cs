using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;

namespace MvcMusicStore.Controllers
{
    public class CheckoutController : Controller
    {

        private MvcMusicStoreEntities context = new MvcMusicStoreEntities();

        //显示添加订单信息页面
        public ActionResult Pay()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            if (cart.GetCount() == 0)
            {
                return Content("<script>alert('您的购物车为空，快去挑选您喜欢的专辑吧！');location.href='" + Url.Action("Index", "Home") + "';</script>");
            }
            else if (Session["User"] == null)
            {
                return Content("<script>alert('请您先登录！');location.href='" + Url.Action("Login", "Account", new { returnUrl = Url.Action("Index", "ShoppingCart") }) + "';</script>");
            }
            return View();
        }

        //提交订单
        [HttpPost]
        public ActionResult Pay(Order order)
        {
            if (ModelState.IsValid)
            {
                var cart = ShoppingCart.GetCart(this.HttpContext);
                order.OrderDate = DateTime.Now;
                order.UserId = (Session["User"] as UserInfo).Id;
                order.Amount = cart.GetTotal();
                //添加订单
                order = context.Orders.Add(order);
                context.SaveChanges();
                //添加详细订单，删除购物车...
                cart.CreateOrder(order);
                Session.Remove(ShoppingCart.CartSessionKey);
                return RedirectToAction("Complete", new { id = order.OrderId});     
            }
            else
            {
                return View(order);
            }
        }

        //显示结算成功页面
        public ActionResult Complete(int id)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.OrderId = id;
            return View();
        }
    }
}
