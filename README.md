# DapperRelMapper

Copyright © 2024 Kvr.DapperRelMapper. All rights reserved.

A lightweight extension for Dapper that provides additional functionality and simplified database querying with relationships.

## Table of Content
- [Features](#features)
- [Installation](#installation)
- [Quick Start Guide](#quick-start-guide)
    - [Entity Setup](#entity-setup)
    - [Traditional Approach](#traditional-approach)
    - [Using DapperRelMapper](#using-dapperrelMapper)
    - [Using SplitOn](#using-spliton)
- [Usage](#usage)
    - [Configuring Mappers](#configuring-mappers)
    - [Using SplitOn Method](#using-spliton-method)
    - [Query Execution](#query-execution)
    - [Callback Support](#callback-support)
    - [SplitOnBuilder](#splitonbuilder)
    - [SqlMultipleQuery](#sqlmultiplequery)
- [Best Practices](#best-practices)
- [Limitation](#limitation)
- [Troubleshooting](#troubleshooting)
    - [Common Issues](#common-issues)
- [Supported Frameworks](#supported-frameworks)
- [Version History](#version-history)
- [License](#license)
- [Contributing](#contributing)
- [Dependencies](#dependencies)
- [Support](#support)
- [Build Status](#build-status)

## Features
- Simple fluent API for querying data with relationships
- Type-safe property selection
- Support for custom split fields
- Callback support for post-processing
- Minimal boilerplate code
- Built on top of Dapper's performance

## Installation

You can install the package via NuGet Package Manager:

```bash
dotnet add package Kvr.DapperRelMapper
```

## Quick Start Guide

Here's a quick example of how to use Kvr.DapperRelMapper to query data with relationships:

```csharp
using Kvr.Dapper;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    // One-to-Many relationship
    public ICollection<Order> Orders { get; set; }
    
    // One-to-One relationship
    public CustomerAddress Address { get; set; }
    
    // One-to-Many relationship
    public ICollection<PhoneNumber> PhoneNumbers { get; set; }
}

public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }    
    public int CustomerId { get; set; }
}

public class CustomerAddress
{
    public int AddressId { get; set; }
    public int CustomerId { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
}

public class PhoneNumber
{
    public int PhoneNumberId { get; set; }
    public int CustomerId { get; set; }
    public string Number { get; set; }
    public string Type { get; set; }
}
```
Old way to get the customer with orders, address and phone numbers with boilerplate code:
```csharp
public class CustomerRepository
{
    private readonly IDbConnection _db;

    public async Task<Customer> GetCustomerWithAllDataAsync(int customerId)
    {
        const string sql = @"
            SELECT 
                c.*,
                o.OrderId, o.OrderDate, o.TotalAmount,
                ca.AddressId, ca.Street, ca.City, ca.Country, ca.PostalCode,
                pn.PhoneNumberId, pn.Number, pn.Type
            FROM Customers c
            LEFT JOIN Orders o ON c.Id = o.CustomerId
            LEFT JOIN CustomerAddresses ca ON c.Id = ca.CustomerId
            LEFT JOIN PhoneNumbers pn ON c.Id = pn.CustomerId
            WHERE c.Id = @CustomerId;";

        var customerDictionary = new Dictionary<int, Customer>();

        await _db.QueryAsync<Customer, Order, CustomerAddress, PhoneNumber, Customer>(
            sql,
            (customer, order, address, phone) =>
            {
                if (!customerDictionary.TryGetValue(customer.Id, out var customerEntry))
                {
                    customerEntry = customer;
                    customerEntry.Orders = new List<Order>();
                    customerEntry.PhoneNumbers = new List<PhoneNumber>();
                    customerDictionary.Add(customer.Id, customerEntry);
                }

                if (order != null && !customerEntry.Orders.Any(o => o.OrderId == order.OrderId))
                    customerEntry.Orders.Add(order);

                if (address != null && customerEntry.Address == null)
                    customerEntry.Address = address;

                if (phone != null && !customerEntry.PhoneNumbers.Any(p => p.PhoneNumberId == phone.PhoneNumberId))
                    customerEntry.PhoneNumbers.Add(phone);

                return customerEntry;
            },
            new { CustomerId = customerId },
            splitOn: "OrderId,AddressId,PhoneNumberId"
        );

        return customerDictionary.Values.FirstOrDefault();
    }
}
```
New way to get the customer with orders, address and phone numbers with Dapper.Extension:
```csharp
var users = 
public class CustomerRepository
{
    private readonly IDbConnection _db;

    public async Task<Customer> GetCustomerWithAllDataAsync(int customerId)
    {
        const string sql = @"
            SELECT 
                c.*,
                o.OrderId, o.OrderDate, o.TotalAmount,
                ca.AddressId, ca.Street, ca.City, ca.Country, ca.PostalCode,
                pn.PhoneNumberId, pn.Number, pn.Type
            FROM Customers c
            LEFT JOIN Orders o ON c.Id = o.CustomerId
            LEFT JOIN CustomerAddresses ca ON c.Id = ca.CustomerId
            LEFT JOIN PhoneNumbers pn ON c.Id = pn.CustomerId
            WHERE c.Id = @CustomerId;";

        return (await connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address,
            customer => customer.PhoneNumbers).QueryAsync(sql, new { customerId = 1 }, splitOn: "Id,OrderId,AddressId,PhoneNumberId")).FirstOrDefault();
    }
}
```
further more,you can use the `SplitOn` method generate `splitOn` parameter instead of writing them manually to reduce the risk of typos   :
```csharp
await connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).SplitOn((Order order) => order.OrderId, (CustomerAddress address) => address.AddressId, 
    (PhoneNumber phoneNumber) => phoneNumber.PhoneNumberId).QueryAsync(sql, new { customerId = 1 });
```
or you could use typed splitOn:
```csharp
await connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).SplitOn<Order>(order => order.OrderId).SplitOn<CustomerAddress>(address => address.AddressId)
    .SplitOn<PhoneNumber>(phoneNumber => phoneNumber.PhoneNumberId).QueryAsync(sql, new { customerId = 1 });
```
## Usage
1. Configure the mapping relationship between the parent and child entities using the `ConfigMapper` method.
* ```csharp
   public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey>(
        this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        params LambdaExpression[] expressions)
   ```

  Usage:
```
connection.ConfigMapper<Customer, int>(customer => customer.Id, (Customer customer) => customer.Orders, (Customer customer) => customer.Address, 
    (Customer customer) => customer.PhoneNumbers)
 ```
* The above `ConfigMapper` needs to input qualifier for lamda function. The following `ConfigMapper` will simplify the input (Here assumes that all the lamda function input is the parent entity):
```csharp
public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey>(
        this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        params Expression<Func<TReturn, object>>[] expressions)
```

Usage:
```csharp
connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers)
```
* Here also have 10 strong types of `ConfigMapper` method, which can be used to specify the relationship between the parent and child entities.
    * `public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild>(
        this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector) where TFirstChild : class;`
    * `public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild>(
        this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector) where TFirstChild : class where TSecondChild : class;`
    * ....

2. Use the `SplitOn` method to specify the splitOn parameters for the query.(Optional)
   We could use the `SplitOn` method to generate the `splitOn` parameter instead of writing them manually to reduce the risk of typos.
```csharp
connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).SplitOn((Order order) => order.OrderId, (CustomerAddress address) => address.AddressId, 
    (PhoneNumber phoneNumber) => phoneNumber.PhoneNumberId)
```
or you could use typed splitOn:
```csharp
connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).SplitOn<Order>(order => order.OrderId).SplitOn<CustomerAddress>(address => address.AddressId)
    .SplitOn<PhoneNumber>(phoneNumber => phoneNumber.PhoneNumberId)
```
It will generate the `splitOn` parameter like this: `splitOn: "Id,OrderId,AddressId,PhoneNumberId"` passed to the Dapper's `QueryAsync` method.
Note: The `SplitOn` methods will override the `splitOn` parameter in the `QueryAsync` method.

3. Use the `QueryAsync` method to execute the query and retrieve the data.
   Use the same method parameters as Dapper's `QueryAsync` method except the `mapper` parameter, `splitOn` parameter if you use the `SplitOn` method.
```
await connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).QueryAsync(sql, new { customerId = 1 }, splitOn: "Id,OrderId,AddressId,PhoneNumberId");
```
Here also have 10 strong types of `QueryAsync` method, which can be used to specify the relationship between the parent and child entities.
* `public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild>? callbackAfterMapRow = null)`
* `public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild>? callbackAfterMapRow = null)`
* ....

4. use optional `callbackAfterMapRow` Action parmeter on `QueryAsync` method to do some post-processing after the mapping is complete.
```csharp
connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).QueryAsync(sql, new { customerId = 1 }, callbackAfterMapRow: (object[] objects) => {
        // first object is the parent entity, the rest are the child entities
    });
```
or you could use 10 strong types of `QueryAsync` method, which can be used to specify the parent and child entities on the callback method.
```csharp
connection.ConfigMapper<Customer, int, Order, CustomerAddress, PhoneNumber>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).QueryAsync<Order, CustomerAddress, PhoneNumber, Customer>(sql, new { customerId = 1 }, callbackAfterMapRow: (Customer customer, Order order, CustomerAddress address, PhoneNumber phoneNumber) => {
        // do something after mapping
    });
``` 
5. `SplitOnBuilder` to generate the `splitOn` parameter
```csharp
var splitOn = SplitOnBuilder.Create().SplitOn<Order>(order => order.OrderId).SplitOn<CustomerAddress>(address => address.AddressId)
    .SplitOn<PhoneNumber>(phoneNumber => phoneNumber.PhoneNumberId).Build();
```
6. `SqlMultipleQuery` to query multiple tables at same time and map them to the parent and child entities
```csharp
var customers = connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, 
    customer => customer.PhoneNumbers).QueryMultiple(sql, new { customerId = 1 });
```
  * The `SqlMultipleQuery` methods will use `Dapper`'s `QueryMultiple` method to query multiple tables at same time. 
  * It will return a parent object which contains the parent entity and child entities.
  * The query will accept the key of the parent entity as the parameter.
  * Assume the each child query will use the same parameter, the parameter will be passed to each child query.
  * This could fix the issue returned duplicate child entities if the query includes two more than two one-to-many relationships.

## Best Practices
- Use strongly-typed mappers when possible
- Leverage SplitOn for better control over field mapping
- Keep SQL queries optimized
- Use appropriate indexes on join columns
- Consider using callbacks for complex post-processing
- Use transactions for data consistency
- For generate sql script, you could use the `SqlBuilder` from [SqlBuilder](https://github.com/guanghuang/SqlBuilder) project which is a lightweight strong typed sql script generator.

## Limitation
1. The return values of the `QueryAsync` methods are `IEnumerable<TReturn>`, which means that the query result will be a list of the parent entity. Unlike Dapper's `QueryAsync` method could return any type (return type is not limited to the parent entity).
2. Currently only support 2 level of child relationships, if you need more than 2 levels of child relationships, you may use the `callbackAfterMapRow` Action to do some post-processing after the mapping is complete.
3. It may cause duplicate data if it inolvues two more than two one-to-many relationships. Do the distinct processing if needed on `PostProcess` method.
```
// two children
connection.CreateMultipleQueryMapper<Customer, int, Address>().ConfigParent("Select * from Customers where Id = @CustomerId")
    .ConfigChild<Address>("Select * from Addresses where CustomerId = @CustomerId", c => c.Addresses).QueryAsync(new { customerId = 1 });
// three children
connection.CreateMultipleQueryMapper<Customer, int, Address, PhoneNumber>().ConfigParent("Select * from Customers where Id = @CustomerId")
    .ConfigChild<Address>("Select * from Addresses where CustomerId = @CustomerId", c => c.Addresses)
    .ConfigChild<PhoneNumber>("Select * from PhoneNumbers where CustomerId = @CustomerId", c => c.PhoneNumbers).QueryAsync(new { customerId = 1 });
```

## Troubleshooting

### Common Issues
1. **Split Field Not Found**
    - Ensure column names match the split fields
    - Check case sensitivity
    - Verify SQL query includes all required fields

2. **Relationship Not Mapping**
    - Verify property names match
    - Check that foreign keys are properly set up
    - Ensure collections are initialized

3. **Performance Issues**
    - Review SQL query optimization
    - Check database indexes
    - Consider using buffered = false for large datasets

## Supported Frameworks

- .NET Standard 2.0+
- .NET 5.0+
- .NET 6.0+
- .NET 7.0+

## Version History
- 1.2.2
    - Add child entity many to one relationship mapping, not need to use `callbackAfterMapRow` to do the mapping
- 1.2.1
    - Add `callbackAfterMapRow` parameter to the `SqlMultipleQuery` to config parent and child callback after mapping
- 1.2.0
    - Add `SqlMultipleQuery` mappers which could query multiple tables at same time and map them to the parent and child entities
    - Add `SplitOnBuilder` to generate the `splitOn` parameter
    - Add xunit test project
- 1.1.5
    - Add `SqlBuilder` from [SqlBuilder](https://github.com/guanghuang/SqlBuilder) project README.md file. No code change.
- 1.1.4
    - Update `SplitOn` with return type of the expression is `object`, remove the boxing/unboxing unnecessary `SplitOn` methods for primitive type
- 1.1.3
    - Fix `SplitOn` on primitive type boxing/unboxing issue
- 1.1.2
    - Add `repeat` parameter to the `SplitOn` method to specify the number of times the field should be repeated in the `splitOn` parameter, also add `SplitOn` method with `string` parameter if the field name is different from the property name.
- 1.1.1
    - Fix the collection type check issue
- 1.1.0
    - Add `PostProcess` method to do the distinct processing if needed
- 1.0.0
    - Initial release
    - Basic relationship mapping query
    - SplitOn functionality

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Dependencies

- Dapper (>= 2.1.0)

## Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.

## Build Status
![Build and Test](https://github.com/guanghuang/DapperRelMapper/actions/workflows/build.yml/badge.svg)
![Publish to NuGet](https://github.com/guanghuang/DapperRelMapper/actions/workflows/publish.yml/badge.svg)

