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
    
    public partial class ErrorMessage
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ErrorMessage()
        {
            this.occurenceCount = 1;
        }
    
        public int Id { get; set; }
        public string formattedMessage { get; set; }
        public string stackTrace { get; set; }
        public System.DateTime lastOccurenceDate { get; set; }
        public int occurenceCount { get; set; }
    
        public virtual ErrorCode ErrorCode { get; set; }
    }
}
