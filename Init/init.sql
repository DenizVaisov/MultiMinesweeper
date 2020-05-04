CREATE TABLE sessions (
id SERIAL PRIMARY KEY,
email varchar(255),
name varchar(255),
password varchar(255)
)

CREATE TABLE news(
id UUID NOT NULL PRIMARY KEY,
title VARCHAR(255),
description text,
image text
)