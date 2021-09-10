DROP TABLE CharacterCardPercentage
GO

CREATE TABLE CharacterCardPercentage
(
	CharacterCardPercentageId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	CharacterId INTEGER NOT NULL,
	CardId INTEGER NOT NULL,
	PercentageType TINYINT NOT NULL,
	GenerationPercentage FLOAT NOT NULL,
	GenerationRatePer2048 SMALLINT NOT NULL,
	CONSTRAINT FK_CharacterCardPercentage_Character FOREIGN KEY (CharacterId) 
		REFERENCES Character (CharacterId) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_CharacterCardPercentage_Card FOREIGN KEY (CardId) 
		REFERENCES Card (CardId) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_CharacterCardPercentage_PercentageType FOREIGN KEY (PercentageType) 
		REFERENCES C_PercentageType (PercentageTypeCode) ON DELETE NO ACTION ON UPDATE NO ACTION
)