using System;
using System.Collections.Generic;
using System.Threading;

// GeoViewAnimationJFrame - UI for controlling a layer animation.

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
// GeoViewAnimationJFrame - GUI for controlling a layer animation.
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
// 2004-08-03	J. Thomas Sapienza, RTi	Initial version.
// 2004-08-04	JTS, RTi		Threaded animation.
// 2004-08-05	JTS, RTi		Revised to use GeoViewAnimationData
//					objects.
// 2004-08-11	JTS, RTi		Due to change in the logic of how
//					animation layers are handled and
//					controlled, the GUI was reorganized
//					and changed so that it builds layers.
// 2004-08-12	JTS, RTi		Added support for 
//					GeoViewAnimationLayerData layer control.
// 2004-08-24	JTS, RTi		Corrected bug in Radio button selected
//					time series where by all the time 
//					series' data were being set to the same
//					value as the very last one.
// 2005-04-27	JTS, RTi		Added finalize().
// 2006-03-06	JTS, RTi		JToggleButtons are sized to be the same
//					size as the JButtons.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{



	using GRSymbol = RTi.GR.GRSymbol;

	using TS = RTi.TS.TS;

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;

	using IOUtil = RTi.Util.IO.IOUtil;
	using ProcessListener = RTi.Util.IO.ProcessListener;
	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	using DateTime = RTi.Util.Time.DateTime;
	using DateTimeBuilderJDialog = RTi.Util.Time.DateTimeBuilderJDialog;

	/// <summary>
	/// This class provides a gui for controlling the animation of animation layers
	/// in geo view.
	/// REVISIT (JTS - 2006-05-23)
	/// How to use the animation stuff to animate data on a map?
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GeoViewAnimationJFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, RTi.Util.IO.ProcessListener, java.awt.event.WindowListener
	public class GeoViewAnimationJFrame : JFrame, ActionListener, ProcessListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_ACCEPT = "Accept", __BUTTON_END = ">>", __BUTTON_NEXT = ">", __BUTTON_PAUSE = "Pause", __BUTTON_PREV = "<", __BUTTON_RUN = "Start", __BUTTON_SET_DATE = "Set Current Date", __BUTTON_START = "<<", __BUTTON_STOP = "Stop";

	/// <summary>
	/// Dates used internally.  One is the current date -- this is the date for which
	/// data are currently shown on the GUI.  The start and end date are the first
	/// and last available dates of data in the time series.
	/// </summary>
	private DateTime __currentDate = null, __endDate = null, __startDate = null;

	/// <summary>
	/// Data objects that control what is animated and how.
	/// </summary>
	private GeoViewAnimationData[] __data = null;

	/// <summary>
	/// The processor that actually runs the animations.
	/// </summary>
	private GeoViewAnimationProcessor __processor = null;

	/// <summary>
	/// The component that actually draws the map display.
	/// </summary>
	private GeoViewJComponent __viewComponent = null;

	/// <summary>
	/// The panel in which the GeoView is located in the main gui.
	/// </summary>
	private GeoViewJPanel __geoViewJPanel = null;

	/// <summary>
	/// The number of data objects being managed.
	/// </summary>
	private int __numData = -1;

	/// <summary>
	/// The interval between time steps in the animation.  Determined from the first
	/// TS found.
	/// </summary>
	private int __interval = -1;

	/// <summary>
	/// GUI buttons.
	/// </summary>
	private JButton __acceptButton, __endButton, __nextButton, __prevButton, __setDateButton, __startButton, __stopButton;

	/// <summary>
	/// Array of textfields, each of which corresponds to a time series group.
	/// The textfields hold the name for each group's layer.
	/// </summary>
	private JTextField[] __layerNameTextField;

	/// <summary>
	/// The text field that displays the current date of data being shown.
	/// </summary>
	private JTextField __currentTextField;

	/// <summary>
	/// Text field that holds the amount of time to pause between animation updates.
	/// </summary>
	private JTextField __pauseTextField = null;

	/// <summary>
	/// The status bar.
	/// </summary>
	private JTextField __statusBar = null;

	/// <summary>
	/// The button for starting a run.
	/// </summary>
	private JToggleButton __pauseButton, __runButton;

	/// <summary>
	/// GUI combo boxes for selecting the start and end dates of an animation.
	/// </summary>
	private SimpleJComboBox __endComboBox, __startComboBox;

	/// <summary>
	/// List to hold all the layer views that were added to the GeoView, so that they 
	/// can be removed when the GUI is closed, if desired.
	/// </summary>
	private IList<GeoLayerView> __layers = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame on which this gui was opened. </param>
	/// <param name="geoViewJPanel"> the panel in which the geoview is found on the main gui. </param>
	/// <param name="dataVector"> the list of GeoViewAnimationData objects that defines
	/// how the GUI should be set up. </param>
	/// <param name="start"> the earliest date of data to animate. </param>
	/// <param name="end"> the last date of data to animate. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null </exception>
	/// <exception cref="Exception"> if the data list is empty </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GeoViewAnimationJFrame(javax.swing.JFrame parent, GeoViewJPanel geoViewJPanel, java.util.List<GeoViewAnimationData> dataVector, RTi.Util.Time.DateTime start, RTi.Util.Time.DateTime end) throws Exception
	public GeoViewAnimationJFrame(JFrame parent, GeoViewJPanel geoViewJPanel, IList<GeoViewAnimationData> dataVector, DateTime start, DateTime end) : base()
	{

		if (parent == null || dataVector == null || start == null || end == null || geoViewJPanel == null)
		{
			throw new System.NullReferenceException();
		}

		if (dataVector.Count == 0)
		{
			throw new Exception("Empty data Vector");
		}

		__geoViewJPanel = geoViewJPanel;
		__viewComponent = __geoViewJPanel.getGeoView();

		setTitle("Animation Control");
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		// move the data objects into an array for better speed -- also, 
		// because objects won't be cast every time they are pulled from
		// the Vector, this will run faster

		__numData = dataVector.Count;
		__data = new GeoViewAnimationData[__numData];
		for (int i = 0; i < __numData; i++)
		{
			__data[i] = (GeoViewAnimationData)dataVector[i];
		}

		dataVector = null;

		determineInterval();

		__startDate = new DateTime(start);
		__currentDate = new DateTime(__startDate);
		__endDate = new DateTime(end);

		setupGUI();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string routine = "GeoViewAnimationJFrame.actionPerformed";

		string action = @event.getActionCommand();

		int e = __endComboBox.getSelectedIndex();
		int index = __startComboBox.indexOf(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
		int s = __startComboBox.getSelectedIndex();
		int size = __startComboBox.getItemCount();

		if (action.Equals(__BUTTON_ACCEPT))
		{
			try
			{
				buildLayers();
			}
			catch (Exception ex)
			{
				Message.printWarning(1, routine, "Error building " + "GeoView layers.");
				Message.printWarning(2, routine, ex);
				return;
			}

			// Disable all the setup parts of the GUI
			IList<string> groupNames = getGroupNames();
			int groupSize = groupNames.Count;
			for (int i = 0; i < groupSize; i++)
			{
				__layerNameTextField[i].setEditable(false);
			}
			for (int i = 0; i < __numData; i++)
			{
				if (__data[i].getJCheckBox() != null)
				{
					__data[i].getJCheckBox().setEnabled(false);
				}
				if (__data[i].getJRadioButton() != null)
				{
					__data[i].getJRadioButton().setEnabled(false);
				}
			}

			__acceptButton.setEnabled(false);
			__endButton.setEnabled(true);
			__nextButton.setEnabled(true);
			__prevButton.setEnabled(true);
			__setDateButton.setEnabled(true);
			__startButton.setEnabled(true);
			__runButton.setEnabled(true);
			__pauseTextField.setEditable(true);
			__endComboBox.setEnabled(true);
			__startComboBox.setEnabled(true);
			processStatus(1, "Ready, displaying data for " + __currentDate.ToString(DateTime.FORMAT_YYYY_MM));
		}
		else if (action.Equals(__BUTTON_END))
		{
			if (index == (size - 1))
			{
				// already at the last date
				return;
			}

			__currentDate.addMonth(((size - 1) - index) * __interval);
			__currentTextField.setText(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
			fillData(__currentDate);
			__endComboBox.select(0);
			__viewComponent.redraw();
		}
		if (action.Equals(__BUTTON_NEXT))
		{
			if (index >= e)
			{
				// already at or beyond the last date
				return;
			}

			__currentDate.addMonth(__interval);
			__currentTextField.setText(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
			fillData(__currentDate);
			__viewComponent.redraw();
		}
		else if (action.Equals(__BUTTON_PAUSE))
		{
			if (__pauseButton.isSelected())
			{
				__processor.pause(true);
			}
			else
			{
				__processor.pause(false);
			}
		}
		else if (action.Equals(__BUTTON_PREV))
		{
			if (index <= s)
			{
				// already at or before the first date
				return;
			}

			__currentDate.addMonth(-1 * __interval);
			__currentTextField.setText(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
			fillData(__currentDate);
			__viewComponent.redraw();
		}
		else if (action.Equals(__BUTTON_RUN))
		{
			startAnimation();
			__endComboBox.setEnabled(false);
			__startComboBox.setEnabled(false);
			__runButton.setEnabled(false);
			__startButton.setEnabled(false);
			__prevButton.setEnabled(false);
			__nextButton.setEnabled(false);
			__endButton.setEnabled(false);
			__pauseButton.setEnabled(true);
			__stopButton.setEnabled(true);
		}
		else if (action.Equals(__BUTTON_SET_DATE))
		{
			PropList props = new PropList("");
			props.set("DatePrecision", "Month");
			new DateTimeBuilderJDialog(this, __currentTextField, __currentDate, props);
			try
			{
				__currentDate = DateTime.parse(__currentTextField.getText().Trim());
			}
			catch (Exception)
			{
			}
			__startComboBox.select(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
			fillData(__currentDate);
			__viewComponent.redraw();
		}
		else if (action.Equals(__BUTTON_START))
		{
			if (index == 0)
			{
				// already at the first date
				return;
			}
			__currentDate.addMonth(-1 * (index - 0) * __interval);
			__currentTextField.setText(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
			fillData(__currentDate);
			__viewComponent.redraw();
			__startComboBox.select(0);
		}
		else if (action.Equals(__BUTTON_STOP))
		{
			__processor.cancel();
			__runButton.setSelected(false);
			__pauseButton.setSelected(false);
			__pauseTextField.setEditable(true);
			__runButton.setEnabled(true);
			__startButton.setEnabled(true);
			__prevButton.setEnabled(true);
			__nextButton.setEnabled(true);
			__endButton.setEnabled(true);
			__stopButton.setEnabled(false);
			__pauseButton.setEnabled(false);
			__endComboBox.setEnabled(true);
			__startComboBox.setEnabled(true);
		}

		else if (@event.getSource() == __startComboBox)
		{
			if (s > e)
			{
				// if the start date was changed so that it is now
				// later than the end date, adjust the end date 
				// selection to be equal to the start date
				__endComboBox.select(s);
			}
			if (index < s)
			{
				__currentDate = new DateTime(__startDate);
				__currentDate.addMonth(s * __interval);
				__currentTextField.setText(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
				fillData(__currentDate);
				__viewComponent.redraw();
			}
		}
		else if (@event.getSource() == __endComboBox)
		{
			if (e < s)
			{
				// if the end date was changed so that it is now
				// earlier than the start date, adjust the start date
				// selection to be equal to the end date
				__startComboBox.select(e);
			}
			if (index > e)
			{
				__currentDate = new DateTime(__startDate);
				__currentDate.addMonth(e * __interval);
				__currentTextField.setText(__currentDate.ToString(DateTime.FORMAT_YYYY_MM));
				fillData(__currentDate);
				__viewComponent.redraw();
			}
		}
	}

	/// <summary>
	/// Called by the threaded animator when the animation has completed.
	/// </summary>
	protected internal virtual void animationDone()
	{
		__runButton.setSelected(false);
		__viewComponent.redraw();
		JGUIUtil.forceRepaint(__viewComponent);
	}

	/// <summary>
	/// Builds layers in the GeoView display as desired by the choices the user made in the GUI.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void buildLayers() throws Exception
	private void buildLayers()
	{
		double[] maxValues = null;
		GeoLayerView layerView = null;
		GeoViewAnimationData data = null;
		GeoViewAnimationLayerData layerData = null;
		int[] animationFields = null;
		int[] dataFields = null;
		int[] temp = null;
		int dataSize = -1;
		int size = -1;
		string layerName = null;
		IList<int> animationFieldsV = null;
		IList<GeoViewAnimationData> dataV = null;
		IList<GeoViewAnimationData> groupDataV = null;
		IList<double> maxValuesV = null;

		__layers = new List<GeoLayerView>();

		IList<string> groupNames = getGroupNames();
		int groupSize = groupNames.Count;
		for (int i = 0; i < groupSize; i++)
		{
			processStatus(1, "Building map layer #" + (i + 1) + " of " + groupSize);
			// go through each group and find out how many data items
			// are selected in each group.  For each group with at 
			// least one item selected, a layer will be built and placed
			// on the GeoView display.

			groupDataV = getGroupData(groupNames[i]);

			// mark all group data items as not visible so they will not
			// be taken into account when filling data fields.  Later,
			// only the data that are actually going to be drawn on the
			// map will be marked as visible.
			size = groupDataV.Count;
			for (int j = 0; j < size; j++)
			{
				data = groupDataV[j];
				data.setVisible(false);
			}

			dataV = findSelectedData(groupDataV);
			dataSize = dataV.Count;

			// if none of the data items are selected, skip to the next
			// group

			if (dataSize == 0)
			{
				continue;
			}

			animationFieldsV = new List<int>();
			maxValuesV = new List<double>();

			// find out the animation fields and the max values that will
			// need to be passed in to addSummaryView for this layer.

			for (int j = 0; j < dataSize; j++)
			{
				data = dataV[j];
				data.setVisible(true);
				animationFieldsV.Add(new int?(data.getAttributeField()));
				maxValuesV.Add(new double?(data.getAnimationFieldMax()));
			}

			// get the GeoViewAnimationLayerData object that tells much
			// about how this layer should be built

			layerData = dataV[0].getGeoViewAnimationLayerData();

			// build the animation fields array from the fields in the
			// data that are set as being animation fields

			size = animationFieldsV.Count;
			animationFields = new int[size];
			for (int j = 0; j < size; j++)
			{
				animationFields[j] = animationFieldsV[j];
			}

			temp = layerData.getDataFields();

			// dataFields are handled differently for different symbols.
			// For non-complicated symbols, the data fields specified in the
			// animation layer data are the fields that will always appear
			// on the display, regardless of whether they are animated or
			// not.  These fields will be combined with the fields stored
			// in the GeoViewAnimationData objects as animation fields.

			// For complicated symbols, such as teacups, the data fields
			// are used specially within addSummaryLayerView() to define
			// some settings.  For teacups, the first element is the field
			// with the maximum content of the teacup, the second element
			// is the field with the minimum content of the teacup, and
			// the third element is the field with the current content of
			// the teacup.

			if (layerData.getSymbolType() == GRSymbol.SYM_TEACUP)
			{
				dataFields = new int[temp.Length];
				for (int j = 0; j < temp.Length; j++)
				{
					dataFields[j] = temp[j];
				}
				animationFields = dataFields;
			}
			else
			{
				dataFields = new int[temp.Length + size];
				for (int j = 0; j < temp.Length; j++)
				{
					dataFields[j] = temp[j];
				}
				for (int j = temp.Length; j < size; j++)
				{
					dataFields[j] = animationFields[j - temp.Length];
				}
			}

			// get out the maximum values for the animation fields

			size = maxValuesV.Count;
			maxValues = new double[size];
			for (int j = 0; j < size; j++)
			{
				maxValues[j] = maxValuesV[j];
			}

			// if the user has set up an alternate layer name in the GUI
			// use it instead of the default one defined in GeoViewAnimationLayerData

			layerName = dataV[0].getLayerNameTextField().getText().Trim();

			if (layerName.Equals(""))
			{
				layerName = layerData.getLayerName();
			}

			// build the layer

			PropList props = layerData.getProps();
			/*
			if (props == null) {
				props = new PropList("");
			}
			props.add("PositiveBarColor.1 = Red");
			props.add("PositiveBarColor.2 = Yellow");
			props.add("NegativeBarColor.1 = Green");
			props.add("NegativeBarColor.2 = Blue");
			*/
			layerView = __geoViewJPanel.addSummaryLayerView(layerData.getTable(), layerData.getSymbolType(), layerName, layerData.getIDFields(), dataFields, layerData.getAvailAppLayerTypes(), layerData.getEqualizeMax(), animationFields, maxValues, props);

			// set some final settings on the layer

			layerView.setAnimationControlJFrame(this);
			layerView.setMissingDoubleValue(layerData.getMissingDoubleValue());
			layerView.setMissingDoubleReplacementValue(layerData.getMissingDoubleReplacementValue());

			__layers.Add(layerView);
		}
	}

	/// <summary>
	/// Builds the time series panel from which different time series can be 
	/// turned on or off in the animation layer. </summary>
	/// <param name="panel"> the JPanel on which to build the GUI information. </param>
	private void buildTimeSeriesPanel(JPanel panel)
	{
		string groupName = null;
		IList<int> dataNums = null;
		IList<string> groupNames = getGroupNames();
		int size = groupNames.Count;
		__layerNameTextField = new JTextField[size];
		for (int i = 0; i < size; i++)
		{
			groupName = groupNames[i];
			dataNums = getGroupDataNums(groupName);
			processGroup(panel, (i + 1), groupName, dataNums);
		}
	}

	/// <summary>
	/// Determines the interval for the time series.  It does this by looking through
	/// all the data objects for time series.  It takes the interval from the very 
	/// first time series it finds. </summary>
	/// <exception cref="Exception"> if no valid interval could be found </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void determineInterval() throws Exception
	private void determineInterval()
	{
		int num = 0;
		TS ts = null;
		for (int i = 0; i < __numData; i++)
		{
			num = __data[i].getNumTimeSeries();
			for (int j = 0; j < num; j++)
			{
				ts = __data[i].getTimeSeries(j);
				if (ts != null)
				{
					__interval = ts.getIdentifier().getIntervalMult();
					return;
				}
			}
		}

		throw new Exception("Could not find a valid time series from which " + "to determine the interval");
	}

	/// <summary>
	/// Populates the date combo boxes.
	/// </summary>
	private void fillComboBoxes()
	{
		// REVISIT (JTS - 2004-08-04)
		// pretty much configured solely for months right now.
		// Will worry about hourly/etc TS later.  At least provides
		// support for doing different intervals right now

		DateTime d = new DateTime(__startDate);
		IList<string> v = new List<string>();

		v.Add(__startDate.ToString(DateTime.FORMAT_YYYY_MM));

		d.addMonth(__interval);

		while (d.lessThan(__endDate))
		{
			v.Add(d.ToString(DateTime.FORMAT_YYYY_MM));
			d.addMonth(__interval);
		}

		v.Add(__endDate.ToString(DateTime.FORMAT_YYYY_MM));

		__startComboBox.setData(v);
		__endComboBox.setData(v);
		__endComboBox.select(__endComboBox.getItemCount() - 1);

		__currentTextField.setText(__startDate.ToString(DateTime.FORMAT_YYYY_MM));
	}

	/// <summary>
	/// Fills attribute table data from time series for the given date. </summary>
	/// <param name="date"> the date for which to put data into the attribute table. </param>
	protected internal virtual void fillData(DateTime date)
	{
		for (int i = 0; i < __numData; i++)
		{
			__data[i].fillData(date);
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewAnimationJFrame()
	{
		__currentDate = null;
		__endDate = null;
		__startDate = null;
		IOUtil.nullArray(__data);
		__processor = null;
		__viewComponent = null;
		__geoViewJPanel = null;
		__acceptButton = null;
		__endButton = null;
		__nextButton = null;
		__prevButton = null;
		__setDateButton = null;
		__startButton = null;
		__stopButton = null;
		IOUtil.nullArray(__layerNameTextField);
		__currentTextField = null;
		__pauseTextField = null;
		__statusBar = null;
		__pauseButton = null;
		__runButton = null;
		__endComboBox = null;
		__startComboBox = null;
		__layers = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Given a list of GeoViewAnimationData objects, this searches it and returns
	/// a list of all the data objects that have their radio button or check box
	/// selected. </summary>
	/// <param name="dataV"> a list of GeoViewAnimationData objects.  Can be null. </param>
	/// <returns> a list of all the GeoViewAnimationData objects in the passed-in 
	/// list that have their radio button or check box selected.  Guaranteed to be non-null. </returns>
	private IList<GeoViewAnimationData> findSelectedData(IList<GeoViewAnimationData> dataV)
	{
		if (dataV == null || dataV.Count == 0)
		{
			return new List<GeoViewAnimationData>();
		}

		GeoViewAnimationData data = null;
		int size = dataV.Count;
		IList<GeoViewAnimationData> v = new List<GeoViewAnimationData>();

		for (int i = 0; i < size; i++)
		{
			data = dataV[i];

			// data objects can either use a checkbox for selection
			// or a radio button, but not both.  If one is null, the other must be non-null.

			if (data.getJCheckBox() != null && data.getJCheckBox().isSelected())
			{
				v.Add(data);
			}
			else if (data.getJRadioButton() != null && data.getJRadioButton().isSelected())
			{
				v.Add(data);
			}
		}
		return v;
	}

	/// <summary>
	/// Returns all the GeoViewAnimationData objects that have the given group name. </summary>
	/// <param name="groupName"> the name of the group for which to return data. </param>
	/// <returns> a list of GeoViewAnimationData objects which have the given group
	/// name.  This list may be empty if the group name is not matched, but it will never be null. </returns>
	private IList<GeoViewAnimationData> getGroupData(string groupName)
	{
		IList<GeoViewAnimationData> found = new List<GeoViewAnimationData>();
		for (int i = 0; i < __numData; i++)
		{
			if (groupName.Equals(__data[i].getGroupName(), StringComparison.OrdinalIgnoreCase))
			{
				found.Add(__data[i]);
			}
		}
		return found;
	}

	/// <summary>
	/// Returns all the data objects that have the given group name.  The group name
	/// is compared without case sensitivity. </summary>
	/// <param name="groupName"> the name of the group for which to return data objects. </param>
	/// <returns> a list of the data objects that match the group name.  Guaranteed
	/// to be non-null. </returns>
	private IList<int> getGroupDataNums(string groupName)
	{
		IList<int> found = new List<int>();
		for (int i = 0; i < __numData; i++)
		{
			if (groupName.Equals(__data[i].getGroupName(), StringComparison.OrdinalIgnoreCase))
			{
				found.Add(new int?(i));
			}
		}
		return found;
	}

	/// <summary>
	/// Gets the names of all the data object groups.  Data group names are compared
	/// case-insensitively. </summary>
	/// <returns> a list of all the unique data object group names. </returns>
	private IList<string> getGroupNames()
	{
		bool found = false;
		string s;
		IList<string> foundV = new List<string>();
		for (int i = 0; i < __numData; i++)
		{
			found = false;
			for (int j = 0; j < foundV.Count; j++)
			{
				s = foundV[j];
				if (s.Equals(__data[i].getGroupName(), StringComparison.OrdinalIgnoreCase))
				{
					found = true;
				}
			}

			if (!found)
			{
				foundV.Add(__data[i].getGroupName());
			}
		}

		return foundV;
	}

	/// <summary>
	/// Adds a group section to the display. </summary>
	/// <param name="panel"> the main panel on which the group display sections will be added. </param>
	/// <param name="panelY"> the y position (in a GridBagLayout) at which the current group 
	/// section will be placed. </param>
	/// <param name="groupName"> the name of the group for which to add a section. </param>
	/// <param name="dataNums"> a Vector of Integers, each of which is the index within the
	/// __data array of one of the members of the group to add to the panel. </param>
	private void processGroup(JPanel panel, int panelY, string groupName, IList<int> dataNums)
	{
		// TODO SAM 2007-05-09 Evaluate if needed
		//boolean visible = false;
		ButtonGroup buttonGroup = new ButtonGroup();
		int dataNum = -1;
		int selectType = -1;
		int size = dataNums.Count;
		JCheckBox checkBox = null;
		JRadioButton radioButton = null;
		JPanel subPanel = new JPanel();
		subPanel.setLayout(new GridBagLayout());
		subPanel.setBorder(BorderFactory.createTitledBorder(groupName));

		int y = 0;

		__layerNameTextField[panelY - 1] = new JTextField(20);

		JGUIUtil.addComponent(subPanel, new JLabel("Layer Name: "), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(subPanel, __layerNameTextField[panelY - 1], 1, y++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

		for (int i = 0; i < size; i++)
		{
			dataNum = dataNums[i];

			// take the type of selection (CheckBox or RadioButton)
			// from the very first data object for this group
			if (i == 0)
			{
				selectType = __data[dataNum].getSelectType();
			}

			if (selectType == GeoViewAnimationData.CHECKBOX)
			{
				checkBox = new JCheckBox((string)null, __data[dataNum].isVisible());
				__data[dataNum].setJCheckBox(checkBox);
			}
			else
			{
				// for radio buttons, the very first radio button 
				// is selected by default (so all the others are not)
				if (i == 0)
				{
					radioButton = new JRadioButton((string)null, true);
				}
				else
				{
					radioButton = new JRadioButton();
				}
				buttonGroup.add(radioButton);
				__data[dataNum].setJRadioButton(radioButton);
			}

			JGUIUtil.addComponent(subPanel, new JLabel(__data[dataNum].getGUILabel()), 0, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);

			if (selectType == GeoViewAnimationData.CHECKBOX)
			{
				JGUIUtil.addComponent(subPanel, checkBox, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
			}
			else
			{
				JGUIUtil.addComponent(subPanel, radioButton, 1, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
			}
			/* TODO SAM 2007-05-09 Evaluate if needed
			if (selectType == GeoViewAnimationData.RADIOBUTTON) {
				if (i == 0) {
					visible = true;
				}		
				else {
					visible = false;
				}
			}
			else {
				visible = __data[dataNum].isVisible();
			}
			*/

			__layerNameTextField[panelY - 1].setText(__data[dataNum].getGeoViewAnimationLayerData().getLayerName());
			__data[dataNum].setLayerNameTextField(__layerNameTextField[panelY - 1]);
			y++;
		}
		JGUIUtil.addComponent(panel, subPanel, 0, panelY, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
	}

	/// <summary>
	/// Handles error processing from the animation processor.  From ProcessListener. </summary>
	/// <param name="error"> the error message that occurred. </param>
	public virtual void processError(string error)
	{
	}

	/// <summary>
	/// Handles output processing messages from the animation processor.  From 
	/// ProcessListener. </summary>
	/// <param name="output"> the output message that occurred. </param>
	public virtual void processOutput(string output)
	{
	}

	/// <summary>
	/// Handles status messages from the animation processor.  From ProcessListener. </summary>
	/// <param name="code"> 0 for sending messages to the current date textfield, 1 for sending
	/// messages to the status bar. </param>
	/// <param name="message"> the message to send. </param>
	public virtual void processStatus(int code, string message)
	{
		if (code == 0)
		{
			__currentTextField.setText(message);
			JGUIUtil.forceRepaint(__currentTextField);
		}
		else if (code == 1)
		{
			__statusBar.setText(message);
			JGUIUtil.forceRepaint(__statusBar);
		}
	}

	/// <summary>
	/// Sets up the GUI.  Does not make the GUI visible, as developers must add
	/// time series to the frame first, and then call setVisible() manually.
	/// </summary>
	private void setupGUI()
	{
		addWindowListener(this);

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		int y = 0;

		__startComboBox = new SimpleJComboBox();
		__endComboBox = new SimpleJComboBox();
		__currentTextField = new JTextField(15);

		JPanel topPanel = new JPanel();
		topPanel.setLayout(new GridBagLayout());
		topPanel.setBorder(BorderFactory.createTitledBorder("Animation Setup"));
		int topY = 0;

		JPanel bottomPanel = new JPanel();
		bottomPanel.setLayout(new GridBagLayout());
		bottomPanel.setBorder(BorderFactory.createTitledBorder("Animation Control"));

		JPanel timePanel = new JPanel();
		timePanel.setLayout(new GridBagLayout());
		timePanel.setBorder(BorderFactory.createTitledBorder("Animation Times"));

		__pauseTextField = new JTextField("XXXXX");

		int ty = 0;

		JGUIUtil.addComponent(timePanel, new JLabel("Start:"), 0, ty, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(timePanel, __startComboBox, 1, ty++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__startComboBox.setEnabled(false);

		JGUIUtil.addComponent(timePanel, new JLabel("End:"), 0, ty, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(timePanel, __endComboBox, 1, ty++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__endComboBox.setEnabled(false);

		JGUIUtil.addComponent(timePanel, new JLabel("Current:"), 0, ty, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(timePanel, __currentTextField, 1, ty++, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__currentTextField.setEditable(false);

		__setDateButton = new JButton(__BUTTON_SET_DATE);
		__setDateButton.addActionListener(this);
		__setDateButton.setToolTipText("Set the current date.");
		JGUIUtil.addComponent(timePanel, __setDateButton, 1, ty++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__setDateButton.setEnabled(false);

		JPanel timeSeriesPanel = new JPanel();
		timeSeriesPanel.setLayout(new GridBagLayout());
		buildTimeSeriesPanel(timeSeriesPanel);

		JPanel buttons = new JPanel();
		buttons.setLayout(new GridBagLayout());
		__startButton = new JButton(__BUTTON_START);
		__startButton.addActionListener(this);
		__startButton.setToolTipText("Set current date to start.");
		__startButton.setEnabled(false);
		__prevButton = new JButton(__BUTTON_PREV);
		__prevButton.addActionListener(this);
		__prevButton.setToolTipText("Go to previous date.");
		__prevButton.setEnabled(false);
		__runButton = new JToggleButton(__BUTTON_RUN);
		__runButton.addActionListener(this);
		__runButton.setToolTipText("Start animating at current date.");
		__runButton.setEnabled(false);
		__pauseButton = new JToggleButton(__BUTTON_PAUSE);
		__pauseButton.addActionListener(this);
		__pauseButton.setToolTipText("Pause at current date.");
		__pauseButton.setEnabled(false);
		__stopButton = new JButton(__BUTTON_STOP);
		__stopButton.addActionListener(this);
		__stopButton.setToolTipText("Stop at current date.");
		__stopButton.setEnabled(false);
		__nextButton = new JButton(__BUTTON_NEXT);
		__nextButton.addActionListener(this);
		__nextButton.setToolTipText("Go to next date.");
		__nextButton.setEnabled(false);
		__endButton = new JButton(__BUTTON_END);
		__endButton.addActionListener(this);
		__endButton.setToolTipText("Set current date to end.");
		__endButton.setEnabled(false);

		__acceptButton = new JButton(__BUTTON_ACCEPT);
		__acceptButton.addActionListener(this);
		__acceptButton.setToolTipText("Accept layer settings and set up " + "GeoView display for animation.");

		int x = 0;
		JGUIUtil.addComponent(buttons, __startButton, x++, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(buttons, __prevButton, x++, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(buttons, __runButton, x++, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(buttons, __pauseButton, x++, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(buttons, __stopButton, x++, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(buttons, __nextButton, x++, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(buttons, __endButton, x++, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

		timeSeriesPanel.setBorder(BorderFactory.createTitledBorder("Time Series Data"));
		JGUIUtil.addComponent(topPanel, timeSeriesPanel, 0, topY++, 2, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(topPanel, __acceptButton, 3, topY++, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(panel, topPanel, 0, y++, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		JGUIUtil.addComponent(bottomPanel, new JLabel("Pause (seconds):"), 2, y, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHEAST);
		JGUIUtil.addComponent(bottomPanel, __pauseTextField, 3, y, 1, 1, 1, 1, GridBagConstraints.HORIZONTAL, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(bottomPanel, timePanel, 0, y++, 2, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(bottomPanel, buttons, 0, y++, 10, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(panel, bottomPanel, 0, y++, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.NORTHWEST);

		getContentPane().add("Center", panel);

		__statusBar = new JTextField("");
		__statusBar.setEditable(false);
		getContentPane().add("South", __statusBar);

		__pauseTextField.setText("5");
		__pauseTextField.setEditable(false);

		fillComboBoxes();

		__startComboBox.addActionListener(this);
		__endComboBox.addActionListener(this);

		JGUIUtil.center(this);

		pack();

		// By default, toggle buttons are smaller than normal JButtons.  The
		// following resizes them to be the same size.

		__pauseButton.setPreferredSize(new java.awt.Dimension(__pauseButton.getWidth(), __prevButton.getHeight()));
		__runButton.setPreferredSize(new java.awt.Dimension(__runButton.getWidth(), __prevButton.getHeight()));
		__pauseButton.setSize(new java.awt.Dimension(__pauseButton.getWidth(), __prevButton.getHeight()));
		__runButton.setSize(new java.awt.Dimension(__runButton.getWidth(), __prevButton.getHeight()));
		__pauseButton.setMinimumSize(new java.awt.Dimension(__pauseButton.getWidth(), __prevButton.getHeight()));
		__runButton.setMinimumSize(new java.awt.Dimension(__runButton.getWidth(), __prevButton.getHeight()));

		// put the fist date's data into the attribute table
		fillData(__startDate);
	}

	/// <summary>
	/// Starts the threaded animation.
	/// </summary>
	public virtual void startAnimation()
	{
		__pauseTextField.setEditable(false);

		string p = __pauseTextField.getText().Trim();
		int millis = 1500;
		try
		{
			double d = (Convert.ToDouble(p)) * 1000;
			millis = (int)d;
		}
		catch (Exception)
		{
			__pauseTextField.setText("1.5");
		}

		int s = __startComboBox.getSelectedIndex();
		int e = __endComboBox.getSelectedIndex();
		int steps = (e - s) + 1;
		DateTime curr = new DateTime(__startDate);
		curr.addMonth(s * __interval);
		DateTime end = new DateTime(curr);
		end.addMonth(steps * __interval);

		if (__processor == null)
		{
			__processor = new GeoViewAnimationProcessor(this, __viewComponent, curr, end, millis);
			__processor.addProcessListener(this);
		}
		else
		{
			__processor.setStartDate(curr);
			__processor.setEndDate(end);
			__processor.setPause(millis);
		}

		// REVISIT (JTS - 2004-08-11)
		// single thread ever?
		Thread thread = new Thread(__processor);
		thread.Start();
	}

	/// <summary>
	/// Forces the window to repaint.
	/// </summary>
	public virtual void windowActivated(WindowEvent @event)
	{
		invalidate();
		validate();
		repaint();
		if (__processor != null)
		{
			try
			{
				__processor.sleep(200);
			}
			catch (Exception)
			{
			}
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosed(WindowEvent @event)
	{
	}

	/// <summary>
	/// Removes layers from the GeoView if the user desires. </summary>
	/// <param name="event"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		if (__layers == null || __layers.Count == 0)
		{
			return;
		}

		if (__processor != null)
		{
			__processor.cancel();
		}

		ResponseJDialog r = new ResponseJDialog(this, "Remove Animation Layers?", "Should the animation layers that were added from this " + "display be removed from the map?", ResponseJDialog.YES | ResponseJDialog.NO);
		int x = r.response();

		if (x == ResponseJDialog.NO)
		{
			return;
		}

		bool redraw = false;
		GeoLayerView layerView = null;
		int size = __layers.Count;
		for (int i = 0; i < size; i++)
		{
			layerView = (GeoLayerView)__layers[i];

			if (i == (size - 1))
			{
				redraw = true;
			}
			else
			{
				redraw = false;
			}
			__geoViewJPanel.removeLayerView(layerView, redraw);
		}
		__viewComponent.redraw();
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeactivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeiconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowIconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowOpened(WindowEvent @event)
	{
	}

	}

}