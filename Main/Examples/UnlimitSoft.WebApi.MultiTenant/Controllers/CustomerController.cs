using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UnlimitSoft.MultiTenant.AspNet;
using UnlimitSoft.WebApi.MultiTenant.Sources.Configuration;
using UnlimitSoft.WebApi.MultiTenant.Sources.Data;
using UnlimitSoft.WebApi.MultiTenant.Sources.Data.Model;

namespace UnlimitSoft.WebApi.EventBus.Controllers;


[ApiController]
[Route("[controller]")]
public sealed class CustomerController : ControllerBase
{
    private readonly Faker _faker;
    private readonly ServiceOptions _options;
    private readonly OtherOptions _otherOptions;
    private readonly IMyUnitOfWork _unitOfWork;
    private readonly IMyRepository<Customer> _customerRepository;
    private readonly ILogger<CustomerController> _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public CustomerController(
        IOptions<ServiceOptions> options, 
        IOptions<OtherOptions> otherOptions, 
        IMyUnitOfWork unitOfWork,
        IMyRepository<Customer> customerRepository,
        IMyQueryRepository<Customer> customerQueryRepository,
        ILogger<CustomerController> logger
    )
    {
        _faker = new Faker();
        _options = options.Value;
        _otherOptions = otherOptions.Value;
        _unitOfWork = unitOfWork;
        _customerRepository = customerRepository;
        _logger = logger;

        _logger.LogInformation("Options: {@Option}", _otherOptions);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] string tenant)
    {
        var customers = await _customerRepository.FindAll().ToArrayAsync();
        await _unitOfWork.SaveChangesAsync();

        return Ok(customers);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> Post([FromQuery] string tenant)
    {
        var t = HttpContext.GetTenant();

        var customer = new Customer
        {
            Id = _faker.Random.Uuid(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
        };
        customer = await _customerRepository.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            Tenant = t,
            Options = _options,
            OtherOptions = _otherOptions,
            Customer = customer
        });
    }
}