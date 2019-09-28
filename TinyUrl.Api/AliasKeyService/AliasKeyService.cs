using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TinyUrl.Data.Models;
using TinyUrl.Data.Repositories;

namespace TinyUrl.Api.AliasKeyService
{
    public class AliasKeyService : IAliasKeyService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly SemaphoreSlim QueueSemaphore = new SemaphoreSlim(1, 1);
        private readonly Queue<string> AvailableAliasKeys = new Queue<string>();

        public AliasKeyService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private async Task GrabKeysIntoCache(int count = 100)
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetService<IUnitOfWork>();
            List<AliasKey> keys = await unitOfWork.AliasKeysRepository.GetRandomKeys(count);
            unitOfWork.AliasKeysRepository.Delete(keys); // go ahead and consider these keys no longer available as it is ok if we loose some between restarts
            await unitOfWork.SaveAsync();
            
            keys.ForEach(o => AvailableAliasKeys.Enqueue(o.Alias));
        }

        public async Task<string> GetKey()
        {
            string key = null;

            await QueueSemaphore.WaitAsync();

            // wrap in a try catch to ensure to semaphore gets released as a connection to the db could throw an exception
            try
            {
                // if the collection is at 0 (start of the app) or less then this value, load more into cache
                if (AvailableAliasKeys.Count < 25) // TODO: make this configurable
                    await GrabKeysIntoCache(100);

                key = AvailableAliasKeys.Dequeue();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                QueueSemaphore.Release();
            }

            return key;
        }

        public async Task SeedKeys(int length = 3, int batchSize = 10000)
        {
            // TODO: need to be able to recover and restart from where we left off if the app stops/crashes while performing this insert
            string start = "1".PadRight(length, '0');
            string next = start;
            List<AliasKey> keys = new List<AliasKey>();
            do
            {
                next = Increment(next);
                keys.Add(new AliasKey()
                {
                    Alias = next
                });

                if (keys.Count == batchSize) // perform inserts in batches
                {
                    await InsertKeys(keys);
                    keys.Clear();
                }
            } while (next != start);

            if (keys.Count != 0)
                await InsertKeys(keys);
        }

        private Task InsertKeys(List<AliasKey> keys)
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetService<IUnitOfWork>();
            unitOfWork.AliasKeysRepository.Insert(keys);
            return unitOfWork.SaveAsync();
        }

        private static readonly List<char> sequence = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToList();
        public static string Increment(string text)
        {
            char[] textArr = text.ToCharArray();

            // loop from the end to beginning
            for (int i = textArr.Length - 1; i >= 0; --i)
            {
                if (textArr[i] == sequence.Last())
                {
                    textArr[i] = sequence.First();
                }
                else
                {
                    textArr[i] = sequence[sequence.IndexOf(textArr[i]) + 1];
                    break;
                }
            }

            return new string(textArr);
        }
    }
}
