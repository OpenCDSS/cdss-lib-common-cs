// ERDiagram_DrawingArea - the graphical drawing area onto which the ER Diagram is drawn

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
// ERDiagram_DrawingArea - the drawing area onto which the ER Diagram is
//	drawn.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-08-27	J. Thomas Sapienza, RTi	Initial changelog.  
// ----------------------------------------------------------------------------

namespace RTi.DMI
{
	using GRAspect = RTi.GR.GRAspect;
	using GRJComponentDrawingArea = RTi.GR.GRJComponentDrawingArea;
	using GRLimits = RTi.GR.GRLimits;
	using GRUnits = RTi.GR.GRUnits;

	/// <summary>
	/// Class that is used as the drawing area for an ER Diagram.
	/// </summary>
	public class ERDiagram_DrawingArea : GRJComponentDrawingArea
	{

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dev"> the ERDiagram_Device to use this drawing area with. </param>
	/// <param name="drawingLimits"> the drawingLimits of the drawing area. </param>
	public ERDiagram_DrawingArea(ERDiagram_Device dev, GRLimits drawingLimits) : base(dev, "ERDiagram_DrawingArea", GRAspect.TRUE, drawingLimits, GRUnits.DEVICE, GRLimits.DEVICE, drawingLimits)
	{
	}

	}

}