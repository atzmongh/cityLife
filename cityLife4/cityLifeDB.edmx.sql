
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/07/2018 13:15:57
-- Generated from EDMX file: C:\software\cityLife4\cityLife4\cityLifeDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
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

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------