USE gamemarket;

create table xboxgametitles(
titleID char(15) not null,
titleName varchar(127) not null,
displayImage varchar (300),
modernTitleID char(15),
isGamePass bool not null,
lastScanned datetime default null,
primary key (titleID)
);

select * from xboxgametitles where titleID = (609700427)