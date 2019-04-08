using System;

// GeoViewApp - simple GeoView application

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
// GeoViewApp - simple GeoView application
// ----------------------------------------------------------------------------
// History:
//
// 2001-11-16	Steven A. Malers, RTi	Initial version.  Test concept of
//					general applet tool.
// 2003-10-06	J. Thomas Sapienza, RTi	Converted to Swing.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// REVISIT (JTS - 2006-05-23)
	/// I'm not sure this is necessary anymore.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GeoViewApp extends java.applet.Applet
	public class GeoViewApp : Applet
	{

	/// <summary>
	/// Instantiates an applet instance.
	/// </summary>
	public virtual void init()
	{
		IOUtil.setApplet(this);
		IOUtil.setProgramData("GeoView", "01.00.00", null);

		string gvp = getParameter("GeoViewProject");
		JFrame f = new JFrame();
		GeoViewJPanel gv = new GeoViewJPanel(f, new PropList(""));
		try
		{
			gv.openGVP(gvp);
			add(gv);
		}
		catch (Exception)
		{
			Message.printWarning(1, "GeoViewApp.init", "Error opening GeoView Project file");
		}
		f = null;
		gv = null;
		gvp = null;
	}

	}

}