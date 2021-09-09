DELETE FROM C_CardType
GO

INSERT INTO C_CardType
(
	CardTypeCode,
	CardTypeDescription
)
VALUES
	(0, 'Unknown'),
	(1, 'Monster'),
	(2, 'Magic'),
	(3, 'Trap'),
	(4, 'Equip'),
	(5, 'Ritual')
GO

-----------------------------------------------------------------------------

DELETE FROM C_FusionType
GO

INSERT INTO C_FusionType
(
	FusionTypeCode,
	FusionTypeDescription
)
VALUES
	(0, 'Unknown'),
	(1, 'General'),
	(2, 'Specific')
GO

-----------------------------------------------------------------------------

DELETE FROM C_GuardianStar
GO

INSERT INTO C_GuardianStar
(
	GuardianStarCode,
	GuardianStarDescription
)	
VALUES
	(0, 'Unknown'),
	(1, 'Sun'),
	(2, 'Moon'),
	(3, 'Mercury'),
	(4, 'Venus'),
	(5, 'Mars'),
	(6, 'Jupiter'),
	(7, 'Saturn'),
	(8, 'Uranus'),
	(9, 'Neptune'),
	(10, 'Pluto')
GO	
	
-----------------------------------------------------------------------------	
	
DELETE FROM C_MonsterType
GO

INSERT INTO C_MonsterType
(
	MonsterTypeCode,
	MonsterTypeDescription
)	
VALUES
	(0, 'Unknown'),
	(1, 'Animal'),
	(2, 'Aqua'),
	(3, 'Beast'),
	(4, 'BeastWarrior'),
	(5, 'DarkMagic'),
	(6, 'DarkSpellcaster'),
	(7, 'Dinosaur'),
	(8, 'Dragon'),
	(9, 'Elf'),
	(10, 'Fairy'),
	(11, 'Female'),
	(12, 'Fiend'),
	(13, 'Fish'),
	(14, 'Insect'),
	(15, 'Jar'),
	(16, 'Machine'),
	(17, 'Plant'),
	(18, 'Pyro'),
	(19, 'Reptile'),
	(20, 'Rock'),
	(21, 'SeaSerpent'),
	(22, 'Spellcaster'),
	(23, 'Thunder'),
	(24, 'Turtle'),
	(25, 'Warrior'),
	(26, 'WingedBeast'),
	(27, 'Zombie'),
	(28, 'SpecialA'),
	(29, 'SpecialB'),
	(30, 'SpecialC'),
	(31, 'SpecialD')
GO
	
-----------------------------------------------------------------------------	
	
DELETE FROM C_ImageEntityType
GO

INSERT INTO C_ImageEntityType
(
	ImageEntityTypeCode,
	ImageEntityTypeDescription
)	
VALUES
	(0, 'Unknown'),
	(1, 'Card'),
	(2, 'CardDetails'),
	(3, 'Character')
GO

-----------------------------------------------------------------------------

DELETE FROM C_PercentageType
GO

INSERT INTO C_PercentageType
(
	PercentageTypeCode,
	PercentageTypeDescription
)	
VALUES
	(0, 'Unknown'),
	(1, 'DeckInclusion'),
	(2, 'SA_POW'),
	(3, 'SA_TEC'),
	(4, 'BCD_POW_TEC')
GO
	
	