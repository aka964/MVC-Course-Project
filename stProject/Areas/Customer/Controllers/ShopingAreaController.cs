using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using stDataAccess.Repository.IRepository;
using stModels.Models;
using stModels.Models.ViewModel;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using Stripe.FinancialConnections;
using stUtility;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using Session = Stripe.Checkout.Session;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;

namespace stZ.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShopingAreaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        ShopingAreaVM shopingAreaVM { get; set; }
        public ShopingAreaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shopingAreaVM = new()
            {
                ShopingAreaList = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                orderHeader = new()

            };
            foreach(var cart in shopingAreaVM.ShopingAreaList)
            {
                cart.Price = getPriceBaseOnQuantity(cart);
                shopingAreaVM.orderHeader.OrderTotal += (cart.Price * cart.Count);
            }

			return View(shopingAreaVM);
        }

        public IActionResult plus(int countId) 
        {
            var cartFromDb = _unitOfWork.ShopingCart.Get(u => u.Id == countId);
            cartFromDb.Count++;
            _unitOfWork.ShopingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int countId)
        {
            var cartFromDb = _unitOfWork.ShopingCart.Get(u => u.Id == countId);
            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShopingCart.Remove(cartFromDb);
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.sessionCart, _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            else
                cartFromDb.Count--;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult remove(int countId)
        {
            var cartFromDb = _unitOfWork.ShopingCart.Get(u => u.Id == countId);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            _unitOfWork.ShopingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.sessionCart, _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary() 
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shopingAreaVM = new()
            {
                ShopingAreaList = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                orderHeader = new()

            };
            shopingAreaVM.orderHeader.ApplicationUser = _unitOfWork.applicationUser.Get(u=>u.Id == userId);

            shopingAreaVM.orderHeader.Name = shopingAreaVM.orderHeader.ApplicationUser.Name;
            shopingAreaVM.orderHeader.StreetAddress = shopingAreaVM.orderHeader.ApplicationUser.StreetAdress;
            shopingAreaVM.orderHeader.PhoneNumber = shopingAreaVM.orderHeader.ApplicationUser.PhoneNumber;
            shopingAreaVM.orderHeader.City = shopingAreaVM.orderHeader.ApplicationUser.City;
            shopingAreaVM.orderHeader.State = shopingAreaVM.orderHeader.ApplicationUser.State;
            shopingAreaVM.orderHeader.PostalCode = shopingAreaVM.orderHeader.ApplicationUser.PostalCode;

            foreach (var cart in shopingAreaVM.ShopingAreaList)
            {
                cart.Price = getPriceBaseOnQuantity(cart);
                shopingAreaVM.orderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shopingAreaVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost(ShopingAreaVM shopingAreaVM)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shopingAreaVM.ShopingAreaList = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

			shopingAreaVM.orderHeader.OrderDate = System.DateTime.Now;
			shopingAreaVM.orderHeader.ApplicationUserId = userId;

			ApplicationUser applicationUser = _unitOfWork.applicationUser.Get(u => u.Id == userId);


			foreach (var cart in shopingAreaVM.ShopingAreaList)
			{
				cart.Price = getPriceBaseOnQuantity(cart);
				shopingAreaVM.orderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shopingAreaVM.orderHeader.PaymentStatus = SD.paymentStatusApproved;
                shopingAreaVM.orderHeader.OrderStatus = SD.statusApproved;
            }else
            {
				shopingAreaVM.orderHeader.PaymentStatus = SD.paymentStatusApprovedForDelayPayment;
				shopingAreaVM.orderHeader.OrderStatus = SD.statusApproved;
			}
            _unitOfWork.OrderHeader.Add(shopingAreaVM.orderHeader);
            _unitOfWork.Save();
            foreach (var cart in shopingAreaVM.ShopingAreaList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shopingAreaVM.orderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                var domain = Request.Scheme+ "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain+ $"customer/shopingArea/orderconfirmation/?id={shopingAreaVM.orderHeader.Id}",
                    CancelUrl = domain+"customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach(var item in shopingAreaVM.ShopingAreaList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
				Session session = service.Create(options);
                var SESSID = session.Id;
                _unitOfWork.OrderHeader.UpdateStripePaymentId(shopingAreaVM.orderHeader.Id, SESSID, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
                
			}
				return RedirectToAction(nameof(OrderConfirmation), new { shopingAreaVM.orderHeader.Id});
		}

        public IActionResult OrderConfirmation(int id)
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u=>u.Id == id);
            if (orderHeader.PaymentStatus != SD.paymentStatusApprovedForDelayPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.sessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    var SESSID = session.Id;
                    var INTID = session.PaymentIntentId;
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, SESSID, INTID);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }
            List<ShopingCart> shopingCarts = _unitOfWork.ShopingCart.GetAll(u=>u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShopingCart.RemoveRange(shopingCarts);
            _unitOfWork.Save();

            return View(id);
        }
		public double getPriceBaseOnQuantity(ShopingCart shopingCart)
        {
            if (shopingCart.Count <= 50)
                return shopingCart.Product.Price;
            else if (shopingCart.Count <= 100)
                return shopingCart.Product.Price50;
            else 
                return shopingCart.Product.Price100;
        }
    }
}
