CREATE TYPE dbo.geo AS TABLE 
(
idGeo bigint NOT NULL,
geo varbinary(max) NOT NULL,
typeGeo smallint NOT NULL
)
GO

CREATE TYPE dbo.nodeRefsWay AS TABLE
(
idWay bigint NOT NULL,
idNode bigint NOT NULL,
orders int NOT NULL
)
GO