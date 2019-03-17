using System.Collections.Generic;

// JComboBoxResponseJDialog - provides a pop-up dialog allowing user to enter a text response

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
// JComboBoxResponseJDialog - provides a pop-up dialog allowing user to enter
//	a text response.
// ----------------------------------------------------------------------------
//  Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History: 
// 2004-02-23	J. Thomas Sapienza, RTi	Initial version adapted from 
//					TextResponseJDialog.
// 2005-04-26	JTS, RTi		Added finalize().
// 2005-06-13	JTS, RTi		* The combo box can now be made editable
//					* Combo box responds to ENTER to submit
//					  a value.
// 2005-08-10	JTS, RTi		* Deprecated some constructors.
//					* Allowed the combo box maximum row 
//					  count to be specified.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Class that provides a dialog from which the user can select a response in a combo box.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class JComboBoxResponseJDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class JComboBoxResponseJDialog : JDialog, ActionListener, KeyListener, WindowListener
	{

	private bool __editable = false;

	/// <summary>
	/// The mode in which the class is instantiated.
	/// </summary>
	private int __mode;

	/// <summary>
	/// The value returned when one of the buttons was pressed.
	/// </summary>
	private int __response;

	/// <summary>
	/// Dialog buttons.
	/// </summary>
	private JButton __cancelButton, __okButton;

	/// <summary>
	/// The combo box from which values are selected on the dialog.
	/// </summary>
	private SimpleJComboBox __comboBox;

	/// <summary>
	/// The title of the string.
	/// </summary>
	private static string __frameTitle;

	/// <summary>
	/// JComboBoxResponseJDialog constructor </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="choices"> the choices to populate the combo box with. </param>
	/// <param name="mode"> mode in which this gui is to be used(i.e., OK, OK | CANCEL)
	/// process different types of yes responses from the calling form. </param>
	public JComboBoxResponseJDialog(JFrame parent, string title, string label, IList<string> choices, int mode) : this(parent, title, label, choices, mode, false)
	{
	}

	/// <summary>
	/// JComboBoxResponseJDialog constructor </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="choices"> the choices to populate the combo box with. </param>
	/// <param name="mode"> mode in which this gui is to be used(i.e., OK, OK | CANCEL)
	/// process different types of yes reponses from the calling form. </param>
	/// <param name="editable"> whether the combo box is editable or not. </param>
	public JComboBoxResponseJDialog(JFrame parent, string title, string label, IList<string> choices, int mode, bool editable) : this(parent, title, label, choices, mode, editable, -1)
	{
	}

	/// <summary>
	/// JComboBoxResponseJDialog constructor </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="choices"> the choices to populate the combo box with. </param>
	/// <param name="mode"> mode in which this gui is to be used(i.e., OK, OK | CANCEL)
	/// process different types of yes reponses from the calling form. </param>
	/// <param name="editable"> whether the combo box is editable or not. </param>
	/// <param name="numRowsVisible"> the number of rows in the JComboBox to ensure are visible
	/// when the user clicks the combo box to select something.  If less than or 
	/// equal to 0, will not be considered and the default will be used. </param>
	public JComboBoxResponseJDialog(JFrame parent, string title, string label, IList<string> choices, int mode, bool editable, int numRowsVisible) : base(parent, true)
	{
		__editable = editable;
		initialize(parent, title, label, choices, mode, numRowsVisible);
	}

	/// <summary>
	/// Responds to ActionEvents </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string s = @event.getActionCommand();
		if (s.Equals("Cancel"))
		{
			__response = ResponseJDialog.CANCEL;
		}
		else if (s.Equals("OK"))
		{
			__response = ResponseJDialog.OK;
		}
		response();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JComboBoxResponseJDialog()
	{
		__cancelButton = null;
		__okButton = null;
		__comboBox = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Instantiates the GUI components </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="choices"> choices to populate the combo box with. </param>
	/// <param name="mode"> mode in which this gui is to be used(i.e., OK, OK | CANCEL)
	/// process different types of yes reponses from the calling form. </param>
	/// <param name="numRowsVisible"> the number of rows in the JComboBox to ensure are visible
	/// when the user clicks the combo box to select something.  If less than or 
	/// equal to 0, will not be considered and the default will be used. </param>
	private void initialize(JFrame parent, string title, string label, IList<string> choices, int mode, int numRowsVisible)
	{
		__mode = mode;

		addWindowListener(this);

			// North Panel
			JPanel north_Panel = new JPanel();

		// Split the text based on the new-line delimiter(we use \n, not the
		// platform's separator!

		IList<string> vec = StringUtil.breakStringList(label, "\n", 0);

		if (vec != null)
		{
			// Add each string...
			for (int i = 0; i < vec.Count; i++)
			{
					north_Panel.add(new JLabel("    " + vec[i] + "     "));
			}
		}
		__comboBox = new SimpleJComboBox(choices, __editable);
		if (numRowsVisible > 0)
		{
			__comboBox.setMaximumRowCount(numRowsVisible);
		}
		if (__editable)
		{
			__comboBox.addTextFieldKeyListener(this);
		}
		__comboBox.select(0);
			north_Panel.add(__comboBox);

			north_Panel.setLayout(new GridLayout(vec.Count + 1, 1));
			getContentPane().add("North", north_Panel);

		// Now add the buttons...

			// South Panel
			JPanel south_Panel = new JPanel();
			south_Panel.setLayout(new BorderLayout());
			getContentPane().add("South", south_Panel);

			// South Panel: North
			JPanel southNorth_Panel = new JPanel();
			southNorth_Panel.setLayout(new FlowLayout(FlowLayout.CENTER));
			south_Panel.add("North", southNorth_Panel);

		if ((__mode & ResponseJDialog.CANCEL) != 0)
		{
			__cancelButton = new JButton("Cancel");
			__cancelButton.addActionListener(this);
		}

		if ((__mode & ResponseJDialog.OK) != 0)
		{
				__okButton = new JButton("OK");
			__okButton.addActionListener(this);
		}

		// show the appropriate buttons depending upon
		// the selected mode.
		if ((__mode & ResponseJDialog.OK) != 0)
		{
				southNorth_Panel.add(__okButton);
		}
		if ((__mode & ResponseJDialog.CANCEL) != 0)
		{
				southNorth_Panel.add(__cancelButton);
		}

			// frame settings
		if (!string.ReferenceEquals(title, null))
		{
			setTitle(title);
		}
		else if (!string.ReferenceEquals(__frameTitle, null))
		{
			setTitle(__frameTitle);
		}
		// Dialogs do no need to be resizable...
		setResizable(false);
			pack();
			JGUIUtil.center(this);
			base.setVisible(true);
	}

	/// <summary>
	/// If the combo box is editable and ENTER is pressed in it, this makes the 
	/// GUI respond as if OK were pressed. </summary>
	/// <param name="event"> the KeyEvent that happened. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		if (@event.getKeyCode() == KeyEvent.VK_ENTER)
		{
			__response = ResponseJDialog.OK;
			response();
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
	}


	/// <summary>
	/// Return the user response. </summary>
	/// <returns> the dialog response string. </returns>
	public virtual string response()
	{
		setVisible(false);
		dispose();
		if (__response == ResponseJDialog.CANCEL)
		{
			return null;
		}
		else
		{
			return __comboBox.getSelected();
		}
	}

	/// <summary>
	/// This function sets the JFrame Title variable that is used
	/// for all instances of this class. </summary>
	/// <param name="title"> Frame title </param>
	public static void setFrameTitle(string title)
	{
		if (!string.ReferenceEquals(title, null))
		{
			__frameTitle = title;
		}
	}

	/// <summary>
	/// Responds to WindowEvents. </summary>
	/// <param name="event"> WindowEvent object </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		__response = ResponseJDialog.CANCEL;
		response();
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowActivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowClosed(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeactivated(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowDeiconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowIconified(WindowEvent @event)
	{
	}

	/// <summary>
	/// Does nothing.
	/// </summary>
	public virtual void windowOpened(WindowEvent @event)
	{
	}

	}

}