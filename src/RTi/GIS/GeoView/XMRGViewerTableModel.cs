using System;

// XMRGViewerTableModel - a table model for displaying XMRG information.

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
// XMRGViewerTableModel - a table model for displaying XMRG information.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-10-14	J. Thomas Sapienza, RTi	Initial version.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using GRLimits = RTi.GR.GRLimits;

	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	using Message = RTi.Util.Message.Message;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class XMRGViewerTableModel extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel
	public class XMRGViewerTableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// The grid from the XMRG that contains the data values.
	/// </summary>
	private GeoGrid __grid = null;

	/// <summary>
	/// Number of columns in the table model.
	/// </summary>
	private int __numColumns = 0;

	/// <summary>
	/// The layer for which data will be displayed in the table model.
	/// </summary>
	private XmrgGridLayer __xmrg = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="xmrg"> the xmrg grid layer for which to show data in the table. </param>
	/// <exception cref="Exception"> if the layer is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public XMRGViewerTableModel(RTi.GIS.GeoView.XmrgGridLayer xmrg) throws Exception
	public XMRGViewerTableModel(XmrgGridLayer xmrg)
	{
		if (xmrg == null)
		{
			throw new Exception("Null XmrgGridLayer passed to " + "XMRGViewerTableModel constructor.");
		}

		__xmrg = xmrg;
		__grid = __xmrg.getGrid();

		_rows = __grid.getNumberOfRows();
		__numColumns = __grid.getNumberOfColumns();

		//dumpXmrg(xmrg);
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given
	/// column. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		return typeof(Double);
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __numColumns;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		return "" + (__grid.getMinColumn() + columnIndex);
	}


	/// <summary>
	/// Returns the format that the specified column should be displayed in when
	/// the table is being displayed in the given table format. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString() in which to display the
	/// column. </returns>
	public override string getFormat(int column)
	{
		return "%10.3f";
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the data that should be placed in the JTable
	/// at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	public virtual object getValueAt(int row, int col)
	{
		try
		{
			return new double?(__grid.getAbsDataValue(col, (_rows - 1) - row));
		}
		catch (Exception)
		{
			return new double?(-999.99);
		}
	}

	/// <summary>
	/// Being used for testing.
	/// </summary>
	public virtual void dumpXmrg(XmrgGridLayer xmrg)
	{
		if (Message.isDebugOn)
		{
			Message.printStatus(1, "", "Dump of Xmrg file information ...");

			GRLimits limits = xmrg.getLimits();

			Message.printStatus(1, "", "Limits: " + limits);
			double max = double.Epsilon;
			int maxX = -999;
			int maxY = -999;
			double min = double.MaxValue;
			int minX = -999;
			int minY = -999;
			try
			{
				double d = 0;
				for (int i = (int)limits.getLeftX(); i < (int)limits.getRightX(); i++)
				{
					for (int j = (int)limits.getBottomY(); j < (int)limits.getTopY(); j++)
					{
						d = xmrg.getDataValue(i, j);
						if (d > max)
						{
							max = d;
							maxX = i;
							maxY = j;
						}
						if (d < min)
						{
							min = d;
							minX = i;
							minY = j;
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			Message.printStatus(1, "", "Min value: " + min + " at " + minX + ", " + minY);
			Message.printStatus(1, "", "Max value: " + max + " at " + maxX + ", " + maxY);
			Message.printStatus(1, "", "Max value header: " + xmrg.getMaxValueHeader());
			Message.printStatus(1, "", "Saved date: " + xmrg.getSavedDate());
			Message.printStatus(1, "", "Valid date: " + xmrg.getValidDate());
			Message.printStatus(1, "", "Operating system: '" + xmrg.getOperSys() + "'");
			Message.printStatus(1, "", "Big Endian? " + xmrg.isBigEndian());
			Message.printStatus(1, "", "Process flag: '" + xmrg.getProcessFlag() + "'");
			Message.printStatus(1, "", "User ID: '" + xmrg.getUserID() + "'");
			Message.printStatus(1, "", "Version: " + xmrg.getVersion());
		}
	}

	}

}