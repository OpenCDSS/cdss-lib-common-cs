using System;
using System.Collections.Generic;
using System.Text;

// DictionaryJDialog - dialog for editing a dictionary string ("property:value,property:value,...")

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



	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// <para>
	/// This class is a dialog for editing a dictionary string ("property:value,property:value,...").  The updated dictionary string is
	/// returned via a response() method call as per the following sample code:
	/// </para>
	/// <code><pre>
	/// String dict = "Prop1:Value1,Prop2:Value1";
	/// String dict2 = (new DictionaryJDialog(parentJFrame, true, dict, "Title", "Property", "Value",10)).response();
	/// if (dict2 == null) {
	///		// user canceled the dialog
	/// }
	/// else {
	///		// user made changes -- accept them.
	/// }
	/// </pre></code>
	/// <para>
	/// The dictionary is displayed as scrollable pairs of key and value pairs.
	/// </para>
	/// </summary>
	public class DictionaryJDialog : JDialog, ActionListener, KeyListener, MouseListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string BUTTON_INSERT = "Insert", BUTTON_ADD = "Add", BUTTON_REMOVE = "Remove", BUTTON_CANCEL = "Cancel", BUTTON_OK = "OK";

	/// <summary>
	/// Components to hold values from the TSIdent.
	/// </summary>
	private List<JTextField> keyTextFieldList = new List<JTextField>();
	private List<JTextField> valueTextFieldList = new List<JTextField>();

	/// <summary>
	/// Text field row (0-index) where 0=first (top) row in entry fields.
	/// This is used to let the Insert functionality know where to insert.
	/// </summary>
	private int selectedTextFieldRow = -1;

	/// <summary>
	/// Dialog buttons.
	/// </summary>
	private SimpleJButton insertButton = null, addButton = null, removeButton = null, cancelButton = null, okButton = null;

	/// <summary>
	/// Scroll panel that manages dictionary entries.
	/// </summary>
	private JPanel scrollPanel = null;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
	private string response_Renamed = null, dictString = null, title = null, keyLabel = null, valueLabel = null;

	private string[] notes = null;

	/// <summary>
	/// Requested dictionary size.
	/// </summary>
	private int initDictSize = 10;

	/// <summary>
	/// Number of rows in the dictionary (some may be blank).
	/// The initial number is displayed and then the Add button can add more.
	/// </summary>
	private int rowCount = 0;

	private bool error_wait = false; // Indicates if there is an error in input (true) from checkInput().

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame on which the dialog will appear.  This cannot
	/// be null.  If necessary, pass in a new JFrame. </param>
	/// <param name="modal"> whether the dialog is modal. </param>
	/// <param name="dictString"> the dictionary string to edit.  Can be null, in which case <code>response()
	/// </code> will return a new dictionary string filled with the values entered on the form. </param>
	/// <param name="title"> dialog title </param>
	/// <param name="notes"> information to display at the top of the dialog, to help explain the input </param>
	/// <param name="keyLabel"> label above keys </param>
	/// <param name="valueLabel"> label above values </param>
	/// <param name="initDictSize"> initial number of key/value pairs to show </param>
	public DictionaryJDialog(JFrame parent, bool modal, string dictString, string title, string[] notes, string keyLabel, string valueLabel, int initDictSize) : base(parent, modal)
	{

		this.dictString = dictString;
		this.title = title;
		if (notes == null)
		{
			notes = new string[0];
		}
		this.notes = notes;
		this.keyLabel = keyLabel;
		this.valueLabel = valueLabel;
		this.initDictSize = initDictSize;
		 setupUI();
	}

	/// <summary>
	/// Responds to ActionEvents. </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string s = @event.getActionCommand();

		if (s.Equals(this.BUTTON_ADD))
		{
			Insets insetsTLBR = new Insets(2,2,2,2);
			// Add a new row.  Rows are 1+ because the column names are in the first row
			++this.rowCount;
			JTextField ktf = new JTextField("",30);
			ktf.addMouseListener(this);
			this.keyTextFieldList.Add(ktf);
			JGUIUtil.addComponent(this.scrollPanel, ktf, 0, this.rowCount, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
			JTextField vtf = new JTextField("",40);
			this.valueTextFieldList.Add(vtf);
			JGUIUtil.addComponent(this.scrollPanel, vtf, 1, this.rowCount, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
			this.scrollPanel.revalidate();
		}
		else if (s.Equals(this.BUTTON_INSERT))
		{
			Insets insetsTLBR = new Insets(2,2,2,2);
			// Insert a new row before the row that was last selected.  Rows are 1+ because the column names are in the first row
			if (this.keyTextFieldList.Count == 0)
			{
				// Add at the end
				++this.rowCount;
				JTextField ktf = new JTextField("",30);
				this.keyTextFieldList.Add(ktf);
				JGUIUtil.addComponent(this.scrollPanel, ktf, 0, this.rowCount, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
				JTextField vtf = new JTextField("",40);
				this.valueTextFieldList.Add(vtf);
				JGUIUtil.addComponent(this.scrollPanel, vtf, 1, this.rowCount, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
			}
			else
			{
				// Insert before the selected
				if (this.selectedTextFieldRow < 0)
				{
					// Nothing previously selected so reset
					this.selectedTextFieldRow = 0;
				}
				// First loop through all the rows after the new and shift later in the grid bag
				for (int i = selectedTextFieldRow; i < this.keyTextFieldList.Count; i++)
				{
					// FIXME SAM 2014-03-02 TOO MUCH WORK - come back and fix this later - for not disable the Insert
				}
				// Now add the new text field
				JTextField ktf = new JTextField("",30);
				this.keyTextFieldList.Insert(this.selectedTextFieldRow,ktf);
				JGUIUtil.addComponent(this.scrollPanel, ktf, 0, (this.selectedTextFieldRow + 1), 1, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
				JTextField vtf = new JTextField("",40);
				this.valueTextFieldList.Insert(this.selectedTextFieldRow,vtf);
				JGUIUtil.addComponent(this.scrollPanel, vtf, 1, (this.selectedTextFieldRow + 1), 1, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
				++this.rowCount;
			}

			this.scrollPanel.revalidate();
		}
		else if (s.Equals(this.BUTTON_CANCEL))
		{
			response(false);
		}
		else if (s.Equals(this.BUTTON_OK))
		{
			checkInputAndCommit();
			if (!this.error_wait)
			{
				response(true);
			}
		}
	}

	public virtual void mouseClicked(MouseEvent e)
	{
		setSelectedTextField(e.getComponent());
	}

	public virtual void mouseEntered(MouseEvent e)
	{

	}

	public virtual void mouseExited(MouseEvent e)
	{

	}

	public virtual void mousePressed(MouseEvent e)
	{
		setSelectedTextField(e.getComponent());
	}

	public virtual void mouseReleased(MouseEvent e)
	{

	}

	/// <summary>
	/// Check the input.  If errors exist, warn the user and set the __error_wait flag
	/// to true.  This should be called before response() is allowed to complete.
	/// </summary>
	private void checkInputAndCommit()
	{
		// Previously show all input to user, even if in error, but check before saving
		this.error_wait = false;

		StringBuilder b = new StringBuilder();
		string chars = ":,\"";
		string message = "";
		// Get from the dialog...
		for (int i = 0; i < this.keyTextFieldList.Count; i++)
		{
			string key = this.keyTextFieldList[i].getText().Trim();
			string value = this.valueTextFieldList[i].getText().Trim();
			// Make sure that the key and value do not contain special characters :,"
			// TODO SAM 2013-09-08 For now see if can parse out intelligently when ${} surrounds property, as in ${TS:property},
			// but this is not a generic behavior and needs to be handled without hard-coding
			// Evaluate whether to implement:  It is OK in the value if the value is completely surrounded by single quotes 
			if (StringUtil.containsAny(key, chars, false))
			{
				if (!key.StartsWith("${", StringComparison.Ordinal) && !key.EndsWith("}", StringComparison.Ordinal))
				{
					message += "\n" + this.keyLabel + " contains special character(s) \"" + chars + "\".  Surround with '  ' to protect.";
				}
			}
			if (StringUtil.containsAny(value, chars, false))
			{
				//if ( (value.charAt(0) != '\'') && (value.charAt(value.length() - 1) != '\'') ) {
				if (!value.StartsWith("${", StringComparison.Ordinal) && !value.EndsWith("}", StringComparison.Ordinal))
				{
					message = "\n" + this.valueLabel + " contains special character(s) \"" + chars + "\".  Surround with '  ' to protect.";
				}
			}
			if (key.Length > 0)
			{
				if (b.Length > 0)
				{
					b.Append(",");
				}
				b.Append(key + ":" + value);
			}
		}
		if (message.Length > 0)
		{
			Message.printWarning(1, "", message);
			this.error_wait = true;
		}
		else
		{
			this.response_Renamed = b.ToString();
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyPressed(KeyEvent e)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyReleased(KeyEvent e)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyTyped(KeyEvent e)
	{
	}

	/// <summary>
	/// Return the user response and dispose the dialog. </summary>
	/// <returns> the dialog response.  If <code>null</code>, the user pressed Cancel. </returns>
	public virtual void response(bool ok)
	{
		setVisible(false);
		dispose();
		if (!ok)
		{
			this.response_Renamed = null;
		}
	}

	/// <summary>
	/// Return the user response and dispose the dialog. </summary>
	/// <returns> the dialog response.  If <code>null</code>, the user pressed Cancel. </returns>
	public virtual string response()
	{
		return this.response_Renamed;
	}

	// TODO SAM 2014-03-02 This will need more work if controls are added to delete or re-order text fields.
	/// <summary>
	/// Set the selected text field, which indicates which row has been clicked on.
	/// </summary>
	private void setSelectedTextField(Component c)
	{
		// Figure out which of the text fields were selected and save the index
		for (int i = 0; i < keyTextFieldList.Count; i++)
		{
			if (c == keyTextFieldList[i])
			{
				this.selectedTextFieldRow = i;
				return;
			}
		}
		for (int i = 0; i < valueTextFieldList.Count; i++)
		{
			if (c == valueTextFieldList[i])
			{
				this.selectedTextFieldRow = i;
				return;
			}
		}
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupUI()
	{
		if (!string.ReferenceEquals(this.title, null))
		{
			setTitle(this.title);
		}

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());
		getContentPane().add("Center", panel);

		Insets insetsTLBR = new Insets(2,2,2,2);

		// Display the notes
		int y = -1;
		for (int i = 0; i < this.notes.Length; i++)
		{
			JGUIUtil.addComponent(panel, new JLabel(this.notes[i]), 0, ++y, 2, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		}

		// Parse out the existing dictionary.
		string[] keyList = new string[0];
		string[] valueList = new string[0];
		if ((!string.ReferenceEquals(this.dictString, null)) && (this.dictString.Length > 0))
		{
			// Have an existing dictionary string so parse and use to populate the dialog
			string[] dictParts;
			if (dictString.IndexOf(",", StringComparison.Ordinal) < 0)
			{
				dictParts = new string[1];
				dictParts[0] = dictString;
			}
			else
			{
				dictParts = this.dictString.Split(",", true);
			}
			if (dictParts != null)
			{
				keyList = new string[dictParts.Length];
				valueList = new string[dictParts.Length];
				for (int i = 0; i < dictParts.Length; i++)
				{
					// Now split the part by :
					// It is possible that the dictionary entry value contains a protected ':' so have to split manually
					// For example, this is used with Property:${TS:property} to retrieve time series properties
					// or ${TS:property}:Property to set properties
					int colonPos = dictParts[i].IndexOf("}:", StringComparison.Ordinal);
					if (colonPos > 0)
					{
						// Have a ${property} property in the key
						++colonPos; // Increment one position since }: is 2 characters
					}
					else
					{
						// No ${property} in the key
						colonPos = dictParts[i].IndexOf(":", StringComparison.Ordinal);
					}
					if (colonPos >= 0)
					{
						  keyList[i] = dictParts[i].Substring(0,colonPos).Trim();
						  if (colonPos == (dictParts[i].Length - 1))
						  {
							  // Colon is at the end of the string
							valueList[i] = "";
						  }
						  else
						  {
							valueList[i] = dictParts[i].Substring(colonPos + 1).Trim();
						  }
					}
					else
					{
						keyList[i] = dictParts[i].Trim();
						valueList[i] = "";
					}
				}
			}
		}
		if (keyList.Length > initDictSize)
		{
			// Increase the initial dictionary size
			initDictSize = keyList.Length;
		}

		this.scrollPanel = new JPanel();
		// Don't set preferred size because it seems to mess up the scroll bars (visible but no "thumb")
		//this.scrollPanel.setPreferredSize(new Dimension(600,300));
		this.scrollPanel.setLayout(new GridBagLayout());
		JGUIUtil.addComponent(panel, new JScrollPane(this.scrollPanel,JScrollPane.VERTICAL_SCROLLBAR_ALWAYS,JScrollPane.HORIZONTAL_SCROLLBAR_ALWAYS), 0, ++y, 2, 1, 1.0, 1.0, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		int yScroll = -1;
		JGUIUtil.addComponent(this.scrollPanel, new JLabel(this.keyLabel), 0, ++yScroll, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		JGUIUtil.addComponent(this.scrollPanel, new JLabel(this.valueLabel), 1, yScroll, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
		this.keyTextFieldList = new List<JTextField>(initDictSize);
		this.valueTextFieldList = new List<JTextField>(initDictSize);
		// Add key value pairs
		for (int i = 0; i < this.initDictSize; i++)
		{
			string key = "";
			string value = "";
			if (i < keyList.Length)
			{
				key = keyList[i];
				value = valueList[i];
			}
			JTextField ktf = new JTextField(key,30);
			this.keyTextFieldList.Add(ktf);
			JGUIUtil.addComponent(this.scrollPanel, ktf, 0, ++yScroll, 1, 1, 1.00, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
			JTextField vtf = new JTextField(value,40);
			this.valueTextFieldList.Add(vtf);
			JGUIUtil.addComponent(this.scrollPanel, vtf, 1, yScroll, 1, 1, 1.0, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);
			++this.rowCount;
		}

		JPanel south = new JPanel();
		south.setLayout(new FlowLayout(FlowLayout.RIGHT));

		this.insertButton = new SimpleJButton(this.BUTTON_INSERT, this);
		this.insertButton.setToolTipText("Insert a new row before the row that is currenty selected.");
		this.insertButton.setEnabled(false);
		this.addButton = new SimpleJButton(this.BUTTON_ADD, this);
		this.addButton.setToolTipText("Add a new row at the bottom of the list.");
		this.removeButton = new SimpleJButton(this.BUTTON_REMOVE, this);
		this.removeButton.setToolTipText("Remove the row that is currenty selected.");
		this.removeButton.setEnabled(false);
		this.okButton = new SimpleJButton(this.BUTTON_OK, this);
		this.okButton.setToolTipText("Accept any changes that have been made.");
		this.cancelButton = new SimpleJButton(this.BUTTON_CANCEL, this);
		this.cancelButton.setToolTipText("Discard changes.");

		south.add(this.insertButton);
		south.add(this.addButton);
		south.add(this.removeButton);
		south.add(this.okButton);
		south.add(this.cancelButton);

		getContentPane().add("South", south);

		pack();
		// Set the window size.  Otherwise large numbers of items in the dictionary will cause the scrolled panel to
		// be bigger than the screen at startup in some cases
		setSize(650,400);
		setResizable(true);
		JGUIUtil.center(this);
		setVisible(true);
		JGUIUtil.center(this);
	}

	/// <summary>
	/// Respond to WindowEvents. </summary>
	/// <param name="event"> WindowEvent object. </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		this.response_Renamed = null;
		response(false);
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowActivated(WindowEvent evt)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosed(WindowEvent evt)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeactivated(WindowEvent evt)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeiconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowIconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowOpened(WindowEvent evt)
	{
	}

	}

}