using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;

namespace MvcMusicStore.Controllers
{
    public class ShoppingCartController : Controller
    {

        private MvcMusicStoreEntities context = new MvcMusicStoreEntities();

        //显示购物车列表
        public ActionResult Index()
        {
            //根据请求上下文来获取购物车业务对象
            var cart = ShoppingCart.GetCart(this.HttpContext);
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),//取出该用户的商品列表
                CartTotal = cart.GetTotal()//商品总金额
            };
            return View(viewModel);
        }
        //添加
        public ActionResult Add(int id)
        {
            Album album = context.Albums.Find(id);
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.AddToCart(album);
            return RedirectToAction("Index", "ShoppingCart");
        }
        //根据记录Id删除
        public ActionResult Delete(int id)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.RemoveFromCart(id);
            return RedirectToAction("Index", "ShoppingCart");
        }
        //改变购物车商品数量，ajax调用
        [HttpPost]
        public ActionResult ChangeCount(int recordId, int count)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            int result = cart.ChangeCount(recordId, count);
            if (result > 0)
            {
                return Content(cart.GetTotal().ToString());
            }
            else
            {
                return Content("-1");
            }
        }
    }
}
