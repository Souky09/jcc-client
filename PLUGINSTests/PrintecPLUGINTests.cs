using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver.Core.Operations;
using PRINTECPLUGIN;
using PRINTECPLUGIN.Data;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PRINTECPLUGIN.Tests
{
    [TestClass()]
    public class PrintecPLUGINTests
    {
        public IOptions<ConfigInfo> Config;
        public PrintecPLUGIN Plugin;
        private readonly Formatting Formatting;
        public PrintecPLUGINTests()
        {
            ConfigureWithoutBindMethod();
            Plugin = new PrintecPLUGIN(Config);
            Formatting = new Formatting()
            {
                 Version="1.0.0",
                  
            };
            var l = new List<Field>();
            l.Add(new Field() { Format = "000", Length = "3",Value="cvv2" });
            Formatting.Fields = l;
        }

        [TestMethod]
        public void SerializeRequestErrorLengthField()
        {

            var r = Plugin.MappingRequest(CreateGenericRequest());
            Assert.IsNotNull(r.GetContent(Formatting));
            //Assert.ThrowsException<ArgumentOutOfRangeException>(()=> r.Serialize(Formatting.Fields));
        }

       // [TestMethod]
        public void MappingRequestTestErroLengthField()
        {
           
            var r=Plugin.MappingRequest(CreateGenericRequest());
            Assert.IsNotNull(r);
        }

        public IGenericRequest CreateGenericRequest()
        {
            var list = new Dictionary<string, string>();
            list.Add("amount", "12.12");
            list.Add("accountNumber", "1");
            list.Add("expirationdate", "10-10-2020");
            list.Add("validfrom", "10-10-2020");
            list.Add("cvv2", "203");
            list.Add("contractorRoomNo", "");
            list.Add("startdate", "10-10-2020");
            list.Add("extterminalid", "1");
            list.Add("extmerchantid", "1");
            list.Add("preparereceiptindicator", "1");
            list.Add("cashierid", "1");
            list.Add("returnpanencrypted", "0");
            list.Add("messagecode", "");
            list.Add("supplycvmresult", "");
            list.Add("bocloyaltyitems", "");
            list.Add("checkoutdate", "");
            list.Add("batchnumber", "1");
            list.Add("agreementNumber", "1");
            list.Add("agreementdate", "10-10-2020");
            list.Add("sessionid", "");


            var request = new GenericRequest()
            {
                Info = new Info()
                {
                    Merchant = "merchant1",
                    Password = "password",
                    StoreId = "1",
                    TerminalID = "T001",
                    User = "sss",
                    Version = "1.0.0",
                },
                OperationType = "Payment",
                PayLoad = new PayLoad()
                {
                    CashierID = "cash1",
                    CurrencyCode = "978",
                    AdditionalInfo = list
                }
            };
            return request;
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
            // Assert.IsNotNull(options);
        }
    }
}