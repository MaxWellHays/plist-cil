﻿using Claunia.PropertyList;
using Xunit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace plistcil.test
{
    public class NSNumberTests
    {
        [Fact]
        public static void NSNumberConstructorTest()
        {
            var number = new NSNumber("10032936613", NumberType.Integer);
            Assert.Equal(NumberType.Integer, number.Type);
            Assert.Equal(10032936613, number.ToObject());
        }

        // The tests below make sure the numbers are being parsed correctly, and do not depend on the culture info
        // being set. Especially, decimal point may vary between cultures and we don't want to take a dependency on that
        // The value being used comes seen in a real property list:
        // <key>TimeZoneOffsetFromUTC</key>
        // <real>7200.000000</real>

#if !NETCORE
        [Fact]
        [UseCulture("en-US")]
        public static void ParseNumberEnTest()
        {
            var number = new NSNumber("7200.000001");
            Assert.True(number.IsReal);
            Assert.Equal(7200.000001d, number.ToDouble());
        }

        [Fact]
        [UseCulture("nl-BE")]
        public static void ParseNumberNlTest()
        {
            // As seen in a real property list:
            // <key>TimeZoneOffsetFromUTC</key>
            // <real>7200.000000</real>
            var number = new NSNumber("7200.000001");
            Assert.True(number.IsReal);
            Assert.Equal(7200.000001d, number.ToDouble());
        }

        [Fact]
        [UseCulture("en-US")]
        public static void ParseNumberEnTest2()
        {
            // As seen in a real property list:
            // <key>TimeZoneOffsetFromUTC</key>
            // <real>7200.000000</real>
            var number = new NSNumber("7200.000000", NumberType.Real);
            Assert.True(number.IsReal);
            Assert.Equal(7200d, number.ToDouble());
        }

        [Fact]
        [UseCulture("nl-BE")]
        public static void ParseNumberNlTest2()
        {
            // As seen in a real property list:
            // <key>TimeZoneOffsetFromUTC</key>
            // <real>7200.000000</real>
            var number = new NSNumber("7200.000000", NumberType.Real);
            Assert.True(number.IsReal);
            Assert.Equal(7200d, number.ToDouble());
        }
#endif
    }
}
