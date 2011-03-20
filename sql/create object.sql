-- Script creates the tables needed to store geographic features and tag values

CREATE TABLE dbo.Geo
(
idGeo bigint,
geo geography,
timestampGeo bigint
)

CREATE TABLE dbo.TagsValues
(
idGeo bigint,
tag int,
vHash int,
vString varchar(max),
vInt int
)

CREATE TABLE dbo.TagsValuesTrans
(
entity int,
osm varchar(max),
ru varchar(max),
be varchar(max),
en varchar(max),
main bit
)