using System;

// DragAndDropTransferPrimitive - class for holding primitive data for easy transfer between drag and drop components

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

//-----------------------------------------------------------------------------
// DragAndDropTransferPrimitive - A class for holding primitive data for easy 
//	transfer between drag and drop components.
//-----------------------------------------------------------------------------
// Copyright: See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History: 
// 2004-02-26	J. Thomas Sapienza, RTi	Initial version.
// 2004-03-04	JTS, RTi		Updated Javadocs in response to 
//					numerous changes.
// 2004-04-27	JTS, RTi		* Revised after SAM's review.
//					* Renamed from DragAndDropPrimitive to
//					  DragAndDropTransferPrimitive.
//-----------------------------------------------------------------------------

namespace RTi.Util.GUI
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class provides a wrapper for transferring primitive data types (boolean,
	/// double, float, int and String) between drag and drop components.  It can be 
	/// used for transferring simple data.  More complex objects can be transferred by:
	/// making the class extend Serializable and Transferable and filling in the 
	/// required method bodies.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DragAndDropTransferPrimitive implements java.io.Serializable, java.awt.datatransfer.Transferable
	[Serializable]
	public class DragAndDropTransferPrimitive : Transferable
	{

	/// <summary>
	/// Class name.
	/// </summary>
	private readonly string __CLASS = "DragAndDropTransferPrimitive";

	/// <summary>
	/// Used to refer to the types of data that can be stored in this class.
	/// </summary>
	public const int TYPE_BOOLEAN = 0, TYPE_DOUBLE = 1, TYPE_FLOAT = 2, TYPE_INT = 3, TYPE_STRING = 4;

	/// <summary>
	/// Boolean data that can be stored in this class.
	/// </summary>
	private bool __b;

	/// <summary>
	/// Flavor for local transfer of boolean data.
	/// </summary>
	public static readonly DataFlavor booleanFlavor = new DataFlavor(typeof(DragAndDropTransferPrimitive), "Boolean");

	/// <summary>
	/// Flavor for local transfer of double data.
	/// </summary>
	public static readonly DataFlavor doubleFlavor = new DataFlavor(typeof(DragAndDropTransferPrimitive), "Double");

	/// <summary>
	/// Flavor for local transfer of float data.
	/// </summary>
	public static readonly DataFlavor floatFlavor = new DataFlavor(typeof(DragAndDropTransferPrimitive), "Float");

	/// <summary>
	/// Flavor for local transfer of int data.
	/// </summary>
	public static readonly DataFlavor intFlavor = new DataFlavor(typeof(DragAndDropTransferPrimitive), "Integer");

	/// <summary>
	/// Flavor for local transfer of string data.
	/// </summary>
	public static readonly DataFlavor stringFlavor = new DataFlavor(typeof(DragAndDropTransferPrimitive), "String");

	/// <summary>
	/// Flavor for simple plain text transfer, including text transfer into
	/// non-Java applications.
	/// </summary>
	public static readonly DataFlavor textFlavor = DataFlavor.stringFlavor;

	/// <summary>
	/// Double data that can be stored in this class.
	/// </summary>
	private double __d;

	/// <summary>
	/// Float data that can be stored in this class.
	/// </summary>
	private float __f;

	/// <summary>
	/// Int data that can be stored in this class.
	/// </summary>
	private int __i;

	/// <summary>
	/// Type of data stored in this class.
	/// </summary>
	private int __type = -1;

	/// <summary>
	/// String data that can be stored in this class.
	/// </summary>
	private string __s;

	/// <summary>
	/// Private constructor so this class can never be instantiated with no parameters.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private DragAndDropTransferPrimitive()
	private DragAndDropTransferPrimitive()
	{
	}

	/// <summary>
	/// Constructor for object to hold boolean data. </summary>
	/// <param name="B"> Boolean wrapper around boolean data to transfer. </param>
	public DragAndDropTransferPrimitive(bool? B)
	{
		__b = B.Value;
		__type = TYPE_BOOLEAN;
	}

	/// <summary>
	/// Constructor for object to hold boolean data. </summary>
	/// <param name="b"> boolean data to transfer. </param>
	public DragAndDropTransferPrimitive(bool b)
	{
		__b = b;
		__type = TYPE_BOOLEAN;
	}

	/// <summary>
	/// Constructor for object to hold double data. </summary>
	/// <param name="D"> Double wrapper around double data to transfer. </param>
	public DragAndDropTransferPrimitive(double? D)
	{
		__d = D.Value;
		__type = TYPE_DOUBLE;
	}

	/// <summary>
	/// Constructor for object to hold double data. </summary>
	/// <param name="d"> double data to transfer. </param>
	public DragAndDropTransferPrimitive(double d)
	{
		__d = d;
		__type = TYPE_DOUBLE;
	}

	/// <summary>
	/// Constructor for object to hold float data. </summary>
	/// <param name="F"> Float wrapper around float data to transfer. </param>
	public DragAndDropTransferPrimitive(float? F)
	{
		__f = F.Value;
		__type = TYPE_FLOAT;
	}

	/// <summary>
	/// Constructor for object to hold float data. </summary>
	/// <param name="f"> float data to transfer. </param>
	public DragAndDropTransferPrimitive(float f)
	{
		__f = f;
		__type = TYPE_FLOAT;
	}

	/// <summary>
	/// Constructor for object to hold int data. </summary>
	/// <param name="I"> Integer wrapper around int data to transfer. </param>
	public DragAndDropTransferPrimitive(int? I)
	{
		__i = I.Value;
		__type = TYPE_INT;
	}

	/// <summary>
	/// Constructor for object to hold int data. </summary>
	/// <param name="i"> int data to transfer. </param>
	public DragAndDropTransferPrimitive(int i)
	{
		__i = i;
		__type = TYPE_INT;
	}

	/// <summary>
	/// Constructor for object to hold String data.  Null strings are turned into
	/// empty strings. </summary>
	/// <param name="s"> String data to transfer. </param>
	public DragAndDropTransferPrimitive(string s)
	{
		if (string.ReferenceEquals(s, null))
		{
			__s = "";
		}
		else
		{
			__s = s;
		}
		__type = TYPE_STRING;
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~DragAndDropTransferPrimitive()
	{
		__s = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the data stored in this object. </summary>
	/// <returns> the data stored in this object. </returns>
	public virtual object getData()
	{
		switch (__type)
		{
			case TYPE_BOOLEAN:
				return new bool?(__b);
			case TYPE_DOUBLE:
				return new double?(__d);
			case TYPE_FLOAT:
				return new float?(__f);
			case TYPE_INT:
				return new int?(__i);
			case TYPE_STRING:
				return __s;
		}
		// will never happen, just required for the compiler:
		return null;
	}

	/// <summary>
	/// Returns the type of data stored in this object. </summary>
	/// <returns> the type of data stored in this object. </returns>
	public virtual int getDataType()
	{
		return __type;
	}

	/// <summary>
	/// Returns the data to be transferred using the specified flavor. </summary>
	/// <param name="flavor"> the flavor in which data should be transferred. </param>
	/// <returns> the data to be transferred using the specified flavor. </returns>
	/// <exception cref="UnsupportedFlavorException"> if the flavor is not supported. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getTransferData(java.awt.datatransfer.DataFlavor flavor) throws java.awt.datatransfer.UnsupportedFlavorException
	public virtual object getTransferData(DataFlavor flavor)
	{
		if (!isDataFlavorSupported(flavor))
		{
			throw new UnsupportedFlavorException(flavor);
		}

		DataFlavor[] flavors = getTransferDataFlavors();

		// check to see if it's a local copy of a data object or a serializable
		// object.  Either way, return the current object.  For local data 
		// it will be returned as normal, otherwise the JVM takes care of
		// serializing it.
		if (flavor.Equals(flavors[0]))
		{
			return this;
		}
		// just a plain text return.  Convert the value to a String and
		// return it.
		else
		{
			string s = null;
			switch (__type)
			{
				case TYPE_BOOLEAN:
					s = "" + __b;
					goto case TYPE_DOUBLE;
				case TYPE_DOUBLE:
					s = "" + __d;
					goto case TYPE_FLOAT;
				case TYPE_FLOAT:
					s = "" + __f;
					goto case TYPE_INT;
				case TYPE_INT:
					s = "" + __i;
					goto case TYPE_STRING;
				case TYPE_STRING:
					s = __s;
				break;
			}
			try
			{
				return s;
			}
			catch (Exception e)
			{
				Message.printWarning(2, __CLASS + ".getTransferData", e);
				return null;
			}
		}
	}

	/// <summary>
	/// Returns the array of data flavors in which the current object can be 
	/// transferred. </summary>
	/// <returns> the array of data flavors in which the current object can be
	/// transferred. </returns>
	public virtual DataFlavor[] getTransferDataFlavors()
	{
		return getTransferDataFlavors(__type);
	}

	/// <summary>
	/// Returns the array of data flavors in which an object of the specified type can
	/// be transferred. </summary>
	/// <param name="type"> the type of data for which to return the data flavors. </param>
	/// <returns> the array of data flavors in which an object of the specified type can
	/// be transferred. </returns>
	public static DataFlavor[] getTransferDataFlavors(int type)
	{
		DataFlavor[] flavors = new DataFlavor[2];
		switch (type)
		{
			case TYPE_BOOLEAN:
				flavors[0] = booleanFlavor;
				break;
			case TYPE_DOUBLE:
				flavors[0] = doubleFlavor;
				break;
			case TYPE_FLOAT:
				flavors[0] = floatFlavor;
				break;
			case TYPE_INT:
				flavors[0] = intFlavor;
				break;
			case TYPE_STRING:
				flavors[0] = stringFlavor;
				break;
		}
		flavors[1] = textFlavor;
		return flavors;
	}

	/// <summary>
	/// Checks to see if a certain data flavor is supported by this class. </summary>
	/// <param name="flavor"> the flavor to check. </param>
	/// <returns> true if the flavor is supported, false if not. </returns>
	public virtual bool isDataFlavorSupported(DataFlavor flavor)
	{
		DataFlavor[] flavors = getTransferDataFlavors();
		for (int i = 0; i < flavors.Length; i++)
		{
			if (flavors[i].Equals(flavor))
			{
				return true;
			}
		}
		return false;
	}

	}

}