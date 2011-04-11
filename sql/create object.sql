DBCC SHRINKFILE(osmarser_log, 2)
DROP TABLE dbo.Geo
DROP TABLE dbo.TagsValues
DROP TABLE dbo.TagsValuesTrans
DROP TABLE dbo.Nodes
DROP TABLE dbo.Ways
DROP TABLE dbo.Relations
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
entity int,
osm varchar(max),
ru nvarchar(max),
be nvarchar(max),
en nvarchar(max),
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

-- Deletes tables
/*
DROP TABLE dbo.Geo
DROP TABLE dbo.TagsValues
DROP TABLE dbo.TagsValuesTrans
DROP TABLE dbo.Nodes
DROP TABLE dbo.Ways
DROP TABLE dbo.Relations
*/

-- Service log file
-- DBCC SHRINKFILE(osmarser_log, 2)
