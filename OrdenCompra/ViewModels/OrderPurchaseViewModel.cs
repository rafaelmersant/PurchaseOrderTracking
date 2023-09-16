using OrdenCompra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrdenCompra.ViewModels
{
    public class OrderPurchaseContainerViewModel
    {
        public GetPurchaseOrderContainer_Result container { get; set; }
        public List<GetPurchaseOrderDetail_Result> Details { get; set; }
    }

    public class OrderPurchaseViewModel
    {
        public GetPurchaseOrderHeader_Result Header { get; set; }
        public List<OrderPurchaseContainerViewModel> Containers { get; set; }
    }
}