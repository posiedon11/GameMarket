use gamemarket;


CREATE TABLE xboxuserprofiles(
xuid char(20) not null,
gamertag varchar(16) not null,
lastScanned datetime  default null,
primary key(xuid)
);

