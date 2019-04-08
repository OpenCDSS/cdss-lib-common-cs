using System.IO;

// EndianDataInputStream - read little or big endian binary data streams

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
// EndianDataInputStream - read little or big endian binary data streams
// ----------------------------------------------------------------------------
// History:
//
// 14 Jun 1999	Steven A. Malers,	Initial version.
//		Riverside Technology,
//		inc.
// 2001-11-26	SAM, RTi		Add readChar1() and readString1() to
//					read big-endian 8-bit characters and
//					strings.  These methods are not found
//					in the standard big-endian
//					DataInputStream.
// 2004-05-05	J. Thomas Sapienza, RTi	Added support for reading both
//					big endian and little endian values 
//					from the stream.
// 2004-5-07	Scott Townsend, RTi	Added a readEndianChar1 method.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	/// <summary>
	/// This class extends the basic DataInputStream class by providing LittleEndian
	/// versions of some read methods.  This allows one input stream to be used to
	/// read big and little endian values.  By default, Java assumes big endian
	/// data.  Use the normal DataInputStream methods for reading unless you
	/// specifically know that values are little endian, in which case the methods of
	/// this derived class should be used. </summary>
	/// <seealso cref= java.io.DataInputStream </seealso>
	public class EndianDataInputStream : DataInputStream
	{

	/// <summary>
	/// Boolean to specify whether the stream is big endian or not.  Java default is
	/// that streams ARE big endian.
	/// </summary>
	private bool __is_big_endian = true;

	/// <summary>
	/// Specifies whether to match the current system's endianness.
	/// </summary>
	private bool __match_system = false;

	/// <summary>
	/// Byte array used for reading from the stream.
	/// </summary>
	private sbyte[] __byte8 = null;

	/// <summary>
	/// Construct using an InputStream. </summary>
	/// <param name="istream"> the InputStream from which to construct this Endian input stream. </param>
	public EndianDataInputStream(Stream istream) : base(istream)
	{
		__byte8 = new sbyte[8];
		__is_big_endian = IOUtil.isBigEndianMachine();
	}

	/// <summary>
	/// Construct using an InputStream. </summary>
	/// <param name="istream"> the InputStream from which to construct this Endian input stream. </param>
	/// <param name="matchSystem"> whether to match the system's current endianness when readin
	/// from the stream. </param>
	public EndianDataInputStream(Stream istream, bool matchSystem) : base(istream)
	{
		__byte8 = new sbyte[8];
		__match_system = matchSystem;
		__is_big_endian = IOUtil.isBigEndianMachine();
	}

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~EndianDataInputStream()
	{
		__byte8 = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Indicate whether the file is big-endian.  The default is to match the
	/// endian-ness of the machine but setBigEndian() can be used to explicitly indicate
	/// that the file is little endian.  For example, if a binary file is written on
	/// one machine and transferred to another of different endian-ness, then the
	/// endian-ness of the file should be based on the file, not the machine. </summary>
	/// <returns> true if the file should be treated as big-endian, false
	/// if it should be treated as little-endian. </returns>
	public virtual bool isBigEndian()
	{
		return __is_big_endian;
	}

	/// <summary>
	/// Similar to DataInputStream.readChar except read a one-byte big-endian character. </summary>
	/// <returns> a character read from a one-byte big endian character. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final char readChar1() throws java.io.IOException
	public char readChar1()
	{
		if (read(__byte8, 0, 1) != 1)
		{
			throw new IOException();
		}
		return (char)(__byte8[0]);
	}

	/// <summary>
	/// Read a 64-bit double from the file using an endian-ness that matches the system
	/// (match_system=true should be used in the constructor if this method is
	/// called - otherwise Java big-endian is assumed). </summary>
	/// <returns> the 64-bit double read from the file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double readEndianDouble() throws java.io.IOException
	public virtual double readEndianDouble()
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			return readLittleEndianDouble();
		}
		else
		{ // Default Java or system that is big-endian...
			return readDouble();
		}
	}

	/// <summary>
	/// Read a 32-bit float from the file using an endian-ness that matches the system
	/// (match_system=true should be used in the constructor if this method is
	/// called - otherwise Java big-endian is assumed). </summary>
	/// <returns> the 32-bit float read from the file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public float readEndianFloat() throws java.io.IOException
	public virtual float readEndianFloat()
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			return readLittleEndianFloat();
		}
		else
		{ // Default Java or system that is big-endian...
			return readFloat();
		}
	}

	/// <summary>
	/// Read a 32-bit int from the file using an endian-ness that matches the system
	/// (match_system=true should be used in the constructor if this method is
	/// called - otherwise Java big-endian is assumed). </summary>
	/// <returns> the 32-bit int read from the file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readEndianInt() throws java.io.IOException
	public virtual int readEndianInt()
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			return readLittleEndianInt();
		}
		else
		{ // Default Java or system that is big-endian...
			return readInt();
		}
	}

	/// <summary>
	/// Read a 64-bit long integer from the file using an endian-ness that matches the
	/// system (match_system=true should be used in the constructor if this method is
	/// called - otherwise Java big-endian is assumed). </summary>
	/// <returns> the 64-bit long integer read from the file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long readEndianLong() throws java.io.IOException
	public virtual long readEndianLong()
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			return readLittleEndianLong();
		}
		else
		{ // Default Java or system that is big-endian...
			return readLong();
		}
	}

	/// <summary>
	/// Read a signed 16-bit integer from the file using an endian-ness that matches the
	/// system (match_system=true should be used in the constructor if this method is
	/// called - otherwise Java big-endian is assumed). </summary>
	/// <returns> the signed 16-bit integer read from the file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public short readEndianShort() throws java.io.IOException
	public virtual short readEndianShort()
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			return readLittleEndianShort();
		}
		else
		{ // Default Java or system that is big-endian...
			return readShort();
		}
	}

	/// <summary>
	/// Similar to DataInputStream.readChar except read a endian 1 byte (8-bit)
	/// from the file using an endian-ness that matches the
	/// system (match_system=true should be used in the constructor if this method is
	/// called - otherwise Java big-endian is assumed). </summary>
	/// <returns> the Character read from the file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public char readEndianChar1() throws java.io.IOException
	public virtual char readEndianChar1()
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			return readLittleEndianChar1();
		}
		else
		{ // Default Java or system that is big-endian...
			return readChar1();
		}
	}

	/// <summary>
	/// Similar to DataInputStream.readShort except read a little endian number. </summary>
	/// <returns> a short read from a little endian number. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final short readLittleEndianShort() throws java.io.IOException
	public short readLittleEndianShort()
	{
		if (read(__byte8, 0, 2) != 2)
		{
			throw new IOException("Unexpected end of file.");
		}
		return (short)((__byte8[1] & 0xff) << 8 | (__byte8[0] & 0xff));
	}

	/// <summary>
	/// Similar to DataInputStream.readUnsignedShort except read a little endian number. </summary>
	/// <returns> Integer value of the short. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final int readLittleEndianUnsignedShort() throws java.io.IOException
	public int readLittleEndianUnsignedShort()
	{
		if (read(__byte8, 0, 2) != 2)
		{
			throw new IOException("Unexpected end of file.");
		}
		return ((__byte8[1] & 0xff) << 8 | (__byte8[0] & 0xff));
	}

	/// <summary>
	/// Similar to DataInputStream.readChar except read a little endian character.
	/// This is a 16-bit Unicode character.  Use readLittleEndianChar1 to read a one
	/// byte character. </summary>
	/// <returns> a character read from a little endian character. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final char readLittleEndianChar() throws java.io.IOException
	public char readLittleEndianChar()
	{
		if (read(__byte8, 0, 2) != 2)
		{
			throw new IOException();
		}
		return (char)((__byte8[1] & 0xff) << 8 | (__byte8[0] & 0xff));
	}

	/// <summary>
	/// Similar to DataInputStream.readChar except read a little endian 1 byte (8-bit)
	/// character.  The 8-bit character is converted to a 16-bit character, which is
	/// returned. </summary>
	/// <returns> a character read from a little endian character. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final char readLittleEndianChar1() throws java.io.IOException
	public char readLittleEndianChar1()
	{
		if (read(__byte8, 0, 1) != 1)
		{
			throw new IOException();
		}
		// Treat as a byte...
		return (char)(__byte8[0] & 0xff);
	}

	/// <summary>
	/// Similar to DataInputStream.readByte except read a little endian 1 byte (8-bit). </summary>
	/// <returns> a byte read from a little endian byte. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final byte readLittleEndianByte() throws java.io.IOException
	public sbyte readLittleEndianByte()
	{
		if (read(__byte8, 0, 1) != 1)
		{
			throw new IOException();
		}
		// Treat as a byte...
		return unchecked((sbyte)(__byte8[0] & 0xff));
	}

	/// <summary>
	/// Simiar to DataInputStream.readInt except read a little endian number. </summary>
	/// <returns> an integer read from a little endian number. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final int readLittleEndianInt() throws java.io.IOException
	public int readLittleEndianInt()
	{
		if (read(__byte8, 0, 4) != 4)
		{
			throw new IOException("Unexpected end of file.");
		}
		return (__byte8[3] & 0xff) << 24 | (__byte8[2] & 0xff) << 16 | (__byte8[1] & 0xff) << 8 | (__byte8[0] & 0xff);
	}

	/// <summary>
	/// Similar to DataInputStream.readInt except read a little endian number. </summary>
	/// <returns> an integer read from a little endian number. </returns>
	/// <param name="bytes"> number of bytes to read for the integer. </param>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final int readLittleEndianInt(int bytes) throws java.io.IOException
	public int readLittleEndianInt(int bytes)
	{
		if (read(__byte8, 0, bytes) != bytes)
		{
			throw new IOException("Premature end of file.");
		}
		int ret = 0;
		for (int i = 0; i < bytes; i++)
		{
			ret = ret | (__byte8[i] << (i * 8));
		}
		return ret;
	}

	/// <summary>
	/// Similar to DataInputStream.readLong except read a little endian number. </summary>
	/// <returns> a long integer read from a little endian number. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final long readLittleEndianLong() throws java.io.IOException
	public long readLittleEndianLong()
	{
		if (read(__byte8, 0, 8) != 8)
		{
			throw new IOException("Premature end of file.");
		}
		return (long)(__byte8[7] & 0xff) << 56 | (long)(__byte8[6] & 0xff) << 48 | (long)(__byte8[5] & 0xff) << 40 | (long)(__byte8[4] & 0xff) << 32 | (long)(__byte8[3] & 0xff) << 24 | (long)(__byte8[2] & 0xff) << 16 | (long)(__byte8[1] & 0xff) << 8 | (long)(__byte8[0] & 0xff);
	}

	/// <summary>
	/// Similar to DataInputStream.readFloat except read a little endian number. </summary>
	/// <returns> a float read from a little endian number. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final float readLittleEndianFloat() throws java.io.IOException
	public float readLittleEndianFloat()
	{
		return Float.intBitsToFloat(readLittleEndianInt());
	}

	/// <summary>
	/// Similar to DataInputStream.readDouble except read a little endian number. </summary>
	/// <returns> a double read from a little endian number. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final double readLittleEndianDouble() throws java.io.IOException
	public double readLittleEndianDouble()
	{
		return Double.longBitsToDouble(readLittleEndianLong());
	}

	/// <summary>
	/// Read a string of one-byte characters. </summary>
	/// <returns> a String read from a 1-byte character string. </returns>
	/// <param name="size"> Number of characters to read. </param>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final String readString1(int size) throws java.io.IOException
	public string readString1(int size)
	{
		char[] c = new char[size];
		for (int i = 0; i < size; i++)
		{
			if (read(__byte8, 0, 1) != 1)
			{
				throw new IOException();
			}
			c[i] = (char)(__byte8[0]);
		}
		string s = new string(c);
		c = null;
		return s;
	}

	/// <summary>
	/// Set whether the file is big-endian.  The default is to match the
	/// endian-ness of the machine but setBigEndian() can be used to explicitly indicate
	/// that the file is little endian.  For example, if a binary file is written on
	/// one machine and transferred to another of different endian-ness, then the
	/// endian-ness of the file should be based on the file, not the machine.
	/// Note that once the endian-ness is set (either by default or with this method),
	/// the writeEndian*() methods can only be called if the match_system parameter
	/// was set to true at construction. </summary>
	/// <param name="is_big_endian"> true if the file should be treated as big-endian, false
	/// if it should be treated as little-endian. </param>
	public virtual void setBigEndian(bool is_big_endian)
	{
		__is_big_endian = is_big_endian;
	}

	} // end EndianDataInputStream class

}