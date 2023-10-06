using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using stDataAccess.Repository;
using stDataAccess.Repository.IRepository;
using stModels.Models;
using stModels.Models.ViewModel;
using stUtility;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace stZ.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.role_admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork uofDb;
        public CompanyController(IUnitOfWork db)
        {
            uofDb = db;

        }

        public IActionResult Index()
        {
            List<Company> Companys = uofDb.Company.GetAll().ToList();

            return View(Companys);
        }

        //Upsert Section
        public IActionResult Upsert(int? id)
        {
 
            if (id == null || id == 0)
                return View(new Company());
            else
            {
                Company companyObj = uofDb.Company.Get(u => u.Id == id);
                return View(companyObj);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company obj, IFormFile? file)
        {
           
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    uofDb.Company.Add(obj);
                    TempData["edit"] = obj.Name + " has been Created Sucessfully";
                }
                else
                {
                    uofDb.Company.Update(obj);
                    TempData["edit"] = obj.Name + " has been Updated Sucessfully";
                }
                uofDb.Save();
                return RedirectToAction("Index", "Company");
            }
            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() 
        {
            List<Company> Companys = uofDb.Company.GetAll().ToList();
            return Json(new { data = Companys });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanysToBeDeleted = uofDb.Company.Get(u=>u.Id == id);
            if (CompanysToBeDeleted is null)
                return Json(new { success = false, message = "Error While Deleteing" });
            else
            {
                uofDb.Company.Remove(CompanysToBeDeleted);
                uofDb.Save();
                return Json(new { success = true, message = "Proudct has been deleted" });
            }
        }
        #endregion
    }
}
