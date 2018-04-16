using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;

namespace MvcMusicStore.Controllers
{
    public class HomeController : Controller
    {

		private MvcMusicStoreEntities context = new MvcMusicStoreEntities ();

        //显示首页
        public ActionResult Index()
        {
			var albums = context.Albums
								.OrderByDescending(m=>m.AlbumId)
								.Take(5).ToList();
            return View(albums);
        }

		//分部视图显示全部音乐种类
		public ActionResult GenreTree()
		{
			//如果音乐种类没有缓存，则新建缓存
			if (HttpContext.Cache["trees"] == null)
			{
				HttpContext.Cache["trees"] = context.Genres.ToList();
			}
			IList<Genre> genres = HttpContext.Cache["trees"] as IList<Genre>;
			return PartialView(genres);//返回分部视图
		}
    }
}
