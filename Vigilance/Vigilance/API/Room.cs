using UnityEngine;
using Vigilance.API.Enums;

namespace Vigilance.API
{
	public class Room
	{
		public Room(string name, Transform transform, Vector3 position)
		{
			this.name = name;
			this.transform = transform;
			this.position = position;
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public Transform Transform
		{
			get
			{
				return this.transform;
			}
		}

		public Vector3 Position
		{
			get
			{
				return this.position;
			}
		}

		public ZoneType Zone
		{
			get
			{
				ZoneType zoneType = ZoneType.Unspecified;
				if (zoneType != ZoneType.Unspecified)
				{
					return zoneType;
				}
				if (this.Position.y == -1997f)
				{
					zoneType = ZoneType.Unspecified;
				}
				if (this.Position.y >= 0f && this.Position.y < 500f)
				{
					zoneType = ZoneType.LightContainment;
				}
				if (this.Position.y < -100f && this.Position.y > -1000f)
				{
					zoneType = ZoneType.HeavyContainment;
				}
				if (this.Name.Contains("ENT") || this.Name.Contains("INTERCOM"))
				{
					zoneType = ZoneType.Entrance;
				}
				if (this.Position.y >= 5f)
				{
					zoneType = ZoneType.Surface;
				}
				return zoneType;
			}
		}

		private string name;
		private Transform transform;
		private Vector3 position;
	}
}
