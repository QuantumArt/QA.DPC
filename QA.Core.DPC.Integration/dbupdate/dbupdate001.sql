GO

/****** Object:  StoredProcedure [dbo].[DeleteProduct]  ******/
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