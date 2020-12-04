using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JCCPLUGIN.Data
{
    public class JCCResponse
    {

        public int Length { get; set; }
        public string SessionId { get; set; }
        public string AuthType { get; set; }
        public string FinalAmount { get; set; }
        public string AdditionalAmount { get; set; }
        public string BasicCurrencyCode { get; set; }
        public string ResponseCode { get; set; }
        public string AuthCode { get; set; }
        public string RRNFinancialAuthorization { get; set; }
        public string PanNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string TokenNo { get; set; }
        public string EncryptedPan { get; set; }
        public DateTime? DateAndDatime { get; set; }
        public string HostResponse { get; set; }
        public string CardProduct { get; set; }
        public string ReceiptNo { get; set; }
        public string BatchNo { get; set; }
        public string TerminalNo { get; set; }
        public string CVMResults { get; set; }
        public string EntryMode { get; set; }
        public string POSVersion { get; set; }
        public string DataToBePrinted { get; set; }
        public decimal DCCAmount { get; set; }
        public string DCCCurrency { get; set; }
        public decimal DCCExchangeRateMarkUp { get; set; }
        public decimal DCCMarkUpPercentage { get; set; }
        public int NOInstalments { get; set; }
        public int DaysForFirstInstalments { get; set; }
        public decimal LoyaltyPointsRedeemed { get; set; }
        public decimal LoyaltyPointsBalance { get; set; }
        public string GiftBalance { get; set; }
        public string GiftReceipt { get; set; }
        public string BOCLoyaltyItems { get; set; }
        public string MerchantName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string VATRegNo { get; set; }
        public string MCC { get; set; }

    }
}
