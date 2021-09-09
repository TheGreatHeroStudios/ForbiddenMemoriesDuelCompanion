﻿DROP TABLE GameImage
GO

CREATE TABLE GameImage
(
	EntityId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	EntityType TINYINT NOT NULL,
	ImageRelativePath NVARCHAR(255) NOT NULL,
	CONSTRAINT FK_GameImage_ImageEntityType FOREIGN KEY (EntityType) 
		REFERENCES C_ImageEntityType (ImageEntityTypeCode) ON DELETE NO ACTION ON UPDATE NO ACTION
)