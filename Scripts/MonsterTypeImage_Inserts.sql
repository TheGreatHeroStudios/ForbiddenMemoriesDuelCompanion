INSERT INTO
	GameImage
	(
		EntityId,
		EntityType,
		ImageRelativePath
	)
SELECT DISTINCT 
	MonsterTypeCode,
	4,
	'Images\MonsterTypes\' || MonsterTypeDescription || CAST('.png' AS VARCHAR)
FROM 
	Card C
	INNER JOIN C_MonsterType CM
		ON C.MonsterType = CM.MonsterTypeCode
ORDER BY 1
