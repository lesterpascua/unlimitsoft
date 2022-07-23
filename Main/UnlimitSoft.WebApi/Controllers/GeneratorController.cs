using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;
using System;

namespace UnlimitSoft.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeneratorController : ControllerBase
    {
        private readonly IMyIdGenerator _gen;

        public GeneratorController(IMyIdGenerator gen)
        {
            _gen = gen;
        }

        [HttpGet]
        public Guid Get()
        {
            return _gen.GenerateId();
        }
    }
}
