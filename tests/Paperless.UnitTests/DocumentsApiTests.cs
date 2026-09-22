using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Paperless.Api.Contracts;
using Paperless.Domain;

namespace Paperless.UnitTests;

// Hosts the real routes, model validation, mapping, DI and business service.
// Only persistence is mocked. These tests do not prove PostgreSQL integration.
public class DocumentsApiTests
{

}
