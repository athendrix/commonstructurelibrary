using CSL.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonStructureLibraryTester.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<List<Tests.TestResult>> RunTests()
        {
            List<Tests.TestResult> toReturn = new List<Tests.TestResult>();
            await foreach (Tests.TestResult test in Tests.RunTests())
            {
                toReturn.Add(test);
            }
            return toReturn;
        }
    }
}
