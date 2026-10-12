using System;
using System.IO;
using ExIni;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class Settings
	{
		public static Settings Instance
		{
			get
			{
				if (Settings.instance == null)
				{
					Settings.instance = new Settings();
				}
				return Settings.instance;
			}
		}

		public Settings()
		{
			this.Load();
		}

		public bool GetBoolValue(string keyString, bool defaultValue)
		{
			IniSection section = this.file.GetSection("GripMovePlugin");
			if (section != null)
			{
				IniKey key = section.GetKey(keyString);
				if (key != null)
				{
					string value = key.Value;
					if (value != null)
					{
						if (value.ToLower().Equals("true"))
						{
							return true;
						}
						if (value.ToLower().Equals("false"))
						{
							return false;
						}
					}
				}
			}
			return defaultValue;
		}

		public EVRButtonId GetButton(string key, EVRButtonId defaultButton)
		{
			string buttonById = this.GetButtonById(key);
			if ("Grip".Equals(buttonById))
			{
				return (EVRButtonId)2;
			}
			if ("Trigger".Equals(buttonById))
			{
				return (EVRButtonId)33;
			}
			if ("Touchpad".Equals(buttonById))
			{
				return (EVRButtonId)32;
			}
			if ("ApplicationMenu".Equals(buttonById))
			{
				return (EVRButtonId)1;
			}
			return defaultButton;
		}

		private string GetButtonById(string keyString)
		{
			IniSection section = this.file.GetSection("GripMovePlugin");
			if (section != null)
			{
				IniKey key = section.GetKey(keyString);
				if (key != null)
				{
					return key.Value;
				}
			}
			return null;
		}

		public float GetFloatValue(string keyString, float defaultValue)
		{
			IniSection section = this.file.GetSection("GripMovePlugin");
			if (section != null)
			{
				IniKey key = section.GetKey(keyString);
				if (key != null)
				{
					string value = key.Value;
					if (value != null)
					{
						float num;
						try
						{
							num = float.Parse(value);
						}
						catch (Exception)
						{
							return defaultValue;
						}
						return num;
					}
				}
			}
			return defaultValue;
		}

		private void Load()
		{
			if (File.Exists(Settings.IniFileName))
			{
				this.file = IniFile.FromFile(Settings.IniFileName);
				return;
			}
			this.file = IniFile.FromString("[GripMovePlugin]");
		}

		public void Save()
		{
			try
			{
				if (!Directory.Exists(Settings.IniDirectory))
				{
					Directory.CreateDirectory(Settings.IniDirectory);
				}
				this.file.Save(Settings.IniFileName);
			}
			catch (Exception)
			{
				Console.WriteLine("Failed to save: {0}", Settings.IniFileName);
			}
		}

		public void SetBoolValue(string keyString, bool value)
		{
			this.SetStringValue(keyString, value.ToString());
		}

		public void SetFloatValue(string keyString, float value)
		{
			this.SetStringValue(keyString, value.ToString());
		}

		public void SetStringValue(string keyString, string value)
		{
			IniSection iniSection = this.file.GetSection("GripMovePlugin");
			if (iniSection != null)
			{
				iniSection = this.file.CreateSection("GripMovePlugin");
			}
			(iniSection.GetKey(keyString) ?? iniSection.CreateKey(keyString)).Value = value;
			this.Save();
		}

		private static Settings instance;

		private IniFile file;

		public static readonly string IniDirectory = GripMovePlugin.PluginDataPath + "\\";

		public static readonly string IniFileName = Settings.IniDirectory + "gripmoveplugin.ini";
	}
}
