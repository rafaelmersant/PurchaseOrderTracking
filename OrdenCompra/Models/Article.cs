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
    
    public partial class Article
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Article()
        {
            this.NotificationCenters = new HashSet<NotificationCenter>();
            this.OrderPurchaseArticlesContainers = new HashSet<OrderPurchaseArticlesContainer>();
        }
    
        public int Id { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string Model { get; set; }
        public Nullable<int> MarkId { get; set; }
        public Nullable<decimal> InventoryStock { get; set; }
        public Nullable<decimal> QuantityTraffic { get; set; }
        public Nullable<decimal> QuantityFactory { get; set; }
        public Nullable<decimal> QuantityAduana { get; set; }
        public Nullable<decimal> QuantityMaxPerContainer { get; set; }
        public Nullable<decimal> QuantityMinStock { get; set; }
        public bool Mix { get; set; }
        public System.DateTime AddedDate { get; set; }
        public int AddedBy { get; set; }
        public bool Active { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotificationCenter> NotificationCenters { get; set; }
        public virtual Mark Mark { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderPurchaseArticlesContainer> OrderPurchaseArticlesContainers { get; set; }
    }
}
