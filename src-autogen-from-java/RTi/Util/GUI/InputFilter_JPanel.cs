using System;
using System.Collections.Generic;

// InputFilter_JPanel - class to display and manage a Vector of InputFilter

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
// InputFilter_JPanel - class to display and manage a Vector of InputFilter
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2003-12-18	Steven A. Malers, RTi	Initial version, for use with software
//					that queries databases.
// 2004-05-19	SAM, RTi		Fix bug where a change in the operator
//					JComboBox was not causing the entry
//					component to show the proper component.
//					Actually - after review it seems OK.
// 2004-08-26	SAM, RTi		* Add toString() to return a string
//					  representation of the filter.
//					* Add addEventListener() to allow
//					  higher level code when to refresh
//					  based on a change in the filter panel.
// 2004-08-28	SAM, RTi		To facilitate this class being a base
//					class, modify setInputFilters() to
//					take the list of filters and PropList -
//					then it can be called separately.
// 2004-09-10	SAM, RTi		* Add getInput() method to return the
//					  input that has been entered for a
//					  requested item.
// 2005-01-06	J. Thomas Sapienza, RTi	Fixed bugs that were making it so that
//					single filters could not be displayed in
//					the panel.
// 2004-01-11	JTS, RTi		Added the property 
//					'NumWhereRowsToDisplay'.  If set, it 
//					specifies the number of rows to be 
//					displayed when the drop-down list is
//					opened on one of the combo boxes that
// 					lists all the fields that a Where clause
//					can operated on.
// 2005-01-12	JTS, RTi		Changed slightly the formatting of 
//					errors in checkInput() to be cleaner.
// 2005-01-31	JTS, RTi		* fillOperatorJComboBox() now takes 
//					  another parameter that specifies the
//					  constraints that should NOT appear in
//					  the combo box.
//					* When the input text field is created
//					  its editability is set according to
//					  the value stored in the filter in:
//					  isInputJTextFieldEditable().
// 2005-02-02	JTS, RTi		When the input JComboBoxes are made
//					they now have a parameter to set the 
//					number of visible rows.
// 2005-04-26	JTS, RTi		Added finalize().
// 2006-04-25	SAM, RTi		Minor change to better handle input
//					filter string at initialization - was
//					having problem with ";Matches;".
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.GUI
{


	using IOUtil = RTi.Util.IO.IOUtil;
	//import RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	// TODO SAM 2004-05-20 - see limitation below.
	/// <summary>
	/// This class provides a JPanel to display a list of InputFilter.  The
	/// constructor should indicate the number of input filter groups to be displayed,
	/// where an input filter group lists all input filters.  For example, if three
	/// input filters are specified (in the constructor or later with setInputFilters),
	/// and NumFilterGroups=2 in the constructor properties, then two rows of input will
	/// be shown, each with the "where" listing each input filter.  The
	/// InputFilter_JPanel internally maintains components to properly display the
	/// correct visible components based on user selections.
	/// There is a limitation in that if an input filter has a list of string choices
	/// and an operator like "Ends with" is chosen, the full list of string choices is
	/// still displayed in the input.  Therefore, a substring cannot be operated on.  This
	/// limitation needs to be removed in later versions of the software if it impacts functionality.
	/// In addition to the fully functional input filter, a constructor is provided for a blank panel, and
	/// a constructor is provided to display text in the panel.  These variations can be used as place holders
	/// or to provide information to users.  Often the panels are added in the same layout position and made active
	/// by setting visible under appropriate conditions.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class InputFilter_JPanel extends javax.swing.JPanel implements java.awt.event.ItemListener
	public class InputFilter_JPanel : JPanel, ItemListener
	{

	/// <summary>
	/// Number of filter groups to display.  Each filter group will list all filters.
	/// </summary>
	private int __numFilterGroups = 1;

	/// <summary>
	/// Number of where choices to display in combobox choices. 
	/// </summary>
	private int __numWhereChoicesToDisplay = -1;

	/// <summary>
	/// List of InputFilter to display.  The original input filter that is supplied is copied for as many input
	/// filter groups as necessary, with the InputFilter instances managing the input field components for the
	/// specific filter.
	/// </summary>
	private IList<InputFilter>[] __inputFilterListArray = null;

	/// <summary>
	/// List of JComponent that display the "where" label for each filter group that is displayed.  This may be
	/// a SimpleJComboBox or a JLabel, depending on how many filters are available.  Each item in the list
	/// corresponds to a different input filter group.
	/// </summary>
	private IList<JComponent> __whereComponentList = new List<JComponent>();

	/// <summary>
	/// List of the operator components between the where an input components, one SimpleJComboBox per input filter.
	/// Each input filter group has a list of operators and the operators are reset as needed in the filter group.
	/// </summary>
	private IList<JComponent> __operatorComponentList = null;

	/// <summary>
	/// Text area to display text (if the text version of constructor is used).
	/// </summary>
	private JTextArea __textArea = null;

	/// <summary>
	/// Construct an input filter panel.  The setInputFilters() method must be called
	/// at some point during initialization in the calling code.  Or, use an empty input filter
	/// for cases where the filters do not apply (see also overloaded version with text).
	/// </summary>
	public InputFilter_JPanel()
	{
		GridBagLayout gbl = new GridBagLayout();
		setLayout(gbl);
	}

	/// <summary>
	/// Construct an input filter panel that will include a text area for a message.
	/// This version is used as a place holder with a message, with visibility being swapped with
	/// standard (or empty input filter panel).
	/// Use setText() to update the text that is displayed. </summary>
	/// <param name="text"> text to display in the input filter panel.  Use \n to indicate newline characters. </param>
	public InputFilter_JPanel(string text)
	{
		GridBagLayout gbl = new GridBagLayout();
		setLayout(gbl);
		__textArea = new JTextArea(text);
		Insets insetsNNNN = new Insets(0,0,0,0);
		JGUIUtil.addComponent(this, __textArea, 0, 0, 1, 1, 0.0, 0.0, insetsNNNN, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		// Set the number of filter groups to zero since components are not initialized
		__numFilterGroups = 0;
	}

	/// <summary>
	/// Construct an input filter panel from data choices. </summary>
	/// <param name="inputFilters"> A list of InputFilter, to be displayed. </param>
	/// <param name="numInputFilters"> the number of input filter rows to be displayed in the panel </param>
	/// <param name="numWhereChoicesToDisplay"> the number of where choices to display in lists for each filter
	/// (-1 to default to list size or an intelligent default) </param>
	public InputFilter_JPanel(IList<InputFilter> inputFilters, int numInputFilters, int numWhereChoicesToDisplay)
	{
		GridBagLayout gbl = new GridBagLayout();
		setLayout(gbl);
		setInputFilters(inputFilters, numInputFilters, numWhereChoicesToDisplay);
	}

	/// <summary>
	/// Add listeners for events generated in the input components in the input
	/// filter.  The code must have implemented ItemListener and KeyListener. </summary>
	/// <param name="component"> The component that wants to listen for events from InputFilter_JPanel components. </param>
	public virtual void addEventListeners(Component component)
	{ // Component is used above (instead of JComponent) because JDialog is not derived from JComponent.
		// Loop through the filter groups...
		bool isKeyListener = false;
		bool isItemListener = false;
		if (component is KeyListener)
		{
			isKeyListener = true;
		}
		if (component is ItemListener)
		{
			isItemListener = true;
		}
		SimpleJComboBox cb;
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			// The where...
			if (isItemListener)
			{
				if (__whereComponentList[ifg] is SimpleJComboBox)
				{
					cb = (SimpleJComboBox)__whereComponentList[ifg];
					cb.addItemListener((ItemListener)component);
				}
				// The operator...
				cb = (SimpleJComboBox)__operatorComponentList[ifg];
				cb.addItemListener((ItemListener)component);
			}
			// The input...
			int numFilters = 0;
			if (__inputFilterListArray[ifg] != null)
			{
				numFilters = __inputFilterListArray[ifg].Count;
			}
			InputFilter filter = null;
			JComponent input_component;
			JTextField tf;
			for (int ifilter = 0; ifilter < numFilters; ifilter++)
			{
				filter = __inputFilterListArray[ifg][ifilter];
				input_component = filter.getInputComponent();
				if (input_component is JTextField)
				{
					if (isKeyListener)
					{
						tf = (JTextField)input_component;
						tf.addKeyListener((KeyListener)component);
					}
				}
				else if (isItemListener)
				{
					// Combo box...
					cb = (SimpleJComboBox)input_component;
					cb.addItemListener((ItemListener)component);
				}
			}
		}
	}

	/// <summary>
	/// Check the input for the current input filter selections.  For example, if an
	/// input filter is for a text field, verify that the contents of the field are
	/// appropriate for the type of input.
	/// This version is similar to checkInput except that it returns the warning string
	/// so that it can be handled in calling code. </summary>
	/// <param name="displayWarning"> If true, display a warning dialog if there are errors in the input.
	/// If false, do not display a warning, in which case
	/// the calling code should generally display a warning and optionally
	/// also perform other checks by overriding this method. </param>
	/// <returns> empty string if no errors occur or the warning string if there are errors,
	/// with newline at front and newlines delimiting each warning. </returns>
	public virtual string checkInputFilters(bool displayWarning)
	{
		string warning = "";
		// Loop through the filter groups...
		InputFilter filter;
		string input; // Input string selected by user.
		string where; // Where label for filter selected by user.
		int inputType; // Input type for the filter.
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			filter = getInputFilter(ifg);
			where = filter.getWhereLabel();
			if (where.Equals(""))
			{
				// Blank where indicates that filter is disabled...
				continue;
			}
			input = filter.getInput(false).Trim();
			if (filter.getChoiceTokenType() > 0)
			{
				inputType = filter.getChoiceTokenType();
			}
			else
			{
				inputType = filter.getInputType();
			}
			if (inputType == StringUtil.TYPE_STRING)
			{
				// Any limitations?  For now assume not.
			}
			else if ((inputType == StringUtil.TYPE_DOUBLE) || (inputType == StringUtil.TYPE_FLOAT))
			{
				if (!StringUtil.isDouble(input))
				{
					warning += "\nInput filter \"" + filter.getWhereLabel() +
					"\", input is not a number:  \"" + input + "\"" + "\n";
				}
			}
			else if (inputType == StringUtil.TYPE_INTEGER)
			{
				if (!StringUtil.isInteger(input))
				{
					warning += "\nInput filter \"" + filter.getWhereLabel() +
					"\", input is not an integer:  \"" + input + "\"" + "\n";
				}
			}
		}
		if (warning.Length > 0 && displayWarning)
		{
			// Requested to display the warnings
			Message.printWarning(1, "InputFilter_JPanel.checkInputFilters", warning);
		}
		// Always return the warning string
		return warning;
	}

	/// <summary>
	/// Check the input for the current input filter selections.  For example, if an
	/// input filter is for a text field, verify that the contents of the field are
	/// appropriate for the type of input.  This is the legacy version.
	/// See also checkInputFilters(), which returns a string with warnings. </summary>
	/// <param name="displayWarning"> If true, display a warning if there are errors in the
	/// input.  If false, do not display a warning (the calling code should generally display a warning). </param>
	/// <returns> true if no errors occur or false if there are input errors. </returns>
	public virtual bool checkInput(bool displayWarning)
	{
		string warning = "\n";
		// Loop through the filter groups...
		InputFilter filter;
		string input; // Input string selected by user.
		string where; // Where label for filter selected by user.
		int inputType; // Input type for the filter.
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			filter = getInputFilter(ifg);
			where = filter.getWhereLabel();
			if (where.Equals(""))
			{
				// Blank where indicates that filter is disabled...
				continue;
			}
			input = filter.getInput(false).Trim();
			if (filter.getChoiceTokenType() > 0)
			{
				inputType = filter.getChoiceTokenType();
			}
			else
			{
				inputType = filter.getInputType();
			}
			if (inputType == StringUtil.TYPE_STRING)
			{
				// Any limitations?  For now assume not.
			}
			else if ((inputType == StringUtil.TYPE_DOUBLE) || (inputType == StringUtil.TYPE_FLOAT))
			{
				if (!StringUtil.isDouble(input))
				{
					warning += "Input filter \"" + filter.getWhereLabel() +
					"\", input is not a number:  \"" + input + "\"" + "\n";
				}
			}
			else if (inputType == StringUtil.TYPE_INTEGER)
			{
				if (!StringUtil.isInteger(input))
				{
					warning += "Input filter \"" + filter.getWhereLabel() +
					"\", input is not an integer:  \"" + input + "\"" + "\n";
				}
			}
		}
		if (warning.Length > 1)
		{
			if (displayWarning)
			{
				Message.printWarning(1, "InputFilter_JPanel.checkInput", warning);
			}
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Clears all the selections the user has made to the combo boxes in the panel.
	/// </summary>
	public virtual void clearInput()
	{
		SimpleJComboBox cb = null;
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			if (__whereComponentList[ifg] is SimpleJComboBox)
			{
				cb = (SimpleJComboBox)__whereComponentList[ifg];
				cb.select(0);
			}
		}
	}

	/// <summary>
	/// Fill a SimpleJComboBox with the appropriate operators for the data type.
	/// The previous contents are first removed. </summary>
	/// <param name="cb"> the SimpleJComboBox to fill. </param>
	/// <param name="type"> See StringUtil.TYPE_*. </param>
	/// <param name="constraintsToRemove"> the constraints that should NOT appear in the 
	/// combo box.  Can be null, if all the appropriate constraints should be shown. </param>
	private void fillOperatorJComboBox(SimpleJComboBox cb, int type, IList<string> constraintsToRemove)
	{
		if (cb == null)
		{
			return;
		}
		// Remove existing...
		cb.removeAll();
		if (type == StringUtil.TYPE_STRING)
		{
			cb.add(InputFilter.INPUT_MATCHES);
			// TODO - add later
			//cb.add ( InputFilter.INPUT_ONE_OF );
			cb.add(InputFilter.INPUT_STARTS_WITH);
			cb.add(InputFilter.INPUT_ENDS_WITH);
			cb.add(InputFilter.INPUT_CONTAINS);
			// TODO SAM 2010-05-23 Evaluate automatically adding
			//cb.add ( InputFilter.INPUT_IS_EMPTY );
		}
		else if ((type == StringUtil.TYPE_DOUBLE) || (type == StringUtil.TYPE_FLOAT) || (type == StringUtil.TYPE_INTEGER))
		{
			cb.add(InputFilter.INPUT_EQUALS);
			// TODO - add later
			//cb.add ( InputFilter.INPUT_ONE_OF );
			//cb.add ( InputFilter.INPUT_BETWEEN );
			cb.add(InputFilter.INPUT_LESS_THAN);
			cb.add(InputFilter.INPUT_LESS_THAN_OR_EQUAL_TO);
			cb.add(InputFilter.INPUT_GREATER_THAN);
			cb.add(InputFilter.INPUT_GREATER_THAN_OR_EQUAL_TO);
		}

		// Remove any constraints that have been explicitly set to NOT appear in the combo box.
		if (constraintsToRemove != null)
		{
			int size = constraintsToRemove.Count;
			for (int i = 0; i < size; i++)
			{
				cb.remove(constraintsToRemove[i]);
			}
		}

		if (cb.getItemCount() > 0)
		{
			// Select the first one...
			cb.select(0);
		}

		// TODO - need to handle "NOT" and perhaps null
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~InputFilter_JPanel()
	{
		IOUtil.nullArray(__inputFilterListArray);
		__whereComponentList = null;
		__operatorComponentList = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the input that has been entered in the panel, for a requested parameter.
	/// If the requested whereLabel is not selected in any of the input filters, a zero length list will be returned. </summary>
	/// <returns> the input that has been entered in the panel, for a requested parameter, guaranteed to be non-null. </returns>
	/// <param name="whereLabel"> The label for the input filter, which is visible to the user for selections (specify this
	/// OR internalWhereLabel) </param>
	/// <param name="internalWhere"> the internal where label for the input filter, which is not visible to the user
	/// but is used internally (specify this whereLabel) </param>
	/// <param name="useWildcards"> Suitable only for string input (treated as false if numeric input).
	/// If true, the returned information will be returned using
	/// wildcards, suitable for "matches" calls (e.g., "*inputvalue*").  If false,
	/// the returned information will be returned in verbose format as per the
	/// toString() method (e.g., "contains;inputvalue"). </param>
	/// <param name="delim"> Delimiter character to use if use_wildcards=false.  See the toString() method.  If null, use ";". </param>
	public virtual IList<string> getInput(string whereLabel, string internalWhere, bool useWildcards, string delim)
	{
		IList<string> inputList = new List<string>();
		if (string.ReferenceEquals(delim, null))
		{
			delim = ";";
		}
		InputFilter filter;
		string input; // Input string selected by user.
		string where; // Where label for filter selected by user.
		string internalWhereString; // Where used internally by the filter
		int inputType; // Input type for the filter.
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			filter = getInputFilter(ifg);
			where = filter.getWhereLabel();
			internalWhereString = filter.getWhereInternal();
			if ((!string.ReferenceEquals(whereLabel, null)) && (whereLabel.Length > 0))
			{
				if (!where.Equals(whereLabel, StringComparison.OrdinalIgnoreCase) || where.Equals(""))
				{
					// No need to evaluate because not the requested input or input is blank...
					continue;
				}
			}
			else if ((!string.ReferenceEquals(internalWhere, null)) && (internalWhere.Length > 0))
			{
				  if (!internalWhereString.Equals(internalWhere, StringComparison.OrdinalIgnoreCase) || internalWhereString.Equals(""))
				  {
						// No need to evaluate because not the requested input or input is blank...
						continue;
				  }
			}
			input = filter.getInput(false).Trim();
			if (filter.getChoiceTokenType() > 0)
			{
				inputType = filter.getChoiceTokenType();
			}
			else
			{
				inputType = filter.getInputType();
			}
			if (inputType == StringUtil.TYPE_STRING)
			{
				if (useWildcards)
				{
					// Insert the wildcard character around the input, as appropriate
					if (getOperator(ifg).Equals(InputFilter.INPUT_MATCHES))
					{
						inputList.Add(input);
					}
					else if (getOperator(ifg).Equals(InputFilter.INPUT_CONTAINS))
					{
						inputList.Add("*" + input + "*");
					}
					else if (getOperator(ifg).Equals(InputFilter.INPUT_STARTS_WITH))
					{
						inputList.Add(input + "*");
					}
					else if (getOperator(ifg).Equals(InputFilter.INPUT_ENDS_WITH))
					{
						inputList.Add("*" + input);
					}
				}
				else
				{
					// Return the input with operator
					inputList.Add(getOperator(ifg) + delim + input);
				}
			}
			else if ((inputType == StringUtil.TYPE_DOUBLE) || (inputType == StringUtil.TYPE_FLOAT))
			{
				if (useWildcards)
				{
					inputList.Add(input);
				}
				else
				{
					inputList.Add(getOperator(ifg) + delim + input);
				}
			}
			else if (inputType == StringUtil.TYPE_INTEGER)
			{
				if (useWildcards)
				{
					inputList.Add(input);
				}
				else
				{
					inputList.Add(getOperator(ifg) + delim + input);
				}
			}
		}
		return inputList;
	}

	/// <summary>
	/// Return the current filter for a filter group. </summary>
	/// <returns> the current filter for a filter group. </returns>
	/// <param name="ifg"> Input filter group. </param>
	public virtual InputFilter getInputFilter(int ifg)
	{
		int filterPos = 0;
		if (__whereComponentList[ifg] is SimpleJComboBox)
		{
			// The where lists all the filter where labels so the current
			// filter is given by the position in the combo box...
			SimpleJComboBox cb = (SimpleJComboBox)__whereComponentList[ifg];
			filterPos = cb.getSelectedIndex();
		}
		else
		{
			// A simple JLabel so there is only one item in the filter...
			filterPos = 0;
		}
		return (InputFilter)__inputFilterListArray[ifg][filterPos];
	}

	/// <summary>
	/// Return the input filters that have been entered in the panel, for a requested parameter.
	/// If the requested where_label is not selected in any of the input filters, a zero length vector will be returned. </summary>
	/// <param name="whereLabel"> The visible label for the input filter. </param>
	/// <returns> the input filters that match the requested persistent where label. </returns>
	public virtual IList<InputFilter> getInputFilters(string whereLabel)
	{
		IList<InputFilter> inputFilterList = new List<InputFilter>();
		InputFilter filter;
		string where; // Where label for filter selected by user.
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			filter = getInputFilter(ifg);
			where = filter.getWhereLabel();
			if (where.Equals(whereLabel, StringComparison.OrdinalIgnoreCase) && !where.Equals(""))
			{
				// Requested input name matches so add the filter...
				inputFilterList.Add(filter);
			}
		}
		return inputFilterList;
	}

	/// <summary>
	/// Return the input filter for an input filter group, for a requested whereLabelPersistent.
	/// If the requested whereLabelPersistent does not match any input filters for the input filter group,
	/// null will be returned. </summary>
	/// <param name="ifg"> input filter group of interest. </param>
	/// <param name="whereLabelPersistent"> The persistent label for the input filter. </param>
	/// <returns> the input filter that matches the requested whereLabelPersistent. </returns>
	public virtual InputFilter getInputFilterForWhereLabelPersistent(int ifg, string whereLabelPersistent)
	{
		string where; // whereLabelPersistent for filter selected by user.
		// First get the list of input filter for the group 
		IList<InputFilter> inputFilters = (IList<InputFilter>)__inputFilterListArray[ifg];
		// Loop through the filters and match the whereLabelPersistent
		foreach (InputFilter inputFilter in inputFilters)
		{
			where = inputFilter.getWhereLabelPersistent();
			if (where.Equals(whereLabelPersistent, StringComparison.OrdinalIgnoreCase) && !where.Equals(""))
			{
				// Requested input name matches so return the filter
				return inputFilter;
			}
		}
		return null;
	}

	/// <summary>
	/// Get the input filter value corresponding to the "where" string.
	/// The InputFilter whereLabel and whereLabelPersistent are checked and the first matching filter value is returned.
	/// This is useful for checking whether filters have been specified in appropriate combination. </summary>
	/// <param name="where"> the where value to match as displayed or the internal persistent label </param>
	/// <param name="returnEmpty"> if true, it is OK to return empty values for a where label,
	/// or if false, only return values that are non-empty. </param>
	public virtual string getInputValue(string where, bool returnEmpty)
	{
		// Loop through the input filter groups and find a matching label
		for (int ifg = 0; ifg < __inputFilterListArray.Length; ifg++)
		{
			InputFilter inputFilter = getInputFilter(ifg);
			string input = inputFilter.getInput(false).Trim();
			// Check the displayed where label
			string whereFromFilter = inputFilter.getWhereLabel();
			// and the persistent where label
			string whereFromFilter2 = inputFilter.getWhereLabelPersistent();
			if (whereFromFilter.Equals(where, StringComparison.OrdinalIgnoreCase) || whereFromFilter2.Equals(where, StringComparison.OrdinalIgnoreCase))
			{
				if (input.Length > 0 || (input.Equals("") && returnEmpty))
				{
					return input;
				}
			}
		}
		// Fall through is no value
		return null;
	}

	/// <summary>
	/// Return the number of filter groups. </summary>
	/// <returns> the number of filter groups. </returns>
	public virtual int getNumFilterGroups()
	{
		return __numFilterGroups;
	}

	/// <summary>
	/// Return the operator for a filter group (one of InputFilter.INPUT_*). </summary>
	/// <returns> the operator for a filter group. </returns>
	/// <param name="ifg"> Filter group. </param>
	public virtual string getOperator(int ifg)
	{
		SimpleJComboBox cb = (SimpleJComboBox)__operatorComponentList[ifg];
		return cb.getSelected();
	}

	/// <summary>
	/// Return the operator for an input filter. </summary>
	/// <returns> the operator for an input filter. </returns>
	/// <param name="filter"> input filter. </param>
	public virtual string getOperator(InputFilter filter)
	{ // First figure out which input filter group the filter is in...
		int ifgFound = -1;
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{

		}
		// Now return the operator that is visible for the filter
		SimpleJComboBox cb = (SimpleJComboBox)__operatorComponentList[ifgFound];
		return cb.getSelected();
	}

	/// <summary>
	/// Return the text that is displayed in the panel, when the text constructor is used. </summary>
	/// <returns> the text displayed in the panel text area. </returns>
	public virtual string getText()
	{
		if (__textArea == null)
		{
			return "";
		}
		else
		{
			return __textArea.getText();
		}
	}

	/// <summary>
	/// Handle item events for JComboBox selections.
	/// </summary>
	public virtual void itemStateChanged(ItemEvent @event)
	{
		object o = @event.getItemSelectable();

		if (@event.getStateChange() != ItemEvent.SELECTED)
		{
			// No reason to process the event...
			return;
		}

		InputFilter filter;
		// Loop through the filter groups, checking the where component to find the match...
		SimpleJComboBox where_JComboBox, operator_JComboBox;
		int filterPos = 0; // Position of the selected filter in the group.
		int operatorPos = 0; // Position of the selected operator in the filter group.

		JComponent component = null;

		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			component = __whereComponentList[ifg];
			if (component is SimpleJComboBox)
			{
				where_JComboBox = (SimpleJComboBox)component;
			}
			else
			{
				where_JComboBox = null;
			}

			operator_JComboBox = (SimpleJComboBox)__operatorComponentList[ifg];
			if (where_JComboBox != null && o == where_JComboBox)
			{
				// Found the component, which indicates which filter group was changed.  Update the operator list.
				// Note that if the original __whereComponentList item was a JLabel, we would not even be here because
				// no ItemEvent would have been generated.
				//Message.printStatus ( 2, "", "SAMX Found where component: " + ifg + " resetting operators..." );
				// Figure out which filter is selected for the filter group.  Because all groups have the same list of
				// filters, the absolute position will be the same in all the lists.
				filterPos = where_JComboBox.getSelectedIndex();
				filter = __inputFilterListArray[ifg][filterPos];
				//Message.printStatus ( 2, "", "Where changed." );
				if (filter.getChoiceTokenType() > 0)
				{
					// The input type is a string that has its token parsed out
					fillOperatorJComboBox((SimpleJComboBox)__operatorComponentList[ifg], filter.getChoiceTokenType(), filter.getConstraintsToRemove());
				}
				else
				{
					// The input type is a basic value, not a string that gets a token split out
					fillOperatorJComboBox((SimpleJComboBox)__operatorComponentList[ifg], filter.getInputType(), filter.getConstraintsToRemove());
				}
				// Set the appropriate component visible and all others not visible.
				// There is an input component for each filter for this filter group...
				operatorPos = 0;
				showInputFilterComponent(ifg, filterPos, operatorPos);
				// No need to keep searching components...
				break;
			}
			else if (o == operator_JComboBox)
			{
				// Set the appropriate component visible and all others not visible.
				// There is an input component for each filter for this filter group...
				// filterPos = getInputFilter();
				// Test...
				//Message.printStatus ( 2, "", "Operator changed." );
				// Figure out which operator...
				if (where_JComboBox == null)
				{
					showInputFilterComponent(ifg, 0, operator_JComboBox.getSelectedIndex());
				}
				else
				{
					showInputFilterComponent(ifg, where_JComboBox.getSelectedIndex(), operator_JComboBox.getSelectedIndex());
				}
				// No need to keep searching components...
				break;
			}
		}
	}

	/// <summary>
	/// Remove input filters and related components from the panel.
	/// </summary>
	private void removeInputFilters()
	{
		int size = 0;
		if (__inputFilterListArray != null)
		{
			size = __inputFilterListArray[0].Count;
		}
		InputFilter filter;
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			// Each group contains the same filter information, but using distinct components...
			for (int ifilter = 0; ifilter < size; ifilter++)
			{
				filter = __inputFilterListArray[ifg][ifilter];
				// Remove the input components for the filter...
				filter.setInputComponent(null);
			}
		}
		// Remove all the components from this JPanel...
		removeAll();
		__inputFilterListArray = null;
		__whereComponentList = null;
		__operatorComponentList = null;
	}

	/// <summary>
	/// Set the contents of an input filter. </summary>
	/// <param name="ifg"> The Filter group to be set (0+). </param>
	/// <param name="inputFilterString"> The where clause as a string, using visible information in the input filters:
	/// <pre>
	///   WhereValue;Operator;InputValue
	/// </pre>
	/// the operator is a string like "=".  Legacy "Equals" is updated to new conventions at construction.
	/// The input string is trimmed before attempting to set. </param>
	/// <param name="delim"> The delimiter used for the above information, or a semicolon if null. </param>
	/// <exception cref="Exception"> if there is an error setting the filter data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setInputFilter(int ifg, String inputFilterString, String delim) throws Exception
	public virtual void setInputFilter(int ifg, string inputFilterString, string delim)
	{
		if (string.ReferenceEquals(delim, null))
		{
			delim = ";";
		}
		IList<string> v = StringUtil.breakStringList(inputFilterString, delim, 0);
		string where = v[0].Trim();
		string @operator = v[1].Trim();
		// Translate legacy operators to new convention
		// TODO SAM 2010-10-29 Evaluate whether to have a method for this but can
		// hopefully phase out legacy convention without too much effort
		if (@operator.Equals(InputFilter.INPUT_EQUALS_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			@operator = InputFilter.INPUT_EQUALS;
		}
		else if (@operator.Equals(InputFilter.INPUT_LESS_THAN_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			@operator = InputFilter.INPUT_LESS_THAN;
		}
		else if (@operator.Equals(InputFilter.INPUT_GREATER_THAN_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			@operator = InputFilter.INPUT_GREATER_THAN;
		}
		// Sometimes during initialization an ending token may not be provided
		// (e.g., ";Matches;") so handle below...
		string input = "";
		if (v.Count > 2)
		{
			input = v[2].Trim();
		}

		// Set the where...

		JComponent component = __whereComponentList[ifg];
		SimpleJComboBox cb = null;
		JTextField tf = null;
		JLabel label = null;
		if (component is SimpleJComboBox)
		{
			cb = (SimpleJComboBox)component;
			// The Where could match the displayed label or an internal label that is more convenient to save,
			// such as in a command string.  At this point don't know which so try multiple.
			// First try exact match of the displayed where clause
			bool selectOk = false;
			try
			{
				JGUIUtil.selectIgnoreCase(cb, where);
				selectOk = true;
			}
			catch (Exception)
			{
				// Handle through selectOk
			}
			if (!selectOk)
			{
				// Look up the choice assuming where is "whereLabelPersistent"
				// First go through the input filters in the group and search for a whereLabelPersistent that matches the given where
				InputFilter inputFilter2 = getInputFilterForWhereLabelPersistent(ifg,where);
				string visibleWhereLabelToUse = null;
				if (inputFilter2 != null)
				{
					visibleWhereLabelToUse = inputFilter2.getWhereLabel();
					try
					{
						JGUIUtil.selectIgnoreCase(cb, visibleWhereLabelToUse);
						selectOk = true;
					}
					catch (Exception)
					{
						// Handle through selectOk
					}
				}
			}
			if (!selectOk)
			{
				// Throw an exception
				throw new Exception("Invalid choice \"" + where + "\" - did not match available choices or persistent label value.");
			}
		}
		else if (component is JLabel)
		{
			label = (JLabel)component;
			label.setText(where);
		}

		// Set the operator...

		cb = (SimpleJComboBox)__operatorComponentList[ifg];
		JGUIUtil.selectIgnoreCase(cb, @operator);

		// Set the input...

		InputFilter selectedInputFilter = getInputFilter(ifg);
		component = selectedInputFilter.getInputComponent();
		if (component is SimpleJComboBox)
		{
			cb = (SimpleJComboBox)component;
			JGUIUtil.selectTokenMatches(cb, true, selectedInputFilter.getChoiceDelimiter(), 0, selectedInputFilter.getChoiceToken(), input, null, true);
		}
		else if (component is JTextField)
		{
			tf = (JTextField)component;
			tf.setText(input);
		}
	}

	/// <summary>
	/// Set the input filters.  The previous contents of the panel are removed and new
	/// contents are created based on the parameters. </summary>
	/// <param name="inputFilters"> A list of InputFilter, containing valid data.  The input
	/// component will be set in each input filter as the GUI is defined. </param>
	/// <param name="numFilterGroups"> how many filter groups should be displayed.  
	/// Each group will include all the filters supplied at construction or with a setInputFilters() call.
	/// This will be reset to zero if no data are available. </param>
	/// <param name="numWhereChoicesToDisplay"> the number of rows to be displayed in the drop-down list of
	/// one of the combo boxes that lists the fields for a filter.  If negative, display the number of items in
	/// the list.  A list longer than that specified will be scrolled. </param>
	public virtual void setInputFilters(IList<InputFilter> inputFilters, int numFilterGroups, int numWhereChoicesToDisplay)
	{ // First remove the existing input filters (the event generators will also be removed so
		// listeners will no longer get the events)...
		removeInputFilters();
		// Duplicate the input filters for each filter group...
		int numFilters = inputFilters.Count; // Number of filters in a filter group
		if (inputFilters != null)
		{
			numFilters = inputFilters.Count;
		}
		if ((inputFilters == null) || (inputFilters.Count == 0))
		{
			// Only display if we actually have data...
			setNumFilterGroups(0);
		}
		else
		{
			setNumFilterGroups(numFilterGroups);
		}
		__inputFilterListArray = new System.Collections.IList[__numFilterGroups];
		InputFilter filter;
		for (int ifg = 0; ifg < __numFilterGroups; ifg++)
		{
			if (ifg == 0)
			{
				// Assign the original...
				__inputFilterListArray[0] = inputFilters;
			}
			else
			{
				// Copy the original...
				// TODO smalers 2018-09-04 is it necessary to clone the data arrays or can they be reused.
				// - maybe need a shallower clone?
				__inputFilterListArray[ifg] = new List<object>(numFilters);
				for (int ifilter = 0; ifilter < numFilters; ifilter++)
				{
					filter = inputFilters[ifilter];
					InputFilter newFilter = (InputFilter)filter.clone();
					__inputFilterListArray[ifg].Add(newFilter);
				}
			}
		}
		// Now add the new input filters...
		Insets insetsNNNN = new Insets(0,0,0,0);
		int x = 0, y = 0;
		int num = 0;
		// Layout is as follows...
		//
		//      0              1            2         3          4
		//   where_label   operators   ......SimpleJComboBox............
		//
		//				OR...
		//
		//                             JTextField    AND     JTextField
		//
		//				Depending on whether the choices are
		//				provided.
		//
		// where positions 2-4 are used as necessary based on the type of the input.
		__whereComponentList = new List<object>(__numFilterGroups);
		__operatorComponentList = new List<object>(__numFilterGroups);

		setNumWhereChoicesToDisplay(numWhereChoicesToDisplay);

		for (int ifg = 0; ifg < __numFilterGroups; ifg++, y++)
		{
			x = 0;
			if (numFilters == 1)
			{
				// Just use a label since the user cannot pick...
				filter = __inputFilterListArray[ifg][0];
				JLabel where_JLabel = new JLabel("Where " + filter.getWhereLabel() + ":");
					JGUIUtil.addComponent(this, where_JLabel, x++, y, 1, 1, 0.0, 0.0, insetsNNNN, GridBagConstraints.NONE, GridBagConstraints.EAST);
				__whereComponentList.Add(where_JLabel);
			}
			else
			{
				// Put the labels in a combo box so the user can pick...
				JGUIUtil.addComponent(this, new JLabel("Where:"), x++, y, 1, 1, 0.0, 0.0, insetsNNNN, GridBagConstraints.NONE, GridBagConstraints.EAST);
				SimpleJComboBox where_JComboBox = new SimpleJComboBox(false);
				IList<string> whereList = new List<string>(numFilters);
				for (int ifilter = 0; ifilter < numFilters; ifilter++)
				{
					filter = __inputFilterListArray[ifg][ifilter];
					whereList.Add(filter.getWhereLabel());
				}
				where_JComboBox.setData(whereList);
				where_JComboBox.addItemListener(this);

				if (numWhereChoicesToDisplay > -1)
				{
					where_JComboBox.setMaximumRowCount(numWhereChoicesToDisplay);
				}

				JGUIUtil.addComponent(this, where_JComboBox, x++, y, 1, 1, 0.0, 0.0, insetsNNNN, GridBagConstraints.NONE, GridBagConstraints.EAST);
				__whereComponentList.Add(where_JComboBox);
			}
			// The operators are reused in the filter group.  Initialize to the first filter...
			filter = __inputFilterListArray[ifg][0];
			// Initialize operators to the first filter.
			// This is reused because it is a simple list based on the current input type.
			SimpleJComboBox operator_JComboBox = new SimpleJComboBox(false);
			if (filter.getChoiceTokenType() > 0)
			{
				// The input type is a string that has its token parsed out
				fillOperatorJComboBox(operator_JComboBox, filter.getChoiceTokenType(), filter.getConstraintsToRemove());
			}
			else
			{
				// The input type is a basic value, not a string that gets a token split out
				fillOperatorJComboBox(operator_JComboBox, filter.getInputType(), filter.getConstraintsToRemove());
			}
			operator_JComboBox.addItemListener(this);
			__operatorComponentList.Add(operator_JComboBox);
			JGUIUtil.addComponent(this, operator_JComboBox, x++, y, 1, 1, 0.0, 0.0, insetsNNNN, GridBagConstraints.NONE, GridBagConstraints.WEST);
			// Now initialize the components used for input, one component per filter in the group...
			for (int ifilter = 0; ifilter < numFilters; ifilter++)
			{
				filter = __inputFilterListArray[ifg][ifilter];
				if (filter.getChoiceLabels() == null)
				{
					// No choices are provided so use a text field...
					num = filter.getInputJTextFieldWidth();
					JTextField input_JTextField = new JTextField(num);
					JGUIUtil.addComponent(this, input_JTextField, x, y, 1, 1, 0.0, 0.0, insetsNNNN, GridBagConstraints.NONE, GridBagConstraints.WEST);
					IList<MouseListener> listeners = filter.getInputComponentMouseListeners();
					if (listeners != null)
					{
						int lsize = listeners.Count;
						for (int l = 0; l < lsize; l++)
						{
							input_JTextField.addMouseListener(listeners[l]);
						}
					}
					// TODO - need to be distinct for each group, not shared...
					//Message.printStatus ( 2, "", "SAMX adding text field as input component ");
					input_JTextField.setEditable(filter.isInputJTextFieldEditable());
					filter.setInputComponent(input_JTextField);
					if (ifilter != 0)
					{
						input_JTextField.setVisible(false);
					}
				}
				else
				{
					// Add the choices...
					SimpleJComboBox input_JComboBox = new SimpleJComboBox(filter.getChoiceLabels(), filter.areChoicesEditable());
					num = filter.getNumberInputJComboBoxRows();
					if (num > 0)
					{
						input_JComboBox.setMaximumRowCount(num);
					}
					else if (num == InputFilter.JCOMBOBOX_ROWS_DISPLAY_DEFAULT)
					{
						// do nothing, display in the default way
					}
					else if (num == InputFilter.JCOMBOBOX_ROWS_DISPLAY_ALL)
					{
						num = input_JComboBox.getItemCount();
						input_JComboBox.setMaximumRowCount(num);
					}
					JGUIUtil.addComponent(this, input_JComboBox, x, y, 1, 1, 0.0, 0.0, insetsNNNN, GridBagConstraints.NONE, GridBagConstraints.WEST);
					// TODO - need to be distinct for each group, not shared...
					//Message.printStatus ( 2, "", "SAMX adding combo box as input component ");
					filter.setInputComponent(input_JComboBox);
					// Only set the first input component visible...
					if (ifilter != 0)
					{
						input_JComboBox.setVisible(false);
					}
					IList<MouseListener> listeners = filter.getInputComponentMouseListeners();
					if (listeners != null)
					{
						int lsize = listeners.Count;
						for (int l = 0; l < lsize; l++)
						{
							input_JComboBox.addMouseListener(listeners[l]);
						}
					}
				}
				// Always add a blank panel on the right side that allows expansion, to fill up the right
				// side of the panel.  Otherwise, each component is not resizable and the filter panel tends
				// to set centered with a lot of space on either side in container panels.
				JGUIUtil.addComponent(this, new JPanel(), (x + 1), y, 1, 1, 1.0, 0.0, insetsNNNN, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
			}
		}
	}

	/// <summary>
	/// Set the number of filter groups to display. </summary>
	/// <param name="numFilterGroups"> number of filter groups to display (vertical components). </param>
	public virtual void setNumFilterGroups(int numFilterGroups)
	{
		__numFilterGroups = numFilterGroups;
	}

	/// <summary>
	/// Set the number of where choices in a . </summary>
	/// <param name="numFilterGroups"> number of where choices to display. </param>
	public virtual void setNumWhereChoicesToDisplay(int numWhereChoicesToDisplay)
	{
		__numWhereChoicesToDisplay = numWhereChoicesToDisplay;
	}

	/// <summary>
	/// Set the text that is displayed in the panel, when the text constructor is used. </summary>
	/// <param name="text"> the text displayed in the panel text area. </param>
	public virtual void setText(string text)
	{
		if (__textArea != null)
		{
			__textArea.setText(text);
		}
	}

	/// <summary>
	/// Show the appropriate input filter component.  This is the component that the
	/// user will enter a value to check for.  It can be either a predefined list
	/// (JComboBox) or a text field, depending on how the InputFilter was initially
	/// defined and depending on the current operator that is selected. </summary>
	/// <param name="ifg"> the InputFilter group to be updated. </param>
	/// <param name="filterPos"> the input filter component that is currently selected (the where item). </param>
	/// <param name="operatorPos"> the operator item that is currently selected.  The input
	/// component that matches this criteria is set visible and all others are set to
	/// not visible.  In actuality, the input component is the same regardless of the
	/// operator component.  If the input are available as choices, then the operator
	/// will use these choices.  If the input is a text field, then the operator will
	/// require user text.  The only limitation is that for a string input type where
	/// the input choices have been supplied, the user will be limited to only available
	/// strings and will therefore not be able to do substrings. </param>
	private void showInputFilterComponent(int ifg, int filterPos, int operatorPos)
	{
		int nfilters = __inputFilterListArray[0].Count;
		InputFilter filter; // Input filter to check
		for (int ifilter = 0; ifilter < nfilters; ifilter++)
		{
			filter = __inputFilterListArray[ifg][ifilter];
			if (ifilter == filterPos)
			{
				// The input component for the selected filter needs to be visible...
				//Message.printStatus ( 2, "","SAMX enabling input component " + ifilter );
				filter.getInputComponent().setVisible(true);
			}
			else
			{
				// All other input components should not be visible...
				//Message.printStatus ( 2, "","SAMX disabling input component " + ifilter );
				filter.getInputComponent().setVisible(false);
			}
		}
	}

	/// <summary>
	/// Return a string representation of an input filter group.  This can be used, for
	/// example, with software that may place a filter in a command.
	/// The displayed label is used. </summary>
	/// <param name="ifg"> the Input filter group </param>
	/// <param name="delim"> Delimiter for the returned filter information.  If null, use ";". </param>
	public virtual string ToString(int ifg, string delim)
	{
		return ToString(ifg, delim, 0);
	}

	/// <summary>
	/// Return a string representation of an input filter group.  This can be used, for
	/// example, with software that may place a filter in a command. </summary>
	/// <param name="ifg"> the Input filter group </param>
	/// <param name="delim"> Delimiter for the returned filter information.  If null, use ";". </param>
	/// <param name="valuePos"> if <= 0, return the where label; if 1, return the internal value; if 2, return the alternate internal value;
	/// if 3 return the persistent label </param>
	public virtual string ToString(int ifg, string delim, int valuePos)
	{
		InputFilter filter = getInputFilter(ifg);
		if (string.ReferenceEquals(delim, null))
		{
			delim = ";";
		}
		if (valuePos == 1)
		{
			// Internal where
			return filter.getWhereInternal() + delim + getOperator(ifg) + delim + filter.getInput(false);
		}
		else if (valuePos == 2)
		{
			// Second internal where
			return filter.getWhereInternal2() + delim + getOperator(ifg) + delim + filter.getInput(false);
		}
		else if (valuePos == 3)
		{
			// Persistent label, intended to represent the where criteria in persistent form such as commands
			return filter.getWhereLabelPersistent() + delim + getOperator(ifg) + delim + filter.getInput(false);
		}
		else
		{
			// Default behavior for (valuePos < 1) || (valuePos > 3) )
			return filter.getWhereLabel() + delim + getOperator(ifg) + delim + filter.getInput(false);
		}
	}

	}

}