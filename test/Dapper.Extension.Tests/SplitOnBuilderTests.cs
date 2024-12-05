using System.Linq.Expressions;
using Kvr.Dapper;
using Xunit;

namespace Dapper.Extension.Tests;

public class SplitOnBuilderTests
{
    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    
    [Fact]
    public void Empty_Builder_Should_Return_Empty_String()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        string result = builder.Build();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Add_Single_SplitOn_Should_Return_Correct_Format()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn("Id");
        string result = builder.Build();

        // Assert
        Assert.Equal("Id", result);
    }

    [Fact]
    public void Add_Multiple_SplitOn_Should_Return_Comma_Separated()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn("Id")
              .SplitOn("Name")
              .SplitOn("Email");
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,Name,Email", result);
    }

    [Fact]
    public void Add_Null_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.SplitOn(null!));
    }
    
    [Fact]
    public void Add_With_Different_Cases_Should_Be_Case_Sensitive()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn("Id")
              .SplitOn("ID")
              .SplitOn("id");
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,ID,id", result);
    }

    [Theory]
    [InlineData("Table.Id")]
    [InlineData("dbo.Users.Email")]
    [InlineData("Schema.Table.Column")]
    public void Add_Qualified_Names_Should_Be_Valid(string qualifiedName)
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn(qualifiedName);
        string result = builder.Build();

        // Assert
        Assert.Equal(qualifiedName, result);
    }
    
        [Fact]
    public void SplitOn_With_Single_Expression_Should_Return_MemberName()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();
        Expression<Func<TestModel, object>> expression = m => m.Id;

        // Act
        builder.SplitOn(expression);
        string result = builder.Build();

        // Assert
        Assert.Equal("Id", result);
    }

    [Fact]
    public void SplitOn_With_Multiple_Expressions_Should_Return_CommaSeparated()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();
        Expression<Func<TestModel, object>> exp1 = m => m.Id;
        Expression<Func<TestModel, object>> exp2 = m => m.Name;
        Expression<Func<TestModel, object>> exp3 = m => m.Email;

        // Act
        builder.SplitOn(exp1, exp2, exp3);
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,Name,Email", result);
    }

    [Fact]
    public void SplitOn_Generic_With_Single_Expression_Should_Return_MemberName()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn<TestModel>(m => m.Id);
        string result = builder.Build();

        // Assert
        Assert.Equal("Id", result);
    }

    [Fact]
    public void SplitOn_Generic_With_Repeat_Should_Return_Repeated_MemberName()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn<TestModel>(m => m.Id, 3);
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,Id,Id", result);
    }

    [Fact]
    public void SplitOn_Generic_Multiple_Calls_Should_Return_CommaSeparated()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn<TestModel>(m => m.Id)
               .SplitOn<TestModel>(m => m.Name)
               .SplitOn<TestModel>(m => m.Email);
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,Name,Email", result);
    }

    [Fact]
    public void SplitOn_String_With_Single_Value_Should_Return_Value()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn("Id");
        string result = builder.Build();

        // Assert
        Assert.Equal("Id", result);
    }

    [Fact]
    public void SplitOn_String_With_Repeat_Should_Return_Repeated_Value()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn("Id", 3);
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,Id,Id", result);
    }

    [Fact]
    public void SplitOn_String_Multiple_Calls_Should_Return_CommaSeparated()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act
        builder.SplitOn("Id")
               .SplitOn("Name")
               .SplitOn("Email");
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,Name,Email", result);
    }

    [Fact]
    public void SplitOn_Mixed_Calls_Should_Return_CommaSeparated()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();
        Expression<Func<TestModel, object>> exp = m => m.Id;

        // Act
        builder.SplitOn(exp)
               .SplitOn<TestModel>(m => m.Name)
               .SplitOn("Email", 2);
        string result = builder.Build();

        // Assert
        Assert.Equal("Id,Name,Email,Email", result);
    }
    
    [Fact]
    public void SplitOn_With_Null_String_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = SplitOnBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.SplitOn(null));
    }
} 