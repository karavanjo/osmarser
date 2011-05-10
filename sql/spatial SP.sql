SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jown Kovaliov>
-- Create date: <2011,04,17>
-- =============================================
CREATE PROCEDURE dbo.IntersectsClick
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

SELECT trans.entity, trans.codeLang, trans.val,  trans.main
FROM @tags AS tag
JOIN dbo.TagsValuesTrans AS trans
ON tag.tag=trans.entity
OR tag.vHash=trans.entity
END
GO
