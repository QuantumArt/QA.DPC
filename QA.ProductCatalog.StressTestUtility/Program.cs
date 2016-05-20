using Microsoft.Practices.Unity;
using QA.ProductCatalog.StressTestUtility.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace QA.ProductCatalog.StressTestUtility
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Init container");

			using (var container = new UnityContainer())
			{
				container.AddNewExtension<UnityConfiguration>();

				Console.WriteLine("Init threads");
				var threads = new List<Thread>();
				var updateIds = new ConcurrentQueue<int>(Configuration.UpdateIds);
			
				var updateService = container.Resolve<IUpdateService>();
				for (int i = 0; i < Configuration.UpdateThreadsCount; i++)
				{
					//var updateThread = new Thread(() => UpdateArticles(Configuration.UpdateDelay, updateIds, updateService));
					var updateThread = new Thread(() => ProcessArticles(Configuration.UpdateDelay, Configuration.UpdateIds, id => updateService.Update(id)));
					updateThread.Name = "update_" + i;

					threads.Add(updateThread);
				}

				var publishService = container.Resolve<IPublishService>();
				for (int i = 0; i < Configuration.PublishThreadsCount; i++)
				{
					var publishThread = new Thread(() => ProcessArticles(Configuration.PublishDelay, Configuration.PublishIds, id => publishService.Publish(id)));
					publishThread.Name = "publish_" + i;
					threads.Add(publishThread);
				}

				var simplePublishService = container.Resolve<ISimplePublishService>();
				for (int i = 0; i < Configuration.SimplePublishThreadsCount; i++)
				{
					var simplePublishThread = new Thread(() => ProcessArticles(Configuration.SimplePublishDelay, Configuration.UpdateIds, id => simplePublishService.Publish(Configuration.UpdateIds)));
					simplePublishThread.Name = "simple_publish_" + i;
					threads.Add(simplePublishThread);
				}

				Console.WriteLine("Start update");
				foreach (var thread in threads)
				{
					thread.Start();
				}

				foreach (var thread in threads)
				{
					thread.Join();
				}
			}
		}

		public static void UpdateArticles(TimeSpan delay, ConcurrentQueue<int> ids, IUpdateService service)
		{
			while (true)
			{
				RandomDelay(delay);
				int id;

				if (ids.TryDequeue(out id))
				{
					service.Update(id);
					ids.Enqueue(id);
				}
			}
		}

		public static void ProcessArticles(TimeSpan delay, int[] ids, Action<int> callService)
		{
			while (true)
			{
				RandomDelay(delay);
				int id = GetRendomId(ids);
				callService(id);
			}
		}

		private static void RandomDelay(TimeSpan delay)
		{
			Random rnd = RandomProvider.GetThreadRandom();
			int randomDelay = (int)delay.TotalMilliseconds;
			randomDelay = rnd.Next(0, 2 * randomDelay);
			Thread.Sleep(randomDelay);
		}

		private static int GetRendomId(int[] ids)
		{
			Random rnd = RandomProvider.GetThreadRandom();
			int index = rnd.Next(0, ids.Length);
			return ids[index];
		}
	}
}
