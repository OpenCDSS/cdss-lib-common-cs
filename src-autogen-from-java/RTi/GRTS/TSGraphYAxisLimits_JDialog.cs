// TSGraphYAxisLimits_JDialog - dialog to specify Y axis limits for a TSGraph, which are used in interactive viewing

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

namespace RTi.GRTS
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;
	using SimpleJComboBox = RTi.Util.GUI.SimpleJComboBox;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// Dialog to specify Y axis limits for a TSGraph, which are used in interactive viewing.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TSGraphYAxisLimits_JDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.KeyListener, java.awt.event.WindowListener
	public class TSGraphYAxisLimits_JDialog : JDialog, ActionListener, ItemListener, KeyListener, WindowListener
	{
		private SimpleJButton cancel_JButton = null;
		private SimpleJButton ok_JButton = null;

		private SimpleJComboBox leftYAxisMinY_JComboBox = null;
		private SimpleJComboBox leftYAxisMaxY_JComboBox = null;
		private SimpleJComboBox rightYAxisMinY_JComboBox = null;
		private SimpleJComboBox rightYAxisMaxY_JComboBox = null;

		/// <summary>
		/// TSGraph for which properties are being set.
		/// </summary>
		private TSGraph tsgraph = null;

		/// <summary>
		/// Drawing device, used to allow repaint when properties are set.
		/// </summary>
		private TSGraphJComponent dev = null;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool ok_Renamed = false; // Has the users pressed OK to close the dialog.

	/// <summary>
	/// Is there an error waiting to be resolved before closing dialog?
	/// </summary>
	private bool errorWait = false;

	/// <summary>
	/// TSTool_Options_JDialog constructor. </summary>
	/// <param name="parent"> JFrame class instantiating this class, used to position this dialog. </param>
	/// <param name="tsgraph"> TSGraph that limits are being set for. </param>
	public TSGraphYAxisLimits_JDialog(TSViewGraphJFrame parent, TSGraphJComponent dev, TSGraph tsgraph) : base(parent, true)
	{
		this.tsgraph = tsgraph;
		this.dev = dev;
		initialize();
	}

	/// <summary>
	/// Responds to ActionEvents. </summary>
	/// <param name="event"> ActionEvent object </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		object o = @event.getSource();

		if (o == cancel_JButton)
		{
			response(false);
		}
		else if (o == ok_JButton)
		{
			checkInput();
			if (!errorWait)
			{
				response(true);
			}
		}
	}

	/// <summary>
	/// Check the input.  If errors exist, warn the user and set the __error_wait flag
	/// to true.  This should be called before response() is allowed to complete.
	/// </summary>
	private void checkInput()
	{ // TODO sam 2017-04-10 need to implement checks for statistics, reference date
	}

	/// <summary>
	/// Instantiates the UI components.
	/// </summary>
	private void initialize()
	{
		addWindowListener(this);

		Insets insetsTLBR = new Insets(2,2,2,2);

		GridBagLayout gbl = new GridBagLayout();
		JPanel main_JPanel = new JPanel();
		main_JPanel.setLayout(gbl);
		getContentPane().add("North", main_JPanel);
		int y = -1;

		TSGraphType rightYAxisGraphType = this.tsgraph.getRightYAxisGraphType();

		JGUIUtil.addComponent(main_JPanel, new JLabel("Specify y-axis limits for this graph (each graph has separate properties)."), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("The settings are active while viewing the graph but are separate from the time series product properties."), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("This ensures that dynamic properties do not override the properties used for automated processing."), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("If a number is specified, it is treated as a hint and a rounded value may be used for nice y-axis labels."), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("Possible values are:"), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("  " + TSGraph.YAXIS_LIMITS_AUTOFILL_AND_KEEP + " - auto-fill using current view's data and keep limits when scrolling"), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		// TODO sam 2017-04-23 evaluate whether to enable - is much more dynamic than the others
		//JGUIUtil.addComponent(main_JPanel, new JLabel ("  " + this.FILL_EACH_VIEW + " - auto-fill using each view's data (y-limits will change when scrolling) "),
		//    0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("  " + TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES + " - use time series product properties for y-axis"), 0, ++y, 7, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JSeparator(SwingConstants.HORIZONTAL), 0, ++y, 7, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

		JGUIUtil.addComponent(main_JPanel, new JLabel("Left y-axis maximum:"), 0, ++y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		leftYAxisMaxY_JComboBox = new SimpleJComboBox(true); // Allow number to be entered
		// Only include types that have been tested for all output.  More specific types may be included in
		// some commands where local handling is enabled.
		leftYAxisMaxY_JComboBox.add("");
		leftYAxisMaxY_JComboBox.add(TSGraph.YAXIS_LIMITS_AUTOFILL_AND_KEEP);
		//leftYAxisMaxY_JComboBox.add ( this.FILL_EACH_VIEW );
		leftYAxisMaxY_JComboBox.add(TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES);
		// Set the value from the TSGraph, which will be default at first and then user's value
		string leftYAxisViewMaxY = this.tsgraph.getLeftYAxisViewMaxY();
		if (JGUIUtil.isSimpleJComboBoxItem(leftYAxisMaxY_JComboBox, leftYAxisViewMaxY, JGUIUtil.NONE, "", null))
		{
			leftYAxisMaxY_JComboBox.select(leftYAxisViewMaxY);
		}
		else
		{
			// Add if a number
			if (StringUtil.isDouble(leftYAxisViewMaxY))
			{
				leftYAxisMaxY_JComboBox.insertItemAt(leftYAxisViewMaxY, 1);
				// Select...
				leftYAxisMaxY_JComboBox.select(leftYAxisViewMaxY);
			}
			else
			{
				// Select the blank...
				leftYAxisMaxY_JComboBox.select(0);
			}
		}
		leftYAxisMaxY_JComboBox.addItemListener(this);
			JGUIUtil.addComponent(main_JPanel, leftYAxisMaxY_JComboBox, 1, y, 2, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("Optional - maximum y for left y-axis (default=" + TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES + ")."), 3, y, 2, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(main_JPanel, new JLabel("Left y-axis minimum:"), 0, ++y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		leftYAxisMinY_JComboBox = new SimpleJComboBox(true); // Allow number to be entered
		// Only include types that have been tested for all output.  More specific types may be included in
		// some commands where local handling is enabled.
		leftYAxisMinY_JComboBox.add("");
		leftYAxisMinY_JComboBox.add(TSGraph.YAXIS_LIMITS_AUTOFILL_AND_KEEP);
		//leftYAxisMinY_JComboBox.add ( this.FILL_EACH_VIEW );
		leftYAxisMinY_JComboBox.add(TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES);
		// Set the value from the TSGraph, which will be default at first and then user's value
		string leftYAxisViewMinY = this.tsgraph.getLeftYAxisViewMinY();
		if (JGUIUtil.isSimpleJComboBoxItem(leftYAxisMinY_JComboBox, leftYAxisViewMinY, JGUIUtil.NONE, "", null))
		{
			leftYAxisMinY_JComboBox.select(leftYAxisViewMinY);
		}
		else
		{
			// Add if a number
			if (StringUtil.isDouble(leftYAxisViewMinY))
			{
				leftYAxisMinY_JComboBox.insertItemAt(leftYAxisViewMinY, 1);
				// Select...
				leftYAxisMinY_JComboBox.select(leftYAxisViewMinY);
			}
			else
			{
				// Select the blank...
				leftYAxisMinY_JComboBox.select(0);
			}
		}
		leftYAxisMinY_JComboBox.addItemListener(this);
		JGUIUtil.addComponent(main_JPanel, leftYAxisMinY_JComboBox, 1, y, 2, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("Optional - minimum y for left y-axis (default=" + TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES + ")."), 3, y, 2, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(main_JPanel, new JLabel("Right y-axis maximum:"), 0, ++y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		rightYAxisMaxY_JComboBox = new SimpleJComboBox(true); // Allow number to be entered
		// Only include types that have been tested for all output.  More specific types may be included in
		// some commands where local handling is enabled.
		rightYAxisMaxY_JComboBox.add("");
		rightYAxisMaxY_JComboBox.add(TSGraph.YAXIS_LIMITS_AUTOFILL_AND_KEEP);
		//rightYAxisMaxY_JComboBox.add ( this.FILL_EACH_VIEW );
		rightYAxisMaxY_JComboBox.add(TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES);
		// Set the value from the TSGraph, which will be default at first and then user's value
		string rightYAxisViewMaxY = this.tsgraph.getRightYAxisViewMaxY();
		if (JGUIUtil.isSimpleJComboBoxItem(rightYAxisMaxY_JComboBox, rightYAxisViewMaxY, JGUIUtil.NONE, "", null))
		{
			rightYAxisMaxY_JComboBox.select(rightYAxisViewMaxY);
		}
		else
		{
			// Add if a number
			if (StringUtil.isDouble(rightYAxisViewMaxY))
			{
				rightYAxisMaxY_JComboBox.insertItemAt(rightYAxisViewMaxY, 1);
				// Select...
				rightYAxisMaxY_JComboBox.select(rightYAxisViewMaxY);
			}
			else
			{
				// Select the blank...
				rightYAxisMaxY_JComboBox.select(0);
			}
		}
		rightYAxisMaxY_JComboBox.addItemListener(this);
			JGUIUtil.addComponent(main_JPanel, rightYAxisMaxY_JComboBox, 1, y, 2, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("Optional - maximum y for right y-axis (default=" + TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES + ")."), 3, y, 2, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		JGUIUtil.addComponent(main_JPanel, new JLabel("Right y-axis minimum:"), 0, ++y, 1, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.EAST);
		rightYAxisMinY_JComboBox = new SimpleJComboBox(true); // Allow number to be entered
		// Only include types that have been tested for all output.  More specific types may be included in
		// some commands where local handling is enabled.
		rightYAxisMinY_JComboBox.add("");
		rightYAxisMinY_JComboBox.add(TSGraph.YAXIS_LIMITS_AUTOFILL_AND_KEEP);
		//rightYAxisMinY_JComboBox.add ( this.FILL_EACH_VIEW );
		rightYAxisMinY_JComboBox.add(TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES);
		// Set the value from the TSGraph, which will be default at first and then user's value
		string rightYAxisViewMinY = this.tsgraph.getRightYAxisViewMinY();
		if (JGUIUtil.isSimpleJComboBoxItem(rightYAxisMinY_JComboBox, rightYAxisViewMinY, JGUIUtil.NONE, "", null))
		{
			rightYAxisMinY_JComboBox.select(rightYAxisViewMinY);
		}
		else
		{
			// Add if a number
			if (StringUtil.isDouble(rightYAxisViewMinY))
			{
				rightYAxisMinY_JComboBox.insertItemAt(rightYAxisViewMinY, 1);
				// Select...
				rightYAxisMinY_JComboBox.select(rightYAxisViewMinY);
			}
			else
			{
				// Select the blank...
				rightYAxisMinY_JComboBox.select(0);
			}
		}
		rightYAxisMinY_JComboBox.addItemListener(this);
			JGUIUtil.addComponent(main_JPanel, rightYAxisMinY_JComboBox, 1, y, 2, 1, 1, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);
		JGUIUtil.addComponent(main_JPanel, new JLabel("Optional - minimum y for right y-axis (default=" + TSGraph.YAXIS_LIMITS_USE_PRODUCT_PROPERTIES + ")."), 3, y, 2, 1, 0, 0, insetsTLBR, GridBagConstraints.NONE, GridBagConstraints.WEST);

		// Disable the right y-axis components if no right y-axis
		if (rightYAxisGraphType == TSGraphType.NONE)
		{
			rightYAxisMinY_JComboBox.setEnabled(false);
			rightYAxisMaxY_JComboBox.setEnabled(false);
		}

		JGUIUtil.addComponent(main_JPanel, new JSeparator(SwingConstants.HORIZONTAL), 0, ++y, 7, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

		// South Panel: North
		JPanel button_JPanel = new JPanel();
		button_JPanel.setLayout(new FlowLayout(FlowLayout.CENTER));
		getContentPane().add("South", button_JPanel);
		//JGUIUtil.addComponent(main_JPanel, button_JPanel,
		//	0, ++y, 1, 1, 1, 0, insetsTLBR, GridBagConstraints.HORIZONTAL, GridBagConstraints.CENTER);

		cancel_JButton = new SimpleJButton("Cancel", this);
		button_JPanel.add(cancel_JButton);
		ok_JButton = new SimpleJButton("OK", this);
		button_JPanel.add(ok_JButton);

		setTitle(JGUIUtil.getAppNameForWindows() + " - Set Y Axis Limits");
		setResizable(false);
		pack();
		JGUIUtil.center(this);
		base.setVisible(true);
	}

	/// <summary>
	/// Handle ItemEvent events. </summary>
	/// <param name="e"> ItemEvent to handle. </param>
	public virtual void itemStateChanged(ItemEvent e)
	{ //refresh();
	}

	/// <summary>
	/// Respond to KeyEvents.
	/// </summary>
	public virtual void keyPressed(KeyEvent @event)
	{
		int code = @event.getKeyCode();

		if (code == KeyEvent.VK_ENTER)
		{
			checkInput();
			if (!errorWait)
			{
				response(true);
			}
		}
	}

	public virtual void keyReleased(KeyEvent @event)
	{
	}

	public virtual void keyTyped(KeyEvent @event)
	{
	}

	/// <summary>
	/// Indicate if the user pressed OK (cancel otherwise). </summary>
	/// <returns> true if the edits were committed, false if the user canceled. </returns>
	public virtual bool ok()
	{
		return this.ok_Renamed;
	}

	/// <summary>
	/// Close and return to parent UI component.
	/// </summary>
	public virtual void response(bool ok)
	{
		this.ok_Renamed = ok;
		if (ok)
		{
			// Apply the choices that were selected
			string leftYAxisMinY = leftYAxisMinY_JComboBox.getSelected().Trim();
			string leftYAxisMaxY = leftYAxisMaxY_JComboBox.getSelected().Trim();
			string rightYAxisMinY = rightYAxisMinY_JComboBox.getSelected().Trim();
			string rightYAxisMaxY = rightYAxisMaxY_JComboBox.getSelected().Trim();
			// Set limits in the TSGraph
			this.tsgraph.setLeftYAxisViewMinY(leftYAxisMinY);
			this.tsgraph.setLeftYAxisViewMaxY(leftYAxisMaxY);
			this.tsgraph.setRightYAxisViewMinY(rightYAxisMinY);
			this.tsgraph.setRightYAxisViewMaxY(rightYAxisMaxY);
			// Refresh the graph, causing the limits to be recognized
			this.dev.repaint();
		}
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Responds to WindowEvents. </summary>
	/// <param name="event"> WindowEvent object </param>
	public virtual void windowClosing(WindowEvent @event)
	{
		response(false);
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