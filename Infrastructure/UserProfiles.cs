using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class UserProfiles
    {
        private readonly EssentiBotContext _context;

        public UserProfiles(EssentiBotContext context)
        {
            _context = context;
        }

        public async Task<UserProfile> GetOrCreateUserProfile(ulong serverId, ulong userId)
        {
            var userProfile = await _context.UserProfiles
                .Where(x => x.ServerId == serverId)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if(userProfile != null)
            {
                return await Task.FromResult(userProfile);
            }

            userProfile = new UserProfile
            {
                ServerId = serverId,
                UserId = userId,
                Level = 1,
                Exp = 0,
                EssentiCoins = 0
            };

            _context.Add(userProfile);

            await _context.SaveChangesAsync();

            return userProfile;
        }

        public async Task<List<UserProfile>> GetAllUserProfiles(ulong serverId)
        {
            var userProfiles = await _context.UserProfiles
                .Where(x => x.ServerId == serverId)
                .OrderByDescending(x => x.Level)
                .Take(10).ToListAsync();
                
                

            return await Task.FromResult(userProfiles);
        }

        public async Task ModifyLevelAsync(ulong userId, int level)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (userProfile != null)
            {
                userProfile.Level = level;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ModifyExpAsync(ulong userId, ulong exp)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (userProfile != null)
            {
                userProfile.Exp += exp;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ModifyEssentiCoinsAsync(ulong userId, ulong essentiCoins)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (userProfile != null)
            {
                userProfile.EssentiCoins += essentiCoins;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SubtractEssentiCoinsAsync(ulong userId, ulong essentiCoins)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (userProfile != null)
            {
                userProfile.EssentiCoins -= essentiCoins;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLevelAsync(ulong userId)
        {
            var userProfile = await _context.UserProfiles
               .FindAsync(userId);

            return await Task.FromResult(userProfile.Level);
        }

        public async Task<ulong> GetExpAsync(ulong userId)
        {
            var userProfile = await _context.UserProfiles
               .FindAsync(userId);

            return await Task.FromResult(userProfile.Exp);
        }

        public async Task<ulong> GetEssentiCoinsAsync(ulong userId)
        {
            var userProfile = await _context.UserProfiles
               .FindAsync(userId);

            return await Task.FromResult(userProfile.EssentiCoins);
        }
    }
}
