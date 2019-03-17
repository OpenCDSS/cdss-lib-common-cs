using System;

// FindInJListJDialog - dialog to search and manipulate a JList

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
// FindInJListJDialog - dialog to search and manipulate a JList
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History: 
//
// 16 Mar 2001	Steven A. Malers, RTi	Copy TextResponseDialog and modify.
// 2002-03-01	SAM, RTi		Add to popup the ability to select the
//					first found item (previously could only
//					select all).
// 2002-11-12	SAM, RTi		Copy FindInListDialog and update to use
//					Swing.
// 2004-03-15	SAM, RTi		* Fix bug where not using a
//					  DefaultListModel was causing a class
//					  cast exception.
//					* Put the results in a JScrollPane.
//					* Add option to select all not found.
// 2004-07-26	J. Thomas Sapienza, RTi	Mouse events only trigger a popup
//					menu now when the left button is not 
//					pressed.
// 2005-08-03	JTS, RTi		* Added tool tips to the text field
//					  in which search terms are typed and
//					  the button at the bottom.
//					* Now when new searches are done, the 
//					  old results are cleared.
//					* The size has been adjusted so the 
//					  button at the bottom doesn't get cut
//					  off.
//					* OK button changed to Close to better
//					  represent what it does, and position
//					  changed slightly.
// 2005-08-15	JTS, RTi		Changed text from "Search For:" to a
//					more expressive phrase.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{




	/// <summary>
	/// The FindInJListJDialog is a dialog containing a list through which users can 
	/// search to find desired information.
	/// </summary>
	public class FindInJListJDialog : JDialog, ActionListener, KeyListener, MouseListener, WindowListener
	{
	private JTextField __find_JTextField; // text response from user
	private JList __original_JList; // Original List to search
	private SimpleJList __find_JList; // List containing found items
							// in the original list.
	/* SAMX not needed??
	private ListSelectionListener	__selection_listener;
							// Selection listener for the
							// original list.
	*/
	private JPopupMenu __find_JPopupMenu; // Popup to edit list.

	private string __GO_TO_ITEM = "Go To First (Selected) Found Item in " +
						"Original List";
	private string __SELECT_FIRST_ITEM = "Select First (Selected) Found " +
					"Item in Original List (deselect others)";
	private string __SELECT_ALL_FOUND_ITEMS = "Select All Found Items in Original"+
						" List (deselect others)";
	private string __SELECT_ALL_NOT_FOUND_ITEMS = "Select All NOT Found Items in Original"
						+ " List (deselect found items)";
	private int[] __find_index = null; // Positions in original List
							// that are found.

	/// <summary>
	/// FindInJListJDialog Constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="list"> JList to operate on. </param>
	/// <param name="title"> JDialog title. </param>
	public FindInJListJDialog(JFrame parent, JList list, string title) : this(parent, true, list, title)
	{
	}

	/// <summary>
	/// FindInJListJDialog Constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="modal"> If true, the dialog is modal.  If false, it is not. </param>
	/// <param name="list"> JList to operate on. </param>
	/// <param name="title"> JDialog title. </param>
	public FindInJListJDialog(JFrame parent, bool modal, JList list, string title) : base(parent, modal)
	{
		initialize(parent, list, title); //, null );
	}

	/// <summary>
	/// FindInJListJDialog Constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="list"> JList to operate on. </param>
	/// <param name="title"> JDialog title. </param>
	/// <param name="selection_listener"> An object that is listening to events on the original
	/// JList.  If supplied, then in addition to adjustments made to the JList, a
	/// ListSelectionEvent will be sent to indicate the adjustment.  This will allow the
	/// JList to be interpreted in container graphical interfaces.  Currently this is
	/// only implemented for the "select first" action. </param>
	/* TODO SAM 2007-06-22 Evaluate if needed
	public FindInJListJDialog (	JFrame parent, JList list, String title,
					ListSelectionListener selection_listener )
	{	super ( parent, true );
		initialize ( parent, list, title, selection_listener );
	}
	*/

	/// <summary>
	/// Responds to ActionEvents. </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();

		if (command.Equals("Close"))
		{
			okClicked();
		}
		else if (command.Equals(__GO_TO_ITEM))
		{
			if (__find_index != null)
			{
				if (JGUIUtil.selectedSize(__find_JList) == 0)
				{
					// Go to first item...
					__original_JList.ensureIndexIsVisible(__find_index[0]);
				}
				else
				{ // Go to first selected item...
					__original_JList.ensureIndexIsVisible(__find_index[JGUIUtil.selectedIndex(__find_JList,0)]);
				}
			}
		}
		else if (command.Equals(__SELECT_ALL_FOUND_ITEMS))
		{
			// Select in the original list all the found items...
			__original_JList.clearSelection();
			__original_JList.setSelectedIndices(__find_index);
		}
		else if (command.Equals(__SELECT_ALL_NOT_FOUND_ITEMS))
		{
			// Select in the original list all the not found items and
			// deselect the found items (useful for deleting not found
			// items)...
			int original_size = __original_JList.getModel().getSize();
			if ((original_size == 0) && (__find_index.Length > 0))
			{
				return;
			}
			// Initialize all to 1 (selected)...
			int selected_size = original_size - __find_index.Length;
			if (selected_size == 0)
			{
				return;
			}
			int[] selected_indices = new int[selected_size];
			// REVISIT 2004-03-15 Need to optimize code...
			// Do this the brute force way for now.
			int count = 0; // Count for selected.
			bool found = false;
			for (int i = 0; i < original_size; i++)
			{
				found = false;
				for (int j = 0; j < __find_index.Length; j++)
				{
					if (__find_index[j] == i)
					{
						// Don't want selected...
						found = true;
						break;
					}
				}
				if (!found)
				{
					// We want to select all non-matching rows...
					selected_indices[count++] = i;
				}
			}
			// Clear out the old list...
			__original_JList.clearSelection();
			// Now select what we thing should be selected...
			__original_JList.setSelectedIndices(selected_indices);
			selected_indices = null;
		}
		else if (command.Equals(__SELECT_FIRST_ITEM))
		{
			if (__find_index != null)
			{
				if (JGUIUtil.selectedSize(__find_JList) == 0)
				{
					// Go to first item...
					__original_JList.ensureIndexIsVisible(__find_index[0]);
					__original_JList.setSelectedIndex(__find_index[0]);
					/* SAMX not needed??
					if ( __selection_listener != null ) {
						__selection_listener.valueChanged (
						new ListSelectionEvent (
						__original_JList,
						__find_index[0], __find_index[0],true));
					}
					*/
				}
				else
				{ // Go to first selected item...
					__original_JList.ensureIndexIsVisible(__find_index[JGUIUtil.selectedIndex(__find_JList,0)]);
					int found_index = __find_index[JGUIUtil.selectedIndex(__find_JList,0)];
					__original_JList.setSelectedIndex(found_index);
					/* SAMX not needed??
					if ( __selection_listener != null ) {
						__selection_listener.valueChanged (
						new ListSelectionEvent (
						__original_JList,
						__found_index, __found_index,true));
					}
					*/
				}
			}
		}
	}

	/// <summary>
	/// Clean up before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~FindInJListJDialog()
	{
		__find_JTextField = null;
		__find_JList = null;
		__original_JList = null;
		__GO_TO_ITEM = null;
		__SELECT_FIRST_ITEM = null;
		__SELECT_ALL_FOUND_ITEMS = null;
		__SELECT_ALL_NOT_FOUND_ITEMS = null;
		__find_index = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Instantiates the components. </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="list"> JList that is being operated on. </param>
	/// <param name="title"> Dialog title. </param>
	//@param selection_listener ListSelectionListener to pass ListSelectionEvents to
	//for the list.
	private void initialize(JFrame parent, JList list, string title) //,
	{
					//ListSelectionListener selection_listener )
		__original_JList = list;
		//__selection_listener = selection_listener;
		if ((!string.ReferenceEquals(title, null)) && (title.Length > 0))
		{
			setTitle(title);
		}
		else
		{
			setTitle("Find Text in List");
		}

		addWindowListener(this);

		// Main panel...

			Insets insetsTLBR = new Insets(2,2,2,2);
		JPanel main_JPanel = new JPanel();
		main_JPanel.setLayout(new GridBagLayout());
		getContentPane().add("Center", main_JPanel);
		int y = 0;

		// Main contents...

			JGUIUtil.addComponent(main_JPanel, new JLabel("Search for rows containing:"), 0, y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		__find_JTextField = new JTextField(30);
		__find_JTextField.setToolTipText("<html>Type the text to search for and press Enter.<br>" + "Then right click on the list below for more options.</html>");
			JGUIUtil.addComponent(main_JPanel, __find_JTextField, 1, y, 6, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);
		__find_JTextField.addKeyListener(this);

			JGUIUtil.addComponent(main_JPanel, new JLabel("Search Results (found items):"), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		__find_JList = new SimpleJList();
		__find_JList.setToolTipText("Right click to see actions to perform on the original list.");
		__find_JList.setVisibleRowCount(10);
		__find_JList.setSelectionMode(ListSelectionModel.MULTIPLE_INTERVAL_SELECTION);
		// Initially a blank list.
		__find_JList.addKeyListener(this);
		__find_JList.addMouseListener(this); // For popup
			JGUIUtil.addComponent(main_JPanel, new JScrollPane(__find_JList), 0, ++y, 7, 1, 1, 1, insetsTLBR, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		// South Panel: North
		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
			JGUIUtil.addComponent(main_JPanel, button_JPanel, 0, ++y, 7, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);

		SimpleJButton Close = new SimpleJButton("Close", this);
		Close.setToolTipText("Closes the dialog window.");
		button_JPanel.add(Close);

		// Add the JPopupMenu...

		// Pop-up menu to manipulate the list...
		__find_JPopupMenu = new JPopupMenu("Search Actions");
		__find_JPopupMenu.add(new SimpleJMenuItem(__GO_TO_ITEM, this));
		__find_JPopupMenu.add(new SimpleJMenuItem(__SELECT_FIRST_ITEM,this));
		if (__original_JList.getSelectionMode() == ListSelectionModel.MULTIPLE_INTERVAL_SELECTION)
		{
			// Only makes sense if we can select more than one thing in the
			// original list...
			__find_JPopupMenu.add(new SimpleJMenuItem(__SELECT_ALL_FOUND_ITEMS,this));
			__find_JPopupMenu.add(new SimpleJMenuItem(__SELECT_ALL_NOT_FOUND_ITEMS,this));
		}
		setResizable(true);
			pack();
		setSize(getWidth(), getHeight() + 10);
			JGUIUtil.center(this);
			base.setVisible(true);
	}

	/// <summary>
	/// Respond to KeyEvents.  If enter is pressed, refreshes the dialog.
	/// </summary>
	public virtual void keyPressed(KeyEvent @event)
	{
		int code = @event.getKeyCode();

		if (code == KeyEvent.VK_ENTER)
		{
			refresh();
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyReleased(KeyEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void keyTyped(KeyEvent @event)
	{
		;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse pressed event.  Shows the popup menu if the popup menu trigger
	/// (right mouse button usually) was pressed.
	/// </summary>
	public virtual void mousePressed(MouseEvent @event)
	{
		if (__find_JList.getItemCount() > 0 && @event.getButton() != MouseEvent.BUTTON1)
		{
			Point pt = JGUIUtil.computeOptimalPosition(@event.getPoint(), @event.getComponent(), __find_JPopupMenu);
			__find_JPopupMenu.show(@event.getComponent(), pt.x, pt.y);
		}
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void mouseReleased(MouseEvent @event)
	{
	}

	/// <summary>
	/// Close the dialog.
	/// </summary>
	private void okClicked()
	{
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Refresh the list based on the current find string.
	/// </summary>
	private void refresh()
	{ // First clear the list...
		__find_JList.removeAll();
		// Now search the original list...
		if (__original_JList == null)
		{
			return;
		}
		int size = __original_JList.getModel().getSize();
		string item = null, item_up = null;
		string find_text = __find_JTextField.getText().Trim().ToUpper();
		int find_count = 0;
		// First cut at index...
		int[] find_index = new int[size];
		JGUIUtil.setWaitCursor(this, true);
		for (int i = 0; i < size; i++)
		{
			item = "" + __original_JList.getModel().getElementAt(i);
			item_up = item.ToUpper();
			if (item_up.IndexOf(find_text, StringComparison.Ordinal) >= 0)
			{
				((DefaultListModel) __find_JList.getModel()).addElement(item);
				find_index[find_count] = i;
				// Set selection to match original list...
				if (__original_JList.isSelectedIndex(i))
				{
					__find_JList.setSelectedIndex(find_count);
				}
				++find_count;
			}
		}
		// Now resize the find index to the final...
		__find_index = new int[find_count];
		for (int i = 0; i < find_count; i++)
		{
			__find_index[i] = find_index[i];
		}
		JGUIUtil.setWaitCursor(this, false);
		find_index = null;
		item = null;
		item_up = null;
		find_text = null;
	}

	/// <summary>
	/// Responds to WindowEvents.  Closes the window. </summary>
	/// <param name="event"> WindowEvent object. </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		okClicked();
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowActivated(WindowEvent evt)
	{
		;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosed(WindowEvent evt)
	{
		;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeactivated(WindowEvent evt)
	{
		;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeiconified(WindowEvent evt)
	{
		;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowIconified(WindowEvent evt)
	{
		;
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowOpened(WindowEvent evt)
	{
		;
	}

	} // end FindInJListJDialog

}