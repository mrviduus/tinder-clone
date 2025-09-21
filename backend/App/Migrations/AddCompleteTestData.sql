-- Comprehensive Test Data for Tinder Clone
-- This script adds complete test data including all users mentioned in README
-- Password for all users: Password123!
-- Password hash: AQAAAAEAACcQAAAAEDqugKZkIKVKlkWThWHjDsHwiSBertNHdfw/k3IdGsuY6t62vpypM48Td0H8AYPvRQ==

DO $$
DECLARE
    alice_id UUID := 'a155f3a5-ef09-469a-909b-95fd7a8d8011';
    bob_id UUID := '57eacf2a-42bf-4ad6-8ed2-5a775117aa59';
    charlie_id UUID := 'c3d4e5f6-1234-5678-9abc-def012345678';
    diana_id UUID := 'd4e5f6a7-2345-6789-abcd-ef0123456789';
    eve_id UUID := 'e5f6a7b8-3456-789a-bcde-f01234567890';
    frank_id UUID := 'f6a7b8c9-4567-89ab-cdef-012345678901';
    grace_id UUID := 'a7b8c9d0-5678-9abc-def0-123456789012';
    henry_id UUID := 'b8c9d0e1-6789-abcd-ef01-234567890123';
BEGIN
    -- Ensure Alice exists with complete profile
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'alice@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            alice_id,
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            alice_id,
            'Alice',
            '1999-03-15'::date,
            2, -- Female
            'Love hiking, yoga, and trying new restaurants! Looking for someone who can make me laugh and enjoys adventure.',
            1, -- Looking for Male
            24,
            35,
            50,
            ST_GeomFromText('POINT(-74.0060 40.7128)', 4326), -- NYC
            NOW()
        );
    END IF;

    -- Ensure Bob exists with complete profile
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'bob@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            bob_id,
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            bob_id,
            'Bob',
            '1996-07-22'::date,
            1, -- Male
            'Software engineer by day, musician by night. Love dogs, coffee, and good conversations.',
            2, -- Looking for Female
            22,
            32,
            50,
            ST_GeomFromText('POINT(-74.0050 40.7130)', 4326), -- NYC (nearby)
            NOW()
        );
    END IF;

    -- Add Charlie (Male, 30)
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'charlie@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            charlie_id,
            'charlie@example.com',
            'CHARLIE@EXAMPLE.COM',
            'charlie@example.com',
            'CHARLIE@EXAMPLE.COM',
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            charlie_id,
            'Charlie',
            '1995-01-10'::date,
            1, -- Male
            'Fitness enthusiast, foodie, and travel lover. Let''s explore the city together!',
            2, -- Looking for Female
            21,
            35,
            40,
            ST_GeomFromText('POINT(-73.9851 40.7580)', 4326), -- Manhattan
            NOW()
        );
    END IF;

    -- Add Diana (Female, 26)
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'diana@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            diana_id,
            'diana@example.com',
            'DIANA@EXAMPLE.COM',
            'diana@example.com',
            'DIANA@EXAMPLE.COM',
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            diana_id,
            'Diana',
            '1999-05-20'::date,
            2, -- Female
            'Artist and designer. Love museums, indie music, and deep conversations over wine.',
            1, -- Looking for Male
            25,
            40,
            30,
            ST_GeomFromText('POINT(-73.9626 40.7794)', 4326), -- Upper East Side
            NOW()
        );
    END IF;

    -- Add Eve (Female, 24)
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'eve@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            eve_id,
            'eve@example.com',
            'EVE@EXAMPLE.COM',
            'eve@example.com',
            'EVE@EXAMPLE.COM',
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            eve_id,
            'Eve',
            '2001-11-08'::date,
            2, -- Female
            'Med student, bookworm, and coffee addict. Looking for someone genuine and kind.',
            1, -- Looking for Male
            23,
            35,
            25,
            ST_GeomFromText('POINT(-73.9934 40.7505)', 4326), -- Chelsea
            NOW()
        );
    END IF;

    -- Add Frank (Male, 27) - Additional test user
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'frank@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            frank_id,
            'frank@example.com',
            'FRANK@EXAMPLE.COM',
            'frank@example.com',
            'FRANK@EXAMPLE.COM',
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            frank_id,
            'Frank',
            '1998-04-18'::date,
            1, -- Male
            'Photographer and adventure seeker. Always looking for the next great shot and story.',
            2, -- Looking for Female
            21,
            30,
            60,
            ST_GeomFromText('POINT(-73.9897 40.7295)', 4326), -- East Village
            NOW()
        );
    END IF;

    -- Add Grace (Female, 29) - Additional test user
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'grace@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            grace_id,
            'grace@example.com',
            'GRACE@EXAMPLE.COM',
            'grace@example.com',
            'GRACE@EXAMPLE.COM',
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            grace_id,
            'Grace',
            '1996-09-03'::date,
            2, -- Female
            'Lawyer by profession, dancer by passion. Wine enthusiast and world traveler.',
            1, -- Looking for Male
            26,
            38,
            45,
            ST_GeomFromText('POINT(-73.9808 40.7648)', 4326), -- Midtown
            NOW()
        );
    END IF;

    -- Add Henry (Male, 32) - Additional test user
    IF NOT EXISTS (SELECT 1 FROM users WHERE "Email" = 'henry@example.com') THEN
        INSERT INTO users ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt")
        VALUES (
            henry_id,
            'henry@example.com',
            'HENRY@EXAMPLE.COM',
            'henry@example.com',
            'HENRY@EXAMPLE.COM',
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

        INSERT INTO profiles ("UserId", "DisplayName", "BirthDate", "Gender", "Bio", "SearchGender", "AgeMin", "AgeMax", "MaxDistanceKm", "Location", "LocationUpdatedAt")
        VALUES (
            henry_id,
            'Henry',
            '1993-12-25'::date,
            1, -- Male
            'Chef and food blogger. Can cook you the best meal of your life. Let''s share recipes and stories!',
            2, -- Looking for Female
            24,
            36,
            35,
            ST_GeomFromText('POINT(-73.9442 40.7378)', 4326), -- Brooklyn
            NOW()
        );
    END IF;

    -- Add placeholder photos for all users who don't have them
    INSERT INTO photos ("Id", "UserId", "Data", "ContentType", "SizeBytes", "IsPrimary", "UploadedAt")
    SELECT
        gen_random_uuid(),
        u."Id",
        decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChAGAhhtQHgAAAABJRU5ErkJggg==', 'base64'),
        'image/png',
        68,
        true,
        NOW()
    FROM users u
    WHERE NOT EXISTS (
        SELECT 1 FROM photos p WHERE p."UserId" = u."Id" AND p."IsPrimary" = true
    )
    AND u."Email" IN (
        'alice@example.com', 'bob@example.com', 'charlie@example.com',
        'diana@example.com', 'eve@example.com', 'frank@example.com',
        'grace@example.com', 'henry@example.com'
    );

    -- Add some sample swipes to create interesting scenarios
    -- Alice likes Bob (potential match)
    INSERT INTO swipes ("Id", "SwiperId", "TargetId", "Direction", "CreatedAt")
    SELECT
        gen_random_uuid(),
        alice_id,
        bob_id,
        1, -- Like
        NOW()
    WHERE NOT EXISTS (
        SELECT 1 FROM swipes WHERE "SwiperId" = alice_id AND "TargetId" = bob_id
    );

    -- Bob likes Alice (creates a match!)
    INSERT INTO swipes ("Id", "SwiperId", "TargetId", "Direction", "CreatedAt")
    SELECT
        gen_random_uuid(),
        bob_id,
        alice_id,
        1, -- Like
        NOW()
    WHERE NOT EXISTS (
        SELECT 1 FROM swipes WHERE "SwiperId" = bob_id AND "TargetId" = alice_id
    );

    -- Create match between Alice and Bob if it doesn't exist
    INSERT INTO matches ("Id", "UserAId", "UserBId", "CreatedAt")
    SELECT
        gen_random_uuid(),
        LEAST(alice_id, bob_id),
        GREATEST(alice_id, bob_id),
        NOW()
    WHERE NOT EXISTS (
        SELECT 1 FROM matches
        WHERE ("UserAId" = alice_id AND "UserBId" = bob_id)
           OR ("UserAId" = bob_id AND "UserBId" = alice_id)
    );

    -- Add some initial messages between Alice and Bob
    INSERT INTO messages ("Id", "MatchId", "SenderId", "Content", "SentAt", "ReadAt")
    SELECT
        gen_random_uuid(),
        m."Id",
        alice_id,
        'Hey Bob! Nice to match with you!',
        NOW() - INTERVAL '1 hour',
        NOW() - INTERVAL '45 minutes'
    FROM matches m
    WHERE (m."UserAId" = alice_id AND m."UserBId" = bob_id)
       OR (m."UserAId" = bob_id AND m."UserBId" = alice_id)
    AND NOT EXISTS (
        SELECT 1 FROM messages WHERE "MatchId" = m."Id"
    );

    INSERT INTO messages ("Id", "MatchId", "SenderId", "Content", "SentAt", "ReadAt")
    SELECT
        gen_random_uuid(),
        m."Id",
        bob_id,
        'Hi Alice! Great to match with you too! How are you?',
        NOW() - INTERVAL '30 minutes',
        NOW() - INTERVAL '20 minutes'
    FROM matches m
    WHERE (m."UserAId" = alice_id AND m."UserBId" = bob_id)
       OR (m."UserAId" = bob_id AND m."UserBId" = alice_id)
    AND EXISTS (
        SELECT 1 FROM messages WHERE "MatchId" = m."Id"
    )
    LIMIT 1;

    -- Add more diverse swipes for testing
    -- Charlie likes Diana
    INSERT INTO swipes ("Id", "SwiperId", "TargetId", "Direction", "CreatedAt")
    SELECT
        gen_random_uuid(),
        charlie_id,
        diana_id,
        1, -- Like
        NOW() - INTERVAL '2 hours'
    WHERE NOT EXISTS (
        SELECT 1 FROM swipes WHERE "SwiperId" = charlie_id AND "TargetId" = diana_id
    );

    -- Eve passes on Charlie
    INSERT INTO swipes ("Id", "SwiperId", "TargetId", "Direction", "CreatedAt")
    SELECT
        gen_random_uuid(),
        eve_id,
        charlie_id,
        0, -- Pass
        NOW() - INTERVAL '3 hours'
    WHERE NOT EXISTS (
        SELECT 1 FROM swipes WHERE "SwiperId" = eve_id AND "TargetId" = charlie_id
    );

    -- Frank likes Eve
    INSERT INTO swipes ("Id", "SwiperId", "TargetId", "Direction", "CreatedAt")
    SELECT
        gen_random_uuid(),
        frank_id,
        eve_id,
        1, -- Like
        NOW() - INTERVAL '4 hours'
    WHERE NOT EXISTS (
        SELECT 1 FROM swipes WHERE "SwiperId" = frank_id AND "TargetId" = eve_id
    );

    RAISE NOTICE 'Test data successfully added!';
    RAISE NOTICE 'Users created: alice@example.com, bob@example.com, charlie@example.com, diana@example.com, eve@example.com, frank@example.com, grace@example.com, henry@example.com';
    RAISE NOTICE 'All users have password: Password123!';
    RAISE NOTICE 'Alice and Bob have a match with chat history';
END $$;