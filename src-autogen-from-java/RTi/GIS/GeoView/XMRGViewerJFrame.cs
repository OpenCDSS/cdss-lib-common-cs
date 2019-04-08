using System;

// XMRGViewerJFrame - a JFrame for displaying XMRG files.

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
// XMRGViewerJFrame - a JFrame for displaying XMRG files.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-10-14	J. Thomas Sapienza, RTi	Initial version.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{


	using JFileChooserFactory = RTi.Util.GUI.JFileChooserFactory;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;

	/// <summary>
	/// A JFrame that contains a panel that can be used to display XMRG file contents.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class XMRGViewerJFrame extends javax.swing.JFrame implements java.awt.event.ActionListener
	public class XMRGViewerJFrame : JFrame, ActionListener
	{

	/// <summary>
	/// Menu strings.
	/// </summary>
	private readonly string __MENU_FILE = "File", __MENU_FILE_OPEN = "Open XMRG File ...", __MENU_FILE_EXIT = "Exit";

	/// <summary>
	/// The directory to browse for xmrg files.
	/// </summary>
	private string __xmrgDir = null;

	/// <summary>
	/// The file that was opened and displayed in the frame.
	/// </summary>
	private string __xmrgFilename = null;

	/// <summary>
	/// The panel in which the XMRG data is shown.
	/// </summary>
	private XMRGViewerJPanel __xmrgPanel = null;

	/// <summary>
	/// Constructor.
	/// </summary>
	public XMRGViewerJFrame() : base(JGUIUtil.getAppNameForWindows() + " - XMRG File Viewer - [No file opened]")
	{

		setupGUI();
	}

	/// @deprecated use XMRGViewJFrame(String, boolean). 
	public XMRGViewerJFrame(string xmrgDir) : this(xmrgDir, false)
	{
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="xmrgDir"> the directory in which xmrg files are being opened. </param>
	/// <param name="xmrgFilename"> the file to open and display. </param>
	public XMRGViewerJFrame(string xmrgDir, string xmrgFilename) : base(JGUIUtil.getAppNameForWindows() + " - XMRG File Viewer - [No file opened]")
	{

		__xmrgDir = xmrgDir;
		JGUIUtil.setLastFileDialogDirectory(__xmrgDir);
		__xmrgFilename = xmrgFilename;

		setupGUI();
	}

	// FIXME SAM 2008-01-08 Need to pass a File parameter and handle automatically.
	/// <summary>
	/// Constructor. </summary>
	/// <param name="xmrgStr"> if the 'dir' param is true, this is the directory in which
	/// to browse for XMRG files.  If 'dir' is false, this is the file to open and 
	/// display. </param>
	/// <param name="dir"> whether the first parameter is a directory (true) or a filename 
	/// (false). </param>
	public XMRGViewerJFrame(string xmrgStr, bool dir) : base(JGUIUtil.getAppNameForWindows() + " - XMRG File Viewer - [No file opened]")
	{

		if (dir)
		{
			__xmrgDir = xmrgStr;
			JGUIUtil.setLastFileDialogDirectory(__xmrgDir);
		}
		else
		{
			__xmrgFilename = xmrgStr;
		}

		setupGUI();
	}

	/// <summary>
	/// Responds to action events. </summary>
	/// <param name="event"> the event that happened. </param>
	public virtual void actionPerformed(ActionEvent @event)
	{
		string command = @event.getActionCommand();

		if (command.Equals(__MENU_FILE_OPEN))
		{
			openFile();
		}
		else if (command.Equals(__MENU_FILE_EXIT))
		{
			setVisible(false);
			dispose();
		}
	}

	/// <summary>
	/// Opens an XMRG file and displays its contents.
	/// </summary>
	private void openFile()
	{
		JGUIUtil.setWaitCursor(this, true);
		string lastDirectorySelected = JGUIUtil.getLastFileDialogDirectory();

		JFileChooser fc = JFileChooserFactory.createJFileChooser(lastDirectorySelected);

		fc.setDialogTitle("Create Data Dictionary");
		fc.setAcceptAllFileFilterUsed(true);
		fc.setDialogType(JFileChooser.OPEN_DIALOG);

		JGUIUtil.setWaitCursor(this, false);
		int retVal = fc.showOpenDialog(this);
		if (retVal != JFileChooser.APPROVE_OPTION)
		{
			return;
		}

		string currDir = (fc.getCurrentDirectory()).ToString();

		if (!currDir.Equals(lastDirectorySelected, StringComparison.OrdinalIgnoreCase))
		{
			JGUIUtil.setLastFileDialogDirectory(currDir);
		}

		string path = fc.getSelectedFile().getPath();
		JGUIUtil.setWaitCursor(this, true);

		__xmrgPanel.openFile(path);

		setTitle(JGUIUtil.getAppNameForWindows() + " - XMRG File Viewer - [" + path + "]");

		JGUIUtil.setWaitCursor(this, false);
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		if (JGUIUtil.getIconImage() != null)
		{
			JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		}

		JGUIUtil.setSystemLookAndFeel(true);

		JMenuBar menuBar = new JMenuBar();
		JMenu fileMenu = new JMenu(__MENU_FILE);
		JMenuItem mi = new JMenuItem(__MENU_FILE_OPEN);
		mi.addActionListener(this);
		fileMenu.add(mi);
		fileMenu.addSeparator();

		mi = new JMenuItem(__MENU_FILE_EXIT);
		mi.addActionListener(this);
		fileMenu.add(mi);
		menuBar.add(fileMenu);

		setJMenuBar(menuBar);

		__xmrgPanel = new XMRGViewerJPanel(__xmrgFilename);

		getContentPane().add(new JScrollPane(__xmrgPanel));

		pack();

		setSize(600, 540);

		JGUIUtil.center(this);

		setVisible(true);
	}

	}

}