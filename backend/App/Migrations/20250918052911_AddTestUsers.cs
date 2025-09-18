using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class AddTestUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Test users with password "Password123!"
            // These will be inserted only if they don't already exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Insert Alice if she doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM users WHERE ""Email"" = 'alice@example.com') THEN
                        INSERT INTO users (""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"", ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"", ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"", ""CreatedAt"")
                        VALUES (
                            'a155f3a5-ef09-469a-909b-95fd7a8d8011',
                            'alice@example.com',
                            'ALICE@EXAMPLE.COM',
                            'alice@example.com',
                            'ALICE@EXAMPLE.COM',
                            true,
                            'AQAAAAEAACcQAAAAEDqugKZkIKVKlkWThWHjDsHwiSBertNHdfw/k3IdGsuY6t62vpypM48Td0H8AYPvRQ==',
                            UPPER(REPLACE(CAST(gen_random_uuid() AS text), '-', '')),
                            CAST(gen_random_uuid() AS text),
                            false,
                            false,
                            true,
                            0,
                            NOW()
                        );

                        -- Insert Alice's profile
                        INSERT INTO profiles (""UserId"", ""DisplayName"", ""BirthDate"", ""Gender"", ""Bio"", ""SearchGender"", ""AgeMin"", ""AgeMax"", ""MaxDistanceKm"", ""Location"", ""LocationUpdatedAt"")
                        VALUES (
                            'a155f3a5-ef09-469a-909b-95fd7a8d8011',
                            'Alice',
                            '1999-01-01'::date,
                            2, -- Female
                            'Hi, I''m Alice! Looking to meet new people in the city.',
                            1, -- Looking for Male
                            22,
                            35,
                            25,
                            ST_GeomFromText('POINT(-74.0060 40.7128)', 4326), -- NYC
                            NOW()
                        );
                    END IF;

                    -- Insert Bob if he doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM users WHERE ""Email"" = 'bob@example.com') THEN
                        INSERT INTO users (""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"", ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"", ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"", ""CreatedAt"")
                        VALUES (
                            '57eacf2a-42bf-4ad6-8ed2-5a775117aa59',
                            'bob@example.com',
                            'BOB@EXAMPLE.COM',
                            'bob@example.com',
                            'BOB@EXAMPLE.COM',
                            true,
                            'AQAAAAEAACcQAAAAEDqugKZkIKVKlkWThWHjDsHwiSBertNHdfw/k3IdGsuY6t62vpypM48Td0H8AYPvRQ==',
                            UPPER(REPLACE(CAST(gen_random_uuid() AS text), '-', '')),
                            CAST(gen_random_uuid() AS text),
                            false,
                            false,
                            true,
                            0,
                            NOW()
                        );

                        -- Insert Bob's profile
                        INSERT INTO profiles (""UserId"", ""DisplayName"", ""BirthDate"", ""Gender"", ""Bio"", ""SearchGender"", ""AgeMin"", ""AgeMax"", ""MaxDistanceKm"", ""Location"", ""LocationUpdatedAt"")
                        VALUES (
                            '57eacf2a-42bf-4ad6-8ed2-5a775117aa59',
                            'Bob',
                            '1996-01-01'::date,
                            1, -- Male
                            'Hi, I''m Bob! Looking to meet new people in the city.',
                            2, -- Looking for Female
                            22,
                            35,
                            25,
                            ST_GeomFromText('POINT(-74.0050 40.7130)', 4326), -- NYC
                            NOW()
                        );

                        -- Insert Alice's profile photo (placeholder)
                        INSERT INTO photos (""Id"", ""UserId"", ""Data"", ""ContentType"", ""SizeBytes"", ""IsPrimary"", ""UploadedAt"")
                        VALUES (
                            gen_random_uuid(),
                            'a155f3a5-ef09-469a-909b-95fd7a8d8011',
                            decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChAGAhhtQHgAAAABJRU5ErkJggg==', 'base64'),
                            'image/png',
                            68,
                            true,
                            NOW()
                        );

                        -- Insert Bob's profile photo (placeholder)
                        INSERT INTO photos (""Id"", ""UserId"", ""Data"", ""ContentType"", ""SizeBytes"", ""IsPrimary"", ""UploadedAt"")
                        VALUES (
                            gen_random_uuid(),
                            '57eacf2a-42bf-4ad6-8ed2-5a775117aa59',
                            decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==', 'base64'),
                            'image/png',
                            68,
                            true,
                            NOW()
                        );
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove test users
            migrationBuilder.Sql(@"
                DELETE FROM messages WHERE ""MatchId"" IN (
                    SELECT ""Id"" FROM matches WHERE ""UserAId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59')
                    OR ""UserBId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59')
                );
                DELETE FROM matches WHERE ""UserAId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59')
                    OR ""UserBId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59');
                DELETE FROM swipes WHERE ""SwiperId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59')
                    OR ""TargetId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59');
                DELETE FROM photos WHERE ""UserId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59');
                DELETE FROM profiles WHERE ""UserId"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59');
                DELETE FROM users WHERE ""Id"" IN ('a155f3a5-ef09-469a-909b-95fd7a8d8011', '57eacf2a-42bf-4ad6-8ed2-5a775117aa59');
            ");
        }
    }
}
