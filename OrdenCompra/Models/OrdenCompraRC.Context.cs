﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class OrdenCompraRCEntities : DbContext
    {
        public OrdenCompraRCEntities()
            : base("name=OrdenCompraRCEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<InventoryHistory> InventoryHistories { get; set; }
        public virtual DbSet<LoginHistory> LoginHistories { get; set; }
        public virtual DbSet<Mark> Marks { get; set; }
        public virtual DbSet<NotificationCenter> NotificationCenters { get; set; }
        public virtual DbSet<NotificationGroup> NotificationGroups { get; set; }
        public virtual DbSet<OrderPurchase> OrderPurchases { get; set; }
        public virtual DbSet<OrderPurchaseDoc> OrderPurchaseDocs { get; set; }
        public virtual DbSet<OrderPurchaseHistory> OrderPurchaseHistories { get; set; }
        public virtual DbSet<Provider> Providers { get; set; }
        public virtual DbSet<ShippingCompany> ShippingCompanies { get; set; }
        public virtual DbSet<StatusContainer> StatusContainers { get; set; }
        public virtual DbSet<StatusOrderPurchase> StatusOrderPurchases { get; set; }
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<TimeLineOrder> TimeLineOrders { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<OrderPurchaseArticlesContainer> OrderPurchaseArticlesContainers { get; set; }
        public virtual DbSet<OrderPurchaseArticlesContainerTmp> OrderPurchaseArticlesContainerTmps { get; set; }
        public virtual DbSet<OrderPurchaseDeliver> OrderPurchaseDelivers { get; set; }
        public virtual DbSet<OrderPurchaseContainer> OrderPurchaseContainers { get; set; }
    
        public virtual ObjectResult<GetPurchaseOrderContainer_Result> GetPurchaseOrderContainer(Nullable<int> containerId)
        {
            var containerIdParameter = containerId.HasValue ?
                new ObjectParameter("containerId", containerId) :
                new ObjectParameter("containerId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPurchaseOrderContainer_Result>("GetPurchaseOrderContainer", containerIdParameter);
        }
    
        public virtual ObjectResult<GetPurchaseOrderHeader_Result> GetPurchaseOrderHeader(Nullable<int> orderID)
        {
            var orderIDParameter = orderID.HasValue ?
                new ObjectParameter("orderID", orderID) :
                new ObjectParameter("orderID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPurchaseOrderHeader_Result>("GetPurchaseOrderHeader", orderIDParameter);
        }
    
        public virtual ObjectResult<GetPurchaseOrderDetail_Result> GetPurchaseOrderDetail(Nullable<int> orderID)
        {
            var orderIDParameter = orderID.HasValue ?
                new ObjectParameter("orderID", orderID) :
                new ObjectParameter("orderID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPurchaseOrderDetail_Result>("GetPurchaseOrderDetail", orderIDParameter);
        }
    
        public virtual ObjectResult<ReportAduana_Result> ReportAduana()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<ReportAduana_Result>("ReportAduana");
        }
    }
}
