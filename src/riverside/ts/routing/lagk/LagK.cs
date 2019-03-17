﻿// LagK - class to implement LagK routing

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
	using TSData = RTi.TS.TSData;
	using TSIterator = RTi.TS.TSIterator;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;
	using Table = riverside.ts.util.Table;

	/// <summary>
	/// This class is a port of the National Weather Service River Forecast System (NWSRFS)
	/// RES-J operation of similar name.  The original C++ code was ported to Java and
	/// implemented in the TSTool VariableLagK command, initially assuming states are 0.
	/// Support for saving and loading states was added 2016-08, with attempts to be consistent
	/// with the legacy functionality and also CHPS FEWS used by the NWS, although JSON is used
	/// to save states to a table in TSTool.
	/// </summary>
	public class LagK
	{

		// from LagK.h

		// K-Outflow table. Outflow in col 0
		// and K in col 1
		internal Table _out_k_tbl = new Table();

		// inflow-lag table. Inflow is in col 0
		// and lag in col 1.
		internal Table _in_lag_tbl = new Table();

		// O2 vs. 2S2/dt+O2 table. S term in
		// col 0 and O2 term in col 1
		internal Table _stor_out_tbl = new Table();

		// O2 vs. 2S2/(1/4*dt)+O2 table. S term
		// in col 0 and O2 term in col 1
		internal Table _stor_out_tbl4 = new Table();

		// Lag value in units of time.
		internal int _lag;

		/// <summary>
		/// If any negative lag values are in the table, this is the corresponding value, rounded to
		/// the base interval and positive.  For example, if -24.2 (hours) is encountered,
		/// the value would be 24.  This is needed to handle negative lag.
		/// </summary>
		internal int _lagMin = 0;

		/// <summary>
		/// If any positive lag values are in the table, this is the corresponding value, rounded to
		/// the base interval.  For example, if 24.2 (hours) is encountered, the value would be 24.
		/// This is needed to handle negative lag.
		/// </summary>
		internal int _lagMax = 0;

	//    int
	//        _n_kval,                // Number of k values in the table.
	//        _n_lagval,              // Number of lag values in the table.
	//        _k_mode,                // Constant or variable K.
	//        _lag_mode,              // Constant or variable LAG.
	//        _interp,                // Will get set to 1 if we need to interpolate for the lagged inflow values.
	//        _n_OutStorVals;         // Number of value pairs in the _stor_out_tbl.

		internal double[] _co_inflow; // Carryover inflow value array

		internal double _outflowCO; // Last outflow
		internal double _laggedInflow; // Lagged inflow

		internal double _storageCO; // Last storage

		// Number of inflow values required for continuous execution of LagK--
		// number of carry over inflow values
		internal int _sizeInflowCO;

		internal double _transLossCoef; // Fort Worth transmission loss Coefficient
		internal double _transLossLevel; // Minimum Level for Fort Worth Loss Coeff

		// from other headers
		private double MISSING = -999.0;

		// iws porting changes
		internal bool variableK = false;
		internal bool variableLag = false;

		// iws porting additions
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		internal Logger logger = Logger.getLogger(this.GetType().FullName);
		internal TS totalInflows; // replace owner->getTotalInflows
		internal DateTime forecastDate1; // from Method.h
		internal int _t_int; // from Method.h
		internal int _t_mult; // from Method.h
		public const int FLOWCOLUMN = Table.GETCOLUMN_1;
		public const int KCOLUMN = Table.GETCOLUMN_2;
		public const int OUTFLOWCOLUMN = Table.GETCOLUMN_1;
		public const int STOR_OUTFLOWCOLUMN = Table.GETCOLUMN_2;
		public const int STOR_SDTCOLUMN = Table.GETCOLUMN_1;
		public const int TIMECOLUMN = Table.GETCOLUMN_2;

		private DateTime getForecastDate1()
		{
			return forecastDate1;
		}

		public virtual int getLag()
		{
			return _lag;
		}

		public virtual int getSizeInflowCO()
		{
			return _sizeInflowCO;
		}

		public virtual void doCarryOver(DateTime cur_date)
		{
			// TODO SAM 2009-04-21 Evaluate whether +1 needed
			//Table coTable = new Table(_sizeInflowCO + 1);
			// Create a new carryover table (will contain all missing)
			Table coTable = new Table(_sizeInflowCO);
			// Fill the table with carryover values for the requested date/time
			getCarryOverValues(cur_date, coTable);
			// Save carryover values from the 2-column table to the one-column array
			_co_inflow = coTable.getColumn(FLOWCOLUMN);
		}

		/// <summary>
		/// Return the current carryover array.
		/// @return
		/// </summary>
		public virtual double[] getCarryoverValues()
		{
			return _co_inflow;
		}

		public virtual double getStorageCarryOver()
		{
			return _storageCO;
		}

		public virtual double getLaggedInflow()
		{
			return _laggedInflow;
		}

		public virtual double[] getCarryOverValues(DateTime cur_date)
		{
			Table coTable = new Table(_sizeInflowCO + 1);
			getCarryOverValues(cur_date, coTable);
			return coTable.getColumn(FLOWCOLUMN);
		}

		/// <summary>
		/// If negative lag, initialize the lag object by grabbing future values from the time series
		/// and putting them in the carryover array.  Also adjust the initial date/time of the iterator
		/// to start at the correct initial time because of this shift. </summary>
		/// <param name="tsi"> time series iterator </param>
		/// <returns> time series iterator that is properly set up to handle the negative lag </returns>
		public virtual void initializeCarryoverForNegativeLag(TSIterator tsi)
		{
			string routine = this.GetType().Name + ".initializeCarryoverForNegativeLag";
			if (_lagMin > 0)
			{
				int numIntervals = _lagMin / _t_mult + 1; // Zero if no negative lag
				TSData data = null;
				for (int i = 0; i < numIntervals; i++)
				{
					data = tsi.next();
					_co_inflow[_sizeInflowCO - numIntervals + i] = data.getDataValue();
					Message.printStatus(2, routine, "Assigning initial carryover at " + data.getDate() + " to " + data.getDataValue());
				}
			}
		}

		public virtual double solveMethod(DateTime cur_date, double previousOutflow)
		{
			double LagdQin1 = 0.0; // Calculated lagged outflow before K (t-1) timestep
			double LagdQin2 = 0.0; // Calculated lagged outflow before K (t) timestep
			double Qout1 = 0.0; // Outflow after K (t-1) timestep
			double Qout2 = 0.0; // Outflow after K (t) timestep

			// The K routines require two lagged inflow values.  The solved
			// value is at position 1, the other value is in carryover.
			LagdQin1 = _laggedInflow;
			LagdQin2 = solveLag(cur_date);

			// Qout1 is the outflow calculated from the last timestep
			// Qout2 is either the result of lagging or the result of K
	//        Qout1 = _owner->_outflow ;
			Qout1 = previousOutflow;
			Qout2 = LagdQin2;

			// Choose algorithm, we prefer Atlanta, except when doing
			// the Fort Worth Transmission Loss Recession calculation
			if (variableK && _transLossCoef == 0)
			{
				Qout2 = solveAtlantaK(LagdQin1, LagdQin2, previousOutflow);
			}
			else if (variableK && _transLossCoef > 0)
			{
				Qout2 = solveMCP2K(LagdQin1, LagdQin2, previousOutflow);
			}


			// If Qout1 is still missing, then we are in the first timestep.
			// We assume outflow was the same as the inflow at the oldest inflow
			// ( LagdQin1 ) .  It is now included in carryover 11/27/01.
	//        if ( Qout1 == MISSING ) {
	//            Qout1 = LagdQin1 ;
	//        }

			// Only set the _owner members if calling function is not
			// a ComboMethod.  This is only RES-J 
	//        if ( !group_val ) {
	//            _owner->_outflow = Qout2 ;
	//        } else {
	//            *group_val[0] = Qout2 ;
	//            PrintError( routine, "Using LagK within a ComboMethod is "
	//                "not currently supported." ) ;
	//            return ( STATUS_FAILURE ) ;
	//        }

	//        PrintDebug( 5, routine, "Current date: %s  QI1:  %f  QI2:  %f",
	//            cur_date.toString( ) , LagdQin1, LagdQin2 ) ;
	//        PrintDebug( 5, routine, "Setting outflow on %s to %f.",
	//            _owner->_id, Qout2 ) ;
			logger.finer(string.Format("Date: {0} QI1 : {1:F} QI2: {2:F}",cur_date.ToString(),LagdQin1,LagdQin2));

			// TODO SAM 2009-04-21 The following code seems to be necessary for negative lag.  However, if run for
			// positive lag, it breaks the code.  Similar logic must be occurring somewhere for positive lag.
			// Add current inflow to carryover array and shift values
			if (_lagMin > 0)
			{
				for (int i = 0; i < _sizeInflowCO - 1; i++)
				{
					_co_inflow[i] = _co_inflow[i + 1];
				}
				_co_inflow[_sizeInflowCO - 1] = totalInflows.getDataValue(cur_date);
			}

			return Qout2;
		}

		private double solveLag(DateTime cur_date)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string routine = this.GetType().FullName + ".solveLag";
	//        _Active = 1 ;

			// Note: A goodly portion of this code has been copied to
			// LagK_SetGet.cxx.  Changes here need to be duplicated there.
			bool add = true; // State variable - inside/outside of lagged TS
			Table calcTable = new Table(); // Q,T pairs, after repeats/missing are removed
			double currentFlow = 0.0; // Flow variable used for lagging calcTable
			DateTime date1; // date of the last carryover
			int i; // loop counter
			int j; // loop counter
			Table laggedTable = new Table(); // The lagged Q,T pairs using lag and calcTable
			double lagHours = 0.0; // calculated Lag, either constant or variable
			int lastJ = 0; // Last J value where a function hit was calculated
			double lastT = 0.0; // Last T value where a function hit was calculated
			int lastValue = 0; // The ending position of a string of repeats
			int nMissing = 0; // The number of missing values in the Q,T pairs
			//int    nIntervals = 0 ;  // nIntervals available from getTotalInflow
			double nValues = 0.0; // nValues in a string of repeats
			double qtj = 0.0; // Time T at current j
			double qtj1 = 0.0; // Time T at j - 1
			double qtj2 = 0.0; // Time T at j + 1
			int requiredCO; // Number of Q,T pairs in either calcTable or
			// laggedTable, whichever is in use
			double result; // Results of the lagging, at one timestep back and
			// at the current timestep
			Table smallTable = new Table(); // Table used for interpolation when qtj <> tStar
			double timeHours = 0.0; // Time counter = T in Q,T initial pairs
			double valueSum = 0.0;
			double tStar = 0.0; // Time T where Q is sampled from laggedTable

			smallTable.allocateDataSpace(2);

			requiredCO = _sizeInflowCO;
			laggedTable.allocateDataSpace(requiredCO + 1);

			// Populate laggedTable with the current carryover values, based on
			// carryover from last time's date.
			getCarryOverValues(cur_date, laggedTable);

			// Lag the values in the working array
			for (i = 0 ; i < requiredCO ; i++)
			{
				currentFlow = laggedTable.lookup(i, Table.GETCOLUMN_1);
				timeHours = laggedTable.lookup(i, Table.GETCOLUMN_2);
				lagHours = _lag;
				if (variableLag)
				{
					lagHours = _in_lag_tbl.lookup(currentFlow, Table.GETCOLUMN_2, true);
				}

				laggedTable.populate(i, Table.GETCOLUMN_2, timeHours + lagHours);
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "cur_date=" + cur_date + " lagHours=" + lagHours + " laggedTable=\n" + laggedTable);
			}

			// If any subsequent lagged values are the same and not missing,
			// average them and set the remainder to missing.
			// Keep track of the number of missing.
			for (i = 0 ; i < requiredCO - 1 ; i++)
			{
				if ((laggedTable.lookup(i, TIMECOLUMN) == laggedTable.lookup(i + 1, TIMECOLUMN)) && (laggedTable.lookup(i, FLOWCOLUMN) > 0))
				{
					valueSum = laggedTable.lookup(i, FLOWCOLUMN);
					nValues = 1.0;
					for (j = i ; j < requiredCO - 1 ; j++)
					{
						if (laggedTable.lookup(j, TIMECOLUMN) == laggedTable.lookup(j + 1, TIMECOLUMN))
						{
							valueSum += laggedTable.lookup(j + 1, FLOWCOLUMN);
							nValues += 1.0;
							// set flow to missing
							laggedTable.populate(j, FLOWCOLUMN, -999);
							lastValue = j + 1;
						}
						else
						{
							break;
						}
					}
					if (nValues > 0)
					{
						laggedTable.populate(lastValue, FLOWCOLUMN, valueSum / nValues);
					}
				}

				if (laggedTable.lookup(i, FLOWCOLUMN) < 0)
				{
					nMissing++;
				}
			}

			// Remove all missing values by copying non-missing to calcTable
			calcTable.allocateDataSpace(requiredCO - nMissing);
			nMissing = 0;
			for (i = 0 ; i < requiredCO ; i++)
			{
				if (laggedTable.lookup(i, FLOWCOLUMN) > -998.5)
				{
					calcTable.populate(i - nMissing, FLOWCOLUMN, laggedTable.lookup(i, FLOWCOLUMN));
					calcTable.populate(i - nMissing, TIMECOLUMN, laggedTable.lookup(i, TIMECOLUMN));
				}
				else
				{
					nMissing++;
				}
			}
			requiredCO -= nMissing;
			laggedTable.freeDataSpace();

			// Loop through the array, checking for double backs
			int doubleBack = 0;
			for (i = 0 ; i < requiredCO - 1 ; i++)
			{
				if (calcTable.lookup(i, TIMECOLUMN) >= calcTable.lookup(i + 1, TIMECOLUMN))
				{
					doubleBack++;
				}
			}

			// If no double backs, perform lookup to get the value at t = 0
			if (doubleBack == 0)
			{
				result = calcTable.lookup(0.0, FLOWCOLUMN, true);
			}
			else // otherwise, calculate resulting flow "inside" curve
			{
				// The blocks of code in this code are based on the NWSRFS code
				// For the most complex portions of the code, the line numbers from
				// the fortran code were retained as labels.  All of the if...goto
				// statements were changed if possible to remove the goto, using
				// if/else instead.

				lastT = 0.0;
				lastJ = 0;
				add = true;

				// Solve at a single time step, t = 0
				tStar = 0.0;
				i = 1;
				result = 0.0;
				for (j = 0 ; j < requiredCO ; j++)
				{
					qtj = calcTable.lookup(j, TIMECOLUMN);

					// Check if exactly at tStar, meaning no interp required
					if (qtj == tStar)
					{

						// If at first position, avoid using qtj1
						if (j == 0)
						{
							// If table is only sized at 1, avoid using qtj2
							if (j >= requiredCO - 1) // rarely true
							{
								result += calcTable.lookup(j, FLOWCOLUMN);
	//                            goto Line295 ;
								// Next line outside above for loop so break is OK.
								break;
							}

							// Function is moving from left to right --> Add
							qtj2 = calcTable.lookup(j + 1, TIMECOLUMN);
							if (qtj2 > tStar)
							{
								result += calcTable.lookup(j, FLOWCOLUMN);
								add = true;
								continue;
							}

							// Function is moving from right to left --> Subtract
							if (qtj2 < tStar)
							{
								result -= calcTable.lookup(j, FLOWCOLUMN);
								add = false;
								lastJ = j;
								lastT = tStar;
								continue;
							}
							continue;
						}
	//                    Line247:
						// If at last pair, avoid using qtj2
						qtj1 = calcTable.lookup(j - 1, TIMECOLUMN);
						if (j >= requiredCO - 1)
						{

	//                            #if 0
	//                                // The code in flag7.f contains this block.
	//                                //
	//                                // This code matches code in flag7.f, but because
	//                                // this code has problems, it is ifdef'ed out.
	//                                //
	//                                // This code causes incorrect solutions for point
	//                                // hits on the last point of the solved time series.
	//                                // Because RES-J solves a time step at a time,
	//                                // this problem hits at almost every time step.
	//                                //
	//                                // This is a change in algorithm, because the
	//                                // original algorithm represented in flag7.f had
	//                                // times when it either ignored the last point or
	//                                // averaged it when carryover values were removed.
	//                                if ( qtj1 > tStar ) {
	//                                result -= calcTable.lookup( j - 1,
	//                                    FLOWCOLUMN ) ;
	//                                add = false ;
	//                                lastJ = j ;
	//                                lastT = tStar ;
	//                                continue ;
	//                                }
	//                            #endif

	//                                Line248:
							// The function is moving from left to right --> Add
							if (qtj1 < tStar)
							{
								result += calcTable.lookup(j - 1, FLOWCOLUMN);
								add = true;
								continue;
							}
							continue;
						}

	//                        Line251:
						// qtj == tStar and in the middle of calcTable
						// The function is moving through qtj from
						// left to right --> Add
						qtj2 = calcTable.lookup(j + 1, TIMECOLUMN);
						if ((qtj1 < tStar) && (qtj2 > tStar))
						{
							result += calcTable.lookup(j, FLOWCOLUMN);
							add = true;
							continue;
						}
							// The function is moving through qtj from
							// right to left --> Subtract
							if ((qtj1 > tStar) && (qtj2 < tStar))
							{
								result -= calcTable.lookup(j, FLOWCOLUMN);
								add = false;
								lastJ = j;
								lastT = tStar;
								continue;
							}
						Line252Break:
							continue;
					}
	//                Line250:
					// Cannot interpolate - the segment is has only one point.
					if (j == requiredCO - 1)
					{
						continue;
					}

					// if qtj < tStar and qtj2 > tStar, add current flow
					// if at the end of the qt array, continue
					qtj2 = calcTable.lookup(j + 1, TIMECOLUMN);

					// When tStar is betwen qtj and qtj2, and the function
					// is moving from left to right --> Add
					if ((qtj < tStar) && (qtj2 > tStar))
					{
						smallTable.populate(0, FLOWCOLUMN, calcTable.lookup(j, FLOWCOLUMN));
						smallTable.populate(1, FLOWCOLUMN, calcTable.lookup(j + 1, FLOWCOLUMN));
						smallTable.populate(0, TIMECOLUMN, calcTable.lookup(j, TIMECOLUMN));
						smallTable.populate(1, TIMECOLUMN, calcTable.lookup(j + 1, TIMECOLUMN));
						result += Interpolate(tStar, smallTable);
						add = true;
						continue;
					}
	//                    Line260:
					// When tStar is betwen qtj2 and qtj, and the function
					// is moving from right to left --> Subtract
					if ((qtj > tStar) && (qtj2 < tStar))
					{
						smallTable.populate(0, FLOWCOLUMN, calcTable.lookup(j, FLOWCOLUMN));
						smallTable.populate(1, FLOWCOLUMN, calcTable.lookup(j + 1, FLOWCOLUMN));
						smallTable.populate(0, TIMECOLUMN, calcTable.lookup(j, TIMECOLUMN));
						smallTable.populate(1, TIMECOLUMN, calcTable.lookup(j + 1, TIMECOLUMN));
						result -= Interpolate(tStar, smallTable);
						add = false;
						lastJ = j;
						lastT = tStar;
						continue;
					}
				}

	//            Line295:
				// If the function subtracted last, add the function at lastT.
				if (!add)
				{
					if (calcTable.lookup(j, TIMECOLUMN) > calcTable.lookup(j + 1, TIMECOLUMN))
					{
						logger.severe(string.Format("Bad interpolation j={0:D}, .", cur_date.ToString()));
					}
					smallTable.populate(0, FLOWCOLUMN, calcTable.lookup(lastJ, FLOWCOLUMN));
					smallTable.populate(1, FLOWCOLUMN, calcTable.lookup(lastJ + 1, FLOWCOLUMN));
					smallTable.populate(0, TIMECOLUMN, calcTable.lookup(lastJ, TIMECOLUMN));
					smallTable.populate(1, TIMECOLUMN, calcTable.lookup(lastJ + 1, TIMECOLUMN));
					result += smallTable.lookup(lastT, FLOWCOLUMN, true);
				}
			}

			return result;
		}

		/// <summary>
		/// Get the carryover values for the specified DateTime.  The overloaded method is called with offset=0. </summary>
		/// <param name="cur_date"> requested date/time for carryover values </param>
		/// <param name="laggedTable"> lagged inflow table to be filled with carryover values </param>
		private void getCarryOverValues(DateTime cur_date, Table laggedTable)
		{
			getCarryOverValues(cur_date,laggedTable,0);
		}

		/// <summary>
		/// Get the carryover values for the specified DateTime. </summary>
		/// <param name="cur_date"> requested date/time for carryover values </param>
		/// <param name="laggedTable"> lagged inflow table to be filled with carryover values </param>
		/// <param name="offset"> is not used </param>
		private void getCarryOverValues(DateTime cur_date, Table carryOverTable, int offset)
		{
			string routine = this.GetType().Name + ".getCarryOverValues";

			int requiredCO = _sizeInflowCO;
	//        char tmp_str3[512];
			int i;

			// Clear the carryover table by filling with zeros
			for (i = 0; i < carryOverTable.getNRows(); i++)
			{
				carryOverTable.populate(i, FLOWCOLUMN, 0.0);
				carryOverTable.populate(i, TIMECOLUMN, 0.0);
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "At " + cur_date + " after clearing carryover table: \n" + carryOverTable);
			}

			// The number of time steps between the forecast date and the current
			// simulation date is the number of steps that are available from
			// simulation.
			int nSimulatedDates = 1;
			/* TODO SAM 2009-03-29 Use the above always - below seems too complicated (working with MLB).
			if ( getForecastDate1().lessThan( cur_date ) ) {
			    nSimulatedDates = TimeUtil.getNumIntervals(
			        getForecastDate1( ) , cur_date, _t_int, _t_mult ) + 1;
			    
			    if ( nSimulatedDates > requiredCO )
			        nSimulatedDates = requiredCO;
			}
			*/
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "getForecastDate1=" + getForecastDate1() + " cur_date=" + cur_date + " SimulatedDates=" + nSimulatedDates + " requiredCO=" + requiredCO);
			}

			// Place numbers into the working array, note that times will be
			// negative.  When nSimulatedDates is larger than the
			// carryover size, the carryover array will not be used.
			//double timeHours = (double) -( requiredCO - 1 )*_t_mult + (_lagMin/_t_mult + 1)*_t_mult;
			double timeHours = 0.0;
			if (_lagMin > 0)
			{
				// Have negative lag - not sure why there is an extra +1 on the second term.
				// This fouls up the case where there is no negative lag
				timeHours = (_lagMin / _t_mult - requiredCO + 1) * _t_mult;
				// Other code initializes the states so no need to do that work below
				nSimulatedDates = 0;
			}
			else
			{
				timeHours = (double) - (requiredCO - 1) * _t_mult;
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "At cur_date=" + cur_date + " initial timeHours=" + timeHours);
			}
			for (i = 0 ; i < requiredCO - nSimulatedDates; i++)
			{
				double currentFlow = _co_inflow[i + nSimulatedDates];
				carryOverTable.populate(i, FLOWCOLUMN, currentFlow);
				carryOverTable.populate(i, TIMECOLUMN, timeHours);
				timeHours += _t_mult;
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "At cur_date=" + cur_date + " computed timeHours=" + timeHours + " after inserting _co_inflow: \n" + carryOverTable);
			}

			// nSimulatedDates includes the current date, so go back
			// ( nSimulatedDates - 1 ) periods
			DateTime date1 = new DateTime(cur_date);
			date1.addInterval(_t_int, -1 * (int)(nSimulatedDates - 1) * _t_mult);
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "date1=" + date1 + " cur_date=" + cur_date);
			}

			// Get the remainder of the values from the calculated inflow time
			// series for this reach.  Move back by nIntervals time steps and save
			// values from that time through the current time.
			// Fill in the remaining values with calculated inflow time series,
			// using getTotalInflow to the get time step inflow for the date.
			for (i = requiredCO - nSimulatedDates ; i < requiredCO; i++)
			{
	//            double currentFlow = _owner->getTotalInflow( date1 ) ;
				double currentFlow = totalInflows.getDataValue(date1);
				date1.addInterval(_t_int, _t_mult);
				carryOverTable.populate(i, FLOWCOLUMN, currentFlow);
				carryOverTable.populate(i, TIMECOLUMN, timeHours);
				timeHours += _t_mult;
			}
			if (Message.isDebugOn)
			{
				Message.printDebug(1, routine, "At " + cur_date + " after adding current flow at end of table: \n" + carryOverTable);
			}
		}

		internal virtual double solveAtlantaK(double LagdQin1, double LagdQin2, double carryover)
		{
			double dx = 0.0; // Delta X
			double dxovr4 = 0.0; // Delta X / 4
			double fact = 1.0; // Current factor, either 1.0 or 0.25
			int i = 0; // Counter variable
			int ipwarn = 0; // >0 when K < 1/2 dt
			int j = 0; // Counter variable
			double s2Odt = 0.0; // Storage term
			double value = 0.0; // Right hand side of routing equation
			double x1 = 0.0; // Initial inflow (post lag)
			double x14 = 0.0; // Initial inflow for current quarter
			double x2 = 0.0; // Final inflow
			double xita = 0.0; // Time step
			double xitao2 = 0.0; // Time step/2
			double xk1 = 0.0; // K for first half segment
			double xk14 = 0.0; // K for first half of current quarter segment
			double xk2 = 0.0; // K for first half segment
			double xk24 = 0.0; // K for first half of current quarter segment
			double y1 = 0.0; // Initial outflow
			double y2 = 0.0; // Solved outflow

			// Constant K is handled by providing this routine a short K table.
			xita = _t_mult / 4.0;

	//        y1 = _owner->_outflow ;              // Carryover value
			y1 = carryover;
			s2Odt = _storageCO * 2.0 / _t_mult; // Carryover Value

			x1 = LagdQin1;
			x2 = LagdQin2;

			// value is rhs_value
			value = x1 + x2 + s2Odt - y1;
			if (value < 1.0e-7)
			{
				value = 0.0;
			}

			// Determine the initial outflow value
			y2 = _stor_out_tbl.lookup(value, STOR_OUTFLOWCOLUMN, true);

			// Do a K lookup for left and right halves
			xk1 = _out_k_tbl.lookup(y1, KCOLUMN, true);
			xk2 = _out_k_tbl.lookup(y2, KCOLUMN, true);

			logger.finest(string.Format("x1={0:F2},x2={1:F2},value={2:F2},y2={3:F2},xk1={4:F2},xk2={5:F2},y1={6:F2}", x1, x2, value, y2, xk1, xk2, y1));

			// Determine whether left and right K values are both within
			// timestep/2, are both above timestep/2, or one is inside and
			// one is outside.  In the last case, split the region into
			// quarters and calculate again.
			if ((xk1 < xita / 2.0) && (xk2 < xita / 2.0))
			{
				// get here if k for y1 and k for y2 are both < dt/2
				// set outflow = minimum of (inflow,value)
				y2 = x2;
				if (value < y2)
				{
					y2 = value;
				}
	//            goto Line9 ;
			}
			else if ((xk1 >= xita * 2.0) && (xk2 >= xita * 2.0))
			{
	//            goto Line9 ;
			}
			else if ((xk1 > xita / 2.0) || (xk2 > xita / 2.0))
			{
				// Do this code if K for y1 is greater than dt/2 OR K for y2 <= dt/2
				// solve equations in this loop with dt=(original dt)/4
				s2Odt = s2Odt * 4.0;
				if (s2Odt < -0.5)
				{
					ipwarn++;
				}
				dx = x2 - x1;

				dxovr4 = dx / 4.0;
				xitao2 = xita / 2.0;

				// Fortran loop called "DO 5"
				// This loop calculates across 1/4 time steps, performing the
				// same K lookups as the non-quarter time algorithm does.
				for (j = 0 ; j < 4 ; j++)
				{
					x14 = x1 + (j * dxovr4);
					x2 = x14 + dxovr4;

					value = x14 + x2 + s2Odt - y1;

					y2 = _stor_out_tbl4.lookup(value, STOR_OUTFLOWCOLUMN, true);

					// Look up K at y1 and y2
					if (variableK)
					{
						xk14 = _out_k_tbl.lookup(y1, KCOLUMN, true);
						xk24 = _out_k_tbl.lookup(y2, KCOLUMN, true);
					}

					if (xk14 < xitao2 || xk24 < xitao2)
					{
						// if either k for y1 or k for y2 is still < new dt/2
						// (i.e. original dt/8) set outflow = min (inflow, value)
						// and continue in quarter period loop.
						y2 = x2;
						if (value < y2)
						{
							y2 = value;
						}
					}

					s2Odt = value - y2;
					if (s2Odt < -0.5)
					{
						ipwarn++;
					}


	//                PrintDebug( 10, routine,  "j, x14, x2, value, y2, s2Odt -- "
	//                    "%d, %.2lf, %.2lf, %.2lf, %.2lf, %.2lf\n",
	//                    j, x14, x2, value, y2, s2Odt ) ;
					y1 = y2;
				}

				fact = 4.0;
			}

	//        Line9:
	//            _owner->_outflow = y2 ;
			s2Odt = (value - y2) / fact;
			if (s2Odt < -0.5)
			{
				ipwarn++;
			}
			fact = 1.0;

			y1 = y2;

			// If it is not in a combo method, it is OK to set members for carryover.
			// @verify
	//            if ( !group_val ) {
			_storageCO = s2Odt * _t_mult / 2.0;
			_laggedInflow = x2;
	//            }

			return y2;
		}

		internal class MCP2KState
		{
			internal virtual void set(double x1, double x2, double y0, double y1, double y2, double xta)
			{
				this.x1 = x1;
				this.x2 = x2;
				this.y0 = y0;
				this.y1 = y1;
				this.y2 = y2;
				this.xta = xta;
			}
			internal bool qdt = false;
			internal double x1, x2, y0, y1, y2, xta;
		}

		internal virtual double solveMCP2K(double LagdQin1, double LagdQin2, double carryover)
		{
	//        boolean qdt = false ;                // Quarter flag
			double qprev = carryover; // last outflow for Fort Worth calc
			double x1 = LagdQin1; // lagged inflow (t-1)
			double x2 = LagdQin2; // lagged inflow (t)
			double xta = _t_mult / 2.0; // Time step for K operation
			double y1 = carryover; // last outflow
			double y0 = _storageCO; // last storage
			double y12 = 0.0; // Average across current segment
			double y2 = 0; // Attenuated outflow

			// perform attenuation on inflow time series.
			// actually route at one half original interval - but only store
			// outflow at whole intervals.

			double x12 = (x1 + x2) / 2.0;
			double y01 = (y0 + y1) / 2.0;

			MCP2KState state = new MCP2KState();
			if (variableK)
			{

				// Compute once for each half routing interval with variable k.
				// If k < (half interval)/4.0 - call CalculateQuarterDT which routes
				// at one quarter of current routine interval - i.e. at one eighth
				// of whole (original) routing interval.
				state.set(x1, x12, y01, y1, y12, xta);
	//            ComputeSegmentRouting( x1, x12, y01, y1, y12, xta, qdt ) ;
				ComputeSegmentRouting(state);

				if (state.qdt)
				{
					state.set(x1, x12, y01, y1, y12, xta);
					CalculateQuarterDT(state);
	//                CalculateQuarterDT( x1, x12, y01, y1, y12, xta, qdt ) ;
				}

				state.set(x12, x2, y1, y12, y2, xta);
				ComputeSegmentRouting(state);
	//            ComputeSegmentRouting( x12, x2, y1, y12, y2, xta, qdt ) ;

				if (state.qdt)
				{
					state.set(x12, x2, y1, y12, y2, xta);
					CalculateQuarterDT(state);
	//                CalculateQuarterDT( x12, x2, y1, y12, y2, xta, qdt ) ;
				}
			}
			else
			{
	//            Line20:
				// Compute once for each half routing interval with constant k.
				state.set(x1, x12, y01, y1, y12, xta);
				ComputeSegmentRouting(state);
	//            ComputeSegmentRouting( x1, x12, y01, y1, y12, xta, qdt ) ;
				state.set(x12, x2, y1, y12, y2, xta);
				ComputeSegmentRouting(state);
	//            ComputeSegmentRouting( x12, x2, y1, y12, y2, xta, qdt ) ;
			}


	//        Line90:
			// If a transmission loss coefficient exists, update y2.
			if (_transLossCoef > 0.0)
			{
				y2 = CalculateFortWorthLoss(x2, qprev, y2);
			}

			// Store carryover values for k operation.
			// @verify
	//            if ( !group_val ) {
			_storageCO = y1;
			_laggedInflow = x2;
	//            }

			return y2;
		}

		internal virtual void ComputeSegmentRouting(MCP2KState state)
		{
			double ye = ((3.0 * state.y1) - state.y0) / 2.0;
			int knt = 0;

			// Maximum 21 iterations to get solution
			for (knt = 0 ; knt <= 20 ; knt++)
			{
				double xk = _out_k_tbl.lookup(ye, KCOLUMN, true);

				// If constant k - i.e. if conk = true - and k lt routing
				// interval/4.0 - do not attenuate inflow.
	//            Line10:
				if (!((xk > state.xta / 4.0) || (variableK)))
				{
					state.y2 = state.x2;
					break;
				}

				// If qdt true - i.e. if calling ComputeSegmentRouting from
				// CalculateQuarterDT, already quartered the routing interval -
				// so no more can be done.  Do not attenuate inflow.
				//
				// If qdt is false - i.e. if calling ComputeSegmentRouting from fk -
				// set qdt = true which will cause CalculateQuarterDT to be called.
				if (xk <= (state.xta / 4.0))
				{
					if (state.qdt)
					{
						state.y2 = state.x2;
					}
					state.qdt = true;
					break;
				}

				// Store outflow in y2 and check convergence with ye.
	//                Line20:
				double tr = 0.5 * state.xta;
				state.y2 = (tr * (state.x1 + state.x2) + state.y1 * (xk - tr)) / (xk + tr);
				if (((1.02 * ye) > state.y2) && ((0.98 * ye) < state.y2))
				{
					break;
				}
				ye = state.y2;
	//                    PrintDebug( 10, routine,
	//                        "KNT+1,TR,X1,X2,Y1,Y2 = %d,%lf,%lf,%lf,%lf,%lf\n",
	//                        knt+1, tr, x1, x2, y1, y2 );
			}

			if (knt >= 20)
			{
				logger.warning("RESJ-ComputeSegmentRouting - Did Not Converge .");
			}
		}

		internal virtual void CalculateQuarterDT(MCP2KState state)
		{
			int i;
			double x1t = 0;
			double y0t = 0;
			double xtat = state.xta / 4.0;
			double x2t = state.x1;
			double y1t = state.y0 * 0.25 + state.y1 * 0.75;
			double y2t = state.y1;

			// Loop through routing interval one quarter of the interval at a time.
			// If k is still greater than routing interval do not attenuate this
			// interval.
			MCP2KState quarter = new MCP2KState();
			for (i = 1 ; i <= 4 ; i++)
			{
				double frac = i / 4.0;
				x1t = x2t;
				x2t = state.x1 * (1.0 - frac) + (state.x2 * frac);
				y0t = y1t;
				y1t = y2t;

				quarter.set(x1t, x2t, y0t, y1t, y2t, xtat);
				ComputeSegmentRouting(quarter);
	//            ComputeSegmentRouting( x1t, x2t, y0t, y1t, y2t, xtat, qdt ) ;
			}

			state.y2 = y2t;
			state.qdt = false;
		}

		internal virtual double CalculateFortWorthLoss(double qli, double qprev, double qk)
		{
			double qtl = -999.0;
			double q = qk;

	//        PrintDebug( 10, routine,
	//            "Enter CalculateFortWorthLoss tlrc=%lf, qbntl=%lf, qli=%lf, "
	//            "qprev=%lf, qk=%lf\n", _transLossCoef, _transLossLevel, qli,
	//            qprev, qk ) ;

			if (qli < qprev)
			{
				qtl = qprev * _transLossCoef;
				if (!((qtl >= qk) || (qtl <= _transLossLevel)))
				{
					q = qtl;
				}
			}

	//        PrintDebug(10, routine, "Exit CalculateFortWorthLoss q=%lf, qtl=%lf\n",
	//            q, qtl);
			return q;
		}

		public virtual string getStateString()
		{
			return string.Format("laggedInflow {0:F}, storageCO {1:F}, inflowCO {2}", _laggedInflow,_storageCO, Arrays.ToString(_co_inflow));
		}

		/// <summary>
		/// Interpolate from table to get value. </summary>
		/// <returns> interpolated value </returns>
		/// <param name="value"> position in the array tsTable to interpolate </param>
		/// <param name="tsTable"> table of 2 values that contains the value </param>
		internal static double Interpolate(double value, Table sTable)
		{
			double ratio = (sTable.lookup(1, FLOWCOLUMN) - sTable.lookup(0, FLOWCOLUMN)) / (sTable.lookup(1, TIMECOLUMN) - sTable.lookup(0, TIMECOLUMN));

			return sTable.lookup(0, FLOWCOLUMN) + ratio * (value - sTable.lookup(0, TIMECOLUMN));
		}


	}

}