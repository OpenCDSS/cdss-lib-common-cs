using System;
using System.Collections.Generic;

// TestCase -- this class extends the basic JUnit TestCase class and adds functionality

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
// TestCase -- this class extends the basic JUnit TestCase class and adds
//	additional functionality.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
//
// 2005-09-19	J. Thomas Sapienza, RTi	Initial version.
// 2005-09-20	JTS, RTi		Overrode the int, boolean and String
//					versions of assertEquals() in order to
//					add hooks for logging to a file.
// 2005-09-21	JTS, RTi		Further revision.
// 2006-06-05	JTS, RTi		Renamed to HydroBaseTestCase.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

// TODO SAM 2007-05-09 Why is this related to HydroBase?

namespace RTi.Util.Test
{

	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	using DateTime = RTi.Util.Time.DateTime;

	/// <summary>
	/// This class contains methods for use in running JUnit tests.  Primarily,
	/// this class contains <code>assertEquals()</code> methods not provided by the
	/// base JUnit code.<para>
	/// Because this class is primarily for use within specific tests in a suite of
	/// JUnit tests, it needs to be able to report to the test runner that a test 
	/// has failed.  It does this by use of the <code>internalFail()</code> 
	/// method.
	/// So it should be noted that even though this class 
	/// extends <code>TestCase</code>, it is not intended to ever be run as a test.
	/// </para>
	/// Instead, it can be used as a base class for other TestCase classes.<para>
	/// </para>
	/// <b>Notes on Use:</b><para>
	/// <ul>
	/// <li>The first test added to any TestSuite must be initializeTestSuite()</li>
	/// <li>If information should be logged to a log file, the TestSuite being run 
	/// should have the <code>test_openLogFile</code> test added before any actual
	/// test.  This isn't 
	/// an actual test, but will ensure that the logging information is set up properly.
	/// </li>
	/// <li>Before adding the <code>test_openLogFile</code> test to a Suite, call the
	/// static methods <code>setLogFileDirectory()</code> and 
	/// <code>setLogFileBaseName()</code> to specify how the log file should be named.
	/// </li>
	/// <li>The first line of every TestCase should be 
	/// <code>initializeTestCase(...)</code>.
	/// </li>
	/// <li>The last line of every TestCase should be 
	/// <code>checkTestStatus()</code>.
	/// </li>
	/// </para>
	/// </summary>
	public class HydroBaseTestCase : junit.framework.TestCase
	{

	/// <summary>
	/// Specifies whether to timestamp the log filename.
	/// </summary>
	private static bool __useTimestamp = true;

	/// <summary>
	/// The output levels for writing to Message.  In order, they correspond to:
	/// Debug/Term, Debug/Log, Status/Term, Status/Log, Warning/Term, Warning/Log.
	/// </summary>
	private static int[] __debugLevels = new int[] {0, 0, 0, 2, 0, 3};

	/// <summary>
	/// Used to keep track of the number of errors that occurred in running a single
	/// test case.  Static so that it can be accessed in <code>internalFail()</code>.
	/// </summary>
	private static int __errorCount = 0;

	/// <summary>
	/// Used to count the number of failed tests in a test suite.
	/// </summary>
	private static int __failCount = 0;

	/// <summary>
	/// Used to count the number of successful tests run in a test suite.
	/// </summary>
	private static int __passCount = 0;

	/// <summary>
	/// Used to count the total number of errors occuring.
	/// </summary>
	private static int __totalErrorCount = 0;

	/// <summary>
	/// The directory in which the log file will be placed.
	/// </summary>
	private static string __logDir = null;

	/// <summary>
	/// The base name of the log file.
	/// </summary>
	private static string __logName = null;

	/// <summary>
	/// The name of the current test being run.
	/// </summary>
	private string __name = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="name"> the name of the test being run. </param>
	public HydroBaseTestCase(string name) : base(name)
	{
	}

	/// <summary>
	/// Checks whether two primitive <code>boolean</code> variables are equal.
	/// If not, the test will fail via <code>internalFail()</code>. </summary>
	/// <param name="d1"> the first boolean to check. </param>
	/// <param name="d2"> the second boolean to check. </param>
	public static void assertEquals(bool b1, bool b2)
	{
		Message.printStatus(4, "", "1: " + b1 + "  2: " + b2);
		if (b1 != b2)
		{
			internalFail("Values " + b1 + " and " + b2 + " are not the same.");
		}
		else
		{
			Message.printStatus(3, "", "    PASS");
		}
	}

	/// <summary>
	/// Checks whether two primitive <code>double</code> variables are equal.
	/// If not, the test will fail via <code>internalFail()</code>. </summary>
	/// <param name="d1"> the first double to check. </param>
	/// <param name="d2"> the second double to check. </param>
	public static void assertEquals(double d1, double d2)
	{
		Message.printStatus(4, "", "1: " + d1 + "  2: " + d2);
		if (d1 != d2)
		{
			internalFail("Values " + d1 + " and " + d2 + " are not the same.");
		}
		else
		{
			Message.printStatus(3, "", "    PASS");
		}
	}

	/// <summary>
	/// Checks whether two primitive <code>float</code> variables are equal.
	/// If not, the test will fail via <code>internalFail()</code>. </summary>
	/// <param name="f1"> the first float to check. </param>
	/// <param name="f2"> the second float to check. </param>
	public static void assertEquals(float f1, float f2)
	{
		Message.printStatus(4, "", "1: " + f1 + "  2: " + f2);
		if (f1 != f2)
		{
			internalFail("Values " + f1 + " and " + f2 + " are not the same.");
		}
		else
		{
			Message.printStatus(3, "", "    PASS");
		}
	}

	/// <summary>
	/// Checks whether two <code>int</code> variables are equal.
	/// If not, the test will fail via <code>internalFail</code>. </summary>
	/// <param name="i1"> the first int to check </param>
	/// <param name="i2"> the second int to check. </param>
	public static void assertEquals(int i1, int i2)
	{
		Message.printStatus(4, "", "1: " + i1 + "  2: " + i2);
		if (i1 != i2)
		{
			internalFail("int 1 is " + i1 + " and int 2 is " + i2);
		}
		else
		{
			Message.printStatus(3, "", "    PASS");
		}
	}

	/// <summary>
	/// Checks whether two <code>String</code> variables are equal.
	/// If not, the test will fail via <code>internalFail()</code>. </summary>
	/// <param name="s1"> the first String to check. </param>
	/// <param name="s2"> the second String to check. </param>
	public static void assertEquals(string s1, string s2)
	{
		Message.printStatus(4, "", "1: '" + s1 + "'   2: '" + s2 + "'");

		if (string.ReferenceEquals(s1, null) && string.ReferenceEquals(s2, null))
		{
			Message.printStatus(3, "", "    PASS");
			return;
		}
		else if (string.ReferenceEquals(s1, null))
		{
			internalFail("String 1 is null and String 2 is '" + s2 + "'");
		}
		else if (string.ReferenceEquals(s2, null))
		{
			internalFail("String 1 is '" + s1 + "' and String 2 is null");
		}
		else if (!s1.Equals(s2))
		{
			internalFail("String 1 is '" + s1 + "' and String 2 is '" + s2 + "'");
		}
		else
		{
			Message.printStatus(3, "", "    PASS");
		}
	}

	/// <summary>
	/// Checks to see whether the data objects in two Vectors of data contain the 
	/// same values in the same order.
	/// <b>Important Notes:</b><para>
	/// <ul>
	/// <li>The object comparison will be made via a call to <code>equals()</code>.
	/// <li>The Vectors can contain null objects.</li>
	/// <li>The Objects in the same position in each Vector can be of different types
	/// (although this will be reported as an error).</li>
	/// </ul>
	/// </para>
	/// </summary>
	/// <param name="v1"> the first Vector to check. </param>
	/// <param name="v2"> the second Vector to check. </param>
	/// <exception cref="Exception"> if there are any errors reading through the Vector data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void assertEquals(java.util.List<Object> v1, java.util.List<Object> v2) throws Exception
	public static void assertEquals(IList<object> v1, IList<object> v2)
	{
		if (v1 != null && v2 != null)
		{
			Message.printStatus(4, "", "  Comparing vectors of " + v1.Count + " and " + v2.Count + " elements.");
		}

		if (v1 == null || v2 == null)
		{
			if (v1 == null && v2 == null)
			{
				// null Vectors are basically the same
				return;
			}
			else if (v1 == null)
			{
				internalFail("Vector 1 is null and Vector 2 is not null.");
				return;
			}
			else
			{
				internalFail("Vector 1 is not null and Vector 2 is null.");
				return;
			}
		}

		int size1 = v1.Count;
		int size2 = v2.Count;

		if (size1 != size2)
		{
			internalFail("Vector 1 contains " + size1 + " elements and Vector 2 " + "contains " + size2 + " elements.");
			return;
		}

		object o1 = null;
		object o2 = null;
		for (int i = 0; i < size1; i++)
		{
			o1 = v1[i];
			o2 = v2[i];

			Message.printStatus(4, "", "O1: '" + o1 + "'   O2: '" + o2 + "'");

			if (o1 == null && o2 == null)
			{
				continue;
			}
			else if (o1 == null)
			{
				internalFail("At Vector position " + i + ": The Object in " + "Vector 1 is null.");
				return;
			}
			else if (o2 == null)
			{
				internalFail("At Vector position " + i + ": The Object in " + "Vector 2 is null.");
				return;
			}

			Type c1 = o1.GetType();
			Type c2 = o2.GetType();

			if (c1 != c2)
			{
				internalFail("At Vector position " + i + ": The Object in " + "Vector 1 is of class " + c1 + " while the " + "Object in Vector 2 is of class " + c2);
				return;
			}

			if (!o1.Equals(o2))
			{
				internalFail("The objects (" + o1 + ") and (" + o2 + ") are not equal.");
				return;
			}
		}

		Message.printStatus(3, "", "    PASS");
	}

	/// <summary>
	/// Checks to see whether the TestCase was run successfully.
	/// </summary>
	public virtual void checkTestStatus()
	{
	Message.printStatus(1, "", "EC: " + __errorCount);
		if (__errorCount == 1)
		{
			Message.printWarning(2, "", "FAIL: '" + __name + "'.  1 error.");
			__failCount++;
			fail("FAIL: '" + __name + "'.  1 error.");
		}
		else if (__errorCount > 1)
		{
			Message.printWarning(2, "", "FAIL: '" + __name + "'.  " + __errorCount + " errors.");
			__failCount++;
			fail("FAIL: '" + __name + "'.  " + __errorCount + " errors.");
		}
		else
		{
			Message.printStatus(2, "", "PASS: '" + __name + "'.");
			__passCount++;
		}
	}

	/// <summary>
	/// Returns the number of failed tests. </summary>
	/// <returns> the number of failed tests. </returns>
	public static int getFailCount()
	{
		return __failCount;
	}

	/// <summary>
	/// Returns the number of passed tests. </summary>
	/// <returns> the number of passed tests. </returns>
	public static int getPassCount()
	{
		return __passCount;
	}

	/// <summary>
	/// Initializes settings in this class for when a new TestCase is being run and
	/// prints header information for the test to the log. </summary>
	/// <param name="name"> the name of the test case being run. </param>
	public virtual void initializeTestCase(string name)
	{
		__name = name;
		__errorCount = 0;

		Message.printStatus(2, "", "");
		Message.printStatus(2, "", "Running tests for " + name);
		Message.printStatus(2, "", "-----------------------------------");
	}

	/// <summary>
	/// Initializes settings in this class for when a new TestSuite is being run.
	/// </summary>
	public static HydroBaseTestCase initializeTestSuite()
	{
		return new HydroBaseTestCase("test_initializeTestSuite");
	}

	/// <summary>
	/// A fail method specific to this class that does some special error handling and
	/// log reporting of the error that occurred.  This method is not named 
	/// <code>fail()</code> because access to the superclass <code>fail()</code> is
	/// needed, and since the superclass method is static it cannot be called via:
	/// <code>super.fail()</code>. </summary>
	/// <param name="message"> the message to report as the reason a test failed. </param>
	public static void internalFail(string message)
	{
	//	Message.printWarning(2, "", "FAIL: " + message);
		try
		{
			// this exception is thrown so that the error shows up 
			// in the log file
			throw new Exception("FAIL: " + message);
		}
		catch (Exception e)
		{
			Message.printWarning(3, "", e);
		}

		__errorCount++;
		__totalErrorCount++;
	}

	/// <summary>
	/// Returns a TestCase that opens a log file for writing.  This TestCase should be
	/// the first one added to a TestSuite.  Prior to calling this method, the log
	/// static log file information should be set up with calls to 
	/// <code>setLogFileDirectory</code> and <code>setLogFileBaseName</code>. </summary>
	/// <returns> a TestCase that opens a lof file for writing. </returns>
	public static HydroBaseTestCase openLogFile()
	{
		return new HydroBaseTestCase("test_openLogFile");
	}

	/// <summary>
	/// Sets the base name of the log file.  To this will be added a time stamp and 
	/// the extension ".log". </summary>
	/// <param name="baseName"> the base name of the log file. </param>
	public static void setLogFileBaseName(string baseName)
	{
		__logName = baseName;
	}

	/// <summary>
	/// Sets the name of the directory in which the log file will be opened. </summary>
	/// <param name="directory"> the directory in which the log file will be opened. </param>
	public static void setLogFileDirectory(string directory)
	{
		__logDir = directory;
	}

	/// <summary>
	/// Specifies whether to timestamp the log filename.  Default is true. </summary>
	/// <param name="useTimestamp"> if true, the log file name will have a timestamp.   </param>
	public static void setLogFileTimeStamp(bool useTimestamp)
	{
		__useTimestamp = useTimestamp;
	}

	/// <summary>
	/// Sets output levels for the Message class. </summary>
	/// <param name="debugTerm"> the ouput level for writing DEBUG messages to the terminal.
	/// Default is 0. </param>
	/// <param name="debugLog"> the ouput level for writing DEBUG messages to the log.
	/// Default is 0. </param>
	/// <param name="statusTerm"> the ouput level for writing STATUS messages to the terminal.
	/// Default is 0. </param>
	/// <param name="statusLog"> the ouput level for writing STATUS messages to the log.
	/// Default is 2. </param>
	/// <param name="warningTerm"> the ouput level for writing WARNING messages to the terminal.
	/// Default is 0. </param>
	/// <param name="warningLog"> the ouput level for writing WARNING messages to the log.
	/// Default is 3. </param>
	public static void setOutputLevels(int debugTerm, int debugLog, int statusTerm, int statusLog, int warningTerm, int warningLog)
	{
		__debugLevels[0] = debugTerm;
		__debugLevels[1] = debugLog;
		__debugLevels[2] = statusTerm;
		__debugLevels[3] = statusLog;
		__debugLevels[4] = warningTerm;
		__debugLevels[5] = warningLog;
	}

	public virtual void test_initializeTestSuite()
	{
		__passCount = 0;
		__failCount = 0;
		__totalErrorCount = 0;
	}

	/// <summary>
	/// Opens the log file for writing.
	/// </summary>
	public virtual void test_openLogFile()
	{
		Message.setDebugLevel(Message.TERM_OUTPUT, __debugLevels[0]);
		Message.setDebugLevel(Message.LOG_OUTPUT, __debugLevels[1]);
		Message.setStatusLevel(Message.TERM_OUTPUT, __debugLevels[2]);
		Message.setStatusLevel(Message.LOG_OUTPUT, __debugLevels[3]);
		Message.setWarningLevel(Message.TERM_OUTPUT, __debugLevels[4]);
		Message.setWarningLevel(Message.LOG_OUTPUT, __debugLevels[5]);

		Message.setOutputFile(Message.TERM_OUTPUT, (PrintWriter)null);
		PrintWriter ofp;
		string logFile = null;
		try
		{
			DateTime dt = new DateTime(DateTime.DATE_CURRENT);
			if (string.ReferenceEquals(__logDir, null))
			{
				__logDir = ".";
			}
			if (string.ReferenceEquals(__logName, null))
			{
				__logName = "log";
			}

			string filename = __logName;

			if (__useTimestamp)
			{
				filename += "_" + dt.getYear() + "-"
					+ StringUtil.formatString(dt.getMonth(), "%02d") + "-"
					+ StringUtil.formatString(dt.getDay(), "%02d") + "_"
					+ StringUtil.formatString(dt.getHour(), "%02d") + ""
					+ StringUtil.formatString(dt.getMinute(), "%02d");
			}

			filename += ".log";

			ofp = Message.openLogFile(__logDir + File.separator + filename);
			Message.setOutputFile(Message.LOG_OUTPUT, ofp);
		}
		catch (Exception)
		{
			Message.printWarning(2, "", "Unable to open log file \"" + logFile + "\"");
		}
	}

	public static HydroBaseTestCase testSummary()
	{
		return new HydroBaseTestCase("test_testSummary");
	}

	public virtual void test_testSummary()
	{
		Message.printStatus(2, "", "");
		Message.printStatus(2, "", "SUMMARY");
		Message.printStatus(2, "", "---------------------------------");

		int testCount = __failCount + __passCount;
		string plural = "s";
		if (testCount == 1)
		{
			plural = "";
		}
		Message.printStatus(2, "", "" + testCount + " test" + plural + " run.");

		plural = "s";
		if (__failCount == 1)
		{
			plural = "";
		}
		string plural2 = "s";
		if (__totalErrorCount == 1)
		{
			plural2 = "";
		}
		Message.printStatus(2, "", "" + __failCount + " test" + plural + " failed (" + __totalErrorCount + " error" + plural2 + ")");

		plural = "s";
		if (__passCount == 1)
		{
			plural = "";
		}
		Message.printStatus(2, "", "" + __passCount + " test" + plural + " passed.");
	}

	}

}