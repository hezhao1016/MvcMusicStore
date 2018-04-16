using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using System.Web.Security;

namespace MvcMusicStore.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {

        private MvcMusicStoreEntities context = new MvcMusicStoreEntities();

        //显示管理员登录页
        public ActionResult Login()
        {
            return View();
        }

        //处理管理员登录
        [HttpPost]
        public ActionResult Login(LoginInfoModel model,string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var admin = context.Admins.Where(m=>m.LoginId==model.LoginId.Trim().ToLower()).SingleOrDefault();
                if (admin == null)
				{
					ModelState.AddModelError("LoginId","用户名不存在");
				}
				else
				{
                    if (model.LoginPwd.Trim().ToLower() == admin.LoginPwd)
					{
                        //记住凭据
                        FormsAuthentication.SetAuthCookie(admin.LoginId, false);
						if (returnUrl != null)
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "StoreManager");
                        }
					}
					else
					{
						ModelState.AddModelError("LoginPwd","密码输入错误");
					}
				}
            }
            return View(model);
        }
        //退出
        [Authorize]
        public ActionResult Exit()
        {
            //删除凭据
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "../Home");
        }
    }
}
