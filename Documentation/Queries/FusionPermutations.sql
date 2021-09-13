SELECT 
	TC.CardId AS TargetCardId,
	TC.Name AS TargetCardName,
	C_TT.MonsterTypeDescription AS TargetMonsterType,
	FMC.CardId AS FusionMaterialCardId,
	FMC.Name AS FusionMaterialCardName,
	C_FMT.MonsterTypeDescription AS FusionMaterialMonsterType,
	RC.CardId AS ResultantCardId,
	RC.Name AS ResultantCardName
FROM 
	Fusion F
	LEFT JOIN Card TC
		ON TC.CardId = F.TargetCardId
	LEFT JOIN
	(
		SELECT 
			ST.MonsterType,
			Name
		FROM
			SecondaryType ST
			INNER JOIN Card C
				ON C.CardId = ST.CardId
		
		UNION
		
		SELECT
			MonsterType,
			Name
		FROM
			Card
	) TT
		ON TT.MonsterType = F.TargetMonsterType
	LEFT JOIN Card FMC
		ON FMC.CardId = F.FusionMaterialCardId
	LEFT JOIN
	(
		SELECT 
			ST.MonsterType,
			Name
		FROM
			SecondaryType ST
			INNER JOIN Card C
				ON C.CardId = ST.CardId
		
		UNION
		
		SELECT
			MonsterType,
			Name
		FROM
			Card
	) FMT
		ON FMT.MonsterType = F.FusionMaterialMonsterType
	LEFT JOIN C_MonsterType C_TT
		ON C_TT.MonsterTypeCode = TT.MonsterType
	LEFT JOIN C_MonsterType C_FMT
		ON C_FMT.MonsterTypeCode = FMT.MonsterType
	INNER JOIN Card RC
		ON RC.CardId = F.ResultantCardId
	