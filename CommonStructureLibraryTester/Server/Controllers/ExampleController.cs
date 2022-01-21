using CSL.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL.ClassCreator;
using System.Text;

namespace CommonStructureLibraryTester.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ExampleController : ControllerBase
    {
        [HttpPost]
        public async Task<string> GetTable()
        {
            byte[] buffer = new byte[Math.Min(HttpContext.Request.ContentLength??16*1024*1024, 16*1024*1024)];
            await HttpContext.Request.Body.ReadAsync(buffer,0,buffer.Length,HttpContext.RequestAborted);
            string content = Encoding.UTF8.GetString(buffer);
            return TableDefinition.ParseTabledef(content).GenerateCode(false);
        }
        //public string GetTable([FromBody]string tabledef) => TableDefinition.ParseTabledef(tabledef).GenerateCode(false);
        public string GetFunc([FromBody]string funcdef) => FunctionDefinition.ParseFunctiondef(funcdef).GenerateCode();
    }
}
