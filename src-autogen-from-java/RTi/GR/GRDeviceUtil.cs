using System;

// GRDeviceUtil - Utility functions and data members for devices.

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
// GRDeviceUtil - Utility functions and data members for devices.
// ----------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2003-05-02	J. Thomas Sapienza, RTi	Initial version made by pulling stuff
//					out of other classes and putting it 
//					here.
// 2003-05-07	JTS, RTi		Made changes following review by SAM.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class contains various utility functions and data members that are
	/// of use to all GR Devices.
	/// </summary>
	public class GRDeviceUtil
	{

	/// <summary>
	/// Output has portrait orientation.
	/// </summary>
	public const int ORIENTATION_PORTRAIT = 1;
	/// <summary>
	/// Output has landscape orientation.
	/// </summary>
	public const int ORIENTATION_LANDSCAPE = 2;

	/// <summary>
	/// A-size (8.5 x 11 in) paper.
	/// </summary>
	public const int SIZE_A = 1;
	/// <summary>
	/// B-size (11 x 17 in) paper.
	/// May no longer be needed -- maybe only for batch printing.  
	/// </summary>
	public const int SIZE_B = 2;
	/// <summary>
	/// C-size (17 x 22) paper.
	/// </summary>
	public const int SIZE_C = 3;
	/// <summary>
	/// D-size (22 x 34 in) paper.
	/// </summary>
	public const int SIZE_D = 4;
	/// <summary>
	/// E-size (unknownsize x unknownsize in) paper.
	/// </summary>
	public const int SIZE_E = 5;

	/// <summary>
	/// Close a device. </summary>
	/// <param name="dev"> GR device. </param>
	/// <param name="flag"> Flag indicating whether device is to be closed GR.CLOSE_HARD or
	/// GR.CLOSE_SOFT.  For example, the former is appropriate if a window is to 
	/// go away, the latter if the window is to remain (because it is ultimately 
	/// controlled by some other code). </param>
	public static void closeDevice(GRDevice dev, int flag)
	{

		// Call the driver routine to end the plot...
		dev.plotEnd(flag);

	// REVISIT NECESSARY? (JTS - 2003-05-05)
	// SAM:
	// Leave revisit; can't remember.
		// Now do the internal bookkeeping for the GR devices and drawing
		// areas...
	/*
		for ( int i = 0; i < GRnum_da; i++ ) {
			if ( GRda[i].dev == idev ) {
				// The drawing area is for the device that is being
				// closed so close the drawing area also...
				GRda[i].status = GRSTAT_CLOSED;
			}
		}
	*/
		//dev._status = STAT_CLOSED;
	}

	/// <summary>
	/// Get a new GRDevice.  Depending on the flag that is passed in, this may be a
	/// GRCanvasDevice (screen or for printing) or a GRPSDevice (PostScript file).
	/// Additional devices will be added later.  This method helps to initialize
	/// graphical devices.  Once a device is returned, use the methods in the
	/// GRDevice class to manipulate the object.
	/// 
	/// // REVISIT (JTS - 2003-05-05)
	/// if the type parameter isn't used anymore, why is it even in here?
	/// </summary>
	/// <param name="type"> GRDevice type (see GR.DEVICE*).  This argument is now ignored. </param>
	/// <param name="props"> Property list appropriate for the device.  Define a property of
	/// "Type" to be "PostScript" for a PostScript file, "AWT" for a GRCanvasDevice,
	/// and "Swing" for a GRJComponentDevice.  Anything else will return a 
	/// GRJComponentDevice.  If the props value is null, a GRJComponentDevice will
	/// be returned. </param>
	/// <returns> A new GRDevice appropriate for the desired output product. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static GRDevice getNewDevice(int type, RTi.Util.IO.PropList props) throws GRException
	public static GRDevice getNewDevice(int type, PropList props)
	{
		string propval = null;
		propval = props.getValue("Type");
		if (!string.ReferenceEquals(propval, null))
		{
			if (propval.Equals("PostScript", StringComparison.OrdinalIgnoreCase))
			{
				// PostScript file...
				return new GRPSDevice(props);
			}
			else if (propval.Equals("AWT", StringComparison.OrdinalIgnoreCase))
			{
				return new GRCanvasDevice(props);
			}
			else if (propval.Equals("Swing", StringComparison.OrdinalIgnoreCase))
			{
				return new GRJComponentDevice(props);
			}
			else
			{
				return new GRJComponentDevice(props);
			}
		}
		else
		{
			return new GRJComponentDevice(props);
		}
	}

	/// <summary>
	/// Version to maintain compatibility with legacy code.  This version sets the
	/// property list and then calls the version of this method that accepts a
	/// PropList.
	/// // REVISIT X (JTS - 2003-05-05)
	/// If this method is here to maintain compatibility with legacy code, is it
	/// necessary anymore?
	/// SAM:
	/// Yes, we can deprecate it later. </summary>
	/// <param name="grdevice"> GRDevice to which the GRDrawingArea is associated. </param>
	/// <param name="name"> Name of the drawing area. </param>
	/// <param name="aspect"> Aspect to use for the X and Y axis of the drawing area. </param>
	/// <seealso cref= GRAspect </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static GRDrawingArea getNewDrawingArea(GRDevice grdevice, String name, int aspect) throws GRException
	public static GRDrawingArea getNewDrawingArea(GRDevice grdevice, string name, int aspect)
	{
		PropList props = new PropList(name);
		props.set("Name=" + name);
		if (aspect == GRAspect.TRUE)
		{
			props.set("Aspect=True");
		}
		else if (aspect == GRAspect.FILL)
		{
			props.set("Aspect=Fill");
		}
		else if (aspect == GRAspect.FILLX)
		{
			props.set("Aspect=FillX");
		}
		else if (aspect == GRAspect.FILLY)
		{
			props.set("Aspect=FillY");
		}
		try
		{
			return getNewDrawingArea(grdevice, props);
		}
		catch (GRException e)
		{
			throw e;
		}
	}

	/// <summary>
	/// Get a new GRDrawingArea.  Depending on the device that is passed in,
	/// this may be a GRCanvasDrawingArea (screen or for printing) or a 
	/// GRPSDrawingArea (PostScript file), or a GRJComponentDrawingArea.  
	/// Additional drawing area types will be added later.  
	/// This method helps to initialize graphical drawing areas.  
	/// Once a drawing area is returned, use the methods in the GRDrawingArea 
	/// class to manipulate the object.
	/// // REVISIT DOCUMENTAITON (JTS - 2003-05-05)
	/// update for the other GR Drawing Areas </summary>
	/// <returns> A new GRDrawingArea appropriate for the desired output product. </returns>
	/// <param name="grdevice"> GRDevice that is receiving output. </param>
	/// <param name="props"> Property list appropriate for the drawing area. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static GRDrawingArea getNewDrawingArea(GRDevice grdevice, RTi.Util.IO.PropList props) throws GRException
	public static GRDrawingArea getNewDrawingArea(GRDevice grdevice, PropList props)
	{
		string routine = "GR.getNewDrawingArea";
		string message;

		if (grdevice == null)
		{
			message = "Null GRDevice";
			Message.printWarning(2, routine, message);
			throw new GRException(message);
		}

		// SAMX - this is only used with PostScript makenet in CDSS - need to
		// remove this code which does not to seem as useful as it was
		// originally

		//int type = grdevice.getType();
		try
		{
		/* SAMX
		// REVISIT NECESSARY? (JTS - 2003-05-05)
		can this commented-code be removed?
		SAM:
		I think that I only use for the makenet PostScript.  Revisit later
		once that is folded into a GUI in StateDMI.
			if ( type == GRDevice.DEFAULT_VISUAL ) {
				Message.printDebug ( 1, routine,
				"Creating new Java drawing area" );
	//			return new GRCanvasDrawingArea ( (GRCanvasDevice)grdevice,
	//				props );
			}
			else if	( type == GRDevice.DEFAULT_PRINTER ) {
				// Need to figure this out...
				Message.printDebug ( 1, routine,
				"Creating new Java Printer drawing area" );
	//			return new GRCanvasDrawingArea ( (GRCanvasDevice)grdevice,
	//			props );
			}
	*/
			//else if ( type == GRDevice.POSTSCRIPT ) {
				// PostScript file...
			if (grdevice is GRPSDevice)
			{
				Message.printDebug(1, routine, "Creating new PostScript drawing area");
				GRPSDrawingArea ps = null;
				ps = new GRPSDrawingArea((GRPSDevice)grdevice, props);
				grdevice.addDrawingArea(ps);
				return ps;
			}
			else if (grdevice is GRCanvasDevice)
			{
				Message.printDebug(1, routine, "Creating new canvas drawing area");
				GRCanvasDrawingArea c = new GRCanvasDrawingArea((GRCanvasDevice)grdevice, props);
				grdevice.addDrawingArea(c);
				return c;
			}
			else if (grdevice is GRJComponentDevice)
			{
				Message.printDebug(1, routine, "Creating new swing drawing area");
				GRJComponentDrawingArea s = new GRJComponentDrawingArea((GRJComponentDevice)grdevice, props);
				grdevice.addDrawingArea(s);
				return s;
			}
			else
			{
				throw new GRException("Unrecognized device type: " + grdevice);
			}
			//}
		}
		catch (GRException e)
		{
			throw e;
		}

		//message="Device is of unknown type.  Unable to create drawing area.";
		//Message.printWarning ( 2, routine, message );
		//throw new GRException ( message );
	}



	}

}