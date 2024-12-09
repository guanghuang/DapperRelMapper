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

        [Fact]
        public void DistinctChildren_SingleParent_ShouldRemoveDuplicates()
        {
            // Arrange
            var parent = new Parent
            {
                Id = 1,
                Name = "Parent 1",
                Children = new List<Child>
                {
                    new() { Id = 1, ParentId = 1, Name = "Child 1" },
                    new() { Id = 1, ParentId = 1, Name = "Child 1" }, // Duplicate
                    new() { Id = 2, ParentId = 1, Name = "Child 2" }
                }
            };

            // Act
            parent.DistinctChildren(p => p.Children, c => c.Id);

            // Assert
            Assert.Equal(2, parent.Children.Count);
            Assert.Contains(parent.Children, c => c.Id == 1);
            Assert.Contains(parent.Children, c => c.Id == 2);
        }

        [Fact]
        public void DistinctChildren_SingleParent_WithNullChildren_ShouldNotThrow()
        {
            // Arrange
            var parent = new Parent
            {
                Id = 1,
                Name = "Parent 1",
                Children = null!
            };

            // Act & Assert
            var exception = Record.Exception(() => parent.DistinctChildren(p => p.Children, c => c.Id));
            Assert.Null(exception);
        }

        [Fact]
        public void DistinctChildren_MultipleParents_ShouldRemoveDuplicatesForEachParent()
        {
            // Arrange
            var parents = new List<Parent>
            {
                new()
                {
                    Id = 1,
                    Name = "Parent 1",
                    Children = new List<Child>
                    {
                        new() { Id = 1, ParentId = 1, Name = "Child 1" },
                        new() { Id = 1, ParentId = 1, Name = "Child 1" }, // Duplicate
                        new() { Id = 2, ParentId = 1, Name = "Child 2" }
                    }
                },
                new()
                {
                    Id = 2,
                    Name = "Parent 2",
                    Children = new List<Child>
                    {
                        new() { Id = 3, ParentId = 2, Name = "Child 3" },
                        new() { Id = 3, ParentId = 2, Name = "Child 3" }, // Duplicate
                        new() { Id = 4, ParentId = 2, Name = "Child 4" }
                    }
                }
            };

            // Act
            parents.DistinctChildren(p => p.Children, c => c.Id);

            // Assert
            Assert.All(parents, parent => Assert.Equal(2, parent.Children.Count));
            Assert.Contains(parents[0].Children, c => c.Id == 1);
            Assert.Contains(parents[0].Children, c => c.Id == 2);
            Assert.Contains(parents[1].Children, c => c.Id == 3);
            Assert.Contains(parents[1].Children, c => c.Id == 4);
        }

        [Fact]
        public void DistinctChildren_MultipleParents_WithNullParents_ShouldNotThrow()
        {
            // Arrange
            List<Parent>? parents = null;

            // Act & Assert
            var exception = Record.Exception(() => parents.DistinctChildren(p => p.Children, c => c.Id));
            Assert.Null(exception);
        }

        [Fact]
        public void DistinctChildren_MultipleParents_WithNullChildren_ShouldNotThrow()
        {
            // Arrange
            var parents = new List<Parent>
            {
                new()
                {
                    Id = 1,
                    Name = "Parent 1",
                    Children = null!
                }
            };

            // Act & Assert
            var exception = Record.Exception(() => parents.DistinctChildren(p => p.Children, c => c.Id));
            Assert.Null(exception);
        }

        [Fact]
        public void DistinctChildren_MultipleParents_WithEmptyList_ShouldNotModify()
        {
            // Arrange
            var parents = new List<Parent>();

            // Act
            var result = parents.DistinctChildren(p => p.Children, c => c.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DistinctChildren_SingleParent_WithComplexKey_ShouldRemoveDuplicates()
        {
            // Arrange
            var parent = new Parent
            {
                Id = 1,
                Name = "Parent 1",
                Children = new List<Child>
                {
                    new() { Id = 1, ParentId = 1, Name = "Child A" },
                    new() { Id = 1, ParentId = 1, Name = "Child A" }, // Duplicate by composite key
                    new() { Id = 1, ParentId = 2, Name = "Child B" }  // Different ParentId
                }
            };

            // Act
            parent.DistinctChildren(p => p.Children, c => new { c.Id, c.ParentId });

            // Assert
            Assert.Equal(2, parent.Children.Count);
            Assert.Contains(parent.Children, c => c.ParentId == 1);
            Assert.Contains(parent.Children, c => c.ParentId == 2);
        }

        [Fact]
        public void DistinctChildren_MultipleParents_WithIEnumerableInput_ShouldWork()
        {
            // Arrange
            IEnumerable<Parent> parents = new List<Parent>
            {
                new()
                {
                    Id = 1,
                    Children = new List<Child>
                    {
                        new() { Id = 1, ParentId = 1 },
                        new() { Id = 1, ParentId = 1 } // Duplicate
                    }
                }
            };

            // Act
            var result = parents.DistinctChildren(p => p.Children, c => c.Id);

            // Assert
            var parentsList = result.ToList();
            Assert.Single(parentsList);
            Assert.Single(parentsList[0].Children);
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

        private class Parent
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public List<Child> Children { get; set; } = new();
        }

        private class Child
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
} 