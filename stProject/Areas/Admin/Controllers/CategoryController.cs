using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stDataAccess.Data;
using stDataAccess.Repository.IRepository;
using stModels.Models;
using stUtility;

namespace stZ.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.role_admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork uofDb;
        public CategoryController(IUnitOfWork db)
        {
            uofDb = db;
        }
        public IActionResult Index()
        {
            List<Category> categories = uofDb.Category.GetAll().ToList();
            return View(categories);
        }

        //Create Section
        public IActionResult create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The name and display order should not be the same");
            }
            if (obj.Name == null || obj.DisplayOrder.ToString() == null)
            {
                ModelState.AddModelError("", "The values must not be empty");
            }
            if (ModelState.IsValid)
            {
                uofDb.Category.Add(obj);
                uofDb.Save();
                return RedirectToAction("index", "Category");
            }
            return View();
        }

        //Edit Section
        public IActionResult edit(int id)
        {
            if (id.ToString() is null || id == 0)
            {
                return NotFound();
            }
            Category? categories = uofDb.Category.Get(u => u.Id == id);
            //  Category? categories1 = _db.Categories.FirstOrDefault(u=>u.Id == id);
            if (categories == null)
            {
                return NotFound();
            }
            return View(categories);
        }
        [HttpPost]
        public IActionResult edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The name and display order should not be the same");
            }
            if (obj.Name == null || obj.DisplayOrder.ToString() == null)
            {
                ModelState.AddModelError("", "The values must not be empty");
            }
            if (ModelState.IsValid)
            {
                uofDb.Category.Update(obj);
                uofDb.Save();
                TempData["edit"] = obj.Name + " has been edited Sucessfully";
                return RedirectToAction("index", "Category");
            }
            return View();
        }

        //Delete Section
        public IActionResult delete(int? id)
        {
            if (id.ToString() is null || id == 0)
            {
                return NotFound();
            }
            Category? categories = uofDb.Category.Get(u => u.Id == id);
            if (categories == null)
            {
                return NotFound();
            }
            return View(categories);
        }
        [HttpPost, ActionName("delete")]
        public IActionResult deletePost(int? id)
        {
            Category? obj = uofDb.Category.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            uofDb.Category.Remove(obj);
            uofDb.Save();
            TempData["delete"] = obj.Name + " has been deleted Sucessfully";

            return RedirectToAction("index", "Category");
        }
    }
}
