USE osmarser;

ALTER DATABASE osmarser SET RECOVERY BULK_LOGGED;
GO
UPDATE dbo.Geo
SET geo= geometry::STPointFromWKB(dbo.Geo.bin,
		4326)
		FROM dbo.Geo
WHERE dbo.Geo.typeGeo=0

UPDATE dbo.Geo
SET geo= geometry::STLineFromWKB(dbo.Geo.bin,
		4326)
		FROM dbo.Geo
WHERE dbo.Geo.typeGeo=2

UPDATE dbo.Geo
SET geo= geometry::STPolyFromWKB(dbo.Geo.bin,
		4326)
		FROM dbo.Geo
WHERE dbo.Geo.typeGeo=4

ALTER DATABASE osmarser SET RECOVERY FULL;
GO

CREATE SPATIAL INDEX [geo] ON [dbo].[Geo] 
(
	[geo]
)
USING  GEOMETRY_GRID 
WITH 
(
BOUNDING_BOX =(-180, -90, 180, 90), 
GRIDS =(LEVEL_1 = HIGH,LEVEL_2 = HIGH,LEVEL_3 = HIGH,LEVEL_4 = HIGH), 
CELLS_PER_OBJECT = 16, 
SORT_IN_TEMPDB = OFF, 
DROP_EXISTING = OFF, 
ALLOW_ROW_LOCKS  = ON, 
ALLOW_PAGE_LOCKS  = ON
)


