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
    
    public partial class ErrorCode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ErrorCode()
        {
            this.occurenceCount = 1;
            this.ErrorMessages = new HashSet<ErrorMessage>();
        }
    
        public int code { get; set; }
        public string message { get; set; }
        public int occurenceCount { get; set; }
        public System.DateTime lastOccurenceDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ErrorMessage> ErrorMessages { get; set; }
    }
}