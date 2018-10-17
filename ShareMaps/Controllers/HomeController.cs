using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using ShareMaps.Helpers;
using ShareMaps.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ShareMaps.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var sme = new ShareMapsEntities())
            {
                string userId = User.Identity.GetUserId() != null ? User.Identity.GetUserId() : string.Empty;
                HomeIndexViewModel homeIndexViewModel = new HomeIndexViewModel();
                homeIndexViewModel.stores = (from s in sme.Stores
                                             select new StoreViewModel()
                                             {
                                                 canEditDelete = s.UserId.Equals(userId) ? true : false,
                                                 store = s,
                                                 storePhotos = s.Photos.ToList(),
                                                 storeTags = s.Tags.ToList()
                                             }).ToList();
                homeIndexViewModel.tags = (from t in sme.Tags
                                           where t.UserId.Equals(userId)
                                           orderby t.Id descending
                                           select new TagCountViewModel
                                           {
                                               tagId = t.Id,
                                               tagName = t.Name,
                                               tagCount = t.Stores.Count,
                                               iconValue = t.Icons.Value
                                           }).ToList();
                var unTagStore = from s in sme.Stores
                                 where s.UserId.Equals(userId) &&
                                 s.Tags.Count.Equals(0)
                                 select s;
                homeIndexViewModel.tags.Add(new TagCountViewModel
                {
                    tagId = -1,
                    tagName = "未標記",
                    tagCount = unTagStore.Count(),
                    iconValue = sme.Icons.Find(1).Value
                });
                return View(homeIndexViewModel);
            }
        }

        // GET: Home/GetVisibleMarker
        [HttpGet]
        public ActionResult GetVisibleMarker(int id)
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                using (var sme = new ShareMapsEntities())
                {
                    try
                    {
                        if (!id.Equals(-1))
                        {
                            Tags tags = sme.Tags.Find(id);

                            if (tags.Stores.Count > 0)
                            {
                                var stores = from t in tags.Stores
                                             select new
                                             {
                                                 lat = t.Lat,
                                                 lng = t.Lng
                                             };
                                jsonHelper.data = stores;
                            }
                        }
                        else
                        {
                            string userId = User.Identity.GetUserId() != null ? User.Identity.GetUserId() : string.Empty;
                            var stores = (from s in sme.Stores
                                     where s.UserId.Equals(userId) && s.Tags.Count.Equals(0)
                                     select new
                                     {
                                         lat = s.Lat,
                                         lng = s.Lng
                                     }).ToList();
                            jsonHelper.data = stores;
                        }
                        jsonHelper.status = true;
                        jsonHelper.message = "資料取得成功";
                        return Json(jsonHelper, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {
                        jsonHelper.status = false;
                        jsonHelper.message = "資料取得錯誤";
                        return Json(jsonHelper, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return RedirectToAction("Login", "Account");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // POST: Home/FileUpload
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult FileUploadToTempFolder()
        {
            JsonHelper jsonHelper = new JsonHelper();
            try
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        if (!Directory.Exists(Server.MapPath("~/tempPhotos/")))
                            Directory.CreateDirectory(Server.MapPath("~/tempPhotos/"));
                        var extension = Path.GetExtension(file.FileName);
                        var fileName = Guid.NewGuid() + extension;
                        var tempPath = "/tempPhotos/" + fileName;
                        var path = Path.Combine(Server.MapPath("~/tempPhotos/"), fileName);
                        file.SaveAs(path);

                        string deleteURL = Url.Action("FileDeleteFromTempFolder", "Home", new { filename = fileName });
                        var showURL = Url.Content(tempPath);
                        var config = new
                        {
                            initialPreview = showURL,
                            initialPreviewConfig = new[] {
                                    new {
                                        caption = fileName,
                                        url = deleteURL,
                                        key =fileName,
                                        width = "120px"
                                    }
                                },
                            append = true
                        };
                        return Json(config);
                    }
                }

                jsonHelper.status = false;
                jsonHelper.message = "尚未接收到檔案";
                return Json(jsonHelper);
            }
            catch
            {
                jsonHelper.status = false;
                jsonHelper.message = "圖片上傳過程出現問題";
                return Json(jsonHelper);
            }
        }

        // POST: Home/FileUpload
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult FileUpload(string id)
        {
            JsonHelper jsonHelper = new JsonHelper();
            try
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        using (var sme = new ShareMapsEntities())
                        {
                            string photoFolder = Server.MapPath("~/Photos/Store/" + id + "/");
                            if (!Directory.Exists(photoFolder))
                                Directory.CreateDirectory(photoFolder);
                            var extension = Path.GetExtension(file.FileName);
                            var fileName = Guid.NewGuid() + extension;
                            var dbPath = "/Photos/Store/" + id + "/" + fileName;
                            var path = Path.Combine(photoFolder, fileName);
                            file.SaveAs(path);

                            Stores stores = sme.Stores.Find(id);
                            Photos photos = new Photos()
                            {
                                Path = dbPath,
                                IsMain = stores.Photos.Count.Equals(0) ? true : false,
                                Sequence = stores.Photos.Count
                            };
                            stores.Photos.Add(photos);
                            sme.SaveChanges();

                            string deleteURL = Url.Action("FileDelete", "Home", new { id = photos.Id });
                            var urlPath = Url.Content(dbPath);
                            var config = new
                            {
                                initialPreview = urlPath,
                                initialPreviewConfig = new[] {
                                    new {
                                        caption = fileName,
                                        url = deleteURL,
                                        key =fileName,
                                        width = "120px"
                                    }
                                },
                                append = true
                            };
                            return Json(config);
                        }
                    }
                }

                jsonHelper.status = false;
                jsonHelper.message = "尚未接收到檔案";
                return Json(jsonHelper);
            }
            catch
            {
                jsonHelper.status = false;
                jsonHelper.message = "圖片上傳過程出現問題";
                return Json(jsonHelper);
            }
        }

        // POST: Home/FileDelete/Filename
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult FileDeleteFromTempFolder(string filename)
        {
            JsonHelper jsonHelper = new JsonHelper();
            string path = Path.Combine(Server.MapPath("~/tempPhotos/"), filename);
            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/tempPhotos/"), filename)))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(path);
                    fileInfo.Delete();
                    jsonHelper.status = true;
                    jsonHelper.message = "檔案刪除成功";
                }
                catch
                {
                    jsonHelper.status = false;
                    jsonHelper.message = "檔案刪除過程中出現問題";
                }
            }

            jsonHelper.data = new
            {
                filename = filename
            };
            return Json(jsonHelper);
        }

        // POST: Home/FileDelete/id
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult FileDelete(int id)
        {
            using (var sme = new ShareMapsEntities())
            {
                JsonHelper jsonHelper = new JsonHelper();
                Photos photos = sme.Photos.Find(id);
                string path = Server.MapPath("~" + photos.Path);
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        fileInfo.Delete();
                        sme.Photos.Remove(photos);
                        sme.SaveChanges();
                        jsonHelper.status = true;
                        jsonHelper.message = "檔案刪除成功";
                    }
                    catch
                    {
                        jsonHelper.status = false;
                        jsonHelper.message = "檔案刪除過程中出現問題";
                    }
                }
                return Json(jsonHelper);
            }
        }

        // GET: Home/StoreCreate
        public ActionResult StoreCreate(decimal lat, decimal lng, string address)
        {
            if (Request.IsAuthenticated)
            {
                using (var sme = new ShareMapsEntities())
                {
                    try
                    {
                        string userId = User.Identity.GetUserId() != null ? User.Identity.GetUserId() : string.Empty;
                        StoreCreateViewModel storeCreateViewModel = new StoreCreateViewModel();
                        Stores stores = new Stores
                        {
                            Address = address,
                            Lat = lat,
                            Lng = lng
                        };
                        storeCreateViewModel.store = stores;
                        storeCreateViewModel.storeTags = (from t in sme.Tags
                                                          where t.UserId.Equals(userId)
                                                          select new TagViewModel
                                                          {
                                                              tag = t,
                                                              isChecked = false
                                                          }).ToList();
                        return PartialView("_StoreCreatePartial", storeCreateViewModel);
                    }
                    catch (Exception ex)
                    {
                        JsonHelper jsonHelper = new JsonHelper();
                        jsonHelper.message = ex.ToString();
                        return Json(jsonHelper);
                    }
                }
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Home/StoreCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StoreCreate(StoreCreateViewModel storeCreateViewModel)
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                if (ModelState.IsValid)
                {
                    using (var sme = new ShareMapsEntities())
                    {
                        try
                        {
                            Stores stores = new Stores();
                            stores = storeCreateViewModel.store;
                            stores.Id = Guid.NewGuid().ToString();
                            stores.CreateDate = DateTime.Now;
                            stores.UpdateDate = DateTime.Now;
                            stores.UserId = User.Identity.GetUserId();
                            if (storeCreateViewModel.storeTags != null)
                            {
                                var checkedTag = from t in storeCreateViewModel.storeTags
                                                 where t.isChecked.Equals(true)
                                                 select t.tag.Id;
                                stores.Tags = (from t in sme.Tags
                                               where checkedTag.Contains(t.Id)
                                               select t).ToList();
                            }

                            string photoFolder = Server.MapPath("~/Photos/Store/" + stores.Id);
                            if (!Directory.Exists(photoFolder))
                                Directory.CreateDirectory(photoFolder);

                            if (storeCreateViewModel.filenames != null)
                            {
                                foreach (string filename in storeCreateViewModel.filenames)
                                {
                                    string filePath = Path.Combine(Server.MapPath("~/tempPhotos/"), filename);
                                    System.IO.File.Move(filePath, photoFolder + @"\" + filename);
                                    Photos photo = new Photos
                                    {
                                        StoreId = stores.Id,
                                        Sequence = storeCreateViewModel.filenames.IndexOf(filename),
                                        IsMain = storeCreateViewModel.filenames.IndexOf(filename).Equals(0) ? true : false,
                                        Path = "/Photos/Store/" + stores.Id + "/" + filename
                                    };
                                    stores.Photos.Add(photo);
                                }
                            }
                            sme.Stores.Add(stores);
                            sme.SaveChanges();

                            jsonHelper.status = true;
                            jsonHelper.message = "新增成功";
                            jsonHelper.data = new []
                            {
                                new
                                {
                                    canEditDelete = true,
                                    id = stores.Id,
                                    name = stores.Name,
                                    address = stores.Address,
                                    phoneNumber = stores.PhoneNumber,
                                    description = string.IsNullOrEmpty(stores.Description) ? "" : stores.Description.Replace("\r\n", "<br />"),
                                    shareTime = stores.ShareTime,
                                    lat = stores.Lat,
                                    lng = stores.Lng,
                                    photos = from sp in stores.Photos
                                             select new
                                             {
                                                filename = Path.GetFileName(sp.Path),
                                                path = Url.Content("~" + sp.Path),
                                                isMain = sp.IsMain,
                                                sequence = sp.Sequence
                                             }
                                }
                            };
                            return Json(jsonHelper);
                        }
                        catch
                        {
                            jsonHelper.status = false;
                            jsonHelper.message = "新增過程出現問題";
                            return Json(jsonHelper);
                        }
                    }
                }
                jsonHelper.status = false;
                jsonHelper.message = "驗證出現問題";
                return Json(jsonHelper);
            }
            return RedirectToAction("Login", "Account");
        }

        // GET: Home/StoreEdit/id
        public ActionResult StoreEdit(string id)
        {
            if (Request.IsAuthenticated)
            {
                using (var sme = new ShareMapsEntities())
                {
                    string userId = User.Identity.GetUserId() != null ? User.Identity.GetUserId() : string.Empty;
                    StoreEditViewModel storeEditViewModel = new StoreEditViewModel();
                    Stores store = sme.Stores.Find(id);
                    storeEditViewModel.store = store;
                    storeEditViewModel.storePhotos = store.Photos.ToList();
                    var storeTagList = (from st in store.Tags
                                        select st.Id).ToList();
                    storeEditViewModel.storeTags = (from t in sme.Tags
                                                    where t.UserId.Equals(userId)
                                                    select new TagViewModel
                                                    {
                                                        tag = t,
                                                        isChecked = storeTagList.Contains(t.Id) ? true : false
                                                    }).ToList();
                    return PartialView("_StoreEditPartial", storeEditViewModel);
                }
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Home/StoreEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StoreEdit(StoreEditViewModel storeEditViewModel)
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                if (ModelState.IsValid)
                {
                    using (var sme = new ShareMapsEntities())
                    {
                        try
                        {
                            Stores stores = sme.Stores.Find(storeEditViewModel.store.Id);
                            string userId = User.Identity.GetUserId();
                            if (stores.UserId.Equals(userId))
                            {
                                stores.Name = storeEditViewModel.store.Name;
                                stores.Address = storeEditViewModel.store.Address;
                                stores.PhoneNumber = storeEditViewModel.store.PhoneNumber;
                                stores.Description = storeEditViewModel.store.Description;
                                stores.Lat = storeEditViewModel.store.Lat;
                                stores.Lng = storeEditViewModel.store.Lng;
                                stores.UpdateDate = DateTime.Now;
                                stores.UserId = userId;

                                if (storeEditViewModel.storeTags != null)
                                {
                                    stores.Tags.Clear();
                                    var checkedTag = (from st in storeEditViewModel.storeTags
                                                      where st.isChecked.Equals(true)
                                                      select st.tag.Id).ToList();
                                    stores.Tags = (from t in sme.Tags
                                                   where checkedTag.Contains(t.Id) &&
                                                   t.UserId.Equals(stores.UserId)
                                                   select t).ToList();
                                }
                                sme.SaveChanges();

                                jsonHelper.status = true;
                                jsonHelper.message = "修改成功";
                                jsonHelper.data = new
                                {
                                    id = stores.Id,
                                    name = stores.Name,
                                    address = stores.Address,
                                    phonenumber = stores.PhoneNumber,
                                    description = string.IsNullOrEmpty(stores.Description) ? "" : stores.Description.Replace("\r\n", "<br />"),
                                    lat = stores.Lat,
                                    lng = stores.Lng,
                                    sharetime = stores.ShareTime,
                                    photos = from sp in stores.Photos
                                             select new
                                             {
                                                 filename = Path.GetFileName(sp.Path),
                                                 path = Url.Content("~" + sp.Path),
                                                 isMain = sp.IsMain,
                                                 sequence = sp.Sequence
                                             }
                                };
                                return Json(jsonHelper);
                            }
                            else
                            {
                                return RedirectToAction("Index");
                            }
                        }
                        catch
                        {
                            jsonHelper.status = false;
                            jsonHelper.message = "修改過程出現問題";
                            return Json(jsonHelper);
                        }
                    }
                }
                jsonHelper.status = false;
                jsonHelper.message = "驗證出現問題";
                return Json(jsonHelper);
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Home/StoreDelete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StoreDelete(string id)
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                if (ModelState.IsValid)
                {
                    using (var sme = new ShareMapsEntities())
                    {
                        try
                        {
                            Stores stores = sme.Stores.Find(id);
                            if (stores.UserId.Equals(User.Identity.GetUserId()))
                            {
                                //清除關聯
                                stores.Tags.Clear();
                                stores.Photos.Clear();
                                //清除photos records
                                var photos = (from p in sme.Photos
                                              where p.Stores.Id.Equals(stores.Id)
                                              select p).ToList();
                                sme.Photos.RemoveRange(photos);
                                sme.Stores.Remove(stores);
                                sme.SaveChanges();

                                jsonHelper.status = true;
                                jsonHelper.message = "刪除成功";
                                return Json(jsonHelper);
                            }
                            else
                            {
                                return RedirectToAction("Index");
                            }
                        }
                        catch
                        {
                            jsonHelper.status = false;
                            jsonHelper.message = "刪除過程出現問題";
                            return Json(jsonHelper);
                        }
                    }
                }
                jsonHelper.status = false;
                jsonHelper.message = "驗證出現問題";
                return Json(jsonHelper);
            }
            return RedirectToAction("Login", "Account");
        }

        #region TagsManagement
        // GET: Home/TagsManagement
        public ActionResult TagsManagement()
        {
            if (Request.IsAuthenticated)
            {
                using (var sme = new ShareMapsEntities())
                {
                    try
                    {
                        string userId = User.Identity.GetUserId() != null ? User.Identity.GetUserId() : string.Empty;
                        TagsManagementViewModel tagsManagementViewModel = new TagsManagementViewModel();
                        SelectList selectLists = new SelectList(sme.Icons, "Id", "Unicode");
                        tagsManagementViewModel.iconList = sme.Icons.ToList();
                        tagsManagementViewModel.iconTagList = (from t in sme.Tags
                                                               where t.UserId.Equals(userId)
                                                               orderby t.Id descending
                                                               select new IconTagViewModel
                                                               {
                                                                   tag = t,
                                                                   icon = (from i in sme.Icons
                                                                           where i.Id.Equals(t.IconId)
                                                                           select i).FirstOrDefault()
                                                               }).ToList();
                        return PartialView("_TagsManagementPartial", tagsManagementViewModel);
                    }
                    catch
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Home/TagCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TagCreate([Bind(Prefix = "addIconTag")]IconTagViewModel iconTagViewModel)
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                if (ModelState.IsValid)
                {
                    using (var sme = new ShareMapsEntities())
                    {
                        try
                        {
                            string userId = User.Identity.GetUserId();

                            var tagSequence = from t in sme.Tags
                                              where t.UserId.Equals(userId)
                                              select t;
                            iconTagViewModel.tag.Sequence = tagSequence.Count();
                            iconTagViewModel.tag.UserId = userId;
                            sme.Tags.Add(iconTagViewModel.tag);
                            sme.SaveChanges();

                            Icons addIcon = sme.Icons.Find(iconTagViewModel.tag.IconId);

                            var data = new
                            {
                                id = iconTagViewModel.tag.Id,
                                name = iconTagViewModel.tag.Name,
                                sequence = iconTagViewModel.tag.Sequence,
                                iconId = addIcon.Id,
                                iconValue = addIcon.Value
                            };

                            jsonHelper.status = true;
                            jsonHelper.message = "新增成功";
                            jsonHelper.data = data;
                            return Json(jsonHelper);
                        }
                        catch
                        {
                            jsonHelper.status = false;
                            jsonHelper.message = "新增過程出現問題";
                            return Json(jsonHelper);
                        }
                    }
                }
                jsonHelper.status = false;
                jsonHelper.message = "驗證出現問題";
                return Json(jsonHelper);
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Home/TagEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TagEdit(int id, string name, int? sequence, int iconId)
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                if (ModelState.IsValid)
                {
                    using (var sme = new ShareMapsEntities())
                    {
                        try
                        {
                            Tags tags = sme.Tags.Find(id);
                            if (tags.UserId.Equals(User.Identity.GetUserId()))
                            {
                                tags.Name = name;
                                tags.Sequence = sequence;
                                tags.IconId = iconId;
                                sme.SaveChanges();

                                Icons addIcon = sme.Icons.Find(tags.IconId);
                                var data = new
                                {
                                    id = tags.Id,
                                    name = tags.Name,
                                    sequence = tags.Sequence,
                                    iconId = addIcon.Id,
                                    iconValue = addIcon.Value
                                };

                                jsonHelper.status = true;
                                jsonHelper.message = "修改成功";
                                jsonHelper.data = data;
                                return Json(jsonHelper);
                            }
                            else
                            {
                                return RedirectToAction("Index");
                            }
                        }
                        catch
                        {
                            jsonHelper.status = false;
                            jsonHelper.message = "修改過程出現問題";
                            return Json(jsonHelper);
                        }
                    }
                }
                jsonHelper.status = false;
                jsonHelper.message = "驗證出現問題";
                return Json(jsonHelper);
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Home/TagDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TagDelete(int? id)
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                if (ModelState.IsValid)
                {
                    using (var sme = new ShareMapsEntities())
                    {
                        try
                        {
                            Tags tags = sme.Tags.Find(id);
                            if (tags.UserId.Equals(User.Identity.GetUserId()))
                            {
                                sme.Tags.Remove(tags);
                                sme.SaveChanges();

                                jsonHelper.status = true;
                                jsonHelper.message = "刪除成功";
                                return Json(jsonHelper);
                            }
                            else
                            {
                                return RedirectToAction("Index");
                            }
                        }
                        catch
                        {
                            jsonHelper.status = false;
                            jsonHelper.message = "修改過程出現問題";
                            return Json(jsonHelper);
                        }
                    }
                }
                jsonHelper.status = false;
                jsonHelper.message = "驗證出現問題";

                return Json(jsonHelper);
            }
            return RedirectToAction("Login", "Account");
        }

        // GET: Home/GetTagsLsit
        [HttpGet]
        public ActionResult GetTagsLsit()
        {
            if (Request.IsAuthenticated)
            {
                JsonHelper jsonHelper = new JsonHelper();
                using (var sme = new ShareMapsEntities())
                {
                    try
                    {
                        string userId = User.Identity.GetUserId() != null ? User.Identity.GetUserId() : string.Empty;

                        jsonHelper.status = true;
                        jsonHelper.message = "";
                        List<TagCountViewModel> ListTagCountViewModels = (from t in sme.Tags
                                                                          where t.UserId.Equals(userId)
                                                                          orderby t.Id descending
                                                                          select new TagCountViewModel
                                                                          {
                                                                              tagId = t.Id,
                                                                              tagName = t.Name,
                                                                              tagCount = t.Stores.Count,
                                                                              iconValue = t.Icons.Value
                                                                          }).ToList();

                        var unTagStore = (from s in sme.Stores
                                          where s.UserId.Equals(userId) &&
                                          s.Tags.Count.Equals(0)
                                          select s).ToList();

                        ListTagCountViewModels.Add(new TagCountViewModel
                        {
                            tagId = -1,
                            tagName = "未標記",
                            tagCount = unTagStore.Count(),
                            iconValue = sme.Icons.Find(1).Value
                        });

                        jsonHelper.data = ListTagCountViewModels;

                        return Json(jsonHelper, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {
                        jsonHelper.status = false;
                        jsonHelper.message = "資料取得錯誤";
                        return Json(jsonHelper, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return RedirectToAction("Login", "Account");
        }
        #endregion
    }
}