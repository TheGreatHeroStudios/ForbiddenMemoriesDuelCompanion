namespace FMDC.DataLoader
{
	/// <summary>
	/// Represents the possible types of row data that can be parsed out of the SpecificFusion data file.
	/// </summary>
	public enum DataRowType
	{
		/// <summary>The row data type could not be determined from the provided data</summary>
		Unknown,
		/// <summary>The row data represents the dividing line between a target card and any fusions</summary>
		Divider,
		/// <summary>The row data represents a delimiting line, ending the fusions for one target and beginning a new target</summary>
		Delimiter,
		/// <summary>The row data represents the card that is the target of a fusion or drop</summary>
		Target,
		/// <summary>The row data represents an 'equip' fusion.  These should be ignored</summary>
		Equip,
		/// <summary>The row data represents a fusion that can be made with the preceeding target</summary>
		Fusion,
		/// <summary>The row data represents the name of the character who drops the following target card(s)</summary>
		Character
	}
}
