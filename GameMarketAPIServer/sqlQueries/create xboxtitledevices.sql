use xbox;

drop table if exists titleDevices;
create table titleDevices(
modernTitleId char(15) not null,
device varchar (15) not null,
foreign key (modernTitleId) references gametitles(moderntitleId) on delete cascade,
primary key (modernTitleId, device)
);