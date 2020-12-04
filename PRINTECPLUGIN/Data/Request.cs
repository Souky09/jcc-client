using Castle.DynamicProxy.Internal;
using Serilog;
using SharedClasses;
using SharedClasses.Persistence;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace PRINTECPLUGIN.Data
{
    public class Request : IRequest
    {
        public string SystemId { get; set; }
        public string TransactionType { get; set; }
        public decimal? OrigTransactionAmount { get { return _amnt; } set { _amnt = value * 100; } }
        //public string OrigTransactionAmount { get { return _amnt.ToString(); } set { decimal.TryParse(value, out var amnt); _amnt = amnt*100; } }
        //[Description("OrigTransactionAmount")]
        private decimal? _amnt;
        public string AccountNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public string OrigReferNumber { get; set; }
        public string AuthCode { get; set; }
        public string CVV2 { get; set; }
        public string ContractOrRoomNo { get; set; }
        public DateTime? StartDate { get; set; }
        public string ExtTerminalId { get; set; }
        public string ExtMerchantId { get; set; }
        public string ExtReferenceNumber { get; set; } = "";
        public string ReceiptContent { get; set; }
        public string PrepareReceiptTicket { get; set; }
        public string Language { get; set; } = "00";
        public string CurrencyCode { get; set; } = "978";
        public string LoyaltyTransactionType { get; set; }
        public string LoyaltyIdentificationMethod { get; set; }
        public string OrigLoyaltyReferenceNumber { get; set; }
        public string CashierID { get; set; }
        public string ReturnPANEncrypted { get; set; } = "0";
        public string GenericLoyaltyFlag { get; set; }
        public string GenericLoyaltyBins { get; set; }
        public string GiftTransactionType { get; set; }
        public string OrigGiftReferenceNumber { get; set; }
        public string SupplyCVMResult { get; set; } = "0";
        public string BOCLoyaltyItems { get; set; }
        public string Password { get; set; }
        public string MessageCode { get; set; } = "00";
        public string MessageType { get; set; } = "000";
        public int? Length { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string BatchNumber { get; set; }
        public string AgreementNumber { get; set; }
        public DateTime? AgreementDate { get; set; }
        public string SessionId { get; set; }
        public decimal? CashBackAmount { get; set; }
        public string EACAccountNumber { get; set; }
        public string EACPhoneNumber { get; set; }
        public string EACPaymentFlag { get; set; }

        //CalculateLength
        public void SettLength(List<Field> formatting)
        {
            var l = 0;
            foreach (var i in this.GetType().GetProperties().OrderBy(x => x.MetadataToken).ToList())
            {
                var initVal = i.GetValue(this);
                var val = initVal != null ? initVal : null;
                var fieldFound = Helper.GetField(i.Name, formatting);
                if (val != null)
                {

                    if (fieldFound != null && !string.IsNullOrEmpty(fieldFound.Format))
                    {

                        l += string.Format($"{{0:{fieldFound.Format}}}", val).Length;
                    }
                    else
                    {
                        l += val.ToString().Length;
                    }
                }
                l++;
            }
            this.Length = l + l.ToString().Length;
        }

        //serialize object to string


        public string GetContent(Formatting formatting)
        {
            this.SettLength(formatting.Fields.ToList());
            var content = "";
            var version = formatting.Version.ToString();
            var fields = formatting.Fields.ToList();
            if (version == "1.0.0")
            {
                content = Serialize(fields);
            }
            return content;
        }


        public string Serialize(List<Field> fields)
        {

            StringBuilder content = new StringBuilder();
            var separator = fields.FirstOrDefault().Value;
            var sep = Convert.ToChar(Convert.ToUInt32(separator, 16));


            foreach (var i in this.GetType().GetProperties().OrderBy(x => x.MetadataToken).ToList())
            {
                if (i != null)
                {

                    var fieldFound = Helper.GetField(i.Name, fields);
                    var toAppend = "";
                    if (fieldFound != null)
                    {
                        if (!string.IsNullOrEmpty(fieldFound.Format))
                        {
                            toAppend = string.Format($"{{0:{fieldFound.Format}}}", i.GetValue(this));
                        }
                        if (!string.IsNullOrEmpty(fieldFound.Length))
                        {
                            if (toAppend.Length > Convert.ToInt32(fieldFound.Length))
                            {
                                Log.Fatal($"Field {i.Name} is out of range length");
                                throw new ArgumentOutOfRangeException(i.Name, new ArgumentOutOfRangeException(), "Field out of range length");
                            }
                        }
                        content.Append(string.Format($"{{0:{fieldFound.Format}}}", i.GetValue(this)));//.Append(separator);
                    }
                    else
                    {
                        content.Append(i.GetValue(this));//.Append(separator);
                    }
                    content.Append(sep);
                    ////////////////////////////
                    //var found = GetFormatDictionnary(i.Name, fields);
                    //if (found != null)
                    //{
                    //    content.Append(string.Format($"{{0:{found}}}", i.GetValue(request))).Append(separator);
                    //}
                    //else
                    //{
                    //    content.Append(i.GetValue(request)).Append(separator);
                    //}
                }
            }

            return content.ToString();
        }


    }
}
