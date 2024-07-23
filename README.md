# Dapper.Extension

A lightweight extension for Dapper that provides additional functionality and simplified database operations.

## Features
Querying data with relationships using Dapper can be challenging, as it often requires extensive boilerplate code to map the data to related objects. This library is designed to simplify the process, making it easier to query data with relationships.


## Installation

You can install the package via NuGet Package Manager:

```bash
dotnet add package Kvr.Dapper.Mapper
```

## Usage

Here's a quick example of how to use Kvr.Dapper.Mapper:

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

var users = await connection.ConfigMapper<Customer, int>(customer => customer.Id, customer => customer.Orders, customer => customer.Address, customer => customer.PhoneNumbers).QueryAsync("SELECT * FROM Customer left join Order on Customer.Id = Order.CustomerId left join CustomerAddress on Customer.Id = CustomerAddress.CustomerId left join PhoneNumber on Customer.Id = PhoneNumber.CustomerId where Customer.Id = @customerId", new { customerId = 1 }, splitOn: "Id,OrderId,AddressId,PhoneNumberId");
```

## Supported Frameworks

- .NET Standard 2.0+
- .NET 5.0+
- .NET 6.0+
- .NET 7.0+

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

- Dapper (>= 2.1.28)

## Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.

