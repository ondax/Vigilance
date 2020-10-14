using System;
using System.Collections.Generic;

namespace Vigilance
{
	public static class EventManager
	{
		private static Dictionary<Type, List<Wrapper>> _events;
		private static Dictionary<Plugin, Snapshot> _snapshots;

		public static void Enable()
		{
			_events = new Dictionary<Type, List<Wrapper>>();
			_snapshots = new Dictionary<Plugin, Snapshot>();
		}

		public static void Trigger<T>(Event ev) where T : EventHandler
		{
			foreach (T t in GetHandlers<T>())
			{
				try
				{
					ev.Execute(t);
				}
				catch (Exception e)
				{
					Log.Add($"{e.Source}", $"An error occured while handling \"{ev.GetType().Name}\"!", LogType.Error);
					Log.Add($"{e.Source}", e);
				}
			}
		}

		public static void RegisterHandler(Plugin plugin, EventHandler handler)
		{
			foreach (Type type in handler.GetType().GetInterfaces())
			{
				if (typeof(EventHandler).IsAssignableFrom(type))
				{
					RegisterHandler(plugin, type, handler);
				}
			}
		}

		public static void UnregisterHandler(Plugin plugin, EventHandler handler)
		{
			foreach (Type type in handler.GetType().GetInterfaces())
			{
				if (typeof(EventHandler).IsAssignableFrom(type))
				{
					UnregisterHandler(plugin, type, handler);
				}
			}
		}

		public static void UnregisterHandlers(Plugin plugin)
        {
			if (!_snapshots.ContainsKey(plugin))
				return;
			List<Type> events = new List<Type>();
			foreach (Snapshot.Entry entry in _snapshots[plugin].Entries)
				events.Add(entry.Type);
			_snapshots[plugin].Active = false;
			_snapshots[plugin].Entries.Clear();
			foreach (Type type in events)
            {
				foreach (Wrapper wrapper in _events[type])
                {
					UnregisterHandler(plugin, wrapper.Handler);
                }
				_events.Remove(type);
            }
			_snapshots.Remove(plugin);
        }

		public static void RegisterHandler(Plugin plugin, Type eventType, EventHandler handler)
		{
			Wrapper wrapper = new Wrapper(plugin, handler);
			if (!_snapshots.ContainsKey(plugin))
				_snapshots.Add(plugin, new Snapshot());
			_snapshots[plugin].Entries.Add(new Snapshot.Entry(eventType, wrapper));
			AddEvent(eventType, wrapper, handler);
		}

		public static void UnregisterHandler(Plugin plugin, Type eventType, EventHandler handler)
        {
			if (!_snapshots.ContainsKey(plugin) || !_events.ContainsKey(eventType))
				return;
			foreach (Snapshot.Entry entry in _snapshots[plugin].Entries)
            {
				if (entry.Type == eventType)
                {
					_snapshots[plugin].Entries.Remove(entry);
                }
            }
			
			foreach (Wrapper wrapper in _events[eventType])
            {
				if (wrapper.Handler == handler && wrapper.Plugin == plugin)
                {
					_events[eventType].Remove(wrapper);
                }
            }
		}

		public static void AddEvent(Type eventType, Wrapper wrapper, EventHandler handler)
		{
			if (!_events.ContainsKey(eventType))
			{
				_events.Add(eventType, new List<Wrapper> { wrapper });
				return;
			}
			_events[eventType].Add(wrapper);
		}

		public static List<T> GetHandlers<T>() where T : EventHandler
		{
			List<T> list = new List<T>();
			if (_events.ContainsKey(typeof(T)))
			{
				using (List<Wrapper>.Enumerator enumerator = _events[typeof(T)].GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						EventHandler handler;
						if ((handler = enumerator.Current.Handler) is T)
						{
							T item = (T)handler;
							list.Add(item);
						}
					}
				}
				return list;
			}
			return list;
		}
	}

    public abstract class Event
    {
        public abstract void Execute(EventHandler handler);
    }

    public interface EventHandler
    {

    }

    public class Wrapper
    {
        public EventHandler Handler { get; }
        public Plugin Plugin { get; }

        public Wrapper(Plugin plugin, EventHandler handler)
        {
            Plugin = plugin;
            Handler = handler;
        }
    }

    public class Snapshot
    {
        public List<Entry> Entries { get; private set; }
        public bool Active { get; set; }

        public Snapshot()
        {
            Entries = new List<Entry>();
        }

        public class Entry
        {
            public Type Type { get; }
            public Wrapper Wrapper { get; }
            public Entry(Type type, Wrapper wrapper)
            {
                Type = type;
                Wrapper = wrapper;
            }
        }
    }
}
