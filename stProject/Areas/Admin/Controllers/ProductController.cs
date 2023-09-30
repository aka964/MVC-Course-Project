using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using stDataAccess.Repository.IRepository;
using stModels.Models;
using stModels.Models.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace stZ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork uofDb;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            uofDb = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = uofDb.Product.GetAll(includeProperties:"Category").ToList();

            return View(products);
        }

        //Upsert Section
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = uofDb.Category.GetAll().Select(u => new SelectListItem
                { Text = u.Name, Value = u.Id.ToString() }),
                Product = new Product()
            };
            if(id == null || id == 0)
                return View(productVM);
            else
            {
                productVM.Product = uofDb.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file) 
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fitePath = Path.Combine(wwwRootPath, @"Images\Product");

                if(!string.IsNullOrEmpty(obj.Product.imageUrl))
                {
                    string oldImagePath = Path.Combine(wwwRootPath,obj.Product.imageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                

                using (var fileStream = new FileStream(Path.Combine(fitePath,fileName),FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                obj.Product.imageUrl = @"\Images\Product\" + fileName;
            }
            if (ModelState.IsValid)
            {
                if (obj.Product.Id == 0)
                {
                    uofDb.Product.Add(obj.Product);
                    TempData["edit"] = obj.Product.Title + " has been Created Sucessfully";
                }
                else
                {
                    uofDb.Product.Update(obj.Product);
                    TempData["edit"] = obj.Product.Title + " has been Updated Sucessfully";
                }
                uofDb.Save();
                return RedirectToAction("Index","Product");
            }
            return View();
        }


        //Delete Product

        public ActionResult Delete(int id) 
        {
            if (id.ToString() is null || id == 0) { return NotFound(); }
            Product product = uofDb.Product.Get(product => product.Id == id);
            if (product == null) { return NotFound(); }

            return View(product);
        }
        [HttpPost, ActionName("delete")]
        public IActionResult DeletePost(int id, IFormFile? file)
        {
            Product product = uofDb.Product.Get(product => product.Id == id);

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fitePath = Path.Combine(wwwRootPath, @"Images\Product");

                if (!string.IsNullOrEmpty(product.imageUrl))
                {
                    string oldImagePath = Path.Combine(wwwRootPath, product.imageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }

            if(product == null) { return NotFound(); }
            uofDb.Product.Remove(product);
            uofDb.Save();
            TempData["delete"] = product.Title + " has been Deleted Sucessfully";
            return RedirectToAction("index", "Product");
        }
    }
}
