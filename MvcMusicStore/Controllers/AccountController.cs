using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using System.Web.Security;
using MvcMusicStore.Helpers;

namespace MvcMusicStore.Controllers
{
    public class AccountController : Controller
    {

		private MvcMusicStoreEntities context = new MvcMusicStoreEntities ();

        //展示登录页
		public ActionResult Login(string returnUrl)
        {
			if (returnUrl == null)
			{
				returnUrl = Url.Action("Index", "Home");
			}
			ViewBag.ReturnUrl = returnUrl;//登录成功后要跳转的页面
            return View();
        }

		//用户登录
		[HttpPost]
		public ActionResult Login(LoginInfoModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var user = context.UserInfoes.Where(m=>m.LoginId==model.LoginId.Trim().ToLower()).SingleOrDefault();
				if (user == null)
				{
					ModelState.AddModelError("LoginId","用户名不存在");
				}
				else
				{
					if (model.LoginPwd.Trim().ToLower() == user.LoginPwd)
					{
						Session["User"] = user;
						if (Request.Form["autologin"] != null)
						{
							HttpCookie loginId = new HttpCookie("LoginId", model.LoginId);
							loginId.Expires = DateTime.MaxValue;
							Response.Cookies.Add(loginId);
						}
						else
						{
							delCookie();
						}
						//用用户的登录名替换原来匿名的GUID
						MigrateShoppingCart(user.LoginId);
						return Redirect(returnUrl);
					}
					else
					{
						ModelState.AddModelError("LoginPwd","密码输入错误");
					}
				}
			}
			ViewBag.ReturnUrl = returnUrl;
			return View(model);
		}

		//在用户注册登录之后用用户的登录名替换原来匿名的GUID
		[NonAction]
		private void MigrateShoppingCart(string userName)
		{
            var cart = ShoppingCart.GetCart(this.HttpContext);
            if (cart.ShoppingCartId == null || cart.ShoppingCartId == userName)
            {
                return;
            }
            cart.MigrateCart(userName);
            Session[ShoppingCart.CartSessionKey] = userName;
		}

		//退出
		public ActionResult Exit()
		{
			if (Session["User"] == null)
			{
				return RedirectToAction("Index", "Home");
			}
			Session.Remove("User");
            Session[ShoppingCart.CartSessionKey] = null;
			delCookie();
			return RedirectToAction("Login", "Account");
		}

		//删除cookie
		[NonAction]
		private void delCookie()
		{
			//使cookie失效
			HttpCookie loginId = Request.Cookies["LoginId"];
			if (loginId != null)
			{
				loginId.Expires = DateTime.Now.AddDays(-1d);
				Response.Cookies.Add(loginId);
			}
		}

		//显示注册页
		public ActionResult Register()
		{
			return View();
		}

		//处理注册
		[HttpPost]
		public ActionResult Register(UserInfo model)
		{
			if (ModelState.IsValid)
			{
				bool flag = false;//是否未通过
				//验证码是否正确
				if (!model.SecurityCode.ToUpper().Equals(TempData["SecurityCode"]))
				{
					ModelState.AddModelError("SecurityCode", "验证码输入有误");
					flag = true;
				}
				//用户名是否重复
				if (context.UserInfoes.Where(m=>m.LoginId == model.LoginId.Trim().ToLower()).Count()>0)
				{
					ModelState.AddModelError("LoginId", "用户名已存在");
					flag = true;
				}
				if (flag)
				{
					return View(model);
				}
				model.LoginId = model.LoginId.Trim().ToLower();
				model.LoginPwd = model.LoginPwd.Trim().ToLower();
				model.Name = model.Name.Trim();
				context.UserInfoes.Add(model);
				context.SaveChanges();
				return Content("<script>alert('注册成功，现在跳转到登录页面');location.href='" + Url.Action("Login", "Account") + "';</script>");
			}
			else
			{
				if (model != null)
				{
					//验证码是否正确
					if (model.SecurityCode != null && !model.SecurityCode.ToUpper().Equals(TempData["SecurityCode"]))
					{
						ModelState.AddModelError("SecurityCode", "验证码输入有误");
					}
					//用户名是否重复
					if (model.LoginId != null && context.UserInfoes.Where(m => m.LoginId == model.LoginId.Trim().ToLower()).Count() > 0)
					{
						ModelState.AddModelError("LoginId", "用户名已存在");
					}
				}
				return View(model);
			}
		}

		#region  生成验证码图片
		public ActionResult SecurityCode()
		{
			SecurityCode sc = new SecurityCode();
			string oldcode = TempData["SecurityCode"] as string;
			string code = sc.CreateRandomCode(5);
			TempData["SecurityCode"] = code;
			return File(sc.CreateValidateGraphic(code), "image/Jpeg");
		}
		#endregion
    }
}
