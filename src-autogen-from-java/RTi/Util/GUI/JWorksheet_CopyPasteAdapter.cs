using System;
using System.Collections.Generic;
using System.Text;

// JWorksheet_CopyPasteAdapter - this class copies data from selected rows and
// columns into a format that can be easily pasted into Microsoft Excel

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

namespace RTi.Util.GUI
{




	using DMIUtil = RTi.DMI.DMIUtil;

	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class copies data from selected rows and columns into a format that can
	/// be easily pasted into Microsoft Excel.  That format is (for two rows of 
	/// three columns):<pre>
	/// value<b>[tab]</b>value<b>[tab]</b>value<b>[newline]</b>
	/// value<b>[tab]</b>value<b>[tab]</b>value<b>[newline]</b>
	/// </pre>
	/// It also has code for pasting values from Excel back into the JWorksheet;
	/// however that has been commented out and will need further review.
	/// 
	/// The copy and paste code right now only responds to control-C and
	/// control-insert (Copy) and control-V and shift-insert (paste) actions.
	/// </summary>
	public class JWorksheet_CopyPasteAdapter : ActionListener
	{

	/// <summary>
	/// Whether the table has data for formatting the output.
	/// </summary>
	private bool __canFormat = false;

	/// <summary>
	/// Whether copying is enabled or not.
	/// </summary>
	private bool __copyEnabled = false;

	/// <summary>
	/// Whether pasting is enabled or not.
	/// </summary>
	private bool __pasteEnabled = false;

	/// <summary>
	/// Cache of the classes for all the columns.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private Class[] __classes = null;
	private Type[] __classes = null;

	/// <summary>
	/// Reference to the clipboard.
	/// </summary>
	private Clipboard __system;

	/// <summary>
	/// The JWorksheet for which this adapter is used.
	/// </summary>
	private JWorksheet __worksheet;

	/// <summary>
	/// Strings to refer to copy and paste operations.
	/// </summary>
	private readonly string __COPY = "Copy", __COPY_HEADER = "Copy Header", __COPY_ALL = "Copy All", __COPY_ALL_HEADER = "Copy All Header", __PASTE = "Paste";

	/// <summary>
	/// Constructor. </summary>
	/// <param name="worksheet"> the JWorksheet for which to do copy and paste handling. </param>
	public JWorksheet_CopyPasteAdapter(JWorksheet worksheet)
	{
		__worksheet = worksheet;
		KeyStroke copyC = KeyStroke.getKeyStroke(KeyEvent.VK_C, ActionEvent.CTRL_MASK, false);
		__worksheet.registerKeyboardAction(this, __COPY, copyC, JComponent.WHEN_FOCUSED);

		KeyStroke copyIns = KeyStroke.getKeyStroke(KeyEvent.VK_INSERT, ActionEvent.CTRL_MASK, false);
		__worksheet.registerKeyboardAction(this, __COPY, copyIns, JComponent.WHEN_FOCUSED);

		KeyStroke pasteV = KeyStroke.getKeyStroke(KeyEvent.VK_V, ActionEvent.CTRL_MASK, false);
		__worksheet.registerKeyboardAction(this, __PASTE, pasteV, JComponent.WHEN_FOCUSED);

		KeyStroke pasteIns = KeyStroke.getKeyStroke(KeyEvent.VK_INSERT, ActionEvent.SHIFT_MASK, false);
		__worksheet.registerKeyboardAction(this, __PASTE, pasteIns, JComponent.WHEN_FOCUSED);

		__system = Toolkit.getDefaultToolkit().getSystemClipboard();
	}

	/// <summary>
	/// Copies or pastes worksheet data, depending on the action that is to be performed. </summary>
	/// <param name="e"> the event that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		if (__worksheet == null)
		{
			return;
		}

		string action = e.getActionCommand();

		JGUIUtil.setWaitCursor(__worksheet.getHourglassJFrame(), true);

		bool copyHeader = false;

		if (action.Equals(__COPY_HEADER, StringComparison.OrdinalIgnoreCase))
		{
			action = __COPY;
			copyHeader = true;
		}
		if (action.Equals(__COPY_ALL_HEADER, StringComparison.OrdinalIgnoreCase))
		{
			action = __COPY_ALL;
			copyHeader = true;
		}

		if (action.Equals(__COPY_ALL, StringComparison.OrdinalIgnoreCase) || action.Equals(__COPY_ALL_HEADER, StringComparison.OrdinalIgnoreCase))
		{
			// copy all is easier than the normal copy -- no checks need
			// to be made for contiguous rows and columns.
			StringBuilder sbf = new StringBuilder();
			int numCols = __worksheet.getColumnCount();
			int numRows = __worksheet.getRowCount();

			__classes = new Type[numCols];
			for (int i = 0; i < numCols; i++)
			{
				__classes[i] = __worksheet.getColumnClass(__worksheet.getAbsoluteColumn(i));
			}

			if (__worksheet.getCellRenderer() is JWorksheet_AbstractExcelCellRenderer)
			{
				__canFormat = true;
			}

			ProgressJDialog progressDialog = new ProgressJDialog(__worksheet.getHourglassJFrame(), "Copy progress", 0, (numRows * numCols));

			int count = 1;

			progressDialog.setVisible(true);

			__worksheet.startNewConsecutiveRead();

			if (copyHeader)
			{
				for (int j = 0; j < numCols; j++)
				{
					sbf.Append(__worksheet.getColumnName(j, true));
					if (j < numCols - 1)
					{
						sbf.Append("\t");
					}
				}
				sbf.Append("\n");
			}

			try
			{
				int[] absCols = new int[numCols];
				for (int i = 0; i < numCols; i++)
				{
					absCols[i] = __worksheet.getAbsoluteColumn(i);
				}

				for (int i = 0; i < numRows; i++)
				{
					for (int j = 0; j < numCols; j++)
					{
						progressDialog.setProgressBarValue(count++);
						sbf.Append(getValue(i, absCols[j]));
						if (j < numCols - 1)
						{
							sbf.Append("\t");
						}
					}
					sbf.Append("\n");
				}

				progressDialog.dispose();

				StringSelection stsel = new StringSelection(sbf.ToString());
				__system = Toolkit.getDefaultToolkit().getSystemClipboard();
				__system.setContents(stsel, stsel);
			}
			catch (Exception ex)
			{
				(new ResponseJDialog(__worksheet.getHourglassJFrame(), "Copy Error", "Copy Error", ResponseJDialog.OK)).response();
				Message.printWarning(2, "", ex);
			}
		}
		else if (action.Equals(__COPY, StringComparison.OrdinalIgnoreCase) && __copyEnabled)
		{
			StringBuilder sbf = new StringBuilder();
			int numCols = __worksheet.getSelectedColumnCount();
			int numRows = __worksheet.getSelectedRowCount();
			int[] selectedRows = __worksheet.getSelectedRows();
			int[] selectedCols = __worksheet.getSelectedColumns();
			int[] visibleCols = new int[selectedCols.Length];
			for (int i = 0; i < selectedCols.Length; i++)
			{
				visibleCols[i] = __worksheet.getVisibleColumn(selectedCols[i]);
			}

			if (numCols == 0 || numRows == 0)
			{
				JGUIUtil.setWaitCursor(__worksheet.getHourglassJFrame(), false);
				return;
			}

			if (numRows == 1 && numCols == 1)
			{
				// Trivial case -- this will always be a successful copy.  This case is just a placeholder.
			}
			else if (numRows == 1)
			{
				// The rows are valid; the only thing left to check is whether the columns are contiguous.
				if (!areCellsContiguous(numRows, selectedRows, numCols, visibleCols))
				{
					showCopyErrorDialog("You must select a contiguous block of columns.");
					return;
				}
			}
			else if (numCols == 1)
			{
				// The cols are valid; the only thing left to check is whether the rows are contiguous.
				if (!areCellsContiguous(numRows, selectedRows, numCols, visibleCols))
				{
					showCopyErrorDialog("You must select a contiguous block of rows.");
					return;
				}
			}
			else
			{
				// There are multiple rows selected and multiple columns selected.  Make sure both are contiguous.
				if (!areCellsContiguous(numRows, selectedRows, numCols, visibleCols))
				{
					showCopyErrorDialog("You must select a contiguous block\nof rows and columns.");
					return;
				}
			}

			int numColumns = __worksheet.getColumnCount();
			__classes = new Type[numColumns];
			for (int i = 0; i < numColumns; i++)
			{
				__classes[i] = __worksheet.getColumnClass(__worksheet.getAbsoluteColumn(i));
			}

			if (__worksheet.getCellRenderer() is JWorksheet_AbstractExcelCellRenderer)
			{
				__canFormat = true;
			}

			ProgressJDialog progressDialog = new ProgressJDialog(__worksheet.getHourglassJFrame(), "Copy progress", 0, (numRows * numCols));

			int count = 1;

			progressDialog.setVisible(true);

			__worksheet.startNewConsecutiveRead();

			if (copyHeader)
			{
				for (int j = 0; j < numCols; j++)
				{
					sbf.Append(__worksheet.getColumnName(visibleCols[j], true));
					if (j < numCols - 1)
					{
						sbf.Append("\t");
					}
				}
				sbf.Append("\n");
			}

			try
			{
				for (int i = 0; i < numRows; i++)
				{
					for (int j = 0; j < numCols; j++)
					{
					/*
						if (test) {
							Message.printStatus(1, "", ""
								+ "Copying row, col: "
								+ selectedRows[i] + ", " 
								+ selectedCols[j]);
						}
					*/
						progressDialog.setProgressBarValue(count++);
						sbf.Append(getValue(selectedRows[i],selectedCols[j]));
						if (j < numCols - 1)
						{
							sbf.Append("\t");
						}
					}
					sbf.Append("\n");
				}

				progressDialog.dispose();

				StringSelection stsel = new StringSelection(sbf.ToString());
				__system = Toolkit.getDefaultToolkit().getSystemClipboard();
				__system.setContents(stsel, stsel);
			}
			catch (Exception ex)
			{
				(new ResponseJDialog(__worksheet.getHourglassJFrame(), "Copy Error", "Copy Error", ResponseJDialog.OK)).response();
				Message.printWarning(2, "", ex);
			}
		}
		else if (action.Equals(__PASTE, StringComparison.OrdinalIgnoreCase) && __pasteEnabled)
		{
			int startRow = (__worksheet.getSelectedRows())[0];
			int startCol = (__worksheet.getSelectedColumns())[0];
			int numCols = __worksheet.getSelectedColumnCount();
			int numRows = __worksheet.getSelectedRowCount();
			int[] selectedRows = __worksheet.getSelectedRows();
			int[] selectedCols = __worksheet.getSelectedColumns();
			int[] visibleCols = new int[selectedCols.Length];
			for (int i = 0; i < selectedCols.Length; i++)
			{
				visibleCols[i] = __worksheet.getVisibleColumn(selectedCols[i]);
			}

			if (!areCellsContiguous(numRows, selectedRows, numCols, visibleCols))
			{
				new ResponseJDialog(__worksheet.getHourglassJFrame(), "Paste Error", "Must select a contiguous range of cells.", ResponseJDialog.OK);
					JGUIUtil.setWaitCursor(__worksheet.getHourglassJFrame(),false);
				return;
			}
			int totalCells = numCols * numRows;
			int relCol = 0;
			try
			{
				string trstring = (string)(__system.getContents(this).getTransferData(DataFlavor.stringFlavor));
				IList<string> v1 = StringUtil.breakStringList(trstring,"\n",0);

				int size1 = v1.Count;
				int size2 = -1;
				bool columnPasteCheck = false;
				if (size1 == 1)
				{
					columnPasteCheck = true;
				}
				string rowString = "";
				string value = "";
				for (int i = 0; i < size1; i++)
				{
					rowString = v1[i];
					if (rowString.Equals(""))
					{
						rowString = " ";
					}
					IList<string> v2 = StringUtil.breakStringList(rowString, "\t", 0);
					size2 = v2.Count;
					if (columnPasteCheck && (size2 == 1) && (totalCells > 1))
					{
						fillCells(v2[0], selectedRows, selectedCols);
					}
					columnPasteCheck = false;
					for (int j = 0; j < size2; j++)
					{
						value = v2[j];
						relCol = __worksheet.getVisibleColumn(startCol + j);
						if ((startRow + i < __worksheet.getRowCount()) && (relCol < __worksheet.getColumnCount()))
						{
							if (__worksheet.isCellEditable(startRow + i, relCol))
							{
								__worksheet.setValueAt(value, startRow + i, relCol);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				(new ResponseJDialog(__worksheet.getHourglassJFrame(), "Paste Error", "Paste Error", ResponseJDialog.OK)).response();
				Message.printWarning(2, "", ex);
			}
			JGUIUtil.forceRepaint(__worksheet);
		}
		JGUIUtil.setWaitCursor(__worksheet.getHourglassJFrame(), false);
	}

	/// <summary>
	/// Checks whether a selection of rows and columns is contiguous. </summary>
	/// <param name="numRows"> the number of rows that are selected </param>
	/// <param name="selectedRows"> an integer array containing the numbers of the rows that are selected. </param>
	/// <param name="numCols"> the number of columns that are selected. </param>
	/// <param name="selectedCols"> an integer array containing the numbers of the columns that are selected. </param>
	private bool areCellsContiguous(int numRows, int[] selectedRows, int numCols, int[] selectedCols)
	{
		// There are two assumptions made about the data passed in:
		//    1) numCols/numRows is > 1.  It should have been checked already in the calling code.
		//    2) the values in selectedCols/selectedRows are sorted from lowest (at pos 0) to highest.

		// trivial case is to make sure that the number of selected rows/columns
		// is equal to the difference between the number of the highest and lowest-selected rows/columns

		if (Message.isDebugOn)
		{
			for (int i = 0; i < selectedRows.Length; i++)
			{
				Message.printDebug(2, "", "selectedRows[" + i + "]: " + selectedRows[i]);
			}
			for (int i = 0; i < selectedCols.Length; i++)
			{
				Message.printDebug(2, "", "selectedCols[" + i + "]: " + selectedCols[i]);
			}
		}

		if ((selectedCols[selectedCols.Length - 1] - selectedCols[0]) + 1 != numCols)
		{

			if (Message.isDebugOn)
			{
				Message.printDebug(2, "", "Number of columns doesn't match column span ((" + selectedCols[selectedCols.Length - 1] + " - " + selectedCols[0] + ") + 1 != " + numCols + ")");
			}

			return false;
		}

		if (((selectedRows[selectedRows.Length - 1] - selectedRows[0]) + 1) != numRows)
		{

			if (Message.isDebugOn)
			{
				Message.printDebug(2, "", "Number of rows doesn't match row span ((" + selectedRows[selectedRows.Length - 1] + " - " + selectedRows[0] + ") + 1 != " + numRows + ")");
			}

			return false;
		}

		// Otherwise, need to scan through the block made by the 
		// top-left-most (lowest row and col) cell and the bottom-right-most (biggest row and col) cell.
		for (int i = selectedRows[0]; i <= selectedRows[numRows - 1]; i++)
		{

			if (Message.isDebugOn)
			{
				Message.printDebug(2, "", "Checking row " + i + " for unselected cells ...");
			}

			for (int j = selectedCols[0]; j <= selectedCols[numCols - 1]; j++)
			{
				if (!__worksheet.isCellSelected(i, j))
				{

					if (Message.isDebugOn)
					{
						Message.printDebug(2, "", "Cell at row: " + i + ", " + "col: " + j + " is not selected, cells are non-contiguous.");
					}

					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Copies all table cells to the clipboard.
	/// </summary>
	public virtual void copyAll()
	{
		copyAll(false);
	}

	/// <summary>
	/// Copies all table cells to the clipboard. </summary>
	/// <param name="includeHeader"> whether to include the header data for the copied cells
	/// in the first line of copied information.  </param>
	public virtual void copyAll(bool includeHeader)
	{
		if (includeHeader)
		{
			ActionEvent e = new ActionEvent(this, 0, __COPY_ALL_HEADER);
			actionPerformed(e);
		}
		else
		{
			ActionEvent e = new ActionEvent(this, 0, __COPY_ALL);
			actionPerformed(e);
		}
	}

	/// <summary>
	/// Copies the selected table cells to the clipboard.
	/// </summary>
	public virtual void copy()
	{
		copy(false);
	}

	/// <summary>
	/// Copies the selected table cells to the clipboard. </summary>
	/// <param name="includeHeader"> whether to include the header data for the copied cells
	/// in the first line of copied information.  </param>
	public virtual void copy(bool includeHeader)
	{
		if (includeHeader)
		{
			ActionEvent e = new ActionEvent(this, 0, __COPY_HEADER);
			actionPerformed(e);
		}
		else
		{
			ActionEvent e = new ActionEvent(this, 0, __COPY);
			actionPerformed(e);
		}
	}

	/// <summary>
	/// Fills a contiguous range of cells with a single value. </summary>
	/// <param name="value"> the value to fill the cells with. </param>
	/// <param name="selectedRows"> the array of the selected rows (already checked that it is contiguous). </param>
	/// <param name="selectedCols"> the array of the selected cols (already checked that is is contiguous). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void fillCells(String value, int selectedRows[], int selectedCols[]) throws Exception
	private void fillCells(string value, int[] selectedRows, int[] selectedCols)
	{
		for (int i = 0; i < selectedCols.Length; i++)
		{
			selectedCols[i] = __worksheet.getVisibleColumn(selectedCols[i]);
		}
		for (int i = 0; i < selectedRows.Length; i++)
		{
			for (int j = 0; j < selectedCols.Length; j++)
			{
				if (__worksheet.isCellEditable(selectedRows[i], selectedCols[j]))
				{
					__worksheet.setValueAt(value, selectedRows[i], selectedCols[j]);
				}
			}
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet_CopyPasteAdapter()
	{
		IOUtil.nullArray(__classes);
		__system = null;
		__worksheet = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the worksheet used with this adapter. </summary>
	/// <returns> the JWorksheet used with this adapter. </returns>
	public virtual JWorksheet getJWorksheet()
	{
		return __worksheet;
	}

	/// <summary>
	/// Pulls a value out of the worksheet from the specified cell and formats it
	/// according to the formatting instructions stored in the cell renderer.  In 
	/// addition, blank data are returned as "". </summary>
	/// <param name="row"> the row of the cell </param>
	/// <param name="absoluteCol"> the <b>absolute</b> column of the cell </param>
	/// <returns> the cell data formatted properly in string format. </returns>
	private string getValue(int row, int absoluteCol)
	{
		int visibleCol = __worksheet.getVisibleColumn(absoluteCol);
		object o = __worksheet.getConsecutiveValueAt(row, absoluteCol);

		if (!__canFormat)
		{
			if (o == null)
			{
				return "";
			}
			else
			{
				return o.ToString();
			}
		}

		string format = __worksheet.getColumnFormat(absoluteCol);

	/*
		Message.printStatus(1, "", "Class[" + visibleCol + "]: " + __classes[visibleCol]);
		Message.printStatus(1, "", "    o: " + o.getClass() + "  (" + o + ")");
		System.out.println("Class[" + visibleCol + "]: " + __classes[visibleCol]);
		System.out.println("    o: " + o.getClass() + "  (" + o + ")");
	*/

		try
		{
			if (__classes[visibleCol] == typeof(Double))
			{
				double? DD = (double?)o;
				if (DMIUtil.isMissing(DD.Value))
				{
					return "";
				}
				else
				{
					return StringUtil.formatString(DD,format);
				}
			}
			else if (__classes[visibleCol] == typeof(Float))
			{
				float? F = (float?)o;
				if (DMIUtil.isMissing(F.Value))
				{
					return "";
				}
				else
				{
					return StringUtil.formatString(F,format);
				}
			}
			else if (__classes[visibleCol] == typeof(Long))
			{
				long? L = (long?)o;
				if (DMIUtil.isMissing(L.Value))
				{
					return "";
				}
				else
				{
					return StringUtil.formatString(L,format);
				}
			}
			else if (__classes[visibleCol] == typeof(Integer))
			{
				int? I = (int?)o;
				if (DMIUtil.isMissing(I.Value))
				{
					return "";
				}
				else
				{
					return StringUtil.formatString(I,format);
				}
			}
			else
			{
				return "" + o;
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2,"JWorksheet_CopyPasteAdapter.getValue()", "Error while copying value.");
			Message.printWarning(2,"JWorksheet_CopyPasteAdapter.getValue()", "   Class[" + visibleCol + "]: " + __classes[visibleCol]);
			Message.printWarning(2,"JWorksheet_CopyPasteAdapter.getValue()", "    o: " + o.GetType() + "  (" + o + ")");
			Message.printWarning(2,"JWorksheet_CopyPasteAdapter.getValue()", e);
			return "" + o;
		}
	}

	/// <summary>
	/// Pastes the clipboard into the table.
	/// </summary>
	public virtual void paste()
	{
		ActionEvent e = new ActionEvent(this, 0, __PASTE);
		actionPerformed(e);
	}

	/// <summary>
	/// Sets whether copying is enabled. </summary>
	/// <param name="setting"> whether copying is enabled. </param>
	public virtual void setCopyEnabled(bool setting)
	{
		__copyEnabled = setting;
	}

	/// <summary>
	/// Sets whether pasting is enabled. </summary>
	/// <param name="setting"> whether pasting is enabled. </param>
	public virtual void setPasteEnabled(bool setting)
	{
		__pasteEnabled = setting;
	}

	/// <summary>
	/// Sets the JWorksheet to use with this adapter. </summary>
	/// <param name="worksheet"> the JWorksheet to use with this adapter.  If null, disables the adapter. </param>
	public virtual void setJWorksheet(JWorksheet worksheet)
	{
		__worksheet = worksheet;
	}

	/// <summary>
	/// Shows an error dialog indicating that the selected cells cannot be copied. </summary>
	/// <param name="addendum"> extra text to add on a newline after the line: "Invalid copy selection." </param>
	private void showCopyErrorDialog(string addendum)
	{
		string message = "Invalid copy selection.";
		if (!string.ReferenceEquals(addendum, null))
		{
			message += "\n" + addendum;
		}
		new ResponseJDialog(new JFrame(), "Invalid Copy Selection", message, ResponseJDialog.OK);
		JGUIUtil.setWaitCursor(__worksheet.getHourglassJFrame(), false);
	}

	}

}