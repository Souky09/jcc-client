using Serilog;
using SharedClasses;
using SharedClasses.Persistence;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace JCCPLUGIN.Data
{
    public class Request : IRequest
    {
        public int? Length { get; set; }
        public string SessionId { get; set; }
        public string AuthType { get; set; }
        public decimal? Amount { get { return _amount; } set { _amount = value * 100; } }
        private decimal? _amount;
        public decimal? AdditionalAmount { get; set; }
        public string CurrencyCode { get; set; } = "978";
        public string OriginalReceiptNumber { get; set; }
        public string AuthNo { get; set; }
        public string RoomNo { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string ContractNoForCarRental { get; set; }
        public DateTime? AgreementDate { get; set; }
        public string PrepareReceiptIndicator { get; set; }
        public string PrintingContext { get; set; }
        public string CashierID { get; set; }
        public string DCCAlertSupport { get; set; }
        public string LoyaltyRequestType { get; set; }
        public string LoyaltyIdentification { get; set; }
        public string OriginalLoyaltyReceiptNumber { get; set; }
        public string GiftTransactionType { get; set; }
        public string OriginalGiftReceiptNo { get; set; }
        public string BOCLoyaltyItems { get; set; }
        public string TerminalId { get; set; }
        public string TokenNo { get; set; }

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
                    
                }
            }

            return content.ToString();
        }

    }
}
