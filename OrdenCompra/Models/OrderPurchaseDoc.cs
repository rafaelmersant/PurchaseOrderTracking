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
    
    public partial class OrderPurchaseDoc
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public System.DateTime UploadedDate { get; set; }
        public int UploadedBy { get; set; }
        public int OrderPurchaseId { get; set; }
    }
}
