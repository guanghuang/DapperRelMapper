using System.Data;
using System.Linq.Expressions;
using Kvr.Dapper;
using Moq;
using Xunit;

namespace Dapper.Extension.Tests
{
    public class SqlMapperExtensionTests
    {
        private readonly Mock<IDbConnection> _mockConnection;

        public SqlMapperExtensionTests()
        {
            _mockConnection = new Mock<IDbConnection>();
        }

        [Fact]
        public void ConfigMapper_WithArrayOfExpressions_ReturnsWrapper()
        {
            // Arrange
            Expression<Func<TestEntity, int>> keySelector = e => e.Id;
            Expression<Func<TestEntity, object>>[] expressions = {
                e => e.Child,
                e => e.Children
            };

            // Act
            var wrapper = _mockConnection.Object.ConfigMapper(keySelector, expressions);

            // Assert
            Assert.NotNull(wrapper);
            Assert.IsType<SqlMapperWrapper<TestEntity, int>>(wrapper);
        }

        [Fact]
        public void ConfigMapper_WithLambdaExpressions_ReturnsWrapper()
        {
            // Arrange
            Expression<Func<TestEntity, int>> keySelector = e => e.Id;
            LambdaExpression[] expressions = {
                (Expression<Func<TestEntity, ChildEntity>>)(e => e.Child),
                (Expression<Func<TestEntity, ICollection<ChildEntity>>>)(e => e.Children)
            };

            // Act
            var wrapper = _mockConnection.Object.ConfigMapper(keySelector, expressions);

            // Assert
            Assert.NotNull(wrapper);
            Assert.IsType<SqlMapperWrapper<TestEntity, int>>(wrapper);
        }

        [Fact]
        public void ConfigMapper_WithSingleChild_ReturnsWrapper()
        {
            // Arrange
            Expression<Func<TestEntity, int>> keySelector = e => e.Id;
            Expression<Func<TestEntity, ChildEntity>> childSelector = e => e.Child;

            // Act
            var wrapper = _mockConnection.Object.ConfigMapper(keySelector, childSelector);

            // Assert
            Assert.NotNull(wrapper);
            Assert.IsType<SqlMapperWrapper<TestEntity, int>>(wrapper);
        }

        [Fact]
        public void ConfigMapper_WithTwoChildren_ReturnsWrapper()
        {
            // Arrange
            Expression<Func<TestEntity, int>> keySelector = e => e.Id;
            Expression<Func<TestEntity, ChildEntity>> firstChildSelector = e => e.Child;
            Expression<Func<TestEntity, SecondChildEntity>> secondChildSelector = e => e.SecondChild;

            // Act
            var wrapper = _mockConnection.Object.ConfigMapper(
                keySelector, 
                firstChildSelector,
                secondChildSelector);

            // Assert
            Assert.NotNull(wrapper);
            Assert.IsType<SqlMapperWrapper<TestEntity, int>>(wrapper);
        }

        [Fact]
        public void ConfigMapper_WithThreeChildren_ReturnsWrapper()
        {
            // Arrange
            Expression<Func<TestEntity, int>> keySelector = e => e.Id;
            Expression<Func<TestEntity, ChildEntity>> firstChildSelector = e => e.Child;
            Expression<Func<TestEntity, SecondChildEntity>> secondChildSelector = e => e.SecondChild;
            Expression<Func<TestEntity, ThirdChildEntity>> thirdChildSelector = e => e.ThirdChild;

            // Act
            var wrapper = _mockConnection.Object.ConfigMapper(
                keySelector, 
                firstChildSelector,
                secondChildSelector,
                thirdChildSelector);

            // Assert
            Assert.NotNull(wrapper);
            Assert.IsType<SqlMapperWrapper<TestEntity, int>>(wrapper);
        }

        [Fact]
        public void ConfigMapper_WithFourChildren_ReturnsWrapper()
        {
            // Arrange
            Expression<Func<TestEntity, int>> keySelector = e => e.Id;
            Expression<Func<TestEntity, ChildEntity>> firstChildSelector = e => e.Child;
            Expression<Func<TestEntity, SecondChildEntity>> secondChildSelector = e => e.SecondChild;
            Expression<Func<TestEntity, ThirdChildEntity>> thirdChildSelector = e => e.ThirdChild;
            Expression<Func<TestEntity, FourthChildEntity>> fourthChildSelector = e => e.FourthChild;

            // Act
            var wrapper = _mockConnection.Object.ConfigMapper(
                keySelector, 
                firstChildSelector,
                secondChildSelector,
                thirdChildSelector,
                fourthChildSelector);

            // Assert
            Assert.NotNull(wrapper);
            Assert.IsType<SqlMapperWrapper<TestEntity, int>>(wrapper);
        }


        private class TestEntity
        {
            public int Id { get; set; }
            public ChildEntity Child { get; set; }
            public ICollection<ChildEntity> Children { get; set; }
            public SecondChildEntity SecondChild { get; set; }
            public ThirdChildEntity ThirdChild { get; set; }
            public FourthChildEntity FourthChild { get; set; }
            public FifthChildEntity FifthChild { get; set; }
        }

        private class ChildEntity
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
        }

        private class SecondChildEntity
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
        }

        private class ThirdChildEntity
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
        }

        private class FourthChildEntity
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
        }

        private class FifthChildEntity
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
        }
    }
} 