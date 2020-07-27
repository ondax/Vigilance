﻿using System;
using System.Collections.Generic;
using Vigilance.API.Events;
using Vigilance.API.Handlers;

namespace Vigilance
{
	public static class EventController
	{
		public static void StartEvent<T>(Event ev) where T : Handler
		{
			foreach (T t in EventController.GetHandlers<T>())
			{
				try
				{
					ev.ExecuteHandler(t);
				}
				catch (Exception ex)
				{
					Log.Error("EventController", string.Format("Failed to handle {0}", ev));
					Log.Error("EventController", ex.ToString());
				}
			}
		}

		public static void AddHandler(Plugin plugin, Handler handler)
		{
			foreach (Type type in handler.GetType().GetInterfaces())
			{
				if (typeof(Handler).IsAssignableFrom(type))
				{
					EventController.AddEventHandler(plugin, type, handler);
				}
			}
		}

		public static void AddEventHandler(Plugin plugin, Type eventType, Handler handler)
		{
			EventHandlerWrapper wrapper = new EventHandlerWrapper(plugin, handler);
			if (!EventController.snapshots.ContainsKey(plugin))
			{
				EventController.snapshots.Add(plugin, new Snapshot());
			}
			EventController.snapshots[plugin].Entries.Add(new Snapshot.SnapshotEntry(eventType, wrapper));
			EventController.AddEventMeta(eventType, wrapper, handler);
		}

		public static void AddEventMeta(Type eventType, EventHandlerWrapper wrapper, Handler handler)
		{
			if (!EventController.event_meta.ContainsKey(eventType))
			{
				EventController.event_meta.Add(eventType, new List<EventHandlerWrapper>
				{
					wrapper
				});
				return;
			}
			EventController.event_meta[eventType].Add(wrapper);
		}

		public static List<T> GetHandlers<T>() where T : Handler
		{
			List<T> list = new List<T>();
			if (EventController.event_meta.ContainsKey(typeof(T)))
			{
				using (List<EventHandlerWrapper>.Enumerator enumerator = EventController.event_meta[typeof(T)].GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Handler handler;
						if ((handler = enumerator.Current.Handler) is T)
						{
							T item = (T)((object)handler);
							list.Add(item);
						}
					}
				}
				return list;
			}
			return list;
		}

		private static Dictionary<Type, List<EventHandlerWrapper>> event_meta = new Dictionary<Type, List<EventHandlerWrapper>>();
		private static readonly Dictionary<Plugin, Snapshot> snapshots = new Dictionary<Plugin, Snapshot>();
	}
}
