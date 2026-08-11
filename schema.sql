-- 1. DEPARTMENTS (Departemen/Divisi)
CREATE TABLE "departments" (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(100) UNIQUE NOT NULL,
  "created_at" TIMESTAMP NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMP NOT NULL DEFAULT now()
);

-- 2. FACILITIES (Fasilitas)
CREATE TABLE "facilities" (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(50) UNIQUE NOT NULL,
  "description" VARCHAR(255) NOT NULL,
  "created_at" TIMESTAMP NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMP NOT NULL DEFAULT now()
);

-- 3. ROOMS (Ruangan)
CREATE TABLE "rooms" (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(100) NOT NULL,
  "location" VARCHAR(100) NOT NULL,
  "capacity" INTEGER NOT NULL,
  "description" VARCHAR(500) NOT NULL,
  "image_url" VARCHAR(255),
  "is_active" BOOLEAN NOT NULL DEFAULT true,
  "created_at" TIMESTAMP NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMP NOT NULL DEFAULT now()
);

-- 4. ROOM_FACILITIES (Junction Table - Many-to-Many)
CREATE TABLE "room_facilities" (
  "id" SERIAL PRIMARY KEY,
  "room_id" INTEGER NOT NULL,
  "facility_id" INTEGER NOT NULL,
  "created_at" TIMESTAMP NOT NULL DEFAULT now(),
  UNIQUE("room_id", "facility_id")
);

CREATE INDEX "idx_room_facilities__room_id" ON "room_facilities" ("room_id");
CREATE INDEX "idx_room_facilities__facility_id" ON "room_facilities" ("facility_id");

ALTER TABLE "room_facilities" 
  ADD CONSTRAINT "fk_room_facilities__room_id" 
  FOREIGN KEY ("room_id") REFERENCES "rooms" ("id") ON DELETE CASCADE;

ALTER TABLE "room_facilities" 
  ADD CONSTRAINT "fk_room_facilities__facility_id" 
  FOREIGN KEY ("facility_id") REFERENCES "facilities" ("id") ON DELETE CASCADE;

-- 5. USERS (Pengguna - User & Admin)
CREATE TABLE "users" (
  "id" SERIAL PRIMARY KEY,
  "email" VARCHAR(100) UNIQUE NOT NULL,
  "password_hash" VARCHAR(255) NOT NULL,
  "full_name" VARCHAR(100) NOT NULL,
  "phone_number" VARCHAR(20) NOT NULL,
  "department_id" INTEGER NOT NULL,
  "role" VARCHAR(20) NOT NULL CHECK ("role" IN ('USER', 'ADMIN')),
  "is_active" BOOLEAN NOT NULL DEFAULT true,
  "last_login" TIMESTAMP,
  "created_at" TIMESTAMP NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMP NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMP
);

CREATE INDEX "idx_users__department_id" ON "users" ("department_id");
CREATE INDEX "idx_users__email" ON "users" ("email");
CREATE INDEX "idx_users__role" ON "users" ("role");

ALTER TABLE "users" 
  ADD CONSTRAINT "fk_users__department_id" 
  FOREIGN KEY ("department_id") REFERENCES "departments" ("id") ON DELETE RESTRICT;

-- 6. BOOKINGS (Pemesanan Ruangan)
CREATE TABLE "bookings" (
  "id" SERIAL PRIMARY KEY,
  "user_id" INTEGER NOT NULL,
  "room_id" INTEGER NOT NULL,
  "booking_date" DATE NOT NULL,
  "start_time" TIME NOT NULL,
  "end_time" TIME NOT NULL,
  "num_people" INTEGER NOT NULL,
  "title" VARCHAR(150),
  "requester_name" VARCHAR(100) NOT NULL,
  "requester_division" VARCHAR(100) NOT NULL,
  "description" VARCHAR(500),
  "status" VARCHAR(20) NOT NULL CHECK ("status" IN ('PENDING', 'APPROVED', 'REJECTED', 'DECLINED', 'CANCELLED')),
  "approval_notes" VARCHAR(500),
  "approved_by_admin_id" INTEGER,
  "created_at" TIMESTAMP NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMP NOT NULL DEFAULT now()
);

CREATE INDEX "idx_bookings__user_id" ON "bookings" ("user_id");
CREATE INDEX "idx_bookings__room_id" ON "bookings" ("room_id");
CREATE INDEX "idx_bookings__approved_by_admin_id" ON "bookings" ("approved_by_admin_id");
CREATE INDEX "idx_bookings__status" ON "bookings" ("status");
CREATE INDEX "idx_bookings__booking_date" ON "bookings" ("booking_date");

ALTER TABLE "bookings" 
  ADD CONSTRAINT "fk_bookings__user_id" 
  FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "bookings" 
  ADD CONSTRAINT "fk_bookings__room_id" 
  FOREIGN KEY ("room_id") REFERENCES "rooms" ("id") ON DELETE CASCADE;

ALTER TABLE "bookings" 
  ADD CONSTRAINT "fk_bookings__approved_by_admin_id" 
  FOREIGN KEY ("approved_by_admin_id") REFERENCES "users" ("id") ON DELETE SET NULL;

-- 7. BOOKING_CANCELLATION (Pembatalan Booking)
CREATE TABLE "booking_cancellation" (
  "id" SERIAL PRIMARY KEY,
  "booking_id" INTEGER NOT NULL UNIQUE,
  "cancellation_reason" VARCHAR(500) NOT NULL,
  "cancelled_by_user_id" INTEGER NOT NULL,
  "cancelled_at" TIMESTAMP NOT NULL DEFAULT now()
);

CREATE INDEX "idx_booking_cancellation__booking_id" ON "booking_cancellation" ("booking_id");
CREATE INDEX "idx_booking_cancellation__cancelled_by_user_id" ON "booking_cancellation" ("cancelled_by_user_id");

ALTER TABLE "booking_cancellation" 
  ADD CONSTRAINT "fk_booking_cancellation__booking_id" 
  FOREIGN KEY ("booking_id") REFERENCES "bookings" ("id") ON DELETE CASCADE;

ALTER TABLE "booking_cancellation" 
  ADD CONSTRAINT "fk_booking_cancellation__cancelled_by_user_id" 
  FOREIGN KEY ("cancelled_by_user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

-- 8. ROOM_CLEANING_SESSION (Sesi Pembersihan Ruangan)
CREATE TABLE "room_cleaning_session" (
  "id" SERIAL PRIMARY KEY,
  "room_id" INTEGER NOT NULL,
  "booking_id" INTEGER,
  "cleaning_duration" VARCHAR(20) NOT NULL,
  "start_time" TIMESTAMP NOT NULL,
  "end_time" TIMESTAMP,
  "is_completed" BOOLEAN NOT NULL DEFAULT false,
  "created_at" TIMESTAMP NOT NULL DEFAULT now()
);

CREATE INDEX "idx_room_cleaning_session__room_id" ON "room_cleaning_session" ("room_id");
CREATE INDEX "idx_room_cleaning_session__booking_id" ON "room_cleaning_session" ("booking_id");

ALTER TABLE "room_cleaning_session" 
  ADD CONSTRAINT "fk_room_cleaning_session__room_id" 
  FOREIGN KEY ("room_id") REFERENCES "rooms" ("id") ON DELETE CASCADE;

ALTER TABLE "room_cleaning_session" 
  ADD CONSTRAINT "fk_room_cleaning_session__booking_id" 
  FOREIGN KEY ("booking_id") REFERENCES "bookings" ("id") ON DELETE SET NULL;

-- 9. ROOM_STATUS_HISTORY (History Status Ruangan)
CREATE TABLE "room_status_history" (
  "id" SERIAL PRIMARY KEY,
  "room_id" INTEGER NOT NULL,
  "status" VARCHAR(50) NOT NULL,
  "reason" VARCHAR(255),
  "changed_by_admin_id" INTEGER,
  "created_at" TIMESTAMP NOT NULL DEFAULT now()
);

CREATE INDEX "idx_room_status_history__room_id" ON "room_status_history" ("room_id");
CREATE INDEX "idx_room_status_history__changed_by_admin_id" ON "room_status_history" ("changed_by_admin_id");

ALTER TABLE "room_status_history" 
  ADD CONSTRAINT "fk_room_status_history__room_id" 
  FOREIGN KEY ("room_id") REFERENCES "rooms" ("id") ON DELETE CASCADE;

ALTER TABLE "room_status_history" 
  ADD CONSTRAINT "fk_room_status_history__changed_by_admin_id" 
  FOREIGN KEY ("changed_by_admin_id") REFERENCES "users" ("id") ON DELETE SET NULL;

-- 10. MAINTENANCE (Maintenance Ruangan)
CREATE TABLE "maintenance" (
  "id" SERIAL PRIMARY KEY,
  "room_id" INTEGER NOT NULL,
  "maintenance_category" VARCHAR(100) NOT NULL,
  "priority_level" VARCHAR(20) NOT NULL CHECK ("priority_level" IN ('LOW', 'MEDIUM', 'HIGH')),
  "facilities_serviced" VARCHAR(500),
  "documentation" VARCHAR(255),
  "description" VARCHAR(500) NOT NULL,
  "created_by_admin_id" INTEGER NOT NULL,
  "created_at" TIMESTAMP NOT NULL DEFAULT now(),
  "completed_at" TIMESTAMP
);

CREATE INDEX "idx_maintenance__room_id" ON "maintenance" ("room_id");
CREATE INDEX "idx_maintenance__created_by_admin_id" ON "maintenance" ("created_by_admin_id");

ALTER TABLE "maintenance" 
  ADD CONSTRAINT "fk_maintenance__room_id" 
  FOREIGN KEY ("room_id") REFERENCES "rooms" ("id") ON DELETE CASCADE;

ALTER TABLE "maintenance" 
  ADD CONSTRAINT "fk_maintenance__created_by_admin_id" 
  FOREIGN KEY ("created_by_admin_id") REFERENCES "users" ("id") ON DELETE CASCADE;

-- 11. NOTIFICATIONS (Email Notifications)
CREATE TABLE "notifications" (
  "id" SERIAL PRIMARY KEY,
  "user_id" INTEGER NOT NULL,
  "booking_id" INTEGER,
  "notification_type" VARCHAR(50) NOT NULL,
  "email_sent" BOOLEAN NOT NULL DEFAULT false,
  "sent_at" TIMESTAMP,
  "created_at" TIMESTAMP NOT NULL DEFAULT now()
);

CREATE INDEX "idx_notifications__user_id" ON "notifications" ("user_id");
CREATE INDEX "idx_notifications__booking_id" ON "notifications" ("booking_id");

ALTER TABLE "notifications" 
  ADD CONSTRAINT "fk_notifications__user_id" 
  FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

ALTER TABLE "notifications" 
  ADD CONSTRAINT "fk_notifications__booking_id" 
  FOREIGN KEY ("booking_id") REFERENCES "bookings" ("id") ON DELETE SET NULL;

-- 12. AUDIT_LOG (Log Semua Aksi Admin)
CREATE TABLE "audit_log" (
  "id" SERIAL PRIMARY KEY,
  "admin_id" INTEGER NOT NULL,
  "action" VARCHAR(50) NOT NULL,
  "entity_type" VARCHAR(50) NOT NULL,
  "entity_id" INTEGER NOT NULL,
  "changes" VARCHAR(1000),
  "created_at" TIMESTAMP NOT NULL DEFAULT now()
);

CREATE INDEX "idx_audit_log__admin_id" ON "audit_log" ("admin_id");
CREATE INDEX "idx_audit_log__created_at" ON "audit_log" ("created_at");

ALTER TABLE "audit_log" 
  ADD CONSTRAINT "fk_audit_log__admin_id" 
  FOREIGN KEY ("admin_id") REFERENCES "users" ("id") ON DELETE CASCADE;