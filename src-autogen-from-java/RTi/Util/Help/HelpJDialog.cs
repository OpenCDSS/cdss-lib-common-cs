using System.Collections.Generic;
using System.Text;

// HelpJDialog - dialog to display initial help information with the potential to lead to the online help

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

//-----------------------------------------------------------------------------
// HelpJDialog - dialog to display initial help information with the
// 	potential to lead to the online help
//-----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// Notes:       (1)  When null is passed as the online help key, additional
//			information is not available and the "More help" 
//			button will not be displayed.  Otherwise, the 
//			"More help" button is enabled.
//		(2)  This doesn't implement the paging ability similar to that
//			of ReportGUI because this is intended for brief help.
//			Anything more should automatically use the 
//			URL.showHelpForKey 
//-----------------------------------------------------------------------------
// History:
// 25 Aug 1999	CEN, RTi		Created class.
// 01 Sep 1999	CEN, RTi		Changed name from PrelimHelpGUI to
//					HelpJDialog and extended from JDialog
//					rather than JFrame.  Added additional
//					javadoc.
// 2001-11-14	Steven A. Malers, RTi	Review javadoc.  Add finalize().  Remove
//					import *.  Verify that variables are set
//					to null when no longer used.  Use
//					GUIUtil instead of GUI.
// 2003-05-27	J. Thomas Sapienza, RTi	Made this class a window listener in 
//					order to remove the HelpJDialog$1.class
//					internal class.
//-----------------------------------------------------------------------------
// 2003-08-25	JTS, RTi		Initial Swing version.
//-----------------------------------------------------------------------------

namespace RTi.Util.Help
{



	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using GUIUtil = RTi.Util.GUI.GUIUtil;
	using PropList = RTi.Util.IO.PropList;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// The HelpJDialog displays a simple message and allows full help to be brought
	/// up, if the user wants more information.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class HelpJDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.WindowListener
	public class HelpJDialog : JDialog, ActionListener, WindowListener
	{

	private JTextArea _help_JTextArea;
	private JButton _help_JButton;
	private JButton _close_JButton;
	private PropList _props;
	private string _help_key = null;

	/// <summary>
	/// HelpJDialog constructor </summary>
	/// <param name="helpInfo"> list of String elements to display </param>
	/// <param name="props"> PropList object as described in the following table
	/// <table width = 80% cellpadding=2 cellspace=0 border=2>
	/// <tr>
	/// <td>Property</td>        <td>Description</td>      <td>Default</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>HelpKey</td>
	/// <td>Search key for help.</td>
	/// <td>Help button is disabled if not specified.</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>TotalWidth</td>
	/// <td>Width of dialog.</td>
	/// <td>600</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>TotalHeight</td>
	/// <td>Height of dialog.</td>
	/// <td>550</td>
	/// </tr>
	/// 
	/// <tr>
	/// <td>Title</td>
	/// <td>Title of dialog.</td>
	/// <td>"Help".</td>
	/// </tr>
	/// 
	/// </table> </param>
	public HelpJDialog(JFrame parent, IList<string> helpInfo, PropList props) : base(parent)
	{
		_props = props;
		setGUI();

		if (helpInfo != null && helpInfo.Count > 0)
		{
			StringBuilder contents = new StringBuilder();
			string newLine = System.getProperty("line.separator");

			int size = helpInfo.Count;
			for (int i = 0; i < size; i++)
			{
				contents.Append((string)helpInfo[i] + newLine);
				_help_JTextArea.setText(contents.ToString());
			}
			contents = null;
			newLine = null;
		}
	}

	/// <summary>
	/// Responds to ActionEvents, including the help and close button presses. </summary>
	/// <param name="ae"> ActionEvent. </param>
	public virtual void actionPerformed(ActionEvent ae)
	{
		object source = ae.getSource();
		if (source == _help_JButton)
		{
			URLHelp.showHelpForKey(_help_key);
		}
		if (source == _close_JButton)
		{
			closeWindow();
		}
		source = null;
	}

	/// <summary>
	/// Close the dialog.
	/// </summary>
	private void closeWindow()
	{
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~HelpJDialog()
	{
		_help_JTextArea = null;
		_props = null;
		_help_key = null;
		_close_JButton = null;
		_help_JButton = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Sets up the awt portion of the GUI.
	/// </summary>
	private void setGUI()
	{
		GridBagLayout gbl = new GridBagLayout();
		string propValue;
		int width, height;

		addWindowListener(this);
		/*
			public void windowClosing ( WindowEvent evt ) {
				closeWindow();
			}
		} );
	*/

		if (_props == null)
		{
			_props = new PropList("PrelimHelpGUIProps");
		}

		_help_key = _props.getValue("HelpKey");

		// Determine the width
		propValue = _props.getValue("TotalWidth");
		if (string.ReferenceEquals(propValue, null))
		{
			width = 600;
		}
		else
		{
			width = StringUtil.atoi(propValue);
		}

		// Determine the height
		propValue = _props.getValue("TotalHeight");
		if (string.ReferenceEquals(propValue, null))
		{
			height = 550;
		}
		else
		{
			height = StringUtil.atoi(propValue);
		}

		propValue = _props.getValue("Title");
		if (!string.ReferenceEquals(propValue, null))
		{
			setTitle(propValue);
		}
		else
		{
			setTitle("Help");
		}

		// Center panel
		JPanel centerJPanel = new JPanel();
		centerJPanel.setLayout(gbl);
		getContentPane().add("Center", centerJPanel);

		_help_JTextArea = new JTextArea();
		_help_JTextArea.setEditable(false);

		// want to set to fixed width font
		Font oldFont = _help_JTextArea.getFont();
		Font newFont = null;
		if (oldFont == null)
		{
			newFont = new Font("Courier", Font.PLAIN, 11);
		}
		else
		{
			newFont = new Font("Courier", oldFont.getStyle(), oldFont.getSize());
		}
		_help_JTextArea.setFont(newFont);

		GUIUtil.addComponent(centerJPanel, new JScrollPane(_help_JTextArea), 0, 0, 1, 1, 1, 1, 10, 10, 10, 10, GridBagConstraints.BOTH, GridBagConstraints.WEST);

		// Bottom JPanel
		JPanel bottomJPanel = new JPanel();
		bottomJPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
		getContentPane().add("South", bottomJPanel);

		_close_JButton = new SimpleJButton("Close", this);
		bottomJPanel.add(_close_JButton);

		if (!string.ReferenceEquals(_help_key, null))
		{
			_help_JButton = new SimpleJButton("More help", this);
			bottomJPanel.add(_help_JButton);
		}

		pack();
		setSize(width, height);
		GUIUtil.center(this);

		setVisible(true);

		// Clean up...

		gbl = null;
		propValue = null;
		centerJPanel = null;
		bottomJPanel = null;
	}

	public virtual void windowActivated(WindowEvent e)
	{
	}
	public virtual void windowClosed(WindowEvent e)
	{
	}
	public virtual void windowClosing(WindowEvent e)
	{
		closeWindow();
	}
	public virtual void windowDeactivated(WindowEvent e)
	{
	}
	public virtual void windowDeiconified(WindowEvent e)
	{
	}
	public virtual void windowIconified(WindowEvent e)
	{
	}
	public virtual void windowOpened(WindowEvent e)
	{
	}

	}

}