//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OrdenCompra.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderPurchaseArticlesContainer
    {
        public int Id { get; set; }
        public int OrderPurchaseId { get; set; }
        public int ContainerId { get; set; }
        public int ArticleId { get; set; }
        public System.DateTime AddedDate { get; set; }
        public decimal QuantityRequested { get; set; }
        public decimal QuantityLeft { get; set; }
        public decimal QuantityFactory { get; set; }
        public decimal QuantityTraffic { get; set; }
        public decimal QuantityAduana { get; set; }
        public decimal Price { get; set; }
        public string MeasureUnit { get; set; }
        public Nullable<System.DateTime> ManufacturingDate { get; set; }
    
        public virtual Article Article { get; set; }
        public virtual OrderPurchase OrderPurchase { get; set; }
        public virtual OrderPurchaseContainer OrderPurchaseContainer { get; set; }
    }
}
