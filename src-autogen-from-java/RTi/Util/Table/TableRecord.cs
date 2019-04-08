using System;

// TableRecord - this class is used to contain all the information associated with one record from a table

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

namespace RTi.Util.Table
{

	//import java.util.ArrayList;
	//import java.util.List;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This class is used to contain all the information associated with one record in
	/// a DataTable.  The field values are stored as an array of Object (formerly List of Object).
	/// The record field values must be consistent with the definition of the DataTable.
	/// An example of defining a TableRecord is as follows.  Setting the record size initially
	/// improves performance because less memory adjustment is required:
	/// <para>
	/// 
	/// <pre>
	/// TableRecord contents = new TableRecord (3);
	/// contents.addFieldValue ( "123456" );
	/// contents.addFieldValue ( new Integer (6));
	/// contents.addFieldValue ( "Station ID" );
	/// </pre>
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= RTi.Util.Table.Table </seealso>
	/// <seealso cref= RTi.Util.Table.TableField </seealso>
	public class TableRecord
	{

	/// <summary>
	/// Whether the data has been changed or not.
	/// </summary>
	private bool __dirty = false;

	/// <summary>
	/// Whether the record is selected, for example for further processing by commands.
	/// </summary>
	private bool __isSelected = false;

	/// <summary>
	/// List of data values corresponding to the different fields.  Currently no list methods are exposed.
	/// </summary>
	//private List<Object> __recordList = null;
	//TODO SAM 2012-05-12 the setSize() method in this class needs to be reviewed - limits using List<Object> here
	private object[] __recordArray = null;
	/// <summary>
	/// Indicate whether record objects should be managed as an array (true) or list (false).
	/// Arrays take less memory but require more memory manipulation if the array is resized dynamically.
	/// </summary>
	private bool __useArray = true;
	/// <summary>
	/// Indicate how many values have been set in the record, needed because array may be sized but not populated.
	/// A value of -1 indicates that no columns have been populated.
	/// </summary>
	private int __colMax = -1;

	/// <summary>
	/// Construct a new record (with no contents).
	/// </summary>
	public TableRecord()
	{
		initialize(1);
	}

	/// <summary>
	/// Construct a new record which will have the specified number of elements.
	/// The number of elements can increase as new items are added; however, specifying
	/// the correct number at initialization increases performance </summary>
	/// <param name="num"> Number of expected fields. </param>
	public TableRecord(int num)
	{
		initialize(num);
	}

	/// <summary>
	/// Create a copy of the object. The result is a complete deep copy. </summary>
	/// <param name="rec"> TableRecord instance to copy </param>
	/// <returns> a copy of the input </returns>
	public TableRecord(TableRecord rec)
	{
		// Copy primitives
		this.__dirty = rec.__dirty;
		this.__useArray = rec.__useArray;
		this.__colMax = rec.__colMax;
		// Now clone the record array including the objects in the record...
		if (rec.__recordArray == null)
		{
			this.__recordArray = null;
		}
		else
		{
			this.__recordArray = new object[rec.__recordArray.Length];
			object o;
			for (int i = 0; i < rec.__recordArray.Length; i++)
			{
				// Could serialize but since only certain classes are handled by DataTable, can handle
				o = rec.__recordArray[i];
				if (o == null)
				{
					this.__recordArray[i] = null;
				}
				// Primitives objects are immutable so can assign directly.  Only need to deal with non-mutable objects
				else if (o is System.DateTime)
				{
					this.__recordArray[i] = new System.DateTime(((System.DateTime)o).Ticks);
				}
				else if (o is DateTime)
				{
					this.__recordArray[i] = new DateTime((DateTime)o);
				}
				else
				{
					// Just set the value
					// TODO SAM 2014-01-09 Could be an issue since object is shared
					this.__recordArray[i] = rec.__recordArray[i];
				}
			}
		}
	}

	/// <summary>
	/// Initialize the record. </summary>
	/// <param name="num"> Number of fields in the record (for memory purposes). </param>
	private void initialize(int num)
	{
		__dirty = false;
		if (__useArray)
		{
			__recordArray = new object[num];
			__colMax = -1;
		}
		else
		{
			//__recordList = new ArrayList<Object>(num);
		}
	}

	/// <summary>
	/// Add a field data value to the record, at the end of the record </summary>
	/// <param name="newElement"> Data object to add to record. </param>
	public virtual void addFieldValue(object newElement)
	{
		addFieldValue(-1,newElement);
	}

	/// <summary>
	/// Add a field data value to the record. </summary>
	/// <param name="insertPos"> insert position (or -1 or >= record field count to insert at end) </param>
	/// <param name="newElement"> Data object to add to record. </param>
	public virtual void addFieldValue(int insertPos, object newElement)
	{
		if (__useArray)
		{
			if ((insertPos < 0) || (insertPos >= __recordArray.Length))
			{
				// Add at the end
				if (__colMax == -1)
				{
					// No array has been assigned
					__recordArray = new object[1];
				}
				else if (__colMax == (__recordArray.Length - 1))
				{
					// Have at least one column and need to increment the array size
					object[] temp = __recordArray;
					__recordArray = new object[__recordArray.Length + 1];
					Array.Copy(temp, 0, __recordArray, 0, temp.Length);
				}
				++__colMax;
				__recordArray[__colMax] = newElement;
			}
			else
			{
				// Insert at the desired position, have to do two array copies on either side
				object[] temp = __recordArray;
				__recordArray = new object[__recordArray.Length + 1];
				if (insertPos > 0)
				{
					// Copy the first part
					Array.Copy(temp, 0, __recordArray, 0, insertPos);
				}
				// Set the new value
				__recordArray[insertPos] = newElement;
				// Copy the second part
				Array.Copy(temp, insertPos, __recordArray, (insertPos + 1), (temp.Length - insertPos));
				if (insertPos > __colMax)
				{
					// Assume inserted column has data
					__colMax = insertPos;
				}
				else
				{
					++__colMax;
				}
			}
		}
		else
		{
			//__recordList.add(new_element);
		}
	}

	/// <summary>
	/// Deletes a field's data value from the record, shifting all other values "left". </summary>
	/// <param name="fieldNum"> the number of the field to delete (0+). </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void deleteField(int fieldNum) throws Exception
	public virtual void deleteField(int fieldNum)
	{
		if (__useArray)
		{
			if (fieldNum < 0 || fieldNum > (__colMax))
			{
				throw new Exception("Field num " + fieldNum + " out of bounds.");
			}
			// Set the internal object to null just to make sure the value does not mistakenly get used
			__recordArray[fieldNum] = null;
			if (fieldNum == (__recordArray.Length - 1))
			{
				// Removing the last value so don't need to do a shift
			}
			else
			{
				// Copy the right-most objects one to the left
				Array.Copy(__recordArray, (fieldNum + 1), __recordArray, fieldNum, (__recordArray.Length - 1 - fieldNum));
			}
			--__colMax;
		}
		else
		{
			/*
			if (fieldNum < 0 || fieldNum > (__recordList.size() - 1)) {
				throw new Exception ("Field num " + fieldNum + " out of bounds.");
			}
			__recordList.remove(fieldNum);
			*/
		}
	}

	/// <summary>
	/// Return the contents of a record field. </summary>
	/// <returns> contents at the specified zero-based index.
	/// The returned object must be properly cast. </returns>
	/// <exception cref="Exception"> if an invalid index is requested. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object getFieldValue(int index) throws Exception
	public virtual object getFieldValue(int index)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(20, "TableRecord.getFieldValue", "Getting index " + index);
		}
		//if ( __useArray ) {
			if (__colMax < index)
			{
				throw new Exception("Column index [" + index + "] invalid (record has " + __colMax + " columns)");
			}
			return __recordArray[index];
		//}
		/*
		else {
			if (__recordList.size() <= index) {
				throw new Exception ("Column index [" + index + "] invalid (record has " + __recordList.size() + " columns)");
			}
			return __recordList.get(index);
		}      */
	}

	/// <summary>
	/// Return the contents of a string record field.
	/// If the field is not of type string then the string version of the field is used from "" + field cast. </summary>
	/// <returns> contents at the specified zero-based index. </returns>
	/// <exception cref="Exception"> if an invalid index is requested. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getFieldValueString(int index) throws Exception
	public virtual string getFieldValueString(int index)
	{
		if (Message.isDebugOn)
		{
			Message.printDebug(20, "TableRecord.getFieldValue", "Getting index " + index);
		}
		object o = null;
		if (__useArray)
		{
			if (__colMax < index)
			{
				throw new Exception("Column index [" + index + "] invalid (record has " + __colMax + " columns)");
			}
			o = __recordArray[index];
		}
		/*
		else {
		    if (__recordList.size() <= index) {
		        throw new Exception ("Column index [" + index + "] invalid (record has " + __recordList.size() + " columns)");
		    }
		    o = __recordList.get(index);
		}*/
		if (o == null)
		{
			return null;
		}
		else
		{
			// Do this to handle cast from integer to string, where integer ID is used in column
			return "" + o;
		}
	}


	/// <summary>
	/// Returns whether the record is dirty or not. </summary>
	/// <returns> whether the record is dirty or not. </returns>
	public virtual bool getIsSelected()
	{
		return __isSelected;
	}

	/// <summary>
	/// Return the number of fields in the record. </summary>
	/// <returns> the number of fields in the record.   </returns>
	public virtual int getNumberOfFields()
	{
		//if ( __useArray ) {
			return (__colMax + 1);
		//}
		//else {
			//return __recordList.size();
		//}
	}

	/// <summary>
	/// Returns whether the record is dirty or not. </summary>
	/// <returns> whether the record is dirty or not. </returns>
	public virtual bool isDirty()
	{
		return __dirty;
	}

	/// <summary>
	/// Sets the field contents of the record at the specified zero-based index.
	/// The number of available fields should be set in the constructor or use setNumberOfFields(). </summary>
	/// <param name="col"> Field position to set (0+). </param>
	/// <param name="contents"> Field contents to set. </param>
	/// <exception cref="if"> the index exceeds the available number of fields within this record. </exception>
	/// <returns> the instance of this record, to facilitate chaining set calls. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TableRecord setFieldValue(int col, Object contents) throws Exception
	public virtual TableRecord setFieldValue(int col, object contents)
	{
		if (__useArray)
		{
			if (col <= __colMax)
			{
				__recordArray[col] = contents;
			}
			else
			{
				throw new Exception("Column index [" + col + "] invalid (record has " + (__colMax + 1) + " columns)");
			}
		}
		else
		{
			/*
			if (index < __recordList.size()) {
				__recordList.set(index,contents);
			}
			else {	
				throw new Exception("Column index [" + index + "] invalid (record has " + __recordList.size() + " columns)");
			}
			*/
		}
		return this;
	}

	/// <summary>
	/// Sets whether the table record is dirty or not. </summary>
	/// <param name="dirty"> whether the table record is dirty or not. </param>
	public virtual void setDirty(bool dirty)
	{
		__dirty = dirty;
	}


	/// <summary>
	/// Sets whether the table record is selected. </summary>
	/// <param name="isSelected"> whether the table record is selected. </param>
	public virtual void setIsSelected(bool isSelected)
	{
		__isSelected = isSelected;
	}

	// TODO SAM 2009-07-26 Evaluate what is using this - limits using more generic List for data
	// FIXME SAM 2013-09-18 Comment out and see what complains.  This method is not useful because can't
	// just add row columns without knowing the column metadata (type, etc.)
	/// <summary>
	/// Sets the number of fields within this record.  If the previous number of
	/// fields is larger than the new number, those fields after the new number of fields will be lost. </summary>
	/// <param name="num"> Number of fields to include in the record. </param>
	//public void setNumberOfFields(int num)
	//{	__recordList.setSize(num);
	//}

	}

}