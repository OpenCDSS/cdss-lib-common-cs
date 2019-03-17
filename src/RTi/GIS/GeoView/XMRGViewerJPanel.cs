using System;

// XMRGViewJPanel - a JPanel for displaying the contents of a XMRG file.

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
// XMRGViewJPanel - a JPanel for displaying the contents of a XMRG file.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2004-10-14	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{



	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using JScrollWorksheet = RTi.Util.GUI.JScrollWorksheet;

	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a JPanel that can be plugged into a JFrame to display the 
	/// contents of an XMRG file.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class XMRGViewerJPanel extends javax.swing.JPanel
	public class XMRGViewerJPanel : JPanel
	{

	/// <summary>
	/// The worksheet that displays the XMRG data.
	/// </summary>
	private JScrollWorksheet __worksheet = null;

	/// <summary>
	/// The name of the file being displayed.
	/// </summary>
	private string __xmrgFilename = null;

	/// <summary>
	/// The XMRG class created from the file, for pulling out additional data.
	/// </summary>
	private XmrgGridLayer __xmrg = null;

	/// <summary>
	/// Constructor.  Opens with no file.
	/// </summary>
	public XMRGViewerJPanel() : this(null)
	{
	}

	/// <summary>
	/// Constructor.  Opens the specified file. </summary>
	/// <param name="xmrgFilename"> the file to open. </param>
	public XMRGViewerJPanel(string xmrgFilename)
	{
		setup();
	}

	/// <summary>
	/// Opens an XMRG file. </summary>
	/// <param name="filename"> the file to open. </param>
	public virtual void openFile(string filename)
	{
		removeAll();
		setLayout(new GridBagLayout());

		try
		{
			__xmrgFilename = filename;
			__xmrg = new XmrgGridLayer(__xmrgFilename, true, true);

			XMRGViewerTableModel model = new XMRGViewerTableModel(__xmrg);
			XMRGViewerCellRenderer renderer = new XMRGViewerCellRenderer(model);
			GeoGrid grid = __xmrg.getGrid();

			PropList props = new PropList("Worksheet");
			props.set("JWorksheet.ShowRowHeader=true");
			props.set("JWorksheet.FirstRowNumber=" + grid.getMaxRow());
			props.set("JWorksheet.AllowCopy=true");
			props.set("JWorksheet.IncrementRowNumbers=false");

			__worksheet = new JScrollWorksheet(renderer, model, props);

			JGUIUtil.addComponent(this, new JLabel("Saved date: "), 0, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
			JGUIUtil.addComponent(this, new JLabel("" + __xmrg.getSavedDate()), 1, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

			JGUIUtil.addComponent(this, new JLabel("Valid date: "), 0, 1, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
			JGUIUtil.addComponent(this, new JLabel("" + __xmrg.getValidDate()), 1, 1, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

			JGUIUtil.addComponent(this, new JLabel("Max Header Value : "), 0, 2, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.EAST);
			JGUIUtil.addComponent(this, new JLabel("" + __xmrg.getMaxValueHeader()), 1, 2, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.WEST);

			JGUIUtil.addComponent(this, __worksheet, 0, 3, 3, 3, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		}
		catch (Exception e)
		{
			JGUIUtil.addComponent(this, new JLabel("No XMRG file read."), 0, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
			Message.printWarning(2, "", e);
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
	}

	/// <summary>
	/// Sets up the layout of the panel.
	/// </summary>
	private void setup()
	{
		setLayout(new GridBagLayout());

		if (string.ReferenceEquals(__xmrgFilename, null))
		{
			JGUIUtil.addComponent(this, new JLabel("No XMRG file read."), 0, 0, 1, 1, 1, 1, GridBagConstraints.NONE, GridBagConstraints.NORTHWEST);
		}
		else
		{
			openFile(__xmrgFilename);
		}
	}

	}

}