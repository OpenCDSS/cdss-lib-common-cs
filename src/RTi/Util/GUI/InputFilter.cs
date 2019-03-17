using System;
using System.Collections.Generic;

// InputFilter - class to handle an input filter "Where...Is"

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
// InputFilter - class to handle an input filter "Where...Is"
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2003-12-18	Steven A. Malers, RTi	Initial version, for use with software
//					that queries databases.
// 2004-02-01	SAM, RTi		* Add clone().
//					* Add delimiter information for choices
//					  to allow generic handling in
//					  getInput().
// 2004-01-06	SAM, RTi		* Add matches() to simplify comparison
//					  of input.
// 2004-10-26	SAM, RTi		* Overload matches() to operate on
//					  integer and double types and
//					  transparently handle different input
//					  types.
// 2005-01-31	J. Thomas Sapienza, RTi	* Added __where_internal_2.
//					* Added addInputComponentMouseListener()
//					* Added getConstraintsToRemove().
//					* Added isInputJTextFieldEditable().
//					* Added removeConstraint().
//					* Added setInputComponentToolTipText().
//					* Added setInputJTextFieldEditable().
// 2005-02-02	JTS, RTi		* Added capability to set the number
//					  of rows in an input JComboBox to
//					  display at once.
//					* Added capability to set the width
//					  of the input JTextFields.
// 2005-04-05	JTS, RTi		Modified getInputInternal() so that if
//					a user enters a value in a combo box
//					entry field, that value can be returned.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.GUI
{



	using StringUtil = RTi.Util.String.StringUtil;

	// TODO SAM 2011-01-09 Need to add validator to each filter, via an interface.
	// * Can color code entry field
	// * When does validation occur so as to not be irritating
	// * Is it passive with tooltip or active with popup?

	// TODO SAM 2011-01-09 Need a way to limit the criteria where all choices do not work, for example, if
	// "Matches" is the only string choice.

	/// <summary>
	/// This class provides a way to define an input filter, for example for use in a
	/// GUI as a "[field] [constraint] [input] choice.  [field] is the data field 
	/// that will be constrained.  [constraint] is the way the data field will be 
	/// matched against (see the INPUT_* data members).  [input] is the value to 
	/// constrain the field against.<para>
	/// 
	/// In most cases, a list of these objects is created as appropriate by 
	/// specific software and is then managed by a InputFilter_JPanel object,
	/// typically a class derived from InputFilter_JPanel.
	/// </para>
	/// </summary>
	public class InputFilter : ICloneable
	{

	// TODO SAM 2010-09-21 Need to convert to enumeration
	/// <summary>
	/// Input filter type, for use with strings that exactly match a pattern.
	/// </summary>
	public const string INPUT_MATCHES = "Matches";

	/// <summary>
	/// Input filter type, for use with strings and numbers that match a case in a list.
	/// </summary>
	public const string INPUT_ONE_OF = "One of";

	/// <summary>
	/// Input filter type, for use with strings starting with a pattern.
	/// </summary>
	public const string INPUT_STARTS_WITH = "Starts with";

	/// <summary>
	/// Input filter type, for use with strings ending with a pattern.
	/// </summary>
	public const string INPUT_ENDS_WITH = "Ends with";

	/// <summary>
	/// Input filter type, for use with strings containing a pattern.
	/// </summary>
	public const string INPUT_CONTAINS = "Contains";

	/// <summary>
	/// Input filter type, for use with numbers that exactly match.
	/// </summary>
	public const string INPUT_EQUALS = "=";
	/// <summary>
	/// Legacy INPUT_EQUALS, phasing out.
	/// </summary>
	public const string INPUT_EQUALS_LEGACY = "Equals";

	/// <summary>
	/// Input filter type, for use with numbers that are between two values.
	/// </summary>
	public const string INPUT_BETWEEN = "Is between";

	/// <summary>
	/// Input filter type, for use with strings that are null or empty.
	/// </summary>
	public const string INPUT_IS_EMPTY = "Is empty";

	/// <summary>
	/// Input filter type, for use with numbers that are less than a value.
	/// </summary>
	public const string INPUT_LESS_THAN = "<";
	/// <summary>
	/// Legacy INPUT_LESS_THAN, phasing out.
	/// </summary>
	public const string INPUT_LESS_THAN_LEGACY = "Less than";

	/// <summary>
	/// Input filter type, for use with numbers that are less than or equal to a value.
	/// </summary>
	public const string INPUT_LESS_THAN_OR_EQUAL_TO = "<=";

	/// <summary>
	/// Input filter type, for use with numbers that are greater than a value.
	/// </summary>
	public const string INPUT_GREATER_THAN = ">";
	/// <summary>
	/// Legacy INPUT_GREATER_THAN, phasing out.
	/// </summary>
	public const string INPUT_GREATER_THAN_LEGACY = "Greater than";

	/// <summary>
	/// Input filter type, for use with numbers that are greater than or equal to a value.
	/// </summary>
	public const string INPUT_GREATER_THAN_OR_EQUAL_TO = ">=";

	/// <summary>
	/// Specifies for an input JComboBox to display all of its rows at once.
	/// </summary>
	public const int JCOMBOBOX_ROWS_DISPLAY_ALL = -1;

	/// <summary>
	/// Specifies for an input JComboBox to display the default number of rows.
	/// </summary>
	public const int JCOMBOBOX_ROWS_DISPLAY_DEFAULT = 0;

	/// <summary>
	/// The string that is internally used in a where clause (e.g., a database table column: "table.column").
	/// </summary>
	private string __whereInternal = "";

	/// <summary>
	/// The string that is internally used in a where clause (e.g., a database table column: "table.column").
	/// This secondary where string can be used in case of times where in certain instances a different thing
	/// is queried against.  For instance, the first where string could be used for SQL queries and the second
	/// where string could be used when querying against fields in a database view. 
	/// </summary>
	private string __whereInternal2 = "";

	/// <summary>
	/// The string label to be visible to the user (e.g., "Station Name").
	/// </summary>
	private string __whereLabel = "";

	/// <summary>
	/// The string label used in persistent forms such as command parameters,
	/// often with minimal formatting (e.g., "StationName").
	/// </summary>
	private string __whereLabelPersistent = "";

	/// <summary>
	/// A value from RTi.Util.String.StringUtil.TYPE_*, indicating the expected input value.  The type is used
	/// to check the validity of the input with checkInput().
	/// </summary>
	private int __inputType = 0;

	/// <summary>
	/// A list of String choices to choose from.  If not null a JComboBox will be displayed to let the user choose from.
	/// </summary>
	private IList<string> __choiceLabelList = null;

	/// <summary>
	/// The internal values (e.g., database column values) corresponding to the visible choices.
	/// Always provide as strings but should ultimately match the column type.
	/// </summary>
	private IList<string> __choiceInternalList = null;

	/// <summary>
	/// Used when the choices are not simple strings but contain informational notes - indicates
	/// the delimiter between data and notes.
	/// </summary>
	private string __choiceDelimiter = null;

	/// <summary>
	/// Used when the choices are not simple strings but contain informational notes - indicates
	/// the token position (relative to 0) for the data in the choices.
	/// </summary>
	private int __choiceToken = 0;

	/// <summary>
	/// Used with __choiceToken to indicate the type of the token for comparison purposes.
	/// Default to unknown, meaning use the primary input type.
	/// </summary>
	private int __choiceTokenType = -1;

	/// <summary>
	/// The component used to enter input, typically assigned by external code like InputFilter_JPanel
	/// </summary>
	private JComponent __inputComponent = null;

	/// <summary>
	/// If true, the JComboBox used with __choicesList will be editable (usually choices are pre-defined
	/// and should not be editable, hence the default value of false).
	/// </summary>
	private bool __areChoicesEditable = false;

	/// <summary>
	/// If the input component is going to be a text field, this can be set to false to
	/// set the text field uneditable (this is only useful in very limited circumstances and is
	/// why the default is true).
	/// </summary>
	private bool __inputJTextFieldEditable = true;

	/// <summary>
	/// The width of the input JTextField (if used) on the GUI, roughly in characters.
	/// </summary>
	private int __inputJTextFieldWidth = 10;

	/// <summary>
	/// Specifies how many rows of values to show at one time in a GUI.  Possible values are
	/// JCOMBOBOX_ROWS_DISPLAY_ALL, JCOMBOBOX_ROWS_DISPLAY_DEFAULT, or a positive integer.
	/// </summary>
	private int __numInputJComboBoxRows = JCOMBOBOX_ROWS_DISPLAY_DEFAULT;

	/// <summary>
	/// If any tool tip text is defined for the InputFilter, it is stored here.  Otherwise, this value is null.
	/// </summary>
	private string __inputComponentToolTipText = null;

	/// <summary>
	/// The listeners that want to listen to mouse events on the input component.
	/// </summary>
	private IList<MouseListener> __inputComponentMouseListeners = null;

	/// <summary>
	/// If not null, contains all the constraints that should NOT appear in the constraint
	/// combo box for this InputFilter.
	/// </summary>
	private IList<string> __removedConstraints = null;

	/// <summary>
	/// Construct an input filter. </summary>
	/// <param name="whereLabel"> A string to be listed in a choice to tell the user what input parameter is being filtered.
	/// A blank string can be specified to indicate that the filter can be disabled. </param>
	/// <param name="whereInternal"> The internal value that can be used to perform a query.
	/// For example, set to a database table and column ("table.column").  This can be
	/// set to null or empty if not used by other software. </param>
	/// <param name="inputType"> The input filter data type, see RTi.Util.String.StringUtil.TYPE_*. </param>
	/// <param name="choiceLabels"> A list of String containing choice values to be
	/// displayed to the user.  If null, the user will not be shown a list of choices. </param>
	/// <param name="choicesInternal"> A list of String containing choice values (e.g., database column values). </param>
	/// <param name="areChoicesEditable"> If true, and a non-null list of choices is provided,
	/// the choices will also be editable (an editable JTextField part of the JComboBox will be shown). </param>
	public InputFilter(string whereLabel, string whereInternal, int inputType, IList<string> choiceLabels, IList<string> choicesInternal, bool areChoicesEditable) : this(whereLabel, whereInternal, "", "", inputType, choiceLabels, choicesInternal, areChoicesEditable, null)
	{
	}

	/// <summary>
	/// Construct an input filter. </summary>
	/// <param name="whereLabel"> A string to be listed in a choice to tell the user what input parameter is being filtered.
	/// A blank string can be specified to indicate that the filter can be disabled. </param>
	/// <param name="whereInternal"> The internal value that can be used to perform a query.
	/// For example, set to a database table and column ("table.column").  This can be
	/// set to null or empty if not used by other software. </param>
	/// <param name="whereInternal2"> the internal value that can be used if in certain 
	/// cases a different field must be used for performing a query.  For instance, if
	/// a database can be used to query with SQL or to query against a database view.
	/// It can be set to null or an empty string if it won't be used by software. </param>
	/// <param name="inputType"> The input filter data type, see RTi.Util.String.StringUtil.TYPE_*. </param>
	/// <param name="choiceLabels"> A list of String containing choice values to be
	/// displayed to the user.  If null, the user will not be shown a list of choices. </param>
	/// <param name="choicesInternal"> A list of String containing choice values (e.g., database column values).  Always
	/// provide strings even if the database column is integer, etc. </param>
	/// <param name="areChoicesEditable"> If true, and a non-null list of choices is provided,
	/// the choices will also be editable (an editable JTextField part of the JComboBox will be shown). </param>
	public InputFilter(string whereLabel, string whereInternal, string whereInternal2, int inputType, IList<string> choiceLabels, IList<string> choicesInternal, bool areChoicesEditable) : this(whereLabel, whereInternal, whereInternal2, "", inputType, choiceLabels, choicesInternal, areChoicesEditable, null)
	{
	}

	/// <summary>
	/// Construct an input filter. </summary>
	/// <param name="whereLabel"> A string to be listed in a choice to tell the user what input parameter is being filtered.
	/// A blank string can be specified to indicate that the filter can be disabled. </param>
	/// <param name="whereInternal"> The internal value that can be used to perform a query.
	/// For example, set to a database table and column ("table.column").  This can be
	/// set to null or empty if not used by other software. </param>
	/// <param name="whereInternal2"> the internal value that can be used if in certain 
	/// cases a different field must be used for performing a query.  For instance, if
	/// a database can be used to query with SQL or to query against a database view.
	/// It can be set to null or an empty string if it won't be used by software. </param>
	/// <param name="whereLabelPersistent"> the where label to use for persistent forms such
	/// as command parameter, useful when the displayed where label is too verbose
	/// or has punctuation that is prone to errors,
	/// and when the internal labels may be redundant or confusing to users. </param>
	/// <param name="inputType"> The input filter data type, see RTi.Util.String.StringUtil.TYPE_*. </param>
	/// <param name="choiceLabels"> A list of String containing choice values to be
	/// displayed to the user.  If null, the user will not be shown a list of choices. </param>
	/// <param name="choicesInternal"> A list of String containing choice values (e.g., database column values).  Always
	/// provide strings even if the database column is integer, etc. </param>
	/// <param name="areChoicesEditable"> If true, and a non-null list of choices is provided,
	/// the choices will also be editable (an editable JTextField part of the JComboBox will be shown). </param>
	public InputFilter(string whereLabel, string whereInternal, string whereInternal2, string whereLabelPersistent, int inputType, IList<string> choiceLabels, IList<string> choicesInternal, bool areChoicesEditable) : this(whereLabel, whereInternal, whereInternal2, whereLabelPersistent, inputType, choiceLabels, choicesInternal, areChoicesEditable, null)
	{
	}

	/// <summary>
	/// Construct an input filter. </summary>
	/// <param name="whereLabel"> A string to be listed in a choice to tell the user what input parameter is being filtered.
	/// A blank string can be specified to indicate that the filter can be disabled. </param>
	/// <param name="whereInternal"> The internal value that can be used to perform a query.
	/// For example, set to a database table and column ("table.column").  This can be
	/// set to null or empty if not used by other software. </param>
	/// <param name="whereInternal2"> the internal value that can be used if in certain 
	/// cases a different field must be used for performing a query.  For instance, if
	/// a database can be used to query with SQL or to query against a database view.
	/// It can be set to null or an empty string if it won't be used by software. </param>
	/// <param name="inputType"> The input filter data type, see RTi.Util.String.StringUtil.TYPE_*. </param>
	/// <param name="choiceLabels"> A list of String containing choice values to be
	/// displayed to the user.  If null, the user will not be shown a list of choices. </param>
	/// <param name="choicesInternal"> A list of String containing choice values (e.g., database column values). </param>
	/// <param name="areChoicesEditable"> If true, and a non-null list of choices is provided,
	/// the choices will also be editable (an editable JTextField part of the JComboBox will be shown). </param>
	/// <param name="inputComponentToolTipText"> tool tip text for the input component. </param>
	public InputFilter(string whereLabel, string whereInternal, string whereInternal2, int inputType, IList<string> choiceLabels, IList<string> choicesInternal, bool areChoicesEditable, string inputComponentToolTipText) : this(whereLabel, whereInternal, whereInternal2, "", inputType, choiceLabels, choicesInternal, areChoicesEditable, inputComponentToolTipText)
	{
	}

	/// <summary>
	/// Construct an input filter. </summary>
	/// <param name="whereLabel"> A string to be listed in a choice to tell the user what input parameter is being filtered.
	/// A blank string can be specified to indicate that the filter can be disabled. </param>
	/// <param name="whereInternal"> The internal value that can be used to perform a query.
	/// For example, set to a database table and column ("table.column").  This can be
	/// set to null or empty if not used by other software. </param>
	/// <param name="whereInternal2"> the internal value that can be used if in certain 
	/// cases a different field must be used for performing a query.  For instance, if
	/// a database can be used to query with SQL or to query against a database view.
	/// It can be set to null or an empty string if it won't be used by software. </param>
	/// <param name="whereLabelPersistent"> the where label to use for persistent forms such
	/// as command parameter, useful when the displayed where label is too verbose
	/// or has punctuation that is prone to errors,
	/// and when the internal labels may be redundant or confusing to users. </param>
	/// <param name="inputType"> The input filter data type, see RTi.Util.String.StringUtil.TYPE_*. </param>
	/// <param name="choiceLabels"> A list of String containing choice values to be
	/// displayed to the user.  If null, the user will not be shown a list of choices. </param>
	/// <param name="choicesInternal"> A list of String containing choice values (e.g., database column values). </param>
	/// <param name="areChoicesEditable"> If true, and a non-null list of choices is provided,
	/// the choices will also be editable (an editable JTextField part of the JComboBox will be shown). </param>
	/// <param name="inputComponentToolTipText"> tool tip text for the input component. </param>
	public InputFilter(string whereLabel, string whereInternal, string whereInternal2, string whereLabelPersistent, int inputType, IList<string> choiceLabels, IList<string> choicesInternal, bool areChoicesEditable, string inputComponentToolTipText)
	{
		__whereLabel = whereLabel;
		__whereInternal = whereInternal;
		__whereInternal2 = whereInternal2;
		__whereLabelPersistent = whereLabelPersistent;
		__inputType = inputType;
		__choiceLabelList = choiceLabels;
		__choiceInternalList = choicesInternal;
		__areChoicesEditable = areChoicesEditable;
		//__toolTipText = toolTipText;
		setInputComponentToolTipText(inputComponentToolTipText);
	}

	/// <summary>
	/// Adds a mouse listener to the input field.  This listener will be notified of
	/// any mouse events on the Input component. </summary>
	/// <param name="listener"> the listener to add to the input component. </param>
	public virtual void addInputComponentMouseListener(MouseListener listener)
	{
		if (__inputComponentMouseListeners == null)
		{
			__inputComponentMouseListeners = new List<MouseListener>();
		}
		__inputComponentMouseListeners.Add(listener);
	}

	/// <summary>
	/// Indicate whether the input choices are editable.  If true, the user should be
	/// able to type in a value in addition to using the choices.  This is only used
	/// when choices are provided.
	/// </summary>
	public virtual bool areChoicesEditable()
	{
		return __areChoicesEditable;
	}

	/// <summary>
	/// Clone the object.  The Object base class clone() method is called and then the
	/// data members are cloned.  The result is a complete deep copy.  The only
	/// exception is that the input component is set to null - it normally needs to be
	/// defined by calling code after the clone() call occurs.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			// Clone the base class...
			InputFilter filter = (InputFilter)base.clone();
			// Now clone the mutable objects...
			// The following clone automatically because they are primitives:
			//
			// __whereInternal
			// __whereLabel
			// __inputType
			// __areChoicesEditable
			// __choiceDelimiter
			// __choiceTtoken

			// Do not clone the __inputCcomponent because it will be set in calling code.

			filter.__inputComponent = null;

			// Copy the contents of the vectors...

			if (__choiceLabelList != null)
			{
				int size = __choiceLabelList.Count;
				filter.__choiceLabelList = new List<string>(size);
				for (int i = 0; i < size; i++)
				{
					filter.__choiceLabelList.Add(__choiceLabelList[i]);
				}
			}
			if (__choiceInternalList != null)
			{
				int size = __choiceInternalList.Count;
				filter.__choiceInternalList = new List<string>(size);
				for (int i = 0; i < size; i++)
				{
					filter.__choiceInternalList.Add(__choiceInternalList[i]);
				}
			}
			return filter;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is clonable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Evaluate whether a string object meets a string criterion. </summary>
	/// <param name="s"> string being evaluated (e.g., "does s criterion s2"?) </param>
	/// <param name="criteria"> criteria being used in comparison </param>
	/// <param name="s2"> string that is being used to match the criterion </param>
	public static bool evaluateCriterion(string s, InputFilterStringCriterionType criterion, string s2)
	{
		if (criterion == InputFilterStringCriterionType.CONTAINS)
		{
			int index = StringUtil.indexOfIgnoreCase(s, s2, 0);
			if (index >= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else if (criterion == InputFilterStringCriterionType.ENDS_WITH)
		{
			return StringUtil.endsWithIgnoreCase(s,s2);
		}
		else if (criterion == InputFilterStringCriterionType.MATCHES)
		{
			return s.Equals(s2, StringComparison.OrdinalIgnoreCase);
		}
		else if (criterion == InputFilterStringCriterionType.STARTS_WITH)
		{
			return StringUtil.startsWithIgnoreCase(s,s2);
		}
		return false;
	}

	/// <summary>
	/// Return the choices that are visible to the user. </summary>
	/// <returns> the choices that are visible to the user.  </returns>
	public virtual IList<string> getChoiceLabels()
	{
		return __choiceLabelList;
	}

	/// <summary>
	/// Return the delimiter used in choices, which is used to separate actual data from descriptive information. </summary>
	/// <returns> the delimiter used in choices. </returns>
	public virtual string getChoiceDelimiter()
	{
		return __choiceDelimiter;
	}

	/// <summary>
	/// Return the token position used in choices, which is the position of actual data, compared to descriptive information. </summary>
	/// <returns> the token position used in choices (0+). </returns>
	public virtual int getChoiceToken()
	{
		return __choiceToken;
	}

	/// <summary>
	/// Return the type of the choice token, which is used to evaluate the criteria.
	/// </summary>
	public virtual int getChoiceTokenType()
	{
		return __choiceTokenType;
	}

	/// <summary>
	/// Returns the constraints that should not appear in the constraint combo box for this InputFilter. </summary>
	/// <returns> the constraints that should not appear in the constraint combo box for this InputFilter. </returns>
	protected internal virtual IList<string> getConstraintsToRemove()
	{
		return __removedConstraints;
	}

	/// <summary>
	/// Return the user-supplied input, as entered in the input component.
	/// If the input component is a SimpleJComboBox, the internal value is returned if
	/// not blank and the input choice is not blank. </summary>
	/// <returns> the user-supplied input, as a string, or null if the input component is null. </returns>
	/// <param name="return_full"> If true, then full input is returned, which may include
	/// informational comments.  If false, only the specific data token is returned,
	/// requiring that setTokenInfo() be called for the filter. </param>
	public virtual string getInput(bool return_full)
	{
		if (__inputComponent == null)
		{
			return null;
		}
		else if (__inputComponent is SimpleJComboBox)
		{
			SimpleJComboBox cb = (SimpleJComboBox)__inputComponent;
			if (return_full || (string.ReferenceEquals(__choiceDelimiter, null)))
			{
				// Input choices are not formatted...
				return cb.getSelected();
			}
			else
			{
				// Return a token from the input choices.
				return StringUtil.getToken(cb.getSelected(), __choiceDelimiter, StringUtil.DELIM_SKIP_BLANKS, __choiceToken);
			}
		}
		else if (__inputComponent is JTextField)
		{
			return ((JTextField)__inputComponent).getText();
		}
		return null;
	}

	/// <summary>
	/// Return the input component for the data filter.  This is used by external code
	/// to manage GUI components used for input.  A distinct component should be
	/// available for each filter, usually a JComboBox or a JTextField. </summary>
	/// <returns> the input component. </returns>
	public virtual JComponent getInputComponent()
	{
		return __inputComponent;
	}

	/// <summary>
	/// Returns the list of mouse listeners for the input component.  May be null if none have been set. </summary>
	/// <returns> the list of mouse listeners for the input component. </returns>
	public virtual IList<MouseListener> getInputComponentMouseListeners()
	{
		return __inputComponentMouseListeners;
	}

	/// <summary>
	/// Returns the tool tip text assigned to the input component, if any. </summary>
	/// <returns> the tool tip text assigned to the input component, if any. </returns>
	protected internal virtual string getInputComponentToolTipText()
	{
		return __inputComponentToolTipText;
	}

	/// <summary>
	/// Return the input using internal notation.  If the input component is a
	/// SimpleJComboBox, the internal input is returned.  If the input component is a
	/// JTextField, the visible input is returned. </summary>
	/// <returns> the input using internal notation. </returns>
	public virtual string getInputInternal()
	{
		if (__inputComponent is SimpleJComboBox)
		{
			SimpleJComboBox cb = (SimpleJComboBox)__inputComponent;
			int pos = cb.getSelectedIndex();
			if (pos == -1)
			{
				return cb.getFieldText();
			}
			else
			{
				return __choiceInternalList[pos];
			}
		}
		else
		{
			// JTextField...
			return getInput(true);
		}
	}

	/// <summary>
	/// Returns the width of the input JTextField, roughly in characters.  Default is 10. </summary>
	/// <returns> the width of the input JTextField. </returns>
	protected internal virtual int getInputJTextFieldWidth()
	{
		return __inputJTextFieldWidth;
	}

	/// <summary>
	/// Return the input type (one of StringUtil.TYPE_*). </summary>
	/// <returns> the input type (one of StringUtil.TYPE_*). </returns>
	public virtual int getInputType()
	{
		return __inputType;
	}

	/// <summary>
	/// Returns the maximum number of rows to display for this InputFilter's input
	/// JComboBox.  Possible values are JCOMBOBOX_ROWS_DISPLAY_ALL,
	/// JCOMBOBOX_ROWS_DISPLAY_DEFAULT and positive integers. </summary>
	/// <returns> the maximum number of rows to display for this InputFilter's input JComboBox. </returns>
	protected internal virtual int getNumberInputJComboBoxRows()
	{
		return __numInputJComboBoxRows;
	}

	/// <summary>
	/// Return the internal Where field that corresponds to the visible label. </summary>
	/// <returns> the internal Where field that corresponds to the visible label.  </returns>
	public virtual string getWhereInternal()
	{
		return __whereInternal;
	}

	/// <summary>
	/// Returns the secondary internal Where field that corresponds to the visible 
	/// label.  If no secondary internal where field has been defined, then the 
	/// primary internal where field is returned (identical to calling getWhereInternal()). </summary>
	/// <returns> the secondary internal Where field that corresponds to the visible label. </returns>
	public virtual string getWhereInternal2()
	{
		if (!string.ReferenceEquals(__whereInternal2, null) && !__whereInternal2.Equals(""))
		{
			return __whereInternal2;
		}
		else
		{
			return getWhereInternal();
		}
	}

	/// <summary>
	/// Return the Where field label that is visible to the user, such as user interfaces. </summary>
	/// <returns> the Where field label that is visible to the user, such as user interfaces.  </returns>
	public virtual string getWhereLabel()
	{
		return __whereLabel;
	}

	/// <summary>
	/// Return the Where field label that is used in persistent forms, such as command parameter. </summary>
	/// <returns> the Where field label that is used in persistent forms, such as command parameter.  </returns>
	public virtual string getWhereLabelPersistent()
	{
		return __whereLabelPersistent;
	}

	/// <summary>
	/// Returns whether the input text field (if a text field is being used for input) is editable or not. </summary>
	/// <returns> whether the input text field (if a text field is being used for input) is editable or not. </returns>
	protected internal virtual bool isInputJTextFieldEditable()
	{
		return __inputJTextFieldEditable;
	}

	/// <summary>
	/// Indicate whether a string matches the filter.  This can be used, for example,
	/// to see if the filter input matches a secondary string when applying the
	/// filter manually (e.g., outside of a database).  The filter type is checked and
	/// appropriate comparisons are made. </summary>
	/// <param name="s"> String to compare to.  Numerical values are converted from the string
	/// (false is returned if a conversion from string to number cannot be made). </param>
	/// <param name="operator"> Operator to apply to the filter (usually managed in InputFilter_JPanel). </param>
	/// <param name="ignore_case"> If true, then case is ignored when comparing the strings. </param>
	/// <returns> true if the string matches the current input for the input filter, or false otherwise. </returns>
	public virtual bool matches(string s, string @operator, bool ignore_case)
	{
		string input = getInput(false);
		if (__inputType == StringUtil.TYPE_STRING)
		{
			if (@operator.Equals(INPUT_MATCHES, StringComparison.OrdinalIgnoreCase))
			{
				// Full string must match...
				if (ignore_case)
				{
					return input.Equals(s, StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					return input.Equals(s);
				}
			}
			else if (@operator.Equals(INPUT_STARTS_WITH, StringComparison.OrdinalIgnoreCase))
			{
				if (ignore_case)
				{
					return s.ToUpper().matches(input.ToUpper() + ".*");
				}
				else
				{
					return s.matches(input + ".*");
				}
			}
			else if (@operator.Equals(INPUT_ENDS_WITH, StringComparison.OrdinalIgnoreCase))
			{
				if (ignore_case)
				{
					return s.ToUpper().matches(".*" + input.ToUpper());
				}
				else
				{
					return s.matches(".*" + input);
				}
			}
			else if (@operator.Equals(INPUT_CONTAINS, StringComparison.OrdinalIgnoreCase))
			{
				if (ignore_case)
				{
					return s.ToUpper().matches(".*" + input.ToUpper() + ".*");
				}
				else
				{
					return s.matches(".*" + input + ".*");
				}
			}
			// Operator not recognized.
			return false;
		}
		else if ((__inputType == StringUtil.TYPE_INTEGER) && StringUtil.isInteger(s))
		{
			// Use the overloaded method...
			return matches(StringUtil.atoi(s), @operator);
		}
		else if ((__inputType == StringUtil.TYPE_DOUBLE) && StringUtil.isDouble(s))
		{
			// Use the overloaded method...
			return matches(StringUtil.atod(s), @operator);
		}
		// Data type not recognized...
		return false;
	}

	/// <summary>
	/// Indicate whether an integer matches the filter.  This can be used, for example,
	/// to see if the filter input matches a secondary integer when applying the
	/// filter manually (e.g., outside of a database).  The filter type is checked and
	/// appropriate comparisons are made. </summary>
	/// <param name="i"> Integer to compare to.  The filter type must be for an integer. </param>
	/// <param name="operator"> Operator to apply to the filter (usually managed in InputFilter_JFrame). </param>
	/// <returns> true if the integer matches the current input for the input filter, or false otherwise. </returns>
	public virtual bool matches(int i, string @operator)
	{
		string input = getInput(false);
		if (__inputType != StringUtil.TYPE_INTEGER)
		{
			return false;
		}
		if (!StringUtil.isInteger(input))
		{
			return false;
		}
		int input_int = StringUtil.atoi(input);
		if (@operator.Equals(INPUT_EQUALS) || @operator.Equals(INPUT_EQUALS_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			return (i == input_int);
		}
		else if (@operator.Equals(INPUT_LESS_THAN) || @operator.Equals(INPUT_LESS_THAN_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			return (i < input_int);
		}
		else if (@operator.Equals(INPUT_LESS_THAN_OR_EQUAL_TO))
		{
			return (i <= input_int);
		}
		else if (@operator.Equals(INPUT_GREATER_THAN) || @operator.Equals(INPUT_GREATER_THAN_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			return (i > input_int);
		}
		else if (@operator.Equals(INPUT_GREATER_THAN_OR_EQUAL_TO))
		{
			return (i >= input_int);
		}
		// Operator not recognized...
		return false;
	}

	/// <summary>
	/// Indicate whether a double matches the filter.  This can be used, for example,
	/// to see if the filter input matches a secondary double when applying the
	/// filter manually (e.g., outside of a database).  The filter type is checked and
	/// appropriate comparisons are made. </summary>
	/// <param name="d"> Double to compare to.  The filter type must be for a double. </param>
	/// <param name="operator"> Operator to apply to the filter (usually managed in InputFilter_JFrame). </param>
	/// <returns> true if the integer matches the current input for the input filter, or false otherwise. </returns>
	public virtual bool matches(double d, string @operator)
	{
		string input = getInput(false);
		if (__inputType != StringUtil.TYPE_DOUBLE)
		{
			return false;
		}
		if (!StringUtil.isDouble(input))
		{
			return false;
		}
		double input_double = StringUtil.atod(input);
		if (@operator.Equals(INPUT_EQUALS) || @operator.Equals(INPUT_EQUALS_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			return (d == input_double);
		}
		else if (@operator.Equals(INPUT_LESS_THAN) || @operator.Equals(INPUT_LESS_THAN_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			return (d < input_double);
		}
		else if (@operator.Equals(INPUT_LESS_THAN_OR_EQUAL_TO))
		{
			return (d <= input_double);
		}
		else if (@operator.Equals(INPUT_GREATER_THAN) || @operator.Equals(INPUT_GREATER_THAN_LEGACY, StringComparison.OrdinalIgnoreCase))
		{
			return (d > input_double);
		}
		else if (@operator.Equals(INPUT_GREATER_THAN_OR_EQUAL_TO))
		{
			return (d >= input_double);
		}
		// Operator not recognized...
		return false;
	}

	/// <summary>
	/// Removes a constraint from the constraint combo box.  If the given constraint
	/// does not exist in the combo box, nothing will be done. </summary>
	/// <param name="constraint"> the constraint (see the INPUT_* data members) to remove from the constraint combo box. </param>
	public virtual void removeConstraint(string constraint)
	{
		if (__removedConstraints == null)
		{
			__removedConstraints = new List<string>();
		}
		__removedConstraints.Add(constraint);
	}

	/// <summary>
	/// Set the choices available to a filter.  This can be called after initialization to change the list of
	/// choices, for example, based on dynamically selected information. </summary>
	/// <param name="choiceLabels"> A list of String containing choice values to be
	/// displayed to the user.  If null, the user will not be shown a list of choices. </param>
	/// <param name="choicesInternal"> A list of String containing choice values (e.g., database column values). </param>
	/// <param name="areChoicesEditable"> If true, and a non-null list of choices is provided,
	/// the choices will also be editable (an editable JTextField part of the JComboBox will be shown). </param>
	public virtual void setChoices(IList<string> choiceLabels, IList<string> choicesInternal, bool areChoicesEditable)
	{ // Clear the list and add the new list so that GUI components that use this class as the data model
		// retain the same references.
		__choiceLabelList.Clear();
		((IList<string>)__choiceLabelList).AddRange(choiceLabels);
		__choiceInternalList.Clear();
		((IList<string>)__choiceInternalList).AddRange(choicesInternal);
		__areChoicesEditable = areChoicesEditable;
	}

	/// <summary>
	/// Set the input component for the data filter.  This is used by external code
	/// to manage GUI components used for input.  A distinct component should be
	/// available for each filter, usually a JComboBox or a JTextField. </summary>
	/// <param name="input_component"> the input component. </param>
	public virtual void setInputComponent(JComponent input_component)
	{
		__inputComponent = input_component;

		if (__inputComponent != null)
		{
			__inputComponent.setToolTipText(__inputComponentToolTipText);
		}
	}

	/// <summary>
	/// Sets the tool tip text for the input component. </summary>
	/// <param name="text"> the tool tip text for the input component. </param>
	public virtual void setInputComponentToolTipText(string text)
	{
		__inputComponentToolTipText = text;
	}

	/// <summary>
	/// Sets whether the input text field (if a text field is being used) is editable
	/// or not.  By default the text field is editable.  This is only useful in 
	/// limited cases, such as when a value is built in the software in a dialog box
	/// and used to fill the text field. </summary>
	/// <param name="editable"> whether the input text field is editable or not. </param>
	public virtual void setInputJTextFieldEditable(bool editable)
	{
		__inputJTextFieldEditable = editable;
	}

	/// <summary>
	/// Sets the width of the input JTextField, in a measurement which roughly 
	/// corresponds to width of a character on the GUI.  Default is 10. </summary>
	/// <param name="width"> the width to set the input JTextField to, in characters. </param>
	public virtual void setInputJTextFieldWidth(int width)
	{
		__inputJTextFieldWidth = width;
	}

	/// <summary>
	/// Sets the maximum number of rows to display in the input JComboBox for this
	/// InputFilter.  If this InputFilter does not use a JComboBox as an input 
	/// component, then this method will do nothing.  If JCOMBOBOX_ROWS_DISPLAY_ALL 
	/// is passed in for the number of rows, the JComboBox will be sized to display all of its rows. </summary>
	/// <param name="num"> the maximum number of rows to display.  If 
	/// JCOMBOBOX_ROWS_DISPLAY_ALL is passed in for the number of rows, the 
	/// JComboBox will be sized to display all of its rows. </param>
	public virtual void setNumberInputJComboBoxRows(int num)
	{
		__numInputJComboBoxRows = num;
	}

	/// <summary>
	/// Set the information needed to parse an expanded input choice (e.g.,
	/// "data - note") into tokens so that only the data value can be retrieved.
	/// It is assumed that the extracted token is a string. </summary>
	/// <param name="delimiter"> The characters to be used as delimiters.  Multiple adjacent
	/// delimiters are treated as one delimiter when parsing. </param>
	/// <param name="token"> After parsing the input choice using the given delimiter, indicate
	/// the token position for the data value. </param>
	public virtual void setTokenInfo(string delimiter, int token)
	{
		// Token type defaults to 0, meaning use the primary input type.
		setTokenInfo(delimiter, token, -1);
	}

	/// <summary>
	/// Set the information needed to parse an expanded input choice (e.g.,
	/// "data - note") into tokens so that only the data value can be retrieved. </summary>
	/// <param name="delimiter"> The characters to be used as delimiters.  Multiple adjacent
	/// delimiters are treated as one delimiter when parsing. </param>
	/// <param name="token"> After parsing the input choice using the given delimiter, indicate
	/// the token position for the data value. </param>
	/// <param name="tokenType"> the type of the token for comparison, StringUtil.TYPE_*.
	/// For example, a string choice can be "Value - Note", where the initial filter type is string.
	/// However, the choices should actually be appropriate for the token type, for example integer. </param>
	public virtual void setTokenInfo(string delimiter, int token, int tokenType)
	{
		__choiceDelimiter = delimiter;
		__choiceToken = token;
		__choiceTokenType = tokenType;
	}

	}

}