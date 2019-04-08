using System.Collections.Generic;

// JWizard - JWizard top level GUI class

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
// JWizard - JWizard top level GUI class.
//-----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
// 15 Jan 1998 DLG, RTi		Created initial class description.
// 05 Mar 1998 DLG, RTi		Added the icon image functionality.
// 11 Mar 1998 DLG, RTi		Changed class to public from public abstract
//				as this was causing a conflict with IE 4.0.
//				Functions are no longer abstract in this class.
// 31 Mar 1998 DLG, RTi		Added Constructor for help key.
// 07 May 1998 DLG, RTi		Added javadoc comments.
// 07 Jan 2001 SAM, RTi		Change import * to specific imports, GUI to
//				GUIUtil, and IO to IOUtil.
//-----------------------------------------------------------------------------
// 2003-11-28	J. Thomas Sapienza, RTi	Initial Swing version.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class assist may be extended from JFrame class in an effort to 
	/// minimize redundant code when constructing a JWizard type GUI.
	/// Classes extended from this class will inherit the following GUI 
	/// objects:
	/// <pre>
	///		1. Cancel JButton - Closes the JWizard GUI and cancels
	///				all selections made in the JWizard.
	///		2. Back JButton - returns to the previous JPanel in the 
	///				JWizard, if available.
	///		3. Next JButton - proceeds to the next JPanel in the 
	///				JWizard, if available.
	///		4. Finish JButton - Signifies that use of the JWizard is 
	///				complete.
	/// </pre>
	/// It is up to the user of this class to generate the code necessary 
	/// to perform the aforementioned behavoir. This class merely instantiates
	/// the objects and enforces the appropriate functions neccesary to handle
	/// the events.<para>
	/// 
	/// Addtional objects may be added at a lower level but they MUST be added
	/// </para>
	/// to the content pane in the "Center" with this code:<para>
	/// <pre>
	/// getContentPane().add("Center", [component to be added]);
	/// </pre>
	/// 
	/// Note that this object may be further
	/// subdivided at the lower level if more JPanels are desired.
	/// A JTextField object, __statusJTextField, is available which is useful
	/// </para>
	/// for printing status information.<para>
	/// 
	/// Note that all swing objects are protected and may be accessed from 
	/// </para>
	/// the lower level.<para>
	/// 
	/// This class implements an ActionListener and WindowListener interface;
	/// however, the necessary functions must be defined at a lower-level if
	/// specific event handling is needed for: cancel, back, next, and finish.
	/// The windowClosing event is defined in this class, but should
	/// </para>
	/// be overriden is dispose of the object is not desired.<para>
	/// 
	/// </para>
	/// </summary>

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class JWizard extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.WindowListener
	public class JWizard : JFrame, ActionListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	public const string BUTTON_BACK = "< Back", BUTTON_CANCEL = "Cancel", BUTTON_FINISH = "Finish", BUTTON_HELP = "Help", BUTTON_NEXT = "Next >";

	/// <summary>
	/// Total number of wizard steps.
	/// </summary>
	private int __maxStep;

	/// <summary>
	/// Current wizard step.
	/// </summary>
	private int __wizardStep;

	/// <summary>
	/// Cancel JButton
	/// </summary>
	protected internal JButton __cancelJButton;

	/// <summary>
	/// Help JButton
	/// </summary>
	protected internal JButton __helpJButton;

	/// <summary>
	/// Finish JButton
	/// </summary>
	protected internal JButton __finishJButton;

	/// <summary>
	/// Next JButton
	/// </summary>
	protected internal JButton __nextJButton;

	/// <summary>
	/// Back JButton
	/// </summary>
	protected internal JButton __backJButton;

	/// <summary>
	/// GUI labels.
	/// </summary>
	private JLabel __stepJLabel;
	/// <summary>
	/// Displays status information
	/// </summary>
	protected internal JTextField __statusJTextField;

	/// <summary>
	/// JPanel for which specfic GUI components are to be added by inherited classes.
	/// </summary>
	protected internal JPanel __centerJPanel;

	/// <summary>
	/// JPanel to add explanation or information string as multiple JLabel
	/// </summary>
	protected internal JPanel __infoJPanel;

	/// <summary>
	/// The help to load if the help button is pressed.
	/// </summary>
	private string __helpKey;

	/// <summary>
	/// Constructor.
	/// </summary>
	public JWizard()
	{
		__helpKey = null;
		setupGUI();
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="helpKey">	Help Key String </param>
	public JWizard(string helpKey)
	{
		// set the help key
		if (!string.ReferenceEquals(helpKey, null))
		{
			__helpKey = helpKey.Trim();
		}
		else
		{
			__helpKey = null;
		}
		setupGUI();
	}

	/// <summary>
	/// Responds to ActionEvents.  This function MUST be caught in the classes that 
	/// extend this one, as it is not recognized when the event occurs at this level.
	/// Only help events are handled at this level.
	/// </summary>
	public virtual void actionPerformed(ActionEvent evt)
	{
		string s = evt.getActionCommand();

		if (s.Equals(BUTTON_HELP))
		{
			// REVISIT HELP (JTS - 2003-11-28)
		}
		else if (s.Equals(BUTTON_BACK))
		{
			backClicked();
		}
		else if (s.Equals(BUTTON_CANCEL))
		{
			cancelClicked();
		}
		else if (s.Equals(BUTTON_FINISH))
		{
			finishClicked();
		}
		else if (s.Equals(BUTTON_NEXT))
		{
			nextClicked();
		}
	}

	/// <summary>
	/// This function responds to the __backJButton action performed event.
	/// </summary>
	protected internal virtual bool backClicked()
	{
		__wizardStep--;
		__stepJLabel.setText("Wizard Step " + __wizardStep + " of " + __maxStep);

		// disable back button if first panel is encountered
		if (__wizardStep == 1)
		{
			__backJButton.setEnabled(false);
		}
		else
		{
			__backJButton.setEnabled(true);
		}
		__finishJButton.setEnabled(false);
		__nextJButton.setEnabled(true);

		return true;
	}

	/// <summary>
	/// This function is responsible for handling GUI closing behavior.
	/// </summary>
	protected internal virtual bool cancelClicked()
	{
		return true;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~JWizard()
	{
		__cancelJButton = null;
		__helpJButton = null;
		__finishJButton = null;
		__nextJButton = null;
		__backJButton = null;
		__stepJLabel = null;
		__statusJTextField = null;
		__centerJPanel = null;
		__infoJPanel = null;
		__helpKey = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// This function responds to the __finishJButton action performed event.
	/// </summary>
	protected internal virtual bool finishClicked()
	{
		return true;
	}

	/// <summary>
	/// Returns the current wizard step. </summary>
	/// <returns> the current wizard step. </returns>
	protected internal virtual int getWizardStep()
	{
		return __wizardStep;
	}

	/// <summary>
	/// This function initializes data members.
	/// </summary>
	private void initialize()
	{
		__maxStep = -1;
		__wizardStep = -1;
		return;
	}

	/// <summary>
	/// This function responds to the __nextJButton action performed event.
	/// </summary>
	protected internal virtual bool nextClicked()
	{
		__wizardStep++;

		if (__wizardStep <= __maxStep)
		{
			__stepJLabel.setText("Wizard Step " + __wizardStep + " of " + __maxStep);
		}

		// disable next button and enable finish JButton 
		// if last panel is encountered 
		if (__wizardStep == __maxStep)
		{
			__nextJButton.setEnabled(false);
			__backJButton.setEnabled(true);
			__finishJButton.setEnabled(true);
		}
		else
		{
			__nextJButton.setEnabled(true);
			__backJButton.setEnabled(true);
			__finishJButton.setEnabled(false);
		}
		return true;
	}

	/// <summary>
	/// This function sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		this.addWindowListener(this);

		// objects to be used in the GUI layout
		GridBagLayout gbl = new GridBagLayout();
		Insets TLNR_Insets = new Insets(7,7,0,7);

		// North JPanel
		JPanel northJPanel = new JPanel();
		northJPanel.setLayout(new BorderLayout());
		getContentPane().add("North", northJPanel);

		// North West JPanel
		JPanel northWJPanel = new JPanel();
		northWJPanel.setLayout(gbl);
		northJPanel.add("West", northWJPanel);

		__stepJLabel = new JLabel("Please Insert WIZARD Step Information Here");
		__stepJLabel.setFont(new Font("Helvetica", Font.BOLD, 18));
		JGUIUtil.addComponent(northWJPanel, __stepJLabel, 1, 0, 1, 1, 0, 0, TLNR_Insets, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		__infoJPanel = new JPanel();
		__infoJPanel.setLayout(gbl);
		northJPanel.add("South", __infoJPanel);

		// Center JPanel
		__centerJPanel = new JPanel();
		getContentPane().add("Center", __centerJPanel);

		// South JPanel
		JPanel southJPanel = new JPanel();
		southJPanel.setLayout(new BorderLayout());
		getContentPane().add("South", southJPanel);

		// South: North JPanel
		JPanel southNJPanel = new JPanel();
		southNJPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
		southJPanel.add("North", southNJPanel);

		JPanel navigateJPanel = new JPanel();
		navigateJPanel.setLayout(new FlowLayout(FlowLayout.LEFT, 0, 0));
		southNJPanel.add(navigateJPanel);

		__backJButton = new JButton(BUTTON_BACK);
		__backJButton.addActionListener(this);
		navigateJPanel.add(__backJButton);

		__nextJButton = new JButton(BUTTON_NEXT);
		__nextJButton.addActionListener(this);
		navigateJPanel.add(__nextJButton);

		__finishJButton = new JButton(BUTTON_FINISH);
		__finishJButton.addActionListener(this);
		navigateJPanel.add(__finishJButton);

		JPanel controlJPanel = new JPanel();
		controlJPanel.setLayout(new FlowLayout(FlowLayout.LEFT, 7, 0));
		southNJPanel.add(controlJPanel);

		__cancelJButton = new JButton(BUTTON_CANCEL);
		__cancelJButton.addActionListener(this);
		controlJPanel.add(__cancelJButton);

		__helpJButton = new JButton(BUTTON_HELP);
		__helpJButton.addActionListener(this);
		if (string.ReferenceEquals(__helpKey, null))
		{
			__helpJButton.setVisible(false);
		}
		controlJPanel.add(__helpJButton);

		// South: South JPanel
		JPanel southSJPanel = new JPanel();
		southSJPanel.setLayout(gbl);
		southJPanel.add("South", southSJPanel);

		__statusJTextField = new JTextField();
		__statusJTextField.setEditable(false);
		JGUIUtil.addComponent(southSJPanel, __statusJTextField, 0, 0, 1, 1, 1, 0, GridBagConstraints.HORIZONTAL, GridBagConstraints.WEST);

		// JFrame settings
		pack();
		initialize();

		return;
	}

	/// <summary>
	/// Sets the state of the back button. </summary>
	/// <param name="state"> if true, the button is enabled.  If false, it is disabled. </param>
	public virtual void setBackEnabled(bool state)
	{
		__backJButton.setEnabled(state);
	}

	/// <summary>
	/// Sets the state of the finish button. </summary>
	/// <param name="state"> if true, the button is enabled.  If false, it is disabled. </param>
	public virtual void setFinishEnabled(bool state)
	{
		__finishJButton.setEnabled(state);
	}

	/// <summary>
	/// Sets the state of the next button. </summary>
	/// <param name="state"> if true, the button is enabled.  If false, it is disabled. </param>
	public virtual void setNextEnabled(bool state)
	{
		__nextJButton.setEnabled(state);
	}

	/// <summary>
	/// This function sets the information string. </summary>
	/// <param name="info"> string to display path icon file path </param>
	protected internal virtual void setInfoString(string info)
	{
		IList<string> vec = StringUtil.breakStringList(info, "\n", 0);

			Insets NLNR_Insets = new Insets(0,7,0,7);

		// clear out any existing labels
		__infoJPanel.removeAll();
		Font font = new Font("Helvetica", Font.BOLD, 12);
		JLabel label;
		int size = 0;
		int count = 0;

		if (vec != null)
		{
			// Add each string...
			size = vec.Count;
			//__infoJPanel.setLayout(new GridLayout(size, 1));

			for (int i = 0; i < size; i++)
			{
				label = new JLabel("          " + vec[i] + "          ");
				label.setFont(font);

					JGUIUtil.addComponent(__infoJPanel, label, 0, count, 1, 1, 0, 0, NLNR_Insets, GridBagConstraints.NONE, GridBagConstraints.WEST);
				count++;
			}
		}
		//pack();
		validate();
	}

	/// <summary>
	/// This function sets the current wizard step. </summary>
	/// <param name="step"> wizard step </param>
	protected internal virtual void setWizardStep(int step)
	{
		if (__maxStep == -999)
		{
			(new ResponseJDialog(this, "Error: Total Wizard steps unknown.", ResponseJDialog.OK)).response();
			(new ResponseJDialog(this, "Set the total steps before setting" + " the wizard step.", ResponseJDialog.OK)).response();
			return;
		}

		__stepJLabel.setText("Wizard Step " + step + " of " + __maxStep);

		// disable buttons accordingly
		if (step == 1 && step == __maxStep)
		{
			__finishJButton.setEnabled(true);
			__backJButton.setEnabled(false);
			__nextJButton.setEnabled(false);
		}
		else if (step == 1)
		{
			__finishJButton.setEnabled(false);
			__backJButton.setEnabled(false);
		}

		if (__maxStep > 1)
		{
			__nextJButton.setEnabled(true);
		}

		__wizardStep = step;
		// setVisible(true);
	}

	/// <summary>
	/// This function sets the total number of wizard steps. </summary>
	/// <param name="step"> total number of wizard steps </param>
	protected internal virtual void setTotalSteps(int step)
	{
		__maxStep = step;
	}

	/// <summary>
	/// This function responds to the window Closing window event. </summary>
	/// <param name="evt"> WindowEvent object </param>
	public virtual void windowClosing(WindowEvent evt)
	{
		setVisible(false);
		dispose();
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
	public virtual void windowOpened(WindowEvent evt)
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

	}

}