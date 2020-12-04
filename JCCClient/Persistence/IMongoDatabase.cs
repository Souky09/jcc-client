using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JCCClient.Persistence
{
    public interface IMongoDatabase
    {
        public bool Create(Result result);
        public IList<Result> Read();
        public Result Find(string id);
        public void Update(Result result);
        public bool Delete(Result result);
    }
}
