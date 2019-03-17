using System;
using System.Collections.Generic;

// TestCollector - create test suite

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

namespace RTi.Util.Test
{

	public class TestCollector
	{
		private IList<string> tests;

	public TestCollector()
	{
		tests = new List<string>();
	}

	public virtual void visitAllFiles(File dir)
	{

		if (dir.isDirectory())
		{
			string[] children = dir.list();
			for (int i = 0; i < children.Length; i++)
			{
				visitAllFiles(new File(dir, children[i]));
			}
		}
		else
		{
			//add to list
			if (dir.ToString().EndsWith("Test.java", StringComparison.Ordinal))
			{
				tests.Add(dir.ToString());
			}

		}
	}

	//	 returns a formatted filename with the correct package
	//	 and filename from a given relative path.
	public virtual string formatFileName(string testCase)
	{
			string fName = "";
			testCase.Trim();
			string[] fileSplit = testCase.Split("\\\\", true);
			int flag = 0;

			for (int i = 2; i < fileSplit.Length; i++)
			{
				if (flag == 1)
				{
					if (i == fileSplit.Length - 1)
					{
						fName += (fileSplit[i].Split("\\.", true))[0];
					}
					else
					{
						fName += ((fileSplit[i] + "."));
					}
				}

				if (fileSplit[i].Equals("src"))
				{
					flag = 1;
				}
			}

			return fName;
	}

	public virtual IList<string> getTestList()
	{
		return tests;
	}

	}

}