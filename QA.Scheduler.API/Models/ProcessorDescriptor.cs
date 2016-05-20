namespace QA.Scheduler.API.Models
{
	public class ProcessorDescriptor
	{
		public string Processor { get; private set; }
		public string Service { get; private set; }
		public string Schedule { get; private set; }

		public ProcessorDescriptor(string processor, string service, string schedule)
		{
			Processor = processor;
			Service = service;
			Schedule = schedule;
		}
	}
}
