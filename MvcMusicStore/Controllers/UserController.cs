using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;

namespace MvcMusicStore.Controllers
{
    public class UserController : Controller
    {

        private MvcMusicStoreEntities context = new MvcMusicStoreEntities();

        //显示用户个人首页
        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = Session["User"] as UserInfo;
            return View(user);
        }

        //显示修改用户资料页面
        public ActionResult Edit()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = Session["User"] as UserInfo;
            EditUserModel model = new EditUserModel
            {
                Id = user.Id,
                LoginId = user.LoginId,
                Name = user.Name,
                Address = user.Address,
                Email = user.Email,
                Phone = user.Phone,
                Birthday = user.Birthday
            };
            return View(model);
        }

        //修改用户资料
        [HttpPost]
        public ActionResult Edit(EditUserModel model)
        {
            if (ModelState.IsValid)
            {
                int id = ((UserInfo)Session["User"]).Id;
                var user = context.UserInfoes.Find(id);
                user.Email = model.Email.Trim();
                user.Name = model.Name.Trim();
                user.Phone = model.Phone.Trim();
                user.Address = model.Address.Trim();
                user.Birthday = model.Birthday;
                user.LoginPwdConfirm = user.LoginPwd;
                user.SecurityCode = "123";
                context.SaveChanges();
                //更新Session里存储的User对象
                Session["User"] = user;
                TempData["message"] = "修改个人信息成功";
                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
            }
        }

        //显示修改用户密码页面
        public ActionResult EditLoginPwd()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //修改用户密码
        [HttpPost]
        public ActionResult EditLoginPwd(EditLoginPwdModel model)
        {
            if (ModelState.IsValid)
            {
                int id = ((UserInfo)Session["User"]).Id;
                var user = context.UserInfoes.Find(id);
                //原密码输入不正确
                if (!model.OldLoginPwd.Trim().ToLower().Equals(user.LoginPwd.Trim().ToLower()))
                {
                    ModelState.AddModelError("OldLoginPwd","密码输入错误");
                    return View(model);
                }
                //修改新密码
                user.LoginPwd = model.NewLoginPwd;
                user.LoginPwdConfirm = user.LoginPwd;
                user.SecurityCode = "123";
                context.SaveChanges();
                //更新Session里存储的User对象
                Session["User"] = user;
                TempData["message"] = "修改密码成功，请牢记您的新密码，切勿泄露给他人";
                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
            }
        }
    }
}
