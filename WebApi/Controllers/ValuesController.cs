using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [ResponseType(typeof(TResult<int>))]
        public IHttpActionResult Get(int id)
        {
            return Ok(new TResult<int>() { data = id });
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    public class TResult<T>
    {
        public TResult()
        {
            code = 0;
            msg = "成功";
        }

        public int code { get; set; }
        public string msg { get; set; }
        public T data { get; set; }
    }
}
