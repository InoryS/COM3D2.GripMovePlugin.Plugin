using System;
using System.IO;
using ExIni;
using Valve.VR;

namespace COM3D2.GripMovePlugin.Plugin;

internal class Settings
{
	private static Settings instance;

	public static readonly string IniDirectory = GripMovePlugin.PluginDataPath + "\\";

	public static readonly string IniFileName = IniDirectory + "gripmoveplugin.ini";

	private IniFile file;

	public Settings()
	{
		Load();
	}

	public static Settings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Settings();
			}

			return instance;
		}
	}

	public bool GetBoolValue(string keyString, bool defaultValue)
	{
		IniSection section = file.GetSection("GripMovePlugin");
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
		string buttonById = GetButtonById(key);
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
		IniSection section = file.GetSection("GripMovePlugin");
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
		IniSection section = file.GetSection("GripMovePlugin");
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
		if (File.Exists(IniFileName))
		{
			file = IniFile.FromFile(IniFileName);
			return;
		}

		file = IniFile.FromString("[GripMovePlugin]");
	}

	public void Save()
	{
		try
		{
			if (!Directory.Exists(IniDirectory))
			{
				Directory.CreateDirectory(IniDirectory);
			}

			file.Save(IniFileName);
		}
		catch (Exception)
		{
			Console.WriteLine("Failed to save: {0}", IniFileName);
		}
	}

	public void SetBoolValue(string keyString, bool value) =>
		SetStringValue(keyString, value.ToString());

	public void SetFloatValue(string keyString, float value) =>
		SetStringValue(keyString, value.ToString());

	public void SetStringValue(string keyString, string value)
	{
		IniSection iniSection = file.GetSection("GripMovePlugin");
		if (iniSection != null)
		{
			iniSection = file.CreateSection("GripMovePlugin");
		}

		(iniSection.GetKey(keyString) ?? iniSection.CreateKey(keyString)).Value = value;
		Save();
	}
}
