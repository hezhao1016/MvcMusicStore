using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using MvcMusicStore.Helpers;

namespace MvcMusicStore.Controllers
{
    public class StoreController : Controller
    {

		private MvcMusicStoreEntities context = new MvcMusicStoreEntities ();

        //显示全部音乐种类
        public ActionResult Index()
        {
            //如果音乐种类没有缓存，则新建缓存
            if (HttpContext.Cache["trees"] == null)
            {
                HttpContext.Cache["trees"] = context.Genres.ToList();
            }
            IList<Genre> genres = HttpContext.Cache["trees"] as IList<Genre>;
            return View(genres);
        }

		//显示专辑详细信息页面
		public ActionResult Detail(int id = 0)
		{
            if (id == 0)
            {
                return RedirectToAction("Index","Home");
            }
			var album = context.Albums.Find(id);
			return View(album);
		}

		//根据音乐种类浏览专辑
		public ActionResult Browse(string keyword,int id = 0,int pageIndex = 1)
		{
            List<Album> albums = null;
            //根据关键字与类型查找
            if (!string.IsNullOrEmpty(keyword))
            {
                albums = context.Albums
								.Where(m=>m.GenreId == id 
								    && (m.Artist.Name.Contains(keyword) || m.Title.Contains(keyword)))
								.OrderByDescending(m=>m.AlbumId)
								.Select(m=>m).ToList();
            }//根据类型查找
            else
            {
                albums = context.Albums
                                .Where(m => m.GenreId == id)
                                .OrderByDescending(m => m.AlbumId)
                                .Select(m => m).ToList();
            }
			int pageSize = 20;
			if (Request.RequestType == "POST")
			{
				pageIndex = 1;
			}
			var pagedAlbums = new PagedList<Album>(albums,pageSize,pageIndex);
			ViewBag.GenreName = context.Genres.Find(id).Name;
			ViewBag.KeyWord = keyword;
			return View(pagedAlbums);
		}
    }
}
