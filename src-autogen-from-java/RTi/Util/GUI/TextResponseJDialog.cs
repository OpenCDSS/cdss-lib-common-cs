using System.Collections.Generic;

// TextResponseJDialog - provides a pop-up dialog allowing user to enter a text response

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
// TextResponseJDialog - provides a pop-up dialog allowing user to enter
//	a text response.
// ----------------------------------------------------------------------------
//  Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
//  Notes:
//	(1)This GUI provides a Dialog which expects
//	a user reponse.
//	(2)flags may be passed through the constructor which
//	determine what sort of dialog will be visible. The
//	flags supported are: OK, OK_CANCEL
//	(3)This object should be instantiated as follows:
//	String x = new TextResponseJDialog( Frame parent, 
//		String label, int flag ).response()
//	where processing is halted until a reponse occures.
//	(4)The user response is returned through the response()
//	function.
//	(5)While the user is able to specify the mode in which the GUI
//	is initially created, the only way to know if the user clicked on
//	"Cancel" is if the response in (4) is null.  Otherwise, the response
//	will contain the text contained in the text field.
// ----------------------------------------------------------------------------
// History: 
//
// 2003-05-11	SAM, RTi		Copy TextResponseDialog and update for
//					Swing.
// 2004-05-03	J. Thomas Sapienza, RTi	Added constructor that allows specifying
//					the text to initially appear in the
//					JTextField.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using StringUtil = RTi.Util.String.StringUtil;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TextResponseJDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class TextResponseJDialog : JDialog, ActionListener, KeyListener, WindowListener
	{

	private JButton _cancel_Button, _ok_Button; // Ok Button
	private JTextField _textResponse; // text response from user
	private static string _frame_title; // Frame Title String
	private int _mode, _response; // button press

	/// <summary>
	/// TextResponseJDialog constructor </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="label"> Label to display in the GUI. </param>
	public TextResponseJDialog(JFrame parent, string label) : base(parent, true)
	{ // Call the full version with no title and ok Button
		initialize(parent, null, label, ResponseJDialog.OK, "");
	}

	/// <summary>
	/// TextResponseJDialog constructor </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="mode"> mode in which this gui is to be used (i.e., OK, OK_CANCEL)
	/// process different types of yes reponses from the calling form. </param>
	public TextResponseJDialog(JFrame parent, string label, int mode) : base(parent, true)
	{ // Call the full version with no title...
		initialize(parent, null, label, mode, "");
	}

	/// <summary>
	/// TextResponseJDialog constructor </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="mode"> mode in which this gui is to be used (i.e., OK, OK_CANCEL)
	/// process different types of yes reponses from the calling form. </param>
	public TextResponseJDialog(JFrame parent, string title, string label, int mode) : base(parent, true)
	{
		initialize(parent, title, label, mode, "");
	}

	/// <summary>
	/// TextResponseJDialog constructor </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="text"> the text to initially put in the text field in the dialog. </param>
	/// <param name="mode"> mode in which this gui is to be used (i.e., OK, OK_CANCEL)
	/// process different types of yes reponses from the calling form. </param>
	public TextResponseJDialog(JFrame parent, string title, string label, string text, int mode) : base(parent, true)
	{
		initialize(parent, title, label, mode, text);
	}

	/// <summary>
	/// Responds to ActionEvents </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string s = @event.getActionCommand();
		if (s.Equals("Cancel"))
		{
			_response = ResponseJDialog.CANCEL;
		}
		else if (s.Equals("OK"))
		{
			_response = ResponseJDialog.OK;
		}
		response();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~TextResponseJDialog()
	{
		_cancel_Button = null;
		_ok_Button = null;
		_textResponse = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Instantiates the GUI components </summary>
	/// <param name="parent"> JFrame class instantiating this class. </param>
	/// <param name="title"> Dialog title </param>
	/// <param name="label"> Label to display in the GUI. </param>
	/// <param name="mode"> mode in which this gui is to be used (i.e., OK, OK_CANCEL)
	/// process different types of yes reponses from the calling form. </param>
	/// <param name="text"> the text to seed the textfield with. </param>
	private void initialize(JFrame parent, string title, string label, int mode, string text)
	{
		_mode = mode;

		addWindowListener(this);

			// North Panel
			JPanel north_Panel = new JPanel();

		// Split the text based on the new-line delimiter (we use \n, not the
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
		_textResponse = new JTextField(20);
		_textResponse.addKeyListener(this);
			north_Panel.add(_textResponse);
		_textResponse.setText(text);

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

		if ((_mode & ResponseJDialog.CANCEL) != 0)
		{
			_cancel_Button = new JButton("Cancel");
			_cancel_Button.addActionListener(this);
		}

		if ((_mode & ResponseJDialog.OK) != 0)
		{
				_ok_Button = new JButton("OK");
			_ok_Button.addActionListener(this);
		}

		// show the appropriate buttons depending upon
		// the selected mode.
		if ((_mode & ResponseJDialog.OK) != 0)
		{
				southNorth_Panel.add(_ok_Button);
		}
		if ((_mode & ResponseJDialog.CANCEL) != 0)
		{
				southNorth_Panel.add(_cancel_Button);
		}

			// frame settings
		if (!string.ReferenceEquals(title, null))
		{
			setTitle(title);
		}
		else if (!string.ReferenceEquals(_frame_title, null))
		{
			setTitle(_frame_title);
		}
		// Dialogs do no need to be resizable...
		setResizable(false);
			pack();
			JGUIUtil.center(this);
			base.setVisible(true);
	}

	/// <summary>
	/// Respond to KeyEvents.
	/// </summary>
	public virtual void keyPressed(KeyEvent @event)
	{
		int code = @event.getKeyCode();

		if (code == KeyEvent.VK_ENTER)
		{
			// Enter key has same effect as "OK" button...
			_response = ResponseJDialog.OK;
			response();
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
	/// Return the user response. </summary>
	/// <returns> the dialog response string. </returns>
	public virtual string response()
	{
		setVisible(false);
		dispose();
		if (_response == ResponseJDialog.CANCEL)
		{
			return null;
		}
		else
		{
			return _textResponse.getText();
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
			_frame_title = title;
		}
	}

	/// <summary>
	/// Responds to WindowEvents. </summary>
	/// <param name="event"> WindowEvent object </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		_response = ResponseJDialog.CANCEL;
		response();
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