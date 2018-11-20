using Levent.Engine;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Levent.Presentation.Hubs
{
    public class GameHub : Hub
    {
        //public static List<string> ConnectionIds = new List<string>();
        public static string waitingUserId;
        public static string waitingUserName;

        private static List<GameController> games = new List<GameController>();

        public async Task JoinGroup(string playerName)
        {
            if (waitingUserId == null)
            {
                waitingUserId = Context.ConnectionId;
                waitingUserName = playerName;
            }
            else
            {
                var game = new GameController(waitingUserName, waitingUserId, playerName, Context.ConnectionId);
                games.Add(game);

                await Clients.Client(waitingUserId).SendAsync("StartGame", "Oyuna girildi. Hamle sırası sizde!", "turnOwner");
                await Clients.Client(Context.ConnectionId).SendAsync("StartGame", "Oyuna girildi. Hamle sırası Rakipte", "opponent");

                waitingUserId = null;
                waitingUserName = null;
            }
        }

        public async Task Play(char letter, int xDimension, int yDimension)
        {
            var game = games.FirstOrDefault(_ => _.waitingPlayerConnectionId == Context.ConnectionId || _.playerConnectionId == Context.ConnectionId);
            var turnOwnerConnectionId = "";
            var opponentConnectionId = "";

            try
            {
                game.GameInstance.Play(letter, xDimension, yDimension);

                if (game.waitingPlayerName == game.GameInstance.TurnOwner.Username)
                {
                    turnOwnerConnectionId = game.waitingPlayerConnectionId;
                    opponentConnectionId = game.playerConnectionId;
                }
                else
                {
                    turnOwnerConnectionId = game.playerConnectionId;
                    opponentConnectionId = game.waitingPlayerConnectionId;
                }

                await Clients.Client(opponentConnectionId)
                    .SendAsync("PlayOpponentLetter", "Your turn! Please use this letter ", letter, "OpponentTurn");

                await Clients.Client(turnOwnerConnectionId)
                    .SendAsync("PlayOpponentLetter", "Hamle sırası rakipte!", letter, "TurnOwner");
            }
            catch (IncorrectLetterException exc)
            {
                await Clients.Client(turnOwnerConnectionId)
                    .SendAsync("PlayOpponentLetter", exc.Message);
            }
            catch (GridCellException exc)
            {
                await Clients.Client(turnOwnerConnectionId)
                    .SendAsync("PlayOpponentLetter", exc.Message);
            }
        }

        public async Task PlayOpponent(int opponentDimensionx, int opponentDimensiony)
        {
            var game = games.FirstOrDefault(_ => _.waitingPlayerConnectionId == Context.ConnectionId || _.playerConnectionId == Context.ConnectionId);
            var turnOwnerConnectionId = "";
            var opponentConnectionId = "";

            try
            {
                game.GameInstance.PlayOpponentLetter(opponentDimensionx, opponentDimensiony);

                if (game.waitingPlayerName == game.GameInstance.TurnOwner.Username)
                {
                    turnOwnerConnectionId = game.waitingPlayerConnectionId;
                    opponentConnectionId = game.playerConnectionId;
                }
                else
                {
                    turnOwnerConnectionId = game.playerConnectionId;
                    opponentConnectionId = game.waitingPlayerConnectionId;
                }

                if (game.GameInstance.GameIsOver())
                {
                    await Clients.Client(turnOwnerConnectionId).SendAsync("GameIsOver", "Game is over!!!");
                    await Clients.Client(opponentConnectionId).SendAsync("GameIsOver", "Game is over!!!");

                    var winner = game.GameInstance.GetResult().Winner;
                    var loser = game.GameInstance.GetResult().Loser;
                    // Return Result
                    var winnerUserName = winner.User.Username;
                    var winnerScore = winner.Score;
                    var winnerMeaningfulWords = winner.MeaningfulWords;

                    var loserUserName = loser.User.Username;
                    var loserScore = loser.Score;
                    var loserMeaningfulWords = loser.MeaningfulWords;

                    await Clients.Client(turnOwnerConnectionId).SendAsync("ResultUserName", winnerUserName, loserUserName);
                    await Clients.Client(turnOwnerConnectionId).SendAsync("ResultUserScore", winnerScore, loserScore);
                    await Clients.Client(turnOwnerConnectionId).SendAsync("ResultMeaningfulWords", winnerMeaningfulWords, loserMeaningfulWords);

                    await Clients.Client(opponentConnectionId).SendAsync("ResultUserName", winnerUserName, loserUserName);
                    await Clients.Client(opponentConnectionId).SendAsync("ResultUserScore", winnerScore, loserScore);
                    await Clients.Client(opponentConnectionId).SendAsync("ResultMeaningfulWords", winnerMeaningfulWords, loserMeaningfulWords);
                }
                else
                {
                    await Clients.Client(turnOwnerConnectionId)
                        .SendAsync("TurnOwnwer", "Your turn!", "TurnOwner");
                    await Clients.Client(opponentConnectionId)
                        .SendAsync("TurnOwnwer", "Hamle sırası rakipte!", "Opponent");
                }
            }
            catch (GridCellException e)
            {
                await Clients.Client(opponentConnectionId)
                    .SendAsync("TurnOwnwer", e.Message);
            }
            catch (IncorrectLetterException e)
            {
                await Clients.Client(opponentConnectionId)
                    .SendAsync("TurnOwnwer", e.Message);
            }
        }
    }

    class GameController
    {
        internal readonly string waitingPlayerName;
        public readonly string waitingPlayerConnectionId;
        internal readonly string playerName;
        public readonly string playerConnectionId;

        public int dimension = 2;

        public Game GameInstance { get; set; }

        public GameController(string waitingPlayerName, string waitingPlayerConnectionId, string playerName, string playerConnectionId)
        {
            this.waitingPlayerName = waitingPlayerName;
            this.waitingPlayerConnectionId = waitingPlayerConnectionId;
            this.playerName = playerName;
            this.playerConnectionId = playerConnectionId;

            this.GameInstance = new Game(waitingPlayerName, playerName, dimension, dimension);
        }
    }
}