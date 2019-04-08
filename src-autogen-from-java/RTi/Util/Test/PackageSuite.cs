using System;

// PackageSuite - a suite builder class that can be used to generate tests for a single package

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
// PackageSuite - a suite builder class that can be used to generate tests
//	for a single package.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2006-06-04	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace RTi.Util.Test
{
	using Test = junit.framework.Test;

	/// <summary>
	/// This class is a generic Suite builder class that can be used to automatically
	/// generate a TestSuite for a single package.  It allows RTi to avoid the need to 
	/// create and maintain a separate Suite class in every package.
	/// </summary>
	public class PackageSuite
	{

	/// <summary>
	/// Create the suite for the package.  The package name should have been set 
	/// as a Java property named "PACKAGE_NAME". </summary>
	/// <returns> the test suite to be run. </returns>
	/// <exception cref="Exception"> if PACKAGE_NAME is not set. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static junit.framework.Test suite() throws Exception
	public static Test suite()
	{
		string packageName = System.getProperty("PACKAGE_NAME");

		if (string.ReferenceEquals(packageName, null))
		{
			throw new Exception("System property 'PACKAGE_NAME' must be set to the " + "name of the package for which to execute unit " + "tests.");
		}

		return TestUtil.suite(packageName);
	}

	}

}