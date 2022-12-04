/* Init db by postgres

create user motel with password 'motel';

create database motel;

grant all privileges  on database  motel to motel;

-- \c motel
   
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO eSumit;

alter default privileges in schema public grant all privileges on tables to motel;
alter default privileges in schema public grant all privileges on sequences to motel;
alter default privileges in schema public grant all privileges on functions to motel;
alter default privileges in schema public grant all privileges on types to motel;
alter default privileges in schema public grant all privileges on routines to motel;

*/

CREATE TABLE posts
(
    id serial PRIMARY KEY,
    name varchar(50) NOT NULL
);

CREATE TABLE employees
(
    id serial PRIMARY KEY,
    last_name varchar(30) NOT NULL,
    first_name varchar(30) NOT NULL,
    patronymic varchar(30),
    passport_serial CHAR(4) NOT NULL,
    passport_number CHAR(6) NOT NULL,
    birthday date NOT NULL,
    phone varchar(20) NOT NULL,
    post_id int NULL references posts(id)
);

create table employee_history
(
    datetime timestamptz primary key default now(),
    employee_id int references employees(id) on delete set null on update cascade,
    message text not null
);

CREATE TABLE users
(
    id int PRIMARY KEY references employees(id) on delete cascade,
    login varchar(30) UNIQUE NOT NULL,
    password text NOT NULL
);

CREATE TABLE refresh_tokens
(
    id uuid PRIMARY KEY,
    user_id integer NOT NULL REFERENCES users (id) on delete cascade,
    device_uid uuid NOT NULL,
    expires timestamptz NOT NULL
);

CREATE TABLE room_types
(
    id serial PRIMARY KEY,
    name varchar(50) NOT NULL,
    price_per_hour money NOT NULL,
    capacity smallint NOT NULL
);

CREATE TABLE rooms
(
    id int PRIMARY KEY,
    is_cleaning_needed boolean NOT NULL default false,
    is_ready boolean NOT NULL default true,
    room_type_id int NOT NULL references room_types(id)
);

CREATE TABLE lease_agreements
(
    id serial PRIMARY KEY,
    client_name varchar(90) NOT NULL,
    start_at timestamptz NOT NULL,
    end_at timestamptz NOT NULL check (end_at > start_at)
);

CREATE TABLE lease_rooms
(
    lease_agreement_id int references lease_agreements(id) on delete cascade,
    room_id int references rooms(id) on update cascade,
    PRIMARY KEY(lease_agreement_id, room_id)
);

CREATE TABLE room_cleaning
(
    room_id int references rooms(id) on delete cascade on update cascade,
    datetime timestamptz,
    employee_id int NOT NULL references employees(id),
    PRIMARY KEY(room_id, datetime)
);

CREATE TABLE services
(
    id serial PRIMARY KEY,
    name varchar(100) NOT NULL,
    price money NOT NULL,
    is_actual boolean not null default true
);

CREATE TABLE service_orders
(
    id serial PRIMARY KEY,
    service_id int NOT NULL references services(id),
    room_id int NOT NULL references rooms(id) on update cascade,
    datetime timestamptz not null
);

CREATE TABLE advertising_contracts
(
    id serial PRIMARY KEY,
    company_name varchar(100) NOT NULL,
    datetime timestamptz NOT NULL,
    description text NOT NULL,
    cost money NOT NULL,
    employee_id int NOT NULL references employees(id),
    is_active boolean NOT NULL default true
);

---------- procedure, frunction, view, trigger, ... ----------

-- Добавление договора аренды со всеми комнатами
CREATE OR REPLACE PROCEDURE create_lease_agreement(
    p_client_name varchar(90),
    p_start_at timestamptz,
    p_end_at timestamptz,
    p_room_ids int[])
AS $$
DECLARE
    lease_agreement_id int;
    room_id int;
BEGIN
    IF array_length(p_room_ids, 1) = 0 THEN
        RAISE EXCEPTION 'List of rooms cannot be empty';
    END IF;

    insert into lease_agreements(client_name, start_at, end_at)
    values (p_client_name, p_start_at, p_end_at)
    returning id into lease_agreement_id;

    foreach room_id in array p_room_ids loop
            insert into lease_rooms(lease_agreement_id, room_id)
            values (lease_agreement_id, room_id);
        end loop;
END $$ LANGUAGE plpgsql;

-- Список конат, в которых необходима уборка
create view cleaning_needed_rooms as
select rooms.id as number, room_types.name as room_type, room_types.capacity
from rooms
inner join room_types on room_types.id = room_type_id;

-- Получение полного имени из составляющих
CREATE OR REPLACE FUNCTION get_full_name(
    p_first_name VARCHAR(30),
    p_last_name VARCHAR(30),
    p_patronymic VARCHAR(30),
    p_initials BOOLEAN DEFAULT FALSE
)
    RETURNS TEXT AS
$$ BEGIN
    IF NOT p_initials THEN
        RETURN p_last_name || ' ' || p_first_name || COALESCE(' ' || p_patronymic, '');
    ELSE
        RETURN p_last_name || ' ' || SUBSTRING(p_first_name FROM 1 FOR 1) || '.' ||
               COALESCE(SUBSTRING(p_patronymic FROM 1 FOR 1) || '.', '');
    END IF;
END; $$ LANGUAGE plpgsql;

-- Получение рекламных контрактор с подробностями о сотруднике и его должности
create view advertising_contracts_view as
select ac.id, company_name, datetime, description, cost, employee_id,
       get_full_name(e.first_name, e.last_name, e.patronymic) || ' ('
           || post.name || ')' as employee,
       is_active
from advertising_contracts ac
         inner join employees e on e.id = employee_id
         inner join posts post on post.id = e.post_id;

-- Получение заказов, связанных с договором аренды
CREATE OR REPLACE FUNCTION get_orders_related_to_lease_agreement(p_lease_agreement_id int)
    RETURNS setof service_orders
AS $$
DECLARE
    v_start_at timestamptz;
    v_end_at timestamptz;
BEGIN
    select start_at, end_at into v_start_at, v_end_at
    from lease_agreements where id = p_lease_agreement_id;

    return query
        select * from service_orders
        where room_id in (select lease_rooms.room_id from lease_rooms where lease_agreement_id = p_lease_agreement_id)
          and datetime >= v_start_at and datetime <= v_end_at;
END $$ LANGUAGE plpgsql;

-- triggers

CREATE OR REPLACE FUNCTION get_employee_info(p_employee employees)
    RETURNS TEXT AS
$$ BEGIN RETURN p_employee.last_name || ' ' || p_employee.first_name || COALESCE(' ' || p_employee.patronymic, '')
                    || ' (' || p_employee.passport_serial || ' ' || p_employee.passport_number || ')';
END; $$ LANGUAGE plpgsql;

CREATE or replace function on_employee_add_or_update()
    returns trigger as
$$ begin
    IF TG_OP = 'INSERT' THEN
        insert into employee_history(employee_id, message)
        values (new.id, case when new.post_id is not null then
                                             'Сотрудник ' || get_employee_info(new) || ' принят на должность ' || (select name from posts where id = new.post_id)
                             else 'Добавлены данные о сотруднике ' || get_employee_info(new) END
               );

        RETURN NULL;
    ELSIF TG_OP = 'UPDATE' THEN -- Если произвошло добавление талона
        IF old.post_id is not null and new.post_id is null THEN
            insert into employee_history(employee_id, message)
            values (new.id, 'Сотрудник ' || get_employee_info(new) || ' (' ||
                            (select name from posts where id = old.post_id) || ') уволен');
        ELSIF old.post_id is null and new.post_id is not null THEN
            insert into employee_history(employee_id, message)
            values (new.id, 'Сотрудник ' || get_employee_info(new) || ' восстановлен на должность "'
                                || (select name from posts where id = new.post_id) || '"');
        ELSIF old.post_id != new.post_id THEN
            insert into employee_history(employee_id, message)
            values (new.id, 'Сотрудник ' || get_employee_info(new) || ' перемещён с должности "'
                                || (select name from posts where id = old.post_id) || '" на должность "'
                                || (select name from posts where id = new.post_id) || '"');
        END IF;

        RETURN new;
    END IF;
end; $$ language plpgsql;

create trigger employee_add_trigger
    after insert on employees
    for each row
    execute procedure on_employee_add_or_update();

create trigger employee_update_status_or_post_trigger
    after update of post_id on employees
    for each row
    execute procedure on_employee_add_or_update();

/*
DO $$ DECLARE
  r RECORD;
BEGIN
  FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = current_schema()) LOOP
    EXECUTE 'DROP TABLE ' || quote_ident(r.tablename) || ' CASCADE';
  END LOOP;
END $$;
*/