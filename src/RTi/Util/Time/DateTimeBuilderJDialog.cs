using System;
using System.Collections.Generic;

// DateTimeBuilderJDialog - UI to assist in building Dates.

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

//-----------------------------------------------------------------------------r
// DateTimeBuilderJDialog - GUI to assist in building Dates.
//-----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 05 May 1998	DLG, RTi		Created initial class description.
// 07 Jul 1998	Steven A. Malers, RTi	Update so that the check on the date
//					takes into account the precision.
// 12 Oct 2000	SAM, RTi		Change dialog title from "Date Builder"
//					to "Enter Date" or "Enter Date/Time".
//					Add "Title" property to let calling code
//					customize title.  Remove import * to
//					optimize code.  Allow "DateJLabel"
//					property to override default of "Date:".
// 07 Jun 2001	SAM, RTi		Add finalize().  Change static data
//					to non-static to save memory.  Change
//					GUI to GUIUtil.
// 2001-11-06	SAM, RTi		Review javadoc.  Verify that variables
//					are set to null when no longer used.
//-----------------------------------------------------------------------------
// 2003-03-31	J. Thomas Sapienza, RTi	Initial Swing version from
//					TSDateBuilderJDialog.
// 2003-04-01	JTS, RTi		* Corrected bug in determining the 
//					  current time.
//					* Corrected bug that ordered the 
//					  precisions wrong.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.Time
{



	using GUIUtil = RTi.Util.GUI.GUIUtil;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	using PropList = RTi.Util.IO.PropList;

	/// <summary>
	/// This class is a GUI to build and/or edit the time fields of a DateTime object
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DateTimeBuilderJDialog extends javax.swing.JDialog implements java.awt.event.WindowListener, java.awt.event.KeyListener, java.awt.event.ActionListener
	public class DateTimeBuilderJDialog : JDialog, WindowListener, KeyListener, ActionListener
	{

	private bool __fromTo;
	private bool __isCancel;
	private bool __isWarned;

	private readonly int __FROM = 0;
	private readonly int __TO = 1;
	private readonly int __DEFAULT_YEAR = 2000;
	private int __datePrecision;
	private int __dateFormat;

	private JTextField __tMonthJTextField;
	private JTextField __fMonthJTextField;
	private JTextField __TdayJTextField;
	private JTextField __fDayJTextField;
	private JTextField __TyearJTextField = null;
	private JTextField __fYearJTextField = null;
	private JTextField __ThourJTextField;
	private JTextField __fHourJTextField;
	private JTextField __tMinuteJTextField;
	private JTextField __fMinuteJTextField;
	private JTextField __fromJTextField;
	private JTextField __toJTextField;

	private PropList __props;

	private readonly string __OK = "OK";
	private readonly string __CANCEL = "Cancel";

	private DateTime __fromDateTime;
	private DateTime __toDateTime;
	private DateTime __modToDateTime;
	private DateTime __modFromDateTime;

	/// <summary>
	/// DateTimeBuilderJDialog constructor.
	/// Notes:
	/// This constructor shows a GUI with 'from' / 'To' Date JTextFields </summary>
	/// <param name="parent"> JFrame class responsible for instantiating this class. </param>
	/// <param name="fromJTextField"> from date JTextField </param>
	/// <param name="toJTextField"> to date JTextField </param>
	/// <param name="fromDateTime"> from date as DateTime </param>
	/// <param name="toDateTime"> to date as DateTime </param>
	public DateTimeBuilderJDialog(JFrame parent, JTextField fromJTextField, JTextField toJTextField, DateTime fromDateTime, DateTime toDateTime, PropList props)
	{
		setModal(true);
			__fromJTextField = fromJTextField;
			__toJTextField = toJTextField;
			__fromTo = true;
		__fromDateTime = fromDateTime;
		__toDateTime = toDateTime;

		if (__fromDateTime == null)
		{
			__fromDateTime = new DateTime(DateTime.DATE_ZERO);
		}
		if (__toDateTime == null)
		{
			__toDateTime = new DateTime(DateTime.DATE_CURRENT);
		}

		__props = props;
		setupGUI(parent);
	}

	/// <summary>
	/// DateTimeBuilderJDialog constructor.
	/// This constructor shows a GUI with 'from' Date JTextFields. </summary>
	/// <param name="parent"> JFrame class responsible for instantiating this class. </param>
	/// <param name="fromJTextField"> from date JTextField </param>
	/// <param name="fromDateTime"> from date as DateTime </param>
	public DateTimeBuilderJDialog(JFrame parent, JTextField fromJTextField, DateTime fromDateTime, PropList props)
	{
		setModal(true);
		__fromJTextField = fromJTextField;
			__toJTextField = null;
			__fromTo = false;
		__fromDateTime = fromDateTime;
		__toDateTime = null;
		__props = props;
		setupGUI(parent);
	}

	/// <summary>
	/// Responds to ActionEvents </summary>
	/// <param name="evt"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent evt)
	{
		string s = evt.getActionCommand().Trim();
		if (s.Equals(__OK))
		{
			__isCancel = false;
			getLimits();
		}
		else if (s.Equals(__CANCEL))
		{
			__isCancel = true;
			getLimits();
		}
	}

	/// <summary>
	/// Checks the validity of the specified date(s).
	/// @returns true if dates are ok, false otherwise
	/// </summary>
	private bool checkDates()
	{
		string function = "DateTimeBuilderJDialog.okClicked()";

			if (!setDate(__FROM))
			{
					return false;
			}

			if (__fromTo)
			{
					if (!setDate(__TO))
					{
							return false;
					}

			DateTime current = new DateTime(DateTime.DATE_CURRENT);

			// ensure that the from date is not greater 
			// than the to date.
			if (__modFromDateTime.greaterThan(__modToDateTime))
			{
				Message.printWarning(1, function, "From date cannot be greater than To date.");
				return false;
			}
			// ensure that the to date is not greater 
			// than the current system time.
			else if (__modToDateTime.greaterThan(current))
			{
				if (!__isWarned)
				{

					__isWarned = true;
					Message.printWarning(1, function, "To date is greater than the " + "current time (" + current.ToString(__datePrecision) + ").");
					return false;
				}
			}
			}

		// set the dates in the JTextFields, if not null and are applicable
		if (__fromJTextField != null)
		{
			__fromJTextField.setText(__modFromDateTime.ToString(__dateFormat));
		}

		if (__toJTextField != null && __fromTo)
		{
			__toJTextField.setText(__modToDateTime.ToString(__dateFormat));
		}
			return true;
	}

	/// <summary>
	/// Closes the GUI.
	/// </summary>
	private void closeClicked()
	{
		setVisible(false);
			dispose();
	}

	/// <summary>
	/// Clean up memory before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DateTimeBuilderJDialog()
	{
		__tMonthJTextField = null;
		__fMonthJTextField = null;
		__TdayJTextField = null;
		__fDayJTextField = null;
		__TyearJTextField = null;
		__fYearJTextField = null;
		__ThourJTextField = null;
		__fHourJTextField = null;
		__tMinuteJTextField = null;
		__fMinuteJTextField = null;
		__fromJTextField = null;
		__toJTextField = null;
		__fromDateTime = null;
		__toDateTime = null;
		__modToDateTime = null;
		__modFromDateTime = null;
		__props = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns a list of the first and last dates in this dialog, which can be
	/// used to calculate time series limits. </summary>
	/// <returns> a list with the first element containing the first date, the second
	/// element containing the second date. </returns>
	public virtual IList<DateTime> getLimits()
	{
		IList<DateTime> limits = null;

		if (!__isCancel)
		{
			if (!checkDates())
			{
				return null;
			}

			limits = new List<DateTime>();
			limits.Add(__modToDateTime);

			if (__fromTo)
			{
				limits.Add(__modFromDateTime);
			}
		}
		closeClicked();
		return limits;
	}

	/// <summary>
	/// Responds to KeyListener events </summary>
	/// <param name="event"> KeyEvent object </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		int code = @event.getKeyCode();

		// enter key same as selecting ok
		if (code == KeyEvent.VK_ENTER)
		{
				getLimits();
		}
	}

	public virtual void keyReleased(KeyEvent @event)
	{
		;
	}
	public virtual void keyTyped(KeyEvent @event)
	{
		;
	}

	/// <summary>
	/// This function sets the appropriate DateTime object depending on the flag. </summary>
	/// <param name="flag"> DateTime to set, __TO or __FROM </param>
	/// <returns> true on success, false otherwise </returns>
	private bool setDate(int flag)
	{
		string function = "DateTimeBuilderJDialog.setDate()";

		int month = 0;
		int day = 0;
		int year = 0;
		int hour = 0;
		int minute = 0;

		if (flag == __FROM)
		{
			month = StringUtil.atoi(__fMonthJTextField.getText());
			day = StringUtil.atoi(__fDayJTextField.getText());
			if (__fYearJTextField != null)
			{
				year = StringUtil.atoi(__fYearJTextField.getText());
			}
			else
			{
				year = __DEFAULT_YEAR;
			}
			hour = StringUtil.atoi(__fHourJTextField.getText());
			minute = StringUtil.atoi(__fMinuteJTextField.getText());
		}
		else if (flag == __TO)
		{
			month = StringUtil.atoi(__tMonthJTextField.getText());
			day = StringUtil.atoi(__TdayJTextField.getText());
			if (__TyearJTextField != null)
			{
				year = StringUtil.atoi(__TyearJTextField.getText());
			}
			else
			{
				year = __DEFAULT_YEAR;
			}
			hour = StringUtil.atoi(__fHourJTextField.getText());
			hour = StringUtil.atoi(__ThourJTextField.getText());
			minute = StringUtil.atoi(__tMinuteJTextField.getText());
		}

		if (__datePrecision <= DateTime.PRECISION_MONTH)
		{
			// For various reasons, we allow year == 0 and month == 0 but
			// not year != 0 and month == 0...
			if ((year != 0) && !TimeUtil.isValidMonth(month))
			{
				Message.printWarning(1, function, "Invalid month:  " + month);
				return false;
			}
		}

		if (__datePrecision <= DateTime.PRECISION_DAY)
		{
			if (!TimeUtil.isValidDay(day,month,year))
			{

				if (__props.getValue("DateFormat").Equals("US"))
				{
					Message.printWarning(1, function, "Invalid day:  " + month + "/" + day + "/" + year);
					return false;
				}
				else
				{
					Message.printWarning(1, function, "Invalid date:  " + year + "-" + month + "-" + day);
					return false;
				}
			}
		}

		if (__datePrecision <= DateTime.PRECISION_HOUR)
		{
			if (!TimeUtil.isValidHour(hour))
			{
				Message.printWarning(1, function, "Invalid hour:  " + hour);
				return false;
			}
		}

		if (__datePrecision <= DateTime.PRECISION_MINUTE)
		{
			if (!TimeUtil.isValidMinute(minute))
			{
				Message.printWarning(1, function, "Invalid minute:  " + minute);
				return false;
			}
		}

		// instantiate a DateTime and set according to the flag
		DateTime t = new DateTime();
		if (__datePrecision <= DateTime.PRECISION_YEAR)
		{
			t.setYear(year);
		}
		if (__datePrecision <= DateTime.PRECISION_MONTH)
		{
			t.setMonth(month);
		}
		if (__datePrecision <= DateTime.PRECISION_DAY)
		{
			t.setDay(day);
		}
		if (__datePrecision <= DateTime.PRECISION_HOUR)
		{
			t.setHour(hour);
		}
		if (__datePrecision <= DateTime.PRECISION_MINUTE)
		{
			t.setMinute(minute);
		}

		if (flag == __TO)
		{
			__modToDateTime = new DateTime(t);
		}
		else if (flag == __FROM)
		{
			__modFromDateTime = new DateTime(t);
		}

		return true;
	}

	/// <summary>
	/// Constructs and displays the GUI layout. </summary>
	/// <param name="parent"> parent frame, used to center the date dialog on the correct screen. </param>
	private void setupGUI(JFrame parent)
	{
		setProperties();

		JLabel f_YM = null;
		JLabel f_MD = null;
		JLabel f_HM = null;
		JLabel t_YM = null;
		JLabel t_MD = null;
		JLabel t_HM = null;

			// objects used throughout the GUI layout
		addWindowListener(this);

			Insets insetsNLNR = new Insets(0,7,0,7);
			Insets insetsNLNN = new Insets(0,7,0,0);
			Insets insetsN4NN = new Insets(0,28,0,0);
			Insets insetsNNNR = new Insets(0,0,0,7);
			GridBagLayout gbl = new GridBagLayout();

			// Center panel
			JPanel centerJPanel = new JPanel();
			centerJPanel.setLayout(gbl);
			getContentPane().add("Center", centerJPanel);

		string prop_value = __props.getValue("EnableYear");
		if (string.ReferenceEquals(prop_value, null))
		{
			prop_value = "true";
		}
		if (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			GUIUtil.addComponent(centerJPanel, new JLabel("Year"), 1, 0, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		}

		if (__datePrecision == DateTime.PRECISION_MONTH || __datePrecision == DateTime.PRECISION_DAY || __datePrecision == DateTime.PRECISION_HOUR || __datePrecision == DateTime.PRECISION_MINUTE)
		{
				GUIUtil.addComponent(centerJPanel, new JLabel("Month"), 3, 0, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		}

		if (__datePrecision == DateTime.PRECISION_DAY || __datePrecision == DateTime.PRECISION_HOUR || __datePrecision == DateTime.PRECISION_MINUTE)
		{
				GUIUtil.addComponent(centerJPanel, new JLabel("Day"), 5, 0, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		}

		if (__datePrecision == DateTime.PRECISION_HOUR || __datePrecision == DateTime.PRECISION_MINUTE)
		{
				GUIUtil.addComponent(centerJPanel, new JLabel("Hour"), 6, 0, 1, 1, 0, 0, insetsN4NN, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		}

		if (__datePrecision == DateTime.PRECISION_MINUTE)
		{
				GUIUtil.addComponent(centerJPanel, new JLabel("Minute"), 8, 0, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		}

			if (__fromTo)
			{
					GUIUtil.addComponent(centerJPanel, new JLabel("From Date:"), 0, 1, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);
			}
			else
			{
				prop_value = __props.getValue("DateJLabel");
			if (string.ReferenceEquals(prop_value, null))
			{
				prop_value = "Date:";
			}
			GUIUtil.addComponent(centerJPanel, new JLabel(prop_value), 0, 1, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);
			}

		prop_value = __props.getValue("EnableYear");
		if (string.ReferenceEquals(prop_value, null))
		{
			prop_value = "true";
		}
		if (prop_value.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
				__fYearJTextField = new JTextField(5);
			__fYearJTextField.addKeyListener(this);
				GUIUtil.addComponent(centerJPanel, __fYearJTextField, 1, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		}

			GUIUtil.addComponent(centerJPanel, f_YM = new JLabel("-"), 2, 1, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);

			__fMonthJTextField = new JTextField(3);
		__fMonthJTextField.addKeyListener(this);
			GUIUtil.addComponent(centerJPanel, __fMonthJTextField, 3, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

			GUIUtil.addComponent(centerJPanel, f_MD = new JLabel("-"), 4, 1, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);

			   __fDayJTextField = new JTextField(3);
		__fDayJTextField.addKeyListener(this);
			GUIUtil.addComponent(centerJPanel, __fDayJTextField, 5, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

			__fHourJTextField = new JTextField(3);
		__fHourJTextField.addKeyListener(this);
			GUIUtil.addComponent(centerJPanel, __fHourJTextField, 6, 1, 1, 1, 0, 0, insetsN4NN, GridBagConstraints.NONE, GridBagConstraints.CENTER);

			GUIUtil.addComponent(centerJPanel, f_HM = new JLabel(":"), 7, 1, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);

			__fMinuteJTextField = new JTextField(3);
		__fMinuteJTextField.addKeyListener(this);
			GUIUtil.addComponent(centerJPanel, __fMinuteJTextField, 8, 1, 1, 1, 0, 0, insetsNNNR, GridBagConstraints.NONE, GridBagConstraints.CENTER);

			if (__fromTo)
			{
					GUIUtil.addComponent(centerJPanel, new JLabel("To Date:"), 0, 2, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

					__TyearJTextField = new JTextField(5);
			__TyearJTextField.addKeyListener(this);
					GUIUtil.addComponent(centerJPanel, __TyearJTextField, 1, 2, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

					GUIUtil.addComponent(centerJPanel, t_YM = new JLabel("-"), 2, 2, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);

					__tMonthJTextField = new JTextField(3);
			__tMonthJTextField.addKeyListener(this);
					GUIUtil.addComponent(centerJPanel, __tMonthJTextField, 3, 2, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

					GUIUtil.addComponent(centerJPanel, t_MD = new JLabel("-"), 4, 2, 1, 1, 0, 0, insetsNLNN, GridBagConstraints.NONE, GridBagConstraints.CENTER);

					__TdayJTextField = new JTextField(3);
			__TdayJTextField.addKeyListener(this);
					GUIUtil.addComponent(centerJPanel, __TdayJTextField, 5, 2, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

					__ThourJTextField = new JTextField(3);
			__ThourJTextField.addKeyListener(this);
					GUIUtil.addComponent(centerJPanel, __ThourJTextField, 6, 2, 1, 1, 0, 0, insetsN4NN, GridBagConstraints.NONE, GridBagConstraints.CENTER);

					GUIUtil.addComponent(centerJPanel, t_HM = new JLabel(":"), 7, 2, 1, 1, 0,0,insetsNLNN, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

					__tMinuteJTextField = new JTextField(3);
			__tMinuteJTextField.addKeyListener(this);
					GUIUtil.addComponent(centerJPanel, __tMinuteJTextField, 8, 2, 1, 1, 0, 0, insetsNNNR, GridBagConstraints.NONE, GridBagConstraints.CENTER);
			}

			// Bottom panel
			JPanel bottomJPanel = new JPanel();
			bottomJPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
			getContentPane().add("South", bottomJPanel);

			bottomJPanel.add(new SimpleJButton(__OK, this));
			bottomJPanel.add(new SimpleJButton(__CANCEL, this));

			setTitle("Enter Date");

			if (__fromDateTime != null)
			{
					__fMonthJTextField.setText("" + __fromDateTime.getMonth());
					   __fDayJTextField.setText("" + __fromDateTime.getDay());
			if (__fYearJTextField != null)
			{
						   __fYearJTextField.setText("" + __fromDateTime.getYear());
			}
					__fHourJTextField.setText("" + __fromDateTime.getHour());
					   __fMinuteJTextField.setText("" + __fromDateTime.getMinute());
				setTitle("Enter Date/Time");
			}

			   if (__fromTo && __toDateTime != null)
			   {
					__tMonthJTextField.setText("" + __toDateTime.getMonth());
					   __TdayJTextField.setText("" + __toDateTime.getDay());
			if (__TyearJTextField != null)
			{
						   __TyearJTextField.setText("" + __toDateTime.getYear());
			}
					__ThourJTextField.setText("" + __toDateTime.getHour());
					   __tMinuteJTextField.setText("" + __toDateTime.getMinute());
				setTitle("Enter Date/Time");
			   }

		// hide JTextFields which do not pertain to the __datePrecision
		if (__datePrecision != DateTime.PRECISION_MONTH && __datePrecision != DateTime.PRECISION_DAY && __datePrecision != DateTime.PRECISION_HOUR && __datePrecision != DateTime.PRECISION_MINUTE)
		{
			__fMonthJTextField.setVisible(false);
			f_YM.setVisible(false);
			if (__fromTo)
			{
				t_YM.setVisible(false);
				__tMonthJTextField.setVisible(false);
			}
		}
		if (__datePrecision != DateTime.PRECISION_DAY && __datePrecision != DateTime.PRECISION_HOUR && __datePrecision != DateTime.PRECISION_MINUTE)
		{
			__fDayJTextField.setVisible(false);
			f_MD.setVisible(false);
			if (__fromTo)
			{
				t_MD.setVisible(false);
				__TdayJTextField.setVisible(false);
			}
		}
		if (__datePrecision != DateTime.PRECISION_HOUR && __datePrecision != DateTime.PRECISION_MINUTE)
		{
			__fHourJTextField.setVisible(false);
			if (__fromTo)
			{
				__ThourJTextField.setVisible(false);
			}
				setTitle("Enter Date/Time");
		}
		if (__datePrecision != DateTime.PRECISION_MINUTE)
		{
			__fMinuteJTextField.setVisible(false);
			f_HM.setVisible(false);
			if (__fromTo)
			{
				t_HM.setVisible(false);
				__tMinuteJTextField.setVisible(false);
			}
				setTitle("Enter Date/Time");
		}

		prop_value = __props.getValue("Title");
		if (!string.ReferenceEquals(prop_value, null))
		{
			setTitle(prop_value);
		}

			setBackground(Color.lightGray);
			pack();
			GUIUtil.center(this,parent);
			setResizable(false);
			base.setVisible(true);
	}

	/// <summary>
	/// Sets the properties to be used in an instance if this class.
	/// </summary>
	private void setProperties()
	{
		if (__props == null)
		{
			__props = new PropList("DateTimeBuilderJDialog properties");
		}

			string propValue = __props.getValue("DatePrecision");
			if (string.ReferenceEquals(propValue, null))
			{
					__datePrecision = DateTime.PRECISION_MINUTE;
			}
			else
			{
			if (propValue.Equals("Year", StringComparison.OrdinalIgnoreCase))
			{
				__datePrecision = DateTime.PRECISION_YEAR;
			}
			else if (propValue.Equals("Month", StringComparison.OrdinalIgnoreCase))
			{
				__datePrecision = DateTime.PRECISION_MONTH;
			}
			else if (propValue.Equals("Day", StringComparison.OrdinalIgnoreCase))
			{
				__datePrecision = DateTime.PRECISION_DAY;
			}
			else if (propValue.Equals("Hour", StringComparison.OrdinalIgnoreCase))
			{
				__datePrecision = DateTime.PRECISION_HOUR;
			}
			else if (propValue.Equals("Minute", StringComparison.OrdinalIgnoreCase))
			{
				__datePrecision = DateTime.PRECISION_MINUTE;
			}
			else
			{
				__datePrecision = DateTime.PRECISION_MINUTE;
			}
			}

			propValue = __props.getValue("DateFormat");
		string dateFormat = "";
			if (string.ReferenceEquals(propValue, null))
			{
			// Default to "Y2K"
			dateFormat = "Y2K";
			__props.setValue("DateFormat", dateFormat);
			}
			else
			{
			dateFormat = propValue;
			}
		if (dateFormat.Equals("US", StringComparison.OrdinalIgnoreCase))
		{
			if (__datePrecision == DateTime.PRECISION_YEAR)
			{
				__dateFormat = DateTime.FORMAT_YYYY;
			}
			else if (__datePrecision == DateTime.PRECISION_MONTH)
			{
				__dateFormat = DateTime.FORMAT_MM_SLASH_YYYY;
			}
			else if (__datePrecision == DateTime.PRECISION_DAY)
			{
				__dateFormat = DateTime.FORMAT_MM_SLASH_DD_SLASH_YYYY;
			}
			else if (__datePrecision == DateTime.PRECISION_HOUR)
			{
				  __dateFormat = DateTime.FORMAT_MM_SLASH_DD_SLASH_YYYY_HH_mm;
			}
			else if (__datePrecision == DateTime.PRECISION_MINUTE)
			{
				  __dateFormat = DateTime.FORMAT_MM_SLASH_DD_SLASH_YYYY_HH_mm;
			}
		}
		else
		{
			// Default or "Y2K"...
			if (__datePrecision == DateTime.PRECISION_YEAR)
			{
				__dateFormat = DateTime.FORMAT_YYYY;
			}
			else if (__datePrecision == DateTime.PRECISION_MONTH)
			{
				__dateFormat = DateTime.FORMAT_YYYY_MM;
			}
			else if (__datePrecision == DateTime.PRECISION_DAY)
			{
				__dateFormat = DateTime.FORMAT_YYYY_MM_DD;
			}
			else if (__datePrecision == DateTime.PRECISION_HOUR)
			{
				__dateFormat = DateTime.FORMAT_YYYY_MM_DD_HH;
			}
			else if (__datePrecision == DateTime.PRECISION_MINUTE)
			{
				__dateFormat = DateTime.FORMAT_YYYY_MM_DD_HH_mm;
			}
		}
	}

	public virtual void windowActivated(WindowEvent evt)
	{
		;
	}
	public virtual void windowClosed(WindowEvent evt)
	{
		;
	}

	/// <param name="evt"> WindowEvent object </param>
	public virtual void windowClosing(WindowEvent evt)
	{
		__isCancel = true;
		getLimits();
	}

	public virtual void windowDeactivated(WindowEvent evt)
	{
		;
	}
	public virtual void windowDeiconified(WindowEvent evt)
	{
		;
	}
	public virtual void windowIconified(WindowEvent evt)
	{
		;
	}
	public virtual void windowOpened(WindowEvent evt)
	{
		;
	}

	} // end class definition

}