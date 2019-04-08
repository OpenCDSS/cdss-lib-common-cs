using System;

// GeoViewJFrame - standard JFrame containing map-based display, based on GeoView package

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
// GeoViewJFrame - standard JFrame containing map-based display, based on
//			GeoView package
// ----------------------------------------------------------------------------
// Copyright:  See COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2001-06	Steven A. Malers, RTi	Put together class from existing code
//					to make more modular and flexible.
// 2001-10-01	SAM, RTi		Add menu to write a GeoViewLayer to a
//					shapefile, for use with grid layers.
// 2001-10-17	SAM, RTi		Review javadoc and set unused data to
//					null to help with garbage collection.
// 2001-12-04	SAM, RTi		Copy GeoViewGUI to GeoViewJFrame and
//					modify to support Swing.
// ----------------------------------------------------------------------------
// 2003-05-06	J. Thomas Sapienza, RTi	Brought code in line with the non-Swing
//					version of the code.
// 2003-06-30	SAM, RTi		Add getGeoViewJPanel().
// 2004-01-08	SAM, RTi		Use the icon and title information set
//					in JGUIUtil, if available.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{


	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Class to display a GeoViewJFrame.  This is a JFrame containing a GeoViewJPanel
	/// and a menu bar with the main GeoView interaction tools.  The events are directed
	/// to the GeoViewJPanel.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GeoViewJFrame extends javax.swing.JFrame implements java.awt.event.ActionListener, java.awt.event.WindowListener
	public class GeoViewJFrame : JFrame, ActionListener, WindowListener
	{

	// Menu items...

	private readonly string OPEN_GVP = "Open Project...";
	private readonly string ADD_LAYER_TO_GEOVIEW = "Add Layer...";
	private readonly string ADD_SUMMARY_LAYER_TO_GEOVIEW = "Add Summary Layer...";
	private readonly string GEOVIEW_ZOOM = "Zoom Mode";
	private readonly string GEOVIEW_ZOOM_OUT = "Zoom Out";
	private readonly string PRINT_GEOVIEW = "Print...";
	private readonly string SAVE_AS_JPEG = "Save As Image ...";
	private readonly string SAVE_AS_SHAPEFILE = "Save As ...";
	private readonly string SELECT_GEOVIEW_ITEM = "Select Mode";
	private readonly string SET_ATTRIBUTE_KEY = "Set Attribute Key...";

	private GeoViewJPanel _the_GeoViewJPanel = null;

	public GeoViewJFrame(JFrame parent, PropList p)
	{
		JGUIUtil.setIcon(this, JGUIUtil.getIconImage());
		if ((string.ReferenceEquals(JGUIUtil.getAppNameForWindows(), null)) || JGUIUtil.getAppNameForWindows().Equals(""))
		{
			setTitle("GeoView");
		}
		else
		{
			setTitle(JGUIUtil.getAppNameForWindows() + " - GeoView");
		}

		JGUIUtil.setSystemLookAndFeel(true);

		addWindowListener(this);
		JMenuItem add_JMenuItem, key_JMenuItem, print_JMenuItem, saveAsImage_JMenuItem, saveAs_JMenuItem, select_JMenuItem, zoom_JMenuItem, zoomOut_JMenuItem;
		bool do_menu = true; // Open in menu is useful to test GeoViewProject

		if (do_menu)
		{ // This may eventually be a constructor option
		JMenuBar menu_bar = new JMenuBar();

		JMenu file_menu = new JMenu("File");

		file_menu.add(add_JMenuItem = new JMenuItem(OPEN_GVP));
		add_JMenuItem.addActionListener(this);
		file_menu.addSeparator();

		file_menu.add(add_JMenuItem = new JMenuItem(ADD_LAYER_TO_GEOVIEW));
		add_JMenuItem.addActionListener(this);

		file_menu.add(add_JMenuItem = new JMenuItem(ADD_SUMMARY_LAYER_TO_GEOVIEW));
		add_JMenuItem.addActionListener(this);

		file_menu.add(key_JMenuItem = new JMenuItem(SET_ATTRIBUTE_KEY));
		key_JMenuItem.setEnabled(false);
		key_JMenuItem.addActionListener(this);

		file_menu.addSeparator();

		file_menu.add(select_JMenuItem = new JMenuItem(SELECT_GEOVIEW_ITEM));
		select_JMenuItem.addActionListener(this);

		file_menu.add(zoom_JMenuItem = new JMenuItem(GEOVIEW_ZOOM));
		zoom_JMenuItem.addActionListener(this);

		file_menu.add(zoomOut_JMenuItem = new JMenuItem(GEOVIEW_ZOOM_OUT));
		zoomOut_JMenuItem.addActionListener(this);

		file_menu.addSeparator();

		file_menu.add(print_JMenuItem = new JMenuItem(PRINT_GEOVIEW));
		print_JMenuItem.addActionListener(this);

		file_menu.add(saveAsImage_JMenuItem = new JMenuItem(SAVE_AS_JPEG));
		saveAsImage_JMenuItem.addActionListener(this);
		file_menu.add(saveAs_JMenuItem = new JMenuItem(SAVE_AS_SHAPEFILE));
		saveAs_JMenuItem.addActionListener(this);

		menu_bar.add(file_menu);

		setJMenuBar(menu_bar);

		menu_bar = null;
		file_menu = null;
		} // End do_menu

		// Add a panel to hold the canvas...

		JToolBar toolbar = new JToolBar();
		_the_GeoViewJPanel = new GeoViewJPanel(this, null, toolbar);
		getContentPane().add("North", toolbar);
		getContentPane().add("Center", _the_GeoViewJPanel);

		setSize(950, 950);
		setBackground(Color.lightGray);
		pack();
		setVisible(true);
		// Clean up...
		add_JMenuItem = null;
		key_JMenuItem = null;
		print_JMenuItem = null;
		saveAsImage_JMenuItem = null;
		saveAs_JMenuItem = null;
		select_JMenuItem = null;
		zoom_JMenuItem = null;
		zoomOut_JMenuItem = null;
	}

	/// <summary>
	/// Handle action events.  Need in case the menu is enabled.  May disable if menus
	/// are never used.  Event-handling actually occurs in the GeoViewPanel class.
	/// </summary>
	public virtual void actionPerformed(ActionEvent evt)
	{
		if (_the_GeoViewJPanel != null)
		{
			_the_GeoViewJPanel.actionPerformed(evt);
		}
	}

	/// <summary>
	/// Close the GUI and dispose.  This does not call any listeners to notify any
	/// components of the closing.  If a parent app wants to know if this window is
	/// closed it should add a WindowListener.
	/// </summary>
	public virtual void close()
	{
		setVisible(false);
		dispose();
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoViewJFrame()
	{
		_the_GeoViewJPanel = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the GeoViewPanel associated with the GeoViewJFrame. </summary>
	/// <returns> the GeoViewPanel associated with the GeoViewJFrame. </returns>
	public virtual GeoViewJPanel getGeoViewJPanel()
	{
		return _the_GeoViewJPanel;
	}

	public virtual void windowActivated(WindowEvent e)
	{
	}

	/// <summary>
	/// This class is listening for GeoViewGUI closing so it can gracefully handle.
	/// </summary>
	public virtual void windowClosed(WindowEvent e)
	{
	}

	/// <summary>
	/// Cause the Frame to close.
	/// </summary>
	public virtual void windowClosing(WindowEvent e)
	{
		close();
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

	public static void Main(string[] args)
	{
		PrintWriter ofp;
		IOUtil.testing(true);
		JGUIUtil.setLastFileDialogDirectory("I:\\DEVELOP\\GIS\\libGeoViewJava\\src\\RTi\\GIS\\GeoView");
		string logFile = "c:\\temp\\test.out";
		try
		{
			ofp = Message.openLogFile(logFile);
			Message.setOutputFile(Message.LOG_OUTPUT, ofp);
			 Message.setDebugLevel(Message.LOG_OUTPUT, 1);
			 Message.setWarningLevel(Message.LOG_OUTPUT, 1);
			 Message.setStatusLevel(Message.LOG_OUTPUT, 1);

			Message.printStatus(1, "", "Using logfile: '" + logFile + "'");
		}
		catch (Exception)
		{
			Message.printWarning(2, "", "Unable to open log file \"" + logFile + "\"");
		}


		JFrame jframe = new JFrame();
		GeoViewJFrame g = new GeoViewJFrame(jframe, new PropList("blah"));
		JGUIUtil.center(g);
		g.setVisible(true);
	}

	} // End GeoViewJFrame class

}