using System.Collections.Generic;

namespace Vigilance.API.Commands
{
	public class SnapshotEntry
	{
		public Dictionary<string, Command> Handlers { get; }
		public bool Enabled { get; set; }
		public SnapshotEntry()
		{
			this.Handlers = new Dictionary<string, Command>();
		}
	}
}
