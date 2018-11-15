//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace cityLife
{
    using System;
    using System.Collections.Generic;
    
    public partial class Apartment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Apartment()
        {
            this.ApartmentPhotoes = new HashSet<ApartmentPhoto>();
            this.Pricings = new HashSet<Pricing>();
            this.ApartmentDays = new HashSet<ApartmentDay>();
        }
    
        public int Id { get; set; }
        public short number { get; set; }
        public string nameKey { get; set; }
        public string descriptionKey { get; set; }
        public string addressKey { get; set; }
        public short size { get; set; }
        public string featuresKeys { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApartmentPhoto> ApartmentPhotoes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pricing> Pricings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApartmentDay> ApartmentDays { get; set; }
    }
}
