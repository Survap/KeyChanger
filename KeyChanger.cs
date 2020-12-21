using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace KeyChanger
{
	[ApiVersion(2, 1)]
	public class KeyChanger : TerrariaPlugin
	{
		public override string Author => "Enerdy - Modified by Tsviets";

		public static Config Config { get; private set; }

		public override string Description => "Lootboxes System: Exchanges keys to award with random customizable prizes and rates.";

		public KeyTypes?[] Exchanging { get; private set; }

		public override string Name => "Lootboxes";

		public override Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

		public KeyChanger(Main game) : base(game) { }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, onInitialize);
				ServerApi.Hooks.NetGetData.Deregister(this, onGetData);
				ServerApi.Hooks.ServerLeave.Deregister(this, onLeave);
			}
		}

		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, onInitialize);
			ServerApi.Hooks.NetGetData.Register(this, onGetData, -1);
			ServerApi.Hooks.ServerLeave.Register(this, onLeave);
		}

		private void onGetData(GetDataEventArgs e)
		{
			if (Config.UseSSC || e.Handled ||e.MsgID != PacketTypes.ItemDrop || TShock.Players[e.Msg.whoAmI] == null
				|| !TShock.Players[e.Msg.whoAmI].Active || Exchanging[e.Msg.whoAmI] == null)
			{
				return;
			}

			using (var reader = new BinaryReader(new MemoryStream(e.Msg.readBuffer, e.Index, e.Length)))
			{
				int id = reader.ReadInt16();
				if (id == 400)				// 400 = new Item
				{
					reader.ReadSingle();	// positionX
					reader.ReadSingle();	// positionY
					reader.ReadSingle();	// velocityX
					reader.ReadSingle();	// positionY

					short stack = reader.ReadInt16();
					reader.ReadByte();		// prefix
					reader.ReadByte();		// no delay..?

					int netID = reader.ReadInt16();
					

					if (netID == (int)Exchanging[e.Msg.whoAmI])
					{
						// Check if the player has available slots and warn them if they do not
						if (!TShock.Players[e.Msg.whoAmI].InventorySlotAvailable)
						{
							TShock.Players[e.Msg.whoAmI].SendWarningMessage("Asegurate de tener espacio disponible en tu inventario antes abrir tu botin.");
							return;
						}

						Key key = Utils.LoadKey(Exchanging[e.Msg.whoAmI].Value);
						if (Config.EnableRegionExchanges)
						{
							Region region;
							if (Config.MarketMode)
								region = TShock.Regions.GetRegionByName(Config.MarketRegion);
							else
								region = key.Region;

							// Checks if the player is inside the region
							if (!region.InArea((int)TShock.Players[e.Msg.whoAmI].X, (int)TShock.Players[e.Msg.whoAmI].Y))
							{
								return;
							}
						}

						// Cancel the drop
						TShock.Players[e.Msg.whoAmI].SendData(PacketTypes.ItemDrop, "", id);
						// If the item is stackable, give them the same amount of in return; otherwise, return the excess
						Random rand = new Random();
						Item give = key.Items[rand.Next(0,key.Items.Count)];
						if (give.maxStack >= stack)
						{
							TShock.Players[e.Msg.whoAmI].GiveItem(give.netID, stack);
							Item take = TShock.Utils.GetItemById((int)key.Type);
							TShock.Players[e.Msg.whoAmI].SendSuccessMessage($"Has usado {stack} {take.Name} y has obtenido {stack} {give.Name}!");
						}
						else
						{
							TShock.Players[e.Msg.whoAmI].GiveItem(give.netID, 1);
							Item take = TShock.Utils.GetItemById((int)key.Type);
							TShock.Players[e.Msg.whoAmI].SendSuccessMessage($"Has usado {take.Name} y has obtenido {give.Name}!");
							TShock.Players[e.Msg.whoAmI].GiveItem(take.netID, stack - 1);
							TShock.Players[e.Msg.whoAmI].SendSuccessMessage("Has recuperado las llaves de botin de exceso.");
						}
						Exchanging[e.Msg.whoAmI] = null;
						e.Handled = true;
					}
				}
			}
		}

		private void onInitialize(EventArgs e)
		{
			Config = Config.Read();

			//This is the main command, which branches to everything the plugin can do, by checking the first parameter a player inputs
			Commands.ChatCommands.Add(new Command(new List<string>() { "botin.abrir", "botin.reload", "botin.modo" }, KeyChange, "botin")
			{
				HelpDesc = new[]
				{
					$"{Commands.Specifier}botin - Muestra informacion del plugin.",
					$"{Commands.Specifier}botin abrir <tipo> - Utiliza la llave correspondiente para abrir el botin especificado en <tipo>.",
					$"{Commands.Specifier}botin lista - Muestra el listado de botines disponibles que puedes conseguir.",
					$"{Commands.Specifier}botin modo <modo> - Cambia el modo de intercambio.",
					$"{Commands.Specifier}botin recargar - Vuelve a cargar la configuracion.",
					"Si no logras realizar el intercambio, asegurate de tener suficiente espacio en tu inventario."
				}
			});

			Exchanging = new KeyTypes?[Main.maxNetPlayers];
		}

		private void onLeave(LeaveEventArgs e)
		{
			if (e.Who >= 0 || e.Who < Main.maxNetPlayers)
			{
				Exchanging[e.Who] = null;
			}
		}

		private void KeyChange(CommandArgs args)
		{
			TSPlayer ply = args.Player;

			// SSC check to alert users
			if (!Main.ServerSideCharacter)
			{
				ply.SendWarningMessage("[Advertencia] Este complemento puede no funcionar en servidores que no tengan SSC activado.");
			}

			if (args.Parameters.Count < 1)
			{
				// Plugin Info
				var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
				ply.SendMessage($"KeyChanger (v{version}) por Enerdy - Actualizado y traducido por Tsviets", Color.SkyBlue);
				ply.SendMessage("Descripcion: Abre cajas de botin con llaves para obtener objetos.", Color.SkyBlue);
				ply.SendMessage($"Modo de uso: {Commands.Specifier}botin <lista/modo/abrir> [tipo]", Color.SkyBlue);
				ply.SendMessage($"Escribe {Commands.Specifier}help botin para más informacion.", Color.SkyBlue);
			}
			else if (args.Parameters[0].ToLower() == "abrir" && args.Parameters.Count == 1)
			{
				ply.SendErrorMessage("Error de uso. Uso correcto: {0}botin abrir <tipo>", Commands.Specifier);
			}
			else if (args.Parameters.Count > 0)
			{
				string cmd = args.Parameters[0].ToLower();
				switch (cmd)
				{
					case "abrir":
						// Prevents cast from the server console
						if (!ply.RealPlayer)
						{
							ply.SendErrorMessage("Debes usar este comando dentro del juego.");
							return;
						}

						if (!ply.HasPermission("botin.abrir"))
						{
							ply.SendErrorMessage("No tienes acceso a este comando.");
							break;
						}

						KeyTypes type;
						if (!Enum.TryParse(args.Parameters[1].ToLowerInvariant(), true, out type))
						{
							ply.SendErrorMessage("El botin seleccionado es invalido. Tipos de botines disponibles: " + String.Join(", ",
								Config.EnableTempleKey ? "templo" : null,
								Config.EnableJungleKey ? "jungla" : null,
								Config.EnableCorruptionKey ? "corrupto" : null,
								Config.EnableCrimsonKey ? "carmesi" : null,
								Config.EnableHallowedKey ? "sagrado" : null,
								Config.EnableFrozenKey ? "helado" : null));
							return;
						}

						Key key = Utils.LoadKey(type);
						// Verifies whether the key has been enabled
						if (!key.Enabled)
						{
							ply.SendInfoMessage("El botin seleccionado se encuentra deshabilitado.");
							return;
						}

						if (!Config.UseSSC)
						{
							// Begin the exchange, expect the player to drop the key
							Exchanging[args.Player.Index] = type;
							ply.SendInfoMessage($"Drop (hold & right-click) any number of {key.Name} keys to proceed.");
							return;
						}

						// Checks if the player carries the necessary key
						var lookup = ply.TPlayer.inventory.FirstOrDefault(i => i.netID == (int)key.Type);
						if (lookup == null)
						{
							ply.SendErrorMessage("Asegurate de tener en tu inventario la llave del botin que quieres abrir.");
							return;
						}

						if (Config.EnableRegionExchanges)
						{
							Region region;
							if (Config.MarketMode)
								region = TShock.Regions.GetRegionByName(Config.MarketRegion);
							else
								region = key.Region;

							// Checks if the required region is set to null
							if (region == null)
							{
								ply.SendInfoMessage("No se ha especificado una region de intercambio para este botin.");
								return;
							}

							// Checks if the player is inside the region
							if (!region.InArea(args.Player.TileX, args.Player.TileY))
							{
								ply.SendErrorMessage("Debes ir a la region indicada para abrir tu botin -> " + region.Name );
								return;
							}
						}

						Item item;
						for (int i = 0; i < 50; i++)
						{
							item = ply.TPlayer.inventory[i];
							// Loops through the player's inventory
							if (item.netID == (int)key.Type)
							{
								// Found the item, checking for available slots. This will increase invManage for each empty slot until ic = 50, as any further slot have other purposes (coins, ammo, etc.)
								var invManage = 0;
								for (int ic = 0; ic < 50; ic++)
								{
									if (ply.TPlayer.inventory[ic].Name == "")
										invManage++;

								}
								var cfgCount = Config.ItemCountGiven;
								var LBenabler = Config.EnableLootboxMode;
								if ((item.stack == 1 & (item.stack + invManage) >= cfgCount & LBenabler == true) || ((invManage >= cfgCount) & LBenabler == true)) // If your key stack is 1, this will consider it as an empty slot as well.
								{																																
										ply.TPlayer.inventory[i].stack--;
										ply.SendSuccessMessage("Has usado una llave de botin => {0}", key.Type); // This here will show the opened lootbox, rename args as you see fit.
										Random rand = new Random();
									for (int icount = 0; icount < cfgCount; icount++)
										{
											NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, ply.Index, i);
											Item give = key.Items[rand.Next(0, key.Items.Count)];
											ply.GiveItem(give.netID, 1);
											Item take = TShock.Utils.GetItemById((int)key.Type);
											ply.SendSuccessMessage("Has recibido {0}!", give.Name);
										}

								}
								else if ((item.stack == 1 & (item.stack + invManage) > key.Items.Count & LBenabler == false) || (( invManage > key.Items.Count) & LBenabler == false))
								{
								ply.TPlayer.inventory[i].stack--;
								ply.SendSuccessMessage("Has abierto usado una llave de botin => {0}", key.Type);
								for (int norand = 0; norand < key.Items.Count; norand++)
									{
										NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, ply.Index, i);
										Item give = key.Items[norand];
										ply.GiveItem(give.netID, 1);
										Item take = TShock.Utils.GetItemById((int)key.Type);
										ply.SendSuccessMessage("Has recibido {0}!", give.Name);
									}
								}
								else
								{
									// Sent if neither of the above conditions were fulfilled.
									ply.SendErrorMessage("Asegurate de tener espacio en tu inventario.");
									return;
								}
							}
						}
						break;

					case "reload":
						{
							if (!ply.HasPermission("botin.reload"))
							{
								ply.SendErrorMessage("No tienes permiso para usar este comando.");
								break;
							}

							Config = Config.Read();
							ply.SendSuccessMessage("Se han actualizado los cambios en la configuracion.");
							break;
						}

					case "lista":
						{
							ply.SendMessage("Botin del Templo - [i:" + String.Join("] [i:", Utils.LoadKey(KeyTypes.Templo).Items.Select(i => i.netID)) + "]", Color.Chocolate);
							ply.SendMessage("Botin de la Jungla - [i:" + String.Join("] [i:", Utils.LoadKey(KeyTypes.Jungla).Items.Select(i => i.netID)) + "]", Color.DarkGreen);
							ply.SendMessage("Botin Corrupto - [i:" + String.Join("] [i:", Utils.LoadKey(KeyTypes.Corrupto).Items.Select(i => i.netID)) + "]", Color.Purple);
							ply.SendMessage("Botin Carmesi - [i:" + String.Join("] [i:", Utils.LoadKey(KeyTypes.Carmesi).Items.Select(i => i.netID)) + "]", Color.OrangeRed);
							ply.SendMessage("Botin Sagrado - [i:" + String.Join("] [i:", Utils.LoadKey(KeyTypes.Sagrado).Items.Select(i => i.netID)) + "]", Color.LightPink);
							ply.SendMessage("Botin Helado - [i:" + String.Join("] [i:", Utils.LoadKey(KeyTypes.Helado).Items.Select(i => i.netID)) + "]", Color.SkyBlue);
							break;
						}

					case "modo":
						{
							if (!ply.HasPermission("botin.modo"))
							{
								ply.SendErrorMessage("No tienes acceso a este comando.");
								break;
							}

							if (args.Parameters.Count < 2)
							{
								ply.SendErrorMessage("Error de uso. Modo de uso: {0}botin modo <normal/region/mercado>", Commands.Specifier);
								break;
							}

							string query = args.Parameters[1].ToLower();

							if (query == "normal")
							{
								Config.EnableRegionExchanges = false;
								ply.SendSuccessMessage("Modo de intercambio asignado: normal (cualquier lugar).");
							}
							else if (query == "region")
							{
								Config.EnableRegionExchanges = true;
								Config.MarketMode = false;
								ply.SendSuccessMessage("Modo de intercambio asignado: region (una region por cada llave).");
							}
							else if (query == "mercado")
							{
								Config.EnableRegionExchanges = true;
								Config.MarketMode = true;
								ply.SendSuccessMessage("Modo de intercambio asignado: mercado (una region para todas las llaves).");
							}
							else
							{
								ply.SendErrorMessage("Sintaxis invalida. Modo de uso: {0}llaves modo <normal/region/mercado>", Commands.Specifier);
								return;
							}
							Config.Write();
							break;
						}
					default:
						{
							ply.SendErrorMessage(Utils.ErrorMessage(ply));
							break;
						}
				}
			}
			else
			{
				ply.SendErrorMessage(Utils.ErrorMessage(ply));
			}
		}
	}
}
