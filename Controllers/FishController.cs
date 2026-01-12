using Khati.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace KhatiSS.Controllers
{
    public class FishController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(string searchQuery, int page = 1, int pageSize = 5)
        {
            var query = db.Fishs.AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(f => f.FishName.Contains(searchQuery));
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var fishs = query
                        .OrderBy(f => f.FishName)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(fishs);
        }


        public ActionResult Create()
        {
            return View(new Fish());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FishId,FishName,UnitPrice,PictureFile")] Fish fish) 
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase file = fish.PictureFile;
                if (file != null && file.ContentLength > 0)
                {
                    string folderPath = "/Images/";
                    string fileName = DateTime.Now.Ticks + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folderPath, fileName);
                    string fullPath = Server.MapPath(filePath);

                    Directory.CreateDirectory(Server.MapPath(folderPath));
                    file.SaveAs(fullPath);
                    fish.Picture = filePath;
                }
                db.Fishs.Add(fish);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fish);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Fish fish = db.Fishs.Find(id);
            if (fish == null)
            {
                return HttpNotFound();
            }
            return View(fish);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FishId,FishName,UnitPrice,Picture")] Fish fish, HttpPostedFileBase PictureFile) 
        {
            if (ModelState.IsValid)
            {
                if (PictureFile != null && PictureFile.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(fish.Picture) && Server.MapPath(fish.Picture) != null)
                    {
                        try
                        {
                            System.IO.File.Delete(Server.MapPath(fish.Picture));
                        }
                        catch (Exception) { }
                    }
                    string folderPath = "/Images/";
                    string fileName = DateTime.Now.Ticks + Path.GetExtension(PictureFile.FileName);
                    string filePath = Path.Combine(folderPath, fileName);
                    string fullPath = Server.MapPath(filePath);

                    Directory.CreateDirectory(Server.MapPath(folderPath));
                    PictureFile.SaveAs(fullPath);
                    fish.Picture = filePath; 
                }
                db.Entry(fish).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fish);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Fish fish = db.Fishs.Find(id);
            if (fish == null)
            {
                return HttpNotFound();
            }
            return View(fish);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Fish fish = db.Fishs.Find(id);
            if (fish == null)
            {
                return HttpNotFound();
            }
            return View(fish);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Fish fish = db.Fishs.Find(id);
            if (fish != null)
            {
                var entries = db.ProductItems.Where(x => x.FishId == id).ToList();
                foreach (var entry in entries)
                {
                    db.ProductItems.Remove(entry);
                }
                if (!string.IsNullOrEmpty(fish.Picture) && Server.MapPath(fish.Picture) != null)
                {
                    try
                    {
                        System.IO.File.Delete(Server.MapPath(fish.Picture));
                    }
                    catch (Exception) { }
                }

                db.Fishs.Remove(fish);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        // ✅ OnScreen Add Fish With Ajax Call Handler
        [HttpPost]
        public async Task<ActionResult> CreateFishAjax(string fishName, decimal? unitPrice, HttpPostedFileBase pictureFile)
        {
            if (string.IsNullOrWhiteSpace(fishName))
                return Json(new { success = false, message = "Need Name।" });

            string filePath = "/Images/default.jpg"; // ডিফল্ট ছবি

            if (pictureFile != null && pictureFile.ContentLength > 0)
            {
                string fileName = DateTime.Now.Ticks + Path.GetExtension(pictureFile.FileName);
                filePath = Path.Combine("/Images/", fileName);
                string fullPath = Server.MapPath(filePath);
                pictureFile.SaveAs(fullPath);
            }

            var fish = new Fish
            {
                FishName = fishName,
                UnitPrice = (int)(unitPrice ?? 0),
                Picture = filePath
            };

            db.Fishs.Add(fish);
            await db.SaveChangesAsync();

            return Json(new { success = true, id = fish.FishId, name = fish.FishName });
        }
    }
}