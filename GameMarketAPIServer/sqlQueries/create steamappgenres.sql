use steam;

drop table if exists appGenres;

create table appGenres(
appID int unsigned not null,
genre varchar(30) not null,
foreign key(appID) references appIds(appid)on delete cascade,
primary key(appID)
);