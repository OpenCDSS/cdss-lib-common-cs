using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// JGUIUtil - Swing GUI utility methods class, containing static methods

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
// JGUIUtil - Swing GUI utility methods class, containing static methods
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
//
// 2002-09-16	J. Thomas Sapienza, RTi	Initial Version
// 2002-11-05	Steven A. Malers, RTi	Add selectAll() for JList (deselectAll
//					is not needed because
//					JList.clearSelection() can be used.
//					Extend this class from GUIUtil so that
//					all features are available in one class.
//					Remove createPanel() since it conflicts
//					with the GUIUtil base class and needs to
//					be phased out anyway (it is used only in
//					very limited cases and does not deserve
//					to take up space here).
// 2002-11-06	SAM, RTi		Add computeOptimalPosition().
//					Add newFontNameJChoice() and
//					newFontStyleJChoice().
//					Add selectIgnoreCase().
//					Add selectedSize().
// 2003-03-24	JTS, RTi		Added setWaitCursor().
// 2003-04-10	SAM, RTi		Add selectTokenMatches().
// 2003-05-08	JTS, RTi		* Added addToJComboBox().
//					* Added setSystemLookAndFeel().
// 2003-05-12	SAM, RTi		Add setEnabled().
// 2003-06-16	SAM, RTi		Add isChoiceItem() for SimpleJChoice.
// 2003-06-18	SAM, RTi		Add writeFile() to write a JTextArea to
//					a file.  This is more generic and simple
//					than the RTi.Util.IO.ExportJGUI way of
//					doing it.
// 2003-07-24	JTS, RTi		Added forceRepaint()
// 2003-09-18	JTS, RTI		* Added setIconImage() and 
//					  getIconImage().
//					* Added setAppNameForWindows() and
//					  getAppNameForWindows().
//					* Added setIcon().
//					* Added loadImageIcon.
// 2003-09-30	SAM, RTI		* Added setIcon(JDialog...).
// 2003-10-06	JTS, RTi		* loadIconImage() now throws an 
//					  exception if no icon could be found 
//					  at the specified location. 
//					* setIconImage(String) now throws an
//					  exception for the same reason.
// 2003-10-06	SAM, RTi		* Add addStringToSelected(), similar to
//					  old GUIUtil, but use Swing components.
// 					* Add removeStringFromSelected(),
//					  similar to old GUIUtil, but use Swing
//					  components.
//					* Add select() similar to old GUIUtil,
//					  to allow choices to be selected when
//					  ignoring case.
//					* Add isSimpleJComboBoxItem() similar to
//					  GUIUtil isChoiceItem().
//					* Change jcheckboxToString() to
//					  simply toString().
//					* Add indexOf() for JList, similar to
//					  the GUIUtil version.
// 2003-12-10	SAM, RTi		* Add selectIgnoreCase(SimpleJComboBox).
// 					* Add newFontNameJComboBox().
// 					* Add newFontStyleJComboBox().
// 2004-05-10	JTS, RTi		* Add copyToClipboard().
//					* Add clearClipboard().
// 2004-07-21	SAM, RTi		In isSimpleJComboBoxItem(), return false
//					if the compare string is null.
// 2004-08-26	SAM, RTi		Overload selectTokenMatches() with the
//					trim_tokens parameter.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------
// EndHeader

namespace RTi.Util.GUI
{


	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class provides useful static functions for handling SWING GUI (graphical 
	/// user interface) components.  This class extends GUIUtil and so inherits the
	/// ability to work with AWT components, as well.
	/// </summary>
	public abstract class JGUIUtil : GUIUtil
	{

	// TODO sam 2017-02-26 seems to only be used in GeoView - maybe should not be global static here?
	/// <summary>
	/// The current status of the wait cursor, as set by the setWaitCursor method.
	/// This will therefore be global within an application.
	/// </summary>
	private static bool __waitCursor = false;

	/// <summary>
	/// The icon to use for an application.
	/// </summary>
	private static ImageIcon __applicationIcon = null;

	/// <summary>
	/// The 'pretty' version of the application name that can be displayed in window
	/// titles, dialog boxes, and more.
	/// </summary>
	private static string __applicationName = "";

	/// <summary>
	/// Given a JList with selected items, add the specified string to the front of the
	/// items if it is not already at the front of the items.  After the changes, the
	/// originally selected items are still selected.  This is useful, for example,
	/// when a popup menu toggles the contents of a list back and forth.
	/// The list model must be the DefaultListModel or an extended class.
	/// REVISIT JAVADOC: see removeStringFromSelected </summary>
	/// <param name="list"> JList to modify. </param>
	/// <param name="prefix"> String to add. </param>
	public static void addStringToSelected(JList list, string prefix)
	{
		if ((list == null) || (string.ReferenceEquals(prefix, null)))
		{
			return;
		}
		int[] selected_indices = list.getSelectedIndices();
		int selected_size = selectedSize(list);
		int len = prefix.Length;
		DefaultListModel model = (DefaultListModel)list.getModel();
		string item;
		for (int i = 0; i < selected_size; i++)
		{
			item = (string)model.getElementAt(selected_indices[i]);
			if (item.Trim().regionMatches(true,0,prefix,0,len))
			{
				model.setElementAt(prefix + item, selected_indices[i]);
			}
		}
		// Make sure the selected indices remain as before...
		list.setSelectedIndices(selected_indices);
		selected_indices = null;
	}

	/// <summary>
	/// Add an array of strings to a JList.  This is useful when a standard set of choices are available. </summary>
	/// <param name="comboBox"> Choice to add items to. </param>
	/// <param name="items"> Items to add. </param>
	public static void addToJComboBox(JComboBox comboBox, string[] items)
	{
		if ((comboBox == null) || (items == null))
		{
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			comboBox.addItem(items[i]);
		}
	}

	/// <summary>
	/// Add a list of strings to a JList.  This is useful when a standard set of
	/// choices are available.  The toString() method of each object in the list is
	/// called, so even non-String items can be added. </summary>
	/// <param name="comboBox"> Choice to add items to. </param>
	/// <param name="items"> Items to add. </param>
	public static void addToJComboBox(JComboBox comboBox, IList<object> items)
	{
		if ((comboBox == null) || (items == null))
		{
			return;
		}
		foreach (object item in items)
		{
			comboBox.addItem(item.ToString());
		}
	}

	/// <summary>
	/// Clears the system clipboard of whatever data exists on it.  This should be 
	/// called at System.exit() time by applications that use the clipboard, otherwise
	/// any data put on the clipboard by the application will remain there and use system resources.
	/// </summary>
	public static void clearClipboard()
	{
		StringBuilder buffer = new StringBuilder("");
		StringSelection selection = new StringSelection(buffer.ToString());
		Clipboard clipboard = Toolkit.getDefaultToolkit().getSystemClipboard();
		clipboard.setContents(selection, selection);
	}

	/// <summary>
	/// Hides and dispose of a JFrame Object. </summary>
	/// <param name="frame"> JFrame object to hide. </param>
	public static void close(JFrame frame)
	{
		if (frame != null)
		{
			frame.setVisible(false);
			frame.dispose();
		}
	}

	/// <summary>
	/// Compute the optimal coordinates to display a JPopupMenu.
	/// This is necessary because of a limitation in JPopupMenu where it does not
	/// automatically adjust for cases where some of the menu would be displayed off
	/// the screen.  See JavaSoft bug 4425878. </summary>
	/// <param name="pt"> Candidate point (e.g., from MouseEvent.getPoint()). </param>
	/// <param name="c"> Component that menu is associated with (e.g., from MouseEvent.getComponent()). </param>
	/// <param name="menu"> JPopupMeni instance to check. </param>
	/// <returns> a Point containing the optimal coordinates. </returns>
	public static Point computeOptimalPosition(Point pt, Component c, JPopupMenu menu)
	{ // The code below is partially taken from the bug report.  However,
		// the fix there for computing coordinates was actually pretty
		// simplistic, so an improvement has been implemented here...
		Dimension menuSize = menu.getPreferredSize();
		Dimension screenSize = Toolkit.getDefaultToolkit().getScreenSize();
		SwingUtilities.convertPointToScreen(pt, c);
		Point optimal_pt = new Point(pt);
		if ((pt.x + menuSize.width) > screenSize.width)
		{
			optimal_pt.x -= menuSize.width;
		}
		if ((pt.y + menuSize.height) > screenSize.height)
		{
			optimal_pt.y -= menuSize.height;
		}
		SwingUtilities.convertPointFromScreen(optimal_pt, c);
		return optimal_pt;
	}

	/// <summary>
	/// Copies a String to the system clipboard.  Once the string has been copied
	/// to the clipboard, it can be pasted into other applications. </summary>
	/// <param name="s"> the String to copy to the clipboard. </param>
	public static void copyToClipboard(string s)
	{
		StringBuilder buffer = new StringBuilder(s);
		StringSelection selection = new StringSelection(buffer.ToString());
		Clipboard clipboard = Toolkit.getDefaultToolkit().getSystemClipboard();
		clipboard.setContents(selection, selection);
	}

	/// <summary>
	/// Enable a list of components.  This method can be called, for example, 
	/// when a data object is selected from a list of objects (e.g., in a JList or JWorksheet). </summary>
	/// <param name="comp"> an array of all the JComponents on the form that can be
	/// enabled when something is selected. </param>
	/// <param name="compNeverEnabled"> an array of the components in comp[] that should 
	/// never be editable.  These components are disabled after ther others are
	/// enabled.  Specify as -1 to ignore. </param>
	/// <param name="editable"> Indicates whether the form is editable or not.  If the form is
	/// not editable, then some components may be disabled to prevent input. </param>
	public static void enableComponents(JComponent[] comp, int[] compNeverEnabled, bool editable)
	{
		for (int i = 0; i < comp.Length; i++)
		{
			if (comp[i] is JTextComponent)
			{
				if (editable)
				{
					setEnabled(comp[i], true);
					((JTextComponent)comp[i]).setEditable(true);
				}
				else
				{
					((JTextComponent)comp[i]).setEditable(false);
				}
			}
			else if (comp[i] is JComboBox)
			{
				if (editable)
				{
					setEnabled(comp[i], true);
					((JComboBox)comp[i]).setEditable(true);
				}
				else
				{
					((JComboBox)comp[i]).setEditable(false);
					setEnabled(comp[i], false);
				}
			}
			else
			{
				setEnabled(comp[i], true);
			}
		}
		if (compNeverEnabled != null)
		{
			for (int i = 0; i < compNeverEnabled.Length; i++)
			{
				if (compNeverEnabled[i] >= 0)
				{
					if (comp[compNeverEnabled[i]] is JTextComponent)
					{
						// Text is hard to read when disabled so just set not editable...
						((JTextComponent)comp[compNeverEnabled[i]]).setEditable(false);
					}
					else
					{
						// All other components...
						setEnabled(comp[compNeverEnabled[i]], false);
					}
				}
			}
		}
	}

	/// <summary>
	/// Disable a list of components.  For example, use when no data item is
	/// selected.  See enableComponents(), which will also disable components globally
	/// if the editable flag is set to false. </summary>
	/// <param name="comp"> an array of all the JComponents on the form to be disabled. </param>
	/// <param name="cleartext"> If true, text components will be cleared when disabled.  If 
	/// false, the text component text will not be changed. </param>
	public static void disableComponents(JComponent[] comp, bool cleartext)
	{
		for (int i = 0; i < comp.Length; i++)
		{
			if (comp[i] is JTextComponent)
			{
				if (cleartext)
				{
					((JTextComponent)comp[i]).setText("");
				}
				((JTextComponent)comp[i]).setEditable(false);
			}
			else if (comp[i] is JComboBox)
			{
				setEnabled(comp[i], false);
				((JComboBox)comp[i]).setEditable(false);
			}
			else
			{
				setEnabled(comp[i], false);
			}
		}
	}

	/// <summary>
	/// Forces the component to repaint immediately.  The JComponent is guaranteed 
	/// to be repainted by the time this method returns. </summary>
	/// <param name="component"> the JComponent to repaint. </param>
	public static void forceRepaint(JComponent component)
	{
		Rectangle rect = component.getBounds();
		if (rect == null)
		{
			return;
		}
		rect.x = 0;
		rect.y = 0;
		component.paintImmediately(rect);
	}

	/// <summary>
	/// Returns the 'pretty' version of the application name, which can be used
	/// in windows, dialog boxes, and titles. </summary>
	/// <returns> the nice version of the application name.  Will never return 
	/// <tt>null</tt>. </returns>
	public static string getAppNameForWindows()
	{
		return __applicationName;
	}

	/// <summary>
	/// Returns the image to use as the application icon. </summary>
	/// <returns> the image to use as the application icon. </returns>
	public static ImageIcon getIconImage()
	{
		return __applicationIcon;
	}

	/// <summary>
	/// Returns the current wait cursor state. </summary>
	/// <returns> the current wait cursor state. </returns>
	public static bool getWaitCursor()
	{
		return __waitCursor;
	}

	/// <summary>
	/// Determine position of a string in a JList.
	/// The JList must use a DefaultListModel or object derived from this class. </summary>
	/// <param name="list"> JList to search. </param>
	/// <param name="item"> String item to search for. </param>
	/// <param name="selected_only"> Indicates if only selected items should be searched. </param>
	/// <param name="ignore_case"> Indicates whether to ignore case (true) or not (false). </param>
	/// <returns> The index of the first match, or -1 if no match. </returns>
	public static int indexOf(JList list, string item, bool selected_only, bool ignore_case)
	{
		if ((list == null) || (string.ReferenceEquals(item, null)) || (item.Length == 0))
		{
			return -1;
		}
		int size = 0;
		string list_item = null;
		DefaultListModel model = (DefaultListModel)list.getModel();
		if (selected_only)
		{
			size = selectedSize(list);
			int[] selected_indices = list.getSelectedIndices();
			for (int i = 0; i < size; i++)
			{
				list_item = (string)model.elementAt(selected_indices[i]);
				if (ignore_case)
				{
					if (list_item.Equals(item, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
				else if (list_item.Equals(item))
				{
					return i;
				}
			}
		}
		else
		{
			size = model.size();
			for (int i = 0; i < size; i++)
			{
				list_item = (string)model.elementAt(i);
				if (ignore_case)
				{
					if (list_item.Equals(item, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
				else if (list_item.Equals(item))
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Determine if the specified compare String exists within a SimpleJComboBox - CASE SENSITIVE.
	/// See the overloaded method for a full description.  This version matches any substring (when flag=CHECK_SUBSTRINGS)
	/// and is case-sensitive. </summary>
	/// <param name="comboBox"> SimpleJComboBox object. </param>
	/// <param name="compare"> String to compare comboBox items against.  If null, false is returned. </param>
	/// <param name="flag"> compare criteria (CHECK_SUBSTRINGS or NONE); currently any substring that matches will return true </param>
	/// <param name="delimiter"> String containing delimiter to parse for flag=CHECK_SUBSTRINGS; </param>
	/// <param name="index"> Index location where the compare String was located
	/// (index[0] is set to the first ComboBox item that matches). </param>
	public static bool isSimpleJComboBoxItem(SimpleJComboBox comboBox, string compare, int flag, string delimiter, int[] index)
	{
		return isSimpleJComboBoxItem(comboBox, compare, flag, delimiter, -1, index, false);
	}

	/// <summary>
	/// Determine if the specified compare String exists within a SimpleJComboBox - CASE SENSITIVE.
	/// <ul>
	/// <li>	Can compare the compare String against substrings for each item
	/// in the comboBox object if FLAG is set to CHECK_SUBSTRINGS.</li>
	/// <li>	To not compare against substrings, set FLAG to NONE.</li>
	/// </ul> </summary>
	/// <param name="comboBox"> SimpleJComboBox object. </param>
	/// <param name="compare"> String to compare comboBox items against.  If null, false is returned. </param>
	/// <param name="flag"> compare criteria (CHECK_SUBSTRINGS or NONE);
	/// currently any trimmed substring that matches will return true </param>
	/// <param name="delimiter"> String containing delimiter to parse for flag=CHECK_SUBSTRINGS; </param>
	/// <param name="compareIndex"> if >= 0, the substring part to compare (e.g., 
	/// may be null if using flag=NONE; specify -1 to compare all parts </param>
	/// <param name="index"> Index location in ComboBox data where the compare String was located
	/// (index[0] is set to the first ComboBox item that matches). </param>
	/// <param name="ignoreCase"> true to ignore case in comparisons; false to require that case matches
	/// This is filled in unless it is passed as null.  For example use this when checking substrings so that an item
	/// can be selected (rather than setting a full string that may not totally match). </param>
	/// <returns> returns true if compare exist in the comboBox items list, false otherwise. </returns>
	public static bool isSimpleJComboBoxItem(SimpleJComboBox comboBox, string compare, int flag, string delimiter, int compareIndex, int[] index, bool ignoreCase)
	{
		string curItem; // current Choice item

		if (string.ReferenceEquals(compare, null))
		{
			return false;
		}
		// Initialize variables
		compare = compare.Trim();
		int size = comboBox.getItemCount(); // number of items in the choices
		IList<string> choiceParts;

		int tokenPos = 0;
		for (int i = 0; i < size; i++)
		{
			curItem = comboBox.getItem(i).Trim();
			tokenPos = -1;
			if (flag == CHECK_SUBSTRINGS)
			{
				// Split the combo box item using the delimiter and check the parts
				choiceParts = StringUtil.breakStringList(curItem, delimiter, 0);
				if (choiceParts != null)
				{
					string subTrimmed;
					foreach (string sub in choiceParts)
					{
						++tokenPos;
						subTrimmed = sub.Trim(); // Trim substring (generally what is needed to compare core content value)
						// If a match occurs, return true and the index in the list in which the match was found.
						// If a requested compare string index was specified, only compare that part
						if ((compareIndex < 0) || (compareIndex == tokenPos))
						{
							if ((ignoreCase && subTrimmed.Equals(compare, StringComparison.OrdinalIgnoreCase)) || (!ignoreCase && subTrimmed.Equals(compare)))
							{
								if (index != null)
								{
									index[0] = i;
								}
								return true;
							}
						}
					}
				}
			}
			else if (flag == NONE)
			{
				// Compare to the curItem String directly
				if ((ignoreCase && curItem.Equals(compare, StringComparison.OrdinalIgnoreCase)) || (!ignoreCase && curItem.Equals(compare)))
				{
					if (index != null)
					{
						index[0] = i;
					}
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Loads an image icon from a location and returns it. </summary>
	/// <param name="location"> the location at which the icon can be found, either a path
	/// to an image on a drive or a path within a JAR file. </param>
	/// <returns> the ImageIcon that was loaded, or null if there was a problem loading the ImageIcon. </returns>
	/// <exception cref="exception"> if no file could be found at the specified location. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static javax.swing.ImageIcon loadIconImage(String location) throws Exception
	public static ImageIcon loadIconImage(string location)
	{
		// first try loading the image as if it were specified in a JAR file.
		URL iconURL = ClassLoader.getSystemResource(location);
		if (iconURL == null)
		{
			// If that failed, try loading the image as if it were specified in a proper file name
			File f = new File(location);
			if (!f.exists())
			{
				throw new Exception("No icon could be found at location '" + location + "'");
			}
			iconURL = f.toURI().toURL();
			if (iconURL == null)
			{
				throw new Exception("No icon could be found at location '" + location + "'");
			}
		}
		ImageIcon i = new ImageIcon(iconURL);

		return i;
	}

	/// <summary>
	/// Return a new SimpleJComboBox that contains a list of standard fonts.
	/// Use the overloaded version to also include a longer list of fonts available in the local graphical environment. </summary>
	/// <returns> a new SimpleJComboBox that contains a list of standard fonts. </returns>
	public static SimpleJComboBox newFontNameJComboBox()
	{
		return newFontNameJComboBox(true, false);
	}

	/// <summary>
	/// Return a new SimpleJComboBox that contains a list of fonts. </summary>
	/// <param name="includeCommonAtTop"> if true, include common fonts at the top for ease of selection. </param>
	/// <param name="includeLocal"> if true, include all available font family names from the local graphics environment. </param>
	/// <returns> a new SimpleJComboBox that contains a list of standard fonts. </returns>
	public static SimpleJComboBox newFontNameJComboBox(bool includeCommonAtTop, bool includeLocal)
	{
		SimpleJComboBox fonts = new SimpleJComboBox(false);
		// Always put common "generic" ones at top, which should be portable to any platform
		if (includeCommonAtTop)
		{
			fonts.add("Arial");
			fonts.add("Courier");
			fonts.add("Courier New");
			fonts.add("Helvetica");
			fonts.add("Times Roman New");
		}
		if (includeLocal)
		{
			// Get more fonts from the local computing environment - some risk these won't be on all machines
			string[] localFonts = GraphicsEnvironment.getLocalGraphicsEnvironment().getAvailableFontFamilyNames();
			// Add to end
			for (int i = 0; i < localFonts.Length; i++)
			{
				fonts.add(localFonts[i]);
			}
		}
		return fonts;
	}

	/// <summary>
	/// Return a new SimpleJComboBox that contains a list of standard font styles. </summary>
	/// <returns> a new SimpleJComboBox that contains a list of standard font styles. </returns>
	public static SimpleJComboBox newFontStyleJComboBox()
	{
		SimpleJComboBox styles = new SimpleJComboBox(false);
		styles.add("Plain");
		styles.add("PlainItalic");
		styles.add("Bold");
		styles.add("BoldItalic");
		return styles;
	}

	/// <summary>
	/// Given a list with selected items, remove the specified string from the front of
	/// the items if is at the front of the items.  After the changes, the
	/// originally selected items are still selected.  This is useful, for example,
	/// when a popup menu toggles the contents of a list back and forth.
	/// The list model must be the DefaultListModel or an extended class.
	/// TODO JAVADOC: see addStringToSelected </summary>
	/// <param name="list"> JList to modify. </param>
	/// <param name="prefix"> String to add. </param>
	public static void removeStringFromSelected(JList list, string prefix)
	{
		if ((list == null) || (string.ReferenceEquals(prefix, null)))
		{
			return;
		}
		int[] selected_indices = list.getSelectedIndices();
		int selected_size = selectedSize(list);
		int len = prefix.Length;
		DefaultListModel model = (DefaultListModel)list.getModel();
		string item;
		for (int i = 0; i < selected_size; i++)
		{
			item = (string)model.getElementAt(selected_indices[i]);
			if (item.Trim().regionMatches(true,0,prefix,0,len) && StringUtil.tokenCount(item," \t", StringUtil.DELIM_SKIP_BLANKS) > 1)
			{
				model.setElementAt(item.Substring(len).Trim(), selected_indices[i]);
			}
		}
		// Make sure the selected indices remain as before...
		list.setSelectedIndices(selected_indices);
		selected_indices = null;
	}

	/// <summary>
	/// Select a single matching item in a JList.  Only the first match is selected.
	/// The DefaultListModel or an extended class should be used for the list model. </summary>
	/// <param name="list"> JList to select from. </param>
	/// <param name="item"> Item to select. </param>
	/// <param name="ignore_case"> Indicates whether case should be ignored when searching the list for a match. </param>
	public static void select(JList list, string item, bool ignore_case)
	{
		if ((list == null) || (string.ReferenceEquals(item, null)))
		{
			return;
		}
		DefaultListModel model = (DefaultListModel)list.getModel();
		int size = model.size();
		string list_item = null;
		for (int i = 0; i < size; i++)
		{
			list_item = (string)model.getElementAt(i);
			if (ignore_case)
			{
				if (list_item.Equals(item, StringComparison.OrdinalIgnoreCase))
				{
					list.setSelectedIndex(i);
					return;
				}
			}
			else if (list_item.Equals(item))
			{
				list.setSelectedIndex(i);
				return;
			}
		}
	}

	/// <summary>
	/// Select all items in a JList. </summary>
	/// <param name="list"> JList to select all items. </param>
	public static void selectAll(JList list)
	{
		if (list == null)
		{
			return;
		}
		// There is no "select" method so need to select all.  Rather than do
		// this item by item, send an array and do it all at once - this should
		// hopefully give the best performance...
		int[] selected = new int[list.getModel().getSize()];
		int size = selected.Length;
		for (int i = 0; i < size; i++)
		{
			selected[i] = i;
		}
		list.setSelectedIndices(selected);
		selected = null;
	}

	/// <summary>
	/// Return the index of the requested selected item.  For example, if there are 5
	/// items selected out of 20 total, requesting index 0 will return index of the
	/// first of the 5 selected items.  This is particularly useful when determining
	/// the first or last selected item in a list. </summary>
	/// <param name="list"> JList to check. </param>
	/// <param name="selected_index"> Position in the selected rows list. </param>
	/// <returns> the position in the original data for the requested selected index or
	/// -1 if unable to determine. </returns>
	public static int selectedIndex(JList list, int selected_index)
	{
		if (list == null)
		{
			return -1;
		}
		int[] selected = list.getSelectedIndices();
		if (selected != null)
		{
			int length = selected.Length;
			if (selected_index > (length - 1))
			{
				selected = null;
				return -1;
			}
			int pos = selected[selected_index];
			selected = null;
			return pos;
		}
		return -1;
	}

	/// <summary>
	/// Return the number of items selected in a JList. </summary>
	/// <param name="list"> JList to check. </param>
	/// <returns> the number items selected in the JList, or 0 if a null List. </returns>
	public static int selectedSize(JList list)
	{
		if (list == null)
		{
			return 0;
		}
		int[] selected = list.getSelectedIndices();
		if (selected != null)
		{
			int length = selected.Length;
			selected = null;
			return length;
		}
		return 0;
	}

	/// <summary>
	/// Select an item in a SimpleJComboBox, ignoring the case.  This is useful when the
	/// SimpleJComboBox shows a valid property but the property may not always exactly
	/// match in case when read from a file or hand-edited. </summary>
	/// <param name="c"> SimpleJComboBox to select from. </param>
	/// <param name="item"> SimpleJComboBox item to select as a string. </param>
	/// <exception cref="Exception"> if the string is not found in the SimpleJComboBox. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void selectIgnoreCase(SimpleJComboBox c, String item) throws Exception
	public static void selectIgnoreCase(SimpleJComboBox c, string item)
	{ // Does not look like SimpleJComboBox.select(String) throws an exception
		// if the item is not found (especially if the SimpleJComboBox is
		// editable) so go through the list every time...
		// Get the list size...
		int size = c.getItemCount();
		for (int i = 0; i < size; i++)
		{
			if (c.getItem(i).Equals(item, StringComparison.OrdinalIgnoreCase))
			{
				c.select(i);
				return;
			}
		}
		throw new Exception("String \"" + item + "\" not found in SimpleJComboBox");
	}

	/// <summary>
	/// Select an item in a JComboBox, comparing a specific token in the choices.  This
	/// is useful when the combo box shows an extended value (e.g., "Value - Description"). </summary>
	/// <param name="c"> JComboBox to select from. </param>
	/// <param name="ignore_case"> Indicates if case should be ignored when comparing strings. </param>
	/// <param name="delimiter"> String delimiter used by StringUtil.breakStringList(). </param>
	/// <param name="flags"> Flags used by StringUtil.breakStringList(). </param>
	/// <param name="token"> Token position in the JComboBox item, to be compared. </param>
	/// <param name="item"> String item to compare to JComboBox item tokens. </param>
	/// <param name="default_item"> String If null, only "item" is evaluated.  If not null and
	/// "item" is not found, then an attempt to match "default" is made, using the same
	/// tokenizing parameters.  If a match is found, it is selected.  If a match is not
	/// found, an Exception is thrown.  This parameter is useful when defaulting a
	/// combo box to a value for a new instance of an object. </param>
	/// <exception cref="Exception"> if the string is not found in the JComboBox. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void selectTokenMatches(javax.swing.JComboBox c, boolean ignore_case, String delimiter, int flags, int token, String item, String default_item) throws Exception
	public static void selectTokenMatches(JComboBox c, bool ignore_case, string delimiter, int flags, int token, string item, string default_item)
	{
		selectTokenMatches(c, ignore_case, delimiter, flags, token, item, default_item, false);
	}

	/// <summary>
	/// Select an item in a JComboBox, comparing a specific token in the choices.  This
	/// is useful when the combo box shows an extended value (e.g., Value - Description"). </summary>
	/// <param name="c"> JComboBox to select from. </param>
	/// <param name="ignore_case"> Indicates if case should be ignored when comparing strings. </param>
	/// <param name="delimiter"> String delimiter used by StringUtil.breakStringList().  If null, compare the whole string. </param>
	/// <param name="flags"> Flags used by StringUtil.breakStringList(). </param>
	/// <param name="token"> Token position in the JComboBox item, to be compared. </param>
	/// <param name="item"> String item to compare to JComboBox item tokens. </param>
	/// <param name="default_item"> If null, only "item" is evaluated.  If not null and
	/// "item" is not found, then an attempt to match "default" is made, using the same
	/// tokenizing parameters.  If a match is found, it is selected.  If a match is not
	/// found, an Exception is thrown.  This parameter is useful when defaulting a
	/// combo box to a value for a new instance of an object. </param>
	/// <param name="trim_tokens"> Indicate whether the tokens should be trimmed when trying to
	/// match - the default is not to trim. </param>
	/// <exception cref="Exception"> if the string is not found in the JComboBox. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void selectTokenMatches(javax.swing.JComboBox c, boolean ignore_case, String delimiter, int flags, int token, String item, String default_item, boolean trim_tokens) throws Exception
	public static void selectTokenMatches(JComboBox c, bool ignore_case, string delimiter, int flags, int token, string item, string default_item, bool trim_tokens)
	{ // Does not look like Choice.select(String) throws an exception if the
		// item is not found so go through the list every time...
		// Get the list size...
		int size = c.getItemCount();
		IList<string> tokens = null;
		int ntokens = 0;
		string choice_token;
		for (int i = 0; i < size; i++)
		{
			if (!string.ReferenceEquals(delimiter, null))
			{
				// Use the delimiter to split the choice
				tokens = StringUtil.breakStringList(c.getItemAt(i).ToString(), delimiter, flags);
				ntokens = 0;
				if (tokens != null)
				{
					ntokens = tokens.Count;
				}
				if (ntokens <= token)
				{
					continue;
				}
				// Now compare.  Do not use region matches because we want an exact match on the token...
				choice_token = tokens[token];
			}
			else
			{
				choice_token = c.getItemAt(i).ToString();
			}
			if (trim_tokens)
			{
				choice_token = choice_token.Trim();
			}
			if (ignore_case)
			{
				if (choice_token.Equals(item, StringComparison.OrdinalIgnoreCase))
				{
					c.setSelectedIndex(i);
					return;
				}
			}
			else
			{
				if (choice_token.Equals(item))
				{
					c.setSelectedIndex(i);
					return;
				}
			}
		}
		// If here, allow the default to be selected.  Because all choices need
		// to be evaluated again, just call the code recursively using the
		// default instead of the item...
		if (!string.ReferenceEquals(default_item, null))
		{
			selectTokenMatches(c, ignore_case, delimiter, flags, token, default_item, null);
		}
		else
		{
			// No default was specified so throw an exception...
			throw new Exception("Token " + token + " \"" + item + "\" not found in available choices");
		}
	}

	/// <summary>
	/// Sets the 'pretty' version of the application name, which can be displayed in
	/// dialog boxes, frames, and window titles. </summary>
	/// <param name="appName"> the application name to use.  If null, the application name
	/// will be set to an empty string (""). </param>
	public static void setAppNameForWindows(string appName)
	{
		if (string.ReferenceEquals(appName, null))
		{
			__applicationName = "";
		}
		else
		{
			__applicationName = appName;
		}
	}

	/// <summary>
	/// Enable a component if the state is different from the requested state.  The
	/// benefit of this method is that it only changes the state if necessary.  Changing
	/// the state without the check may result in unnecessary flashing of the interface.
	/// The object is also checked for null. </summary>
	/// <param name="component"> A component to enable/disable. </param>
	/// <param name="enabled"> Indicates whether to enable or disable the component. </param>
	public static void setEnabled(Component component, bool enabled)
	{
		if (component == null)
		{
			return;
		}
		if (enabled)
		{
			// Need to enable the item, but only if it is not already...
			if (!component.isEnabled())
			{
				component.setEnabled(true);
			}
		}
		else
		{
			// Need to disable the item, but only if it is not already...
			if (component.isEnabled())
			{
				component.setEnabled(false);
			}
		}
	}

	/// <summary>
	/// Sets an icon in a JFrame; all JDialogs that are opened with this frame as their
	/// parent will take this icon, as well. </summary>
	/// <param name="frame"> the frame in which to set an icon. </param>
	/// <param name="i"> the ImageIcon to use as the window icon. </param>
	public static void setIcon(JFrame frame, ImageIcon i)
	{
		string routine = "JGUIUtil.setIcon()";
		try
		{
			if (i != null)
			{
				Image image = i.getImage();
				if (image != null)
				{
					frame.setIconImage(image);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(2, routine, e);
		}
	}

	/// <summary>
	/// Sets the icon to use for an application. </summary>
	/// <param name="i"> the ImageIcon to use.  Can be null. </param>
	public static void setIconImage(ImageIcon i)
	{
		__applicationIcon = i;
	}

	/// <summary>
	/// Sets the icon to use for an application.  The icon is read from the specified location and stored. </summary>
	/// <param name="location"> a path to an icon or the location within a jar file. </param>
	/// <exception cref="Exception"> if no icon could be found at the specified location. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setIconImage(String location) throws Exception
	public static void setIconImage(string location)
	{
		ImageIcon icon = loadIconImage(location);
		if (icon != null)
		{
			setIconImage(icon);
		}
	}

	// The following static data are used by setWaitCursor()
	//Static map of glass pane component and adapters that were added to intercept events.
	//Use these maps to clear adapters when going to normal state.
	//Otherwise a new adapter will get added every time waiting is turned on
	private static Dictionary<Component, KeyAdapter> waitKeyListenerMap = new Dictionary<Component, KeyAdapter>();
	private static Dictionary<Component, MouseMotionAdapter> waitMouseMotionListenerMap = new Dictionary<Component, MouseMotionAdapter>();

	/// <summary>
	/// Activates the wait cursor for a component and by default use the glass pane
	/// if a JFrame or JDialog to intercept mouse and key actions while in wait mode (state=true). </summary>
	/// <param name="component"> the JFrame or JDialog for which to set the wait cursor </param>
	/// <param name="state"> whether to set the wait cursor to on (true) or off (false) </param>
	public static void setWaitCursor(Component component, bool state)
	{
		// By default use the glass pane to intercept key and mouse events on the component
		// because this is the historical behavior.
		setWaitCursor(component, state, true);
	}

	/// <summary>
	/// Activates the wait cursor for a top-level Swing container (JFrame, JDialog, JApplet). </summary>
	/// <param name="component"> the top-level component for which to set the wait cursor </param>
	/// <param name="state"> whether to set the wait cursor to on (true) or off (false) </param>
	/// <param name="useGlassPaneToInterceptEvents"> whether to use the glass pane on the component
	/// to intercept key and mouse events. This should only be specified as true
	/// when the glass pane is not used in any other way, such as an overlaid mouse tracker. </param>
	public static void setWaitCursor(Component component, bool state, bool useGlassPaneToInterceptEvents)
	{

		if (component == null)
		{
			return;
		}

		Component rootPane = SwingUtilities.getRoot(component);
		Component glassPane = null;

		if (rootPane != null && rootPane.isShowing())
		{
			if (component is JFrame)
			{
				glassPane = ((JFrame)component).getGlassPane();
			}
			else if (component is JDialog)
			{
				glassPane = ((JDialog)component).getGlassPane();
			}
			else if (component is JApplet)
			{
				glassPane = ((JApplet)component).getGlassPane();
			}
			if (state)
			{
				// Setting state to wait
				if (useGlassPaneToInterceptEvents)
				{
					// The following will intercept events on the component until
					// the wait is disabled
					// TODO sam 2017-02-25 problem is that repeated calls will add more and more listeners
					// that are never removed.  Is this really doing anything?
					if (glassPane != null)
					{
						// Temporarily add a key listener so that user's key events don't pass to
						// the component.  This is removed later using the map to find the listener.
						WaitCursorKeyListener kl = new WaitCursorKeyListener(glassPane);
						glassPane.addKeyListener(kl);
						waitKeyListenerMap[glassPane] = kl;
						// Temporarily add a mouse motion listener so that user's mouse events don't pass to
						// the component.  This is removed later using the map to find the listener.
						WaitCursorMouseMotionListener mml = new WaitCursorMouseMotionListener(glassPane);
						glassPane.addMouseMotionListener(mml);
						waitMouseMotionListenerMap[glassPane] = mml;
					}
				}
				if (glassPane != null)
				{
					glassPane.setCursor(Cursor.getPredefinedCursor(Cursor.WAIT_CURSOR));
				}
				rootPane.setCursor(Cursor.getPredefinedCursor(Cursor.WAIT_CURSOR));
			}
			else
			{
				// Setting state to not wait (clear wait)
				if (glassPane != null)
				{
					glassPane.setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
					// Remove the listeners that were intercepting events.
					// Although the same listeners might be appropriate in the next call,
					// new listeners will get re-added above just to make sure the proper glass pane is used
					// (other code may manipulate the glass pane).
					waitKeyListenerMap.Remove(glassPane);
					waitMouseMotionListenerMap.Remove(glassPane);
				}
				rootPane.setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
			}
			if (useGlassPaneToInterceptEvents)
			{
				// If the glass pane is set to not visible, then the adapters won't be active.
				glassPane.setVisible(state);
			}
		}

		__waitCursor = state;
	}

	/// <summary>
	/// Sets the current java look and feel to be like the current System theme 
	/// (Windows, Motif, etc) or the Java Metal theme. </summary>
	/// <param name="set"> if set is true, the look and feel is set to be the standard system
	/// look and feel.  If false, the Java metal theme is used. </param>
	public static void setSystemLookAndFeel(bool set)
	{
		bool error = false;
		if (set == true)
		{
			try
			{
				UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
			}
			catch (Exception e)
			{
				error = true;
				if (Message.isDebugOn)
				{
					Message.printWarning(2, "", e);
				}
			}
		}
		else
		{
			try
			{
				UIManager.setLookAndFeel(UIManager.getCrossPlatformLookAndFeelClassName());
			}
			catch (Exception e)
			{
				error = true;
				if (Message.isDebugOn)
				{
					Message.printWarning(2, "", e);
				}
			}
		}

		if (error)
		{
			try
			{
				UIManager.setLookAndFeel(UIManager.getCrossPlatformLookAndFeelClassName());
			}
			catch (Exception e)
			{
				Message.printWarning(1, "setWindowsLookAndFeel", "Unable to set a new look and feel.");
				Message.printWarning(2, "setWindowsLookAndFeel", e);
			}
		}
	}

	/// <summary>
	/// Returns a String version of a JCheckBox's state suitable for display on a 
	/// text form.  The JCheckbox will be represented by <pre>"[X]"</pre> if it is
	/// selected, otherwise by <pre>"[ ]"</pre>. </summary>
	/// <returns> a String version of a JCheckBox's state suitable for display on a form. </returns>
	public static string ToString(JCheckBox box)
	{
		if (box.isSelected())
		{
			return "[X]";
		}
		return "[ ]";
	}

	/// <summary>
	/// Return the text from a TextArea as a Vector of strings, each of which has had
	/// the newline removed.  This is useful for exporting the text to a file or for
	/// printing.  At some point Sun may change the delimiter returned but we can isolate to this routine. </summary>
	/// <param name="ta"> TextArea of interest. </param>
	/// <returns> A list of strings containing the text from the text area or a 
	/// Vector with no elements if a null TextArea. </returns>
	public static IList<string> toList(JTextArea ta)
	{
		if (ta == null)
		{
			return new List<string>();
		}
		IList<string> v = StringUtil.breakStringList(ta.getText(), "\n", 0);
		// Just to be sure, remove any trailing carriage-return characters from the end...
		string @string;
		for (int i = 0; i < v.Count; i++)
		{
			@string = v[i];
			v[i] = StringUtil.removeNewline(@string);
		}
		return v;
	}

	/// <summary>
	/// Write a JTextArea to a file.  For example, this can be used to save a report
	/// that is displayed in a JTextArea. </summary>
	/// <param name="ta"> JTextArea to write. </param>
	/// <param name="filename"> Name of file to write. </param>
	/// <exception cref="if"> there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeFile(javax.swing.JTextArea ta, String filename) throws Exception
	public static void writeFile(JTextArea ta, string filename)
	{
		IList<string> v = toList(ta);
		PrintWriter @out = new PrintWriter(new StreamWriter(filename));
		//
		// Write each element of the _export Vector to a file.
		//
		int size = v.Count;
		for (int i = 0; i < size; i++)
		{
			@out.println(v[i]);
		}
		//
		// close the PrintStream Object
		//
		@out.flush();
		@out.close();
	}

	}

}