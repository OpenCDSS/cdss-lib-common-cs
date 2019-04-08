using System;

// GRSymbol - store symbol definition information for points, lines, polygons

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
// GRSymbol - store symbol definition information for points, lines, polygons
// ----------------------------------------------------------------------------
// History:
//
// 2000-10-15	Steven A. Malers, RTi	Add SYMBOL_NAMES array.
// 2001-09-17	SAM, RTi		Add classification data to allow
//					polygons to be drawn as colored
//					polygons.
// 2001-10-10	SAM, RTi		Add getStyle() to start phasing in line
//					and polygon styles.  Add getColorTable()
//					so it can be displayed in properties
//					dialog.  Add _label_field and
//					_label_format and associated set/get
//					methods.
// 2001-10-15	SAM, RTi		Change toInteger() to throw an exception
//					if the string symbol name cannot be
//					matched.  Add _label_selected_only
//					property to help clarify displays.
// 2001-12-02	SAM, RTi		Add getColorNumber().
// 2002-07-11	SAM, RTi		Add more symbol names/numbers to lookup
//					arrays to support more applications.
// 2002-07-23	SAM, RTi		Add VerticalBar-Signed and
//					TeaCup symbols.  Remove deprecated
//					static data.  Add isPrimarySymbol() to
//					allow an array of GRSymbol to be used
//					where one symbol is treated as a primary
//					symbol (e.g., to draw polygon where
//					secondary symbols are drawn at the
//					centroid).  Save the size as an X and
//					Y size to facilitate symbols like bars.
//					Change symbol "subtype" to "style".
// 2002-09-24	SAM, RTi		Break out the scaled classification
//					symbol data into the
//					GRScaledClassificationSymbol class.
// 2002-12-19	SAM, RTi		Add transparency data field for use with
//					filled polygons.
// ----------------------------------------------------------------------------
// 2003-05-08	J. Thomas Sapienza, RTi	Made changes following SAM's review
// 2004-08-10	JTS, RTi		Added support for scaled teacup symbols.
// 2004-09-16	JTS, RTi		Corrected 1-off bug in getting the 
//					colors for class break symbols.
// 2004-10-06	JTS, RTi		Added new symbol:
//					  VBARUNSIGNED
// 2004-10-26	JTS, RTi		Added new symbol:
//					  CIRCLE_PLUS
// 2004-10-27	JTS, RTi		Implements Cloneable.
// 2005-04-26	JTS, RTi		finalize() uses IOUtil.nullArray().
// 2006-02-08	JTS, RTi		Added:
//					  SYM_FUTRI_TOPLINE
//					  SYM_UTRI_TOPLINE
//					  SYM_FDTRI_BOTLINE
//					  SYM_DTRI_BOTLINE
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	using IOUtil = RTi.Util.IO.IOUtil;

	using StringUtil = RTi.Util.String.StringUtil;

	// TODO SAM 2010-12-25 Need to evaluate using enumeration for symbols
	/// <summary>
	/// This class stores information necessary to draw symbology.  Symbols may be used
	/// with points, lines (in which case a line style, width, and color are used),
	/// and polygons (in which case color, outline color, and fill pattern are used).
	/// Consequently, symbols can be used for graphs, maps, and other visualization.
	/// Symbols can currently be varied in color based on a classification; however,
	/// varying the size (e.g., for point symbols) is NOT enabled.  If classification
	/// is used, then additional data like a color table are used.  Typically the color
	/// table is constructed using GRColorTable.createColorTable().  If multiple symbols
	/// are used for a shape, an array of GRSymbol can be used (e.g., see GRLegend).
	/// Additional features may be added later to help with optimization of multi-symbol layout.
	/// NEED TO ADD DERIVED CLASSES TO STORE THE MORE SPECIFIC INFORMATION (like
	/// color table) SO THIS CLASS IS NOT SO BLOATED.
	/// TODO (JTS - 2003-05-05) color table has been moved out, but this still has a lot of stuff in it.
	/// does it need any anti-bloating?
	/// SAM: agree -- later
	/// </summary>
	public class GRSymbol : ICloneable
	{

	/// <summary>
	/// Symbol types.  The first is no symbol.
	/// </summary>
	public const int TYPE_NONE = 0;

	/// <summary>
	/// Draw a point symbol.  Color and symbol size can be specified.
	/// </summary>
	public const int TYPE_POINT = 1;

	/// <summary>
	/// Draw a line symbol.  Color, line width, and pattern can be specified.
	/// </summary>
	public const int TYPE_LINE = 2;

	/// <summary>
	/// Draw a line symbol with points.  This is a combination of lines and points, suitable for a graph.
	/// </summary>
	public const int TYPE_LINE_AND_POINTS = 3;

	/// <summary>
	/// Fill a polygon.  Color, outline color, and pattern can be specified.
	/// </summary>
	public const int TYPE_POLYGON = 4;

	/// <summary>
	/// Fill a polygon and draw a symbol.  Symbol, color, outline color, and pattern can be specified.
	/// </summary>
	public const int TYPE_POLYGON_AND_POINT = 5;

	/// <summary>
	/// Names for symbols, for use with persistent storage.  These are consistent with
	/// SYMBOL_NUMBERS.  Only symbols that are useful in a final application are
	/// included (not building-block symbols).
	/// </summary>
	public static readonly string[] SYMBOL_NAMES = new string[] {"None", "Arrow-Down", "Arrow-Down-Left", "Arrow-Down-Right", "Arrow-Left", "Arrow-Right", "Arrow-Up", "Arrow-Up-Left", "Arrow-Up-Right", "Asterisk", "Circle-Hollow", "Circle-Filled", "Circle-Plus", "Diamond-Hollow", "Diamond-Filled", "InstreamFlow", "Plus", "Plus-Square", "Square-Hollow", "Square-Filled", "TeaCup", "Triangle-Down-Hollow", "Triangle-Down-Filled", "Triangle-Left-Hollow", "Triangle-Left-Filled", "Triangle-Right-Hollow", "Triangle-Right-Filled", "Triangle-Up-Hollow", "Triangle-Up-Filled", "VerticalBar-Signed", "X", "X-Cap", "X-Diamond", "X-Edge", "X-Square", "Triangle-Up-Filled-Topline", "Triangle-Down-Filled-Botline", "Triangle-Up-Hollow-Topline", "Triangle-Down-Hollow-Botline", "Pushpin-Vertical"};

	/// <summary>
	/// Simple line symbol: no symbol</b>
	/// </summary>
	public const int SYM_NONE = 0;
	/// <summary>
	/// Simple line symbol: <b>circle</b>
	/// </summary>
	public const int SYM_CIR = 1;
	/// <summary>
	/// Simple line symbol: <b>square</b>
	/// </summary>
	public const int SYM_SQ = 2;
	/// <summary>
	/// Simple line symbol: <b>triangle pointing up</b>
	/// </summary>
	public const int SYM_UTRI = 3;
	/// <summary>
	/// Simple line symbol: <b>triangle pointing down</b>
	/// </summary>
	public const int SYM_DTRI = 4;
	/// <summary>
	/// Simple line symbol: <b>triangle pointing left</b>
	/// </summary>
	public const int SYM_LTRI = 5;
	/// <summary>
	/// Simple line symbol: <b>triangle pointing right</b>
	/// </summary>
	public const int SYM_RTRI = 6;
	/// <summary>
	/// Simple line symbol: <b>diamond</b>
	/// </summary>
	public const int SYM_DIA = 7;

	/// <summary>
	/// Compound line symbol: <b>+</b>
	/// </summary>
	public const int SYM_PLUS = 8;
	/// <summary>
	/// Compound line symbol: <b>+ with square around it</b>
	/// </summary>
	public const int SYM_PLUSQ = 9;
	/// <summary>
	/// Compound line symbol: <b>X</b>
	/// </summary>
	public const int SYM_EX = 10;
	/// <summary>
	/// Compound line symbol: <b>X with line over top and bottom</b>
	/// </summary>
	public const int SYM_EXCAP = 11;
	/// <summary>
	/// Compound line symbol: <b>X with lines on edge</b>
	/// </summary>
	public const int SYM_EXEDGE = 12;
	/// <summary>
	/// Compound line symbol: <b>X with square around it</b>
	/// </summary>
	public const int SYM_EXSQ = 13;
	/// <summary>
	/// Compound line symbol: <b>Right arrow</b>
	/// </summary>
	public const int SYM_RARR = 14;
	/// <summary>
	/// Compound line symbol: <b>Left arrow</b>
	/// </summary>
	public const int SYM_LARR = 15;
	/// <summary>
	/// Compound line symbol: <b>Up arrow</b>
	/// </summary>
	public const int SYM_UARR = 16;
	/// <summary>
	/// Compound line symbol: <b>Down arrow</b>
	/// </summary>
	public const int SYM_DARR = 17;
	/// <summary>
	/// Compound line symbol: <b>Asterisk</b>
	/// </summary>
	public const int SYM_AST = 18;
	/// <summary>
	/// Compound line symbol: <b>X with diamond around it</b>
	/// </summary>
	public const int SYM_EXDIA = 19;

	/// <summary>
	/// Simple filled symbol: <b>filled circle</b>
	/// </summary>
	public const int SYM_FCIR = 20; // filled circle
	/// <summary>
	/// Simple filled symbol: <b>filled square</b>
	/// </summary>
	public const int SYM_FSQ = 21; // filled square
	/// <summary>
	/// Simple filled symbol: <b>filled triangle pointing up</b>
	/// </summary>
	public const int SYM_FUTRI = 22; // filled up triangle
	/// <summary>
	/// Simple filled symbol: <b>filled triangle pointing down</b>
	/// </summary>
	public const int SYM_FDTRI = 23; // filled down triangle
	/// <summary>
	/// Simple filled symbol: <b>filled triangle pointing left</b>
	/// </summary>
	public const int SYM_FLTRI = 24; // filled left triangle
	/// <summary>
	/// Simple filled symbol: <b>filled triangle pointing right</b>
	/// </summary>
	public const int SYM_FRTRI = 25; // filled right triangle
	/// <summary>
	/// Simple filled symbol: <b>filled diamond</b>
	/// </summary>
	public const int SYM_FDIA = 26; // filled diamond

	// Start complicated filled symbols

	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in 
	/// top diamond, nothing in other three number areas.
	/// </summary>
	public const int SYM_EXDIA1 = 27;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "2" in right 
	/// diamond, nothing in other three number areas.
	/// </summary>
	public const int SYM_EXDIA2 = 28;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "3" in bottom 
	/// diamond, nothing in other three number areas.
	/// </summary>
	public const int SYM_EXDIA3 = 29;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "4" in bottom 
	/// diamond, nothing in other three number areas.
	/// </summary>
	public const int SYM_EXDIA4 = 30;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in top 
	/// diamond, "2" in right diamond, nothing in other two number areas.
	/// </summary>
	public const int SYM_EXDIA12 = 31;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in top 
	/// diamond, "2" in right diamond, "3" in bottom diamond, nothing in fourth diamond.
	/// </summary>
	public const int SYM_EXDIA123 = 32;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in top 
	/// diamond, "2" in right diamond, "3" in bottom diamond, "4" in left diamond.
	/// </summary>
	public const int SYM_EXDIA1234 = 33;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in top 
	/// diamond, "3" in bottom diamond, nothing in other two number areas.
	/// </summary>
	public const int SYM_EXDIA13 = 34;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in top 
	/// diamond, "4" in left diamond, nothing in other two number areas.
	/// </summary>
	public const int SYM_EXDIA14 = 35;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "2" in right 
	/// diamond, "3" in bottom diamond, nothing in other two number areas.
	/// </summary>
	public const int SYM_EXDIA23 = 36;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "2" in right 
	/// diamond, "4" in left diamond, nothing in other two number areas.
	/// </summary>
	public const int SYM_EXDIA24 = 37;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "2" in right 
	/// diamond, "3" in bottom diamond, "4" in left diamond, nothing in fourth number area.
	/// </summary>
	public const int SYM_EXDIA234 = 38;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in top 
	/// number area, "2" in right diamond, "4" in left diamond, nothing in fourth number area.
	/// </summary>
	public const int SYM_EXDIA124 = 39;
	/// <summary>
	/// Complicated filled symbol: Diamond made of four diamonds with "1" in top 
	/// number area, "3" in bottom diamond, "4" in left diamond, nothing in 2nd diamond area.
	/// </summary>
	public const int SYM_EXDIA134 = 40;

	/// <summary>
	/// Complicated filled symbol: <b>square filled on top</b>
	/// </summary>
	public const int SYM_TOPFSQ = 41;
	/// <summary>
	/// Complicated filled symbol: <b>square filled on bottom</b>
	/// </summary>
	public const int SYM_BOTFSQ = 42;
	/// <summary>
	/// Complicated filled symbol: <b>square filled on right</b>
	/// </summary>
	public const int SYM_RFSQ = 43;
	/// <summary>
	/// Complicated filled symbol: <b>square filled on left</b>
	/// </summary>
	public const int SYM_LFSQ = 44;
	/// <summary>
	/// Complicated filled symbol: <b>filled arrow to upper right</b>
	/// </summary>
	public const int SYM_FARR1 = 45;
	/// <summary>
	/// Complicated filled symbol: <b>filled arrow to lower right</b>
	/// </summary>
	public const int SYM_FARR2 = 46;
	/// <summary>
	/// Complicated filled symbol: <b>filled arrow to lower left</b>
	/// </summary>
	public const int SYM_FARR3 = 47;
	/// <summary>
	/// Complicated filled symbol: <b>filled arrow to upper left</b>
	/// </summary>
	public const int SYM_FARR4 = 48;
	/// <summary>
	/// Complicated filled symbol: <b>instream flow symbol</b>
	/// </summary>
	public const int SYM_INSTREAM = 49;
	/// <summary>
	/// Complicated filled symbol: <b>tea cup symbol</b>
	/// </summary>
	public const int SYM_TEACUP = 50;
	/// <summary>
	/// Complicated filled symbol: <b>vertical bar where positive values are above
	/// center and negative values are below center</b>
	/// </summary>
	public const int SYM_VBARSIGNED = 51;

	/// <summary>
	/// Building blocks (incomplete symbols): <b>minus (-)</b>
	/// </summary>
	public const int SYM_MIN = 52;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>bar (|)</b>
	/// </summary>
	public const int SYM_BAR = 53;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>forward slash (/)</b>
	/// </summary>
	public const int SYM_FSLASH = 54;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>backslash (\)</b>
	/// </summary>
	public const int SYM_BSLASH = 55;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>line on top</b>
	/// </summary>
	public const int SYM_TOPLINE = 56;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>line on bottom</b>
	/// </summary>
	public const int SYM_BOTLINE = 57;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>line on right</b>
	/// </summary>
	public const int SYM_RLINE = 58;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>line on left</b>
	/// </summary>
	public const int SYM_LLINE = 59;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>lines on top and bottom</b>
	/// </summary>
	public const int SYM_CAP = 60;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>lines on left and right</b>
	/// </summary>
	public const int SYM_EDGE = 61;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>caret (^)</b>
	/// </summary>
	public const int SYM_UCAR = 62;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>down caret</b>
	/// </summary>
	public const int SYM_DCAR = 63;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>left caret</b>
	/// </summary>
	public const int SYM_LCAR = 64;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>right caret</b>
	/// </summary>
	public const int SYM_RCAR = 65;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>smaller X that can be placed inside of a diamond</b>
	/// </summary>
	public const int SYM_EXFORDIA = 66;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled top quad of a diamond</b>
	/// </summary>
	public const int SYM_FDIA1 = 67;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled right quad of a diamond</b>
	/// </summary>
	public const int SYM_FDIA2 = 68;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled bottom quad of a diamond</b>
	/// </summary>
	public const int SYM_FDIA3 = 69;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled left quad of a diamond</b>
	/// </summary>
	public const int SYM_FDIA4 = 70;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled upper right triangle in square</b>
	/// </summary>
	public const int SYM_FSQTRI1 = 71;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled lower right triangle in square</b>
	/// </summary>
	public const int SYM_FSQTRI2 = 72;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled lower left triangle in square</b>
	/// </summary>
	public const int SYM_FSQTRI3 = 73;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled upper left triangle in square</b>
	/// </summary>
	public const int SYM_FSQTRI4 = 74;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled upper half of diamond</b>
	/// </summary>
	public const int SYM_FTOPDIA = 75;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled bottom half of diamond</b>
	/// </summary>
	public const int SYM_FBOTDIA = 76;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled right half of diamond</b>
	/// </summary>
	public const int SYM_FRDIA = 77;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled left half of diamond</b>
	/// </summary>
	public const int SYM_FLDIA = 78;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled upper 1/4 of diamond</b>
	/// </summary>
	public const int SYM_FTOPDIA4 = 79;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled bottom 1/4 of diamond</b>
	/// </summary>
	public const int SYM_FBOTDIA4 = 80;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled right 1/4 of diamond</b>
	/// </summary>
	public const int SYM_FRDIA4 = 81;
	/// <summary>
	/// Building blocks (incomplete symbols): <b>filled left 1/4 of diamond</b>
	/// </summary>
	public const int SYM_FLDIA4 = 82;
	/// <summary>
	/// Complicated filled symbol: <b>vertical bar where all values are positive</b>.
	/// The bar is centered on its X location and rises up from its Y location.
	/// </summary>
	public const int SYM_VBARUNSIGNED = 83;
	/// <summary>
	/// Compound line symbol: <b>+ with circle around it</b>
	/// </summary>
	public const int SYM_PLUSCIR = 84;

	/// <summary>
	/// Compound symbol: <b>Filled triangle pointing up with a line on the point.</b>
	/// </summary>
	public const int SYM_FUTRI_TOPLINE = 85;

	/// <summary>
	/// Compound symbol: <b>Filled triangle pointing down with a line on the point.</b>
	/// </summary>
	public const int SYM_FDTRI_BOTLINE = 86;

	/// <summary>
	/// Compound symbol: <b>Hollow triangle pointing up with a line on the point.</b>
	/// </summary>
	public const int SYM_UTRI_TOPLINE = 87;

	/// <summary>
	/// Compound symbol: <b>Hollow triangle pointing down with a line on the point.</b>
	/// </summary>
	public const int SYM_DTRI_BOTLINE = 88;

	/// <summary>
	/// Complex symbol: <b>Filled push-pin, vertical (no lean).</b>
	/// </summary>
	public const int SYM_PUSHPIN_VERTICAL = 89;

	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>first "nice" symbol</b>
	/// </summary>
	public const int SYM_FIRST = SYM_CIR;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>last "nice" symbol</b>
	/// </summary>
	public const int SYM_LAST = SYM_FARR4;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>last of all symbols</b>
	/// </summary>
	public const int SYM_LASTALL = SYM_FLDIA4;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>first outline symbol</b>
	/// </summary>
	public const int SYM_FIRST_OUT = SYM_CIR;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>last outline symbol</b>
	/// </summary>
	public const int SYM_LAST_OUT = SYM_DIA;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>first filled symbol</b>
	/// </summary>
	public const int SYM_FIRST_FILL = SYM_FCIR;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>last filled symbol</b>
	/// </summary>
	public const int SYM_LAST_FILL = SYM_FDIA;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>first line symbol</b>
	/// </summary>
	public const int SYM_FIRST_LINE = SYM_CIR;
	/// <summary>
	/// Summary definition (used by routines to limit symbol selection): <b>last line symbol</b>
	/// </summary>
	public const int SYM_LAST_LINE = SYM_EXDIA;

	/// <summary>
	/// Orientation for symbols.  The following means the left side of the symbol is at the coordinate.
	/// </summary>
	public const int SYM_LEFT = 0x01;
	/// <summary>
	/// Orientation for symbols.  The following means the device is centered around the X point of the coordinate
	/// </summary>
	public const int SYM_CENTER_X = 0x02;
	/// <summary>
	/// Orientation for symbols.  The following means the right side of the symbol is at the coordinate.
	/// </summary>
	public const int SYM_RIGHT = 0x04;
	/// <summary>
	/// Orientation for symbols.  The following means the bottom side of the symbol is at the coordinate.
	/// </summary>
	public const int SYM_BOTTOM = 0x08;
	/// <summary>
	/// Orientation for symbols.  The following means the top side of the symbol is at the coordinate.
	/// </summary>
	public const int SYM_TOP = 0x10;
	/// <summary>
	/// Orientation for symbols.  The following means the device is centered around the Y point of the coordinate
	/// </summary>
	public const int SYM_CENTER_Y = 0x20;

	/// <summary>
	/// Symbol numbers.  These are consistent with SYMBOL_NAMES.
	/// </summary>
	public static readonly int[] SYMBOL_NUMBERS = new int[] {SYM_NONE, SYM_DARR, SYM_FARR3, SYM_FARR2, SYM_LARR, SYM_RARR, SYM_UARR, SYM_FARR4, SYM_FARR1, SYM_AST, SYM_CIR, SYM_FCIR, SYM_PLUSCIR, SYM_DIA, SYM_FDIA, SYM_INSTREAM, SYM_PLUS, SYM_PLUSQ, SYM_SQ, SYM_FSQ, SYM_TEACUP, SYM_DTRI, SYM_FDTRI, SYM_LTRI, SYM_FLTRI, SYM_RTRI, SYM_FRTRI, SYM_UTRI, SYM_FUTRI, SYM_VBARSIGNED, SYM_EX, SYM_EXCAP, SYM_EXDIA, SYM_EXEDGE, SYM_EXSQ, SYM_FUTRI_TOPLINE, SYM_FDTRI_BOTLINE, SYM_UTRI_TOPLINE, SYM_DTRI_BOTLINE, SYM_PUSHPIN_VERTICAL};

	/// <summary>
	/// Names for symbol classifications, for use with persistent storage and graphical
	/// user interfaces.  These are consistent with CLASSIFICATION_NUMBERS.
	/// </summary>
	public static readonly string[] CLASSIFICATION_NAMES = new string[] {"SingleSymbol", "UniqueValues", "ClassBreaks", "ScaledSymbol", "ScaledTeacupSymbol"};

	/// <summary>
	/// Classification number for a single symbol.
	/// </summary>
	public const int CLASSIFICATION_SINGLE = 0;
	/// <summary>
	/// Classification number for one symbol per value, but all symbols are 
	/// the same other than color or other characteristics that can automatically be assigned.
	/// </summary>
	public const int CLASSIFICATION_UNIQUE_VALUES = 1;
	/// <summary>
	/// Classification number for class breaks, in which the symbol is graded based on
	/// the data value into groups of colors, sizes, patterns, etc.
	/// </summary>
	public const int CLASSIFICATION_CLASS_BREAKS = 2;
	/// <summary>
	/// Classification number for a single symbol style, but the size of the symbol is
	/// scaled to "exact" data size based on a data value (unlike unique values
	/// where different symbols are used for each value or class breaks where symbols
	/// have definite breaks).
	/// </summary>
	public const int CLASSIFICATION_SCALED_SYMBOL = 3;

	/// <summary>
	/// Classification number for a single symbol style, where the size of the teacup
	/// is scaled and the amount that the teacup is filled is dependent on values
	/// read from an attribute table.
	/// </summary>
	public const int CLASSIFICATION_SCALED_TEACUP_SYMBOL = 4;

	/// <summary>
	/// Symbol classification type numbers.  These are consistent with CLASSIFICATION_NAMES.
	/// </summary>
	public static readonly int[] CLASSIFICATION_NUMBERS = new int[] {CLASSIFICATION_SINGLE, CLASSIFICATION_UNIQUE_VALUES, CLASSIFICATION_CLASS_BREAKS, CLASSIFICATION_SCALED_SYMBOL, CLASSIFICATION_SCALED_TEACUP_SYMBOL};

	/// <summary>
	/// Indicates whether only selected shapes should be labeled.  This is useful to clarify displays.
	/// </summary>
	private bool _label_selected_only = false;

	/// <summary>
	/// Indicates whether the symbol is a primary symbol (simple).
	/// </summary>
	private bool __is_primary = false;

	/// <summary>
	/// Array of double precision data corresponding to colors in the color table.
	/// </summary>
	private double[] _double_data = null;
	/// <summary>
	/// Font height in points for symbol labels.
	/// </summary>
	private double _label_font_height = 10.0;
	/// <summary>
	/// Symbol size in the X direction.
	/// </summary>
	private double __size_x = 0.0;
	/// <summary>
	/// Symbol size in the Y direction.
	/// </summary>
	private double __size_y = 0.0;

	/// <summary>
	/// Foreground color.
	/// </summary>
	private GRColor _color = null;

	/// <summary>
	/// Secondary foreground color (e.g., for negative bars)
	/// </summary>
	private GRColor _color2 = null;

	/// <summary>
	/// Outline color.
	/// </summary>
	private GRColor _outline_color = null;

	/// <summary>
	/// A color table is used when data fields are classified for output.
	/// </summary>
	private GRColorTable _color_table = null;

	/// <summary>
	/// The type of classification for this symbol.
	/// </summary>
	private int _classification_type = CLASSIFICATION_SINGLE;
	/// <summary>
	/// Array of integer data corresponding to colors in the color table.
	/// </summary>
	private int[] _int_data = null;
	/// <summary>
	/// Label position (left-justified against the symbol, centered in Y direction).
	/// </summary>
	private int _label_position = GRText.LEFT | GRText.CENTER_Y;
	/// <summary>
	/// Symbol style.
	/// </summary>
	private int _style = SYM_NONE;
	/// <summary>
	/// Transparency level.  Default is totally opaque.  The transparency is the
	/// reverse of the alpha.  0 transparency is totally opaque, alpha 255.
	/// </summary>
	private int __transparency = 0;
	/// <summary>
	/// The type of the symbol.
	/// </summary>
	private int _type = TYPE_NONE;

	/// <summary>
	/// The classification field to be used (used by higher-level code).
	/// </summary>
	private string _classification_field = "";
	/// <summary>
	/// Array of string data corresponding to the color table.
	/// </summary>
	private string[] _string_data = null;
	/// <summary>
	/// Name of field(s) to use for labeling.
	/// </summary>
	private string _label_field = null;
	/// <summary>
	/// Format to use for labeling.
	/// </summary>
	private string _label_format = null;
	/// <summary>
	/// Name of font for labels.
	/// </summary>
	private string _label_font_name = "Helvetica";

	/// <summary>
	/// Construct.  Colors are initialized to null and the symbol values to TYPE_NONE, SYM_NONE.
	/// </summary>
	public GRSymbol()
	{
		initialize();
	}

	/// <summary>
	/// Construct using the given parameters. </summary>
	/// <param name="type"> Symbol type (see TYPE_*). </param>
	/// <param name="style"> Indicates the symbol style for the symbol type.
	/// For example, if the type is TYPE_POINT, then the style can be set to any SYM_* values). </param>
	/// <param name="color"> Foreground color. </param>
	/// <param name="outline_color"> Outline color for polygons. </param>
	/// <param name="size"> Symbol size.  Currently units are not handled.  Treat is a storage
	/// area for the size that will be specified to GR.drawSymbol().  The x and y
	/// direction sizes are set to the single value. </param>
	public GRSymbol(int type, int style, GRColor color, GRColor outline_color, double size) : this(type, style, color, outline_color, size, size)
	{
	}

	/// <summary>
	/// Construct using the given parameters. </summary>
	/// <param name="type"> Symbol type (see TYPE_*). </param>
	/// <param name="style"> Indicates the symbol style for the symbol type.
	/// For example, if the type is TYPE_POINT, then the style can be set to any SYM_* values). </param>
	/// <param name="color"> Foreground color. </param>
	/// <param name="outline_color"> Outline color for polygons. </param>
	/// <param name="size_x"> Symbol size in the X direction.  Currently units are not handled.
	/// Treat is a storage area for the size that will be specified to GR.drawSymbol(). </param>
	/// <param name="size_y"> Symbol size in the Y direction. </param>
	public GRSymbol(int type, int style, GRColor color, GRColor outline_color, double size_x, double size_y)
	{
		initialize();
		_type = type;
		_color = color;
		_color2 = _color;
		_outline_color = outline_color;
		__size_x = size_x;
		__size_y = size_y;
		_style = style;
	}

	/// <summary>
	/// Clear the data arrays used to look up a color in the color table.  This method
	/// should be called when the lookup values are reset.
	/// </summary>
	private void clearData()
	{
		_double_data = null;
		_int_data = null;
		_string_data = null;
	}

	/// <summary>
	/// Clones the object. </summary>
	/// <returns> a Clone of the Object. </returns>
	public virtual object clone()
	{
		GRSymbol s = null;
		try
		{
			s = (GRSymbol)base.clone();
		}
		catch (Exception)
		{
			return null;
		}

		if (_double_data != null)
		{
			s._double_data = (double[])_double_data.Clone();
		}

		if (_color != null)
		{
			s._color = (GRColor)_color.clone();
		}

		if (_color2 != null)
		{
			s._color2 = (GRColor)_color2.clone();
		}

		if (_outline_color != null)
		{
			s._outline_color = (GRColor)_outline_color.clone();
		}

		if (_color_table != null)
		{
			s._color_table = (GRColorTable)_color_table.clone();
		}

		if (_int_data != null)
		{
			s._int_data = (int[])_int_data.Clone();
		}

		if (_string_data != null)
		{
			s._string_data = (string[])_string_data.Clone();
		}

		return s;
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRSymbol()
	{
		_color = null;
		_color2 = null;
		_outline_color = null;
		_color_table = null;
		_classification_field = null;
		_double_data = null;
		_int_data = null;
		IOUtil.nullArray(_string_data);
		_label_field = null;
		_label_format = null;
		_label_font_name = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the classification color.  If the classification type is single, the
	/// main color is returned.  Otherwise the classification value is used to look up
	/// the color in the color table.  This is useful when displaying a legend. </summary>
	/// <param name="classification"> Symbol classification color. </param>
	/// <returns> the symbol classification color (zero index). </returns>
	public virtual GRColor getClassificationColor(int classification)
	{
		if (_classification_type == CLASSIFICATION_SINGLE)
		{
			return _color;
		}
		else
		{
			return _color_table[classification];
		}
	}

	/// <summary>
	/// Return the classification second color.  This is being phased in for use with
	/// the CLASSIFICATION_SCALED_SYMBOL type. </summary>
	/// <param name="classification"> Symbol classification second color. </param>
	/// <returns> the second symbol classification color (zero index). </returns>
	public virtual GRColor getClassificationColor2(int classification)
	{
		if (_classification_type == CLASSIFICATION_SINGLE)
		{
			return _color2;
		}
		else
		{
			return _color_table[classification];
		}
	}

	/// <summary>
	/// Return the classification field.  This is just a place-holder to store the
	/// classification field name (e.g., for display on a legend). </summary>
	/// <returns> the symbol classification field. </returns>
	public virtual string getClassificationField()
	{
		return _classification_field;
	}

	/// <summary>
	/// Return the classification label string.  For single classification this is an
	/// empty string.  For class breaks, the label has "&lt; x" for the first class,
	/// "&gt; x AND &lt;= y" for the middle classes and "&gt; x" for the last class.  
	/// For unique values the label is just the unique value. </summary>
	/// <param name="classification"> Classification to get a label for. </param>
	public virtual string getClassificationLabel(int classification)
	{
		if (_classification_type == CLASSIFICATION_CLASS_BREAKS)
		{
			if (_double_data != null)
			{
				if (classification == 0)
				{
					return "< " + StringUtil.formatString(_double_data[0],"%.3f");
				}
				else if (classification == (_color_table.Count - 1))
				{
					return ">= " + StringUtil.formatString(_double_data[classification],"%.3f");
				}
				else
				{
					return ">= " + StringUtil.formatString(_double_data[classification - 1],"%.3f") +
					" AND < " + StringUtil.formatString(_double_data[classification],"%.3f");
				}
			}
			else if (_string_data != null)
			{
				return "" + classification;
			}
			else if (_int_data != null)
			{
				if (classification == 0)
				{
					return "< " + _int_data[0];
				}
				else if (classification == (_color_table.Count - 1))
				{
					return ">= " + _int_data[classification];
				}
				else
				{
					return ">= " + _int_data[classification - 1] +
					" AND < " + _int_data[classification];
				}
			}
		}
		else if (_classification_type == CLASSIFICATION_UNIQUE_VALUES)
		{
			// Need to work on this...
			return "" + classification;
		}
		return "";
	}

	/// <summary>
	/// Return the classification type. </summary>
	/// <returns> the symbol classification type.  See CLASSIFICATION_*. </returns>
	public virtual int getClassificationType()
	{
		return _classification_type;
	}

	/// <summary>
	/// Return the color used for the symbol if a single color is used.
	/// Note that the color will be null if no color has been selected. </summary>
	/// <returns> the color for the symbol (equivalent to the foreground color). </returns>
	public virtual GRColor getColor()
	{
		return _color;
	}

	/// <summary>
	/// Return the second color used for the symbol if a single color scheme is used (in
	/// this case, two colors as part of the symbol are supported).
	/// Note that the color will be null if no color has been selected. </summary>
	/// <returns> the second color for the symbol (equivalent to the second foreground color). </returns>
	public virtual GRColor getColor2()
	{
		return _color2;
	}

	/// <summary>
	/// Return the color used for the symbol if a classification is used.
	/// Note that the color will be null if no color has been selected or no color
	/// table has been set for the symbol.
	/// The internal data arrays for double, integer, and String will be checked.  Only
	/// one of these arrays is allowed to have values at any time. </summary>
	/// <param name="value"> Data value for color. </param>
	/// <returns> the color for the symbol (equivalent to the foreground color). </returns>
	public virtual GRColor getColor(object value)
	{
		if (_double_data != null)
		{
			return getColor(((double?)value).Value);
		}
		//else if ( _string_data != null ) {
			//return getColor ( (String)value );
		//}
		//else if ( _int_data != null ) {
			//return getColor ( ((Integer)value).intValue() );
		//}
		return null;
	}

	/// <summary>
	/// Return the color used for the symbol.  If a classification or scaled symbol
	/// is used the value is used to determine the symbol color.
	/// Note that the color will be null if no color has been selected or no color
	/// table has been set for the symbol. </summary>
	/// <param name="value"> Data value to find color. </param>
	/// <returns> the color for the symbol (equivalent to the foreground color). </returns>
	public virtual GRColor getColor(double value)
	{
		if (_classification_type == CLASSIFICATION_SINGLE)
		{
			//Message.printStatus ( 1, "", "Returning single color" );
			return _color;
		}
		else if (_classification_type == CLASSIFICATION_SCALED_SYMBOL || _classification_type == CLASSIFICATION_SCALED_TEACUP_SYMBOL)
		{
			//Message.printStatus ( 1, "", "Returning single color" );
			if (_style == SYM_VBARSIGNED)
			{
				if (value >= 0)
				{
					return _color;
				}
				else
				{
					return _color2;
				}
			}
			else
			{
				return _color;
			}
		}
		else if (_classification_type == CLASSIFICATION_CLASS_BREAKS)
		{
			// Need to check data value against ranges...
			if (value < _double_data[0])
			{
				//Message.printStatus(1, "", "Returning first color for " + value );
				return _color_table[0];
			}
			else if (value >= _double_data[_double_data.Length - 1])
			{
				//Message.printStatus(1, "", "Returning last color for "+ value  );
				return _color_table[_double_data.Length - 1];
			}
			else
			{
				int iend = _double_data.Length - 1;
				for (int i = 1; i < iend; i++)
				{
					if (value < _double_data[i])
					{
						//Message.printStatus(1, "", "Returning color " + i + " for " + value
						//	+ "  (" + _double_data[i] + ")");
						return _color_table[i];
					}
				}
				return null;
			}
		}
		else
		{
			// Unique value so just find the value...
			for (int i = 0; i < _double_data.Length; i++)
			{
				if (_double_data[i] == value)
				{
					return _color_table[i];
				}
			}
			return null;
		}
	}

	/// <summary>
	/// Return the color table color position for the symbol if a classification is
	/// used.  This method is useful if an external set of colors is set up and only the position is used.
	/// Note that the position will be -1 if no color has been selected or no color
	/// table has been set for the symbol. </summary>
	/// <param name="value"> Data value to find color. </param>
	/// <returns> the color table position for the color for the symbol (equivalent to
	/// the foreground color). </returns>
	public virtual int getColorNumber(double value)
	{
		if (_classification_type == CLASSIFICATION_SINGLE)
		{
			//Message.printStatus ( 1, "", "Returning single color" );
			return -1;
		}
		else if (_classification_type == CLASSIFICATION_CLASS_BREAKS)
		{
			// Need to check data value against ranges...
			if (value < _double_data[0])
			{
				//Message.printStatus ( 1, "", "Returning first color for " + value );
				return 0;
			}
			else if (value >= _double_data[_double_data.Length - 1])
			{
				//Message.printStatus ( 1, "", "Returning last color for "+ value  );
				return _double_data.Length - 1;
			}
			else
			{
				int iend = _double_data.Length - 1;
				for (int i = 1; i < iend; i++)
				{
					if (value < _double_data[i + 1])
					{
						//Message.printStatus ( 1, "", "Returning color " + i+" for "+value);
						return i;
					}
				}
				return -1;
			}
		}
		else
		{ // Unique value so just find the value...
			for (int i = 0; i < _double_data.Length; i++)
			{
				if (_double_data[i] == value)
				{
					return i;
				}
			}
			return -1;
		}
	}

	/// <summary>
	/// Return the GRColorTable, or null if one is not used. </summary>
	/// <returns> the GRColorTable. </returns>
	public virtual GRColorTable getColorTable()
	{
		return _color_table;
	}

	/// <summary>
	/// Return the label field. </summary>
	/// <returns> the label field. </returns>
	public virtual string getLabelField()
	{
		return _label_field;
	}

	/// <summary>
	/// Return the label font height, points. </summary>
	/// <returns> the label font height, points. </returns>
	public virtual double getLabelFontHeight()
	{
		return _label_font_height;
	}

	/// <summary>
	/// Return the label font name. </summary>
	/// <returns> the label font name. </returns>
	public virtual string getLabelFontName()
	{
		return _label_font_name;
	}

	/// <summary>
	/// Return the label format. </summary>
	/// <returns> the label format. </returns>
	public virtual string getLabelFormat()
	{
		return _label_format;
	}

	/// <summary>
	/// Return the label position. </summary>
	/// <returns> the label position. </returns>
	public virtual int getLabelPosition()
	{
		return _label_position;
	}

	/// <summary>
	/// Return the number of classifications, which will be the number of colors in
	/// the color table.  If a single classification (or scaled symbol) is used, then 1 is returned. </summary>
	/// <returns> the number of classifications. </returns>
	public virtual int getNumberOfClassifications()
	{
		if ((_classification_type == CLASSIFICATION_SINGLE) || (_classification_type == CLASSIFICATION_SCALED_SYMBOL) || _classification_type == CLASSIFICATION_SCALED_TEACUP_SYMBOL)
		{
			//Message.printStatus ( 1, "", "Returning single color" );
			return 1;
		}
		else
		{
			// Use the color table size so the data type checks don't have to be done...
			return _color_table.Count;
		}
	}

	/// <summary>
	/// Return the outline color.  The outline color is used for all classifications. </summary>
	/// <returns> the outline color.  Note that the color will be null if no color has been selected. </returns>
	public virtual GRColor getOutlineColor()
	{
		return _outline_color;
	}

	/// <summary>
	/// Return the symbol size.  Although no distinction is made internally, the symbol
	/// size is typically implemented in higher level software as a pixel size (not data
	/// units).  Symbols can have different sizes in the X and Y direction but return
	/// the X size here (assumed to be the same as the Y direction size). </summary>
	/// <returns> the symbol size. </returns>
	public virtual double getSize()
	{
		return __size_x;
	}

	/// <summary>
	/// Return the symbol size in the X direction.  Although no distinction is made
	/// internally, the symbol size is typically implemented in higher level software
	/// as a pixel size (not data units). </summary>
	/// <returns> the symbol size in the X direction. </returns>
	public virtual double getSizeX()
	{
		return __size_x;
	}

	/// <summary>
	/// Return the symbol size in the Y direction.  Although no distinction is made
	/// internally, the symbol size is typically implemented in higher level software
	/// as a pixel size (not data units). </summary>
	/// <returns> the symbol size in the Y direction. </returns>
	public virtual double getSizeY()
	{
		return __size_y;
	}

	/// <summary>
	/// Return the symbol style.  If a point type, then the point symbol will be
	/// returned.  If a line type, then the line style (e.g., SOLID) will be returned.
	/// If a polygon, then the fill type will be returned.  Currently only point types are fully supported.
	/// </summary>
	public virtual int getStyle()
	{
		return _style;
	}

	/// <summary>
	/// Return the style as a string name. </summary>
	/// <returns> the style as a string name (e.g., "Circle-Filled"). </returns>
	public virtual string getStyleName()
	{
		return SYMBOL_NAMES[_style];
	}

	/// <summary>
	/// Return the transparency (255 = completely transparent, 0 = opaque). </summary>
	/// <returns> the transparency. </returns>
	public virtual int getTransparency()
	{
		return __transparency;
	}

	/// <summary>
	/// Return the symbol type (e.g., whether a point or polygon type). </summary>
	/// <returns> the symbol type.  See TYPE_*. </returns>
	public virtual int getType()
	{
		return _type;
	}

	/// <summary>
	/// Initialize data.
	/// </summary>
	private void initialize()
	{
		_color = null;
		_color2 = null;
		_outline_color = null;
		_style = SYM_NONE;
		__size_x = 0.0;
		__size_y = 0.0;
		_type = TYPE_NONE;
	}

	/// <summary>
	/// Specify whether the symbol is a primary symbol.  A primary symbol is, for
	/// example, the symbol for a polygon, where secondary symbols may be drawn at the polygon centroid. </summary>
	/// <param name="is_primary"> Indicates whether the symbol is a primary symbol. </param>
	/// <returns> the value of the flag after setting. </returns>
	public virtual bool isPrimary(bool is_primary)
	{
		__is_primary = is_primary;
		return __is_primary;
	}

	/// <summary>
	/// Indicate whether the symbol is a primary symbol.  A primary symbol is, for
	/// example, the symbol for a polygon, where secondary symbols may be drawn at the polygon centroid. </summary>
	/// <returns> true if the symbol is the primary symbol, false if not. </returns>
	public virtual bool isPrimary()
	{
		return __is_primary;
	}

	/// <summary>
	/// Indicate whether a symbol can be used for the specified classification. </summary>
	/// <returns> true if the symbol can be used with scaled symbol classification. </returns>
	public static bool isSymbolForClassification(int classification_type, int sym)
	{
		if (classification_type == CLASSIFICATION_SCALED_SYMBOL)
		{
			if (sym == SYM_VBARSIGNED || sym == SYM_VBARUNSIGNED)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Indicate whether only selected shapes should be labeled. </summary>
	/// <returns> true if only selected shapes should be labeled. </returns>
	public virtual bool labelSelectedOnly()
	{
		return _label_selected_only;
	}

	/// <summary>
	/// Set whether only selected shapes should be labeled. </summary>
	/// <param name="label_selected_only"> Indicates that only selected shapes should be labeled. </param>
	/// <returns> value of flag, after reset. </returns>
	public virtual bool labelSelectedOnly(bool label_selected_only)
	{
		_label_selected_only = label_selected_only;
		return _label_selected_only;
	}

	/// <summary>
	/// Set the data that are used with a classification to look up a color for
	/// drawing.  Currently, only one data type (double, int, etc.) can be used for the
	/// classification.  It is assumed that external code is used to define the data
	/// values and that the number of values corresponds to the number of colors in the color table. </summary>
	/// <param name="data"> Data to use for classification.  The number of data values should
	/// be less than or equal to the number of colors in the color map. </param>
	/// <param name="make_copy"> Indicates whether a copy of the data array should be made
	/// (true) or not (false), in which case the calling code should maintain the list. </param>
	public virtual void setClassificationData(double[] data, bool make_copy)
	{
		clearData();
		if (data == null)
		{
			return;
		}
		if (make_copy)
		{
			_double_data = new double[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				_double_data[i] = data[i];
			}
		}
		else
		{
			_double_data = data;
		}
	}

	/// <summary>
	/// Set the classification field.  This is just a helper function to carry around
	/// data that may be useful to an application (e.g., so a property display dialog
	/// can be created) but the field name is not used internally. </summary>
	/// <param name="classification_field"> field (e.g., from database) used to determine 
	/// color, etc., for visualization. </param>
	public virtual void setClassificationField(string classification_field)
	{
		if (!string.ReferenceEquals(classification_field, null))
		{
			_classification_field = classification_field;
		}
	}

	/// <summary>
	/// Set the classification type. </summary>
	/// <param name="classification_type"> Classification field name corresponding to CLASSIFICATION_NAMES. </param>
	public virtual void setClassificationType(string classification_type)
	{
		for (int i = 0; i < CLASSIFICATION_NAMES.Length; i++)
		{
			if (classification_type.Equals(CLASSIFICATION_NAMES[i], StringComparison.OrdinalIgnoreCase))
			{
				_classification_type = i;
				break;
			}
		}
	}

	/// <summary>
	/// Set the color for the symbol (equivalent to the foreground color).  This is
	/// used with single classification. </summary>
	/// <param name="color"> Color to use for symbol. </param>
	public virtual void setColor(GRColor color)
	{
		_color = color;
	}

	/// <summary>
	/// Set the second color for the symbol (equivalent to the second foreground color).
	/// This is used with scaled symbol classification where necessary. </summary>
	/// <param name="color2"> Second color to use for symbol. </param>
	public virtual void setColor2(GRColor color2)
	{
		_color2 = color2;
	}

	/// <summary>
	/// Set the color table for the symbol, used for looking up colors when other than
	/// a single classification is being used.  The number of colors in the color table
	/// should agree with the number of classifications. </summary>
	/// <param name="color_table"> GRColorTable to use. </param>
	public virtual void setColorTable(GRColorTable color_table)
	{
		_color_table = color_table;
	}

	/// <summary>
	/// Set the color table for the symbol, given the color table name.  A new GRColorTable will be created. </summary>
	/// <param name="table_name"> GRColorTable name. </param>
	/// <param name="ncolors"> Number of colors to use. </param>
	public virtual void setColorTable(string table_name, int ncolors)
	{
		_color_table = GRColorTable.createColorTable(table_name, ncolors, false);
	}

	/// <summary>
	/// Set the label field.  Currently this is just carried around to help other graphics code. </summary>
	/// <param name="label_field"> field (e.g., from database) used to label a symbol. </param>
	public virtual void setLabelField(string label_field)
	{
		if (!string.ReferenceEquals(label_field, null))
		{
			_label_field = label_field;
		}
	}

	/// <summary>
	/// Set the label font height (points). </summary>
	/// <param name="label_font_height"> Label font height. </param>
	public virtual void setLabelFontHeight(double label_font_height)
	{
		_label_font_height = label_font_height;
	}

	/// <summary>
	/// Set the label font name (e.g., "Helvetica").  More information about available
	/// font names will be added later.  This does not create a Font for the symbol.  It
	/// just stores the font name. </summary>
	/// <param name="label_font_name"> Font name to use for labels. </param>
	public virtual void setLabelFontName(string label_font_name)
	{
		if (!string.ReferenceEquals(label_font_name, null))
		{
			_label_font_name = label_font_name;
		}
	}

	/// <summary>
	/// Set the label format.  The format is used to format label fields and should
	/// contain a StringUtil format specifier appropriate for the label field data types. </summary>
	/// <param name="label_format"> Format used to label a symbol. </param>
	public virtual void setLabelFormat(string label_format)
	{
		if (!string.ReferenceEquals(label_format, null))
		{
			_label_format = label_format;
		}
	}

	/// <summary>
	/// Set the label position (to combination of GRText position mask values). </summary>
	/// <param name="label_position"> Position for labels. </param>
	public virtual void setLabelPosition(int label_position)
	{
		_label_position = label_position;
	}

	/// <summary>
	/// Set the outline color.
	/// </summary>
	public virtual void setOutlineColor(GRColor outline_color)
	{
		_outline_color = outline_color;
	}

	/// <summary>
	/// Set the symbol size.  Both the x and y direction sizes are set to the same value.
	/// </summary>
	public virtual void setSize(double size)
	{
		__size_x = size;
		__size_y = size;
	}

	/// <summary>
	/// Set the symbol size in the X direction. </summary>
	/// <param name="size_x"> Symbol size in the X direction. </param>
	public virtual void setSizeX(double size_x)
	{
		__size_x = size_x;
	}

	/// <summary>
	/// Set the symbol size in the Y direction. </summary>
	/// <param name="size_y"> Symbol size in the Y direction. </param>
	public virtual void setSizeY(double size_y)
	{
		__size_y = size_y;
	}

	/// <summary>
	/// Set the symbol style. </summary>
	/// <param name="style"> For point symbols, see SYM_*. </param>
	public virtual void setStyle(int style)
	{
		_style = style;
	}

	/// <summary>
	/// Set the transparency (255 = completely transparent, 0 = opaque). </summary>
	/// <param name="transparency"> the transparency. </param>
	public virtual void setTransparency(int transparency)
	{
		if (transparency < 0)
		{
			transparency = 0;
		}
		else if (transparency > 255)
		{
			transparency = 255;
		}
		else
		{
			__transparency = transparency;
		}
	}

	/// <summary>
	/// Look up a symbol number given a name.  This is useful when the symbol names are
	/// stored in a persistent way (avoid using numbers in config files because symbol numbers may change). </summary>
	/// <returns> the symbol number given the symbol name. </returns>
	/// <exception cref="Exception"> if a symbol cannot be matched. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int toInteger(String symbolName) throws Exception
	public static int toInteger(string symbolName)
	{
		if (string.ReferenceEquals(symbolName, null))
		{
			return SYM_NONE;
		}
		int length = SYMBOL_NAMES.Length;
		for (int i = 0; i < length; i++)
		{
			if (symbolName.Equals(SYMBOL_NAMES[i], StringComparison.OrdinalIgnoreCase))
			{
				return SYMBOL_NUMBERS[i];
			}
		}
		throw new Exception("Cannot convert symbol \"" + symbolName + "\" to integer value.");
	}

	/// <summary>
	/// Return a string representation of the symbol. </summary>
	/// <returns> a string representation of the symbol. </returns>
	public override string ToString()
	{
		return "Type: " + _type + " Color: " + _color + " Color2: " + _color2 + " Size: " + __size_x + "," + __size_y;
	}

	/// <summary>
	/// Look up a symbol name given a number.  This is useful when the symbol names are
	/// stored in a persistent way (avoid using numbers in configuration files because
	/// symbol numbers may change). </summary>
	/// <param name="symbolNumber"> the number of the symbol to look up. </param>
	/// <returns> the symbol name given the symbol number, or "None" if a matching symbol cannot be found. </returns>
	public static string ToString(int symbolNumber)
	{
		int length = SYMBOL_NUMBERS.Length;
		for (int i = 0; i < length; i++)
		{
			if (symbolNumber == SYMBOL_NUMBERS[i])
			{
				return SYMBOL_NAMES[i];
			}
		}
		return SYMBOL_NAMES[0]; // "None"
	}

	}

}