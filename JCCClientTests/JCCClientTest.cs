using EFT;
using JCCClient.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PRINTECPLUGIN;
using PRINTECPLUGIN.Data;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JCCClientTests
{
    [TestClass]
    public  class JCCClientTest
    {
        public IEFTPlugin _plugins = null;
        public IOptions<ConfigInfo> Config;
        public GenericRequest request;
        public GenericResponse response;
        public JCCClientTest()
        {
            
            request = new GenericRequest()
            {
                   PayLoad=new PayLoad()
                   {
                        CashierID="Cash001",
                         CurrencyCode="978",
                          SessionId="sess001",
                   }
            };
            response = new GenericResponse()
            {
                Payload = request.PayLoad,
                 Result=new Result() { Status="Error"}
            };

           
        }
        
        
        [TestMethod]
        public void GetInfo_ShouldReturnPluginInfoIfExist()
        {
            ConfigureWithoutBindMethod();

            try
            {
                _plugins = PluginLoadContext.ReadExtensions(Config);

            }
            catch (Exception e)
            {

            }
            ConfigureWithoutBindMethod();
            var infoPlugin = _plugins.GetInfo();
            Assert.IsNotNull(infoPlugin);
        }

        [TestMethod]
        public void InitMethod_ReturnError()
        {
            ConfigureWithoutBindMethod();

            try
            {
                _plugins = PluginLoadContext.ReadExtensions(Config);

            }
            catch (Exception e)
            {

            }

            request.OperationType = "init";
            var init =_plugins.Init(request,response);
            
            Assert.AreEqual(init.Result.Status,"Success");


            //Mock<IEFTPlugin> mock1 = new Mock<IEFTPlugin>();


            //var tes=mock1.Setup(x => x.Init(request, response)).Returns(response);
            //var t = tes;
        }
       
        public void ConfigureWithoutBindMethod()
        {
            var collection = new ServiceCollection();
            collection.AddOptions();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName)
                .AddJsonFile("appsettings.test.json", optional: false)
                .Build();

            collection.Configure<ConfigInfo>(config.GetSection("ConfigInfo"));

            var services = collection.BuildServiceProvider();

            var options = services.GetService<IOptions<ConfigInfo>>();
            Config = options;
        }
    
}
}
