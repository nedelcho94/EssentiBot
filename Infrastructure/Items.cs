using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{

    public class Items
    {
        private readonly EssentiBotContext _context;

        public Items(EssentiBotContext context)
        {
            _context = context;
        }

        public async Task<Item> GetItemByRarityAsync(string rarity)
        {
            var item = await _context.Items
                .FindAsync(rarity);

            return await Task.FromResult(item);
        }

        public async Task<Item> GetItemByIdAsync(int itemId)
        {
            var item = await _context.Items
                .FindAsync(itemId);

            return await Task.FromResult(item);
        }

        public async Task AddItemAsync(string itemName, ulong sellPrice, string rarity)
        {
            _context.Add(new Item { ItemName = itemName, SellPrice = sellPrice, Rarity = rarity}); //change rarity to int
            await _context.SaveChangesAsync();
        }

        //public async Task AddItemToUser(string itemName, ulong userId, ulong serverId)
        //{
        //    var server = await _context.Servers.FindAsync(serverId);

        //    var user = await _context.UserProfiles.FindAsync(userId);
        //}

        public async Task AddInventory(ulong userId)
        {
            _context.Add(new Inventory { UserId = userId });
            await _context.SaveChangesAsync();
        }

        public async Task<Inventory> GetInventoryByUserId(ulong userId)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(x => x.UserId == userId);

            return await Task.FromResult(inventory);
        }

        public async Task<List<Item>> GetItemsFromInventory()
        {
            var items = await _context.Items
                .Include(nameof(Inventory.InventoryId))
                .Include(nameof(InventoryItem.Quantity))
                .AsNoTracking()
                .ToListAsync();

            return await Task.FromResult(items);
        }

    }
}
