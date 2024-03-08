USE gamemarket;

create table xboxgamegenres(
titleId char(15) not null,
genre char (10) not null,
primary key(titleId, genre),
foreign key(titleId) references xboxgametitles(titleId)
)