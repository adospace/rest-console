using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestConsole.Core;

interface IHttpResponseHandler
{
    Task HandleResponse(HttpResponseMessage message);
}
