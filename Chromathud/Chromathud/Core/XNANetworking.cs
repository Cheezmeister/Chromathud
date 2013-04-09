#define TRACE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework;


namespace Chromathud.Core
{
    static class XNANetworking
    {
#region EventHandlers
        public static EventHandler<SignedInEventArgs> signInHandler = new EventHandler<SignedInEventArgs>(Gamer_LoginSuccess);
        public static EventHandler<SignedOutEventArgs> signOutHandler = new EventHandler<SignedOutEventArgs>(Gamer_LogoutSuccess);
        public static EventHandler<GamerJoinedEventArgs> gamerJoinedHandler = new EventHandler<GamerJoinedEventArgs>(Session_GamerJoined);
        public static EventHandler<GamerLeftEventArgs> gamerLeftHandler = new EventHandler<GamerLeftEventArgs>(Session_GamerLeft);
        public static EventHandler<GameStartedEventArgs> gameStartedHandler = new EventHandler<GameStartedEventArgs>(Session_GameStarted);
        public static EventHandler<GameEndedEventArgs> gameEndedHandler = new EventHandler<GameEndedEventArgs>(Session_GameEnded);
        public static EventHandler<NetworkSessionEndedEventArgs> networkSessionEndedHandler = new EventHandler<NetworkSessionEndedEventArgs>(Session_SessionEnded);
        public static event EventHandler sessionsFound;
#endregion

        static NetworkSession session;
        static SignedInGamer gamer;
        static GamerProfile gamerProfile;
        static AvailableNetworkSessionCollection availableSessions;
        static PacketReader packetReader;
        static PacketWriter packetWriter;

        public static void DisplaySessions(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private static void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            if (gamer == null && !Guide.IsVisible)
                Guide.ShowSignIn(1, true);

            if (session != null)
            {
                if (session.IsHost && session.IsEveryoneReady)
                    session.StartGame();

                foreach (LocalNetworkGamer aGamer in session.AllGamers)
                {
                    HandleData(aGamer, gameTime);
                }

                session.Update();
            }
        }

        private static void HandleData(LocalNetworkGamer aGamer, GameTime gameTime)
        {
            while (aGamer.IsDataAvailable)
            {
                NetworkGamer sender;
                aGamer.ReceiveData(packetReader, out sender);

                if (!sender.IsLocal)
                {
                    Console.WriteLine("Button: " + packetReader.ReadChar() + " pressed by player: " + aGamer.Gamertag);
                }
            }
        }

        public static void Gamer_LoginSuccess(object sender, SignedInEventArgs e)
        {
            // set the local gamer to the signedin gamer
            gamer = e.Gamer;

            // begin asynchronously getting the player's profile. Do this because it may take time to get over the network otherwise.
            gamer.BeginGetProfile(endGetProfile, gamer);

            // remove the signedin handler
            SignedInGamer.SignedIn -= signInHandler;
            Console.WriteLine("Gamer: " + gamer.Gamertag + " signed in.");

            int maxNumberOfPlayers = 1;
            NetworkSession.BeginFind(NetworkSessionType.PlayerMatch, maxNumberOfPlayers, null, endFind, session);
        }

        public static void Gamer_LogoutSuccess(object sender, SignedOutEventArgs e)
        {
            Console.WriteLine("Gamer: " + e.Gamer.Gamertag + " signed out.");

            // reset gamer
            gamer = null;

            // add signedin handler
            SignedInGamer.SignedIn += signInHandler;
        }


        // once we have the gamer's profile, set the local profile.
        public static void endGetProfile(IAsyncResult result)
        {
            gamerProfile = (result.AsyncState as Gamer).EndGetProfile(result);
        }

        public static void endFind(IAsyncResult result)
        {
            availableSessions = NetworkSession.EndFind(result);
            if (availableSessions.Count > 0)
            {
                foreach (var session in availableSessions)
                {
                    Console.WriteLine("Game by: " + session.HostGamertag + " found.");
                    NetworkSession.BeginJoin(session, endJoin, session);
                }
            }
            else
            {
                Console.WriteLine("No games found, creating a new one.");
                session = NetworkSession.Create(NetworkSessionType.PlayerMatch, 1, 4);
                session.AllowHostMigration = true;
                session.AllowJoinInProgress = true;

                session.GamerJoined += gamerJoinedHandler;
                session.GamerLeft += gamerLeftHandler;
                session.GameStarted += gameStartedHandler;
                session.GameEnded += gameEndedHandler;
                session.SessionEnded += networkSessionEndedHandler;
            }
        }

        public static void endJoin(IAsyncResult result)
        {
            session = NetworkSession.EndJoin(result);

            packetReader = new PacketReader();
            packetWriter = new PacketWriter();

            session.GamerJoined += gamerJoinedHandler;
            session.GamerLeft += gamerLeftHandler;
            session.GameStarted += gameStartedHandler;
            session.GameEnded += gameEndedHandler;
            session.SessionEnded += networkSessionEndedHandler;
        }

        public static void Session_GamerJoined(object sender, GamerJoinedEventArgs args)
        {
            Console.WriteLine("Player:" + args.Gamer.Gamertag + " has joined the game.");
        }
        public static void Session_GamerLeft(object sender, GamerLeftEventArgs args)
        {
            Console.WriteLine("Player:" + args.Gamer.Gamertag + " has left the game.");
        }
        public static void Session_GameStarted(object sender, GameStartedEventArgs args)
        {
            Console.WriteLine("The game has started.");
        }
        public static void Session_GameEnded(object sender, GameEndedEventArgs args)
        {
            Console.WriteLine("The game has ended.");
        }
        public static void Session_SessionEnded(object sender, NetworkSessionEndedEventArgs args)
        {
            Console.WriteLine("The network session has closed.");
        }
    }
}