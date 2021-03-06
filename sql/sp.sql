USE [osmarser]
GO
/****** Object:  StoredProcedure [dbo].[CreateViewTagsTransOsm]    Script Date: 05/10/2011 09:42:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateViewTagsTransOsm]
AS
BEGIN

SET NOCOUNT ON;

IF OBJECT_ID ('dbo.transOsm', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.transOsm;
	EXEC ('CREATE VIEW dbo.transOsm
			AS
			SELECT id, tagHash, valueHash, LCID, typeTrans, tagTrans, valTrans, main,
			(SELECT COUNT(tv.tag) FROM dbo.TagsValues AS tv WHERE tv.tag=tagHash AND tv.vHash=valueHash) 
			AS freq
			FROM TagsValuesTrans
			WHERE LCID = -1;');
	END
END
GO

USE [osmarser]
GO
/****** Object:  StoredProcedure [dbo].[GetAllMemberRoles]    Script Date: 05/10/2011 09:43:22 ******/
CREATE PROCEDURE [dbo].[GetAllMemberRoles]
AS
BEGIN	
	SET NOCOUNT ON;
	
SELECT id, memberRole FROM dbo.MemberRole;

SELECT MAX(id) AS maximum FROM dbo.MemberRole

END

GO

USE [osmarser]
GO
/****** Object:  StoredProcedure [dbo].[GetSortedPageTrans]    Script Date: 05/10/2011 09:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSortedPageTrans](
  @SortField VARCHAR(100) = 'id',
  @PageSize INT = 25,
  @PageIndex INT = 1,
  @QueryFilter VARCHAR(100) = NULL,
 @lcid smallint
) AS
SET NOCOUNT ON

EXEC dbo.CreateViewTagsTransOsm;

DECLARE @SizeString AS VARCHAR(5)
DECLARE @PrevString AS VARCHAR(5)

DECLARE @trans table(
tagHash int,
valueHash int,
LCID smallint,
typeTrans tinyint,
tagTrans varchar(max),
valTrans varchar(max),
main bit,
freq int
);

SET @SizeString = CONVERT(VARCHAR, @PageSize)
SET @PrevString = CONVERT(VARCHAR, @PageSize * (@PageIndex - 1))

IF @QueryFilter IS NULL OR @QueryFilter = ''

BEGIN
INSERT INTO @trans 
  EXEC(
  'SELECT tagHash, valueHash, LCID, typeTrans, tagTrans, valTrans, main, freq 
  FROM dbo.transOsm ' +  ' WHERE id IN
    (SELECT TOP ' + @SizeString + ' id' + ' FROM dbo.transOsm ' + ' WHERE id NOT IN
      (SELECT TOP ' + @PrevString + ' id' + ' FROM dbo.transOsm ' + ' ORDER BY ' + @SortField + ')
    ORDER BY ' + @SortField + ')
  ORDER BY ' + @SortField
  );
  EXEC('SELECT (COUNT(id) - 1)/' + @SizeString + ' + 1 AS PageCount, COUNT(id) AS RowsCount FROM TagsValuesTrans');
END
ELSE
BEGIN
INSERT INTO @trans 
  EXEC(
  'SELECT tagHash, valueHash, LCID, typeTrans, tagTrans, valTrans, main, freq 
  FROM dbo.transOsm ' +  ' WHERE id IN
    (SELECT TOP ' + @SizeString + ' id' + ' FROM dbo.transOsm ' + ' WHERE ' + @QueryFilter + ' AND id NOT IN
      (SELECT TOP ' + @PrevString + ' id' + ' FROM dbo.transOsm WHERE ' + @QueryFilter + ' ORDER BY ' + @SortField + ')
    ORDER BY ' + @SortField + ')
  ORDER BY ' + @SortField
  )
  EXEC('SELECT (COUNT(id) - 1)/' + @SizeString + ' + 1 AS PageCount, COUNT(id) AS RowsCount FROM TagsValuesTrans' + ' WHERE ' + @QueryFilter);
END

SELECT tagHash, valueHash, LCID, typeTrans, tagTrans, valTrans, main, freq
FROM @trans

SELECT transTable.tagHash, transTable.valueHash, transTable.LCID, transTable.typeTrans, transTable.tagTrans, transTable.valTrans, transTable.main 
FROM @trans AS trans
JOIN TagsValuesTrans AS transTable
ON (trans.tagHash=transTable.tagHash
AND trans.valueHash=transTable.valueHash)
OR ((trans.tagHash=transTable.tagHash OR trans.valueHash=transTable.valueHash)
AND trans.valueHash IS NULL
AND transTable.valueHash IS NULL)
WHERE transTable.LCID = @lcid

GO

USE [osmarser]
GO
/****** Object:  StoredProcedure [dbo].[IntersectsClick]    Script Date: 05/10/2011 09:44:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jown Kovaliov>
-- Create date: <2011,04,17>
-- =============================================
CREATE PROCEDURE [dbo].[IntersectsClick]
@lat varchar(max),
@lon varchar(max),
@lang smallint,
@buffer float,
@box varbinary(max)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


DECLARE @geo table(
    geo geometry,
    idGeo bigint,
    typeGeo smallint);

DECLARE @g geometry;
SET @g = geometry::STGeomFromText('POINT(' + @lat + ' ' + @lon + ' )', 4326);

DECLARE @boxGeometry geometry;
SET @boxGeometry = geometry::STPolyFromWKB(@box, 4326);


INSERT INTO @geo SELECT geo.STIntersection(@boxGeometry), idGeo, typeGeo
FROM dbo.Geo geod
WHERE @g.STBuffer(@buffer).STIntersects(geod.geo) = 1

SELECT idGeo, Geo, typeGeo FROM @geo

DECLARE @tags table(
idGeo bigint,
tag int,
vHash int,
vInt int,
vString varchar(max),
vType smallint
);

INSERT INTO @tags
SELECT tv.idGeo, tv.tag, tv.vHash, tv.vInt, tv.vString, tv.vType
FROM @geo AS g
JOIN dbo.TagsValues AS tv
ON (g.idGeo=tv.idGeo)

SELECT idGeo,tag,vHash,vInt,vString,vType
FROM @tags

SELECT trans.tagTrans, trans.valTrans, trans.LCID, trans.typeTrans, trans.main,
		tag.tag, tag.vHash
FROM @tags AS tag
JOIN dbo.TagsValuesTrans AS trans
ON (tag.tag=trans.tagHash
AND tag.vHash=trans.valueHash)
OR ((tag.tag=trans.tagHash OR tag.vHash=trans.valueHash)
AND tag.vHash IS NULL
AND trans.valueHash IS NULL)
END


