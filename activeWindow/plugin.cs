using System;
using Xml = System.Xml;
using System.IO;
/**
 * 
 * @revision 
 * 20120428 修改plugin临时文件路径,修改前为系统临时文件夹，修改后为plugins目录下tmp文件夹
 * 
 */
namespace activeWindow
{
	/// <summary>
	/// This class handles all issues related to loading plugins on the fly
	/// </summary>
	public abstract class PluginLoader
	{
		#region FindAssembly()
		/// <summary>
		/// Populates the plugin definition file if necessary and returns the Type requested
		/// </summary>
		/// <param name="PluginClass">Unique identifer for this "kind" of plugin</param>
		/// <param name="Class">The class name requested</param>
		/// <param name="Folders">The folders to search through</param>
		/// <param name="PluginTypes">The valid Interfaces</param>
		/// <returns></returns>
		protected static Type FindAssembly(string PluginClass, string Class, string[] Folders, params Type[] PluginTypes)
		{
			// Loop through the folders
			foreach (string Folder in Folders)
			{
				// Load the file (or get a null if there's an error)
				Xml.XmlDocument PluginLibrary = LoadXmlFile(System.IO.Path.Combine(Folder, "plugins.xml"));

				// Create or update the file as necessary
				if (PluginLibrary == null)
					PluginLibrary = CreatePluginFile(Folder, PluginClass, PluginTypes);
				else
					PluginLibrary = UpdatePluginFile(Folder, PluginClass, PluginTypes);

				// If we were able to get the file...
				if (PluginLibrary != null)
				{
					// ... find the appropriate node for this class...
					Xml.XmlElement LibraryNode = (Xml.XmlElement)PluginLibrary.SelectSingleNode("plugins/active[@type='" + PluginClass + "']/plugin[@name='" + Class.ToLower() + "' or @fullname='" + Class.ToLower() + "']");
					if (LibraryNode != null)
					{
						// ... and load it into a System.Type object and return it
						System.Reflection.Assembly PluginAssembly = System.Reflection.Assembly.LoadFile(Path.GetFullPath(LibraryNode.InnerText));
						return PluginAssembly.GetType(LibraryNode.GetAttribute("fullname"), false, true);
					}
				}
			}

			return null;
		}


		protected static Type FindAssembly(string PluginClass, string Class, Type PluginType, params string[] Folders)
		{
			// Pass Through
			return FindAssembly(PluginClass, Class, Folders, PluginType);
		}
		

		protected static Type FindAssembly(string PluginClass, string Class, string Folder, params Type[] PluginTypes)
		{
			// Pass Through
			return FindAssembly(PluginClass, Class, new string[1] {Folder}, PluginTypes);
		}
		
		
		#endregion


		#region FindAllAssemblies()
		/// <summary>
		/// This returns all classes known about by this system
		/// </summary>
		/// <param name="PluginClass">Unique identifer for this "kind" of plugin</param>
		/// <param name="Class">The class name requested</param>
		/// <param name="Folders">The folders to search through</param>
		/// <param name="PluginTypes">The valid Interfaces</param>
		/// <returns></returns>
		protected static Type[] FindAllAssemblies(string PluginClass, string[] Folders, params Type[] PluginTypes)
		{
			// Set up our results list
			System.Collections.ArrayList Result = new System.Collections.ArrayList();

			// Loop through all folders
			foreach (string Folder in Folders)
			{
				Xml.XmlDocument PluginLibrary = LoadXmlFile(System.IO.Path.Combine(Folder, "plugins.xml"));

				// Create or update the plugin file as necessary
				if (PluginLibrary == null)
					PluginLibrary = CreatePluginFile(Folder, PluginClass, PluginTypes);
				else
					PluginLibrary = UpdatePluginFile(Folder, PluginClass, PluginTypes);

				if (PluginLibrary != null)
				{
					Xml.XmlNodeList LibraryList = PluginLibrary.SelectNodes("plugins/active[@type='" + PluginClass + "']/plugin");
					foreach (Xml.XmlElement LibraryNode in LibraryList)
					{
                        // Add each class to our results
                        try
                        {
                            System.Reflection.Assembly PluginAssembly = System.Reflection.Assembly.LoadFile(Path.GetFullPath(LibraryNode.InnerText));
                            Result.Add(PluginAssembly.GetType(LibraryNode.GetAttribute("fullname"), false, true));
                        }
                        catch (Exception e) {

                        }
					}
				}
			}

			// Convert results to an array at the last moment
			return (System.Type[])Result.ToArray(typeof(System.Type));
		}

		
		protected static Type[] FindAllAssemblies(string PluginClass, Type PluginType, string[] Folders)
		{
			// Pass Through
			return FindAllAssemblies(PluginClass, Folders, PluginType);
		}
		

		protected static Type[] FindAllAssemblies(string PluginClass, string Folder, params Type[] PluginTypes)
		{
			// Pass Through
			return FindAllAssemblies(PluginClass, new string[1] {Folder}, PluginTypes);
		}

		
		#endregion


		#region Private Methods

		/// <summary>
		/// Load up a new plugin file and populate it
		/// </summary>
		private static Xml.XmlDocument CreatePluginFile(string PluginFolder, string PluginClass, Type[] PluginTypes)
		{
			Xml.XmlDocument PluginLibrary = new Xml.XmlDocument();

			PluginLibrary.LoadXml("<plugins><retired/></plugins>");

			AddAssembliesToPluginFile(PluginFolder, PluginClass, PluginLibrary, PluginTypes);

			PluginLibrary.Save(System.IO.Path.Combine(PluginFolder, "plugins.xml"));
			return PluginLibrary;
		}
		

		/// <summary>
		/// Updates a plugin file, removing all dead classes
		/// </summary>
		private static Xml.XmlDocument UpdatePluginFile(string PluginFolder, string PluginClass, Type[] PluginTypes)
		{
			Xml.XmlDocument PluginLibrary = new Xml.XmlDocument();

			try
			{
				PluginLibrary.Load(System.IO.Path.Combine(PluginFolder, "plugins.xml"));
			}
			catch
			{
				PluginLibrary = CreatePluginFile(PluginFolder, PluginClass, PluginTypes);
			}

			bool FileChanged = false;

			foreach (string PluginFile in System.IO.Directory.GetFiles(PluginFolder, "*.dll"))
			{
				DateTime LastUpdate = new DateTime();
				try
				{
					LastUpdate = DateTime.Parse(((Xml.XmlElement)PluginLibrary.SelectSingleNode("/plugins/active[@type='" + PluginClass + "']")).GetAttribute("updated"));
				}
				catch
				{ }
				if (System.IO.File.GetLastWriteTime(PluginFile) > LastUpdate)
				{
					foreach (Xml.XmlElement OldAssembly in PluginLibrary.SelectNodes("/plugins/active[@type='" + PluginClass + "']/plugin"))
					{
						OldAssembly.ParentNode.RemoveChild(OldAssembly);
						PluginLibrary.SelectSingleNode("/plugins/retired").AppendChild(OldAssembly);
					}

					AddAssembliesToPluginFile(PluginFolder, PluginClass, PluginLibrary, PluginTypes);

					FileChanged = true;

					break;
				}
			}

			// Nuke Old Assemblies if the filesystem will let us
			foreach (Xml.XmlElement OldAssembly in PluginLibrary.SelectNodes("/plugins/retired/plugin"))
			{
				try
				{
					System.IO.File.Delete(OldAssembly.InnerText);
					OldAssembly.ParentNode.RemoveChild(OldAssembly);

					FileChanged = true;
				}
				catch 
				{
					// File Is In Use
				}
			}

			// We Changed the Plugin File, So Save It
			if (FileChanged)
				PluginLibrary.Save(System.IO.Path.Combine(PluginFolder, "plugins.xml"));

			return PluginLibrary;
		}
		

		/// <summary>
		/// Adds the active assemblies to a plugin file
		/// </summary>
		private static void AddAssembliesToPluginFile(string PluginFolder, string PluginClass, Xml.XmlDocument PluginLibrary, Type[] PluginTypes)
		{
			if (System.IO.Directory.Exists(PluginFolder))
			{
				foreach (string PluginFile in System.IO.Directory.GetFiles(PluginFolder, "*.dll"))
				{
					bool FoundOne = false;                    
					string OldFileName = PluginFile.Substring(PluginFile.LastIndexOf("\\") + 1);
                    string NewFileName =  ".\\plugins\\tmp\\";//huangxy

                    if (!Directory.Exists(NewFileName))
                    {
                        Directory.CreateDirectory(NewFileName);
                    }                    
                    NewFileName += OldFileName+".tmp";
                    
					File.Copy(PluginFile, NewFileName, true);

                    System.Reflection.Assembly PluginAssembly = System.Reflection.Assembly.LoadFile(Path.GetFullPath(NewFileName));
                    System.Type[] types =null;
                    try
                    {
                        types = PluginAssembly.GetTypes();
                    }catch(Exception e){
                        Console.WriteLine(e.Message);
                    }
                    if (types == null)
                        continue;
					foreach (System.Type NewType in types)
					{
						bool Found = false;

						foreach (System.Type InterfaceType in NewType.GetInterfaces())
							foreach (System.Type DesiredType in PluginTypes)
							{
								if (InterfaceType == DesiredType)
								{
								
									string ClassName = NewType.Name.ToLower();
									if (NewType.Namespace != null)
										ClassName = NewType.Namespace.ToLower() + "." + ClassName;

									FoundOne = true;
									Xml.XmlElement NewNode = PluginLibrary.CreateElement("plugin");
									NewNode.SetAttribute("library", OldFileName);
									NewNode.SetAttribute("interface", DesiredType.Name);
									NewNode.SetAttribute("name", NewType.Name.ToLower());
									NewNode.SetAttribute("fullname", ClassName);
									NewNode.AppendChild(PluginLibrary.CreateTextNode(NewFileName));

									Xml.XmlElement Parent = (Xml.XmlElement)PluginLibrary.SelectSingleNode("/plugins/active[@type='" + PluginClass + "']");
									if (Parent == null)
									{
										Parent = PluginLibrary.CreateElement("active");
										Parent.SetAttribute("type", PluginClass);
										PluginLibrary.SelectSingleNode("/plugins").AppendChild(Parent);
									}
									Parent.AppendChild(NewNode);
									Parent.SetAttribute("updated", System.DateTime.Now.ToString());

									Found = true;
									break;
								}
								if (Found) break;
							}
					}

					if (!FoundOne)
					{
						Xml.XmlElement NewNode = PluginLibrary.CreateElement("plugin");
						NewNode.AppendChild(PluginLibrary.CreateTextNode(NewFileName));

						PluginLibrary.SelectSingleNode("/plugins/retired").AppendChild(NewNode);
						PluginLibrary.DocumentElement.SetAttribute("updated", System.DateTime.Now.ToString());
					}
				}
			}
		}

	
		/// <summary>
		/// Loads an xml file, returning null if there's any problem
		/// </summary>
		private static Xml.XmlDocument LoadXmlFile(string Path)
		{
			if (System.IO.File.Exists(Path))
			{
				try
				{
					Xml.XmlDocument Result = new Xml.XmlDocument();
					Result.Load(Path);
					return Result;
				}
				catch
				{
					return null;
				}
			}
			else
				return null;
		}
		
		
		#endregion
	}
    class PluginBroker : PluginLoader
    {
        private PluginBroker() { }

        public static Object Load(string name, Type type)
        {
            Type InputElementClass = FindAssembly(type.ToString(), name, System.IO.Path.Combine(Environment.CurrentDirectory, "plugins"), type);
            if (InputElementClass == null)
                throw new Exception("Type Not Found");

            return InputElementClass.GetConstructor(System.Type.EmptyTypes).Invoke(System.Type.EmptyTypes);
        }

        public static string[] List(Type type)
        {
            Type[] TypeList = FindAllAssemblies(type.ToString(), System.IO.Path.Combine(Environment.CurrentDirectory, "plugins"), type);
            string[] Result = new string[TypeList.Length];
            for (int I = 0; I < TypeList.Length; I++)
                Result[I] = TypeList[I].ToString();

            return Result;
        }
    }
}
