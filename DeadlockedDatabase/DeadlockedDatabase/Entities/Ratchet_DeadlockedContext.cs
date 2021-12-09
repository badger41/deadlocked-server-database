using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DeadlockedDatabase.Entities
{
    public partial class Ratchet_DeadlockedContext : DbContext
    {
        public Ratchet_DeadlockedContext()
        {
        }

        public Ratchet_DeadlockedContext(DbContextOptions<Ratchet_DeadlockedContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<AccountFriend> AccountFriend { get; set; }
        public virtual DbSet<AccountIgnored> AccountIgnored { get; set; }
        public virtual DbSet<AccountStat> AccountStat { get; set; }
        public virtual DbSet<AccountStatus> AccountStatus { get; set; }
        public virtual DbSet<Banned> Banned { get; set; }
        public virtual DbSet<BannedIp> BannedIp { get; set; }
        public virtual DbSet<BannedMac> BannedMac { get; set; }
        public virtual DbSet<Clan> Clan { get; set; }
        public virtual DbSet<ClanInvitation> ClanInvitation { get; set; }
        public virtual DbSet<ClanMember> ClanMember { get; set; }
        public virtual DbSet<ClanMessage> ClanMessage { get; set; }
        public virtual DbSet<ClanStat> ClanStat { get; set; }
        public virtual DbSet<DimAnnouncements> DimAnnouncements { get; set; }
        public virtual DbSet<DimEula> DimEula { get; set; }
        public virtual DbSet<DimStats> DimStats { get; set; }
        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<GameHistory> GameHistory { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<ServerFlags> ServerFlags { get; set; }
        public virtual DbSet<ServerLog> ServerLog { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account", "ACCOUNTS");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.AccountName)
                    .IsRequired()
                    .HasColumnName("account_name")
                    .HasMaxLength(32);

                entity.Property(e => e.AccountPassword)
                    .IsRequired()
                    .HasColumnName("account_password")
                    .HasMaxLength(200);

                entity.Property(e => e.AppId).HasColumnName("app_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastSignInDt).HasColumnName("last_sign_in_dt");

                entity.Property(e => e.LastSignInIp)
                    .HasColumnName("last_sign_in_ip")
                    .HasMaxLength(50);

                entity.Property(e => e.MachineId)
                    .HasColumnName("machine_id")
                    .HasMaxLength(100);

                entity.Property(e => e.MediusStats)
                    .HasColumnName("medius_stats")
                    .HasMaxLength(350);

                entity.Property(e => e.Metadata).HasColumnName("metadata");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");
            });

            modelBuilder.Entity<AccountFriend>(entity =>
            {
                entity.ToTable("account_friend", "ACCOUNTS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.FriendAccountId).HasColumnName("friend_account_id");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountFriend)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_account_friend_account");
            });

            modelBuilder.Entity<AccountIgnored>(entity =>
            {
                entity.ToTable("account_ignored", "ACCOUNTS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IgnoredAccountId).HasColumnName("ignored_account_id");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountIgnored)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_account_ignored_account");
            });

            modelBuilder.Entity<AccountStat>(entity =>
            {
                entity.ToTable("account_stat", "STATS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");

                entity.Property(e => e.StatId).HasColumnName("stat_id");

                entity.Property(e => e.StatValue).HasColumnName("stat_value");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountStat)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_account_stat_account");

                entity.HasOne(d => d.Stat)
                    .WithMany(p => p.AccountStat)
                    .HasForeignKey(d => d.StatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_account_stat_dim_stats");
            });

            modelBuilder.Entity<AccountStatus>(entity =>
            {
                entity.ToTable("account_status", "ACCOUNTS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.ChannelId).HasColumnName("channel_id");

                entity.Property(e => e.GameId).HasColumnName("game_id");

                entity.Property(e => e.GameName)
                    .HasColumnName("game_name")
                    .HasMaxLength(32);

                entity.Property(e => e.LoggedIn).HasColumnName("logged_in");

                entity.Property(e => e.WorldId).HasColumnName("world_id");
            });

            modelBuilder.Entity<Banned>(entity =>
            {
                entity.ToTable("banned", "ACCOUNTS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.FromDt)
                    .HasColumnName("from_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ToDt).HasColumnName("to_dt");
            });

            modelBuilder.Entity<BannedIp>(entity =>
            {
                entity.ToTable("banned_ip", "ACCOUNTS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FromDt)
                    .HasColumnName("from_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IpAddress)
                    .IsRequired()
                    .HasColumnName("ip_address")
                    .HasMaxLength(50);

                entity.Property(e => e.ToDt).HasColumnName("to_dt");
            });

            modelBuilder.Entity<BannedMac>(entity =>
            {
                entity.ToTable("banned_mac", "ACCOUNTS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FromDt)
                    .HasColumnName("from_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.MacAddress)
                    .IsRequired()
                    .HasColumnName("mac_address")
                    .HasMaxLength(50);

                entity.Property(e => e.ToDt).HasColumnName("to_dt");
            });

            modelBuilder.Entity<Clan>(entity =>
            {
                entity.ToTable("clan", "CLANS");

                entity.Property(e => e.ClanId).HasColumnName("clan_id");

                entity.Property(e => e.AppId).HasColumnName("app_id");

                entity.Property(e => e.ClanLeaderAccountId).HasColumnName("clan_leader_account_id");

                entity.Property(e => e.ClanName)
                    .IsRequired()
                    .HasColumnName("clan_name")
                    .HasMaxLength(32);

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.MediusStats)
                    .HasColumnName("medius_stats")
                    .HasMaxLength(350);

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");

                entity.HasOne(d => d.ClanLeaderAccount)
                    .WithMany(p => p.Clan)
                    .HasForeignKey(d => d.ClanLeaderAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_account");
            });

            modelBuilder.Entity<ClanInvitation>(entity =>
            {
                entity.ToTable("clan_invitation", "CLANS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.ClanId).HasColumnName("clan_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");

                entity.Property(e => e.ResponseDt).HasColumnName("response_dt");

                entity.Property(e => e.ResponseId).HasColumnName("response_id");

                entity.Property(e => e.ResponseMsg)
                    .HasColumnName("response_msg")
                    .HasMaxLength(50);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ClanInvitation)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_invitation_account");

                entity.HasOne(d => d.Clan)
                    .WithMany(p => p.ClanInvitation)
                    .HasForeignKey(d => d.ClanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_invitation_clan");

                entity.Property(e => e.InviteMsg)
                        .HasColumnName("invite_msg")
                        .HasMaxLength(512);
            });

            modelBuilder.Entity<ClanMember>(entity =>
            {
                entity.ToTable("clan_member", "CLANS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.ClanId).HasColumnName("clan_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ClanMember)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_member_account");

                entity.HasOne(d => d.Clan)
                    .WithMany(p => p.ClanMember)
                    .HasForeignKey(d => d.ClanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_member_clan");
            });

            modelBuilder.Entity<ClanMessage>(entity =>
            {
                entity.ToTable("clan_message", "CLANS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ClanId).HasColumnName("clan_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnName("message")
                    .HasMaxLength(200);

                entity.HasOne(d => d.Clan)
                    .WithMany(p => p.ClanMessage)
                    .HasForeignKey(d => d.ClanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_message_clan");
            });

            modelBuilder.Entity<ClanStat>(entity =>
            {
                entity.ToTable("clan_stat", "STATS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ClanId).HasColumnName("clan_id");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");

                entity.Property(e => e.StatId).HasColumnName("stat_id");

                entity.Property(e => e.StatValue).HasColumnName("stat_value");

                entity.HasOne(d => d.Clan)
                    .WithMany(p => p.ClanStat)
                    .HasForeignKey(d => d.ClanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_stat_clan");

                entity.HasOne(d => d.Stat)
                    .WithMany(p => p.ClanStat)
                    .HasForeignKey(d => d.StatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_clan_stat_dim_stats");
            });

            modelBuilder.Entity<DimAnnouncements>(entity =>
            {
                entity.ToTable("dim_announcements", "KEYS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AnnouncementBody)
                    .IsRequired()
                    .HasColumnName("announcement_body")
                    .HasMaxLength(1000);

                entity.Property(e => e.AnnouncementTitle)
                    .IsRequired()
                    .HasColumnName("announcement_title")
                    .HasMaxLength(50);

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.FromDt)
                    .HasColumnName("from_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");

                entity.Property(e => e.ToDt).HasColumnName("to_dt");
            });

            modelBuilder.Entity<DimEula>(entity =>
            {
                entity.ToTable("dim_eula", "KEYS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.EulaBody)
                    .IsRequired()
                    .HasColumnName("eula_body");

                entity.Property(e => e.EulaTitle)
                    .IsRequired()
                    .HasColumnName("eula_title")
                    .HasMaxLength(50);

                entity.Property(e => e.FromDt)
                    .HasColumnName("from_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ModifiedDt).HasColumnName("modified_dt");

                entity.Property(e => e.ToDt).HasColumnName("to_dt");
            });

            modelBuilder.Entity<DimStats>(entity =>
            {
                entity.HasKey(e => e.StatId);

                entity.ToTable("dim_stats", "KEYS");

                entity.Property(e => e.StatId).HasColumnName("stat_id");

                entity.Property(e => e.DefaultValue).HasColumnName("default_value");

                entity.Property(e => e.StatName)
                    .IsRequired()
                    .HasColumnName("stat_name")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("game", "WORLD");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AppId).HasColumnName("app_id");

                entity.Property(e => e.GameCreateDt)
                    .HasColumnName("game_create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.GameHostType)
                    .IsRequired()
                    .HasColumnName("game_host_type")
                    .HasMaxLength(32);

                entity.Property(e => e.GameId).HasColumnName("game_id");

                entity.Property(e => e.GameLevel).HasColumnName("game_level");

                entity.Property(e => e.GameName)
                    .IsRequired()
                    .HasColumnName("game_name")
                    .HasMaxLength(64);

                entity.Property(e => e.GameStartDt).HasColumnName("game_start_dt");

                entity.Property(e => e.GameStats)
                    .IsRequired()
                    .HasColumnName("game_stats")
                    .HasMaxLength(256)
                    .IsFixedLength();

                entity.Property(e => e.GenericField1).HasColumnName("generic_field_1");

                entity.Property(e => e.GenericField2).HasColumnName("generic_field_2");

                entity.Property(e => e.GenericField3).HasColumnName("generic_field_3");

                entity.Property(e => e.GenericField4).HasColumnName("generic_field_4");

                entity.Property(e => e.GenericField5).HasColumnName("generic_field_5");

                entity.Property(e => e.GenericField6).HasColumnName("generic_field_6");

                entity.Property(e => e.GenericField7).HasColumnName("generic_field_7");

                entity.Property(e => e.GenericField8).HasColumnName("generic_field_8");

                entity.Property(e => e.MaxPlayers).HasColumnName("max_players");

                entity.Property(e => e.Metadata).HasColumnName("metadata");

                entity.Property(e => e.MinPlayers).HasColumnName("min_players");

                entity.Property(e => e.PlayerCount).HasColumnName("player_count");

                entity.Property(e => e.PlayerListCurrent)
                    .HasColumnName("player_list_current")
                    .HasMaxLength(250);

                entity.Property(e => e.PlayerListStart)
                    .HasColumnName("player_list_start")
                    .HasMaxLength(250);

                entity.Property(e => e.PlayerSkillLevel).HasColumnName("player_skill_level");

                entity.Property(e => e.RuleSet).HasColumnName("rule_set");

                entity.Property(e => e.WorldStatus)
                    .IsRequired()
                    .HasColumnName("world_status")
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<GameHistory>(entity =>
            {
                entity.ToTable("game_history", "WORLD");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AppId).HasColumnName("app_id");

                entity.Property(e => e.GameCreateDt).HasColumnName("game_create_dt");

                entity.Property(e => e.GameEndDt).HasColumnName("game_end_dt");

                entity.Property(e => e.GameHostType)
                    .IsRequired()
                    .HasColumnName("game_host_type")
                    .HasMaxLength(32);

                entity.Property(e => e.GameId).HasColumnName("game_id");

                entity.Property(e => e.GameLevel).HasColumnName("game_level");

                entity.Property(e => e.GameName)
                    .IsRequired()
                    .HasColumnName("game_name")
                    .HasMaxLength(64);

                entity.Property(e => e.GameStartDt).HasColumnName("game_start_dt");

                entity.Property(e => e.GameStats)
                    .IsRequired()
                    .HasColumnName("game_stats")
                    .HasMaxLength(256)
                    .IsFixedLength();

                entity.Property(e => e.GenericField1).HasColumnName("generic_field_1");

                entity.Property(e => e.GenericField2).HasColumnName("generic_field_2");

                entity.Property(e => e.GenericField3).HasColumnName("generic_field_3");

                entity.Property(e => e.GenericField4).HasColumnName("generic_field_4");

                entity.Property(e => e.GenericField5).HasColumnName("generic_field_5");

                entity.Property(e => e.GenericField6).HasColumnName("generic_field_6");

                entity.Property(e => e.GenericField7).HasColumnName("generic_field_7");

                entity.Property(e => e.GenericField8).HasColumnName("generic_field_8");

                entity.Property(e => e.MaxPlayers).HasColumnName("max_players");

                entity.Property(e => e.Metadata).HasColumnName("metadata");

                entity.Property(e => e.MinPlayers).HasColumnName("min_players");

                entity.Property(e => e.PlayerCount).HasColumnName("player_count");

                entity.Property(e => e.PlayerListCurrent)
                    .HasColumnName("player_list_current")
                    .HasMaxLength(250);

                entity.Property(e => e.PlayerListStart)
                    .HasColumnName("player_list_start")
                    .HasMaxLength(250);

                entity.Property(e => e.PlayerSkillLevel).HasColumnName("player_skill_level");

                entity.Property(e => e.RuleSet).HasColumnName("rule_set");

                entity.Property(e => e.WorldStatus)
                    .IsRequired()
                    .HasColumnName("world_status")
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(e => e.RoleId);

                entity.ToTable("roles", "KEYS");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasColumnName("role_name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ServerFlags>(entity =>
            {
                entity.ToTable("server_flags", "KEYS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FromDt).HasColumnName("from_dt");

                entity.Property(e => e.ServerFlag)
                    .IsRequired()
                    .HasColumnName("server_flag")
                    .HasMaxLength(50);

                entity.Property(e => e.ToDt).HasColumnName("to_dt");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ServerLog>(entity =>
            {
                entity.ToTable("server_log", "LOGS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.LogDt)
                    .HasColumnName("log_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.LogMsg)
                    .IsRequired()
                    .HasColumnName("log_msg");

                entity.Property(e => e.LogStacktrace).HasColumnName("log_stacktrace");

                entity.Property(e => e.LogTitle)
                    .IsRequired()
                    .HasColumnName("log_title")
                    .HasMaxLength(200);

                entity.Property(e => e.MethodName)
                    .HasColumnName("method_name")
                    .HasMaxLength(50);

                entity.Property(e => e.Payload).HasColumnName("payload");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_role", "ACCOUNTS");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.FromDt)
                    .HasColumnName("from_dt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.ToDt).HasColumnName("to_dt");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
