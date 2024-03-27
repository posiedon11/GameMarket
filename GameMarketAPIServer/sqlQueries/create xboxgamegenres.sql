USE xbox;

drop table if exists gamegenres;
create table gamegenres(
modernTitleId varchar(15) not null,
genre varchar (15) not null,
foreign key(titleID) references gametitles(modernTitleID) on delete cascade,
primary key(titleId, genre)
)