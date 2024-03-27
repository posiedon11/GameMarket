USE xbox;

drop table if exists gametitles;
create table gametitles(
titleID varchar(15) not null,
titleName varchar(127) not null,
displayImage varchar (300),
modernTitleID varchar(15) not null,
isGamePass bool not null,
groupID varchar(45) default null,
lastScanned datetime default null,
primary key (modernTitleID)
);
