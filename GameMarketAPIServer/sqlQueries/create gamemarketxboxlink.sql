use gamemarket;

drop table if exists xboxLink;
create table xboxLink(
gameID int not null,
modernTitleID varchar(15) not null,
foreign key(modernTitleID) references xbox.gametitles(modernTitleID) on delete cascade,
foreign key(gameID) references gamemarket.gametitles(gameId) on delete cascade,
primary key(gameId, modernTitleId)
);
