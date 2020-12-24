﻿using System;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace KeyChanger
{
	public class Config
	{
		public bool UseSSC = true;										// |Enabling grants legacy functionality, disabling uses the safer drop method
		public bool EnableRegionExchanges = false;						// |Default set to false
		public bool MarketMode = false;									// |Use only a general market region
		//public bool EnableGoldKey = true;								// |Gold Key works similar to a currency
		public bool EnableTempleKey = true;
		public bool EnableJungleKey = true;
		public bool EnableCorruptionKey = true;
		public bool EnableCrimsonKey = true;
		public bool EnableHallowedKey = true;
		public bool EnableFrozenKey = true;
		public bool EnableGoldenKey = true;
		public bool EnableShadowKey = true;
		public bool EnableLightKey = true;
		public bool EnableNightKey = true;
		public bool EnableDesertKey = true;
		public bool EnableLootboxMode = true;							// |If set true, you'll get a random amount of items defined by 'ItemCountGiven' below. If set to false, player will get every possible content from the used key, regardless of 'ItemCountGiven'.

		public int[] TempleKeyItem = new int[] { 1293 };				// |Lihzahrd Power Cell
		public int[] JungleKeyItem = new int[] { 1156 };				// |Piranha Gun
		public int[] CorruptionKeyItem = new int[] { 1571 };			// |Scourge of the Corruptor
		public int[] CrimsonKeyItem = new int[] { 1569 };				// |Vampire Knifes
		public int[] HallowedKeyItem = new int[] { 1260 };				// |Rainbow Gun
		public int[] FrozenKeyItem = new int[] { 1572 };                // |Frost Hydra Staff
		public int[] GoldenKeyItem = new int[] { };						// |Golden Key
		public int[] ShadowKeyItem = new int[] { };						// |Shadow Key
		public int[] LightKeyItem = new int[] { };					    // |Key of Light
		public int[] NightKeyItem = new int[] { };                      // |Key of Night
		public int[] DesertKeyItem = new int[] { };                     // |Key of Night
		public int ItemCountGiven = 1 ;									// |Stablish the amount of random items given when EnableLootBoxMode = true. Value can't be less than 1 or higher than the amount of items specified within a key.

		// Those are optional; They're only needed if EnableRegionExchanges is set to true. Default is set to null,
		// so that players can be informed of non-existing regions.
		public string MarketRegion = null;
		public string TempleRegion = null;
		public string JungleRegion = null;
		public string CorruptionRegion = null;
		public string CrimsonRegion = null;
		public string HallowedRegion = null;
		public string FrozenRegion = null;
		public string GoldenRegion = null;
		public string ShadowRegion = null;
		public string LightRegion = null;
		public string NightRegion = null;
		public string DesertRegion = null;

		public static Config Read(string savepath = "")
		{
			// Default to tshock path if no path is given
			if (String.IsNullOrWhiteSpace(savepath))
			{
				savepath = TShock.SavePath;
			}

			// Append the file name to the given path
			savepath = Path.Combine(savepath, "KeyChangerConfig.json");

			// Creates any missing folders
			Directory.CreateDirectory(Path.GetDirectoryName(savepath));

			try
			{
				Config file = new Config();
				if (File.Exists(savepath))
				{
					// Get data from the file
					file = JsonConvert.DeserializeObject<Config>(File.ReadAllText(savepath));
				}
				else
				{
					TShock.Log.ConsoleInfo("Creando archivo de configuracion para KeyChangerSSC...");
				}

				// Create or update the file to fix missing options
				File.WriteAllText(savepath, JsonConvert.SerializeObject(file, Formatting.Indented));
				return file;
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError(ex.ToString());
				return new Config();
			}
		}

		public bool Write(string savepath = "")
		{
			if (String.IsNullOrWhiteSpace(savepath))
			{
				savepath = TShock.SavePath;
			}

			savepath = Path.Combine(savepath, "KeyChangerConfig.json");

			Directory.CreateDirectory(Path.GetDirectoryName(savepath));

			try
			{
				File.WriteAllText(savepath, JsonConvert.SerializeObject(this, Formatting.Indented));
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError(ex.ToString());
				return false;
			}
		}
	}
}
