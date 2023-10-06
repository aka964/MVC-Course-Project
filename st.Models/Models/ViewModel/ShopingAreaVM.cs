using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace stModels.Models.ViewModel
{
    public class ShopingAreaVM
    {
        public IEnumerable<ShopingCart> ShopingAreaList { get; set; }
        public OrderHeader orderHeader { get; set; }
    }
}
