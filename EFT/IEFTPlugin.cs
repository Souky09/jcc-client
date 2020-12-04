using SharedClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EFT
{
    public interface IEFTPlugin 
    {
        public PluginInfo GetInfo();
        public GenericResponse Init(IGenericRequest request,GenericResponse response);
        public string MapOpreation(string operation);
        public GenericResponse Ping(IGenericRequest request, GenericResponse response);
        public GenericResponse Refund(IGenericRequest request, GenericResponse response);
        public GenericResponse Void(IGenericRequest request, GenericResponse response);
        public GenericResponse Reversal(IGenericRequest request, GenericResponse response);
        public GenericResponse Settlement(IGenericRequest request, GenericResponse response);
        public GenericResponse PrintReceipt(IGenericRequest request, GenericResponse response);
        public GenericResponse CloseBatch(IGenericRequest request, GenericResponse response);
        public GenericResponse PaymentOld(IGenericRequest request, GenericResponse response);
        public GenericResponse Payment(IGenericRequest request, GenericResponse response);
      //  public T MappingRequest<T>(IGenericRequest gRequest);

    }
}
