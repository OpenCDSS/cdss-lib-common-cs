// GRScaledClassificationSymbol - store symbol definition information for GRSymbol.CLASSIFICATION_SCALED_SYMBOL type

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
// GRScaledClassificationSymbol - store symbol definition information for
//				GRSymbol.CLASSIFICATION_SCALED_SYMBOL type.
// ----------------------------------------------------------------------------
// History:
//
// 2002-09-24	Steven A. Malers, RTi	Create this class to extend GRSymbol.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	/// <summary>
	/// This class stores information necessary to draw symbols for scaled symbol
	/// classifications.  For example, a scaled symbol is used where the symbol
	/// appearance changes only in size based on the data value at the point.
	/// Additional symbols will be recognized later but currently only SYM_VBARSIGNED
	/// is recognized.  For this symbol, the following methods should be called after
	/// construction:  setSizeX() (bar width), setSizeY() (max bar height),
	/// setColor() (positive/up bar color), setColor2() (negative/down bar color).
	/// </summary>
	public class GRScaledClassificationSymbol : GRSymbol
	{

	/// <summary>
	/// Maximum actual data value.
	/// </summary>
	protected internal double _double_data_max = 0.0;

	/// <summary>
	/// Maximum displayed data value.  User-defined or automatically-determined.
	/// </summary>
	protected internal double _double_data_display_max = 0.0;

	/// <summary>
	/// Constructor.  The symbol style defaults to GRSymbol.SYM_VBARSIGNED.
	/// </summary>
	public GRScaledClassificationSymbol() : base()
	{
		setStyle(SYM_VBARSIGNED);
		setClassificationType("ScaledSymbol");
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRScaledClassificationSymbol()
	{
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the maximum displayed value used in the classification. </summary>
	/// <returns> the maximum displayed value used in the classification. </returns>
	public virtual double getClassificationDataDisplayMax()
	{
		return _double_data_display_max;
	}

	/// <summary>
	/// Return the maximum data value used in the classification. </summary>
	/// <returns> the maximum data value used in the classification. </returns>
	public virtual double getClassificationDataMax()
	{
		return _double_data_max;
	}

	/// <summary>
	/// Set the maximum displayed value used in the classification.
	/// This is typically the maximum absolute value rounded to a more presentable
	/// value. </summary>
	/// <param name="max_data"> Maximum value to display. </param>
	public virtual void setClassificationDataDisplayMax(double max_data)
	{
		_double_data_display_max = max_data;
	}

	/// <summary>
	/// Set the maximum value used in the classification (for use with scaled
	/// classification).  This is typically the maximum absolute value. </summary>
	/// <param name="max_data"> Maximum value from the data set. </param>
	public virtual void setClassificationDataMax(double max_data)
	{
		_double_data_max = max_data;
	}

	} // End of GRScaledClassificationSymbol

}