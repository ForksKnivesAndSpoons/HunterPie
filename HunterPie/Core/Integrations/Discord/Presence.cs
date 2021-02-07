﻿using System;
using DiscordRPC;
using HunterPie.Logger;

namespace HunterPie.Core.Integrations.Discord
{
    internal class Presence : IDisposable
    {
        public bool IsDisposed { get; private set; }
        private const string AppId = "567152028070051859";
        private bool FailedToRegisterScheme { get; set; }
        private bool isOffline = false;
        private bool isVisible = true;
        private RichPresence Instance;
        public DiscordRpcClient Client;
        public Game ctx;

        /* Constructor and base functions */

        public Presence(Game context)
        {
            ctx = context;
            HookEvents();
        }

        public void SetOfflineMode() => isOffline = true;

        ~Presence()
        {
            Dispose(false);
        }

        /* Event handlers */

        private void HookEvents()
        {
            ConfigManager.OnSettingsUpdate += HandleSettings;
            // Game context
            ctx.OnClockChange += HandlePresence;
            ctx.Player.OnZoneChange += HandlePresence;
        }

        private void UnhookEvents()
        {
            ConfigManager.OnSettingsUpdate -= HandleSettings;
            ctx.OnClockChange -= HandlePresence;
            ctx.Player.OnZoneChange -= HandlePresence;
        }

        /* Connection */

        public void StartRPC()
        {
            if (isOffline)
                return;

            // Check if connection exists to avoid creating multiple connections
            Instance = new RichPresence();
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_CONNECTED']"));
            Instance.Secrets = new Secrets();

            try
            {
                Client = new DiscordRpcClient(AppId, autoEvents: true);
            } catch (Exception err)
            {
                Debugger.Error($"Failed to create Rich Presence connection:\n{err}");
                return;
            }


            try
            {
                Client.RegisterUriScheme("582010");

            }
            catch (Exception err)
            {
                Debugger.Error(err);
                FailedToRegisterScheme = true;
            }

            if (!FailedToRegisterScheme)
            {
                // Events
                Client.OnReady += Client_OnReady;
                Client.OnJoinRequested += Client_OnJoinRequested;
                Client.OnJoin += Client_OnJoin;

                Client.SetSubscription(EventType.JoinRequest | EventType.Join);
            }

            Client.Initialize();
            if (!ConfigManager.Settings.RichPresence.Enabled && isVisible)
            {
                Client?.ClearPresence();
                isVisible = false;
            }
        }

        private void Client_OnJoin(object sender, DiscordRPC.Message.JoinMessage args)
        {
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_JOINING']"));
            System.Diagnostics.Process.Start($"steam://joinlobby/582010/{args.Secret}");
            Debugger.Debug($"steam://joinlobby/582010/{args.Secret}");
        }

        private void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args) => Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_USER_CONNECTED']").Replace("{Username}", args.User.ToString()));

        private void Client_OnJoinRequested(object sender, DiscordRPC.Message.JoinRequestMessage args)
        {
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_JOIN_REQUEST']").Replace("{Username}", args.User.ToString()));

            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                GUI.Widgets.Notifications.DiscordNotify DiscordNotification = new GUI.Widgets.Notifications.DiscordNotify(args);

                DiscordNotification.OnRequestAccepted += OnDiscordRequestAccepted;
                DiscordNotification.OnRequestRejected += OnDiscordRequestRejected;

                DiscordNotification.Show();
            }));
        }

        private void OnDiscordRequestRejected(object source, DiscordRPC.Message.JoinRequestMessage args)
        {
            GUI.Widgets.Notifications.DiscordNotify src = (GUI.Widgets.Notifications.DiscordNotify)source;
            src.OnRequestAccepted -= OnDiscordRequestAccepted;
            src.OnRequestRejected -= OnDiscordRequestRejected;

            Client.Respond(args, false);

            App.Current.Dispatcher.Invoke(new Action(() => { src.Close(); }));
        }

        private void OnDiscordRequestAccepted(object source, DiscordRPC.Message.JoinRequestMessage args)
        {
            GUI.Widgets.Notifications.DiscordNotify src = (GUI.Widgets.Notifications.DiscordNotify)source;
            src.OnRequestAccepted -= OnDiscordRequestAccepted;
            src.OnRequestRejected -= OnDiscordRequestRejected;

            Client.Respond(args, true);

            src.Close();
        }

        public void HandleSettings(object source, EventArgs e)
        {
            if (ConfigManager.Settings.RichPresence.Enabled && !isVisible)
            {
                isVisible = true;
            }
            else if (!ConfigManager.Settings.RichPresence.Enabled && isVisible)
            {
                try
                {
                    Client.ClearPresence();
                }
                catch { }
                isVisible = false;
            }
        }

        public void HandlePresence(object source, EventArgs e)
        {
            if (Instance is null || ctx is null || IsDisposed)
                return;

            // Do nothing if RPC is disabled
            if (!isVisible)
                return;

            if (!FailedToRegisterScheme)
            {
                if (ctx.Player.SteamSession != 0 && ConfigManager.Settings.RichPresence.LetPeopleJoinSession)
                {
                    Instance.Secrets.JoinSecret = $"{ctx.Player.SteamSession}/{ctx.Player.SteamID}";
                }
                else
                {
                    Instance.Secrets.JoinSecret = null;
                }
            }

            // Only update RPC if player isn't in loading screen
            switch (ctx.Player.ZoneID)
            {
                case 0:
                    Instance.Details = ctx.Player.PlayerAddress == 0 ? GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_MAIN_MENU']") : GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_LOADING_SCREEN']");
                    Instance.State = null;
                    GenerateAssets("main-menu", null, null, null);
                    Instance.Party = null;
                    break;
                default:
                    if (ctx.Player.PlayerAddress == 0)
                    {
                        Instance.Details = GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_MAIN_MENU']");
                        Instance.State = null;
                        GenerateAssets("main-menu", null, null, null);
                        Instance.Party = null;
                        break;
                    }

                    try
                    {
                        Instance.Details = GetDescription();
                        Instance.State = GetState();
                    } catch
                    {
                        return;
                    }
                    
                    GenerateAssets(ctx.Player.ZoneName == null ? "main-menu" : $"st{ctx.Player.ZoneID}", ctx.Player.ZoneID == 0 ? null : ctx.Player.ZoneName, ctx.Player.WeaponName == null ? "hunter-rank" : $"weap{ctx.Player.WeaponID}", $"{ctx.Player.Name} | HR: {ctx.Player.Level} | MR: {ctx.Player.MasterRank}");
                    if (!ctx.Player.InPeaceZone)
                    {
                        MakeParty(ctx.Player.PlayerParty.Size, ctx.Player.PlayerParty.MaxSize, ctx.Player.PlayerParty.PartyHash);
                    }
                    else
                    {
                        MakeParty(ctx.Player.PlayerParty.LobbySize, ctx.Player.PlayerParty.MaxLobbySize, ctx.Player.SteamSession.ToString());
                    }
                    Instance.Timestamps = NewTimestamp(ctx.Time);
                    break;
            }
            Client.SetPresence(Instance);
        }

        private string GetDescription()
        {
            if (ctx is null || ctx?.Player is null || IsDisposed)
            {
                return "";
            }
            // Custom description for special zones
            switch (ctx.Player.ZoneID)
            {
                case 504:
                    return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_TRAINING']");
            }
            if (ctx.Player.InPeaceZone) return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_IN_TOWN']");
            if (ctx.HuntedMonster == null) return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_EXPLORING']");
            else
            {
                if (string.IsNullOrEmpty(ctx.HuntedMonster?.Name) || ctx.HuntedMonster?.Name == "Missing Translation") return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_EXPLORING']");
                return ConfigManager.Settings.RichPresence.ShowMonsterHealth ? GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_HUNTING']").Replace("{Monster}", ctx.HuntedMonster.Name).Replace("{Health}", $"{(int)(ctx.HuntedMonster.HPPercentage * 100)}%") : GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_DESCRIPTION_HUNTING']").Replace("{Monster}", ctx.HuntedMonster.Name).Replace("({Health})", null);
            }
        }

        private string GetState()
        {
            if (ctx.Player.PlayerParty.Size > 1 || ctx.Player.PlayerParty.LobbySize > 1)
            {
                if (ctx.Player.InPeaceZone)
                {
                    return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_LOBBY']");
                }
                else { return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_PARTY']"); }
            }
            else { return GStrings.GetLocalizationByXPath("/RichPresence/String[@ID='RPC_STATE_SOLO']"); }
        }

        /* Helpers */

        public void GenerateAssets(string largeImage, string largeImageText, string smallImage, string smallImageText)
        {
            if (Instance.Assets == null) { Instance.Assets = new Assets(); }
            Instance.Assets.LargeImageKey = largeImage;
            Instance.Assets.LargeImageText = largeImageText;
            Instance.Assets.SmallImageKey = smallImage;
            Instance.Assets.SmallImageText = smallImageText;
        }

        public void MakeParty(int partySize, int maxParty, string partyHash)
        {
            if (Instance.Party == null) { Instance.Party = new DiscordRPC.Party(); }
            Instance.Party.Size = partySize;
            Instance.Party.Max = maxParty;
            Instance.Party.ID = partyHash == "0" ? "USER_IN_OFFLINE_MODE" : partyHash;
        }

        public Timestamps NewTimestamp(DateTime? start)
        {
            Timestamps timestamp = new Timestamps();
            try
            {
                timestamp.Start = start;
            }
            catch
            {
                timestamp.Start = null;
            }
            return timestamp;
        }


        /* Dispose */
        public void Dispose()
        {
            Debugger.Discord(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_DISCORD_DISCONNECTED']"));
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                UnhookEvents();
                if (!(Client is null) && Client.IsInitialized)
                {
                    Client.ClearPresence();
                    Client.Dispose();
                }
                Instance = null;
            }
            IsDisposed = true;
        }
    }
}
