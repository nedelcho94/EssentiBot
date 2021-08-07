using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure
{
    public class EssentiBotContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer("server=localhost;Database=EssentiBot;Trusted_Connection=True;MultipleActiveResultSets=true");

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<InventoryItem>()
                .HasKey(ii => new { ii.InventoryId, ii.ItemId });
            builder.Entity<InventoryItem>()
                .HasOne(ii => ii.Inventory)
                .WithMany(ii => ii.Items)
                .HasForeignKey(ii => ii.InventoryId);
            builder.Entity<InventoryItem>()
                .HasOne(ii => ii.Item)
                .WithMany(ii => ii.Inventories)
                .HasForeignKey(ii => ii.ItemId);
        }
    }

    public class Server
    {
        public ulong ServerId { get; set; }
        public string Prefix { get; set; }
        public ulong Welcome { get; set; }
        public string Background { get; set; }
        public ulong LevelUpChannel { get; set; }
        public string LevelUp { get; set; }
        public ulong Logs { get; set; }
    }

    public class Rank
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }

        public Server Server { get; set; }
    }

    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }

        public Server Server { get; set; }
    }

    public class UserProfile
    {
        [Key]
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }
        public int Level { get; set; }
        public ulong Exp { get; set; }
        public ulong EssentiCoins { get; set; }

        public virtual Inventory Inventory { get; set; }
        public Server Server { get; set; }
    }

    public class Inventory
    {
        public int InventoryId { get; set; }
        public ulong UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserProfile UserProfile { get; set; }
        public ICollection<InventoryItem> Items { get; set; }
    }

    public class Item
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public ulong SellPrice { get; set; }
        public string Rarity { get; set; } //change to int

        public ICollection<InventoryItem> Inventories { get; set; }
    }

    public class InventoryItem
    {
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }
}
