// EndianRandomAccessFile - RandomAccessFile for big or little endian file

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
// EndianRandomAccessFile.java - RandomAccessFile for big or little endian file
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
//
// 2001-08-12	Morgan Sheedy,		Initial Implementation for reading
//		Riverside Technology,	little endians.
//		inc.
// 2001-08-15	AMS, RTi		Added methods to write out little
//					endians.
// 2001-08-27	Steven A. Malers, RTi	Review code and incorporate into RTi
//					IO package.
// 2001-08-31	AMS,RTi			Overloaded every read/write method to
//					add seek methods.
// 2001-09-26	SAM, RTi		Add readLittleEndianChar1().  Change so
//					the mode is actually passed to the base
//					class in the constructor.  Fix
//					read methods.  Many were rereading
//					bytes over the first byte in _b.
// 2003-01-23	SAM, RTi		Overload the constructor to add the
//					flag indicating whether the class should
//					by default write in the same endianness
//					as the computer.  Add readEndian*() and
//					writeEndian*() methods.  Clean up the
//					javadoc.
// 2003-03-27	SAM, RTi		Change so that the buffer array _b is
//					resized when dealing with character
//					strings.  Previously a local array was
//					often created, resulting in slower
//					performance.
// 2003-11-23	SAM, RTi		Add writeChar1() since Java does not
//					have this equivalent without dealing
//					with bytes manually.  Similarly add
//					readChar1(), and readString1().
// 2003-11-26	SAM, RTi		Add readLittleEndianString1().
// 2003-12-04	SAM, RTi		* Add isBigEndian() and setBigEndian().
// 2004-03-11	Scott Townsend		Add readLittleEndianByte().
// 2004-04-06	SAT,RTi			Add writeLittleEndianByte(...).
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	/// <summary>
	/// The EndianRandomAccessFile class is used to read and write little and big endian
	/// values to/from a random access file.  The RandomAccessFile base class big endian
	/// Java methods are available and "LittleEndian" versions are available in this
	/// class for reading/writing little endian data.  Consequently, it is possible to
	/// open a binary file and read/write little and big endian values for the same
	/// file.  More often, a file will include data of one endian type.  For example,
	/// temporary binary files written with a C, C++, or Fortran program on a little
	/// endian machine will contain little endian values.  In this case, an
	/// EndianRandomAccessFile can be constructed with match_system=true and then the
	/// readEndian*() and writeEndian*() methods can be used.  If a binary file is known
	/// to always contain only big-endian values, then a RandomAccessFile should be
	/// used.  If a binary file is known to always contain only little endian values,
	/// then a EndianRandomAccessFile should be used and the LittleEndian methods should
	/// be called.
	/// </summary>
	public class EndianRandomAccessFile : RandomAccessFile
	{

	private sbyte[] _b; // Internal buffer used for byte manipulation.

	private bool __match_system = false; // Default behavior is to act like
	private bool __is_big_endian = true; // big-endian (Java default).

	/// <summary>
	/// Create a random access file stream to read from and/or write to. </summary>
	/// <param name="file"> File to read from and/or write to. </param>
	/// <param name="mode"> String indicating access mode.  Specify "r" to only read from the
	/// file (the file should already exist).  Specify "rw" to read from and/or write
	/// to the file (in this case it may be important to delete the file first so that
	/// new data are not confused with the existing contents). </param>
	/// <exception cref="IOException"> - if the file can not be found or read, if 
	/// the mode is not equal to "r" or "rw", or if the security permissions
	/// prevent read and/or write permissions. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EndianRandomAccessFile(java.io.File file, String mode) throws java.io.IOException
	public EndianRandomAccessFile(File file, string mode) : this(file, mode, false)
	{
	}

	/// <summary>
	/// Create a random access file stream to read from and/or write to. </summary>
	/// <param name="file"> File to read from and/or write to. </param>
	/// <param name="mode"> String indicating access mode.  Specify "r" to only read from the
	/// file (the file should already exist).  Specify "rw" to read from and/or write
	/// to the file (in this case it may be important to delete the file first so that
	/// new data are not confused with the existing contents). </param>
	/// <param name="match_system"> If true, then the endianness of data will match the
	/// operating system when calling the readEndian*() or writeEndian*() methods.  For
	/// example, if true and the system is big-endian, a call to
	/// writeEndianDouble() will result in the base class RandomAccessFile.writeDouble()
	/// method being called.  If true and the system is little-endian, then a call to
	/// writeEndianDouble() will result in a call to littleEndianWriteDouble().  Use
	/// this version if the file endian-ness will always be the same as the operating
	/// system endian-ness. </param>
	/// <exception cref="IOException"> - if the file can not be found or read, if 
	/// the mode is not equal to "r" or "rw", or if the security permissions
	/// prevent read and/or write permissions. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EndianRandomAccessFile(java.io.File file, String mode, boolean match_system) throws java.io.IOException
	public EndianRandomAccessFile(File file, string mode, bool match_system) : base(file, mode)
	{

		//make byte array
		_b = new sbyte[8];

		__match_system = match_system;
		__is_big_endian = IOUtil.isBigEndianMachine();
	}

	/// <summary>
	/// Create a random access file stream to read from and/or write to. </summary>
	/// <param name="file"> The name of a file to read from and/or write to. </param>
	/// <param name="mode"> String indicating access mode.  Specify "r" to only read from the
	/// file (the file should already exist).  Specify "rw" to read from and/or write
	/// to the file (in this case it may be important to delete the file first so that
	/// new data is not confused with the existing contents). </param>
	/// <exception cref="IOException"> - if the file can not be found or read, if 
	/// the mode is not equal to "r" or "rw", or if the security permissions
	/// prevent read and/or write permissions. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EndianRandomAccessFile(String file, String mode) throws java.io.IOException
	public EndianRandomAccessFile(string file, string mode) : this(file, mode, false)
	{
	}

	/// <summary>
	/// Create a random access file stream to read from and/or write to. </summary>
	/// <param name="file"> The name of a file to read from and/or write to. </param>
	/// <param name="mode"> String indicating access mode.  Specify "r" to only read from the
	/// file (the file should already exist).  Specify "rw" to read from and/or write
	/// to the file (in this case it may be important to delete the file first so that
	/// new data are not confused with the existing contents). </param>
	/// <param name="match_system"> If true, then the endianness of data will match the
	/// operating system when calling the readEndian*() or writeEndian*() methods.  For
	/// example, if true and the system is big-endian, a call to
	/// writeEndianDouble() will result in the base class RandomAccessFile.writeDouble()
	/// method being called.  If true and the system is little-endian, then a call to
	/// writeEndianDouble() will result in a call to littleEndianWriteDouble().  Use
	/// this version if the file endian-ness will always be the same as the operating
	/// system endian-ness. </param>
	/// <exception cref="IOException"> - if the file can not be found or read, if 
	/// the mode is not equal to "r" or "rw", or if the security permissions
	/// prevent read and/or write permissions. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EndianRandomAccessFile(String file, String mode, boolean match_system) throws java.io.IOException
	public EndianRandomAccessFile(string file, string mode, bool match_system) : base(file, mode)
	{
		//make byte array
		_b = new sbyte[8];
		__match_system = match_system;
		__is_big_endian = IOUtil.isBigEndianMachine();
	}

	/// <summary>
	/// Finalizes and cleans up. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~EndianRandomAccessFile()
	{
		_b = null;
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
	/// Similar to readChar except read a one-byte big-endian character. </summary>
	/// <returns> a character read from a one-byte big endian character. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final char readChar1() throws java.io.IOException
	public char readChar1()
	{
		if (read(_b, 0, 1) != 1)
		{
			throw new IOException();
		}
		return (char)(_b[0]);
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
	/// Similar to readByte() except read a little endian 1 byte (8-bit)
	/// character. </summary>
	/// <returns> a byte read from a little endian byte. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final byte readLittleEndianByte() throws java.io.IOException
	public sbyte readLittleEndianByte()
	{
		if (read(_b, 0, 1) != 1)
		{
			throw new IOException();
		}
		// Treat as a byte...
		return unchecked((sbyte)(_b[0] & 0xff));
	}

	/// <summary>
	/// Similar to readChar() except read a little endian 1 byte (8-bit)
	/// character.  The 8-bit character is converted to a 16-bit (Unicode) character,
	/// which is returned. </summary>
	/// <returns> a character read from a little endian character. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final char readLittleEndianChar1() throws java.io.IOException
	public char readLittleEndianChar1()
	{
		if (read(_b, 0, 1) != 1)
		{
			throw new IOException();
		}
		// Treat as a byte...
		return (char)(_b[0] & 0xff);
	}

	/// <summary>
	/// Read a 64-bit little endian double. </summary>
	/// <returns> Value for the 64-bit double. </returns>
	/// <exception cref="IOException"> if there is an error reading from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final double readLittleEndianDouble() throws java.io.IOException
	public double readLittleEndianDouble()
	{
		return Double.longBitsToDouble(readLittleEndianLong());
	}

	/// <summary>
	/// Read a 32-bit little endian float. </summary>
	/// <returns> Value for the 32-bit float. </returns>
	/// <exception cref="IOException"> if there is an error reading from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final float readLittleEndianFloat() throws java.io.IOException
	public float readLittleEndianFloat()
	{
		return Float.intBitsToFloat(readLittleEndianInt());
	}

	/// <summary>
	/// Read a little endian 64-bit double starting at the specified file location. </summary>
	/// <returns> Value for the 64-bit double. </returns>
	/// <param name="offset">  Number of bytes from the beginning of the file to begin 
	/// reading the double. </param>
	/// <exception cref="IOException"> if the offset value passed in is negative or there is an
	/// error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final double readLittleEndianDouble(int offset) throws java.io.IOException
	public double readLittleEndianDouble(int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);
		return Double.longBitsToDouble(readLittleEndianLong());
	}

	/// <summary>
	/// Read a little endian 32-bit integer. </summary>
	/// <returns> Value for the 32-bit integer. </returns>
	/// <exception cref="IOException"> if there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final int readLittleEndianInt() throws java.io.IOException
	public int readLittleEndianInt()
	{
		if (read(_b, 0, 4) != 4)
		{
			throw new IOException("Unexpected end of file.");
		}
		return (_b[3] & 0xff) << 24 | (_b[2] & 0xff) << 16 | (_b[1] & 0xff) << 8 | (_b[0] & 0xff);
	}

	/// <summary>
	/// Read a little endian 32-bit integer starting at the specified location. </summary>
	/// <returns> Value for the 32-bit integer. </returns>
	/// <param name="offset">  Number of bytes from the beginning of the file to begin 
	/// reading the integer. </param>
	/// <exception cref="IOException"> if the offset value passed in is negative or if
	/// there is an error reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final int readLittleEndianInt(int offset) throws java.io.IOException
	public int readLittleEndianInt(int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		if (read(_b, 0, 4) != 4)
		{
			throw new IOException("Unexpected end of file.");
		}
		return (_b[3] & 0xff) << 24 | (_b[2] & 0xff) << 16 | (_b[1] & 0xff) << 8 | (_b[0] & 0xff);
	}

	/// <summary>
	/// Read a 64-bit little endian long integer. </summary>
	/// <returns> a 64-bit long integer that is read. </returns>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final long readLittleEndianLong() throws java.io.IOException
	public long readLittleEndianLong()
	{
		if (read(_b, 0, 8) != 8)
		{
			throw new IOException("Premature end of file.");
		}
		return (long)(_b[7] & 0xff) << 56 | (long)(_b[6] & 0xff) << 48 | (long)(_b[5] & 0xff) << 40 | (long)(_b[4] & 0xff) << 32 | (long)(_b[3] & 0xff) << 24 | (long)(_b[2] & 0xff) << 16 | (long)(_b[1] & 0xff) << 8 | (long)(_b[0] & 0xff);
	}

	/// <summary>
	/// Read a 16-bit little endian short integer value. </summary>
	/// <returns> Value for the 16-bit short integer. </returns>
	/// <exception cref="IOException"> if there is a problem reading the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final short readLittleEndianShort() throws java.io.IOException
	public short readLittleEndianShort()
	{
		if (read(_b, 0, 2) != 2)
		{
			throw new IOException("Unexpected end of file.");
		}
		return (short)((_b[1] & 0xff) << 8 | (_b[0] & 0xff));
	}

	/// <summary>
	/// Read a 16-bit little endian short integer value starting at the specified file
	/// location. </summary>
	/// <returns> Value for the 16-bit short integer. </returns>
	/// <param name="offset">  Number of bytes from the beginning of the file to 
	/// begin reading the short integer. </param>
	/// <exception cref="IOException"> if the offset value passed in is negative or if
	/// there is an error reading from the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final short readLittleEndianShort(int offset) throws java.io.IOException
	public short readLittleEndianShort(int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);
		if (read(_b, 0, 2) != 2)
		{
			throw new IOException("Unexpected end of file.");
		}
		return (short)((_b[1] & 0xff) << 8 | (_b[0] & 0xff));
	}

	/// <summary>
	/// Read a string of one-byte little endian characters. </summary>
	/// <returns> a String read from a 1-byte character string. </returns>
	/// <param name="size"> Number of characters to read. </param>
	/// <exception cref="IOException"> if there is a read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public final String readLittleEndianString1(int size) throws java.io.IOException
	public string readLittleEndianString1(int size)
	{
		char[] c = new char[size];
		for (int i = 0; i < size; i++)
		{
			c[i] = readLittleEndianChar1();
		}
		string s = new string(c);
		c = null;
		return s;
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
			if (read(_b, 0, 1) != 1)
			{
				throw new IOException();
			}
			c[i] = (char)(_b[0]);
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

	/// <summary>
	/// Write a 1-byte (not Unicode) character to the file using a big-endian format.
	/// The character is written as a single byte. </summary>
	/// <param name="c"> Character to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeChar1(char c) throws java.io.IOException
	public virtual void writeChar1(char c)
	{
		_b[0] = (sbyte)c;
		write(_b, 0, 1);
	}

	/// <summary>
	/// Write a string as 1-byte (not Unicode) characters to the file using a big-endian
	/// format.
	/// Each character is written as a single byte. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeChar1(String s) throws java.io.IOException
	public virtual void writeChar1(string s)
	{
		int len = s.Length;
		for (int i = 0; i < len; i++)
		{
			_b[0] = (sbyte)s[i];
			write(_b, 0, 1);
		}
	}

	/// <summary>
	/// Write a 2-byte Unicode character to the file using an endian-ness that matches
	/// the system (match_system=true should be used in the constructor if this method
	/// is called - otherwise Java big-endian is assumed). </summary>
	/// <param name="c"> Character to write, as its Unicode value. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
		{ // Default Java or system that is big-endian...
			writeChar(c);
		}
	}

	/// <summary>
	/// Write a 1-byte (not Unicode) character to the file using an endian-ness that
	/// matches the system (match_system=true should be used in the constructor if this
	/// method is called - otherwise Java big-endian is assumed).  The character is
	/// written as a single byte. </summary>
	/// <param name="c"> Character to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
		{ // Default Java or system that is big-endian...
			_b[0] = (sbyte)c;
			write(_b, 0, 1);
		}
	}

	/// <summary>
	/// Write a string as 1-byte (not Unicode) characters to the file using an
	/// endian-ness that matches the system (match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed).
	/// Each character is written as a single byte. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
		{ // Default Java or system that is big-endian...
			int len = s.Length;
			for (int i = 0; i < len; i++)
			{
				_b[0] = (sbyte)s[i];
				write(_b, 0, 1);
			}
		}
	}

	/// <summary>
	/// Write a string as 2-byte (Unicode) characters to the file using an
	/// endian-ness that matches the system (match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed).
	/// Each character is written as two-bytes. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
		{ // Default Java or system that is big-endian...
			writeChars(s);
		}
	}

	/// <summary>
	/// Write a 64-bit double to the file using an
	/// endian-ness that matches the system (match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed). </summary>
	/// <param name="d"> 64-bit double to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
		{ // Default Java or system that is big-endian...
			writeDouble(d);
		}
	}

	/// <summary>
	/// Write a 32-bit float to the file using an
	/// endian-ness that matches the system (match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed). </summary>
	/// <param name="f"> 32-bit float to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
		{ // Default Java or system that is big-endian...
			writeFloat(f);
		}
	}

	/// <summary>
	/// Write a 32-bit integer to the file using an
	/// endian-ness that matches the system (match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed). </summary>
	/// <param name="i"> 32-bit integer to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
		{ // Default Java or system that is big-endian...
			writeInt(i);
		}
	}

	/// <summary>
	/// Write a 16-bit integer to the file using an
	/// endian-ness that matches the system (match_system=true should be used in the
	/// constructor if this method is called - otherwise Java big-endian is assumed). </summary>
	/// <param name="i"> 16-bit integer to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeEndianShort(short i) throws java.io.IOException
	public virtual void writeEndianShort(short i)
	{
		if (__match_system && !__is_big_endian)
		{
			// Little endian system...
			writeLittleEndianShort(i);
		}
		else
		{ // Default Java or system that is big-endian...
			writeShort(i);
		}
	}

	/// <summary>
	/// Write a little endian char (2-byte Unicode), specifying the character as its
	/// Unicode integer equivalent. </summary>
	/// <param name="c"> Int to write. </param>
	/// <exception cref="IOException"> if there is an error writing the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar(int c) throws java.io.IOException
	public virtual void writeLittleEndianChar(int c)
	{ //same as for shorts...

		_b[0] = (sbyte)c;
		_b[1] = (sbyte)(c >> 8);

		write(_b, 0, 2);
	}

	/// <summary>
	/// Write a little endian char (2-byte Unicode) at the specified file location. </summary>
	/// <param name="i"> Character to write, as Unicode integer equivalent. </param>
	/// <param name="offset"> Byte number to begin writing. </param>
	/// <exception cref="IOException"> - if the offset passed in is negative or if 
	/// there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar(int i, int offset) throws java.io.IOException
	public virtual void writeLittleEndianChar(int i, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		//same as for shorts...

		_b[0] = (sbyte)i;
		_b[1] = (sbyte)(i >> 8);

		write(_b, 0, 2);
	}

	/// <summary>
	/// Write a little endian 8-bit char (not Unicode). </summary>
	/// <param name="c"> Char to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar1(char c) throws java.io.IOException
	public virtual void writeLittleEndianChar1(char c)
	{
		_b[0] = (sbyte)c;
		write(_b, 0, 1);
	}

	/// <summary>
	/// Write a little endian 8-bit char (not Unicode) at the byte offset indicated. </summary>
	/// <param name="c"> Char to write. </param>
	/// <param name="offset"> Byte number to start writing. </param>
	/// <exception cref="IOException"> if the offset passed in is negative or there is an error
	/// writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar1(char c, int offset) throws java.io.IOException
	public virtual void writeLittleEndianChar1(char c, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		_b[0] = (sbyte)c;
		write(_b, 0, 1);
	}

	/// <summary>
	/// Write a little endian 8-bit byte. </summary>
	/// <param name="b"> byte to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianByte(byte b) throws java.io.IOException
	public virtual void writeLittleEndianByte(sbyte b)
	{
		_b[0] = (sbyte)b;
		write(_b, 0, 1);
	}

	/// <summary>
	/// Write a little endian 8-bit byte at the byte offset indicated. </summary>
	/// <param name="b"> byte to write. </param>
	/// <param name="offset"> byte number to start writing. </param>
	/// <exception cref="IOException"> if the offset passed in is negative or there is an error
	/// writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianByte(byte b, int offset) throws java.io.IOException
	public virtual void writeLittleEndianByte(sbyte b, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		_b[0] = (sbyte)b;
		write(_b, 0, 1);
	}

	/// <summary>
	/// Write a String as little endian 8-bit chars (not Unicode). </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar1(String s) throws java.io.IOException
	public virtual void writeLittleEndianChar1(string s)
	{
		int size = s.Length;

		if (size > _b.Length)
		{
			// Reallocate the array...
			_b = new sbyte[size];
			for (int i = 0; i < size; i++)
			{
				_b[i] = (sbyte)s[i];
			}

			write(_b, 0, size);
		}
		else
		{
			for (int i = 0; i < size; i++)
			{
				_b[i] = (sbyte)s[i];
			}

			write(_b, 0, size);
		}
	}

	/// <summary>
	/// Write a String as little endian 8-bit chars (not Unicode) at the specified file
	/// location. </summary>
	/// <param name="s"> String to write. </param>
	/// <param name="offset"> Byte number to begin writing. </param>
	/// <exception cref="IOException"> if offset is negative or if there is an error writing to
	/// the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChar1(String s, int offset) throws java.io.IOException
	public virtual void writeLittleEndianChar1(string s, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		int size = s.Length;

		if (size > _b.Length)
		{
			// Reallocate the array...
			_b = new sbyte[size];
			for (int i = 0; i < size; i++)
			{
				_b[i] = (sbyte)s[i];
			}

			write(_b, 0, size);
		}
		else
		{
			for (int i = 0; i < size; i++)
			{
				_b[i] = (sbyte)s[i];
			}

			write(_b, 0, size);
		}
	}

	/// <summary>
	/// Write a String as little endian (2-byte Unicode) chars. </summary>
	/// <param name="s"> String to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
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
	/// Writes a String as little endian (2-byte Unicode) chars at the specified file
	/// location. </summary>
	/// <param name="s"> String to write. </param>
	/// <param name="offset"> Byte number to begin writing. </param>
	/// <exception cref="IOException"> if offset is negative or if there is an error writing to
	/// the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianChars(String s, int offset) throws java.io.IOException
	public virtual void writeLittleEndianChars(string s, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

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
	{ //convert to floating point long
		long vl = System.BitConverter.DoubleToInt64Bits(d);

		_b[0] = (sbyte)vl;
		_b[1] = (sbyte)(vl >> 8);
		_b[2] = (sbyte)(vl >> 16);
		_b[3] = (sbyte)(vl >> 24);
		_b[4] = (sbyte)(vl >> 32);
		_b[5] = (sbyte)(vl >> 40);
		_b[6] = (sbyte)(vl >> 48);
		_b[7] = (sbyte)(vl >> 56);

		write(_b, 0, 8);
	}

	/// <summary>
	/// Write a little endian 64-bit double at the specified file location. </summary>
	/// <param name="d"> Double to write. </param>
	/// <param name="offset"> Byte number to begin writing. </param>
	/// <exception cref="IOException"> if offset is negative or if there is an error writing to
	/// the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianDouble(double d, int offset) throws java.io.IOException
	public virtual void writeLittleEndianDouble(double d, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		//convert to floating point long
		long vl = System.BitConverter.DoubleToInt64Bits(d);

		_b[0] = (sbyte)vl;
		_b[1] = (sbyte)(vl >> 8);
		_b[2] = (sbyte)(vl >> 16);
		_b[3] = (sbyte)(vl >> 24);
		_b[4] = (sbyte)(vl >> 32);
		_b[5] = (sbyte)(vl >> 40);
		_b[6] = (sbyte)(vl >> 48);
		_b[7] = (sbyte)(vl >> 56);

		write(_b, 0, 8);
	}

	/// <summary>
	/// Write a little endian 32-bit float. </summary>
	/// <param name="f"> 32-bit float to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianFloat(float f) throws java.io.IOException
	public virtual void writeLittleEndianFloat(float f)
	{
		writeLittleEndianInt(Float.floatToIntBits(f));
	}

	/// <summary>
	/// Write a 32-bit little endian float at the specified file location. </summary>
	/// <param name="f"> 32-bit float to write. </param>
	/// <param name="offset"> Byte number to begin writing. </param>
	/// <exception cref="IOException"> if offset is negative or if there is an error writing to
	/// the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianFloat(float f, int offset) throws java.io.IOException
	public virtual void writeLittleEndianFloat(float f, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		writeLittleEndianInt(Float.floatToIntBits(f));
	}

	/// <summary>
	/// Write a 32-bit little endian int. </summary>
	/// <param name="i"> 32-bit integer to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianInt(int i) throws java.io.IOException
	public virtual void writeLittleEndianInt(int i)
	{
		_b[0] = (sbyte)i;
		_b[1] = (sbyte)(i >> 8);
		_b[2] = (sbyte)(i >> 16);
		_b[3] = (sbyte)(i >> 24);

		write(_b, 0, 4);

	}

	/// <summary>
	/// Write a 32-bit little endian int at the specified file location. </summary>
	/// <param name="i"> 32-bit integer to write. </param>
	/// <param name="offset"> Byte location to start writing int. </param>
	/// <exception cref="IOException"> if the offset is negative or if there is an error writing
	/// to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianInt(int i, int offset) throws java.io.IOException
	public virtual void writeLittleEndianInt(int i, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		_b[0] = (sbyte)i;
		_b[1] = (sbyte)(i >> 8);
		_b[2] = (sbyte)(i >> 16);
		_b[3] = (sbyte)(i >> 24);

		write(_b, 0, 4);

	}

	/// <summary>
	/// Write a 16-bit little endian short integer. </summary>
	/// <param name="s"> 16-bit short integer value to write. </param>
	/// <exception cref="IOException"> if there is an error writing to the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianShort(short s) throws java.io.IOException
	public virtual void writeLittleEndianShort(short s)
	{
		_b[0] = (sbyte)s;
		_b[1] = (sbyte)(s >> 8);

		write(_b, 0, 2);
	}

	/// <summary>
	/// Write a 16-bit little endian short at the specified file location. </summary>
	/// <param name="s"> 16-bit short inteter value to write. </param>
	/// <param name="offset"> Byte number to begin writing. </param>
	/// <exception cref="IOException"> if offset is negative or if there is an error writing to
	/// the file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeLittleEndianShort(short s, int offset) throws java.io.IOException
	public virtual void writeLittleEndianShort(short s, int offset)
	{
		if (offset < 0)
		{
			throw new IOException("Offset value must be greater " + "than or equal to zero.");
		}

		seek(offset);

		_b[0] = (sbyte)s;
		_b[1] = (sbyte)(s >> 8);

		write(_b, 0, 2);
	}

	} //end EndianRandomAccessFile

}