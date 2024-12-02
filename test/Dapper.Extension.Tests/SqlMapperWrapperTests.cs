using System.Data;
using System.Linq.Expressions;
using Kvr.Dapper;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Dapper.Extension.Tests
{
    public class SqlMapperWrapperTests : IDisposable
    {
        private readonly SqliteConnection _connection;

        public SqlMapperWrapperTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            // Create test tables
            _connection.Execute(@"
                CREATE TABLE Parent (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL
                );

                CREATE TABLE Child (
                    ChildId INTEGER PRIMARY KEY,
                    ParentId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    FOREIGN KEY (ParentId) REFERENCES Parent(Id)
                );

                CREATE TABLE SecondChild (
                    SecondChildId INTEGER PRIMARY KEY,
                    ParentId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    FOREIGN KEY (ParentId) REFERENCES Parent(Id)
                );

                -- Insert test data
                INSERT INTO Parent (Id, Name) VALUES (1, 'Parent 1'), (2, 'Parent 2');
                
                INSERT INTO Child (ChildId, ParentId, Name) VALUES 
                    (1, 1, 'Child 1.1'),
                    (2, 1, 'Child 1.2'),
                    (3, 2, 'Child 2.1');

                INSERT INTO SecondChild (SecondChildId, ParentId, Name) VALUES 
                    (1, 1, 'SecondChild 1.1'),
                    (2, 2, 'SecondChild 2.1');
            ");
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        [Fact]
        public async Task QueryAsync_WithParameters_MapsCorrectly()
        {
            // Arrange
            var sql = @"
                SELECT p.*, c.*
                FROM Parent p
                LEFT JOIN Child c ON p.Id = c.ParentId
                WHERE p.Id = @ParentId";

            var wrapper = new SqlMapperWrapper<ParentEntity, int>(_connection, p => p.Id, new LambdaExpression[] 
            {
                (ParentEntity p) => p.Children
            });

            // Act
            var result = (await wrapper.QueryAsync(sql, new { ParentId = 1 }, splitOn: "ChildId")).First();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Parent 1", result.Name);
            Assert.NotNull(result.Children);
            Assert.Equal(2, result.Children.Count);
            Assert.Equal("Child 1.1", result.Children.ElementAt(0).Name);
            Assert.Equal("Child 1.2", result.Children.ElementAt(1).Name);
        }


        [Fact]
        public async Task QueryAsync_WithCallbackAfterMapRow_ExecutesCallback()
        {
            // Arrange
            var sql = @"
                SELECT p.*, c.*
                FROM Parent p
                LEFT JOIN Child c ON p.Id = c.ParentId
                WHERE p.Id = 1";

            var callbackExecuted = false;
            var wrapper = new SqlMapperWrapper<ParentEntity, int>(_connection, p => p.Id, new LambdaExpression[] 
            {
                (ParentEntity p) => p.Children
            });

            // Act
            var result = await wrapper.QueryAsync(sql, callbackAfterMapRow: objects => 
            {
                callbackExecuted = true;
                Assert.Equal(2, objects.Length); // Parent and Child
            }, splitOn: "ChildId");

            // Assert
            Assert.True(callbackExecuted);
        }
        
        [Fact]
        public async Task QueryAsync_WithNullChild_HandlesNullCorrectly()
        {
            // Arrange
            var sql = @"
                SELECT p.*, c.*
                FROM Parent p
                LEFT JOIN Child c ON p.Id = c.ParentId
                WHERE p.Id = 2";  // Parent 2 has only one child

            var wrapper = new SqlMapperWrapper<ParentEntity, int>(_connection, p => p.Id, new LambdaExpression[] 
            {
                (ParentEntity p) => p.Children
            });

            // Act
            var result = (await wrapper.QueryAsync(sql, splitOn: "ChildId")).First();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("Parent 2", result.Name);
            Assert.NotNull(result.Children);
            Assert.Single(result.Children);
            Assert.Equal("Child 2.1", result.Children.First().Name);
        }

        [Fact]
        public async Task QueryAsync_WithTypedCallback_ExecutesCallbackCorrectly()
        {
            // Arrange
            var sql = @"
                SELECT p.*, c.*
                FROM Parent p
                LEFT JOIN Child c ON p.Id = c.ParentId
                WHERE p.Id = 1";

            var callbackExecuted = false;
            var wrapper = new SqlMapperWrapper<ParentEntity, int>(_connection, p => p.Id, new LambdaExpression[] 
            {
                (ParentEntity p) => p.Child
            });

            // Act
            var result = await wrapper.QueryAsync<ChildEntity>(sql, callbackAfterMapRow: (parent, child) =>
            {
                callbackExecuted = true;
                Assert.Equal(parent.Id, child.ParentId);
            }, splitOn: "ChildId");

            // Assert
            Assert.True(callbackExecuted);
        }

        [Fact]
        public async Task QueryAsync_WithSingleChild_MapsCorrectly()
        {
            // Arrange
            var sql = @"
                SELECT p.*, c.*
                FROM Parent p
                LEFT JOIN Child c ON p.Id = c.ParentId
                WHERE p.Id = 1";

            var wrapper = new SqlMapperWrapper<ParentEntity, int>(_connection, p => p.Id, new LambdaExpression[] 
            {
                (ParentEntity p) => p.Children
            });

            // Act
            var result = (await wrapper.QueryAsync(sql, splitOn: "ChildId")).First();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Parent 1", result.Name);
            Assert.NotNull(result.Children);
            Assert.Equal(2, result.Children.Count);
            Assert.Contains(result.Children, c => c.Name == "Child 1.1");
            Assert.Contains(result.Children, c => c.Name == "Child 1.2");
        }

        [Fact]
        public async Task QueryAsync_WithMultipleChildren_MapsCorrectly()
        {
            // Arrange
            var sql = @"
                SELECT p.*, c.*, sc.*
                FROM Parent p
                LEFT JOIN Child c ON p.Id = c.ParentId
                LEFT JOIN SecondChild sc ON p.Id = sc.ParentId
                WHERE p.Id = 1";

            var wrapper = new SqlMapperWrapper<ParentEntity, int>(_connection, p => p.Id, new LambdaExpression[] 
            {
                (ParentEntity p) => p.Children,
                (ParentEntity p) => p.SecondChildren
            }).PostProcess(p => {
                p.Children = p.Children.DistinctBy(c => c.ChildId).ToList();
                p.SecondChildren = p.SecondChildren.DistinctBy(c => c.SecondChildId).ToList();
            });


            // Act
            var result = (await wrapper.QueryAsync(sql, splitOn: "ChildId,SecondChildId")).First();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Parent 1", result.Name);
            Assert.NotNull(result.Children);
            Assert.Equal(2, result.Children.Count);
            Assert.Contains(result.Children, c => c.Name == "Child 1.1");
            Assert.Contains(result.Children, c => c.Name == "Child 1.2");
            Assert.NotNull(result.SecondChildren);
            Assert.Single(result.SecondChildren);
            Assert.Contains(result.SecondChildren, c => c.Name == "SecondChild 1.1");
            Assert.Equal(1, result.SecondChildren.First().ParentId);
        }

        private class ParentEntity
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public ChildEntity? Child { get; set; }
            public ICollection<ChildEntity> Children { get; set; } = new List<ChildEntity>();
            public ICollection<SecondChildEntity> SecondChildren { get; set; } = new List<SecondChildEntity>();
        }

        private class ChildEntity
        {
            public int ChildId { get; set; }
            public int ParentId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class SecondChildEntity
        {
            public int SecondChildId { get; set; }
            public int ParentId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class TestProcResult
        {
            public int Id { get; set; }
            public string Result { get; set; } = string.Empty;
        }
    }
} 