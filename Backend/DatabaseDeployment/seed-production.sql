INSERT INTO public.departments (name)
VALUES ('General');

INSERT INTO public.users (
    email,
    password_hash,
    full_name,
    phone_number,
    department_id,
    role,
    is_active
)
SELECT
    :'admin_email',
    :'admin_hash',
    :'admin_name',
    :'admin_phone',
    d.id,
    'ADMIN',
    true
FROM public.departments d
WHERE d.name = 'General';

INSERT INTO public.rooms (
    name,
    location,
    capacity,
    description,
    is_active
)
VALUES (
    'Ruang Utama',
    'Gedung Utama',
    10,
    'Ruang awal production',
    true
);

INSERT INTO public.facilities (
    name,
    description
)
VALUES (
    'WiFi',
    'Akses internet'
);
