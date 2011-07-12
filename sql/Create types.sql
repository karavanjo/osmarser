CREATE TYPE dbo.geo AS TABLE 
(
idGeo bigint NOT NULL,
geo varbinary(max) NOT NULL,
typeGeo smallint NOT NULL
)
GO

CREATE TYPE dbo.nodeRefsWay AS TABLE
(
nodesId bigint NOT NULL,
wayId bigint NOT NULL,
orders int NOT NULL
)
GO