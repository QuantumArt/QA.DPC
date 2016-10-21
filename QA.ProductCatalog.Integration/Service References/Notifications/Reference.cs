﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QA.ProductCatalog.Integration.Notifications {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="NotificationItem", Namespace="http://schemas.datacontract.org/2004/07/QA.Core.DPC")]
    [System.SerializableAttribute()]
    public partial class NotificationItem : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string[] ChannelsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int ProductIdField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string[] Channels {
            get {
                return this.ChannelsField;
            }
            set {
                if ((object.ReferenceEquals(this.ChannelsField, value) != true)) {
                    this.ChannelsField = value;
                    this.RaisePropertyChanged("Channels");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Data {
            get {
                return this.DataField;
            }
            set {
                if ((object.ReferenceEquals(this.DataField, value) != true)) {
                    this.DataField = value;
                    this.RaisePropertyChanged("Data");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ProductId {
            get {
                return this.ProductIdField;
            }
            set {
                if ((this.ProductIdField.Equals(value) != true)) {
                    this.ProductIdField = value;
                    this.RaisePropertyChanged("ProductId");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ConfigurationInfo", Namespace="http://schemas.datacontract.org/2004/07/QA.Core.DPC")]
    [System.SerializableAttribute()]
    public partial class ConfigurationInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private QA.ProductCatalog.Integration.Notifications.ChannelInfo[] ChannelsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsAtualField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NotificationProviderField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime StartedField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public QA.ProductCatalog.Integration.Notifications.ChannelInfo[] Channels {
            get {
                return this.ChannelsField;
            }
            set {
                if ((object.ReferenceEquals(this.ChannelsField, value) != true)) {
                    this.ChannelsField = value;
                    this.RaisePropertyChanged("Channels");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsAtual {
            get {
                return this.IsAtualField;
            }
            set {
                if ((this.IsAtualField.Equals(value) != true)) {
                    this.IsAtualField = value;
                    this.RaisePropertyChanged("IsAtual");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string NotificationProvider {
            get {
                return this.NotificationProviderField;
            }
            set {
                if ((object.ReferenceEquals(this.NotificationProviderField, value) != true)) {
                    this.NotificationProviderField = value;
                    this.RaisePropertyChanged("NotificationProvider");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime Started {
            get {
                return this.StartedField;
            }
            set {
                if ((this.StartedField.Equals(value) != true)) {
                    this.StartedField = value;
                    this.RaisePropertyChanged("Started");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ChannelInfo", Namespace="http://schemas.datacontract.org/2004/07/QA.Core.DPC")]
    [System.SerializableAttribute()]
    public partial class ChannelInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int CountField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<int> LastIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> LastPublishedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> LastQueuedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LastStatusField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private QA.ProductCatalog.Integration.Notifications.State StateField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Count {
            get {
                return this.CountField;
            }
            set {
                if ((this.CountField.Equals(value) != true)) {
                    this.CountField = value;
                    this.RaisePropertyChanged("Count");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> LastId {
            get {
                return this.LastIdField;
            }
            set {
                if ((this.LastIdField.Equals(value) != true)) {
                    this.LastIdField = value;
                    this.RaisePropertyChanged("LastId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> LastPublished {
            get {
                return this.LastPublishedField;
            }
            set {
                if ((this.LastPublishedField.Equals(value) != true)) {
                    this.LastPublishedField = value;
                    this.RaisePropertyChanged("LastPublished");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> LastQueued {
            get {
                return this.LastQueuedField;
            }
            set {
                if ((this.LastQueuedField.Equals(value) != true)) {
                    this.LastQueuedField = value;
                    this.RaisePropertyChanged("LastQueued");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LastStatus {
            get {
                return this.LastStatusField;
            }
            set {
                if ((object.ReferenceEquals(this.LastStatusField, value) != true)) {
                    this.LastStatusField = value;
                    this.RaisePropertyChanged("LastStatus");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public QA.ProductCatalog.Integration.Notifications.State State {
            get {
                return this.StateField;
            }
            set {
                if ((this.StateField.Equals(value) != true)) {
                    this.StateField = value;
                    this.RaisePropertyChanged("State");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="State", Namespace="http://schemas.datacontract.org/2004/07/QA.Core.DPC")]
    public enum State : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        New = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Actual = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Chanded = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Deleted = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="Notifications.INotificationService")]
    public interface INotificationService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/INotificationService/PushNotifications", ReplyAction="http://tempuri.org/INotificationService/PushNotificationsResponse")]
        void PushNotifications(QA.ProductCatalog.Integration.Notifications.NotificationItem[] notifications, bool isStage, int userId, string userName, string method);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/INotificationService/PushNotifications", ReplyAction="http://tempuri.org/INotificationService/PushNotificationsResponse")]
        System.Threading.Tasks.Task PushNotificationsAsync(QA.ProductCatalog.Integration.Notifications.NotificationItem[] notifications, bool isStage, int userId, string userName, string method);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/INotificationService/UpdateConfiguration", ReplyAction="http://tempuri.org/INotificationService/UpdateConfigurationResponse")]
        void UpdateConfiguration();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/INotificationService/UpdateConfiguration", ReplyAction="http://tempuri.org/INotificationService/UpdateConfigurationResponse")]
        System.Threading.Tasks.Task UpdateConfigurationAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/INotificationService/GetConfigurationInfo", ReplyAction="http://tempuri.org/INotificationService/GetConfigurationInfoResponse")]
        QA.ProductCatalog.Integration.Notifications.ConfigurationInfo GetConfigurationInfo();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/INotificationService/GetConfigurationInfo", ReplyAction="http://tempuri.org/INotificationService/GetConfigurationInfoResponse")]
        System.Threading.Tasks.Task<QA.ProductCatalog.Integration.Notifications.ConfigurationInfo> GetConfigurationInfoAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface INotificationServiceChannel : QA.ProductCatalog.Integration.Notifications.INotificationService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class NotificationServiceClient : System.ServiceModel.ClientBase<QA.ProductCatalog.Integration.Notifications.INotificationService>, QA.ProductCatalog.Integration.Notifications.INotificationService {
        
        public NotificationServiceClient() {
        }
        
        public NotificationServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public NotificationServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public NotificationServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public NotificationServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void PushNotifications(QA.ProductCatalog.Integration.Notifications.NotificationItem[] notifications, bool isStage, int userId, string userName, string method) {
            base.Channel.PushNotifications(notifications, isStage, userId, userName, method);
        }
        
        public System.Threading.Tasks.Task PushNotificationsAsync(QA.ProductCatalog.Integration.Notifications.NotificationItem[] notifications, bool isStage, int userId, string userName, string method) {
            return base.Channel.PushNotificationsAsync(notifications, isStage, userId, userName, method);
        }
        
        public void UpdateConfiguration() {
            base.Channel.UpdateConfiguration();
        }
        
        public System.Threading.Tasks.Task UpdateConfigurationAsync() {
            return base.Channel.UpdateConfigurationAsync();
        }
        
        public QA.ProductCatalog.Integration.Notifications.ConfigurationInfo GetConfigurationInfo() {
            return base.Channel.GetConfigurationInfo();
        }
        
        public System.Threading.Tasks.Task<QA.ProductCatalog.Integration.Notifications.ConfigurationInfo> GetConfigurationInfoAsync() {
            return base.Channel.GetConfigurationInfoAsync();
        }
    }
}
