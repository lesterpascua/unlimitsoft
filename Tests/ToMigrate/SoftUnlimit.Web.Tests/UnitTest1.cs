using SoftUnlimit.Web.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace SoftUnlimit.Web.Tests
{
    public class DefaultApiClientTests
    {
        [Fact]
        public async Task Access_To_500_Rest_Throws_Exception()
        {
            using var httpClient = new HttpClient();
            using var apiClient = new DefaultApiClient(httpClient, "http://httpstat.us");
            using var client = new TestRestErrorCodeService(apiClient);

            await Assert.ThrowsAsync<HttpException>(() => client.Get500Error());
        }
    }

    public class TestRestErrorCodeService : BaseApiService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiClient"></param>
        public TestRestErrorCodeService(IApiClient apiClient)
            : base(apiClient)
        {
        }

        public Task<(Response<object>, HttpStatusCode)> Get500Error() => ApiClient.SendAsync<Response<object>>(HttpMethod.Get, "500");

        public Task<(Response<object>, HttpStatusCode)> CreateAsync(AccountCreate vm) => ApiClient.SendAsync<Response<object>>(HttpMethod.Post, "api/v1/account", model: vm);
    }

    /// <summary>
    /// Create an account
    /// </summary>
    public class AccountCreate
    {
        /// <summary>
        /// User name for the account
        /// <h3>Validation Rules:</h3>
        /// <ul>
        ///     <li>NotEmpty: true (code 2)</li>
        ///     <li>MinimumLength: 8 characters (code 3)</li>
        ///     <li>MaximumLength: 25 characters (code 4)</li>
        /// </ul>
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Password for the account
        /// <h3>Validation Rules:</h3>
        /// <ul>
        ///     <li>Mandatory (code 2)</li>
        ///     <li>MinimumLength: 8 characters (code 3)</li>
        ///     <li>MaximumLength: 128 characters (code 4)</li>
        /// </ul>
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Frirst name of the user.
        /// <h3>Validation Rules:</h3>
        /// <ul>
        ///     <li>Mandatory (code 2)</li>
        ///     <li>MinimumLength: 1 characters (code 3)</li>
        ///     <li>MaximumLength: 40 characters (code 4)</li>
        /// </ul>
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Middle name of the user.
        /// <h3>Validation Rules:</h3>
        /// <ul>
        ///     <li>MinimumLength: 1 characters (code 3)</li>
        ///     <li>MaximumLength: 40 characters (code 4)</li>
        /// </ul>
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Last name of the user.
        /// <h3>Validation Rules:</h3>
        /// <ul>
        ///     <li>Mandatory (code 2)</li>
        ///     <li>MinimumLength: 2 characters (code 3)</li>
        ///     <li>MaximumLength: 40 characters (code 4)</li>
        /// </ul>
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Email of the user.
        /// <h3>Validation Rules:</h3>
        /// <ul>
        ///     <li>Mandatory (code 2)</li>
        ///     <li>MinimumLength: 2 characters (code 3)</li>
        ///     <li>MaximumLength: 254 characters (code 4)</li>
        ///     <li>Email can't be duplicate in the system (code 20002)</li>
        ///     <li>Valid email (code 5)</li>
        /// </ul>
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Phone of the user. Should be in international standard format.
        /// <h3>Validation Rules:</h3>
        /// <ul>
        ///     <li>Mandatory (code 2)</li>
        ///     <li>MinimumLength: 6 characters (code 3)</li>
        ///     <li>MaximumLength: 14 characters (code 4)</li>
        ///     <li>Phone can't be duplicate in the system (code 20005)</li>
        ///     <li>Valid phone (code 1)</li>
        /// </ul>
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// If true send a sms code to the user with the Phone validation code.
        /// </summary>
        public bool ValidatePhone { get; set; }
        /// <summary>
        /// if true send and email to the user with the email validation code.
        /// </summary>
        public bool ValidateEmail { get; set; }
        /// <summary>
        /// Indicathe to force user login and return bearer token.
        /// </summary>
        public bool AutoLogin { get; set; }
    }
}