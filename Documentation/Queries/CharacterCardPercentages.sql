SELECT 
	C.CharacterId,
	C.Name AS CharacterName,
	CA.CardId,
	CA.Name AS CardName,
	CA.AttackPoints,
	CA.DefensePoints,
	CPT.PercentageTypeDescription AS PercentType,
	CP.GenerationPercentage
FROM
	Character C
	INNER JOIN CardPercentage CP
		ON C.CharacterId = CP.CharacterId
	INNER JOIN C_PercentageType CPT
		ON CPT.PercentageTypeCode = CP.PercentageType
	INNER JOIN Card CA
		ON CA.CardId = CP.CardId
	