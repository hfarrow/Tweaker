using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ghostbit.Tweaker.Core;

namespace Ghostbit.Tweaker.Core.Tests
{
	[TestFixture]
	public class TypeExtensionsTest
	{
		[SetUp]
		public void Init()
		{

		}

		[Test]
		public void IsNumericType()
		{
			// Non-numeric types
			Assert.False(typeof(object).IsNumericType());
			Assert.False(typeof(DBNull).IsNumericType());
			Assert.False(typeof(bool).IsNumericType());
			Assert.False(typeof(char).IsNumericType());
			Assert.False(typeof(DateTime).IsNumericType());
			Assert.False(typeof(string).IsNumericType());

			// Arrays of numeric and non-numeric types
			Assert.False(typeof(object[]).IsNumericType());
			Assert.False(typeof(DBNull[]).IsNumericType());
			Assert.False(typeof(bool[]).IsNumericType());
			Assert.False(typeof(char[]).IsNumericType());
			Assert.False(typeof(DateTime[]).IsNumericType());
			Assert.False(typeof(string[]).IsNumericType());
			Assert.False(typeof(byte[]).IsNumericType());
			Assert.False(typeof(decimal[]).IsNumericType());
			Assert.False(typeof(double[]).IsNumericType());
			Assert.False(typeof(short[]).IsNumericType());
			Assert.False(typeof(int[]).IsNumericType());
			Assert.False(typeof(long[]).IsNumericType());
			Assert.False(typeof(sbyte[]).IsNumericType());
			Assert.False(typeof(float[]).IsNumericType());
			Assert.False(typeof(ushort[]).IsNumericType());
			Assert.False(typeof(uint[]).IsNumericType());
			Assert.False(typeof(ulong[]).IsNumericType());

			// numeric types
			Assert.True(typeof(byte).IsNumericType());
			Assert.True(typeof(decimal).IsNumericType());
			Assert.True(typeof(double).IsNumericType());
			Assert.True(typeof(short).IsNumericType());
			Assert.True(typeof(int).IsNumericType());
			Assert.True(typeof(long).IsNumericType());
			Assert.True(typeof(sbyte).IsNumericType());
			Assert.True(typeof(float).IsNumericType());
			Assert.True(typeof(ushort).IsNumericType());
			Assert.True(typeof(uint).IsNumericType());
			Assert.True(typeof(ulong).IsNumericType());

			// Nullable non-numeric types
			Assert.False(typeof(bool?).IsNumericType());
			Assert.False(typeof(char?).IsNumericType());
			Assert.False(typeof(DateTime?).IsNumericType());

			// Nullable numeric types
			Assert.True(typeof(byte?).IsNumericType());
			Assert.True(typeof(decimal?).IsNumericType());
			Assert.True(typeof(double?).IsNumericType());
			Assert.True(typeof(short?).IsNumericType());
			Assert.True(typeof(int?).IsNumericType());
			Assert.True(typeof(long?).IsNumericType());
			Assert.True(typeof(sbyte?).IsNumericType());
			Assert.True(typeof(float?).IsNumericType());
			Assert.True(typeof(ushort?).IsNumericType());
			Assert.True(typeof(uint?).IsNumericType());
			Assert.True(typeof(ulong?).IsNumericType());

			// Testing with GetType because of handling with non-numerics. See:
			// http://msdn.microsoft.com/en-us/library/ms366789.aspx

			// Using GetType - non-numeric
			Assert.False((new object()).GetType().IsNumericType());
			Assert.False(DBNull.Value.GetType().IsNumericType());
			Assert.False(true.GetType().IsNumericType());
			Assert.False('a'.GetType().IsNumericType());
			Assert.False((new DateTime(2009, 1, 1)).GetType().IsNumericType());
			Assert.False(string.Empty.GetType().IsNumericType());

			// Using GetType - numeric types
			// ReSharper disable RedundantCast
			Assert.True((new byte()).GetType().IsNumericType());
			Assert.True(43.2m.GetType().IsNumericType());
			Assert.True(43.2d.GetType().IsNumericType());
			Assert.True(((short)2).GetType().IsNumericType());
			Assert.True(((int)2).GetType().IsNumericType());
			Assert.True(((long)2).GetType().IsNumericType());
			Assert.True(((sbyte)2).GetType().IsNumericType());
			Assert.True(2f.GetType().IsNumericType());
			Assert.True(((ushort)2).GetType().IsNumericType());
			Assert.True(((uint)2).GetType().IsNumericType());
			Assert.True(((ulong)2).GetType().IsNumericType());
			// ReSharper restore RedundantCast

			// Using GetType - nullable non-numeric types
			bool? nullableBool = true;
			Assert.False((nullableBool.GetType().IsNumericType()));
			char? nullableChar = ' ';
			Assert.False(nullableChar.GetType().IsNumericType());
			DateTime? nullableDateTime = new DateTime(2009, 1, 1);
			Assert.False(nullableDateTime.GetType().IsNumericType());

			// Using GetType - nullable numeric types
			byte? nullableByte = 12;
			Assert.True(nullableByte.GetType().IsNumericType());
			decimal? nullableDecimal = 12.2m;
			Assert.True(nullableDecimal.GetType().IsNumericType());
			double? nullableDouble = 12.32;
			Assert.True(nullableDouble.GetType().IsNumericType());
			short? nullableInt16 = 12;
			Assert.True(nullableInt16.GetType().IsNumericType());
			short? nullableInt32 = 12;
			Assert.True(nullableInt32.GetType().IsNumericType());
			short? nullableInt64 = 12;
			Assert.True(nullableInt64.GetType().IsNumericType());
			sbyte? nullableSByte = 12;
			Assert.True(nullableSByte.GetType().IsNumericType());
			float? nullableSingle = 3.2f;
			Assert.True(nullableSingle.GetType().IsNumericType());
			ushort? nullableUInt16 = 12;
			Assert.True(nullableUInt16.GetType().IsNumericType());
			ushort? nullableUInt32 = 12;
			Assert.True(nullableUInt32.GetType().IsNumericType());
			ushort? nullableUInt64 = 12;
			Assert.True(nullableUInt64.GetType().IsNumericType());
		}
	}
}
