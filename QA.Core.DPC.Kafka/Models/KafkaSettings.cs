﻿using Confluent.Kafka;

namespace QA.Core.DPC.Kafka.Models
{
    public class KafkaSettings
    {
        /// <summary>
        /// -1 - all
        /// 0 - None
        /// 1 - leader
        /// </summary>
        public Acks Acks { get; set; } = Acks.All;
        /// <summary>
        /// IP:Port without schema
        /// </summary>
        public string? BootstrapServers { get; set; }
        public int MessageSendMaxRetries { get; set; } = int.MaxValue;
        
        public int RequestTimeoutMs { get; set; } = 30_000;
        
        public int MessageTimeoutMs { get; set; } = 120_000;

        public int RetryBackoffMs { get; set; } = 10_000;
        /// <summary>
        /// 0 - Plaintext
        /// 1 - Ssl
        /// 2 - SaslPlaintext
        /// 3 - SaslSsl
        /// </summary>
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;
        public string SaslUsername { get; set; } = string.Empty;
        public string SaslPassword { get; set; } = string.Empty;
        
        public string TopicName { get; set; } = string.Empty;
        
        public bool CheckTopicExists { get; set; }
    }
}
