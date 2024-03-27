use xbox;


CREATE TABLE userprofiles(
xuid char(20) not null,
gamertag varchar(16) not null,
lastScanned datetime  default null,
primary key(xuid)
);

