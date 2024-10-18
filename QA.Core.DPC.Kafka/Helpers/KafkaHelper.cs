using Confluent.Kafka;
using QA.Core.DPC.Kafka.Services;
using NLog;

namespace QA.Core.DPC.Kafka.Helpers;

public class KafkaHelper
{
    
    public static void LogSysLogMessage(ILogger logger, LogMessage message)
    {
        switch (message.Level)
        {
            case SyslogLevel.Alert:
            case SyslogLevel.Emergency:
            case SyslogLevel.Critical:
                logger.ForFatalEvent().Message(message.Message).Log();
                break;
            case SyslogLevel.Error:
                logger.ForErrorEvent().Message(message.Message).Log();
                break;
            case SyslogLevel.Warning:
                logger.ForWarnEvent().Message(message.Message).Log();
                break;
            case SyslogLevel.Info:
            case SyslogLevel.Notice:
                logger.ForInfoEvent().Message(message.Message).Log();
                break;
            case SyslogLevel.Debug:
                logger.ForDebugEvent().Message(message.Message).Log();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}