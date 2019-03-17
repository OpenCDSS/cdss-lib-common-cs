// JWorksheet_AbstractTableModel - abstract table model that will be used in a JWorksheet

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
// JWorksheet - Class from which all the Table Models 
// 	that will be used in a JWorksheet and which will have individual 
//	data objects in each row should be built.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-03-06	J. Thomas Sapienza, RTi	Initial version.
// 2003-03-20	JTS, RTi		Renamed to JWorksheet_RowTableModel and
//					now extends 
//					JWorksheet_AbstractTableModel
// 2003-05-20	JTS, RTi		Added code so that getRowData returns
//					the proper row of data even if the 
//					data is sorted.
// 2003-07-23	JTS, RTi		Renamed to 
//					JWorksheet_AbstractRowTableModel
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{
	/// <summary>
	/// This is the class from which all the classes that will be used as RowTableModels
	/// in a JWorksheet, and which will have individual data objects in each row should
	/// be built.It implements a method to return the data stored at a given row.
	/// <P>
	/// TODO (JTS - 2006-05-25)
	/// If I could do this over, I would combine this table model with 
	/// AbstractTableModel, in order to simplify things.  I don't see a very good reason
	/// to require both of these, honestly.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public abstract class JWorksheet_AbstractRowTableModel<T> extends JWorksheet_AbstractTableModel<T>
	public abstract class JWorksheet_AbstractRowTableModel<T> : JWorksheet_AbstractTableModel<T>
	{

	/// <summary>
	/// Returns the Object stored in the Table Model data at the given position, or
	/// null if the given row is out of the range of the rows. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <returns> the Object stored in the _data Vector at the given position. </returns>
	public virtual T getRowData(int row)
	{
		if (row > _rows || row < 0)
		{
			return default(T);
		}

		if (_sortOrder == null)
		{
			return _data[row];
		}
		else
		{
			int realRow = _sortOrder[row];
			return _data[realRow];
		}
	}

	}

}