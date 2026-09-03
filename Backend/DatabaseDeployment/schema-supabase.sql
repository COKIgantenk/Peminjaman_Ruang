--
-- PostgreSQL database dump
--

\restrict HrohNsdpaojPDtAcLGz7ETbNtuZ7umg7cnI6ZaF5ttJ5Clo0PqCKmyxFbeoAmSd

-- Dumped from database version 16.14
-- Dumped by pg_dump version 17.11 (Debian 17.11-1.pgdg13+2)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: public; Type: SCHEMA; Schema: -; Owner: -
--



--
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: -
--



SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: audit_log; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.audit_log (
    id integer NOT NULL,
    admin_id integer NOT NULL,
    action character varying(50) NOT NULL,
    entity_type character varying(50) NOT NULL,
    entity_id integer NOT NULL,
    changes character varying(1000),
    created_at timestamp without time zone DEFAULT now() NOT NULL
);


--
-- Name: audit_log_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.audit_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: audit_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.audit_log_id_seq OWNED BY public.audit_log.id;


--
-- Name: booking_cancellation; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.booking_cancellation (
    id integer NOT NULL,
    booking_id integer NOT NULL,
    cancellation_reason character varying(500) NOT NULL,
    cancelled_by_user_id integer NOT NULL,
    cancelled_at timestamp without time zone DEFAULT now() NOT NULL
);


--
-- Name: booking_cancellation_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.booking_cancellation_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: booking_cancellation_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.booking_cancellation_id_seq OWNED BY public.booking_cancellation.id;


--
-- Name: bookings; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.bookings (
    id integer NOT NULL,
    user_id integer NOT NULL,
    room_id integer NOT NULL,
    booking_date date NOT NULL,
    start_time time without time zone NOT NULL,
    end_time time without time zone NOT NULL,
    num_people integer NOT NULL,
    title character varying(150),
    requester_name character varying(100) NOT NULL,
    requester_division character varying(100) NOT NULL,
    description character varying(500),
    status character varying(20) NOT NULL,
    approval_notes character varying(500),
    approved_by_admin_id integer,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT bookings_status_check CHECK (((status)::text = ANY ((ARRAY['PENDING'::character varying, 'APPROVED'::character varying, 'REJECTED'::character varying, 'CANCELLED'::character varying])::text[]))),
    CONSTRAINT chk_bookings_num_people_positive CHECK ((num_people > 0)),
    CONSTRAINT chk_bookings_time_range CHECK ((start_time < end_time))
);


--
-- Name: bookings_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.bookings_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: bookings_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.bookings_id_seq OWNED BY public.bookings.id;


--
-- Name: departments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.departments (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now() NOT NULL
);


--
-- Name: departments_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.departments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: departments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.departments_id_seq OWNED BY public.departments.id;


--
-- Name: facilities; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.facilities (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    description character varying(255) NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now() NOT NULL
);


--
-- Name: facilities_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.facilities_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: facilities_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.facilities_id_seq OWNED BY public.facilities.id;


--
-- Name: maintenance; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.maintenance (
    id integer NOT NULL,
    room_id integer NOT NULL,
    maintenance_category character varying(100) NOT NULL,
    priority_level character varying(20) NOT NULL,
    facilities_serviced character varying(500),
    documentation character varying(255),
    description character varying(500) NOT NULL,
    created_by_admin_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    completed_at timestamp without time zone,
    start_date date NOT NULL,
    end_date date,
    activated_at timestamp without time zone,
    CONSTRAINT chk_maintenance_date_range CHECK (((end_date IS NULL) OR (end_date >= start_date))),
    CONSTRAINT maintenance_priority_level_check CHECK (((priority_level)::text = ANY ((ARRAY['LOW'::character varying, 'MEDIUM'::character varying, 'HIGH'::character varying])::text[])))
);


--
-- Name: maintenance_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.maintenance_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: maintenance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.maintenance_id_seq OWNED BY public.maintenance.id;


--
-- Name: notifications; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.notifications (
    id integer NOT NULL,
    user_id integer NOT NULL,
    booking_id integer,
    notification_type character varying(50) NOT NULL,
    email_sent boolean DEFAULT false NOT NULL,
    sent_at timestamp without time zone,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    is_read boolean DEFAULT false NOT NULL,
    read_at timestamp without time zone
);


--
-- Name: notifications_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.notifications_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: notifications_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.notifications_id_seq OWNED BY public.notifications.id;


--
-- Name: room_cleaning_session; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.room_cleaning_session (
    id integer NOT NULL,
    room_id integer NOT NULL,
    booking_id integer,
    cleaning_duration character varying(20),
    start_time timestamp without time zone NOT NULL,
    end_time timestamp without time zone,
    is_completed boolean DEFAULT false NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    custom_duration_minutes integer,
    scheduled_end_time timestamp without time zone,
    CONSTRAINT chk_room_cleaning_completion CHECK ((((is_completed = true) AND (end_time IS NOT NULL)) OR ((is_completed = false) AND (end_time IS NULL)))),
    CONSTRAINT chk_room_cleaning_custom_duration CHECK (((((cleaning_duration)::text = 'CUSTOM'::text) AND (custom_duration_minutes IS NOT NULL) AND (custom_duration_minutes > 0)) OR (((cleaning_duration)::text IS DISTINCT FROM 'CUSTOM'::text) AND (custom_duration_minutes IS NULL)))),
    CONSTRAINT chk_room_cleaning_duration CHECK (((cleaning_duration IS NULL) OR ((cleaning_duration)::text = ANY ((ARRAY['10_MINUTES'::character varying, '20_MINUTES'::character varying, '30_MINUTES'::character varying, 'CUSTOM'::character varying])::text[])))),
    CONSTRAINT chk_room_cleaning_end_time CHECK (((end_time IS NULL) OR (end_time >= start_time))),
    CONSTRAINT chk_room_cleaning_scheduled_end CHECK (((scheduled_end_time IS NULL) OR (scheduled_end_time > start_time)))
);


--
-- Name: room_cleaning_session_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.room_cleaning_session_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: room_cleaning_session_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.room_cleaning_session_id_seq OWNED BY public.room_cleaning_session.id;


--
-- Name: room_facilities; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.room_facilities (
    id integer NOT NULL,
    room_id integer NOT NULL,
    facility_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL
);


--
-- Name: room_facilities_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.room_facilities_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: room_facilities_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.room_facilities_id_seq OWNED BY public.room_facilities.id;


--
-- Name: room_status_history; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.room_status_history (
    id integer NOT NULL,
    room_id integer NOT NULL,
    status character varying(50) NOT NULL,
    reason character varying(255),
    changed_by_admin_id integer,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT chk_room_status_history_status CHECK (((status)::text = ANY ((ARRAY['ACTIVE'::character varying, 'OUT_OF_SERVICE'::character varying, 'MAINTENANCE'::character varying, 'CLEANING'::character varying])::text[])))
);


--
-- Name: room_status_history_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.room_status_history_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: room_status_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.room_status_history_id_seq OWNED BY public.room_status_history.id;


--
-- Name: rooms; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.rooms (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    location character varying(100) NOT NULL,
    capacity integer NOT NULL,
    description character varying(500) NOT NULL,
    image_url character varying(255),
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT chk_rooms_capacity_positive CHECK ((capacity > 0))
);


--
-- Name: rooms_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.rooms_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: rooms_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.rooms_id_seq OWNED BY public.rooms.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.users (
    id integer NOT NULL,
    email character varying(100) NOT NULL,
    password_hash character varying(255) NOT NULL,
    full_name character varying(100) NOT NULL,
    phone_number character varying(20) NOT NULL,
    department_id integer NOT NULL,
    role character varying(20) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    last_login timestamp without time zone,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now() NOT NULL,
    deleted_at timestamp without time zone,
    CONSTRAINT users_role_check CHECK (((role)::text = ANY ((ARRAY['USER'::character varying, 'ADMIN'::character varying])::text[])))
);


--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: audit_log id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_log ALTER COLUMN id SET DEFAULT nextval('public.audit_log_id_seq'::regclass);


--
-- Name: booking_cancellation id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.booking_cancellation ALTER COLUMN id SET DEFAULT nextval('public.booking_cancellation_id_seq'::regclass);


--
-- Name: bookings id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings ALTER COLUMN id SET DEFAULT nextval('public.bookings_id_seq'::regclass);


--
-- Name: departments id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.departments ALTER COLUMN id SET DEFAULT nextval('public.departments_id_seq'::regclass);


--
-- Name: facilities id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.facilities ALTER COLUMN id SET DEFAULT nextval('public.facilities_id_seq'::regclass);


--
-- Name: maintenance id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.maintenance ALTER COLUMN id SET DEFAULT nextval('public.maintenance_id_seq'::regclass);


--
-- Name: notifications id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notifications ALTER COLUMN id SET DEFAULT nextval('public.notifications_id_seq'::regclass);


--
-- Name: room_cleaning_session id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_cleaning_session ALTER COLUMN id SET DEFAULT nextval('public.room_cleaning_session_id_seq'::regclass);


--
-- Name: room_facilities id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_facilities ALTER COLUMN id SET DEFAULT nextval('public.room_facilities_id_seq'::regclass);


--
-- Name: room_status_history id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_status_history ALTER COLUMN id SET DEFAULT nextval('public.room_status_history_id_seq'::regclass);


--
-- Name: rooms id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.rooms ALTER COLUMN id SET DEFAULT nextval('public.rooms_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Name: audit_log audit_log_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_log
    ADD CONSTRAINT audit_log_pkey PRIMARY KEY (id);


--
-- Name: booking_cancellation booking_cancellation_booking_id_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.booking_cancellation
    ADD CONSTRAINT booking_cancellation_booking_id_key UNIQUE (booking_id);


--
-- Name: booking_cancellation booking_cancellation_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.booking_cancellation
    ADD CONSTRAINT booking_cancellation_pkey PRIMARY KEY (id);


--
-- Name: bookings bookings_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);


--
-- Name: departments departments_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_name_key UNIQUE (name);


--
-- Name: departments departments_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_pkey PRIMARY KEY (id);


--
-- Name: facilities facilities_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.facilities
    ADD CONSTRAINT facilities_name_key UNIQUE (name);


--
-- Name: facilities facilities_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.facilities
    ADD CONSTRAINT facilities_pkey PRIMARY KEY (id);


--
-- Name: maintenance maintenance_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.maintenance
    ADD CONSTRAINT maintenance_pkey PRIMARY KEY (id);


--
-- Name: notifications notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_pkey PRIMARY KEY (id);


--
-- Name: room_cleaning_session room_cleaning_session_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_cleaning_session
    ADD CONSTRAINT room_cleaning_session_pkey PRIMARY KEY (id);


--
-- Name: room_facilities room_facilities_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_facilities
    ADD CONSTRAINT room_facilities_pkey PRIMARY KEY (id);


--
-- Name: room_facilities room_facilities_room_id_facility_id_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_facilities
    ADD CONSTRAINT room_facilities_room_id_facility_id_key UNIQUE (room_id, facility_id);


--
-- Name: room_status_history room_status_history_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_status_history
    ADD CONSTRAINT room_status_history_pkey PRIMARY KEY (id);


--
-- Name: rooms rooms_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.rooms
    ADD CONSTRAINT rooms_pkey PRIMARY KEY (id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: idx_audit_log__admin_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_audit_log__admin_id ON public.audit_log USING btree (admin_id);


--
-- Name: idx_audit_log__created_at; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_audit_log__created_at ON public.audit_log USING btree (created_at);


--
-- Name: idx_booking_cancellation__cancelled_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_booking_cancellation__cancelled_by_user_id ON public.booking_cancellation USING btree (cancelled_by_user_id);


--
-- Name: idx_bookings__approved_by_admin_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_bookings__approved_by_admin_id ON public.bookings USING btree (approved_by_admin_id);


--
-- Name: idx_bookings__room_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_bookings__room_id ON public.bookings USING btree (room_id);


--
-- Name: idx_bookings__user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_bookings__user_id ON public.bookings USING btree (user_id);


--
-- Name: idx_bookings_active_room_schedule; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_bookings_active_room_schedule ON public.bookings USING btree (room_id, booking_date, start_time, end_time) WHERE ((status)::text = ANY ((ARRAY['PENDING'::character varying, 'APPROVED'::character varying])::text[]));


--
-- Name: idx_bookings_date_start_time; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_bookings_date_start_time ON public.bookings USING btree (booking_date, start_time);


--
-- Name: idx_bookings_status_schedule; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_bookings_status_schedule ON public.bookings USING btree (status, booking_date DESC, start_time DESC);


--
-- Name: idx_maintenance__created_by_admin_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_maintenance__created_by_admin_id ON public.maintenance USING btree (created_by_admin_id);


--
-- Name: idx_maintenance__room_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_maintenance__room_id ON public.maintenance USING btree (room_id);


--
-- Name: idx_maintenance_active_room_dates; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_maintenance_active_room_dates ON public.maintenance USING btree (room_id, start_date, end_date) WHERE (completed_at IS NULL);


--
-- Name: idx_maintenance_pending_activation; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_maintenance_pending_activation ON public.maintenance USING btree (start_date) WHERE ((completed_at IS NULL) AND (activated_at IS NULL));


--
-- Name: idx_maintenance_pending_completion; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_maintenance_pending_completion ON public.maintenance USING btree (end_date) WHERE ((activated_at IS NOT NULL) AND (completed_at IS NULL) AND (end_date IS NOT NULL));


--
-- Name: idx_notifications__booking_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_notifications__booking_id ON public.notifications USING btree (booking_id);


--
-- Name: idx_notifications_user_created; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_notifications_user_created ON public.notifications USING btree (user_id, created_at DESC);


--
-- Name: idx_room_cleaning_pending_completion; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_room_cleaning_pending_completion ON public.room_cleaning_session USING btree (scheduled_end_time) WHERE ((is_completed = false) AND (scheduled_end_time IS NOT NULL));


--
-- Name: idx_room_cleaning_room_created; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_room_cleaning_room_created ON public.room_cleaning_session USING btree (room_id, created_at DESC);


--
-- Name: idx_room_facilities__facility_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_room_facilities__facility_id ON public.room_facilities USING btree (facility_id);


--
-- Name: idx_room_status_history__changed_by_admin_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_room_status_history__changed_by_admin_id ON public.room_status_history USING btree (changed_by_admin_id);


--
-- Name: idx_room_status_history_room_created; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_room_status_history_room_created ON public.room_status_history USING btree (room_id, created_at DESC);


--
-- Name: idx_users__department_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_users__department_id ON public.users USING btree (department_id);


--
-- Name: idx_users__email; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_users__email ON public.users USING btree (email);


--
-- Name: idx_users__role; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_users__role ON public.users USING btree (role);


--
-- Name: uq_room_cleaning_session_booking_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX uq_room_cleaning_session_booking_id ON public.room_cleaning_session USING btree (booking_id) WHERE (booking_id IS NOT NULL);


--
-- Name: ux_room_cleaning_session_booking_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ux_room_cleaning_session_booking_id ON public.room_cleaning_session USING btree (booking_id) WHERE (booking_id IS NOT NULL);


--
-- Name: ux_users_email_active; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ux_users_email_active ON public.users USING btree (email) WHERE (deleted_at IS NULL);


--
-- Name: audit_log fk_audit_log__admin_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_log
    ADD CONSTRAINT fk_audit_log__admin_id FOREIGN KEY (admin_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: booking_cancellation fk_booking_cancellation__booking_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.booking_cancellation
    ADD CONSTRAINT fk_booking_cancellation__booking_id FOREIGN KEY (booking_id) REFERENCES public.bookings(id) ON DELETE CASCADE;


--
-- Name: booking_cancellation fk_booking_cancellation__cancelled_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.booking_cancellation
    ADD CONSTRAINT fk_booking_cancellation__cancelled_by_user_id FOREIGN KEY (cancelled_by_user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: bookings fk_bookings__approved_by_admin_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT fk_bookings__approved_by_admin_id FOREIGN KEY (approved_by_admin_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: bookings fk_bookings__room_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT fk_bookings__room_id FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON DELETE CASCADE;


--
-- Name: bookings fk_bookings__user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT fk_bookings__user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: maintenance fk_maintenance__created_by_admin_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.maintenance
    ADD CONSTRAINT fk_maintenance__created_by_admin_id FOREIGN KEY (created_by_admin_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: maintenance fk_maintenance__room_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.maintenance
    ADD CONSTRAINT fk_maintenance__room_id FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON DELETE CASCADE;


--
-- Name: notifications fk_notifications__booking_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT fk_notifications__booking_id FOREIGN KEY (booking_id) REFERENCES public.bookings(id) ON DELETE SET NULL;


--
-- Name: notifications fk_notifications__user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT fk_notifications__user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: room_cleaning_session fk_room_cleaning_session__booking_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_cleaning_session
    ADD CONSTRAINT fk_room_cleaning_session__booking_id FOREIGN KEY (booking_id) REFERENCES public.bookings(id) ON DELETE SET NULL;


--
-- Name: room_cleaning_session fk_room_cleaning_session__room_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_cleaning_session
    ADD CONSTRAINT fk_room_cleaning_session__room_id FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON DELETE CASCADE;


--
-- Name: room_facilities fk_room_facilities__facility_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_facilities
    ADD CONSTRAINT fk_room_facilities__facility_id FOREIGN KEY (facility_id) REFERENCES public.facilities(id) ON DELETE CASCADE;


--
-- Name: room_facilities fk_room_facilities__room_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_facilities
    ADD CONSTRAINT fk_room_facilities__room_id FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON DELETE CASCADE;


--
-- Name: room_status_history fk_room_status_history__changed_by_admin_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_status_history
    ADD CONSTRAINT fk_room_status_history__changed_by_admin_id FOREIGN KEY (changed_by_admin_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: room_status_history fk_room_status_history__room_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.room_status_history
    ADD CONSTRAINT fk_room_status_history__room_id FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON DELETE CASCADE;


--
-- Name: users fk_users__department_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT fk_users__department_id FOREIGN KEY (department_id) REFERENCES public.departments(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

\unrestrict HrohNsdpaojPDtAcLGz7ETbNtuZ7umg7cnI6ZaF5ttJ5Clo0PqCKmyxFbeoAmSd

