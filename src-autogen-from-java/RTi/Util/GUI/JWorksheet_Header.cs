// JWorksheet_Header - the header for a JWorksheet

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

// ----------------------------------------------------------------------------
// JWorksheet_Header - The header for a JWorksheet.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-07-01	J. Thomas Sapienza, RTi	Initial version.
// 2003-11-18	JTS, RTi		Added finalize().
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using IOUtil = RTi.Util.IO.IOUtil;

	/// <summary>
	/// This class is the header that a JWorksheet uses.  It provides additional 
	/// functionality that normal headers do not, such as the ability to set tooltips
	/// on columns.
	/// </summary>
	public class JWorksheet_Header : JTableHeader
	{

	/// <summary>
	/// The number of columns in the table model.
	/// </summary>
	private int __numColumns;

	/// <summary>
	/// An array of tooltips, one per absolute column.  If a column has a 
	/// <code>null</code> tooltip, no tooltip will be shown for that column.
	/// </summary>
	private string[] __tooltips;

	/// <summary>
	/// Constructor.
	/// </summary>
	public JWorksheet_Header() : base()
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tcm"> the TableColumnModel for which this will be the header. </param>
	public JWorksheet_Header(TableColumnModel tcm) : base(tcm)
	{

		initialize();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_Header()
	{
		IOUtil.nullArray(__tooltips);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the tooltip text for a column. </summary>
	/// <param name="e"> the MouseEvent that caused a tooltip to be shown. </param>
	public virtual string getToolTipText(MouseEvent e)
	{
		int col = columnAtPoint(e.getPoint());

		if (getTable() == null)
		{
			return null;
		}

		if (col < 0 || col >= getTable().getColumnCount())
		{
			return "";
		}

		col = ((JWorksheet)getTable()).getAbsoluteColumn(col);

		string s = __tooltips[col];
		return s;
	}

	/// <summary>
	/// Initializes settings and the tooltip array.
	/// </summary>
	private void initialize()
	{
		__numColumns = getColumnModel().getColumnCount();

		__tooltips = new string[__numColumns];

		for (int i = 0; i < __numColumns; i++)
		{
			__tooltips[i] = null;
		}
	}

	/// <summary>
	/// Sets a tool tip for a column. </summary>
	/// <param name="column"> the column for which to set a tooltip. </param>
	/// <param name="tip"> the tooltip to set for the column. </param>
	public virtual void setColumnToolTip(int column, string tip)
	{
		if (column < 0 || column > __numColumns)
		{
			return;
		}

		__tooltips[column] = tip;
	}

	/// <summary>
	/// Sets the column model to use with the header. </summary>
	/// <param name="tcm"> the column model to use with the header. </param>
	public virtual void setColumnModel(TableColumnModel tcm)
	{
		base.setColumnModel(tcm);
		initialize();
	}

	}

}