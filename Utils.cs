using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace KeyChanger
{
	class Utils
	{
		/// <summary>
		/// Loads a key from KeyChangerConfig.json.
		/// </summary>
		/// <param name="type">The type of key to load.</param>
		/// <returns>The key with all the required data.</returns>
		public static Key LoadKey(KeyTypes type)
		{
			Key key;
			switch (type)
			{
				case KeyTypes.Templo:
					key = new Key("templo", KeyTypes.Templo, KeyChanger.Config.EnableTempleKey);
					key.Items = GetItems(KeyChanger.Config.TempleKeyItem);
					key.Region = TShock.Regions.GetRegionByName(KeyChanger.Config.TempleRegion);
					break;
				case KeyTypes.Jungla:
					key = new Key("jungla", KeyTypes.Jungla, KeyChanger.Config.EnableJungleKey);
					key.Items = GetItems(KeyChanger.Config.JungleKeyItem);
					key.Region = TShock.Regions.GetRegionByName(KeyChanger.Config.JungleRegion);
					break;
				case KeyTypes.Corrupto:
					key = new Key("corrupto", KeyTypes.Corrupto, KeyChanger.Config.EnableCorruptionKey);
					key.Items = GetItems(KeyChanger.Config.CorruptionKeyItem);
					key.Region = TShock.Regions.GetRegionByName(KeyChanger.Config.CorruptionRegion);
					break;
				case KeyTypes.Carmesi:
					key = new Key("carmesi", KeyTypes.Carmesi, KeyChanger.Config.EnableCrimsonKey);
					key.Items = GetItems(KeyChanger.Config.CrimsonKeyItem);
					key.Region = TShock.Regions.GetRegionByName(KeyChanger.Config.CrimsonRegion);
					break;
				case KeyTypes.Sagrado:
					key = new Key("sagrado", KeyTypes.Sagrado, KeyChanger.Config.EnableHallowedKey);
					key.Items = GetItems(KeyChanger.Config.HallowedKeyItem);
					key.Region = TShock.Regions.GetRegionByName(KeyChanger.Config.HallowedRegion);
					break;
				case KeyTypes.Helado:
					key = new Key("helado", KeyTypes.Helado, KeyChanger.Config.EnableFrozenKey);
					key.Items = GetItems(KeyChanger.Config.FrozenKeyItem);
					key.Region = TShock.Regions.GetRegionByName(KeyChanger.Config.FrozenRegion);
					break;
				default:
					return null;
			}
			return key;
		}

		/// <summary>
		/// Returns a list of Terraria.Item from a list of Item ids.
		/// </summary>
		/// <param name="id">The int[] containing the Item ids.</param>
		/// <returns>List[Item]</returns>
		public static List<Item> GetItems(int[] id)
		{
			List<Item> list = new List<Item>();
			foreach (int item in id)
			{
				list.Add(TShock.Utils.GetItemById(item));
			}
			return list;
		}

		/// <summary>
		/// Handles error messages thrown by erroneous / lack of parameters by checking a player's group permissions.
		/// </summary>
		/// <param name="ply">The player executing the command.</param>
		/// <returns>A string matching the error message.</returns>
		public static string ErrorMessage(TSPlayer ply)
		{
			string error;
			var list = new List<string>()
			{
				ply.HasPermission("botin.abrir") ? "abrir" : null,
				ply.HasPermission("botin.reload") ? "reload" : null,
				ply.HasPermission("botin.modo") ? "modo" : null,
				"lista"
			};

			string valid = String.Join("/", list.FindAll(i => i != null));
			error = String.Format("Error de uso. Modo de uso: {0}botin <{1}> [tipo]", Commands.Specifier, valid);
			return error;
		}
	}
}
