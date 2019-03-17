// JWorksheet_Listener - an interface for classes that want to respond to JWorksheet events

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
// JWorksheet_Listener - An interface for classes that want to respond to
//	JWorksheet events.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-06-30	J. Thomas Sapienza, RTi	Initial version.
// 2006-03-02	JTS, RTi		* All existing method names now 
//					  preceded by "worksheet".
//					* Added worksheetSelectAllRows() and
//					  worksheetDeselectAllRows().
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{
	/// <summary>
	/// This interface is for classes that want to be able to respond to JWorksheet
	/// events, such as when rows are added or removed.   This class will likely be
	/// renamed in the future.
	/// </summary>
	public interface JWorksheet_Listener
	{

	/// <summary>
	/// Called when deselectAllRows() is called on the worksheet.  This method is never
	/// called for any sort of selection done with keyboard or mouse, but <i>only</i>
	/// when the JWorksheet.deselectAllRows() method is called. <para>
	/// In order to capture all selection events, selection events will need to be
	/// bubbled up from the column and row selection models to the worksheet.
	/// </para>
	/// </summary>
	/// <param name="timeframe"> when the even occurred.  One of 
	/// JWorksheet.PRE_SELECTION_CHANGE or JWorksheet.POST_SELECTION_CHANGE. </param>
	void worksheetDeselectAllRows(int time);

	/// <summary>
	/// Called when the worksheet adds a row.  By the time this is called, the 
	/// row has already been added. </summary>
	/// <param name="rowNumber"> the number of the row to add </param>
	void worksheetRowAdded(int rowNumber);

	/// <summary>
	/// Called when the worksheet deletes a row.  By the time this is called, the 
	/// row has already been deleted. </summary>
	/// <param name="rowNumber"> the number of the deleted row. </param>
	void worksheetRowDeleted(int rowNumber);

	/// <summary>
	/// Called when selectAllRows() is called on the worksheet.  This method is never
	/// called for any sort of selection done with keyboard or mouse, but <i>only</i>
	/// when the JWorksheet.selectAllRows() method is called.  <para>
	/// In order to capture all selection events, selection events will need to be
	/// bubbled up from the column and row selection models to the worksheet.
	/// </para>
	/// </summary>
	/// <param name="timeframe"> when the even occurred.  One of 
	/// JWorksheet.PRE_SELECTION_CHANGE or JWorksheet.POST_SELECTION_CHANGE. </param>
	void worksheetSelectAllRows(int timeframe);

	/// <summary>
	/// Called when the number of rows in the worksheet changes drastically, i.e,
	/// when setData() is called. </summary>
	/// <param name="rows"> the number of rows in the worksheet. </param>
	void worksheetSetRowCount(int rows);

	}

}