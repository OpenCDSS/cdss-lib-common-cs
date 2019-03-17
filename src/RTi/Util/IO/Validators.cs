using System;
using System.Collections.Generic;
using System.Text;

// Validators - class that returns a Validator based on the method called

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

/// <summary>
///***************************************************************************
/// Validators.java - 2007-03-26
/// ******************************************************************************
/// Revisions
/// 2007-03-21	Ian Schneider, RTi		Initial Version.
/// 2007-03-26	Kurt Tometich, RTi		Added new methods and inner classes
///								for RangeValidation, BlankStringValidation
///								and StringLengthValidator.
/// ****************************************************************************
/// </summary>
namespace RTi.Util.IO
{

	/// <summary>
	/// This class returns a Validator based on the method called.  Each
	/// validator implements the Validator interface.  Once a validator is
	/// returned only the validate() method needs to be called.  A Status
	/// object is returned from running a Validator.  If the object value
	/// is validated then a Status of Status.OK will be returned; otherwise a
	/// Status of the given level will be returned with a status message
	/// describing the reason the value was not valid.  If the level is
	/// not specified then it defaults to Status.ERROR which is the error level.
	/// See the Status class for more info on status levels.
	/// </summary>
	public class Validators
	{

		public Validators()
		{
		}

		/// <summary>
		/// simple validators, which are inner classes * </summary>
		// each method can be called w/ or w/o a level.
		// The level is used as the log level if the validation fails.
		// If a level is not specified it defaults to error.
		public static Validator and(Validator[] rules)
		{
			return and(rules,Status.ERROR);
		}
		public static Validator and(Validator[] rules, int level)
		{
			return new AndValidator(rules,level);
		}
		public static Validator or(Validator[] rules)
		{
			return or(rules,Status.ERROR);
		}
		public static Validator or(Validator[] rules, int level)
		{
			return new OrValidator(rules,level);
		}
		public static Validator lessThan(double value)
		{
			return lessThan(value,Status.ERROR);
		}
		public static Validator lessThan(double value, int level)
		{
			return new LogicalValidator(LogicalValidator.LT,value,level);
		}
		public static Validator greaterThan(double value)
		{
			return greaterThan(value,Status.ERROR);
		}
		public static Validator greaterThan(double value, int level)
		{
			return new LogicalValidator(LogicalValidator.GT,value,level);
		}
		public static Validator isDate(string format)
		{
			return isDateValidator(format, Status.ERROR);
		}
		public static Validator isEquals(object value)
		{
			return isEqualsValidator(value, Status.ERROR);
		}
		public static Validator not(double value)
		{
			return not(value,Status.ERROR);
		}
		public static Validator not(double value, int level)
		{
			return new LogicalValidator(LogicalValidator.NOT,value,level);
		}

		/// <summary>
		/// More complex validators that build on the simple ones * </summary>

		// Regular Expression Validator
		public static Validator regexValidator(string regex)
		{
			return new RegexValidator(Status.ERROR, regex);
		}
		public static Validator regexValidator(string regex, int level)
		{
			return new RegexValidator(level, regex);
		}
		// RangeValidator
		public static Validator rangeValidator(double min, double max)
		{
			return new RangeValidator(Status.ERROR, min, max);
		}
		public static Validator rangeValidator(double min, double max, int level)
		{
			return new RangeValidator(level, min, max);
		}
		// String Length Validator
		public static Validator stringLengthValidator(int maxLen)
		{
			return new StringLengthValidator(maxLen, Status.ERROR);
		}
		public static Validator stringLengthValidator(int maxLen, int level)
		{
			return new StringLengthValidator(maxLen, level);
		}

		// Blank String Validator
		public static Validator notBlankValidator()
		{
			return new NotBlankValidator(Status.ERROR);
		}
		public static Validator notBlankValidator(int level)
		{
			return new NotBlankValidator(level);
		}


		/// <summary>
		/// Checks the given date to see if it is valid
		/// based on a given format. </summary>
		/// <param name="format"> String format to check for. </param>
		/// <param name="level"> Status level to log the Status message as if this
		/// check fails. </param>
		/// <returns> Returns a Validator object. </returns>
		// TODO KAT 2007-04-09
		// Need to update this method to check valid dates from DateTime
		// Might need to add a new data validator to validate specific dates
		// created from the DateTime class.
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static Validator isDateValidator(final String format,final int level)
		public static Validator isDateValidator(string format, int level)
		{
			return new ValidatorAnonymousInnerClass(format, level);
		}

		private class ValidatorAnonymousInnerClass : Validator
		{
			private string format;
			private int level;

			public ValidatorAnonymousInnerClass(string format, int level)
			{
				this.format = format;
				this.level = level;
			}

			internal ParsePosition p = new ParsePosition(0);
			internal DateFormat f = new SimpleDateFormat(format);
		   /// <summary>
		   /// Validates the given object value for a date string format. </summary>
		   /// <param name="value"> The object value to check. </param>
		   /// <returns> Status object.  </returns>
			public Status validate(object value)
			{
				p.setIndex(0);
				Status status = Status.OKAY;
				if (value == null)
				{
					status = Status.status("Date is not set",level);
				}
				else
				{
					try
					{
						f.parse(value.ToString());
					}
					catch (ParseException)
					{
						status = Status.status("Not a valid date",level);
					}
				}
				return status;
			}
			/// <summary>
			/// Returns the validator rules. </summary>
			/// <returns> List of validator rules. </returns>
			public override string ToString()
			{
				return "Value must match date format " + format;
			}
		}

		/// <summary>
		/// Checks whether an object equals another. </summary>
		/// <param name="object"> Object value to check. </param>
		/// <param name="level"> Status level to log the Status message as if this
		/// check fails. </param>
		/// <returns> Returns a validator object. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static Validator isEqualsValidator(final Object object, final int level)
		public static Validator isEqualsValidator(object @object, int level)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object value1;
			object value1;
			if (@object == null)
			{
				throw new System.NullReferenceException("object");
			}
			value1 = @object;
			return new ValidatorAnonymousInnerClass2(@object, level, value1);
		}

		private class ValidatorAnonymousInnerClass2 : Validator
		{
			private object @object;
			private int level;
			private object value1;

			public ValidatorAnonymousInnerClass2(object @object, int level, object value1)
			{
				this.@object = @object;
				this.level = level;
				this.value1 = value1;
			}

					/// <summary>
					/// Validates whether one object is equal to another. </summary>
					/// <param name="value"> Object value to check. </param>
					/// <returns> Status object. </returns>
			public Status validate(object value)
			{
				Status status = Status.OKAY;
				if (value == null)
				{
					status = Status.status("Value is not set",level);
				}
				else
				{
					if (!@object.Equals(value))
					{
						status = Status.status(value.ToString() + " is not equal to " + @object.ToString(),level);
					}
				}
				return status;
			}
			/// <summary>
			/// Returns the validator rules. </summary>
			/// <returns> List of validator rules. </returns>
			public override string ToString()
			{
				string str = "Value must be equal to " + value1.ToString() + "\n";
				return str;
			}
		}

		// Inner classes follow

		/// <summary>
		/// Simple "And" validator that checks whether all values pass
		/// the specified rules.
		/// </summary>
		private sealed class AndValidator : Validator
		{
			internal readonly Validator[] rules;
			internal readonly int level;
			internal AndValidator(Validator[] rules, int level)
			{
				this.rules = rules;
				this.level = level;
			}
			/// <summary>
			/// Validates whether the object value is valid using the
			/// rules specified. </summary>
			/// <param name="value"> The object value to check. </param>
			/// <returns> Status object. </returns>
			public Status validate(object value)
			{
				IList<Status> results = new List<Status>();
				Status status = Status.OKAY;
				for (int i = 0; i < rules.Length; i++)
				{
					status = rules[i].validate(value);
					if (status != Status.OKAY)
					{
						results.Add(status);
					}
				}
				if (results.Count > 0)
				{
					StringBuilder sb = new StringBuilder("The following rules were not valid:\n");
					for (int i = 0; i < results.Count; i++)
					{
						sb.Append(results[i].ToString());
						if (i + 1 < results.Count)
						{
							sb.Append('\n');
						}
					}
					status = Status.status(sb.ToString(),level);
				}
				return status;
			}
			/// <summary>
			/// Overriden method to return the validator and its rules </summary>
			/// <returns> String with validator type and associated rules. </returns>
			public override string ToString()
			{
				string str = "All of the following rules must be valid:\n";
				for (int i = 0; i < rules.Length; i++)
				{
					str = str + " - " + rules[i];
				}
				return str;
			}
		}

		/// <summary>
		/// Simple "Or" validator that checks whether any of the values
		/// pass the rules specified.
		/// </summary>
		private sealed class OrValidator : Validator
		{
			internal readonly Validator[] rules;
			internal readonly int level;
			/// <summary>
			/// Initializes the OrValidator object. </summary>
			/// <param name="rules"> The list of rules to use when checking for
			/// validity. </param>
			/// <param name="Level"> The Status level to use if the value is not valid. </param>
			internal OrValidator(Validator[] rules, int level)
			{
				this.rules = rules;
				this.level = level;
			}
			/// <summary>
			/// Validates whether the rules specified are valid. </summary>
			/// <param name="value"> The object value to check. </param>
			/// <returns> Status object. </returns>
			public Status validate(object value)
			{
				Status[] results = new Status[rules.Length];
				bool ok = false;
				Status status = Status.OKAY;
				for (int i = 0; i < rules.Length && !ok; i++)
				{
					status = rules[i].validate(value);
					ok = status == Status.OKAY;
					results[i] = status;
				}
				if (!ok)
				{
					StringBuilder sb = new StringBuilder("None of the rules were valid:\n");
					for (int i = 0; i < results.Length; i++)
					{
						sb.Append(results[i].ToString());
						if (i + 1 < results.Length)
						{
							sb.Append('\n');
						}
					}
					status = Status.status(sb.ToString(),level);
				}
				return status;
			}
			/// <summary>
			/// Overriden method to return the validator and its rules </summary>
			/// <returns> String with validator type and associated rules. </returns>
			public override string ToString()
			{
				string str = "One of the following rules must be valid:\n";
				for (int i = 0; i < rules.Length; i++)
				{
					str = str + " - " + rules[i];
				}
				return str;
			}
		}

		/// <summary>
		/// Validates simple logical checks.  This is used by several other
		/// validators as a base for checking low level logical operations.
		/// </summary>
		private sealed class LogicalValidator : Validator
		{
			internal readonly int type;
			internal readonly int level;
			internal readonly double value;
			internal const int LT = 1;
			internal static readonly int GT = LT << 1;
			internal static readonly int NOT = GT << 1;
			/// <summary>
			/// Initializes the Logical object. </summary>
			/// <param name="type"> The type of the logical check.  LT for less than, GT for
			/// greater than and NOT for negation. </param>
			/// <param name="value"> The value to validate. </param>
			/// <param name="Level"> The Status level to use if the value is not valid. </param>
			internal LogicalValidator(int type, double value, int level)
			{
				this.type = type;
				this.level = level;
				this.value = value;
			}
			/// <summary>
			/// Validates whether the given object value passes the specified
			/// logical rules.
			/// </summary>
			public Status validate(object value)
			{
				Status status = Status.OKAY;
				Number number = null;
				if (value is Number)
				{
					number = (Number) value;
				}
				else if (value != null)
				{
					string svalue = value.ToString();
					try
					{
						number = Convert.ToDouble(svalue);
					}
					catch (System.FormatException)
					{
						status = Status.status("Could not use " + svalue + " as a number",level);
					}
				}
				else
				{
					status = Status.status("Value is not set",level);
				}
				if (number != null)
				{
					double nval = number.doubleValue();
					string message = null;
					switch (type)
					{
						case LT:
							if (nval >= this.value)
							{
								message = "Value must be less than " + this.value + " but was " + nval;
							}
								break;
						case GT:
							if (nval <= this.value)
							{
								message = "Value must be greater than " + this.value +
							" but was " + nval; break;
							}
							goto case NOT;
						case NOT:
							if (nval >= this.value)
							{
								message = nval +
						" is not " + this.value; break;
							}
						break;
					}
					if (!string.ReferenceEquals(message, null))
					{
						status = Status.status(message,level);
					}
				}
				return status;
			}
			public override string ToString()
			{
				switch (type)
				{
				case LT:
					return "Value must be less than " + this.value;
				case GT:
					return "Value must be greater than " + this.value;
				case NOT:
					return "Value must not equal " + this.value;
				default:
					return "Invalid type: " + type + "." +
				" Valid types are LT(" + LT + "), GT(" + GT + ") and NOT(" +
				NOT + ").";
			break;
				}
			}
		}

		/// <summary>
		/// Validates whether the given value matches a regular expression.
		/// </summary>
	   private sealed class RegexValidator : Validator
	   {
		   internal readonly int level;
		   internal string regex;

		   /// <summary>
		   /// Initializes the RegexValidator object. </summary>
		   /// <param name="Level"> The Status level to use if the value is not valid. </param>
		   /// <param name="Regex"> The regular expression to match the value with. </param>
		   internal RegexValidator(int Level, string Regex)
		   {
			   this.level = Level;
			   this.regex = Regex;
		   }

		   /// <summary>
		   /// Validates the given value is matched by the given regular expression. </summary>
		   /// <returns> Status object. </returns>
		   public Status validate(object value)
		   {
			   Status status = Status.OKAY;
			   if (value == null)
			   {
				   status = Status.status("Value is not set", level);
			   }
			   else if (string.ReferenceEquals(regex, null))
			   {
				   status = Status.status("The regular expression to check" + " was not set", level);
			   }
			   else
			   {
				   if (value is string)
				   {
					   Pattern pattern = Pattern.compile(regex);
					   CharSequence inputStr = value.ToString();
					   Matcher matcher = pattern.matcher(inputStr);
					   bool matchFound = matcher.find();
					if (!matchFound)
					{
						status = Status.status("String " + inputStr + " must match regular expression: " + regex, level);
					}
				   }
				   else
				   {
					   status = Status.status("Value must be a String", level);
				   }
			   }

			   return status;
		   }
		/// <summary>
		/// Overriden method to return the validator and its rules </summary>
		/// <returns> String with validator type and associated rules. </returns>
		public override string ToString()
		{
			string str = "Value must match regular expression: " + regex + "\n";
			return str;
		}
	   }

		/// <summary>
		/// Validates whether the given value is greater than a minimum and
		/// less than a maximum value.  If the value is valid then a status of OKAY
		/// is returned; otherwise, an message is appended to the Status.
		/// </summary>
		private sealed class RangeValidator : Validator
		{
			internal readonly int level;
			internal double min;
			internal double max;

			/// <summary>
			/// Initializes the RangeValidator object. </summary>
			/// <param name="Level"> The Status level to use if the value is not valid. </param>
			/// <param name="Min"> Valid value must be greater than this number. </param>
			/// <param name="Max"> Valid value must be less than this number. </param>
			internal RangeValidator(int Level, double Min, double Max)
			{
				this.level = Level;
				this.min = Min;
				this.max = Max;
			}

			/// <summary>
			/// Validates the given value is in the range of values. </summary>
			/// <returns> Status object. </returns>
			public Status validate(object value)
			{
				Status status = Status.OKAY;
				if (value == null)
				{
					status = Status.status("Value is not set", level);
				}
				// check if greater than the minimum value
				// AND if less than the maximum value
				status = Validators.and(new Validator [] {Validators.greaterThan(this.min, this.level), Validators.lessThan(this.max, this.level)}).validate(value);
				return status;
			}
			 /// <summary>
			 ///  Overriden method to return the validator and its rules </summary>
			 ///  <returns> String with validator type and associated rules. </returns>
			public override string ToString()
			{
				string str = "Value must be greater than " + min +
					" and less than " + max + "\n";
				return str;
			}
		}

		/// <summary>
		/// Validates whether a given object is a blank string.  Useful
		/// for checking ID's or Strings that must not be blank.
		/// </summary>
		private sealed class NotBlankValidator : Validator
		{
			internal readonly int level;
			/// <summary>
			/// Initializes the BlankValidator object. </summary>
			/// <param name="Level"> The Status level to use if the value is not valid. </param>
			internal NotBlankValidator(int Level)
			{
				level = Level;
			}

			/// <summary>
			/// Validates whether the given object value is blank. </summary>
			/// <returns> Status object. </returns>
			public Status validate(object value)
			{
				Status status = Status.OKAY;
				if (value == null)
				{
					status = Status.status("Value is not set", level);
				}
				else if (value is string && value.ToString().Length == 0)
				{
					status = Status.status("Value cannot be blank", level);
				}
				return status;
			}
			 /// <summary>
			 ///  Overriden method to return the validator and its rules </summary>
			 ///  <returns> String with validator type and associated rules. </returns>
			public override string ToString()
			{
				string str = "Value cannot be blank\n";
				return str;
			}
		}

		/// <summary>
		/// Validates the max length of a specified String.
		/// </summary>
		private sealed class StringLengthValidator : Validator
		{
			internal readonly int level;
			internal readonly int maxLen;
			/// <summary>
			/// Initializes the StringLengthValidator object. </summary>
			/// <param name="MaxLen"> The mamixum number of characters the String must
			/// be in order to be considered valid. </param>
			/// <param name="Level"> The Status level to use if the value is not valid. </param>
			internal StringLengthValidator(int MaxLen, int Level)
			{
				level = Level;
				maxLen = MaxLen;
			}

			/// <summary>
			/// Validates whether the given String objects length is less than
			/// or equal to the maximum length. </summary>
			/// <returns> Status object. </returns>
			public Status validate(object value)
			{
				Status status = Status.OKAY;
				if (value == null)
				{
					status = Status.status("Value is not set", level);
				}
				else if (value is string && value.ToString().Length > maxLen)
				{
					status = Status.status("Value length must not exceed " + maxLen, level);
				}
				return status;
			}
			 /// <summary>
			 ///  Overriden method to return the validator and its rules </summary>
			 ///  <returns> String with validator type and associated rules. </returns>
			public override string ToString()
			{
				string str = "Value length must not exceed " + maxLen + "\n";
				return str;
			}
		}


	}

}