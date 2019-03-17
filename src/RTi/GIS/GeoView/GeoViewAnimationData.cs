using System;
using System.Collections.Generic;

// GeoViewAnimationData - data for controlling layer animation.

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
// GeoViewAnimationData - Data for controlling layer animation.
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
// 2004-08-05	JTS, RTi		Threaded animation.
// 2004-08-10	JTS, RTi		Added helper time series (mostly for
//					use with teacup displays).
// 2004-08-12	JTS, RTi		Added GeoViewAnimationLayerData.
// 2004-08-24	JTS, RTi		Added setVisible().
// 2005-04-27	JTS, RTi		Added all member variables to finalize()
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{


	using TS = RTi.TS.TS;

	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	using DataTable = RTi.Util.Table.DataTable;

	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This class holds data that tell a GeoViewAnimationJFrame how to initialize 
	/// and control animation for layers on a GeoView.  The purpose of the 
	/// GeoViewAnimationData objects is to set up all the information for controlling
	/// animation of a particular time series and also to tell the GUI how it should
	/// set itself for display of that time series.
	/// </summary>
	public class GeoViewAnimationData
	{

	/// <summary>
	/// Determines whether a data group will have a radio button or a checkbox for
	/// turning time series displays on the map on or off.  Radio button data
	/// should all be set up to put their data in the same field in the attribute table,
	/// because only one can be visible at a time.  Checkbox data should all have
	/// separate fields for entering data.
	/// </summary>
	public const int CHECKBOX = 0, RADIOBUTTON = 1;

	/// <summary>
	/// Whether the time series managed by this object is initially visible in an 
	/// animation.
	/// </summary>
	private bool __visible = false;

	/// <summary>
	/// The attribute table in which the animation data are stored.  This is a reference
	/// to the attribute table in the GeoLayer.
	/// </summary>
	private DataTable __table = null;

	/// <summary>
	/// The maximum value for the animation field (used with equalize max).
	/// </summary>
	private double __animationFieldMax = -1;

	/// <summary>
	/// The object that controls how the layer that may hold this animation data
	/// will be built.
	/// </summary>
	private GeoViewAnimationLayerData __layerData = null;

	/// <summary>
	/// The number of the field in the attribute table that stores the data for this
	/// object's time series -- this is the data that will be animated.
	/// </summary>
	private int __attributeField = -1;

	/// <summary>
	/// The number of the field in the attribute table that stores the date that 
	/// was last animated.
	/// </summary>
	private int __dateField = -1;

	/// <summary>
	/// The fields into which time series helper data are placed.  The correspond to
	/// the __helpers[] array.  
	/// </summary>
	private int[] __helperFields = null;

	/// <summary>
	/// The type of selection done to turn a time series visible or not on the GeoView.
	/// Must be one of RADIOBUTTON or CHECKBOX.
	/// </summary>
	private int __selectType = 0;

	/// <summary>
	/// If using CHECKBOX as the selection type, the checkbox that corresponds to this
	/// object's data.
	/// </summary>
	private JCheckBox __checkBox = null;

	/// <summary>
	/// If using RADIOBUTTON as the selection type, the radio button that corresponds
	/// to this object's data.
	/// </summary>
	private JRadioButton __radioButton = null;

	/// <summary>
	/// The text field on the GUI that can be used to name this layer if the layer
	/// name provided with the GeoViewAnimationLayerData is not the one desired.
	/// </summary>
	private JTextField __layerNameTextField = null;

	/// <summary>
	/// The name of the field in the attribute table that holds this object's time
	/// series data.
	/// </summary>
	private string __attributeFieldName = null;

	/// <summary>
	/// The name of the field in the attribute table that holds this object's date
	/// data.
	/// </summary>
	private string __dateFieldName = null;

	/// <summary>
	/// The name of the group to which this object belongs.  This is used to group
	/// time series together on the GUI.  Example: "Reservoir EOM Time Series".
	/// </summary>
	private string __groupName = null;

	/// <summary>
	/// The label put next to this object's checkbox or radio button on the GUI.
	/// Example: "Reservoir EOM: ".  
	/// </summary>
	private string __guiLabel = null;

	/// <summary>
	/// Helper time series that are necessary to fill in all the data in the table.
	/// Because every GeoViewAnimationData object is used to create a section in 
	/// the GUI from which a time series animation can be turned on or off, the helpers
	/// are used to fill in additional data for displays which use more than one
	/// data point to draw the animation.<para>
	/// </para>
	/// For example, for teacup displays three time series are used: <para>
	/// <ol><li>Drawing the maximum fill level</li>
	/// <li>Drawing the minimum fill level</li>
	/// <li>Drawing the current fill level</li></ol>
	/// The main time series (current fill level) is passed into the data object when
	/// it is created.  The helper time series (max and min) are simply constant 
	/// </para>
	/// time series where every data value is the same, and are passed in as helpers.<para>
	/// __helpers[] corresponds to the __helperFields[] array, while __helpers[][] 
	/// corresponds to the rows in the attribute table.
	/// </para>
	/// </summary>
	private TS[][] __helpers = null;

	/// <summary>
	/// The time series this data object manages.
	/// </summary>
	private TS[] __tsArray = null;

	/// <summary>
	/// Constructor.  Builds with a selection type of CHECKBOX. </summary>
	/// <param name="layerData"> a GeoViewAnimationLayerData object that has information 
	/// on how to construct the layer on which this data will appear. </param>
	/// <param name="tsList"> a Vector of time series that correspond to rows in the 
	/// attribute table of the animation layer view.  Element X in this Vector 
	/// corresponds to row X in the table. </param>
	/// <param name="attributeFieldName"> the name of the field in the attribute table where
	/// the values from this object's time series will be placed. </param>
	/// <param name="dateFieldName"> the name of the field in the attribute table where
	/// the date of the data last animated is stored. </param>
	/// <param name="guiLabel"> the String that will be placed on the animation control GUI
	/// next to the data managed by this object. </param>
	/// <param name="groupname"> the name of the group to which this data object belongs.
	/// Group names are used to organize groups of time series on the GUI, and are
	/// used with the selectionType to know whether to use checkboxes or radiobuttons 
	/// to select among different time series. </param>
	/// <param name="visible"> whether this time series is initially visible on the display. </param>
	/// <param name="selectType"> the way by which users will choose among time series with
	/// the same group name for which should be visible.  All of the data objects 
	/// passed to the animation control GUI that have the same group name (which is
	/// compared without case sensitivity) are placed in the same section of the GUI.
	/// If selectionType is CHECKBOX, any data object's time series can be turned on
	/// or off and not affect the visibility of other time series in the same group.
	/// If selectionType is RADIOBUTTON, turning on one time series so that it is 
	/// visible will make all the other time series in the same group not visible.  If
	/// different objects in the same group have different selectTypes, the type that
	/// will be used is the first one the GUI finds for a given group. </param>
	/// <param name="animationFieldMax"> the maximum value of the data in the animation field. </param>
	/// <exception cref="NullPointerException"> if any of the parameters are null. </exception>
	/// <exception cref="Exception"> if the selectionType is not CHECKBOX or RADIOBUTTON. </exception>
	/// <exception cref="Exception"> if the attribute field name passed to the constructor
	/// cannot be found in the layer view's attribute table. </exception>
	/// <exception cref="Exception"> if the date field name passed to the constructor
	/// cannot be found in the layer view's attribute table. </exception>
	/// <exception cref="Exception"> if the number of rows in the attribute table does not equal
	/// the size of the TS Vector passed to the constructor. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GeoViewAnimationData(GeoViewAnimationLayerData layerData, java.util.List<RTi.TS.TS> tsList, String attributeFieldName, String dateFieldName, String guiLabel, String groupName, boolean visible, int selectType, double animationFieldMax) throws Exception
	public GeoViewAnimationData(GeoViewAnimationLayerData layerData, IList<TS> tsList, string attributeFieldName, string dateFieldName, string guiLabel, string groupName, bool visible, int selectType, double animationFieldMax)
	{
		if (tsList == null || string.ReferenceEquals(attributeFieldName, null) || string.ReferenceEquals(dateFieldName, null) || string.ReferenceEquals(guiLabel, null) || string.ReferenceEquals(groupName, null) || layerData == null)
		{
			throw new System.NullReferenceException();
		}

		if (selectType != CHECKBOX && selectType != RADIOBUTTON)
		{
			throw new Exception("Invalid select value: " + selectType);
		}

		__layerData = layerData;

		// put into an array for faster access

		int size = tsList.Count;
		__tsArray = new TS[size];
		for (int i = 0; i < size; i++)
		{
			__tsArray[i] = (TS)tsList[i];
		}

		tsList = null;

		__attributeFieldName = attributeFieldName;
		__dateFieldName = dateFieldName;
		__guiLabel = guiLabel;
		__groupName = groupName;
		__visible = visible;
		__selectType = selectType;
		__animationFieldMax = animationFieldMax;

		initialize();
	}

	/// <summary>
	/// Puts data into the attribute table from the time series for the given date. </summary>
	/// <param name="date"> the date for which to put data into the attribute table.  Cannot
	/// be null. </param>
	protected internal virtual void fillData(DateTime date)
	{
		if (!__visible)
		{
			// not the visible field (in radio button sections,
			// only one data can be visible at a time), so return
			return;
		}

		for (int i = 0; i < __tsArray.Length; i++)
		{
			if (__tsArray[i] != null)
			{

			//if (i == 0 && __visible) {
			//	Message.printStatus(1, "", "'" + __guiLabel + "'");
			//	Message.printStatus(1, "", "   " + date + ": " 
			//		+ __tsArray[i].getDataValue(date));
			//}
			//else if (i == 0 && !__visible) {
			//	Message.printStatus(1, "", "--" + __guiLabel);
			//}

				try
				{
					__table.setFieldValue(i, __attributeField, new double?(__tsArray[i].getDataValue(date)));
				}
				catch (Exception e)
				{
					Message.printWarning(2, "GeoViewAnimationData.fillData", "Error putting data into the attribute " + "table in row " + i);
					Message.printWarning(2, "GeoViewAnimationData.fillData", e);
				}

				try
				{
					__table.setFieldValue(i, __dateField, date);
				}
				catch (Exception e)
				{
					Message.printWarning(2, "GeoViewAnimationData.fillData", "Error putting date data into the " + "attribute table in row " + i);
					Message.printWarning(2, "GeoViewAnimationData.fillData", e);
				}
			}

			if (__helpers != null)
			{
				for (int j = 0; j < __helpers.Length; j++)
				{
					if (__helpers[j][i] != null)
					{
					try
					{
					__table.setFieldValue(i, __helperFields[j], new double?(__helpers[j][i].getDataValue(date)));
					}
					catch (Exception)
					{
	//					e.printStackTrace();
					}
					}
				}
			}
		}
	}

	/// <summary>
	/// Cleans up member data.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewAnimationData()
	{
		__table = null;
		__layerData = null;
		__helperFields = null;
		__checkBox = null;
		__radioButton = null;
		__attributeFieldName = null;
		__dateFieldName = null;
		__groupName = null;
		__guiLabel = null;
		__helpers = null;
		IOUtil.nullArray(__tsArray);
		__layerNameTextField = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the maximum value for the animation field. </summary>
	/// <returns> the maximum value for the animation field. </returns>
	public virtual double getAnimationFieldMax()
	{
		return __animationFieldMax;
	}

	/// <summary>
	/// Returns the attribute field number. </summary>
	/// <returns> the attribute field number. </returns>
	public virtual int getAttributeField()
	{
		return __attributeField;
	}

	/// <summary>
	/// Returns the name of the attribute field. </summary>
	/// <returns> the name of the attribute field. </returns>
	public virtual string getAttributeFieldName()
	{
		return __attributeFieldName;
	}

	/// <summary>
	/// Returns the GeoViewAnimationLayerData object used to build the layer for
	/// this animation. </summary>
	/// <returns> the GeoViewAnimationLayerData object for this data. </returns>
	public virtual GeoViewAnimationLayerData getGeoViewAnimationLayerData()
	{
		return __layerData;
	}

	/// <summary>
	/// Returns the group name. </summary>
	/// <returns> the group name. </returns>
	public virtual string getGroupName()
	{
		return __groupName;
	}

	/// <summary>
	/// Returns the GUI label. </summary>
	/// <returns> the GUI label. </returns>
	public virtual string getGUILabel()
	{
		return __guiLabel;
	}

	/// <summary>
	/// Returns the JCheckBox this object uses.  If it uses a JRadioButton instead,
	/// this will return null. </summary>
	/// <returns> the JCheckBox this object uses. </returns>
	public virtual JCheckBox getJCheckBox()
	{
		return __checkBox;
	}

	/// <summary>
	/// Returns the JRadioButton this object uses.  If it uses a JCheckBox instead,
	/// this will return null. </summary>
	/// <returns> the JCheckBox this object uses. </returns>
	public virtual JRadioButton getJRadioButton()
	{
		return __radioButton;
	}

	/// <summary>
	/// Returns the textfield into which users can enter a name for the layer this
	/// data appears in. </summary>
	/// <returns> the textfield into which users can enter a name for the layer this
	/// data appears in. </returns>
	protected internal virtual JTextField getLayerNameTextField()
	{
		return __layerNameTextField;
	}

	/// <summary>
	/// Returns the number of time series managed by this object. </summary>
	/// <returns> the number of time series managed by this object. </returns>
	public virtual int getNumTimeSeries()
	{
		if (__tsArray == null)
		{
			return 0;
		}
		return __tsArray.Length;
	}

	/// <summary>
	/// Returns this object's select type. </summary>
	/// <returns> this object's select type. </returns>
	public virtual int getSelectType()
	{
		return __selectType;
	}

	/// <summary>
	/// Returns the given time series. </summary>
	/// <param name="num"> the number of the time series to return. </param>
	/// <returns> the given time series. </returns>
	public virtual TS getTimeSeries(int num)
	{
		return __tsArray[num];
	}

	/// <summary>
	/// Initializes internal settings. </summary>
	/// <exception cref="Exception"> if the attribute field name passed to the constructor
	/// cannot be found in the layer view's attribute table. </exception>
	/// <exception cref="Exception"> if the attribute date field name passed to the constructor
	/// cannot be found in the layer view's attribute table. </exception>
	/// <exception cref="Exception"> if the number of rows in the attribute table does not equal
	/// the size of the TS Vector passed to the constructor. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize() throws Exception
	private void initialize()
	{
		__table = __layerData.getTable();
		__attributeField = __table.getFieldIndex(__attributeFieldName);
		__dateField = __table.getFieldIndex(__dateFieldName);
			// the above will throw an exception if the fields
			// are not found

		int rows = __table.getNumberOfRecords();
		int size = __tsArray.Length;

		if (rows != size)
		{
			throw new Exception("Mismatch in number of rows in attribute " + "table (" + rows + ") and number of time series (" + size + ").  Both values must be the same.");
		}
	}

	/// <summary>
	/// Returns whether this object's time series is visible. </summary>
	/// <returns> whether this object's time series is visible. </returns>
	public virtual bool isVisible()
	{
		return __visible;
	}

	/// <summary>
	/// Sets a helper time series in place. </summary>
	/// <param name="helperNum"> the number of the helper time series (base 0) </param>
	/// <param name="tsNum"> the time series to which this data is a helper (corresponds to
	/// the rows in the attribute table). </param>
	/// <param name="ts"> the time series to place </param>
	/// <param name="field"> the field in the table to which the time series corresponds. </param>
	public virtual void setHelperTS(int helperNum, int tsNum, TS ts)
	{
		__helpers[helperNum][tsNum] = ts;
	}

	public virtual void setHelperTSField(int helperNum, int field)
	{
		__helperFields[helperNum] = field;
	}

	/// <summary>
	/// Sets the JCheckBox this object will use. </summary>
	/// <param name="checkBox"> the checkBox this object will use. </param>
	public virtual void setJCheckBox(JCheckBox checkBox)
	{
		__checkBox = checkBox;
	}

	/// <summary>
	/// Sets the JRadioButton this object will use. </summary>
	/// <param name="radioButton"> the radiobutton this object will use. </param>
	public virtual void setJRadioButton(JRadioButton radioButton)
	{
		__radioButton = radioButton;
	}

	/// <summary>
	/// Sets the textfield that is used by the GUI to set a name for the layer this
	/// data appears in. </summary>
	/// <param name="textField"> the textfield that users can enter a name for this layer in. </param>
	protected internal virtual void setLayerNameTextField(JTextField textField)
	{
		__layerNameTextField = textField;
	}

	/// <summary>
	/// Sets the number of helper time series to be used.  The number is the number
	/// of fields in the attribute table that will be filled by helper data. </summary>
	/// <param name="num"> the number of helper time series to be used. </param>
	public virtual void setNumHelperTS(int num)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: __helpers = new TS[num][__tsArray.Length];
		__helpers = RectangularArrays.RectangularTSArray(num, __tsArray.Length);
		__helperFields = new int[num];
	}

	/// <summary>
	/// Sets whether the data object should be visible during the animation.  This 
	/// method cannot be used to turn animation data on and off on a map layer -- it
	/// is only used by the GeoViewAnimationJFrame in setting up all the data for 
	/// animation once the user presses 'Accept'. </summary>
	/// <param name="visible"> whether the data are visible or not. </param>
	protected internal virtual void setVisible(bool visible)
	{
		__visible = visible;
	}

	}

}