DECLARE @lat varchar(max);
DECLARE @lon varchar(max);
DECLARE @lang smallint;
SET @lang = -1;

DECLARE @geo table(
    geo varchar(max),
    idGeo bigint,
    typeGeo smallint);

-- bbox	30.339963965607,53.896243716597,30.351561837387,53.899006321542
-- lat	53.897267861474
-- lon	30.348278813553

SET @lat = '53.897267861474';
SET @lon = '30.348278813553';
DECLARE @g geometry;
SET @g = geometry::STGeomFromText('POINT(' + @lat + ' ' + @lon + ' )', 4326);

DECLARE @box geometry;
SET @box = geometry::STGeomFromText('POLYGON ((53.896243716597 30.339963965607, 
53.899006321542 30.339963965607, 
53.899006321542 30.351561837387, 
53.896243716597 30.351561837387, 53.896243716597 30.339963965607))', 4326);

INSERT INTO @geo SELECT geo.STIntersection(@box).STAsText(), idGeo, typeGeo
FROM dbo.Geo geod
WHERE @g.STBuffer(0.0001).STIntersects(geod.geo) = 1

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
