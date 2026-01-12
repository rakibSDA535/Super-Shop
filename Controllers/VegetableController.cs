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

namespace Khati.Controllers
{
    public class VegetableController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(string searchQuery, int page = 1, int pageSize = 5)
        {
            var query = db.Vegetables.AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(v => v.VegetableName.Contains(searchQuery));
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var vegetables = query
                            .OrderBy(v => v.VegetableName)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(vegetables);
        }
        public ActionResult Create()
        {
            return View(new Vegetable());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VegetableId,VegetableName,UnitPrice,PictureFile")] Vegetable vegetable)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase file = vegetable.PictureFile;
                if (file != null && file.ContentLength > 0)
                {
                    string folderPath = "/Images/";
                    string fileName = DateTime.Now.Ticks + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folderPath, fileName);
                    string fullPath = Server.MapPath(filePath);

                    Directory.CreateDirectory(Server.MapPath(folderPath));
                    file.SaveAs(fullPath);
                    vegetable.Picture = filePath;
                }

                db.Vegetables.Add(vegetable);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vegetable);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vegetable vegetable = db.Vegetables.Find(id);
            if (vegetable == null)
            {
                return HttpNotFound();
            }
            return View(vegetable);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VegetableId,VegetableName,UnitPrice,Picture")] Vegetable vegetable, HttpPostedFileBase PictureFile)
        {
            if (ModelState.IsValid)
            {
                if (PictureFile != null && PictureFile.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(vegetable.Picture) && Server.MapPath(vegetable.Picture) != null)
                    {
                        try
                        {
                            System.IO.File.Delete(Server.MapPath(vegetable.Picture));
                        }
                        catch (Exception) { }
                    }

                    string folderPath = "/Images/";
                    string fileName = DateTime.Now.Ticks + Path.GetExtension(PictureFile.FileName);
                    string filePath = Path.Combine(folderPath, fileName);
                    string fullPath = Server.MapPath(filePath);

                    Directory.CreateDirectory(Server.MapPath(folderPath));
                    PictureFile.SaveAs(fullPath);
                    vegetable.Picture = filePath;
                }

                db.Entry(vegetable).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vegetable);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vegetable vegetable = db.Vegetables.Find(id);
            if (vegetable == null)
            {
                return HttpNotFound();
            }
            return View(vegetable);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vegetable vegetable = db.Vegetables.Find(id);
            if (vegetable == null)
            {
                return HttpNotFound();
            }
            return View(vegetable);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vegetable vegetable = db.Vegetables.Find(id);
            if (vegetable != null)
            {
                var entries = db.ProductItems.Where(x => x.VegetableId == id).ToList();
                foreach (var entry in entries)
                {
                    db.ProductItems.Remove(entry);
                }

                if (!string.IsNullOrEmpty(vegetable.Picture) && Server.MapPath(vegetable.Picture) != null)
                {
                    try
                    {
                        System.IO.File.Delete(Server.MapPath(vegetable.Picture));
                    }
                    catch (Exception) { }
                }

                db.Vegetables.Remove(vegetable);
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
        // ✅ OnScreen Add Vegetable With Ajax Call Handler
        [HttpPost]
        public async Task<ActionResult> CreateVegetableAjax(string vegName, decimal? unitPrice, HttpPostedFileBase pictureFile)
        {
            if (string.IsNullOrWhiteSpace(vegName))
            {
                return Json(new { success = false, message = "Fixed vegetable Name।" });
            }

            var exists = await db.Vegetables.AnyAsync(v => v.VegetableName.ToLower() == vegName.ToLower());
            if (exists)
            {
                return Json(new { success = false, message = "This Picture Already Creted।" });
            }

            string filePath = "/Images/veg-default.jpg";
            if (pictureFile != null && pictureFile.ContentLength > 0)
            {
                string fileName = "veg_" + DateTime.Now.Ticks + Path.GetExtension(pictureFile.FileName);
                filePath = Path.Combine("/Images/", fileName);
                pictureFile.SaveAs(Server.MapPath(filePath));
            }

            var vegetable = new Vegetable
            {
                VegetableName = vegName,
                UnitPrice = (int)(unitPrice ?? 0),
                Picture = filePath
            };

            db.Vegetables.Add(vegetable);
            await db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = vegetable.VegetableId,
                name = vegetable.VegetableName
            });
        }
    }
}