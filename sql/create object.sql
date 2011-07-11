USE osmarser;

DROP TABLE dbo.Geo
DROP TABLE dbo.TagsValues
DROP TABLE dbo.TagsValuesTrans
DROP TABLE dbo.WaysRefs
DROP TABLE dbo.Ways
DROP TABLE dbo.Relations
DROP TABLE dbo.MemberRoles
DROP TABLE dbo.Nodes

ALTER DATABASE osmarser SET RECOVERY SIMPLE;
GO
DBCC SHRINKFILE(osmarser_log)
DBCC SHRINKDATABASE(osmarser)
ALTER DATABASE osmarser SET RECOVERY FULL;
GO
-- Script creates the tables needed to store geographic features and tag values
-- Create tables to store data in a geographical form
CREATE TABLE dbo.Geo
(
idGeo bigint PRIMARY KEY,
geo geometry,
bin varbinary(max),
typeGeo smallint
)

-- Create tables to store data Tags and Tag values
CREATE TABLE dbo.TagsValues
(
idGeo bigint,
tag int,
vType smallint,
vHash int,
vString nvarchar(max),
vInt int
)

CREATE TABLE dbo.TagsValuesTrans
(
id int IDENTITY(1,1) PRIMARY KEY,
tagHash int,
valueHash int,
LCID smallint,
typeTrans tinyint,
tagTrans varchar(max),
valTrans varchar(max),
main bit
)

-- Create tables to store data in a structured form osm
CREATE TABLE dbo.Nodes
(
id bigint PRIMARY KEY,
lat float(24),
lon float(24),
times datetime
)

CREATE TABLE dbo.Ways
(
id bigint PRIMARY KEY,
typeGeo tinyint,
times datetime
)

CREATE TABLE dbo.WaysRefs
(
idWay bigint REFERENCES Ways(id),
idNode bigint REFERENCES Nodes(id),
orders int
)

CREATE TABLE dbo.MemberRoles
(
id int PRIMARY KEY,
memberRole varchar(max)
)

CREATE TABLE dbo.Relations
(
id bigint PRIMARY KEY,
orders smallint,
ref bigint,
memberType bit,
memberRole int REFERENCES dbo.MemberRoles(id),
times datetime
)



-- IF EXISTS(SELECT 1 FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'nodeRefsWay') AND type = (N'U'))
-- CREATE TYPE [dbo].[nodeRefsWay] AS TABLE(nodesId bigint, wayId bigint, orders int)