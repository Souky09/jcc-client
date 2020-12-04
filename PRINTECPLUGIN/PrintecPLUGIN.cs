using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Options;
using PRINTECPLUGIN.Data;
using EFT;
using System.Runtime.CompilerServices;
using System.Runtime;
using Newtonsoft.Json.Linq;
using AutoMapper;
using Serilog;
using SharedClasses;
using SharedClasses.Persistence;
using Formatting = SharedClasses.Formatting;
using Newtonsoft.Json;
using System.Globalization;

namespace PRINTECPLUGIN
{

    public class PrintecPLUGIN : IEFTPlugin
    {
        private readonly IOptions<ConfigInfo> configuration;
        private readonly PluginInfo sectionConfig;
        public readonly SocketClient Socket;
        private IDictionary<string, string> map = new Dictionary<string, string>();
        private readonly Formatting Formatting;
        public PrintecPLUGIN(IOptions<ConfigInfo> config)
        {
            configuration = config;
            sectionConfig = configuration.Value.PluginsInfo
                                         .FirstOrDefault(x => x.Name.ToLower().Trim().Contains("printec"));
            Socket = new SocketClient();
            map.Add("payment", "00");
            map.Add("refund", "02");
            map.Add("void", "04");
            map.Add("reversal", "05");
            map.Add("Settlment", "10");
            map.Add("Print Ticket", "30");

            
            var file = "";
            try
            {
                file = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),"Template",string.Concat(sectionConfig.Name,"Template.json")));
                //file = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),"Template", "TemplateFormatting.json"));
               // file = File.ReadAllText(@"C:\Users\Greencore\Projects\NCR\JCC\JCC\Project\ProjectGit\PLUGINS\PRINTECPLUGIN\TemplateFormatting.json");
                Formatting = JsonConvert.DeserializeObject<Formatting>(file);
                

            }
            catch (Exception e)
            {
                Log.Fatal("Cannot found File Json");
            }
            

        }

        
        public string MapOpreation(string operation)
        {
            return map[operation.Trim().ToLower()];
        }

        public PluginInfo GetInfo()
        {
            return sectionConfig;
        }

        public GenericResponse Ping(IGenericRequest request, GenericResponse response)
        {
            
            var success = "Error";
            var ip = request.PayLoad.AdditionalInfo["ipAddress"];
            try
            {
                if (ip != null)
                {
                    if (PingHelper.Ping(ip.ToString()).Status == IPStatus.Success)
                        success = "success";
                }
            }
            catch (PingException e)
            {

            }

            response.Result.Status = success;
            return response;
        }
        public GenericResponse Init(IGenericRequest request, GenericResponse response)
        {
            IPAddress IP;
            var success = "Error";
            bool result = false;
            try
            {

                result = Socket.Connect(configuration.Value.EFT_IPAddress, 11000);

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



        public bool DoRequest(string content)
        {
            var result = false;
            try
            {
                result = Socket.Send(content, configuration.Value.EFT_IPAddress, 11000);

                //responseString = Socket.SocketStart(configuration.Value.EFT_IPAddress, 44388, content);
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
            return result;

        }

        public GenericResponse Payment(IGenericRequest gRequest, GenericResponse response)
        {
            var request =  MappingRequest(gRequest);
            var status = "Error";
            var stringResponse = "";
            var content = "";
           
            try
            {
                 content = request.GetContent(Formatting);  // BuildPaymentRequestString(gRequest).ToString();
                if (DoRequest(content))
                {
                    try
                    {
                        stringResponse = Socket.Receive();

                        if (stringResponse != null)
                        {
                            var r = Helper.ResponseSplitedBySeparator(stringResponse, Convert.ToChar(Convert.ToUInt32(Formatting.Fields.FirstOrDefault().Value, 16)));
                            response = BuildPaymentResponse(response, ParseEFTTransactionResponse(r));
                            status = "success";
                        }
                    }
                    catch(Exception e)
                    {
                        response.Result.ErrorCode = e.Message;
                    }
                    
                }
            }
            catch(ArgumentOutOfRangeException e)
            {
                response.Result.ErrorCode = e.Message;
            }
            
            response.Result.Status = status;
            return response;
        }


        public Request MappingRequest(IGenericRequest gRequest) 
        {
            var request = new Request()
            {
                CashierID = gRequest.PayLoad.CashierID,
                CurrencyCode = gRequest.PayLoad.CurrencyCode,
                SessionId = gRequest.PayLoad.SessionId,
                TransactionType = MapOpreation(gRequest.OperationType)
            };
            var mapper = new Mapper<Request>()
            //.ConstructUsing(() => new Request())
            .ConstructUsing(() => request)
            .Map(x => x.AccountNumber, "accountnumber", p => (string)p)
            .Map(x => x.AgreementDate, "agreementdate", p => string.IsNullOrEmpty(((string)p)) ? null : (DateTime?)DateTime.ParseExact((string)p, "MM-dd-yyyy", null))
            .Map(x => x.AgreementNumber, "agreementnumber", p => (string)p)
            .Map(x => x.AuthCode, "authno", p => (string)p)
            .Map(x => x.BatchNumber, "batchno", p => (string)p)
            .Map(x => x.BOCLoyaltyItems, "bocloyaltyitems", p => (string)p)
            .Map(x => x.CheckOutDate, "checkoutdate", p => string.IsNullOrEmpty(((string)p)) ? null : (DateTime?)DateTime.ParseExact((string)p, "MM-dd-yyyy", null))
            .Map(x => x.ContractOrRoomNo, "contractorroomno", p => (string)p)
            .Map(x => x.CVV2, "cvv2", p => (string)p)
            .Map(x => x.ExpirationDate, "expirationdate", p => string.IsNullOrEmpty(((string)p)) ? null : (DateTime?)DateTime.ParseExact((string)p, "MM-dd-yyyy", null))
            .Map(x => x.ExtMerchantId, "extmerchantid", p => (string)p)
            .Map(x => x.ExtReferenceNumber, "extreferencenumber", p => (string)p)
            .Map(x => x.ExtTerminalId, "extterminalid", p => (string)p)
            .Map(x => x.GenericLoyaltyBins, "genericloyaltybins", p => (string)p)
            .Map(x => x.GenericLoyaltyFlag, "genericloyaltyflag", p => (string)p)
            .Map(x => x.GiftTransactionType, "gifttransactiontype", p => (string)p)
            .Map(x => x.Language, "language", p => (string)p)
            .Map(x => x.LoyaltyIdentificationMethod, "loyaltyidentification", p => (string)p)
            .Map(x => x.LoyaltyTransactionType, "loyaltyrequesttype", p => (string)p)
            .Map(x => x.MessageCode, "messagecode", p => (string)p)
            .Map(x => x.MessageType, "messagetype", p => (string)p)
            .Map(x => x.OrigGiftReferenceNumber, "origgiftreferenceno", p => (string)p)
            .Map(x => x.OrigLoyaltyReferenceNumber, "origloyaltyreferencenumber", p => (string)p)
            .Map(x => x.OrigReferNumber, "origrefernumber", p => (string)p)
            .Map(x => x.OrigTransactionAmount, "amount", p =>string.IsNullOrEmpty(((string)p)) ? null : (decimal?)decimal.Parse((string)p))
            //.Map(x => x.OrigTransactionAmount, "amount", p =>(string)p)
            .Map(x => x.Password, "password", p => (string)p)
            .Map(x => x.PrepareReceiptTicket, "preparereceiptindicator", p => ((string)p))
            .Map(x => x.ReceiptContent, "receiptcontent", p => (string)p)
            .Map(x => x.ReturnPANEncrypted, "returnpanencrypted", p => (string)p)
            .Map(x => x.StartDate, "startdate", p => string.IsNullOrEmpty(((string)p)) ? null : (DateTime?)DateTime.ParseExact((string)p, "MM-dd-yyyy", null))
            .Map(x => x.SupplyCVMResult, "supplycvmresult", p => (string)p)
            .Map(x => x.TransactionType, "authtype", p => (string)p)
            .Map(x => x.ValidFrom, "validfrom", p => string.IsNullOrEmpty(((string)p)) ? null : (DateTime?)DateTime.ParseExact((string)p, "MM-dd-yyyy", null))
            .Map(x => x.CashBackAmount, "cachbackamount", p => string.IsNullOrEmpty(((string)p)) ? null : (decimal?)decimal.Parse((string)p))
            .Map(x=>x.EACAccountNumber, "eacaccountnumber",p=>(string)p)
            .Map(x=>x.EACPhoneNumber,"eacphonenumber",p=>(string)p)
            .Map(x=>x.EACPaymentFlag,"eacpaymentflag",p=>(string)p)
            ;
            var result =mapper.MapFrom(gRequest.PayLoad.AdditionalInfo);
            return result;

        }



        public GenericResponse PaymentOld(IGenericRequest request, GenericResponse response)
        {
            var status = "Error";
            var stringResponse = "";

            var content = "";//BuildPaymentRequestString(request).ToString();
            if (DoRequest(content))
            {
                stringResponse = Socket.Receive();
                if (stringResponse != null)
                {
                    //response = BuildPaymentResponse(response, stringResponse);
                    status = "success";
                }
            }
            response.Result.Status = status;
            return response;

        }
        public GenericResponse Refund(IGenericRequest gRequest, GenericResponse response)
        {
            var request = MappingRequest(gRequest);
            var status = "Error";
            var stringResponse = "";

            var content = request.GetContent(Formatting);  // BuildPaymentRequestString(gRequest).ToString();
            if (DoRequest(content))
            {
                stringResponse = Socket.Receive();
                if (stringResponse != null)
                {
                    //response = BuildPaymentResponse(response, stringResponse);
                    status = "success";
                }
            }
            response.Result.Status = status;
            return response;
        }

        public GenericResponse Void(IGenericRequest gRequest, GenericResponse response)
        {
            var request = MappingRequest(gRequest);
            var status = "Error";
            var stringResponse = "";

            var content = request.GetContent(Formatting);  // BuildPaymentRequestString(gRequest).ToString();
            if (DoRequest(content))
            {
                stringResponse = Socket.Receive();
                if (stringResponse != null)
                {
                    //response = BuildPaymentResponse(response, stringResponse);
                    status = "success";
                }
            }
            response.Result.Status = status;
            return response;
        }

        public GenericResponse Reversal(IGenericRequest gRequest, GenericResponse response)
        {
            var request = MappingRequest(gRequest);
            var status = "Error";
            var stringResponse = "";

            var content = request.GetContent(Formatting);  // BuildPaymentRequestString(gRequest).ToString();
            if (DoRequest(content))
            {
                stringResponse = Socket.Receive();
                if (stringResponse != null)
                {
                    //response = BuildPaymentResponse(response, stringResponse);
                    status = "success";
                }
            }
            response.Result.Status = status;
            return response;
        }

        public GenericResponse Settlement(IGenericRequest gRequest, GenericResponse response)
        {
            var request = MappingRequest(gRequest);
            var status = "Error";
            var stringResponse = "";

            var content = request.GetContent(Formatting);  // BuildPaymentRequestString(gRequest).ToString();
            if (DoRequest(content))
            {
                stringResponse = Socket.Receive();
                if (stringResponse != null)
                {
                  //  response = BuildPaymentResponse(response, stringResponse);
                    status = "success";
                }
            }
            response.Result.Status = status;
            return response;
        }

        public GenericResponse PrintReceipt(IGenericRequest gRequest, GenericResponse response)
        {
            var request = MappingRequest(gRequest);
            var status = "Error";
            var stringResponse = "";

            var content = request.GetContent(Formatting);  // BuildPaymentRequestString(gRequest).ToString();
            if (DoRequest(content))
            {
                stringResponse = Socket.Receive();
                if (stringResponse != null)
                {
                    //response = BuildPaymentResponse(response, stringResponse);
                    status = "success";
                }
            }
            response.Result.Status = status;
            return response;
        }


        //private string BuildRefundRequestString(Operation request)
        //{

        //    var separator = (char)0x1C;
        //    StringBuilder content = null;
        //    content.Append(request.SystemId).Append(separator);
        //    content.Append(request.TransactionType).Append(separator);
        //    content.Append(request.Amount).Append(separator);
        //    content.Append(request.AccountNumber).Append(separator);
        //    content.Append(request.ExpirationDate.Value.ToString("yyMM")).Append(separator);
        //    content.Append(request.ValidFrom.Value.ToString("yyMM")).Append(separator);
        //    content.Append(request.CCV2).Append(separator);
        //    content.Append(request.ContractOrRoomNo).Append(separator);
        //    content.Append(request.StartDate.Value.ToString("ddMMyyyy")).Append(separator);
        //    content.Append(request.PrepareReceiptIndicator ? "1" : "0").Append(separator);
        //    content.Append(request.Language).Append(separator);
        //    content.Append(request.CurrencyCode).Append(separator);
        //    content.Append(request.CashierID).Append(separator);
        //    content.Append(request.ReturnPANEncrypted).Append(separator);
        //    content.Append(request.SupplyCVMResult).Append(separator);
        //    content.Append(request.BOCLoyaltyItems).Append(separator);
        //    content.Append(request.Password).Append(separator);
        //    content.Append(request.MessageCode).Append(separator);
        //    content.Append(request.MessageType).Append(separator);
        //    var len1 = content.Length;
        //    content.Append(request.CheckOutDate.Value.ToString("ddMMyyyy")).Append(separator);
        //    content.Append(request.BatchNumber).Append(separator);
        //    content.Append(request.AgreementNumber).Append(separator);
        //    content.Append(request.AgreementDate.Value.ToString("ddMMyyyy")).Append(separator);
        //    content.Append(request.SessionId).Append(separator);

        //    //var totalLength = content.Length + content.Length.ToString("D4").Length;

        //    //content.Insert(content.Length - len1, content.Length.ToString("D4"));
        //    return content.ToString();
        //}
        //private string BuildVoidRequestString(Operation request)
        //{
        //    var separator = (char)0x1C;
        //    StringBuilder content = null;
        //    content.Append(request.SystemId).Append(separator);
        //    content.Append(request.TransactionType).Append(separator);
        //    content.Append(request.Amount).Append(separator);
        //    content.Append(request.OriginalReceiptNumber).Append(separator);
        //    content.Append(request.PrepareReceiptIndicator ? "1" : "0").Append(separator);
        //    content.Append(request.Language).Append(separator);
        //    content.Append(request.CurrencyCode).Append(separator);
        //    content.Append(request.CashierID).Append(separator);
        //    content.Append(request.ReturnPANEncrypted).Append(separator);
        //    content.Append(request.SupplyCVMResult).Append(separator);
        //    content.Append(request.BOCLoyaltyItems).Append(separator);
        //    content.Append(request.Password).Append(separator);
        //    content.Append(request.MessageCode).Append(separator);
        //    content.Append(request.MessageType).Append(separator);
        //    var len1 = content.Length;
        //    content.Append(request.BatchNumber).Append(separator);
        //    content.Append(request.SessionId).Append(separator);

        //    //var totalLength = content.Length + content.Length.ToString("D4").Length;

        //    //content.Insert(content.Length - len1, content.Length.ToString("D4"));
        //    return content.ToString();
        //}

        //private string BuildSettlmentRequestString(Operation request)
        //{
        //    var separator = (char)0x1C;
        //    StringBuilder content = null;
        //    content.Append(request.SystemId).Append(separator);
        //    content.Append(request.TransactionType).Append(separator);
        //    content.Append(request.CashierID).Append(separator);
        //    content.Append(request.MessageCode).Append(separator);
        //    content.Append(request.MessageType).Append(separator);
        //    var len1 = content.Length;
        //    content.Append(request.BatchNumber).Append(separator);
        //    content.Append(request.SessionId).Append(separator);

        //    //var totalLength = content.Length + content.Length.ToString("D4").Length;

        //    //content.Insert(content.Length - len1, content.Length.ToString("D4"));
        //    return content.ToString();
        //}

        //private string BuildPrintReceiptRequestString(Operation request)
        //{
        //    var separator = (char)0x1C;
        //    StringBuilder content = null;
        //    content.Append(request.SystemId).Append(separator);
        //    content.Append(request.TransactionType).Append(separator);
        //    content.Append(request.PrintingContext).Append(separator);
        //    content.Append(request.CashierID).Append(separator);
        //    content.Append(request.MessageCode).Append(separator);
        //    content.Append(request.MessageType).Append(separator);
        //    var len1 = content.Length;
        //    content.Append(request.BatchNumber).Append(separator);
        //    content.Append(request.SessionId).Append(separator);

        //    //var totalLength = content.Length + content.Length.ToString("D4").Length;

        //    //content.Insert(content.Length - len1, content.Length.ToString("D4"));
        //    return content.ToString();
        //}


        //private string BuildPaymentRequestString(Operation request)
        //{
        //    var separator = (char)0x1C;
        //    StringBuilder content = new StringBuilder();
        //    content.Append(MapOpreation(request.OperationType)).Append(separator);
        //    content.Append(request.Amount.ToString().PadLeft(10, '0')).Append(separator);
        //    content.Append(request.AccountNumber.PadLeft(22, '0')).Append(separator);
        //    content.Append(request.ExpirationDate.Value.ToString("yyMM") ?? "    ").Append(separator);
        //    content.Append(request.ValidFrom.Value.ToString("yyMM") ?? "    ").Append(separator);
        //    content.Append(request.CCV2).Append(separator);
        //    content.Append(request.ContractOrRoomNo).Append(separator);
        //    content.Append(request.StartDate.Value.ToString("ddMMyyyy")).Append(separator);
        //    content.Append(request.ExtTerminalId).Append(separator);
        //    content.Append(request.ExtMerchantId).Append(separator);
        //    content.Append(request.PrepareReceiptIndicator ? "1" : "0").Append(separator);
        //    content.Append(request.Language).Append(separator);
        //    content.Append(request.CurrencyCode).Append(separator);
        //    content.Append(request.CashierID).Append(separator);
        //    content.Append(request.ReturnPANEncrypted).Append(separator);
        //    content.Append(request.SupplyCVMResult).Append(separator);
        //    content.Append(request.BOCLoyaltyItems).Append(separator);
        //    content.Append(request.MessageCode).Append(separator);
        //    content.Append(request.MessageType).Append(separator);
        //    content.Append("[len]").Append(separator);
        //    content.Append(request.CheckOutDate.Value.ToString("ddMMyyyy")).Append(separator);
        //    content.Append(request.BatchNumber).Append(separator);
        //    content.Append(request.AgreementNumber).Append(separator);
        //    content.Append(request.AgreementDate.Value.ToString("ddMMyyyy")).Append(separator);
        //    content.Append(request.SessionId).Append(separator);
        //    var len = request.GetLength(content) + 4;

        //    content.Replace("[len]", len.ToString().PadLeft(4, '0'));
        //    return content.ToString();
        //}

        public GenericResponse CloseBatch(IGenericRequest request, GenericResponse response)
        {
            throw new NotImplementedException();
        }


        private GenericResponse BuildVoidResponse(GenericResponse response, string responseString)
        {
            var resObj = new PrintecResponse();
            string[] valuesArray;
            valuesArray = responseString.Split((char)0x1C);
            //resObj.SystemId = valuesArray[0];
            //resObj.TransactionType = valuesArray[1];
            //resObj.NOInstallments = Int32.Parse(valuesArray[2]);
            //resObj.NoOfPostdatedMonths = Int32.Parse(valuesArray[3]);
            //resObj.TerminalIdentification = valuesArray[4];
            //resObj.BatchNumber = Int32.Parse(valuesArray[5]);
            //resObj.ResponseCode = valuesArray[6];
            //resObj.LoyaltyRedemptionIndicator = bool.Parse(valuesArray[8]);
            //resObj.ModifiedAmount = decimal.Parse(valuesArray[9]);
            //resObj.RetrievalReferenceNumber = valuesArray[10];
            //resObj.TransReferNumberOrBatchTotalCounter = valuesArray[11];
            //resObj.AuthCode = valuesArray[12];
            //resObj.AccountNumber = valuesArray[13];
            //resObj.ExpirationDate = DateTime.Parse(valuesArray[14]);
            //resObj.CardholderName = valuesArray[15];
            //resObj.DCCTransactionAmount = valuesArray[16];
            //resObj.DCCCurrency = valuesArray[17];
            //resObj.DCCMarkUpPercentage = valuesArray[19];
            //resObj.DCCExchangeDateOfRate = valuesArray[18];
            //resObj.ResponseTextMessage = valuesArray[20];
            //resObj.CardProductName = valuesArray[21];
            //resObj.OriginalResponseCode = valuesArray[23];
            //resObj.FirstInstallmentDate = DateTime.Parse(valuesArray[22]);
            //resObj.NetAmount = valuesArray[24];
            //resObj.TransactionReceiptTicket = valuesArray[25];
            //resObj.CurrencyCode = valuesArray[26];
            //resObj.PanEncrypted = valuesArray[27];
            //resObj.TransCVMResult = valuesArray[28];
            //resObj.BOCLoyaltyItems = valuesArray[29];
            response.Result.ResponseCode = resObj.ResponseCode;
            return response;
        }

        public GenericResponse BuildPaymentResponse(GenericResponse response, PrintecResponse res)
        {
            response.Result.ResponseCode = res.ResponseCode;
            response.Result.ResponseContent = res.TransactionReceiptTicket;
            return response;
        }

        public GenericResponse BuildRefundResponse(GenericResponse response, string responseString)
        {
            var resObj = new PrintecResponse();
            string[] valuesArray;
            //valuesArray = responseString.Split((char)0x1C);
            //resObj.SystemId = valuesArray[0];
            //resObj.TransactionType = valuesArray[1];
            //resObj.NOInstallments = Int32.Parse(valuesArray[2]);
            //resObj.NoOfPostdatedMonths = Int32.Parse(valuesArray[3]);
            //resObj.TerminalIdentification = valuesArray[4];
            //resObj.BatchNumber = Int32.Parse(valuesArray[5]);
            //resObj.ResponseCode = valuesArray[6];
            //resObj.OriginalAmountOrBatchTotalNetAmount = decimal.Parse(valuesArray[7]);
            //resObj.ModifiedAmount = decimal.Parse(valuesArray[8]);
            //resObj.RetrievalReferenceNumber = valuesArray[9];
            //resObj.TransReferNumberOrBatchTotalCounter = valuesArray[10];
            //resObj.AuthCode = valuesArray[11];
            //resObj.AccountNumber = valuesArray[12];
            //resObj.ExpirationDate = DateTime.Parse(valuesArray[13]);
            //resObj.CardholderName = valuesArray[14];
            //resObj.DCCTransactionAmount = valuesArray[15];
            //resObj.DCCCurrency = valuesArray[16];
            //resObj.DCCMarkUpPercentage = valuesArray[17];
            //resObj.DCCExchangeDateOfRate = valuesArray[18];
            //resObj.ResponseTextMessage = valuesArray[19];
            //resObj.CardProductName = valuesArray[20];
            //resObj.OriginalResponseCode = valuesArray[21];
            //resObj.FirstInstallmentDate = DateTime.Parse(valuesArray[22]);
            //resObj.NetAmount = valuesArray[23];
            //resObj.ExpirationDate = DateTime.Parse(valuesArray[24]);
            //resObj.ExpirationDate = DateTime.Parse(valuesArray[25]);
            //resObj.ExpirationDate = DateTime.Parse(valuesArray[26]);
            //response.Result.ResponseCode = resObj.ResponseCode;
            return response;
        }
        public PrintecResponse ParseEFTTransactionResponse(string[] msg)
        {
            var index = msg.Length;
            var result = new PrintecResponse();
            var sepDecimal = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
           
            result.SystemId = Helper.TryParse<string>(msg[0], 32,ref index);
            result.TransactionType = Helper.TryParse<string>(!string.IsNullOrEmpty(msg[1])?msg[1].Substring(0, 2):"", 2,ref index);
            result.NOInstallments = Helper.TryParse<int>(!string.IsNullOrEmpty(msg[1])?msg[1].Substring(2, 2):"", 2, "00", ref index);
            result.NoOfPostdatedMonths = Helper.TryParse<int>(!string.IsNullOrEmpty(msg[1]) ? msg[1].Substring(4, 2):"", 2, "00", ref index);
            result.TerminalIdentification = Helper.TryParse<string>(!string.IsNullOrEmpty(msg[1]) ? msg[1].Substring(6, 12):"", 12, ref index);
            result.BatchNumber = Helper.TryParse<string>(msg[2], 3, ref index);
            result.ResponseCode = Helper.TryParse<string>(msg[3], 2, ref index);
            result.OriginalAmountOrBatchTotalNetAmount = Helper.TryParseDecimal<decimal>(!string.IsNullOrEmpty(msg[4])?msg[4].Substring(1):"", 11, sepDecimal, 'V', ref index);
            result.LoyaltyRedemptionIndicator = Helper.TryParse<bool>(msg[5], 1, ref index);
            result.ModifiedAmount = Helper.TryParseDecimal<decimal>(msg[6], 10, sepDecimal, 'V', ref index);
            result.RetrievalReferenceNumber = Helper.TryParse<string>(msg[7], 12, ref index);
            result.TransReferNumberOrBatchTotalCounter = Helper.TryParse<string>(msg[8], 4, ref index);
            result.AuthCode = Helper.TryParse<string>(msg[9], 6, ref index);
            result.AccountNumber = Helper.TryParse<string>(msg[10], 40, ref index);
            result.ExpirationDate = Helper.TryParse<DateTime>(msg[11], 4, "yyMM", ref index);
            result.CardholderName = Helper.TryParse<string>(msg[12], 80, ref index);
            result.DCCTransactionAmount = Helper.TryParseDecimal<decimal>(msg[13], 10, sepDecimal, 'V', ref index);
            result.DCCCurrency = Helper.TryParse<string>(msg[14], 3, ref index);
            result.DCCExchangeRateMarkUp = Helper.TryParseDecimal<decimal>(msg[15], 10, sepDecimal, 'V', ref index);
            result.DCCMarkUpPercentage = Helper.TryParseDecimal<decimal>(msg[16], 10, sepDecimal, 'V', ref index);
            result.DCCExchangeDateOfRate = Helper.TryParse<DateTime>(msg[17], 4, "ddMM", ref index);
            result.ResponseTextMessage = Helper.TryParse<string>(msg[18], 200, ref index);
            result.CardProductName = Helper.TryParse<string>(msg[19], 24, ref index);
            result.OriginalResponseCode = Helper.TryParse<string>(msg[20], 3, ref index);
            result.FirstInstallmentDate = Helper.TryParse<DateTime>(msg[21], 6, "ddMMyy", ref index);
            result.NetAmount = Helper.TryParseDecimal<decimal>(msg[22], 10, sepDecimal, 'V', ref index);
            result.TransactionReceiptTicket = Helper.TryParse<string>(msg[23], 2499, ref index);
            result.BatchStatus = Helper.TryParse<bool>(msg[24], 1, ref index);
            result.CurrencyCode = Helper.TryParse<string>(msg[25], 3, ref index);
            result.LoyaltyBalance = Helper.TryParse<string>(msg[26], 12, ref index);
            result.LoyaltyTransactionPoint = Helper.TryParse<string>(msg[27], 12, ref index);
            result.LoyaltyTransReferenceNo = Helper.TryParse<string>(msg[28], 4, ref index);
            result.PanEncrypted = Helper.TryParse<string>(msg[29], 512, ref index);
            result.Track2Data = Helper.TryParse<string>(msg[30], 40, ref index);
            result.PINValue = Helper.TryParse<string>(msg[31], 16, ref index);
            result.KilometresIndicator = Helper.TryParse<string>(msg[32], 16, ref index);
            result.GiftBalance = Helper.TryParse<string>(msg[33], 12, ref index);
            result.GiftTransRefNo = Helper.TryParse<string>(msg[34], 4, ref index);
            result.TransCVMResult = Helper.TryParse<bool>(msg[35], 1, ref index);
            result.BOCLoyaltyItems = Helper.TryParse<string>(msg[36], 1990, ref index);
            result.IsBOCLoyaltyTransaction = Helper.TryParse<bool>(msg[37], 1, ref index);
            result.BOCLoyaltyPointsEarned = Helper.TryParse<string>(msg[38], 10, ref index);
            result.MerchantDiscount = Helper.TryParse<string>(msg[39], 10, ref index);
            result.VoucherDiscount = Helper.TryParse<string>(msg[40], 10, ref index);
            result.BOCRedemptionAmount = Helper.TryParse<string>(msg[41], 10, ref index);
            result.CashbackAmount = Helper.TryParseDecimal<decimal>(msg[42], 10, sepDecimal, 'V', ref index);
            return result;
           
        }

    }
}
