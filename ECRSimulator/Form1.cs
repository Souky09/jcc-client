using SharedClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace ECRSimulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
           

            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox1.Items[comboBox1.SelectedIndex].ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string[] lines = File.ReadAllLines(Path.Combine(Directory
                                                .GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "dataText.txt"));
            var dict = lines.Select(l => l.Split('=')).ToDictionary(a => a[0], a => a[1]);
            var url = dict["url"];
            var requestStatusUri = new UriBuilder(dict["urlStatus"]).ToString();
            var request = CreateRequest();

            var requestUri = new UriBuilder(url).ToString();
            using (var client = new HttpClient())
            {
                //var timeout = 180;
                //client.Timeout = new TimeSpan(0, 0, timeout);
                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json,
                                Encoding.UTF8, "application/json");
                
                var response = client.PostAsync(requestUri, httpContent).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var responseobject = JsonConvert.DeserializeObject<GenericResponse>(responseString);

                while (responseobject.Result.Status == "processing")
                {

                    httpContent = new StringContent(JsonConvert.SerializeObject(JObject.Parse("{'id':'" + responseobject.Result.OperationID + "'}").ToString()),
                                  Encoding.UTF8, "application/json");
                    
                    response = client.PostAsync(requestStatusUri, httpContent).Result;

                    responseString = response.Content.ReadAsStringAsync().Result;
                    responseobject.Result = JsonConvert.DeserializeObject<Result>(responseString);
                }
            }




        }
        public GenericRequest CreateRequest()
        {
            string operation =  comboBox1.Items[comboBox1.SelectedIndex].ToString();
            string amount = amountTxt.Text;
            var list = new Dictionary<string, string>();
            list.Add("amount", amount);
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
                OperationType = operation,
                PayLoad = new PayLoad() { CashierID = "002", CurrencyCode = "001", AdditionalInfo = list },
                Info = new Info() { TerminalID = "11", Merchant="merchant1", StoreId="1", User="Souky",  Password="sssss" }
            };
            return request;
        }
    }
}
