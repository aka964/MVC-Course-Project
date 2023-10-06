using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stDataAccess.Repository.IRepository;
using stModels.Models;
using stUtility;
using System.Diagnostics;
using System.Security.Claims;

namespace stZ.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> showProducts = _unitOfWork.Product.GetAll(includeProperties: "Category");
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            if (claimsIdentity.Name != null)
            {
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                HttpContext.Session.SetInt32(SD.sessionCart, _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            return View(showProducts);
        }
        public IActionResult Details(int? productId)
        {
            ShopingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1
            };
           return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShopingCart shopingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shopingCart.ApplicationUserId = userId;
            ShopingCart shopingfromDb = _unitOfWork.ShopingCart.Get(u => u.ApplicationUserId == userId
            && u.ProductId == shopingCart.ProductId);
            if(shopingfromDb == null)
            {
                _unitOfWork.ShopingCart.Add(shopingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.sessionCart, _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            else
            {
                shopingfromDb.Count += shopingCart.Count;
                _unitOfWork.ShopingCart.Update(shopingfromDb);
                _unitOfWork.Save();
            }
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}