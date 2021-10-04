using FluentValidation;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Query.Validator
{
    public class DummyQueryValidator : AbstractQueryValidator<DummyQuery>
    {
        public DummyQueryValidator()
        {
            Console.WriteLine();
        }
    }
}
