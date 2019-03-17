// RTiCryptoData - class that holds information about the different cipher packages, passwords and seeds used.

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
// RTiCryptoData.java - Class that holds information about the different
//			cipher packages, passwords and seeds used.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-01-09	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{
	/// <summary>
	/// The RTiCryptoData class stores the encryption and decryption data 
	/// (passwords and seeds) for a given RTi cipher (as determined by a 
	/// two-character prefix on encrypted data).
	/// </summary>
	public class RTiCryptoData
	{

	/// <summary>
	/// Returns a Blowfish CBCIV code for a given prefix.  CBCIV codes are used
	/// to seed the Blowfish encryption/decryption cipher. </summary>
	/// <param name="prefix"> the prefix for which to return the CBCIV code. </param>
	/// <returns> a CBCIV string for the prefix. </returns>
	public static long getBlowfishCBCIV(string prefix)
	{
		if (prefix.Equals("00"))
		{
			return unchecked((long)0xadeadcafeBeefcabL);
		}
		return 0x0000000000000000L;
	}

	/// <summary>
	/// Returns a Blowfish password for the given prefix. </summary>
	/// <param name="prefix"> the prefix for which to return the password. </param>
	/// <returns> a password for the given prefix. </returns>
	public static sbyte[] getBlowfishPassword(string prefix)
	{
		string pw = "";
		if (prefix.Equals("00"))
		{
			pw = "default RTi first version password";
		}

		int len = pw.Length;
		sbyte[] bpw = new sbyte[len];

		for (int i = 0; i < len; i++)
		{
			bpw[i] = (sbyte)pw[i];
		}

		return bpw;
	}

	/// <summary>
	/// Returns the type of Cipher package being used based on a two-character
	/// prefix. </summary>
	/// <param name="prefix"> the two-character prefix to specify the package. </param>
	/// <returns> the cipher package that is associated with the two-character prefix,
	/// or -1 if the package was unknown.   </returns>
	public static int lookupCipherPackage(string prefix)
	{
		if (prefix.Equals("00"))
		{
			return Cipher.BLOWFISH;
		}
		return -1;
	}

	}

}