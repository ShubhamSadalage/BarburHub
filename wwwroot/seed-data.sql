-- ============================================================
-- BarberHub Seed Data
-- 5 barbers · 15 products each (75 total) · 8 users · 12 bookings
-- All passwords: Test@123
-- Run on a database that already has the schema (after dotnet ef database update)
-- Safe to re-run: uses ON CONFLICT DO NOTHING for users, deletes & re-inserts data
-- ============================================================

BEGIN;

-- 1) Roles
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES
  (gen_random_uuid()::text, 'User',  'USER',  gen_random_uuid()::text),
  (gen_random_uuid()::text, 'Admin', 'ADMIN', gen_random_uuid()::text)
ON CONFLICT ("NormalizedName") DO NOTHING;

-- 2) Barber accounts (Admin role)
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "Address", "City", "Latitude", "Longitude",
  "ShopName", "ShopDescription", "WeeklyHoliday", "OpeningTime", "ClosingTime",
  "CreatedAt", "IsActive"
) VALUES (
  '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', 'rohit@sharpedge.in', 'ROHIT@SHARPEDGE.IN', 'rohit@sharpedge.in', 'ROHIT@SHARPEDGE.IN',
  TRUE, 'AQAAAAEAACcQAAAAEDirper8vzNpjAVR5NtmIh2CC+WSe1Al0kduKwzpY9KzalfjhAEV0jIc8Lrmi68Agw==', '682A1002B40A4F558CE79844350FFCEE', 'ee9a54db-9444-469f-90bc-26bf6cabef6c',
  '+919811100001', TRUE, FALSE, TRUE, 0,
  'Rohit Sharma', '/uploads/profiles/barber-the-sharp-edge.svg', 'Shop 12, Linking Road', 'Mumbai', 19.0596, 72.8295,
  'The Sharp Edge', 'Modern cuts, classic vibe. Mumbai''s go-to for fades, beard sculpting, and grooming consultations.', 0, '09:00:00', '21:00:00',
  NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "Address", "City", "Latitude", "Longitude",
  "ShopName", "ShopDescription", "WeeklyHoliday", "OpeningTime", "ClosingTime",
  "CreatedAt", "IsActive"
) VALUES (
  'a344efdc-131b-454c-9e64-4616bbc77438', 'vikram@kingsbarber.in', 'VIKRAM@KINGSBARBER.IN', 'vikram@kingsbarber.in', 'VIKRAM@KINGSBARBER.IN',
  TRUE, 'AQAAAAEAACcQAAAAEK00MoAyELir+/69KF8KqB7wFMAd1oADihywoPkMCG2dKfLC5XgQBztBm70J/23WQw==', 'CDDF6BB5C12A420EAFFA5F015CC61522', 'dcfb418d-6a51-4d04-b98a-8802ca7f27bb',
  '+919811100002', TRUE, FALSE, TRUE, 0,
  'Vikram Singh', '/uploads/profiles/barber-kings-barber-lounge.svg', '23 MG Road, Brigade Road', 'Bengaluru', 12.9716, 77.5946,
  'Kings Barber Lounge', 'Premium grooming experience with hot towel shaves, scalp massages, and signature haircuts.', 1, '10:00:00', '22:00:00',
  NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "Address", "City", "Latitude", "Longitude",
  "ShopName", "ShopDescription", "WeeklyHoliday", "OpeningTime", "ClosingTime",
  "CreatedAt", "IsActive"
) VALUES (
  '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', 'arjun@thedapper.in', 'ARJUN@THEDAPPER.IN', 'arjun@thedapper.in', 'ARJUN@THEDAPPER.IN',
  TRUE, 'AQAAAAEAACcQAAAAEB5FFUWeZD+WGwwkclxl9qrO9enEZt4DD9Vlg7ggEkQO97GVdr8uOohRcvL6wHuBUw==', '06E5EAF6367E41A1A25C88E1E26D3D93', '2b80a2d6-e376-4597-975e-3563d414c9b9',
  '+919811100003', TRUE, FALSE, TRUE, 0,
  'Arjun Mehta', '/uploads/profiles/barber-the-dapper-den.svg', 'C-45 Connaught Place', 'New Delhi', 28.6328, 77.2197,
  'The Dapper Den', 'Old-school barbershop charm meets modern style. Specialists in pompadours and traditional shaves.', 2, '09:00:00', '20:00:00',
  NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "Address", "City", "Latitude", "Longitude",
  "ShopName", "ShopDescription", "WeeklyHoliday", "OpeningTime", "ClosingTime",
  "CreatedAt", "IsActive"
) VALUES (
  '52da7eca-2eb8-4ad4-b944-e6128d350e3c', 'saurabh@trimnstyle.in', 'SAURABH@TRIMNSTYLE.IN', 'saurabh@trimnstyle.in', 'SAURABH@TRIMNSTYLE.IN',
  TRUE, 'AQAAAAEAACcQAAAAEMGuKUQX5526//bIvQ1RhsNxVSoVFFWt43olHU3m95wUVI2iE/iNKcCiWE/2PiNTxg==', 'BA5098908DEB4BF79E839B804AE2669F', '1c1851b8-d318-43e0-9412-1d98af819736',
  '+919811100004', TRUE, FALSE, TRUE, 0,
  'Saurabh Patil', '/uploads/profiles/barber-trim--style-studio.svg', 'Plot 8, FC Road', 'Pune', 18.5204, 73.8567,
  'Trim & Style Studio', 'Friendly neighborhood barber with affordable rates and quality service. Family-run since 2010.', 0, '08:00:00', '21:00:00',
  NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "Address", "City", "Latitude", "Longitude",
  "ShopName", "ShopDescription", "WeeklyHoliday", "OpeningTime", "ClosingTime",
  "CreatedAt", "IsActive"
) VALUES (
  'b4e17884-cf2c-4146-9af3-096fe1f3256a', 'rahul@thebarberco.in', 'RAHUL@THEBARBERCO.IN', 'rahul@thebarberco.in', 'RAHUL@THEBARBERCO.IN',
  TRUE, 'AQAAAAEAACcQAAAAEH8IaPAQ1+rh5Jjit46Zl4J88eATN5SNmG257TYwNm6v1So7EWoVV6qruLFXXQuOXw==', 'E941B50B37C54456A8819BAD85943BA2', 'bdd6ef46-0e88-4f3e-be47-0ede00939926',
  '+919811100005', TRUE, FALSE, TRUE, 0,
  'Rahul Verma', '/uploads/profiles/barber-the-barber-co.svg', 'Sector 29, Cyber Hub', 'Gurugram', 28.4595, 77.0266,
  'The Barber Co.', 'Boutique unisex salon offering precision cuts, color services, and luxury grooming products.', 3, '10:00:00', '22:00:00',
  NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;

-- 3) Assign Admin role to barbers
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 'a344efdc-131b-454c-9e64-4616bbc77438', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '52da7eca-2eb8-4ad4-b944-e6128d350e3c', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 'b4e17884-cf2c-4146-9af3-096fe1f3256a', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN'
ON CONFLICT DO NOTHING;

-- 4) Customer accounts (User role)
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  '1b2b0a87-aaa2-4b2f-9c54-969e0703cffd', 'priya.k@example.com', 'PRIYA.K@EXAMPLE.COM', 'priya.k@example.com', 'PRIYA.K@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAEAOpJN3ci5S4FeMtozVbyHimTJTj5y5/SuxDOwrihatYBU86YTxT33noyEmEvZfevw==', 'FE1722429B3B457BBA214F595FEC8CC9', 'f16badee-7eba-40bf-9866-176956870d54',
  '+919999900001', TRUE, FALSE, TRUE, 0,
  'Priya Kapoor', '/uploads/profiles/user-priya-kapoor.svg', 'Mumbai', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  '6592b3bd-56e6-44f2-9615-161b1a522f06', 'rajesh.n@example.com', 'RAJESH.N@EXAMPLE.COM', 'rajesh.n@example.com', 'RAJESH.N@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAEKUN77yTtbbc6vMJBXiZuEQsMyluoKUdEnQqq4ZXhCVxpQFH+/CidOytrXLgDrB6ew==', 'D740058D39D6477BB3903770D74DD36C', '911754f5-a0e1-47e6-82e2-47759b968b62',
  '+919999900002', TRUE, FALSE, TRUE, 0,
  'Rajesh Nair', '/uploads/profiles/user-rajesh-nair.svg', 'Bengaluru', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  '748d72f2-0732-4491-a226-a17549e3dc89', 'anita.s@example.com', 'ANITA.S@EXAMPLE.COM', 'anita.s@example.com', 'ANITA.S@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAEHfj1iWkbqbZBBqrVCMQdHvSoK/3zgYyS7Ny+eIglo5/AtkhJtddGzru0p+5M8Na+g==', 'CBFC399C46E44B19A46CB488CB675AF2', 'e8ee778e-4978-40b7-ab9e-51b2c32d6a0d',
  '+919999900003', TRUE, FALSE, TRUE, 0,
  'Anita Singh', '/uploads/profiles/user-anita-singh.svg', 'New Delhi', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  'd70fae7b-8cb3-4b0b-ad39-715595d45f55', 'kunal.j@example.com', 'KUNAL.J@EXAMPLE.COM', 'kunal.j@example.com', 'KUNAL.J@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAEDG4bz1bYQu1tO7UG1G2Y9W1EiQjwh9hQ0+iNgujFjrLQ8eAJXY7lpa8JeOHWSSBrw==', 'DDD8CA452F9E4EE6841770BC6C54B49C', 'f6dcbd4d-1e6c-42b5-bf27-34206a1f6de9',
  '+919999900004', TRUE, FALSE, TRUE, 0,
  'Kunal Joshi', '/uploads/profiles/user-kunal-joshi.svg', 'Pune', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  'b077bc1d-d19b-4bc1-b40b-164e96cdd6e6', 'neha.r@example.com', 'NEHA.R@EXAMPLE.COM', 'neha.r@example.com', 'NEHA.R@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAECtcp0rC7MHHc9bkKf/LgyPP32WwR27T1cLrO3MLTcCxZDpmEPMhf6w7/fDFRmBfqA==', 'FE2483118C1045179FEF4AD87DD2F27A', 'fc1df3cd-ee0b-431a-bb1b-2275befcf586',
  '+919999900005', TRUE, FALSE, TRUE, 0,
  'Neha Reddy', '/uploads/profiles/user-neha-reddy.svg', 'Gurugram', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  '9f14df1f-5d53-4141-9abe-e4259bb73601', 'amit.g@example.com', 'AMIT.G@EXAMPLE.COM', 'amit.g@example.com', 'AMIT.G@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAENgWSwfb1hQp314gANi58W1M/tkyjviDneMKv+N2E0sQXVUepOD8BmIm2MFM0PwFSQ==', 'E7B94D52D7FE43F9B337900BBAD817E1', '20d3aa1f-1682-4840-a445-fff11dcbd769',
  '+919999900006', TRUE, FALSE, TRUE, 0,
  'Amit Gupta', '/uploads/profiles/user-amit-gupta.svg', 'Mumbai', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  '631002d9-d080-46d6-8afa-0e2fa1547142', 'sneha.m@example.com', 'SNEHA.M@EXAMPLE.COM', 'sneha.m@example.com', 'SNEHA.M@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAEPt9vnUBlryY7c/WwIGe0bxS8SSg3WBOKgCeRVOdjIrqaxg8gvsCxeh9cZiL0XgTGw==', '1278C126865F417884760F758214DFFC', '1f45dc54-6c54-40ab-a01f-a5a0ba684051',
  '+919999900007', TRUE, FALSE, TRUE, 0,
  'Sneha Menon', '/uploads/profiles/user-sneha-menon.svg', 'Bengaluru', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;
INSERT INTO "AspNetUsers" (
  "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
  "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
  "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
  "FullName", "ProfileImageUrl", "City", "CreatedAt", "IsActive"
) VALUES (
  '00db77d6-c0e9-4950-b422-b92bee563aea', 'varun.t@example.com', 'VARUN.T@EXAMPLE.COM', 'varun.t@example.com', 'VARUN.T@EXAMPLE.COM',
  TRUE, 'AQAAAAEAACcQAAAAEOqevql7Zj/WZ9A6BkCQZWj14/fKbSJWlKZZG86dnygFlLSh7XEXwxW3hj46Rkb7/w==', 'BEA4676FD5724C8AAF7B70FAD5540543', '05506c4f-8672-4fb7-8999-1b8f3e973f70',
  '+919999900008', TRUE, FALSE, TRUE, 0,
  'Varun Thakur', '/uploads/profiles/user-varun-thakur.svg', 'Pune', NOW(), TRUE
) ON CONFLICT ("NormalizedEmail") DO NOTHING;

-- 5) Assign User role to customers
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '1b2b0a87-aaa2-4b2f-9c54-969e0703cffd', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '6592b3bd-56e6-44f2-9615-161b1a522f06', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '748d72f2-0732-4491-a226-a17549e3dc89', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 'd70fae7b-8cb3-4b0b-ad39-715595d45f55', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 'b077bc1d-d19b-4bc1-b40b-164e96cdd6e6', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '9f14df1f-5d53-4141-9abe-e4259bb73601', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '631002d9-d080-46d6-8afa-0e2fa1547142', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT '00db77d6-c0e9-4950-b422-b92bee563aea', "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'USER'
ON CONFLICT DO NOTHING;

-- 6) Services (5 per barber)
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('c2a2a149-7bb7-4edf-8963-53885f7aa89a', 'Haircut', 'Classic men''s haircut with styling.', 300.0, 30, TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('b7a1f037-4a8a-430a-8446-61887b67d446', 'Beard Trim', 'Professional beard trimming and shaping.', 200.0, 20, TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('31ef7697-0629-47aa-a785-89a0bbb10838', 'Haircut + Beard', 'Complete grooming package, save more.', 450.0, 45, TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('6dffc126-b62c-489f-ad71-36be58611504', 'Hair Color', 'Professional hair coloring service.', 1200.0, 90, TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('e1526b5f-64d9-46d2-a1b0-efcb367ec9d7', 'Head Massage', 'Relaxing head and shoulder massage.', 400.0, 30, TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('fd77b077-4cc0-4132-9ad5-a3650bbc0cf7', 'Haircut', 'Classic men''s haircut with styling.', 390.0, 30, TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('b32c0a44-b9c7-49ec-985e-c9afd1838786', 'Beard Trim', 'Professional beard trimming and shaping.', 260.0, 20, TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('e01bf665-f5ed-46bd-befc-1795c6651bc4', 'Haircut + Beard', 'Complete grooming package, save more.', 585.0, 45, TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('f93dec4f-50ea-49d0-878d-7eec635c1e0c', 'Hair Color', 'Professional hair coloring service.', 1560.0, 90, TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('2246f201-466a-45fc-813b-95ed3624b494', 'Head Massage', 'Relaxing head and shoulder massage.', 520.0, 30, TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('f7d2e06a-97bd-436a-a07f-590e602dd069', 'Haircut', 'Classic men''s haircut with styling.', 330.0, 30, TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('b2c898d6-ff13-4298-bd7a-f1f3f4ff4f5c', 'Beard Trim', 'Professional beard trimming and shaping.', 220.0, 20, TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('a075bc25-1591-4053-81e9-66344334a764', 'Haircut + Beard', 'Complete grooming package, save more.', 495.0, 45, TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('1c16cac4-095e-4b1d-82a7-d84d596845c1', 'Hair Color', 'Professional hair coloring service.', 1320.0, 90, TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('f20f6a88-12d4-474b-93cf-261378fa0d0b', 'Head Massage', 'Relaxing head and shoulder massage.', 440.0, 30, TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('dc7f6152-41e5-4174-bf77-7c9c4c8c583c', 'Haircut', 'Classic men''s haircut with styling.', 255.0, 30, TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('3f9d899b-0d83-4d51-9541-087684dee6b8', 'Beard Trim', 'Professional beard trimming and shaping.', 170.0, 20, TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('e8abf33d-73cc-4ec9-bbdc-806bb28cbcae', 'Haircut + Beard', 'Complete grooming package, save more.', 382.5, 45, TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('a5fb5538-b3f3-46d6-b5cd-6789bde3f089', 'Hair Color', 'Professional hair coloring service.', 1020.0, 90, TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('25e31876-9c2a-44e9-b500-03980a9e34d3', 'Head Massage', 'Relaxing head and shoulder massage.', 340.0, 30, TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('1cca2b5e-d227-4502-9e8f-0fe56e96a140', 'Haircut', 'Classic men''s haircut with styling.', 360.0, 30, TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('07f4bb76-15b7-4ee4-b362-b7ab62524fed', 'Beard Trim', 'Professional beard trimming and shaping.', 240.0, 20, TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('ffed8fa3-f443-49d1-b070-8b33c61dfe68', 'Haircut + Beard', 'Complete grooming package, save more.', 540.0, 45, TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('5e170010-4ec3-4db7-99dd-9a062850551b', 'Hair Color', 'Professional hair coloring service.', 1440.0, 90, TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Services" ("Id","Name","Description","Price","DurationMinutes","IsActive","BarberId","CreatedAt")
VALUES ('79b76945-c38c-4545-91ef-2668e06cfcf8', 'Head Massage', 'Relaxing head and shoulder massage.', 480.0, 30, TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());

-- 7) Products (15 per barber)
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('deba4341-8835-4cf1-9772-6f21429794b9', 'Premium Hair Wax', 'Strong-hold matte hair wax for textured, all-day styles.', 450.0, NULL, 18, '/uploads/products/the-sharp-edge-premium-hair-wax.svg', 'Styling', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('17b20166-ab8f-4c5f-8d35-6a79fa4ae5ac', 'Beard Oil', 'Nourishing beard oil with argan and jojoba.', 550.0, NULL, 46, '/uploads/products/the-sharp-edge-beard-oil.svg', 'Beard Care', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('51a87f7b-c2f4-448a-86d7-6866ded9673b', 'Anti-Dandruff Shampoo', 'Gentle daily shampoo that fights flakes.', 320.0, NULL, 101, '/uploads/products/the-sharp-edge-anti-dandruff-shampoo.svg', 'Hair Care', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('545155b9-dc4e-47be-b161-1becff27d8bc', 'Aftershave Lotion', 'Soothing aftershave with menthol and aloe.', 380.0, NULL, 84, '/uploads/products/the-sharp-edge-aftershave-lotion.svg', 'Shaving', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('bb7b7f08-d5d5-4880-ab1f-5b2833a82361', 'Hair Pomade', 'Classic pomade for a sleek, glossy finish.', 650.0, 15, 19, '/uploads/products/the-sharp-edge-hair-pomade.svg', 'Styling', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('6ad20e1e-d20e-4073-a388-e3d3204b734e', 'Wooden Beard Comb', 'Anti-static wooden comb for daily beard grooming.', 200.0, 5, 44, '/uploads/products/the-sharp-edge-wooden-beard-comb.svg', 'Accessories', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('8b0efb21-74b0-45ce-a408-656949df5d1b', 'Hair Serum', 'Lightweight serum for smooth, frizz-free hair.', 480.0, NULL, 86, '/uploads/products/the-sharp-edge-hair-serum.svg', 'Hair Care', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('1153d026-ab93-4f85-b94a-8962146d8ca3', 'Beard Balm', 'Rich balm that conditions and tames coarse beards.', 620.0, 15, 43, '/uploads/products/the-sharp-edge-beard-balm.svg', 'Beard Care', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('b6130a4f-e196-4c1d-ac06-ac7a9b2f9f53', 'Shaving Cream', 'Rich-lather shaving cream for a close, smooth shave.', 290.0, 10, 118, '/uploads/products/the-sharp-edge-shaving-cream.svg', 'Shaving', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('f6e1c519-5cec-48a0-a542-18d359028979', 'Styling Hair Spray', 'Flexible-hold finishing spray for any look.', 420.0, NULL, 112, '/uploads/products/the-sharp-edge-styling-hair-spray.svg', 'Styling', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('090dd385-432a-437a-8399-2686b187f4e0', 'Hair Conditioner', 'Deep moisturizing conditioner for soft, shiny hair.', 350.0, NULL, 104, '/uploads/products/the-sharp-edge-hair-conditioner.svg', 'Hair Care', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('3d48bf51-2d70-4663-ba3a-d12da51314cb', 'Beard Wash', 'Sulphate-free wash that cleans without drying.', 410.0, 10, 34, '/uploads/products/the-sharp-edge-beard-wash.svg', 'Beard Care', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('e8534541-9ea5-400b-a6f7-2a56d4daa86f', 'Pre-Shave Oil', 'Softens stubble for a smoother, irritation-free shave.', 540.0, 10, 28, '/uploads/products/the-sharp-edge-pre-shave-oil.svg', 'Shaving', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('676c9aa4-9bd8-417d-8304-28530eabc90b', 'Hair Texture Powder', 'Volumizing powder for instant lift and grip.', 590.0, NULL, 60, '/uploads/products/the-sharp-edge-hair-texture-powder.svg', 'Styling', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('af86c812-d0ca-4ba9-b241-76783289a8fd', 'Grooming Scissors', 'Stainless-steel scissors for precise trims at home.', 850.0, NULL, 92, '/uploads/products/the-sharp-edge-grooming-scissors.svg', 'Accessories', TRUE, '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('8039e996-18dd-4cce-83d4-7fc5cc54c9bc', 'Premium Hair Wax', 'Strong-hold matte hair wax for textured, all-day styles.', 585.0, NULL, 108, '/uploads/products/kings-barber-lounge-premium-hair-wax.svg', 'Styling', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('3f7ec3fd-7210-484a-b664-3e632163f8a9', 'Beard Oil', 'Nourishing beard oil with argan and jojoba.', 715.0, NULL, 63, '/uploads/products/kings-barber-lounge-beard-oil.svg', 'Beard Care', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('863b570c-7c48-443a-9967-5b12f9c652a2', 'Anti-Dandruff Shampoo', 'Gentle daily shampoo that fights flakes.', 416.0, 10, 95, '/uploads/products/kings-barber-lounge-anti-dandruff-shampoo.svg', 'Hair Care', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('c6e7f8ba-b6aa-4875-9a60-bf53fc7b1c8d', 'Aftershave Lotion', 'Soothing aftershave with menthol and aloe.', 494.0, NULL, 61, '/uploads/products/kings-barber-lounge-aftershave-lotion.svg', 'Shaving', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('c970c4c9-25b4-4212-9a0c-dafb8b940664', 'Hair Pomade', 'Classic pomade for a sleek, glossy finish.', 845.0, NULL, 105, '/uploads/products/kings-barber-lounge-hair-pomade.svg', 'Styling', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('f1c91f4c-95d4-4a4d-ab1a-5715f6a07a8c', 'Wooden Beard Comb', 'Anti-static wooden comb for daily beard grooming.', 260.0, 5, 113, '/uploads/products/kings-barber-lounge-wooden-beard-comb.svg', 'Accessories', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('72656200-ac73-4e07-aaf7-f3d000106091', 'Hair Serum', 'Lightweight serum for smooth, frizz-free hair.', 624.0, NULL, 44, '/uploads/products/kings-barber-lounge-hair-serum.svg', 'Hair Care', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('e4079a5d-4ecc-4040-addc-f3cce10f5f4c', 'Beard Balm', 'Rich balm that conditions and tames coarse beards.', 806.0, NULL, 63, '/uploads/products/kings-barber-lounge-beard-balm.svg', 'Beard Care', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('ab827216-7f44-495d-8516-b8b98f86c678', 'Shaving Cream', 'Rich-lather shaving cream for a close, smooth shave.', 377.0, 10, 35, '/uploads/products/kings-barber-lounge-shaving-cream.svg', 'Shaving', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('a3e72b7b-23da-48cb-bc8d-6016bedd5f5a', 'Styling Hair Spray', 'Flexible-hold finishing spray for any look.', 546.0, 5, 100, '/uploads/products/kings-barber-lounge-styling-hair-spray.svg', 'Styling', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('0d964a08-756d-4a2e-9c58-dd66a1f40e1f', 'Hair Conditioner', 'Deep moisturizing conditioner for soft, shiny hair.', 455.0, NULL, 92, '/uploads/products/kings-barber-lounge-hair-conditioner.svg', 'Hair Care', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('8164e839-4845-4cb8-a410-829a3eda18b4', 'Beard Wash', 'Sulphate-free wash that cleans without drying.', 533.0, NULL, 83, '/uploads/products/kings-barber-lounge-beard-wash.svg', 'Beard Care', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('afdeee4f-1334-4987-9dfb-356c6f2ca5e2', 'Pre-Shave Oil', 'Softens stubble for a smoother, irritation-free shave.', 702.0, NULL, 35, '/uploads/products/kings-barber-lounge-pre-shave-oil.svg', 'Shaving', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('5ad03966-73fd-421b-a1bf-e29f9fa9200e', 'Hair Texture Powder', 'Volumizing powder for instant lift and grip.', 767.0, 10, 96, '/uploads/products/kings-barber-lounge-hair-texture-powder.svg', 'Styling', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('898f7af8-7316-4e6a-898e-53ded6cac80c', 'Grooming Scissors', 'Stainless-steel scissors for precise trims at home.', 1105.0, NULL, 43, '/uploads/products/kings-barber-lounge-grooming-scissors.svg', 'Accessories', TRUE, 'a344efdc-131b-454c-9e64-4616bbc77438', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('4fca159f-8db2-467b-a5e2-86b467cb37a2', 'Premium Hair Wax', 'Strong-hold matte hair wax for textured, all-day styles.', 495.0, NULL, 113, '/uploads/products/the-dapper-den-premium-hair-wax.svg', 'Styling', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('82e8c0ec-e542-4774-8e2a-aff69b244c9a', 'Beard Oil', 'Nourishing beard oil with argan and jojoba.', 605.0, NULL, 44, '/uploads/products/the-dapper-den-beard-oil.svg', 'Beard Care', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('97518747-abbf-4950-a3f7-379f54fba332', 'Anti-Dandruff Shampoo', 'Gentle daily shampoo that fights flakes.', 352.0, NULL, 118, '/uploads/products/the-dapper-den-anti-dandruff-shampoo.svg', 'Hair Care', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('75fff172-3ab8-466f-a3ca-81728531c943', 'Aftershave Lotion', 'Soothing aftershave with menthol and aloe.', 418.0, 10, 23, '/uploads/products/the-dapper-den-aftershave-lotion.svg', 'Shaving', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('38efc13d-09a2-42d0-8315-3f7c1d9d4879', 'Hair Pomade', 'Classic pomade for a sleek, glossy finish.', 715.0, 10, 42, '/uploads/products/the-dapper-den-hair-pomade.svg', 'Styling', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('99bd56a1-7eb7-4724-832f-5e1abb543bd0', 'Wooden Beard Comb', 'Anti-static wooden comb for daily beard grooming.', 220.0, NULL, 65, '/uploads/products/the-dapper-den-wooden-beard-comb.svg', 'Accessories', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('2c8fd9dc-9535-4eb6-872f-87e15e2c290f', 'Hair Serum', 'Lightweight serum for smooth, frizz-free hair.', 528.0, NULL, 97, '/uploads/products/the-dapper-den-hair-serum.svg', 'Hair Care', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('b60dea88-0e3a-4ddf-a590-acd77c87c9b4', 'Beard Balm', 'Rich balm that conditions and tames coarse beards.', 682.0, 10, 32, '/uploads/products/the-dapper-den-beard-balm.svg', 'Beard Care', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('2b336b6e-944c-4bff-a1dd-b9c44f47677d', 'Shaving Cream', 'Rich-lather shaving cream for a close, smooth shave.', 319.0, 10, 110, '/uploads/products/the-dapper-den-shaving-cream.svg', 'Shaving', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('3effbf2a-8e84-413b-9c73-872948aaabf4', 'Styling Hair Spray', 'Flexible-hold finishing spray for any look.', 462.0, NULL, 89, '/uploads/products/the-dapper-den-styling-hair-spray.svg', 'Styling', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('3db3afdf-a130-43eb-bea5-0e1c28d2c9d3', 'Hair Conditioner', 'Deep moisturizing conditioner for soft, shiny hair.', 385.0, 5, 32, '/uploads/products/the-dapper-den-hair-conditioner.svg', 'Hair Care', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('54d6629a-3f1f-48fc-a74b-69c336e7094e', 'Beard Wash', 'Sulphate-free wash that cleans without drying.', 451.0, NULL, 111, '/uploads/products/the-dapper-den-beard-wash.svg', 'Beard Care', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('506da6dc-f22c-43fe-a047-a6203e9bd0a1', 'Pre-Shave Oil', 'Softens stubble for a smoother, irritation-free shave.', 594.0, NULL, 34, '/uploads/products/the-dapper-den-pre-shave-oil.svg', 'Shaving', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('56d06952-816b-4e29-901a-debc17aab0c6', 'Hair Texture Powder', 'Volumizing powder for instant lift and grip.', 649.0, NULL, 116, '/uploads/products/the-dapper-den-hair-texture-powder.svg', 'Styling', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('8fe62960-1c95-493b-8b97-8bc552870910', 'Grooming Scissors', 'Stainless-steel scissors for precise trims at home.', 935.0, NULL, 91, '/uploads/products/the-dapper-den-grooming-scissors.svg', 'Accessories', TRUE, '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('bbb728ab-d244-41dc-809a-3d1e41965101', 'Premium Hair Wax', 'Strong-hold matte hair wax for textured, all-day styles.', 382.5, 15, 91, '/uploads/products/trim--style-studio-premium-hair-wax.svg', 'Styling', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('74779384-3410-4e51-9958-e1fd939ff6c9', 'Beard Oil', 'Nourishing beard oil with argan and jojoba.', 467.5, NULL, 82, '/uploads/products/trim--style-studio-beard-oil.svg', 'Beard Care', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('94d3233c-8866-4746-9cb4-ad85d540b86f', 'Anti-Dandruff Shampoo', 'Gentle daily shampoo that fights flakes.', 272.0, NULL, 102, '/uploads/products/trim--style-studio-anti-dandruff-shampoo.svg', 'Hair Care', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('2a36434d-a865-48e4-99c4-5024c1ecc097', 'Aftershave Lotion', 'Soothing aftershave with menthol and aloe.', 323.0, NULL, 102, '/uploads/products/trim--style-studio-aftershave-lotion.svg', 'Shaving', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('a04ab13b-72d1-44b0-9462-9fb3ef4306ee', 'Hair Pomade', 'Classic pomade for a sleek, glossy finish.', 552.5, NULL, 111, '/uploads/products/trim--style-studio-hair-pomade.svg', 'Styling', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('1725a0c3-d33c-4d3b-9052-0657df50258d', 'Wooden Beard Comb', 'Anti-static wooden comb for daily beard grooming.', 170.0, 10, 29, '/uploads/products/trim--style-studio-wooden-beard-comb.svg', 'Accessories', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('8c9e2856-da47-44dc-a847-9c815afad59b', 'Hair Serum', 'Lightweight serum for smooth, frizz-free hair.', 408.0, NULL, 73, '/uploads/products/trim--style-studio-hair-serum.svg', 'Hair Care', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('f3ee9357-db1a-4dbd-bc75-a31e5e9d11dc', 'Beard Balm', 'Rich balm that conditions and tames coarse beards.', 527.0, 10, 79, '/uploads/products/trim--style-studio-beard-balm.svg', 'Beard Care', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('de44c8b3-62e8-48ec-a252-b5bb9579c82b', 'Shaving Cream', 'Rich-lather shaving cream for a close, smooth shave.', 246.5, NULL, 79, '/uploads/products/trim--style-studio-shaving-cream.svg', 'Shaving', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('5930ee8c-be42-4da4-a4b1-12cb5f9f2aa3', 'Styling Hair Spray', 'Flexible-hold finishing spray for any look.', 357.0, NULL, 95, '/uploads/products/trim--style-studio-styling-hair-spray.svg', 'Styling', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('0e015505-37e3-40a0-a1b3-196e09328c70', 'Hair Conditioner', 'Deep moisturizing conditioner for soft, shiny hair.', 297.5, 5, 34, '/uploads/products/trim--style-studio-hair-conditioner.svg', 'Hair Care', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('167eedd8-eb83-4ac3-b3a8-676d94ecbf20', 'Beard Wash', 'Sulphate-free wash that cleans without drying.', 348.5, NULL, 84, '/uploads/products/trim--style-studio-beard-wash.svg', 'Beard Care', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('071e0464-d2ce-4c9f-80a6-5df0f91f0102', 'Pre-Shave Oil', 'Softens stubble for a smoother, irritation-free shave.', 459.0, NULL, 82, '/uploads/products/trim--style-studio-pre-shave-oil.svg', 'Shaving', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('49c09941-e459-4e56-bd6e-5d92adae616f', 'Hair Texture Powder', 'Volumizing powder for instant lift and grip.', 501.5, NULL, 91, '/uploads/products/trim--style-studio-hair-texture-powder.svg', 'Styling', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('746b0440-46d5-4d30-8db7-821c2ddb2fae', 'Grooming Scissors', 'Stainless-steel scissors for precise trims at home.', 722.5, NULL, 29, '/uploads/products/trim--style-studio-grooming-scissors.svg', 'Accessories', TRUE, '52da7eca-2eb8-4ad4-b944-e6128d350e3c', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('b1f50182-7138-474c-8527-e579bc447f78', 'Premium Hair Wax', 'Strong-hold matte hair wax for textured, all-day styles.', 540.0, NULL, 118, '/uploads/products/the-barber-co-premium-hair-wax.svg', 'Styling', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('47c34a63-8860-4b01-ba34-2fdcd34158b1', 'Beard Oil', 'Nourishing beard oil with argan and jojoba.', 660.0, NULL, 45, '/uploads/products/the-barber-co-beard-oil.svg', 'Beard Care', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('c5191faf-3c9e-4c0f-9749-ef0cf71c9a63', 'Anti-Dandruff Shampoo', 'Gentle daily shampoo that fights flakes.', 384.0, NULL, 25, '/uploads/products/the-barber-co-anti-dandruff-shampoo.svg', 'Hair Care', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('b71f5bd7-4042-4bcf-92f4-b18b0d0f9eb0', 'Aftershave Lotion', 'Soothing aftershave with menthol and aloe.', 456.0, 20, 119, '/uploads/products/the-barber-co-aftershave-lotion.svg', 'Shaving', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('a82587b2-c524-43a9-9a17-2590c965857a', 'Hair Pomade', 'Classic pomade for a sleek, glossy finish.', 780.0, NULL, 31, '/uploads/products/the-barber-co-hair-pomade.svg', 'Styling', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('d4cc7009-e39d-4948-b8bd-8a7394bcadc7', 'Wooden Beard Comb', 'Anti-static wooden comb for daily beard grooming.', 240.0, NULL, 85, '/uploads/products/the-barber-co-wooden-beard-comb.svg', 'Accessories', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('f0db9838-a027-4fde-aee7-fc51d456ee4a', 'Hair Serum', 'Lightweight serum for smooth, frizz-free hair.', 576.0, 15, 42, '/uploads/products/the-barber-co-hair-serum.svg', 'Hair Care', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('cb28a66d-f7b0-432f-ad52-72d84f687e77', 'Beard Balm', 'Rich balm that conditions and tames coarse beards.', 744.0, NULL, 111, '/uploads/products/the-barber-co-beard-balm.svg', 'Beard Care', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('1bac72f8-c5ef-423b-a4e9-389b26b5414d', 'Shaving Cream', 'Rich-lather shaving cream for a close, smooth shave.', 348.0, NULL, 40, '/uploads/products/the-barber-co-shaving-cream.svg', 'Shaving', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('468c72e5-df49-404d-98cd-0112157df405', 'Styling Hair Spray', 'Flexible-hold finishing spray for any look.', 504.0, NULL, 66, '/uploads/products/the-barber-co-styling-hair-spray.svg', 'Styling', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('92538d29-9488-4dbe-9fd2-e36b2b705b36', 'Hair Conditioner', 'Deep moisturizing conditioner for soft, shiny hair.', 420.0, NULL, 98, '/uploads/products/the-barber-co-hair-conditioner.svg', 'Hair Care', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('0849a3e1-77e3-4313-8025-6df87b90b0b0', 'Beard Wash', 'Sulphate-free wash that cleans without drying.', 492.0, 20, 30, '/uploads/products/the-barber-co-beard-wash.svg', 'Beard Care', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('91164a20-6bd3-4c10-bc38-a80246755fea', 'Pre-Shave Oil', 'Softens stubble for a smoother, irritation-free shave.', 648.0, NULL, 58, '/uploads/products/the-barber-co-pre-shave-oil.svg', 'Shaving', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('762e50cc-2e12-47af-9d1d-bfc5d4b92d50', 'Hair Texture Powder', 'Volumizing powder for instant lift and grip.', 708.0, 5, 90, '/uploads/products/the-barber-co-hair-texture-powder.svg', 'Styling', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPercentage","StockQuantity","ImageUrl","Category","IsActive","BarberId","CreatedAt")
VALUES ('bf0c61f7-f9e3-4ffd-b912-358fc0e10c7d', 'Grooming Scissors', 'Stainless-steel scissors for precise trims at home.', 1020.0, NULL, 105, '/uploads/products/the-barber-co-grooming-scissors.svg', 'Accessories', TRUE, 'b4e17884-cf2c-4146-9af3-096fe1f3256a', NOW());

-- 8) Bookings (mixed statuses across users & barbers)
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('56cc162a-d697-4d8c-bb9e-217aa342812f', '1b2b0a87-aaa2-4b2f-9c54-969e0703cffd', '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', 'c2a2a149-7bb7-4edf-8963-53885f7aa89a', '2026-04-05', '10:00:00', '10:30:00', 0, 'Looking forward to it.', NULL, 300.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('0f4b331f-edd3-4020-95fd-c28370d63369', '6592b3bd-56e6-44f2-9615-161b1a522f06', 'a344efdc-131b-454c-9e64-4616bbc77438', 'b32c0a44-b9c7-49ec-985e-c9afd1838786', '2026-04-08', '11:15:00', '11:35:00', 1, NULL, NULL, 260.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('c3d2a930-5b07-4461-a626-5e3f73cf2730', '748d72f2-0732-4491-a226-a17549e3dc89', '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', 'a075bc25-1591-4053-81e9-66344334a764', '2026-04-11', '12:30:00', '13:15:00', 3, 'Looking forward to it.', NULL, 495.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('78dbd089-fa16-4bb1-8bbd-1c0131f21a43', 'd70fae7b-8cb3-4b0b-ad39-715595d45f55', '52da7eca-2eb8-4ad4-b944-e6128d350e3c', 'a5fb5538-b3f3-46d6-b5cd-6789bde3f089', '2026-04-14', '13:45:00', '15:15:00', 3, NULL, NULL, 1020.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('d3b04351-7fca-4728-aa76-69cb3520df11', 'b077bc1d-d19b-4bc1-b40b-164e96cdd6e6', 'b4e17884-cf2c-4146-9af3-096fe1f3256a', '79b76945-c38c-4545-91ef-2668e06cfcf8', '2026-04-17', '14:00:00', '14:30:00', 4, 'Looking forward to it.', NULL, 480.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('79d51460-65f9-40f2-8075-bc8c105f345e', '9f14df1f-5d53-4141-9abe-e4259bb73601', '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', 'c2a2a149-7bb7-4edf-8963-53885f7aa89a', '2026-04-20', '15:15:00', '15:45:00', 0, NULL, NULL, 300.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('217902dd-1d33-4da7-93fd-60f7ac62c2da', '631002d9-d080-46d6-8afa-0e2fa1547142', 'a344efdc-131b-454c-9e64-4616bbc77438', 'b32c0a44-b9c7-49ec-985e-c9afd1838786', '2026-04-23', '16:30:00', '16:50:00', 1, 'Looking forward to it.', NULL, 260.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('32f8fa7c-e91d-46cf-9b21-78b0068102b9', '00db77d6-c0e9-4950-b422-b92bee563aea', '4b8a5b52-65d1-45ff-a2ff-0b6b8a328a09', 'a075bc25-1591-4053-81e9-66344334a764', '2026-04-26', '17:45:00', '18:30:00', 3, NULL, NULL, 495.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('2bb8f19f-40c2-4763-9e32-9a1cc8c5abac', '1b2b0a87-aaa2-4b2f-9c54-969e0703cffd', '52da7eca-2eb8-4ad4-b944-e6128d350e3c', 'a5fb5538-b3f3-46d6-b5cd-6789bde3f089', '2026-04-29', '10:00:00', '11:30:00', 3, 'Looking forward to it.', NULL, 1020.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('ed1c32da-e484-4011-9c87-3054e15430f5', '6592b3bd-56e6-44f2-9615-161b1a522f06', 'b4e17884-cf2c-4146-9af3-096fe1f3256a', '79b76945-c38c-4545-91ef-2668e06cfcf8', '2026-05-02', '11:15:00', '11:45:00', 4, NULL, NULL, 480.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('ca8e697d-f95b-412d-ab5f-e99bfb93e07d', '748d72f2-0732-4491-a226-a17549e3dc89', '90eee14b-eeb8-43d0-b2d8-34f119fb4a4e', 'c2a2a149-7bb7-4edf-8963-53885f7aa89a', '2026-05-05', '12:30:00', '13:00:00', 0, 'Looking forward to it.', NULL, 300.0, NOW());
INSERT INTO "Bookings" ("Id","UserId","BarberId","ServiceId","AppointmentDate","StartTime","EndTime","Status","Notes","RejectionReason","TotalAmount","CreatedAt")
VALUES ('9e3a836d-66ef-4fd3-be85-c6922597330d', 'd70fae7b-8cb3-4b0b-ad39-715595d45f55', 'a344efdc-131b-454c-9e64-4616bbc77438', 'b32c0a44-b9c7-49ec-985e-c9afd1838786', '2026-05-08', '13:45:00', '14:05:00', 1, NULL, NULL, 260.0, NOW());

COMMIT;

-- Inserted: 5 barbers, 8 users, 25 services, 75 products, 12 bookings.