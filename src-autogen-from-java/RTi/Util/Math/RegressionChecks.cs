using System.Text;

// RegressionChecks - this class provides storage for check on the regression data and results, including ensuring that the

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

namespace RTi.Util.Math
{

	/// <summary>
	/// This class provides storage for check on the regression data and results, including ensuring that the
	/// sample size, correlation coefficient, and T-test (confidence interval) meet required constraints.  The class
	/// is immutable.
	/// </summary>
	public class RegressionChecks
	{

	/// <summary>
	/// Whether the analysis could be performed OK.
	/// </summary>
	internal bool __isAnalysisPerformedOK = false;

	/// <summary>
	/// Minimum sample size required to demonstrate relationship.
	/// </summary>
	internal int? __minimumSampleSize = null;

	/// <summary>
	/// Actual sample size.
	/// </summary>
	internal int? __sampleSize = null;

	/// <summary>
	/// Is sample size OK?
	/// </summary>
	internal bool __isSampleSizeOK = false;

	/// <summary>
	/// Minimum R required to demonstrate relationship.
	/// </summary>
	internal double? __minimumR = null;

	/// <summary>
	/// Actual R for relationship.
	/// </summary>
	internal double? __R = null;

	/// <summary>
	/// Is R OK?
	/// </summary>
	internal bool __isROK = false;

	/// <summary>
	/// Confidence interval required to demonstrate relationship, percent.
	/// </summary>
	internal double? __confidenceIntervalPercent = null;

	/// <summary>
	/// Is the confidence interval met using T test?
	/// </summary>
	internal bool? __isTestOK = null;

	/// <summary>
	/// Create check object indicating check criteria and actual values. </summary>
	/// <param name="analysisPerformedOK"> if true, then the regression analysis was performed OK; if false there was an
	/// error such as divide by zero </param>
	/// <param name="minimumSampleSize"> the minimum sample size that is required </param>
	/// <param name="sampleSize"> the actual sample size </param>
	/// <param name="minimumR"> the minimum R that is required </param>
	/// <param name="R"> the actual R </param>
	/// <param name="confidenceIntervalPercent"> the confidence interval in percent (e.g., 95.0) - can be null if the
	/// confidence interval is not being checked - this is set by the calling code due to complexity in checking the
	/// value based on data in other objects </param>
	/// <param name="isTestOK"> whether the relationship satisfies the confidence interval when T test is checked </param>
	public RegressionChecks(bool analysisPerformedOK, int? minimumSampleSize, int? sampleSize, double? minimumR, double? R, double? confidenceIntervalPercent, bool? isTestOK)
	{
		__isAnalysisPerformedOK = analysisPerformedOK;

		__minimumSampleSize = minimumSampleSize;
		__sampleSize = sampleSize;
		__isSampleSizeOK = true;
		if (__minimumSampleSize != null)
		{
			if (__minimumSampleSize <= 0)
			{
				throw new InvalidParameterException("Minimum sample size (" + __minimumSampleSize + ") is invalid - must be >= 0");
			}
			if ((__sampleSize == null) || (__sampleSize < __minimumSampleSize))
			{
				__isSampleSizeOK = false;
			}
		}

		__minimumR = minimumR;
		__R = R;
		__isROK = true;
		if (__minimumR != null)
		{
			if ((R == null) || (R < minimumR))
			{
				__isROK = false;
			}
		}

		__confidenceIntervalPercent = confidenceIntervalPercent;
		if (__confidenceIntervalPercent == null)
		{
			// No interval specified so OK
			__isTestOK = true;
		}
		else
		{
			__isTestOK = isTestOK;
		}
	}

	/// <summary>
	/// Format a string explaining why a relationship was invalid.  This is useful for logging.
	/// </summary>
	public virtual string formatInvalidRelationshipReason()
	{
		StringBuilder b = new StringBuilder();
		if (!getIsAnalysisPerformedOK())
		{
			b.Append("analysis not performed");
		}
		if (!getIsSampleSizeOK())
		{
			if (b.Length > 0)
			{
				b.Append(", ");
			}
			b.Append("sample size (" + getSampleSize() + ") < " + getMinimumSampleSize());
		}
		if (!getIsROK())
		{
			if (b.Length > 0)
			{
				b.Append(", ");
			}
			b.Append("R (" + getR() + ") < " + getMinimumR());
		}
		double? confidenceIntervalPercent = getConfidenceIntervalPercent();
		if (confidenceIntervalPercent != null)
		{
			if ((getIsTestOK() == null) || !getIsTestOK())
			{
				if (b.Length > 0)
				{
					b.Append(", ");
				}
				b.Append("CI not met");
			}
		}
		return b.ToString();
	}

	/// <summary>
	/// Return the confidence interval that must be met - if null then confidence interval is not checked. </summary>
	/// <returns> the confidence interval that must be met. </returns>
	public virtual double? getConfidenceIntervalPercent()
	{
		return __confidenceIntervalPercent;
	}

	/// <summary>
	/// Indicate whether the analysis was performed OK. </summary>
	/// <returns> true if the analysis was performed OK, false if not. </returns>
	public virtual bool getIsAnalysisPerformedOK()
	{
		return __isAnalysisPerformedOK;
	}

	/// <summary>
	/// Indicate whether R has met the minimum criteria. </summary>
	/// <returns> true if R has met the minimum criteria. </returns>
	public virtual bool getIsROK()
	{
		return __isROK;
	}

	/// <summary>
	/// Indicate whether the sample size has met the minimum criteria. </summary>
	/// <returns> true if the sample size has met the minimum criteria. </returns>
	public virtual bool getIsSampleSizeOK()
	{
		return __isSampleSizeOK;
	}

	/// <summary>
	/// Indicate whether the confidence interval has been met in the T test. </summary>
	/// <returns> true if the confidence interval has been met in the T test. </returns>
	public virtual bool? getIsTestOK()
	{
		return __isTestOK;
	}

	/// <summary>
	/// Return the minimum acceptable R - if null then R is not checked. </summary>
	/// <returns> the minimum acceptable R. </returns>
	public virtual double? getMinimumR()
	{
		return __minimumR;
	}

	/// <summary>
	/// Return the minimum acceptable sample size - if null then sample size is not checked. </summary>
	/// <returns> the minimum acceptable sample size. </returns>
	public virtual int? getMinimumSampleSize()
	{
		return __minimumSampleSize;
	}

	/// <summary>
	/// Return the actual R. </summary>
	/// <returns> the actual R. </returns>
	public virtual double? getR()
	{
		return __R;
	}

	/// <summary>
	/// Return the actual sample size. </summary>
	/// <returns> the actual sample size, may be null. </returns>
	public virtual int? getSampleSize()
	{
		return __sampleSize;
	}

	/// <summary>
	/// Return a simple string indicating the values in the object, useful for logging.
	/// </summary>
	public override string ToString()
	{
		return ("isSampleSizeOK=" + getIsSampleSizeOK() + ", isROK=" + getIsROK() + ", isAnalysisPerformedOK=" + getIsAnalysisPerformedOK() + ", isTestOK=" + getIsTestOK());
	}

	}

}