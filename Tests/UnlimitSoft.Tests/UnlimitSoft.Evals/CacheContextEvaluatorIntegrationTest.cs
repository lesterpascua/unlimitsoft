//using System;
//using System.Reflection;
//using UnlimitSoft.Evals.Compiler;
//using UnlimitSoft.Evals.Utility;
//using Xunit;

//namespace UnlimitSoft.Evals.Test
//{
//    public class CacheContextEvaluatorIntegrationTest
//    {
//        public class MyContext
//        {
//            public string Name { get; set; }
//            public string LastName { get; set; }
//        }

//        [Fact]
//        public void Test1()
//        {
//            var functionTable = Scan.Inspect(typeof(CacheContextEvaluatorIntegrationTest).Assembly);

//            var compiler = new Antlr.Antlr4Compiler<MyContext>(typeof(CacheContextEvaluatorIntegrationTest).Module, functionTable);
//            var evaluator = new ILContextEvaluator<MyContext>(compiler, true);

//            var result = evaluator.Eval(null, "2 + 3 + l(p(\"Na.me\"))", new MyContext { Name = "Lester", LastName = "Pastrana" });
//            Assert.Equal(10, result);
//        }

//        [Function("p")]
//        public static string Price(IServiceProvider _, string context, string name)
//        {
//            return name;
//        }
//        [Function("l")]
//        public static double Length(IServiceProvider _, string context, string name)
//        {
//            return name.Length;
//        }
//    }
//}
