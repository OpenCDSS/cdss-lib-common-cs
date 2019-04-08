using System;
using System.Collections.Generic;

// GeoViewSummaryFileJDialog - dialog for selecting fields from a summary file that will be used for the id field and the data fields

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

//-----------------------------------------------------------------------------
// GeoViewSummaryFileJDialog - Dialog for selecting the fields from a summary
//	file that will be used for the id field and the data fields.
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
// 2004-08-05	J. Thomas Sapienza, RTi	Initial version.
// 2004-08-07	JTS, RTi		Revised the threading model.
// 2004-08-09	JTS, RTi		Revised the GUI setup.
// 2005-04-27	JTS, RTi		* Added finalize().
//					* Replaced the MutableJList with a
//					  SimpleJList.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{



	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using SimpleJList = RTi.Util.GUI.SimpleJList;

	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	using DataTable = RTi.Util.Table.DataTable;
	using DataTable_CellRenderer = RTi.Util.Table.DataTable_CellRenderer;
	using DataTable_TableModel = RTi.Util.Table.DataTable_TableModel;
	using TableField = RTi.Util.Table.TableField;

	/// <summary>
	/// This class is a small dialog that provides a preview of the table records plus
	/// combo boxes from which the user can choose the fields they want to use as
	/// the ID fields and the data fields.<para>
	/// <b>Using this Class</b>
	/// To use this class, simply instantiate it and let it run.  It's modal, so it 
	/// will stop the thread it is called from.  After execution returns to the calling
	/// method, get the id field value.  If it is null, the user pressed cancel.  
	/// </para>
	/// Otherwise, get the values and use them.<para>
	/// <pre>
	/// GeoViewSummaryFileJDialog d = GeoViewSummaryFileJDialog(parent, 
	///		filename, tableFields, delimiter);
	/// if (d.cancelled()) {
	///		// user pressed cancel
	///		return;
	/// }
	/// </pre>
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GeoViewSummaryFileJDialog extends javax.swing.JDialog implements java.awt.event.ActionListener
	public class GeoViewSummaryFileJDialog : JDialog, ActionListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_CANCEL = "Cancel", __BUTTON_OK = "OK";

	/// <summary>
	/// Whether the user pressed cancel to close the GUI or not.  True by default -- 
	/// only set to false if they actually press OKAY.  This is to handle the times
	/// when they close the GUI via the upper-right-hand X.
	/// </summary>
	private bool __cancelled = true;

	/// <summary>
	/// Whether field maximums will be equalized for a layer.
	/// </summary>
	private bool __equalizeMax = false;

	/// <summary>
	/// The table of data read from the file.  At most, 5 lines will be contained
	/// in this table.
	/// </summary>
	private DataTable __table = null;

	/// <summary>
	/// The fields that the user selected as the data fields.
	/// </summary>
	private int[] __dataFields = null;

	/// <summary>
	/// The field that the user selected as the id field.
	/// </summary>
	private int[] __idFields = null;

	/// <summary>
	/// The checkbox for choosing to equalize maximum values.
	/// </summary>
	private JCheckBox __equalizeCheckBox;

	/// <summary>
	/// The parent JFrame.
	/// </summary>
	private JFrame __parent = null;

	/// <summary>
	/// The textfield for the layer name.
	/// </summary>
	private JTextField __layerNameTextField = null;

	/// <summary>
	/// A list for selecting the app layer types to include.
	/// </summary>
	private SimpleJList __list;

	/// <summary>
	/// Combo boxes for choosing fields.
	/// </summary>
	private SimpleJComboBox __dataFieldsComboBox = null, __idFieldsComboBox = null;

	/// <summary>
	/// Name of the file from which data is read.
	/// </summary>
	private string __filename = null;

	/// <summary>
	/// Name of the layer.
	/// </summary>
	private string __layerName = null;

	/// <summary>
	/// The app layer views on the geo view display.
	/// </summary>
	private IList<GeoLayerView> __appLayerViews = null;

	/// <summary>
	/// The app layer types the user selected.
	/// </summary>
	private IList<string> __appLayerTypes = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame on which this dialog is opened. </param>
	/// <param name="filename"> the name of the file to read the table from.  This is not
	/// checked for validity. </param>
	/// <param name="tableFields"> the table fields that define the table. </param>
	/// <param name="delimiter"> the delimiter that separates fields in the table. </param>
	/// <param name="appLayers"> the names of the layers in the GeoView. </param>
	/// <exception cref="Exception"> if there is an error reading the table file. </exception>
	/// <exception cref="NullPointerException"> if any parameter is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GeoViewSummaryFileJDialog(javax.swing.JFrame parent, String filename, java.util.List<RTi.Util.Table.TableField> tableFields, String delimiter, java.util.List<GeoLayerView> appLayers) throws Exception
	public GeoViewSummaryFileJDialog(JFrame parent, string filename, IList<TableField> tableFields, string delimiter, IList<GeoLayerView> appLayers) : base(parent, true)
	{

		if (parent == null || string.ReferenceEquals(filename, null) || tableFields == null || string.ReferenceEquals(delimiter, null) || appLayers == null)
		{
			throw new System.NullReferenceException();
		}

		__table = DataTable.parseDelimitedFile(filename, delimiter, tableFields, 1, false, 6);
		__filename = filename;
		__appLayerViews = appLayers;

		string title = "Select Summary Fields";
		if (string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null) || JGUIUtil.getAppNameForWindows().Equals(""))
		{
			setTitle(title);
		}
		else
		{
			setTitle(JGUIUtil.getAppNameForWindows() + " - " + title);
		}

		setupGUI();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string action = @event.getActionCommand();

		if (action.Equals(__BUTTON_CANCEL))
		{
			__idFields = null;
			__dataFields = null;
			__appLayerTypes = null;
			__layerName = null;
			dispose();
		}
		else if (action.Equals(__BUTTON_OK))
		{
			if (determineFieldValues())
			{
				__cancelled = false;
				dispose();
			}
			else
			{
			/*
				new ResponseJDialog(this, "Invalid Field Range",
					"The range of fields in the data fields "
					+ "entry field is not valid.  Please "
					+ "correct.", 
					ResponseJDialog.OK).response();
			*/
			}
		}
		else if (@event.getSource() == __dataFieldsComboBox)
		{
		}
		else if (@event.getSource() == __idFieldsComboBox)
		{
		}
	}

	/// <summary>
	/// Returns whether the user pressed CANCEL or not. </summary>
	/// <returns> true if the user cancelled out of the GUI.  false if not. </returns>
	public virtual bool cancelled()
	{
		return __cancelled;
	}

	/// <summary>
	/// Determines what the fields are the user selected from the combo boxes and 
	/// fills in the __idFields and __dataFields variables appropriately. </summary>
	/// <returns> true if the field values could be determined.  False if there was
	/// a problem parsing them. </returns>
	private bool determineFieldValues()
	{
		string routine = "GeoViewSummaryFileJDialog.determineFieldValues";

		// overarching try/catch will capture any bad integer parses, etc,
		// and return false. 
		try
		{
			__idFields = getRange(__idFieldsComboBox);
			if (__idFields == null)
			{
				return false;
			}

			__dataFields = getRange(__dataFieldsComboBox);
			if (__dataFields == null)
			{
				return false;
			}

		}
		catch (Exception e)
		{
			Message.printWarning(1, routine, "There was an error with the specified column ranges.");
			Message.printWarning(2, routine, e);
			return false;
		}

		__equalizeMax = __equalizeCheckBox.isSelected();

		__layerName = __layerNameTextField.getText();
		if (__layerName.Trim().Equals(""))
		{
			Message.printWarning(1, routine, "A layer name must be entered.");
			return false;
		}

		__appLayerTypes = __list.getSelectedItems();

		return true;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewSummaryFileJDialog()
	{
		__table = null;
		__dataFields = null;
		__idFields = null;
		__equalizeCheckBox = null;
		__parent = null;
		__layerNameTextField = null;
		__list = null;
		__dataFieldsComboBox = null;
		__idFieldsComboBox = null;
		__filename = null;
		__layerName = null;
		__appLayerViews = null;
		__appLayerTypes = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}
	/// <summary>
	/// Returns the app layers the user selected.  If null, the user hit cancel. </summary>
	/// <returns> the app layers the user selected. </returns>
	public virtual IList<string> getAppLayerTypes()
	{
		return __appLayerTypes;
	}

	/// <summary>
	/// Returns the information for all the app layers on the GeoView display, 
	/// formatted so that it can be placed into a Vector and selected. </summary>
	/// <param name="layerViews"> list of layer views on the GeoView display. </param>
	/// <returns> a list of Strings describing the layer views and their join fields,
	/// suitable for use in the list from which users select. </returns>
	public virtual IList<string> getAppLayersInfo(IList<GeoLayerView> layerViews)
	{
		if (layerViews == null)
		{
			return new List<string>();
		}

		GeoLayer layer = null;
		GeoLayerView layerView = null;
		int size = layerViews.Count;
		string joinFields = null;
		string s = null;
		IList<string> joinFieldsList = null;
		IList<string> v = new List<string>();
		for (int i = 0; i < size; i++)
		{
			layerView = layerViews[i];
			layer = layerView.getLayer();
			s = layer.getAppLayerType() + " - "
				+ layerView.getLegend().getText() + " - ";

			joinFields = layerView.getPropList().getValue("AppJoinField");
			if (string.ReferenceEquals(joinFields, null))
			{
				s += "[No join field]";
			}
			else
			{
				joinFieldsList = StringUtil.breakStringList(joinFields, ",", 0);
				for (int j = 0; j < joinFieldsList.Count; j++)
				{
					if (j != 0)
					{
						s += ", ";
					}
					s += joinFieldsList[j];
				}
			}
			v.Add(s);
		}
		return v;
	}

	/// <summary>
	/// Returns the data fields the user selected.  If null, the user hit cancel. </summary>
	/// <returns> the data fields the user selected. </returns>
	public virtual int[] getDataFields()
	{
		return __dataFields;
	}

	/// <summary>
	/// Returns whether field values will be equalized for a layer. </summary>
	/// <returns> whether field values will be equalized for a layer. </returns>
	public virtual bool getEqualizeMax()
	{
		return __equalizeMax;
	}

	/// <summary>
	/// Returns the id field the user selected.  If null, the user hit cancel. </summary>
	/// <returns> the id field the user selected. </returns>
	public virtual int[] getIDFields()
	{
		return __idFields;
	}

	/// <summary>
	/// Returns the name of the layer.  If null, the user hit cancel. </summary>
	/// <returns> the name of the layer. </returns>
	public virtual string getLayerName()
	{
		return __layerName;
	}

	/// <summary>
	/// Returns the range of values selected in a combo box.  Users can either choose
	/// one of the items from the combo box, in which case only a single field is
	/// selected, or they can provide a series of numbers where ranges are specified
	/// by commas and dashes. </summary>
	/// <param name="comboBox"> the comboBox to check to return the selected fields. </param>
	/// <returns> an int array where each array position contains the number of a field
	/// that was selected.  No field number will be included in the array more than
	/// once.  If null, an error occurred. </returns>
	/// <exception cref="Exception"> if there are any errors getting the fields. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int[] getRange(RTi.Util.GUI.SimpleJComboBox comboBox) throws Exception
	private int[] getRange(SimpleJComboBox comboBox)
	{
		// Data fields
		string dataS = comboBox.getSelected();
		int index = dataS.IndexOf("(", StringComparison.Ordinal);
		if (index > -1)
		{
			dataS = dataS.Substring(0, index).Trim();
		}

		bool[] fields = new bool[__table.getNumberOfFields() + 1];
		for (int i = 0; i < fields.Length; i++)
		{
			fields[i] = false;
		}

		IList<string> v = StringUtil.breakStringList(dataS, ",", 0);

		// basically, first parse out all the comma-separated values in
		// the string.  Then, check each one to see if it's actually a 
		// field range (has a dash).  Set the field[] value to true if
		// the field is specified anywhere, in a range or in the comma
		// list.  Ranges can overlap previously-specified numbers and
		// vice versa with no ill effect.

		int size = v.Count;
		string s = null;
		IList<string> v2 = null;
		string ss1 = null;
		string ss2 = null;
		int i1 = -1;
		int i2 = -1;
		for (int i = 0; i < size; i++)
		{
			s = v[i];
			s = s.Trim();
			if (s.IndexOf("-", StringComparison.Ordinal) > -1)
			{
				v2 = StringUtil.breakStringList(s, "-", 0);
				if (v2.Count != 2)
				{
					return null;
				}
				ss1 = v2[0].Trim();
				ss2 = v2[1].Trim();

				i1 = Integer.decode(ss1).intValue();
				i2 = Integer.decode(ss2).intValue();

				if (i1 <= 0 || i2 <= 0)
				{
					return null;
				}

				for (int j = i1; j <= i2; j++)
				{
					fields[j] = true;
				}
			}
			else
			{
				i1 = Integer.decode(s).intValue();
				if (i1 <= 0)
				{
					return null;
				}
				fields[i1] = true;
			}
		}

		size = 0;
		for (int i = 1; i < fields.Length; i++)
		{
			if (fields[i])
			{
				size++;
			}
		}

		int count = 0;
		int[] finalFields = new int[size];
		for (int i = 1; i < fields.Length; i++)
		{
			if (fields[i])
			{
				finalFields[count++] = i - 1;
			}
		}

		return finalFields;
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		string routine = "GeoViewSummaryFileJDialog.setupGUI";

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		IList<string> v = new List<string>();
		int fieldCount = __table.getNumberOfFields();
		string fieldName = null;
		for (int i = 0; i < fieldCount; i++)
		{
			fieldName = __table.getFieldName(i);
			if (string.ReferenceEquals(fieldName, null) || fieldName.Trim().Equals(""))
			{
				v.Add("" + (i + 1));
			}
			else
			{
				v.Add("" + (i + 1) + " (" + fieldName + ")");
			}
		}

		__dataFieldsComboBox = new SimpleJComboBox(v, true);
		__idFieldsComboBox = new SimpleJComboBox(v, true);
		__equalizeCheckBox = new JCheckBox((string)null, true);
		__layerNameTextField = new JTextField(15);

		int y = 0;

		JPanel temp1 = new JPanel();
		temp1.setLayout(new GridBagLayout());

		JGUIUtil.addComponent(temp1, new JLabel("Layer name: "), 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(temp1, __layerNameTextField, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, temp1, 0, y++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(panel, new JLabel("Select the columns that will be used for the summary layer " + "display."), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel("The ID columns will be used to relate data to " + "specific features on the map."), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel("The data columns contain numerical data that will be " + "shown as symbols."), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel("To select one data column, choose it from the list."), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel("To enter multiple columns, specify as " + "comma-separated numeric values or as a "), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel("pair of numbers separated by a dash.  e.g:"), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel("     1,3,4-10,15"), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel("     11-19, 12, 13, 1"), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JLabel(""), 0, y++, 2, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JPanel temp2 = new JPanel();
		temp2.setLayout(new GridBagLayout());

		JGUIUtil.addComponent(temp2, new JLabel("ID Column(s): "), 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(temp2, __idFieldsComboBox, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(temp2, new JLabel("    The numbers of the columns to be used for ID matching."), 2, 0, 1, 1, 1, 0, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, temp2, 0, y++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JPanel temp3 = new JPanel();
		temp3.setLayout(new GridBagLayout());

		JGUIUtil.addComponent(temp3, new JLabel("Data Column(s): "), 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(temp3, __dataFieldsComboBox, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(temp3, new JLabel("    The numbers of the columns with data values to plot."), 2, 0, 1, 1, 1, 0, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, temp3, 0, y++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JPanel temp4 = new JPanel();
		temp4.setLayout(new GridBagLayout());

		JGUIUtil.addComponent(temp4, new JLabel("Equalize max values: "), 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(temp4, __equalizeCheckBox, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(temp4, new JLabel("    Whether all layer data should be plotted to the same " + "maximum value."), 2, 0, 1, 1, 1, 0, GridBagConstraints.BOTH, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, temp4, 0, y++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		PropList props = new PropList("JWorksheet");
		props.add("JWorksheet.ShowPopupMenu=true");
		props.add("JWorksheet.SelectionMode=ExcelSelection");
		props.add("JWorksheet.AllowCopy=true");
		props.add("JWorksheet.ColumnNumbering=Base1");

		JScrollWorksheet jsw = null;
		JWorksheet worksheet = null;
		try
		{
			DataTable_TableModel tm = new DataTable_TableModel(__table);
			DataTable_CellRenderer cr = new DataTable_CellRenderer(tm);

			jsw = new JScrollWorksheet(cr, tm, props);
			worksheet = jsw.getJWorksheet();
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
			jsw = new JScrollWorksheet(0, 0, props);
			worksheet = jsw.getJWorksheet();
		}
		worksheet.setPreferredScrollableViewportSize(null);
		worksheet.setHourglassJFrame(__parent);

		JLabel label = null;
		int num = __table.getNumberOfRecords();
		if (num == 1)
		{
			label = new JLabel("Only row of " + __filename);
		}
		else
		{
			label = new JLabel("First " + num + " rows of " + __filename);
		}
		JGUIUtil.addComponent(panel, label, 0, y++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, jsw, 0, y++, 10, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		IList<string> layerInfo = getAppLayersInfo(__appLayerViews);
		__list = new SimpleJList(layerInfo);
		__list.setVisibleRowCount(4);

		JLabel label2 = new JLabel("Available layer types, names, " + "and join fields:");
		JGUIUtil.addComponent(panel, label2, 0, y++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, new JScrollPane(__list), 0, y++, 10, 1, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		JPanel buttons = new JPanel();
		buttons.setLayout(new GridBagLayout());

		JButton ok = new JButton(__BUTTON_OK);
		ok.addActionListener(this);
		JGUIUtil.addComponent(buttons, ok, 0, 0, 1, 1, 1, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JButton cancel = new JButton(__BUTTON_CANCEL);
		cancel.addActionListener(this);
		JGUIUtil.addComponent(buttons, cancel, 1, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);

		JGUIUtil.addComponent(panel, buttons, 1, y++, 1, 1, 1, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);

		getContentPane().add(panel, "Center");

		__dataFieldsComboBox.addActionListener(this);
		__idFieldsComboBox.addActionListener(this);
		pack();
		setSize(getWidth(), getHeight() + 125);
		JGUIUtil.center(this);
		setVisible(true);
	}

	}

}