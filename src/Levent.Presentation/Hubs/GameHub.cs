using Levent.Engine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Levent.Presentation.Hubs
{
    public class GameHub : Hub
    {
        public static string waitingUserId;
        public static string waitingUserName;

        private static List<GameController> games = new List<GameController>();

        public async Task JoinGroup(string playerName)
        {
            await base.OnConnectedAsync();
            if (waitingUserId == null)
            {
                waitingUserId = Context.ConnectionId;
                waitingUserName = playerName;
                await Clients.Client(waitingUserId).SendAsync("WaitingOpponent", "Rakip bekleniyor..");
            }
            else
            {
                var game = new GameController(waitingUserName, waitingUserId, playerName, Context.ConnectionId);
                games.Add(game);

                await Clients.Client(waitingUserId).SendAsync("StartGame",
                    "Oyuna girildi. Rakibiniz " + playerName + ". Hamle sırası sizde!",
                    "turnOwner",
                    waitingUserName, playerName,
                    Game.LettersPoints);
                await Clients.Client(Context.ConnectionId).SendAsync("StartGame",
                    "Oyuna girildi. Rakibiniz " + waitingUserName + ". Hamle sırası rakipte.",
                    "opponent",
                    waitingUserName, playerName,
                    Game.LettersPoints);

                waitingUserId = null;
                waitingUserName = null;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var game = games.FirstOrDefault(_ => _.waitingPlayerConnectionId == Context.ConnectionId || _.playerConnectionId == Context.ConnectionId);
            if (game.waitingPlayerConnectionId == Context.ConnectionId)
            {
                await Clients.Client(game.playerConnectionId).SendAsync("RestartGame", game.playerName, "Rakip oyundan ayrıldı. Lütfen oyunu yeniden başlatın!");
            }
            else
            {
                await Clients.Client(game.waitingPlayerConnectionId).SendAsync("RestartGame", game.waitingPlayerName, "Rakip oyundan ayrıldı. Lütfen oyunu yeniden başlatın!");
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
                    .SendAsync("PlayOpponentLetter", "Hamle sırası sizde! Lütfen " + letter + " harfini kullanın. ", letter, "OpponentTurn");

                await Clients.Client(turnOwnerConnectionId)
                    .SendAsync("PlayOpponentLetter", game.GameInstance.GetOpponent().Username + " 'in hamle sırası!", letter, "TurnOwner");
            }
            catch (IncorrectLetterException exc)
            {
                await Clients.Client(turnOwnerConnectionId)
                    .SendAsync("IncorrectLetterException", exc.Message);
            }
            catch (GridCellException exc)
            {
                await Clients.Client(turnOwnerConnectionId)
                    .SendAsync("GridCellException", exc.Message);
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
                    var winner = game.GameInstance.GetResult().Winner;
                    var loser = game.GameInstance.GetResult().Loser;

                    await Clients.Client(turnOwnerConnectionId).SendAsync("GameIsOver", "OYUN BİTTİ!!!", new
                    {
                        winnerName = winner.User.Username,
                        winnerScore = winner.Score,
                        winnerMeaningfulWords = winner.MeaningfulWords,
                        loserName = loser.User.Username,
                        loserScore = loser.Score,
                        loserMeaningfulWords = loser.MeaningfulWords
                    });
                    await Clients.Client(opponentConnectionId).SendAsync("GameIsOver", "OYUN BİTTİ!!!", new
                    {
                        winnerName = winner.User.Username,
                        winnerScore = winner.Score,
                        winnerMeaningfulWords = winner.MeaningfulWords,
                        loserName = loser.User.Username,
                        loserScore = loser.Score,
                        loserMeaningfulWords = loser.MeaningfulWords
                    });
                }
                else
                {
                    await Clients.Client(turnOwnerConnectionId)
                        .SendAsync("TurnOwnwer", "Hamle sırası sizde!", "TurnOwner");
                    await Clients.Client(opponentConnectionId)
                        .SendAsync("TurnOwnwer", game.GameInstance.TurnOwner.Username + " 'in hamle sırası!", "Opponent");
                }
            }
            catch (GridCellException e)
            {
                await Clients.Client(opponentConnectionId)
                    .SendAsync("GridCellException", e.Message);
            }
            catch (IncorrectLetterException e)
            {
                await Clients.Client(opponentConnectionId)
                    .SendAsync("IncorrectLetterException", e.Message);
            }
        }
    }

    class GameController
    {
        internal readonly string waitingPlayerName;
        public readonly string waitingPlayerConnectionId;
        internal readonly string playerName;
        public readonly string playerConnectionId;

        public int dimension = 6;

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