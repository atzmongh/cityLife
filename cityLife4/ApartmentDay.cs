//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace cityLife4
{
    using System;
    using System.Collections.Generic;
    
    public partial class ApartmentDay
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ApartmentDay()
        {
            this.isCleaned = false;
            this.revenue = 0;
            this.priceFactor = 1m;
        }
    
        public int Id { get; set; }
        public ApartOccuStatus status { get; set; }
        public bool isCleaned { get; set; }
        public int revenue { get; set; }
        public System.DateTime date { get; set; }
        public decimal priceFactor { get; set; }
    
        public virtual Currency Currency { get; set; }
        public virtual Apartment Apartment { get; set; }
    }
}
