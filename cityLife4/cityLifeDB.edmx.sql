
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 01/26/2019 21:38:03
-- Generated from EDMX file: C:\software\cityLife\cityLife4\cityLifeDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
--GO
USE [C:\SOFTWARE\CITYLIFEDB8.MDF];
--GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
--GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_ApartmentApartmentPhoto]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ApartmentPhotoes] DROP CONSTRAINT [FK_ApartmentApartmentPhoto];
--GO
IF OBJECT_ID(N'[dbo].[FK_ApartmentPricing]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Pricings] DROP CONSTRAINT [FK_ApartmentPricing];
--GO
IF OBJECT_ID(N'[dbo].[FK_TranslationKeyTranslatio]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Translations] DROP CONSTRAINT [FK_TranslationKeyTranslatio];
--GO
IF OBJECT_ID(N'[dbo].[FK_LanguageTranslation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Translations] DROP CONSTRAINT [FK_LanguageTranslation];
--GO
IF OBJECT_ID(N'[dbo].[FK_CurrencyPricing]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Pricings] DROP CONSTRAINT [FK_CurrencyPricing];
--GO
IF OBJECT_ID(N'[dbo].[FK_CurrencyCurrencyExchangeFrom]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CurrencyExchanges] DROP CONSTRAINT [FK_CurrencyCurrencyExchangeFrom];
--GO
IF OBJECT_ID(N'[dbo].[FK_CurrencyCurrencyExchangeTo]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CurrencyExchanges] DROP CONSTRAINT [FK_CurrencyCurrencyExchangeTo];
--GO
IF OBJECT_ID(N'[dbo].[FK_CurrencyApartmentDay]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ApartmentDays] DROP CONSTRAINT [FK_CurrencyApartmentDay];
--GO
IF OBJECT_ID(N'[dbo].[FK_ApartmentApartmentDay]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ApartmentDays] DROP CONSTRAINT [FK_ApartmentApartmentDay];
--GO
IF OBJECT_ID(N'[dbo].[FK_ErrorCodeErrorMessage]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ErrorMessages] DROP CONSTRAINT [FK_ErrorCodeErrorMessage];
--GO
IF OBJECT_ID(N'[dbo].[FK_CurrencyOrder]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_CurrencyOrder];
--GO
IF OBJECT_ID(N'[dbo].[FK_OrderApartmentDay]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ApartmentDays] DROP CONSTRAINT [FK_OrderApartmentDay];
--GO
IF OBJECT_ID(N'[dbo].[FK_ApartmentOrder]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_ApartmentOrder];
--GO
IF OBJECT_ID(N'[dbo].[FK_GuestOrder]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_GuestOrder];
--GO
IF OBJECT_ID(N'[dbo].[FK_CountryGuest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Guests] DROP CONSTRAINT [FK_CountryGuest];
--GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Apartments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Apartments];
--GO
IF OBJECT_ID(N'[dbo].[ApartmentPhotoes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ApartmentPhotoes];
--GO
IF OBJECT_ID(N'[dbo].[Pricings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Pricings];
--GO
IF OBJECT_ID(N'[dbo].[Languages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Languages];
--GO
IF OBJECT_ID(N'[dbo].[TranslationKeys]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TranslationKeys];
--GO
IF OBJECT_ID(N'[dbo].[Translations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Translations];
--GO
IF OBJECT_ID(N'[dbo].[Currencies]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Currencies];
--GO
IF OBJECT_ID(N'[dbo].[CurrencyExchanges]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CurrencyExchanges];
--GO
IF OBJECT_ID(N'[dbo].[ApartmentDays]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ApartmentDays];
--GO
IF OBJECT_ID(N'[dbo].[unitTests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[unitTests];
--GO
IF OBJECT_ID(N'[dbo].[ErrorCodes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ErrorCodes];
--GO
IF OBJECT_ID(N'[dbo].[ErrorMessages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ErrorMessages];
--GO
IF OBJECT_ID(N'[dbo].[Orders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Orders];
--GO
IF OBJECT_ID(N'[dbo].[Guests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Guests];
--GO
IF OBJECT_ID(N'[dbo].[Employees]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Employees];
--GO
IF OBJECT_ID(N'[dbo].[Countries]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Countries];
--GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Apartments'
CREATE TABLE [dbo].[Apartments] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [number] smallint  NOT NULL,
    [nameKey] nvarchar(max)  NOT NULL,
    [descriptionKey] nvarchar(max)  NOT NULL,
    [addressKey] nvarchar(max)  NOT NULL,
    [size] smallint  NOT NULL,
    [featuresKeys] nvarchar(max)  NULL
);
--GO

-- Creating table 'ApartmentPhotoes'
CREATE TABLE [dbo].[ApartmentPhotoes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [filePath] nvarchar(max)  NOT NULL,
    [type] smallint  NOT NULL,
    [orientation] smallint  NOT NULL,
    [width] int  NULL,
    [height] int  NULL,
    [sortOrder] smallint  NOT NULL,
    [thumbnailPath] nvarchar(max)  NULL,
    [forDesktop] bit  NOT NULL,
    [forMobile] bit  NOT NULL,
    [Apartment_Id] int  NOT NULL
);
--GO

-- Creating table 'Pricings'
CREATE TABLE [dbo].[Pricings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [adults] smallint  NOT NULL,
    [children] smallint  NOT NULL,
    [priceWeekDay] int  NOT NULL,
    [priceWeekEnd] int  NOT NULL,
    [Apartment_Id] int  NOT NULL,
    [Currency_currencyCode] nchar(3)  NOT NULL
);
--GO

-- Creating table 'Languages'
CREATE TABLE [dbo].[Languages] (
    [languageCode] nvarchar(10)  NOT NULL,
    [name] nvarchar(30)  NOT NULL,
    [isDefault] bit  NOT NULL
);
--GO

-- Creating table 'TranslationKeys'
CREATE TABLE [dbo].[TranslationKeys] (
    [id] int IDENTITY(1,1) NOT NULL,
    [transKey] nvarchar(max)  NOT NULL,
    [isUsed] bit  NOT NULL,
    [filePath] nvarchar(max)  NULL,
    [lineNumber] int  NULL
);
--GO

-- Creating table 'Translations'
CREATE TABLE [dbo].[Translations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [message] nvarchar(max)  NOT NULL,
    [TranslationKey_id] int  NOT NULL,
    [Language_languageCode] nvarchar(10)  NOT NULL
);
--GO

-- Creating table 'Currencies'
CREATE TABLE [dbo].[Currencies] (
    [currencyCode] nchar(3)  NOT NULL,
    [symbol] nchar(1)  NOT NULL,
    [name] nvarchar(max)  NOT NULL
);
--GO

-- Creating table 'CurrencyExchanges'
CREATE TABLE [dbo].[CurrencyExchanges] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [conversionRate] decimal(10,4)  NOT NULL,
    [date] datetime  NOT NULL,
    [FromCurrency_currencyCode] nchar(3)  NOT NULL,
    [ToCurrency_currencyCode] nchar(3)  NOT NULL
);
--GO

-- Creating table 'ApartmentDays'
CREATE TABLE [dbo].[ApartmentDays] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [status] int  NOT NULL,
    [isCleaned] bit  NOT NULL,
    [revenue] int  NOT NULL,
    [date] datetime  NOT NULL,
    [priceFactor] decimal(10,4)  NOT NULL,
    [Currency_currencyCode] nchar(3)  NULL,
    [Apartment_Id] int  NOT NULL,
    [Order_Id] int  NOT NULL
);
--GO

-- Creating table 'unitTests'
CREATE TABLE [dbo].[unitTests] (
    [series] nvarchar(64)  NOT NULL,
    [number] int  NOT NULL,
    [expectedResult] nvarchar(max)  NULL,
    [actualResult] nvarchar(max)  NULL,
    [dateLastRun] datetime  NOT NULL,
    [correctFlag] bit  NULL
);
--GO

-- Creating table 'ErrorCodes'
CREATE TABLE [dbo].[ErrorCodes] (
    [code] int  NOT NULL,
    [message] nvarchar(max)  NULL,
    [occurenceCount] int  NOT NULL,
    [lastOccurenceDate] datetime  NOT NULL
);
--GO

-- Creating table 'ErrorMessages'
CREATE TABLE [dbo].[ErrorMessages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [formattedMessage] nvarchar(max)  NOT NULL,
    [stackTrace] nvarchar(max)  NULL,
    [lastOccurenceDate] datetime  NOT NULL,
    [occurenceCount] int  NOT NULL,
    [ErrorCode_code] int  NOT NULL
);
--GO

-- Creating table 'Orders'
CREATE TABLE [dbo].[Orders] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [checkinDate] datetime  NOT NULL,
    [checkoutDate] datetime  NOT NULL,
    [dayCount] int  NOT NULL,
    [adultCount] int  NOT NULL,
    [childrenCount] int  NOT NULL,
    [expectedArrival] nvarchar(max)  NULL,
    [specialRequest] nvarchar(max)  NULL,
    [status] int  NOT NULL,
    [confirmationNumber] nvarchar(max)  NOT NULL,
    [cancellationNumber] nvarchar(max)  NULL,
    [price] int  NOT NULL,
    [amountPaid] int  NOT NULL,
    [bookedBy] nvarchar(max)  NULL,
    [Currency_currencyCode] nchar(3)  NOT NULL,
    [Apartment_Id] int  NOT NULL,
    [Guest_Id] int  NOT NULL
);
--GO

-- Creating table 'Guests'
CREATE TABLE [dbo].[Guests] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(max)  NOT NULL,
    [phone] nvarchar(max)  NOT NULL,
    [email] nvarchar(max)  NOT NULL,
    [Country_code] nchar(3)  NOT NULL
);
--GO

-- Creating table 'Employees'
CREATE TABLE [dbo].[Employees] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(max)  NOT NULL,
    [phone] nvarchar(max)  NOT NULL,
    [role] nvarchar(max)  NULL,
    [passwordHash] nvarchar(max)  NOT NULL,
    [email] nvarchar(max)  NOT NULL,
    [userName] nvarchar(max)  NOT NULL
);
--GO

-- Creating table 'Countries'
CREATE TABLE [dbo].[Countries] (
    [code] nchar(3)  NOT NULL,
    [name] nvarchar(max)  NOT NULL
);
--GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Apartments'
ALTER TABLE [dbo].[Apartments]
ADD CONSTRAINT [PK_Apartments]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [Id] in table 'ApartmentPhotoes'
ALTER TABLE [dbo].[ApartmentPhotoes]
ADD CONSTRAINT [PK_ApartmentPhotoes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [Id] in table 'Pricings'
ALTER TABLE [dbo].[Pricings]
ADD CONSTRAINT [PK_Pricings]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [languageCode] in table 'Languages'
ALTER TABLE [dbo].[Languages]
ADD CONSTRAINT [PK_Languages]
    PRIMARY KEY CLUSTERED ([languageCode] ASC);
--GO

-- Creating primary key on [id] in table 'TranslationKeys'
ALTER TABLE [dbo].[TranslationKeys]
ADD CONSTRAINT [PK_TranslationKeys]
    PRIMARY KEY CLUSTERED ([id] ASC);
--GO

-- Creating primary key on [Id] in table 'Translations'
ALTER TABLE [dbo].[Translations]
ADD CONSTRAINT [PK_Translations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [currencyCode] in table 'Currencies'
ALTER TABLE [dbo].[Currencies]
ADD CONSTRAINT [PK_Currencies]
    PRIMARY KEY CLUSTERED ([currencyCode] ASC);
--GO

-- Creating primary key on [Id] in table 'CurrencyExchanges'
ALTER TABLE [dbo].[CurrencyExchanges]
ADD CONSTRAINT [PK_CurrencyExchanges]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [Id] in table 'ApartmentDays'
ALTER TABLE [dbo].[ApartmentDays]
ADD CONSTRAINT [PK_ApartmentDays]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [series], [number] in table 'unitTests'
ALTER TABLE [dbo].[unitTests]
ADD CONSTRAINT [PK_unitTests]
    PRIMARY KEY CLUSTERED ([series], [number] ASC);
--GO

-- Creating primary key on [code] in table 'ErrorCodes'
ALTER TABLE [dbo].[ErrorCodes]
ADD CONSTRAINT [PK_ErrorCodes]
    PRIMARY KEY CLUSTERED ([code] ASC);
--GO

-- Creating primary key on [Id] in table 'ErrorMessages'
ALTER TABLE [dbo].[ErrorMessages]
ADD CONSTRAINT [PK_ErrorMessages]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [Id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [PK_Orders]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [Id] in table 'Guests'
ALTER TABLE [dbo].[Guests]
ADD CONSTRAINT [PK_Guests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [Id] in table 'Employees'
ALTER TABLE [dbo].[Employees]
ADD CONSTRAINT [PK_Employees]
    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [code] in table 'Countries'
ALTER TABLE [dbo].[Countries]
ADD CONSTRAINT [PK_Countries]
    PRIMARY KEY CLUSTERED ([code] ASC);
--GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Apartment_Id] in table 'ApartmentPhotoes'
ALTER TABLE [dbo].[ApartmentPhotoes]
ADD CONSTRAINT [FK_ApartmentApartmentPhoto]
    FOREIGN KEY ([Apartment_Id])
    REFERENCES [dbo].[Apartments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ApartmentApartmentPhoto'
CREATE INDEX [IX_FK_ApartmentApartmentPhoto]
ON [dbo].[ApartmentPhotoes]
    ([Apartment_Id]);
--GO

-- Creating foreign key on [Apartment_Id] in table 'Pricings'
ALTER TABLE [dbo].[Pricings]
ADD CONSTRAINT [FK_ApartmentPricing]
    FOREIGN KEY ([Apartment_Id])
    REFERENCES [dbo].[Apartments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ApartmentPricing'
CREATE INDEX [IX_FK_ApartmentPricing]
ON [dbo].[Pricings]
    ([Apartment_Id]);
--GO

-- Creating foreign key on [TranslationKey_id] in table 'Translations'
ALTER TABLE [dbo].[Translations]
ADD CONSTRAINT [FK_TranslationKeyTranslatio]
    FOREIGN KEY ([TranslationKey_id])
    REFERENCES [dbo].[TranslationKeys]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TranslationKeyTranslatio'
CREATE INDEX [IX_FK_TranslationKeyTranslatio]
ON [dbo].[Translations]
    ([TranslationKey_id]);
--GO

-- Creating foreign key on [Language_languageCode] in table 'Translations'
ALTER TABLE [dbo].[Translations]
ADD CONSTRAINT [FK_LanguageTranslation]
    FOREIGN KEY ([Language_languageCode])
    REFERENCES [dbo].[Languages]
        ([languageCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LanguageTranslation'
CREATE INDEX [IX_FK_LanguageTranslation]
ON [dbo].[Translations]
    ([Language_languageCode]);
--GO

-- Creating foreign key on [Currency_currencyCode] in table 'Pricings'
ALTER TABLE [dbo].[Pricings]
ADD CONSTRAINT [FK_CurrencyPricing]
    FOREIGN KEY ([Currency_currencyCode])
    REFERENCES [dbo].[Currencies]
        ([currencyCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CurrencyPricing'
CREATE INDEX [IX_FK_CurrencyPricing]
ON [dbo].[Pricings]
    ([Currency_currencyCode]);
--GO

-- Creating foreign key on [FromCurrency_currencyCode] in table 'CurrencyExchanges'
ALTER TABLE [dbo].[CurrencyExchanges]
ADD CONSTRAINT [FK_CurrencyCurrencyExchangeFrom]
    FOREIGN KEY ([FromCurrency_currencyCode])
    REFERENCES [dbo].[Currencies]
        ([currencyCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CurrencyCurrencyExchangeFrom'
CREATE INDEX [IX_FK_CurrencyCurrencyExchangeFrom]
ON [dbo].[CurrencyExchanges]
    ([FromCurrency_currencyCode]);
--GO

-- Creating foreign key on [ToCurrency_currencyCode] in table 'CurrencyExchanges'
ALTER TABLE [dbo].[CurrencyExchanges]
ADD CONSTRAINT [FK_CurrencyCurrencyExchangeTo]
    FOREIGN KEY ([ToCurrency_currencyCode])
    REFERENCES [dbo].[Currencies]
        ([currencyCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CurrencyCurrencyExchangeTo'
CREATE INDEX [IX_FK_CurrencyCurrencyExchangeTo]
ON [dbo].[CurrencyExchanges]
    ([ToCurrency_currencyCode]);
--GO

-- Creating foreign key on [Currency_currencyCode] in table 'ApartmentDays'
ALTER TABLE [dbo].[ApartmentDays]
ADD CONSTRAINT [FK_CurrencyApartmentDay]
    FOREIGN KEY ([Currency_currencyCode])
    REFERENCES [dbo].[Currencies]
        ([currencyCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CurrencyApartmentDay'
CREATE INDEX [IX_FK_CurrencyApartmentDay]
ON [dbo].[ApartmentDays]
    ([Currency_currencyCode]);
--GO

-- Creating foreign key on [Apartment_Id] in table 'ApartmentDays'
ALTER TABLE [dbo].[ApartmentDays]
ADD CONSTRAINT [FK_ApartmentApartmentDay]
    FOREIGN KEY ([Apartment_Id])
    REFERENCES [dbo].[Apartments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ApartmentApartmentDay'
CREATE INDEX [IX_FK_ApartmentApartmentDay]
ON [dbo].[ApartmentDays]
    ([Apartment_Id]);
--GO

-- Creating foreign key on [ErrorCode_code] in table 'ErrorMessages'
ALTER TABLE [dbo].[ErrorMessages]
ADD CONSTRAINT [FK_ErrorCodeErrorMessage]
    FOREIGN KEY ([ErrorCode_code])
    REFERENCES [dbo].[ErrorCodes]
        ([code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ErrorCodeErrorMessage'
CREATE INDEX [IX_FK_ErrorCodeErrorMessage]
ON [dbo].[ErrorMessages]
    ([ErrorCode_code]);
--GO

-- Creating foreign key on [Currency_currencyCode] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_CurrencyOrder]
    FOREIGN KEY ([Currency_currencyCode])
    REFERENCES [dbo].[Currencies]
        ([currencyCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CurrencyOrder'
CREATE INDEX [IX_FK_CurrencyOrder]
ON [dbo].[Orders]
    ([Currency_currencyCode]);
--GO

-- Creating foreign key on [Order_Id] in table 'ApartmentDays'
ALTER TABLE [dbo].[ApartmentDays]
ADD CONSTRAINT [FK_OrderApartmentDay]
    FOREIGN KEY ([Order_Id])
    REFERENCES [dbo].[Orders]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrderApartmentDay'
CREATE INDEX [IX_FK_OrderApartmentDay]
ON [dbo].[ApartmentDays]
    ([Order_Id]);
--GO

-- Creating foreign key on [Apartment_Id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_ApartmentOrder]
    FOREIGN KEY ([Apartment_Id])
    REFERENCES [dbo].[Apartments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ApartmentOrder'
CREATE INDEX [IX_FK_ApartmentOrder]
ON [dbo].[Orders]
    ([Apartment_Id]);
--GO

-- Creating foreign key on [Guest_Id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_GuestOrder]
    FOREIGN KEY ([Guest_Id])
    REFERENCES [dbo].[Guests]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GuestOrder'
CREATE INDEX [IX_FK_GuestOrder]
ON [dbo].[Orders]
    ([Guest_Id]);
--GO

-- Creating foreign key on [Country_code] in table 'Guests'
ALTER TABLE [dbo].[Guests]
ADD CONSTRAINT [FK_CountryGuest]
    FOREIGN KEY ([Country_code])
    REFERENCES [dbo].[Countries]
        ([code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CountryGuest'
CREATE INDEX [IX_FK_CountryGuest]
ON [dbo].[Guests]
    ([Country_code]);
--GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------