using System.Data.Entity;

namespace SongSelector.Models
{
    public class SongSelectorEntities : DbContext
    {
        public DbSet<Song> Songs { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
    }
}