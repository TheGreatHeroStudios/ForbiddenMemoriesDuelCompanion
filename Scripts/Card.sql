﻿DROP TABLE Card
GO

CREATE TABLE Card
(
	CardId INTEGER PRIMARY KEY NOT NULL,
	CardImageId INTEGER NOT NULL,
	CardDescriptionImageId INTEGER NOT NULL,
	Name NVARCHAR(50) NOT NULL,
	CardType TINYINT NOT NULL,
	MonsterType TINYINT NULL,
	Description NVARCHAR(4000) NOT NULL,
	FirstGuardianStar TINYINT NULL,
	SecondGuardianStar TINYINT NULL,
	Level TINYINT NULL,
	AttackPoints SMALLINT NULL,
	DefensePoints SMALLINT NULL,
	Password NVARCHAR(8) NULL,
	StarchipCost INT NULL,
	CONSTRAINT FK_Card_CardImage FOREIGN KEY (CardImageId) 
		REFERENCES GameImage (GameImageId) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_Card_CardDescriptionImage FOREIGN KEY (CardDescriptionImageId) 
		REFERENCES GameImage (GameImageId) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_Card_CardType FOREIGN KEY (CardType) 
		REFERENCES C_CardType (CardTypeCode) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_Card_MonsterType FOREIGN KEY (MonsterType) 
		REFERENCES C_MonsterType (MonsterTypeCode) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_Card_FirstGuardianStarType FOREIGN KEY (FirstGuardianStar) 
		REFERENCES C_GuardianStar (GuardianStarCode) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_Card_SecondGuardianStarType FOREIGN KEY (SecondGuardianStar) 
		REFERENCES C_GuardianStar (GuardianStarCode) ON DELETE NO ACTION ON UPDATE NO ACTION
)