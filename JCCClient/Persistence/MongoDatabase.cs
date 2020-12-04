using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JCCClient.Persistence
{
    public class MongoDatabase //: IMongoDatabase
    {
        private readonly IMongoCollection<Result> results;

        public MongoDatabase(IConfiguration config)
        {
            var t = config.GetSection("ConfigInfo:ConnectionString").Value;
            var client = new MongoClient(config.GetSection("ConfigInfo:ConnectionString").Value);
            var database = client.GetDatabase(config.GetSection("ConfigInfo:Database").Value);
            //database.CreateCollection("mycoll");
            results = database.GetCollection<Result>("Results");
        }
        public bool Create(Result result)
        {
            try
            {
                results.InsertOne(result);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
               
        }

        public bool Delete(Result result)
        {
            try
            {
                results.DeleteOne(r=>r.OperationID==result.OperationID);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Result Find(string id)
        {
            return results.Find(x => x.OperationID == id).FirstOrDefault();
        }

        public IList<Result> Read()
        {
            return results.Find(x => true).ToList();
        }

        public bool Update(Result result)
        {
            var found = Find(result.OperationID);
            var res= results.ReplaceOneAsync(item =>item.OperationID==result.OperationID, result).Result;
            return res.IsAcknowledged && res.ModifiedCount > 0;
         }
    }
}
