using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// 
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<TModel> SendAsync<TModel>(HttpMethod method, string uri, object model = null);
        /// <summary>
        /// Upload file (multipart/form-data)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        /// <param name="streams"></param>
        /// <param name="qs"></param>
        /// <returns></returns>
        Task<TModel> UploadAsync<TModel>(HttpMethod method, string uri, string fileName, IEnumerable<Stream> streams, object qs = null);
    }
}
