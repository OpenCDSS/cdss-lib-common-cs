﻿using System;

// LagKBuilder - class to handle LagK instance

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

namespace riverside.ts.routing.lagk
{
	using TS = RTi.TS.TS;
	using DataUnits = RTi.Util.IO.DataUnits;
	using DataUnitsConversion = RTi.Util.IO.DataUnitsConversion;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;
	using TimeInterval = RTi.Util.Time.TimeInterval;
	using Table = riverside.ts.util.Table;

	/// <summary>
	/// This class provides the builder for a LagK object.
	/// </summary>
	public class LagKBuilder
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			lk = createDefault();
		}


		private LagK lk;
		private int _n_kval = 1;
		private int _n_lagval = 1;
		private double valueOfK = 0;
		private const int MAXISEGS = 20;

		// This value needs to be the same as VariableLagK_Command.__BIG_DATA_VALUE;
		public static double BIG_DATA_VALUE = 1.0e20;

		public LagKBuilder(TS inflows)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			lk._t_mult = inflows.getDataIntervalMult();
			lk._t_int = inflows.getDataIntervalBase();
			DateTime dt = new DateTime(inflows.getDate1());
			lk.forecastDate1 = dt;
			lk.totalInflows = inflows;
		}

		public static void update(LagK lagK, TS inflows)
		{
			DateTime dt = new DateTime(inflows.getDate1());
	//        dt.addInterval(inflows.getDataIntervalBase(),1);
			lagK.forecastDate1 = dt;
		}

		/// <summary>
		/// Create a LagK object that will do the calculations. </summary>
		/// <param name="coLaggedInflow"> carryover for lagged inflow, or null to default to zero. </param>
		/// <param name="coOutflow"> carryover for current outflow, or null to default to zero. </param>
		/// <param name="coStorage"> carryover for current storage, or null to default to zero. </param>
		/// <param name="coInitial"> initial carryover from a previous run, or null to default to zeros. </param>
		public virtual LagK create(double? coLaggedInflow, double? coOutflow, double? coStorage, double[] coInitial)
		{
			string routine = "LagK.create";

			// Size of carryover array includes lag window divided by the interval, plus 1 to be
			// inclusive of the end-points (?maybe?), plus 2 for K.
			// _lagMax is from the lag table
			// _lagMin accounts for negative lag and will be zero in most cases
			// Therefore the total period of lag is the sum of _lagMax and _lagMin
			lk._sizeInflowCO = (lk._lagMax + lk._lagMin) / lk._t_mult + 3;

			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "lk._lag=" + lk._lag + " lk._t_mult=" + lk._t_mult + " lk._sizeInflowCO =" + lk._sizeInflowCO);
			}

			lk.variableK = _n_kval != 1;
			// FIXME SAM 2009-04-16 Negative lag requires variable lag and K and need > 1 row in lag table
			lk.variableLag = _n_lagval != 1;
			if (lk._in_lag_tbl.getNRows() > lk._sizeInflowCO)
			{
				lk._sizeInflowCO = lk._in_lag_tbl.getNRows();
			}
			if (lk._co_inflow == null)
			{
				// The following will default _co_inflow to zeros.
				lk._co_inflow = new double[lk._sizeInflowCO];
			}
			else
			{
				// Set the initial states to zero by default - this should generally be the case because the above will occur
				for (int i = 0; i < lk._co_inflow.Length; i++)
				{
					lk._co_inflow[i] = 0.0;
				}
			}
			// Set the carryover provided initial values
			if (coInitial != null)
			{
				// Copy the array into the carryover, with last value copied first into latest time
				int copyCount = 0;
				for (int i = (lk._co_inflow.Length - 1); i >= 0; i--)
				{
					if (copyCount >= coInitial.Length)
					{
						// No more states to use for initialization
						break;
					}
					lk._co_inflow[i] = coInitial[coInitial.Length - 1 - copyCount];
					++copyCount;
				}
			}
			if (coLaggedInflow != null)
			{
				// Set initial lagged inflow (default is 0.0 if not set)
				lk._laggedInflow = coLaggedInflow.Value;
			}
			if (coOutflow != null)
			{
				// Set initial outflow (default is 0.0 if not set)
				lk._outflowCO = coOutflow.Value;
			}
			if (coStorage != null)
			{
				// Set initial storage (default is 0.0 if not set)
				lk._storageCO = coStorage.Value;
			}

			if (lk.variableLag)
			{
				if (lk._co_inflow == null)
				{
					// TODO SAM 2016-08-28 This should never happen given the logic above that creates the array.
					throw new System.ArgumentException("Must have carry over values");
				}
			}
			else
			{
				//@todo - removed this because the returned _co_inflow array is size 2, but when passing it back,
				// it barfs and says it only wants 1, not 2!

	//            if (lk._sizeInflowCO != lk._co_inflow.length) {
	//                throw new IllegalArgumentException("Must have " + lk._sizeInflowCO + " carry over values, not " + lk._co_inflow.length);
	//            }
			}

			if (lk.variableK)
			{
				makeStorageVSOutflowTable();
				makeStorageVSOutflowTableQuarter();
			}

			// Check to see if a single K value of 0 is set.  If so, ensure that
			// INITIALOUTFLOW is consistent with the first value (and second,
			// if applicable) in the COINFLOW section.
			if (_n_kval == 1 && valueOfK == 0)
			{
				if ((lk._lag % lk._t_mult) != 0)
				{
					double tmpP = lk._co_inflow[0];
					double tmpN = lk._co_inflow[1];
					double diff = lk._t_mult * ((int)lk._lag / lk._t_mult + 1) - lk._lag;
					// Interpolate
					double LagdQin1 = tmpP + (tmpN - tmpP) * (diff / (double)lk._t_mult);
					// Test equality using inequalities
					double tolerance = 0.0001 * lk._outflowCO;
					if (LagdQin1 > lk._outflowCO + tolerance || LagdQin1 < lk._outflowCO - tolerance)
					{
						// Print Warning and revise _outflowCO
						lk.logger.warning(string.Format("INITIALOUTFLOW " + "value \"{0:F}\" not consistent with " + "COINFLOW values \"{1:F} and {2:F}\" when " + "lagged resulting in \"{3:F}\"" + "Instability may result.  Revising " + "INITIALOUTFLOW value to \"{4:F}\".", lk._outflowCO, tmpP, tmpN, LagdQin1, LagdQin1));
						lk._outflowCO = LagdQin1;
	//                    _owner->_outflow = _outflowCO ;
					}
				}
				else
				{
					double LagdQin1 = lk._co_inflow[0];
					// Test equality using inequalities
					double tolerance = 0.0001 * lk._outflowCO;
					if (LagdQin1 > lk._outflowCO + tolerance || LagdQin1 < lk._outflowCO - tolerance)
					{
						// Print Warning and revise _outflowCO
						lk.logger.warning(string.Format("INITIALOUTFLOW " + "value \"{0:F}\" not consistent with " + "COINFLOW value \"{1:F}\".  Instability " + "may result.  Revising INITIALOUTFLOW " + "value to \"{2:F}\".", lk._outflowCO, LagdQin1, LagdQin1));
						lk._outflowCO = LagdQin1;
	//                    _owner->_outflow = _outflowCO ;
					}
				}
			}

			return lk;
		}

		/// <summary>
		/// Set the initial carryover inflows.
		/// This method is apparently not called because initialization occurs in the create() method. </summary>
		/// <param name="d"> </param>
		public virtual void setCarryOverInflows(double[] d)
		{
			lk._co_inflow = d;
		}

		/// <summary>
		/// Set the transit loss coefficient used with the Fort Worth logic.
		/// TODO SAM 2016-08-29 not currently supported by TSTool VariableLagK.
		/// </summary>
		public virtual void setTransLossCoef(double d)
		{
			lk._transLossCoef = d;
		}

		public virtual void setTransLossLevel(double d)
		{
			lk._transLossLevel = d;
		}

		public virtual void setLag(int lag)
		{
			lk._lag = lag;
		}

		public virtual void setInitialLaggedInflow(double inflow)
		{
			lk._laggedInflow = inflow;
		}

		public virtual void setInitialOutflow(double initialOutflow)
		{
			lk._outflowCO = initialOutflow;
		}

		public virtual void setInitialStorage(double initialStorage)
		{
			lk._storageCO = initialStorage;
		}

		public virtual void setK(double k)
		{
			lk._out_k_tbl.allocateDataSpace(2);
			lk._out_k_tbl.populate(0,LagK.FLOWCOLUMN, 0.0);
			lk._out_k_tbl.populate(0,LagK.KCOLUMN, k);
			lk._out_k_tbl.populate(1,LagK.FLOWCOLUMN, double.MaxValue);
			lk._out_k_tbl.populate(1,LagK.KCOLUMN, k);
			_n_kval = 2;
			valueOfK = k;
			lk.variableK = true; // @verify ??T
		}

		public virtual void setLagIn(Table table)
		{
			string routine = this.GetType().Name + ".setLagIn";
			// Add an extra row if negative lag and only one row - at least two are required
			// Also add if one row for positive to force variable lagK logic
			if ((table.getNRows() == 1) && (table.get(0, Table.GETCOLUMN_2) <= 0))
			{
				// Resize the table to 2 rows, copy the first row, and add another
				Message.printStatus(2, routine, "Automatically adding a row to lag table - 2+ are required when using negative lag.");
				Table tableBigger = new Table();
				tableBigger.allocateDataSpace(2);
				tableBigger.populate(0,Table.GETCOLUMN_1, table.get(0, Table.GETCOLUMN_1));
				tableBigger.populate(0,Table.GETCOLUMN_2, table.get(0, Table.GETCOLUMN_2));
				tableBigger.populate(1,Table.GETCOLUMN_1, double.MaxValue);
				tableBigger.populate(1,Table.GETCOLUMN_2, table.get(0, Table.GETCOLUMN_2));
				table = tableBigger;
			}

			lk._in_lag_tbl = table;
			int l = 0;
			for (int i = 0; i < table.getNRows(); i++)
			{
				double v = table.lookup(i,Table.GETCOLUMN_2);
				if (v > l)
				{
					l = (int)(v + .5);
					// Needed for negative lag and to compute carryover size...
					lk._lagMax = l;
				}
				// Needed for negative lag and to compute carryover size...
				if ((v < 0) && (-v > lk._lagMin))
				{
					lk._lagMin = (int)(-v + .5);
				}
			}
			// Round to next even multiple of the time series interval
			if (lk._lagMin % lk._t_mult > 0)
			{
				lk._lagMin = lk._t_mult * ((lk._lagMin / lk._t_mult) + 1);
			}
			Message.printStatus(2,routine, "lagMin=" + lk._lagMin + " lagMax=" + lk._lagMax);
			_n_lagval = table.getNRows();
			lk._lag = l;
			// If negative lagging, all the lag values must be negative or zero
			if (lk._lagMin > 0 && lk._lagMax > 0)
			{
				string message = "Negative and positive lag values cannot occur together (lagMin=" + lk._lagMin + " lagMax=" + lk._lagMax + ")";
				Message.printWarning(3,routine, message);
				throw new Exception(message);
			}
		}

		public virtual void setKOut(Table outKTable)
		{
			// @todo check sorting
			// This next section adds to the K table if the table either
			// starts with non-zero flow or ends with a large flow.
			int flagNeedZero = 0;
			int flagNeedInf = 0;
			double zeroValue = 0.0;
			double infValue = 0.0;

			// REVISIT: when table has getFirst and getLast, update
			// this code to use it.  (Table.getFirst, Table.getLast)
			if (outKTable.lookup(0, LagK.FLOWCOLUMN) > .001)
			{
				flagNeedZero = 1;
				zeroValue = outKTable.lookup(0, LagK.KCOLUMN);
			}
			// Detect if the last value is below a large value, keeping last value
			if (outKTable.lookup(0, LagK.FLOWCOLUMN) < BIG_DATA_VALUE)
			{
				flagNeedInf = 1;
				infValue = outKTable.lookup(outKTable.getNRows() - 1, LagK.KCOLUMN);
			}

			// REVISIT: when table has addFirst and addLast, update
			// this code to use it.  (Table.addFirst, Table.addLast)
			Table tmpTable = new Table();
			tmpTable.allocateDataSpace(outKTable.getNRows() + flagNeedInf + flagNeedZero);

			tmpTable.populate(0, LagK.FLOWCOLUMN, 0.0);
			tmpTable.populate(0, LagK.KCOLUMN, zeroValue);

			for (int j = 0 + flagNeedZero; j < outKTable.getNRows() + flagNeedZero; j++)
			{
				tmpTable.populate(j, LagK.FLOWCOLUMN, outKTable.lookup(j - flagNeedZero, LagK.FLOWCOLUMN));
				tmpTable.populate(j, LagK.KCOLUMN, outKTable.lookup(j - flagNeedZero, LagK.KCOLUMN));
			}

			if (flagNeedInf > 0)
			{
				int tablePosition = outKTable.getNRows() + flagNeedZero;
				tmpTable.populate(tablePosition, LagK.KCOLUMN, infValue);
				tmpTable.populate(tablePosition, LagK.FLOWCOLUMN, BIG_DATA_VALUE);
			}

			lk._out_k_tbl = tmpTable;
			_n_kval = tmpTable.getNRows();
		}

		private LagK createDefault()
		{
			LagK lk = new LagK();
	//        lk._out_k_tbl.allocateDataSpace(1);
	//        lk._out_k_tbl.populate( 0, 0, __BIG_DATA_VALUE );
	//        lk._out_k_tbl.populate( 0, 1, 0 );
	//        lk._transLossCoef = 0;
	//        lk._transLossLevel = 0;

			return lk;
		}

		private void makeStorageVSOutflowTable()
		{
			makeStorageVSOutflowTableBase(lk._stor_out_tbl,1.0);
		}

		private void makeStorageVSOutflowTableQuarter()
		{
			makeStorageVSOutflowTableBase(lk._stor_out_tbl4,4.0);
		}


		internal virtual void makeStorageVSOutflowTableBase(Table outTable, double divisor)
		{
			// This routine will loop through and construct a O2 vs.
			// 2S/dt+O2 table for as many O2 values as specified in the
			// _k_out_tbl.

			double c1 = 12; // Constant for intermediate points
			double c2 = 100; // Constant for intermediate points
			double deltaK = 0.0; // Delta K for i and i+1
			double deltaQ = 0.0; // Delta Q for i and i+1
			int i; // loop variable
			int ipart = 0; // loop variable for intermediate point
			int isegs = 0; // total number of intermediate points
			int npkq = _n_kval; // number of k vs. q pairs
			int nPos = 0; // current number of K/Outflow pairs
			double q1 = 0.0; // flow at i
			double q2 = 0.0; // flow at i+1
			double qbar = 0.0; // average flow across period
			Table result_table; // Results storage before copy to outTable
			double storage = 0.0; // Storage term
			double xita = lk._t_mult / divisor; // time step
	//
	//        PrintDebug( 10, routine, "Lag-K Storage-Outflow Table &"
	//            " Outflow-K table." );

			// To avoid pre-counting, allocate maximum possible space for local
			// results table
			result_table = new Table();
			result_table.allocateDataSpace(npkq * MAXISEGS + 2);

			// q1/q2 are used in the algorithm in place of o1/o2 in the
			// external documentation.
			for (i = 0 ; i < npkq ; i++)
			{
				if (i < npkq - 1)
				{
					deltaK = Math.Abs(lk._out_k_tbl.lookup(i, LagK.KCOLUMN) - lk._out_k_tbl.lookup(i + 1, LagK.KCOLUMN));
					deltaQ = Math.Abs(lk._out_k_tbl.lookup(i, LagK.OUTFLOWCOLUMN) - lk._out_k_tbl.lookup(i + 1, LagK.OUTFLOWCOLUMN));

					isegs = 1;
					if (deltaK != 0)
					{
						isegs = (int)(((deltaQ + (c1 * deltaK)) / c2) + 1.5);
					}
					if (isegs > MAXISEGS)
					{
						isegs = MAXISEGS;
					}


	//                Line20:
					// For each sample point, interpolate to get the 2S/dt+O2 vs.
					// O2 table.
					for (ipart = 0 ; ipart < isegs ; ipart++)
					{
						q2 = lk._out_k_tbl.lookup(i, LagK.OUTFLOWCOLUMN) + deltaQ * ipart / isegs;
						result_table.populate(nPos, LagK.STOR_OUTFLOWCOLUMN, q2);
						qbar = (q2 + q1) / 2.0;
						storage = lk._out_k_tbl.lookup(qbar, LagK.KCOLUMN, true) * (q2 - q1) + storage;
						result_table.populate(nPos, LagK.STOR_SDTCOLUMN, (2.0 * storage / xita) + q2);

						q1 = q2;
						nPos++;
					}
				}
				else
				{
					q2 = lk._out_k_tbl.lookup(i, LagK.OUTFLOWCOLUMN);
					result_table.populate(nPos, LagK.STOR_OUTFLOWCOLUMN, q2);
					qbar = (q2 + q1) / 2;
					storage = lk._out_k_tbl.lookup(qbar, LagK.KCOLUMN, true) * (q2 - q1) + storage;
					result_table.populate(nPos, LagK.STOR_SDTCOLUMN, (2.0 * storage / xita) + q2);

					q1 = q2;
					nPos++;
				}
			}

	//        Line100:
			result_table.populate(nPos, LagK.STOR_OUTFLOWCOLUMN, q2);
			qbar = (q2 + q1) / 2;
			storage = lk._out_k_tbl.lookup(qbar, LagK.KCOLUMN, true) * (q2 - q1) + storage;
			result_table.populate(nPos, LagK.STOR_SDTCOLUMN, (2.0 * storage / xita) + q2);

			// Verify this - A table without zero should return a non-zero value
			// or a failure for this to work.
			if (result_table.lookup(nPos, LagK.STOR_SDTCOLUMN, true) == 0.0)
			{
	//                goto Line200;
			}
			else
			{

				// Add a (0, 0) point to the result_table.  Allocate and copy because
				// table resizing is not available.

				if ((result_table.lookup(0, LagK.STOR_SDTCOLUMN) > 0.01) || (result_table.lookup(0, LagK.STOR_OUTFLOWCOLUMN) > 0.01))
				{
				for (i = nPos - 1 ; i >= 0 ; i--)
				{
					result_table.populate(i + 1, LagK.STOR_SDTCOLUMN, result_table.lookup(i, LagK.STOR_SDTCOLUMN));
					result_table.populate(i + 1, LagK.STOR_OUTFLOWCOLUMN, result_table.lookup(i, LagK.STOR_OUTFLOWCOLUMN));
				}
				result_table.populate(0, LagK.STOR_SDTCOLUMN, 0.0);
				result_table.populate(0, LagK.STOR_OUTFLOWCOLUMN, 0.0);
				nPos++;
				}
			}

	//            Line200:
			//q2 = 1.0e+6 ;
			q2 = BIG_DATA_VALUE;
			result_table.populate(nPos, LagK.STOR_OUTFLOWCOLUMN, q2);
			qbar = (q2 + q1) / 2;
			storage = lk._out_k_tbl.lookup(qbar, LagK.KCOLUMN, true) * (q2 - q1) + storage;
			result_table.populate(nPos, LagK.STOR_SDTCOLUMN, (2.0 * storage / xita) + q2);
			nPos++;


			// For the final result, copy to the outTable
			outTable.allocateDataSpace(nPos);
			for (i = 0 ; i < nPos ; i++)
			{
				outTable.populate(i, LagK.STOR_OUTFLOWCOLUMN, result_table.lookup(i, LagK.STOR_OUTFLOWCOLUMN));
				outTable.populate(i, LagK.STOR_SDTCOLUMN, result_table.lookup(i, LagK.STOR_SDTCOLUMN));
	//            PrintDebug( 10, "", "Stor:  %f  Out:  %f",
	//                outTable.lookup( i, LagK.STOR_SDTCOLUMN ),
	//                outTable.lookup( i, LagK.STOR_OUTFLOWCOLUMN ) ) ;
			}

		}


		/// <summary>
		/// Ensure that the values in a Table are normalized to a time series with respect to interval and units. </summary>
		/// <param name="originalTable"> The Table to check </param>
		/// <param name="lagInterval"> The interval of the Table </param>
		/// <param name="flowUnits"> The units of the Table </param>
		/// <param name="ts"> The time series to use for units and interval </param>
		/// <returns> A new normalized Table or the original if no changes were needed </returns>
		/// <exception cref="IllegalArgumentException"> if either the interval or units could not be converted. </exception>
		public static Table normalizeTable(Table originalTable, TimeInterval lagInterval, string flowUnits, TS ts)
		{
			if (originalTable.getNRows() == 0)
			{
				return originalTable;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean intervalBaseExact = lagInterval.getBase() == ts.getDataIntervalBase();
			bool intervalBaseExact = lagInterval.getBase() == ts.getDataIntervalBase();
			DataUnitsConversion unitsConversion = null;
			bool unitsExact = DataUnits.areUnitsStringsCompatible(ts.getDataUnits(), flowUnits, true);
			if (!unitsExact)
			{
				// Verify that conversion factors can be obtained.
				bool unitsCompatible = DataUnits.areUnitsStringsCompatible(ts.getDataUnits(), flowUnits, false);
				if (unitsCompatible)
				{
					// Get the conversion factor on the units.
					try
					{
						unitsConversion = DataUnits.getConversion(flowUnits, ts.getDataUnits());
					}
					catch (Exception e)
					{
						throw new Exception("Obtaining data units conversion failed", e);
					}
				}
				else if (!unitsExact && unitsCompatible)
				{
					// This is a problem
					throw new System.ArgumentException(string.Format("The input time series units \"{0}\" " + "are not compatible with table flow values \"{1}\" - cannot convert to use for routing.", ts.getDataUnits(), flowUnits));

				}
			}
			if (unitsExact && intervalBaseExact)
			{
				return originalTable;
			}

			Table newTable = new Table();
			newTable.allocateDataSpace(originalTable.getNRows());
			double flowAddFactor = unitsConversion == null ? 0 : unitsConversion.getAddFactor();
			double flowMultFactor = unitsConversion == null ? 1 : unitsConversion.getMultFactor();
			double timeMultFactor; // Convert from original table to time series base interval, 1.0 for no change
			if (intervalBaseExact)
			{
				timeMultFactor = 1.0;
			}
			else if ((lagInterval.getBase() == TimeInterval.DAY) && (ts.getDataIntervalBase() == TimeInterval.HOUR))
			{
			   timeMultFactor = lagInterval.getMultiplier() * 24.0;
			}
			else if ((lagInterval.getBase() == TimeInterval.DAY) && (ts.getDataIntervalBase() == TimeInterval.MINUTE))
			{
				timeMultFactor = lagInterval.getMultiplier() * 24.0 * 60.0 / ts.getDataIntervalMult();
			}
			else if ((lagInterval.getBase() == TimeInterval.HOUR) && (ts.getDataIntervalBase() == TimeInterval.MINUTE))
			{
				timeMultFactor = lagInterval.getMultiplier() * 60.0 / ts.getDataIntervalMult();
			}
			else if ((lagInterval.getBase() == TimeInterval.HOUR) && (ts.getDataIntervalBase() == TimeInterval.DAY))
			{
				timeMultFactor = lagInterval.getMultiplier() / ts.getDataIntervalMult() * 24.0;
			}
			else if ((lagInterval.getBase() == TimeInterval.MINUTE) && (ts.getDataIntervalBase() == TimeInterval.DAY))
			{
				timeMultFactor = lagInterval.getMultiplier() / ts.getDataIntervalMult() * 24.0 * 60.0;
			}
			else if ((lagInterval.getBase() == TimeInterval.MINUTE) && (ts.getDataIntervalBase() == TimeInterval.HOUR))
			{
				timeMultFactor = lagInterval.getMultiplier() / ts.getDataIntervalMult() * 60.0;
			}
			else
			{
				throw new System.ArgumentException(string.Format("The input time series interval \"{0}\" cannot be converted to internal values of \"{1}\"", ts.getIdentifier().getInterval(), lagInterval));
			}
			for (int i = 0; i < originalTable.getNRows(); i++)
			{
				double newFlowValue = flowAddFactor + flowMultFactor * originalTable.get(i,0);
				double newTimeValue = timeMultFactor * originalTable.get(i,1);
				newTable.set(i, newFlowValue, newTimeValue);
			}
			return newTable;
		}

	}

}