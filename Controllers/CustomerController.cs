using Khati.Models;
using Khati.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Khati.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string searchQuery, int page = 1, int pageSize = 5)
        {
            var query = db.Customers
                          .Include(c => c.ProductItems.Select(pi => pi.Fish))
                          .Include(c => c.ProductItems.Select(pi => pi.Vegetable))
                          .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(c => c.CustomerName.Contains(searchQuery) ||
                                         c.Phone.ToString().Contains(searchQuery));
            }

            int totalCustomers = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalCustomers / pageSize);

            var customers = query
                            .OrderByDescending(x => x.CustomerId)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(customers);
        }


        //[Authorize]
        public ActionResult Create()
        {
            var viewModel = new CustomerVM();
            viewModel.SelectedFishIds = new List<int>();
            viewModel.SelectedVegetableIds = new List<int>();
            ViewBag.Fishs = GetFishSelectList();
            ViewBag.Vegetables = GetVegetableSelectList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerVM customerVM)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    CustomerName = customerVM.CustomerName,
                    Phone = customerVM.Phone,
                    DateOfPurchase = customerVM.DateOfPurchase,
                    Age = customerVM.Age,
                    IsMember = customerVM.IsMember,
                    TotalPrice = customerVM.TotalPrice,
                };
                HttpPostedFileBase file = customerVM.PictureFile;
                if (file != null && file.ContentLength > 0)
                {
                    string folderPath = "/Images/CustomerPictures/";
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folderPath, fileName);
                    string fullPath = Server.MapPath(filePath);
                    Directory.CreateDirectory(Server.MapPath(folderPath));
                    file.SaveAs(fullPath);
                    customer.Picture = filePath;
                }
                db.Customers.Add(customer);
                db.SaveChanges();

                if (customerVM.SelectedFishIds != null)
                {
                    foreach (var fishId in customerVM.SelectedFishIds.Where(id => id > 0))
                    {
                        var productItem = new ProductItem
                        {
                            CustomerId = customer.CustomerId,
                            FishId = fishId,
                            VegetableId = null
                        };
                        db.ProductItems.Add(productItem);
                    }
                }
                if (customerVM.SelectedVegetableIds != null)
                {
                    foreach (var vegetableId in customerVM.SelectedVegetableIds.Where(id => id > 0))
                    {
                        var productItem = new ProductItem
                        {
                            CustomerId = customer.CustomerId,
                            VegetableId = vegetableId,
                            FishId = null
                        };
                        db.ProductItems.Add(productItem);
                    }
                }
                db.SaveChanges();
                return Json(new { success = true, message = "Customer added successfully!" });
            }
            ViewBag.Fishs = GetFishSelectList(customerVM.SelectedFishIds);
            ViewBag.Vegetables = GetVegetableSelectList(customerVM.SelectedVegetableIds);
            return View(customerVM);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var customer = db.Customers
                             .Include(c => c.ProductItems)
                             .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null)
                return HttpNotFound();

            var customerVM = new CustomerVM
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                Phone = customer.Phone,
                Age = customer.Age,
                DateOfPurchase = customer.DateOfPurchase,
                Picture = customer.Picture,
                IsMember = customer.IsMember,
                TotalPrice = customer.TotalPrice,
                SelectedFishIds = customer.ProductItems
                                            .Where(pi => pi.FishId.HasValue)
                                            .Select(pi => pi.FishId.Value)
                                            .ToList(),
                SelectedVegetableIds = customer.ProductItems
                                                .Where(pi => pi.VegetableId.HasValue)
                                                .Select(pi => pi.VegetableId.Value)
                                                .ToList()
            };

            ViewBag.Fishs = GetFishSelectList(customerVM.SelectedFishIds);
            ViewBag.Vegetables = GetVegetableSelectList(customerVM.SelectedVegetableIds);

            return View(customerVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerVM customerVM)
        {
            if (ModelState.IsValid)
            {
                var customer = db.Customers.Find(customerVM.CustomerId);
                if (customer == null)
                    return Json(new { success = false, message = "Customer not found." });

                customer.CustomerName = customerVM.CustomerName;
                customer.Phone = customerVM.Phone;
                customer.Age = customerVM.Age;
                customer.DateOfPurchase = customerVM.DateOfPurchase;
                customer.IsMember = customerVM.IsMember;
                customer.TotalPrice = customerVM.TotalPrice;

                HttpPostedFileBase file = customerVM.PictureFile;
                if (file != null && file.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(customer.Picture))
                    {
                        try
                        {
                            System.IO.File.Delete(Server.MapPath(customer.Picture));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deleting old picture: {ex.Message}");
                        }
                    }
                    string folderPath = "/Images/CustomerPictures/";
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folderPath, fileName);
                    string fullPath = Server.MapPath(filePath);

                    Directory.CreateDirectory(Server.MapPath(folderPath));
                    file.SaveAs(fullPath);
                    customer.Picture = filePath;
                }

                var existingProductItems = db.ProductItems.Where(pi => pi.CustomerId == customer.CustomerId).ToList();
                db.ProductItems.RemoveRange(existingProductItems);

                if (customerVM.SelectedFishIds != null)
                {
                    foreach (var fishId in customerVM.SelectedFishIds.Where(id => id > 0))
                    {
                        var productItem = new ProductItem
                        {
                            CustomerId = customer.CustomerId,
                            FishId = fishId,
                            VegetableId = null
                        };
                        db.ProductItems.Add(productItem);
                    }
                }
                if (customerVM.SelectedVegetableIds != null)
                {
                    foreach (var vegetableId in customerVM.SelectedVegetableIds.Where(id => id > 0))
                    {
                        var productItem = new ProductItem
                        {
                            CustomerId = customer.CustomerId,
                            VegetableId = vegetableId,
                            FishId = null
                        };
                        db.ProductItems.Add(productItem);
                    }
                }

                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Customer updated successfully!" });
            }

            ViewBag.Fishs = GetFishSelectList(customerVM.SelectedFishIds);
            ViewBag.Vegetables = GetVegetableSelectList(customerVM.SelectedVegetableIds);

            return View(customerVM);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var customer = db.Customers
                             .Include(c => c.ProductItems.Select(pi => pi.Fish))
                             .Include(c => c.ProductItems.Select(pi => pi.Vegetable))
                             .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        public ActionResult Delete(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer == null)
                return HttpNotFound();
            var entriesToDelete = db.ProductItems.Where(x => x.CustomerId == id).ToList();
            foreach (var entry in entriesToDelete)
            {
                db.ProductItems.Remove(entry);
            }
            db.Customers.Remove(customer);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetFishSelectList(List<int> selectedFishIds = null)
        {
            var fishList = db.Fishs.ToList();
            if (selectedFishIds == null)
            {
                selectedFishIds = new List<int>();
            }

            return fishList.Select(f => new SelectListItem
            {
                Value = f.FishId.ToString(),
                Text = f.FishName,
                Selected = selectedFishIds.Contains(f.FishId)
            }).ToList();
        }

        private List<SelectListItem> GetVegetableSelectList(List<int> selectedVegetableIds = null)
        {
            var vegetableList = db.Vegetables.ToList();
            if (selectedVegetableIds == null)
            {
                selectedVegetableIds = new List<int>();
            }

            return vegetableList.Select(v => new SelectListItem
            {
                Value = v.VegetableId.ToString(),
                Text = v.VegetableName,
                Selected = selectedVegetableIds.Contains(v.VegetableId)
            }).ToList();
        }

        public ActionResult AddNewFishRow(int index = 0, int? selectedId = null)
        {
            var fishList = db.Fishs.ToList();
            ViewBag.Fishs = fishList.Select(f => new SelectListItem
            {
                Value = f.FishId.ToString(),
                Text = f.FishName,
                Selected = (f.FishId == selectedId)
            }).ToList();

            ViewData["Index"] = index;
            ViewData["SelectedFishId"] = selectedId;
            return PartialView("_addNewFish");
        }

        public ActionResult AddNewVegetableRow(int index = 0, int? selectedId = null)
        {
            var vegetableList = db.Vegetables.ToList();
            ViewBag.Vegetables = vegetableList.Select(v => new SelectListItem
            {
                Value = v.VegetableId.ToString(),
                Text = v.VegetableName,
                Selected = (v.VegetableId == selectedId)
            }).ToList();

            ViewData["Index"] = index;
            ViewData["SelectedVegetableId"] = selectedId;
            return PartialView("_addNewVegetable");
        }
    }
}