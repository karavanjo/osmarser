﻿USE osmarser;

DROP TABLE dbo.Geo
DROP TABLE dbo.TagsValues
DROP TABLE dbo.TagsValuesTrans
DROP TABLE dbo.Nodes
DROP TABLE dbo.Ways
DROP TABLE dbo.Relations
DROP TABLE dbo.MemberRole

ALTER DATABASE osmarser SET RECOVERY SIMPLE;
GO
DBCC SHRINKFILE(osmarser_log, 3)
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
id bigint,
lat float(24),
lon float(24),
times datetime
)
CREATE TABLE dbo.Ways
(
id bigint,
idNode bigint,
times datetime
)
CREATE TABLE dbo.Relations
(
id bigint,
ref bigint,
memberType bit,
memberRole int,
times datetime
)

CREATE TABLE dbo.MemberRole
(
id int,
memberRole varchar(max)
)