﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QA.ProductCatalog.HighloadFront.Importer.Service_References.DpcServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DpcServiceReference.IDpcService")]
    public interface IDpcService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDpcService/GetAllProductId", ReplyAction="http://tempuri.org/IDpcService/GetAllProductIdResponse")]
        int[] GetAllProductId(int page, int pageSize);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDpcService/GetAllProductId", ReplyAction="http://tempuri.org/IDpcService/GetAllProductIdResponse")]
        System.Threading.Tasks.Task<int[]> GetAllProductIdAsync(int page, int pageSize);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDpcService/GetProduct", ReplyAction="http://tempuri.org/IDpcService/GetProductResponse")]
        string GetProduct(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDpcService/GetProduct", ReplyAction="http://tempuri.org/IDpcService/GetProductResponse")]
        System.Threading.Tasks.Task<string> GetProductAsync(int id);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDpcServiceChannel : IDpcService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DpcServiceClient : System.ServiceModel.ClientBase<IDpcService>, IDpcService {
        
        public DpcServiceClient() {
        }
        
        public DpcServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DpcServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DpcServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DpcServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public int[] GetAllProductId(int page, int pageSize) {
            return base.Channel.GetAllProductId(page, pageSize);
        }
        
        public System.Threading.Tasks.Task<int[]> GetAllProductIdAsync(int page, int pageSize) {
            return base.Channel.GetAllProductIdAsync(page, pageSize);
        }
        
        public string GetProduct(int id) {
            return base.Channel.GetProduct(id);
        }
        
        public System.Threading.Tasks.Task<string> GetProductAsync(int id) {
            return base.Channel.GetProductAsync(id);
        }
    }
}
