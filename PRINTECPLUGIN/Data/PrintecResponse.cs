using SharedClasses.Persistence;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace PRINTECPLUGIN.Data
{
    public class PrintecResponse
    {
        public string SystemId { get; set; }
       
        public string TransactionType { get; set; }
        public int NOInstallments { get; set; }
        public int NoOfPostdatedMonths { get; set; }
        public string TerminalIdentification { get; set; }

        public string BatchNumber { get; set; }

        public string ResponseCode { get; set; }
        public decimal OriginalAmountOrBatchTotalNetAmount { get; set; }
        public bool LoyaltyRedemptionIndicator { get; set; }
        public decimal ModifiedAmount { get; set; }
        public string RetrievalReferenceNumber { get; set; }
        public string TransReferNumberOrBatchTotalCounter { get; set; }
        public string AuthCode { get; set; }
        
        public string AccountNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CardholderName { get; set; }
        public decimal DCCTransactionAmount { get; set; }
        public string DCCCurrency { get; set; }
        public decimal DCCExchangeRateMarkUp { get; set; }
        public decimal DCCMarkUpPercentage { get; set; }
        public DateTime DCCExchangeDateOfRate { get; set; }
        public string ResponseTextMessage { get; set; }
        public string CardProductName { get; set; }
        public string OriginalResponseCode { get; set; }
        public DateTime? FirstInstallmentDate { get; set; }
        public decimal NetAmount { get; set; }
        public string TransactionReceiptTicket { get; set; }
        public bool BatchStatus { get; set; }
        public string CurrencyCode { get; set; }
        public string LoyaltyBalance { get; set; }
        public string LoyaltyTransactionPoint { get; set; }
        public string LoyaltyTransReferenceNo { get; set; }
        public string PanEncrypted { get; set; }
        public string Track2Data { get; set; }
        public string PINValue { get; set; }
        public string KilometresIndicator  { get; set; }
        public string GiftBalance  { get; set; }
        public string GiftTransRefNo  { get; set; }
        public bool TransCVMResult { get; set; }
        public string BOCLoyaltyItems { get; set; }
        public bool IsBOCLoyaltyTransaction { get; set; }
        public string BOCLoyaltyPointsEarned { get; set; }
        public string MerchantDiscount { get; set; }
        public string VoucherDiscount { get; set; }
        public string BOCRedemptionAmount { get; set; }
        public  decimal CashbackAmount { get; set; }


       

        }
    }
