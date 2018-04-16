using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using MvcMusicStore.Helpers;
using System.IO;

namespace MvcMusicStore.Areas.Admin.Controllers
{
    [Authorize]
    public class StoreManagerController : Controller
    {

        private MvcMusicStoreEntities context = new MvcMusicStoreEntities();

        //显示后台管理首页
        public ActionResult Index(int? genreId, string keyword, int pageIndex = 1)
        {
            List<Album> albums = null;
            //根据音乐类型查找
            if (genreId.HasValue && string.IsNullOrEmpty(keyword))
	        {
                albums = context.Albums
                                .Where(a => a.GenreId == genreId.Value)
                                .OrderByDescending(a=>a.AlbumId)
                                .Select(a => a).ToList();
            }//根据关键字查找
            else if (!genreId.HasValue && !string.IsNullOrEmpty(keyword))
            {
                albums = context.Albums
                                .Where(a => a.Title.Contains(keyword) || a.Artist.Name.Contains(keyword))
                                .OrderByDescending(a=>a.AlbumId)
                                .Select(a => a).ToList();
            }//根据关键字与音乐类型查找
            else if (genreId.HasValue && !string.IsNullOrEmpty(keyword))
            {
                albums = context.Albums
                               .Where(a => a.GenreId == genreId.Value 
                                    && (a.Title.Contains(keyword) || a.Artist.Name.Contains(keyword)))
                               .OrderByDescending(a => a.AlbumId)
                               .Select(a => a).ToList();
            }//查询全部
            else
            {
                albums = context.Albums.OrderByDescending(a => a.AlbumId).ToList();
            }
            //分页
            int pageSize = 20;
            if (Request.RequestType == "POST")
            {
                pageIndex = 1;
            }
            var pagedAlbums = new PagedList<Album>(albums, pageSize, pageIndex);
            ViewBag.KeyWord = keyword;
            ViewBag.GenreId = new SelectList(context.Genres.ToList(), "GenreId", "Name");
            return View(pagedAlbums);
        }

        //显示专辑详情页
        public ActionResult Detail(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var album = context.Albums.Find(id);
            return View(album);
        }

        //显示添加专辑页
        public ActionResult Create()
        {
            PrepareSelectedItems(null);
            return View();
        }

        //添加专辑
        [HttpPost]
        public ActionResult Create(Album album, HttpPostedFileBase cover)
        {
            if (ModelState.IsValid)
            {
                if (UploadFiles(album, cover))
                {
                    context.Albums.Add(album);
                    context.SaveChanges();
                    string info = "<script>location.href=confirm('添加专辑成功，是否继续添加专辑？')?'" + Url.Action("Create") + "':'" + Url.Action("Index") + "';</script>";
                    return Content(info);
                }
                else
                {
                    //重新获得下拉列表数据
                    PrepareSelectedItems(album);
                    return View(album);
                }
            }
            else
            {
                //给出版社与分类添加错误提示
                if (album.GenreId == 0)
                {
                    ModelState.AddModelError("GenreId", "请选择音乐流派");
                }
                if (album.ArtistId == 0)
                {
                    ModelState.AddModelError("ArtistId", "请选择艺术家");
                }
                //重新获得下拉列表数据
                PrepareSelectedItems(album);
                return View(album);
            }
        }

        //显示编辑专辑页面
        public ActionResult Edit(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var album = context.Albums.Find(id);
            PrepareSelectedItems(album);
            return View(album);
        }

        //编辑专辑
        [HttpPost]
        public ActionResult Edit(Album album, HttpPostedFileBase cover)
        {
            if (ModelState.IsValid)
            {
                if (UploadFiles(album, cover))
                {
                    var context_album = context.Albums.Find(album.AlbumId);
                    context_album.AlbumArtUrl = album.AlbumArtUrl;
                    context_album.ArtistId = album.ArtistId;
                    context_album.GenreId = album.GenreId;
                    context_album.Price = album.Price;
                    context_album.Title = album.Title;
                    context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    //重新获得下拉列表数据
                    PrepareSelectedItems(album);
                    return View(album);
                }
            }
            else
            {
                //给出版社与分类添加错误提示
                if (album.GenreId == 0)
                {
                    ModelState.AddModelError("GenreId", "请选择音乐流派");
                }
                if (album.ArtistId == 0)
                {
                    ModelState.AddModelError("ArtistId", "请选择艺术家");
                }
                //重新获得下拉列表数据
                PrepareSelectedItems(album);
                return View(album);
            }
        }

        //准备下拉列表数据
        [NonAction]
        private void PrepareSelectedItems(Album album)
        {
            //添加
            if (album == null)
            {
                ViewBag.GenreId = new SelectList(context.Genres.ToList(), "GenreId", "Name");
                ViewBag.ArtistId = new SelectList(context.Artists.ToList(), "ArtistId", "Name");
            }
            else//修改
            {
                ViewBag.GenreId = new SelectList(context.Genres.ToList(), "GenreId", "Name", album.GenreId);
                ViewBag.ArtistId = new SelectList(context.Artists.ToList(), "ArtistId", "Name", album.ArtistId);
            }
        }

        //上传文件
        [NonAction]
        private bool UploadFiles(Album album, HttpPostedFileBase cover)
        {
            try
            {
                //原来的AlbumArtUrl
                string oldAlbumArtUrl = album.AlbumId != 0 ? context.Albums.Find(album.AlbumId).AlbumArtUrl : null;
                
                //没有选择图片，或选择0字节文件
                if (cover == null || cover.ContentLength == 0)
                {
                    //如果是修改专辑，且没有选择封面，则仍用原来的图片
                    if (album.AlbumId != 0)
                    {
                        album.AlbumArtUrl = oldAlbumArtUrl;
                    }//如果是添加专辑，就使用默认图片
                    else
                    {
                        album.AlbumArtUrl = "placeholder.gif";
                    }
                    return true;
                }
                
                //获取后缀名
                string exeName = cover.FileName.Substring(cover.FileName.LastIndexOf(".") + 1);
                //图片后缀名数组
                List<string> pictureExeList = new List<string> { "jpg", "png", "gif", "bmp", "jpeg", "pcx", "tiff", "psd", "tga", "eps", "raw" };

                //图片格式控制
                if (!pictureExeList.Contains(exeName.ToLower()))
                {
                    ModelState.AddModelError("", "您上传的图片格式不正确");
                    return false;
                }
                int size = cover.ContentLength / 1024 / 1024;
                //图片大小控制
                if (size > 5)
                {
                    ModelState.AddModelError("", "图片不能大于5MB");
                    return false;
                }

                //获取时间
                string now = DateTime.Now.ToString("yyyyMMddhhmmss");
                //获取1到1000000的随机数
                Random rd = new Random();
                int math = rd.Next(1000000);
                //文件夹路径
                string path = Server.MapPath("~/Images/Covers/");

                //如果是修改专辑且图片不是默认图片，就删除原来的图片
                if (album.AlbumId != 0 && oldAlbumArtUrl != "placeholder.gif")
                {
                    FileInfo file = new FileInfo(path + oldAlbumArtUrl);
                    if (file.Exists)
                    {
                        file.Delete();//删除图片
                    }
                }

                //完整文件名称
                String fileName = now + "_" + math + "." + exeName;
                //完整路径
                path += fileName;
                //上传文件
                cover.SaveAs(path);
                album.AlbumArtUrl = fileName;
                return true;
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "图片上传失败");
                return false;
            }
        }

        //显示删除专辑页面
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.AlbumTitle = context.Albums.Find(id).Title;
            return View();
        }

        //处理删除专辑
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DoDelete(int id)
        {
            try
            {
                var album = context.Albums.Find(id);
                //文件夹路径
                string path = Server.MapPath("~/Images/Covers/");
                string albumArtUrl = album.AlbumArtUrl;
                //从数据库删除
                context.Albums.Remove(album);
                context.SaveChanges();
                //再从磁盘上删除图片
                if (albumArtUrl != "placeholder.gif")
                {
                    FileInfo file = new FileInfo(path + albumArtUrl);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                string message = "数据库在执行删除专辑操作时，出现异常。";
                Exception ex = new Exception(message);
                return View("Error", new HandleErrorInfo(ex, @"Admin\StoreManager", "Delete"));
            }
        }
    }
}
