-- Adminer 4.8.1 PostgreSQL 16.4 (Debian 16.4-1.pgdg120+2) dump

\connect "brocker";

CREATE TABLE "public"."attributes" (
    "id" uuid NOT NULL,
    "Key" character varying(255) NOT NULL,
    "value" text,
    CONSTRAINT "attributes_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."chanel_chanels" (
    "from_chanel_id" uuid NOT NULL,
    "to_chanel_id" uuid NOT NULL,
    CONSTRAINT "chanel_chanels_pkey" PRIMARY KEY ("from_chanel_id", "to_chanel_id")
) WITH (oids = false);


CREATE TABLE "public"."chanels" (
    "id" uuid NOT NULL,
    "name" character varying(255) NOT NULL,
    CONSTRAINT "chanels_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."communication_attributes" (
    "id" uuid NOT NULL,
    "connection_id" uuid NOT NULL,
    CONSTRAINT "communication_attributes_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."communications" (
    "id" uuid NOT NULL,
    "type_identifier" uuid NOT NULL,
    "name" character varying(255) NOT NULL,
    "description" text,
    "status" boolean NOT NULL,
    CONSTRAINT "communications_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."connection_attributes" (
    "id" uuid NOT NULL,
    "connection_id" uuid NOT NULL,
    CONSTRAINT "connection_attributes_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."connection_chanels" (
    "connection_id" uuid NOT NULL,
    "chanel_id" uuid NOT NULL,
    CONSTRAINT "connection_chanels_pkey" PRIMARY KEY ("connection_id", "chanel_id")
) WITH (oids = false);


CREATE TABLE "public"."connections" (
    "id" uuid NOT NULL,
    "communication_id" uuid NOT NULL,
    "name" character varying(255) NOT NULL,
    "isinput" boolean NOT NULL,
    "path" text NOT NULL,
    CONSTRAINT "connections_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."execution_logs" (
    "id" uuid NOT NULL,
    "datetime" timestamptz NOT NULL,
    "lavel" character varying(255) NOT NULL,
    "text" text NOT NULL,
    "script_id" uuid NOT NULL,
    CONSTRAINT "execution_logs_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."headers" (
    "id" uuid NOT NULL,
    "name" character varying(255) NOT NULL,
    "value" text NOT NULL,
    "messages_id" uuid NOT NULL,
    CONSTRAINT "headers_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."messages" (
    "id" uuid NOT NULL,
    "datetime" timestamptz NOT NULL,
    "data" bytea NOT NULL,
    "connection_id" uuid,
    "source_channel_id" uuid,
    "target_channel_id" uuid,
    CONSTRAINT "messages_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


CREATE TABLE "public"."scripts" (
    "id" uuid NOT NULL,
    "path" text NOT NULL,
    CONSTRAINT "scripts_pkey" PRIMARY KEY ("id")
) WITH (oids = false);


ALTER TABLE ONLY "public"."chanel_chanels" ADD CONSTRAINT "chanel_chanels_from_chanel_id_fkey" FOREIGN KEY (from_chanel_id) REFERENCES chanels(id) ON DELETE CASCADE NOT DEFERRABLE;
ALTER TABLE ONLY "public"."chanel_chanels" ADD CONSTRAINT "chanel_chanels_to_chanel_id_fkey" FOREIGN KEY (to_chanel_id) REFERENCES chanels(id) ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."communication_attributes" ADD CONSTRAINT "communication_attributes_connection_id_fkey" FOREIGN KEY (connection_id) REFERENCES communications(id) ON DELETE CASCADE NOT DEFERRABLE;
ALTER TABLE ONLY "public"."communication_attributes" ADD CONSTRAINT "communication_attributes_id_fkey" FOREIGN KEY (id) REFERENCES attributes(id) ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."connection_attributes" ADD CONSTRAINT "connection_attributes_connection_id_fkey" FOREIGN KEY (connection_id) REFERENCES connections(id) ON DELETE CASCADE NOT DEFERRABLE;
ALTER TABLE ONLY "public"."connection_attributes" ADD CONSTRAINT "connection_attributes_id_fkey" FOREIGN KEY (id) REFERENCES attributes(id) ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."connection_chanels" ADD CONSTRAINT "connection_chanels_chanel_id_fkey" FOREIGN KEY (chanel_id) REFERENCES chanels(id) ON DELETE CASCADE NOT DEFERRABLE;
ALTER TABLE ONLY "public"."connection_chanels" ADD CONSTRAINT "connection_chanels_connection_id_fkey" FOREIGN KEY (connection_id) REFERENCES connections(id) ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."connections" ADD CONSTRAINT "connections_communication_id_fkey" FOREIGN KEY (communication_id) REFERENCES communications(id) ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."execution_logs" ADD CONSTRAINT "execution_logs_script_id_fkey" FOREIGN KEY (script_id) REFERENCES scripts(id) ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."headers" ADD CONSTRAINT "headers_messages_id_fkey" FOREIGN KEY (messages_id) REFERENCES messages(id) ON DELETE CASCADE NOT DEFERRABLE;

ALTER TABLE ONLY "public"."messages" ADD CONSTRAINT "messages_connection_id_fkey" FOREIGN KEY (connection_id) REFERENCES connections(id) ON DELETE SET NULL NOT DEFERRABLE;
ALTER TABLE ONLY "public"."messages" ADD CONSTRAINT "messages_source_channel_id_fkey" FOREIGN KEY (source_channel_id) REFERENCES chanels(id) ON DELETE SET NULL NOT DEFERRABLE;
ALTER TABLE ONLY "public"."messages" ADD CONSTRAINT "messages_target_channel_id_fkey" FOREIGN KEY (target_channel_id) REFERENCES chanels(id) ON DELETE SET NULL NOT DEFERRABLE;

ALTER TABLE ONLY "public"."scripts" ADD CONSTRAINT "fkscripts802619" FOREIGN KEY (id) REFERENCES chanels(id) NOT DEFERRABLE;

-- 2024-12-14 18:02:28.426728+00
