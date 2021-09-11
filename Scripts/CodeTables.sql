CREATE TABLE C_PercentageType
(
    PercentageTypeCode TINYINT PRIMARY KEY NOT NULL,
    PercentageTypeDescription NVARCHAR(15) NOT NULL
)
GO

CREATE TABLE C_MonsterType
(
	MonsterTypeCode TINYINT PRIMARY KEY NOT NULL,
	MonsterTypeDescription NVARCHAR(20) NOT NULL
)
GO

CREATE TABLE C_ImageEntityType
(
	ImageEntityTypeCode TINYINT PRIMARY KEY NOT NULL,
	ImageEntityTypeDescription NVARCHAR(20) NOT NULL
)
GO

CREATE TABLE C_GuardianStar
(
	GuardianStarCode TINYINT PRIMARY KEY NOT NULL,
	GuardianStarDescription NVARCHAR(10) NOT NULL
)
GO

CREATE TABLE C_FusionType
(
	FusionTypeCode TINYINT PRIMARY KEY NOT NULL,
	FusionTypeDescription NVARCHAR(10) NOT NULL
)
GO

CREATE TABLE C_CardType
(
	CardTypeCode TINYINT PRIMARY KEY NOT NULL,
	CardTypeDescription NVARCHAR(10) NOT NULL
)
