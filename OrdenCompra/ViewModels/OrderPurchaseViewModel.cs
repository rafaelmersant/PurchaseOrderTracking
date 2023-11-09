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
        public List<ArticleSumarized> Articles { get; set; }
    }

    public class OrderPurchaseQueryViewModel
    {
        public List<OrderPurchase> Orders { get; set; }
        public List<ArticleSumarized> Articles { get; set; }
    }

    public class ArticleSumarized
    {
        public int Id {get; set; }
        public string Description { get; set; }
        public decimal TotalRequested { get; set; }
        public decimal TotalFactory { get; set; }
        public decimal TotalTraffic { get; set; }
        public decimal TotalReceived { get; set; }
    }
}