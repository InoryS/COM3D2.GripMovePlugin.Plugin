using System;
using System.IO;
using ExIni;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200003F RID: 63
	internal class Settings
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x0600024C RID: 588 RVA: 0x00003B08 File Offset: 0x00001D08
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

		// Token: 0x0600024E RID: 590 RVA: 0x00003B4A File Offset: 0x00001D4A
		public Settings()
		{
			this.Load();
		}

		// Token: 0x0600024F RID: 591 RVA: 0x0000EF38 File Offset: 0x0000D138
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

		// Token: 0x06000250 RID: 592 RVA: 0x0000EF98 File Offset: 0x0000D198
		public EVRButtonId GetButton(string key, EVRButtonId defaultButton)
		{
			string buttonById = this.GetButtonById(key);
			if ("Grip".Equals(buttonById))
			{
				return EVRButtonId.k_EButton_Grip;
			}
			if ("Trigger".Equals(buttonById))
			{
				return EVRButtonId.k_EButton_Axis1;
			}
			if ("Touchpad".Equals(buttonById))
			{
				return EVRButtonId.k_EButton_Axis0;
			}
			if ("ApplicationMenu".Equals(buttonById))
			{
				return EVRButtonId.k_EButton_ApplicationMenu;
			}
			return defaultButton;
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000EFEC File Offset: 0x0000D1EC
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

		// Token: 0x06000252 RID: 594 RVA: 0x0000F020 File Offset: 0x0000D220
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

		// Token: 0x06000253 RID: 595 RVA: 0x00003B58 File Offset: 0x00001D58
		private void Load()
		{
			if (File.Exists(Settings.IniFileName))
			{
				this.file = IniFile.FromFile(Settings.IniFileName);
				return;
			}
			this.file = IniFile.FromString("[GripMovePlugin]");
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000F07C File Offset: 0x0000D27C
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

		// Token: 0x06000255 RID: 597 RVA: 0x00003B87 File Offset: 0x00001D87
		public void SetBoolValue(string keyString, bool value)
		{
			this.SetStringValue(keyString, value.ToString());
		}

		// Token: 0x06000256 RID: 598 RVA: 0x00003B97 File Offset: 0x00001D97
		public void SetFloatValue(string keyString, float value)
		{
			this.SetStringValue(keyString, value.ToString());
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000F0D4 File Offset: 0x0000D2D4
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

		// Token: 0x04000196 RID: 406
		private static Settings instance;

		// Token: 0x04000197 RID: 407
		private IniFile file;

		// Token: 0x04000198 RID: 408
		public static readonly string IniDirectory = GripMovePlugin.PluginDataPath + "\\";

		// Token: 0x04000199 RID: 409
		public static readonly string IniFileName = Settings.IniDirectory + "gripmoveplugin.ini";
	}
}
