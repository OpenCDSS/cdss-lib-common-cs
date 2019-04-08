using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// JWorksheet - class that extends the Swing JTable to create a Worksheet that mimics Excel's behavior

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
	using GRColor = RTi.GR.GRColor;
	using HTMLWriter = RTi.Util.IO.HTMLWriter;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using MathUtil = RTi.Util.Math.MathUtil;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DataTable = RTi.Util.Table.DataTable;
	using DataTable_CellRenderer = RTi.Util.Table.DataTable_CellRenderer;
	using DataTable_TableModel = RTi.Util.Table.DataTable_TableModel;
	using TableField = RTi.Util.Table.TableField;
	using TableRecord = RTi.Util.Table.TableRecord;
	using StopWatch = RTi.Util.Time.StopWatch;

	// TODO (2004-01-20) need to add main javadocs for dealing with the new row headers.

	/// <summary>
	/// This class extends the Swing JTable to create a Worksheet that mimics Excel's behavior.<para>
	/// <b>Import note:</b><br>
	/// Because of their reliance on underlying JTable code, JWorksheets do <b>NOT</b>
	/// do well with multiple columns that have the exact same name.  Unexpected
	/// results can occur.  This is an issue that may be REVISITed in the future, 
	/// but in the meantime developers should not hope to have worksheets with two columns of the same name.
	/// </para>
	/// <para>
	/// <b>Important note:</b><br>
	/// In the documentation below, mention is made to <b>absolute</b> and <b>visible</b>
	/// columns.  <b>Absolute</b> column numbers never change.  The seventh
	/// <b>absolute</b> column is always the seventh <b>absolute</b> column, because
	/// </para>
	/// it is listed in the table model as being column seven.<para>
	/// <b>Visible</b> column numbers change depending on how many columns have been
	/// removed.  The seventh <b>absolute</b> column may be the third <b>visible</b>
	/// column -- but if the first <b>visible</b> column is removed, the seventh
	/// </para>
	/// <b>absolute</b> column becomes the second <b>visible</b> column.<para>
	/// For example, if a table has 5 columns, some of which are not visible, the 
	/// </para>
	/// absolute and visible column numbers are as shown:<para>
	/// <pre>
	/// [0] - visible     - 0
	/// [1] - visible     - 1
	/// [2] - not visible - n/a
	/// [3] - not visible - n/a
	/// [4] - visible     - 2
	/// </pre>
	/// The <b>absolute</b> column numbers are listed on the left-hand side.  The
	/// </para>
	/// <b>visible</b> column numbers are listed on the right-hand side.<para>
	/// Methods explicitly say in their documentation whether they take an 
	/// <b>absolute</b> or <b>visible</b> column number.  The methods 
	/// <code>getVisibleColumn()</code> and <code>getAbsoluteColumn()</code> can be
	/// </para>
	/// used to convert between the two column types.<para>
	/// In all cases, column and row numbers begin at zero.  A header is always
	/// shown above the table columns -- but it is not a row, nor can it
	/// be referenced as row 0, and it contains no row data.  It is a separate
	/// object.  The worksheet is often used with table models to display the row
	/// number in the first column, in which case column 1 (the second column)
	/// begins the actual worksheet data.  This method is the old way of doing it,
	/// however, as setting the property ShowRowHeader=true will use a separate
	/// </para>
	/// object to keep track of row numbers, and this is the preferred way.<para>
	/// 
	/// </para>
	/// <b>Working with JWorksheets</b><para>
	/// </para>
	/// <b>Setting up a JWorksheet</b><para>
	/// Here is some example code that sets up a JWorksheet (note that it uses 
	/// </para>
	/// a subclassed table renderer and table model data).<para><pre>
	/// // Create the proplist containing JWorksheet setup information.
	/// PropList p = new PropList("StateMod_Diversion_JFrame.JWorksheet");
	/// p.add("JWorksheet.CellFont=Courier");
	/// p.add("JWorksheet.CellStyle=Plain");
	/// p.add("JWorksheet.CellSize=11");
	/// p.add("JWorksheet.HeaderFont=Arial");
	/// p.add("JWorksheet.HeaderStyle=Plain");
	/// p.add("JWorksheet.HeaderSize=11");
	/// p.add("JWorksheet.HeaderBackground=LightGray");
	/// p.add("JWorksheet.RowColumnPresent=false");
	/// p.add("JWorksheet.ShowPopupMenu=true");
	/// p.add("JWorksheet.SelectionMode=SingleRowSelection");
	/// 
	/// // try to create the worksheet.
	/// int[] widths = null;
	/// try {
	///		// custom table model
	///		StateMod_Diversion_TableModel tmd = new StateMod_Diversion_TableModel(__diversionsVector, __readOnly);
	///		// custom cell renderer
	///		StateMod_Diversion_CellRenderer crd = new StateMod_Diversion_CellRenderer(tmd);
	/// 
	///		// create the table
	///		__worksheet = new JWorksheet(crd, tmd, p);
	/// 
	///		// get the column widths
	///		widths = crd.getColumnWidths();
	/// }
	/// catch (Exception e) {
	///		// if there was a problem, simply create an empty JWorksheet.
	///		Message.printWarning(3, routine, e);
	///		__worksheet = new JWorksheet(0, 0, p);
	///		e.printStackTrace();
	/// }
	/// // this call prevents some odd resizing problems with JTable/JWorksheet
	/// __worksheet.setPreferredScrollableViewportSize(null);
	/// 
	/// // set up the current JFrame to display the JWorksheet's hourglass when
	/// // necessary and also to listen to its key and mouse events.
	/// __worksheet.setHourglassJFrame(this);
	/// __worksheet.addMouseListener(this);	
	/// __worksheet.addKeyListener(this);
	/// 
	/// ...
	/// 
	/// // this code must appear *AFTER* the GUI on which the JWorksheet
	/// // appears is shown.  
	/// if (widths != null) {
	///		__worksheet.setColumnWidths(widths);
	/// }
	/// </pre>
	/// </para>
	/// <para>
	/// </para>
	/// <b>Cell Attributes</b><para>
	/// The above example sets up the font the JWorksheet will use for rendering 
	/// the cells in the JWorksheet and in the JWorksheet's header.  Different cell
	/// attributes can still be applied to individual cells to override the JWorksheet
	/// defaults.  The following code will change every other cell in the 3rd column
	/// </para>
	/// to have different attributes:<para><pre>
	/// JWorksheet_CellAttributes ca = new JWorksheet_CellAttributes();
	/// ca.backgroundColor = Color.red;
	/// ca.foregroundColor = Color.blue;
	/// ca.Font = new Font("Arial", Font.PLAIN, 11);	
	/// 
	/// for (int i = 0; i < __worksheet.getRowCount(); i++) {
	///		__worksheet.setCellAttributes(i, 2, ca);
	/// }
	/// </pre>
	/// </para>
	/// <para>
	/// </para>
	/// <b>JComboBoxes as Data Entry Fields</b><para>
	/// The JWorksheet offers support for using JComboBoxes for data entry.  The 
	/// </para>
	/// following code demonstrates setting a JComboBox on all cells in a column:<para>
	/// <pre>
	/// List<String> v = new Vector<String>();
	/// v.add("Red");
	/// v.add("Green");
	/// v.add("Blue");
	/// __worksheet.setColumnJComboBoxValues(4, v, true);
	/// </pre>
	/// The call to setColumnJComboBoxValues() sets the fifth column (4) to use 
	/// a SimpleJComboBox for its data entry.  Users can select from a list of three
	/// colors as possible data.  Because the third parameter was provided to 
	/// setColumnJComboBoxValues() and the parameter is 'true', users can also type
	/// </para>
	/// in another color that doesn't appear in the combo box.<para>
	/// 
	/// As opposed to putting a single combo box with the same values on a column, 
	/// individual cells within a column can be set to use combo boxes, and each 
	/// combo box can have a different list of values.  If a column is set up to 
	/// allow cell-specific placement of combo boxes, all the cells in which a 
	/// combo box was NOT explicitly set will use the same text field data entry as 
	/// normal data entry cells.  The following code demonstrates
	/// </para>
	/// placing different combo boxes on cells within a column:<para><pre>
	/// __worksheet.setCellSpecificJComboBoxColumn(3, true);
	/// 
	/// List<String> diversions = new ArrayList<String>();
	/// diversions.add("Diversion 1");
	/// diversions.add("Diversion 2");
	/// 
	/// List<String> reservoirs = new ArrayList<String>();
	/// reservoirs.add("Reservoir 1");
	/// reservoirs.add("Reservoir 2");
	/// 
	/// List<String> wells = new ArrayList<String>();
	/// wells.add("Well 1");
	/// wells.add("Well 2");
	/// 
	/// __worksheet.setCellSpecificJComboBoxValues(4, 3, diversions);
	/// __worksheet.setCellSpecificJComboBoxValues(5, 3, reservoirs);
	/// __worksheet.setCellSpecificJComboBoxValues(3, 3, wells);
	/// 
	/// __worksheet.setCellSpecificJComboBoxEditorPreviousRowCopy(3, true);
	/// </pre>
	/// The above code first sets up the 3rd column to allow cell-specific placement
	/// of combo boxes.  The 'true' parameter means that users will be able to type in
	/// </para>
	/// other values to the JComboBoxes if they value they want does not appear.<para>
	/// 
	/// The code then sets up a few lists of example entry values and applies 
	/// </para>
	/// combo boxes to rows 3, 4 and 5 in the worksheet.<para>
	/// 
	/// The last part of the code sets up what the worksheet column's behavior should be
	/// when new rows are added after the last row.  In this case, the 'true' parameter
	/// specifies that if the next-to-last row (the one just before the new row added)
	/// has a cell-specific combobox set up in column 3, the same combo box should be
	/// </para>
	/// set up in column 3 of the new row, too, with all the same values.<para>
	/// 
	/// </para>
	/// <b>Class Descriptions</b><para>
	/// </para>
	/// The following is a brief list of all the related JWorksheet_* classes and their purposes:<para>
	/// <ul>
	/// <li><b>JWorksheet_AbstractExcelCellRenderer</b> - This class is the cell
	/// renderer used by most applications, as it provides the capability to properly
	/// left- and right-justify the text in table cells depending on the kind of 
	/// data stored in the cell.</li>
	/// <li><b>JWorksheet_AbstractRowTableModel</b> - This abstract class is the table 
	/// model from which many application table models will be built, as it provides 
	/// support for storing a single data object in each row of a JWorksheet.</li>
	/// <li><b>JWorksheet_AbstractTableCellRenderer</b> - This is the base class for
	/// building all other Cell Renderers for JWorksheets.  It ensures that all 
	/// JWorksheet cell renderers provide at least a getColumnWidths() method.</li>
	/// <li><b>JWorksheet_AbstractTableModel</b> - This is the class from which all
	/// the table models used in a JWorksheet must be built.  It provides some base
	/// functionality common to all JWorksheet table models, such as row sorting.</li>
	/// <li><b>JWorksheet_CellAttributes</b> - This class contains attributes that can
	/// be set and applied to individual cells within the table.</li>
	/// <li><b>JWorksheet_ColSelectionModel & JWorksheet_RowSelectionModel</b> - 
	/// These classes provide the JWorksheet with the ability to do Microsoft Excel-like
	/// cell selection.  Programmers should not need to work with these classes.</li>
	/// <li><b>JWorksheet_CopyPasteAdapter</b> - This class provides support for copying
	/// and pasting to JWorksheets.  Programs should not need to work directly with this class.</li>
	/// <li><b>JWorksheet_DefaultTableCellEditor</b> - This class overrides the normal
	/// editor used for editing cell values.  Programs should not need to work directly
	/// with this class.</li>
	/// <li><b>JWorksheet_DefaultTableCellRenderer</b> - This is the first
	/// implementation of a Cell Renderer that can be use by a JWorksheet, if no
	/// other Cell Renderer is provided.</li>
	/// <li><b>JWorksheet_Header</b> - This is the class used to render the JWorksheet's
	/// header.  It provide capability like column header tool tips.  Programmers should
	/// not need to work with this class.</li>
	/// <li><b>JWorksheet_HeaderCellRenderer</b> - This is the class used to render
	/// the JWorksheet's header.  Programmers probably won't need to subclass this.</li>
	/// <li><b>JWorksheet_JComboBoxCellEditor</b> - This class provides columns with
	/// the ability to have combo boxes as data entry editors.</li>
	/// <li><b>JWorksheet_Listener</b> - This class is an interface for other classes
	/// that want to be informed whenever the table performs some actions such as 
	/// adding or removing rows.</li>
	/// <li><b>JWorksheet_RowCountCellRenderer</b> - This class is a cell renderer
	/// for the optional first column of worksheet cells that display the number of the row.</li>
	/// </ul>
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class JWorksheet extends javax.swing.JTable implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.MouseListener, java.awt.event.AdjustmentListener
	public class JWorksheet : JTable, ActionListener, KeyListener, MouseListener, AdjustmentListener
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			__columnNumbering = __NUMBERING_NONE;
		}


	// TODO TODO add setColumnEditable()

	/// <summary>
	/// JWorksheet selection model in which cells can be non-contiguously selected like in an Excel worksheet.
	/// </summary>
	private const int __EXCEL_SELECTION = 1000;

	/// <summary>
	/// JTable Selection mode in which only a single cell can be selected at a time. 
	/// </summary>
	private static readonly int __SINGLE_CELL_SELECTION = ListSelectionModel.SINGLE_SELECTION + 100;

	/// <summary>
	/// Selection mode in which only a single row can be selected at a time.
	/// </summary>
	private static readonly int __SINGLE_ROW_SELECTION = ListSelectionModel.SINGLE_SELECTION;

	/// <summary>
	/// Selection mode in which multiple contiguous rows can be selected.
	/// </summary>
	private static readonly int __MULTIPLE_ROW_SELECTION = ListSelectionModel.SINGLE_INTERVAL_SELECTION;

	/// <summary>
	/// Selection mode in which multiple discontinuous rows can be selected.
	/// </summary>
	private static readonly int __MULTIPLE_DISCONTINUOUS_ROW_SELECTION = ListSelectionModel.MULTIPLE_INTERVAL_SELECTION;

	public const int PRE_SELECTION_CHANGE = 0;
	public const int POST_SELECTION_CHANGE = 1;

	private const int __DESELECT_ALL = 100;
	private const int __SELECT_ALL = 101;

	/// <summary>
	/// Bit-mask parameter for find() that can be used with all searches to check 
	/// if a value is equal to another value.  There is no default specifying how 
	/// searches must be done, so parameters must be specified for searches.  For 
	/// numeric and date searches, one of FIND_EQUAL_TO, FIND_LESS_THAN or 
	/// FIND_GREATER_THAN must be specified.  For String searches, one of 
	/// FIND_EQUAL_TO, FIND_CONTAINS, FIND_STARTS_WITH, or FIND_ENDS_WITH must be specified.
	/// </summary>
	public const int FIND_EQUAL_TO = 1;

	/// <summary>
	/// Bit-mask parameter for numeric find()s that can check for values less than the 
	/// specified value.  There is no default specifying how 
	/// searches must be done, so parameters must be specified for searches.  For 
	/// numeric and date searches, one of FIND_EQUAL_TO, FIND_LESS_THAN or 
	/// FIND_GREATER_THAN must be specified.  For String searches, one of 
	/// FIND_EQUAL_TO, FIND_CONTAINS, FIND_STARTS_WITH, or FIND_ENDS_WITH must be specified.
	/// </summary>
	public const int FIND_LESS_THAN = 2;

	/// <summary>
	/// Bit-mask parameter for numeric find()s that can check for values greater 
	/// than the 
	/// specified value.  There is no default specifying how 
	/// searches must be done, so parameters must be specified for searches.  For 
	/// numeric and date searches, one of FIND_EQUAL_TO, FIND_LESS_THAN or 
	/// FIND_GREATER_THAN must be specified.  For String searches, one of 
	/// FIND_EQUAL_TO, FIND_CONTAINS, FIND_STARTS_WITH, or FIND_ENDS_WITH must be specified.
	/// </summary>
	public const int FIND_GREATER_THAN = 4;

	/// <summary>
	/// Bit-mask parameter for String find()s that turns off case sensitivity.  By default, 
	/// String searches are case-sensitive.  There is no default specifying how 
	/// searches must be done, so parameters must be specified for searches.  For 
	/// numeric and date searches, one of FIND_EQUAL_TO, FIND_LESS_THAN or 
	/// FIND_GREATER_THAN must be specified.  For String searches, one of 
	/// FIND_EQUAL_TO, FIND_CONTAINS, FIND_STARTS_WITH, or FIND_ENDS_WITH must be specified.
	/// </summary>
	public const int FIND_CASE_INSENSITIVE = 16;

	/// <summary>
	/// Bit-mask parameter for String find()s to check for the value as a substring that
	/// starts another string.  There is no default specifying how 
	/// searches must be done, so parameters must be specified for searches.  For 
	/// numeric and date searches, one of FIND_EQUAL_TO, FIND_LESS_THAN or 
	/// FIND_GREATER_THAN must be specified.  For String searches, one of 
	/// FIND_EQUAL_TO, FIND_CONTAINS, FIND_STARTS_WITH, or FIND_ENDS_WITH must be specified.
	/// </summary>
	public const int FIND_STARTS_WITH = 32;

	/// <summary>
	/// Bit-mask parameter for String find()s to check for the value as a substring that
	/// ends another string.  There is no default specifying how 
	/// searches must be done, so parameters must be specified for searches.  For 
	/// numeric and date searches, one of FIND_EQUAL_TO, FIND_LESS_THAN or 
	/// FIND_GREATER_THAN must be specified.  For String searches, one of 
	/// FIND_EQUAL_TO, FIND_CONTAINS, FIND_STARTS_WITH, or FIND_ENDS_WITH must be specified.
	/// </summary>
	public const int FIND_ENDS_WITH = 64;

	/// <summary>
	/// Bit-mask parameter for String find()s to check for the value as a substring that
	/// is contained within another string.  There is no default specifying how 
	/// searches must be done, so parameters must be specified for searches.  For 
	/// numeric and date searches, one of FIND_EQUAL_TO, FIND_LESS_THAN or 
	/// FIND_GREATER_THAN must be specified.  For String searches, one of 
	/// FIND_EQUAL_TO, FIND_CONTAINS, FIND_STARTS_WITH, or FIND_ENDS_WITH must be specified.
	/// </summary>
	public const int FIND_CONTAINS = 128;

	/// <summary>
	/// Bit-mask parameter for all find()s to start searching from the back of the list
	/// towards the front.  By default, finds work from the first record towards the 
	/// last.  "First record" refers to the first record returned from the table model
	/// when getValueAt() is called.
	/// </summary>
	public const int FIND_REVERSE = 256;

	/// <summary>
	/// Bit-mask parameter for all find()s so that a search can start in the middle 
	/// of a worksheet's data and wrap around and start again at the beginning.  
	/// By default, searches start at the beginning of the table's data.
	/// </summary>
	public const int FIND_WRAPAROUND = 512;

	/// <summary>
	/// Constant values used in specifying column alignments.
	/// </summary>
	public const int DEFAULT = -1, CENTER = SwingConstants.CENTER, LEFT = SwingConstants.LEFT, RIGHT = SwingConstants.RIGHT;

	/// <summary>
	/// Types of column prefix numbering support.  __NUMBERING_NONE means no column
	/// prefixes are set.  __NUMBERING_EXCEL means that column numbers in the format
	/// A...Z AA...AZ BA...BZ ... are done.  __NUMBERING_0 means that the column prefix
	/// is the number of the column, base-0.  __NUMBERING_1 means that the column
	/// prefix is the number of the column, base-1.
	/// </summary>
	private readonly int __NUMBERING_NONE = -1, __NUMBERING_EXCEL = 0, __NUMBERING_0 = 1, __NUMBERING_1 = 2;

	/// <summary>
	/// Used with notifyAllWorksheetListeners to let JWorksheet_Listeners know that a 
	/// row has been added.  Also used with adjustCellAttributesAndText() to inform that a row has been inserted.
	/// </summary>
	private readonly int __ROW_ADDED = 0;

	/// <summary>
	/// Used with notifyAllWorksheetListeners to let JWorksheet_Listeners know that a 
	/// row has been deleted.  Also used with adjustCellAttributesAndText() to inform that a row has been deleted.
	/// </summary>
	private readonly int __ROW_DELETED = 1;

	/// <summary>
	/// Used with notifyAllWorksheetListeners to let JWorksheet_Listeners know that the 
	/// row data has been changed with setData().  
	/// </summary>
	private readonly int __DATA_RESET = 2;

	/// <summary>
	/// Used with adjustCellAttributesAndText() to inform that a column has been removed from the table.
	/// </summary>
	private readonly int __COL_DELETED = 3;

	/// <summary>
	/// Popup menu labels.
	/// </summary>
	private readonly string __MENU_ORIGINAL_ORDER = "Original Order", __MENU_SORT_ASCENDING = "Sort Ascending", __MENU_SORT_DESCENDING = "Sort Descending", __MENU_COPY = "Copy", __MENU_COPY_HEADER = "Copy with Header", __MENU_COPY_ALL = "Copy All", __MENU_COPY_ALL_HEADER = "Copy All with Header", __MENU_DESELECT_ALL = "Deselect All", __MENU_PASTE = "Paste", __MENU_CALCULATE_STATISTICS = "Calculate Statistics", __MENU_SAVE_TO_FILE = "Save to file ...", __MENU_SELECT_ALL = "Select All";

	/// <summary>
	/// Action listeners that listing to the popup menu events.
	/// These are called from the actionPerformed() method.
	/// </summary>
	private IList<ActionListener> popupMenuActionListeners = new List<ActionListener>();

	/// <summary>
	/// The initial size of and size by which the cell attribute arrays grow.
	/// </summary>
	private readonly int __ARRAY_SIZE = 50;

	/// <summary>
	/// The class name.
	/// </summary>
	public const string CLASS = "JWorksheet";

	/// <summary>
	/// An array sized the same as the number of columns in the current data model,
	/// used to tell when columns have been hidden from view.
	/// </summary>
	private bool[] __columnRemoved = null;

	/// <summary>
	/// Whether copying from the table has been enabled or not.
	/// </summary>
	private bool __copyEnabled = false;

	/// <summary>
	/// Whether the worksheet is dirty or not.
	/// </summary>
	private bool __dirty = false;

	/// <summary>
	/// Whether the row numbers along the left side go up in value or not.
	/// </summary>
	private bool __incrementRowNumbers = true;

	/// <summary>
	/// Whether the control key is depressed.
	/// </summary>
	private bool __isControlDown = false;

	/// <summary>
	/// Whether the shift key is depressed.
	/// </summary>
	private bool __isShiftDown = false;

	/// <summary>
	/// Whether pasting from the table has been enabled or not (default=false).
	/// </summary>
	private bool __pasteEnabled = false;

	/// <summary>
	/// Whether calculate statistics for the table has been enabled or not (default=false).
	/// If true, a pop-up menu item "Calculate Statistics" will be shown.
	/// If the DelegateCalculateStatistics=true is set, then the action event in this
	/// class will ignore the event, assuming that another action listener is registered on the sheet to handle.
	/// </summary>
	private bool __calculateStatisticsEnabled = false;

	/// <summary>
	/// Whether the "Calculate Statistics" menu item should be handled in this class (false)
	/// or delegated to another class that has registered an ActionListener (true).
	/// </summary>
	private bool __delegateCalculateStatistics = false;

	/// <summary>
	/// Whether to select an entire column (with the Excel selection mode only) when the column header is clicked on.
	/// </summary>
	private bool __oneClickColumnSelection = false;

	/// <summary>
	/// Whether to select an entire row when the row header is clicked on.
	/// TODO (JTS - 2004-11-19) with the new row headers, it's very likely this won't work anymore.
	/// </summary>
	private bool __oneClickRowSelection = false;

	/// <summary>
	/// Whether this worksheet's cells can be selected or not.
	/// </summary>
	private bool __selectable = true;

	/// <summary>
	/// Whether to show the sorting popup menu.  Defaults to true unless another value is provided by the proplist.
	/// </summary>
	private bool __showPopup = true;

	/// <summary>
	/// Whether to show the first column with the row count.
	/// </summary>
	private bool __showRowCountColumn = true;

	/// <summary>
	/// Whether worksheet code is currently running any test version of the code.
	/// </summary>
	private bool __testing = false;

	/// <summary>
	/// Whether to use the row headers that work similarly to the standard JTable column headers.
	/// </summary>
	private bool __useRowHeaders = false;

	/// <summary>
	/// Whether the worksheet should handle displaying the regular popup menu or something external will.
	/// </summary>
	private bool __worksheetHandlePopup = true;

	/// <summary>
	/// The color in which the header cells will be drawn.  Defaults to Color.LIGHT_GRAY
	/// unless a new value is provided by the proplist.
	/// </summary>
	private Color __columnHeaderColor = null;

	/// <summary>
	/// The color in which the row count cells will be drawn.  Defaults to 
	/// Color.LIGHT_GRAY unless a new value is provided by the proplist.
	/// </summary>
	// TODO SAM 2007-05-09 Evaluate whether used
	//private Color __rowCountColumnColor = Color.LIGHT_GRAY;

	/// <summary>
	/// The background color for the row header.  Defaults to the system standard color.
	/// </summary>
	private Color __rowHeaderColor = null;

	/// <summary>
	/// Used to hold the 'compiled' version of the cell font, for quick retrieval by the cell renderers.
	/// </summary>
	private Font __cellFont = null;

	/// <summary>
	/// Array marking the columns of the cells with alternate text.
	/// </summary>
	private int[] __altTextCols;

	/// <summary>
	/// Array marking the rows of the cells with alternate text.
	/// </summary>
	private int[] __altTextRows;

	/// <summary>
	/// Array marking the columns of the cells with attributes.
	/// </summary>
	private int[] __attrCols;

	/// <summary>
	/// Array marking the rows of the cells with attributes.
	/// </summary>
	private int[] __attrRows;

	/// <summary>
	/// Used to override the default alignment of columns in the table.
	/// </summary>
	private int[] __columnAlignments = null;

	/// <summary>
	/// Count of all the cells with alternate text.
	/// </summary>
	private int __altTextCount = 0;

	/// <summary>
	/// The size of the font in which the table data should be displayed.  By default is 11.
	/// </summary>
	private int __cellFontSize = -1;

	/// <summary>
	/// The style of the cell font.  By default is Font.PLAIN.
	/// </summary>
	private int __cellFontStyle = -1;

	/// <summary>
	/// The size of the font in which the header should be displayed.  By default, is 12.
	/// </summary>
	private int __columnHeaderFontSize = -1;

	/// <summary>
	/// The style of the header font.  By default, is Font.BOLD.
	/// </summary>
	private int __columnHeaderFontStyle = -1;

	/// <summary>
	/// The size of the font in which the header should be displayed.  By default, is 12.
	/// </summary>
	private int __rowHeaderFontSize = -1;

	/// <summary>
	/// The style of the header font.  By default, is Font.BOLD.
	/// </summary>
	private int __rowHeaderFontStyle = -1;

	/// <summary>
	/// Count of all the cells with attributes.
	/// </summary>
	private int __attrCount = 0;

	/// <summary>
	/// The kind of column prefix numbering done.
	/// </summary>
	private int __columnNumbering;

	/// <summary>
	/// The <b>visible</b> column of the cell that was last edited.
	/// </summary>
	private int __editCol = -1;

	/// <summary>
	/// The row of the cell that was last edited.
	/// </summary>
	private int __editRow = -1;

	/// <summary>
	/// The first row number that appears in the row header.  All other row numbers are determined from this one.
	/// </summary>
	private int __firstRowNum = 1;

	/// <summary>
	/// The last row selected by clicking on the row header.
	/// </summary>
	private int __lastRowSelected = -1;

	/// <summary>
	/// The <b>visible</b> column on which the popup menu was last opened, used to keep track of which column to sort. 
	/// </summary>
	private int __popupColumn = -1;

	/// <summary>
	/// The selection mode in which the table is currently operating.  A -1 means
	/// the JWorksheet mode, as opposed to one of the JTable modes, is in effect.
	/// </summary>
	private int __selectionMode = __EXCEL_SELECTION;

	/// <summary>
	/// The dialog in which the hourglass will be shown for sorting.  If null, the hourglass won't be shown.
	/// </summary>
	private JDialog __hourglassJDialog = null;

	/// <summary>
	/// The frame in which the hourglass will be shown for sorting.  If null, the hourglass won't be shown.
	/// </summary>
	private JFrame __hourglassJFrame = null;

	/// <summary>
	/// The item in the popup menu that allows a user to undo a sort operation.
	/// </summary>
	private JMenuItem __cancelMenuItem = null;

	/// <summary>
	/// The item in the popup menu that allows a user to copy cell contents.
	/// </summary>
	private JMenuItem __copyMenuItem = null;
	private JMenuItem __copyAllMenuItem = null;
	/// <summary>
	/// The item in the popup menu that allows a user to copy cell contents with the
	/// appropriate header information, too.
	/// </summary>
	private JMenuItem __copyHeaderMenuItem = null;
	private JMenuItem __copyAllHeaderMenuItem = null;

	private JMenuItem __deselectAllMenuItem = null;
	private JMenuItem __selectAllMenuItem = null;

	/// <summary>
	/// The item in the popup menu that allows a user to paste into cells.
	/// </summary>
	private JMenuItem __pasteMenuItem = null;

	/// <summary>
	/// The item in the popup menu that allows a user to calculate statistics.
	/// </summary>
	private JMenuItem __calculateStatisticsMenuItem = null;


	/// <summary>
	/// The popup menu that can be set to open when the table is right-clicked on.
	/// </summary>
	private JPopupMenu __mainPopup = null;

	/// <summary>
	/// The JPopupMenu that will appear when the table header is right-clicked on.
	/// </summary>
	private JPopupMenu __popup;

	/// <summary>
	/// The header of the table -- used in case the user turns off the header and then wants to turn it back on later.
	/// </summary>
	private JViewport __columnHeaderView = null;

	/// <summary>
	/// If this worksheet uses another worksheet as its row header, this is the reference to it.
	/// </summary>
	private JWorksheet __worksheetRowHeader = null;

	/// <summary>
	/// Array of the cell attributes for the cells with attributes.
	/// </summary>
	private JWorksheet_CellAttributes[] __cellAttrs;

	/// <summary>
	/// The attributes for the cells that compromise the "row count column" (in other words, column 0).
	/// </summary>
	private JWorksheet_CellAttributes __rowCountColumnAttributes;

	/// <summary>
	/// The adapter for doing copy and paste operations.
	/// </summary>
	private JWorksheet_CopyPasteAdapter __copyPasteAdapter = null;

	/// <summary>
	/// The default table cell renderer used for rendering cells in the table.
	/// </summary>
	private JWorksheet_DefaultTableCellRenderer __defaultCellRenderer = null;

	/// <summary>
	/// The renderer used to render the JWorksheet's header.
	/// </summary>
	private JWorksheet_HeaderCellRenderer __hcr;

	/// <summary>
	/// When two tables are tied together, for instance, one is the row header of the
	/// other, this row model is used so that certain selections on the row header one
	/// will make selections on the other one as well.
	/// </summary>
	private JWorksheet_RowSelectionModel __partner = null;

	/// <summary>
	/// The List that is used to function as a row header.
	/// </summary>
	private SimpleJList __listRowHeader = null;

	/// <summary>
	/// Array of the cell attributes for the cells with alternate text.
	/// </summary>
	private string[] __altText;
	/// <summary>
	/// An array, sized the same as the number of columns in the current table model,
	/// that holds the names of the columns.
	/// </summary>
	private string[] __columnNames = null;

	/// <summary>
	/// The font name for use in table cells.  By default is "Arial".
	/// </summary>
	private string __cellFontName = null;

	/// <summary>
	/// The font name for use in the table header.  By default, is "Arial".
	/// </summary>
	private string __columnHeaderFontName = null;

	/// <summary>
	/// The font name for use in the table header.  By default, is "Arial".
	/// </summary>
	private string __rowHeaderFontName = null;

	/// <summary>
	/// A list of registered sort listeners.
	/// </summary>
	private IList<JWorksheet_SortListener> __sortListeners = null;

	/// <summary>
	/// A Vector of registered JWorksheet_Listeners.
	/// </summary>
	private IList<JWorksheet_Listener> __worksheetListeners = null;

	/// <summary>
	/// In testing -- will probably be moved into a property, but maybe not, so it can be turned on and off.
	/// This specifies whether when doing any sort of internal processing of numeric
	/// data (eg, copying to clipboard, writing to a file, etc), missing values (-999)
	/// will be output as -999 or as empty strings ("").
	/// TODO (JTS - 2005-11-15)
	/// </summary>
	public bool COPY_MISSING_AS_EMPTY_STRING = true;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="cellRenderer"> a JWorksheet_DefaultTableCellRenderer object 
	/// (or a class derived from JWorksheet_DefaultTableCellRenderer) that will be 
	/// used for rendering cells holding Integers, Strings, Dates and Doubles. </param>
	/// <param name="tableModel"> the TableModel that will be used to fill the worksheet with data. </param>
	/// <param name="props"> the properties to configure the JWorksheet:
	/// <table width=80% cellpadding=2 cellspacing=0 border=2>
	/// <tr>
	/// <td>Property</td>        <td>Description</td>     <td>Default</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.CellFontName</td>
	/// <td>The font face in which data cells will be drawn.  Example values are 
	/// "Courier", "Arial", "Helvetica"</td>
	/// <td>Default JTable font</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.CellFontStyle</td>
	/// <td>The font style in which cells will be drawn.  Example values 
	/// are "Italic", "Bold", or "Plain"</td>
	/// <td>"Plain"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.CellFontSize</td>
	/// <td>The font size (in points) in which cells will be drawn.  Example values 
	/// are 11, 14</td>
	/// <td>Default JTable font size</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.HeaderFontName</td>
	/// <td>The font face in which data header cells will drawn.  Example values are
	/// "Courier", "Arial", "Helvetica"</td>
	/// <td>Default Windows font</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.HeaderFontStyle</td>
	/// <td>The font style in which header cells will be drawn.  Example values are 
	/// "Italic", "Bold", or "Plain"</td>
	/// <td>"Plain"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.HeaderFontSize</td>
	/// <td>The font size in which header cells will be drawn.  Example values are 11,
	/// 14</td>
	/// <td>Default Windows font size</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.HeaderBackground</td>
	/// <td>The background color in which header cells will be drawn. Example values are
	/// "LightGray", "Blue"</td>
	/// <td>"LightGray"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.RowColumnPresent</td>
	/// <td>Whether the column with the row # count is shown or not.  Possible values 
	/// are "true" or "false"</td>
	/// <td>"true"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.RowColumnBackground</td>
	/// <td>The color in which row count cells will be drawn.  Example values are 
	/// "LightGray", "Blue"</td>
	/// <td>"White"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.ShowPopupMenu</td>
	/// <td>Whether to show the popup menu when the user right-clicks on the 
	/// worksheet header.  This popup menu usually is used to sort the data in the 
	/// column beneath the popup menu.
	/// Values are "true" or "false"</td>
	/// <td>"true"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.SelectionMode</td>
	/// <td>The kind of selection mode to use in the JWorksheet.  Possible values are:
	/// <ul>
	/// <li><b>ExcelSelection</b> - Cell selection is similar to Microsoft Excel.  
	/// Discontiguous cells and ranges of cells can all be selected at the same time.
	/// </li>
	/// <li><b>SingleCellSelection</b> - A single cell can be selected at a time.</li>
	/// <li><b>SingleRowSelection</b> - A single row can be selected at a time.</li>
	/// <li><b>MultipleRowSelection</b> - Multiple continuous rows can be selected
	/// at one time.</b></li>
	/// <li><b>MultipleDiscontinuousRowSelection</b> - Multiple discontinuous rows can
	/// be selected at one time.</b></li>
	/// </ul>
	/// </td>
	/// <td>"ExcelSelection"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.OneClickRowSelection</td>
	/// <td>Whether to select all the values in a row when the row header is clicked
	/// on.  Possible values are "true" or "false".  This doesn't work with the 
	/// single-cell selection mode.</td>
	/// <td>"false"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.OneClickColumnSelection</td>
	/// <td>Whether to select all the values in a column when the column header is
	/// clicked on.  Possible values are "true" or "false".  This only works
	/// with the Excel selection mode.</td>
	/// <td>"false"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.Unselectable</td>
	/// <td>Whether any cell in the worksheet can be selected or not.  Possible values
	/// are "true" or "false."  
	/// REVISIT (JTS - 2004-11-19)
	/// rename this to selectable, for clarity
	/// </td>
	/// <td>"false"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.Selectable</td>
	/// <td>Whether any cell in the worksheet can be selected or not.  Possible values
	/// are "true" or "false."  
	/// </td>
	/// <td>"true"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.AllowCopy</td>
	/// <td>Whether to allow the user to copy data from worksheet cells to the 
	/// clipboard.  Possible values are "true" or "false."</td>
	/// <td>"false"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.AllowPaste</td>
	/// <td>Whether to allow users to paste in values from the clipboard to groups
	/// of cells.  Possible values are "true" or "false."</td>
	/// <td>"false"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.ColumnNumbering</td>
	/// <td>How to number the values in the header of the worksheet.  Possible 
	/// values are "Base0", "Base1", "Excel", or "None."
	/// </td>
	/// <td>"Base1"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.FirstRowNumber</td>
	/// <td>The number of the very top-most row in the worksheet.</td>
	/// <td>"1"</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>JWorksheet.IncrementRowNumbers</td>
	/// <td>Whether to increment or decrement the row numbers, starting from the
	/// first row number.  If "true", the row numbers will get bigger from the
	/// first row number by increments of 1.  If "false", the row numbers will get
	/// smaller from the first row number by increments of 1.</td>
	/// <td>"true"</td>
	/// </tr>
	/// 
	/// </table>
	/// </ul>
	/// 
	/// TODO (JTS - 2004-11-19) alphabetize the above </param>
	public JWorksheet(JWorksheet_DefaultTableCellRenderer cellRenderer, JWorksheet_AbstractTableModel tableModel, PropList props)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		setTableHeader(new JWorksheet_Header());
		__rowCountColumnAttributes = new JWorksheet_CellAttributes();

		// TODO (JTS - 2005-01-25)
		// reading the proplist is done twice in this constructor.  which 
		// one can be removed -- make sure it doesn't break existing worksheets!
		readPropList(props);
		tableModel._worksheet = this;
		__worksheetListeners = new List<JWorksheet_Listener>();

		// if one is not defined, do the following to avoid null checks
		__hourglassJFrame = new JFrame();

		__columnHeaderColor = null;
		// TODO SAM 2007-05-09 Evaluate whether used
		//__rowCountColumnColor = Color.LIGHT_GRAY;

		// create the arrays to hold cell attribute information
		__attrCols = new int[__ARRAY_SIZE];
		__attrRows = new int[__ARRAY_SIZE];
		__cellAttrs = new JWorksheet_CellAttributes[__ARRAY_SIZE];
		for (int i = 0; i < __ARRAY_SIZE; i++)
		{
			__attrCols[i] = -1;
			__attrRows[i] = -1;
			__cellAttrs[i] = null;
		}

		// create the arrays to hold cell alternate text
		__altTextCols = new int[__ARRAY_SIZE];
		__altTextRows = new int[__ARRAY_SIZE];
		__altText = new string[__ARRAY_SIZE];
		for (int i = 0; i < __ARRAY_SIZE; i++)
		{
			__altTextCols[i] = -1;
			__altTextRows[i] = -1;
			__altText[i] = null;
		}

		readPropList(props);

		setCellRenderer(cellRenderer);
		setModel(tableModel);
		if (tableModel.getColumnToolTips() != null)
		{
			setColumnsToolTipText(tableModel.getColumnToolTips());
		}

		__cellFont = new Font(__cellFontName, __cellFontStyle, __cellFontSize);
		// added as its own key listener here so that it is only intentionally done once	
		addKeyListener(this);
		if (getTableHeader() != null)
		{
			getTableHeader().addKeyListener(this);
		}
	}

	/// <summary>
	/// Constructor.  Builds a JWorksheet with all empty cells with the given number
	/// of rows and columns.  This version is mostly used to create a blank JWorksheet
	/// object (0 rows, 0 cols) with the specified properties, before data are populated. </summary>
	/// <param name="rows"> the number of rows in the empty worksheet. </param>
	/// <param name="cols"> the number of columns in the empty worksheet. </param>
	/// <param name="props"> PropList defining table characteristics.  See the first constructor. </param>
	public JWorksheet(int rows, int cols, PropList props) : base(rows, cols)
	{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		setTableHeader(new JWorksheet_Header());

		// if one is not defined, do the following to avoid null checks	
		__hourglassJFrame = new JFrame();

		// create the arrays to hold cell attribute information
		__attrCols = new int[__ARRAY_SIZE];
		__attrRows = new int[__ARRAY_SIZE];
		__cellAttrs = new JWorksheet_CellAttributes[__ARRAY_SIZE];
		for (int i = 0; i < __ARRAY_SIZE; i++)
		{
			__attrCols[i] = -1;
			__attrRows[i] = -1;
			__cellAttrs[i] = null;
		}

		// create the arrays to hold cell alternate text
		__altTextCols = new int[__ARRAY_SIZE];
		__altTextRows = new int[__ARRAY_SIZE];
		__altText = new string[__ARRAY_SIZE];
		for (int i = 0; i < __ARRAY_SIZE; i++)
		{
			__altTextCols[i] = -1;
			__altTextRows[i] = -1;
			__altText[i] = null;
		}

		__worksheetListeners = new List<JWorksheet_Listener>();

		__columnHeaderColor = null;
		// TODO 2007-05-09 Evaluate whether used
		//__rowCountColumnColor = Color.LIGHT_GRAY;	

		__rowCountColumnAttributes = new JWorksheet_CellAttributes();
		readPropList(props);
		initialize(rows, cols);
		__cellFont = new Font(__cellFontName, __cellFontStyle, __cellFontSize);
		// added as its own key listener here so that it is only intentionally done once
		addKeyListener(this);
		if (getTableHeader() != null)
		{
			getTableHeader().addKeyListener(this);
		}
	}

	/// <summary>
	/// Responds to actions, in this case popup menu actions. </summary>
	/// <param name="event"> the ActionEvent that occurred. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();
		// Set to true below - used to call external listeners
		// -for now it is only used for Calculate Statistics
		bool popupMenuEvent = false;

		if (command.Equals(__MENU_SORT_ASCENDING))
		{
			setWaitCursor(true);
			notifySortListenersSortAboutToChange(StringUtil.SORT_ASCENDING);
			sortColumn(StringUtil.SORT_ASCENDING);
			notifySortListenersSortChanged(StringUtil.SORT_ASCENDING);
			setWaitCursor(false);
		}
		else if (command.Equals(__MENU_SORT_DESCENDING))
		{
			setWaitCursor(true);
			notifySortListenersSortAboutToChange(StringUtil.SORT_DESCENDING);
			sortColumn(StringUtil.SORT_DESCENDING);
			notifySortListenersSortChanged(StringUtil.SORT_DESCENDING);
			setWaitCursor(false);
		}
		else if (command.Equals(__MENU_ORIGINAL_ORDER))
		{
			setWaitCursor(true);
			notifySortListenersSortAboutToChange(-1);
			((JWorksheet_AbstractTableModel)getModel()).setSortedOrder(null);
			((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();
			__cancelMenuItem.setEnabled(false);
			notifySortListenersSortChanged(-1);
			setWaitCursor(false);
		}
		else if (command.Equals(__MENU_COPY))
		{
			copyToClipboard();
		}
		else if (command.Equals(__MENU_COPY_HEADER))
		{
			copyToClipboard(true);
		}
		else if (command.Equals(__MENU_COPY_ALL))
		{
			copyAllToClipboard();
		}
		else if (command.Equals(__MENU_COPY_ALL_HEADER))
		{
			copyAllToClipboard(true);
		}
		else if (command.Equals(__MENU_DESELECT_ALL))
		{
			deselectAll();
		}
		else if (command.Equals(__MENU_PASTE))
		{
			pasteFromClipboard();
		}
		else if (command.Equals(__MENU_CALCULATE_STATISTICS))
		{
			try
			{
				if (!__delegateCalculateStatistics)
				{
					// Calculate the statistics in this class
					calculateStatistics();
				}
				else
				{
					popupMenuEvent = true;
					// Event will be handled below calling ActionListeners
				}
			}
			catch (Exception)
			{
				Message.printWarning(1, "", "Error calculating statistics");
			}
		}
		else if (command.Equals(__MENU_SAVE_TO_FILE))
		{
			saveToFile();
		}
		else if (command.Equals(__MENU_SELECT_ALL))
		{
			selectAllRows();
		}
		if (popupMenuEvent)
		{
			// Call the ActionListener that have been registered
			foreach (ActionListener l in popupMenuActionListeners)
			{
				l.actionPerformed(@event);
			}
		}
	}

	/// <summary>
	/// Adds a JWorksheet_Listener to the list of registered listeners. </summary>
	/// <param name="l"> the listener to register. </param>
	public virtual void addJWorksheetListener(JWorksheet_Listener l)
	{
		__worksheetListeners.Add(l);
	}

	/// <summary>
	/// Adds an action listener for the popup menu. </summary>
	/// <param name="l"> the MouseListener to add. </param>
	public virtual void addPopupMenuActionListener(ActionListener l)
	{
		this.popupMenuActionListeners.Add(l);
	}

	/// <summary>
	/// Adds a listener for mouse events on the worksheet's header. </summary>
	/// <param name="l"> the MouseListener to add. </param>
	public virtual void addHeaderMouseListener(MouseListener l)
	{
		if (getTableHeader() != null)
		{
			getTableHeader().addMouseListener(l);
		}
	}

	/// <summary>
	/// Adds a mouse listener for the JWorksheet.  To add a mouse listener for the 
	/// worksheet's header, use addHeaderMouseListener(). </summary>
	/// <param name="l"> the MouseListener to add. </param>
	public virtual void addMouseListener(MouseListener l)
	{
		base.addMouseListener(l);
	}

	/// <summary>
	/// Adds a new row of data after all the existing rows.  The data object passed
	/// in must be valid for the current table model -- but this method will not do any checking to verify that. </summary>
	/// <param name="o"> the object to add to the table model. </param>
	public virtual void addRow(object o)
	{
		__lastRowSelected = -1;
		((JWorksheet_AbstractTableModel)getModel()).addRow(o);
		((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();

		int rows = getRowCount();
		if (__selectionMode == __EXCEL_SELECTION)
		{
			int cols = getColumnCount();
			setSelectionMode(ListSelectionModel.MULTIPLE_INTERVAL_SELECTION);
			setCellSelectionEnabled(true);
			JWorksheet_RowSelectionModel r = new JWorksheet_RowSelectionModel(rows, cols);
			r.setPartner(__partner);
			if (!__selectable)
			{
				r.setSelectable(false);
			}

			JWorksheet_ColSelectionModel c = new JWorksheet_ColSelectionModel();
			r.setColSelectionModel(c);
			c.setRowSelectionModel(r);
			setSelectionModel(r);
			setOneClickRowSelection(__oneClickRowSelection);
			getColumnModel().setSelectionModel(c);
		}

		if (__listRowHeader != null)
		{
			adjustListRowHeaderSize(__ROW_ADDED);
		}
		notifyAllWorksheetListeners(__ROW_ADDED, (getRowCount() - 1));
	}

	/// <summary>
	/// Adds a sort listener. </summary>
	/// <param name="listener"> the sort listener to add. </param>
	public virtual void addSortListener(JWorksheet_SortListener listener)
	{
		if (__sortListeners == null)
		{
			__sortListeners = new List<JWorksheet_SortListener>();
		}
		__sortListeners.Add(listener);
	}

	/// <summary>
	/// Adjusts the cell attributes and alternate texts following a row being deleted,
	/// a row being inserted within the table, or a column being deleted.  This is
	/// so that all rows maintain the same attributes even when a new row is inserted or a row is deleted.  <para>
	/// For example, consider a table had three rows and row 2 had cell attributes so 
	/// </para>
	/// that all of its cells were red, and rows 1 and 3 had no attributes and their cells appear normal.<para>
	/// If a new row is inserted after the first row, this method makes sure that the
	/// cell attributes that were previously on the second row now appear on the third 
	/// row.  If the first row is deleted, this makes sure that the second row's 
	/// </para>
	/// attributes follow it as it becomes the new first row.<para>
	/// All the above is done for cell alternate text, as well.
	/// </para>
	/// </summary>
	/// <param name="adjustment"> one of __ROW_ADDED, __ROW_DELETED or __COL_DELETED. </param>
	/// <param name="row"> the row that was inserted or deleted, or the column that was deleted. </param>
	private void adjustCellAttributesAndText(int adjustment, int pos)
	{
		bool compact = false;
		int attrLength = __attrRows.Length;
		int altLength = __altTextRows.Length;
		int row = pos;
		int col = pos;
		if (adjustment == __ROW_ADDED)
		{
			for (int i = 0; i < attrLength; i++)
			{
				if (__attrRows[i] >= row)
				{
					__attrRows[i]++;
				}
			}
			for (int i = 0; i < altLength; i++)
			{
				if (__altTextRows[i] >= row)
				{
					__attrRows[i]++;
					__altTextRows[i]++;
				}
			}
		}
		else if (adjustment == __ROW_DELETED)
		{
			for (int i = 0; i < attrLength; i++)
			{
				if (__attrRows[i] == row)
				{
					__attrCols[i] = -1;
					__attrRows[i] = -1;
					__cellAttrs[i] = null;
					__attrCount--;
				}
				else if (__attrRows[i] > row)
				{
					__attrRows[i] = __attrRows[i] - 1;
				}
			}
			for (int i = 0; i < altLength; i++)
			{
				if (__altTextRows[i] == row)
				{
					__altTextCols[i] = -1;
					__altTextRows[i] = -1;
					__altText[i] = null;
					__altTextCount--;
				}
				else if (__altTextRows[i] > row)
				{
					__altTextRows[i] = __altTextRows[i] - 1;
				}
			}

			compact = true;
		}
		else if (adjustment == __COL_DELETED)
		{
			for (int i = 0; i < attrLength; i++)
			{
				if (__attrCols[i] == col)
				{
					__attrCols[i] = -1;
					__attrRows[i] = -1;
					__cellAttrs[i] = null;
					__attrCount--;
				}
			}
			for (int i = 0; i < altLength; i++)
			{
				if (__attrCols[i] == col)
				{
					__altTextCols[i] = -1;
					__altTextRows[i] = -1;
					__altText[i] = null;
					__altTextCount--;
				}
			}

			compact = true;
		}

		if (compact)
		{
			compactAttrArrays();
			compactAltTextArrays();
		}
	}

	/// <summary>
	/// Adjusts the row header in response to a change in the table size.
	/// </para>
	/// </summary>
	/// <param name="adjustment"> the kind of change that happened.  Must be one of:<para>
	/// <ul><li>__ROW_ADDED - if a single row was added</li>
	/// <li>__ROW_DELETED - if a single row was deleted</li>
	/// <li>__DATA_RESET - if more than a single row changed</li>
	/// </ul> </param>
	private void adjustListRowHeaderSize(int adjustment)
	{
		int size = __listRowHeader.getListSize();
		if (adjustment == __ROW_ADDED)
		{
			__listRowHeader.add("" + (size + 1));
		}
		else if (adjustment == __ROW_DELETED)
		{
			__listRowHeader.remove(size - 1);
		}
		else if (adjustment == __DATA_RESET)
		{
			__listRowHeader.removeAll();
			int rows = getRowCount();
			// Simple row header is just the number of the row
			IList<string> v = new List<string>();
			for (int i = 0; i < rows; i++)
			{
				v.Add("" + (i + 1));
			}
			__listRowHeader = new SimpleJList(v);
			__listRowHeader.addKeyListener(this);
			__listRowHeader.setFixedCellWidth(50);
			__listRowHeader.setFixedCellHeight(getRowHeight());
			Font font = new Font(__rowHeaderFontName, __rowHeaderFontStyle, __rowHeaderFontSize);
			__listRowHeader.setCellRenderer(new JWorksheet_RowHeader(this, font, __rowHeaderColor));
			__listRowHeader.addMouseListener(this);
			__worksheetRowHeader = null;
			Container p = getParent();
			if (p is JViewport)
			{
				Container gp = p.getParent();
				if (gp is JScrollPane)
				{
					JScrollPane jsp = (JScrollPane)gp;
					__listRowHeader.setBackground(jsp.getBackground());
					jsp.setRowHeaderView(__listRowHeader);
				}
			}
		}
	}

	/// <summary>
	/// Responds to adjustment events caused by JScrollPanes being scrolled.
	/// This method does nothing on Windows machines, but on UNIX machines it forces a
	/// repaint of the JScrollPane for every scroll adjustment.  This is because on 
	/// certain exceed connections, scrolling a large worksheet was resulting in the
	/// worksheet becoming unreadable.  No exceed settings could be found that would 
	/// solve this, so the following is done.  There is a slight performance hit, and
	/// the display scrolls a little less smoothly, but at least the data are legible. </summary>
	/// <param name="event"> the AdjustmentEvent that occurred. </param>
	public virtual void adjustmentValueChanged(AdjustmentEvent @event)
	{
		if (IOUtil.isUNIXMachine())
		{
			getParent().repaint();
		}
	}

	/// <summary>
	/// Applies cell attributes to the specified cell. </summary>
	/// <param name="cell"> the cell to which to apply attributes </param>
	/// <param name="ca"> the attributes to apply.  If null, then the default JTable settings will be used. </param>
	/// <param name="selected"> whether this cell is selected or not. </param>
	private Component applyCellAttributes(Component cell, JWorksheet_CellAttributes ca, bool selected)
	{
		if (ca == null)
		{
	//		Message.printStatus(1, "", "NULL NULL - " + selected);
			Color bg = null;
			Color fg = null;
			if (selected)
			{
				bg = UIManager.getColor("Table.selectionBackground");
				fg = UIManager.getColor("Table.selectionForeground");
			}
			else
			{
				bg = UIManager.getColor("Table.background");
				fg = UIManager.getColor("Table.foreground");
			}
			cell.setBackground(bg);
			cell.setForeground(fg);
			cell.setEnabled(true);
	//		cell.setFont(UIManager.getFont("Table.font"));
			cell.setFont(__cellFont);
			if (cell is JLabel)
			{
				((JLabel)cell).setVerticalAlignment(SwingConstants.CENTER);
			}
			return cell;
		}
		else
		{
	//		Message.printStatus(1, "", "NOT NULL - " + selected);
	//		Message.printStatus(1, "", ca.toString());
			if (ca.font != null)
			{
				cell.setFont(ca.font);
			}
			else
			{
				if (!string.ReferenceEquals(ca.fontName, null) && ca.fontSize != -1 && ca.fontStyle != -1)
				{
					cell.setFont(new Font(ca.fontName, ca.fontSize, ca.fontStyle));
				}
				else
				{
					cell.setFont(UIManager.getFont("Table.font"));
				}
			}
			if (!selected)
			{
				if (ca.backgroundColor != null)
				{
					cell.setBackground(ca.backgroundColor);
				}
				else
				{
					cell.setBackground(UIManager.getColor("Table.background"));
				}

				if (ca.foregroundColor != null)
				{
					cell.setForeground(ca.foregroundColor);
				}
				else
				{
					cell.setForeground(UIManager.getColor("Table.foreground"));
				}
			}
			else
			{
				if (ca.backgroundSelectedColor != null)
				{
					cell.setBackground(ca.backgroundSelectedColor);
				}
				else
				{
					cell.setBackground(UIManager.getColor("Table.selectionBackground"));
				}

				if (ca.foregroundSelectedColor != null)
				{
					cell.setForeground(ca.foregroundSelectedColor);
				}
				else
				{
					cell.setForeground(UIManager.getColor("Table.selectionForeground"));
				}
			}

			if (cell is JComponent)
			{
				if (ca.borderColor != null)
				{
					((JComponent)cell).setBorder(new LineBorder(ca.borderColor));
				}
				else
				{
					((JComponent)cell).setBorder(null);
				}
			}
			if (cell is JLabel)
			{
				if (ca.horizontalAlignment != -1)
				{
					((JLabel)cell).setHorizontalAlignment(ca.horizontalAlignment);
				}
				if (ca.verticalAlignment != -1)
				{
					((JLabel)cell).setVerticalAlignment(ca.verticalAlignment);
				}
				else
				{
					((JLabel)cell).setVerticalAlignment(SwingConstants.CENTER);
				}
			}

			cell.setEnabled(ca.enabled);

			return cell;
		}
	}

	/// <summary>
	/// Checks to see whether any columns have been removed </summary>
	/// <returns> true if any columns have been removed, false if not.
	/// TODO (JTS - 2004-11-19) I really dislike this method name.  But what's better to fit the style:
	/// 
	/// if (worksheet.XXXXX())
	/// 
	/// ?
	/// 
	/// if (worksheet.hasRemovedColumns()) {
	/// 
	/// if (worksheet.isMissingColumns()) {
	/// 
	/// ???? </returns>
	public virtual bool areColumnsRemoved()
	{
		int size = __columnNames.Length;
		for (int i = 0; i < size; i++)
		{
			if (__columnRemoved[i] == true)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Tries to determine column widths that look best.  It does this by looking at
	/// the column name and finding the largest token (as split out by spaces, commas
	/// and newlines), and then forming the rest of the tokens into lines of text no
	/// larger than the longest one.  <para>
	/// This method should be used for tables that are on GUIs that are visible.  
	/// For other tables (tables being created behind the scenes) get a Graphics object
	/// and pass it to the other calculateColumnWidths method.
	/// </para>
	/// </summary>
	public virtual void calculateColumnWidths()
	{
		calculateColumnWidths(0, getGraphics());
	}

	/// <summary>
	/// Tries to determine column widths that look best.  It does this by looking at
	/// the column name and finding the largest token (as split out by spaces, commas
	/// and newlines), and then forming the rest of the tokens into lines of text no
	/// larger than the longest one.  Alternately, a minimum width can be set so that
	/// even if the length of the longest token (in pixels drawn on-screen) is less
	/// than this width, the column will be padded out to that many pixels wide. <para>
	/// This method should be used for tables that are on GUIs that are visible.  
	/// For other tables (tables being created behind the scenes) get a Graphics object
	/// and pass it to the other calculateColumnWidths method.
	/// </para>
	/// </summary>
	/// <param name="minWidth"> the mininimum number of pixels wide the column should be. </param>
	public virtual void calculateColumnWidths(int minWidth)
	{
		calculateColumnWidths(minWidth, getGraphics());
	}

	/// <summary>
	/// Tries to determine column widths that look best.  It does this by looking at
	/// the column name and finding the largest token (as split out by spaces, commas
	/// and newlines), and then forming the rest of the tokens into lines of text no
	/// larger than the longest one.  Alternately, a minimum width can be set so that
	/// even if the length of the longest token (in pixels drawn on-screen) is less
	/// than this width, the column will be padded out to that many pixels wide. </summary>
	/// <param name="minWidth"> the mininimum number of pixels wide the column should be. </param>
	/// <param name="g"> a Graphics context to use for determining the width of certain 
	/// Strings in pixels.  If the worksheet is on a visible gui, 
	/// worksheet.getGraphics() should be passed in. </param>
	public virtual void calculateColumnWidths(int minWidth, Graphics g)
	{
		calculateColumnWidths(minWidth, 0, g);
	}

	/// <summary>
	/// Tries to determine column widths that look best.  It does this by looking at
	/// the column name and finding the largest token (as split out by spaces, commas
	/// and newlines), and then forming the rest of the tokens into lines of text no
	/// larger than the longest one.  The minimum width, if greater than 0, 
	/// means that even if the length of the longest token (in pixels drawn on-screen) 
	/// is less than this width, the column will be padded out to that many pixels 
	/// wide. In addition, if rows is greater than 0, then the data in the column 
	/// in rows from 0 to rows (or getRowCount(), if less than rows) will be used
	/// for determining how wide the column should be.  If the width of the widest 
	/// data item is greater than the minimum width and the width of the widest token
	/// in the column name, then it will be used. </summary>
	/// <param name="minWidth"> the mininimum number of pixels wide the column should be. </param>
	/// <param name="rows"> the number of rows to look through and check data for widths.  
	/// <b>Caution:</b> while checking the data to determine a column width can be
	/// effective for properly sizing a column, it can also be very inefficient and performance will be slow. </param>
	/// <param name="g"> a Graphics context to use for determining the width of certain 
	/// Strings in pixels.  If the worksheet is on a visible gui, 
	/// worksheet.getGraphics() should be passed in. </param>
	public virtual void calculateColumnWidths(int minWidth, int rows, Graphics g)
	{
		calculateColumnWidths(minWidth, rows, null, g);
	}

	public virtual void calculateColumnWidths(int minWidth, int rows, int[] skipCols, Graphics g)
	{
		int count = __columnNames.Length;
		string name = null;
		TableColumn tc = null;
		int size = 0;
		int maxLines = 1;
		int lineCount = 0;
		string[] names = new string[count];

		// first loop through all the columns, building a nicely-sized
		// column name and counting the number of lines the biggest column name will occupy.
		for (int i = 0; i < count; i++)
		{
			if (!__columnRemoved[i])
			{
				name = getColumnName(getVisibleColumn(i));
				name = determineName(name, minWidth, g);

				lineCount = countLines(name);
				if (lineCount > maxLines)
				{
					maxLines = lineCount;
				}

				names[i] = name;
			}
		}

		bool shouldDo = ((JWorksheet_AbstractTableModel)getModel()).shouldDoGetConsecutiveValueAt();

		((JWorksheet_AbstractTableModel)getModel()).shouldDoGetConsecutiveValueAt(true);

		// then loop through all the columns, making sure that there are an 
		// equal number of lines in every column name (and if not, pad out
		// the column name with newlines at its beginning), and then putting
		// the name and the width into the column.
		bool skip = false;
		int dataWidth = 0;
		for (int i = 0; i < count; i++)
		{
			skip = false;
			if (skipCols != null)
			{
				for (int j = 0; j < skipCols.Length; j++)
				{
					if (i == skipCols[j])
					{
						skip = true;
					}
				}
			}
			if (!skip && !__columnRemoved[i])
			{
				StopWatch sw = new StopWatch();
				sw.clear();
				sw.start();
				((JWorksheet_AbstractTableModel)getModel()).shouldResetGetConsecutiveValueAt(true);

				name = names[i];
				lineCount = countLines(name);
				for (int j = lineCount; j < maxLines; j++)
				{
					name = "\n" + name;
				}

				setColumnName(i, name);
				size = getColumnNameFitSize(name, g);
				if (size < minWidth)
				{
					size = minWidth;
				}
				if (rows > 0)
				{
					dataWidth = getDataMaxWidth(i, rows, g);
					if (dataWidth > size)
					{
						size = dataWidth;
					}
				}
				tc = getColumnModel().getColumn(getVisibleColumn(i));
				tc.setPreferredWidth(size);
				sw.stop();
			}
		}

		((JWorksheet_AbstractTableModel)getModel()).shouldDoGetConsecutiveValueAt(shouldDo);
		((JWorksheet_AbstractTableModel)getModel()).shouldResetGetConsecutiveValueAt(true);
	}

	// TODO sam 2017-04-01 evaluate whether to use something other than DataTable
	// in order to be more generic.
	// TODO sam 2017-04-01 need to figure out how to get precision from the table model.
	/// <summary>
	/// Copy the selected cells (or all none are selected) to a new worksheet,
	/// add a column for the statistic type, calculate the statistics,
	/// and display in a new worksheet.
	/// This method handles generic JWorksheets.
	/// If more specific behavior is needed (for example time series data with potentially
	/// inconsistent missing data values), then create a JWorksheet and set the properties
	/// <pre>
	/// JWorksheet.AllowCalculateStatistics=true
	/// JWorksheet.DelegateCalculateStatistics=true
	/// </pre>
	/// The latter property will tell this class to not process the "Calculate Statistics"
	/// action event and let another registered ActionListener handle.
	/// If the Calculate Statistics functionality is enabled and not delegated,
	/// then this method is called.
	/// This code is substantially copied from JWorksheet_CopyPasteAdapter.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void calculateStatistics() throws Exception
	private void calculateStatistics()
	{
		ProgressJDialog progressDialog = null;
		try
		{
			int numSelectedCols = getSelectedColumnCount();
			int numSelectedRows = getSelectedRowCount();
			int[] selectedRows = getSelectedRows();
			int[] selectedCols = getSelectedColumns();
			int[] visibleCols = new int[selectedCols.Length];
			for (int icol = 0; icol < selectedCols.Length; icol++)
			{
				visibleCols[icol] = getVisibleColumn(selectedCols[icol]);
			}

			// No data to process
			if (numSelectedCols == 0 || numSelectedRows == 0)
			{
				JGUIUtil.setWaitCursor(getHourglassJFrame(), false);
				return;
			}

			/// <summary>
			/// TODO sam 2017-04-01 the following may or may not be helpful.
			///  - the statistics code implemented below processes the bounding block rather than specific selections
			/// if (numSelectedRows == 1 && numSelectedCols == 1) {
			/// // Trivial case -- this will always be a successful copy.  This case is just a placeholder.
			/// }
			/// else if (numSelectedRows == 1) {
			/// // The rows are valid; the only thing left to check is whether the columns are contiguous.
			/// if (!areCellsContiguous(numSelectedRows, selectedRows, numSelectedCols, visibleCols)) {
			///		showCopyErrorDialog("You must select a contiguous block of columns.");
			///		return;
			/// }
			/// }
			/// else if (numSelectedCols == 1) {
			/// // The cols are valid; the only thing left to check is whether the rows are contiguous.
			/// if (!areCellsContiguous(numSelectedRows, selectedRows, numSelectedCols, visibleCols)) {
			///		showCopyErrorDialog("You must select a contiguous block of rows.");
			///		return;
			/// }
			/// }
			/// else {
			/// // There are multiple rows selected and multiple columns selected.  Make sure both are contiguous.
			/// if (!areCellsContiguous(numSelectedRows, selectedRows, numSelectedCols, visibleCols)) {
			///		showCopyErrorDialog("You must select a contiguous block\nof rows and columns.");
			///		return;
			/// }			
			/// }
			/// </summary>

			int numColumns = getColumnCount();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") Class[] classes = new Class[numColumns];
			Type[] classes = new Type[numColumns];
			bool[] canCalcStats = new bool[numColumns];
			// Arrays for statistics
			int[] count = new int[numColumns];
			// Allocate arrays for all columns, but only some will be used
			// Use highest precision types and then cast to lower if needed
			// For floating point results...
			double[] min = new double[numColumns];
			double[] max = new double[numColumns];
			double[] sum = new double[numColumns];
			// For integer results...
			long[] imin = new long[numColumns];
			long[] imax = new long[numColumns];
			long[] isum = new long[numColumns];
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				classes[icol] = getColumnClass(getAbsoluteColumn(selectedCols[icol]));
				//classes[icol] = getColumnClass(getAbsoluteColumn(icol));
				canCalcStats[icol] = false;
				count[icol] = 0;
				sum[icol] = Double.NaN;
				min[icol] = Double.NaN;
				max[icol] = Double.NaN;
				isum[icol] = 0;
				imin[icol] = long.MaxValue;
				imax[icol] = long.MinValue;
			}

			// Progress dialog to show how cells are processed - cellCount is used to increment
			progressDialog = new ProgressJDialog(getHourglassJFrame(), "Copy progress", 0, (numSelectedRows * numSelectedCols));

			int cellCount = 1;

			progressDialog.setVisible(true);

			// Initialize the list of table fields that contains a leftmost column "Statistic".
			IList<TableField> tableFieldList = new List<TableField>();
			tableFieldList.Add(new TableField(TableField.DATA_TYPE_STRING, "Statistic", -1, -1));
			// Add columns for the selected columns
			bool copyHeader = true;
			if (copyHeader)
			{
				for (int icol = 0; icol < numSelectedCols; icol++)
				{
					if (classes[icol] == typeof(Double))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_DOUBLE, getColumnName(selectedCols[icol], true), -1, -1));
						canCalcStats[icol] = true;
					}
					else if (classes[icol] == typeof(Float))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_FLOAT, getColumnName(selectedCols[icol], true), -1, -1));
						canCalcStats[icol] = true;
					}
					else if (classes[icol] == typeof(Integer))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_INT, getColumnName(selectedCols[icol], true), -1, -1));
						canCalcStats[icol] = true;
					}
					else if (classes[icol] == typeof(Long))
					{
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_LONG, getColumnName(selectedCols[icol], true), -1, -1));
						canCalcStats[icol] = true;
					}
					else
					{
						// Add a string class
						tableFieldList.Add(new TableField(TableField.DATA_TYPE_STRING, getColumnName(selectedCols[icol], true), -1, -1));
						canCalcStats[icol] = false;
					}
				}
			}

			// Create the table
			DataTable table = new DataTable(tableFieldList);

			JWorksheet_AbstractTableModel tableModel = getTableModel();
			// Transfer the data from the worksheet to the subset table
			object cellContents;
			for (int irow = 0; irow < numSelectedRows; irow++)
			{
				TableRecord rec = table.emptyRecord();
				rec.setFieldValue(0, ""); // Blanks for most rows until the statistics added at the end
				for (int icol = 0; icol < numSelectedCols; icol++)
				{
					progressDialog.setProgressBarValue(cellCount++);
					cellContents = tableModel.getValueAt(selectedRows[irow],selectedCols[icol]);
					rec.setFieldValue((icol + 1), cellContents);
					// Calculate the statistics
					if (canCalcStats[icol])
					{
						// Column type allows calculating statistics so do some basic math
						if (cellContents != null)
						{
							if ((classes[icol] == typeof(Double)))
							{
								double? d = (double?)cellContents;
								if (!d.isNaN())
								{
									++count[icol];
									// Sum, used directly and for mean
									if (double.IsNaN(sum[icol]))
									{
										sum[icol] = d.Value;
									}
									else
									{
										sum[icol] += d.Value;
									}
									// Min statistic
									if (double.IsNaN(min[icol]))
									{
										min[icol] = d.Value;
									}
									else if (d.Value < min[icol])
									{
										min[icol] = d.Value;
									}
									// Max statistic
									if (double.IsNaN(max[icol]))
									{
										max[icol] = d.Value;
									}
									else if (d.Value > max[icol])
									{
										max[icol] = d.Value;
									}
								}
							}
							else if ((classes[icol] == typeof(Float)))
							{
								float? f = (float?)cellContents;
								if (!f.isNaN())
								{
									++count[icol];
									// Sum, used directly and for mean
									if (double.IsNaN(sum[icol]))
									{
										sum[icol] = f;
									}
									else
									{
										sum[icol] += f;
									}
									// Min statistic
									if (double.IsNaN(min[icol]))
									{
										min[icol] = f;
									}
									else if (f < min[icol])
									{
										min[icol] = f;
									}
									// Max statistic
									if (double.IsNaN(max[icol]))
									{
										max[icol] = f;
									}
									else if (f > max[icol])
									{
										max[icol] = f;
									}
								}
							}
							else if ((classes[icol] == typeof(Integer)))
							{
								int? i = (int?)cellContents;
								// No concept of NaN so previous null check is
								// main check for missing
								++count[icol];
								// Sum, used directly and for mean
								sum[icol] += i;
								// Min statistic
								if (imin[icol] == long.MaxValue)
								{
									imin[icol] = i;
								}
								else if (i < imin[icol])
								{
									imin[icol] = i;
								}
								// Max statistic
								if (imax[icol] == long.MinValue)
								{
									imax[icol] = i;
								}
								else if (i > imax[icol])
								{
									imax[icol] = i;
								}
							}
							else if ((classes[icol] == typeof(Long)))
							{
								long? i = (long?)cellContents;
								// No concept of NaN so previous null check is
								// main check for missing
								++count[icol];
								// Sum, used directly and for mean
								sum[icol] += i;
								// Min statistic
								if (imin[icol] == long.MaxValue)
								{
									imin[icol] = i.Value;
								}
								else if (i.Value < imin[icol])
								{
									imin[icol] = i.Value;
								}
								// Max statistic
								if (imax[icol] == long.MinValue)
								{
									imax[icol] = i.Value;
								}
								else if (i.Value > imax[icol])
								{
									imax[icol] = i.Value;
								}
							}
						}
					}
				}
				table.addRecord(rec);
			}
			// Add statistics at the bottom
			TableRecord rec = table.emptyRecord();
			rec.setFieldValue(0, "Count");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				// TODO sam 2017-04-01 Worksheet should handle case even when object
				// is a different type than the column, but this is generally not done in tables
				if (canCalcStats[icol])
				{
					rec.setFieldValue((icol + 1), new int?(count[icol]));
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Mean");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if ((count[icol] > 0) && !double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(sum[icol]) / count[icol]);
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if ((count[icol] > 0) && !double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(sum[icol]) / count[icol]);
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						if (count[icol] > 0)
						{
							rec.setFieldValue((icol + 1), new long?(isum[icol]) / count[icol]);
						}
					}
					else if (classes[icol] == typeof(Integer))
					{
						if (count[icol] > 0)
						{
							rec.setFieldValue((icol + 1), new int?((int)isum[icol]) / count[icol]);
						}
					}
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Min");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if (!double.IsNaN(min[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(min[icol]));
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if (!double.IsNaN(min[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(min[icol]));
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						if (imin[icol] != long.MaxValue)
						{
							rec.setFieldValue((icol + 1), new long?(imin[icol]));
						}
					}
					else if (classes[icol] == typeof(Integer))
					{
						if (imin[icol] != long.MaxValue)
						{
							rec.setFieldValue((icol + 1), new int?((int)imin[icol]));
						}
					}
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Max");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if (!double.IsNaN(max[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(max[icol]));
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if (!double.IsNaN(max[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(max[icol]));
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						if (imax[icol] != long.MinValue)
						{
							rec.setFieldValue((icol + 1), new long?(imax[icol]));
						}
					}
					else if (classes[icol] == typeof(Integer))
					{
						if (imax[icol] != long.MinValue)
						{
							rec.setFieldValue((icol + 1), new long?(imax[icol]));
						}
					}
				}
			}
			table.addRecord(rec);
			rec = table.emptyRecord();
			rec.setFieldValue(0, "Sum");
			for (int icol = 0; icol < numSelectedCols; icol++)
			{
				if (canCalcStats[icol])
				{
					if (classes[icol] == typeof(Double))
					{
						if (!double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new double?(sum[icol]));
						}
					}
					else if (classes[icol] == typeof(Float))
					{
						if (!double.IsNaN(sum[icol]))
						{
							rec.setFieldValue((icol + 1), new float?(sum[icol]));
						}
					}
					else if (classes[icol] == typeof(Long))
					{
						rec.setFieldValue((icol + 1), new long?(isum[icol]));
					}
					else if (classes[icol] == typeof(Integer))
					{
						rec.setFieldValue((icol + 1), new int?((int)isum[icol]));
					}
				}
			}
			table.addRecord(rec);

			DataTable_TableModel dttm = new DataTable_TableModel(table);
			DataTable_CellRenderer scr = new DataTable_CellRenderer(dttm);
			PropList frameProps = new PropList("");
			frameProps.set("Title","Table Statistics");
			PropList worksheetProps = new PropList("");
			// The following will be default center on its parent and be shown in front
			TableModel_JFrame f = new TableModel_JFrame(dttm, scr, frameProps, worksheetProps);
			Component parent = SwingUtilities.getWindowAncestor(this);
			JGUIUtil.center(f,parent);
			f.toFront(); // This does not seem to always work
			f.setAlwaysOnTop(true); // TODO sam 2017-04-01 don't like to do this but seems necessary
		}
		catch (Exception e)
		{
			(new ResponseJDialog(getHourglassJFrame(), "Error", "Error calculating statistics.", ResponseJDialog.OK)).response();
			Message.printWarning(2, "", e);
		}
		finally
		{
			if (progressDialog != null)
			{
				progressDialog.dispose();
			}
		}
	}

	/// <summary>
	/// Programmatically stops any cell editing that is taking place.  Cell editing
	/// happens when a user double-clicks in a cell or begins typing in a cell.  
	/// This method will stop the editing and will NOT accept the data the user has
	/// entered up to this method call.  To accept the data the user has already entered, use stopEditing().
	/// </summary>
	public virtual void cancelEditing()
	{
		if (getCellEditor() != null)
		{
			getCellEditor().cancelCellEditing();
		}
	}

	/// <summary>
	/// Returns whether the specified cell has any attributes set. </summary>
	/// <param name="row"> the row of the cell to check </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column of the cell to check </param>
	/// <returns> true if the cell has any attributes set, false if not. </returns>
	public virtual bool cellHasAttributes(int row, int absoluteColumn)
	{
		if (getCellAttributes(row, absoluteColumn) == null)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Clears the existing data from the worksheet and leaves it empty.
	/// </summary>
	public virtual void clear()
	{
		__lastRowSelected = -1;
		if (getRowCount() > 0)
		{
			((JWorksheet_AbstractTableModel)getModel()).clear();
			((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();
			if (__listRowHeader != null)
			{
				adjustListRowHeaderSize(__DATA_RESET);
			}
		}
	}

	/// <summary>
	/// Returns the <b>absolute</b> column at the specified point, or -1 if none are at that point. </summary>
	/// <param name="point"> the Point at which to return the column. </param>
	/// <returns> the <b>absolute</b> column at the specified point, or -1 if none are at that point. </returns>
	public virtual int columnAtPoint(Point point)
	{
		return base.columnAtPoint(point);
	}

	/// <summary>
	/// Compacts the arrays used for storing alternate cell text so that 
	/// any used parts of the arrays are at the end.
	/// </summary>
	private void compactAltTextArrays()
	{
		int hit = -1;
		int length = __altTextCols.Length;
	//	String c = "";
	//	for (int i = 0; i < length; i++) {
	//		c = c + __altTextCols[i] + "  ";
	//	}
	//	Message.printStatus(1, "", c);
		for (int i = 0; i < __altTextCount; i++)
		{
			for (int j = i; j < length; j++)
			{
				if (__altTextCols[j] > -1 && __altTextRows[j] > -1)
				{
					hit = j;
					j = length + 1;
				}
			}
			if (i != hit)
			{
				__altTextCols[i] = __altTextCols[hit];
				__altTextRows[i] = __altTextRows[hit];
				__altText[i] = __altText[hit];

				__altTextCols[hit] = -1;
				__altTextRows[hit] = -1;
				__altText[hit] = null;
			}
		}
	//	c = "";
	//	for (int i = 0; i < length; i++) {
	//		c = c + __altTextCols[i] + "  ";
	//	}
	//	Message.printStatus(1, "", c);	
	}

	/// <summary>
	/// Compacts the arrays used for storing cell attribute information so that 
	/// any used parts of the arrays are at the end.
	/// </summary>
	private void compactAttrArrays()
	{
		int hit = -1;
		int length = __attrCols.Length;
	//	String c = "";
	//	for (int i = 0; i < length; i++) {
	//		c = c + __attrCols[i] + "  ";
	//	}
	//	Message.printStatus(1, "", c);
		for (int i = 0; i < __attrCount; i++)
		{
			for (int j = i; j < length; j++)
			{
				if (__attrCols[j] > -1 && __attrRows[j] > -1)
				{
					hit = j;
					j = length + 1;
				}
			}
			if (i != hit)
			{
				__attrCols[i] = __attrCols[hit];
				__attrRows[i] = __attrRows[hit];
				__cellAttrs[i] = __cellAttrs[hit];

				__attrCols[hit] = -1;
				__attrRows[hit] = -1;
				__cellAttrs[hit] = null;
			}
		}
	//	c = "";
	//	for (int i = 0; i < length; i++) {
	//		c = c + __attrCols[i] + "  ";
	//	}
	//	Message.printStatus(1, "", c);	
	}

	/// <summary>
	/// Checks to see whether the worksheet contains the given object.  This method
	/// should only be used with table models that store an object in each row.
	/// It checks through each row and compares the object stored at that row with
	/// the specified object using '<tt>.equals()</tt>'.  If the objects match, true is
	/// returned.  Otherwise, false is returned. </summary>
	/// <param name="o"> the object that should be compared to the objects stored in the table.
	/// Null is an acceptable value to pass in, in which case the worksheet will search
	/// for whether the table contains any null values. </param>
	/// <returns> true if the object could be found in the worksheet.  False if it could not. </returns>
	public virtual bool contains(object o)
	{
		if (!(getModel() is JWorksheet_AbstractRowTableModel))
		{
			return false;
		}

		int size = getRowCount();
		if (o != null)
		{
			for (int i = 0; i < size; i++)
			{
				if (o.Equals(getRowData(i)))
				{
					return true;
				}
			}
		}
		else
		{
			for (int i = 0; i < size; i++)
			{
				if (getRowData(i) == null)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Converts a column name with newlines into one without. </summary>
	/// <param name="s"> the column name to convert. </param>
	/// <returns> a column name without newlines or back-to-back spaces. </returns>
	public static string convertColumnName(string s)
	{
		s = StringUtil.replaceString(s, "\n", " ");
		while (s.IndexOf("  ", StringComparison.Ordinal) > -1)
		{
			s = StringUtil.replaceString(s, "  ", " ");
		}
		s = s.Trim();

		return s;
	}

	/// <summary>
	/// Copies the table to the clipboard in HTML format.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void copyAsHTML() throws Exception
	public virtual void copyAsHTML()
	{
		copyAsHTML(0, getRowCount() - 1);
	}

	/// <summary>
	/// Copies the specified rows to the clipboard in HTML format. </summary>
	/// <param name="firstRow"> the firstRow to copy to the clipboard. </param>
	/// <param name="lastRow"> the lastRow to be copied. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void copyAsHTML(int firstRow, int lastRow) throws Exception
	public virtual void copyAsHTML(int firstRow, int lastRow)
	{
		IOUtil.copyToClipboard(createHTML(null, null, firstRow, lastRow));
	}

	/// <summary>
	/// Copies the currently selected cells to the clipboard.
	/// </summary>
	public virtual void copyAllToClipboard()
	{
		copyAllToClipboard(false);
	}

	/// <summary>
	/// Copies the currently selected cells to the clipboard. </summary>
	/// <param name="includeHeader"> whether to include the header data for the copied cells
	/// in the first line of copied information.  </param>
	public virtual void copyAllToClipboard(bool includeHeader)
	{
		if (__copyPasteAdapter == null)
		{
			__copyPasteAdapter = new JWorksheet_CopyPasteAdapter(this);
		}
		__copyPasteAdapter.copyAll(includeHeader);
	}

	/// <summary>
	/// Copies the currently selected cells to the clipboard.
	/// </summary>
	public virtual void copyToClipboard()
	{
		copyToClipboard(false);
	}

	/// <summary>
	/// Copies the currently selected cells to the clipboard. </summary>
	/// <param name="includeHeader"> whether to include the header data for the copied cells
	/// in the first line of copied information.  </param>
	public virtual void copyToClipboard(bool includeHeader)
	{
		if (__copyPasteAdapter == null)
		{
			__copyPasteAdapter = new JWorksheet_CopyPasteAdapter(this);
		}
		__copyPasteAdapter.copy(includeHeader);
	}

	/// <summary>
	/// Used by calculateColumnWidths as a utility method for counting the number of lines in a String. </summary>
	/// <param name="name"> a column name. </param>
	/// <returns> the number of lines high the name is (i.e., how many lines it will occupy in the header). </returns>
	private int countLines(string name)
	{
		IList<string> v = StringUtil.breakStringList(name, "\n", 0);
		return v.Count;
	}

	/// <summary>
	/// This is used to make removing columns nicer.  In the original JTable code, if
	/// there are 3 columns ("a", "b" and "c") and the user removes column 2, then 
	/// they have to know that "c" is now column 2, not column 3.  Hassles for the
	/// programmer.  This way, if the programmer says to remove column 18, it ALWAYS
	/// refers to the column that was #18 in the table model when the table was first built.
	/// </summary>
	private void createColumnList()
	{
		int columnCount = getColumnCount();

		__columnRemoved = new bool[columnCount];
		__columnNames = new string[columnCount];
		__columnAlignments = new int[columnCount];

		TableColumn col = null;
		JWorksheet_DefaultTableCellEditor dtce = null;
		for (int i = 0; i < columnCount; i++)
		{
			__columnRemoved[i] = false;
			__columnNames[i] = getColumnName(i);
			__columnAlignments[i] = DEFAULT;

			col = getColumnModel().getColumn(i);
			dtce = new JWorksheet_DefaultTableCellEditor();
			col.setCellEditor(dtce);
		}
	}

	/// <summary>
	/// Creates an HTML representation of the table and possibly returns it. </summary>
	/// <param name="htmlWriter"> the HTMLWriter into which to create the html representation.
	/// This parameter can be null, in which case a filename can be specified.  If
	/// both the htmlWriter and the filename parameters are not null, the htmlWriter
	/// takes precedence and HTML will be written there. </param>
	/// <param name="filename"> the name of the file to write the table into.  If null and
	/// the htmlWriter parameter is null, the HTML will be generated in memory and 
	/// returned as a String.  If both the htmlWriter and the filename parameters 
	/// are not null, the htmlWriter takes precedence and HTML will be written there. </param>
	/// <param name="firstRow"> the first row of the table from which to begin turning data into HTML. </param>
	/// <param name="lastRow"> the last row of the table to be written to HTML. </param>
	/// <returns> a String representation of the table, if both the htmlWriter and 
	/// filename parameters are null.  If either are not null, null is returned. </returns>
	/// <exception cref="Exception"> if an error occurs, or if the table model is not derived from
	/// JWorksheet_AbstractTableModel, or if firstRow or lastRow are out of bounds. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String createHTML(RTi.Util.IO.HTMLWriter htmlWriter, String filename, int firstRow, int lastRow) throws Exception
	private string createHTML(HTMLWriter htmlWriter, string filename, int firstRow, int lastRow)
	{
		// the table model used in the worksheet absolute must be derived
		// from the JWorksheet_AbstractTableModel, in order to have access
		// to methods for formatting and aligning the row data.
		TableModel model = getModel();
		if (!(model is JWorksheet_AbstractTableModel))
		{
			throw new Exception("Table model not derived from JWorksheet_AbstractTableModel");
		}

		if (firstRow < 0)
		{
			throw new Exception("First row less than zero: " + firstRow);
		}
		if (lastRow >= getRowCount())
		{
			string plural = "s";
			if (getRowCount() == 1)
			{
				plural = "";
			}
			throw new Exception("Last row out of bounds: " + lastRow + "  (" + getRowCount() + " row" + plural + " in table)");
		}
		if (lastRow < firstRow)
		{
			throw new Exception("Last row (" + lastRow + ") less than first row (" + firstRow + ")");
		}

		JWorksheet_AbstractTableModel tableModel = (JWorksheet_AbstractTableModel)model;
		JWorksheet_AbstractExcelCellRenderer renderer = (JWorksheet_AbstractExcelCellRenderer)getCellRenderer();

		HTMLWriter html = null;
		if (htmlWriter == null)
		{
			// only create an HTML header if a filename was specified.  If
			// no filename was specified, then this is just creating a snippet of HTML code.
			bool createHeader = false;
			if (!string.ReferenceEquals(filename, null))
			{
				createHeader = true;
			}
			html = new HTMLWriter(filename, "Table Data", createHeader);
		}
		else
		{
			html = htmlWriter;
		}

		html.tableStartFloatLeft("border=1");

		int columnCount = getColumnCount();
		int curCol = 0;
		string s;

		// prints out the first row -- which is the name of all the visible columns in the table.
		html.tableRowStart();
		for (curCol = 0; curCol < getColumnCount(); curCol++)
		{
			s = getColumnName(curCol, true);
			html.tableHeaderStart("align=center");
			html.addText(s);
			html.tableHeaderEnd();
		}
		html.tableRowEnd();

		// mass of variables used in looping through all the cells 
		bool right = false;
		Type colClass = null;
		Color bg = null;
		Color fg = null;
		double d = 0;
		int align = 0;
		int i = 0;
		JWorksheet_CellAttributes ca = null;
		string alignCode = null;
		string altText = null;
		string format = null;

		// output all the specified rows of data.  Loop through row by row ...
		for (int curRow = firstRow; curRow <= lastRow; curRow++)
		{
			html.tableRowStart();

			// and then loop through column by column ...
			for (curCol = 0; curCol < columnCount; curCol++)
			{
				right = false;
				alignCode = "";

				// if the current cell has any attributes set, get them
				// and store the settings for the background color and the foreground color.
				ca = getCellAttributes(curRow, getAbsoluteColumn(curCol));
				if (ca != null)
				{
					bg = ca.backgroundColor;
					fg = ca.foregroundColor;
				}
				else
				{
					bg = null;
					fg = null;
				}

				// retrieve all the important data about this cell, such
				// as the data in it, the kind of data in it, the
				// way the data should be formatted, and whether there is any alternate text specified.
				colClass = tableModel.getColumnClass(getAbsoluteColumn(curCol));
				object o = getValueAt(curRow, curCol);
				format = renderer.getFormat(getAbsoluteColumn(curCol));
				align = getColumnAlignment(getAbsoluteColumn(curCol));
				altText = getCellAlternateText(curRow, getAbsoluteColumn(curCol));
	//			Message.printStatus(1, "", "[" + curRow + "][" + curCol + "]: " + "class: " + colClass 
	//				+ "  o: " + o + "  format: " + format + "  align: " + align + "  alt: " + altText);

				// o should probably never be null, but just in case set its cell's data to be equivalent to a 
				// blank string
				if (o == null)
				{
					s = " ";
				}
				// if there is any alternate text in the cell, that overrides whatever data is stored in the cell
				else if (!string.ReferenceEquals(altText, null))
				{
					s = altText;
				}
				// integer cells will have formatting information and should be aligned differently from
				// other cells.
				else if (colClass == typeof(Integer))
				{
					right = true;
					i = (Convert.ToInt32(o.ToString()));
					if (DMIUtil.isMissing(i))
					{
						s = " ";
					}
					else
					{
						s = StringUtil.formatString(o.ToString(), format);
					}
				}
				// double cells will have formatting information and
				// should be aligned differently from other cells.
				else if (colClass == typeof(Double))
				{
					right = true;
					d = (Convert.ToDouble(o.ToString()));
					if (DMIUtil.isMissing(d))
					{
						s = " ";
					}
					else
					{
						s = StringUtil.formatString(o.ToString(), format);
					}
				}
				// string cells will have formatting information.
				else if (colClass == typeof(string))
				{
					s = StringUtil.formatString(o.ToString(), format);
				}
				// all other cell data should just be turned into a string.
				else
				{
					s = o.ToString();
				}

				s = s.Trim();

				// if the cell's data was blank, pad it out to at least two spaces, so that the cell actually
				// shows up in the HTML version of the table.  HTML table cells with only " " or "" stored in
				// them are not even rendered in most HTML tables!
				if (s.Equals(""))
				{
					s = "  ";
				}

				// if no special alignment information has been set for this cell, determine the align code for
				// the cell from the cell data type specified above.
				if (align == DEFAULT)
				{
					if (right == false)
					{
						alignCode = "align=left";
					}
					else
					{
						alignCode = "align=right";
					}
				}
				// otherwise, if special alignment information has been set, that overrides all other alignment 
				// information and should be used.
				else
				{
					if (align == LEFT)
					{
						alignCode = "align=left";
					}
					else if (align == RIGHT)
					{
						alignCode = "align=right";
					}
					else if (align == CENTER)
					{
						alignCode = "align=center";
					}
				}

				// if the cell has a background color, then specify it in the cell opening tag.
				if (bg != null)
				{
					html.tableCellStart(alignCode + " " + "bgcolor=#" + MathUtil.decimalToHex(bg.getRed()) + "" + MathUtil.decimalToHex(bg.getGreen()) + "" + MathUtil.decimalToHex(bg.getBlue()));
				}
				else
				{
					html.tableCellStart(alignCode);
				}

				// if the cell has a foreground color, then open a font tag that changes the font color to the 
				// specified color
				if (fg != null)
				{
					html.fontStart("color=#" + MathUtil.decimalToHex(fg.getRed()) + "" + MathUtil.decimalToHex(fg.getGreen()) + "" + MathUtil.decimalToHex(fg.getBlue()));
				}

				// finally, put the text that should appear in the cell into the cell
				html.addText(s);

				// and if a font tag was set because of a foreground color, turn it off
				if (fg != null)
				{
					html.fontEnd();
				}
				html.tableCellEnd();
			}

			html.tableRowEnd();
		}
		html.tableEnd();

		// if these are true, the HTML is being generated in memory
		if (string.ReferenceEquals(filename, null) && htmlWriter == null)
		{
			return html.getHTML();
		}

		// if this is true, then a new html file was created and must be closed.
		if (htmlWriter == null)
		{
			html.closeFile();
		}
		return null;
	}

	/// <summary>
	/// Deletes a row from the table model.  <b>Note:</b> When deleting multiple rows,
	/// deleting from the last row to the first row is the easiest way.  Otherwise, 
	/// if row X is deleted, all the rows X+1, X+2, ... X+N will shift down one and
	/// need to be referenced by X, X+1, ... X+N-1.  <para>
	/// So to delete rows 5, 6, and 7 in a table, either of these pieces of code
	/// will work:<br><pre>
	/// worksheet.deleteRow(7);
	/// worksheet.deleteRow(6);
	/// worksheet.deleteRow(5);
	/// 
	///        (or)
	/// 
	/// worksheet.deleteRow(5);
	/// worksheet.deleteRow(5);
	/// worksheet.deleteRow(5);
	/// </pre>
	/// </para>
	/// </summary>
	/// <param name="row"> the row to delete from the table model.  Rows are numbered starting at 0. </param>
	public virtual void deleteRow(int row)
	{
		__lastRowSelected = -1;
		((JWorksheet_AbstractTableModel)getModel()).deleteRow(row);
		((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();

		if (__selectionMode == __EXCEL_SELECTION)
		{
			int rows = getRowCount();
			int cols = getColumnCount();
			setSelectionMode(ListSelectionModel.MULTIPLE_INTERVAL_SELECTION);
			setCellSelectionEnabled(true);
			JWorksheet_RowSelectionModel r = new JWorksheet_RowSelectionModel(rows, cols);
			r.setPartner(__partner);
			if (!__selectable)
			{
				r.setSelectable(false);
			}

			JWorksheet_ColSelectionModel c = new JWorksheet_ColSelectionModel();
			r.setColSelectionModel(c);
			c.setRowSelectionModel(r);
			setSelectionModel(r);
			setOneClickRowSelection(__oneClickRowSelection);
			getColumnModel().setSelectionModel(c);
		}

		if (__listRowHeader != null)
		{
			adjustListRowHeaderSize(__ROW_DELETED);
		}
		notifyAllWorksheetListeners(__ROW_DELETED, row);
		adjustCellAttributesAndText(__ROW_DELETED, row);
	}

	/// <summary>
	/// Deletes all the rows listed in the integer array. </summary>
	/// <param name="rows"> integer array for which each value in it is the number of a row
	/// to delete.  This array should be sorted in ascending value (i.e., array element
	/// X is a lower number than array element X+1) and cannot be null. </param>
	public virtual void deleteRows(int[] rows)
	{
		for (int i = (rows.Length - 1); i >= 0; i--)
		{
			deleteRow(rows[i]);
		}
	}

	/// <summary>
	/// Deselects all selected cells.
	/// </summary>
	public virtual void deselectAll()
	{
		notifyAllWorksheetListeners(__DESELECT_ALL, PRE_SELECTION_CHANGE);
		if (__selectionMode == __EXCEL_SELECTION)
		{
			((JWorksheet_RowSelectionModel)getSelectionModel()).clearSelection();
			refresh();
		}
		else
		{
			base.clearSelection();
		}
		notifyAllWorksheetListeners(__DESELECT_ALL, POST_SELECTION_CHANGE);
	}

	/// <summary>
	/// Deselects the specified row, leaving the other row selections alone.  If the
	/// row isn't currently-selected, nothing will happen. </summary>
	/// <param name="row"> the row to deselect. </param>
	public virtual void deselectRow(int row)
	{
		if (__selectionMode == __EXCEL_SELECTION)
		{
			int[] selectedRows = getSelectedRows();
			deselectAll();
			for (int i = 0; i < selectedRows.Length; i++)
			{
				if (selectedRows[i] != row)
				{
					selectRow(selectedRows[i], false);
				}
			}
		}
		else
		{
			((DefaultListSelectionModel)getSelectionModel()).removeSelectionInterval(row, row);
		}
	}

	/// <summary>
	/// Used by calculateColumnWidths to determine a column's name, as it should fit within certain constraints. </summary>
	/// <param name="the"> name of the column. </param>
	/// <param name="minWidth"> the minimum width (in pixels) that the column name should be
	/// tailored to fit.  Only used if the single largest token in the column name is smaller than the minimum width. </param>
	/// <param name="g"> the Graphics used to determine the width of Strings in certain fonts. </param>
	private string determineName(string name, int minWidth, Graphics g)
	{
		// because commas should be retained in the final column name, pad
		// them out to all be "comma-space" ...
		string temp = StringUtil.replaceString(name, ",", ", ");
		// ... and then split the string based on newlines and spaces.
		IList<string> v = StringUtil.breakStringList(temp, " \n", StringUtil.DELIM_SKIP_BLANKS);

		int[] sizes = new int[v.Count];
		string[] strings = new string[sizes.Length];
		FontMetrics fh = g.getFontMetrics(new Font(__columnHeaderFontName, __columnHeaderFontStyle, __columnHeaderFontSize));

		// go through all the strings that were broken out and determine 
		// the size each will take up in pixels when drawn on the screen
		for (int i = 0; i < sizes.Length; i++)
		{
			strings[i] = v[i].Trim();
			sizes[i] = fh.stringWidth(strings[i]);
		}

		// determine what the largest token is
		int biggest = -1;
		int max = 0;
		for (int i = 0; i < sizes.Length; i++)
		{
			if (sizes[i] > max)
			{
				biggest = i;
				max = sizes[i];
			}
		}

		// if the largest string is still less than what the minimum width
		// of the column is, create the column name and force it to pad out to the minimum width.
		if ((biggest >= 0) && (sizes[biggest] < minWidth))
		{
			return determineNameHelper(name, minWidth, g);
		}
		// otherwise ...
		else
		{
			string fullName = "";

			// if the largest token does not appear at the beginning of
			// the string, gather all the tokens that appear before it 
			// and create a String that will be no larger than the largest token
			if (biggest > 0)
			{
				string pre = "";
				for (int i = 0; i < biggest; i++)
				{
					pre += strings[i] + " ";
				}
				fullName += determineNameHelper(pre, sizes[biggest], g) + "\n";
			}

			if (biggest >= 0)
			{
				fullName += strings[biggest];
			}

			// if the largest token does not appear at the end of the string, gather all the rest of the tokens
			// and create a String that will be no larger than the longest token
			if ((biggest >= 0) && (biggest < (sizes.Length - 1)))
			{
				fullName += "\n";
				string post = "";
				for (int i = biggest + 1; i < sizes.Length; i++)
				{
					post += strings[i] + " ";
				}
				fullName += determineNameHelper(post, sizes[biggest], g);
			}

			return fullName;
		}
	}

	/// <summary>
	/// A helper method for determineName which takes a column name and constrains it
	/// to fit within the bounds of the width passed in the method.  It does this by 
	/// separating out words into separate lines, to return a multiple-line
	/// column name.  Words are separated by commas, spaces and newlines. </summary>
	/// <param name="name"> the column name to constrain to fit certain proportions. </param>
	/// <param name="maxWidth"> the point at which text should be wrapped to a new line. </param>
	/// <param name="g"> the Graphics context to use for determining how wide certain Strings are. </param>
	/// <returns> the column name with newlines that will fit in the desired space. </returns>
	private string determineNameHelper(string name, int maxWidth, Graphics g)
	{
		// because commas should be retained in the final column name, pad
		// them out to all be "comma-space" ...
		string temp = StringUtil.replaceString(name, ",", ", ");
		// ... and then split the string based on newlines and spaces.
		IList<string> v = StringUtil.breakStringList(temp, " \n", StringUtil.DELIM_SKIP_BLANKS);
		FontMetrics fh = g.getFontMetrics(new Font(__columnHeaderFontName, __columnHeaderFontStyle, __columnHeaderFontSize));

		// determine the sizes of all the split-out tokens
		int[] sizes = new int[v.Count];
		string[] strings = new string[sizes.Length];
		for (int i = 0; i < sizes.Length; i++)
		{
			strings[i] = v[i].Trim();
			sizes[i] = fh.stringWidth(strings[i]);
		}

		IList<string> lines = new List<string>();
		bool done = false;
		bool invalid = false;
		int curr = 0;
		int max = sizes.Length - 1;
		string s = null;
		int size = 0;

		while (!done)
		{
			s = "";
			// if on the very last one (so the previous string was added already), or the size of the current
			// one is too big, just add it straight to the list and set to done
			if (sizes[curr] > maxWidth || curr == max)
			{
				lines.Add(strings[curr]);
				curr++;
				if (curr > max)
				{
					done = true;
				}
			}
			else
			{
				invalid = false;
				// The curr string is at least a valid size.  Try adding the next ones on until the size is 
				// too large for the width.  Guaranteed to have at least one more after curr.
				s = strings[curr];
				size = sizes[curr];
				curr++;
				while (!invalid)
				{
					// If adding the next one would result in a string too long, set the loop to invalid.
					// wait for another time through the main loop.  
					if ((size + sizes[curr] + 1) > maxWidth)
					{
						invalid = true;
					}
					// Otherwise, append the string and set up for another loop through this one.
					else
					{
						size += sizes[curr] + 1;
						s += " " + strings[curr];
						curr++;
					}

					if (curr > max)
					{
						invalid = true;
						done = true;
					}
				}
				lines.Add(s);
			}
		}

		// Concatenate all the strings back together, putting in newlines as appropriate
		s = "";
		size = lines.Count;
		for (int i = 0; i < size - 1; i++)
		{
			s += lines[i].Trim() + "\n";
		}
		s += lines[size - 1].Trim();

		return s;
	}

	/// <summary>
	/// Turns on the row header.  This method should probably never be called by 
	/// programmers, as they should rely on the Worksheet to do this programmatically
	/// when the property is set to use row headers.
	/// </summary>
	protected internal virtual void enableRowHeader()
	{
		Container p = getParent();
		if (p is JViewport)
		{
			Container gp = p.getParent();
			if (gp is JScrollPane)
			{
				IList<string> v = new List<string>();
				JScrollPane jsp = (JScrollPane)gp;
				int rows = getRowCount();
				if (__incrementRowNumbers)
				{
					for (int i = 0; i < rows; i++)
					{
						v.Add("" + (i + __firstRowNum));
					}
				}
				else
				{
					for (int i = 0; i < rows; i++)
					{
						v.Add("" + (__firstRowNum - i));
					}
				}
				__listRowHeader = new SimpleJList(v);
				__listRowHeader.addKeyListener(this);
				__listRowHeader.setBackground(jsp.getBackground());
				__listRowHeader.setFixedCellWidth(50);
				__listRowHeader.setFixedCellHeight(getRowHeight());
				Font font = new Font(__rowHeaderFontName, __rowHeaderFontStyle, __rowHeaderFontSize);
				__listRowHeader.setCellRenderer(new JWorksheet_RowHeader(this, font, __rowHeaderColor));
				jsp.setRowHeaderView(__listRowHeader);
				__listRowHeader.addMouseListener(this);
				__worksheetRowHeader = null;
			}
		}
	}

	/// <summary>
	/// Turns on row headers using a worksheet as the row header.  Use in conjunction
	/// with the row selection model partner, thusly:<para><tt>
	/// worksheet1.enableRowHeader(headerWorksheet, cols);
	/// headerWorksheet.setRowSelectionModelPartner(
	///		worksheet1.getRowSelectionModel());
	/// </tt>
	/// </para>
	/// </summary>
	/// <param name="worksheet"> the worksheet to use as the row header. </param>
	/// <param name="cols"> the Columns to use from the worksheet. </param>
	public virtual void enableRowHeader(JWorksheet worksheet, int[] cols)
	{
		Container p = getParent();
		if (p is JViewport)
		{
			Container gp = p.getParent();
			if (gp is JScrollPane)
			{
				JScrollPane jsp = (JScrollPane)gp;

				__worksheetRowHeader = worksheet;
				jsp.setRowHeaderView(__worksheetRowHeader);
				__worksheetRowHeader.addMouseListener(this);
				__listRowHeader = null;
			}
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWorksheet()
	{
		__columnRemoved = null;
		__columnHeaderColor = null;
		__rowHeaderColor = null;
		__cellFont = null;
		__altTextCols = null;
		__altTextRows = null;
		__attrCols = null;
		__attrRows = null;
		__columnAlignments = null;
		__hourglassJDialog = null;
		__hourglassJFrame = null;
		__cancelMenuItem = null;
		__copyMenuItem = null;
		__copyHeaderMenuItem = null;
		__pasteMenuItem = null;
		__mainPopup = null;
		__popup = null;
		__columnHeaderView = null;
		__worksheetRowHeader = null;
		IOUtil.nullArray(__cellAttrs);
		__rowCountColumnAttributes = null;
		__copyPasteAdapter = null;
		__defaultCellRenderer = null;
		__hcr = null;
		__partner = null;
		__listRowHeader = null;
		IOUtil.nullArray(__altText);
		IOUtil.nullArray(__columnNames);
		__cellFontName = null;
		__columnHeaderFontName = null;
		__rowHeaderFontName = null;
		__worksheetListeners = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Finds the first row containing the specified value in the specified column </summary>
	/// <param name="value"> the value to match </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number to search.  Columns are 
	/// numbered starting at 0, and 0 is usually the row count column. </param>
	/// <param name="startingRow"> the row to start searching from </param>
	/// <param name="flags"> a bit-mask of flags specifying how to search </param>
	/// <returns> the index of the row containing the value, or -1 if not found (also
	/// -1 if the column class is not Integer). </returns>
	public virtual int find(int value, int absoluteColumn, int startingRow, int flags)
	{
		if (flags == 0)
		{
			flags = FIND_EQUAL_TO;
		}
		Type c = getModel().getColumnClass(absoluteColumn);
		// make sure this column holds integers
		if (!(c == typeof(Integer)))
		{
			return -1;
		}

		bool wrap = false;
		if ((flags & FIND_WRAPAROUND) == FIND_WRAPAROUND)
		{
			wrap = true;
		}

		// check if the starting row is out of bounds
		if (startingRow < 0)
		{
				return -1;
		}
		if (startingRow > (getRowCount() - 1))
		{
			if (wrap)
			{
				startingRow = 0;
			}
		}

		// check if the column is out of bounds
		if ((absoluteColumn < 0) || (absoluteColumn > (__columnNames.Length - 1)))
		{
				return -1;
		}

		int visibleColumn = getVisibleColumn(absoluteColumn);

		int endingRow = getRowCount();
		bool reverse = false;

		if ((flags & FIND_REVERSE) == FIND_REVERSE)
		{
			reverse = true;
			endingRow = 0;
		}
		bool equalTo = false;
		if ((flags & FIND_EQUAL_TO) == FIND_EQUAL_TO)
		{
			equalTo = true;
		}
		bool lessThan = false;
		if ((flags & FIND_LESS_THAN) == FIND_LESS_THAN)
		{
			lessThan = true;
		}
		bool greaterThan = false;
		if ((flags & FIND_GREATER_THAN) == FIND_GREATER_THAN)
		{
			greaterThan = true;
		}

		bool done = false;
		int rowVal;
		int row = startingRow;
		while (!done)
		{
			rowVal = ((int?)((getModel().getValueAt(row, visibleColumn)))).Value;

			if (equalTo)
			{
				if (value == rowVal)
				{
					return row;
				}
			}
			if (lessThan)
			{
				if (value < rowVal)
				{
					return row;
				}
			}
			if (greaterThan)
			{
				if (value > rowVal)
				{
					return row;
				}
			}

			if (reverse)
			{
				row--;
				if (row < endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = getRowCount() - 1;
						endingRow = startingRow;
						if (row < endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
			else
			{
				row++;
				if (row > endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = 0;
						endingRow = startingRow;
						if (row > endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first row containing the specified value in the specified column </summary>
	/// <param name="value"> the value to match </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number to search.  Columns are 
	/// numbered starting at 0, and 0 is usually the row count column. </param>
	/// <param name="startingRow"> the row to start searching from </param>
	/// <param name="flags"> a bit-mask of flags specifying how to search </param>
	/// <returns> the index of the row containing the value, or -1 if not found (also
	/// -1 if the column class is not Double). </returns>
	public virtual int find(double value, int absoluteColumn, int startingRow, int flags)
	{
		if (flags == 0)
		{
			flags = FIND_EQUAL_TO;
		}

		Type c = getModel().getColumnClass(absoluteColumn);
		// make sure this absoluteColumn holds integers
		if (!(c == typeof(Double)))
		{
			return -1;
		}

		bool wrap = false;
		if ((flags & FIND_WRAPAROUND) == FIND_WRAPAROUND)
		{
			wrap = true;
		}

		// check if the starting row is out of bounds
		if (startingRow < 0)
		{
				return -1;
		}
		if (startingRow > (getRowCount() - 1))
		{
			if (wrap)
			{
				startingRow = 0;
			}
		}

		// check if the column is out of bounds
		if ((absoluteColumn < 0) || (absoluteColumn > (__columnNames.Length - 1)))
		{
				return -1;
		}

		int visibleColumn = getVisibleColumn(absoluteColumn);

		int endingRow = getRowCount();
		bool reverse = false;

		if ((flags & FIND_REVERSE) == FIND_REVERSE)
		{
			reverse = true;
			endingRow = 0;
		}
		bool equalTo = false;
		if ((flags & FIND_EQUAL_TO) == FIND_EQUAL_TO)
		{
			equalTo = true;
		}
		bool lessThan = false;
		if ((flags & FIND_LESS_THAN) == FIND_LESS_THAN)
		{
			lessThan = true;
		}
		bool greaterThan = false;
		if ((flags & FIND_GREATER_THAN) == FIND_GREATER_THAN)
		{
			greaterThan = true;
		}

		bool done = false;
		double rowVal;
		int row = startingRow;
		while (!done)
		{
			rowVal = ((double?)((getModel().getValueAt(row, visibleColumn)))).Value;

			if (equalTo)
			{
				if (value == rowVal)
				{
					return row;
				}
			}
			if (lessThan)
			{
				if (value < rowVal)
				{
					return row;
				}
			}
			if (greaterThan)
			{
				if (value > rowVal)
				{
					return row;
				}
			}

			if (reverse)
			{
				row--;
				if (row < endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = getRowCount() - 1;
						endingRow = startingRow;
						if (row < endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
			else
			{
				row++;
				if (row > endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = 0;
						endingRow = startingRow;
						if (row > endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first row containing the specified value in the specified column </summary>
	/// <param name="value"> the value to match </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number to search.  Columns are 
	/// numbered starting at 0, and 0 is usually the row count column. </param>
	/// <param name="startingRow"> the row to start searching from </param>
	/// <param name="flags"> a bit-mask of flags specifying how to search </param>
	/// <returns> the index of the row containing the value, or -1 if not found (also
	/// -1 if the column class is not Date). </returns>
	public virtual int find(System.DateTime value, int absoluteColumn, int startingRow, int flags)
	{
		if (flags == 0)
		{
			flags = FIND_EQUAL_TO;
		}

		Type c = getModel().getColumnClass(absoluteColumn);
		// make sure this column holds integers
		if (!(c == typeof(System.DateTime)))
		{
			return -1;
		}

		bool wrap = false;
		if ((flags & FIND_WRAPAROUND) == FIND_WRAPAROUND)
		{
			wrap = true;
		}

		// check if the starting row is out of bounds
		if (startingRow < 0)
		{
				return -1;
		}
		if (startingRow > (getRowCount() - 1))
		{
			if (wrap)
			{
				startingRow = 0;
			}
		}

		// check if the column is out of bounds
		if ((absoluteColumn < 0) || (absoluteColumn > (__columnNames.Length - 1)))
		{
				return -1;
		}

		int visibleColumn = getVisibleColumn(absoluteColumn);

		int endingRow = getRowCount();
		bool reverse = false;

		if ((flags & FIND_REVERSE) == FIND_REVERSE)
		{
			reverse = true;
			endingRow = 0;
		}
		bool equalTo = false;
		if ((flags & FIND_EQUAL_TO) == FIND_EQUAL_TO)
		{
			equalTo = true;
		}
		bool lessThan = false;
		if ((flags & FIND_LESS_THAN) == FIND_LESS_THAN)
		{
			lessThan = true;
		}
		bool greaterThan = false;
		if ((flags & FIND_GREATER_THAN) == FIND_GREATER_THAN)
		{
			greaterThan = true;
		}

		bool done = false;
		System.DateTime rowVal;
		int row = startingRow;
		int result;
		while (!done)
		{
			rowVal = ((System.DateTime)((getModel().getValueAt(row, visibleColumn))));

			result = rowVal.compareTo(value);

			if (equalTo)
			{
				if (result == 0)
				{
					return row;
				}
			}
			if (lessThan)
			{
				if (result == -1)
				{
					return row;
				}
			}
			if (greaterThan)
			{
				if (result == 1)
				{
					return row;
				}
			}

			if (reverse)
			{
				row--;
				if (row < endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = getRowCount() - 1;
						endingRow = startingRow;
						if (row < endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
			else
			{
				row++;
				if (row > endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = 0;
						endingRow = startingRow;
						if (row > endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first row containing the specified object.  This method can only
	/// be used with worksheets that store a single object in a single row.  The find
	/// uses '<tt>.equals</tt> to compare the objects to see if they match. </summary>
	/// <param name="o"> the object for which to search.  Null can be passed in. </param>
	/// <param name="startingRow"> the row to start searching from. </param>
	/// <param name="flags"> a bit-mask of flags specifying how to search </param>
	/// <returns> the index of the row containing the object, or -1 if not found. </returns>
	public virtual int find(object o, int startingRow, int flags)
	{
		bool wrap = false;
		if ((flags & FIND_WRAPAROUND) == FIND_WRAPAROUND)
		{
			wrap = true;
		}

		// check if the starting row is out of bounds
		if (startingRow < 0)
		{
				return -1;
		}
		if (startingRow > (getRowCount() - 1))
		{
			if (wrap)
			{
				startingRow = 0;
			}
		}

		int endingRow = getRowCount();
		bool reverse = false;

		if ((flags & FIND_REVERSE) == FIND_REVERSE)
		{
			reverse = true;
			endingRow = 0;
		}

		bool done = false;
		object rowObj;
		int row = startingRow;
		while (!done)
		{
			rowObj = getRowData(row);

			if (o == null)
			{
				if (rowObj == null)
				{
					return row;
				}
			}
			else
			{
				if (o.Equals(rowObj))
				{
					return row;
				}
			}

			if (reverse)
			{
				row--;
				if (row < endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = getRowCount() - 1;
						endingRow = startingRow;
						if (row < endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
			else
			{
				row++;
				if (row > endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = 0;
						endingRow = startingRow;
						if (row > endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first row containing the specified value in the specified column </summary>
	/// <param name="findValue"> the value to match </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number to search.  Columns are 
	/// numbered starting at 0, and 0 is usually the row count column. </param>
	/// <param name="startingRow"> the row to start searching from </param>
	/// <param name="flags"> a bit-mask of flags specifying how to search </param>
	/// <returns> the index of the row containing the value, or -1 if not found (also
	/// -1 if the column class is not String). </returns>
	public virtual int find(string findValue, int absoluteColumn, int startingRow, int flags)
	{
		Type c = getModel().getColumnClass(absoluteColumn);
		// make sure this column holds integers
		if (!(c == typeof(string)))
		{
			return -1;
		}

		bool wrap = false;
		if ((flags & FIND_WRAPAROUND) == FIND_WRAPAROUND)
		{
			wrap = true;
		}

		// check if the starting row is out of bounds
		if (startingRow < 0)
		{
				return -1;
		}
		if (startingRow > (getRowCount() - 1))
		{
			if (wrap)
			{
				startingRow = 0;
			}
		}

		// check if the column is out of bounds
		if ((absoluteColumn < 0) || (absoluteColumn > (__columnNames.Length - 1)))
		{
				return -1;
		}

		string value = findValue.Trim();

		int endingRow = getRowCount();
		bool done = false;
		string rowVal;
		int row = startingRow;
		int result;

		bool reverse = false;
		if ((flags & FIND_REVERSE) == FIND_REVERSE)
		{
			reverse = true;
			endingRow = 0;
		}
		bool caseSensitive = true;
		if ((flags & FIND_CASE_INSENSITIVE) == FIND_CASE_INSENSITIVE)
		{
			caseSensitive = false;
		}
		bool contains = false;
		if ((flags & FIND_CONTAINS) == FIND_CONTAINS)
		{
			contains = true;
		}
		bool equals = false;
		if ((flags & FIND_EQUAL_TO) == FIND_EQUAL_TO)
		{
			equals = true;
		}
		bool startsWith = false;
		if ((flags & FIND_STARTS_WITH) == FIND_STARTS_WITH)
		{
			startsWith = true;
		}
		bool endsWith = false;
		if ((flags & FIND_ENDS_WITH) == FIND_ENDS_WITH)
		{
			endsWith = true;
		}

		while (!done)
		{
			rowVal = ((string)((getModel().getValueAt(row, absoluteColumn)))).Trim();

			if (equals)
			{
				if (caseSensitive)
				{
					if (rowVal.Equals(value))
					{
						return row;
					}
				}
				else
				{
					if (rowVal.Equals(value, StringComparison.OrdinalIgnoreCase))
					{
						return row;
					}
				}
			}

			if (contains)
			{
				if (caseSensitive)
				{
					result = rowVal.IndexOf(value, StringComparison.Ordinal);
				}
				else
				{
					result = StringUtil.indexOfIgnoreCase(rowVal, value, 0);
				}

				if (result != -1)
				{
					return row;
				}
			}

			if (startsWith)
			{
				if (caseSensitive)
				{
					if (rowVal.StartsWith(value, StringComparison.Ordinal))
					{
						return row;
					}
				}
				else
				{
					if (StringUtil.startsWithIgnoreCase(rowVal, value))
					{
						return row;
					}
				}
			}

			if (endsWith)
			{
				if (caseSensitive)
				{
					if (rowVal.EndsWith(value, StringComparison.Ordinal))
					{
						return row;
					}
				}
				else
				{
					if (StringUtil.endsWithIgnoreCase(rowVal, value))
					{
						return row;
					}
				}
			}

			if (reverse)
			{
				row--;
				if (row < endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = getRowCount() - 1;
						endingRow = startingRow;
						if (row < endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
			else
			{
				row++;
				if (row >= endingRow)
				{
					if (wrap)
					{
						wrap = false;
						row = 0;
						endingRow = startingRow;
						if (row > endingRow)
						{
							done = true;
						}
					}
					else
					{
						done = true;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the <b>absolute</b> column number (i.e., it includes the column 
	/// numbers that are hidden) for a <b>visible</b> column number.<para>
	/// For example, if a table has 5 columns, some of which are not visible, the 
	/// </para>
	/// absolute and visible column numbers are as shown:<para>
	/// <pre>
	/// [0] - visible     - 0
	/// [1] - visible     - 1
	/// [2] - not visible - n/a
	/// [3] - not visible - n/a
	/// [4] - visible     - 2
	/// </pre>
	/// The <b>absolute</b> column numbers are listed on the left-hand side.  The
	/// </para>
	/// <b>visible</b> column numbers are listed on the right-hand side.<para>
	/// </para>
	/// </summary>
	/// <param name="visibleColumn"> the <b>visible</b> column number for which to return 
	/// the <b>absolute</b> column number </param>
	/// <returns> the <b>absolute</b> column number from the <b>visible</b> column number </returns>
	/// <seealso cref= #getVisibleColumn(int) </seealso>
	public virtual int getAbsoluteColumn(int visibleColumn)
	{
		int visHit = -1;
		if (__columnRemoved == null)
		{
			return visibleColumn;
		}
		int size = __columnRemoved.Length;

		for (int i = 0; i < size; i++)
		{
			if (__columnRemoved[i] == false)
			{
				visHit++;
			}
			if (visHit == visibleColumn)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the total count of all columns in the worksheet, not just those that are visible. </summary>
	/// <returns> the total count of all columns in the worksheet, not just those that are visible. </returns>
	public virtual int getAbsoluteColumnCount()
	{
		return __columnRemoved.Length;
	}

	/// <summary>
	/// Returns all the data objects in the table as a single Vector.  Use only 
	/// if the table model for the worksheet stores each row as a separate data object. </summary>
	/// <returns> a list of all data objects in the table. </returns>
	public virtual System.Collections.IList getAllData()
	{
		if (!(getModel() is JWorksheet_AbstractTableModel))
		{
			return getRowData(0, getRowCount() - 1);
		}
		else
		{
			return ((JWorksheet_AbstractTableModel)getModel()).getData();
		}
	}

	/// <summary>
	/// Finds alternate text for the specified cell in the alternate text arrays and returns the text. </summary>
	/// <param name="row"> the row of the cell.  Rows are numbered starting at 0. </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column of the cell.  Columns are 
	/// numbered starting at 0, and column 0 is usually the row count column. </param>
	/// <returns> the alternate text of the cell. </returns>
	public virtual string getCellAlternateText(int row, int absoluteColumn)
	{
		if (__altTextCount == 0)
		{
			return null;
		}

		int visCol = getVisibleColumn(absoluteColumn);
		for (int i = 0; i <= (__altTextCount - 1); i++)
		{
			if (__altTextRows[i] == row)
			{
				if (__altTextCols[i] == visCol)
				{
					return __altText[i];
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Returns the cell that is located at a mouse click on the table.  The cell is
	/// represented as a two-element integer array.  Array element 0 contains the 
	/// row number (or -1 if no rows were clicked on).  Array element 1 contains the
	/// column number (or -1 if no columns were clicked on). </summary>
	/// <returns> the cell that is located at a mouse click on the table. </returns>
	public virtual int[] getCellAtClick(MouseEvent @event)
	{
		int[] cell = new int[2];

		cell[0] = rowAtPoint(@event.getPoint());
		cell[1] = columnAtPoint(@event.getPoint());

		return cell;
	}

	/// <summary>
	/// Finds cell attributes for the specified cell in the cell attribute arrays and returns the attributes. </summary>
	/// <param name="row"> the row of the cell.  Rows are numbered starting at 0. </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column of the cell.  Columns are 
	/// numbered starting at 0, and column 0 is usually the row count column. </param>
	/// <returns> the cell attributes if the cell has them, or null. </returns>
	public virtual JWorksheet_CellAttributes getCellAttributes(int row, int absoluteColumn)
	{
		if (__attrCount == 0)
		{
			return null;
		}
		int visCol = getVisibleColumn(absoluteColumn);
		for (int i = 0; i <= (__attrCount - 1); i++)
		{
			if (__attrRows[i] == row)
			{
				if (__attrCols[i] == visCol)
				{
					return __cellAttrs[i];
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Returns the Font in which to render worksheet cells.  While individual cell
	/// attributes can be used to change the font in different cells, getCellFont()
	/// and setCellFont() are quicker for changing and returning the font used 
	/// everywhere in the table where a specific cell font attribute has not been set. </summary>
	/// <returns> the Font in which to render a given cell. </returns>
	public virtual Font getCellFont()
	{
		return __cellFont;
	}

	/// <summary>
	/// Returns the cell renderer being used by the worksheet. </summary>
	/// <returns> the cell renderer being used by the worksheet. </returns>
	public virtual JWorksheet_DefaultTableCellRenderer getCellRenderer()
	{
		return __defaultCellRenderer;
	}

	/// <summary>
	/// Returns the list of values stored in a cell-specific JComboBox. </summary>
	/// <param name="row"> the row of the cell. </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column of the cell. </param>
	/// <returns> the list of values stored in a cell-specific JComboBox, or null
	/// if the cell does not use a combo box. </returns>
	public virtual System.Collections.IList getCellSpecificJComboBoxValues(int row, int absoluteColumn)
	{
		TableColumn col = getColumnModel().getColumn(getVisibleColumn(absoluteColumn));
		TableCellEditor editor = col.getCellEditor();

		if (editor == null)
		{
			return null;
		}

		return ((JWorksheet_JComboBoxCellEditor)editor).getJComboBoxModel(row);
	}

	/// <summary>
	/// Returns the user-specified alignment for a column. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column for which to return the alignment. </param>
	/// <returns> the alignment that has been set with setColumnAlignment(), or DEFAULT
	/// if the column has not had an alignment set yet. </returns>
	public virtual int getColumnAlignment(int absoluteColumn)
	{
		if (__columnAlignments == null)
		{
			return LEFT;
		}
		return __columnAlignments[absoluteColumn];
	}

	/// <summary>
	/// Returns the class of data stored in the table at the specified column. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column for which to return the class. </param>
	/// <returns> the class of data stored in the table at the specified column.  Compare
	/// to other classes with code like:
	/// <pre>
	/// if (getColumnClass(0) == Double.class) { ... }
	/// if (getColumnClass(col) != String.class) { ... }
	/// </pre> </returns>
	public virtual Type getColumnClass(int absoluteColumn)
	{
		return getModel().getColumnClass(absoluteColumn);
	}

	/// <summary>
	/// Returns the format for the data in the specified column.  TOD -- only 
	/// works for AbstractExcelCellRenderers </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column for which to return the column format. </param>
	/// <returns> the format for the data in the specified column. </returns>
	public virtual string getColumnFormat(int absoluteColumn)
	{
		JWorksheet_AbstractExcelCellRenderer renderer = (JWorksheet_AbstractExcelCellRenderer)getCellRenderer();
		return renderer.getFormat(absoluteColumn);
	}

	/// <summary>
	/// Returns the name of the specified column.  Overrides the original JTable
	/// code in order to provide documentation for the column to provide. </summary>
	/// <param name="visibleColumn"> the <b>visible</b> column for which to return the name. </param>
	/// <returns> the name of the specified column. </returns>
	public virtual string getColumnName(int visibleColumn)
	{
		return getColumnName(visibleColumn, false);
	}

	/// <summary>
	/// Returns the name of the specified column. </summary>
	/// <param name="visibleColumn"> the <b>visible</b> column for which to return the name. </param>
	/// <param name="convertNewlines"> if true, then any newlines in the column name (which
	/// would be used in tables that have multiple-line headers) will be stripped out
	/// and replaced with spaces.  This is useful for getting single-line versions of
	/// multiple-line column names.  If false, the standard column name (with newlines,
	/// if they are in the name) will be returned. </param>
	/// <returns> the name of the specified column. </returns>
	public virtual string getColumnName(int visibleColumn, bool convertNewlines)
	{
		string s = base.getColumnName(visibleColumn);

		if (!convertNewlines)
		{
			return s;
		}
		else
		{
			return convertColumnName(s);
		}
	}

	/// <summary>
	/// Determines the width of the largest token in the column name in pixels.  This
	/// is the width at which the other tokens in the column name must be wrapped to
	/// other lines in order to fit the space allotted. </summary>
	/// <param name="name"> the column name. </param>
	/// <param name="g"> the Graphics context to use for determining the width of certain 
	/// Strings in pixels. </param>
	private int getColumnNameFitSize(string name, Graphics g)
	{
		IList<string> v = StringUtil.breakStringList(name, "\n", 0);
		FontMetrics fh = g.getFontMetrics(new Font(__columnHeaderFontName, __columnHeaderFontStyle, __columnHeaderFontSize));

		int size = v.Count;
		int maxSize = 0;
		int width = 0;
		for (int i = 0; i < size; i++)
		{
			width = fh.stringWidth((string)v[i]);
			if (width > maxSize)
			{
				maxSize = width;
			}
		}
		return maxSize + 15;
	}

	/// <summary>
	/// For a column with the given name, returns the <i>visible</i> column number. 
	/// If the table contains more than one column with the same name, the number of the first one will be returned. </summary>
	/// <returns> the <i>visible</i> column number. </returns>
	public virtual int getColumnNumber(string columnName)
	{
		string name = null;

		for (int i = 0; i < getVisibleColumnCount(); i++)
		{
			name = getColumnName(i);
			if (name.Equals(columnName))
			{
				return i;
			}
			if (convertColumnName(name).Equals(columnName))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Returns the prefix being stored before column names, which depends on the kind
	/// of column prefix that was set up. </summary>
	/// <param name="columnNum"> the <b>relative</b> column for which to return the prefix. </param>
	public virtual string getColumnPrefix(int columnNum)
	{
		if (__columnNumbering == __NUMBERING_NONE)
		{
			return "";
		}
		else if (__columnNumbering == __NUMBERING_EXCEL)
		{
			return getExcelNumber(columnNum);
		}
		else if (__columnNumbering == __NUMBERING_0)
		{
			return "" + columnNum + " - ";
		}
		else if (__columnNumbering == __NUMBERING_1)
		{
			return "" + (columnNum + 1) + " - ";
		}
		return "";
	}

	/// <summary>
	/// Gets a value from the table at the specified row and column, using a consecutive read policy.<para>
	/// Some table models may store data (e.g., time series dates), in which 
	/// data values are calculated based on the previous row's data.  In this case,
	/// this method can be used and they would know that a consecutive read of the table
	/// data will be done, and that everytime a call is made to getValueAt() in the 
	/// table model, the row parameter is guaranteed to be the same row as the last 
	/// time getValueAt() was called (if the column is different), or 1 more than
	/// the previous row.
	/// </para>
	/// </summary>
	public virtual object getConsecutiveValueAt(int row, int visibleColumn)
	{
		return ((JWorksheet_AbstractTableModel)getModel()).getConsecutiveValueAt(row, visibleColumn);
	}

	/// <summary>
	/// Returns the <b>visible</b> column of the cell being edited, or -1 if no cell is being edited. </summary>
	/// <returns> the <b>visible</b> column of the cell being edited, or -1 if no cell is being edited. </returns>
	public virtual int getEditColumn()
	{
		return __editCol;
	}

	/// <summary>
	/// Returns the row of the cell being edited, or -1 if no cell is being edited. </summary>
	/// <returns> the row of the cell being edited, or -1 if no cell is being edited. </returns>
	public virtual int getEditRow()
	{
		return __editRow;
	}

	/// <summary>
	/// Returns the JDialog being used as the Hourglass display dialog.  Will never return null. </summary>
	/// <returns> the JDialog being used as the Hourglass display dialog. </returns>
	public virtual JDialog getHourglassJDialog()
	{
		return __hourglassJDialog;
	}

	/// <summary>
	/// Returns the JFrame being used as the Hourglass display frame.  Will never return null. </summary>
	/// <returns> the JFrame being used as the Hourglass display frame. </returns>
	public virtual JFrame getHourglassJFrame()
	{
		return __hourglassJFrame;
	}

	/// <summary>
	/// Returns the data stored in the last row of the worksheet.  Only works for
	/// worksheetss whose table models stored a single data object per row. </summary>
	/// <returns> the data object stored in the last row of the worksheet. </returns>
	public virtual object getLastRowData()
	{
		return getRowData(getRowCount() - 1);
	}

	/// <summary>
	/// Returns the maximum width of the data in the specified column in the given rows, in pixels. </summary>
	/// <param name="absoluteColumn"> the column in which to check the data. </param>
	/// <param name="rows"> the number of rows (from 0) to check the data in.  If greater 
	/// than getRowCount() will be set equal to getRowCount(). </param>
	/// <param name="g"> the Graphics context to use for determining font widths. </param>
	private int getDataMaxWidth(int absoluteColumn, int rows, Graphics g)
	{
		string s = null;
		if (rows > getRowCount())
		{
			rows = getRowCount();
		}
		FontMetrics fc = g.getFontMetrics(new Font(__cellFontName, __cellFontStyle, __cellFontSize));

		int widest = 0;
		int width = 0;
		int col = getVisibleColumn(absoluteColumn);

		for (int i = 0; i < rows; i++)
		{
			s = getValueAtAsString(i, col);
			width = fc.stringWidth(s);
			if (width > widest)
			{
				widest = width;
			}
		}
		return widest + 15;
	}

	/// <summary>
	/// Returns the Excel column heading that corresponds to the given column number. </summary>
	/// <param name="columnNumber"> the number of the column (base 0). </param>
	/// <returns> the column header that would appear in an Excel worksheet. </returns>
	public static string getExcelNumber(int columnNumber)
	{
		int[] val = new int[2];
		if (columnNumber > 25)
		{
			val[0] = columnNumber / 26;
		}
		else
		{
			val[0] = 0;
		}
		val[1] = columnNumber % 26;

		string excel = "";

		int num = -1;
		for (int i = 0; i < 2; i++)
		{
			num = val[i];
			if (i == 0)
			{
				num--;
			}
			switch (num)
			{
				case -1:
					excel += "";
					break;
				case 0:
					excel += "A";
					break;
				case 1:
					excel += "B";
					break;
				case 2:
					excel += "C";
					break;
				case 3:
					excel += "D";
					break;
				case 4:
					excel += "E";
					break;
				case 5:
					excel += "F";
					break;
				case 6:
					excel += "G";
					break;
				case 7:
					excel += "H";
					break;
				case 8:
					excel += "I";
					break;
				case 9:
					excel += "J";
					break;
				case 10:
					excel += "K";
					break;
				case 11:
					excel += "L";
					break;
				case 12:
					excel += "M";
					break;
				case 13:
					excel += "N";
					break;
				case 14:
					excel += "O";
					break;
				case 15:
					excel += "P";
					break;
				case 16:
					excel += "Q";
					break;
				case 17:
					excel += "R";
					break;
				case 18:
					excel += "S";
					break;
				case 19:
					excel += "T";
					break;
				case 20:
					excel += "U";
					break;
				case 21:
					excel += "V";
					break;
				case 22:
					excel += "W";
					break;
				case 23:
					excel += "X";
					break;
				case 24:
					excel += "Y";
					break;
				case 25:
					excel += "Z";
					break;
			}
		}
		return excel;
	}

	/// <summary>
	/// Returns whether the user can select an entire row by clicking on the first (0th)
	/// column.  One click row selection is not possible if the worksheet was built
	/// with the JWorksheet.SelectionMode property set to: SingleCellSelection,
	/// SingleRowSelection, MultipleRowSelection, or MultipleDiscontinuousRowSelection. </summary>
	/// <returns> whether the user can select an entire row by clicking on the first (0th) column. </returns>
	public virtual bool getOneClickRowSelection()
	{
		if (__selectionMode == __EXCEL_SELECTION)
		{
			return ((JWorksheet_RowSelectionModel)getSelectionModel()).getOneClickRowSelection();
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Gets the original row number a row had, given its current position in the sorted rows. </summary>
	/// <param name="sortedRow"> the sorted row number of the row. </param>
	/// <returns> the original, unsorted row number. </returns>
	public virtual int getOriginalRowNumber(int sortedRow)
	{
		int[] sortedOrder = ((JWorksheet_AbstractTableModel)getModel())._sortOrder;
		if (sortedOrder == null)
		{
			return sortedRow;
		}
		if (sortedRow < 0 || sortedRow > sortedOrder.Length)
		{
			return -1;
		}
		return sortedOrder[sortedRow];
	}

	/// <summary>
	/// Returns the data element at the given row.  The data must be cast
	/// to the proper data type; the table has no idea what it is.  Row 0 is the 
	/// first row.   If the row is out of the range of rows in the table, or less than 0, null will be returned.
	/// This only works with JWorksheet_AbstractRowTableModels. </summary>
	/// <param name="row"> the row for which to return the data.  Rows are numbered starting at 0. </param>
	/// <returns> the object stored at the given row, or null if the row was invalid. 
	/// Returns null if the table model is not derived from JWorksheet_AbstractRowTableModel. </returns>
	public virtual object getRowData(int row)
	{
		if (!(getModel() is JWorksheet_AbstractRowTableModel))
		{
			return null;
		}

		IList<object> v = getRowData(row, row);
		if (v != null && v.Count > 0)
		{
			return (getRowData(row, row)).get(0);
		}
		return null;
	}

	/// <summary>
	/// Returns the data elements from the given rows.  Row 0 is the first row.  If
	/// the range of rows goes out of range of the number of rows in the table, or 
	/// goes less than 0, a null value will be returned for each row for which the row number is out of range.
	/// This only works with JWorksheet_AbstractRowTableModels. </summary>
	/// <param name="row1"> the first row for which to return data.  Rows are numbered starting at 0. </param>
	/// <param name="row2"> the last row for which to return data.  Rows are numbered starting at 0. </param>
	/// <returns> a list of the objects stored in the given rows.
	/// Returns null if the table model is not derived from 
	/// JWorksheet_AbstractRowTableModel, or if the range of row numbers was invalid. </returns>
	public virtual IList<object> getRowData(int row1, int row2)
	{
		if (!(getModel() is JWorksheet_AbstractRowTableModel))
		{
			return null;
		}
		if (row1 == 0 && row2 == -1)
		{
			// special case -- getAllData called on an empty worksheet
			return new List<object>();
		}

		if (row1 > row2)
		{
			row2 = row1;
			row1 = row2;
		}

		if (row1 < 0 || row2 < 0)
		{
			return null;
		}
		int size = getRowCount();
		if (row1 >= size || row2 >= size)
		{
			return null;
		}

		IList<object> v = new List<object>();
		for (int i = row1; i <= row2; i++)
		{
			v.Add(((JWorksheet_AbstractRowTableModel)getModel()).getRowData(i));
		}
		return v;
	}

	/// <summary>
	/// Returns the row data for the rows specified in the parameter array. </summary>
	/// <param name="rows"> the integer array containing the row numbers for which to return
	/// data.  Cannot be null.  Data will be returned in the order of the row numbers in this array. </param>
	public virtual IList<object> getRowData(int[] rows)
	{
		IList<object> v = new List<object>();
		for (int i = 0; i < rows.Length; i++)
		{
			v.Add(getRowData(rows[i]));
		}
		return v;
	}

	/// <summary>
	/// Returns the row selection model. </summary>
	/// <returns> the row selection model. </returns>
	public virtual JWorksheet_RowSelectionModel getRowSelectionModel()
	{
		return (JWorksheet_RowSelectionModel)getSelectionModel();
	}

	/// <summary>
	/// Returns a list containing two integer arrays, the first of which contains all 
	/// the rows of the selected cells, and the second of which contains the matching
	/// columns for the rows in order to determine which cells are selected.
	/// For a given cell I, the row of the cell is the first array's element at 
	/// position I and the column of the cell is the second array's element at
	/// position I.  The arrays are guaranteed to be non-null.  The list will never be null. </summary>
	/// <returns> a list containing two integer arrays. </returns>
	public virtual IList<int []> getSelectedCells()
	{
		IList<int> rows = new List<int>();
		IList<int> cols = new List<int>();

		for (int i = 0; i < getRowCount(); i++)
		{
			for (int j = 0; j < getColumnCount(); j++)
			{
				if (isCellSelected(i, j))
				{
					rows.Add(new int?(i));
					cols.Add(new int?(j));
				}
			}
		}

		int size = rows.Count;

		int[] rowCells = new int[size];
		int[] colCells = new int[size];

		for (int i = 0; i < size; i++)
		{
			rowCells[i] = ((int?)rows[i]).Value;
			colCells[i] = ((int?)cols[i]).Value;
		}

		IList<int []> v = new List<int []>();
		v.Add(rowCells);
		v.Add(colCells);
		return v;
	}

	/// <summary>
	/// Returns the first <b>absolute</b> selected column number, or -1 if none 
	/// are selected.  A column is considered to be selected if any of its cells have are selected. </summary>
	/// <returns> the first selected column number, or -1 if none are selected. </returns>
	public virtual int getSelectedColumn()
	{
		if (__selectionMode == __EXCEL_SELECTION)
		{
			return getAbsoluteColumn(((JWorksheet_RowSelectionModel)getSelectionModel()).getSelectedColumn());
		}
		else
		{
			// necessary because of how the normal selection models have been mistreated by JWorksheet.
			for (int i = 0; i < getColumnCount(); i++)
			{
				for (int j = 0; j < getRowCount(); j++)
				{
					if (isCellSelected(j, i))
					{
						return getAbsoluteColumn(i);
					}
				}
			}
			return -1;
		}
	}

	/// <summary>
	/// Returns a count of the number of columns selected.  Columns are considered to
	/// be selected if any of their cells are selected. </summary>
	/// <returns> a count of the number of columns selected. </returns>
	public virtual int getSelectedColumnCount()
	{
		return (getSelectedColumns()).length;
	}

	/// <summary>
	/// Returns an integer array of the selected <b>absolute</b> column numbers.  
	/// Columns are considered to be selected if any of their cells are selected. </summary>
	/// <returns> an integer array of the selected <b>absolute</b> column numbers.  The
	/// columns will be in order from the lowest selected column to the highest. </returns>
	public virtual int[] getSelectedColumns()
	{
		if (__selectionMode == __EXCEL_SELECTION)
		{
			int[] selected = ((JWorksheet_RowSelectionModel)getSelectionModel()).getSelectedColumns();
			for (int i = 0; i < selected.Length; i++)
			{
	//			Message.printStatus(1, "", "" + selected[i] + " --> " + getAbsoluteColumn(selected[i]));
				selected[i] = getAbsoluteColumn(selected[i]);
			}
			return selected;
		}
		else
		{
			int[] selectedCols = new int[getColumnCount()];
			int count = 0;
			// necessary because of how the normal selection models have been mistreated by JWorksheet.
			int rows = getRowCount();
			int cols = getColumnCount();
			for (int i = 0; i < cols; i++)
			{
				for (int j = 0; j < rows; j++)
				{
					if (isCellSelected(j, i))
					{
						selectedCols[count] = getAbsoluteColumn(i);
						count++;
						j = rows + 1;
					}
				}
			}

			int[] selected = new int[count];
			for (int i = 0; i < count; i++)
			{
				selected[i] = selectedCols[i];
			}
			return selected;
		}
	}

	/// <summary>
	/// Returns an integer of the first row in the table that is selected or -1 if
	/// none are selected.  Rows are considered to be selected if any of their cells are selected. </summary>
	/// <returns> an integer of the first row in the table that is selected or -1 if none are selected. </returns>
	public virtual int getSelectedRow()
	{
		if (__selectionMode == __EXCEL_SELECTION)
		{
			return ((JWorksheet_RowSelectionModel)getSelectionModel()).getSelectedRow();
		}
		else
		{
			// necessary because of how the normal selection models have been mistreated by JWorksheet.
			for (int i = 0; i < getRowCount(); i++)
			{
				for (int j = 0; j < getColumnCount(); j++)
				{
					if (isCellSelected(i, j))
					{
						return i;
					}
				}
			}
			return -1;
		}
	}

	/// <summary>
	/// Returns a count of the number of rows in the worksheet that are selected.
	/// Overrides the method in JTable so that it works correctly.  Rows are considered
	/// to be selected if any of their cells are selected. </summary>
	/// <returns> the number of rows in the worksheet that are selected. </returns>
	public virtual int getSelectedRowCount()
	{
		return (getSelectedRows()).length;
	}

	/// <summary>
	/// Returns an integer array of the rows in the table that have been selected.
	/// Rows are considered to be selected if any of their cells are selected. </summary>
	/// <returns> an integer array of the rows in the table that have been selected, guaranteed to be non-null but may
	/// be zero length.  The array results will be in order from lowest row to highest row. </returns>
	public virtual int[] getSelectedRows()
	{
		if (__selectionMode == __EXCEL_SELECTION)
		{
			return ((JWorksheet_RowSelectionModel)getSelectionModel()).getSelectedRows();
		}
		else
		{
			int[] selectedRows = new int[getRowCount()];
			int count = 0;
			// necessary because of how the normal selection models have been mistreated by JWorksheet.
			int rows = getRowCount();
			int cols = getColumnCount();
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					if (isCellSelected(i, j))
					{
						selectedRows[count] = i;
						count++;
						j = cols + 1;
					}
				}
			}

			int[] selected = new int[count];
			for (int i = 0; i < count; i++)
			{
				selected[i] = selectedRows[i];
			}
			return selected;
		}
	}

	/// <summary>
	/// Returns the sorted row number of a row in the JWorksheet, given its original unsorted row number. </summary>
	/// <param name="unsortedRow"> the unsorted row number of a row in the worksheet. </param>
	/// <returns> the sorted row number of the row.  If the table is not sorted, the passed-in row is returned. </returns>
	public virtual int getSortedRowNumber(int unsortedRow)
	{
		int[] sortedOrder = ((JWorksheet_AbstractTableModel)getModel())._sortOrder;
		if (sortedOrder == null)
		{
			return unsortedRow;
		}
		for (int i = 0; i < sortedOrder.Length; i++)
		{
			if (sortedOrder[i] == unsortedRow)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// For a number format (e.g., "%9d"), this returns an equivalent format that can
	/// be used to parse the number as a string. </summary>
	/// <param name="numberFormat"> the number format to process. </param>
	/// <returns> a string format code (e.g., "%9s") that can be used to read a value in as a String. </returns>
	public virtual string getStringFormat(string numberFormat)
	{
		if (string.ReferenceEquals(numberFormat, null))
		{
			return "%s";
		}
		try
		{
			string format = numberFormat.Trim();
			int len = format.Length;
			if (StringUtil.endsWithIgnoreCase(format, "d"))
			{
				format = format.Substring(1, (len - 1) - 1);
				int n = StringUtil.atoi(format);
				return "%" + n + "." + n + "s";
			}
			else if (StringUtil.endsWithIgnoreCase(format, "f"))
			{
				format = format.Substring(1, (len - 1) - 1);
				if (format.StartsWith("#", StringComparison.Ordinal))
				{
					format = format.Substring(1);
				}
				int index = format.IndexOf(".", StringComparison.Ordinal);
				if (index > -1)
				{
					format = format.Substring(0, index);
				}
				int n = StringUtil.atoi(format);
				return "%" + n + "." + n + "s";
			}
			else if (StringUtil.endsWithIgnoreCase(format, "s"))
			{
				format = format.Substring(1, (len - 1) - 1);
				return numberFormat;
			}
			else
			{
				 return "%s";
			}
		}
		catch (Exception e)
		{
			string routine = "JWorksheet.getStringFormat()";
			Message.printWarning(3, routine, e);
			Message.printWarning(3, routine, "Could not parse format: '" + numberFormat + "'");
		}
		return "%s";
	}

	/// <summary>
	/// Returns the table model being used by the worksheet. </summary>
	/// <returns> the table model being used by the worksheet. </returns>
	public virtual JWorksheet_AbstractTableModel getTableModel()
	{
		return (JWorksheet_AbstractTableModel)getModel();
	}

	/// <summary>
	/// Returns the value at the specified position. </summary>
	/// <param name="row"> the row of the value to return. </param>
	/// <param name="column"> the <b>visible</b> column from which to return the value. </param>
	public virtual object getValueAt(int row, int column)
	{
		return base.getValueAt(row, column);
	}

	/// <summary>
	/// Returns the value at the specified position formatted as a String with the format stored in the table model. </summary>
	/// <param name="row"> the row of the value to return. </param>
	/// <param name="column"> the <b>visible</b> column from which to return the value. </param>
	/// <returns> the value formatted as a String. </returns>
	public virtual string getValueAtAsString(int row, int column)
	{
		// TODO (JTS - 2005-11-07) this could probably be sped up significantly if we weren't getting
		// the renderer every time for every cell
		JWorksheet_AbstractTableCellRenderer renderer = getCellRenderer();
		if (renderer is JWorksheet_AbstractExcelCellRenderer)
		{
			return getValueAtAsString(row, column, ((JWorksheet_AbstractExcelCellRenderer)renderer).getFormat(getAbsoluteColumn(column)));
		}
		else
		{
			return "" + getValueAt(row, column);
		}
	}

	/// <summary>
	/// Returns the value at the specified position formatted as a String with the specified format.
	/// Return an empty string if null. </summary>
	/// <param name="row"> the row of the value to return. </param>
	/// <param name="column"> the <b>visible</b> column from which to return the value. </param>
	/// <param name="format"> the format to use for formatting the value as a String. </param>
	/// <returns> the value in the position formatted as a String. </returns>
	public virtual string getValueAtAsString(int row, int column, string format)
	{
		object o = getValueAt(row, column);
		Type c = getColumnClass(getAbsoluteColumn(column));
		try
		{

		if (o == null)
		{
			return "";
		}
		else if (c == typeof(Integer))
		{
			int? I = (int?)o;
			if (COPY_MISSING_AS_EMPTY_STRING && DMIUtil.isMissing(I.Value))
			{
				format = getStringFormat(format);
				return StringUtil.formatString("", format);
			}
			else
			{
				if (!string.ReferenceEquals(format, null))
				{
					return StringUtil.formatString(I.Value, format);
				}
				else
				{
					return "" + I.Value;
				}
			}
		}
		else if (c == typeof(Double))
		{
			double? d = (double?)o;
			if (COPY_MISSING_AS_EMPTY_STRING && DMIUtil.isMissing(d.Value))
			{
				format = getStringFormat(format);
				return StringUtil.formatString("", format);
			}
			else
			{
				if (!string.ReferenceEquals(format, null))
				{
					return StringUtil.formatString(d.Value, format);
				}
				else
				{
					return "" + d.Value;
				}
			}
		}
		else if (c == typeof(System.DateTime))
		{
			System.DateTime d = (System.DateTime)o;
			if (DMIUtil.isMissing(d))
			{
				return "";
			}
			else
			{
				return "" + d;
			}
		}
		else if (c == typeof(string))
		{
			string s = (string)o;
			if (DMIUtil.isMissing(s))
			{
				return "";
			}
			else
			{
				return StringUtil.formatString(s, format);
			}
		}
		else if (c == typeof(Float))
		{
			float? F = (float?)o;
			if (COPY_MISSING_AS_EMPTY_STRING && DMIUtil.isMissing(F.Value))
			{
				return "";
			}
			else
			{
				return "" + F;
			}
		}
		else
		{
			return "" + o;
		}
		}
		catch (Exception e)
		{
			string routine = "JWorksheet.getValueAtAsString()";
			Message.printWarning(3, routine, e);
			Message.printWarning(3, "", "getValueAsString(" + row + ", " + column + ", " + format + "): class(" + getAbsoluteColumn(column) + ": " + c + "  data: " + o + "  data class: " + o.GetType());
		}
		return "" + o;
	}

	/// <summary>
	/// Translates the absolute column number (i.e., as used in the table model) to 
	/// the visible column number (in case columns have been removed). </summary>
	/// <param name="absoluteColumn"> the absolute column number.  Columns are numbered 
	/// starting at 0, though column 0 is usually the row count column. </param>
	/// <returns> the number of the column on the screen, or -1 if not visible. </returns>
	/// <seealso cref= #getAbsoluteColumn(int) </seealso>
	public virtual int getVisibleColumn(int absoluteColumn)
	{
		int hit = -1;
		for (int i = 0; i < absoluteColumn + 1; i++)
		{
			if (__columnRemoved[i] == false)
			{
				hit++;
			}
		}
		return hit;
	}

	/// <summary>
	/// Returns a count of the visible columns. </summary>
	/// <returns> a count of the visible columns. </returns>
	public virtual int getVisibleColumnCount()
	{
		int visColumnCount = 0;

		for (int i = 0; i < __columnRemoved.Length; i++)
		{
			if (__columnRemoved[i] == false)
			{
				visColumnCount++;
			}
		}
		return visColumnCount;
	}

	/// <summary>
	/// Returns the worksheet header. </summary>
	/// <returns> the worksheet header. </returns>
	public virtual JWorksheet_Header getWorksheetHeader()
	{
		return (JWorksheet_Header)getTableHeader();
	}

	/// <summary>
	/// Initializes data members in the worksheet, but mostly sets up the 
	/// specialized row and column selection models that allow selection to mimic
	/// Excel rather than the limited selection model provided by JTable. </summary>
	/// <param name="rows"> the number of rows in the worksheet. </param>
	/// <param name="cols"> the number of columns in the worksheet. </param>
	private void initialize(int rows, int cols)
	{
		string routine = CLASS + ".initialize";
		setAutoResizeMode(JTable.AUTO_RESIZE_OFF);

		if (getTableHeader() != null)
		{
			getTableHeader().addMouseListener(this);

			// cannot simply call "setTableHeader(__header)" here 
			// because then the table columns are always UNresizable 
			// (no matter what) from this point on.
			getTableHeader().setColumnModel(getColumnModel());

			getTableHeader().setReorderingAllowed(false);
			getTableHeader().setResizingAllowed(true);
		}

		if (__selectionMode == __EXCEL_SELECTION)
		{
			setSelectionMode(ListSelectionModel.MULTIPLE_INTERVAL_SELECTION);
			setCellSelectionEnabled(true);
			JWorksheet_RowSelectionModel r = new JWorksheet_RowSelectionModel(rows, cols);
			r.setPartner(__partner);
			if (!__selectable)
			{
				r.setSelectable(false);
			}

			JWorksheet_ColSelectionModel c = new JWorksheet_ColSelectionModel();
			r.setColSelectionModel(c);
			c.setRowSelectionModel(r);
			setSelectionModel(r);
			setOneClickRowSelection(__oneClickRowSelection);
			getColumnModel().setSelectionModel(c);
		}
		else
		{
			if (__selectionMode == __SINGLE_CELL_SELECTION)
			{
				setSelectionMode((__selectionMode - 100));
				setCellSelectionEnabled(true);
			}
			else
			{
				setSelectionMode(__selectionMode);
			}
		}

		if (__showPopup)
		{
			__popup = new JPopupMenu();
			JMenuItem mi = null;
			mi = new JMenuItem(__MENU_SORT_ASCENDING);
			mi.addActionListener(this);
			__popup.add(mi);
			mi = new JMenuItem(__MENU_SORT_DESCENDING);
			mi.addActionListener(this);
			__popup.add(mi);
			__cancelMenuItem = new JMenuItem(__MENU_ORIGINAL_ORDER);
			__cancelMenuItem.addActionListener(this);
			__popup.add(__cancelMenuItem);
			__cancelMenuItem.setEnabled(false);
			if (getTableHeader() != null)
			{
				getTableHeader().addMouseListener(this);
			}
		}

		__hcr = new JWorksheet_HeaderCellRenderer(__columnHeaderFontName, __columnHeaderFontStyle, __columnHeaderFontSize, SwingConstants.CENTER, __columnHeaderColor);

		TableColumn tc = null;
		for (int i = 0; i < getColumnCount(); i++)
		{
			if (Message.isDebugOn)
			{
				Message.printDebug(10, routine, "Setting column header " + "for column #" + i + ", '" + getColumnName(i) + "'");
			}
			tc = getColumnModel().getColumn(i);
	//		tc = getColumn(getColumnName(i));
			tc.setHeaderRenderer(__hcr);
		}

		setMultipleLineHeaderEnabled(true);

		setOneClickRowSelection(__oneClickRowSelection);
	}

	/// <summary>
	/// Inserts a new element to the table model, at the specified position. </summary>
	/// <param name="o"> the object to add to the table model. </param>
	/// <param name="pos"> the position at which to insert the record.  If the position is
	/// less than 0, nothing will be done.  If the position is greater than the number
	/// of records in the table, the record will be added at the very end. </param>
	public virtual void insertRowAt(object o, int pos)
	{
		__lastRowSelected = -1;
		string routine = CLASS + ".insertRowAt(Object, int)";

		if (pos < 0)
		{
			Message.printWarning(3, routine, "Attempting to insert at a negative position, not inserting.");
			return;
		}

		if (pos >= getRowCount())
		{
			addRow(o);
			return;
		}

		((JWorksheet_AbstractTableModel)getModel()).insertRowAt(o, pos);
		((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();

		if (__selectionMode == __EXCEL_SELECTION)
		{
			int rows = getRowCount();
			int cols = getColumnCount();
			setSelectionMode(ListSelectionModel.MULTIPLE_INTERVAL_SELECTION);
			setCellSelectionEnabled(true);
			JWorksheet_RowSelectionModel r = new JWorksheet_RowSelectionModel(rows, cols);
			r.setPartner(__partner);
			if (!__selectable)
			{
				r.setSelectable(false);
			}

			JWorksheet_ColSelectionModel c = new JWorksheet_ColSelectionModel();
			r.setColSelectionModel(c);
			c.setRowSelectionModel(r);
			setSelectionModel(r);
			setOneClickRowSelection(__oneClickRowSelection);
			getColumnModel().setSelectionModel(c);
		}

		if (__listRowHeader != null)
		{
			adjustListRowHeaderSize(__ROW_ADDED);
		}

		notifyAllWorksheetListeners(__ROW_ADDED, pos);
		adjustCellAttributesAndText(__ROW_ADDED, pos);
	}

	/// <summary>
	/// Determines whether a cell is editable or not by checking cell attributes and
	/// the normal JTable isCellEditable().  Overrides JTable.isCellEditable().  
	/// This method first checks to see if the ell in question has any attributes assigned to it.  
	/// If the cell has attributes and the value of 'editable' is set to false, the cell is returned as
	/// uneditable (false).  If there are no attributes set on the cell, a call is made to the default
	/// JTable isCelEditable() to return the value. </summary>
	/// <param name="row"> the row of the cell in question.  Rows are numbered starting at 0. </param>
	/// <param name="visibleColumn"> the <b>visible</b> column of the cell in question.  
	/// Columns are numbered starting at 0, though column 0 is usually the row count column. </param>
	/// <returns> whether the cell is editable or not </returns>
	public virtual bool isCellEditable(int row, int visibleColumn)
	{
		JWorksheet_CellAttributes ca = getCellAttributes(row, getAbsoluteColumn(visibleColumn));
		if (ca == null)
		{
			return base.isCellEditable(row, visibleColumn);
		}
		else
		{
			if (ca.editable == false)
			{
				return false;
			}
			else
			{
				return base.isCellEditable(row, visibleColumn);
			}
		}
	}

	/// <summary>
	/// Checks to see whether a cell is selected. </summary>
	/// <param name="row"> the row to check </param>
	/// <param name="col"> the <b>visible</b> column to check. </param>
	/// <returns> true if the cell is selected, false if not. </returns>
	public virtual bool isCellSelected(int row, int col)
	{
	//	Message.printStatus(1, "", "Checking cell selected: " + row + ", " + col);
		return base.isCellSelected(row, col);
	}

	/// <summary>
	/// Returns whether the worksheet is dirty. </summary>
	/// <returns> whether the worksheet is dirty. </returns>
	public virtual bool isDirty()
	{
		return __dirty;
	}

	/// <summary>
	/// Returns whether a cell in the worksheet is currently being edited. </summary>
	/// <returns> whether a cell in the worksheet is currently being edited. </returns>
	public virtual bool isEditing()
	{
		if (getCellEditor() != null)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Returns whether the table is empty or not. </summary>
	/// <returns> true if the table is empty (has no rows), or false if it has rows. </returns>
	public virtual bool isEmpty()
	{
		if (getRowCount() > 0)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Returns whether this worksheet's cells can be selected or not. </summary>
	/// <returns> whether this worksheet's cells can be selected or not. </returns>
	public virtual bool isSelectable()
	{
		return __selectable;
	}

	/// <summary>
	/// Returns whether the table is using the row headers that work similarly to the JTable column headers. </summary>
	/// <returns> whether the table is using row headers. </returns>
	protected internal virtual bool isUsingRowHeaders()
	{
		return __useRowHeaders;
	}

	/// <summary>
	/// Responds to key press events.  <br>
	/// TODO (JTS - 2003-11-17) What's this doing? (JTS - 2004-01-20) Still no clue. </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		/*
		TODO (JTS - 2004-11-22) commented out, see if anything misbehaves (I don't think we'll see
		any problems.
	
		// do nothing if a cell is being edited
		if (isEditing()) {
			return;
		}
		__isControlDown = false;
		__isShiftDown = false;
	
		// look for control-? events
		if (event.isControlDown()) {
			__isControlDown = true;
			int code = event.getKeyCode();
			if (code == event.VK_A) {
				selectAllRows();
			}
		}
	
		if (event.isShiftDown()) {
			__isShiftDown = true;
		}
		*/
	}

	/// 
	public virtual void keyReleased(KeyEvent @event)
	{
		__isControlDown = false;
		__isShiftDown = false;

		if (@event.isControlDown())
		{
			__isControlDown = true;
		}
		if (@event.isShiftDown())
		{
			__isShiftDown = true;
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Shows the popup menu if the mouse was pressed on the table header. </summary>
	/// <param name="event"> the MouseEvent that occurred. </param>
	private void maybeShowHeaderPopup(MouseEvent @event)
	{
		if (__showPopup)
		{
			if (__popup.isPopupTrigger(@event))
			{
				__popupColumn = columnAtPoint(new Point(@event.getX(), @event.getY()));
				__popup.show(@event.getComponent(), @event.getX(), @event.getY());
			}
		}
	}

	/// <summary>
	/// Shows the popup menu, if it has been set and if the table was right-clicked on. </summary>
	/// <param name="event"> the MouseEvent that happened. </param>
	private void maybeShowPopup(MouseEvent @event)
	{
		if (__mainPopup != null)
		{
			if (__mainPopup.isPopupTrigger(@event))
			{
				if (getSelectedRowCount() > 0)
				{
					__copyMenuItem.setEnabled(true);
					__copyHeaderMenuItem.setEnabled(true);
					__pasteMenuItem.setEnabled(true);
					__deselectAllMenuItem.setEnabled(true);
					__selectAllMenuItem.setEnabled(true);
				}
				else
				{
					__copyMenuItem.setEnabled(false);
					__copyHeaderMenuItem.setEnabled(false);
					__pasteMenuItem.setEnabled(false);
					__deselectAllMenuItem.setEnabled(false);
					__selectAllMenuItem.setEnabled(true);
				}
				__mainPopup.show(@event.getComponent(), @event.getX(), @event.getY());
			}
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// When a mouse key is pressed, if the header was clicked on and one click column
	/// selection is turned on, selects all the values in the clicked-on column. </summary>
	/// <param name="event"> the MouseEvent that occurred. </param>
	public virtual void mousePressed(MouseEvent @event)
	{
		Component c = @event.getComponent();

		// if the header was clicked on ...
		if (getTableHeader() != null && c == getTableHeader())
		{
			if (__oneClickColumnSelection)
			{
				int column = columnAtPoint(new Point(@event.getX(), 0));
				if (column == -1)
				{
					return;
				}
				selectColumn(column);
			}
		}
		else if (c == __listRowHeader)
		{
			if (__oneClickRowSelection)
			{
				int row = rowAtPoint(new Point(0, @event.getY()));
				if (row == -1)
				{
					return;
				}

				if ((!__isControlDown && !__isShiftDown) || (__lastRowSelected == -1))
				{
					selectRow(row, true);
				}
				else
				{
					if (__isControlDown && !__isShiftDown)
					{
						if (rowIsSelected(row))
						{
							deselectRow(row);
						}
						else
						{
							selectRow(row, false);
						}
					}
					else
					{
						int low = (row < __lastRowSelected ? row : __lastRowSelected);
						int high = (row > __lastRowSelected ? row : __lastRowSelected);
						if (__isShiftDown && !__isControlDown)
						{
							deselectAll();
						}

						for (int i = low; i <= high; i++)
						{
							selectRow(i, false);
						}
					}
				}

				if (__isShiftDown && !__isControlDown)
				{
					//__lastRowSelected = __lastRowSelected;
				}
				else
				{
					__lastRowSelected = row;
				}
			}
		}
	}

	/// <summary>
	/// When the mouse key is released, shows the popup menu if appropriate. </summary>
	/// <param name="event"> the MouseEvent that occurred. </param>
	public virtual void mouseReleased(MouseEvent @event)
	{
		Component c = @event.getComponent();

		// if the header was clicked on ...
		if (getTableHeader() != null && c == getTableHeader())
		{
			maybeShowHeaderPopup(@event);
		}
		else if (__worksheetHandlePopup)
		{
			maybeShowPopup(@event);
		}
	}

	/// <summary>
	/// Notifies all listeners of a specific message.  Listeners will have their 
	/// appropriate worksheetRowAdded(), worksheetRowDeleted(), or worksheetSetRowCount() methods called. </summary>
	/// <param name="message"> the message being sent. </param>
	/// <param name="row"> the row to which the message is referring. </param>
	public virtual void notifyAllWorksheetListeners(int message, int row)
	{
		if (__worksheetListeners == null)
		{
			return;
		}
		for (int i = 0; i < __worksheetListeners.Count; i++)
		{
			JWorksheet_Listener l = __worksheetListeners[i];
			switch (message)
			{
				case __ROW_ADDED:
					l.worksheetRowAdded(row);
					break;
				case __ROW_DELETED:
					l.worksheetRowDeleted(row);
					break;
				case __DATA_RESET:
					l.worksheetSetRowCount(row);
					break;
				case __SELECT_ALL:
					l.worksheetSelectAllRows(row);
					break;
				case __DESELECT_ALL:
					l.worksheetDeselectAllRows(row);
					break;
				default:
			break;
			}
		}
	}

	/// <summary>
	/// Notifies all the registered sort listeners that a sort is about to occur. </summary>
	/// <param name="sort"> the type of sort occurring, one of StringUtil.SORT_ASCENDING,
	/// StringUtil.SORT_DESCENDING, or -1 if the original order is being restored. </param>
	private void notifySortListenersSortAboutToChange(int sort)
	{
		if (__sortListeners == null)
		{
			return;
		}

		JWorksheet_SortListener listener = null;
		int size = __sortListeners.Count;
		for (int i = 0; i < size; i++)
		{
			listener = __sortListeners[i];
			listener.worksheetSortAboutToChange(this, sort);
		}
	}

	/// <summary>
	/// Notifies all the registered sort listeners that a sort has occurred. </summary>
	/// <param name="sort"> the type of sort that occurred, one of StringUtil.SORT_ASCENDING,
	/// StringUtil.SORT_DESCENDING, or -1 if the original order is being restored. </param>
	private void notifySortListenersSortChanged(int sort)
	{
		if (__sortListeners == null)
		{
			return;
		}

		JWorksheet_SortListener listener = null;
		int size = __sortListeners.Count;
		for (int i = 0; i < size; i++)
		{
			listener = __sortListeners[i];
			listener.worksheetSortChanged(this, sort);
		}
	}

	/// <summary>
	/// Attempts to paste the values in the clipboard into the worksheet.
	/// </summary>
	public virtual void pasteFromClipboard()
	{
		if (__copyPasteAdapter == null)
		{
			__copyPasteAdapter = new JWorksheet_CopyPasteAdapter(this);
		}
		__copyPasteAdapter.paste();
	}

	/// <summary>
	/// Prepares the JWorksheet to render a cell; overrides the normal JTable 
	/// prepareRender call.  This sets attributes on the cell if the cell has 
	/// attributes.  Programmers should not need to call this.  It is public because
	/// it overrides JTable.prepareRenderer(). </summary>
	/// <param name="tcr"> the TableCellRenderer used to render the cell </param>
	/// <param name="row"> the row of the cell </param>
	/// <param name="column"> the <b>visible</b> column of the cell </param>
	/// <returns> the rendered cell. </returns>
	public virtual Component prepareRenderer(TableCellRenderer tcr, int row, int column)
	{
		JWorksheet_CellAttributes ca = null;

		// only set the default row count column attributes for the 0th 
		// column if the row count is present.  Otherwise, the 0th column
		// attributes need to be set manually
		if (column == 0 && __showRowCountColumn && !__useRowHeaders)
		{
			ca = __rowCountColumnAttributes;
		}
		else
		{
			ca = getCellAttributes(row, getAbsoluteColumn(column));
		}

		Component cell = base.prepareRenderer(tcr, row, column);
		if (__altTextCount > 0)
		{
			if (cell is JLabel)
			{
				string text = getCellAlternateText(row, getAbsoluteColumn(column));
				if (!string.ReferenceEquals(text, null))
				{
					((JLabel)cell).setText(text);
				}
			}
		}

		bool selected = isCellSelected(row, column);
		if (ca == null)
		{
			cell = applyCellAttributes(cell, null, selected);
			return cell;
		}
		else
		{
			cell = applyCellAttributes(cell, ca, selected);
			return cell;
		}
	}

	/// <summary>
	/// Reads the provided proplist and sets up values within the JWorksheet. </summary>
	/// <param name="p"> the PropList containing JWorksheet setup values. </param>
	private void readPropList(PropList p)
	{
		string routine = "JWorksheet.readPropList";

		JTableHeader header = new JTableHeader();
		int hfsize = 12;
		int hfstyle = Font.BOLD;
		string hfname = "Arial";

		// SAM 2007-05-09 Evaluate use
		//Color cellBackground = (Color)(getClientProperty("Table.background"));
		//Color cellForeground = (Color)(getClientProperty("Table.foreground"));
		Color headerBackground = (Color)(header.getClientProperty("TableHeader.background"));
		//Color headerForeground = (Color)(header.getClientProperty(
			//"TableHeader.foreground"));

		int tfsize = 11;
		int tfstyle = Font.PLAIN;
		string tfname = "Arial";

		bool paste = false;
		bool copy = false;

		if (p == null)
		{
			__cellFontName = tfname;
			__cellFontStyle = tfstyle;
			__cellFontSize = tfsize;
			__columnHeaderFontName = hfname;
			__rowHeaderFontName = hfname;
			__columnHeaderFontStyle = hfstyle;
			__rowHeaderFontStyle = hfstyle;
			__columnHeaderFontSize = hfsize;
			__rowHeaderFontSize = hfsize;
			__columnHeaderColor = headerBackground;
			__rowHeaderColor = headerBackground;
			__showRowCountColumn = false;
			__useRowHeaders = false;
			__showPopup = false;
			__selectionMode = __EXCEL_SELECTION;
			__oneClickRowSelection = false;
			__oneClickColumnSelection = false;
			return;
		}

		string s = p.getValue("JWorksheet.CellFontName");
		if (!string.ReferenceEquals(s, null))
		{
			__cellFontName = s;
		}
		else
		{
			__cellFontName = tfname;
		}

		s = p.getValue("JWorksheet.CellFontStyle");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("Plain", StringComparison.OrdinalIgnoreCase))
			{
				__cellFontStyle = Font.PLAIN;
			}
			else if (s.Equals("Bold", StringComparison.OrdinalIgnoreCase))
			{
				__cellFontStyle = Font.BOLD;
			}
			else if (s.Equals("Italic", StringComparison.OrdinalIgnoreCase))
			{
				__cellFontStyle = Font.ITALIC;
			}
		}
		else
		{
			__cellFontStyle = tfstyle;
		}

		s = p.getValue("JWorksheet.CellFontSize");
		if (!string.ReferenceEquals(s, null))
		{
			__cellFontSize = (Convert.ToInt32(s));
		}
		else
		{
			__cellFontSize = tfsize;
		}

		s = p.getValue("JWorksheet.ColumnHeaderFontName");
		if (!string.ReferenceEquals(s, null))
		{
			__columnHeaderFontName = s;
		}
		else
		{
			__columnHeaderFontName = hfname;
		}

		s = p.getValue("JWorksheet.RowHeaderFontName");
		if (!string.ReferenceEquals(s, null))
		{
			__rowHeaderFontName = s;
		}
		else
		{
			__rowHeaderFontName = hfname;
		}

		s = p.getValue("JWorksheet.ColumnHeaderFontStyle");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("Plain", StringComparison.OrdinalIgnoreCase))
			{
				__columnHeaderFontStyle = Font.PLAIN;
			}
			else if (s.Equals("Bold", StringComparison.OrdinalIgnoreCase))
			{
				__columnHeaderFontStyle = Font.BOLD;
			}
			else if (s.Equals("Italic", StringComparison.OrdinalIgnoreCase))
			{
				__columnHeaderFontStyle = Font.ITALIC;
			}
		}
		else
		{
			__columnHeaderFontStyle = hfstyle;
		}

		s = p.getValue("JWorksheet.RowHeaderFontStyle");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("Plain", StringComparison.OrdinalIgnoreCase))
			{
				__rowHeaderFontStyle = Font.PLAIN;
			}
			else if (s.Equals("Bold", StringComparison.OrdinalIgnoreCase))
			{
				__rowHeaderFontStyle = Font.BOLD;
			}
			else if (s.Equals("Italic", StringComparison.OrdinalIgnoreCase))
			{
				__rowHeaderFontStyle = Font.ITALIC;
			}
		}
		else
		{
			__rowHeaderFontStyle = hfstyle;
		}

		s = p.getValue("JWorksheet.ColumnHeaderFontSize");
		if (!string.ReferenceEquals(s, null))
		{
			__columnHeaderFontSize = (Convert.ToInt32(s));
		}
		else
		{
			__columnHeaderFontSize = hfsize;
		}

		s = p.getValue("JWorksheet.RowHeaderFontSize");
		if (!string.ReferenceEquals(s, null))
		{
			__rowHeaderFontSize = (Convert.ToInt32(s));
		}
		else
		{
			__rowHeaderFontSize = hfsize;
		}

		s = p.getValue("JWorksheet.ColumnHeaderBackground");
		if (!string.ReferenceEquals(s, null))
		{
			__columnHeaderColor = (Color)GRColor.parseColor(s);
		}
		else
		{
			__columnHeaderColor = headerBackground;
		}

		s = p.getValue("JWorksheet.RowHeaderBackground");
		if (!string.ReferenceEquals(s, null))
		{
			__rowHeaderColor = (Color)GRColor.parseColor(s);
		}
		else
		{
			__rowHeaderColor = headerBackground;
		}

		s = p.getValue("JWorksheet.RowColumnPresent");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__showRowCountColumn = true;
			}
			else
			{
				__showRowCountColumn = false;
			}
		}

		s = p.getValue("JWorksheet.ShowRowHeader");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__showRowCountColumn = true;
				__useRowHeaders = true;
			}
			else
			{
				__useRowHeaders = false;
			}
		}

		s = p.getValue("JWorksheet.RowColumnBackground");
		if (!string.ReferenceEquals(s, null))
		{
			__rowCountColumnAttributes.backgroundColor = GRColor.parseColor(s);
		}

		s = p.getValue("JWorksheet.ShowPopupMenu");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__showPopup = true;
			}
			else
			{
				__showPopup = false;
			}
		}

		s = p.getValue("JWorksheet.SelectionMode");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("SingleRowSelection", StringComparison.OrdinalIgnoreCase))
			{
				__selectionMode = __SINGLE_ROW_SELECTION;
			}
			else if (s.Equals("MultipleRowSelection", StringComparison.OrdinalIgnoreCase))
			{
				__selectionMode = __MULTIPLE_ROW_SELECTION;
			}
			else if (s.Equals("MultipleDiscontinuousRowSelection", StringComparison.OrdinalIgnoreCase))
			{
				__selectionMode = __MULTIPLE_DISCONTINUOUS_ROW_SELECTION;
			}
			else if (s.Equals("SingleCellSelection", StringComparison.OrdinalIgnoreCase))
			{
				__selectionMode = __SINGLE_CELL_SELECTION;
			}
			else if (s.Equals("ExcelSelection", StringComparison.OrdinalIgnoreCase))
			{
				__selectionMode = __EXCEL_SELECTION;
			}
			else
			{
				Message.printWarning(3, routine, "Unrecognized selection mode: " + s);
				__selectionMode = __EXCEL_SELECTION;
			}
		}
		else
		{
			__selectionMode = __EXCEL_SELECTION;
		}

		s = p.getValue("JWorksheet.OneClickRowSelection");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__oneClickRowSelection = true;
			}
			else
			{
				__oneClickRowSelection = false;
			}
		}
		else
		{
			__oneClickRowSelection = false;
		}

		s = p.getValue("JWorksheet.OneClickColumnSelection");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__oneClickColumnSelection = true;
			}
			else
			{
				__oneClickColumnSelection = false;
			}
		}
		else
		{
			__oneClickColumnSelection = false;
		}

		s = p.getValue("JWorksheet.Unselectable");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				setSelectable(false);
			}
			Message.printWarning(3, routine, "Unselectable is being phased out.  Use property 'Selectable' instead.");
		}

		s = p.getValue("JWorksheet.Selectable");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("false", StringComparison.OrdinalIgnoreCase))
			{
				setSelectable(false);
			}
		}

		s = p.getValue("JWorksheet.AllowCopy");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				copy = true;
				__copyEnabled = true;
			}
		}

		s = p.getValue("JWorksheet.AllowPaste");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				paste = true;
				__pasteEnabled = true;
			}
		}

		s = p.getValue("JWorksheet.AllowCalculateStatistics");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__calculateStatisticsEnabled = true;
			}
		}
		s = p.getValue("JWorksheet.DelegateCalculateStatistics");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				__delegateCalculateStatistics = true;
			}
		}

		setCopyPasteEnabled(copy, paste);
		if (paste || copy)
		{
			__mainPopup = new JPopupMenu();
			addMouseListener(this);
			setupPopupMenu(__mainPopup, true);
		}

		// check for old properties no longer supported
		s = p.getValue("JWorksheet.HeaderFont");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "HeaderFont) is no longer supported.  Use JWorksheet.ColumnHeaderFontName instead.");
		}

		s = p.getValue("JWorksheet.HeaderFontName");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "HeaderFontName) is no longer supported.  Use JWorksheet.ColumnHeaderFontName instead.");
		}

		s = p.getValue("JWorksheet.HeaderFontStyle");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "HeaderFontStyle) is no longer supported.  Use JWorksheet.ColumnHeaderFontStyle instead.");
		}

		s = p.getValue("JWorksheet.HeaderSize");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "HeaderSize) is no longer supported.  Use JWorksheet.ColumnHeaderFontSize instead.");
		}

		s = p.getValue("JWorksheet.HeaderBackground");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "HeaderBackground) is no longer supported.  Use JWorksheet.ColumnHeaderBackground instead.");
		}

		s = p.getValue("JWorksheet.HeaderStyle");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "HeaderStyle) is no longer supported.  Use JWorksheet.ColumnHeaderFontStyle instead.");
		}

		s = p.getValue("JWorksheet.HeaderSize");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "HeaderSize) is no longer supported.  Use JWorksheet.ColumnHeaderFontSize instead.");
		}

		s = p.getValue("JWorksheet.CellFont");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "CellFont) is no longer supported.  Use JWorksheet.CellFontName instead.");
		}

		s = p.getValue("JWorksheet.CellStyle");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "CellStyle) is no longer supported.  Use JWorksheet.CellFontStyle instead.");
		}

		s = p.getValue("JWorksheet.CellSize");
		if (!string.ReferenceEquals(s, null))
		{
			Message.printWarning(3, routine, "This property (JWorksheet." + "CellSize) is no longer supported.  Use JWorksheet.CellFontSize instead.");
		}

		s = p.getValue("JWorksheet.ColumnNumbering");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Equals("Base0", StringComparison.OrdinalIgnoreCase))
			{
				__columnNumbering = __NUMBERING_0;
			}
			else if (s.Equals("Base1", StringComparison.OrdinalIgnoreCase))
			{
				__columnNumbering = __NUMBERING_1;
			}
			else if (s.Equals("Excel", StringComparison.OrdinalIgnoreCase))
			{
				__columnNumbering = __NUMBERING_EXCEL;
			}
			else if (s.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				__columnNumbering = __NUMBERING_NONE;
			}
		}

		s = p.getValue("JWorksheet.FirstRowNumber");
		if (!string.ReferenceEquals(s, null))
		{
			if (StringUtil.isInteger(s))
			{
				__firstRowNum = StringUtil.atoi(s);
			}
		}

		s = p.getValue("JWorksheet.IncrementRowNumbers");
		if (!string.ReferenceEquals(s, null))
		{
			if (s.Trim().Equals("false", StringComparison.OrdinalIgnoreCase))
			{
				__incrementRowNumbers = false;
			}
		}
	}

	/// <summary>
	/// Refreshes the table, repainting all the visible cells.<para>
	/// <b>IMPORTANT!</b>  This method currently will NOT redraw the currently
	/// selected cell in certain cases (such as when SingleCellSelection is turned on),
	/// so do not rely on this 100% to repaint the selected cells.
	/// TODO JTS to REVISIT as soon as possible.
	/// </para>
	/// </summary>
	public virtual void refresh()
	{
		((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();
	}

	/// <summary>
	/// Removes a column from the table so that it doesn't appear any more. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number of the column to prevent from displaying.
	/// Columns are numbered starting at 0, though column 0 is usually the row count column. </param>
	public virtual void removeColumn(int absoluteColumn)
	{
		int vis = getVisibleColumn(absoluteColumn);
		if (__columnRemoved[absoluteColumn] == true)
		{
			return;
		}
		else
		{
			__columnRemoved[absoluteColumn] = true;
		}

		TableColumn tc = getColumnModel().getColumn(vis);
		removeColumn(tc);

		if (__selectionMode == __EXCEL_SELECTION)
		{
			JWorksheet_RowSelectionModel r = new JWorksheet_RowSelectionModel(getRowCount(), getColumnCount());
			r.setPartner(__partner);
			if (!__selectable)
			{
				r.setSelectable(false);
			}
			JWorksheet_ColSelectionModel c = new JWorksheet_ColSelectionModel();
			r.setColSelectionModel(c);
			c.setRowSelectionModel(r);
			setSelectionModel(r);
			setOneClickRowSelection(__oneClickRowSelection);
			getColumnModel().setSelectionModel(c);
			adjustCellAttributesAndText(__COL_DELETED, absoluteColumn);
		}
	}

	/// <summary>
	/// Removes the column header from each column.
	/// </summary>
	public virtual void removeColumnHeader()
	{
		TableColumn tc = null;
		for (int i = 0; i < getColumnCount(); i++)
		{
			tc = getColumnModel().getColumn(i);
			tc.setHeaderRenderer(null);
		}
	}

	/// <summary>
	/// Sets a column to use the default cell editor if it has been to set to use a SimpleJComboBox as an editor. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number of the column for 
	/// which to remove a SimpleJComboBox as an editor.  Columns are numbered starting
	/// at 0, though column 0 is usually the row count column. </param>
	public virtual void removeColumnJComboBox(int absoluteColumn)
	{
		TableColumn col = getColumnModel().getColumn(getVisibleColumn(absoluteColumn));
		col.setCellEditor(null);
	}

	/// <summary>
	/// Removes a JWorksheet_Listener from the list of registered listeners. </summary>
	/// <param name="l"> the listener to remove. </param>
	public virtual void removeJWorksheetListener(JWorksheet_Listener l)
	{
		for (int i = 0; i < __worksheetListeners.Count; i++)
		{
			if (l == __worksheetListeners[i])
			{
				__worksheetListeners.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Removes a mouse listener from both the worksheet and its header. </summary>
	/// <param name="l"> the MouseListener to remove. </param>
	public virtual void removeMouseListener(MouseListener l)
	{
		base.removeMouseListener(l);
		if (getTableHeader() != null)
		{
			getTableHeader().removeMouseListener(l);
		}
	}

	/// <summary>
	/// Removes a sort listener from the list of registered sort listeners. </summary>
	/// <param name="listener"> the listener to remove. </param>
	public virtual void removeSortListener(JWorksheet_SortListener listener)
	{
		if (__sortListeners == null)
		{
			return;
		}

		int size = __sortListeners.Count;
		for (int i = (size - 1); i <= 0; i--)
		{
			if (__sortListeners[i] == listener)
			{
				__sortListeners.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Returns whether a row is selected or not. </summary>
	/// <param name="row"> the row to check for whethether it is selected. </param>
	/// <returns> true if the row is selected, false if not. </returns>
	public virtual bool rowIsSelected(int row)
	{
		int[] rows = getSelectedRows();
		for (int i = 0; i < rows.Length; i++)
		{
			if (rows[i] == row)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Opens a file chooser for selecting a file with a delimiter type and then writes
	/// out all data from the table to that file with that delimiter.
	/// </summary>
	public virtual void saveToFile()
	{
		JGUIUtil.setWaitCursor(this, true);
		JFileChooser jfc = JFileChooserFactory.createJFileChooser(JGUIUtil.getLastFileDialogDirectory());

		jfc.setDialogTitle("Save Worksheet to File");
		SimpleFileFilter comma = new SimpleFileFilter("csv", "Comma-delimited text file");
		SimpleFileFilter commatxt = new SimpleFileFilter("txt", "Comma-delimited text file");
		SimpleFileFilter tab = new SimpleFileFilter("txt", "Tab-delimited text file");
		SimpleFileFilter semicolon = new SimpleFileFilter("txt", "Semicolon-delimited text file");
		jfc.addChoosableFileFilter(comma);
		jfc.addChoosableFileFilter(commatxt);
		jfc.addChoosableFileFilter(tab);
		jfc.addChoosableFileFilter(semicolon);
		jfc.setAcceptAllFileFilterUsed(false);
		jfc.setFileFilter(comma);
		jfc.setDialogType(JFileChooser.SAVE_DIALOG);
		JGUIUtil.setWaitCursor(this, false);

		int retVal = jfc.showSaveDialog(this);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return;
		}

		string currDir = (jfc.getCurrentDirectory()).ToString();
		JGUIUtil.setLastFileDialogDirectory(currDir);

		JGUIUtil.setWaitCursor(this, true);

		string filename = jfc.getSelectedFile().getPath();

		string delimiter = ",";
		if (jfc.getFileFilter() == comma)
		{
			if (!StringUtil.endsWithIgnoreCase(filename, ".csv"))
			{
				filename += ".csv";
			}
		}
		else if (jfc.getFileFilter() == commatxt)
		{
			if (!StringUtil.endsWithIgnoreCase(filename, ".txt"))
			{
				filename += ".txt";
			}
		}
		if (jfc.getFileFilter() == tab)
		{
			delimiter = "\t";
			if (!StringUtil.endsWithIgnoreCase(filename, ".txt"))
			{
				filename += ".txt";
			}
		}
		else if (jfc.getFileFilter() == semicolon)
		{
			delimiter = ";";
			if (!StringUtil.endsWithIgnoreCase(filename, ".txt"))
			{
				filename += ".txt";
			}
		}
		saveToFile(filename, delimiter);
	}

	/// <summary>
	/// Saves the contents of the worksheet (in all visible columns) to a file. </summary>
	/// <param name="filename"> the name of the file to which to write. </param>
	/// <param name="delimiter"> the delimiter to use for separating field values. </param>
	public virtual void saveToFile(string filename, string delimiter)
	{
		string routine = "saveToFile";

		IList<string> lines = new List<string>();

		int numRows = getRowCount();
		int numCols = getColumnCount();

		StringBuilder line = new StringBuilder();

		for (int i = 0; i < numCols; i++)
		{
			line.Append("\"");
			line.Append(getColumnName(i, true));
			line.Append("\"");
			if (i < numCols - 1)
			{
				line.Append(delimiter);
			}
		}
		lines.Add(line.ToString());

		string s = null;
		bool quote = false;

		for (int i = 0; i < numRows; i++)
		{
			line = new StringBuilder();
			for (int j = 0; j < numCols; j++)
			{
				quote = false;
				s = getValueAtAsString(i, j);

				// Check to see if the field contains the delimiter.
				// If it does, the field string needs to be quoted.
				if (s.IndexOf(delimiter, StringComparison.Ordinal) > -1)
				{
					quote = true;
				}

				// Remove any newlines.
				if (s.IndexOf("\n", StringComparison.Ordinal) > -1)
				{
					s = StringUtil.replaceString(s, "\n", "");
				}

				if (quote)
				{
					line.Append("\"");
				}

				line.Append(s);

				if (quote)
				{
					line.Append("\"");
				}

				if (j < numCols - 1)
				{
					line.Append(delimiter);
				}
			}
			lines.Add(line.ToString());
		}

		// Create a new FileOutputStream wrapped with a DataOutputStream for writing to a file.
		try
		{
			PrintWriter oStream = new PrintWriter(new StreamWriter(filename));

			// Write each element of the lines list to a file.
			// For some reason, when just using println in an
			// applet, the cr-nl pair is not output like it should be on Windows95.  Java Bug???
			string linesep = System.getProperty("line.separator");
			for (int i = 0; i < lines.Count; i++)
			{
				oStream.print(lines[i].ToString() + linesep);
			}
			oStream.flush();
			oStream.close();
		}
		catch (Exception e)
		{
			JGUIUtil.setWaitCursor(this, false);
			Message.printWarning(3, routine, "Error writing to file.");
			Message.printWarning(3, routine, e);
		}

		JGUIUtil.setWaitCursor(this, false);
	}

	/// <summary>
	/// Scrolls the table to the specified cell. </summary>
	/// <param name="row"> the row of the cell </param>
	/// <param name="visibleColumn"> the <b>visible</b> column number of the cell. </param>
	public virtual void scrollToCell(int row, int visibleColumn)
	{
		scrollRectToVisible(getCellRect(row, visibleColumn, true));
	}

	/// <summary>
	/// Scrolls to the last row of data.
	/// </summary>
	public virtual void scrollToLastRow()
	{
		scrollToRow(getRowCount() - 1);
	}

	/// <summary>
	/// Scrolls the table to the specified row. </summary>
	/// <param name="row"> the row to scroll to.  Rows are numbered starting at 0. </param>
	public virtual void scrollToRow(int row)
	{
		string routine = CLASS + ".scrollToRow";

		if (row >= getRowCount())
		{
			Message.printWarning(3, routine, "Will not scroll to row " + row + ", total row count is " + getRowCount());
			return;
		}
		if (row < 0)
		{
			Message.printWarning(3, routine, "Will not scroll to negative row");
			return;
		}
		scrollToCell(row, 0);
	}

	/// <summary>
	/// Selects all the rows in the worksheet.
	/// </summary>
	public virtual void selectAllRows()
	{
		notifyAllWorksheetListeners(__SELECT_ALL, PRE_SELECTION_CHANGE);
		int size = getRowCount();

		if (size == 0)
		{
			return;
		}

		if (__selectionMode == __EXCEL_SELECTION)
		{
			((JWorksheet_RowSelectionModel)getSelectionModel()).selectAllRows();
		}
		else
		{
			((DefaultListSelectionModel)getSelectionModel()).setSelectionInterval(0, size);
		}
		notifyAllWorksheetListeners(__SELECT_ALL, POST_SELECTION_CHANGE);
	}

	/// <summary>
	/// Selects a single cell. </summary>
	/// <param name="row"> the row of the cell to select. </param>
	/// <param name="visibleColumn"> the <b>visible</b> column to select. </param>
	public virtual void selectCell(int row, int visibleColumn)
	{
		if (row < 0 || visibleColumn < 0)
		{
			return;
		}

		if (__selectionMode == __EXCEL_SELECTION)
		{
			((JWorksheet_RowSelectionModel)getSelectionModel()).selectCell(row, visibleColumn);
		}
		else
		{
			setRowSelectionInterval(row, row);
			setColumnSelectionInterval(visibleColumn, visibleColumn);
		}
	}

	/// <summary>
	/// Selects a column. </summary>
	/// <param name="visibleColumn"> the <b>visible</b> column to select. </param>
	public virtual void selectColumn(int visibleColumn)
	{
		if (__selectionMode == __EXCEL_SELECTION)
		{
			((JWorksheet_RowSelectionModel)getSelectionModel()).selectColumn(visibleColumn);
		}
		else
		{
			setColumnSelectionInterval(visibleColumn, visibleColumn);
		}
	}

	/// <summary>
	/// Selects the last row of data.  
	/// </summary>
	public virtual void selectLastRow()
	{
		selectRow(getRowCount() - 1);
	}

	/// <summary>
	/// Programmatically selects the specified row -- but does not scroll to the row.
	/// Call scrollToRow(row) for that.  Deselects all the rows prior to selecting the new row. </summary>
	/// <param name="row"> the row to select.  Rows are numbered starting at 0. </param>
	public virtual void selectRow(int row)
	{
		selectRow(row, true);
	}

	/// <summary>
	/// Programmatically selects the specified row -- but does not scroll to the row.  Call scrollToRow(row) for that. </summary>
	/// <param name="row"> the row to select.  Rows are numbered starting at 0. </param>
	/// <param name="deselectFirst"> if true, then all other selected rows will be deselected
	/// prior to the row being selected.  Otherwise, this row and all the currently-
	/// selectedted rows will end up being selected. </param>
	public virtual void selectRow(int row, bool deselectFirst)
	{
		string routine = CLASS + ".selectRow";

		if (row >= getRowCount())
		{
			Message.printWarning(3, routine, "Cannot select row " + row + ", there are only " + getRowCount() + " rows in the worksheet.");
			return;
		}
		if (row < 0)
		{
			Message.printWarning(3, routine, "Cannot select a negative row.");
			return;
		}

		if (deselectFirst)
		{
			deselectAll();
		}

		if (deselectFirst)
		{
			if (__selectionMode == __EXCEL_SELECTION)
			{
				((JWorksheet_RowSelectionModel)getSelectionModel()).selectRow(row);
			}
			else
			{
				((DefaultListSelectionModel)getSelectionModel()).setSelectionInterval(row, row);
			}
		}
		else
		{
			if (__selectionMode == __EXCEL_SELECTION)
			{
				((JWorksheet_RowSelectionModel)getSelectionModel()).selectRowWithoutDeselecting(row);
			}
			else
			{
				((DefaultListSelectionModel)getSelectionModel()).addSelectionInterval(row, row);
			}
		}
	}

	/// <summary>
	/// Sets cell alternate text for a specified cell.  If the cell already has 
	/// alternate text, it is replaced with the new text.  If the specified text is
	/// null, the alternate text is removed from the cell.<para>
	/// Alternate text can be used so that the worksheet shows other values temporarily,
	/// and the actual data stored in the cells do not need to be disturbed.  For 
	/// instance, this is used in the HydroBase Water Information Sheets to set certain
	/// cells to say "DRY" when the user clicks a button.  If the user clicks the button
	/// again, the "DRY" goes away (the alternate text is removed) and the user can 
	/// then see the data that appears in the cell.
	/// </para>
	/// </summary>
	/// <param name="row"> the row of the cell.  Rows are numbered starting at 0. </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column of the cell.  Columns are 
	/// numbered starting at 0, though column 0 is usually the row count column. </param>
	/// <param name="text"> the alternate text to set </param>
	public virtual void setCellAlternateText(int row, int absoluteColumn, string text)
	{
		string routine = CLASS + ".setCellAlternateText";

		if (row < 0 || absoluteColumn < 0)
		{
			Message.printWarning(3, routine, "Row " + row + " or column " + absoluteColumn + " is out of bounds.");
			return;
		}

		// passing in null alt text removes the alt text for the specified cell
		if (string.ReferenceEquals(text, null))
		{
			int visCol = getVisibleColumn(absoluteColumn);
			if (__altTextCount > 0 && absoluteColumn > -1)
			{
				for (int i = 0; i < __altTextCols.Length; i++)
				{
					if (__altTextCols[i] == visCol && __altTextRows[i] == row)
					{
						__altTextCols[i] = -1;
						__altTextRows[i] = -1;
						__altText[i] = null;
						__altTextCount--;
	//					Message.printStatus(1, "", "Cell alt text removed for: " + row + ", " + visCol);
						compactAltTextArrays();
						refresh();
						return;
					}
				}
			}
			return;
		}

		int visCol = getVisibleColumn(absoluteColumn);
		// search to see if the cell already has alt text, and if so, reset it to the new alt text 
		if (__altTextCount > 0)
		{
			for (int i = 0; i < __altTextCols.Length; i++)
			{
				if (__altTextCols[i] == visCol && __altTextRows[i] == row)
				{
					__altText[i] = text;
					refresh();
	//				Message.printStatus(1, "", 
	//					"Cell alt text replaced old cell alt text at: " + row + ", " + visCol);
					return;
				}
			}
		}

		// otherwise, add a new alt text to the array.  Check the array sizes and resize if necessary
		if (((__altTextCount + 1) % __ARRAY_SIZE) == 0)
		{
	//		Message.printStatus(1, "", "Need to resize data arrays to: " + (__altTextCount + __ARRAY_SIZE));
			int[] temp = new int[(__altTextCount + 1) + __ARRAY_SIZE];
			for (int i = 0; i < temp.Length; i++)
			{
				temp[i] = -1;
			}
			Array.Copy(__altTextCols, 0, temp, 0, __altTextCount);
			__altTextCols = temp;

			temp = new int[(__altTextCount + 1) + __ARRAY_SIZE];
			for (int i = 0; i < temp.Length; i++)
			{
				temp[i] = -1;
			}
			Array.Copy(__altTextRows, 0, temp, 0, __altTextCount);
			__altTextRows = temp;

			string[] tempat = new string[(__altTextCount + 1) + __ARRAY_SIZE];
			Array.Copy(__altText, 0, tempat, 0, __altTextCount);
			__altText = tempat;
		}

		// the arrays are always compacted when alt text is removed,
		// so the __altTextCount var can be used safely for putting a new 
		// alt text at the very end of the array
	//	Message.printStatus(1, "", "Alt text set at the very end for " + row + ", " + visCol);
		__altTextCols[__altTextCount] = getVisibleColumn(absoluteColumn);
		__altTextRows[__altTextCount] = row;
		__altText[__altTextCount] = text;
		__altTextCount++;

		refresh();
	}

	/// <summary>
	/// Sets cell attributes for a specified cell.  If the cell already has attributes,
	/// they are replaced with the new attributes.  If the specified cell attributes are
	/// null, the attributes are removed from the cell. </summary>
	/// <param name="row"> the row of the cell.  Rows are numbered starting at 0. </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column of the cell.  Columns are 
	/// numbered starting at 0, though column 0 is usually the row count column. </param>
	/// <param name="ca"> the cell attributes to set </param>
	public virtual void setCellAttributes(int row, int absoluteColumn, JWorksheet_CellAttributes ca)
	{
		string routine = CLASS + ".setCellAttributes";

		if (row < 0 || absoluteColumn < 0)
		{
			Message.printWarning(3, routine, "Row " + row + " or column " + absoluteColumn + " is out of bounds.");
			return;
		}

		// passing in null cell attributes removes the cell attributes for the specified cell
		if (ca == null)
		{
			int visCol = getVisibleColumn(absoluteColumn);
			if (__attrCount > 0 && absoluteColumn > -1)
			{
				for (int i = 0; i < __attrCols.Length; i++)
				{
					if (__attrCols[i] == visCol && __attrRows[i] == row)
					{
						__attrCols[i] = -1;
						__attrRows[i] = -1;
						__cellAttrs[i] = null;
						__attrCount--;
	//					Message.printStatus(1, "", "Cell attributes removed for: " + row + ", " + visCol);
						compactAttrArrays();
						refresh();
						return;
					}
				}
			}
			return;
		}

		int visCol = getVisibleColumn(absoluteColumn);
		// search to see if the cell already has cell attributes, and if
		// so, reset them to the new attributes
		if (__attrCount > 0)
		{
			for (int i = 0; i < __attrCols.Length; i++)
			{
				if (__attrCols[i] == visCol && __attrRows[i] == row)
				{
					__cellAttrs[i] = (JWorksheet_CellAttributes)ca.clone();
					refresh();
	//				Message.printStatus(1, "", "Cell attributes replaced old cell attributes at: " + row + ", " + visCol);
					return;
				}
			}
		}

		// otherwise, add a new attribute to the array.  Check the array sizes and resize if necessary
		if (((__attrCount + 1) % __ARRAY_SIZE) == 0)
		{
	//		Message.printStatus(1, "", "Need to resize data arrays to: " + (__attrCount + __ARRAY_SIZE));
			int[] temp = new int[(__attrCount + 1) + __ARRAY_SIZE];
			for (int i = 0; i < temp.Length; i++)
			{
				temp[i] = -1;
			}
			Array.Copy(__attrCols, 0, temp, 0, __attrCount);
			__attrCols = temp;

			temp = new int[(__attrCount + 1) + __ARRAY_SIZE];
			for (int i = 0; i < temp.Length; i++)
			{
				temp[i] = -1;
			}
			Array.Copy(__attrRows, 0, temp, 0, __attrCount);
			__attrRows = temp;

			JWorksheet_CellAttributes[] tempca = new JWorksheet_CellAttributes[(__attrCount + 1) + __ARRAY_SIZE];
			Array.Copy(__cellAttrs, 0, tempca, 0, __attrCount);
			__cellAttrs = tempca;
		}

		// the arrays are always compacted when cell attributes are removed,
		// so the __attrCount var can be used safely for putting a new attribute
		// at the very end of the array
	//	Message.printStatus(1, "", "Cell attribute set at the very end for " + row + ", " + visCol);
		__attrCols[__attrCount] = getVisibleColumn(absoluteColumn);
		__attrRows[__attrCount] = row;
		__cellAttrs[__attrCount] = ca;
		__attrCount++;

		refresh();
	}

	/// <summary>
	/// Overrides the specified cell's default editability and sets whether the value
	/// in the cell may be edited or not. </summary>
	/// <param name="row"> the row of the cell.  Rows are numbered starting at 0. </param>
	/// <param name="column"> the column of the cell
	/// TODO (JTS - 2003-07-23)absolute or visible? </param>
	/// <param name="state"> whether the cell should be editable or not. </param>
	public virtual void setCellEditable(int row, int column, bool state)
	{
		((JWorksheet_AbstractTableModel)getModel()).overrideCellEdit(row, column, state);
	}

	/// <summary>
	/// Sets the font name to be used with worksheet cells.  While individual cell
	/// attributes can be used to change the font in different cells, getCellFont()
	/// and setCellFont*() are quicker for changing and returning the font used 
	/// everywhere in the table where a specific cell font attribute has not been set. </summary>
	/// <param name="cellFontName"> the font name </param>
	public virtual void setCellFontName(string cellFontName)
	{
		__cellFontName = cellFontName;
	}

	/// <summary>
	/// Sets the font size in which items in the table should be displayed.  While individual cell
	/// attributes can be used to change the font in different cells, getCellFont()
	/// and setCellFont*() are quicker for changing and returning the font used 
	/// everywhere in the table where a specific cell font attribute has not been set. </summary>
	/// <param name="size"> the size of the font (in points) in which table items should be displayed. </param>
	public virtual void setCellFontSize(int size)
	{
		__cellFontSize = size;
	}

	/// <summary>
	/// Sets the font style in which items in the table should be displayed.  While individual cell
	/// attributes can be used to change the font in different cells, getCellFont()
	/// and setCellFont*() are quicker for changing and returning the font used 
	/// everywhere in the table where a specific cell font attribute has not been set. </summary>
	/// <param name="style"> the style of the font in which table items should be displayed. </param>
	public virtual void setCellFontStyle(int style)
	{
		__cellFontStyle = style;
	}

	/// <summary>
	/// Sets the cell renderer that the table will use for these classes:<br>
	/// <ul>
	/// <li>Date</li>
	/// <li>Double</li>
	/// <li>Float</li>
	/// <li>Integer</li>
	/// <li>String</li>
	/// </ul> </summary>
	/// <param name="tcr"> the TableCellRenderer object to be used. </param>
	public virtual void setCellRenderer(JWorksheet_DefaultTableCellRenderer tcr)
	{
		__defaultCellRenderer = tcr;
		setDefaultRenderer(typeof(Integer), tcr);
		setDefaultRenderer(typeof(Double), tcr);
		setDefaultRenderer(typeof(string), tcr);
		setDefaultRenderer(typeof(System.DateTime), tcr);
		setDefaultRenderer(typeof(Float), tcr);
	}

	/// <summary>
	/// Sets up a column to use a cell-specific JComboBox editor.  This is opposed
	/// to setting up a column so that every cell in it is a JComboBox.  Using this
	/// method, the cells in the column can use
	/// either a JComboBox as the editor, or a textfield, or both.  
	/// This must be used in conjunction with setCellSpecificJComboBoxValues(), and
	/// can be used with setJComboBoxEditorPreviousRowCopy().  This method will 
	/// create the combo boxes so that the user cannot enter new values; they must
	/// select one from the list. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column to set up with a 
	/// cell-specific editor.  Columns are numbered starting at 0, though column 0 
	/// is usually the row count column. </param>
	public virtual void setCellSpecificJComboBoxColumn(int absoluteColumn)
	{
		setCellSpecificJComboBoxColumn(absoluteColumn, false);
	}

	/// <summary>
	/// Sets up a column to use a cell-specific JComboBox editor.  This is opposed
	/// to setting up a column so that every cell in it is a JComboBox.  Using this
	/// method, the cells in the column can use
	/// either a JComboBox as the editor, or a textfield, or both.  
	/// This must be used in conjunction with setCellSpecificJComboBoxValues(), and
	/// can be used with setJComboBoxEditorPreviousRowCopy(). </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column to set up with a 
	/// cell-specific editor.  Columns are numbered starting at 0, though column 0 is
	/// usually the row count column. </param>
	/// <param name="editable"> whether the ComboBoxes in the column should allow the user to
	/// enter a new value (true), or if the user can only select what is already in the list (false) </param>
	public virtual void setCellSpecificJComboBoxColumn(int absoluteColumn, bool editable)
	{
		TableColumn col = getColumnModel().getColumn(getVisibleColumn(absoluteColumn));
		int rows = getRowCount();
		JWorksheet_JComboBoxCellEditor editor = new JWorksheet_JComboBoxCellEditor(this, rows, editable);
		addJWorksheetListener(editor);
		col.setCellEditor(editor);
	}

	/// <summary>
	/// Sets the values to be used in a JComboBox cell editor for a specific cell. </summary>
	/// <param name="row"> the row in which the cell is located.  Rows are numbered starting at 0. </param>
	/// <param name="absoluteColumn"> the <b>absolute</b> column in which the cell is located.  
	/// A call must have already been made to 
	/// setCellSpecificJComboBoxColumn(absoluteColumn) for this to work.
	/// Columns are numbered starting at 0, though column 0 is usually the row count column. </param>
	/// <param name="v"> a list of values to populate the JComboBox editor with. </param>
	public virtual void setCellSpecificJComboBoxValues(int row, int absoluteColumn, System.Collections.IList v)
	{
		string routine = CLASS + ".setCellSpecificJComboBoxValues";

		TableColumn col = getColumnModel().getColumn(getVisibleColumn(absoluteColumn));
		TableCellEditor editor = col.getCellEditor();

		if (editor == null)
		{
			Message.printStatus(1, routine, "No combo box editor set " + "up for column " + absoluteColumn + ", not setting values.");
			return;
		}

		((JWorksheet_JComboBoxCellEditor)editor).setJComboBoxModel(row, v);
	}

	/// <summary>
	/// Sets whether when adding a row to a column that has been set to use 
	/// JComboBox editors (via setCellSpecificJComboBoxColumn) should use the same
	/// data model for the JComboBox as the cell immediately above it. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> absoluteColumn to set previous copy.
	/// Columns are numbered starting at 0, though column 0 is usually the row count column. </param>
	/// <param name="copy"> whether or not to copy the previous data model </param>
	/// <seealso cref= RTi.Util.GUI.JWorksheet_JComboBoxCellEditor#setPreviousRowCopy(boolean) </seealso>
	public virtual void setCellSpecificJComboBoxEditorPreviousRowCopy(int absoluteColumn, bool copy)
	{
		string routine = CLASS + ".setCellSpecificJComboBoxEditorPreviousRowCopy";

		TableColumn col = getColumnModel().getColumn(getVisibleColumn(absoluteColumn));
		TableCellEditor editor = col.getCellEditor();

		if (editor == null)
		{
			Message.printStatus(1, routine, "No combo box editor set " + "up for column " + absoluteColumn + ", not setting values.");
			return;
		}

		((JWorksheet_JComboBoxCellEditor)editor).setPreviousRowCopy(copy);
	}

	/// <summary>
	/// Sets the alignment that a column should display its data with.  This overrides
	/// any column alignment code in the cell renderer. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column of the column for which to set an alignment. </param>
	/// <param name="alignment"> one of DEFAULT (allows the cell renderer to determine the 
	/// alignment), CENTER, LEFT, or RIGHT. </param>
	public virtual void setColumnAlignment(int absoluteColumn, int alignment)
	{
		if (alignment < DEFAULT || alignment > RIGHT)
		{
			Message.printStatus(1, "", "Invalid alignment: " + alignment);
			return;
		}
		__columnAlignments[absoluteColumn] = alignment;
	}

	/// <summary>
	/// Sets a column to use a SimpleJComboBox as an editor (for all cells in the
	/// column).  The SimpleJComboBox will contain the values in the passed-in list.
	/// <para>
	/// <b>Note:</b> If all the cells in a particular column with use a combo box that
	/// has the same possible values (e.g., a boolean field in which the user can 
	/// select either 'True' or 'False'), it is more efficient to use this method 
	/// rather than using the setCellSpecific*ComboBox*() methods.
	/// </para>
	/// </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number of the column for 
	/// which to use a SimpleJComboBo as the editor.  Columns are numbered starting at
	/// 0, though column 0 is usually the row count column. </param>
	/// <param name="v"> a list of Strings with which to populate the 
	/// SimpleJComboBox.  If null, then the Combo box will be removed from the column. </param>
	public virtual void setColumnJComboBoxValues(int absoluteColumn, System.Collections.IList v)
	{
		setColumnJComboBoxValues(absoluteColumn, v, false);
	}

	/// <summary>
	/// Sets a column to use a SimpleJComboBox as an editor (for all the cells in the
	/// column).  The SimpleJComboBox will contain the values in the passed-in list.
	/// <para>
	/// <b>Note:</b> If all the cells in a particular column with use a combo box that
	/// has the same possible values (e.g., a boolean field in which the user can 
	/// select either 'True' or 'False'), it is more efficient to use this method 
	/// rather than using the setCellSpecific*ComboBox*() methods.
	/// </para>
	/// </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column number of the column for 
	/// which to use a SimpleJComboBo as the editor.  Columns are numbered starting at
	/// 0, though column 0 is usually the row count column. </param>
	/// <param name="v"> a list of Strings with which to populate the SimpleJComboBox.  If 
	/// null, then the combo box will be removed from the column. </param>
	/// <param name="editable"> if true, the SimpleJComboBox values can be selected and also edited by the user. </param>
	public virtual void setColumnJComboBoxValues(int absoluteColumn, System.Collections.IList v, bool editable)
	{
		TableColumn col = getColumnModel().getColumn(getVisibleColumn(absoluteColumn));
		if (v == null)
		{
			col.setCellEditor(null);
		}
		else
		{
			SimpleJComboBox editor = new SimpleJComboBox(v, editable);
			col.setCellEditor(new DefaultCellEditor(editor));
		}
	}

	/// <summary>
	/// Sets a new value for a Table column name. </summary>
	/// <param name="absoluteColumn"> the <b>visible</b> column for which to set the name. </param>
	/// <param name="name"> the new column name. </param>
	public virtual void setColumnName(int absoluteColumn, string name)
	{
		TableColumn col = getColumnModel().getColumn(getVisibleColumn(absoluteColumn));
		col.setHeaderValue(name);
		__columnNames[absoluteColumn] = name;
		if (getTableHeader() != null)
		{
			getTableHeader().repaint();
		}
	}

	/// <summary>
	/// Sets a tooltip for a column. </summary>
	/// <param name="absoluteColumn"> the <b>absolute</b> column to assign the tooltip. </param>
	/// <param name="tip"> the tooltip. </param>
	public virtual void setColumnToolTipText(int absoluteColumn, string tip)
	{
		try
		{
			getWorksheetHeader().setColumnToolTip(absoluteColumn, tip);
		}
		catch (Exception e)
		{
			Message.printWarning(3, CLASS + ".setColumnToolTipText", "Error setting column tool tip (column " + absoluteColumn + ").  Check log for details.");
			Message.printWarning(3, CLASS + ".setColumnToolTipText", e);
		}
	}

	/// <summary>
	/// Sets tooltips for all the columns in the worksheet. </summary>
	/// <param name="tips"> array of Strings, each one of which is a tooltip for an absolute column. </param>
	public virtual void setColumnsToolTipText(string[] tips)
	{
		for (int i = 0; i < tips.Length; i++)
		{
			setColumnToolTipText(i, tips[i]);
		}
	}

	/// <summary>
	/// Set the widths of the columns in the worksheet. <b>NOTE!</b> This method
	/// will not work until the GUI on which the JWorksheet is located is visible, 
	/// because otherwise the calls to getGraphics() will return null values.  For
	/// that reason, setVisible(true) must have been called -- or the other version of the
	/// method should be used and a valid Graphics object should be passed in. </summary>
	/// <param name="widths"> an integer array of widths, one for each column in the table.
	/// The widths are measured in terms of how many characters a column 
	/// should be able to accomodate, not in pixels or font sizes.  <para>
	/// For example, A column 
	/// that needs to be able to display "2003-03" would have a width of 7.
	/// The character "X" is used as the sizing character for calculating how large 
	/// (in screen pixel terms) the column will be to accomodate the given number of characters. </param>
	public virtual void setColumnWidths(int[] widths)
	{
		setColumnWidths(widths, getGraphics());
	}

	/// <summary>
	/// Sets the widths of the columns in the worksheet, using the provided 
	/// Graphics object to determine how wide the columns should be. </summary>
	/// <param name="widths"> an integer array of widths, one for each column in the table.
	/// The widths are actually measured in terms of how many characters a column 
	/// should be able to accomodate, not in pixels or font sizes.  e.g., A column 
	/// that needs to be able to display "2003-03" would have a width of 7.
	/// The character "X" is used as the sizing character for calculating how large 
	/// (in screen pixel terms) the column will be to accomodate the given number of characters. </param>
	/// <param name="g"> a Graphics object that can be used to determine how many pixels
	/// a certain font takes up in a given graphics context. </param>
	public virtual void setColumnWidths(int[] widths, Graphics g)
	{
		string routine = CLASS + ".setColumnWidths";

		if (g == null)
		{
			Message.printWarning(3, routine, "Graphics are null, not setting column widths.");
			return;
		}
		if (widths == null)
		{
			Message.printWarning(3, routine, "Widths are null, not setting column widths.");
			return;
		}
		if (__columnNames == null)
		{
			Message.printWarning(3, routine, "Column names are null, not setting column widths.");
			return;
		}
		if (widths.Length != __columnNames.Length)
		{
			Message.printWarning(3, routine, "Length of widths array (" + widths.Length + ") does not match internal column " + "names array length (" + __columnNames.Length + ").");
			return;
		}

		// test if the graphics have been 

		// Get the font metrics for each of the main fonts used in the
		// worksheet (the font used for the header and the font used for the cells).
		FontMetrics fh = g.getFontMetrics(new Font(__columnHeaderFontName, __columnHeaderFontStyle, __columnHeaderFontSize));
		FontMetrics fc = g.getFontMetrics(new Font(__cellFontName, __cellFontStyle, __cellFontSize));

		string s = "";
		int i = 0;
		int hwidth = 0;
		int cwidth = 0;

		int count = __columnNames.Length;

		// loop through each of the columns and get the desired width 
		// (in characters) from the table model's set widths.  
		// Calculate how many pixels would be needed for each of the
		// above font metrics to show that many characters, and then
		// use the larger measurement as the field width.
		for (i = 0; i < count; i++)
		{
			s = "";
			if (!__columnRemoved[i])
			{
				TableColumn tc = getColumnModel().getColumn(getVisibleColumn(i));
				for (int j = 0; j < widths[i]; j++)
				{
					s += "X";
				}
				hwidth = fh.stringWidth(s);
				cwidth = fc.stringWidth(s);
				if (hwidth > cwidth)
				{
					tc.setPreferredWidth(hwidth + 15);
				}
				else
				{
					tc.setPreferredWidth(cwidth + 15);
				}
			}
		}
	}

	/// <summary>
	/// Sets whether copying and pasting is enabled in the JWorksheet. </summary>
	/// <param name="copySetting"> whether users can copy from the worksheet. </param>
	/// <param name="pasteSetting"> whether users can paste to the worksheet. </param>
	protected internal virtual void setCopyPasteEnabled(bool copySetting, bool pasteSetting)
	{
	//	if (!IOUtil.isRunningApplet()) {
			if (__copyPasteAdapter == null)
			{
				__copyPasteAdapter = new JWorksheet_CopyPasteAdapter(this);
			}

			__copyPasteAdapter.setCopyEnabled(copySetting);
			__copyPasteAdapter.setPasteEnabled(pasteSetting);
			__copyEnabled = copySetting;
			__pasteEnabled = pasteSetting;
	/*		
		}
		else {
			__copyEnabled = false;
			__pasteEnabled = false;
		}
	*/
	}

	/// <summary>
	/// Sets new data in the existing table model.  This should be used if the 
	/// definition of the table model doesn't change, but the data in the table 
	/// model changes extensively.  Otherwise, just call addRow() </summary>
	/// <param name="data"> that will be set in the existing table model. </param>
	public virtual void setData(System.Collections.IList data)
	{
		__lastRowSelected = -1;
		((JWorksheet_AbstractTableModel)getModel()).setNewData(data);
		((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();

		if (__selectionMode == __EXCEL_SELECTION)
		{
			int rows = getRowCount();
			int cols = getColumnCount();
			setSelectionMode(ListSelectionModel.MULTIPLE_INTERVAL_SELECTION);
			setCellSelectionEnabled(true);
			JWorksheet_RowSelectionModel r = new JWorksheet_RowSelectionModel(rows, cols);
			r.setPartner(__partner);
			if (!__selectable)
			{
				r.setSelectable(false);
			}

			JWorksheet_ColSelectionModel c = new JWorksheet_ColSelectionModel();
			r.setColSelectionModel(c);
			c.setRowSelectionModel(r);
			setSelectionModel(r);
			setOneClickRowSelection(__oneClickRowSelection);
			getColumnModel().setSelectionModel(c);
		}

		if (__listRowHeader != null)
		{
			adjustListRowHeaderSize(__DATA_RESET);
		}
		notifyAllWorksheetListeners(__DATA_RESET, getRowCount());
	}

	/// <summary>
	/// Sets whether the worksheet is dirty. </summary>
	/// <param name="dirty"> whether the worksheet is dirty. </param>
	public virtual void setDirty(bool dirty)
	{
		__dirty = dirty;
	}

	/// <summary>
	/// Called by the JWorksheet_DefaultTableCellEditor to set the cell which is being edited. </summary>
	/// <param name="row"> the row of the cell being edited </param>
	/// <param name="visibleColumn"> the <b>visible</b> column of the cell being editedd </param>
	protected internal virtual void setEditCell(int row, int visibleColumn)
	{
		__editRow = row;
		__editCol = visibleColumn;
	}

	/// <summary>
	/// Sets the number of the first row of the table.  All other row numbers in the
	/// row header are determined from this. </summary>
	/// <param name="firstRowNumber"> the number of the first row in the row header. </param>
	public virtual void setFirstRowNumber(int firstRowNumber)
	{
		__firstRowNum = firstRowNumber;

		if (__listRowHeader != null)
		{
			enableRowHeader();
		}
	}

	/// <summary>
	/// Sets the frame in which the hourglass will be displayed when sorting.
	/// </summary>
	public virtual void setHourglassJDialog(JDialog dialog)
	{
		__hourglassJDialog = dialog;
	}

	/// <summary>
	/// Sets the frame in which the hourglass will be displayed when sorting.
	/// </summary>
	public virtual void setHourglassJFrame(JFrame frame)
	{
		__hourglassJFrame = frame;
	}

	/// <summary>
	/// Sets whether the worksheet's header should support displaying multiple lines.
	/// If this is turned on, linebreaks ('\n') in the column names will result in line breaks in the header. </summary>
	/// <param name="multiline"> whether to support multiple line headers. </param>
	private void setMultipleLineHeaderEnabled(bool multiline)
	{
		__hcr.setMultiLine(multiline);
	}

	/// <summary>
	/// Sets a new table model into the worksheet and populates the worksheet with the data in the table model. </summary>
	/// <param name="tm"> the TableModel with which to populate the worksheet. </param>
	public virtual void setModel(JWorksheet_AbstractTableModel tm)
	{
		__lastRowSelected = -1;
		tm._worksheet = this;
		base.setModel(tm);
		__dirty = false;
		initialize(tm.getRowCount(), tm.getColumnCount());

		createColumnList();

		if (!__showRowCountColumn && !__useRowHeaders)
		{
			removeColumn(0);
		}

		if (__listRowHeader != null)
		{
			adjustListRowHeaderSize(__DATA_RESET);
		}

		if (tm.getColumnToolTips() != null)
		{
			setColumnsToolTipText(tm.getColumnToolTips());
		}
		notifyAllWorksheetListeners(__DATA_RESET, getRowCount());
	}

	/// <summary>
	/// Sets whether the user should be able to select an entire row just by clicking
	/// on the first (0th) column.  Not usable with default JTable selection models. </summary>
	/// <param name="oneClick"> if true, the user can select an entire row by clicking on the first (0th) column. </param>
	public virtual void setOneClickRowSelection(bool oneClick)
	{
		__oneClickRowSelection = oneClick;
		if (__selectionMode == __EXCEL_SELECTION)
		{
			if (__listRowHeader == null)
			{
				((JWorksheet_RowSelectionModel)getSelectionModel()).setOneClickRowSelection(oneClick);
			}
		}
		else
		{
	//		return;
		}
	}

	/// <summary>
	/// Sets the popup menu to display if the table is right-clicked on. </summary>
	/// <param name="popup"> the popup menu to display. </param>
	public virtual void setPopupMenu(JPopupMenu popup, bool worksheetHandlePopup)
	{
		setupPopupMenu(popup, worksheetHandlePopup);
		__mainPopup = popup;
	}

	/// <summary>
	/// Sets the partner row selection model used when this table is the row header 
	/// of another in a JScrollWorksheet. </summary>
	/// <param name="partner"> the JWorksheet_RowSelectionModel of the main table, the one 
	/// for which another this table is its row header. </param>
	public virtual void setRowSelectionModelPartner(JWorksheet_RowSelectionModel partner)
	{
		((JWorksheet_RowSelectionModel)getSelectionModel()).setPartner(partner);
		__partner = partner;
	}

	/// <summary>
	/// Sets a data object in a row.  Only works with table models that store 
	/// each rows as a separate data object (are descended from JWorksheet_AbstractRowTableModel). </summary>
	/// <param name="o"> the data object to replace the object at the specified row with. </param>
	/// <param name="pos"> the row at which to replace the object. </param>
	public virtual void setRowData(object o, int pos)
	{
		if (!(getModel() is JWorksheet_AbstractRowTableModel))
		{
			return;
		}

		if (pos < 0)
		{
			return;
		}
		if (pos > getRowCount())
		{
			return;
		}

		((JWorksheet_AbstractTableModel)getModel()).setRowData(o, pos);
		((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();
	}

	/// <summary>
	/// Sets whether this worksheet's cells can be selected or not. </summary>
	/// <param name="selectable"> whether this worksheet's cells can be selected or not. </param>
	public virtual void setSelectable(bool selectable)
	{
		__selectable = selectable;
	}

	/// <summary>
	/// Sets up the popup menu for copying and pasting cell values. </summary>
	/// <param name="menu"> the menu in which to set up the menu items. </param>
	private void setupPopupMenu(JPopupMenu menu, bool worksheetHandlePopup)
	{
		if (__mainPopup == null && worksheetHandlePopup)
		{
			addMouseListener(this);
		}
		__copyMenuItem = new JMenuItem(__MENU_COPY);
		__copyHeaderMenuItem = new JMenuItem(__MENU_COPY_HEADER);
		__pasteMenuItem = new JMenuItem(__MENU_PASTE);
		__copyAllMenuItem = new JMenuItem(__MENU_COPY_ALL);
		__copyAllHeaderMenuItem = new JMenuItem(__MENU_COPY_ALL_HEADER);

		__deselectAllMenuItem = new JMenuItem(__MENU_DESELECT_ALL);
		__deselectAllMenuItem.addActionListener(this);
		__selectAllMenuItem = new JMenuItem(__MENU_SELECT_ALL);
		__selectAllMenuItem.addActionListener(this);

		menu.add(__selectAllMenuItem);
		menu.add(__deselectAllMenuItem);

		if (__copyEnabled || __pasteEnabled)
		{
			menu.addSeparator();
		}

		if (__copyEnabled)
		{
			__copyMenuItem.addActionListener(this);
			menu.add(__copyMenuItem);
			__copyHeaderMenuItem.addActionListener(this);
			menu.add(__copyHeaderMenuItem);
			__copyAllMenuItem.addActionListener(this);
			menu.add(__copyAllMenuItem);
			__copyAllHeaderMenuItem.addActionListener(this);
			menu.add(__copyAllHeaderMenuItem);
		}
		if (__pasteEnabled)
		{
			__pasteMenuItem.addActionListener(this);
			menu.add(__pasteMenuItem);
		}
		if (__calculateStatisticsEnabled)
		{
			menu.addSeparator();
			__calculateStatisticsMenuItem = new JMenuItem(__MENU_CALCULATE_STATISTICS);
			__calculateStatisticsMenuItem.addActionListener(this);
			menu.add(__calculateStatisticsMenuItem);
		}
		if (1 == 1)
		{
			// TODO (JTS - 2004-10-21) activate with a property later.
			menu.addSeparator();
			JMenuItem saveMenuItem = new JMenuItem(__MENU_SAVE_TO_FILE);
			saveMenuItem.addActionListener(this);
			menu.add(saveMenuItem);
		}

		__worksheetHandlePopup = worksheetHandlePopup;
	}

	/// <summary>
	/// Overrides the default JTable implementation of setValue at. </summary>
	/// <param name="o"> the value to set in the cell </param>
	/// <param name="row"> the row of the cell </param>
	/// <param name="col"> the <b>visible</b> column of the cell. </param>
	public virtual void setValueAt(object o, int row, int col)
	{
		base.setValueAt(o, row, col);
	}

	/// <summary>
	/// Turns a wait cursor on or off on the worksheet. </summary>
	/// <param name="hourglassEnabled"> if true, the wait cursor will be displayed.  If false, it will be hidden. </param>
	public virtual void setWaitCursor(bool hourglassEnabled)
	{
		if (__hourglassJDialog == null && __hourglassJFrame == null)
		{
			return;
		}
		if (__hourglassJDialog == null)
		{
			JGUIUtil.setWaitCursor(__hourglassJFrame, hourglassEnabled);
		}
		else
		{
			JGUIUtil.setWaitCursor(__hourglassJDialog, hourglassEnabled);
		}
	}

	/// <summary>
	/// Selects whether to show the header on the JWorksheet or not.  This method
	/// currently only works if the JWorksheet is in a JScrollPane. </summary>
	/// <param name="show"> whether to show the header or not.  If the header is already showing
	/// and it is set to be shown, nothing changes.  Otherwise, it is hidden and
	/// no longer shows.  If the header is hidden and then showColumnHeader(true) is 
	/// called, the header (which is stored internally when hidden) will be put back in place. </param>
	public virtual void showColumnHeader(bool show)
	{
		Container p = getParent();
		if (p is JViewport)
		{
			Container gp = p.getParent();
			if (gp is JScrollPane)
			{
				JScrollPane scrollPane = (JScrollPane)gp;
				if (!show)
				{
					__columnHeaderView = scrollPane.getColumnHeader();
					scrollPane.setColumnHeader(null);
				}
				else
				{
					if (__columnHeaderView != null)
					{
						scrollPane.setColumnHeader(__columnHeaderView);
						__columnHeaderView = null;
					}
				}
			}
		}
	}

	/// <summary>
	/// Sorts a column in a given sort order.  Missing data is handled as follows:
	/// Missing data is initially handled by the cell renderer, which will
	/// paint an empty string ("") instead of -999 or -999.00, etc.  When the empty
	/// string is attempted to be turned into a Double or an Integer, an exception is
	/// thrown and caught, and the appropriate DMIUtil.MISSING value is placed in the
	/// list to be sorted, so missing data will sort as much lower than other data. </summary>
	/// <param name="order"> the order in which to sort the column, either SORT_ASCENDING or 
	/// SORT_DESCENDING as defined in StringUtil. </param>
	private void sortColumn(int order)
	{
		// clear out the sorted order for the table model if one has 
		// already been generated by a previous sort.	
		((JWorksheet_AbstractTableModel)getModel()).setSortedOrder(null);

		int size = getRowCount();
		int[] sortOrder = new int[size];

		int absColumn = getAbsoluteColumn(__popupColumn);

		// Sort numbers with MathUtil.sort()	
		int exceptionCount = 0;
		if (getColumnClass(absColumn) == typeof(Integer))
		{
			int[] unsorted = new int[size];
			int? I = null;
			for (int i = 0; i < size; i++)
			{
				try
				{
					I = (int?)getValueAt(i, __popupColumn);
					unsorted[i] = I.Value;
				}
				catch (Exception e)
				{
					++exceptionCount;
					if (exceptionCount < 10)
					{
						Message.printWarning(3,"","Exception getting data for sort:");
						Message.printWarning(3,"",e);
					}
					unsorted[i] = DMIUtil.MISSING_INT;
				}
			}
			MathUtil.sort(unsorted, MathUtil.SORT_QUICK, order, sortOrder, true);
		}
		else if (getColumnClass(absColumn) == typeof(Long))
		{
			long[] unsorted = new long[size];
			long? l = null;
			for (int i = 0; i < size; i++)
			{
				try
				{
					l = (long?)getValueAt(i, __popupColumn);
					unsorted[i] = l.Value;
				}
				catch (Exception e)
				{
					++exceptionCount;
					if (exceptionCount < 10)
					{
						Message.printWarning(3,"","Exception getting data for sort:");
						Message.printWarning(3,"",e);
					}
					unsorted[i] = DMIUtil.MISSING_LONG;
				}
			}
			MathUtil.sort(unsorted, MathUtil.SORT_QUICK, order, sortOrder, true);
		}
		else if ((getColumnClass(absColumn) == typeof(Double)) || (getColumnClass(absColumn) == typeof(Float)))
		{
			// Sort numbers with MathUtil.sort()
			// Treat Float as Double since sort method does not handle float[]
			double[] unsorted = new double[size];
			object o = null;
			for (int i = 0; i < size; i++)
			{
				try
				{
					o = getValueAt(i, __popupColumn);
					if (o == null)
					{
						unsorted[i] = DMIUtil.MISSING_DOUBLE;
					}
					else if (o is double?)
					{
						unsorted[i] = ((double?)o).Value;
					}
					else
					{
						unsorted[i] = ((float?)o).Value;
					}
				}
				catch (Exception e)
				{
					++exceptionCount;
					if (exceptionCount < 10)
					{
						Message.printWarning(3,"","Exception getting data for sort:");
						Message.printWarning(3,"",e);
					}
					unsorted[i] = DMIUtil.MISSING_DOUBLE;
				}
			}
			MathUtil.sort(unsorted, MathUtil.SORT_QUICK, order, sortOrder, true);
		}
		// Sort Dates by turning them into Strings first and sorting with StringUtil.sort()
		else if (getColumnClass(absColumn) == typeof(System.DateTime))
		{
			// Since sorting by dates, handle the dates generically.  This allows Date and DateTime to be used
			IList<string> v = new List<string>(size);
			object o = null;
			for (int i = 0; i < size; i++)
			{
				//d = (Date)
				o = getValueAt(i, __popupColumn);
				if (o == null)
				{
					v.Add("");
				}
				else
				{
					v.Add("" + o);
				}
			}
			StringUtil.sortStringList(v, order, sortOrder, true, true);
		}
		// Sort booleans by converting to numbers and sorting with quick sort.
		// trues are turned into -1s and falses into 0s so that trues sort
		// to the top when doing sort ascending, like in Microsoft Access.
		else if (getColumnClass(absColumn) == typeof(Boolean))
		{
			int[] unsorted = new int[size];
			bool? B = null;
			for (int i = 0; i < size; i++)
			{
				try
				{
					B = (bool?)getValueAt(i, __popupColumn);
					if (B.Value)
					{
						unsorted[i] = -1;
					}
					else
					{
						unsorted[i] = 0;
					}
				}
				catch (Exception e)
				{
					++exceptionCount;
					if (exceptionCount < 10)
					{
						Message.printWarning(3,"","Exception getting data for sort:");
						Message.printWarning(3,"",e);
					}
					unsorted[i] = DMIUtil.MISSING_INT;
				}
			}
			MathUtil.sort(unsorted, MathUtil.SORT_QUICK, order, sortOrder, true);
		}
		// Sort Strings with StringUtil.sort()
		else
		{
			IList<string> v = new List<string>(size);
			object o = null;
			for (int i = 0; i < size; i++)
			{
				o = getValueAt(i, __popupColumn);
				if (o == null)
				{
					v.Add("");
				}
				else
				{
					if (o is string)
					{
						v.Add((string)o);
					}
					else
					{
						v.Add("" + o);
					}
				}
			}
			StringUtil.sortStringList(v, order, sortOrder, true, true);
		}
		// Set the sorted order into the table model ...
		((JWorksheet_AbstractTableModel)getModel()).setSortedOrder(sortOrder);
		// ... and force a redraw on the table model.
		((JWorksheet_AbstractTableModel)getModel()).fireTableDataChanged();

		__cancelMenuItem.setEnabled(true);
	}

	/// <summary>
	/// Sets up the table model to be prepared to do a consecutive row read.<para>
	/// Some table models may store data (e.g., time series dates), in which 
	/// data values are calculated based on the previous row's data.  In this case,
	/// this method can be used to let them know that a consecutive read of the table
	/// data will be done, and that every time a call is made to getValueAt() in the 
	/// table model, the row parameter is guaranteed to be the same row as the last 
	/// time getValueAt() was called (if the column is different), or 1 more than the previous row.
	/// </para>
	/// </summary>
	public virtual void startNewConsecutiveRead()
	{
		((JWorksheet_AbstractTableModel)getModel()).startNewConsecutiveRead();
	}

	/// <summary>
	/// Programmatically stops any cell editing that is taking place.  Cell editing
	/// happens when a user double-clicks in a cell or begins typing in a cell.  
	/// This method will stop the editing and WILL accept the data the user has
	/// entered up to this method call.  To abort saving the data the user has already entered, use cancelEditing(). </summary>
	/// <returns> true if the editing was stopped, false if it wasn't (because there were errors in the data). </returns>
	public virtual bool stopEditing()
	{
		if (getCellEditor() != null)
		{
			getCellEditor().stopCellEditing();
			if (getCellEditor() == null)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Returns whether test code is turned on for the worksheet.  If testing is on, 
	/// then new code (with possibly undesirable side-effects) may be run.  This should
	/// never be true in publicly-released code. </summary>
	/// <returns> whether test code is turned on for the worksheet. </returns>
	public virtual bool testing()
	{
		return __testing;
	}

	/// <summary>
	/// Sets whether test code is turned on for the worksheet.  If testing is on,
	/// then new code (with possibly undesirable side-effects) may be run.  This should
	/// never be true in publicly-released code. </summary>
	/// <returns> whether test code is turned on for the worksheet. </returns>
	public virtual void testing(bool testing)
	{
		__testing = testing;
	}

	/// <summary>
	/// Writes the table as HTML out to a file and closes it. </summary>
	/// <param name="filename"> the name of the file to write. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeAsHTML(String filename) throws Exception
	public virtual void writeAsHTML(string filename)
	{
		writeAsHTML(filename, 0, getRowCount() - 1);
	}

	/// <summary>
	/// Writes the specified rows as HTML out to a file and closes it. </summary>
	/// <param name="filename"> the name of the file to write. </param>
	/// <param name="firstRow"> the first row to start writing. </param>
	/// <param name="lastRow"> the last row to be written. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeAsHTML(String filename, int firstRow, int lastRow) throws Exception
	public virtual void writeAsHTML(string filename, int firstRow, int lastRow)
	{
		createHTML(null, filename, firstRow, lastRow);
	}

	/// <summary>
	/// Writes the table out as HTML to an already-created HTMLWriter.  If the 
	/// HTMLWriter is writing to a file, the file is not closed after the table is written. </summary>
	/// <param name="htmlWriter"> the HTMLWriter object to which to write the table. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeAsHTML(RTi.Util.IO.HTMLWriter htmlWriter) throws Exception
	public virtual void writeAsHTML(HTMLWriter htmlWriter)
	{
		writeAsHTML(htmlWriter, 0, getRowCount() - 1);
	}

	/// <summary>
	/// Writes the specified rows as HTML out to an already-created HTMLWriter.  If the
	/// HTMLWriter is writing to a file, the file is not closed after the table is written. </summary>
	/// <param name="htmlWriter"> the HTMLWriter object to which to write the table. </param>
	/// <param name="firstRow"> the first row to start writing. </param>
	/// <param name="lastRow"> the last row to be written. </param>
	/// <exception cref="Exception"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeAsHTML(RTi.Util.IO.HTMLWriter htmlWriter, int firstRow, int lastRow) throws Exception
	public virtual void writeAsHTML(HTMLWriter htmlWriter, int firstRow, int lastRow)
	{
		createHTML(htmlWriter, null, firstRow, lastRow);
	}

	}

	// TODO (JTS - 2004-02-12 something to set cell background colors on the table as a whole?
	// TODO document getColumnCount() -- abs or vis?

	// TODO (JTS - 2005-10-19) something so that if the row header is clicked on, the entire row is selected

}