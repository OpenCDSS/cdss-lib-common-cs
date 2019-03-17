using System.Collections.Generic;
using System.Text;

// DateTime_JPanel - a JPanel to edit a DateTime and retrieve a date/time as the corresponding DateTime object or string

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

namespace RTi.Util.Time
{



	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// <para>
	/// A JPanel to edit a DateTime and retrieve a date/time as the corresponding DateTime object or string.
	/// The constructor indicates the range of date/time fields to display, which should normally correspond
	/// to the detail needed for input.  However, because of variability in data (such as time series intervals
	/// only being known at run-time), the editor may show extra precision.  Also, in order to facilitate parsing
	/// the date/time string after editing, it is necessary to include blank indicators (such as 99) in the string.
	/// </para>
	/// <para>
	/// Currently the minimum precision allowed is TimeInterval.MINUTE.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DateTime_JPanel extends javax.swing.JPanel implements java.awt.event.ActionListener
	public class DateTime_JPanel : JPanel, ActionListener
	{

	/// <summary>
	/// Maximum interval shown in the panel, where YEAR is the big and MINUTE is small.
	/// For example, specifying TimeInterval.MONTH will omit display of year.
	/// </summary>
	private int __intervalMax = TimeInterval.YEAR;

	/// <summary>
	/// Minimum interval shown in the panel, where YEAR is the big and MINUTE is small.
	/// For example, specifying TimeInterval.DAY will omit display of hour, minute, etc.
	/// </summary>
	private int __intervalMin = TimeInterval.MINUTE;

	/// <summary>
	/// Day choices.
	/// </summary>
	internal SimpleJComboBox __day_JComboBox = null;

	/// <summary>
	/// Day choices label.
	/// </summary>
	internal JLabel __day_JLabel = null;

	/// <summary>
	/// Hour choices.
	/// </summary>
	internal SimpleJComboBox __hour_JComboBox = null;

	/// <summary>
	/// Hour choices label.
	/// </summary>
	internal JLabel __hour_JLabel = null;

	/// <summary>
	/// Minute choices.
	/// </summary>
	internal SimpleJComboBox __minute_JComboBox = null;

	/// <summary>
	/// Minute choices label.
	/// </summary>
	internal JLabel __minute_JLabel = null;

	/// <summary>
	/// Month choices.
	/// </summary>
	internal SimpleJComboBox __month_JComboBox = null;

	/// <summary>
	/// Month choices label.
	/// </summary>
	internal JLabel __month_JLabel = null;

	/// <summary>
	/// Title of the panel.  If specified, use a border to draw around the panel.
	/// </summary>
	private string __title = "";

	/// <summary>
	/// Year choices.
	/// </summary>
	internal SimpleJComboBox __year_JComboBox = null;

	/// <summary>
	/// Year choices label.
	/// </summary>
	internal JLabel __year_JLabel = null;

	// FIXME SAM 2008-01-23 Change intervals from integers to enumeration when
	// code base has moved to Java 1.5
	/// <summary>
	/// Constructor. </summary>
	/// <param name="title"> Title for the panel, shown in a border title. </param>
	/// <param name="intervalMax"> The maximum interval shown in choices (use TimeInterval.MONTH, etc.). </param>
	/// <param name="intervalMin"> The minimum interval shown in choices (use TimeInterval.HOUR, etc.). </param>
	/// <param name="initial"> Initial DateTime to be displayed.  The precision will override interval_min. </param>
	public DateTime_JPanel(string title, int intervalMax, int intervalMin, DateTime initial) : base()
	{
		setTitle(title);
		setIntervalMax(intervalMax);
		setIntervalMin(intervalMin);
		setupUI();
		// Set the date time after setting the interval_min because the DateTime may override.
		if (initial != null)
		{
			setDateTime(initial);
		}
	}

	/// <summary>
	/// Handle action events. </summary>
	/// <param name="event"> action event to handle. </param>
	public override void actionPerformed(ActionEvent @event)
	{
		// If the event source is the month choice, update the day choices to be correct.
		if (@event.getSource() == __month_JComboBox)
		{
			setDayChoices(__year_JComboBox, __month_JComboBox, __day_JComboBox);
		}

	}

	/// <summary>
	/// Add an action listener for all of the components.  The listeners will be notified if
	/// any action events occur.
	/// </summary>
	public virtual void addActionListener(ActionListener listener)
	{
		if (__year_JComboBox != null)
		{
			__year_JComboBox.addActionListener(listener);
		}
		if (__month_JComboBox != null)
		{
			__month_JComboBox.addActionListener(listener);
		}
		if (__day_JComboBox != null)
		{
			__day_JComboBox.addActionListener(listener);
		}
		if (__hour_JComboBox != null)
		{
			__hour_JComboBox.addActionListener(listener);
		}
		if (__minute_JComboBox != null)
		{
			__minute_JComboBox.addActionListener(listener);
		}
	}

	/// <summary>
	/// Set the DateTime that is selected in choices.
	/// Any values that are 99, etc. are assumed to not be displayed
	/// </summary>
	public virtual void setDateTime(DateTime dt)
	{
		//Message.printStatus ( 2, "", "Setting panel DateTime to " + dt );
		// TODO SAM 2011-12-12 Should the following be done?  The user may want to change and the
		// command handles blank date/time properly.  The problem is that the command parameter will not have
		// 99 saved (may have zeros) - setting the precision here prevents the zeros from propagating to output.
		setIntervalMin(dt.getPrecision());
		// Update the visibility of components.
		setVisibility();
		if ((TimeInterval.YEAR <= __intervalMax) && (TimeInterval.YEAR >= __intervalMin))
		{
			int year = dt.getYear();
			if (TimeUtil.isValidYear(year))
			{
				__year_JComboBox.select("" + year);
			}
			else
			{
				// Select blank
				__year_JComboBox.select("");
			}
		}
		if ((TimeInterval.MONTH <= __intervalMax) && (TimeInterval.MONTH >= __intervalMin))
		{
			int month = dt.getMonth();
			if (TimeUtil.isValidMonth(month))
			{
				__month_JComboBox.select("" + month);
			}
			else
			{
				// Select blank
				__month_JComboBox.select("");
			}
		}
		if ((TimeInterval.DAY <= __intervalMax) && (TimeInterval.DAY >= __intervalMin))
		{
			int day = dt.getDay();
			if (TimeUtil.isValidDay(day))
			{
				__day_JComboBox.select("" + day);
			}
			else
			{
				// Select blank
				__day_JComboBox.select("");
			}
		}
		if ((TimeInterval.HOUR <= __intervalMax) && (TimeInterval.HOUR >= __intervalMin))
		{
			int hour = dt.getHour();
			if (TimeUtil.isValidHour(hour))
			{
				__hour_JComboBox.select(StringUtil.formatString(hour,"%02d"));
			}
			else
			{
				// Select blank
				__hour_JComboBox.select("");
			}
		}
		if ((TimeInterval.MINUTE <= __intervalMax) && (TimeInterval.MINUTE >= __intervalMin))
		{
			int minute = dt.getMinute();
			if (TimeUtil.isValidMinute(minute))
			{
				__minute_JComboBox.select(StringUtil.formatString(minute,"%02d"));
			}
			else
			{
				// Select blank
				__minute_JComboBox.select("");
			}
		}
	}

	/// <summary>
	/// Set up the daily choices based on the selected year.  If the year is not visible and the month is 2,
	/// include 29 days.
	/// </summary>
	private void setDayChoices(SimpleJComboBox year_JComboBox, SimpleJComboBox month_JComboBox, SimpleJComboBox day_JComboBox)
	{
		IList<string> dayList = new List<string>(31);
		dayList.Add(""); // Select to ignore this information
		int maxDay = 31;
		int year = -9999;
		int month = -9999;
		if (year_JComboBox.isVisible())
		{
			string yearSelection = year_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(yearSelection))
			{
				year = int.Parse(yearSelection);
			}
		}
		if (month_JComboBox.isVisible())
		{
			string monthSelection = month_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(monthSelection))
			{
				month = int.Parse(monthSelection);
			}
		}
		if (month != -9999)
		{
			// Try to be more specific with the number of days.
			if (year != -9999)
			{
				maxDay = TimeUtil.numDaysInMonth(month, year);
			}
			else
			{
				// Assume a leap year...
				maxDay = TimeUtil.numDaysInMonth(month, 1976);
			}
		}
		for (int i = 1; i <= maxDay; i++)
		{
			dayList.Add("" + i);
		}
		day_JComboBox.setData(dayList);
	}

	/// <summary>
	/// Set the panel enabled/disabled by setting each component enabled/disabled. </summary>
	/// <param name="enabled"> Indicates whether the panel components should be enabled or disabled. </param>
	public virtual void setEnabled(bool enabled)
	{
		if (__year_JComboBox != null)
		{
			__year_JComboBox.setEnabled(enabled);
		}
		if (__month_JComboBox != null)
		{
			__month_JComboBox.setEnabled(enabled);
		}
		if (__day_JComboBox != null)
		{
			__day_JComboBox.setEnabled(enabled);
		}
		if (__hour_JComboBox != null)
		{
			__hour_JComboBox.setEnabled(enabled);
		}
		if (__minute_JComboBox != null)
		{
			__minute_JComboBox.setEnabled(enabled);
		}
	}

	/// <summary>
	/// Set the maximum interval displayed in the panel. </summary>
	/// <param name="interval_max"> see TimeInterval definitions. </param>
	private void setIntervalMax(int interval_max)
	{
		__intervalMax = interval_max;
	}

	/// <summary>
	/// Set the minimum interval displayed in the panel. </summary>
	/// <param name="interval_min"> see TimeInterval definitions. </param>
	public virtual void setIntervalMin(int interval_min)
	{
		__intervalMin = interval_min;
	}

	/// <summary>
	/// Set the title for the panel. </summary>
	/// <param name="title">  </param>
	public virtual void setTitle(string title)
	{
		__title = title;
	}

	/// <summary>
	/// Setup the UI.
	/// </summary>
	private void setupUI()
	{
		if ((!string.ReferenceEquals(__title, null)) && __title.Length > 0)
		{
			setBorder(BorderFactory.createTitledBorder(BorderFactory.createLineBorder(Color.black),__title));
		}

		// Use SimpleJComboBox instances in the layout.
		// Create instances for all interval parts but only set visible what is requested.
		// If setDateTime() is called later, all necessary components can be set visible.

		Insets insetsTLBR = new Insets(2,2,2,2);
		setLayout(new GridBagLayout());

		int x = 0; // X position in grid bag layout
		int y_label = 0; // Positions of labels and entry fields.
		int y_entry = 1;
		// Add the year...
		IList<string> yearList = new List<string>(201);
		yearList.Add("");
		for (int i = 1900; i <= 2100; i++)
		{
			yearList.Add("" + i);
		}
		// Allow edits because may not have all years of interest in the list
		__year_JComboBox = new SimpleJComboBox(yearList, true);
		__year_JLabel = new JLabel("Year:");
		JGUIUtil.addComponent(this, __year_JLabel, x, y_label, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(this, __year_JComboBox, x++, y_entry, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// Add the month...
		IList<string> monthList = new List<string>(13);
		monthList.Add(""); // Select to ignore this information
		for (int i = 1; i <= 12; i++)
		{
			monthList.Add("" + i);
		}
		__month_JComboBox = new SimpleJComboBox(monthList, false);
		__month_JComboBox.addActionListener(this);
		__month_JLabel = new JLabel("Month:");
		JGUIUtil.addComponent(this, __month_JLabel, x, y_label, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(this, __month_JComboBox, x++, y_entry, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// Add the day...
		__day_JComboBox = new SimpleJComboBox(false); // Not editable
		setDayChoices(__year_JComboBox, __month_JComboBox, __day_JComboBox);
		__day_JLabel = new JLabel("Day:");
		JGUIUtil.addComponent(this, __day_JLabel, x, y_label, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(this, __day_JComboBox, x++, y_entry, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// Add the hour...
		IList<string> hourList = new List<string>(25);
		hourList.Add(""); // Select to ignore this information
		for (int i = 0; i <= 23; i++)
		{
			hourList.Add("" + StringUtil.formatString(i,"%02d"));
		}
		__hour_JComboBox = new SimpleJComboBox(hourList, false); // Not editable
		__hour_JLabel = new JLabel("Hour:");
		JGUIUtil.addComponent(this, __hour_JLabel, x, y_label, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(this, __hour_JComboBox, x++, y_entry, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// Add the minute...
		IList<string> minuteList = new List<string>(61);
		minuteList.Add(""); // Select to ignore this information
		for (int i = 0; i <= 59; i++)
		{
			minuteList.Add("" + StringUtil.formatString(i,"%02d"));
		}
		__minute_JComboBox = new SimpleJComboBox(minuteList, true);
		__minute_JLabel = new JLabel("Minute:");
		JGUIUtil.addComponent(this, __minute_JLabel, x, y_label, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(this, __minute_JComboBox, x++, y_entry, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		// Set the visibility on the choices consistent with the min/max interval to display.
		setVisibility();
	}

	/// <summary>
	/// Set the visibility on the choices consistent with the min/max interval to display.
	/// For example, don't show the year if year interval is not within the min and max intervals.
	/// </summary>
	private void setVisibility()
	{
		if ((TimeInterval.YEAR <= __intervalMax) && (TimeInterval.YEAR >= __intervalMin))
		{
			__year_JComboBox.setVisible(true);
			__year_JLabel.setVisible(true);
		}
		else
		{
			__year_JComboBox.setVisible(false);
			__year_JLabel.setVisible(false);
		}
		if ((TimeInterval.MONTH <= __intervalMax) && (TimeInterval.MONTH >= __intervalMin))
		{
			__month_JComboBox.setVisible(true);
			__month_JLabel.setVisible(true);
		}
		else
		{
			__month_JComboBox.setVisible(false);
			__month_JLabel.setVisible(false);
		}
		if ((TimeInterval.DAY <= __intervalMax) && (TimeInterval.DAY >= __intervalMin))
		{
			__day_JComboBox.setVisible(true);
			__day_JLabel.setVisible(true);
		}
		else
		{
			__day_JComboBox.setVisible(false);
			__day_JLabel.setVisible(false);
		}
		if ((TimeInterval.HOUR <= __intervalMax) && (TimeInterval.HOUR >= __intervalMin))
		{
			__hour_JComboBox.setVisible(true);
			__hour_JLabel.setVisible(true);
		}
		else
		{
			__hour_JComboBox.setVisible(false);
			__hour_JLabel.setVisible(false);
		}
		if ((TimeInterval.MINUTE <= __intervalMax) && (TimeInterval.MINUTE >= __intervalMin))
		{
			__minute_JComboBox.setVisible(true);
			__minute_JLabel.setVisible(true);
		}
		else
		{
			__minute_JComboBox.setVisible(false);
			__minute_JLabel.setVisible(false);
		}
	}

	/// <summary>
	/// Return a DateTime (DATE_FAST) representation for the data in the panel.
	/// The date/time will be initialized to TimeUtil.BLANK values if not specified
	/// and will have parts set for the enabled fields that are not blank. </summary>
	/// <returns> a DateTime instance with values set to that of the input components. </returns>
	public virtual DateTime toDateTime()
	{
		DateTime dt = new DateTime(DateTime.DATE_FAST);
		if (__year_JComboBox.isEnabled())
		{
			string year = __year_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(year))
			{
				dt.setYear(int.Parse(year));
			}
			else
			{
				dt.setYear(TimeUtil.BLANK_YEAR);
			}
		}
		if (__month_JComboBox.isEnabled())
		{
			string month = __month_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(month))
			{
				dt.setMonth(int.Parse(month));
			}
			else
			{
				dt.setYear(TimeUtil.BLANK_MONTH);
			}
		}
		if (__day_JComboBox.isEnabled())
		{
			string day = __day_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(day))
			{
				dt.setYear(int.Parse(day));
			}
			else
			{
				dt.setYear(TimeUtil.BLANK_DAY);
			}
		}
		if (__hour_JComboBox.isEnabled())
		{
			string hour = __hour_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(hour))
			{
				dt.setYear(int.Parse(hour));
			}
			else
			{
				dt.setYear(TimeUtil.BLANK_HOUR);
			}
		}
		if (__minute_JComboBox.isEnabled())
		{
			string minute = __minute_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(minute))
			{
				dt.setYear(int.Parse(minute));
			}
			else
			{
				dt.setYear(TimeUtil.BLANK_MINUTE);
			}
		}
		return dt;
	}

	/// <summary>
	/// Return a string representation of the data in the panel, using ISO format.
	/// Enabled fields are included, with default (99) values to indicate blanks.
	/// The year is always included - only parts on the small end are left off if not visible.
	/// </summary>
	public override string ToString()
	{
		return ToString(true, false);
	}

	// FIXME SAM 2009-11-13 stripping works for hour and minute but not other parts - probably OK
	// for now to keep integrity of the date/time string
	/// <summary>
	/// Return a string representation of the data in the panel, using ISO format.
	/// Enabled fields are included.  Left-most fields are always included to ensure a parse-able representation of
	/// the string, with default (99) values to indicate blanks.  Right-most fields are only included if non-blank. </summary>
	/// <param name="includeYear"> indicate whether the year should be included on front of the string.  If the year is not
	/// included, it can be added later if necessary, for example using DateTimeWindow.WINDOW_YEAR. </param>
	/// <param name="stripBlanksFromEnd"> if true, strip blanks from the end of the string. </param>
	public virtual string ToString(bool includeYear, bool stripBlanksFromEnd)
	{
		// The following indicate if the delimiter between date and time parts need to be added (because a
		// left-side data value has been encountered).  The intent is to avoid a left-most hanging leading delimiter.
		bool doDateDelim = false;
		bool doTimeDelim = false;

		string hour = __hour_JComboBox.getSelected().Trim();
		string minute = __minute_JComboBox.getSelected().Trim();
		StringBuilder yearString = new StringBuilder();
		StringBuilder monthString = new StringBuilder();
		StringBuilder dayString = new StringBuilder();
		StringBuilder hourString = new StringBuilder();
		StringBuilder minuteString = new StringBuilder();
		bool monthSpecified = false;
		bool daySpecified = false;
		bool hourSpecified = false;
		bool minuteSpecified = false;
		// First form the string parts going from left (year) to right (minute)
		// Year
		if (includeYear)
		{
			if (__year_JComboBox.isVisible())
			{
				string year = __year_JComboBox.getSelected().Trim();
				doDateDelim = true;
				if (StringUtil.isInteger(year))
				{
					yearString.Append(StringUtil.formatString(int.Parse(year),"%04d"));
				}
				else
				{
					yearString.Append(StringUtil.formatString(TimeUtil.BLANK_YEAR,"%04d"));
				}
			}
		}
		// Month...
		if (doDateDelim)
		{
			monthString.Append("-");
		}
		else
		{
			// Since month has now been included, need to add the date delimiter to following strings
			doDateDelim = true;
		}
		if (__month_JComboBox.isVisible())
		{
			string month = __month_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(month))
			{
				monthString.Append(StringUtil.formatString(int.Parse(month),"%02d"));
				monthSpecified = true;
			}
			else
			{
				monthString.Append(StringUtil.formatString(TimeUtil.BLANK_MONTH,"%02d"));
			}
		}
		else
		{
			// Always need to include month
			monthString.Append(StringUtil.formatString(TimeUtil.BLANK_MONTH,"%02d"));
		}
		// Day...
		if (doDateDelim)
		{
			dayString.Append("-");
		}
		if (__day_JComboBox.isVisible())
		{
			string day = __day_JComboBox.getSelected().Trim();
			if (StringUtil.isInteger(day))
			{
				dayString.Append(StringUtil.formatString(int.Parse(day),"%02d"));
				daySpecified = true;
			}
			else
			{
				dayString.Append(StringUtil.formatString(TimeUtil.BLANK_DAY,"%02d"));
			}
		}
		else
		{
			// Always need to include day
			dayString.Append(StringUtil.formatString(TimeUtil.BLANK_MONTH,"%02d"));
		}
		// Hour...
		if (doDateDelim)
		{
			hourString.Append(" ");
		}
		doTimeDelim = true;
		if (__hour_JComboBox.isVisible())
		{
			if (StringUtil.isInteger(hour))
			{
				hourString.Append(StringUtil.formatString(int.Parse(hour),"%02d"));
				hourSpecified = true;
			}
			else
			{
				hourString.Append(StringUtil.formatString(TimeUtil.BLANK_HOUR,"%02d"));
			}
		}
		else
		{
			hourString.Append(StringUtil.formatString(TimeUtil.BLANK_HOUR,"%02d"));
		}
		// Minute...
		if (doTimeDelim)
		{
			minuteString.Append(":");
		}
		if (__minute_JComboBox.isVisible())
		{
			if (StringUtil.isInteger(minute))
			{
				minuteString.Append(StringUtil.formatString(int.Parse(minute),"%02d"));
				minuteSpecified = true;
			}
			else
			{
				minuteString.Append(StringUtil.formatString(TimeUtil.BLANK_MINUTE,"%02d"));
			}
		}
		else
		{
			minuteString.Append(StringUtil.formatString(TimeUtil.BLANK_MINUTE,"%02d"));
		}
		// Now concatenate the strings for the final result - delimiters will have been included
		StringBuilder b = new StringBuilder();
		if (includeYear)
		{
			b.Append(yearString);
		}
		// Include the month only if month and something to the right of month is specified
		if (monthSpecified || daySpecified || hourSpecified || minuteSpecified)
		{
			b.Append(monthString);
		}
		// Include the day only if day and something to the right of day is specified
		if (daySpecified || hourSpecified || minuteSpecified)
		{
			b.Append(dayString);
		}
		// Include the hour only if hour and something to the right of hour is specified
		if (hourSpecified || minuteSpecified)
		{
			b.Append(hourString);
		}
		// Include the minute only if specified
		if (minuteSpecified)
		{
			b.Append(minuteString);
		}
		return b.ToString();
	}

	}

}