using System;

// ERDiagram_Relationship - class to represent the relationship between two database tables

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
// ERDiagram_Relationship - class to represent the relationship between two
//	tables.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2003-08-27	J. Thomas Sapienza, RTi	* Initial changelog.  
// 					* Added color coding capability.
// ----------------------------------------------------------------------------

namespace RTi.DMI
{

	using GRColor = RTi.GR.GRColor;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRText = RTi.GR.GRText;

	/// <summary>
	/// Class that represents the relationship between two tables, and which can
	/// also draw the relationship.
	/// </summary>
	public class ERDiagram_Relationship : DMIDataObject
	{

	/// <summary>
	/// Whether the relationship should be drawn in bold or not.  This is a public 
	/// field because it will be accessed every time the mouse is pressed, so an 
	/// accessor function could concievably affect performance.
	/// </summary>
	public bool bold = false;

	/// <summary>
	/// Whether the relationships are color-coded, so that it is easier to tell which
	/// algorithm was used to draw each one.
	/// </summary>
	private bool __colorCodedRelationships;

	/// <summary>
	/// Whether the relationship is between at least one reference tables.
	/// </summary>
	private bool __isReference = false;

	/// <summary>
	/// Whether the relationship is visible (should be drawn) or not.
	/// </summary>
	private bool __visible = false;

	/// <summary>
	/// The first line of the relationship, from the start table out to the right.
	/// </summary>
	private Line2D.Double __l1;

	/// <summary>
	/// The second line of the relationship, a diagonal connecting #1 and #3.
	/// </summary>
	private Line2D.Double __l2;

	/// <summary>
	/// The third line of the relationship, from the left to the end table.
	/// </summary>
	private Line2D.Double __l3;

	/// <summary>
	/// The name of the field at which the relationship ends.
	/// </summary>
	private string __endField;

	/// <summary>
	/// The name of the table at which the relationship ends.
	/// </summary>
	private string __endTable;

	/// <summary>
	/// In a relationship involving a reference table, the name of the table that is
	/// NOT a relationship table.  Null if both are relationship tables.
	/// </summary>
	private string __nonReferenceTable = "";

	/// <summary>
	/// The name of the field from which the relationship begins.
	/// </summary>
	private string __startField;

	/// <summary>
	/// The name of the table from which the relationship begins.
	/// </summary>
	private string __startTable;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="startTable"> the name of the table that the relationship starts at. </param>
	/// <param name="startField"> the name of the field that the relationship starts at. </param>
	/// <param name="endTable"> the name of the table that the relationship ends at. </param>
	/// <param name="endField"> the name of the field that the relationship ends at. </param>
	public ERDiagram_Relationship(string startTable, string startField, string endTable, string endField)
	{
		__colorCodedRelationships = false;

		__startTable = startTable;
		__startField = startField;
		__endTable = endTable;
		__endField = endField;

		__l1 = new Line2D.Double();
		__l2 = new Line2D.Double();
		__l3 = new Line2D.Double();
	}

	/// <summary>
	/// Returns true if any of the lines in the relationship intersect with the given
	/// point or not.  This actually checks a 5x5 square formed around the specified
	/// point for an intersection, so that users can click near a relationship line
	/// and select it. </summary>
	/// <param name="x"> the x location to check. </param>
	/// <param name="y"> the y location to check. </param>
	/// <returns> true if the point is contained, false otherwise. </returns>
	public virtual bool contains(double x, double y)
	{
		if (__l1.intersects(x - 2, y - 2, 5, 5))
		{
			return true;
		}
		if (__l2.intersects(x - 2, y - 2, 5, 5))
		{
			return true;
		}
		if (__l3.intersects(x - 2, y - 2, 5, 5))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Draws the relationship on the specified drawing area. </summary>
	/// <param name="da"> the drawing area on which to draw the relationship. </param>
	/// <param name="tables"> the array of all the tables in the er diagram. </param>
	public virtual void draw(ERDiagram_DrawingArea da, ERDiagram_Table[] tables)
	{
		bool grey = false;
		ERDiagram_Table start = getTableByName(__startTable, tables);
		ERDiagram_Table end = getTableByName(__endTable, tables);

		if (start == null || end == null)
		{
			return;
		}

		if (!start.isVisible() || !end.isVisible())
		{
			return;
		}

		// if the "ending" table is left of the "starting" table, 
		// the "starting" and "ending" tables will be swapped.  Originally,
		// the starting table was the parent and the ending table was
		// the child of the parent-child table relationship.  Now that
		// information isn't worried about.
		if (end.getX() < start.getX())
		{
			string stemp = __startField;
			__startField = __endField;
			__endField = stemp;

			stemp = __startTable;
			__startTable = __endTable;
			__endTable = stemp;

			start = getTableByName(__startTable, tables);
			end = getTableByName(__endTable, tables);
		}

		double sxl = start.getX();
		double sxr = start.getX() + start.getWidth() + start.getDropShadowOffset();

		double exl = end.getX();
		double exr = end.getX() + end.getWidth() + end.getDropShadowOffset();

		double sy = start.getFieldY(da, __startField, GRText.CENTER_Y);
		double ey = end.getFieldY(da, __endField, GRText.CENTER_Y);

		double extend = 20;

		if (!grey)
		{
			GRDrawingAreaUtil.setColor(da, GRColor.black);
		}
		else
		{
			GRDrawingAreaUtil.setColor(da, GRColor.lightGray);
		}

		if (__isReference)
		{
			if (string.ReferenceEquals(__nonReferenceTable, null))
			{
				// both tables are reference tables (probably will never
				// happen)
				return;
			}
			else if (__nonReferenceTable.Equals(__startTable))
			{
				GRDrawingAreaUtil.drawLine(da, sxr, sy, sxr + extend, sy);
				GRDrawingAreaUtil.drawText(da, "(" + __endTable + "." + __endField + ")", sxr + extend, sy - 3, 0, 0);
			}
			else
			{
				GRDrawingAreaUtil.drawLine(da, exr + extend, ey, exr, ey);
				GRDrawingAreaUtil.drawText(da, "(" + __startTable + "." + __startField + ")", exr + extend, ey - 3, 0, 0);
			}
			return;
		}

		// REVISIT (JTS - 2003-08-27)
		// the drawing code can be done better, I think.

		// both the right drawing points are the same, so draw
		// a line to the right of the first table up or down to the
		// second, and then to the left
		//
		//   TABLE1 ---+
		//             |
		//             |
		//   TABLE2 ---+
		//
		// Color code: yellow
		if (sxr == exr)
		{
			if (__colorCodedRelationships)
			{
				GRDrawingAreaUtil.setColor(da, GRColor.yellow);
			}
			GRDrawingAreaUtil.fillRectangle(da, sxr, sy - 2, 4, 4);
			GRDrawingAreaUtil.fillRectangle(da, exr, ey - 2, 4, 4);

			GRDrawingAreaUtil.drawLine(da, sxr, sy, sxr + extend, sy);
			__l1.x1 = sxr;
			__l1.x2 = sxr + extend;
			__l1.y1 = sy;
			__l1.y2 = sy;
			GRDrawingAreaUtil.drawLine(da, sxr + extend,sy,exr + extend,ey);
			__l2.x1 = sxr + extend;
			__l2.x2 = exr + extend;
			__l2.y1 = sy;
			__l2.y2 = ey;
			GRDrawingAreaUtil.drawLine(da, exr + extend, ey, exr, ey);
			__l3.x1 = exr + extend;
			__l3.x2 = exr;
			__l3.y1 = ey;
			__l3.y2 = ey;
		}
		// both the left drawing points are the same, so draw a line out
		// to the left of the first table, up or down to the second,
		// and then back to the right
		//
		//   +--- TABLE1
		//   | 
		//   | 
		//   +--- TABLE2
		//
		// Color code: red
		else if (sxl == exl)
		{
			if (__colorCodedRelationships)
			{
				GRDrawingAreaUtil.setColor(da, GRColor.red);
			}
			GRDrawingAreaUtil.fillRectangle(da, sxl - 4, sy - 2, 4, 4);
			GRDrawingAreaUtil.fillRectangle(da, exl - 4, ey - 2, 4, 4);

			GRDrawingAreaUtil.drawLine(da, sxl, sy, sxl - extend, sy);
			__l1.x1 = sxl;
			__l1.x2 = sxl - extend;
			__l1.y1 = sy;
			__l1.y2 = sy;
			GRDrawingAreaUtil.drawLine(da, sxl - extend,sy,exl - extend,ey);
			__l2.x1 = sxl - extend;
			__l2.x2 = exl - extend;
			__l2.y1 = sy;
			__l2.y2 = ey;
			GRDrawingAreaUtil.drawLine(da, exl - extend, ey, exl, ey);
			__l3.x1 = exl - extend;
			__l3.x2 = exl;
			__l3.y1 = ey;
			__l3.y2 = ey;
		}
		// draw a line out the right side of the first table, then diagonally
		// towards the second table, then draw a line left into the second
		// table
		//
		//   TABLE1 ---+
		//              \
		//               \
		//                +--- TABLE2
		//
		// Color code: green
		else if (exl > (extend * 1.5) + sxr)
		{
			if (__colorCodedRelationships)
			{
				GRDrawingAreaUtil.setColor(da, GRColor.green);
			}
			GRDrawingAreaUtil.fillRectangle(da, sxr, sy - 2, 4, 4);
			GRDrawingAreaUtil.fillRectangle(da, exl - 4, ey - 2, 4, 4);

			GRDrawingAreaUtil.drawLine(da, sxr, sy, sxr + extend, sy);
			__l1.x1 = sxr;
			__l1.x2 = sxr + extend;
			__l1.y1 = sy;
			__l1.y2 = sy;
			GRDrawingAreaUtil.drawLine(da, sxr + extend,sy,exl - extend,ey);
			__l2.x1 = sxr + extend;
			__l2.x2 = exl - extend;
			__l2.y1 = sy;
			__l2.y2 = ey;
			GRDrawingAreaUtil.drawLine(da, exl - extend, ey, exl, ey);
			__l3.x1 = exl - extend;
			__l3.x2 = exl;
			__l3.y1 = ey;
			__l3.y2 = ey;
		}
		// if the tables are not perfectly aligned with each other (i.e., 
		// neither their left sides or right sides are even), but they are
		// still close to each other, avoid drawing diagonal lines (which
		// are messy) and draw a line out the left side of one and into the
		// left side of the other
		//
		//     +---- TABLE1
		//     |
		//     |
		//     +-- TABLE2
		//
		// Color code: blue
		else if (exl <= (extend * 1.5) + sxr)
		{
			if (__colorCodedRelationships)
			{
				GRDrawingAreaUtil.setColor(da, GRColor.blue);
			}
			if (Math.Abs(exl - sxl) < Math.Abs(exr - sxr))
			{
			double minX = 0;
			if (sxl < exl)
			{
				minX = sxl - extend;
			}
			else
			{
				minX = exl - extend;
			}
			GRDrawingAreaUtil.fillRectangle(da, sxl - 4, sy - 2, 4, 4);
			GRDrawingAreaUtil.fillRectangle(da, exl - 4, ey - 2, 4, 4);

			GRDrawingAreaUtil.drawLine(da, sxl, sy, minX, sy);
			__l1.x1 = sxr;
			__l1.x2 = minX;
			__l1.y1 = sy;
			__l1.y2 = sy;
			GRDrawingAreaUtil.drawLine(da, minX, sy, minX, ey);
			__l2.x1 = minX;
			__l2.x2 = minX;
			__l2.y1 = sy;
			__l2.y2 = ey;
			GRDrawingAreaUtil.drawLine(da, minX, ey, exl, ey);
			__l3.x1 = minX;
			__l3.x2 = exl;
			__l3.y1 = ey;
			__l3.y2 = ey;
			}
			else
			{
			double farX = 0;
			if (sxr > exr)
			{
				farX = sxr + extend;
			}
			else
			{
				farX = exr + extend;
			}
			GRDrawingAreaUtil.fillRectangle(da, sxr, sy - 2, 4, 4);
			GRDrawingAreaUtil.fillRectangle(da, exr, ey - 2, 4, 4);

			GRDrawingAreaUtil.drawLine(da, sxr, sy, farX, sy);
			__l1.x1 = sxr;
			__l1.x2 = farX;
			__l1.y1 = sy;
			__l1.y2 = sy;
			GRDrawingAreaUtil.drawLine(da, farX, sy, farX, ey);
			__l2.x1 = farX;
			__l2.x2 = farX;
			__l2.y1 = sy;
			__l2.y2 = ey;
			GRDrawingAreaUtil.drawLine(da, farX, ey, exr, ey);
			__l3.x1 = farX;
			__l3.x2 = exl;
			__l3.y1 = ey;
			__l3.y2 = ey;
			}
		}
		else
		{
			// this case shouldn't come up ... 
		}

		// if the line was clicked on, fake a "bold" line by drawing 4 lines:
		// 1) the original line (drawn above)
		// 2) a line drawn exactly 1 pixel to the right of the original
		// 3) a line drawn exactly 1 pixel higher than the original
		// 4) a line drawn exactly 1 pixel to the right and higher than the
		// 	original
		if (bold)
		{
			GRDrawingAreaUtil.drawLine(da, __l1.x1 + 1, __l1.y1, __l1.x2 + 1, __l1.y2);
			GRDrawingAreaUtil.drawLine(da, __l2.x1 + 1, __l2.y1, __l2.x2 + 1, __l2.y2);
			GRDrawingAreaUtil.drawLine(da, __l3.x1 + 1, __l3.y1, __l3.x2 + 1, __l3.y2);

			GRDrawingAreaUtil.drawLine(da, __l1.x1 + 1, __l1.y1 + 1, __l1.x2 + 1, __l1.y2 + 1);
			GRDrawingAreaUtil.drawLine(da, __l2.x1 + 1, __l2.y1 + 1, __l2.x2 + 1, __l2.y2 + 1);
			GRDrawingAreaUtil.drawLine(da, __l3.x1, __l3.y1 + 1, __l3.x2, __l3.y2 + 1);

			GRDrawingAreaUtil.drawLine(da, __l1.x1, __l1.y1 + 1, __l1.x2, __l1.y2 + 1);
			GRDrawingAreaUtil.drawLine(da, __l2.x1, __l2.y1 + 1, __l2.x2, __l2.y2 + 1);
			GRDrawingAreaUtil.drawLine(da, __l3.x1, __l3.y1 + 1, __l3.x2, __l3.y2 + 1);
		}
	}

	/// <summary>
	/// Checks to see whether the relationship ends with the specified table. </summary>
	/// <param name="table"> the name of the table. </param>
	/// <returns> if the relationship ends with the specified table, otherwise false. </returns>
	public virtual bool endsWithTable(string table)
	{
		if (table.Equals(__endTable, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks to see whether the relationship ends with the specified table and
	/// field. </summary>
	/// <param name="table"> the name of the table. </param>
	/// <param name="field"> the name of the field. </param>
	/// <returns> if the relationship ends with the specified table and field, 
	/// otherwise false. </returns>
	public virtual bool endsWithTableField(string table, string field)
	{
		if (table.Equals(__endTable, StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals(__endField, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns whether the relationships are color-coded for debugging. </summary>
	/// <returns> whether the relationships are color-coded for debugging. </returns>
	public virtual bool getColorCodeRelationships()
	{
		return __colorCodedRelationships;
	}

	/// <summary>
	/// Returns the end field name. </summary>
	/// <returns> the end field name. </returns>
	public virtual string getEndField()
	{
		return __endField;
	}

	/// <summary>
	/// Returns the end table name. </summary>
	/// <returns> the end table name. </returns>
	public virtual string getEndTable()
	{
		return __endTable;
	}

	/// <summary>
	/// In the case of a reference relationship, returns the name of the table
	/// that is NOT a reference table. </summary>
	/// <returns> the name of the table that is not a reference table. </returns>
	public virtual string getNonReferenceTable()
	{
		return __nonReferenceTable;
	}

	/// <summary>
	/// Returns the start field name. </summary>
	/// <returns> the start field name. </returns>
	public virtual string getStartField()
	{
		return __startField;
	}

	/// <summary>
	/// Returns the start table name. </summary>
	/// <returns> the start table name. </returns>
	public virtual string getStartTable()
	{
		return __startTable;
	}

	/// <summary>
	/// Finds the table in the array with the specified name and returns it. </summary>
	/// <param name="name"> the name of the table to find. </param>
	/// <param name="tables"> array of tables. </param>
	/// <returns> the table with the specified name, or null if it can't be found. </returns>
	private ERDiagram_Table getTableByName(string name, ERDiagram_Table[] tables)
	{
		for (int i = 0; i < tables.Length; i++)
		{
			if (tables[i].getName().Equals(name))
			{
				return tables[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Checks to see whether the relationship involves the specified table. </summary>
	/// <param name="table"> the name of the table to check for. </param>
	/// <returns> true if the table starts with or ends with the specified table. </returns>
	public virtual bool involvesTable(string table)
	{
		if (table.Equals(__startTable, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (table.Equals(__endTable, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Whether this relationship involves reference tables. </summary>
	/// <returns> whether this relationship involves reference tables. </returns>
	public virtual bool isReference()
	{
		return __isReference;
	}

	/// <summary>
	/// Returns whether the relationship is visible or not. </summary>
	/// <returns> whether the relationship is visible or not. </returns>
	public virtual bool isVisible()
	{
		return __visible;
	}

	/// <summary>
	/// Sets whether the relationships should be color-coded (for use in debugging to
	/// tell which algorithm was used to draw each. </summary>
	/// <param name="code"> whether to color code the relationships. </param>
	public virtual void setColorCodeRelationships(bool code)
	{
		__colorCodedRelationships = code;
	}

	/// <summary>
	/// Sets the name of the table in the relationship (if one is a reference table)
	/// that is NOT a reference table.  If both tables in the relationship are reference
	/// tables, this will be null. </summary>
	/// <param name="nonReferenceTable"> the name of the table in a reference relationship
	/// that is not a reference table. </param>
	public virtual void setNonReferenceTable(string nonReferenceTable)
	{
		__nonReferenceTable = nonReferenceTable;
	}

	/// <summary>
	/// Sets whether this relationship involved any reference tables. </summary>
	/// <param name="isReference"> whether this relationship involves any reference tables. </param>
	public virtual void setReference(bool isReference)
	{
		__isReference = isReference;
	}

	/// <summary>
	/// Sets whether the relationship is visible or not. </summary>
	/// <param name="visible"> whether the relationship is visible or not. </param>
	public virtual void setVisible(bool visible)
	{
		__visible = visible;
	}

	/// <summary>
	/// Checks to see whether the relationship starts with the specified table. </summary>
	/// <param name="table"> the name of the table. </param>
	/// <returns> if the relationship starts with the specified table, otherwise false. </returns>
	public virtual bool startsWithTable(string table)
	{
		if (table.Equals(__startTable, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks to see whether the relationship starts with the specified table and
	/// field. </summary>
	/// <param name="table"> the name of the table. </param>
	/// <param name="field"> the name of the field. </param>
	/// <returns> if the relationship starts with the specified table and field, 
	/// otherwise false. </returns>
	public virtual bool startsWithTableField(string table, string field)
	{
		if (table.Equals(__startTable, StringComparison.OrdinalIgnoreCase))
		{
			if (field.Equals(__startField, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns a String representation of this relationship. </summary>
	/// <returns> a String representation of this relationship. </returns>
	public override string ToString()
	{
		return __startTable + "." + __startField + "\t->\t" + __endTable + "." + __endField;
	}

	}

}