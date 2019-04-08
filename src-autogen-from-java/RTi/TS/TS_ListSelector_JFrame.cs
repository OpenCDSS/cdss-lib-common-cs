using System.Collections.Generic;

// TS_ListSelector_JFrame - a GUI for selecting a group of time series from a large list of time series.

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
// TS_ListSelector_JFrame - a GUI for selecting a group of time series from
//	a large list of time series.
// ----------------------------------------------------------------------------
// History:
//
// 2005-03-29	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-04	JTS, RTi		GUI revised to allow display of time
//					series as graph, summary, table, or
//					to initiate other actions, depending
//					on the values passed into the 
//					constructor in a PropList.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.TS
{




	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;
	using JWorksheet = RTi.Util.GUI.JWorksheet;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;

	using PropList = RTi.Util.IO.PropList;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is a GUI for selecting time series from a list.
	/// The class should be used as follows:<para><ul>
	/// <li>declare an instance of this class that will display a list of time 
	/// series</li>
	/// <li>add all the TS_ListSelector_Listeners that should be informed when the 
	/// user selects time series from the list and presses OK</li>
	/// <li>when the user presses okay, theTS_ListSelector_Listeners will be 
	/// notified.</li>
	/// </ul>
	/// </para>
	/// <para><b>Example:</b><p>
	/// <pre>
	/// PropList props = new PropList("TS_ListSelector_JFrame");
	/// props.add("ActionButtons=Graph,Table,Summary");
	/// TS_ListSelector_JFrame listGUI = new TS_ListSelector_JFrame(data,props);
	/// listGUI.addTSListSelectorListener(this);
	/// </pre>
	/// Buttons will be added for each "ActionButtons" value that is listed, and when
	/// the button is pressed, the timeSeriesSelected() method will be called with the
	/// list of selected time series.  The button label will be passed to the calling
	/// code and can be interpreted as appropriate.
	/// </para>
	/// </summary>
	public class TS_ListSelector_JFrame : JFrame, ActionListener, MouseListener
	{

	/// <summary>
	/// Button labels.  "Cancel", "Deselect All", and "Select All" are always present
	/// on the GUI.  "OK" is only added if no ActionButtons are specified in the 
	/// constructor PropList.
	/// </summary>
	private string __BUTTON_CANCEL = "Cancel", __BUTTON_OK = "OK", __BUTTON_DESELECT_ALL = "Deselect All", __BUTTON_SELECT_ALL = "Select All";

	/// <summary>
	/// The worksheet that appears in the GUI.
	/// </summary>
	private JWorksheet __worksheet = null;

	/// <summary>
	/// The PropList passed to the constructor to control GUI behavior.
	/// </summary>
	private PropList __props = null;

	/// <summary>
	/// After a proplist is processed this contains the order of the buttons that
	/// should appear on the GUI.  Once the GUI is set up, this contains the 
	/// instantiated buttons so that they can be enabled or disable as appropriate.
	/// </summary>
	private System.Collections.IList __buttons = null;

	/// <summary>
	/// The TS_ListSelector_Listeners that have been registered to be informed when OK is pressed.
	/// </summary>
	private System.Collections.IList __listeners = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="data"> the Vector of time series to display in the worksheet. </param>
	/// <param name="props"> a PropList to control the behavior of the GUI.  Recognized 
	/// properties are:
	/// <table width=100% cellpadding=10 cellspacing=0 border=2>
	/// <tr>
	/// <td><b>Property</b></td>   
	/// <td><b>Description</b></td>   
	/// <td><b>Default</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td><b>ActionButtons</b></td>
	/// <td>Defines the buttons that will be present on the
	/// GUI.  The buttons appear at the bottom of the GUI between the 
	/// "Select All" and "Cancel" buttons, and are added in
	/// the order they appear in this property.<para>
	/// </para>
	/// </ul><para>
	/// </para>
	/// The following buttons will ALWAYS be present on the GUI:<para>
	/// <ul>
	/// <li>Deselect All -- deselects all selected time series in the list.</li>
	/// <li>Select all -- selects all time series in the list.</li>
	/// <li>Cancel -- closes the GUI.</li>
	/// </para>
	/// </ul><para>
	/// If this property is not set, the GUI will contain the select/deselect
	/// buttons, a cancel button, and an OK button.
	/// <td>OK, Cancel</td>
	/// </tr>
	/// </table>
	/// REVISIT (JTS - 2005-04-05)
	/// Perhaps later add a "CloseButtons=OK" property that specifies the button that, 
	/// when pressed, will cause the GUI to be closed. </param>
	public TS_ListSelector_JFrame(System.Collections.IList data, PropList props) : base()
	{

		processPropList(props);

		__listeners = new List<object>();

		if (data == null)
		{
			setupGUI(new List<object>());
		}
		else
		{
			setupGUI(data);
		}
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the event that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();

		if (command.Equals(__BUTTON_CANCEL))
		{
			setVisible(false);
			dispose();
		}
		else if (command.Equals(__BUTTON_DESELECT_ALL))
		{
			__worksheet.deselectAll();
			enableButtons(false);
		}
		else if (command.Equals(__BUTTON_SELECT_ALL))
		{
			__worksheet.selectAllRows();
			enableButtons(true);
		}
		else if (@event.getSource() is SimpleJButton)
		{
			notifyListeners(getSelectedRows(), command);
		}
	}

	/// <summary>
	/// Adds a TS_ListSelector_Listener.  These listeners are notified when the user
	/// has selected some time series and pressed a button. </summary>
	/// <param name="listener"> the listener to register. </param>
	public virtual void addTSListSelectorListener(TS_ListSelector_Listener listener)
	{
		__listeners.Add(listener);
	}

	/// <summary>
	/// Enables or disables the buttons on the form that are dependent on rows being
	/// selected. </summary>
	/// <param name="enabled"> if true, the buttons will be enabled.  If false, they will 
	/// be disabled. </param>
	private void enableButtons(bool enabled)
	{
		int size = __buttons.Count;
		SimpleJButton button = null;
		for (int i = 0; i < size; i++)
		{
			button = (SimpleJButton)__buttons[i];
			button.setEnabled(enabled);
		}
	}

	/// <summary>
	/// Gets the rows that are currently selected in the worksheet and returns them as
	/// a non-null Vector.  If 0 rows are selected, an empty Vector will be returned. </summary>
	/// <returns> the rows selected in the worksheet. </returns>
	private System.Collections.IList getSelectedRows()
	{
		System.Collections.IList v = new List<object>();
		int[] rows = __worksheet.getSelectedRows();
		for (int i = 0; i < rows.Length; i++)
		{
			v.Add(__worksheet.getRowData(rows[i]));
		}

		return v;
	}

	/// <summary>
	/// Returns the Vector of registered TS_ListSelector_Listeners. </summary>
	/// <returns> the Vector of registered TS_ListSelector_Listeners. </returns>
	public virtual System.Collections.IList getTSListSelectorListeners()
	{
		return __listeners;
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
	/// Does nothing.
	/// </summary>
	public virtual void mousePressed(MouseEvent @event)
	{
	}

	/// <summary>
	/// Enables or disabled buttons depending on whether any rows in the worksheet are
	/// selected.
	/// </summary>
	public virtual void mouseReleased(MouseEvent @event)
	{
		bool enabled = false;
		int num = __worksheet.getSelectedRowCount();
		if (num > 0)
		{
			enabled = true;
		}

		enableButtons(enabled);
	}

	/// <summary>
	/// Notifies listeners that data have been selected. </summary>
	/// <param name="data"> the Vector of time series that were selected from the worksheet. </param>
	private void notifyListeners(System.Collections.IList data, string action)
	{
		int size = __listeners.Count;
		TS_ListSelector_Listener l = null;
		for (int i = 0; i < size; i++)
		{
			l = (TS_ListSelector_Listener)__listeners[i];
			l.timeSeriesSelected(this, data, action);
		}
	}

	/// <summary>
	/// Processes the PropList passed into the constructor. </summary>
	/// <param name="props"> the PropList passed into the constructor. </param>
	private void processPropList(PropList props)
	{
		__props = props;

		string s = __props.getValue("ActionButtons");
		__buttons = new List<object>();
		if (!string.ReferenceEquals(s, null))
		{
			System.Collections.IList v = StringUtil.breakStringList(s, ",", 0);
			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				s = (string)v[i];
				__buttons.Add(s.Trim());
			}
		}
		else
		{
			__buttons.Add(__BUTTON_OK);
		}

	}

	/// <summary>
	/// Removes the specified TS_List_Selector_Listener from the registered list of
	/// listeners.  Even if the listener was registered more than once, all instances 
	/// of it are removed. </summary>
	/// <param name="listener"> the listener to remove. </param>
	public virtual void removeTSListSelectorListener(TS_ListSelector_Listener listener)
	{
		int size = __listeners.Count;
		TS_ListSelector_Listener l = null;
		for (int i = (size - 1); i >= 0; i--)
		{
			l = (TS_ListSelector_Listener)__listeners[i];
			if (l == listener)
			{
				__listeners.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Sets up the GUI. </summary>
	/// <param name="data"> the Vector of TS to display in the worksheet. </param>
	private void setupGUI(System.Collections.IList data)
	{
		TS_List_TableModel tableModel = new TS_List_TableModel(data);
		TS_List_CellRenderer cr = new TS_List_CellRenderer(tableModel);

		JPanel worksheetPanel = new JPanel();
		worksheetPanel.setLayout(new GridBagLayout());

		PropList props = new PropList("");
		props.set("JWorksheet.ShowRowHeader=true");
		props.add("JWorksheet.ShowPopupMenu=true");
		props.add("JWorksheet.SelectionMode=MultipleDiscontinuousRowSelection");

		JScrollWorksheet jsw = new JScrollWorksheet(cr, tableModel, props);
		__worksheet = jsw.getJWorksheet();
		__worksheet.setHourglassJFrame(this);
		__worksheet.addMouseListener(this);
		JGUIUtil.addComponent(worksheetPanel, jsw, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		getContentPane().add("Center", worksheetPanel);

		JPanel buttonPanel = new JPanel();
		buttonPanel.setLayout(new GridBagLayout());
		SimpleJButton deselectAll = new SimpleJButton(__BUTTON_DESELECT_ALL, this);
		SimpleJButton selectAll = new SimpleJButton(__BUTTON_SELECT_ALL, this);

		JGUIUtil.addComponent(buttonPanel, deselectAll, 0, 0, 1, 1, 1, 0, 2, 5, 2, 5, GridBagConstraints.NONE, GridBagConstraints.EAST);
		JGUIUtil.addComponent(buttonPanel, selectAll, 1, 0, 1, 1, 0, 0, 2, 5, 2, 5, GridBagConstraints.NONE, GridBagConstraints.EAST);

		int size = __buttons.Count;
		SimpleJButton button = null;
		string s = null;
		System.Collections.IList v = new List<object>();
		for (int i = 0; i < size; i++)
		{
			s = (string)__buttons[i];
			button = new SimpleJButton(s, this);
			button.setEnabled(false);
			v.Add(button);

			JGUIUtil.addComponent(buttonPanel, button, (2 + i), 0, 1, 1, 0, 0, 2, 5, 2, 5, GridBagConstraints.NONE, GridBagConstraints.EAST);
		}

		__buttons = v;

		SimpleJButton cancel = new SimpleJButton(__BUTTON_CANCEL, this);

		JGUIUtil.addComponent(buttonPanel, cancel, (2 + size), 0, 1, 1, 0, 0, 2, 5, 2, 5, GridBagConstraints.NONE, GridBagConstraints.EAST);

		getContentPane().add("South", buttonPanel);

		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());

		pack();

		string app = JGUIUtil.getAppNameForWindows();
		string title = "Select Time Series";
		if (string.ReferenceEquals(app, null))
		{
			setTitle(title);
		}
		else
		{
			setTitle(app + " - " + title);
		}

		JGUIUtil.center(this);

		setVisible(true);
	}

	}

}