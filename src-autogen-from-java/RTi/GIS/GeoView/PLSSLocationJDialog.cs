using System;

// PLSSLocationJDialog - a dialog for entering a PLSS location.

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
// PLSSLocationJDialog - a dialog for entering a PLSS location.
//-----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
//
// 2005-01-14	J. Thomas Sapienza, RTi	Initial version from 
//					HydroBase_GUI_BuildLocationQuery.
// 2005-02-01	JTS, RTi		The wildcard character can now be
//					changed.
// 2005-02-02	JTS, RTi		* Added support for half sections.
//					* Added Clear button.
// 2005-04-27	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{



	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using ResponseJDialog = RTi.Util.GUI.ResponseJDialog;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Class that assists various forms in building a location query.
	/// TODO (JTS - 2006-05-23) Example of using this.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class PLSSLocationJDialog extends javax.swing.JDialog implements java.awt.event.WindowListener, java.awt.event.KeyListener, java.awt.event.ActionListener
	public class PLSSLocationJDialog : JDialog, WindowListener, KeyListener, ActionListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_CANCEL = "Cancel", __BUTTON_CLEAR = "Clear", __BUTTON_OK = "OK";

	/// <summary>
	/// A string used to represent an unset value in the location.
	/// </summary>
	private string __wildcard = " ";

	/// <summary>
	/// The interaction the user had with the dialog to close it (either 
	/// ResponseJDialog.OK or ResponseJDialog.CANCEL).
	/// </summary>
	private int __response = -1;

	/// <summary>
	/// JTextField to hold the range.
	/// </summary>
	private JTextField __rangeJTextField;

	/// <summary>
	/// JTextField to hold the section.
	/// </summary>
	private JTextField __sectionJTextField;

	/// <summary>
	/// JTextField to hold the township.
	/// </summary>
	private JTextField __tsJTextField;

	// TODO SAM 2007-05-09 Evaluate how used
	/// <summary>
	/// The location object that stores the data initially passed into this dialog.
	/// </summary>
	//private PLSSLocation __location = null;

	/// <summary>
	/// Combo box for holding the half section.
	/// </summary>
	private SimpleJComboBox __halfSectionJComboBox;

	/// <summary>
	/// Combo box for holding pm.
	/// </summary>
	private SimpleJComboBox __pmJComboBox;

	/// <summary>
	/// Combo box for holding q10 (1/4 1/4 1/4 section).
	/// </summary>
	private SimpleJComboBox __q10JComboBox;

	/// <summary>
	/// Combo box for holding q40 (1/4 1/4 section).
	/// </summary>
	private SimpleJComboBox __q40JComboBox;

	/// <summary>
	/// Combo box for holding q160 (1/4 section).
	/// </summary>
	private SimpleJComboBox __q160JComboBox;

	/// <summary>
	/// Combo box for holding range.
	/// </summary>
	private SimpleJComboBox __rangeJComboBox;

	/// <summary>
	/// Combo box for holding township.
	/// </summary>
	private SimpleJComboBox __tsJComboBox;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent frame on which to build this modal dialog. </param>
	/// <param name="cdssFormat"> whether the location data are in CDSS format. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PLSSLocationJDialog(javax.swing.JFrame parent, boolean cdssFormat) throws Exception
	public PLSSLocationJDialog(JFrame parent, bool cdssFormat) : base(parent, true)
	{

		if (cdssFormat)
		{
			__wildcard = "*";
		}

		//__location = new PLSSLocation();

		setupGUI();

			base.setVisible(true);
	}

	/// <summary>
	/// Constructor.  Builds a dialog and populates the components with the data
	/// stored in the given location object. </summary>
	/// <param name="parent"> the parent frame on which to build this modal dialog. </param>
	/// <param name="location"> the location object to use for filling in data in the dialog. </param>
	/// <param name="cdssFormat"> whether the location data are in CDSS format. </param>
	/// <exception cref="Exception"> if an error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PLSSLocationJDialog(javax.swing.JFrame parent, PLSSLocation location, boolean cdssFormat) throws Exception
	public PLSSLocationJDialog(JFrame parent, PLSSLocation location, bool cdssFormat) : base(parent, true)
	{

		if (cdssFormat)
		{
			__wildcard = "*";
		}

		//__location = location;

		setupGUI();

		fillComponentData(location);

			base.setVisible(true);
	}

	/// <summary>
	/// Responds to action performed events. </summary>
	/// <param name="evt"> the action event that happened. </param>
	public virtual void actionPerformed(ActionEvent evt)
	{
		string command = evt.getActionCommand().Trim();

			if (command.Equals(__BUTTON_CANCEL))
			{
			__response = ResponseJDialog.CANCEL;
			response();
			}
		else if (command.Equals(__BUTTON_CLEAR))
		{
			clear();
		}
			else if (command.Equals(__BUTTON_OK))
			{
					__response = ResponseJDialog.OK;
			if (checkData())
			{
				response();
			}
			}
	}

	/// <summary>
	/// Builds a PLSSLocation object from the data in the dialog. </summary>
	/// <returns> a PLSSLocation object filled with data from the dialog. </returns>
	public virtual PLSSLocation buildLocation()
	{
		string routine = "PLSSLocationJDialog.buildLocation";

		PLSSLocation location = new PLSSLocation();

		try
		{
			string s = __pmJComboBox.getSelected();
			if (!s.Equals(__wildcard))
			{
				location.setPM(s);
			}

			s = __tsJTextField.getText();
			if (!s.Trim().Equals(""))
			{
				location.setTownship(StringUtil.atoi(s));
			}

			s = __tsJComboBox.getSelected();
			if (!s.Equals(__wildcard))
			{
				location.setTownshipDirection(s);
			}

			s = __rangeJTextField.getText();
			if (!s.Trim().Equals(""))
			{
				location.setRange(StringUtil.atoi(s));
			}

			s = __rangeJComboBox.getSelected();
			if (!s.Equals(__wildcard))
			{
				location.setRangeDirection(s);
			}

			s = __sectionJTextField.getText();
			if (!s.Trim().Equals(""))
			{
				location.setSection(StringUtil.atoi(s));
			}

			s = __halfSectionJComboBox.getSelected();
			if (!s.Equals(__wildcard))
			{
				location.setHalfSection(s);
			}

			s = __q160JComboBox.getSelected();
			if (!s.Equals(__wildcard))
			{
				location.setQ(s);
			}

			s = __q40JComboBox.getSelected();
			if (!s.Equals(__wildcard))
			{
				location.setQQ(s);
			}

			s = __q10JComboBox.getSelected();
			if (!s.Equals(__wildcard))
			{
				location.setQQQ(s);
			}
		}
		catch (Exception e)
		{
			// this should never happen since the inputs are constrained
			// (mostly) by the dialog
			Message.printWarning(1, routine, "Error creating location.");
			Message.printWarning(2, routine, e);
			return null;
		}

		return location;
	}

	/// <summary>
	/// Checks the data values to make sure they are valid. </summary>
	/// <returns> true if the data values are valid, false if they are not. </returns>
	private bool checkData()
	{
		string message = "";

		string s = __tsJTextField.getText();
		int i = StringUtil.atoi(s);
		if (!s.Trim().Equals(""))
		{
			if (i <= 0)
			{
				message += "Township (" + s + ") must be greater "
					+ "than 0\n";
			}
		}

		s = __rangeJTextField.getText();
		i = StringUtil.atoi(s);
		if (!s.Trim().Equals(""))
		{
			if (i <= 0)
			{
				message += "Range (" + s + ") must be greater than 0\n";
			}
		}

		s = __sectionJTextField.getText();
		i = StringUtil.atoi(s);
		if (!s.Trim().Equals(""))
		{
			if (i <= 0)
			{
				message += "Section (" + s + ") must be greater "
					+ "than 0\n";
			}
			else if (i > 36)
			{
				message += "Section (" + s + ") must be less than 37\n";
			}
		}

		if (message.Length == 0)
		{
			return true;
		}

		new ResponseJDialog(this, "Error in Location data", message, ResponseJDialog.OK);
		return false;
	}

	/// <summary>
	/// Clears any data that has been entered on the GUI, clearing all the text fields 
	/// and setting all the combo boxes to the wildcard value.
	/// </summary>
	private void clear()
	{
		__pmJComboBox.select(__wildcard);
		__tsJTextField.setText("");
		__tsJComboBox.select(__wildcard);
		__rangeJTextField.setText("");
		__rangeJComboBox.select(__wildcard);
		__sectionJTextField.setText("");
		__halfSectionJComboBox.select(__wildcard);
		__q160JComboBox.select(__wildcard);
		__q40JComboBox.select(__wildcard);
		__q10JComboBox.select(__wildcard);
	}

	/// <summary>
	/// Fills the components on the dialog with the location data stored in the 
	/// specified PLSSLocation object. </summary>
	/// <param name="location"> the PLSSLocation object to use for filling in the components. </param>
	private void fillComponentData(PLSSLocation location)
	{
		string s = location.getPM();
		if (string.ReferenceEquals(s, null))
		{
			__pmJComboBox.select(__wildcard);
		}
		else
		{
			__pmJComboBox.setText(s);
		}

		int i = location.getTownship();
		if (i == PLSSLocation.UNSET)
		{
			__tsJTextField.setText("");
		}
		else
		{
			__tsJTextField.setText("" + i);
		}

		s = location.getTownshipDirection();
		if (string.ReferenceEquals(s, null))
		{
			__tsJComboBox.select(__wildcard);
		}
		else
		{
			__tsJComboBox.setText(s);
		}

		i = location.getRange();
		if (i == PLSSLocation.UNSET)
		{
			__rangeJTextField.setText("");
		}
		else
		{
			__rangeJTextField.setText("" + i);
		}

		s = location.getRangeDirection();
		if (string.ReferenceEquals(s, null))
		{
			__rangeJComboBox.select(__wildcard);
		}
		else
		{
			__rangeJComboBox.setText(s);
		}

		i = location.getSection();
		if (i == PLSSLocation.UNSET)
		{
			__sectionJTextField.setText("");
		}
		else
		{
			__sectionJTextField.setText("" + i);
		}

		s = location.getHalfSection();
		if (string.ReferenceEquals(s, null))
		{
			__halfSectionJComboBox.select(__wildcard);
		}
		else
		{
			__halfSectionJComboBox.setText(s);
		}

		s = location.getQ();
		if (string.ReferenceEquals(s, null))
		{
			__q160JComboBox.select(__wildcard);
		}
		else
		{
			__q160JComboBox.setText(s);
		}

		s = location.getQQ();
		if (string.ReferenceEquals(s, null))
		{
			__q40JComboBox.select(__wildcard);
		}
		else
		{
			__q40JComboBox.setText(s);
		}

		s = location.getQQQ();
		if (string.ReferenceEquals(s, null))
		{
			__q10JComboBox.select(__wildcard);
		}
		else
		{
			__q10JComboBox.setText(s);
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~PLSSLocationJDialog()
	{
		__wildcard = null;
		__rangeJTextField = null;
		__sectionJTextField = null;
		__tsJTextField = null;
		//__location = null;
		__halfSectionJComboBox = null;
		__pmJComboBox = null;
		__q10JComboBox = null;
		__q40JComboBox = null;
		__q160JComboBox = null;
		__rangeJComboBox = null;
		__tsJComboBox = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//			 base.finalize();
	}

	/// <summary>
	/// Responds to key pressed events. </summary>
	/// <param name="event"> the key event that happened. </param>
	public virtual void keyPressed(KeyEvent @event)
	{
		int code = @event.getKeyCode();

			// enter key acts as an ok action event
		if (code == KeyEvent.VK_ENTER)
		{
			__response = ResponseJDialog.OK;
			response();
		}
	}

	/// <summary>
	/// Responds to key released events. </summary>
	/// <param name="event"> the key event that happened. </param>
	public virtual void keyReleased(KeyEvent @event)
	{
	}

	/// <summary>
	/// Responds to key typed events. </summary>
	/// <param name="event"> the key event that happened. </param>
	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Return the user response. </summary>
	/// <returns> the dialog response string. </returns>
	public virtual PLSSLocation response()
	{
		setVisible(false);
		dispose();
		if (__response == ResponseJDialog.CANCEL)
		{
			return null;
		}
		else
		{
			return buildLocation();
		}
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		addWindowListener(this);

			// objects used throughout the GUI layout
			Insets insetsTLNR = new Insets(7,7,0,7);
			Insets insetsTNNN = new Insets(7,0,0,0);
			Insets insetsNLNR = new Insets(0,7,0,7);
			GridBagLayout gbl = new GridBagLayout();

			// Center panel
			JPanel centerJPanel = new JPanel();
			centerJPanel.setLayout(gbl);
			getContentPane().add("Center", centerJPanel);

			JGUIUtil.addComponent(centerJPanel, new JLabel("PM:"), 0, 0, 1, 1, 0, 0, insetsTLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

			__pmJComboBox = new SimpleJComboBox();
			__pmJComboBox.add(__wildcard);
			__pmJComboBox.add("B");
			__pmJComboBox.add("C");
			__pmJComboBox.add("N");
			__pmJComboBox.add("S");
			__pmJComboBox.add("U");
			__pmJComboBox.select(0);
			JGUIUtil.addComponent(centerJPanel, __pmJComboBox, 1, 0, 1, 1, 0, 0, insetsTNNN, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		JGUIUtil.addComponent(centerJPanel, new JLabel("Township:"), 0, 1, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

			__tsJTextField = new JTextField(10);
		__tsJTextField.addKeyListener(this);
			JGUIUtil.addComponent(centerJPanel, __tsJTextField, 1, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

			__tsJComboBox = new SimpleJComboBox();
			__tsJComboBox.add(__wildcard);
			__tsJComboBox.add("N");
			__tsJComboBox.add("S");
			__tsJComboBox.select(0);
			JGUIUtil.addComponent(centerJPanel, __tsJComboBox, 2, 1, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

			JGUIUtil.addComponent(centerJPanel, new JLabel("Range:"), 0, 2, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

			__rangeJTextField = new JTextField(10);
		__rangeJTextField.addKeyListener(this);
			JGUIUtil.addComponent(centerJPanel, __rangeJTextField, 1, 2, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.WEST);

			__rangeJComboBox = new SimpleJComboBox();
			__rangeJComboBox.add(__wildcard);
			__rangeJComboBox.add("E");
			__rangeJComboBox.add("W");
			__rangeJComboBox.select(0);
			JGUIUtil.addComponent(centerJPanel, __rangeJComboBox, 2, 2, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

			JGUIUtil.addComponent(centerJPanel, new JLabel("Section:"), 0, 3, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

			__sectionJTextField = new JTextField(10);
		__sectionJTextField.addKeyListener(this);
			JGUIUtil.addComponent(centerJPanel, __sectionJTextField, 1, 3, 1, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		  __halfSectionJComboBox = new SimpleJComboBox();
		__halfSectionJComboBox.add(__wildcard);
		__halfSectionJComboBox.add("U");
		  JGUIUtil.addComponent(centerJPanel, __halfSectionJComboBox, 2, 3, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

			JGUIUtil.addComponent(centerJPanel, new JLabel("1/4 Section:"), 0, 4, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

			__q160JComboBox = new SimpleJComboBox();
			__q160JComboBox.add(__wildcard);
			__q160JComboBox.add("NE");
			__q160JComboBox.add("NW");
			__q160JComboBox.add("SE");
			__q160JComboBox.add("SW");
			JGUIUtil.addComponent(centerJPanel, __q160JComboBox, 1, 4, 1, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

			JGUIUtil.addComponent(centerJPanel, new JLabel("1/4 1/4 Section:"), 0, 5, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

			__q40JComboBox = new SimpleJComboBox();
			__q40JComboBox.add(__wildcard);
			__q40JComboBox.add("NE");
			__q40JComboBox.add("NW");
			__q40JComboBox.add("SE");
			__q40JComboBox.add("SW");
			JGUIUtil.addComponent(centerJPanel, __q40JComboBox, 1, 5, 1, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

			JGUIUtil.addComponent(centerJPanel, new JLabel("1/4 1/4 1/4 Section:"), 0, 6, 1, 1, 0, 0, insetsNLNR, GridBagConstraints.NONE, GridBagConstraints.EAST);

			__q10JComboBox = new SimpleJComboBox();
			__q10JComboBox.add(__wildcard);
			__q10JComboBox.add("NE");
			__q10JComboBox.add("NW");
			__q10JComboBox.add("SE");
			__q10JComboBox.add("SW");
			JGUIUtil.addComponent(centerJPanel, __q10JComboBox, 1, 6, 1, 1, 0, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

			// Bottom panel
			JPanel bottomJPanel = new JPanel();
			bottomJPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
			getContentPane().add("South", bottomJPanel);

		JButton clearJButton = new JButton(__BUTTON_CLEAR);
		clearJButton.addActionListener(this);
		clearJButton.setToolTipText("Clear all text fields and set all choices " + "to the wildcard value (" + __wildcard + ").");
		bottomJPanel.add(clearJButton);

			JButton okJButton = new JButton(__BUTTON_OK);
		okJButton.addActionListener(this);
		okJButton.setToolTipText("Accept PLSS location.");
			bottomJPanel.add(okJButton);

			JButton cancelJButton = new JButton(__BUTTON_CANCEL);
		cancelJButton.addActionListener(this);
		cancelJButton.setToolTipText("Close this window and discard the " + "PLSS location.");
			bottomJPanel.add(cancelJButton);

		string app = JGUIUtil.getAppNameForWindows();
		if (string.ReferenceEquals(app, null) || app.Trim().Equals(""))
		{
			app = "";
		}
		else
		{
			app += " - ";
		}
			setTitle(app + "Specify PLSS Location");
			pack();
		JGUIUtil.center(this);
			setResizable(false);
	}

	/// <summary>
	/// Responds to window activated events. </summary>
	/// <param name="evt"> the window event that happened. </param>
	public virtual void windowActivated(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window closed events. </summary>
	/// <param name="evt"> the window event that happened. </param>
	public virtual void windowClosed(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window closing events. </summary>
	/// <param name="evt"> the window event that happened. </param>
	public virtual void windowClosing(WindowEvent evt)
	{
		__response = ResponseJDialog.CANCEL;
		response();
	}

	/// <summary>
	/// Responds to window deactivated events. </summary>
	/// <param name="evt"> the window event that happened. </param>
	public virtual void windowDeactivated(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window deiconified events. </summary>
	/// <param name="evt"> the window event that happened. </param>
	public virtual void windowDeiconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window iconified events. </summary>
	/// <param name="evt"> the window event that happened. </param>
	public virtual void windowIconified(WindowEvent evt)
	{
	}

	/// <summary>
	/// Responds to window opened events. </summary>
	/// <param name="evt"> the window event that happened. </param>
	public virtual void windowOpened(WindowEvent evt)
	{
	}

	}

}