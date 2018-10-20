
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/19/2018 22:54:27
-- Generated from EDMX file: C:\software\cityLife\cityLife4\cityLifeDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [C:\SOFTWARE\CITYLIFE7.MDF];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_ApartmentApartmentPhoto]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ApartmentPhotoes] DROP CONSTRAINT [FK_ApartmentApartmentPhoto];
GO
IF OBJECT_ID(N'[dbo].[FK_ApartmentPricing]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Pricings] DROP CONSTRAINT [FK_ApartmentPricing];
GO
IF OBJECT_ID(N'[dbo].[FK_TranslationKeyTranslatio]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Translations] DROP CONSTRAINT [FK_TranslationKeyTranslatio];
GO
IF OBJECT_ID(N'[dbo].[FK_LanguageTranslation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Translations] DROP CONSTRAINT [FK_LanguageTranslation];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Apartments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Apartments];
GO
IF OBJECT_ID(N'[dbo].[ApartmentPhotoes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ApartmentPhotoes];
GO
IF OBJECT_ID(N'[dbo].[Pricings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Pricings];
GO
IF OBJECT_ID(N'[dbo].[Languages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Languages];
GO
IF OBJECT_ID(N'[dbo].[TranslationKeys]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TranslationKeys];
GO
IF OBJECT_ID(N'[dbo].[Translations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Translations];
GO

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
GO

-- Creating table 'ApartmentPhotoes'
CREATE TABLE [dbo].[ApartmentPhotoes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [filePath] nvarchar(max)  NOT NULL,
    [type] smallint  NOT NULL,
    [orientation] smallint  NOT NULL,
    [width] int  NULL,
    [height] int  NULL,
    [sortOrder] smallint  NOT NULL,
    [Apartment_Id] int  NOT NULL
);
GO

-- Creating table 'Pricings'
CREATE TABLE [dbo].[Pricings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [adults] smallint  NOT NULL,
    [children] smallint  NOT NULL,
    [priceWeekDay] int  NOT NULL,
    [priceWeekEnd] int  NOT NULL,
    [Apartment_Id] int  NOT NULL
);
GO

-- Creating table 'Languages'
CREATE TABLE [dbo].[Languages] (
    [languageCode] nchar(10)  NOT NULL,
    [name] nchar(30)  NOT NULL,
    [isDefault] bit  NOT NULL
);
GO

-- Creating table 'TranslationKeys'
CREATE TABLE [dbo].[TranslationKeys] (
    [id] int IDENTITY(1,1) NOT NULL,
    [transKey] nvarchar(max)  NOT NULL,
    [isUsed] bit  NOT NULL
);
GO

-- Creating table 'Translations'
CREATE TABLE [dbo].[Translations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [message] nvarchar(max)  NOT NULL,
    [TranslationKey_id] int  NOT NULL,
    [Language_languageCode] nchar(10)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Apartments'
ALTER TABLE [dbo].[Apartments]
ADD CONSTRAINT [PK_Apartments]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ApartmentPhotoes'
ALTER TABLE [dbo].[ApartmentPhotoes]
ADD CONSTRAINT [PK_ApartmentPhotoes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Pricings'
ALTER TABLE [dbo].[Pricings]
ADD CONSTRAINT [PK_Pricings]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [languageCode] in table 'Languages'
ALTER TABLE [dbo].[Languages]
ADD CONSTRAINT [PK_Languages]
    PRIMARY KEY CLUSTERED ([languageCode] ASC);
GO

-- Creating primary key on [id] in table 'TranslationKeys'
ALTER TABLE [dbo].[TranslationKeys]
ADD CONSTRAINT [PK_TranslationKeys]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [Id] in table 'Translations'
ALTER TABLE [dbo].[Translations]
ADD CONSTRAINT [PK_Translations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

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
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ApartmentApartmentPhoto'
CREATE INDEX [IX_FK_ApartmentApartmentPhoto]
ON [dbo].[ApartmentPhotoes]
    ([Apartment_Id]);
GO

-- Creating foreign key on [Apartment_Id] in table 'Pricings'
ALTER TABLE [dbo].[Pricings]
ADD CONSTRAINT [FK_ApartmentPricing]
    FOREIGN KEY ([Apartment_Id])
    REFERENCES [dbo].[Apartments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ApartmentPricing'
CREATE INDEX [IX_FK_ApartmentPricing]
ON [dbo].[Pricings]
    ([Apartment_Id]);
GO

-- Creating foreign key on [TranslationKey_id] in table 'Translations'
ALTER TABLE [dbo].[Translations]
ADD CONSTRAINT [FK_TranslationKeyTranslatio]
    FOREIGN KEY ([TranslationKey_id])
    REFERENCES [dbo].[TranslationKeys]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TranslationKeyTranslatio'
CREATE INDEX [IX_FK_TranslationKeyTranslatio]
ON [dbo].[Translations]
    ([TranslationKey_id]);
GO

-- Creating foreign key on [Language_languageCode] in table 'Translations'
ALTER TABLE [dbo].[Translations]
ADD CONSTRAINT [FK_LanguageTranslation]
    FOREIGN KEY ([Language_languageCode])
    REFERENCES [dbo].[Languages]
        ([languageCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LanguageTranslation'
CREATE INDEX [IX_FK_LanguageTranslation]
ON [dbo].[Translations]
    ([Language_languageCode]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------