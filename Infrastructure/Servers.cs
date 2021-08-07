using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Servers
    {
        private readonly EssentiBotContext _context;

        public Servers(EssentiBotContext context)
        {
            _context = context;
        }

        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { ServerId = id, Prefix = prefix });
            else
                server.Prefix = prefix;

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers
                .Where(x => x.ServerId == id)
                .Select(x => x.Prefix)
                .FirstOrDefaultAsync();

            return await Task.FromResult(prefix);
        }

        public async Task ModifyWelcomeAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { ServerId = id, Welcome = channelId });
            else
                server.Welcome = channelId;

            await _context.SaveChangesAsync();
        }

        public async Task ClearWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Welcome = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Welcome);
        }

        public async Task ModifyLogsAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { ServerId = id, Logs = channelId });
            else
                server.Logs = channelId;

            await _context.SaveChangesAsync();
        }

        public async Task ClearLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Logs = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Logs);
        }

        public async Task ModifyBackgroundAsync(ulong id, string url)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { ServerId = id, Background = url });
            else
                server.Background = url;

            await _context.SaveChangesAsync();
        }

        public async Task ClearBckgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Background = null;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetBackgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Background);
        }

        public async Task ModifyLevelUpChannelAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { ServerId = id, LevelUpChannel = channelId });
            else
                server.LevelUpChannel = channelId;

            await _context.SaveChangesAsync();
        }

        public async Task ClearLevelUpChannelAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.LevelUpChannel = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetLevelUpChannelAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.LevelUpChannel);
        }

        public async Task ModifyLevelUpAsync(ulong id, string url)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { ServerId = id, LevelUp = url });
            else
                server.LevelUp = url;

            await _context.SaveChangesAsync();
        }

        public async Task ClearLevelUpAsync(ulong id)
        {
            var server = await _context.Servers
                   .FindAsync(id);

            server.LevelUp = null;
            await _context.SaveChangesAsync();

        }

        public async Task<string> GetLevelUpAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.LevelUp);
        }
    }
}
