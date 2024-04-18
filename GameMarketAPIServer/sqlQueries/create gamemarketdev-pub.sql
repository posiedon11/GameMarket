use gamemarket;

drop table if exists developers;
drop table if exists publishers;
create table developers
(
gameID int not null,
deveoper varchar(250) not null,
foreign key(gameID) references gamemarket.gametitles(gameId) on delete cascade,
primary key(gameId, deveoper)
);

create table publishers
(
gameID int not null,
publisher varchar(250) not null,
foreign key(gameID) references gamemarket.gametitles(gameId) on delete cascade,
primary key(gameId, publisher)
);