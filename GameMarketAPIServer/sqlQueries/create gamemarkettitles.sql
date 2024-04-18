use gamemarket;

drop table if exists gameTitles;
create table gameTitles(
gameId int unsigned not null auto_increment,
gameTitle varchar(200) not null,
primary key (gameId)
) auto_increment = 10000;