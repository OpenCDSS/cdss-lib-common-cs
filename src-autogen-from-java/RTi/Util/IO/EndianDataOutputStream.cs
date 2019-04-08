using System.IO;

// EndianDataOutputStream - write little or big endian binary data streams

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
// EndianDataOutputStream - write little or big endian binary data streams
// ----------------------------------------------------------------------------
// History:
//
// 2004-05-05	J. Thomas Sapienza, RTi	Initial version.  Most code adapted
//					from EndianDataInputStream and
//					EndianRandomAccessFile.
// 2004-12-01	Steven A. Malers, RTi	Review code.
//					* Change javadoc "read" to "write" to
//					  reflect the functionality of the code.
//					* Change parameter names from "i"
//					  (input) to "o" (output) to reflect the
//					  functionality of the code.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	/// <summary>
	/// This class extends the basic DataOutputStream class by providing LittleEndian
	/// versions of some write methods.  This allows one output stream to be used to
	/// write big and little endian values.  By default, Java assumes big endian
	/// data, and unicode (2-byte) strings.  Use this class if there is a chance that
	/// binary files may be written on multiple platforms and a standard needs to be
	/// enforced. </summary>
	/// <seealso cref= java.io.DataOutputStream </seealso>
	public class EndianDataOutputStream : DataOutputStream
	{

	/// <summary>
	/// Boolean to specify whether the stream is big endian or not.  The Java default is
	/// that streams ARE big endian.
	/// </summary>
	private bool __is_big_endian = true;

	/// <summary>
	/// Specifies whether to match the current system's endianness.
	/// </summary>
	private bool __match_system = false;

	/// <summary>
	/// Byte array used for writing to the stream.
	/// </summary>
	private sbyte[] __byte8 = null;

	/// <summary>
	/// Construct using an OutputStream. </summary>
	/// <param name="ostream"> the OutputStream from which to construct this Endian 
	/// output stream. </param>
	public EndianDataOutputStream(Stream ostream) : base(ostream)
	{
		__byte8 = new sbyte[8];
		__is_big_endian = IOUtil.isBigEndianMachine();
	}

	/// <summary>
	/// Construct using an OutputStream. </summary>
	/// <param name="ostream"> the OutputStream from which to construct this Endian 
	/// output stream. </param>
	/// <param name="match_system"> whether to match the system's current endianness when
	/// writing from the stream. </param>
	public EndianDataOutputStream(Stream ostream, bool match_system) : base(ostream)
	{
		__byte8 = new sbyte[8];
		__match_system = match_system;
		__is_big_endian = IOUtil.isBigEndianMachine();
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~EndianDataOutputStream()
	{
		__byte8 = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Indicate whether the stream is big-endian.  The default is to match the
	/// endian-ness of the machine but setBigEndian() can be used to explicitly indicate
	/// that the stream is little endian.  For example, if a binary file is written on
	/// one machine and transferred to another of different endian-ness, then the
	/// endian-ness of the stream should be based on the stream, not the machine. </summary>
	/// <returns> true if the stream should be treated as big-endian, false
	/// if it should be treated as little-endian. </returns>
	public virtual bool isBigEndian()
	{
		return __is_big_endian;
	}

	/// <summary>
	/// Set whether the stream is big-endian.  The default is to match the
	/// endian-ness of the machine but setBigEndian() can be used to explicitly indicate
	/// that the stream is little endian.  For example, if a binary file is written on
	/// one machine and transferred to another of different endian-ness, then the
	/// endian-ness of the stream should be based on the stream, not the machine.
	/// Note that once the endian-ness is set (either by default or with this method),
	/// the writeEndian*() methods can only be called if the match_system parameter
	/// was set to true at construction. </summary>
	/// <param name="is_big_endian"> true if the stream should be treated as big-endian, false
	/// if it should be treated as little-endian. </param>
	public virtual void setBigEndian(bool is_big_endian)
	{
		__is_big_endian = is_big_endian;
	}

	/// <summary>
	/// Write a 1-byte (not Unicode) character to the stream using a big-endian format.
	/// The character is written as a single byte. </summary>
	/// <param name="c"> Character to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeChar1(char c) throws java.io.IOException
	public virtual void writeChar1(char c)
	{
		__byte8[0] = (sbyte)c;
		write(__byte8, 0, 1);
	}

	/// <summary>
	/// Write a string as 1-byte (not Unicode) characters to the stream using a 
	/// big-endian format.
	/// Each character is written as a single byte. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeChar1(String s) throws java.io.IOException
	public virtual void writeChar1(string s)
	{
		int len = s.Length;
		for (int i = 0; i < len; i++)
		{
			// Get the character from the first byte from every
			// Unicode character...
			__byte8[0] = (sbyte)s[i];
			write(__byte8, 0, 1);
		}
	}

	/// <summary>
	/// Write a 2-byte Unicode character to the stream using an endian-ness that matches
	/// the system.  match_system=true should be used in the constructor if this method
	/// is called - otherwise Java big-endian is assumed. </summary>
	/// <param name="c"> Character to write, as its Unicode value. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianChar(int c) throws java.io.IOException
	public virtual void writeEndianChar(int c)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianChar(c);
		}
		else
		{
			// Default Java or system that is big-endian...
			writeChar(c);
		}
	}

	/// <summary>
	/// Write a 1-byte (not Unicode) character to the stream using an endian-ness that
	/// matches the system.  match_system=true should be used in the constructor if this
	/// method is called - otherwise Java big-endian is assumed.  The character is
	/// written as a single byte. </summary>
	/// <param name="c"> Character to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianChar1(char c) throws java.io.IOException
	public virtual void writeEndianChar1(char c)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianChar1(c);
		}
		else
		{
			// Default Java or system that is big-endian...
			__byte8[0] = (sbyte)c;
			write(__byte8, 0, 1);
		}
	}

	/// <summary>
	/// Write a string as 1-byte (not Unicode) characters to the stream using an
	/// endian-ness that matches the system.  match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed.
	/// Each character is written as a single byte. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianChar1(String s) throws java.io.IOException
	public virtual void writeEndianChar1(string s)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianChar1(s);
		}
		else
		{
			// Default Java or system that is big-endian...
			int len = s.Length;
			for (int i = 0; i < len; i++)
			{
				__byte8[0] = (sbyte)s[i];
				write(__byte8, 0, 1);
			}
		}
	}

	/// <summary>
	/// Write a string as 2-byte (Unicode) characters to the stream using an
	/// endian-ness that matches the system.  match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed.
	/// Each character is written as two-bytes. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianChars(String s) throws java.io.IOException
	public virtual void writeEndianChars(string s)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianChars(s);
		}
		else
		{
			// Default Java or system that is big-endian...
			writeChars(s);
		}
	}

	/// <summary>
	/// Write a 64-bit double to the stream using an
	/// endian-ness that matches the system.  match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed. </summary>
	/// <param name="d"> 64-bit double to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianDouble(double d) throws java.io.IOException
	public virtual void writeEndianDouble(double d)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianDouble(d);
		}
		else
		{
			// Default Java or system that is big-endian...
			writeDouble(d);
		}
	}

	/// <summary>
	/// Write a 32-bit float to the stream using an
	/// endian-ness that matches the system.  match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed. </summary>
	/// <param name="f"> 32-bit float to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianFloat(float f) throws java.io.IOException
	public virtual void writeEndianFloat(float f)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianFloat(f);
		}
		else
		{
			// Default Java or system that is big-endian...
			writeFloat(f);
		}
	}

	/// <summary>
	/// Write a 32-bit integer to the stream using an
	/// endian-ness that matches the system.  match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed. </summary>
	/// <param name="i"> 32-bit integer to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianInt(int i) throws java.io.IOException
	public virtual void writeEndianInt(int i)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianInt(i);
		}
		else
		{
			// Default Java or system that is big-endian...
			writeInt(i);
		}
	}

	/// <summary>
	/// Write a little endian char (2-byte Unicode), specifying the character as its
	/// Unicode integer equivalent. </summary>
	/// <param name="c"> Int to write. </param>
	/// <exception cref="IOException"> if there is an error writing the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar(int c) throws java.io.IOException
	public virtual void writeLittleEndianChar(int c)
	{
		//same as for shorts...

		__byte8[0] = (sbyte)c;
		__byte8[1] = (sbyte)(c >> 8);

		write(__byte8, 0, 2);
	}

	/// <summary>
	/// Write a little endian 8-bit char (not Unicode). </summary>
	/// <param name="c"> Char to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar1(char c) throws java.io.IOException
	public virtual void writeLittleEndianChar1(char c)
	{
		__byte8[0] = (sbyte)c;
		write(__byte8, 0, 1);
	}

	/// <summary>
	/// Write a little endian 8-bit byte. </summary>
	/// <param name="b"> byte to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianByte(byte b) throws java.io.IOException
	public virtual void writeLittleEndianByte(sbyte b)
	{
		__byte8[0] = (sbyte)b;
		write(__byte8, 0, 1);
	}

	/// <summary>
	/// Write a String as little endian 8-bit chars (not Unicode). </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar1(String s) throws java.io.IOException
	public virtual void writeLittleEndianChar1(string s)
	{
		int size = s.Length;

		if (size > __byte8.Length)
		{
			// Reallocate the array...
			__byte8 = new sbyte[size];
			for (int i = 0; i < size; i++)
			{
				__byte8[i] = (sbyte)s[i];
			}

			write(__byte8, 0, size);
		}
		else
		{
			for (int i = 0; i < size; i++)
			{
				__byte8[i] = (sbyte)s[i];
			}

			write(__byte8, 0, size);
		}
	}

	/// <summary>
	/// Write a String as little endian (2-byte Unicode)chars. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChars(String s) throws java.io.IOException
	public virtual void writeLittleEndianChars(string s)
	{
		int len = s.Length;
		for (int i = 0;i < len;i++)
		{
			writeChar(s[i]);
		}
	}

	/// <summary>
	/// Write a little endian 64-bit double. </summary>
	/// <param name="d"> 64-bit double to write. </param>
	/// <exception cref="IOException"> if there is an error writing to file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianDouble(double d) throws java.io.IOException
	public virtual void writeLittleEndianDouble(double d)
	{
		 //convert to floating point long
		long vl = System.BitConverter.DoubleToInt64Bits(d);

		__byte8[0] = (sbyte)vl;
		__byte8[1] = (sbyte)(vl >> 8);
		__byte8[2] = (sbyte)(vl >> 16);
		__byte8[3] = (sbyte)(vl >> 24);
		__byte8[4] = (sbyte)(vl >> 32);
		__byte8[5] = (sbyte)(vl >> 40);
		__byte8[6] = (sbyte)(vl >> 48);
		__byte8[7] = (sbyte)(vl >> 56);

		write(__byte8, 0, 8);
	}

	/// <summary>
	/// Write a little endian 32-bit float. </summary>
	/// <param name="f"> 32-bit float to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianFloat(float f) throws java.io.IOException
	public virtual void writeLittleEndianFloat(float f)
	{
		writeLittleEndianInt(Float.floatToIntBits(f));
	}

	/// <summary>
	/// Write a 32-bit little endian int. </summary>
	/// <param name="i"> 32-bit integer to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianInt(int i) throws java.io.IOException
	public virtual void writeLittleEndianInt(int i)
	{
		__byte8[0] = (sbyte)i;
		__byte8[1] = (sbyte)(i >> 8);
		__byte8[2] = (sbyte)(i >> 16);
		__byte8[3] = (sbyte)(i >> 24);

		write(__byte8, 0, 4);
	}

	/// <summary>
	/// Write a 16-bit little endian short integer. </summary>
	/// <param name="s"> 16-bit short integer value to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the stream. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianShort(short s) throws java.io.IOException
	public virtual void writeLittleEndianShort(short s)
	{
		__byte8[0] = (sbyte)s;
		__byte8[1] = (sbyte)(s >> 8);

		write(__byte8, 0, 2);
	}

	}

}