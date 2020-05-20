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

CREATE TABLE high_scores(
id UUID primary key NOT NULL,
points integer NOT NULL,
plus_points integer NOT NULL,
minus_points integer NOT NULL,	
win varchar(255) NOT NULL,
lose varchar(255) NOT NULL	
)