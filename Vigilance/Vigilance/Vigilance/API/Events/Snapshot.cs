using System;
using System.Collections.Generic;

namespace Vigilance.API.Events
{
	public class Snapshot
	{
		public List<Snapshot.SnapshotEntry> Entries { get; private set; }
		public bool Active { get; set; }

		public Snapshot()
		{
			this.Entries = new List<Snapshot.SnapshotEntry>();
		}

		public class SnapshotEntry
		{
			public Type Type { get; }
			public EventHandlerWrapper Wrapper { get; }
			public SnapshotEntry(Type type, EventHandlerWrapper wrapper)
			{
				this.Type = type;
				this.Wrapper = wrapper;
			}
		}
	}
}
