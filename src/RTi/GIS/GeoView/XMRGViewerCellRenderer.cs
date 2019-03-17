// XMRGViewerCellRenderer - cell renderer for XMRGViewer tables.

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
// XMRGViewerCellRenderer - cell renderer for XMRGViewer tables.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-10-14	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-27	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{


	using DMIUtil = RTi.DMI.DMIUtil;

	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is the class from which other Cell Renderers for HydroBase
	/// should be built.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class XMRGViewerCellRenderer extends RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer
	public class XMRGViewerCellRenderer : JWorksheet_AbstractExcelCellRenderer
	{

	/// <summary>
	/// The table model for which this class will render cells.
	/// </summary>
	private XMRGViewerTableModel __tableModel = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="tableModel"> the table model for which to render cells. </param>
	public XMRGViewerCellRenderer(XMRGViewerTableModel tableModel)
	{
		__tableModel = tableModel;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~XMRGViewerCellRenderer()
	{
		__tableModel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the format for a given column. </summary>
	/// <param name="column"> the colum for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.format) for a column. </returns>
	public override string getFormat(int column)
	{
		return __tableModel.getFormat(column);
	}


	/// <summary>
	/// Renders a value for a cell in a JTable.  This method is called automatically
	/// by the JTable when it is rendering its cells.  This overrides some code from
	/// DefaultTableCellRenderer. </summary>
	/// <param name="table"> the JTable (in this case, JWorksheet) in which the cell
	/// to be rendered will appear. </param>
	/// <param name="value"> the cell's value to be rendered. </param>
	/// <param name="isSelected"> whether the cell is selected or not. </param>
	/// <param name="hasFocus"> whether the cell has focus or not. </param>
	/// <param name="row"> the row in which the cell appears. </param>
	/// <param name="column"> the column in which the cell appears. </param>
	/// <returns> a properly-rendered cell that can be placed in the table. </returns>
	public override Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
	{
		string str = "";
		 if (value != null)
		 {
			str = value.ToString();
		 }

		int abscolumn = ((JWorksheet)table).getAbsoluteColumn(column);

		string format = getFormat(abscolumn);

		int justification = SwingConstants.LEFT;

		if (value is int?)
		{
			if (DMIUtil.isMissing(((int?)value).Value))
			{
				str = "";
			}
			else
			{
				justification = SwingConstants.RIGHT;
				str = StringUtil.formatString(value, format);
			}
		}
		else if (value is double?)
		{
			if (DMIUtil.isMissing(((double?)value).Value))
			{
				str = "";
			}
			else
			{
				justification = SwingConstants.RIGHT;
				str = StringUtil.formatString(value, format);
			}
		}
		else if (value is System.DateTime)
		{
			justification = SwingConstants.LEFT;
			// FYI: str has been set above with str = value.toString()
		}
		else if (value is string)
		{
			justification = SwingConstants.LEFT;
			str = StringUtil.formatString(value, format);
		}
		else
		{
			justification = SwingConstants.LEFT;
		}

		str = str.Trim();

		// call DefaultTableCellRenderer's version of this method so that
		// all the cell highlighting is handled properly.
		base.getTableCellRendererComponent(table, str, isSelected, hasFocus, row, column);

		int tableAlignment = ((JWorksheet)table).getColumnAlignment(abscolumn);
		if (tableAlignment != JWorksheet.DEFAULT)
		{
			justification = tableAlignment;
		}

		setHorizontalAlignment(justification);
		setFont(((JWorksheet)table).getCellFont());

		return this;
	}

	}

}