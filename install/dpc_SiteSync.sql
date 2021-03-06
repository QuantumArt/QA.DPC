/****** Object:  Table [dbo].[ProductRegions]    Script Date: 12/7/2016 8:54:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductRegions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[RegionId] [int] NOT NULL,
 CONSTRAINT [PK_ProductRegions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductRelevance]    Script Date: 12/7/2016 8:54:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductRelevance](
	[ProductID] [int] NOT NULL,
	[LastUpdateTime] [datetime] NOT NULL,
	[StatusID] [tinyint] NOT NULL,
	[IsLive] [bit] NOT NULL,
 CONSTRAINT [PK_ProductRelevance] PRIMARY KEY NONCLUSTERED 
(
	[ProductID] ASC,
	[IsLive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Products]    Script Date: 12/7/2016 8:54:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
	[Alias] [nvarchar](250) NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NOT NULL,
	[Hash] [nvarchar](2000) NOT NULL,
	[MarketingProductId] [int] NULL,
	[Title] [nvarchar](500) NULL,
	[UserUpdated] [nvarchar](50) NULL,
	[UserUpdatedId] [int] NULL,
	[ProductType] [nvarchar](250) NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RegionUpdates]    Script Date: 12/7/2016 8:54:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegionUpdates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Updated] [datetime] NOT NULL,
	[RegionId] [int] NULL,
 CONSTRAINT [PK_RegionUpdates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ProductRelevance] ADD  CONSTRAINT [DF_ProductRelevance_LastUpdateTime]  DEFAULT (getdate()) FOR [LastUpdateTime]
GO
ALTER TABLE [dbo].[ProductRelevance] ADD  DEFAULT ((1)) FOR [IsLive]
GO
ALTER TABLE [dbo].[ProductRegions]  WITH CHECK ADD  CONSTRAINT [FK_ProductRegions_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductRegions] CHECK CONSTRAINT [FK_ProductRegions_Products]
GO
/****** Object:  StoredProcedure [dbo].[ClearAll]    Script Date: 12/7/2016 8:54:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ClearAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

 delete from  ProductRelevance

  delete from  ProductRegions
   
  delete from  Products
   
END


GO
/****** Object:  StoredProcedure [dbo].[DeleteProduct]    Script Date: 12/7/2016 8:54:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DeleteProduct]
	@id int
AS
BEGIN
	delete from ProductRegions with(rowlock) where ProductId = @id
	delete from Products with(rowlock) where Id = @id
END


GO
/****** Object:  StoredProcedure [dbo].[RegionUpdated]    Script Date: 12/7/2016 8:54:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RegionUpdated] 
	-- Add the parameters for the stored procedure here
	@regionId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @hasRecord int = 0

   set @hasRecord = (select COUNT(*) from RegionUpdates
    where RegionId = @regionId)

	if(@hasRecord > 0 )

	update RegionUpdates set Updated = getdate() where RegionId = @regionId
	
	else
	 insert into RegionUpdates (RegionId, Updated) values (@regionId, getdate())

END


GO
