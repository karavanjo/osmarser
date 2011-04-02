-- Script creates the tables needed to store geographic features and tag values

-- Create tables to store data in a geographical form
CREATE TABLE dbo.Geo
(
idGeo bigint,
geo geography
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
lat decimal(9,7),
lot decimal(9,7)
)
CREATE TABLE dbo.Ways
(
id bigint,
idNode bigint
)
CREATE TABLE dbo.Relations
(
id bigint,
idGeo bigint,
nodeOrWay bit
)