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
    
    public partial class TimeLineOrder
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Source { get; set; }
        public string Comment { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual User User { get; set; }
    }
}
