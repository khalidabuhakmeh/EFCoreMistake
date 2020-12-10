using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCoreMistake.Models;
using EFCoreMistake.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreMistake.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ILogger<CompaniesController> logger;
        private readonly Database database;

        public CompaniesController(
            ILogger<CompaniesController> logger,
            Database database
        )
        {
            this.logger = logger;
            this.database = database;
        }

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


        [HttpGet]
        [Route("projection-anon")]
        public async Task<IEnumerable<object>> Projection()
        {
            // We're using anonymous object projections
            // to build our response object. Since we
            // are defining the LINQ query, there is a finite
            // end to our results.
            return await database
                .Companies
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    Employees = c
                        .Employees
                        .Select(e => new { e.Id, e.Name})
                        .ToList()
                }).ToListAsync();
        }
        
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
    }
}