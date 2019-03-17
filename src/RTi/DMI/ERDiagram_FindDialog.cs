// ERDiagram_FindDialog - a dialog for finding a table by name in an ER diagram

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
// ERDiagram_FindDialog - A dialog for finding a table by name.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-08-27	J. Thomas Sapienza, RTi	* Initial changelog.  
//					* Added JFrame parameter to constructor.
//					* Added labels describing the search.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;

	/// <summary>
	/// This class is a dialog box used when finding a table in the ER Diagram.
	/// It contains a text field that responds to the 'enter' key being pressed.  
	/// The user types in the first few characters of the table name and the tables
	/// are searched for all those tables that start with the same case-insensitive 
	/// character.
	/// REVISIT (JTS - 2006-05-22)
	/// Should probably use TextResponseJDialog instead and remove this class.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ERDiagram_FindDialog extends javax.swing.JDialog
	public class ERDiagram_FindDialog : JDialog
	{

	/// <summary>
	/// The text field into which the user types the name of the table.
	/// </summary>
	private JTextField __tableNameTextField;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame on which this dialog will be displayed. </param>
	/// <param name="al"> the ActionListener that will respond when the user press enter
	/// in the text field. </param>
	public ERDiagram_FindDialog(JFrame parent, ActionListener al) : base(parent, false)
	{
		setTitle("Find Table");

		JPanel panel = new JPanel();
		__tableNameTextField = new JTextField(30);

		panel.add(__tableNameTextField);
		__tableNameTextField.addActionListener(al);

		JPanel labelPanel = new JPanel();
		labelPanel.setLayout(new GridBagLayout());
		JGUIUtil.addComponent(labelPanel, new JLabel("Enter a substring of the name of the"), 0, 0, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		JGUIUtil.addComponent(labelPanel, new JLabel("table for which you are looking and press enter."), 0, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);

		getContentPane().add("North", labelPanel);

		getContentPane().add("Center", panel);
		pack();
		setVisible(true);
		JGUIUtil.center(this);
	}

	/// <summary>
	/// Returns the text entered in the text field before the user hit 'enter'. </summary>
	/// <returns> the text entered in the text field before the user hit 'enter'. </returns>
	public virtual string getText()
	{
		return __tableNameTextField.getText();
	}

	}

}