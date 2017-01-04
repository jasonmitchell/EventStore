using System;
using System.Linq;
using System.Collections.Generic;
using EventStore.Core.DataStructures;

namespace EventStore.Core.Services.PersistentSubscription
{
    public enum StartMessageResult
    {
        Success,
        SkippedDuplicate
    }

    public class OutstandingMessageCache
    {
        private readonly Dictionary<Guid, OutstandingMessage> _outstandingRequests;
        private readonly SortedDictionary<Tuple<DateTime, RetryableMessage>, bool> _byTime;
        private readonly SortedList<int, int> _bySequences;

        public class ByTypeComparer : IComparer<Tuple<DateTime, RetryableMessage>>
        {
            public int Compare(Tuple<DateTime, RetryableMessage> x, Tuple<DateTime, RetryableMessage> y)
            {
                if(x.Item1 != y.Item1) return x.Item1 < y.Item1 ? -1 : 1;
                return x.Item2.MessageId.CompareTo(y.Item2.MessageId);
            }
        }

        public OutstandingMessageCache()
        {
            _outstandingRequests = new Dictionary<Guid, OutstandingMessage>();
            _byTime = new SortedDictionary<Tuple<DateTime, RetryableMessage>, bool>(new ByTypeComparer());
            _bySequences = new SortedList<int, int>();
        }

        public int Count { get { return _outstandingRequests.Count; }}

        public void Remove(Guid messageId)
        {
            OutstandingMessage m;
            if (_outstandingRequests.TryGetValue(messageId, out m))
            {
                _outstandingRequests.Remove(messageId);
                _bySequences.Remove(m.ResolvedEvent.OriginalEventNumber);
            }
        }

        public void Remove(IEnumerable<Guid> messageIds)
        {
            foreach(var m in messageIds) Remove(m);
        }

        public StartMessageResult StartMessage(OutstandingMessage message, DateTime expires)
        {
            if (_outstandingRequests.ContainsKey(message.EventId))
                return StartMessageResult.SkippedDuplicate;
            Console.WriteLine("starting message " + message.EventId);
            _outstandingRequests[message.EventId] = message;
            _bySequences.Add(message.ResolvedEvent.OriginalEventNumber, message.ResolvedEvent.OriginalEventNumber);
            _byTime.Add(new Tuple<DateTime, RetryableMessage>(expires, new RetryableMessage(message.EventId, expires)), false);

            return StartMessageResult.Success;
        }

        public IEnumerable<OutstandingMessage> GetMessagesExpiringBefore(DateTime time)
        {
            while (_byTime.Count > 0)
            {
                var item = _byTime.Keys.First();
                Console.WriteLine("first is " + item.Item1 + " " + item.Item2.MessageId);
                if(item.Item1 > time) {
                    Console.WriteLine("breaking");
                    yield break;
                }
                _byTime.Remove(item);
                OutstandingMessage m;
                if (_outstandingRequests.TryGetValue(item.Item2.MessageId, out m))
                {
                    yield return _outstandingRequests[item.Item2.MessageId];
                    _outstandingRequests.Remove(item.Item2.MessageId);
                    _bySequences.Remove(m.ResolvedEvent.OriginalEventNumber);
                }
            }
        }

        public int GetLowestPosition()
        {
            //TODO is there a better way of doing this?
            if (_bySequences.Count == 0) return int.MinValue;
            return _bySequences.Values[0];
        }

        public bool GetMessageById(Guid id, out OutstandingMessage outstandingMessage)
        {
            return _outstandingRequests.TryGetValue(id, out outstandingMessage);
        }
    }
}