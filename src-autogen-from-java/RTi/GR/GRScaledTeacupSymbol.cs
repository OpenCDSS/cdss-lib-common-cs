﻿using System;

// GRScaledTeacupSymbol - symbol definition information for GRSymbol.CLASSIFICATION_SCALED_TEACUP_SYMBOL type

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
// GRScaledTeacupSymbol - 	Store symbol definition information for
//				GRSymbol.CLASSIFICATION_SCALED_TEACUP_SYMBOL 
//				type.
// ----------------------------------------------------------------------------
// History:
//
// 2004-08-10	J. Thomas Sapienza, RTi	Initial version.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GR
{
	/// <summary>
	/// This class stores information necessary to draw scaled teacups.  Scale 
	/// teacups are used when it is necessary to show how much something is filled
	/// and also to scale the entire teacup to represent greater capacities.
	/// </summary>
	public class GRScaledTeacupSymbol : GRSymbol
	{

	/// <summary>
	/// The maximum capacity of any feature being represented by a teacup.  All the 
	/// teacups that are being drawn for a single layer should have this value set
	/// so that it is known how to scale them based on their max capacity.
	/// </summary>
	private double __maxCapacity = -1;

	/// <summary>
	/// The field in the attribute table that has the current capacity of the teacup.
	/// </summary>
	private int __currentCapacityField = -1;

	/// <summary>
	/// The field in the attribute table that has the maximum capacity of the teacup.
	/// </summary>
	private int __maxCapacityField = -1;

	/// <summary>
	/// The field in the attribute table that has the minimum capacity of the teacup.
	/// </summary>
	private int __minCapacityField = -1;

	/// <summary>
	/// Constructor.  The symbol style defaults to GRSymbol.SYM_TEACUP. </summary>
	/// <param name="fields"> integer array of the field numbers in the attribute table that
	/// provide data about how to scale and fill the teacup.  The array must be 3 
	/// elements large, and the order of the values is important:<para>
	/// <ol>
	/// <li>MaxCapacityField - the first element should have the field that stores
	/// the maximum capacity of the teacup.</li>
	/// <li>MinCapacityField - the second element should have the field that stores
	/// the minimum capacity of the teacup.</li>
	/// <li>CurrentCapacity - the third element should have the field that stores 
	/// the current capacity of the teacup.</li>
	/// </ol>
	/// </para>
	/// </param>
	/// <exception cref="Exception"> if the array is not 3 elements in size </exception>
	/// <exception cref="NullPointerException"> if the array is null </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GRScaledTeacupSymbol(int[] fields) throws Exception
	public GRScaledTeacupSymbol(int[] fields) : base()
	{

		if (fields == null)
		{
			throw new System.NullReferenceException();
		}

		if (fields.Length != 3)
		{
			throw new Exception("Fields array must be size 3, not: " + fields.Length);
		}

		setStyle(SYM_TEACUP);
		setClassificationType("ScaledTeacupSymbol");

		__maxCapacityField = fields[0];
		__minCapacityField = fields[1];
		__currentCapacityField = fields[2];
	}

	public virtual int getCurrentCapacityField()
	{
		return __currentCapacityField;
	}

	public virtual double getMaxCapacity()
	{
		return __maxCapacity;
	}

	public virtual int getMaxCapacityField()
	{
		return __maxCapacityField;
	}

	public virtual int getMinCapacityField()
	{
		return __minCapacityField;
	}

	public virtual void setMaxCapacity(double max)
	{
		__maxCapacity = max;
	}

	}

}