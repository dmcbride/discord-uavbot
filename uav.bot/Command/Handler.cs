using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using uav.logic.Constants;
using uav.logic.Database;
using uav.logic.Extensions;
using uav.logic.Service;

namespace uav.Command
{
    public class Handler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseService db;

        // Retrieve client and CommandService instance via ctor
        public Handler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
            db = new DatabaseService();
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageReceived += HandleAutoEmojisAsync;
            _client.MessageReceived += HandleMonthlyCounts;
            _client.MessageReceived += HandleFinalRanks;

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), 
                                            services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) || 
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context, 
                argPos: argPos,
                services: null);
        }

        private static readonly Regex isImageUrl = new Regex(@"\.(?:png|jpe?g|gif)$");
        private static readonly Regex isWordlePost = new Regex(@"^Wordle \d+ ([X\d])/\d");
        private Task HandleAutoEmojisAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return Task.CompletedTask;

            var reactions = null as IEnumerable<Emoji>;
            switch(message.Channel.Id)
            {
                case Channels.AsteroidPics:
                    var hasImageAttachments = message.Attachments.Any(a => isImageUrl.IsMatch(a.Url));
                    if (!hasImageAttachments) return Task.CompletedTask;

                    reactions = IpmEmoji.TeamEmojis.Take(10);
                    break;

                case Channels.LoungeOffTopic:
                    var m = isWordlePost.Match(message.Content);
                    if (m.Success)
                    {
                        var count = m.Groups[1].Value;
                        reactions = new[] {
                            count == "X" ? IpmEmoji.X : IpmEmoji.NumberEmojis[int.Parse(count)]
                        };
                    }

                    break;
                
                default:
                    return Task.CompletedTask;
            }

            if (reactions != null)
            {
                // spawn this off so we don't wait on it. It can be slow.
                _ = Task.Run(() => message.AddReactionsAsync(reactions.ToArray()));
            }

            return Task.CompletedTask;
        }

        private async Task HandleMonthlyCounts(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            var user = message.Author as SocketGuildUser;
            if (user is null)
            {
                // might be webhook user, doesn't have a nickname, can't really save it off.
                return;
            }

            if (user.IsBot)
            {
                // nope, not saving that.
                return;
            }

            var dbUser = message.Author.ToDbUser();
            await db.SaveUser(dbUser);

            // then set up their role
            var playerId = (await db.GetUserPlayerIds(new[] {dbUser.User_Id})).First().Player_Id;
            if (playerId.HasValue())
            {
                if (!dbUser.Roles.Any(r => r.Id == Roles.GameRegistered))
                {
                    _ = user.AddRoleAsync(Roles.GameRegistered);
                }
            }
            else
            {
                if (dbUser.Roles.Any(r => r.Id == Roles.GameRegistered))
                {
                    _ = user.RemoveRoleAsync(Roles.GameRegistered);
                }
            }

            if (!Channels.ParticipationChannels.Contains(message.Channel.Id))
            {
                return;
            }

            await db.AddParticipationHistory(dbUser);
        }

        private static Regex RankExtractor = new Regex(@"You\s+finished\s+at\s+rank\s+(?<rank>\d+)", RegexOptions.Singleline);
        private async Task HandleFinalRanks(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message?.Channel.Id != Channels.SubmitFinalRanksHere) return;
            if (message.Author.IsBot) return;

            if (!message.Attachments.Any(x => isImageUrl.IsMatch(x.Url))) return;
            var text = await Tesseract.RunTesseract(message.Attachments.First(x => isImageUrl.IsMatch(x.Url)).Url);

            var m = RankExtractor.Match(text);
            if (m.Success)
            {
                var rank = m.Groups["rank"].Value;
                _ = message.Channel.SendMessageAsync($"{message.Author.Mention} got rank **{rank}**");
            }
        }
   }
}