using System;

// SearchDialog - dialog to search a JTextComponent, similar to standard search tools

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

// ----------------------------------------------------------------------------
// SearchDialog - dialog to search a JTextComponent, similar to standard search
//			tools
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History: 
//
// 29 Mar 2001	Steven A. Malers, RTi	Copy FindInListDialog and modify.
// 16 May 2001	SAM, RTi		Change to highlight found text.
// 2001-11-19	SAM, RTi		Change default to case-insensitive
//					search and add a "Match case" checkbox.
//					Change "Next" button to "Find Next".
//					Add "Reset" button.
// ============================================================================
// 2002-10-24	SAM, RTi		Copy SearchDialog to this class and
//					update to use Swing.
// 2003-06-02	J. Thomas Sapienza, RTi	Corrected code so that text selection
//					now works.
// 2003-10-02	JTS, RTi		* Added code for wrapping around in the
//					  text area during a search.
//					* Added code so that if nothing is found
//					  during a search, any previously-
//					  selected text is deselected.
// 2005-11-16	JTS, RTi		* Added a constructor that will accept
//					  a JDialog parent.
//					* Eliminated the unused member variable
//					  _parent.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The SearchJDialog searches a JTextComponent and positions the cursor at found
	/// text.
	/// </summary>
	public class SearchJDialog : JDialog, ActionListener, KeyListener, WindowListener
	{

	private JTextField _find_JTextField; // text response from user
	private JTextComponent _search_JTextComponent; // Original JTextComponent to
							// search.
	private string _text; // Text from JTextArea to search
	private int _last_find_pos = -1; // Position of last find.
	private int __searchCount = 0;
	private JCheckBox _case_JCheckBox = null; // Check box to match case
	private JCheckBox __wrapAroundJCheckBox = null;

	/// <summary>
	/// SearchJDialog constructor </summary>
	/// <param name="parent"> class instantiating this class. </param>
	/// <param name="text_component"> JTextComponent to be searched. </param>
	/// <param name="title"> Dialog title. </param>
	public SearchJDialog(JFrame parent, JTextComponent text_component, string title) : base(parent, true)
	{
		initialize(text_component, title);
	}

	/// <summary>
	/// SearchJDialog constructor </summary>
	/// <param name="parent"> class instantiating this class. </param>
	/// <param name="text_component"> JTextComponent to be searched. </param>
	/// <param name="title"> Dialog title. </param>
	public SearchJDialog(JDialog parent, JTextComponent text_component, string title) : base(parent, true)
	{
		initialize(text_component, title);
	}

	/// <summary>
	/// Responds to ActionEvents. </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();

		if (command.Equals("Cancel"))
		{
			cancelClicked();
		}
		else if (command.Equals("Find Next"))
		{
			__searchCount = 0;
			search();
		}
		else if (command.Equals("Reset"))
		{
			// Reset for a new search...
			_last_find_pos = -1;
			_search_JTextComponent.setCaretPosition(0);
			_search_JTextComponent.select(0, 0);
		}
		command = null;
	}

	/// <summary>
	/// Close the dialog.
	/// </summary>
	private void cancelClicked()
	{
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Clean up before garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~SearchJDialog()
	{
		_find_JTextField = null;
		_search_JTextComponent = null;
		_text = null;
		_case_JCheckBox = null;
		__wrapAroundJCheckBox = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Instantiates the dialog components. </summary>
	/// <param name="text_area"> JTextComponent to search. </param>
	/// <param name="title"> JDialog title </param>
	private void initialize(JTextComponent text_area, string title)
	{
		_search_JTextComponent = text_area;
		if (text_area != null)
		{
			_text = _search_JTextComponent.getText();
		}
		if ((!string.ReferenceEquals(title, null)) && (title.Length > 0))
		{
			setTitle(title);
		}
		else
		{
			setTitle("Find Text");
		}
		setModal(false);

		addWindowListener(this);

		// Main panel...

			Insets insetsTLBR = new Insets(1,2,1,2);
		JPanel main_JPanel = new JPanel();
		main_JPanel.setLayout(new GridBagLayout());
		getContentPane().add("North", main_JPanel);
		int y = 0;

		// Main contents...

			JGUIUtil.addComponent(main_JPanel, new JLabel("Search for:"), 0, y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		_find_JTextField = new JTextField(40);
			JGUIUtil.addComponent(main_JPanel, _find_JTextField, 1, y, 6, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);
		_find_JTextField.addKeyListener(this);

		_case_JCheckBox = new JCheckBox("Match case", false);
			JGUIUtil.addComponent(main_JPanel, _case_JCheckBox, 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		__wrapAroundJCheckBox = new JCheckBox("Wrap around", false);
		JGUIUtil.addComponent(main_JPanel, __wrapAroundJCheckBox, 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// South Panel: North
		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
			JGUIUtil.addComponent(main_JPanel, button_JPanel, 0, ++y, 7, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

		button_JPanel.add(new SimpleJButton("Find Next", "Find Next", this));
		button_JPanel.add(new SimpleJButton("Reset", "Reset", this));
		button_JPanel.add(new SimpleJButton("Cancel", "Cancel", this));

		setResizable(true);
			pack();
			JGUIUtil.center(this);
			base.setVisible(true);
		setResizable(false);

			insetsTLBR = null;
		main_JPanel = null;
		button_JPanel = null;
	}

	/// <summary>
	/// Respond to KeyEvents.
	/// </summary>
	public virtual void keyPressed(KeyEvent @event)
	{
		if (@event.getKeyCode() == KeyEvent.VK_ENTER)
		{
			__searchCount = 0;
			search();
		}
	}

	public virtual void keyReleased(KeyEvent @event)
	{
	}

	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Do the search.
	/// </summary>
	private void search()
	{
		string search_text = _find_JTextField.getText();
		int length = search_text.Length;
		if (length == 0)
		{
			return;
		}
		int pos = -1;
		if (_case_JCheckBox.isSelected())
		{
			// Match the case...
			pos = _text.IndexOf(search_text, (_last_find_pos + 1), StringComparison.Ordinal);
		}
		else
		{ // Case-independent...
			pos = StringUtil.indexOfIgnoreCase(_text, search_text, (_last_find_pos + 1));
		}
		if (pos >= 0)
		{
			// set the selected text color to white
			_search_JTextComponent.setSelectedTextColor(new Color(255, 255, 255));
			// set the selection color as the default system selection
			// color
			_search_JTextComponent.setSelectionColor(UIManager.getColor("textHighlight"));
			// this makes the selection visible
			_search_JTextComponent.getCaret().setSelectionVisible(true);
			_search_JTextComponent.select(pos, (pos + length));
			// SAMX does not seem to be selecting with color???
			//Message.printStatus ( 1, "", "Selection color is " +
			//_search_JTextComponent.getSelectionColor() );
			_last_find_pos = pos;
		}
		else if (__wrapAroundJCheckBox.isSelected())
		{
			_last_find_pos = -1;
			__searchCount++;
			if (__searchCount > 1)
			{
				// deselect whatever is currently selected, because 
				// nothing matching was found
				_search_JTextComponent.getCaret().setSelectionVisible(false);
				return;
			}
			else
			{
				search();
			}
		}
		else
		{
			// deselect whatever is currently selected, because nothing
			// matching was found
			_search_JTextComponent.getCaret().setSelectionVisible(false);
		}

		search_text = null;
	}

	/// <summary>
	/// Responds to WindowEvents. </summary>
	/// <param name="event"> WindowEvent object. </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		cancelClicked();
	}

	public virtual void windowActivated(WindowEvent evt)
	{
		;
	}
	public virtual void windowClosed(WindowEvent evt)
	{
		;
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

	}

}