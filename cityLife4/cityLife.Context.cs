﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class cityLifeDBContainer1 : DbContext
    {
        public cityLifeDBContainer1()
            : base("name=cityLifeDBContainer1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Apartment> Apartments { get; set; }
        public virtual DbSet<ApartmentPhoto> ApartmentPhotoes { get; set; }
        public virtual DbSet<Pricing> Pricings { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<TranslationKey> TranslationKeys { get; set; }
        public virtual DbSet<Translation> Translations { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<CurrencyExchange> CurrencyExchanges { get; set; }
        public virtual DbSet<ApartmentDay> ApartmentDays { get; set; }
        public virtual DbSet<unitTest> unitTests { get; set; }
    }
}
