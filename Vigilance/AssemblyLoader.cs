// This code is the only edit made in the Assembly-CSharp.dll
			try
			{
				string path = $"{Application.dataPath}/Managed/Vigilance.dll";
				bool exists = File.Exists(path);
				ServerConsole.AddLog($"Vigilance.dll {(exists ? "has been found" : "has not been found")}", exists ? ConsoleColor.Green : ConsoleColor.Red);
				if (exists)
				{
					ServerConsole.AddLog("Loading Vigilance.dll ...", ConsoleColor.DarkCyan);
					Assembly assembly = Assembly.LoadFrom(path);
					if (assembly != null)
					{
						Type type = assembly.GetType("Vigilance.PluginManager");
						if (type != null)
						{
							MethodInfo method = type.GetMethod("Enable");
							if (method != null)
							{
								method.Invoke(null, null);
							}
						}
						else
						{
							ServerConsole.AddLog("Cannot find Vigilance.PluginManager.Enable!", ConsoleColor.Red);
						}
					}
					else
					{
						ServerConsole.AddLog("Cannot find Vigilance.PluginManager!", ConsoleColor.Red);
					}
				}
				else
				{
					ServerConsole.AddLog("Cannot find Vigilance.dll!", ConsoleColor.Red);
				}
			}
			catch (Exception e)
			{
				ServerConsole.AddLog(e.ToString(), ConsoleColor.DarkRed);
			}