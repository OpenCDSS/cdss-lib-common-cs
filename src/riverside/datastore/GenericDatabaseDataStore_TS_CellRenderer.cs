// GenericDatabaseDataStore_TS_CellRenderer - UI cell renderer for time series table

/* NoticeStart

CDSS Common Java Library
CDSS Common Java Library is a part of Colorado's Decision Support Systems (CDSS)
Copyright (C) 1994-2019 Colorado Department of Natural Resources

CDSS Common Java Library is free software:  you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CDSS Common Java Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CDSS Common Java Library.  If not, see <https://www.gnu.org/licenses/>.

NoticeEnd */

namespace riverside.datastore
{
	using JWorksheet_DefaultTableCellRenderer = RTi.Util.GUI.JWorksheet_DefaultTableCellRenderer;

	/// <summary>
	/// This class is used to render cells for TSTool_TS_TableModel data.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GenericDatabaseDataStore_TS_CellRenderer extends RTi.Util.GUI.JWorksheet_DefaultTableCellRenderer
	public class GenericDatabaseDataStore_TS_CellRenderer : JWorksheet_DefaultTableCellRenderer
	{

	internal GenericDatabaseDataStore_TS_TableModel __table_model = null; // Table model to render

	/// <summary>
	/// Constructor. </summary>
	/// <param name="table_model"> The TSTool_TS_TableModel to render. </param>
	public GenericDatabaseDataStore_TS_CellRenderer(GenericDatabaseDataStore_TS_TableModel table_model)
	{
		__table_model = table_model;
	}

	/// <summary>
	/// Returns the format for a given column. </summary>
	/// <param name="column"> the column for which to return the format. </param>
	/// <returns> the column format as used by StringUtil.formatString(). </returns>
	public virtual string getFormat(int column)
	{
		return __table_model.getFormat(column);
	}

	/// <summary>
	/// Returns the widths of the columns in the table. </summary>
	/// <returns> an integer array of the widths of the columns in the table. </returns>
	public override int[] getColumnWidths()
	{
		return __table_model.getColumnWidths();
	}

	}

}