//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RadioCentroServicios.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderPurchaseDeliver
    {
        public int Id { get; set; }
        public int SequenceId { get; set; }
        public int OrderPurchaseId { get; set; }
        public int ArticleId { get; set; }
        public decimal QuantityDelivered { get; set; }
        public Nullable<System.DateTime> ReceivedDate { get; set; }
        public int ContainerId { get; set; }
        public string BL { get; set; }
    
        public virtual OrderPurchase OrderPurchase { get; set; }
    }
}
