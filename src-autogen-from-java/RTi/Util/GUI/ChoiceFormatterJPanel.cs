using System.Collections.Generic;

// ChoiceFormatterJPanel - panel to provide simple editing capabilities to construct a string given a list of choices

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


	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Panel to provide simple editing capabilities to construct a string given a list of choices from which
	/// one or more selections can be made.  The panel exposes the getText() method from the JTextField so that
	/// the edited contents can be retrieved.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ChoiceFormatterJPanel extends javax.swing.JPanel implements java.awt.event.ItemListener
	public class ChoiceFormatterJPanel : JPanel, ItemListener
	{

	/// <summary>
	/// Hint to aid user.
	/// </summary>
	private string __hint = "-- Select Specifier --";

	/// <summary>
	/// Delimiter that separates choice from description (e.g., "Choice - Description...").
	/// </summary>
	private string __choiceDelim = null;

	/// <summary>
	/// Delimiter that separates selected choices (e.g., comma in "choice1,choice2").
	/// </summary>
	private string __insertDelim = null;

	/// <summary>
	/// Whether append is allowed.  If true, then append choices using the delimiter.  If false, overwrite the
	/// text field contents with choice.
	/// </summary>
	private bool __append = true;

	/// <summary>
	/// Text field containing the edited format specifier.
	/// </summary>
	private JTextField __inputJTextField = null;

	/// <summary>
	/// Choices for the list of format specifiers.
	/// </summary>
	private SimpleJComboBox __formatJComboBox = null;

	/// <summary>
	/// Control constructor. </summary>
	/// <param name="choices"> string choices that will be displayed to the user; choices can be simple strings or compound
	/// strings consisting of values and description (the "delim" parameter indices if a delimiter is used, for
	/// example as VALUE - DESCRIPTION) </param>
	/// <param name="choiceDelim"> delimiter that is used to separate data choices and description in the choices string;
	/// specify as null or blank if not used </param>
	/// <param name="tooltip"> tooltip for the choice component </param>
	/// <param name="hint"> string to display by default to guide the user like "Please select...", and which will
	/// be ignored as a valid choice </param>
	/// <param name="insertDelim"> delimiter to be inserted when transferring choices (e.g, if a comma is specified then
	/// a comma is automatically inserted when choices are selected; specify as null or blank if not used </param>
	/// <param name="width"> width of the JTextField to be included in the control (or -1) to not specify. </param>
	/// <param name="append"> if true, then choices are appended to the text field, separated by the delimited; if false,
	/// choices replace the contents of the text field </param>
	public ChoiceFormatterJPanel(IList<string> choices, string choiceDelim, string tooltip, string hint, string insertDelim, int width, bool append)
	{
		__hint = hint;
		__choiceDelim = choiceDelim;
		__insertDelim = insertDelim;
		__append = append;
		setLayout(new GridBagLayout());
		Insets insetsTLBR = new Insets(0,0,0,0);

		int y = 0;
		int x = 0;
		__formatJComboBox = new SimpleJComboBox(false);
		if ((!string.ReferenceEquals(hint, null)) && (hint.Length > 0))
		{
			choices.Insert(0,__hint);
		}
		__formatJComboBox.setData(choices);
		__formatJComboBox.addItemListener(this);
		JGUIUtil.addComponent(this, __formatJComboBox, x++, y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(this, new JLabel(" => "), x++, y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		if (width > 0)
		{
			__inputJTextField = new JTextField(width);
		}
		else
		{
			__inputJTextField = new JTextField();
		}
		if ((!string.ReferenceEquals(tooltip, null)) && (tooltip.Length > 0))
		{
			__formatJComboBox.setToolTipText(tooltip);
		}
		// Make sure caret stays visible even when not in focus, but use light gray so as to not
		// confuse with the component that is in focus
		__inputJTextField.setCaretColor(Color.lightGray);
		__inputJTextField.getCaret().setVisible(true);
		__inputJTextField.getCaret().setSelectionVisible(true);
		JGUIUtil.addComponent(this, __inputJTextField, x++, y, 2, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
	}

	/// <summary>
	/// Add a KeyListener for the text field.
	/// </summary>
	public virtual void addKeyListener(KeyListener listener)
	{
		__inputJTextField.addKeyListener(listener);
	}

	/// <summary>
	/// Add a DocumentListener for the text field.
	/// </summary>
	public virtual void addDocumentListener(DocumentListener listener)
	{
		__inputJTextField.getDocument().addDocumentListener(listener);
	}

	/// <summary>
	/// Return the Document associated with the text field.
	/// </summary>
	public virtual Document getDocument()
	{
		return __inputJTextField.getDocument();
	}

	/// <summary>
	/// Return the SimpleJComboBox used in the panel, useful for setting properties in calling code.
	/// </summary>
	public virtual SimpleJComboBox getSimpleJComboBox()
	{
		return __formatJComboBox;
	}

	/// <summary>
	/// Return the text in the text field.
	/// </summary>
	public virtual string getText()
	{
		return __inputJTextField.getText();
	}

	/// <summary>
	/// Respond to ItemEvents - user has selected from the list so insert into the cursor position in the
	/// text field. </summary>
	/// <param name="evt"> Item event due to list change, etc. </param>
	public virtual void itemStateChanged(ItemEvent evt)
	{
		// Only insert on select..
		if (evt.getStateChange() == ItemEvent.SELECTED)
		{
			string selection = __formatJComboBox.getSelected();
			if ((string.ReferenceEquals(__hint, null)) || (__hint.Length == 0) || !selection.Equals(__hint))
			{
				// Selection is not the hint so process the selection
				if ((!string.ReferenceEquals(__choiceDelim, null)) && (__choiceDelim.Length != 0))
				{
					// Further split out the selection
					selection = StringUtil.getToken(__formatJComboBox.getSelected(), __choiceDelim, 0, 0).Trim();
				}
				if (__append)
				{
					int pos = __inputJTextField.getCaretPosition();
					string text = __inputJTextField.getText();
					string delim1 = "", delim2 = "";
					if ((!string.ReferenceEquals(__insertDelim, null)) && !__insertDelim.Equals(""))
					{
						if (pos == 0)
						{
							// Inserting at the start
							if ((text.Length > 0) && (text[0] != ','))
							{
								delim2 = ",";
							}
						}
						else if (pos == text.Length)
						{
							// Inserting at the end
							if ((text[pos - 1] != ','))
							{
								delim1 = ",";
							}
						}
						else
						{
							// Inserting in the middle
							if ((text[pos - 1] != ','))
							{
								delim1 = ",";
							}
							if ((text[pos] != ','))
							{
								delim2 = ",";
							}
						}
					}
					string newText = text.Substring(0,pos) + delim1 + selection + delim2 + text.Substring(pos);
					__inputJTextField.setText(newText);
					// Make sure caret stays visible even when not in focus
					__inputJTextField.getCaret().setVisible(true);
					__inputJTextField.getCaret().setSelectionVisible(true);
				}
				else
				{
					// Just transfer the value to the text field.
					__inputJTextField.setText(selection);
				}
			}
		}
	}

	/// <summary>
	/// Set the components enabled state. </summary>
	/// <param name="enabled"> whether or not the components should be enabled </param>
	public virtual void setEnabled(bool enabled)
	{
		__inputJTextField.setEnabled(enabled);
		__formatJComboBox.setEnabled(enabled);
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