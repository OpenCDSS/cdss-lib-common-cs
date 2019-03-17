// JWorksheet_AbstractExcelCellRenderer - renderer that displays similar to Microsoft Excel

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
// JWorksheet_AbstractExcelCellRenderer - Renderer that displays things
//	in a fashion (i.e., left-justified, right-justified) similar to
//	Microsoft Excel
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-09	J. Thomas Sapienza, RTI	Initial version from 
//					HydroBase_CellRenderer_Default
// 2003-06-20	JTS, RTi		Added call to getAbsoluteColumn in
//					order to get the proper column even
//					when columns have been hidden in the
//					JWorksheet.
// 2003-10-13	JTS, RTi		Alignment can now be overridden based
//					on values set in the worksheet.
// 2004-02-03	JTS, RTi		Added support for Float.
// 2004-11-01	JTS, RTi		Added the renderBooleanAsCheckBox() 
//					method to allow booleans to come through
//					as they do in the core JTable code.
// 2005-04-26	JTS, RTi		Added finalize().
// 2005-06-02	JTS, RTi		Added checks so that NaN values will
//					be shown in the table as empty Strings.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using DMIUtil = RTi.DMI.DMIUtil;


	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is the class from which other Cell Renderers for HydroBase
	/// should be built. <para>
	/// </para>
	/// TODO (JTS - 2006-05-25)<para>
	/// If I could do this over again, I would have combined AbstractTableCellRenderer,
	/// DefaultTableCellRenderer and AbstractExcelCellRenderer into a single cell 
	/// renderer.  The reasoning for having the separation came about from the 
	/// </para>
	/// way the JWorksheet was designed originally.<para>
	/// AbstractTableCellRenderer was supposed to be The Base Class for all other 
	/// </para>
	/// renderers, providing the basic outline of what they would do.<para>
	/// DefaultTableCellRenderer was supposed to be used for worksheets that didn't
	/// </para>
	/// require any special cell formatting.<para>
	/// AbstractExcelCellRenderer was supposed to be the base class for cell renderers
	/// </para>
	/// that would do formatting of cell contents.<para>
	/// </para>
	/// In theory.<para>
	/// In practice, ALL cell renderers are doing cell formatting, so the 
	/// AbstractTableCellRenderer and DefaultTableCellRenderer are unnecessary overhead.
	/// </para>
	/// <para>
	/// </para>
	/// <b>Also</b><para>
	/// I really don't see much of a good reason to even REQUIRE cell renderers for
	/// most classes.  There are a lot of cell renderers out there that are almost 100%
	/// the same class.  At this point there's little chance of going back and 
	/// eliminating them, but if I could I would.  Use a default cell renderer for all
	/// those classes and eliminate a lot of maintenance problems.
	/// 
	/// </para>
	/// </summary>
	public abstract class JWorksheet_AbstractExcelCellRenderer : JWorksheet_DefaultTableCellRenderer
	{

	/// <summary>
	/// Whether to render a boolean value as text or as a checkbox.
	/// </summary>
	private bool __renderBooleanAsCheckBox = false;

	/// <summary>
	/// The border to use when the cell is not selected.
	/// </summary>
	protected internal static Border noFocusBorder = new EmptyBorder(1, 1, 1, 1);

	/// <summary>
	/// The colors that have been set to use as the unselected foreground and background colors.
	/// </summary>
	private Color unselectedForeground, unselectedBackground;

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_AbstractExcelCellRenderer()
	{
		unselectedForeground = null;
		unselectedBackground = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Method to return the format for a given column. </summary>
	/// <param name="column"> the column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString()) for a column. </returns>
	public abstract string getFormat(int column);

	// FIXME SAM 2008-11-10 Using DMIUtil.isMissing() may not be appropriate for some data - need to use NaN
	/// <summary>
	/// Renders a value for a cell in a JTable.  This method is called automatically
	/// by the JTable when it is rendering its cells.  This overrides some code from DefaultTableCellRenderer. </summary>
	/// <param name="table"> the JTable (in this case, JWorksheet) in which the cell to be rendered will appear. </param>
	/// <param name="value"> the cell's value to be rendered. </param>
	/// <param name="isSelected"> whether the cell is selected or not. </param>
	/// <param name="hasFocus"> whether the cell has focus or not. </param>
	/// <param name="row"> the row in which the cell appears. </param>
	/// <param name="column"> the column in which the cell appears. </param>
	/// <returns> a properly-rendered cell that can be placed in the table. </returns>
	public override Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
	{
		JWorksheet jworksheet = (JWorksheet)table;
		string str = "";
		 if (value != null)
		 {
			 // Value as string
			str = value.ToString();
		 }

		int abscolumn = jworksheet.getAbsoluteColumn(column);

		// Get the format from the cell renderer
		string format = getFormat(abscolumn);
		//Message.printStatus(2, "SAMX", "formatting " + value + " with " + format );

		int justification = SwingConstants.LEFT; // Default for strings, dates

		if (value is int?)
		{
			int? i = (int?)value;
			if ((value == null) || DMIUtil.isMissing(i.Value))
			{
				str = "";
			}
			else
			{
				justification = SwingConstants.RIGHT;
				str = StringUtil.formatString(i.Value, format);
			}
		}
		else if (value is double?)
		{
			double? d = (double?)value;
			// Display the value as a space if it is missing (typically one of the values shown)
			// The latter was added to support HEC-DSS time series database files (correspondence indicated
			// that Float.MAX_VALUE was used but it seems to be negative Float.MAX_VALUE).
			// FIXME SAM 2008-11-11 Need a way to register some type of interface method that is called when
			// checking for missing data, rather than hard-coding for all instances of worksheet here.
			if ((d == null) || double.IsNaN(d) || (d >= float.MaxValue) || (d <= -float.MaxValue) || DMIUtil.isMissing(d.Value))
			{
				str = "";
			}
			else
			{
				justification = SwingConstants.RIGHT;
				//Message.printStatus(2, "SAMX", "formatting " + d.doubleValue() + " with " + format );
				str = StringUtil.formatString(d.Value, format);
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
			str = StringUtil.formatString((string)value, format);
		}
		else if (value is float?)
		{
			float? f = (float?)value;
			if ((f == null) || float.IsNaN(f.Value) || DMIUtil.isMissing(f.Value))
			{
				str = "";
			}
			else
			{
				justification = SwingConstants.RIGHT;
				str = StringUtil.formatString(f.Value, format);
			}
		}
		else if (value is long?)
		{
			long? l = (long?)value;
			if ((value == null) || DMIUtil.isMissing(l.Value))
			{
				str = "";
			}
			else
			{
				justification = SwingConstants.RIGHT;
				str = StringUtil.formatString(l.Value, format);
			}
		}
		else if (value is bool? && __renderBooleanAsCheckBox)
		{
			JCheckBox component = new JCheckBox((string)null, ((bool?)value).Value);
			setProperColors(component, table, isSelected, hasFocus, row, column);
		}
		else
		{
			justification = SwingConstants.LEFT;
		}

		str = str.Trim();

		// call DefaultTableCellRenderer's version of this method so that
		// all the cell highlighting is handled properly.
		base.getTableCellRendererComponent(table, str, isSelected, hasFocus, row, column);

		// TODO SAM 2010-03-12 Seems to not do anything...
		int tableAlignment = jworksheet.getColumnAlignment(abscolumn);
		if (tableAlignment != JWorksheet.DEFAULT)
		{
			justification = tableAlignment;
		}

		setHorizontalAlignment(justification);
		setFont(jworksheet.getCellFont());

		return this;
	}

	/// <summary>
	/// Sets the color to use as the unselected background color. </summary>
	/// <param name="c"> the Color to use as the unselected background color. </param>
	public virtual void setBackground(Color c)
	{
		base.setBackground(c);
		unselectedBackground = c;
	}

	/// <summary>
	/// Sets the color to use as the unselected foreground color. </summary>
	/// <param name="c"> the Color to use as the unselected foreground color. </param>
	public virtual void setForeground(Color c)
	{
		base.setForeground(c);
		unselectedForeground = c;
	}


	/// <summary>
	/// Sets whether to render booleans as text (false) or checkboxes (true). </summary>
	/// <param name="renderAsCheckBox"> if true, booleans are not rendered as text in a cell but as a checkbox. </param>
	public virtual void setRenderBooleanAsCheckBox(bool renderBooleanAsCheckBox)
	{
		__renderBooleanAsCheckBox = renderBooleanAsCheckBox;
	}

	/// <summary>
	/// Sets the colors for the rendered cell properly.  From the original Java code.
	/// </summary>
	public virtual void setProperColors(JComponent component, JTable table, bool isSelected, bool hasFocus, int row, int column)
	{
		if (isSelected)
		{
			component.setForeground(table.getSelectionForeground());
			component.setBackground(table.getSelectionBackground());
		}
		else
		{
			component.setForeground((unselectedForeground != null) ? unselectedForeground : table.getForeground());
			component.setBackground((unselectedBackground != null) ? unselectedBackground : table.getBackground());
		}
		setFont(table.getFont());

		if (hasFocus)
		{
			component.setBorder(UIManager.getBorder("Table.focusCellHighlightBorder"));
			if (table.isCellEditable(row, column))
			{
				component.setForeground(UIManager.getColor("Table.focusCellForeground"));
				component.setBackground(UIManager.getColor("Table.focusCellBackground"));
			}
		}
		else
		{
			component.setBorder(noFocusBorder);
		}
	}

	}

}