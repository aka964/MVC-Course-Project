using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using stDataAccess.Repository.IRepository;
using stModels.Models;
using stModels.Models.ViewModel;
using Stripe;
using stUtility;
using System.Numerics;
using System.Security.Claims;

namespace stZ.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
	public class OrderManagementController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        private OrderVM orderVM;
        public OrderManagementController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            orderVM = new()
            {
                orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                orderDetail = _unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.role_admin+","+SD.role_emp)]
        public IActionResult UpdateOrderDetail(OrderVM orderVM)
        {
            var orderDataFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.orderHeader.Id);
            orderDataFromDb.Name = orderVM.orderHeader.Name;
            orderDataFromDb.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderDataFromDb.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderDataFromDb.State = orderVM.orderHeader.State;
            orderDataFromDb.PostalCode = orderVM.orderHeader.PostalCode;
            if(!string.IsNullOrEmpty(orderVM.orderHeader.Carrer))
                orderDataFromDb.Carrer = orderVM.orderHeader.Carrer;
            if (!string.IsNullOrEmpty(orderVM.orderHeader.TrackingNumber))
                orderDataFromDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            _unitOfWork.OrderHeader.Update(orderDataFromDb);
            _unitOfWork.Save();
            TempData["edit"] = "Order details has been updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderDataFromDb.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.role_admin + "," + SD.role_emp)]
        public ActionResult StartProccess(OrderVM orderVM) 
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.orderHeader.Id, SD.statusProccess);
            _unitOfWork.Save();
            TempData["edit"] = "Order Status has been updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.role_admin + "," + SD.role_emp)]
        public ActionResult ShippingProccess(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u=> u.Id == orderVM.orderHeader.Id);
            orderHeader.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeader.Carrer = orderVM.orderHeader.Carrer;
            orderHeader.OrderStatus = SD.statusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.OrderStatus == SD.paymentStatusApprovedForDelayPayment)
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["edit"] = "Order shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.role_admin + "," + SD.role_emp)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.orderHeader.Id);
            if(orderHeader.PaymentStatus == SD.paymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.statusCanceled, SD.statusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.statusCanceled, SD.statusCanceled);
            }
            _unitOfWork.Save();
            TempData["edit"] = "Order Refunded successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> objOrderHeader;
			
            if(User.IsInRole(SD.role_admin) || User.IsInRole(SD.role_emp))
            {
                objOrderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeader = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == userId).ToList();
            }
            switch (status)
            {
                case "pending":
                    objOrderHeader = objOrderHeader.Where(u=>u.PaymentStatus == SD.statusPending);
                    break;
                case "inproccess":
                    objOrderHeader = objOrderHeader.Where(u=>u.OrderStatus == SD.statusProccess);
                    break;
                case "complete":
                    objOrderHeader = objOrderHeader.Where(u=>u.OrderStatus == SD.statusShipped);
                    break;
                case "approved":
                    objOrderHeader = objOrderHeader.Where(u=>u.OrderStatus == SD.statusApproved);
                    break;
                default:
                    break;
            }


            return Json(new { data = objOrderHeader });
		}
		#endregion
	}
}
