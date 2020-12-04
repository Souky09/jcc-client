using EFT;
using JCCPLUGIN.Data;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using SharedClasses;
using SharedClasses.Persistence;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Formatting = SharedClasses.Formatting;

namespace JCCPLUGIN
{
    public class JCCPlugin : IEFTPlugin
    {
        private readonly IOptions<ConfigInfo> configuration;
        private readonly PluginInfo sectionConfig;
        private readonly SocketClient Socket;
        private IDictionary<string, string> Map = new Dictionary<string, string>();
        private readonly Formatting Formatting;


        public JCCPlugin(IOptions<ConfigInfo> config)
        {
            configuration = config;
            sectionConfig = configuration.Value.PluginsInfo.FirstOrDefault(x => x.Name.ToLower().Trim().Contains("jcc"));
            Socket = new SocketClient();
            Map.Add("payment", "00");
            Map.Add("refund", "02");
            Map.Add("void", "04");
            Map.Add("reversal", "05");
            Map.Add("Settlment", "10");
            Map.Add("Print Ticket", "30");


            var file = "";
            try
            {
                file = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Template", string.Concat(sectionConfig.Name, "Template.json")));
                Formatting = JsonConvert.DeserializeObject<Formatting>(file);


            }
            catch (Exception e)
            {
                Log.Fatal("Cannot found File Json");
            }


        }

        public PluginInfo GetInfo()
        {
            return sectionConfig;
        }

        public GenericResponse Init(GenericRequest request, GenericResponse response)
        {
            IPAddress IP;
            var success = "Error";
            bool result = false;
            try
            {

                result = Socket.Connect(configuration.Value.EFT_IPAddress, 11111);
                Socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("error:", e);
            }
            if (result)
            {
                success = "success";
            }
            response.Result.Status = success;
            return response;
        }

        public GenericResponse Payment(IGenericRequest gRequest, GenericResponse response)
        {
            var request = MappingRequest(gRequest);
            var status = "Error";
            var stringResponse = "";
            var res = new JCCResponse();
            var content = request.GetContent(Formatting);  
            if (DoRequest(content))
            {
                stringResponse = Socket.Receive();
                if (stringResponse != null)
                {
                    var r = Helper.ResponseSplitedBySeparator(stringResponse, Convert.ToChar(Convert.ToUInt32(Formatting.Fields.FirstOrDefault().Value, 16)));
                    res = ParseEFTTransactionResponse(r);
                    response = BuildPaymentResponse(response, stringResponse);
                    status = "success";
                }
            }
            response.Result.Status = status;
            return response;
            
        }

        private JCCResponse ParseEFTTransactionResponse(string[] msg)
        {
            var result = new JCCResponse();
            var sepDecimal = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            result.Length = Helper.TryParse<int>(msg[0], 6);
            result.SessionId= Helper.TryParse<string>(msg[1], 12);
            result.AuthType= Helper.TryParse<string>(msg[2], 2);
            result.FinalAmount= Helper.TryParse<string>(msg[3], 12);
            result.AdditionalAmount= Helper.TryParse<string>(msg[4], 12);
            result.BasicCurrencyCode= Helper.TryParse<string>(msg[5], 3);
            result.ResponseCode= Helper.TryParse<string>(msg[6], 2);
            result.AuthCode= Helper.TryParse<string>(msg[7], 6);
            result.RRNFinancialAuthorization= Helper.TryParse<string>(msg[8], 12);
            result.PanNo= Helper.TryParse<string>(msg[9], 20);
            result.ExpiryDate= Helper.TryParse<DateTime>(msg[10], 4,"yyMM");
            result.TokenNo= Helper.TryParse<string>(msg[11], 300);
            result.EncryptedPan= Helper.TryParse<string>(msg[12], 300);
            result.DateAndDatime= Helper.TryParse<DateTime>(msg[13], 12, "ddMMyyHHmmss");
            result.HostResponse= Helper.TryParse<string>(msg[14], 3);
            result.CardProduct= Helper.TryParse<string>(msg[15], 30);
            result.ReceiptNo= Helper.TryParse<string>(msg[16], 4);
            result.BatchNo= Helper.TryParse<string>(msg[17], 6);
            result.TerminalNo= Helper.TryParse<string>(msg[18], 12);
            result.CVMResults= Helper.TryParse<string>(msg[19], 2);
            result.EntryMode= Helper.TryParse<string>(msg[20], 4);
            result.POSVersion= Helper.TryParse<string>(msg[21], 12);
            result.DataToBePrinted= Helper.TryParse<string>(msg[22], 3000);
            result.DCCAmount= Helper.TryParse<decimal>(msg[23], 12);
            result.DCCCurrency= Helper.TryParse<string>(msg[24], 3);
            result.DCCExchangeRateMarkUp= Helper.TryParse<decimal>(msg[25], 12);
            result.DCCMarkUpPercentage= Helper.TryParse<decimal>(msg[26], 8);
            result.NOInstalments= Helper.TryParse<int>(msg[27], 2);
            result.DaysForFirstInstalments= Helper.TryParse<int>(msg[28], 2);
            result.LoyaltyPointsBalance= Helper.TryParse<decimal>(msg[29], 12)/100;
            result.LoyaltyPointsRedeemed= Helper.TryParse<decimal>(msg[30], 12) / 100;
            result.GiftBalance= Helper.TryParse<string>(msg[31], 12);
            result.GiftReceipt= Helper.TryParse<string>(msg[32], 4);
            result.BOCLoyaltyItems= Helper.TryParse<string>(msg[33], 2000);
            result.MerchantName= Helper.TryParse<string>(msg[34], 24);
            result.Address= Helper.TryParse<string>(msg[35], 30);
            result.PostalCode= Helper.TryParse<string>(msg[36], 10);
            result.VATRegNo= Helper.TryParse<string>(msg[37], 9);
            result.MCC= Helper.TryParse<string>(msg[38], 4);
        }

        public Request MappingRequest(IGenericRequest gRequest)
        {
            var request = new Request()
            {
                CashierID = gRequest.PayLoad.CashierID,
            CurrencyCode = gRequest.PayLoad.CurrencyCode,
            SessionId = gRequest.PayLoad.SessionId,
            AuthNo = MapOpreation(gRequest.OperationType),
            TerminalId = gRequest.Info.TerminalID

            };
            var mapper = new Mapper<Request>()
            .ConstructUsing(() => request)
            .Map(x => x.AgreementDate, "agreementdate", p => string.IsNullOrEmpty(((string)p)) ? null : (DateTime?)DateTime.ParseExact((string)p, "MM-dd-yyyy", null))
            .Map(x => x.BOCLoyaltyItems, "bocloyaltyitems", p => (string)p)
            .Map(x => x.CheckOutDate, "checkoutdate", p => string.IsNullOrEmpty(((string)p)) ? null : (DateTime?)DateTime.ParseExact((string)p, "MM-dd-yyyy", null))
            .Map(x => x.GiftTransactionType, "gifttransactiontype", p => (string)p)
            .Map(x => x.Amount, "amount", p => string.IsNullOrEmpty(((string)p)) ? null : (decimal?)decimal.Parse((string)p))
            .Map(x => x.AdditionalAmount, "additionalamount", p => string.IsNullOrEmpty(((string)p)) ? null : (decimal?)decimal.Parse((string)p))
            .Map(x => x.AuthNo, "authno", p => (string)p)
            .Map(x => x.ContractNoForCarRental, "contractnoforcarrental", p => (string)p)
            .Map(x => x.DCCAlertSupport, "dccalertsupport", p => (string)p)
            .Map(x => x.LoyaltyIdentification, "loyaltyidentification", p => (string)p)
            .Map(x => x.LoyaltyRequestType, "loyaltyrequesttype", p => (string)p)
            .Map(x => x.OriginalGiftReceiptNo, "origgiftreferenceno", p => (string)p)
            .Map(x => x.OriginalLoyaltyReceiptNumber, "origloyaltyreferencenumber", p => (string)p)
            .Map(x => x.OriginalReceiptNumber, "origrefernumber", p => (string)p)
            .Map(x => x.PrepareReceiptIndicator, "preparereceiptindicator", p => (string)p)
            .Map(x => x.PrintingContext, "receiptcontent", p => (string)p)
            .Map(x => x.RoomNo, "contractorroomno", p => (string)p)
            .Map(x => x.TokenNo, "tokenno", p => (string)p)
            ;
            
            var r = mapper.MapFrom(gRequest.PayLoad.AdditionalInfo);

            return r;

        }


        public GenericResponse BuildPaymentResponse(GenericResponse response, string responseString)
        {
            var resObj = new JCCResponse();
            string[] valuesArray;
            valuesArray = responseString.Split((char)0x1C);
            resObj.Length = Int32.Parse(valuesArray[0]);
            resObj.SessionId = valuesArray[1];
            resObj.AuthType = valuesArray[2];
            resObj.FinalAmount = double.Parse(valuesArray[3]);
            resObj.AdditionalAmount = double.Parse(valuesArray[4]);
            resObj.BasicCurrencyCode = valuesArray[5];
            resObj.ResponseCode = valuesArray[6];
            resObj.AuthCode = valuesArray[7];
            resObj.RRNFinancialAuthorization = valuesArray[8];
            resObj.PanNo = valuesArray[9];
            resObj.ExpiryDate = DateTime.Parse(valuesArray[10]);
            resObj.TokenNo = valuesArray[11];
            resObj.EncryptedPan = valuesArray[12];
            resObj.DateAndDatime = DateTime.Parse(valuesArray[13]);
            resObj.HostResponse = valuesArray[14];
            resObj.CardProduct = valuesArray[15];
            resObj.ReceiptNo = valuesArray[16];
            resObj.BatchNo = valuesArray[17];
            resObj.TerminalNo = valuesArray[18];
            resObj.CVMResults = valuesArray[19];
            resObj.EntryMode = valuesArray[20];
            resObj.POSVersion = valuesArray[21];
            resObj.DataToBePrinted = valuesArray[22];
            resObj.DCCAmount = decimal.Parse(valuesArray[23]);
            resObj.DCCCurrency = valuesArray[24];
            resObj.DCCExchangeRateMarkUp = valuesArray[25];
            resObj.DCCMarkUpPercentage = valuesArray[26];
            resObj.NOInstalments = Int32.Parse(valuesArray[27]);
            resObj.DaysForFirstInstalments = Int32.Parse(valuesArray[28]);
            resObj.LoyaltyPointsRedeemed = decimal.Parse(valuesArray[29]);
            resObj.LoyaltyPointsBalance = decimal.Parse(valuesArray[30]);
            resObj.GiftBalance = valuesArray[31];
            resObj.GiftReceipt = valuesArray[32];
            resObj.BOCLoyaltyItems = valuesArray[33];

            response.Result.ResponseCode = resObj.ResponseCode;
            return response;
        }
        public bool DoRequest(string content)
        {
            var result = false;
            try
            {
                result = Socket.Send(content, configuration.Value.EFT_IPAddress, 44388);

                //responseString = Socket.SocketStart(configuration.Value.EFT_IPAddress, 44388, content);
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
            return result;

        }
        public string BuildPaymentRequestString(IGenericRequest request)
        {
            var separator = (char)0x1E;
            StringBuilder content = null;
            //content.Append(separator);
            //content.Append(request.SessionId).Append(separator);
            //content.Append(request.TransactionType).Append(separator);
            //content.Append(request.Amount).Append(separator);
            //content.Append(request.AdditionalAmount).Append(separator);
            //content.Append(request.CurrencyCode).Append(separator);
            //content.Append(request.ContractOrRoomNo).Append(separator);
            //content.Append(request.CheckOutDate.Value.ToString("ddMMyyyy")).Append(separator);
            //content.Append(request.ContractNoForCarRental).Append(separator);
            //content.Append(request.AgreementDate.Value.ToString("ddMMyyyy")).Append(separator);
            //content.Append(request.PrepareReceiptIndicator ? "1" : "0").Append(separator);
            //content.Append(request.CashierID).Append(separator);
            //content.Append(request.DCCAlertSupport ? "1" : "0").Append(separator);
            //content.Append(request.BOCLoyaltyItems).Append(separator);
            //content.Append(request.TokenNumber).Append(separator);
            return content.ToString();
        }

        public GenericResponse Ping(GenericRequest request, GenericResponse response)
        {
            var success = "Error";
            var ip = request.PayLoad.AdditionalInfo["ipAddress"];
            try
            {
                if (ip != null)
                {
                    if (PingHelper.Ping(ip).Status == IPStatus.Success)
                        success = "success";
                }
            }
            catch (Exception e)
            {

            }


            response.Result.Status = success;
            return response;
        }

        public GenericResponse PrintReceipt(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }

        public GenericResponse Refund(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }

        public GenericResponse Reversal(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }

        public GenericResponse Settlement(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }

        public GenericResponse Void(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }
        public GenericResponse CloseBatch(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }

        public string BuildPaymentRequestString2(IGenericRequest request)
        {
            var separator = (char)0x1E;
            StringBuilder content = null;
            //content.Append(request.SessionId).Append(separator);
            //content.Append(request.AuthType).Append(separator);
            //content.Append(request.Amount).Append(separator);
            //content.Append(request.AdditionalAmount).Append(separator);
            //content.Append(request.CurrencyCode).Append(separator);
            //content.Append(request.ContractOrRoomNo).Append(separator);
            //content.Append(request.CheckOutDate).Append(separator);
            //content.Append(request.ContractNoForCarRental).Append(separator);
            //content.Append(request.AgreementDate).Append(separator);
            //content.Append(request.PrepareReceiptIndicator ? "1" : "0").Append(separator);
            //content.Append(request.CashierID).Append(separator);
            //content.Append(request.DCCAlertSupport ? "1" : "0").Append(separator);
            //content.Append(request.BOCLoyaltyItems).Append(separator);
            //content.Append(request.ExtTerminalId).Append(separator);
            //content.Append(request.TokenNumber).Append(separator);
            return content.ToString();
        }

        public string MapOpreation(string operation)
        {
            throw new NotImplementedException();
        }

        public GenericResponse PaymentOld(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }

     

        public GenericResponse Init(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }

        public GenericResponse Ping(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
