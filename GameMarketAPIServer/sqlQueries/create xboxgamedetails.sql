USE gamemarket;

drop table if exists xboxgamedetails;
create table xboxgamedetails(
productId char(15) not null,
titleId  char(15) not null,
publisher varchar(30) not null,
developer varchar (30) not null,
primary key(productId),
foreign key (titleID) references xboxgametitles(titleID) on delete cascade
);
