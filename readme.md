# ASP.NET Core and EF Core Mistake

## Getting Started

You'll need .NET 5 SDK from https://dot.net

## Issue

Folks want to write as little code as possible, so they attempt to return the EF Core models from an ASP.NET Core endpoint. **Don't do this! EVER!**

```c#
[HttpGet]
[Route("/oops")]
public async Task<List<Company>> Oops()
{
    // this is a mistake, don't do this!
    // The Json serializer will keep following
    // reference properties until it gives up
    // by throwing a JsonException: A possible object cycle was detected
    return await database
        .Companies
        .Include(c => c.Employees)
        .ToListAsync();
}
```

This results in a `JsonException`.

```console
System.Text.Json.JsonException: A possible object cycle was detected. This can either be due to a cycle or if the object depth is larger than the maximum allowed depth of 32. Consider using ReferenceHandler.Preserve on JsonSerializerOptions to support cycles.
   at System.Text.Json.ThrowHelper.ThrowJsonException_SerializerCycleDetected(Int32 maxDepth)
...
```

This occurs because ASP.NET Core is serializing entities still linked to the `DbContext`. EF Core is following the navigation properties that have a cyclical relationship.

## Solution

Use EF Core and Projections to limit the scope of your result.

```c#
[HttpGet]
[Route("projection-anon-wrapper")]
public async Task<object> Anonymous()
{ 
    // still using projection,
    // we'll choose to wrap our model
    // in another anonymous object.
    //
    // this gives us room to grow
    // and evolve our response, unlike
    // returning an array directly
    return new
    {
        Results = await database
            .Companies
            .Select(c => new
            {
                c.Id,
                c.Name,
                Employees = c
                    .Employees
                    .Select(e => new {e.Id, e.Name})
                    .ToList()
            }).ToListAsync()
    };
}
```

Hopefully this helps folks getting started with ASP.NET Core HTTP APIs powered by Entity Framework Core.

## Recommended Solution

Use strongly-typed responses instead. It adds better development time support.

```c#
[HttpGet]
[Route("projection-type-wrapper")]
public async Task<Response<CompanyResponse>> Wrapper()
{
    // still using LINQ projection,
    // but now using strongly-typed models.
    //
    // this allows for reuse and a better understanding
    // of our responses.
    // 
    // By using a Response<T> model, we can add additional
    // metadata as well, like stats (i.e. total count, pages, cursor, etc.)
    var result = await database
        .Companies
        .Select(c => new CompanyResponse
        {
            Id = c.Id,
            Name = c.Name,
            Employees = c
                .Employees
                .Select(e => new EmployeeResponse {
                    Id = e.Id,
                    Name = e.Name
                })
        }).ToListAsync();

    return new Response<CompanyResponse>(result);
}
```