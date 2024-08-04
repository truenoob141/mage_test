using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace MageTest
{
    public class EventDispatcher
    {
        private readonly Dictionary<string, List<EventHandler>> _stringEvents = new();
        private readonly Dictionary<Type, List<EventHandler>> _events = new();

        public void Subscribe<T>(string key, Action<T> handler)
        {
            Subscribe(key, new ActionHandler<T>(handler));
        }

        public void Subscribe<T>(Action handler)
        {
            Subscribe(typeof(T), new ActionHandler(handler));
        }

        public void Subscribe<T>(Action<T> handler)
        {
            Subscribe(typeof(T), new ActionHandler<T>(handler));
        }

        public void Unsubscribe<T>(Action handler)
        {
            Unsubscribe(typeof(T), handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            Unsubscribe(typeof(T), handler);
        }

        public void Trigger<T>()
        {
            Trigger(typeof(T), Activator.CreateInstance<T>());
        }

        public void Trigger<T>(T arg)
        {
            Trigger(typeof(T), arg);
        }

        private void Trigger(string key, object arg)
        {
            List<EventHandler> handlers;
            if (!_stringEvents.TryGetValue(key, out handlers))
                return;

            foreach (var handler in handlers.ToArray())
            {
                try
                {
                    handler.Invoke(arg);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void Trigger(Type type, object arg)
        {
            List<EventHandler> handlers;
            if (!_events.TryGetValue(type, out handlers))
                return;

            foreach (var handler in handlers.ToArray())
            {
                try
                {
                    handler.Invoke(arg);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public async UniTask WaitTrigger<T>(CancellationToken token)
        {
            var type = typeof(T);

            var utcs = new UniTaskCompletionSource();
            var handler = new UtcsHandler(utcs);

            List<EventHandler> handlers;
            if (!_events.TryGetValue(type, out handlers))
            {
                handlers = new List<EventHandler>(1);
                _events.Add(type, handlers);
            }

            handlers.Add(handler);
            try
            {
                await utcs.Task.AttachExternalCancellation(token);
            }
            finally
            {
                handlers.Remove(handler);
            }
        }

        public async UniTask WaitTrigger<T>(Func<T, bool> predicate, CancellationToken token)
        {
            var type = typeof(T);

            var utcs = new UniTaskCompletionSource();
            var handler = new UtcsHandler<T>(utcs, predicate);

            List<EventHandler> handlers;
            if (!_events.TryGetValue(type, out handlers))
            {
                handlers = new List<EventHandler>(1);
                _events.Add(type, handlers);
            }

            handlers.Add(handler);
            try
            {
                await utcs.Task.AttachExternalCancellation(token);
            }
            finally
            {
                handlers.Remove(handler);
            }
        }
    
        public async UniTask<T> WaitTriggerResult<T>(CancellationToken token)
            where T : class
        {
            var type = typeof(T);

            var utcs = new UniTaskCompletionSource<T>();
            var handler = new UtcsResultHandler<T>(utcs);

            List<EventHandler> handlers;
            if (!_events.TryGetValue(type, out handlers))
            {
                handlers = new List<EventHandler>(1);
                _events.Add(type, handlers);
            }

            handlers.Add(handler);
            try
            {
                return await utcs.Task.AttachExternalCancellation(token);
            }
            finally
            {
                handlers.Remove(handler);
            }
        }

        private void Subscribe(string key, EventHandler handler)
        {
            List<EventHandler> handlers;
            if (!_stringEvents.TryGetValue(key, out handlers))
            {
                handlers = new List<EventHandler>(1);
                _stringEvents.Add(key, handlers);
            }

            Assert.IsFalse(handlers.Any(h => h.Handler.Equals(handler.Handler)),
                "Already subscribe " + handler.Handler);

            handlers.Add(handler);
        }

        private void Subscribe(Type type, EventHandler handler)
        {
            List<EventHandler> handlers;
            if (!_events.TryGetValue(type, out handlers))
            {
                handlers = new List<EventHandler>(1);
                _events.Add(type, handlers);
            }

            if (handlers.Any(h => h.Handler.Equals(handler.Handler)))
                throw new ArgumentException($"Already subscribe {type}  {handler.Handler}");

            handlers.Add(handler);
        }

        private void Unsubscribe(string key, object handler)
        {
            List<EventHandler> handlers;
            if (!_stringEvents.TryGetValue(key, out handlers))
                return;

            if (handlers.RemoveAll(h => h.Handler.Equals(handler)) > 0)
            {
                if (handlers.Count == 0)
                    _stringEvents.Remove(key);
            }
        }

        private void Unsubscribe(Type type, object handler)
        {
            List<EventHandler> handlers;
            if (!_events.TryGetValue(type, out handlers))
                return;

            if (handlers.RemoveAll(h => h.Handler.Equals(handler)) > 0)
            {
                if (handlers.Count == 0)
                    _events.Remove(type);
            }
        }

        private abstract class EventHandler
        {
            public abstract object Handler { get; }

            public abstract void Invoke(object arg);
        }

        private class ActionHandler : EventHandler
        {
            public override object Handler => _handler;

            private readonly Action _handler;

            public ActionHandler(Action handler)
            {
                this._handler = handler;
            }

            public override void Invoke(object arg)
            {
                _handler();
            }
        }

        private class ActionHandler<T> : EventHandler
        {
            public override object Handler => _handler;

            private readonly Action<T> _handler;

            public ActionHandler(Action<T> handler)
            {
                this._handler = handler;
            }

            public override void Invoke(object arg)
            {
                _handler((T) arg);
            }
        }

        private class UtcsHandler : EventHandler
        {
            public override object Handler => _handler;

            private readonly Action _handler;
            private readonly UniTaskCompletionSource _utcs;

            public UtcsHandler(UniTaskCompletionSource utcs)
            {
                this._handler = () => utcs.TrySetResult();
            }

            public override void Invoke(object arg)
            {
                _handler();
            }
        }

        private class UtcsHandler<T> : EventHandler
        {
            public override object Handler => _handler;

            private readonly Action<object> _handler;
            private readonly UniTaskCompletionSource _utcs;

            public UtcsHandler(UniTaskCompletionSource utcs, Func<T, bool> predicate)
            {
                this._handler = (arg) =>
                {
                    if (predicate((T) arg))
                        utcs.TrySetResult();
                };
            }

            public override void Invoke(object arg)
            {
                _handler((T) arg);
            }
        }
    
        private class UtcsResultHandler<T> : EventHandler where T : class
        {
            public override object Handler => _handler;

            private readonly Action<T> _handler;
            private readonly UniTaskCompletionSource<T> _utcs;

            public UtcsResultHandler(UniTaskCompletionSource<T> utcs)
            {
                this._handler = arg => utcs.TrySetResult(arg);
            }

            public override void Invoke(object arg)
            {
                _handler(arg as T);
            }
        }
    }
}