using System.Collections.Generic;
using System.Linq;
using Dapper;
using uav.logic.Database.Model;

namespace uav.logic.Database;

partial class DatabaseService
{
    public async Task<ulong> CreatePoll(Poll poll)
    {
        using var connect = Connect;
        // cspell:disable
        var query = $@"
            INSERT INTO {Table.Polls} (
                poll_user_key,
                guild_id,
                channel_id,
                msg_id,
                description,
                end_date,
                max_options,
                options
            ) VALUES (
                @polluserkey,
                @guildid,
                @channelid,
                @msgid,
                @description,
                @enddate,
                @maxoptions,
                @options
            );
            SELECT LAST_INSERT_ID();";
        // cspell:enable
        var id = await connect.QuerySingleAsync<ulong>(query, poll);
        poll.PollId = id;
        return id;
    }

    public async Task UpdatePoll(Poll poll)
    {
        using var connect = Connect;
        // cspell:disable
        var query = $@"
            UPDATE {Table.Polls} SET
                guild_id = @guildid,
                channel_id = @channelid,
                msg_id = @msgid,
                poll_user_key = @polluserkey,
                description = @description,
                end_date = @enddate,
                max_options = @maxoptions,
                options = @options
            WHERE poll_id = @pollid";
        // cspell:enable
        await connect.ExecuteAsync(query, poll);
    }

    public async Task<Poll?> GetPoll(ulong pollId, ulong guildId)
    {
        using var connect = Connect;
        var query = $@"
            SELECT *
            FROM {Table.Polls}
            WHERE poll_id = @poll_id
            AND guild_id = @guild_id";
        return await connect.QueryFirstOrDefaultAsync<Poll>(query, new { poll_id = pollId, guild_id = guildId });
    }

    public async Task<Poll?> GetPollByUserKey(string userKey, ulong guildId)
    {
        using var connect = Connect;
        var query = $@"
            SELECT *
            FROM {Table.Polls}
            WHERE poll_user_key = @poll_user_key
            AND guild_id = @guild_id";
        return await connect.QueryFirstOrDefaultAsync<Poll>(query, new { poll_user_key = userKey, guild_id = guildId });
    }

    public async Task<(bool votedPreviously, int voteCount)> VotePoll(Poll poll, ulong userId, IReadOnlyCollection<string> votes)
    {
        using var connect = Connect;
        // first, try to find the user's vote(s) and delete them.
        var query = $@"
            DELETE FROM {Table.PollVotes}
            WHERE poll_id = @poll_id
            AND user_id = @user_id";
        var rows = await connect.ExecuteAsync(query, new { poll_id = poll.PollId, user_id = userId });

        // then insert the new vote(s)
        query = $@"
            INSERT INTO {Table.PollVotes} (
                poll_id,
                user_id,
                vote
            ) VALUES (
                @poll_id,
                @user_id,
                @vote
            );";

        await connect.ExecuteAsync(query, votes.Select(vote => new { poll_id = poll.PollId, user_id = userId, vote }));

        // then get the number of votes
        var voteCount = await PollVoteCount(poll.PollId, poll.GuildId);

        return (rows > 0, voteCount); // return true if the user had already voted
    }

    public async Task<int> PollVoteCount(ulong pollId, ulong guildId)
    {
        using var connect = Connect;
        var query = $"""
            SELECT COUNT(DISTINCT user_id)
            FROM {Table.PollVotes} pv
            JOIN {Table.Polls} ON {Table.Polls}.poll_id = pv.poll_id
            WHERE pv.poll_id = @poll_id
            AND guild_id = @guild_id
            """;
        return await connect.QuerySingleAsync<int>(query, new { poll_id = pollId, guild_id = guildId });
    }

    public async Task<(bool voted, string? vote)> GetUserVote(ulong pollId, ulong guildId, ulong userId)
    {
        using var connect = Connect;
        var query = $@"
            SELECT vote
            FROM {Table.PollVotes}
            WHERE poll_id = @poll_id
            AND user_id = @user_id";
        var votes = (await connect.QueryAsync<int>(query, new { poll_id = pollId, user_id = userId })).ToArray();
        var poll = await GetPoll(pollId, guildId);

        return (votes is not null && votes.Length != 0, votes is null ? null : string.Join(", ", votes.Select(v => poll!.GetOptionText(v))));
    }

    public async Task<Poll?> GetNextExpiringPoll()
    {
        using var connect = Connect;
        var query = $@"
            SELECT *
            FROM {Table.Polls}
            WHERE end_date IS NOT NULL
            AND end_date < DATE_ADD(utc_timestamp(), INTERVAL 1 HOUR)
            AND NOT completed
            ORDER BY end_date ASC
            LIMIT 1";
        var poll = await connect.QueryFirstOrDefaultAsync<Poll>(query);
        // the times are stored in UTC, but we want it to actually be in UTC
        if (poll is not null)
        {
            poll.EndDate = poll.EndDate.Add(poll.EndDate.Offset).ToUniversalTime();
            poll.CreatedDate = poll.CreatedDate.Add(poll.CreatedDate.Offset).ToUniversalTime();
        }
        return poll;
    }

    public struct PollResults
    {
        public int Vote { get; init; }
        public int Count { get; init; }
    }

    public async Task<IEnumerable<PollResults>> GetPollResults(ulong pollId)
    {
        using var connect = Connect;
        var query = $@"
            SELECT vote, COUNT(*) AS count
            FROM {Table.PollVotes}
            WHERE poll_id = @poll_id
            GROUP BY vote
            ORDER BY vote ASC";
        return await connect.QueryAsync<PollResults>(query, new { poll_id = pollId });
    }

    public async Task<int> GetNumberOfUsersVotingFor(ulong pollId)
    {
        using var connect = Connect;
        var query = $@"
            SELECT COUNT(DISTINCT user_id)
            FROM {Table.PollVotes}
            WHERE poll_id = @poll_id";
        return await connect.QuerySingleAsync<int>(query, new { poll_id = pollId });
    }

    public struct DetailedPollResults
    {
        public int vote { get; set; }
        public KnownUser[] users { get; set; }

        public DetailedPollResults(int vote, KnownUser[] users)
        {
            this.vote = vote;
            this.users = users;
        }
    }

    public async Task<ICollection<DetailedPollResults>> GetDetailedPollResults(ulong pollId)
    {
        using var connect = Connect;
        var query = $@"
            SELECT vote, user_id
            FROM {Table.PollVotes}
            WHERE poll_id = @poll_id
            ORDER BY vote ASC;
            
            SELECT DISTINCT u.*
            FROM {Table.Users} u
            INNER JOIN {Table.PollVotes} v ON v.user_id = u.user_id
            WHERE v.poll_id = @poll_id;";
        using var multi = await connect.QueryMultipleAsync(query, new { poll_id = pollId });
        var votes = await multi.ReadAsync<(int vote, ulong userId)>();
        var users = await multi.ReadAsync<KnownUser>();
        var userDict = users.ToDictionary(u => u.User_Id);
        return votes
            .GroupBy(v => v.vote)
            .Select(g => new DetailedPollResults(g.Key, g.Select(v => userDict[v.userId]).ToArray()))
            .ToArray();
    }

    public async Task CompletePoll(Poll poll)
    {
        using var connect = Connect;
        var query = $@"
            UPDATE {Table.Polls} SET
                completed = TRUE
            WHERE poll_id = @poll_id";
        await connect.ExecuteAsync(query, new { poll_id = poll.PollId });
        poll.Completed = true;
    }

    public async Task<int> GetNumberOfPollsUserVotedIn(ulong userId, ulong guildId)
    {
        using var connect = Connect;
        var query = $@"
            SELECT COUNT(DISTINCT p.poll_id)
            FROM {Table.PollVotes} pv
            JOIN {Table.Polls} p ON pv.poll_id = p.poll_id
            WHERE pv.user_id = @user_id
              AND p.guild_id = @guild_id";
        return await connect.QuerySingleAsync<int>(query, new { user_id = userId, guild_id = guildId });
    }
}