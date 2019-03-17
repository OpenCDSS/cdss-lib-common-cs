// GRDevice - GR interface for a drawable device

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
// GRDevice - GR interface for a drawable device
// ----------------------------------------------------------------------------
// History:
//
// 2002-01-07	Steven A. Malers, RTi	Major rework.  The previous GRDevice
//					class extended Canvas.  In order to
//					support JComponent (lightweight Swing)
//					and Canvas (heavyweight AWT), the
//					GRDevice has been folded into the
//					GRCanvasDevice, GRJComponentDevice, and
//					GRPSDevice classes and the GRDevice
//					interface has been defined to allow
//					GRDrawingArea to remain generic.  This
//					results in some redundant data and code
//					in the classes but is necessary to
//					support the different windowing
//					environments.  Define in this interface
//					all the "virtual" methods that were
//					defined in the GRDevice class
//					previously.
// 2003-05-07	J. Thomas Sapienza, RTi	Made changes following review by SAM.
// 2005-04-29	JTS, RTi		Added isAntiAliased() and
//					setAntiAlias().
// ----------------------------------------------------------------------------

namespace RTi.GR
{

	// REVISIT (JTS - 2003-05-05)
	// since this class is abstract, should the methods below be made abstract?

	/// <summary>
	/// GR interface for a drawable device.
	/// </summary>
	public abstract interface GRDevice
	{

	/// <summary>
	/// Add a drawing area to the device.  The device will then manage the drawing
	/// areas as much as possible. </summary>
	/// <param name="grda"> GRDrawingArea to add. </param>
	void addDrawingArea(GRDrawingArea grda);

	/// <summary>
	/// Return the current limits of the device, in device units.  This should be
	/// defined in the derived class. </summary>
	/// <returns> the current limits of the device. </returns>
	GRLimits getLimits();

	/// <summary>
	/// Return the Graphics instance to use for drawing.  This is typically a shared
	/// resource set when a GRCanvasDevice or GRJComponentDevice paint() method is
	/// called. </summary>
	/// <returns> the Graphics instance to use for drawing. </returns>
	Graphics getPaintGraphics();

	/// <summary>
	/// Indicate whether the device has a "reversed" Y axis (one that starts at the top
	/// and increments down). </summary>
	/// <returns> true if the Y axis starts at the top (upper left) and goes down (lower
	/// left). </returns>
	bool getReverseY();

	/// <summary>
	/// Return the device units (GRUnits.*). </summary>
	/// <returns> The device units. </returns>
	int getUnits();

	/// <summary>
	/// Returns whether the graphics are currently being drawn antialiased. </summary>
	/// <returns> true if antialiased, false if not. </returns>
	bool isAntiAliased();

	/// <summary>
	/// Indicate whether the device is in the process of printing. </summary>
	/// <returns> true if the device is in the process of printing, false if not. </returns>
	bool isPrinting();

	/// <summary>
	/// Complete the plot. </summary>
	/// <param name="close_flag"> GRUtil.CLOSE_HARD to close the output completely (e.g., 
	/// to close an output file). </param>
	void plotEnd(int close_flag);

	/// <summary>
	/// Sets whether the graphics should be drawn anti aliased. </summary>
	/// <param name="antiAlias"> if true, the graphics will be drawn anti aliased. </param>
	void setAntiAlias(bool antiAlias);

	/// <summary>
	/// Set the Graphics used by the device for drawing.  This Graphics should be
	/// reset at each paint() call in code that implements a GRDevice because Graphics
	/// resources are typically created and destroyed dynamically by the application.
	/// The Graphics will be used by all GRDrawingArea associated with the device. </summary>
	/// <param name="graphics"> instance to use for drawing to the device. </param>
	void setPaintGraphics(Graphics graphics);

	} // End GRDevice

}