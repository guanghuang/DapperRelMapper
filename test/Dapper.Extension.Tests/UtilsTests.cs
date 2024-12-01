using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Kvr.Dapper;
using Xunit;

namespace Dapper.Extension.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void GetMemberExpression_WithPropertyExpression_ReturnsMemberExpression()
        {
            // Arrange
            var testObj = new TestClass();
            Expression<Func<TestClass, string>> expression = t => t.StringProperty;

            // Act
            var memberExpression = expression.GetMemberExpression();

            // Assert
            Assert.NotNull(memberExpression);
            Assert.Equal(nameof(TestClass.StringProperty), memberExpression.Member.Name);
        }

        [Fact]
        public void GetMemberExpression_WithNullableProperty_ReturnsMemberExpression()
        {
            // Arrange
            var testObj = new TestClass();
            Expression<Func<TestClass, int?>> expression = t => t.NullableIntProperty;

            // Act
            var memberExpression = expression.GetMemberExpression();

            // Assert
            Assert.NotNull(memberExpression);
            Assert.Equal(nameof(TestClass.NullableIntProperty), memberExpression.Member.Name);
        }

        [Fact]
        public void GetMemberExpression_WithInvalidExpression_ThrowsArgumentException()
        {
            // Arrange
            Expression<Func<TestClass, int>> expression = t => 42;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => expression.GetMemberExpression());
        }

        [Fact]
        public void GetMapType_WithSimpleType_ReturnsType()
        {
            // Arrange
            Expression<Func<TestClass, string>> expression = t => t.StringProperty;
            var memberExpression = expression.GetMemberExpression();

            // Act
            var mapType = memberExpression.GetMapType();

            // Assert
            Assert.Equal(typeof(string), mapType);
        }

        [Fact]
        public void GetMapType_WithCollectionType_ReturnsElementType()
        {
            // Arrange
            Expression<Func<TestClass, ICollection<string>>> expression = t => t.CollectionProperty;
            var memberExpression = expression.GetMemberExpression();

            // Act
            var mapType = memberExpression.GetMapType();

            // Assert
            Assert.Equal(typeof(string), mapType);
        }

        [Theory]
        [InlineData(typeof(List<string>), true)]
        [InlineData(typeof(string[]), true)]
        [InlineData(typeof(ArrayList), true)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(int), false)]
        public void IsCollectionType_WithVariousTypes_ReturnsExpectedResult(Type type, bool expected)
        {
            // Act
            var result = Utils.IsCollectionType(type);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPropertyValue_WithValidProperty_ReturnsValue()
        {
            // Arrange
            var testObj = new TestClass { StringProperty = "test" };
            Expression<Func<TestClass, string>> expression = t => t.StringProperty;

            // Act
            var value = Utils.GetPropertyValue(testObj, expression);

            // Assert
            Assert.Equal("test", value);
        }

        [Fact]
        public void SetPropertyValue_WithValidProperty_SetsValue()
        {
            // Arrange
            var testObj = new TestClass();
            Expression<Func<TestClass, string>> expression = t => t.StringProperty;
            var memberExpression = expression.GetMemberExpression();

            // Act
            Utils.SetPropertyValue(testObj, memberExpression, "test");

            // Assert
            Assert.Equal("test", testObj.StringProperty);
        }

        [Fact]
        public void GetPropertyValue_WithMemberExpression_ReturnsValue()
        {
            // Arrange
            var testObj = new TestClass { StringProperty = "test" };
            Expression<Func<TestClass, string>> expression = t => t.StringProperty;
            var memberExpression = expression.GetMemberExpression();

            // Act
            var value = Utils.GetPropertyValue<TestClass, string>(testObj, memberExpression);

            // Assert
            Assert.Equal("test", value);
        }

        [Fact]
        public void GetPropertyValue_WithNullMemberExpression_ReturnsDefault()
        {
            // Arrange
            var testObj = new TestClass();

            // Act
            var value = Utils.GetPropertyValue<TestClass, string>(testObj, (MemberExpression)null);

            // Assert
            Assert.Null(value);
        }

        private class TestClass
        {
            public string StringProperty { get; set; }
            public int? NullableIntProperty { get; set; }
            public ICollection<string> CollectionProperty { get; set; }
        }
    }
} 