﻿using System;

// LicenseManager -- manages reading license files that have encrypted strings in them.

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
// LicenseManager.java -- manages reading license files that have encrypted
//	strings in them.
// ----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
// ----------------------------------------------------------------------------
// History:
//
// 2002-08-26	J. Thomas Sapienza, RTi	Initial version.
// 2002-08-07	JTS, RTi		Added javadoc, suport for writing 
//					persistent files.
// 2002-11-11	JTS, RTi		Revised some commenting.
// 2003-01-09	JTS, RTi		The class was almost completely gutted
//					and restarted.
// 2003-01-11	JTS, RTi		Revisions to accomodate SAM's
//					LicenseManager design.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeUtil = RTi.Util.Time.TimeUtil;

	/// <summary>
	/// Class to handle encryption, decryption, checking and validation of RTi licenses.
	/// 
	/// The LicenseManager provides a useful separation between applications and the
	/// encryption/decryption routines for two reasons:<ol>
	/// <li>it makes the application programmer's job easier.</li>
	/// <li>it makes the encryption code more modular -- possibly for removal to native
	/// methods or RMI later.</li></ol>
	/// 
	/// (See the constructors for parameter definitions in regard to the next section)
	/// LicenseManager is most-commonly used in one of two ways:<ol>
	/// <li>To take an application's license information and a license key and see if
	///    the license key is the proper key for the given application information.
	///    This can be done by comparing the license key with a generated license
	///    key, and by checking that the expiration date for a license key has not
	///    been passed.
	/// <pre>
	///    LicenseManager lm = new LicenseManager(
	///        product, licenseOwner, licenseType, licenseCount, licenseExpires,
	/// licenseKey);
	///    if (lm.isLicenseValid()) {
	///        // checks whether the application information encrypts to the same
	/// // value as the licenseKey
	///    }
	///    if (!lm.isLicenseExpired()) {
	///        // checks whether the license has expired yet or not
	///    }
	/// </pre>
	/// </li>
	/// 
	/// <li>To take an application's information and return an encrypted license key.
	///    This is used in the RTiCipher application to generate license keys for
	///    clients.
	/// <pre>
	///    LicenseManager lm = new LicenseManager(
	///        product, licenseOwner, licenseType, licenseCount, licenseExpires);
	///    lm.encrypt();
	///    String licenseKey = lm.getLicenseKey();
	/// </pre>
	/// </li>
	/// 
	/// <b>Miscellaneous Notes:</b>
	/// With the Blowfish cipher, the length of the encrypted strings is not directly
	/// related to the length of the string from which it was encrypted.
	/// 
	/// The exact length of the string generated by Blowfish can be calculated by:
	/// <ul>
	/// <li>take the length of the original string and pad it out with spaces until
	///    it is 8, 16, 24 ... etc characters long.
	///    (thus, the string "Hello" will be padded to "Hello   ", and the string
	///    "Colorado State will be padded to "Colorado State  ".  The string 
	///    "Colorado" will remain the same)</li>
	/// <li>multiply length of the new and padded string by 2</li>
	/// <li>add 2 to this number.  This is the cipher string used by RTi to determine
	///    the cipher package and encryption seeds.  RTi's licenses have this number
	///    only at the very beginning of the whole license, so this step can be
	///    disregarded for those.</li>
	/// </ul>
	/// 
	/// So the encrypted String's length can be fairly-closely configured through 
	/// trimming the input string to a desired length.
	/// </summary>

	public class LicenseManager
	{

	/// <summary>
	/// The count of licensed products.
	/// </summary>
	private string __licenseCount;

	/// <summary>
	/// When the license expires.  Format is YYYYMMDD.
	/// </summary>
	private string __licenseExpires;

	/// <summary>
	/// The encrypted license key.
	/// </summary>
	private string __licenseKey;

	/// <summary>
	/// The owner of the licensed product.
	/// </summary>
	private string __licenseOwner;

	/// <summary>
	/// The type of license, which indicates how the software is licensed.
	/// </summary>
	private string __licenseType;

	/// <summary>
	/// The product that is being licensed with a given license key.
	/// </summary>
	private string __product;

	/// <summary>
	/// Constructor for a LicenseManager that will encrypt the provided information
	/// into a license key, as used by a license key generator. </summary>
	/// <param name="product"> the product being licensed. </param>
	/// <param name="licenseOwner"> the owner of the licensed product. </param>
	/// <param name="licenseType"> the type of license. </param>
	/// <param name="licenseCount"> the license count. </param>
	/// <param name="licenseExpires"> when the license expires, YYYYMMDD. </param>
	public LicenseManager(string product, string licenseOwner, string licenseType, string licenseCount, string licenseExpires)
	{
		initialize(product, licenseOwner, licenseType, licenseCount, licenseExpires, null);
	}

	/// <summary>
	/// Constructor for a LicenseManager that can compare the provided encryption
	/// key to the provided information and see if they match. </summary>
	/// <param name="product"> the product being licensed. </param>
	/// <param name="licenseOwner"> the owner of the licensed product. </param>
	/// <param name="licenseType"> the type of license. </param>
	/// <param name="licenseCount"> the license count. </param>
	/// <param name="licenseExpires"> when the license expires, YYYYMMDD. </param>
	/// <param name="licenseKey"> the encrypted license key. </param>
	public LicenseManager(string product, string licenseOwner, string licenseType, string licenseCount, string licenseExpires, string licenseKey)
	{
		initialize(product, licenseOwner, licenseType, licenseCount, licenseExpires, licenseKey);
	}

	/// <summary>
	/// Encrypts the license info (product, type, count, owner, expires) into a 
	/// license key, stores the value in the object, and returns the value.
	/// <para>
	/// This version of the encrypt() method is used for the encryption of the license key.
	/// </para>
	/// </summary>
	/// <param name="prefix"> the two-character prefix that specifies the encryption type. </param>
	/// <returns> the encrypted license key. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String encrypt(String prefix) throws Exception
	public virtual string encrypt(string prefix)
	{
		return encrypt(prefix, true);
	}

	/// <summary>
	/// Encrypts the license info (product, type, count, owner, expires) into a 
	/// license key.  If the overwriteLicenseKey parameter is set to true, then
	/// the generated key is also stored in the object.  The generated license key
	/// is also returned from this procedure.  This procedure is called with a 
	/// parameter of 'false' when the 'isLicenseValid' method is used.
	/// <para>
	/// This version of the encrypt() method is used more for the validation of 
	/// license keys.
	/// </para>
	/// </summary>
	/// <param name="prefix"> the two-character prefix that specifies the encryption type. </param>
	/// <param name="overwriteLicenseKey"> whether to save the generated license key value in the object. </param>
	/// <returns> the encrypted license key. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String encrypt(String prefix, boolean overwriteLicenseKey) throws Exception
	public virtual string encrypt(string prefix, bool overwriteLicenseKey)
	{
		string product = "";
		if (__product.regionMatches(false, 0, "River", 0, 5))
		{
			// This takes the two words in the RiverTrak product line
			// names (RiverTrak/Assistant, RiverTrak/Viewer, etc) and
			// takes the first letter of each name and concatenates them.
			// This is done to shorten the length of the encrypted string.
			product = "R" + __product.Substring(9, 1);
		}
		else if (__product.regionMatches(false, 0, "NWSRFS", 0, 6))
		{
			product = "NG";
		}
		else
		{
			product = "TSTool";
		}

		string license = "";

		Cipher c = new Cipher(prefix);

		product = c.encrypt(product);
		 string licenseType = c.encrypt(__licenseType);
		string licenseCount = c.encrypt(__licenseCount);
		string licenseOwner = c.encrypt(__licenseOwner);
		string licenseExpires = c.encrypt(__licenseExpires);

		license = prefix + "-"
			+ product.Substring(2, product.Length - 2) + "-"
			+ licenseType.Substring(2, licenseType.Length - 2) + "-"
			+ licenseCount.Substring(2, licenseCount.Length - 2) + "-"
			+ licenseExpires.Substring(2, licenseExpires.Length - 2) + "-"
			+ licenseOwner.Substring(2, licenseOwner.Length - 2);

		if (overwriteLicenseKey)
		{
			__licenseKey = license;
		}
		return license;
	}

	/// <summary>
	/// Clean up data members.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~LicenseManager()
	{
		__product = null;
		__licenseOwner = null;
		__licenseType = null;
		__licenseCount = null;
		__licenseExpires = null;
		__licenseKey = null;

//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Get the number of days until the license expires.  A license has expired if the date
	/// specified in the LicenseExpires property is later than the current date.
	/// Note that this is a secondary check.  'isLicenseValid()' should always be
	/// called first to make sure that the license itself is valid before checking
	/// for an expired license. </summary>
	/// <returns> The number of days until the license expires.  If license expires date is today,
	/// the license won't technically expire until tomorrow and zero is returned.  If the license
	/// never expires, return 10000.  If the license is expired, a negative number will be
	/// returned. </returns>
	public virtual int getDaysToExpiration()
	{
		// Check special cases.
		if (__licenseExpires.Equals("Never", StringComparison.OrdinalIgnoreCase))
		{
			return 10000;
		}

		// Get the current date.
		DateTime now = new DateTime(DateTime.PRECISION_DAY | DateTime.DATE_CURRENT);
		// Need a new DateTime format to parse YYYYMMDD!...
		if (__licenseExpires.Length != 8)
		{
			return -1;
		}

		string expiresString = __licenseExpires.Substring(0, 4) + "-"
			+ __licenseExpires.Substring(4, 2) + "-"
			+ __licenseExpires.Substring(6, 2);
		Message.printStatus(1, "", "Expires string=\"" + expiresString + "\"");
		DateTime expires = null;
		try
		{
			expires = DateTime.parse(expiresString);
		}
		catch (Exception)
		{
			// Assume bad date format - expired ...
			return -1;
		}
		Message.printStatus(2, "", "Now: " + now + ", license expires: " + expires);
		int now_days = TimeUtil.absoluteDay(now.getYear(), now.getMonth(), now.getDay());
		int expires_days = TimeUtil.absoluteDay(expires.getYear(), expires.getMonth(), expires.getDay());
		return expires_days - now_days;
	}

	/// <summary>
	/// Returns the license count. </summary>
	/// <returns> the license count. </returns>
	public virtual string getLicenseCount()
	{
		return __licenseCount;
	}

	/// <summary>
	/// Returns when the license expires. </summary>
	/// <returns> when the license expires. </returns>
	public virtual string getLicenseExpires()
	{
		return __licenseExpires;
	}

	/// <summary>
	/// Returns the encrypted license key. </summary>
	/// <returns> the encrypted license key. </returns>
	public virtual string getLicenseKey()
	{
		return __licenseKey;
	}

	/// <summary>
	/// Returns the license owner. </summary>
	/// <returns> the license owner. </returns>
	public virtual string getLicenseOwner()
	{
		return __licenseOwner;
	}

	/// <summary>
	/// Returns the license type. </summary>
	/// <returns> the license type. </returns>
	public virtual string getLicenseType()
	{
		return __licenseType;
	}

	/// <summary>
	/// Returns the product. </summary>
	/// <returns> the product. </returns>
	public virtual string getProduct()
	{
		return __product;
	}

	/// <summary>
	/// Initializes the data members of the LicenseManager.  Any of the String values
	/// that are <b>null</b> will be set to an empty string (""). </summary>
	/// <param name="product"> the product being licensed. </param>
	/// <param name="licenseOwner"> the owner of the licensed product. </param>
	/// <param name="licenseType"> the type of license. </param>
	/// <param name="licenseCount"> the license count. </param>
	/// <param name="licenseExpires"> when the license expires. </param>
	/// <param name="licenseKey"> the encrypted license key. </param>
	private void initialize(string product, string licenseOwner, string licenseType, string licenseCount, string licenseExpires, string licenseKey)
	{

		if (string.ReferenceEquals(product, null))
		{
			__product = "";
		}
		else
		{
			__product = product;
		}

		if (string.ReferenceEquals(licenseOwner, null))
		{
			__licenseOwner = "";
		}
		else
		{
			__licenseOwner = licenseOwner;
		}

		if (string.ReferenceEquals(licenseType, null))
		{
			__licenseType = "";
		}
		else
		{
			__licenseType = licenseType;
		}

		if (string.ReferenceEquals(licenseCount, null))
		{
			__licenseCount = "";
		}
		else
		{
			__licenseCount = licenseCount;
		}

		if (string.ReferenceEquals(licenseExpires, null))
		{
			__licenseExpires = "";
		}
		else
		{
			__licenseExpires = licenseExpires;
		}

		if (string.ReferenceEquals(licenseKey, null))
		{
			__licenseKey = "";
		}
		else
		{
			__licenseKey = licenseKey;
		}
	}

	/// <summary>
	/// Indicate whether the license is a demo license.  This simply checks whether the
	/// string "Demo" is in the license type. </summary>
	/// <returns> true if the license type is a demo license. </returns>
	public virtual bool isLicenseDemo()
	{
		if (StringUtil.indexOfIgnoreCase(getLicenseType(),"Demo",0) >= 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determine whether a license is expired.  A license has expired if the date
	/// specified in the LicenseExpires property is later than the current date.
	/// Note that this is a secondary check.  'isLicenseValid()' should always be
	/// called first to make sure that the license itself is valid before checking
	/// for an expired license. </summary>
	/// <returns> true if the licenseExpires date in the license data is after the 
	/// current date.  Returns false if the licenseExpires date is less-than or 
	/// equal-to the current date.  A value of "Never" in licenseExpires will 
	/// result in 'false' being returned. </returns>
	public virtual bool isLicenseExpired()
	{
		// Check special cases.
		if (__licenseExpires.Equals("Never", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		// Get the current date.
		DateTime now = new DateTime(DateTime.PRECISION_DAY | DateTime.DATE_CURRENT);
		// Need a new DateTime format to parse YYYYMMDD!...
		if (__licenseExpires.Length != 8)
		{
			return true;
		}

		string expiresString = __licenseExpires.Substring(0, 4) + "-"
			+ __licenseExpires.Substring(4, 2) + "-"
			+ __licenseExpires.Substring(6, 2);
		Message.printStatus(1, "", "Expires string=\"" + expiresString + "\"");
		DateTime expires = null;
		try
		{
			expires = DateTime.parse(expiresString);
		}
		catch (Exception)
		{
			// Assume bad date format - expired ...
			return true;
		}
		Message.printStatus(2, "", "Now: " + now + ", license expires: " + expires);
		if (now.greaterThan(expires))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks the validity of the license information.  It does this by encrypting
	/// the product, type, count, owner, and expires date into a license key and then
	/// comparing the result to the license key passed in at object creation.  If the
	/// two are the same, then the license is correct.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isLicenseValid() throws Exception
	public virtual bool isLicenseValid()
	{
		if (encrypt(__licenseKey.Substring(0, 2), false).Equals(__licenseKey))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Returns a string with important info from the LicenseManager object.  
	/// For debugging purposes only.
	/// </summary>
	/// <returns> a string with important info from the LicenseManager object. </returns>
	public override string ToString()
	{
		return ""
			+ "Product:         " + __product + "\n"
			+ "License Owner:   " + __licenseOwner + "\n"
			+ "License Type:    " + __licenseType + "\n"
			+ "License Count:   " + __licenseCount + "\n"
			+ "License Expires: " + __licenseExpires + "\n"
			+ "License Key:     " + __licenseKey + "\n";
	}

	}

}