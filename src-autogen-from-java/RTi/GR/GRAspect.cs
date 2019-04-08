﻿// GRAspect - holds data fields of information on Aspects.

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

// ---------------------------------------------------------------------------
// GRAspect - holds data fields of information on Aspects.
// ---------------------------------------------------------------------------
// Copyright:	See the COPYRIGHT file.
// ---------------------------------------------------------------------------
//
// History:
// ????-??-??	Steven A. Malers, RTi	Initial version.
// ---------------------------------------------------------------------------

namespace RTi.GR
{
	/// <summary>
	/// This class defines aspect types for GRDrawingArea axes. </summary>
	/// <seealso cref= GRDrawingArea </seealso>
	public class GRAspect
	{

	/// <summary>
	/// True aspect on both axes.
	/// </summary>
	public const int TRUE = 1;
	/// <summary>
	/// Fill both axes.
	/// </summary>
	public const int FILL = 2;
	/// <summary>
	/// True aspect on the Y-axis and filled on the X-axis.
	/// </summary>
	public const int FILLX = 3;
	/// <summary>
	/// True aspect on the X-axis and filled on the Y-axis.
	/// </summary>
	public const int FILLY = 4;

	} // End class GRAspect

}