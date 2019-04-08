using System.Collections.Generic;

// DateTimeFormatterSpecifiersJPanel - panel to provide editing capabilities to construct a format specifier string

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
	/// Panel to provide editing capabilities to construct a format specifier string, which includes
	/// one or more of the individual specifiers and literals.  The control consists of an editable text field,
	/// an Insert button, and a JChoice with a list of available specifiers.  Use getText() to get
	/// the contents of the text field.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DateTimeFormatterSpecifiersJPanel extends javax.swing.JPanel implements java.awt.event.ItemListener
	public class DateTimeFormatterSpecifiersJPanel : JPanel, ItemListener
	{
	/// <summary>
	/// Hint to aid user.
	/// </summary>
	private string __hint = "----- Select Specifier -----";

	/// <summary>
	/// Text field containing the edited format specifier.
	/// </summary>
	private JTextField __inputJTextField = null;

	/// <summary>
	/// Choices for the list of formatter types.
	/// </summary>
	private SimpleJComboBox __formatterTypeJComboBox = null;

	/// <summary>
	/// Choices for the list of format specifiers.
	/// </summary>
	private SimpleJComboBox __specifierJComboBox = null;

	/// <summary>
	/// Default formatter type for use with blank formatter type choice.
	/// </summary>
	private DateTimeFormatterType __defaultFormatter = DateTimeFormatterType.C;

	/// <summary>
	/// Indicate whether format specifier choices should be shown for output (true) or parsing (false).
	/// </summary>
	private bool __forOutput = false;

	/// <summary>
	/// Indicate whether format specifier choices should include properties like ${YearTypeYear}, used only for output.
	/// </summary>
	private bool __includeProps = false;

	/// <summary>
	/// Control constructor. </summary>
	/// <param name="width"> width of the JTextField to be included in the control (or -1) to not specify. </param>
	/// <param name="includeFormatterType"> if true, include a choice of the supported formatter types with "C", "ISO", etc.; if
	/// false the default is C </param>
	/// <param name="includeBlankFormatterType"> if true, include a blank choice in the formatter type; this is useful when
	/// the panel is being used for a command parameter and the formatter is optional </param>
	/// <param name="defaultFormatter"> if specified, this is the default formatter that is used when the choice is blank (default is
	/// DateTimeFormatterType.C) </param>
	/// <param name="forOutput"> if true, then include more specifiers used for formatting output; if false, include only choices that have
	/// been enabled for parsing </param>
	/// <param name="includeProps"> if true, include properties like ${YearTypeYear}, which are a more verbose way of specifying properties
	/// (only used for output) </param>
	public DateTimeFormatterSpecifiersJPanel(int width, bool includeFormatterType, bool includeBlankFormatterType, DateTimeFormatterType defaultFormatter, bool forOutput, bool includeProps)
	{
		if (defaultFormatter == null)
		{
			defaultFormatter = DateTimeFormatterType.C;
		}
		__forOutput = forOutput;
		__includeProps = includeProps;
		setLayout(new GridBagLayout());
		Insets insetsTLBR = new Insets(0,0,0,0);

		int y = 0;
		int x = 0;

		if (includeFormatterType)
		{
			__formatterTypeJComboBox = new SimpleJComboBox(false);
			__formatterTypeJComboBox.setToolTipText("Select the formatter type to use.");
			__formatterTypeJComboBox.setPrototypeDisplayValue("" + DateTimeFormatterType.ISO);
			IList<string> choicesList = new List<string>();
			if (includeBlankFormatterType)
			{
				choicesList.Add("");
			}
			choicesList.Add("" + DateTimeFormatterType.C);
			// TODO SAM 2012-04-10 Need to add other formatter types
			__formatterTypeJComboBox.setData(choicesList);
			// Don't select choice here.  Do it below so that event will trigger populating specifier choices
			__formatterTypeJComboBox.addItemListener(this);
			JGUIUtil.addComponent(this, __formatterTypeJComboBox, x++, y, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		}

		__specifierJComboBox = new SimpleJComboBox(false);
		__specifierJComboBox.setToolTipText("Selecting a specifier will insert at the cursor position in the format string.");
		__specifierJComboBox.setPrototypeDisplayValue(__hint + "WWWWWWWW"); // Biggest formatter name
		__specifierJComboBox.addItemListener(this);
		JGUIUtil.addComponent(this, __specifierJComboBox, x++, y, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		if (includeFormatterType)
		{
			__formatterTypeJComboBox.select(null);
			__formatterTypeJComboBox.select(0); // Do this here to trigger population of the format specifier choices
		}
		if (__specifierJComboBox.getItemCount() > 0)
		{
			__specifierJComboBox.select(0); // Now select the specifier corresponding to the formatter
		}

		JGUIUtil.addComponent(this, new JLabel(" => "), x++, y, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		if (width > 0)
		{
			__inputJTextField = new JTextField(width);
		}
		else
		{
			__inputJTextField = new JTextField();
		}
		__inputJTextField.setToolTipText("Enter a combination of literal strings and/or format specifiers from the list on the left.");
		// Make sure caret stays visible even when not in focus
		__inputJTextField.setCaretColor(Color.lightGray);
		__inputJTextField.getCaret().setVisible(true);
		__inputJTextField.getCaret().setSelectionVisible(true);
		JGUIUtil.addComponent(this, __inputJTextField, x, y, 2, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
	}

	/// <summary>
	/// Add a DocumentListener for the text field.
	/// </summary>
	public virtual void addDocumentListener(DocumentListener listener)
	{
		__inputJTextField.getDocument().addDocumentListener(listener);
	}

	/// <summary>
	/// Add an ItemListener for the formatter type combo box.  A listener for the format specifier combo box is not added
	/// because its selections result in document events, which can be listed to by calling addDocumentListener().
	/// </summary>
	public virtual void addFormatterTypeItemListener(ItemListener listener)
	{
		__formatterTypeJComboBox.addItemListener(listener);
	}

	/// <summary>
	/// Add a KeyListener for the text field.
	/// </summary>
	public virtual void addKeyListener(KeyListener listener)
	{
		__inputJTextField.addKeyListener(listener);
	}

	/// <summary>
	/// Return the DateTimeFormatterType that is in effect for the format string.  This can be used, for example, to
	/// prefix a format string to indicate the formatter type.  The value returned should not be null. </summary>
	/// <returns> the formatter type that is visible or in effect, depending on "visible" parameter </returns>
	/// <param name="onlyIfVisible"> if false and no formatter is shown in the choice, return null; if true and no formatter is shown,
	/// return the default </param>
	public virtual DateTimeFormatterType getDateTimeFormatterType(bool onlyIfVisible)
	{
		if (onlyIfVisible)
		{
			if ((__formatterTypeJComboBox == null) || __formatterTypeJComboBox.getSelected().Equals(""))
			{
				return null; // No formatter visible in interface
			}
			else
			{
				// Get formatter that corresponds to what is shown
				return DateTimeFormatterType.valueOfIgnoreCase(__formatterTypeJComboBox.getSelected());
			}
		}
		else
		{
			// Return the formatter that is in effect, whether visibly shown or not
			if ((__formatterTypeJComboBox == null) || __formatterTypeJComboBox.getSelected().Equals(""))
			{
				return DateTimeFormatterType.C; // Default when not visible
			}
			else
			{
				// Get formatter that corresponds to what is shown
				return DateTimeFormatterType.valueOfIgnoreCase(__formatterTypeJComboBox.getSelected());
			}
		}
	}

	/// <summary>
	/// Return the Document associated with the text field.
	/// </summary>
	public virtual Document getDocument()
	{
		return __inputJTextField.getDocument();
	}

	/// <summary>
	/// Return the selected formatter type (e.g., "C").
	/// </summary>
	public virtual string getSelectedFormatterType()
	{
		if (__formatterTypeJComboBox == null)
		{
			return "";
		}
		else
		{
			return __formatterTypeJComboBox.getSelected();
		}
	}

	/// <summary>
	/// Return the text in the text field and do not prepend the formatter type.
	/// </summary>
	public virtual string getText()
	{
		return getText(false,false);
	}

	/// <summary>
	/// Return the text in the text field. </summary>
	/// <param name="includeFormatterType"> if false, return the text field contents; if true and a formatter is known,
	/// prepend the formatter display name (e.g., "C:xxxx"). </param>
	/// <param name="onlyIfVisible"> if true, only include the formatter type prefix if the formatter is visible; if false,
	/// always include the formatter type </param>
	public virtual string getText(bool includeFormatterType, bool onlyIfVisible)
	{
		if (includeFormatterType)
		{
			// Get the formatter for what is visible
			DateTimeFormatterType t = getDateTimeFormatterType(onlyIfVisible);
			if (t == null)
			{
				return __inputJTextField.getText();
			}
			else
			{
				return "" + t + ":" + __inputJTextField.getText();
			}
		}
		else
		{
			return __inputJTextField.getText();
		}
	}

	/// <summary>
	/// Return the text field component, for example to allow tool tips to be set. </summary>
	/// <returns> the text field component. </returns>
	public virtual JTextField getTextField()
	{
		return __inputJTextField;
	}

	/// <summary>
	/// Respond to ItemEvents - user has selected from the list so insert into the cursor position in the
	/// text field. </summary>
	/// <param name="evt"> Item event due to list change, etc. </param>
	public virtual void itemStateChanged(ItemEvent evt)
	{
		object source = evt.getSource();
		// Only insert on select..
		if (evt.getStateChange() == ItemEvent.SELECTED)
		{
			if (source == __specifierJComboBox)
			{
				string selection = StringUtil.getToken(__specifierJComboBox.getSelected(), "-", 0, 0).Trim();
				if (!selection.Equals(__hint))
				{
					int pos = __inputJTextField.getCaretPosition();
					string text = __inputJTextField.getText();
					string newText = text.Substring(0,pos) + selection + text.Substring(pos);
					__inputJTextField.setText(newText);
					// Make sure caret stays visible even when not in focus
					__inputJTextField.getCaret().setVisible(true);
					__inputJTextField.getCaret().setSelectionVisible(true);
				}
			}
			else if ((__formatterTypeJComboBox != null) && (source == __formatterTypeJComboBox))
			{
				populateFormatSpecifiers();
			}
		}
	}

	/// <summary>
	/// Populate the format specifiers based on the formatter type.
	/// This does not select any of the items (should do that immediately after calling this method).
	/// </summary>
	private void populateFormatSpecifiers()
	{
		string selectedFormatterType = __formatterTypeJComboBox.getSelected();
		DateTimeFormatterType formatterType = null;
		try
		{
			formatterType = DateTimeFormatterType.valueOfIgnoreCase(selectedFormatterType);
		}
		catch (System.ArgumentException)
		{
			formatterType = null;
		}
		IList<string> choicesList = null;
		// Because the choices get reset there is a chance that this will cause layout problems.  Consequently, it is
		// best to make sure that the hint takes up enough space that the choice width does not change when repopulated
		choicesList = new List<string>();
		if ((formatterType == null) && (__defaultFormatter != null))
		{
			formatterType = __defaultFormatter;
		}
		if (formatterType == null)
		{
			choicesList.Add(__hint);
		}
		else if (formatterType == DateTimeFormatterType.C)
		{
			choicesList.Add(__hint);
			((IList<string>)choicesList).AddRange(Arrays.asList(TimeUtil.getDateTimeFormatSpecifiers(true,__forOutput,__includeProps)));
		}
		__specifierJComboBox.setData(choicesList);
		int max = 20;
		if (choicesList.Count < 20)
		{
			max = choicesList.Count;
		}
		__specifierJComboBox.setMaximumRowCount(max);
	}

	/// <summary>
	/// Select the formatter type.  Select the empty string if formatterType=null.
	/// </summary>
	public virtual void selectFormatterType(DateTimeFormatterType formatterType)
	{
		if (formatterType == null)
		{
			__formatterTypeJComboBox.selectIgnoreCase("");
		}
		__formatterTypeJComboBox.selectIgnoreCase("" + formatterType);
	}

	/// <summary>
	/// Set the text in the text field. </summary>
	/// <param name="text"> text to set in the textfield </param>
	public virtual void setText(string text)
	{
		__inputJTextField.setText(text);
	}

	}

}