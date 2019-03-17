using System.Collections.Generic;

// CommandPropertyFormatterJPanel - panel to provide editing capabilities to construct a format specifier string

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

namespace RTi.Util.IO
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Panel to provide editing capabilities to construct a format specifier string, which includes
	/// specifiers like ${c:PropName} where the "c" indicates command scope and PropName is a parameter name.
	/// The control consists of an editable text field, an Insert button, and a JChoice with a list of
	/// available specifiers.  Use getText() to get the contents of the text field.
	/// @author sam
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class CommandPropertyFormatterJPanel extends javax.swing.JPanel implements java.awt.event.ItemListener
	public class CommandPropertyFormatterJPanel : JPanel, ItemListener
	{
		/// <summary>
		/// Hint to aid user.
		/// </summary>
		internal string __hint = "-- Select Specifier --";

		/// <summary>
		/// Text field containing the edited format specifier.
		/// </summary>
		internal JTextField __inputJTextField = null;

		/// <summary>
		/// Choices for the list of format specifiers.
		/// </summary>
		internal SimpleJComboBox __formatJComboBox = null;

		/// <summary>
		/// Control constructor. </summary>
		/// <param name="width"> width of the JTextField to be included in the control (or -1) to not specify. </param>
		/// <param name="choices"> specifier choices, used because there is not yet a standard bullet-proof way
		/// to generically request the choices, for example from a Command instance </param>
		public CommandPropertyFormatterJPanel(int width, IList<string> choices)
		{
			setLayout(new GridBagLayout());
			Insets insetsTLBR = new Insets(0,0,0,0);

			int y = 0;
			int x = 0;
			__formatJComboBox = new SimpleJComboBox(false);
			__formatJComboBox.setToolTipText("Selecting a specifier will insert at the cursor position in the text field.");
			IList<string> choicesList = choices;
			choicesList.Insert(0,__hint);
			__formatJComboBox.setData(choicesList);
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
			__inputJTextField.setToolTipText("Enter a combination of literal strings and/or format specifiers from the list on the left.");
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
				string selection = StringUtil.getToken(__formatJComboBox.getSelected(), "-", 0, 0).Trim();
				if (!selection.Equals(__hint))
				{
					int pos = __inputJTextField.getCaretPosition();
					string text = __inputJTextField.getText();
					string newText = text.Substring(0,pos) + selection + text.Substring(pos);
					__inputJTextField.setText(newText);
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