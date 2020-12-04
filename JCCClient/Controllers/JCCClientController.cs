using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using EFT;
using SharedClasses;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Extensions.Options;
using JCCClient.Data;
using MongoDB.Driver;
using JCCClient.Persistence;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.AccessControl;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog;

namespace JCCClient.Controllers
{
    [Route("api")]
    //[ApiController]
    public class JCCClientController : ControllerBase
    {
        public bool synch = false;
        public IEFTPlugin _plugins = null;
        protected readonly IOptions<ConfigInfo> Config;
        protected readonly Persistence.MongoDatabase MongoDatabase;
        public JCCClientController(IOptions<ConfigInfo> config, Persistence.MongoDatabase mongoDatabase)
        {
            Config = config;
            MongoDatabase = mongoDatabase;
        }



        [HttpPost]
        [Route("genericPost")]
        public IActionResult GenericPost([FromBody] GenericRequest request)
        {
            Log.Debug("BEGUN: Start Post Method");
            IEFTPlugin plugin;
            var response = new GenericResponse()
            {
                Payload = request.PayLoad,
                Info = request.Info,
                Result = new Result()
                {
                    ErrorCode = "",
                    OperationType = request.OperationType,
                }
            };
            //var operation = DictionaryExtensions.DictionaryToObject<Operation>(request.PayLoad.AdditionalInfo);


            //operation.CashierID = request.PayLoad.CashierID;
            //operation.CurrencyCode = request.PayLoad.CurrencyCode;
            //operation.SessionId = request.PayLoad.SessionId;
            //operation.OperationType = request.OperationType;
            try
            {
                _plugins = PluginLoadContext.ReadExtensions(Config);
                plugin = _plugins;
                var infoPlugin = plugin.GetInfo();
                response.Info.PluginInfo = infoPlugin;

                if (!infoPlugin.Capabilities.Any(x => x.ToLower() == request.OperationType.ToLower()))
                {
                    response.Result.Status = "Error";
                    response.Result.ResponseCode = "Operation not supported";
                    Log.Fatal("END: Operation not supported by plugin");
                    return BadRequest(JsonConvert.SerializeObject(response));

                }
                else
                {
                    if (synch)
                    {
                        Log.Debug("END: Post Method");

                        return Post(request, response, plugin);
                    }
                    else
                    {
                        Log.Debug("END: Post Method");

                        return PostAsync(request, response, plugin);

                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal("END: Error ", e);
                return BadRequest(JsonConvert.SerializeObject(response));
            }


        }

        public IActionResult PostAsync(GenericRequest request, GenericResponse response, IEFTPlugin plugin)
        {
            response.Result.Status = "processing";
            new Task(() =>
            {
                MethodInfo theMethod = plugin.GetType().GetMethod(request.OperationType);

                response = (GenericResponse)theMethod.Invoke(plugin, new object[] { request, response });
                MongoDatabase.Update(response.Result);
            }).Start();

          

            Save(response.Result);
            return Ok(JsonConvert.SerializeObject(response));

        }



        [HttpPost]
        [Route("status")]
        public IActionResult PostStatus([FromBody]string id)
        {
            var body = JObject.Parse(id);

            var result = MongoDatabase.Find(body["id"].ToString());
            return Ok(JsonConvert.SerializeObject(result));
        }




        public IActionResult Post(GenericRequest request, GenericResponse response, IEFTPlugin plugin)
        {

            MethodInfo theMethod = plugin.GetType().GetMethod(request.OperationType);

            response=(GenericResponse)theMethod.Invoke(plugin, new object[] { request, response });


            // response = plugin.Request(request, response);

            return Ok(JsonConvert.SerializeObject(response));

        }
        public IActionResult Post1(IGenericRequest request, GenericResponse response, IEFTPlugin plugin)
        {



            MethodInfo theMethod = plugin.GetType().GetMethod(request.OperationType);

            theMethod.Invoke(plugin, new object[] { request, response });

            
            // response = plugin.Request(request, response);

            return Ok(JsonConvert.SerializeObject(response));

        }


        public void Save(Result resultResponse)
        {
            var list = MongoDatabase.Read().ToList();
            var id = 0;
            if (list.Count() > 0)
            {

                id = Int32.Parse(list
                                .OrderByDescending(x => x.OperationID).FirstOrDefault().OperationID) + 1;

            }
            else
            {
                id += 1;
            }
            resultResponse.OperationID = id.ToString();

            MongoDatabase.Create(resultResponse);

        }


    }
}
