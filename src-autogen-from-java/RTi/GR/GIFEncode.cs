using System;
using System.IO;

// Code to handle GIF files

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

// History:
//
// 2007-05-08  Steven A. Malers, RTi  Clean up based on Eclipse feedback.

namespace RTi.GR
{

	using Message = RTi.Util.Message.Message;

	internal class Packetizer
	{
		internal Stream os = null;
		internal sbyte[] packet = new sbyte[256];
		internal int count = 0;
		internal Packetizer(Stream os)
		{
		this.os = os;
		count = 0;
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void flush() throws java.io.IOException
		internal virtual void flush()
		{
		if (count > 0)
		{
			Console.WriteLine("Packetizer flushing " + count + " bytes");
			os.WriteByte(count);
			os.Write(packet, 0, count);
			int i;
			for (i = 0; i < count; i++)
			{
			Console.WriteLine("Byte " + i + " is " + (packet[i] & 0xff));
			}
			count = 0;
		}
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void write(int c) throws java.io.IOException
		internal virtual void write(int c)
		{
		packet[count++] = (sbyte)c;
		if (count == 255)
		{
			flush();
		}
		}
	}

	internal class BitStream
	{
		internal Packetizer packetizer;
		internal BitStream(Stream os)
		{
		packetizer = new Packetizer(os);
		}
		internal int currentBits = 0;
		internal int accumulator = 0;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void write(int code, int nBits) throws java.io.IOException
		internal virtual void write(int code, int nBits)
		{
		Console.WriteLine("Bitstream writing " + code + " as " + nBits + " bits");
		if (currentBits > 0)
		{
			accumulator &= (1 << currentBits) - 1;
			accumulator |= code << currentBits;
		}
		else
		{
			accumulator = code;
		}
		currentBits += nBits;
		flush(8);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void flush(int bitsLeft) throws java.io.IOException
		internal virtual void flush(int bitsLeft)
		{
		while (currentBits > bitsLeft)
		{
			packetizer.write(accumulator & 0xff);
			accumulator >>= 8;
			currentBits -= 8;
		}
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void flush() throws java.io.IOException
		internal virtual void flush()
		{
		flush(0);
		packetizer.flush();
		}
	}

	internal class Compressor
	{
		internal Stream os;
		internal BitStream bitStream;
		internal int initBits;
		internal int clearCode;
		internal int eofCode;
		internal int nBits;
		internal int maxCode;
		internal int freeEntry;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Compressor(int bitsPerCode, java.io.OutputStream os) throws java.io.IOException
		internal Compressor(int bitsPerCode, Stream os)
		{
		int initCodeSize = (bitsPerCode <= 1 ? 2 : bitsPerCode);
		initBits = initCodeSize + 1;
		clearCode = (1 << (initBits - 1));
		eofCode = clearCode + 1;
		this.os = os;
		// Write the initial code size
		os.WriteByte(initCodeSize);
		bitStream = new BitStream(os);
		nBits = initBits;
		clearHashTable();
		}
		/// <summary>
		/// Clear out the hash table
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void clearHashTable() throws java.io.IOException
		internal virtual void clearHashTable()
		{
		// Output the clear code
		bitStream.write(clearCode, nBits);
		freeEntry = eofCode + 1;
		nBits = initBits;
		maxCode = (1 << nBits) - 1;
		int i;
		for (i = 0; i < HASH_TABLE_SIZE; i++)
		{
			hashTable[i] = -1;
		}
		}
		/// <summary>
		/// Hash table to look up prefix/suffix pairs
		/// This table contains the search keys
		/// </summary>
		internal int[] hashTable = new int[HASH_TABLE_SIZE];
		/// <summary>
		/// Code mappings for hash table entries
		/// i.e. search key -> new code value
		/// </summary>
		internal int[] codeTable = new int[HASH_TABLE_SIZE];
		/// <summary>
		/// Size of the hash table
		/// This gives an occupancy of 4096/5003 (over 80%)
		/// since we can have at most 4096 (2^MAX_BITS) codes to look up
		/// </summary>
		internal const int HASH_TABLE_SIZE = 5003;
		/// <summary>
		/// Maximum bits allowed for encoding
		/// </summary>
		internal const int MAX_BITS = 12;
		/// <summary>
		/// Maximum code for maximum bits; never generated
		/// </summary>
		internal static readonly int MAX_MAX_CODE = 1 << MAX_BITS;
		/// <summary>
		/// Shift amount for generating primary hash code
		/// </summary>
		internal const int HASH_SHIFT = 4;
		/// <summary>
		/// Compress a data stream
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void noCompress(byte[] data, int length) throws java.io.IOException
		internal virtual void noCompress(sbyte[] data, int length)
		{
		int byteNo;
		for (byteNo = 1; byteNo < length; byteNo++)
		{
			int c = data[byteNo] & 0xff;
			bitStream.write(c, nBits);
		}
		// Write the end of the compressed stream
		bitStream.write(eofCode, nBits);
		bitStream.flush();
		// Write the block terminator
		os.WriteByte(0);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void compress(byte[] data, int length) throws java.io.IOException
		internal virtual void compress(sbyte[] data, int length)
		{
		int prefixCode = data[0] & 0xff;
		int byteNo;
		for (byteNo = 1; byteNo < length; byteNo++)
		{
			int suffixCode = data[byteNo] & 0xff;
			// look up the prefix followed by suffix in the hash table
			int searchKey = (suffixCode << MAX_BITS) + prefixCode;
			int hash = (suffixCode << HASH_SHIFT) ^ prefixCode;
			if (hashTable[hash] != searchKey && hashTable[hash] >= 0)
			{
			// not found on primary hash; try secondary hash
			int displacement;
			if (hash == 0)
			{
				displacement = 1;
			}
			else
			{
				displacement = HASH_TABLE_SIZE - hash;
			}
			do
			{
				hash -= displacement;
				if (hash < 0)
				{
				hash += HASH_TABLE_SIZE;
				}
			} while (hashTable[hash] != searchKey && hashTable[hash] >= 0);
			}
			if (hashTable[hash] == searchKey)
			{
			// we have a code for prefix followed by suffix
			prefixCode = codeTable[hash];
			continue;
			}
			else
			{
			// empty slot
			bitStream.write(prefixCode, nBits);
			// If the next entry is going to be too big for the code size,
			// then increase it, if possible.
			if (freeEntry > maxCode)
			{
				nBits++;
				if (nBits == MAX_BITS)
				{
				maxCode = MAX_MAX_CODE;
				}
				else
				{
				maxCode = (1 << nBits) - 1;
				}
			}
			prefixCode = suffixCode;

			if (freeEntry < MAX_MAX_CODE)
			{
				// put this entry in the hash table
				hashTable[hash] = searchKey;
				codeTable[hash] = freeEntry++;
			}
			else
			{
				// Clear out the hash table
				clearHashTable();
			}
			}
		}
		// Write the final code
		bitStream.write(prefixCode, nBits);
		// If the next entry is going to be too big for the code size,
		// then increase it, if possible.
		if (freeEntry > maxCode)
		{
			nBits++;
			if (nBits == MAX_BITS)
			{
			maxCode = MAX_MAX_CODE;
			}
			else
			{
			maxCode = (1 << nBits) - 1;
			}
		}
		// Write the end of the compressed stream
		bitStream.write(eofCode, nBits);
		bitStream.flush();
		// Write the block terminator
		os.WriteByte(0);
		}
	}

	public class GIFEncode
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void putWord(java.io.OutputStream os, int v) throws java.io.IOException
		internal static void putWord(Stream os, int v)
		{
		putByte(os, v);
		putByte(os, v >> 8);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void putByte(java.io.OutputStream os, int v) throws java.io.IOException
		internal static void putByte(Stream os, int v)
		{
		os.WriteByte(v & 0xff);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void writeImageData(int bitsPerPixel, int width, int height, byte[] pixels, java.io.OutputStream os) throws java.io.IOException
		internal static void writeImageData(int bitsPerPixel, int width, int height, sbyte[] pixels, Stream os)
		{
		Console.WriteLine("Writing image data " + bitsPerPixel + " " + width + " " + height);
		// Compress the data
		Compressor compressor = new Compressor(bitsPerPixel, os);
		compressor.compress(pixels, width * height);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void encode(int width, int height, int numberColors, byte[] red, byte[] green, byte[] blue, byte[] pixels, java.io.OutputStream os) throws java.io.IOException
		internal static void encode(int width, int height, int numberColors, sbyte[] red, sbyte[] green, sbyte[] blue, sbyte[] pixels, Stream os)
		{

		// Determine the number of bits per pixel
		int bitsPerPixel;
		for (bitsPerPixel = 1; bitsPerPixel < 8; bitsPerPixel++)
		{
			if ((1 << bitsPerPixel) >= numberColors)
			{
			break;
			}
		}
		Message.printStatus(1, "encode", "bitsPerPixel = " + bitsPerPixel);
		int colorMapSize = 1 << bitsPerPixel;
		Message.printStatus(1, "encode", "colorMapSize = " + colorMapSize);

		// Write the Header
		string signature = "GIF";
		string version = "87a";
		putByte(os, signature[0]);
		putByte(os, signature[1]);
		putByte(os, signature[2]);
		putByte(os, version[0]);
		putByte(os, version[1]);
		putByte(os, version[2]);

		// Write the Logical Screen Descriptor
		bool globalColorTable = true;
		int colorResolution = 8;
		bool globalSortFlag = false;
		int backgroundColorIndex = 0;
		int pixelAspectRatio = 0;
		putWord(os, width);
		putWord(os, height);
		int flags = 0;
		flags |= (globalColorTable ? 1 : 0) << 7;
		flags |= (colorResolution - 1) << 4;
		flags |= (globalSortFlag ? 1 : 0) << 3;
		flags |= (bitsPerPixel - 1);
		putByte(os, flags);
		putByte(os, backgroundColorIndex);
		putByte(os, pixelAspectRatio);

		// Write the global color table
		int i;
		for (i = 0; i < colorMapSize; i++)
		{
			sbyte r, g, b;
			if (i < numberColors)
			{
			r = red[i];
			g = green[i];
			b = blue[i];
			}
			else
			{
			r = 0;
			g = 0;
			b = 0;
			}
			putByte(os, r);
			putByte(os, g);
			putByte(os, b);
		}

		// Write the image descriptor
		sbyte imageSeparator = 0x2C;
		int imageLeftPosition = 0;
		int imageTopPosition = 0;
		int imageWidth = width;
		int imageHeight = height;
		bool localColorTableFlag = false;
		bool interlaceFlag = false;
		bool localSortFlag = false;
		int localColorTableSize = 0;
		putByte(os, imageSeparator);
		putWord(os, imageLeftPosition);
		putWord(os, imageTopPosition);
		putWord(os, imageWidth);
		putWord(os, imageHeight);
		int imageFlags = 0;
		imageFlags |= (localColorTableFlag ? 1 : 0) << 7;
		imageFlags |= (interlaceFlag ? 1 : 0) << 6;
		imageFlags |= (localSortFlag ? 1 : 0) << 5;
		imageFlags |= localColorTableSize;
		putByte(os, imageFlags);

		// Write the image data
		writeImageData(bitsPerPixel, width, height, pixels, os);

		// Write Trailer
		sbyte trailer = 0x3B;
		putByte(os, trailer);
		}
	}

}