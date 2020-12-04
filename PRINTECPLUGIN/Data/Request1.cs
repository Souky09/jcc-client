using Serilog;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PRINTECPLUGIN.Data
{
    public class Request1
    {
        public string SystemId { get; set; }
        public string TransactionType { get; set; }
        public decimal?  Amount { get { return _amount; } set { _amount = value * 100; } }
        private decimal? _amount;
        public string AccountNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public string OriginalReceiptNumber { get; set; }
        public string AuthNo { get; set; }
        public string CVV2 { get; set; }
        public string ContractOrRoomNo { get; set; }
        public DateTime? StartDate { get; set; }
        public string ExtTerminalId { get; set; }
        public string ExtMerchantId { get; set; }
        public string ExtReferenceNumber { get; set; } = "";
        public string ReceiptContent { get; set; }
        public bool? PrepareReceiptIndicator { get; set; }
        public string Language { get; set; } = "00";
        public string CurrencyCode { get; set; } = "978";
        public string LoyaltyRequestType { get; set; }
        public string LoyaltyIdentificationMethod { get; set; }
        public string OrgLoyaltyReferenceNumber { get; set; }
        public string CashierID { get; set; }
        public string ReturnPANEncrypted { get; set; } = "0";
        public string GenericLoyaltyFlag { get; set; }
        public string GenericLoyaltyBins{ get; set; }
        public string GiftTransactionType { get; set; }
        public string OrigGiftReferenceNumber { get; set; }
        public string SupplyCVMResult { get; set; } = "0";
        public string BOCLoyaltyItems { get; set; }
        public string Password { get; set; }
        public string MessageCode { get; set; } = "00";
        public string MessageType { get; set; } = "000";
        public string Length { get; set; }
        public DateTime? CheckOutDate { get; set; }
        
        public string BatchNumber { get; set; }
       
        public DateTime? AgreementDate { get; set; }
        public string AgreementNumber { get; set; }
        
        public string SessionId { get; set; }


        //serilize object to string
        public string GetContent(Request request,Formatting formatting)
        {

            var content = "";
            if(formatting.Version=="1.0.0")
            {
                content = Serialize(request, formatting.Fields);
            }
           
            return content;
        }

        public string GetValueDictionnary(string key, List<Field> addInfo)
        {
            var exist = addInfo.FirstOrDefault(x => x.Value.ToLower() == key.ToLower());
            if (exist!=null && !string.IsNullOrEmpty(exist.Format))
            {
                return exist.Format;

            }
            return null;
        }
        public string Serialize(Request request,List<Field> fields)
        {
           
            
            StringBuilder content = new StringBuilder();
            var separator =fields.First().Value;

            //content.Append( string.Format($"{{0:{fields.FirstOrDefault(x => x.Value == "amount").Format}}}", Amount*100)).Append(separator);
            foreach(var i in request.GetType().GetProperties().OrderBy(x=>x.MetadataToken))
            {
                if(i!=null )
                {
                    if (GetValueDictionnary(i.Name, fields) != null)
                    {
                       
                        content.Append(string.Format($"{{0:{GetValueDictionnary(i.Name, fields)}}}", i.GetValue(request))).Append(separator);
                    }
                    else
                    {
                        content.Append(i.GetValue(request)).Append(separator);
                    }

                }
            }
            //content.Append(GetValueDictionnary("systemId", fields) != null && request.SystemId != null ? string.Format($"{{0:{GetValueDictionnary("systemId", fields)}}}", request.SystemId) : request.SystemId).Append(separator);
            //content.Append(GetValueDictionnary("transactionType", fields) != null && request.TransactionType != null ? string.Format($"{{0:{GetValueDictionnary("transactionType", fields)}}}", request.TransactionType) : request.TransactionType.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("amount", fields) != null && request.Amount != null ? string.Format($"{{0:{GetValueDictionnary("amount", fields)}}}", request.Amount * 100) : request.Amount.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("accountNumber", fields) != null && request.AccountNumber != null ? string.Format($"{{0:{GetValueDictionnary("accountNumber", fields)}}}", request.AccountNumber) : request.AccountNumber.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("expirationdate", fields) != null && request.ExpirationDate != null ? string.Format($"{{0:{GetValueDictionnary("expirationdate", fields)}}}", request.ExpirationDate) : request.ExpirationDate.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("validFrom", fields) != null && request.ValidFrom != null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom) : request.ValidFrom.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("originalReceiptNumber", fields) != null && request.OriginalReceiptNumber != null ? string.Format($"{{0:{GetValueDictionnary("originalReceiptNumber", fields)}}}", request.OriginalReceiptNumber) : request.OriginalReceiptNumber.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("authNo", fields) != null && request.AuthNo != null ? string.Format($"{{0:{GetValueDictionnary("authNo", fields)}}}", request.AuthNo) : request.AuthNo.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("cvv2", fields) != null && !string.IsNullOrEmpty(request.CVV2) ? string.Format($"{{0:{GetValueDictionnary("cvv2",fields)}}}", request.CVV2): request.CVV2).Append(separator);
            //content.Append(GetValueDictionnary("contractOrRoomNo", fields)!=null && request.ContractOrRoomNo != null ? string.Format($"{{0:{GetValueDictionnary("contractOrRoomNo", fields)}}}", request.ContractOrRoomNo) : request.ContractOrRoomNo.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("extTerminalId", fields)!=null && request.ExtTerminalId !=null ? string.Format($"{{0:{GetValueDictionnary("extTerminalId", fields)}}}", request.ExtTerminalId): request.ExtTerminalId.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("extMerchantId", fields)!=null && request.ExtMerchantId !=null ? string.Format($"{{0:{GetValueDictionnary("extMerchantId", fields)}}}", request.ExtMerchantId): request.ExtMerchantId.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("extReferenceNumber", fields)!=null && request.ExtReferenceNumber !=null ? string.Format($"{{0:{GetValueDictionnary("extReferenceNumber", fields)}}}", request.ExtReferenceNumber): request.ExtReferenceNumber.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("receiptContent", fields)!=null && request.ReceiptContent !=null ? string.Format($"{{0:{GetValueDictionnary("receiptContent", fields)}}}", request.ReceiptContent) : request.ReceiptContent.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("prepareReceiptIndicator", fields)!=null && request.PrepareReceiptIndicator !=null ? string.Format($"{{0:{GetValueDictionnary("prepareReceiptIndicator", fields)}}}", request.PrepareReceiptIndicator): request.PrepareReceiptIndicator.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("language",fields)!=null && request.Language !=null ? string.Format($"{{0:{GetValueDictionnary("language", fields)}}}", request.Language): request.Language.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("currencyCode",fields)!=null && request.CurrencyCode !=null ? string.Format($"{{0:{GetValueDictionnary("currencyCode", fields)}}}", request.CurrencyCode): request.CurrencyCode.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("loyaltyRequestType", fields)!=null && request.LoyaltyRequestType !=null ? string.Format($"{{0:{GetValueDictionnary("loyaltyRequestType", fields)}}}", request.LoyaltyRequestType): request.LoyaltyRequestType.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("loyaltyIdentificationMethod", fields)!=null && request.LoyaltyIdentificationMethod !=null ? string.Format($"{{0:{GetValueDictionnary("loyaltyIdentificationMethod", fields)}}}", request.LoyaltyIdentificationMethod) : request.LoyaltyIdentificationMethod.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("orgLoyaltyReferenceNumber", fields)!=null && request.OrgLoyaltyReferenceNumber !=null ? string.Format($"{{0:{GetValueDictionnary("orgLoyaltyReferenceNumber", fields)}}}", request.OrgLoyaltyReferenceNumber): request.OrgLoyaltyReferenceNumber.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("cashierId",fields)!=null && request.CashierID !=null ? string.Format($"{{0:{GetValueDictionnary("cashierId", fields)}}}", request.CashierID): request.CashierID.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("returnPANEncrypted", fields)!=null && request.ReturnPANEncrypted != null ? string.Format($"{{0:{GetValueDictionnary("returnPANEncrypted", fields)}}}", request.ReturnPANEncrypted): request.ReturnPANEncrypted.ToString()).Append(separator);
            //content.Append(GetValueDictionnary("genericLoyaltyFlag", fields)!=null && request.GenericLoyaltyFlag !=null ? string.Format($"{{0:{GetValueDictionnary("genericLoyaltyFlag", fields)}}}", request.GenericLoyaltyFlag): request.GenericLoyaltyFlag.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            ////content.Append(GetValueDictionnary("validFrom",fields)!=null && request.ValidFrom !=null ? string.Format($"{{0:{GetValueDictionnary("validFrom", fields)}}}", request.ValidFrom): request.ValidFrom.ToString()).Append(separator);
            //content.Append("[ln]").Append(separator);
            //content.Append(GetValueDictionnary("sessionId", fields)!=null && request.SessionId !=null ? string.Format($"{{0:{GetValueDictionnary("sessionId", fields)}}}", request.SessionId): request.SessionId.ToString()).Append(separator);
            //content.Replace("[ln]", content.Length.ToString().PadLeft(4,'0'));
            //var len = content.Length;
            //content.Replace("[ln]", len.ToString().PadLeft(4, '0'));
            //return content.ToString();
            return content.ToString();
        }


    }
}
