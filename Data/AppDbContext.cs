using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Models;

namespace UsuariosAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── Tablas ───────────────────────────────────────────────────────────
        public DbSet<Usuario>       Usuarios       { get; set; }
        public DbSet<CuentaUsuario> CuentasUsuario { get; set; }
        public DbSet<Producto>      Productos      { get; set; }
        public DbSet<Proveedor>     Proveedores    { get; set; }
        public DbSet<Categoria>     Categorias     { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Usuarios ─────────────────────────────────────────────────────
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            modelBuilder.Entity<CuentaUsuario>()
                .HasIndex(c => c.Username)
                .IsUnique();

            modelBuilder.Entity<CuentaUsuario>()
                .HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<CuentaUsuario>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Producto → Categoria (N:1) ────────────────────────────────────
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Producto → Proveedor (N:1) ────────────────────────────────────
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
