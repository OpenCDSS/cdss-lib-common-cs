﻿using System;
using System.Collections.Generic;
using System.IO;

// GRPSDevice - GR device to plot to a PostScript file

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
// GRPSDevice - GR device to plot to a PostScript file
// ----------------------------------------------------------------------------
// History:
//
// 2002-01-07	Steven A. Malers, RTi	Update to use new GRDevice interface
//					that allows support of Canvas and
//					JComponent.  Data and some methods are
//					moved into this class.
// ----------------------------------------------------------------------------
// 2003-05-01	J. Thomas Sapienza, RTi	Made changes to accomodate the massive
//					restructuring of GR.java.
// 2003-05-07	JTS, RTi		Made changes following SAM's review.
// 2005-04-26	JTS, RTi		Added finalize().
// 2005-04-29	JTS, RTi		Added anti alias methods (they do 
//					nothing but are now required by 
//					GRDevice).
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{

	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// GR device corresponding to a PostScript file.  Does not extend canvas.
	/// REVISIT (SAM - 2003-05-08)
	/// It is unclear whether we will need this class.  really only need for batch
	/// creation of PostScript files.  Revisit later.  Hopefully all this will still
	/// work.
	/// </summary>
	public class GRPSDevice : GRDevice
	{

	/// <summary>
	/// Maximum lineTo calls with stroke.
	/// </summary>
	protected internal static int _MAXLineTo_count = 1000;

	/// <summary>
	/// Indicates if the device is being used for printing.  For example, the
	/// GRCanvasDevice is used for screen and printed output but for printed output
	/// the Y-Axis does not need to be shifted.
	/// </summary>
	protected internal bool _printing;
	/// <summary>
	/// Indicates that the Y axis must be reversed for the GR zero at the bottom.
	/// </summary>
	protected internal bool _reverse_y;

	/// <summary>
	/// Minimum X coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0x1;
	/// <summary>
	/// Maximum X coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0x2;
	/// <summary>
	/// Minimum Y coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0y1;
	/// <summary>
	/// Maximum Y coordinate, absolute (relative to screen)
	/// </summary>
	protected internal double _dev0y2;

	/// <summary>
	/// Minimum X coordinate, relative.
	/// </summary>
	protected internal double _devx1;
	/// <summary>
	/// Maximum X coordinate, relative.
	/// </summary>
	protected internal double _devx2;
	/// <summary>
	/// Minimum Y coordinate, relative.
	/// </summary>
	protected internal double _devy1;
	/// <summary>
	/// Maximum Y coordinate, relative.
	/// </summary>
	protected internal double _devy2;

	/// <summary>
	/// GRLimits containing the relative points of the display device.
	/// </summary>
	protected internal GRLimits _limits;

	/// <summary>
	/// The graphics used for drawing.
	/// </summary>
	protected internal Graphics _graphics = null;

	/// <summary>
	/// counter for lineTo calls.
	/// </summary>
	protected internal int _LineTo_count = 0;
	/// <summary>
	/// Display mode (allows recording).
	/// </summary>
	protected internal int _mode;
	/// <summary>
	/// Page orientation.  
	/// </summary>
	protected internal int _orientation;
	/// <summary>
	/// Page count.
	/// </summary>
	protected internal int _page;
	/// <summary>
	/// Size that is used by calling drawing routines.  Used in Postscript/page 
	/// systems where drawing can be to one page size with a single "scale"
	/// command.  
	/// </summary>
	protected internal int _sizedrawn;
	/// <summary>
	/// Size of output after scaling.  Used in Postscript/page 
	/// systems where drawing can be to one page size with a single "scale"
	/// command.  
	/// </summary>
	protected internal int _sizeout;
	/// <summary>
	/// Indicates the status of the drawing area.  See GRUtil.STATUS_*.  Might be
	/// an equivalent of a C++ option.
	/// </summary>
	protected internal int _status;
	/// <summary>
	/// Graphics driver type.  Offered because different graphics code might make
	/// different decisions, e.g., Postscript draws thick lines, Canvas does not.
	/// </summary>
	protected internal int _type;
	/// <summary>
	/// Device units.
	/// </summary>
	protected internal int _units;

	/// <summary>
	/// Writer associated with PostScript files.
	/// </summary>
	protected internal PrintWriter _fp = null;

	/// <summary>
	/// Name of this device (assigned by creating code).  It will be used as a window
	/// name if necessary.
	/// </summary>
	protected internal string _name;
	/// <summary>
	/// Note for this device.  Used for simple on-line help for the GUI.
	/// </summary>
	protected internal string _note;
	/// <summary>
	/// New line for platform.
	/// </summary>
	protected internal string _nl = null;
	/// <summary>
	/// Indicates a specific device that requires special attention, such as a plotter.
	/// </summary>
	protected internal string _specific_device = "";

	/// <summary>
	/// List of GRDrawingArea objects for this device.
	/// </summary>
	protected internal IList<GRDrawingArea> _drawing_area_list;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="name"> the name of the GRPSDevice. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GRPSDevice(String name) throws GRException
	public GRPSDevice(string name)
	{
		PropList props = new PropList("GRPSDevice");
		props.set("Name", name);
		initialize(props);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="props"> a PropList specifying settings for the device. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GRPSDevice(RTi.Util.IO.PropList props) throws GRException
	public GRPSDevice(PropList props)
	{
		initialize(props);
	}

	/// <summary>
	/// Constructor. </summary>
	/// <param name="name"> the name of the device. </param>
	/// <param name="limits"> UNUSED </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GRPSDevice(String name, GRLimits limits) throws GRException
	public GRPSDevice(string name, GRLimits limits)
	{
		PropList props = new PropList("GRPSDevice");
		props.set("Name", name);
		initialize(props);
		props = null;
	}

	/// <summary>
	/// Add a drawing area to the device.  The device will then manage the drawing
	/// areas as much as possible. </summary>
	/// <param name="grda"> GRDrawingArea to add. </param>
	public virtual void addDrawingArea(GRDrawingArea grda)
	{
		string routine = "GRDevice.addDrawingArea";

		if (grda == null)
		{
			Message.printWarning(2, routine, "NULL drawing area");
			return;
		}

		if (Message.isDebugOn)
		{
			Message.printDebug(10, routine, "Adding drawing area \"" + grda.getName() + "\" to device \"" + _name + "\"");
		}
		_drawing_area_list.Add((GRPSDrawingArea)grda);
	}

	/// <summary>
	/// Clear the device and fill with white.  Should be defined in derived class.
	/// </summary>
	public virtual void clear()
	{
	}

	/// <summary>
	/// Close the device (used with PS files).  Should be defined in derived class.
	/// </summary>
	public virtual void close()
	{
	}

	/// <summary>
	/// Fill the device with the current color.  Should be defined in derived class.
	/// </summary>
	public virtual void fill()
	{
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GRPSDevice()
	{
		_limits = null;
		_graphics = null;
		_fp = null;
		_name = null;
		_note = null;
		_nl = null;
		_specific_device = null;
		_drawing_area_list = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Flush the device (used by PS devices and X-Windoes).  Should be defined in 
	/// derived class.
	/// </summary>
	public virtual void flush()
	{
		_fp.print("ST" + _nl);
	}

	/// <summary>
	/// Return the current limits of the device.  This should be defined in the
	/// derived class.  This only returns the limits that have been saved for the
	/// device, regardless of device type. </summary>
	/// <returns> the current limits of the device.   </returns>
	public virtual GRLimits getLimits()
	{
		return _limits;
	}

	/// <summary>
	/// Returns the graphics used for drawing. </summary>
	/// <returns> the graphics used for drawing. </returns>
	public virtual Graphics getPaintGraphics()
	{
		return _graphics;
	}

	/// <summary>
	/// Indicate whether the coordinate system has the Y-axis starting at the top.
	/// For postscript files, this method always returns false. </summary>
	/// <returns> whether the coordinate system has the Y-axis starting at the top. </returns>
	public virtual bool getReverseY()
	{
		return _reverse_y;
	}

	/// <summary>
	/// Returns the device type (GR.DEVICE*). </summary>
	/// <returns> The device type (GR.DEVICE*). </returns>
	public virtual int getType()
	{
		return _type;
	}

	/// <summary>
	/// Returns the device units (GRUnits.*) </summary>
	/// <returns> The device units (GRUnits.*). </returns>
	public virtual int getUnits()
	{
		return _units;
	}


	/// <summary>
	/// Returns the page size in points given the page size and orientation. </summary>
	/// <param name="orientation"> Either "landscape" or "portrait". </param>
	/// <param name="size"> Page size ("A", "B", etc.). </param>
	/// <returns> the page size in points given the page size and orientation. </returns>
	public virtual GRLimits getPageSize(string size, string orientation)
	{
		int isize = 0;
		int iorientation = 0;
		if ((size[0] == 'a') || (size[0] == 'A'))
		{
			isize = GRDeviceUtil.SIZE_B;
		}
		else if ((size[0] == 'b') || (size[0] == 'B'))
		{
			isize = GRDeviceUtil.SIZE_B;
		}
		else if ((size[0] == 'c') || (size[0] == 'C'))
		{
			isize = GRDeviceUtil.SIZE_C;
		}
		else if ((size[0] == 'd') || (size[0] == 'D'))
		{
			isize = GRDeviceUtil.SIZE_D;
		}
		else if ((size[0] == 'e') || (size[0] == 'E'))
		{
			isize = GRDeviceUtil.SIZE_E;
		}

		if ((orientation[0] == 'l') || (orientation[0] == 'L'))
		{
			iorientation = GRDeviceUtil.ORIENTATION_LANDSCAPE;
		}
		else if ((orientation[0] == 'p') || (orientation[0] == 'P'))
		{
			iorientation = GRDeviceUtil.ORIENTATION_PORTRAIT;
		}
		return getPageSize(isize, iorientation);
	}

	/// <summary>
	/// Returns the page size in points given the page size and orientation. </summary>
	/// <param name="size"> Page size as defined in GRDeviceUtil.SIZE_* </param>
	/// <param name="orientation"> Either "landscape" or "portrait". </param>
	/// <returns> the page size in points given the page size and orientation. </returns>
	public virtual GRLimits getPageSize(int size, int orientation)
	{
		if (size == GRDeviceUtil.SIZE_B)
		{
			// B-sized...
			if (orientation == GRDeviceUtil.ORIENTATION_PORTRAIT)
			{
				// Portrait...
				return new GRLimits(0.0, 0.0, 792.0, 1224.0);
			}
			else
			{ // Landscape...
				return new GRLimits(0.0, 0.0, 1224.0, 792.0);
			}
		}
		else if (size == GRDeviceUtil.SIZE_D)
		{
			// D-sized...
			if (orientation == GRDeviceUtil.ORIENTATION_PORTRAIT)
			{
				// Portrait...
				return new GRLimits(0.0, 0.0, 1584.0, 2448.0);
			}
			else
			{ // Landscape...
				return new GRLimits(0.0, 0.0, 2448.0, 1584.0);
			}
		}
		else
		{ // Default is A-sized...
			if (orientation == GRDeviceUtil.ORIENTATION_PORTRAIT)
			{
				// Portrait...
				return new GRLimits(0.0, 0.0, 612.0, 792.0);
			}
			else
			{ // Landscape...
				return new GRLimits(0.0, 0.0, 792.0, 612.0);
			}
		}
	}

	/// <summary>
	/// Initializes member variables. </summary>
	/// <param name="props"> PropList with device settings. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void initialize(RTi.Util.IO.PropList props) throws GRException
	private void initialize(PropList props)
	{
		string message, routine = "GRPSDevice.initialize(PropList)";
		string caller = "", note = "", user = "";

		// --- Start from GRDevice ----
		// Set the general information...

		_drawing_area_list = new List<GRDrawingArea>();
		_mode = GRUtil.MODE_DRAW;
		_name = "";
		_note = "";
		_orientation = GRDeviceUtil.ORIENTATION_PORTRAIT;
		_page = 0;
		_printing = false;
		_reverse_y = false;
		_sizedrawn = -1;
		_sizeout = -1;
		_status = GRUtil.STATUS_OPEN;
		_type = 0;
		_units = GRUnits.MM; // Default but needs to be reset.

		/// Set the values that were passed in...

		if (props == null)
		{
			return;
		}

		string prop_value;
		prop_value = props.getValue("Name");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_name = prop_value;
		}
		prop_value = props.getValue("PageSizeDrawn");
		if (!string.ReferenceEquals(prop_value, null))
		{
			if (prop_value[0] == 'A')
			{
				_sizedrawn = GRDeviceUtil.SIZE_A;
			}
			else if (prop_value[0] == 'B')
			{
				_sizedrawn = GRDeviceUtil.SIZE_B;
			}
			else if (prop_value[0] == 'C')
			{
				_sizedrawn = GRDeviceUtil.SIZE_C;
			}
			else if (prop_value[0] == 'D')
			{
				_sizedrawn = GRDeviceUtil.SIZE_D;
			}
			else if (prop_value[0] == 'E')
			{
				_sizedrawn = GRDeviceUtil.SIZE_E;
			}
		}
		prop_value = props.getValue("PageSizeOutput");
		if (!string.ReferenceEquals(prop_value, null))
		{
			if (prop_value[0] == 'A')
			{
				_sizeout = GRDeviceUtil.SIZE_A;
			}
			else if (prop_value[0] == 'B')
			{
				_sizeout = GRDeviceUtil.SIZE_B;
			}
			else if (prop_value[0] == 'C')
			{
				_sizeout = GRDeviceUtil.SIZE_C;
			}
			else if (prop_value[0] == 'D')
			{
				_sizeout = GRDeviceUtil.SIZE_D;
			}
			else if (prop_value[0] == 'E')
			{
				_sizeout = GRDeviceUtil.SIZE_E;
			}
		}
		if ((_sizeout < 0) && (_sizedrawn < 0))
		{
			// Neither specified...
			_sizedrawn = GRDeviceUtil.SIZE_A;
			_sizeout = GRDeviceUtil.SIZE_A;
		}
		else if (_sizedrawn < 0)
		{
			_sizedrawn = _sizeout;
		}
		else if (_sizeout < 0)
		{
			_sizeout = _sizedrawn;
		}
		prop_value = props.getValue("Orientation");
		if (!string.ReferenceEquals(prop_value, null))
		{
			if (prop_value[0] == 'p' || prop_value[0] == 'P')
			{
				_orientation = GRDeviceUtil.ORIENTATION_PORTRAIT;
			}
			else
			{
				_orientation = GRDeviceUtil.ORIENTATION_LANDSCAPE;
			}
		}
		prop_value = props.getValue("Note");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_note = prop_value;
		}

		// Fill in later...
		GRLimits limits = null;
		if (limits == null)
		{
			// Use default sizes...
			_dev0x1 = 0.0;
			_dev0x2 = 1.0;
			_dev0y1 = 0.0;
			_dev0y2 = 1.0;

			_devx1 = 0.0;
			_devx2 = 1.0;
			_devy1 = 0.0;
			_devy2 = 1.0;
			_limits = new GRLimits(_devx1, _devy1, _devx2, _devy2);
		}
		else
		{ // Set the limits...
			setLimits(limits);
		}
		// ---- End from GRDevice -----

		// Initialize general information for driver...

		//_type = POSTSCRIPT;
		_units = GRUnits.POINT;

		// Initialize information from PropList...

		prop_value = props.getValue("Name");
		if (!string.ReferenceEquals(prop_value, null))
		{
			_name = prop_value;
			if (_name.Equals("stdout", StringComparison.OrdinalIgnoreCase))
			{
				// Output is to standard out (remember that this file is
				// included in another file where "stdio.h" has been
				// included...
				_fp = new PrintWriter(System.out);
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Using System.out for output");
				}
			}
			else
			{
				try
				{
					_fp = new PrintWriter(new StreamWriter(_name));
				}
				catch (IOException)
				{
					message = "Can't open file \"" + _name +
					"\".  Check existence and status";
					Message.printWarning(1, routine, message);
					throw new GRException(message);
				}
				if (Message.isDebugOn)
				{
					Message.printDebug(1, routine, "Using \"" + _name + "\" for output");
				}
			}
		}
		prop_value = props.getValue("Caller");
		if (!string.ReferenceEquals(prop_value, null))
		{
			caller = prop_value;
		}
		prop_value = props.getValue("User");
		if (!string.ReferenceEquals(prop_value, null))
		{
			user = prop_value;
		}
		prop_value = props.getValue("Note");
		if (!string.ReferenceEquals(prop_value, null))
		{
			note = prop_value;
		}

		// Set the device size based on the indicated size and orientation.
		// This is needed to set the bounding box information in the file
		// header.  The output size and orientation are trapped in the
		// GRDevice initialize method.
		limits = getPageSize(_sizeout, _orientation);
		if (limits != null)
		{
			_dev0x1 = limits.getLeftX();
			_dev0y1 = limits.getBottomY();
			_dev0x2 = limits.getRightX();
			_dev0y2 = limits.getTopY();
		}

		_LineTo_count = 0;
		_nl = System.getProperty("line.separator");

		// Now check to make sure all is OK...

		if (_fp == null)
		{
			message = "No PostScript file has been opened.  Has name been set " +
			"using Name property?";
			Message.printWarning(1, routine, message);
			throw new GRException(message);
		}
		else
		{
			printHeader(caller, user, note);
		}
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual bool isAntiAliased()
	{
		return false;
	}

	/// <summary>
	/// Indicate whether the device is printing (always returns false). </summary>
	/// <returns> false. </returns>
	public virtual bool isPrinting()
	{
		return false;
	}

	/// <summary>
	/// Indicates a page end.
	/// </summary>
	public virtual void pageEnd()
	{
		_fp.print("PE" + _nl);
	}

	/// <summary>
	/// Starts a postscript page.
	/// </summary>
	public virtual void pageStart()
	{
		++_page;
		_fp.print("%%Page: " + _page + " " + _page + _nl + " PS" + _nl);
	}

	/// <summary>
	/// Print a postscript header with function definitions. </summary>
	/// <param name="caller"> program that is calling the graphics library. </param>
	/// <param name="user"> the user running the program </param>
	/// <param name="note"> a note to be added to the output. </param>
	public virtual void printHeader(string caller, string user, string note)
	{
		string routine = "GRPSDevice.printHeader";

		_fp.print("%!PS-Adobe-2.0" + _nl + "%%Title:  Output for " + caller + " (" + note + ")" + _nl + "%%Creator:  " + user + _nl + "%%BoundingBox:  " + (int)_dev0x1 + " " + (int)_dev0y1 + " " + (int)_dev0x2 + " " + (int)_dev0y2 + _nl + "%%EndComments" + _nl);

		try
		{
			_fp.print("% DrawSize=" + GRUtil.getStringPageSize(_sizedrawn) + " OutSize=" + GRUtil.getStringPageSize(_sizeout) + " Orientation=" + GRUtil.getStringOrientation(_orientation) + "  SpecificDevice=\"" + _specific_device + "\"" + _nl);
		}
		catch (GRException)
		{
			// Trouble looking up strings.  Should never happen.
			Message.printWarning(2, routine, "Error looking up string for a page size, orientation");
		}

		_fp.print("%-----" + _nl + "% WI - string width" + _nl + "%" + _nl + "% Called by: (string) WI" + _nl + "%" + _nl + "/WI {  stringwidth	% size of string" + _nl + "       pop             % don't need height" + _nl + "} bind def" + _nl);
		_fp.print("%-----" + _nl + "% CS - center a string" + _nl + "%" + _nl + "% Called by: (string) x y CS" + _nl + "%" + _nl + "/CS {  moveto          % move to specified point" + _nl + "       dup             % duplicate string on stack" + _nl + "       stringwidth     % size of string" + _nl + "       pop             % don't need height" + _nl + "       2 div           % half width of string" + _nl + "       currentpoint    % current position on page" + _nl + "       3 -2 roll       % shift contents of stack" + _nl + "       exch sub exch   % new coordinates at front of string" + _nl + "       moveto          % move to front" + _nl + "       show            % show string" + _nl + "       } bind def" + _nl);
		// Do not include operations for some types of devices:
		//
		//	HPDesignJet650C		Orientation determined automatically.
		if (_specific_device.Equals("HPDesignJet650C", StringComparison.OrdinalIgnoreCase))
		{
			_fp.print("%-----" + _nl + "% LNSCA - put the printer into landscape mode with the " + "origin at the" + _nl + "%       lower left corner of the page (A-size)." + _nl + "%       Don't need for the HPDesignJet650C." + _nl + "%" + _nl + "/LNSCA { } bind def" + _nl);
		}
		else
		{
			_fp.print("%-----" + _nl + "% LNSCA - put the printer into landscape mode with the " + "origin at the" + _nl + "%         lower left corner of the page (A-size)." + _nl + "%" + _nl + "/LNSCA {90 rotate          % rotate page" + _nl + "        0 -612 translate   % move origin" + _nl + "        } bind def" + _nl);
		}
		// Do not include operations for some types of devices:
		//
		//	HPDesignJet650C		Orientation determined automatically.
		if (_specific_device.Equals("HPDesignJet650C", StringComparison.OrdinalIgnoreCase))
		{
			_fp.print("%-----" + _nl + "% LNSCB - put the printer into landscape mode with the " + "origin at the" + _nl + "%       lower left corner of the page (B-size)." + _nl + "%       Don't need for the HPDesignJet650C." + _nl + "%" + _nl + "/LNSCB { } bind def" + _nl);
		}
		else
		{
			_fp.print("%-----" + _nl + "% LNSCB - put the printer into landscape mode with the " + "origin at the" + _nl + "%        lower left corner of the page (B-size)." + _nl + "%" + _nl + "/LNSCB {90 rotate         % rotate page" + _nl + "        0 -792 translate  % move origin" + _nl + "        } bind def" + _nl);
		}
		_fp.print("%-----" + _nl + "% LNSCD - put the printer into landscape mode with the origin at the" + _nl + "%         lower left corner of the page (D-size)." + _nl + "%" + _nl + "/LNSCD {90 rotate		% rotate page" + _nl + "       0 -1584 translate	% move origin" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% LNSCATOLNSCB - scale all output from A to B" + _nl + "%" + _nl + "/LNSCATOLNSCB {" + _nl + "       1.29 1.29 scale" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% LNSCATOLNSCD - scale all output from A to D" + _nl + "%" + _nl + "/LNSCATOLNSCD {" + _nl + "       2.59 2.59 scale" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% LNSCBTOLNSCA - scale all output from B to A" + _nl + "%" + _nl + "% Should actually translate by 180, but image gets cut off sometimes." + _nl + "%" + _nl + "/LNSCBTOLNSCA {" + _nl + "       0 190 translate" + _nl + "       .64 .64 scale" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% LS - left-justify a string" + _nl + "%" + _nl + "% Called by:  (string) x y LS" + _nl + "%" + _nl + "/LS {  moveto	% move to specified point" + _nl + "       show	% show string" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% LT - redefine \"lineto\"" + _nl + "%" + _nl + "/LT {  lineto } bind def" + _nl);
		_fp.print("%-----" + _nl + "% MT - redefine \"moveto\"" + _nl + "%" + _nl + "/MT {  moveto } bind def" + _nl);
		_fp.print("%-----" + _nl + "% PE - end of page" + _nl + "%" + _nl + "/PE {  stroke" + _nl + "       showpage } bind def" + _nl);
		_fp.print("%-----" + _nl + "% PORTATOPORTB - scale all output from A to B" + _nl + "%" + _nl + "/PORTATOPORTB {" + _nl + "       1.29 1.29 scale" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% PORTATOPORTD - scale all output from A to D" + _nl + "%" + _nl + "/PORTATOPORTD {" + _nl + "       2.59 2.59 scale" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% RS - right-justify a string" + _nl + "%" + _nl + "% Called by: (string) x y RS" + _nl + "%" + _nl + "/RS {  moveto		% move to specified point" + _nl + "       dup		% duplicate string on stack" + _nl + "       stringwidth	% size of string" + _nl + "       pop		% don't need height" + _nl + "       currentpoint	% current position on page" + _nl + "       3 -2 roll	% shift contents of stack" + _nl + "       exch sub exch	% new coordinates at front of string" + _nl + "       moveto		% move to front" + _nl + "       show		% show string" + _nl + "       } bind def" + _nl);
		_fp.print("%-----" + _nl + "% ST - redefine \"stroke\"" + _nl + "%" + _nl + "/ST {	stroke } bind def" + _nl);
		_fp.print("%-----" + _nl + "% PS - operations to do at top of page (start)" + _nl + "%" + _nl + "/PS {");
		if (_orientation == GRDeviceUtil.ORIENTATION_LANDSCAPE)
		{
			if (_sizedrawn == GRDeviceUtil.SIZE_A)
			{
				_fp.print("	LNSCA" + _nl);
				if (_sizeout == GRDeviceUtil.SIZE_B)
				{
					_fp.print("	LNSCATOLNSCB" + _nl);
				}
				if (_sizeout == GRDeviceUtil.SIZE_D)
				{
					_fp.print("	LNSCATOLNSCD" + _nl);
				}
			}
			else if (_sizedrawn == GRDeviceUtil.SIZE_B)
			{
				_fp.print("	LNSCB" + _nl);
				if (_sizeout == GRDeviceUtil.SIZE_A)
				{
					_fp.print("	LNSCBTOLNSCA" + _nl);
				}
			}
		}
		else if (_orientation == GRDeviceUtil.ORIENTATION_PORTRAIT)
		{
			if (_sizedrawn == GRDeviceUtil.SIZE_A)
			{
				if (_sizeout == GRDeviceUtil.SIZE_B)
				{
					_fp.print("	PORTATOPORTB" + _nl);
				}
				if (_sizeout == GRDeviceUtil.SIZE_D)
				{
					_fp.print("	PORTATOPORTD" + _nl);
				}
			}
			else if (_sizedrawn == GRDeviceUtil.SIZE_B)
			{
				if (_sizeout == GRDeviceUtil.SIZE_A)
				{
					_fp.print("	PORTBTOPORTA" + _nl);
				}
			}
		}
		_fp.print("   } bind def" + _nl);
		_fp.print("%" + _nl + "%%EndProlog" + _nl + "%" + _nl);
		_fp.flush();
	}

	/// <summary>
	/// Ends the plot. </summary>
	/// <param name="flag"> flag indicating how the file printer should be closed 
	/// (GRUtil.CLOSE_*) </param>
	public virtual void plotEnd(int flag)
	{
		if ((_fp != null) && !_name.Equals("stdout", StringComparison.OrdinalIgnoreCase))
		{
			_fp.flush();
			if (flag == GRUtil.CLOSE_HARD)
			{
				_fp.close();
			}
		}
	}

	/// <summary>
	/// Resize the device to the given size. </summary>
	/// <param name="x1"> New lower-left X-coordinte of device (usually zero). </param>
	/// <param name="y1"> New lower-left Y-coordinte of device (usually zero). </param>
	/// <param name="x2"> New top-right X-coordinte of device. </param>
	/// <param name="y2"> New top-right Y-coordinte of device. </param>
	public virtual void resize(double x1, double y1, double x2, double y2)
	{
		resize2((int)(x2 - x1), (int)(y2 - y1));
	}

	/// <summary>
	/// Resize the device to the given size.  Rename the routine resize2 because there
	/// is a deprecated resize method in canvas. </summary>
	/// <param name="width"> New width of the device (canvas). </param>
	/// <param name="height"> New height of the device (canvas). </param>
	public virtual void resize2(int width, int height)
	{ // Don't need to actually do anything.
		// setSize ( width, height );
	}

	/// <summary>
	/// Not implemented.
	/// </summary>
	public virtual void setAntiAlias(bool antiAlias)
	{
	}

	/// <summary>
	/// Set the device limits (size) using a GRLimits.  This only sets the limits.  The
	/// device must be resized in the derived class. </summary>
	/// <param name="limits"> GRLimits indicating the size of the device. </param>
	public virtual void setLimits(GRLimits limits)
	{
		_devx1 = limits.getLeftX();
		_devy1 = limits.getBottomY();
		_devx2 = limits.getRightX();
		_devy2 = limits.getTopY();
		_limits = new GRLimits(limits);
		if (Message.isDebugOn)
		{
			string routine = "GRDevice.setLimits";
			Message.printDebug(1, routine, "Setting \"" + _name + "\" device limits to " + limits);
		}
	}

	/// <summary>
	/// Set the Graphics used by the device for drawing.  This Graphics should be
	/// reset at each paint in code that implements a GRDevice because Graphics
	/// resources are typically created and destroyed dynamically by the application. </summary>
	/// <param name="graphics"> instance to use for drawing to the device. </param>
	public virtual void setPaintGraphics(Graphics graphics)
	{
		_graphics = graphics;
	}

	/// <summary>
	/// Set the printing flag.  Set to true when the device is being used for printing,
	/// false when drawing to the screen. </summary>
	/// <param name="printing"> printing flag. </param>
	public virtual void setPrinting(bool printing)
	{
		_printing = printing;
	}

	} // End GRPSDevice

}